using StockMan.Message.DataAccess;
using StockMan.Message.Model;
using StockMan.Message.Task;
using StockMan.Message.Task.Biz;
using StockMan.Message.Task.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockMan.Message.Task.Worder
{
    public class WorkerThread : IReceiveCommand
    {
        TaskService taskService = new TaskService();
        MessageService messageService = new MessageService();
        Dictionary<string, TaskExcuter> taskExcuters = new Dictionary<string, TaskExcuter>();

        MessageListener worklistener = null;
        private Thread thread = null;
        private Thread moniterTread = null;
        private bool running = false;
        public WorkerThread()
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            taskExcuters = new Dictionary<string, TaskExcuter>();

            this.moniterTread = new Thread(Moniter);
            this.moniterTread.Start();

        }

        public void Start()
        {
            if (!this.running)
            {
                this.thread = new Thread(Run);
                this.thread.Start();

                this.running = true;
            }
        }
        public void Run()
        {
            if (worklistener != null) { worklistener.Dispose(); }

            using (worklistener = new MessageListener())
            {
                worklistener.onReceive += this.HandleMessage;
                worklistener.onError += listener_onError;
                worklistener.connect();
            }
            this.running = false;
            this.Log().Info("消息监听线程终止");
        }
        public void Moniter()
        {
            while (true)
            {
                IList<string> delList = new List<string>();
                foreach (var taskType in taskExcuters.Keys)
                {
                    this.Log().Info(taskExcuters[taskType].GetStatus());
                    if (!taskExcuters[taskType].IsBusy())
                    {
                        this.Log().Info(string.Format("任务服务:{0}空闲，卸载开始", taskType));
                        Loader.Instance.UnLoad(taskType);
                        delList.Add(taskType);
                        this.Log().Info(string.Format("任务服务:{0}空闲，卸载成功", taskType));
                    }

                }
                foreach (var code in delList)
                {
                    taskExcuters.Remove(code);
                }
                this.Log().Info(string.Format("{0}：监控服务监控中……,状态：{1}",DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),this.running? "运行中":"已停止"));
                Thread.Sleep(1000 * 60);
            }
        }
        internal void Stop()
        {
            this.Log().Info("关闭");

            if (this.worklistener != null)
                this.worklistener.Close();

            this.Log().Info("关闭结束");
        }
        public void UnLoadExcuter()
        {
            foreach (var taskType in taskExcuters.Keys)
            {
                this.Log().Info(string.Format("卸载任务执行器:{0}", taskType));
                while (true)
                {
                    if (taskExcuters[taskType].Status == TaskExcuterStatus.Running)
                    {
                        this.Log().Info(string.Format("等待执行结束:{0}", taskType));
                        Thread.Sleep(1000);
                        continue;
                    }
                    else
                    {
                        Loader.Instance.UnLoad(taskType);
                        this.Log().Info(string.Format("卸载完成:{0}", taskType));
                        break;
                    }
                }
            }
            taskExcuters.Clear();
        }
        public void InitTaskList()
        {
            //IList<mq_task> list = taskService.GetTaskList();

            //foreach (var task in list)
            //{
            //    RemoteLoader rl =  Loader.Instance.GetRemoteLoader(task.assembly, task.type);
            //    var excuter = rl.GetTaskExcuteer();
            //    excuter.Load(task.assembly, task.type);
            //    excuter.Start();
            //    taskExcuters.Add(task.assembly + task.type, excuter);
            //}
            if (taskExcuters != null)
                taskExcuters.Clear();
        }
        public void HandleMessage(TaskMessage msg)
        {
            if (!this.taskExcuters.ContainsKey(msg.task_code))
            {
                this.Log().Error("未找到消息处理服务:task_code:" + msg.task_code);

                if (!initExcuter(msg.task_code))
                {
                    this.Log().Info("消息重置为未处理:message_code:" + msg.code);
                    messageService.UpdateStatus(msg.code, MessageStatus.UnHandle);
                    Thread.Sleep(30000);
                    return;
                }
            }

            var task = this.taskExcuters[msg.task_code];
            if (task == null)
            {
                this.Log().Error("消息处理服务初始化错误:task_code:" + msg.task_code);
                return;
            }

            try
            {
                taskExcuters[msg.task_code].Post(msg);
            }
            catch (Exception ex)
            {
                this.Log().Error(string.Format("分发消息失败：code:{0},description:{1},Exception:{2}", msg.code, msg.description, ex.Message));
            }

        }

        private bool initExcuter(string taskCode)
        {
            this.Log().Info("初始构建Task执行器:task_code:" + taskCode);
            try
            {
                mq_task task = taskService.Find(taskCode);

                RemoteLoader rl = Loader.Instance.GetRemoteLoader(task.assembly,task.code);

                var excuter = rl.GetTaskExcuteer();
                excuter.Load(task.assembly, task.type);
                excuter.Start();

                taskExcuters.Add(task.code, excuter);

                return true;
            }
            catch (Exception ex)
            {
                this.Log().Error(string.Format("构建Task执行器异常：taskCode:{0}，异常:{1}", taskCode, ex.Message));
                return false;
            }
        }

        private void listener_onError(string obj)
        {
            this.Log().Error(obj);
        }

        public void Receive(CmdMessage cmdMsg)
        {
            var cmd = cmdMsg.command;
            if (cmd == "start")
            {
                this.Log().Info("任务开始");
                this.Start();
            }
            else if (cmd == "stop")
            {
                this.Stop();
            }
            else if (cmd == "init")
            {
                this.Stop();
                this.Clear();

                UnLoadExcuter();

                var assembly = cmdMsg.Get("task_assembly");
                Loader.Instance.CreateTaskAssembly(assembly, cmdMsg.attachment);            
                
            }
        }

        private void Clear()
        {
            this.Log().Info("清除缓存消息");
            foreach (var value in this.taskExcuters.Values)
            {
                this.Log().Info("清除缓存消息：" + value.GetStatus());
                value.Clear();
            }
            this.Log().Info("清除缓存消息j结束");
        }
    }
}
