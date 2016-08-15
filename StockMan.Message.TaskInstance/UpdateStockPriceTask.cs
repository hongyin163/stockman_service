using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.Message.Task.Interface;
using StockMan.Service.Rds;
using StockMan.Service.Interface.Rds;
using StockMan.EntityModel;
using StockMan.Jobs.Stock.tencent;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using data = StockMan.EntityModel;

using StockMan.Message.Model;
using StockMan.Jobs.Biz.Model;
using System.Threading;
using Newtonsoft.Json;
using StockMan.Jobs.Stock;

namespace StockMan.Message.TaskInstance
{
    /// <summary>
    /// 更新股票价格
    /// </summary>
    class UpdateStockPriceTask : Message.Task.ITask
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();
        IStockSync sync = new StockSync_Tencent();
        public void Excute(string message)
        {
            this.Log().Info("处理消息");
            var log = this.Log();
            var msg = JsonConvert.DeserializeObject<PriceUpdate>(message);

            var StartDate = msg.StartDate;
            if (!string.IsNullOrEmpty(msg.StockCode))
            {
                log.Info(string.Format("导入日线开始，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), msg.StockCode));
                ImportDay(msg.StockCode, StartDate);
                log.Info(string.Format("导入周线开始，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), msg.StockCode));
                ImportWeek(msg.StockCode, StartDate);
                log.Info(string.Format("导入月线开始，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), msg.StockCode));
                ImportMonth(msg.StockCode, StartDate);
                log.Info(string.Format("导入结束，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), msg.StockCode));
            }
            else if (!string.IsNullOrEmpty(msg.CateCode))
            {

                IList<data.stock> stockList = stockService.GetStockByCategory(msg.CateCode);
                ImportTodayPrice(stockList);
            }
        }

        public string GetCode()
        {
            return "T0002";
        }

        public void Send(IMessageSender sender)
        {
            this.Log().Info("开始计算个股形态");

            DateTime StartDate0 = stockService.GetLatestDate(ObjectType.Stock, "0601988");
            DateTime StartDate1 = stockService.GetLatestDate(ObjectType.Stock, "0601318");

            var StartDate = StartDate0.CompareTo(StartDate1) >= 0 ? StartDate0 : StartDate1;

            IList<stockcategory> cateList = cateService.FindAll();
            TechCycle[] cycleList = new TechCycle[] { TechCycle.day, TechCycle.week, TechCycle.month };
            var log = this.Log();

            foreach (var category in cateList)
            {
                if (StartDate.ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd"))
                {
                    log.Info(string.Format("导入今天日线数据，分类:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), category.name));

                    var task1 = new PriceUpdate
                    {
                        StartDate = StartDate,
                        CateCode = category.code
                    };
                    sender.Send(JsonConvert.SerializeObject(task1));
                    //ImportTodayPrice(stockList);
                }
                else
                {
                    IList<data.stock> stockList = stockService.GetStockByCategory(category.code);

                    foreach (var stock in stockList)
                    {
                        var task1 = new PriceUpdate
                        {
                            StartDate = StartDate,
                            StockCode = stock.code
                        };
                        sender.Send(JsonConvert.SerializeObject(task1));
                        //log.Info(string.Format("导入日线开始，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), stock.name));
                        //ImportDay(stock, StartDate);
                        //log.Info(string.Format("导入周线开始，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), stock.name));
                        //ImportWeek(stock, StartDate);
                        //log.Info(string.Format("导入月线开始，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), stock.name));
                        //ImportMonth(stock, StartDate);
                        //log.Info(string.Format("导入结束，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), stock.name));
                    }
                }

            }
        }



        private bool ImportDay(string stockCode, DateTime startDate)
        {
            IList<data.PriceInfo> spday = sync.GetPriceByDay(stockCode);

            var list = spday.Where(p => p.date >= startDate.AddDays(-1)).OrderBy(p => p.date).ToList();

            for (int i = 1; i < list.Count(); i++)
            {
                list[i].yestclose = list[i - 1].price;
                list[i].updown = list[i].price - list[i].yestclose;
                list[i].percent = list[i].updown / list[i].yestclose;
            }
            if (list.Count > 0)
                list.RemoveAt(0);
            stockService.AddPriceByDay<data.data_stock_day_latest>(list);
            this.Log().Info("日线：" + stockCode);
            return true;

        }

        private bool ImportMonth(string stockCode, DateTime startDate)
        {

            IList<data.PriceInfo> spmonth = sync.GetPriceByMonth(stockCode);

            var list = spmonth.Where(p => p.date >= startDate.AddDays(-30)).OrderBy(p => p.date).ToList();

            for (int i = 1; i < list.Count(); i++)
            {
                list[i].yestclose = list[i - 1].price;
                list[i].updown = list[i].price - list[i].yestclose;
                list[i].percent = list[i].updown / list[i].yestclose;
            }
            if (list.Count > 0)
                list.RemoveAt(0);
            stockService.AddPriceByMonth<data.data_stock_month_latest>(list);
            this.Log().Info("月线：" + stockCode);
            return true;

        }

        private bool ImportWeek(string stockCode, DateTime startDate)
        {
            IList<data.PriceInfo> spweek = sync.GetPriceByWeek(stockCode);

            var list = spweek.Where(p => p.date >= startDate.AddDays(-7)).OrderBy(p => p.date).ToList();

            for (int i = 1; i < list.Count(); i++)
            {
                list[i].yestclose = list[i - 1].price;
                list[i].updown = list[i].price - list[i].yestclose;
                list[i].percent = list[i].updown / list[i].yestclose;
            }
            if (list.Count > 0)
                list.RemoveAt(0);
            stockService.AddPriceByWeek<data.data_stock_week_latest>(list);
            this.Log().Info("周线：" + stockCode);
            return true;

        }
        private void ImportTodayPrice(IList<data.stock> stockList)
        {
            int batchNum = 40;
            for (int i = 0; i < stockList.Count; i += batchNum)
            {
                IList<data.StockInfo> spList = sync.GetPrice(stockList.Skip(i).Take(batchNum).ToList());

                IList<data.PriceInfo> spiList = spList.Select(p => new data.PriceInfo
                {
                    code = p.stock_code,
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
                stockService.UpdateStockPrice(spList);
                stockService.AddPriceByDay<data.data_stock_day_latest>(spiList);
                stockService.AddPriceByWeek<data.data_stock_week_latest>(spiList);
                stockService.AddPriceByMonth<data.data_stock_month_latest>(spiList);
            }
        }

    }

    public class PriceUpdate
    {
        public DateTime StartDate { get; set; }
        public string StockCode { get; set; }
        public string CateCode { get; set; }
    }
}
