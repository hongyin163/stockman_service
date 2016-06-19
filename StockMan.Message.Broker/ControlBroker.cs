using NetMQ;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockMan.Message.Broker
{
    public class ControlBroker
    {
        private NetMQContext context;
        private NetMQSocket frontend;
        private NetMQSocket backend;
        NetMQ.Proxy proxy;

        public void Initialize()
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();


            context = NetMQContext.Create();
            frontend = context.CreateResponseSocket();
            backend = context.CreatePublisherSocket();
            var frontAdress = ConfigurationManager.AppSettings["ctrl_frontendBindAddress"];
            var bakdendAdress = ConfigurationManager.AppSettings["ctrl_backendBindAddress"];
            frontend.Bind(frontAdress);
            backend.Bind(bakdendAdress);
            
            this.Log().Info("初始化ZeroMq完成");
        }
        private void backend_SendReady(object sender, NetMQSocketEventArgs e)
        {
            this.Log().Info("发送消息" + e.Socket.ReceiveString());
        }

        private void frontend_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            this.Log().Info("接收消息" + e.Socket.ReceiveString());
        }
        public void Start()
        {
            while (true)
            {
                var msg = frontend.ReceiveMessage();
                backend.SendMessage(msg);
                frontend.Send("Success");
            }
        }

        public void Dispose()
        {
            if (frontend != null)
            {
                frontend.Close();
                frontend.Dispose();
            }
            if (backend != null)
            {
                backend.Close();
                backend.Dispose();
            }
            if (context != null) context.Dispose();

        }
    }
}
