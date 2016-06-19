using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using StockMan.MySqlAccess;
using StockMan.EntityModel;
using StockMan.Index;
using StockMan.Index.Tech;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;

namespace StockMan.Jobs.State
{
    public class StockStateJob : StateJobBase, IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();
        public void Execute(IJobExecutionContext context)
        {
            var type = context.Trigger.JobDataMap["cycle"] + "";
            var ct = (TechCycle)Enum.Parse(typeof(TechCycle), type, true);

            //获取指数定义列表
            var defineList = index_definition_infos(ct);
            //根据定义生成指数信息

            foreach (IndexDefinitionInfo define in defineList)
            {
                //计算
                Calculate(define, ct);
            }
        }

        private void Calculate(IndexDefinitionInfo define,TechCycle cycle)
        {

            IIndex indexGener = new Dma();// CodeHelper.GetIEvaluator(define.server_algorithm_code);

            IList<stockcategory> cateList = cateService.GetCategoryList("tencent");

            int tatol = 0;
            foreach (var category in cateList)
            {
                IList<stock> stockList = stockService.GetStockByCategory(category.code);

                foreach (stock stock in stockList)
                {

                    IList<PriceInfo> list = stockService.GetStockPriceDayInfo(stock);
                    IList<IndexData> listIndexData = GetLastIndexData(stock, define);

                    var result = indexGener.GetState(listIndexData);
                    this.Log().Info(stock.name + "_" + result);
                    using (StockManDBEntities entity = new StockManDBEntities())
                    {
                        var cate = "1";
                        var code = cate + "_" + stock.code + "_" + define.code;
                        var objectState = entity.objectstate.FirstOrDefault(p => p.code == code);
                        if (objectState == null)
                        {
                            var temp = new objectstate()
                            {
                                code = code,
                                category_code = cate,
                                object_code = stock.code,
                                index_code = define.code,
                                date = DateTime.Now
                            };

                            switch (cycle)
                            {
                                case TechCycle.day:
                                    temp.day = (int) result;
                                    break;
                                case TechCycle.week:
                                    temp.week = (int)result;
                                    break;
                                default:
                                    temp.month = (int)result;
                                    break;
                            }
                            entity.objectstate.Add(temp);
                            
                        }
                        else
                        {
                            switch (cycle)
                            {
                                case TechCycle.day:
                                    objectState.last_day = objectState.day;
                                    objectState.day = (int)result;
                                    break;
                                case TechCycle.week:
                                    objectState.last_week = objectState.week;
                                    objectState.week = (int)result;
                                    break;
                                default:
                                    objectState.last_month = objectState.month;
                                    objectState.month = (int)result;
                                    break;
                            }
                        }
                       
                        entity.SaveChanges();
                    }

                    if (++tatol >3)
                        break;
                }
                if (tatol >3)
                    break;
            }
        }
    }
}
