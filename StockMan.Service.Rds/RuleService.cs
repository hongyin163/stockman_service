using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.MySqlAccess;
using StockMan.Service.Interface.Rds;
using StockMan.EntityModel;

namespace StockMan.Service.Rds
{
    public class RuleService : ServiceBase<rule>, IRuleService
    {
        public IList<rule> GetMyRule(string userId)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var query = from p in entity.rule.Include("rulecondition")
                            where p.user_id == userId
                            select p;
                return query.ToList();
            }
        }

        public void AddMyRule(rule myRule)
        {

            this.Add(myRule);
        }

        //public override void AddRange(IList<rule> datas)
        //{
        //    using (var entity = new StockManDBEntities())
        //    {
        //        foreach (var data in datas)
        //        {
        //            var rule = entity.rule.FirstOrDefault(p => p.code == data.code);
        //            if (rule != null)
        //            {
        //                rule.name = data.name;
        //                rule.state = data.state;
        //                rule.description = data.description;
        //            }
        //            else
        //            {
        //                entity.Rule.Add(data);
        //            }
        //        }
        //        entity.SaveChanges();

        //        foreach (var data in datas)
        //        {
        //            foreach (var condition in data.RuleCondition)
        //            {

        //                var rule = entity.RuleCondition.FirstOrDefault(p => p.code == condition.code);
        //                if (rule != null)
        //                {
        //                    rule.category_code = condition.category_code;
        //                    rule.category_name = condition.category_name;
        //                    rule.object_code = condition.object_code;
        //                    rule.object_name = condition.object_name;
        //                    rule.index_code = condition.index_code;
        //                    rule.index_name = condition.index_name;
        //                    rule.sort = condition.sort;
        //                    rule.rule_code = condition.rule_code;
        //                }
        //                else
        //                {
        //                    entity.RuleCondition.Add(condition);
        //                }
        //            }
        //        }

        //    }
        //}

        public void RemoveByUserId(string userId)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var list = entity.rule.Where(p => p.user_id == userId).ToList();
                if (list.Count == 0)
                    return;

                var ids = string.Empty;
                foreach (var rule in list)
                {
                    if (ids.Length == 0)
                    {
                        ids = "'" + rule.code + "'";
                    }
                    else
                    {
                        ids += ",'" + rule.code + "'";
                    }
                }

                string sql1 = @"delete from RuleCondition where rule_code in (" + ids + ");";

                //entity.Database.ExecuteSqlCommand(sql1);

                sql1 += @"delete from rule where code in (" + ids + ");";
                entity.Database.Connection.Open();
                using (entity.Database.Connection)
                {
                    System.Data.IDbCommand commond = entity.Database.Connection.CreateCommand();
                    commond.CommandText = sql1;
                    commond.ExecuteNonQuery();
                    entity.Database.Connection.Close();
                }
            }
        }

    }
}
