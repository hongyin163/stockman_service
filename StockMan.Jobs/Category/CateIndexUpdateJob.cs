using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using data = StockMan.EntityModel;
namespace StockMan.Jobs.Category
{
    /// <summary>
    /// 根据股票价格，更新行业指数
    /// </summary>
    public class CateIndexUpdateJob : IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            this.Log().Info("计算开始");
            //获取行业列表
            IList<data.stockcategory> list;

            if (string.IsNullOrEmpty(this.CategoryCode) || this.CategoryCode == "-1")
            {
                list = cateService.GetCategoryList("tencent");
            }
            else
            {
                string[] cates = this.CategoryCode.Split(',');
                list = cateService.GetCategoryList("tencent").Where(p => cates.Contains(p.code)).ToList();
            }


            //遍历列表，获取行业下的股票，计算指数，更新指数

            IList<data.PriceInfo> priceInfoList = new List<data.PriceInfo>();
            foreach (var item in list)
            {

                //IList<data.stock> stocks = stockService.GetStockByCategory(item.code);
                DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                decimal yestclose = GetYestclose(item.code, today);

                //获取基期的值
                int i = -1;
                bool isOk = false;
                decimal baseValue = 0;
                while (!isOk)
                {
                    var priceList = stockService.GetStockPriceByDate(item.code, data.TechCycle.day, DateTime.Now.AddDays(i--));
                    if (priceList == null || priceList.Count == 0)
                        continue;
                    decimal total = 0, baseIndex = 100;
                    foreach (var priceInfo in priceList)
                    {
                        total += priceInfo.price ?? 1 * priceInfo.volume ?? 1;
                    }
                    baseValue = (total * 100) / yestclose;
                    isOk = true;
                }

                //index = (total / baseValue) * 100;


                var priceList0 = stockService.GetStockPriceByDate(item.code, data.TechCycle.day, today);
                decimal vol = 0, turnover = 0;
                decimal total1 = 0;
                foreach (var priceInfo in priceList0)
                {
                    total1 += priceInfo.price ?? 1 * priceInfo.volume ?? 1;
                    vol += priceInfo.volume ?? 1;
                    turnover += priceInfo.turnover ?? 1;
                }

                decimal index = (total1 / baseValue) * 100;

                var percent = (index - yestclose) * 100 / yestclose;
                var info = new data.PriceInfo
                {
                    code = item.code,
                    date = today,
                    price = Math.Round(index, 2),
                    high = 0,
                    low = 0,
                    yestclose = yestclose,
                    volume = vol,
                    turnover = turnover,
                    percent = percent,
                    open = Math.Round(index, 2),
                };

                priceInfoList.Add(info);
                this.Log().Info(string.Format("行业：{0}，值：{1}", item.code, info.price));

            }
            cateService.UpdateCategoryPrice(priceInfoList);
            cateService.AddPriceByDay<data.data_category_day_latest>(priceInfoList);
            cateService.AddPriceByWeek<data.data_category_week_latest>(priceInfoList);
            cateService.AddPriceByMonth<data.data_category_month_latest>(priceInfoList);
            this.Log().Info("计算结束");
        }

        private decimal GetYestclose(string p, DateTime today)
        {
            int i = -1;
            var list = cateService.GetPriceInfo(p, data.TechCycle.day, today.AddDays(i));
            while (list == null || list.Count == 0)
            {
                list = cateService.GetPriceInfo(p, data.TechCycle.day, today.AddDays(--i));

                if (list != null && list.Count > 0)
                {
                    return list[0].price ?? 0;
                }
            }
            return list[0].price ?? 1;
        }



        public string CategoryCode { get; set; }
    }
}
