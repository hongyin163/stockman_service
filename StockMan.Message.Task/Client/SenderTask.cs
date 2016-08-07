using StockMan.Message.Task.Biz;
using StockMan.Message.Task.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.Message.Model;
using System.Threading;
using Newtonsoft.Json;

namespace StockMan.Message.Task.Client
{
    public class SenderTask : IRunable, IMessageSender
    {
        private ITask task = null;
        private MessageClient client = null;
        public event Action<string> onComplete;
        public SenderTask(ITask task)
        {

            this.task = task;
        }

        public void Run(object state)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();
            using (this.client = new MessageClient())
            {
                try
                {
                    this.Log().Info("任务发送消息开始：" + this.task.GetCode());

                    this.task.Send(this);

                    if (this.onComplete != null)
                        this.onComplete(this.task.GetCode());

                }
                catch (Exception ex)
                {
                    Console.WriteLine("发送异常：" + ex.Message);
                }
                this.Log().Info("任务完成:" + this.task.GetCode());
            }
        }


        public void Send(string message)
        {
            Thread.Sleep(100);
            TaskMessage msg = new TaskMessage
            {
                code = this.task.GetCode()+"_"+ message.GetHashCode().ToString(),
                task_code = this.task.GetCode(),
                description = "",
                values = message
            };
            this.client.Send(msg);
            log4net.LogManager.GetLogger(this.GetType()).Info("发送消息：" + msg.values);
        }
    }
}
