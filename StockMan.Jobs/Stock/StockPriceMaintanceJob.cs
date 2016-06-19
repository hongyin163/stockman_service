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
using StockMan.MySqlAccess;


namespace StockMan.Jobs.Stock
{
    public class StockPriceMaintanceJob : IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();

        IStockSync sync = new StockSync_Tencent();


        public string StockCode { get; set; }
        public void Execute(IJobExecutionContext context)
        {

            //获取分类
            string[] codeList = StockCode.Split(',');

            IList<data.stock> stockList = stockService.GetStocksByIds(StockCode);

            foreach (data.stock stock in stockList)
            {
                this.Log().Info("开始导入股票：" + stock.name);
                Thread.Sleep(50);
                ImportDay(stock);

                ImportWeek(stock);

                ImportMonth(stock);

                clearTechData(stock);

                this.Log().Info("完成导入股票：" + stock.name);
            }
            this.Log().Info("结束导入股票");
        }

        private void clearTechData(data.stock stock)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string sql = string.Format("delete from tech_context where object_code='{0}'", stock.code);
                entity.Database.ExecuteSqlCommand(sql);
              
            }
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
                return false;
            }
        }
    }
}
