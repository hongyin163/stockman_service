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

namespace StockMan.Jobs.Broker
{
    class Program
    {
        public static void Main(string[] args)
        {
            Broker server = new Broker();
            server.Initialize();
            server.Start();
            //Host host = HostFactory.New(x =>
            //{
            //    x.Service<IWinService>(s =>
            //    {

            //        s.ConstructUsing(builder =>
            //        {
            //            Broker server = new Broker();
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
            //    x.SetDescription("消息代理服务器");
            //    x.SetDisplayName("消息代理服务器");
            //    x.SetServiceName("StockMan.Jobs.Broker");
            //});

            //host.Run();


            //using (var context = NetMQContext.Create())
            //{
            //    using (var frontend = context.CreateSocket(ZmqSocketType.Router))
            //    using (var backend = context.CreateSocket(ZmqSocketType.Dealer))
            //    {
            //        frontend.Bind("tcp://*:5559");
            //        backend.Bind("tcp://*:5560");

            //        frontend.ReceiveReady += (sender, e) => FrontendPollInHandler(e.Socket, backend);
            //        backend.ReceiveReady += (sender, e) => BackendPollInHandler(e.Socket, frontend);

            //        var poller = new NetMQ.Poller(frontend, backend);

            //        //while (true)
            //        //{
            //        //    poller.Start();
            //        //}
            //        poller.Start();
            //    }
            //}


        }
        private static void FrontendPollInHandler(NetMQSocket frontend, NetMQSocket backend)
        {
            RelayMessage(frontend, backend);
        }

        private static void BackendPollInHandler(NetMQSocket backend, NetMQSocket frontend)
        {
            RelayMessage(backend, frontend);
        }

        private static void RelayMessage(NetMQSocket source, NetMQSocket destination)
        {
            bool hasMore = true;
            while (hasMore)
            {
                // side effect warning!
                // note! that this uses Receive mode that gets a byte[], the router c# implementation
                // doesnt work if you get a string message instead of the byte[] i would prefer the solution thats commented.
                // but the router doesnt seem to be able to handle the response back to the client
                //string message = source.Receive(Encoding.Unicode);
                //hasMore = source.RcvMore;
                //destination.Send(message, Encoding.Unicode, hasMore ? SendRecvOpt.SNDMORE : SendRecvOpt.NONE);

                //int bytesReceived;
                byte[] message = source.Receive(true, out hasMore);
                //hasMore = source.ReceiveMore;
                destination.Send(message, message.Length, hasMore ? SendReceiveOptions.SendMore : SendReceiveOptions.None);
            }
        }


    }
}
