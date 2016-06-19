using Quartz.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace StockMan.Jobs.WinService
{
    class Program
    {
        static void Main(string[] args)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();
          
            Host host = HostFactory.New(x =>
            {
                x.Service<IQuartzServer>(s =>
                {

                    s.ConstructUsing(builder =>
                    {
                        QuartzServer server = new QuartzServer();
                        server.Initialize();
                        return server;
                    });
                    s.WhenStarted(server => server.Start());
                    s.WhenPaused(server => server.Pause());
                    s.WhenContinued(server => server.Resume());
                    s.WhenStopped(server => server.Stop());
                });

                //x.RunAs("administrator","password.1");
                x.RunAsLocalSystem();
                x.SetDescription(Configuration.ServiceDescription);
                x.SetDisplayName(Configuration.ServiceDisplayName);
                x.SetServiceName(Configuration.ServiceName);

            });

            host.Run();
        }
    }
}
