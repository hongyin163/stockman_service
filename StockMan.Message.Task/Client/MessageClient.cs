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
        }

        public void Send(TaskMessage message)
        {
            if (requester == null)
                init();

            var msg = JsonConvert.SerializeObject(message);
            requester.SendFrame(msg);
        }

        private void init()
        {
            requester = new NetMQ.Sockets.PushSocket();
            requester.Connect(System.Configuration.ConfigurationManager.AppSettings["broker"]);
        }
    }
}
