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

            ControlThread thread = new ControlThread("main",main);
            thread.Start();

            Console.ReadLine();
        }

        private void push()
        {
            using (NetMQ.NetMQContext context = NetMQContext.Create())
            using (var requester = context.CreateSocket(NetMQ.zmq.ZmqSocketType.Push))
            {
                requester.Connect("tcp://127.0.0.1:5559");

                for (int n = 0; n < 100; ++n)
                {
                    requester.Send("Hello" + n);

                    //var reply = requester.ReceiveString();
                    Console.WriteLine("Hello");
                    //Thread.Sleep(100);
                    //Console.WriteLine("Hello {0}!", reply);
                }
            }
        }
    }
}
