using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.zmq;
using StockMan.Jobs.Biz;
using Topshelf;
using StockMan.Jobs.Biz.Model;
using Newtonsoft.Json;

namespace StockMan.TechJob
{
    class Program
    {
        public static void Main(string[] args)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            TechDataJob server = new TechDataJob();
            server.Initialize();
            server.Start();

            //string comStr =
            // "0：更新技术数据\r\n" +
            // "1：更新技术形态\r\n" +
            // "exit：退出\r\n";

            //bool run = true;
            //while (run)
            //{
            //    Console.Write(comStr);
            //    var p = Console.ReadLine();
            //    var startTime = DateTime.Now;
            //    p = "0";
            //    switch (p)
            //    {
            //        case "0":
            //            TechDataJob server = new TechDataJob();
            //            server.Initialize();
            //            server.Start();
            //            break;
            //        case "1":
            //            TechDataJob server1 = new TechDataJob();
            //            server1.Initialize();
            //            server1.Start();
            //            break;
            //        default:

            //            break;

            //    }

            //}




            //Host host = HostFactory.New(x =>
            //{
            //    x.Service<IWinService>(s =>
            //    {

            //        s.ConstructUsing(builder =>
            //        {
            //            TechServer server = new TechServer();
            //            server.Initialize();
            //            return server;
            //        });
            //        s.WhenStarted(server => server.Start());
            //        s.WhenPaused(server => server.Pause());
            //        s.WhenContinued(server => server.Resume());
            //        s.WhenStopped(server => server.Stop());
            //    });

            //    //x.RunAs("administrator","password.1");
            //    x.RunAsLocalSystem();
            //    x.SetDescription("技术指标计算服务");
            //    x.SetDisplayName("技术指标计算服务");
            //    x.SetServiceName("StockMan.Jobs.Techjob");
            //});

            //host.Run();



        }


    }
}
