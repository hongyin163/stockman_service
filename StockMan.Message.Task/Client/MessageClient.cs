using NetMQ;
using Newtonsoft.Json;
using StockMan.Message.Model;
using StockMan.Message.Task.Biz;
using StockMan.Message.Task.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockMan.Message.Task
{
    public class MessageClient : IDisposable
    {
        //private NetMQ.NetMQContext context = null;
        private NetMQ.NetMQSocket requester = null;
        public MessageClient()
        {
            init();
        }

        public void Dispose()
        {
            if (requester != null)
            {
                requester.Close();
                requester.Dispose();
            }
            //if (context != null)
            //    context.Dispose();
        }

        public void Send(TaskMessage message)
        {
            if (requester == null)
                init();

            //NetMQMessage msg = new NetMQMessage();
            //msg.Push();
            byte[] data = SerializeHelper.BinarySerialize(message);
            requester.SendFrame(data);
        }

        private void init()
        {
            //context = NetMQContext.Create();
            requester = new NetMQ.Sockets.PushSocket();// context.CreateSocket(NetMQ.zmq.ZmqSocketType.Push);
            requester.Connect(System.Configuration.ConfigurationManager.AppSettings["broker"]);
        }
    }
}
