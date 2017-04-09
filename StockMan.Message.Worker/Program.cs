using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using System.Threading;
using System.Configuration;
namespace StockMan.Message.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            Worker worker = new Worker();
            worker.Start();

            Console.Read();        
        }

    }

}
