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
        public WorkerThread()
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();
            this.thread = new Thread(Run);
            this.moniterTread = new Thread(Moniter);
        }

        public void Start()
        {
            this.thread.Start();
            this.moniterTread.Start();
        }
        public void Run()
        {
            if (worklistener != null) { worklistener.Dispose(); }
            worklistener = new MessageListener();
            worklistener.onReceive += this.HandleMessage;
            worklistener.onError += listener_onError;
            worklistener.connect();
        }
        public void Moniter()
        {
            while (true)
            {
                foreach (var taskType in taskExcuters.Keys)
                {
                    if (!taskExcuters[taskType].IsBusy())
                    {
                        this.Log().Info(string.Format("任务服务:{0}空闲，卸载", taskType));
                         Loader.Instance.UnLoad(taskType);
                    }
                    this.Log().Info(taskExcuters[taskType].GetStatus());
                }

                Thread.Sleep(1000 * 60);
            }
        }
        internal void Pause()
        {
            this.Log().Info("暂停");
            if (this.worklistener != null)
                this.worklistener.Pause();
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

        internal void Resume()
        {
            this.Log().Info("恢复");
            if (this.worklistener != null)
                this.worklistener.Resume();
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

                RemoteLoader rl =  Loader.Instance.GetRemoteLoader(task.code);

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
                this.Start();
            }
            else if (cmd == "reload")
            {
                this.Pause();
                //this.worklistener.Close();
                //this.workThread.Interrupt();
                this.Log().Info("重新初始化服务列表");
                this.InitTaskList();

                this.Resume();
            }
            else if (cmd == "pause")
            {
                this.Pause();
            }
            else if (cmd == "resume")
            {
                this.Resume();
            }
            else if (cmd == "clear")
            {
                this.Clear();
            }
            else if (cmd == "upload")
            {
                this.Pause();
                this.Clear();

                var assembly = cmdMsg.Get("assembly");
                var type = cmdMsg.Get("type");
                Loader.Instance.CreateTaskAssembly("T0001", cmdMsg.attachment);

                UnLoadExcuter();

                this.Resume();
            }
            else if (cmd == "init")
            {
                this.Pause();
                this.Clear();

                var code = cmdMsg.Get("task_code");
                Loader.Instance.CreateTaskAssembly(code, cmdMsg.attachment);

                UnLoadExcuter();

                this.Resume();
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
        }
    }
}
