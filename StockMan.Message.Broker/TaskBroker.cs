using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Devices;

namespace StockMan.Message.Broker
{
    public class TaskBroker : IDisposable
    {
        private NetMQContext context;
        private NetMQSocket frontend;
        private NetMQSocket backend;
        NetMQ.Proxy proxy;

        public void Initialize()
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();


            context = NetMQContext.Create();
            frontend = context.CreateRouterSocket();
            backend = context.CreateDealerSocket();
            var frontAdress = ConfigurationManager.AppSettings["task_frontendBindAddress"];
            var bakdendAdress = ConfigurationManager.AppSettings["task_backendBindAddress"];
            frontend.Bind(frontAdress);
            backend.Bind(bakdendAdress);
            //frontend.ReceiveReady += frontend_ReceiveReady;
            //backend.SendReady += backend_SendReady;
            proxy = new Proxy(frontend, backend, null);
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
            if (proxy != null)
            {
                this.Log().Info("启动ZeroMq队列服务");
                proxy.Start();
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
