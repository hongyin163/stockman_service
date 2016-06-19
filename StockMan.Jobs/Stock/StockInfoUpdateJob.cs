using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using StockMan.Jobs.Stock.tencent;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using data = StockMan.EntityModel;

namespace StockMan.Jobs.Stock
{
    public class StockInfoUpdateJob : IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();

        IStockSync sync = new StockSync_Tencent();

        int batchNum = 40;

        public void Execute(IJobExecutionContext context)
        {
           

            //获取分类
            IList<data.stockcategory> cateList = cateService.GetCategoryList("tencent");

            foreach (var category in cateList)
            {
                IList<data.stock> stockList = stockService.GetStockByCategory(category.code);

                for (int i = 0; i < stockList.Count; i += batchNum)
                {
                    IList<data.StockInfo> spList = sync.GetPrice(stockList.Skip(i).Take(batchNum).ToList());

                    foreach (data.StockInfo price in spList)
                    {
                        //stockService.UpdateStockPrice(price.stock_code, price);
                       this.Log().Info("更新价格：" + price.stock_code + "-" + price.name + "-" + price.price);
                    }
                    stockService.UpdateStockPrice(spList);
                }
    
            }

            //变量分类下的股票
        }
    }
}
