using Quartz;
using StockMan.MySqlAccess;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data = StockMan.EntityModel;
using facade = StockMan.Facade.Models;
using StockMan.Common;
namespace StockMan.Jobs.Recommend
{
    public class CategoryRecoJob : IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();
        IIndexService indexService = new IndexService();
        ICustomObjectService objectService = new CustomObjectService();
        public string CategoryCode { get; set; }
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            //清空
            this.Log().Info("清空数据");
            ClearRecormend();
            //推荐行业股票
            this.Log().Info("推荐行业股票");
            RecoCategoryStock("tencent");

            //推荐市场股票
            this.Log().Info("推荐市场股票");
            RecoCategoryStock("market");

            //计算行业，市场，推荐股票的个数
            //ComputeCategoryCount("tencent");

            //ComputeCategoryCount("market");


        }

        private void ComputeCategoryCount(string group)
        {
            IList<data.stockcategory> cateList = null;
            IList<data.indexdefinition> indexList = indexService.FindAll().Where(p => p.state == 1).ToList();

            if (string.IsNullOrEmpty(this.CategoryCode) || this.CategoryCode == "-1")
            {
                cateList = cateService.GetCategoryList(group);
            }
            else
            {
                string[] cates = this.CategoryCode.Split(',');
                cateList = cateService.GetCategoryList(group).Where(p => cates.Contains(p.code)).ToList(); ;
            }

            using (var entity = new StockManDBEntities())
            {
                cateList.Each(p =>
                {
                    indexList.Each(i =>
                    {
                        this.Log().Info("推荐行业：" + p.name + ",技术" + i.name);
                        var count = entity.reco_stock_category_index.Count(s => s.cate_code == p.code && s.index_code == i.code);
                        var code = p.code + "_" + i.code;

                        var rcc = entity.reco_category_count.FirstOrDefault(s => s.code == code);
                        if (rcc == null)
                        {
                            entity.reco_category_count.Add(new data.reco_category_count
                            {
                                code = code,
                                cate_code = p.code,
                                cate_type = group,
                                index_code = i.code,
                                count = count
                            });
                        }
                        else
                        {
                            rcc.count = count;
                        }

                    });

                    entity.SaveChanges();
                });
            }
        }

        private void RecoCategoryStock(string group)
        {

            IList<data.stockcategory> cateList = null;
            IList<data.indexdefinition> indexList = indexService.FindAll().Where(p => p.state == 1).ToList();

            if (string.IsNullOrEmpty(this.CategoryCode) || this.CategoryCode == "-1")
            {
                cateList = cateService.GetCategoryList(group);
            }
            else
            {
                string[] cates = this.CategoryCode.Split(',');
                cateList = cateService.GetCategoryList(group).Where(p => cates.Contains(p.code)).ToList(); ;
            }



            //每个行业，每个技术，最多推荐5只股票。

            cateList.Each(p =>
            {
                indexList.Each(i =>
                {
                    this.Log().Info("推荐行业：" + p.name + ",技术" + i.name);
                    //and (c.day=1 or ( c.last_day=-1 and c.day=0))
                    string sql = string.Format(@"select distinct CONCAT(b.cate_code,'_',c.index_code,'_',a.code) as code, a.code as object_code,a.name as object_name,a.price,a.yestclose,a.percent,a.pe,
                            a.pb,a.mv,a.fv,b.cate_code as cate_code,d.name as cate_name,c.index_code as index_code,e.name as index_name,
                            c.day,c.week,c.month,c.last_day,c.last_week,c.last_month  
                            from stock a 
                            inner join stock_category_map b on b.stock_code=a.code
                            inner join stockcategory d on b.cate_code=d.code
                            inner join objectstate c on a.code=c.object_code
                            inner join indexdefinition e on c.index_code=e.code
                            where b.cate_code ='{0}' and c.index_code ='{1}'  and c.week=1 and c.month=1 and (c.day=1 or ( c.last_day=-1 and c.day=0))
                            LIMIT 5;", p.code, i.code);

                    using (var entity = new StockManDBEntities())
                    {
                        var recos = entity.Database.SqlQuery<data.reco_stock_category_index>(sql).ToList();

                        recos.Each(reco =>
                        {
                            this.Log().Info("推荐股票：" + reco.object_code + "_" + reco.object_name);
                            if (entity.reco_stock_category_index.Any(x => x.code == reco.code))
                            {
                                entity.Entry(reco).State = EntityState.Modified;
                            }
                            else
                            {
                                entity.reco_stock_category_index.Add(reco);
                            }
                        });
                        entity.SaveChanges();
                    }
                });
            });
        }

        private void ClearRecormend()
        {
            string sql = string.Format(@"delete from  `reco_stock_category_index`");

            using (var entity = new StockManDBEntities())
            {
                entity.Database.ExecuteSqlCommand(sql);
            }
        }
    }
}
