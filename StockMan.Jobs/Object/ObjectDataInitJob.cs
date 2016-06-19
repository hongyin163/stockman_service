using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.zmq;
using Newtonsoft.Json;
using Quartz;
using StockMan.Jobs.Biz.Model;
using StockMan.Jobs.Object.tencent;
using StockMan.Jobs.Stock.tencent;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using data = StockMan.EntityModel;
using StockMan.Common;
using StockMan.Jobs.Biz;
using StockMan.EntityModel;

namespace StockMan.Jobs.Object
{
    /// <summary>
    /// 初始化行业指数，从历史数据
    /// </summary>
    public class ObjectDataInitJob : IJob
    {
        ICustomObjectService objService = new CustomObjectService();
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            //init();

            IObjectSync sync = new OjbectSync_Tencent();
            //获取所有大盘指标的名称和值，
            var objList = sync.GetAllObjects();

            //存储到数据库
            foreach (var obj in objList)
            {
                this.Log().Info("添加大盘指标：" + obj.name);
                if (objService.Find(obj.code) == null)
                    objService.Add(obj);
            }

            //遍历大盘指标，获取每个指标的日周月数据，存储到数据库。
            foreach (var obj in objList)
            {
                this.Log().Info("初始化大盘指标" + obj.name + "的日线数据：");
                var dayList = sync.GetPriceByDay(obj);
                Complete(dayList);
                objService.AddPriceByDay<data.data_object_day_latest>(dayList,false);

                //Send(obj.code, data.TechCycle.Day);


                this.Log().Info("初始化大盘指标" + obj.name + "的周线数据：");
                var weekList = sync.GetPriceByWeek(obj);
                Complete(weekList);
                objService.AddPriceByWeek<data.data_object_week_latest>(weekList,false);

                //Send(obj.code, data.TechCycle.Week);

                this.Log().Info("初始化大盘指标" + obj.name + "的月线数据：");
                var monthList = sync.GetPriceByMonth(obj);
                Complete(monthList);
                objService.AddPriceByMonth<data.data_object_month_latest>(monthList,false);

                //Send(obj.code, data.TechCycle.Month);
            }

            this.Log().Info("完成初始化大盘指标线数据！");
        }

        private static void Complete(IList<data.PriceInfo> dayList)
        {
            for (int i = 0; i < dayList.Count(); i++)
            {
                if (i == 0)
                {
                    dayList[i].yestclose = dayList[i].price;
                }
                else
                {
                    dayList[i].yestclose = dayList[i - 1].price;
                }

                dayList[i].updown = dayList[i].price - dayList[i].yestclose;
                dayList[i].percent = dayList[i].updown / dayList[i].yestclose;
            }
        }

        private NetMQContext _context;
        private NetMQSocket _socket;

        private void init()
        {
            _context = NetMQContext.Create();

            _socket = _context.CreateSocket(ZmqSocketType.Req);

            _socket.Connect("tcp://localhost:5559");
        }

        private void Send(string objCode, data.TechCycle cycle)
        {
            string msg = JsonConvert.SerializeObject(new IndexTask
            {
                type = ObjectType.Object,
                code = objCode,
                cycle = cycle
            });
            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
            _socket.Send(msg);
            this.Log().Info("发送:{" + msg + "}");
            string message = _socket.ReceiveString();
            this.Log().Info("接收:" + message);
        }

    }
}
