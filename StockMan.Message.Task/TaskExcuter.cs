using StockMan.Message.DataAccess;
using StockMan.Message.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockMan.Message.Task
{
    public class TaskExcuter : MarshalByRefObject
    {
        private Thread workThread = null;
        private Queue<TaskMessage> messageQueue;
        private ITask taskServie;
        private bool running = true;
        private DateTime lastExcuteTime = DateTime.Now;
        public TimeSpan freeTime = TimeSpan.FromMinutes(0);
        private int waitMinutes = 1;
        public TaskExcuterStatus Status
        {
            get;
            set;
        }
        protected Queue<TaskMessage> MessageQueue
        {
            get
            {
                if (this.messageQueue == null)
                    this.messageQueue = new Queue<TaskMessage>();
                return this.messageQueue;
            }
        }

        public void Post(TaskMessage message)
        {
            this.MessageQueue.Enqueue(message);
        }

        public void Load(string assembleName, string typeName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath, assembleName + ".dll");

            if (!File.Exists(path))
                throw new Exception(string.Format("程序集文件不存在:{0},{1}", assembleName, typeName));

            var asseblyBytes = File.ReadAllBytes(path);

            Assembly assebly = Assembly.Load(asseblyBytes);
            ITask taskInstance = assebly.CreateInstance(typeName) as ITask;
            if (taskInstance != null)
            {
                taskServie = taskInstance;
            }
            else
            {
                throw new Exception(string.Format("任务执行器创建失败:程序集{0}：类型：{1}", assembleName, typeName));
            }
        }
        public void Start()
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();
            if (workThread != null)
            {
                workThread.Abort();
            }
            this.running = true;
            workThread = new Thread(Run);
            workThread.Start();
        }
        public void Stop()
        {
            this.Log().Info("任务停止:" + this.taskServie.GetCode());
            this.running = false;
        }
        public void Clear()
        {
            lock (this.MessageQueue)
            {
                this.messageQueue.Clear();
            }
        }
        public void Run()
        {
            while (running)
            {
                TaskMessage msg = null;
                lock (this.MessageQueue)
                {
                    if (this.MessageQueue.Count <= 0)
                    {
                        this.Status = TaskExcuterStatus.Wait;
                        Thread.Sleep(500);
                        freeTime = DateTime.Now - lastExcuteTime;
                        if (freeTime > TimeSpan.FromMinutes(waitMinutes))
                        {
                            this.Stop();
                        }
                        continue;
                    }
                    msg = this.MessageQueue.Dequeue();
                }
                try
                {
                    this.Status = TaskExcuterStatus.Running;
                    lastExcuteTime = DateTime.Now;
                    freeTime = new TimeSpan(0);
                    this.Log().Info(string.Format("开始处理消息：code:{0},description:{1}", msg.code, msg.description));
                    //updateMessageStatus(msg, MessageStatus.Running);
                    this.taskServie.Excute(msg.values);
                    //updateMessageStatus(msg, MessageStatus.Success);
                    this.Log().Info(string.Format("消息处理成功：code:{0},description:{1}", msg.code, msg.description));

                }
                catch (Exception ex)
                {
                    //updateMessageStatus(msg, MessageStatus.Failed);
                    this.Log().Error(string.Format("消息处理失败：code:{0},description:{1},Exception:{2}", msg.code, msg.description, ex.Message));
                }
            }
            this.Log().Info("任务线程停止:" + this.taskServie.GetCode());
            this.Status = TaskExcuterStatus.Stop;
        }

        public bool IsBusy()
        {
            if (this.MessageQueue.Count > 0 && this.running)
                return true;

            if (this.Status == TaskExcuterStatus.Stop)
                return false;

            return true;
        }

        public string GetStatus()
        {
            string msg = "是否运行{0},消息剩余:{1},状态:{2}";
            return string.Format(msg, this.running, this.MessageQueue.Count, this.Status.ToString());
        }

        private void updateMessageStatus(TaskMessage msg, MessageStatus status)
        {
            using (var entity = new messageEntities())
            {
                var m = entity.mq_message.FirstOrDefault(p => p.code == msg.code);
                if (m != null)
                {
                    m.status = (int)status;
                    entity.SaveChanges();
                }
            }
        }
    }
}
