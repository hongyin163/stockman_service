using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using System.Threading;
using StockMan.Message.Task.Biz;
using Quartz;
using Quartz.Impl;
using StockMan.Message.Task.Control;
namespace StockMan.Message.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            var main = new ClientThread();
            //main.Start();

            ControlThread thread = new ControlThread("main", main);
            thread.Start();

            Console.ReadLine();
        } 
    }
}
