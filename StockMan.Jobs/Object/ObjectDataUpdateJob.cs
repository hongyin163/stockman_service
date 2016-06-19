using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.zmq;
using Newtonsoft.Json;
using Quartz;
using data = StockMan.EntityModel;
using StockMan.Jobs.Biz.Model;
using StockMan.Jobs.Object.tencent;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;

namespace StockMan.Jobs.Object
{
    public class ObjectDataUpdateJob : IJob
    {
        ICustomObjectService objService = new CustomObjectService();

        public DateTime _startDate = DateTime.Now;
        public DateTime StartDate { get { return _startDate; } set { _startDate = value; } }
        IObjectSync sync = null;
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();
            sync = new OjbectSync_Tencent();
            //获取所有大盘指标的名称和值，
            var objList = objService.FindAll();


            //遍历大盘指标，获取每个指标的日周月数据，存储到数据库。


            if (StartDate.ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd"))
            {
                //log.Info(string.Format("导入今天日线数据，分类:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), category.name));

                ImportTodayPrice(objList);
            }
            else
            {
                foreach (var obj in objList)
                {
                    this.Log().Info("初始化大盘指标" + obj.name + "的日线数据：");
                    var dayList = sync.GetPriceByDay(obj);
                    if (dayList.Count > 0)
                    {
                        dayList = dayList.Where(p => p.date >= StartDate.AddDays(-1)).ToList();
                        Complete(dayList);
                        dayList.RemoveAt(0);
                        objService.AddPriceByDay<data.data_object_day_latest>(dayList);
                    }
                    //Send(obj.code, data.TechCycle.Day);


                    this.Log().Info("初始化大盘指标" + obj.name + "的周线数据：");
                    var weekList = sync.GetPriceByWeek(obj);
                    if (weekList.Count > 0)
                    {
                        weekList = weekList.Where(p => p.date >= StartDate.AddDays(-1)).ToList();
                        Complete(weekList);
                        weekList.RemoveAt(0);
                        objService.AddPriceByWeek<data.data_object_week_latest>(weekList);
                    }
                    //Send(obj.code, data.TechCycle.Week);

                    this.Log().Info("初始化大盘指标" + obj.name + "的月线数据：");
                    var monthList = sync.GetPriceByMonth(obj);
                    if (monthList.Count > 0)
                    {
                        monthList = monthList.Where(p => p.date >= StartDate.AddDays(-1)).ToList();
                        Complete(monthList);
                        monthList.RemoveAt(0);
                        objService.AddPriceByMonth<data.data_object_month_latest>(monthList);
                    }
                    //Send(obj.code, data.TechCycle.Month);
                }

            }



            //using (var mqContext = NetMQContext.Create())
            //{
            //    using (var socket = mqContext.CreateSocket(ZmqSocketType.Req))
            //    {
            //        socket.Connect("tcp://localhost:5559");

            //        foreach (var priceInfo in priceList)
            //        {
            //            socket.Send(JsonConvert.SerializeObject(new IndexTask
            //            {
            //                type = "Object",
            //                code = priceInfo.code,
            //                cycle = TechCycle.Day.ToString()
            //            }));
            //            socket.Send(JsonConvert.SerializeObject(new IndexTask
            //            {
            //                type = "Object",
            //                code = priceInfo.code,
            //                cycle = TechCycle.Week.ToString()
            //            }));
            //            socket.Send(JsonConvert.SerializeObject(new IndexTask
            //            {
            //                type = "Object",
            //                code = priceInfo.code,
            //                cycle = TechCycle.Month.ToString()
            //            }));
            //        }
            //    }
            //}



        }
        private static void Complete(IList<data.PriceInfo> dayList)
        {
            for (int i = 1; i < dayList.Count(); i++)
            {
                dayList[i].yestclose = dayList[i - 1].price;
                dayList[i].updown = dayList[i].price - dayList[i].yestclose;
                dayList[i].percent = dayList[i].updown / dayList[i].yestclose;
            }
        }
        private void ImportTodayPrice(IList<data.customobject> objList)
        {
            var objInfoList = sync.GetPrice(objList);

            IList<data.PriceInfo> priceList = objInfoList.Select(p => new data.PriceInfo
            {
                code = p.object_code,
                date = p.date,
                high = p.high,
                low = p.low,
                open = p.open,
                percent = p.percent,
                price = p.price,
                updown = p.updown,
                volume = p.volume,
                yestclose = p.yestclose
            }).ToList();
            objService.UpdateObjectInfo(objInfoList);
            objService.AddPriceByDay<data.data_object_day_latest>(priceList);
            objService.AddPriceByWeek<data.data_object_week_latest>(priceList);
            objService.AddPriceByMonth<data.data_object_month_latest>(priceList);
        }
    }
}
