using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.Message.Task.Interface;
using data = StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.MySqlAccess;
using System.Data;
using Newtonsoft.Json;

namespace StockMan.Message.TaskInstance
{
    public class RecoCateStockTask : Message.Task.ITask
    {
        IStockCategoryService cateService = new StockCategoryService();
        IStockService stockService = new StockService();
        IIndexService indexService = new IndexService();
        ICustomObjectService objectService = new CustomObjectService();
        public void Excute(string message)
        {

            RecoCateStock msg = JsonConvert.DeserializeObject<RecoCateStock>(message);
            string cateCode = msg.CateCode;
            string cateName = msg.CateName;
            string indexCode = msg.IndexCode;
            string indexName = msg.IndexName;

            this.Log().Info("计算开始：推荐行业：" + cateName + ",技术" + indexName);

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
                            LIMIT 5;", cateCode, indexCode);

            using (var entity = new StockManDBEntities())
            {
                var recos = entity.Database.SqlQuery<data.reco_stock_category_index>(sql).ToList();
                foreach (var reco in recos)
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
                }
                entity.SaveChanges();
            }
            this.Log().Info("计算完成：推荐行业：" + cateName + ",技术" + indexName);
        }

        public string GetCode()
        {
            return "T0005";
        }

        public void Send(IMessageSender sender)
        {
            this.Log().Info("清空数据");
            ClearRecormend();

            var groups = new string[] { "tencent", "market" };
            foreach (var group in groups)
            {
                this.Log().Info("推荐" + group + "股票");

                IList<data.stockcategory> cateList = cateService.GetCategoryList(group);
                IList<data.indexdefinition> indexList = indexService.FindAll().Where(p => p.state == 1).ToList();
                foreach (var cate in cateList)
                {
                    foreach (var index in indexList)
                    {
                        RecoCateStock msg = new RecoCateStock
                        {
                            CateCode = cate.code,
                            CateName = cate.name,
                            IndexCode = index.code,
                            IndexName = index.name
                        };
                        sender.Send(JsonConvert.SerializeObject(msg));
                    }
                }
            }
        }

        private void RecoCategoryStock(string group)
        {

            IList<data.stockcategory> cateList = cateService.GetCategoryList(group);
            IList<data.indexdefinition> indexList = indexService.FindAll().Where(p => p.state == 1).ToList();

            //每个行业，每个技术，最多推荐5只股票。

            //每个行业，每个技术，最多推荐5只股票。
            foreach (var cate in cateList)
            {
                foreach (var index in indexList)
                {
                    this.Log().Info("推荐行业：" + cate.name + ",技术" + index.name);
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
                            LIMIT 5;", cate.code, index.code);

                    using (var entity = new StockManDBEntities())
                    {
                        var recos = entity.Database.SqlQuery<data.reco_stock_category_index>(sql).ToList();
                        foreach (var reco in recos)
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
                        }
                        entity.SaveChanges();
                    }
                }
            }
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

    public class RecoCateStock
    {
        public string CateCode { get; set; }
        public string CateName { get; set; }
        public string IndexCode { get; set; }
        public string IndexName { get; set; }
    }
}
