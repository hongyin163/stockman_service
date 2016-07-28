using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Devices;
using NetMQ.Sockets;
namespace StockMan.Message.Broker
{
    public class TaskBroker : IDisposable
    {
        //private NetMQContext context;
        private NetMQSocket frontend;
        private NetMQSocket backend;
        NetMQ.Proxy proxy;

        public void Initialize()
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            

            //context = NetMQContext.Create();
            frontend = new RouterSocket();// context.CreateRouterSocket();
            backend = new DealerSocket();// context.CreateDealerSocket();
            var frontAdress = ConfigurationManager.AppSettings["task_frontendBindAddress"];
            var bakdendAdress = ConfigurationManager.AppSettings["task_backendBindAddress"];
            frontend.Bind(frontAdress);
            backend.Bind(bakdendAdress);

            var contrlIn = new PushSocket();// context.CreatePushSocket();
            contrlIn.Bind(ConfigurationManager.AppSettings["mon_controlInBindAddress"]);
            //var controlOut=
            //frontend.ReceiveReady += frontend_ReceiveReady;
            //backend.SendReady += backend_SendReady;
            proxy = new Proxy(frontend, backend, null);

        
            this.Log().Info("初始化ZeroMq完成");

            //Monitor();
           
          
        }

        private void ContrlIn_EventReceived(object sender, NetMQ.Monitoring.NetMQMonitorEventArgs e)
        {
            this.Log().Info("EventReceived");
        }

        private void ContrlIn_Accepted(object sender, NetMQ.Monitoring.NetMQMonitorSocketEventArgs e)
        {
            this.Log().Info("Accepted");
        }

        private void ContrlIn_Connected(object sender, NetMQ.Monitoring.NetMQMonitorSocketEventArgs e)
        {
            this.Log().Info("Connected");
        }

        public void Monitor()
        {
            var context = NetMQContext.Create();
            var contrlSub = context.CreateSubscriberSocket();
            contrlSub.Connect(ConfigurationManager.AppSettings["mon_controlInBindAddress"]);
            //while (true)
            //{
            //    string msg = contrlSub.ReceiveFrameString();
            //    this.Log().Info(msg);
            //}
            //var frontAdress = ConfigurationManager.AppSettings["task_frontendBindAddress"];
            //var contrlIn = context.CreateMonitorSocket(frontAdress);
            //contrlIn.Connected += ContrlIn_Connected;
            //contrlIn.Accepted += ContrlIn_Accepted;
            //contrlIn.EventReceived += ContrlIn_EventReceived;
            //contrlIn.Start();
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
            //if (context != null) context.Dispose();

        }
    }
}
