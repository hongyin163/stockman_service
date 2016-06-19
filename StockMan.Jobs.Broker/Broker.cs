using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Devices;
using StockMan.Jobs.Biz;

namespace StockMan.Jobs.Broker
{
    public class Broker : IWinService
    {
        private NetMQContext _context;

        private QueueDevice _queue;

        private NetMQ.Poller _poller;
        public void Initialize()
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();


            _context = NetMQContext.Create();

            var frontAdress = ConfigurationManager.AppSettings["frontendBindAddress"];
            var bakdendAdress = ConfigurationManager.AppSettings["backendBindAddress"];
            _poller = new NetMQ.Poller();
            _queue = new NetMQ.Devices.QueueDevice(_context, _poller, frontAdress, bakdendAdress);

            this.Log().Info("初始化ZeroMq完成");
        }

        public void Start()
        {
            if (_queue != null && _poller != null)
            {
                this.Log().Info("启动ZeroMq队列服务");
                _queue.Start();
                _poller.Start();
            }
        }

        public void Stop()
        {
            if (_queue != null && _poller != null)
            {
                _queue.Stop();
                _poller.Stop();
            }
        }

        public void Pause()
        {
            if (_queue != null && _poller != null)
            {
                _queue.Stop();
                _poller.Stop();
            }
        }

        public void Resume()
        {
            if (_queue != null && _poller != null)
            {
                _queue.Run();
                _poller.Start();
            }
        }
    }
}
