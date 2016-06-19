using NetMQ;
using Newtonsoft.Json;
using StockMan.Message.Model;
using StockMan.Message.Task;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.Task.Control
{
    public class ControlListener : IDisposable
    {
        public event Action<CmdMessage> onReceive;
        public event Action<String> onError;
        NetMQ.Sockets.SubscriberSocket responder = null;
        NetMQ.NetMQContext context = null;
        private string target = string.Empty;
        public ControlListener(string target)
        {
            this.target = target;
        }
        public void connect()
        {
            //string endpoint = "tcp://127.0.0.1:5561";
            //using (NetMQContext context = NetMQContext.Create())
            //using (NetMQ.Sockets.SubscriberSocket responder = context.CreateSubscriberSocket())
            //{
            //    responder.Connect(endpoint);
            //    responder.Subscribe("hello");
            //    while (true)
            //    {
            //        string msg = responder.ReceiveString();

            //        Console.WriteLine(msg);
            //        Thread.Sleep(500);
            //    }
            //}

            string name = AppDomain.CurrentDomain.FriendlyName;
            string endpoint = ConfigurationManager.AppSettings["pubAdress"];
            using (context = NetMQContext.Create())
            using (responder = context.CreateSubscriberSocket())
            {
                responder.Connect(endpoint);
                responder.Subscribe(this.target);
                while (true)
                {
                    NetMQMessage msgList = responder.ReceiveMessage();
                    msgList.Pop();
                    byte[] data = msgList.First.ToByteArray();
                    MemoryStream ms = new MemoryStream(data);
                    BinaryFormatter bf = new BinaryFormatter();
                    CmdMessage cmd = (CmdMessage)bf.Deserialize(ms);
                    ms.Close();
                    if (this.onReceive != null)
                    {
                        this.onReceive(cmd);
                    }
                }

            }
        }

        private void close()
        {
            if (responder != null)
                responder.Close();
            if (context != null)
                context.Dispose();

        }

        public void Dispose()
        {
            this.close();
        }
    }
}
