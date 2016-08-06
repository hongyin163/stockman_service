using StockMan.Message.DataAccess;
using StockMan.Message.Model;
using StockMan.Message.Task.Biz;
using StockMan.Message.Task.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using thread = System.Threading.Tasks;

namespace StockMan.Message.Task
{
    /// <summary>
    /// 负责管理Task
    /// </summary>
    public class TaskSender : MarshalByRefObject, IMessageSender,IDisposable
    {
        private Thread workThread = null;
        private TaskService taskService = new TaskService();
        private MessageService messageService = new MessageService();
        private MessageClient client = null;
        private Queue<TaskMessage> messageQueue = null;
        private Queue<TaskMessage> MessageQueue
        {
            get
            {
                if (messageQueue == null)
                    messageQueue = new Queue<TaskMessage>();
                return messageQueue;
            }
        }
        public event Action<string> onStop;
        private ITask taskInstance;
        public TaskExcuterStatus Status;
        public TaskSender()
        {
            //this.client = new MessageClient();
        }
        public void Start()
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();
            Status = TaskExcuterStatus.Running;
            //开线程
            workThread = new Thread(Run);
            workThread.Start();
        }

        private void Run()
        {
            this.Log().Info("构造消息开始");
    
            this.Status = TaskExcuterStatus.Running;
            //while (true)
            //{
            //    IList<TaskMessage> msgList = this.taskServie.GetMessage();
            //    if (msgList.Count == 0)
            //        break;

            //    save(msgList);
            //    this.Log().Info("保存消息" + msgList.Count + "条");
            //    Thread.Sleep(100);
            //}
            //用户构建消息发送
            this.taskInstance.Send(this);

            //处理为完成的消息
            //handleMessage();

            this.Status = TaskExcuterStatus.Stop;

        }

        public void Load(string assembleName, string typeName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath, assembleName + ".dll");

            if (!File.Exists(path))
                throw new Exception(string.Format("程序集文件不存在:{0},{1}", assembleName, typeName));

            var asseblyBytes = File.ReadAllBytes(path);

            Assembly assebly = Assembly.Load(asseblyBytes);
            ITask taskIns = assebly.CreateInstance(typeName) as ITask;
            if (taskIns != null)
            {
                taskInstance = taskIns;
            }
            else
            {
                throw new Exception(string.Format("任务执行器创建失败:程序集{0}：类型：{1}", assembleName, typeName));
            }
        }

        public void Send(TaskMessage message)
        {
            //先保存消息
            //this.save(message);

            //发送
            //this.sendMessage(message);

            //更新状态
            //this.updateMessageStatus(message, MessageStatus.Wait);
            lock (this.MessageQueue)
            {
                this.MessageQueue.Enqueue(message);
            }
            this.Log().Info("消息入列:" + message.code);
        }

        private void sendMessage(TaskMessage message)
        {
            this.client.Send(message);
        }

        public IList<TaskMessage> GetMessage()
        {
            IList<TaskMessage> list = new List<TaskMessage>();
            lock (this.MessageQueue)
            {
                if (this.MessageQueue.Count > 0)
                {
                    list = this.MessageQueue.ToList();
                    this.MessageQueue.Clear();
                }
            }
            return list;
        }

        public bool IsBusy()
        {
            return this.Status==TaskExcuterStatus.Running;
        }

        public string GetStatus()
        {
            string msg = "状态:{0}";
            //return string.Format(msg, this.running, this.MessageQueue.Count, this.Status.ToString());
            return string.Format(msg, this.Status.ToString());
        }

        public void Dispose()
        {
            //if (this.client != null) this.client.Dispose();

            if (this.workThread != null)
            {
                try {
                    this.workThread.Abort();
                }
                catch { }               
            }
        }
    }
}
