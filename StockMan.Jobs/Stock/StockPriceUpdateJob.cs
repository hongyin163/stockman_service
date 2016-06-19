using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using StockMan.Jobs.Stock.tencent;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using data = StockMan.EntityModel;
using LoggingExtensions.Logging;

namespace StockMan.Jobs.Stock
{
    /// <summary>
    /// 每天收盘后执行，把数据存储到MYSQL
    /// </summary>
    public class StockPriceUpdateJob : IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();

        IStockSync sync = new StockSync_Tencent();

        DateTime timestamp;
        int batchNum = 40;
        public DateTime _startDate = DateTime.Now;
        public DateTime StartDate { get { return _startDate; } set { _startDate = value; } }
        public string CategoryCode { get; set; }
        ILog log = null;
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            log = log4net.LogManager.GetLogger(this.GetType()).Log();
            IList<data.stockcategory> cateList = null;
            if (string.IsNullOrEmpty(this.CategoryCode) || this.CategoryCode == "-1")
            {
                cateList = cateService.GetCategoryList("tencent");
            }
            else
            {
                string[] cates = this.CategoryCode.Split(',');
                cateList = cateService.GetCategoryList("tencent").Where(p => cates.Contains(p.code)).ToList();
            }
            foreach (var category in cateList)
            {
                IList<data.stock> stockList = stockService.GetStockByCategory(category.code);

                if (StartDate.ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd"))
                {
                    log.Info(string.Format("导入今天日线数据，分类:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), category.name));

                    ImportTodayPrice(stockList);
                }
                else
                {
                    foreach (var stock in stockList)
                    {

                        log.Info(string.Format("导入日线开始，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), stock.name));
                        ImportDay(stock, StartDate);
                        log.Info(string.Format("导入周线开始，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), stock.name));
                        ImportWeek(stock, StartDate);
                        log.Info(string.Format("导入月线开始，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), stock.name));
                        ImportMonth(stock, StartDate);
                        log.Info(string.Format("导入结束，股票:{1}：从日期{0}", StartDate.ToString("yyyy-MM-dd"), stock.name));
                    }
                }

            }
        }
        private bool ImportDay(data.stock stock, DateTime startDate)
        {

            IList<data.PriceInfo> spday = sync.GetPriceByDay(stock);

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
            this.Log().Info("日线：" + stock.code + stock.name);
            return true;

        }

        private bool ImportMonth(data.stock stock, DateTime startDate)
        {

            IList<data.PriceInfo> spmonth = sync.GetPriceByMonth(stock);

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
            this.Log().Info("月线：" + stock.code + stock.name);
            return true;

        }

        private bool ImportWeek(data.stock stock, DateTime startDate)
        {

            IList<data.PriceInfo> spweek = sync.GetPriceByWeek(stock);

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
            this.Log().Info("周线：" + stock.code + stock.name);
            return true;

        }
        private void ImportTodayPrice(IList<data.stock> stockList)
        {

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
}
