using Quartz;
using StockMan.MySqlAccess;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data = StockMan.EntityModel;
namespace StockMan.Jobs.Tech
{
    public class TechResetJob : IJob
    {
        IIndexService indexService = new IndexService();
        public void Execute(IJobExecutionContext context)
        {
            string[] cateList = new string[] { "stock", "category", "object" };
            string[] cycleList = new string[] { "day", "week", "month" };
            IList<data.indexdefinition> indexList = indexService.FindAll().Where(p => p.state == 1).ToList();
            //清空技术数据

            //股票
            foreach (var cate in cateList)
            {
                foreach (string cycle in cycleList)
                {
                    foreach (var tech in indexList)
                    {
                        this.Log().Info("清空:" + cate + "_" + tech.name + "_" + cycle);
                        ClearTable(cate, tech.name, cycle);
                    }
                }
            }


            //清空技术上下文
            this.Log().Info("清空技术上下文");
            ClearContext();
        }

        private void ClearTable(string cate, string tech, string cycle)
        {
            string sql = string.Format(@"truncate table tech_{0}_{1}_{2};", cate, tech, cycle);

            using (var entity = new StockManDBEntities())
            {
                entity.Database.ExecuteSqlCommand(sql);
            }
        }
        private void ClearContext()
        {
            string sql = string.Format(@"truncate table tech_context;");

            using (var entity = new StockManDBEntities())
            {
                entity.Database.ExecuteSqlCommand(sql);
            }
        }
    }
}
