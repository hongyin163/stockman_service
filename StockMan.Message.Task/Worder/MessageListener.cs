using NetMQ;
using StockMan.Message.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using System.Threading;
using NetMQ.Sockets;
using StockMan.Message.Model;
using StockMan.Message.Task.Biz;
namespace StockMan.Message.Task.Worder
{
    public class MessageListener : IDisposable
    {
        public event Action<TaskMessage> onReceive;
        public event Action<String> onError;
        PullSocket responder = null;
        //NetMQ.NetMQContext context = null;
        private bool running = true;
        private bool pause = false;
        private Mutex mPause = new Mutex(false, null);
        private string endpoint;
        public void connect()
        {
            string name = AppDomain.CurrentDomain.FriendlyName;
            endpoint = ConfigurationManager.AppSettings["broker"];
            //using (context = NetMQContext.Create())
            using (responder =new PullSocket())
            {
                responder.Connect(endpoint);
                while (this.running)
                {
                    mPause.WaitOne();
                    try
                    {
                        NetMQMessage msgList = responder.ReceiveMultipartMessage();
                        //msgList.Pop();
                        //byte[] data = responder.ReceiveFrameBytes();
                        //if (data.Length <= 5)
                        //{
                        //    Console.WriteLine("消息小于5个字节");
                        //    continue;
                        //}


                        //TaskMessage msg = (TaskMessage)SerializeHelper.BinaryDeserialize(data);

                        //if (this.onReceive != null)
                        //    this.onReceive(msg);
                        msgList.Pop();

                        foreach (var msg in msgList)
                        {

                            string result = msg.ConvertToString();
                            //if (string.IsNullOrEmpty(result))
                            //{
                            //    if (onError != null)
                            //        this.onError("消息为空");
                            //    continue;
                            //}

                            int size = msg.MessageSize;
                            if (size <= 5)
                            {
                                //if (onError != null)
                                //    this.onError("消息格式错误："+ result);
                                continue;
                            }
                            try
                            {
                                var strMsg = JsonConvert.DeserializeObject<TaskMessage>(result);
                                if (this.onReceive != null)
                                    this.onReceive(strMsg);
                            }
                            catch (Exception ex)
                            {
                                if (onError != null)
                                    this.onError(String.Format("消息:{0},反序列化异常:{1}", result, ex.Message));
                            }
                        }
                    }
                    catch (NetMQ.TerminatingException te)
                    {
                        if (onError != null)
                            this.onError(String.Format("连接终止异常:" + te.Message));
                        break;
                    }

                    mPause.ReleaseMutex();
                }
            }
        }

        public void Close()
        {
            this.running = false;

            //if (responder != null)
            //{
            //    responder.Disconnect(endpoint);              
            //}

        }
        public void Pause()
        {
            mPause.WaitOne();
            //Thread.
            //var task = System.Threading.Tasks.Task.Factory.StartNew(() => { });
            //task.Start();


        }
        public void Dispose()
        {
            if (responder != null)
            {
                responder.Dispose();
            }
            //if (context != null)
            //    context.Dispose();
            GC.Collect();

        }

        internal void Resume()
        {
            mPause.ReleaseMutex();
            //Thread.CurrentThread.Resume();
        }
    }
}
