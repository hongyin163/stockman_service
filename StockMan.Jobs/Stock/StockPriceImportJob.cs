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


namespace StockMan.Jobs.Stock
{
    public class StockPriceImportJob : IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();

        IStockSync sync = new StockSync_Tencent();

        Guid batch = Guid.Empty;

        public string CategoryCode { get; set; }
        public void Execute(IJobExecutionContext context)
        {
            this.batch = Guid.NewGuid();
            //获取分类
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
            int total = 0;
            foreach (var category in cateList)
            {
                this.Log().Info("开始导入行业:" + category.code + "-" + category.name);
                IList<data.stock> stockList = stockService.GetStockByCategory(category.code);

                foreach (data.stock stock in stockList)
                {
                    Thread.Sleep(50);
                    ImportDay(stock);

                    ImportWeek(stock);

                    ImportMonth(stock);
                }
                this.Log().Info("结束导入行业:" + category.code + "-" + category.name);

            }


            //处理失败的记录         
            ReImport();
        }

        private void ReImport()
        {

            int errorCount = 0;
            IList<data.pricesynclog> errorList = stockService.GetPriceSyncLog(this.batch, 0);
            errorCount = errorList.Count;
            this.Log().Info("开始导入失败的记录");
            while (errorCount > 0)
            {
                foreach (var log in errorList)
                {
                    var stock = stockService.Find(log.stock_code);
                    var succes = false;
                    if (log.type == "day")
                    {
                        succes = ImportDay(stock);
                    }
                    else if (log.type == "week")
                    {
                        succes = ImportWeek(stock);
                    }
                    else
                    {
                        succes = ImportMonth(stock);
                    }
                    if (succes)
                    {
                        stockService.UpdatePriceSyncLog(this.batch, stock.code, 1);
                    }
                }
                errorList = stockService.GetPriceSyncLog(this.batch, 0);
                errorCount = errorList.Count;
            }
            this.Log().Info("结束导入失败的记录");
        }

        private bool ImportMonth(data.stock stock)
        {
            try
            {
                IList<data.PriceInfo> spmonth = sync.GetPriceByMonth(stock);

                var list = spmonth.OrderBy(p => p.date).ToList();

                for (int i = 1; i < list.Count(); i++)
                {
                    list[i].yestclose = list[i - 1].price;
                    list[i].updown = list[i].price - list[i].yestclose;
                    list[i].percent = list[i].updown / list[i].yestclose;
                }
                stockService.AddPriceByMonth<data.data_stock_month_latest>(spmonth, false);
                this.Log().Info("月线：" + stock.code + stock.name);
                return true;
            }
            catch (Exception ex)
            {
                stockService.AddPriceSyncLog(new data.pricesynclog
                {
                    batch = this.batch.ToString(),
                    stock_code = stock.code,
                    description = ex.GetBaseException().Message,
                    creatime = DateTime.Now,
                    type = "month"
                });
                return false;
            }
        }

        private bool ImportWeek(data.stock stock)
        {
            try
            {
                IList<data.PriceInfo> spweek = sync.GetPriceByWeek(stock);

                var list = spweek.OrderBy(p => p.date).ToList();

                for (int i = 1; i < list.Count(); i++)
                {
                    list[i].yestclose = list[i - 1].price;
                    list[i].updown = list[i].price - list[i].yestclose;
                    list[i].percent = list[i].updown / list[i].yestclose;
                }
                stockService.AddPriceByWeek<data.data_stock_week_latest>(spweek, false);
                this.Log().Info("周线：" + stock.code + stock.name);
                return true;
            }
            catch (Exception ex)
            {
                stockService.AddPriceSyncLog(new data.pricesynclog
                {
                    batch = this.batch.ToString(),
                    stock_code = stock.code,
                    description = ex.GetBaseException().Message,
                    creatime = DateTime.Now,
                    type = "week"
                });
                return false;
            }
        }

        private bool ImportDay(data.stock stock)
        {
            try
            {
                IList<data.PriceInfo> spday = sync.GetPriceByDay(stock);

                var list = spday.OrderBy(p => p.date).ToList();

                for (int i = 1; i < list.Count(); i++)
                {
                    list[i].yestclose = list[i - 1].price;
                    list[i].updown = list[i].price - list[i].yestclose;
                    list[i].percent = list[i].updown / list[i].yestclose;
                }

                stockService.AddPriceByDay<data.data_stock_day_latest>(spday, false);
                this.Log().Info("日线：" + stock.code + stock.name);
                return true;
            }
            catch (Exception ex)
            {
                stockService.AddPriceSyncLog(new data.pricesynclog
                {
                    batch = this.batch.ToString(),
                    stock_code = stock.code,
                    description = ex.GetBaseException().Message,
                    creatime = DateTime.Now,
                    type = "day"
                });
                return false;
            }
        }
    }
}
