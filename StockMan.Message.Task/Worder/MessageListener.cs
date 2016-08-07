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
        //private Mutex mPause = new Mutex(false, null);
        private string endpoint;
        public void connect()
        {
            string name = AppDomain.CurrentDomain.FriendlyName;
            endpoint = ConfigurationManager.AppSettings["broker"];
            using (responder =new PullSocket())
            {
                responder.Connect(endpoint);
                while (this.running)
                {
                    try
                    {
                        NetMQMessage msgList = responder.ReceiveMultipartMessage();

                        msgList.Pop();

                        foreach (var msg in msgList)
                        {
                            string result = msg.ConvertToString();

                            int size = msg.MessageSize;
                            if (size <= 5)
                            {
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
                    catch(Exception ex)
                    {
                        if (onError != null)
                            this.onError(String.Format("异常:" + ex.Message));
                        break;
                    }
                }
            }
        }

        public void Close()
        {
            this.running = false;

            if (responder != null)
            {
                try
                {
                    responder.Disconnect(endpoint);
                }
                catch(Exception ex)
                {

                }
                finally
                {
                    responder.Dispose();
                }
               
            }

        }

        public void Dispose()
        {
            if (responder != null)
            {
                responder.Dispose();
            }
            GC.Collect();

        }
    }
}
