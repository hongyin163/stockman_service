using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Jobs.Biz;

namespace StockMan.Jobs.Tech.Stock
{
    public class TechStockInit : IJob
    {
        public TechCycle _cycleType = TechCycle.day;
        public TechCycle CycleType
        {
            get { return this._cycleType; }
            set { this._cycleType = value; }
        }

        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            var cateService = new StockCategoryService();
            var stockService = new StockService();



            var cycleType = this.CycleType;

            IList<stockcategory> cateList = cateService.FindAll();

            foreach (var category in cateList)
            {
                IList<stock> stockList = stockService.GetStockByCategory(category.code);

                foreach (var stock in stockList)
                {
                    IList<PriceInfo> priceList = stockService.GetPriceInfo(stock.code, cycleType);
                   
                    this.Log().Error("开始计算股票技术指数："+stock.name+"，Code："+ stock.code);
                    var tech = new TechCalculate(ObjectType.Stock, cycleType, stock.code, priceList);
                    tech.Run();
                    this.Log().Error("计算结束：" + stock.name);

                    Thread.Sleep(100);
                }

            }
        }
    }
}
