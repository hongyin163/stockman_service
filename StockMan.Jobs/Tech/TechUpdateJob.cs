using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ.Sockets;
using NetMQ.zmq;
using Newtonsoft.Json;
using Quartz;
using StockMan.EntityModel;
using StockMan.Jobs.Biz.Model;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using NetMQ;
namespace StockMan.Jobs.Tech
{
    public class TechUpdateJob : IJob
    {

        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            using (NetMQContext nmContext = NetMQContext.Create())
            using (NetMQSocket socket = nmContext.CreateSocket(ZmqSocketType.Rep))
            {
                socket.Connect("tcp://localhost:5566");
                while (true)
                {
                    var message = socket.ReceiveString();

                    var taskMesg = JsonConvert.DeserializeObject<IndexTask>(message);

                    var objType = taskMesg.type;
                    var objCode = taskMesg.code;
                    var cycleType = taskMesg.cycle;// (TechCycle)Enum.Parse(typeof(TechCycle), taskMesg.cycle, true);
                  
                    IIndexService indexService = new IndexService();
                    //获取数据
                    var priceList = indexService.GetObjectData(objType.ToString(), cycleType, objCode);

                    //计算指数结果                    
                    //存储指数到MangoDB
                    //更新状态数据到MangoDB
                    var tech = new TechCalculate(objType, cycleType, objCode, priceList);
                    tech.Run();

                }
            }
        }
    }
}
