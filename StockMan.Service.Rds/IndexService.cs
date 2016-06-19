using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StockMan.MySqlAccess;
using StockMan.EntityModel;
using StockMan.Service.Interface.Rds;

namespace StockMan.Service.Rds
{
    public class IndexService : ServiceBase<indexdefinition>, IIndexService
    {
        public override IList<indexdefinition> FindAll()
        {
            using (var entity = new StockManDBEntities())
            {
                return entity.indexdefinition.Include("indexdefinegroup").ToList<indexdefinition>();
            }
        }

        public void Release(string code)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var index = entity.indexdefinition.First(p => p.code == code);
                if (index != null)
                {
                    index.state = (int)IndexEnableState.Running;
                    entity.SaveChanges();
                }

            }
        }


        public IList<PriceInfo> GetObjectData(string entityType, TechCycle cycle, string objCode)
        {
            //data_object_day_latest
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string sql = string.Format(@"SELECT  code
                      ,date
                      ,open
                      ,low
                      ,price
                      ,updown                  
                      ,yestclose                    
                      ,volume
                      ,high
                      ,percent
                      ,turnover
                  FROM Data_{0}_{1}_Latest where object_code='{2}' order by code", entityType, cycle, objCode);


                return entity.Database.SqlQuery<PriceInfo>(sql).ToList();
            }
        }

        public string[][] GetIndexData(string entityType, string techName, TechCycle cycle, string dataId)
        {


            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var indexDefine = entity.indexdefinition.FirstOrDefault(p => p.name == techName);

                if (indexDefine == null)
                    return null;

                var fields = JsonConvert.DeserializeObject<IList<Field>>(indexDefine.fields);
                string tableName = "Tech_" + entityType + "_" + indexDefine.table_name + "_" + cycle;
                string sql = @"select * from " + tableName + " where f_code='" + dataId + "' order by date desc limit 500";

                entity.Database.Connection.Open();
                using (entity.Database.Connection)
                {
                    System.Data.IDbCommand commond = entity.Database.Connection.CreateCommand();
                    commond.CommandText = sql;
                    IDataReader reader = commond.ExecuteReader();

                    List<string[]> indexData = new List<string[]>();
                    while (reader.Read())
                    {
                        var data = new List<string>();
                        data.Add(DateTime.Parse(reader["date"] + "").ToString("yyyyMMdd"));

                        data.AddRange(fields.Select(filed => reader[filed.name] + ""));

                        indexData.Add(data.ToArray());
                    }
                    entity.Database.Connection.Close();
                    indexData.Reverse();
                    return indexData.ToArray();
                }
            }
        }

        public IList<index_user_map> GetMyIndexs(string userId)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var index = entity.index_user_map
                    .Include("indexdefinition")
                    .Where(p => p.user_id == userId)
                    .ToList();
                return index;
            }
        }

        /// <summary>
        /// 同步自选股
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="indexCodeList"></param>
        /// <returns></returns>
        public bool SyncMyIndex(string userId, IList<index_user_map> indexCodeList)
        {

            int count = indexCodeList.Count(p => string.IsNullOrEmpty(p.user_id) || string.IsNullOrEmpty(p.index_code));
            if (count > 0)
            {
                throw new Exception("缺失属性user_id和index_code");
            }
            //新增
            IList<string> newCodeList = indexCodeList.Select(p => p.index_code).ToList();

            using (StockManDBEntities entity = new StockManDBEntities())
            {

                var oldMapList = (from t in entity.index_user_map
                                  where t.user_id == userId
                                  select t).ToList();

                var oldCodeList = (from t in oldMapList
                                   select t.index_code).ToList();
                var addCodeList = newCodeList.Except(oldCodeList).ToList();
                var deleteCodeList = oldCodeList.Except(newCodeList).ToList();
                var updateCodeLIst = newCodeList.Intersect(oldCodeList).ToList();

                List<index_user_map> addMapList = indexCodeList.Where(p => addCodeList.Contains(p.index_code)).ToList();

                foreach (var add in addMapList)
                {
                    entity.index_user_map.Add(add);
                }


                List<index_user_map> updateMapList = indexCodeList.Where(p => updateCodeLIst.Contains(p.index_code)).ToList();
                index_user_map oldMap = null;
                foreach (var map in updateMapList)
                {
                    oldMap = oldMapList.First(p => p.index_code == map.index_code);
                    oldMap.sort = map.sort;
                }

                List<index_user_map> removeMapList = oldMapList.Where(p => deleteCodeList.Contains(p.index_code)).ToList();

                foreach (var r in removeMapList)
                {
                    entity.index_user_map.Remove(r);
                }


                entity.SaveChanges();

                return true;
            }
        }

        public indexdefinegroup AddGroup(indexdefinegroup group)
        {
            if (string.IsNullOrEmpty(group.code) || string.IsNullOrEmpty(group.group_name))
                return null;

            using (StockManDBEntities entity = new StockManDBEntities())
            {
                entity.indexdefinegroup.Add(group);
                entity.SaveChanges();
                return group;
            }
        }

        public void DeleteGroup(string code)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var obj = entity.indexdefinegroup.FirstOrDefault(p => p.code == code);
                if (obj != null)
                {
                    entity.indexdefinegroup.Remove(obj);
                    entity.SaveChanges();

                }
            }
        }

        public IList<indexdefinegroup> GetGroups()
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.indexdefinegroup.ToList();
            }
        }

        public IList<objectstate> GetObjectStates(IList<string> codeList)
        {
            if (codeList == null || codeList.Count == 0)
                return new List<objectstate>();
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.objectstate.Where(p => codeList.Contains(p.code)).ToList();
            }
        }



        public void AddTechByDay(string table, IList<string> fields, string objectCode, IList<IndexData> result)
        {
            string str = "";
            foreach (string f in fields)
            {
                if (str.Length == 0)
                    str = f;
                else
                    str += "," + f;
            }

            string sqlTemplate = string.Format(@"insert into {0} (code,f_code,date,{1})values", table, str) + "({0});";
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string batchSql = string.Empty;
                int batchNum = 50,i=0;
                foreach (IndexData ds in result)
                {
                    string code = objectCode + "_" + ds.date.ToString("yyyyMMdd");

                    string exist = string.Format(@"select count(0) from {0} where code='{1}';", table, code);
                    var count = entity.Database.SqlQuery<int>(exist).First();
                    if (count > 0)
                    {
                        entity.Database.ExecuteSqlCommand(string.Format(@"delete from {0} where code='{1}';", table, code));
                    }

                    //入库
                    //字段映射，0->k,1->d,2->j
                    string v = string.Format("'{0}','{1}','{2}'", code, objectCode, ds.date.ToString("yyyy-MM-dd"));

                    foreach (double d in ds)
                    {
                        v += ",'" + d + "'";
                    }
                    string sql = string.Format(sqlTemplate, v);
                    batchSql += sql;

                    if (++i % 50 == 0)
                    {
                        entity.Database.ExecuteSqlCommand(batchSql);
                        batchSql = string.Empty;
                    }
                }
                if (!string.IsNullOrEmpty(batchSql))
                {
                    entity.Database.ExecuteSqlCommand(batchSql);
                }
                
            }
        }

        public void AddTechByWeek(string table, IList<string> fields, string objectCode, IList<IndexData> result)
        {
            string str = "";
            foreach (string f in fields)
            {
                if (str.Length == 0)
                    str = f;
                else
                    str += "," + f;
            }

            string insertSqlTpl = string.Format(@"insert into {0} (code,f_code,date,{1})values", table, str) + "({0});";

            DateTime endDate = DateTime.Now.AddDays(DayOfWeek.Friday - DateTime.Now.DayOfWeek + 1);

            DateTime startDate = endDate.AddDays(-5);
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string batchSql = string.Empty;
                int batchNum = 50, i = 0;
                foreach (IndexData ds in result)
                {

                    string startCode = objectCode + "_" + startDate.ToString("yyyyMMdd");
                    string endCode = objectCode + "_" + endDate.ToString("yyyyMMdd");
                    string code = objectCode + "_" + ds.date.ToString("yyyyMMdd");
                    var countRow = entity.Database.SqlQuery<int>(string.Format("select count(0) from {0} a where a.code=@p0 or a.code between @p1 and @p2", table), code, startCode, endCode);
                    var count = countRow.First();

                    if (count > 0)
                    {
                        entity.Database.ExecuteSqlCommand(string.Format("delete from {0} where code=@p0 or code between @p1 and @p2", table), code, startCode, endCode);
                    }


                    //入库
                    //字段映射，0->k,1->d,2->j
                    string v = string.Format("'{0}','{1}','{2}'", code, objectCode, ds.date.ToString("yyyy-MM-dd"));

                    foreach (double d in ds)
                    {
                        v += ",'" + d + "'";
                    }
                    string insertSql = string.Format(insertSqlTpl, v);


                    batchSql += insertSql;

                    if (++i % 50 == 0)
                    {
                        entity.Database.ExecuteSqlCommand(batchSql);
                        batchSql = string.Empty;
                    }                 

                }
                if (!string.IsNullOrEmpty(batchSql))
                {
                    entity.Database.ExecuteSqlCommand(batchSql);
                }
            }

        }

        public void AddTechByMonth(string table, IList<string> fields, string objectCode, IList<IndexData> result)
        {
            string str = "";
            foreach (string f in fields)
            {
                if (str.Length == 0)
                    str = f;
                else
                    str += "," + f;
            }

            string insertSqlTpl = string.Format(@"insert into {0} (code,f_code,date,{1})values", table, str) + "({0});";

            DateTime nextMonthData = DateTime.Now.AddMonths(1);
            DateTime endDate = new DateTime(nextMonthData.Year, nextMonthData.Month, 1);
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string batchSql = string.Empty;
                int batchNum = 50, i = 0;
                foreach (IndexData ds in result)
                {

                    string startCode = objectCode + "_" + startDate.ToString("yyyyMMdd");
                    string endCode = objectCode + "_" + endDate.ToString("yyyyMMdd");
                    string code = objectCode + "_" + ds.date.ToString("yyyyMMdd");
                    var countRow = entity.Database.SqlQuery<int>(string.Format("select count(0) from {0} a where a.code=@p0 or a.code between @p0 and @p1", table), code, startCode, endCode);

                    var count = countRow.First();
                    if (count > 0)
                    {
                        entity.Database.ExecuteSqlCommand(string.Format("delete from {0} where code=@p0 or code between @p0 and @p1", table), code, startCode, endCode);
                    }
                    //入库
                    //字段映射，0->k,1->d,2->j
                    string v = string.Format("'{0}','{1}','{2}'", code, objectCode, ds.date.ToString("yyyy-MM-dd"));

                    foreach (double d in ds)
                    {
                        v += ",'" + d + "'";
                    }
                    string insertSql = string.Format(insertSqlTpl, v);

                    batchSql += insertSql;
                    if (++i % 50 == 0)
                    {
                        entity.Database.ExecuteSqlCommand(batchSql);
                        batchSql = string.Empty;
                    } 
                }
                if (!string.IsNullOrEmpty(batchSql))
                {
                    entity.Database.ExecuteSqlCommand(batchSql);
                }
            }

        }


        public IList<indexdefinition> GetIndexByCodes(string[] codes)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.indexdefinition.Where(p => codes.Contains(p.code)).ToList();
            }
        }
    }
}
