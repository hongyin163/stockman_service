using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using StockMan.Jobs.Stock.tencent;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using data = StockMan.EntityModel;
using StockMan.EntityModel;


namespace StockMan.Jobs.Category
{
    /// <summary>
    /// 初始化行业指数，从历史数据
    /// </summary>
    public class CateIndexInitJob : IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();
        public string CategoryCode { get; set; }
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();


            //获取分类
            IList<data.stockcategory> cateList;

            if (string.IsNullOrEmpty(this.CategoryCode) || this.CategoryCode == "-1")
            {
                cateList = cateService.GetCategoryList("tencent");
            }
            else
            {
                string[] cates = this.CategoryCode.Split(',');
                cateList = cateService.GetCategoryList("tencent").Where(p => cates.Contains(p.code)).ToList();
            }


            int total = 0;
            foreach (var category in cateList)
            {

                this.Log().Info(string.Format("行业指数日线开始，行业：{0}", category.name));
                //获取分类，某个周期，所有股票的价格

                try
                {
                    InitCategoryIndexByDay(category.code);
                }
                catch (Exception ex)
                {
                    this.Log().Info(string.Format("异常：{0},堆栈：{1}", ex.Message, ex.StackTrace));
                }

                this.Log().Info(string.Format("行业指数周线开始，行业：{0}", category.name));
                try
                {
                    InitCategoryIndexByWeek(category.code);
                }
                catch (Exception ex)
                {
                    this.Log().Info(string.Format("异常：{0},堆栈：{1}", ex.Message, ex.StackTrace));
                }
                this.Log().Info(string.Format("行业指数月线开始，行业：{0}", category.name));
                try
                {
                    InitCategoryIndexByMonth(category.code);
                }
                catch (Exception ex)
                {
                    this.Log().Info(string.Format("异常：{0},堆栈：{1}", ex.Message, ex.StackTrace));
                }
            }
        }

        /// <summary>
        /// 计算某行业，每周期的指数值
        /// </summary>
        /// <param name="categoryCode"></param>
        /// <param name="cycle"></param>
        private void InitCategoryIndexByCycle(string categoryCode, data.TechCycle cycle)
        {
            //获取某只股票的所有价格列表

            var stocks = stockService.GetPriceInfo("0600000", cycle);
            decimal yestclose = 0;
            decimal baseIndex = 100;
            decimal baseValue = 0;

            //计算每个周期的指数值
            for (int i = 0; i < stocks.Count; i++)
            {
                var stock = stocks[i];
                var priceList = stockService.GetStockPriceByDate(categoryCode, cycle, stock.date);
                if (priceList == null || priceList.Count == 0)
                    continue;
                decimal total = 0, vol = 0, turnover = 0, index = 0;
                foreach (var priceInfo in priceList)
                {
                    total += priceInfo.price ?? 1 * priceInfo.volume ?? 1;
                    vol += priceInfo.volume ?? 1;
                    turnover += priceInfo.turnover ?? 1;
                }

                if (baseValue == 0)
                {
                    baseValue = total;
                    index = 100;
                }
                else
                {
                    index = (total / baseValue) * 100;
                }

                var info = new data.PriceInfo
                {
                    code = categoryCode,
                    date = stock.date,
                    price = Math.Round(index, 2),
                    high = 0,
                    low = 0,
                    yestclose = yestclose,
                    volume = vol,
                    turnover = turnover,
                    open = Math.Round(index, 2),
                };

                yestclose = index;

                try
                {
                    //cateService.AddPriceInfo(categoryCode, info, cycle);
                    if (cycle == TechCycle.day)
                    {
                        cateService.AddPriceByDay<data_category_day_latest>(new List<PriceInfo>() { info }, true);
                    }
                    else if (cycle == TechCycle.week)
                    {
                        cateService.AddPriceByWeek<data_category_week_latest>(new List<PriceInfo>() { info }, true);

                    }
                    else if (cycle == TechCycle.month)
                    {
                        cateService.AddPriceByMonth<data_category_month_latest>(new List<PriceInfo>() { info }, true);

                    }

                    this.Log().Info(string.Format("行业指数：周期：{0}，日期：{1}，类别：{2}，数值：{3}", cycle, info.date, categoryCode, info.price));
                }
                catch (Exception ex)
                {
                    //(new System.Collections.Generic.Mscorlib_CollectionDebugView<System.Data.Entity.Validation.DbEntityValidationResult>(((System.Data.Entity.Validation.DbEntityValidationException)(ex)).EntityValidationErrors as System.Collections.Generic.List<System.Data.Entity.Validation.DbEntityValidationResult>)).Items[0].ValidationErrors


                    this.Log().Error(ex.Message);
                }

            }
        }


        private void InitCategoryIndexByDay(string categoryCode)
        {
            //获取某只股票的所有价格列表
            TechCycle cycle = TechCycle.day;
            var stocks = stockService.GetPriceInfo("0600000", cycle);
            decimal yestclose = 0;
            decimal baseIndex = 100;
            decimal baseValue = 0;

            //计算每个周期的指数值
            for (int i = 0; i < stocks.Count; i++)
            {
                var stock = stocks[i];
                var priceList = stockService.GetStockPriceByDate(categoryCode, cycle, stock.date);
                if (priceList == null || priceList.Count == 0)
                    continue;
                decimal total = 0, vol = 0, turnover = 0, index = 0;
                foreach (var priceInfo in priceList)
                {
                    total += priceInfo.price ?? 1 * priceInfo.volume ?? 1;
                    vol += priceInfo.volume ?? 1;
                    turnover += priceInfo.turnover ?? 1;
                }

                if (baseValue == 0)
                {
                    baseValue = total;
                    index = 100;
                }
                else
                {
                    index = (total / baseValue) * 100;
                }

                var info = new data.PriceInfo
                {
                    code = categoryCode,
                    date = stock.date,
                    price = Math.Round(index, 2),
                    high = 0,
                    low = 0,
                    yestclose = yestclose,
                    volume = vol,
                    turnover = turnover,
                    open = Math.Round(index, 2),
                };

                yestclose = index;

                try
                {
                    //cateService.AddPriceInfo(categoryCode, info, cycle);
                    //if (cycle == TechCycle.day)
                    //{
                    cateService.AddPriceByDay<data_category_day_latest>(new List<PriceInfo>() { info }, true);
                    //}
                    //else if (cycle == TechCycle.week)
                    //{
                    //    cateService.AddPriceByWeek<data_category_week_latest>(new List<PriceInfo>() { info }, true);

                    //}
                    //else if (cycle == TechCycle.month)
                    //{
                    //    cateService.AddPriceByMonth<data_category_month_latest>(new List<PriceInfo>() { info }, true);

                    //}

                    this.Log().Info(string.Format("行业指数：周期：{0}，日期：{1}，类别：{2}，数值：{3}", cycle, info.date, categoryCode, info.price));
                }
                catch (Exception ex)
                {
                    //(new System.Collections.Generic.Mscorlib_CollectionDebugView<System.Data.Entity.Validation.DbEntityValidationResult>(((System.Data.Entity.Validation.DbEntityValidationException)(ex)).EntityValidationErrors as System.Collections.Generic.List<System.Data.Entity.Validation.DbEntityValidationResult>)).Items[0].ValidationErrors


                    this.Log().Error(ex.Message);
                }

            }
        }
        private void InitCategoryIndexByWeek(string categoryCode)
        {
            //获取某只股票的所有价格列表
            TechCycle cycle = TechCycle.day;

            //获取日线数据
            var dayList = cateService.GetPriceInfo(categoryCode, TechCycle.day);

            if (dayList.Count == 0)
                return;

            var weekList = new List<PriceInfo>();

            DateTime startTime = dayList[0].date;

            PriceInfo lastWeekPrice = null;
            while (startTime < DateTime.Now)
            {
                DateTime nextWeek = startTime.AddDays(7);
                DateTime Monday = nextWeek.AddDays(DayOfWeek.Monday - nextWeek.DayOfWeek);
                DateTime endTime = Monday.AddDays(-1);
                var rangePrice = dayList.Where(p => p.date > startTime && p.date < endTime).OrderBy(p => p.date).ToList();

                if (rangePrice.Count == 0)
                {
                    startTime = endTime;
                    continue;
                }
                var lastPrice = rangePrice[rangePrice.Count - 1];

                decimal? open = rangePrice[0].price;
                decimal? close = lastPrice.price;
                decimal? high = rangePrice.Max(p => p.price);
                decimal? low = rangePrice.Min(p => p.price);
                decimal? volume = rangePrice.Sum(p => p.volume);
                decimal? turnover = rangePrice.Sum(p => p.turnover);

                decimal? percent = 0;
                decimal? updown = 0;
                decimal? yestclose = 0;
                if (lastWeekPrice != null)
                {
                    yestclose = lastWeekPrice.price;
                    updown = close - yestclose;
                    if (yestclose != 0)
                        percent = updown / yestclose;
                }
                var currentPrice = new PriceInfo
                {
                    code = categoryCode,
                    open = open,
                    price = close,
                    low = low,
                    high = high,
                    date = lastPrice.date,
                    percent = Math.Round(percent??0, 2),
                    turnover = turnover,
                    updown = updown,
                    volume = volume,
                    yestclose = yestclose
                };
                weekList.Add(currentPrice);

                lastWeekPrice = currentPrice;
                startTime = endTime;
            }

            try
            {
                cateService.AddPriceByWeek<data_category_week_latest>(weekList, false);

                this.Log().Info(string.Format("行业指数计算完成：周期：{0}，类别：{1}", TechCycle.week, categoryCode));

            }
            catch (Exception ex)
            {
                //(new System.Collections.Generic.Mscorlib_CollectionDebugView<System.Data.Entity.Validation.DbEntityValidationResult>(((System.Data.Entity.Validation.DbEntityValidationException)(ex)).EntityValidationErrors as System.Collections.Generic.List<System.Data.Entity.Validation.DbEntityValidationResult>)).Items[0].ValidationErrors
                this.Log().Error(ex.Message);
            }

        }

        private void InitCategoryIndexByMonth(string categoryCode)
        {
            //获取某只股票的所有价格列表
            TechCycle cycle = TechCycle.day;

            //获取日线数据
            var dayList = cateService.GetPriceInfo(categoryCode, TechCycle.day);

            if (dayList.Count == 0)
                return;

            var weekList = new List<PriceInfo>();

            DateTime startTime = dayList[0].date;

            PriceInfo lastWeekPrice = null;

            while (startTime < DateTime.Now)
            {
                DateTime nextDate = startTime.AddMonths(1);
                DateTime endTime = new DateTime(nextDate.Year, nextDate.Month, 1);
                var rangePrice = dayList.Where(p => p.date >= startTime && p.date < endTime).OrderBy(p => p.date).ToList();

                if (rangePrice.Count == 0)
                {
                    startTime = endTime;
                    continue;
                }
                var lastPrice = rangePrice[rangePrice.Count - 1];

                decimal? open = rangePrice[0].price;
                decimal? close = lastPrice.price;
                decimal? high = rangePrice.Max(p => p.price);
                decimal? low = rangePrice.Min(p => p.price);
                decimal? volume = rangePrice.Sum(p => p.volume);
                decimal? turnover = rangePrice.Sum(p => p.turnover);

                decimal? percent = 0;
                decimal? updown = 0;
                decimal? yestclose = 0;
                if (lastWeekPrice != null)
                {
                    yestclose = lastWeekPrice.price;
                    updown = close - yestclose;
                    if (yestclose != 0)
                        percent = updown / yestclose;
                }
                var currentPrice = new PriceInfo
                {
                    code = categoryCode,
                    open = open,
                    price = close,
                    low = low,
                    high = high,
                    date = lastPrice.date,
                    percent = Math.Round(percent ?? 0, 2),
                    turnover = turnover,
                    updown = updown,
                    volume = volume,
                    yestclose = yestclose
                };
                weekList.Add(currentPrice);

                lastWeekPrice = currentPrice;
                startTime = endTime;
            }

            try
            {
                cateService.AddPriceByMonth<data_category_month_latest>(weekList, false);

                this.Log().Info(string.Format("行业指数计算完成：周期：{0}，类别：{1}", TechCycle.month, categoryCode));

            }
            catch (Exception ex)
            {
                //(new System.Collections.Generic.Mscorlib_CollectionDebugView<System.Data.Entity.Validation.DbEntityValidationResult>(((System.Data.Entity.Validation.DbEntityValidationException)(ex)).EntityValidationErrors as System.Collections.Generic.List<System.Data.Entity.Validation.DbEntityValidationResult>)).Items[0].ValidationErrors
                this.Log().Error(ex.Message);
            }

        }
    }

}
