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
    public class RankRecoJob : IJob
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();
        IIndexService indexService = new IndexService();
        ICustomObjectService objectService = new CustomObjectService();
        public string CategoryCode { get; set; }
        public void Execute(IJobExecutionContext context)
        {
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();

            //清空推荐表
            ClearRecormend();

            this.Log().Info("初始化表结构");
            BuidTable();

            //个股排名
            this.Log().Info("个股排名");
            InitStockRankTableData();

            //行业排名
            this.Log().Info("行业排名");
            InitCateRankTableData();

            //推荐行业股票
            this.Log().Info("推荐行业股票");
            RecoCategoryStock("tencent");

            //推荐市场股票
            this.Log().Info("推荐市场股票");
            RecoCategoryStock("market");

            //tag股票推荐
            RecoTagStock("tencent");
            RecoTagStock("market");

            //状态反转推荐
            RecoStateReverseStock("tencent", "day");
            RecoStateReverseStock("tencent", "week");
            RecoStateReverseStock("tencent", "month");
        }

        private void RecoStateReverseStock(string group, string cycle)
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

                this.Log().Info("推荐行业：" + p.name);

                string sql = string.Format(@"select distinct CONCAT(b.cate_code,'_',a.code,'_',c.index_code,'_','{1}') as code, a.code as object_code,a.name as object_name,a.price,a.yestclose,a.percent,a.pe,
                            a.pb,a.mv,a.fv,b.cate_code as cate_code,d.name as cate_name,c.index_code,i.name as index_name,c.index_code as tag_code,i.name as tag_name,
                            c.day,c.week,c.month,c.last_day,c.last_week,c.last_month,'{1}' as cycle                    
                            from objectstate c
                            left join stock a on a.code=c.object_code
                            left join stock_category_map b on b.stock_code=a.code
                            left join stockcategory d on b.cate_code=d.code
                            inner join indexdefinition i on c.index_code=i.code
                            where c.{1}='1' and c.last_{1}='-1' and c.month='1' and b.cate_code ='{0}' limit 5", p.code, cycle);
                using (var entity = new StockManDBEntities())
                {
                    var recos = entity.Database.SqlQuery<data.reco_stock_category_state>(sql).ToList();

                    recos.Each(reco =>
                    {
                        this.Log().Info("推荐股票：" + reco.object_code + "_" + reco.object_name);
                        if (entity.reco_stock_category_state.Any(x => x.code == reco.code))
                        {
                            entity.Entry(reco).State = EntityState.Modified;
                        }
                        else
                        {
                            entity.reco_stock_category_state.Add(reco);
                        }
                    });
                    entity.SaveChanges();
                }

            });
        }

        private void RecoTagStock(string group)
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

                this.Log().Info("推荐行业：" + p.name);
                //and (c.day=1 or ( c.last_day=-1 and c.day=0))
                string sql = string.Format(@"select distinct CONCAT(b.cate_code,'_',a.code,'_',c.tag_code,'_',c.cycle) as code, a.code as object_code,a.name as object_name,a.price,a.yestclose,a.percent,a.pe,
                            a.pb,a.mv,a.fv,b.cate_code as cate_code,d.name as cate_name,c.tag_code,c.tag_name,c.index_code,i.name as index_name,
                            s.day,s.week,s.month,s.last_day,s.last_week,s.last_month,c.cycle                        
                            from object_tag_map c
                            left join stock a on a.code=c.object_code
                            left join stock_category_map b on b.stock_code=a.code
                            left join stockcategory d on b.cate_code=d.code
                            left join objectstate s on a.code=s.object_code and c.index_code=s.index_code                      
                            inner join indexdefinition i on c.index_code=i.code
                            where s.day='1' and s.month='1' and s.week='1' and  b.cate_code ='{0}' limit 5", p.code);

                using (var entity = new StockManDBEntities())
                {
                    var recos = entity.Database.SqlQuery<data.reco_stock_category_tag>(sql).ToList();

                    recos.Each(reco =>
                    {
                        this.Log().Info("推荐股票：" + reco.object_code + "_" + reco.object_name);
                        if (entity.reco_stock_category_tag.Any(x => x.code == reco.code))
                        {
                            entity.Entry(reco).State = EntityState.Modified;
                        }
                        else
                        {
                            entity.reco_stock_category_tag.Add(reco);
                        }
                    });
                    entity.SaveChanges();
                }

            });
        }

        private void InitStockRankTableData()
        {

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
            IList<data.indexdefinition> indexList = indexService.FindAll().Where(p => p.state == 1).ToList();

            using (var entity = new StockManDBEntities())
            {
                foreach (var category in cateList)
                {
                    IList<data.stock> stockList = stockService.GetStockByCategory(category.code);

                    foreach (var stock in stockList)
                    {

                        var stateList = entity.objectstate.Where(p => p.category_code == "1" && p.object_code == stock.code).ToList();

                        var ga = stateList.FirstOrDefault(p => p.index_code == "T0007");
                        if (ga != null)
                        {
                            if (ga.week != 1 || ga.month != 1)
                                continue;
                        }
                        else continue;

                        var dma = stateList.FirstOrDefault(p => p.index_code == "T0005");
                        if (dma != null)
                        {
                            if (dma.week != 1 || dma.month != 1)
                                continue;
                        }
                        else continue;

                        var macd = stateList.FirstOrDefault(p => p.index_code == "T0001");
                        if (macd != null)
                        {
                            if (macd.week != 1 || macd.month != 1)
                                continue;
                        }
                        else continue;

                        //var kdj = stateList.FirstOrDefault(p => p.index_code == "T0002");
                        //if (kdj != null)
                        //{
                        //    if (kdj.week != 1 || kdj.month != 1)
                        //        continue;
                        //}
                        //else
                        //{
                        //    continue;
                        //}

                        this.Log().Info("推荐:" + stock.name);
                        var insertSql = "insert into `reco_object_rank` (`code`,`date`,`category_code`,`object_code`{0}) values ('{1}','{2}','{3}','{4}'{5})";
                        string fieldSql = "", valueSql = "";
                        foreach (var state in stateList)
                        {
                            fieldSql += ",`" + state.index_code + "`";
                            if (state.day == 1 && state.week == 1 && state.month == 1)
                                valueSql += "," + "'1'";
                            else
                                valueSql += "," + "'0'";
                        }
                        insertSql = string.Format(insertSql, fieldSql, "1_" + stock.code, DateTime.Now.ToString("yyyy-MM-dd"), "1", stock.code, valueSql);
                        entity.Database.ExecuteSqlCommand(insertSql);
                    }
                }

                string filedAddStr = "";
                foreach (var index in indexList)
                {
                    if (filedAddStr.Length == 0)
                        filedAddStr = "`" + index.code + "`";
                    else
                        filedAddStr += "+`" + index.code + "`";
                }
                string updateSql = "update `reco_object_rank` set `rank`=" + filedAddStr + " where category_code='1'";
                entity.Database.ExecuteSqlCommand(updateSql);
            }
        }

        private void InitCateRankTableData()
        {

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
            IList<data.indexdefinition> indexList = indexService.FindAll().Where(p => p.state == 1).ToList();

            using (var entity = new StockManDBEntities())
            {
                foreach (var cate in cateList)
                {
                    var stateList = entity.objectstate.Where(p => p.category_code == "2" && p.object_code == cate.code).ToList();

                    var dma = stateList.FirstOrDefault(p => p.index_code == "T0005");
                    if (dma != null)
                    {
                        if (dma.week != 1 || dma.month != 1)
                            continue;
                    }
                    else
                    {
                        continue;
                    }

                    var macd = stateList.FirstOrDefault(p => p.index_code == "T0001");
                    if (macd != null)
                    {
                        if (macd.week != 1 || macd.month != 1)
                            continue;
                    }
                    else
                    {
                        continue;
                    }

                    //var kdj = stateList.FirstOrDefault(p => p.index_code == "T0002");
                    //if (kdj != null)
                    //{
                    //    if (kdj.month != 1)
                    //        continue;
                    //}
                    //else
                    //{
                    //    continue;
                    //}

                    this.Log().Info("推荐:" + cate.name);
                    var insertSql = "insert into `reco_object_rank` (`code`,`date`,`category_code`,`object_code`{0}) values ('{1}','{2}','{3}','{4}'{5})";
                    string fieldSql = "", valueSql = "";
                    foreach (var state in stateList)
                    {
                        fieldSql += ",`" + state.index_code + "`";
                        if (state.day == 1 && state.week == 1 && state.month == 1)
                            valueSql += "," + "'1'";
                        else
                            valueSql += "," + "'0'";
                    }
                    insertSql = string.Format(insertSql, fieldSql, "2_" + cate.code, DateTime.Now.ToString("yyyy-MM-dd"), "2", cate.code, valueSql);
                    entity.Database.ExecuteSqlCommand(insertSql);

                }

                string filedAddStr = "";
                foreach (var index in indexList)
                {
                    if (filedAddStr.Length == 0)
                        filedAddStr = "`" + index.code + "`";
                    else
                        filedAddStr += "+`" + index.code + "`";
                }
                string updateSql = "update `reco_object_rank` set `rank`=" + filedAddStr + " where category_code='2'";
                entity.Database.ExecuteSqlCommand(updateSql);
            }
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

                    this.Log().Info("推荐行业：" + p.name);
                    var count = entity.reco_stock_category_rank.Count(s => s.cate_code == p.code);
                    var code = p.code;

                    var rcc = entity.reco_category_count.FirstOrDefault(s => s.code == code);
                    if (rcc == null)
                    {
                        entity.reco_category_count.Add(new data.reco_category_count
                        {
                            code = code,
                            cate_code = p.code,
                            cate_type = group,
                            //index_code = i.code,
                            count = count
                        });
                    }
                    else
                    {
                        rcc.count = count;
                    }



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

                this.Log().Info("推荐行业：" + p.name);
                //and (c.day=1 or ( c.last_day=-1 and c.day=0))
                string sql = string.Format(@"select distinct CONCAT(b.cate_code,'_',a.code) as code, a.code as object_code,a.name as object_name,a.price,a.yestclose,a.percent,a.pe,
                            a.pb,a.mv,a.fv,b.cate_code as cate_code,d.name as cate_name,c.rank                        
                            from stock a 
                            inner join stock_category_map b on b.stock_code=a.code
                            inner join stockcategory d on b.cate_code=d.code
                            inner join reco_object_rank c on a.code=c.object_code
                            where b.cate_code ='{0}' order by c.rank desc
                            LIMIT 5;", p.code);

                using (var entity = new StockManDBEntities())
                {
                    var recos = entity.Database.SqlQuery<data.reco_stock_category_rank>(sql).ToList();

                    recos.Each(reco =>
                    {
                        this.Log().Info("推荐股票：" + reco.object_code + "_" + reco.object_name);
                        if (entity.reco_stock_category_rank.Any(x => x.code == reco.code))
                        {
                            entity.Entry(reco).State = EntityState.Modified;
                        }
                        else
                        {
                            entity.reco_stock_category_rank.Add(reco);
                        }
                    });
                    entity.SaveChanges();
                }

            });
        }
        private void ClearRecormend()
        {
            string sql = string.Format(@"delete from  `reco_stock_category_rank`;
                                         delete from  `reco_stock_category_tag`;
                                         delete from  `reco_stock_category_state`;");

            using (var entity = new StockManDBEntities())
            {
                entity.Database.ExecuteSqlCommand(sql);
            }
        }
        protected void BuidTable()
        {
            IList<data.indexdefinition> indexList = indexService.FindAll().Where(p => p.state == 1).ToList();

            string dropSql = "drop table if exists `reco_object_rank`";

            string sql = @"create table if not exists `reco_object_rank` ({0}, PRIMARY KEY (`code`));";

            string sql1 = @"code varchar(50) NOT NULL, date datetime NULL,category_code varchar(50) null,object_code varchar(50) null,rank int default 0";
            foreach (var index in indexList)
            {
                sql1 += "," + index.code + " int default 0";
            }

            string result = string.Format(sql, sql1);

            using (var entity = new StockManDBEntities())
            {
                this.Log().Info("删除表：reco_stock_rank");
                entity.Database.ExecuteSqlCommand(dropSql);
                this.Log().Info("构建表：reco_stock_rank");
                entity.Database.ExecuteSqlCommand(result);
            }
        }
    }
}
