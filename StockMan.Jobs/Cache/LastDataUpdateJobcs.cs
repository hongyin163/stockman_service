using Newtonsoft.Json;
using Quartz;
using StockMan.EntityModel;
using StockMan.Jobs.Category;
using StockMan.Jobs.Object;
using StockMan.Jobs.Stock;
using StockMan.Service.Cache;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data = StockMan.EntityModel;
namespace StockMan.Jobs.Cache
{
    /// <summary>
    /// 更新价格，缓存，收盘后执行，先更新rds，再更新rides
    /// 凌晨一点执行
    /// </summary>
    public class LastDataUpdateJob : IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();
        ICustomObjectService objectService = new CustomObjectService();
        private string _category = "-1";
        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                _category = value;
            }
        }
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            this.Log().Info("开始更新缓存数据");

            //大盘
            if (this.Category == "3" || this.Category == "-1")
            {
                this.Log().Info("开始大盘缓存数据");
                var objList = objectService.FindAll();
                foreach (var obj in objList)
                {
                    this.Log().Info("更新：" + obj.name);
                    UpdateCacheData(ObjectType.Object, obj.code);
                }
                this.Log().Info("结束大盘缓存数据");
            }
            //个股
            if (this.Category == "1" || this.Category == "-1")
            {
                this.Log().Info("开始更新个股数据");
                //var stockList = stockService.FindAll();

                IList<data.stockcategory> cateList1 = cateService.GetCategoryList("tencent").ToList();

                int total = 0;
                foreach (var category in cateList1)
                {
                    IList<data.stock> stockList = stockService.GetStockByCategory(category.code);

                    foreach (var stock in stockList)
                    {
                        this.Log().Info("更新：" + stock.name);
                        UpdateCacheData(ObjectType.Stock, stock.code);
                    }
                    this.Log().Info("结束更新个股数据");
                }
            }

            //行业
            if (this.Category == "2" || this.Category == "-1")
            {
                this.Log().Info("开始更新行业数据");
                var cateList = cateService.FindAll();
                foreach (var cate in cateList)
                {
                    this.Log().Info("更新：" + cate.name);
                    UpdateCacheData(ObjectType.Category, cate.code);
                }
                this.Log().Info("结束更新行业数据");
            }
            this.Log().Info("更新缓存数据结束！");
        }

        private void UpdateCacheData(ObjectType objType, string code)
        {
            var priceList = objectService.GetHistoryData(objType, TechCycle.day, code);
            string ocode = ((int)objType).ToString();
            CacheHelper.Set(string.Format("{0}_{1}_{2}_history", ocode, code, TechCycle.day).ToLower(), priceList);
            //日线清空current
            //PriceInfo weekD = objectService.GetCurrentData(objType, TechCycle.Day, code);
            CacheHelper.Set(string.Format("{0}_{1}_{2}_current", ocode, code, TechCycle.day).ToLower(), string.Empty);

            //周线，获取上周以前的历史数据
            priceList = objectService.GetHistoryData(objType, TechCycle.week, code);
            CacheHelper.Set(string.Format("{0}_{1}_{2}_history", ocode, code, TechCycle.week).ToLower(), priceList);

            //当前周的数据
            PriceInfo weekP = objectService.GetCurrentData(objType, TechCycle.week, code);
            CacheHelper.Set(string.Format("{0}_{1}_{2}_current", ocode, code, TechCycle.week).ToLower(), JsonConvert.SerializeObject(weekP));

            //月线，获取上月以前的数据
            priceList = objectService.GetHistoryData(objType, TechCycle.month, code);
            CacheHelper.Set(string.Format("{0}_{1}_{2}_history", ocode, code, TechCycle.month).ToLower(), priceList);
            //获取当前月的数据
            PriceInfo monthP = objectService.GetCurrentData(objType, TechCycle.month, code);
            CacheHelper.Set(string.Format("{0}_{1}_{2}_current", ocode, code, TechCycle.month).ToLower(), JsonConvert.SerializeObject(monthP));
        }
    }
}
