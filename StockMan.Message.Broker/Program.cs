using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using System.Configuration;
using NetMQ.Devices;
namespace StockMan.Message.Broker
{
    class Program
    {
        static void Main(string[] args)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();


            Task t1 = new TaskFactory().StartNew(() =>
            {
                TaskBroker server = new TaskBroker();
                server.Initialize();
                server.Start();
            });

            Task t2 = new TaskFactory().StartNew(() =>
            {
                ControlBroker server = new ControlBroker();
                server.Initialize();
                server.Start();
            });
            t1.Wait();
            t2.Wait();
            //NetMQContext context = NetMQContext.Create();
            //NetMQSocket frontend = context.CreateRouterSocket();
            //NetMQSocket backend = context.CreateDealerSocket();
            //var frontAdress = ConfigurationManager.AppSettings["frontendBindAddress"];
            //var bakdendAdress = ConfigurationManager.AppSettings["backendBindAddress"];
            //frontend.Bind(frontAdress);
            //backend.Bind(bakdendAdress);
            ////frontend.ReceiveReady += frontend_ReceiveReady;
            ////backend.SendReady += backend_SendReady;
            //Proxy proxy = new Proxy(frontend, backend, null);
            //proxy.Start();

            //Poller _poller = new NetMQ.Poller();
            //QueueDevice _queue = new NetMQ.Devices.QueueDevice(context, _poller, frontAdress, bakdendAdress);
            //_queue.Start();
            //_poller.Start();
        }


    }
}
