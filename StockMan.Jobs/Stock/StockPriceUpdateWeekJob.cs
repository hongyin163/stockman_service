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
using data = StockMan.DataModel;
namespace StockMan.Jobs.Stock
{
    public class StockPriceUpdateWeekJob : IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();

        IStockSync sync = new StockSync_Tencent();

        DateTime timestamp;

        int batchNum = 40;

        public void Execute(IJobExecutionContext context)
        {
            IList<data.StockCategory> cateList = cateService.GetCategoryList("tencent");

            foreach (var category in cateList)
            {
                IList<data.Stock> stockList = stockService.GetStockByCategory(category.code);


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

                    stockService.AddPriceByWeek(spiList);
                }
            }
        }
    }
}
