using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Devices;
using NetMQ.zmq;
using Newtonsoft.Json;
using StockMan.EntityModel;
using StockMan.Jobs.Biz;
using StockMan.Jobs.Biz.Model;
using StockMan.Jobs.Tech;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
namespace StockMan.TechJob
{
    public class TechStateJob : IWinService
    {
        private NetMQContext _context;
        private NetMQSocket _socket;
        private bool _running = false;
        public void Initialize()
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();
        }

        public void Start()
        {
            this.Log().Info("启动状态计算服务");
            _context = NetMQContext.Create();
            _socket = _context.CreateSocket(ZmqSocketType.Rep);
            _socket.Connect(ConfigurationManager.AppSettings["broker"]);
            _running = true;
            while (_running)
            {
                this.Log().Info("开始接收消息");
                var message = _socket.ReceiveString();
                this.Log().Info("接收消息：{" + message + "}");
                var taskMesg = JsonConvert.DeserializeObject<IndexTask>(message);

                var objType = taskMesg.type;
                var objCode = taskMesg.code;
                var cycleType = taskMesg.cycle;// (TechCycle)Enum.Parse(typeof(TechCycle), taskMesg.cycle, true);

                var tech = new TechCalculate(objType, cycleType, objCode, null);
                tech.CalculateState();

                this.Log().Info("处理完成：" + message);
                _socket.Send("处理完成：{" + message + "}");
            }
        }

        public void Stop()
        {
            _running = false;
            if (_socket != null && _context != null)
            {
                _socket.Close();
                _context.Dispose();

            }
        }

        public void Pause()
        {
            this._running = false;
        }

        public void Resume()
        {
            _running = true;
            this.Start();
        }
    }
}
