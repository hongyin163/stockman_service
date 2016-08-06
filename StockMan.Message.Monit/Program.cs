using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.Sockets;
using NetMQ;
using System.Configuration;

namespace StockMan.Message.Monit
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var contrlSub = new PullSocket())
            {
                contrlSub.Connect(ConfigurationManager.AppSettings["mon_controlInBindAddress"]);
                while (true)
                {
                    Console.WriteLine(contrlSub.ReceiveFrameString());
                }
            }
        }
    }
}
