using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StockMan.EntityModel;
using StockMan.MySqlAccess;
using StockMan.Service.Interface.Rds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.Common;
using System.Data;
namespace StockMan.Service.Rds
{
    public class RelatedDataService : ServiceBase<related_object_define>, IRelatedDataService
    {

        public override related_object_define Find(string code)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.related_object_define.Include("related_object_fields").Where(p => p.code == code).FirstOrDefault();
            }
        }
        /// <summary>
        /// {date:'',a:0,b:0}
        /// </summary>
        /// <param name="defineCode"></param>
        /// <param name="data"></param>
        public void InsertData(string defineCode, string jsonData)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var data = JsonConvert.DeserializeObject(jsonData) as JObject;
                var define = entity.related_object_define
                    .Include("related_object_fields").FirstOrDefault(p => p.code == defineCode);
                string sql = @"insert into related_object_data (code,type,date,createtime,{{0}}) values ('{0}','{1}','{2}','{3}',{{1}})"
                    .Format(Guid.NewGuid(), defineCode, data["date"], DateTime.Now);

                string fieldStr = "", fieldValue = "";
                foreach (var field in define.related_object_fields)
                {
                    if (fieldStr.Length == 0)
                        fieldStr = "pro" + field.map;
                    else
                        fieldStr += ",pro" + field.map;

                    if (fieldValue.Length == 0)
                    {
                        fieldValue = "'" + data[field.name] + "'";
                    }
                    else
                    {
                        fieldValue += ",'" + data[field.name] + "'";
                    }

                }
                sql = string.Format(sql, fieldStr, fieldValue);
                entity.Database.ExecuteSqlCommand(sql);

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="defineCode"></param>
        /// <returns>[{date:'',a:1,b:1},{date:'',a:1,b:1}]</returns>
        public string GetData(string defineCode)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var define = entity.related_object_define
                    .Include("related_object_fields").FirstOrDefault(p => p.code == defineCode);
                string sql = @"select date,{0} from  related_object_data where type='{1}' order by 'date'";

                string fieldStr = "", fieldName = "";
                IList<related_object_fields> fields = define.related_object_fields.OrderBy(p => p.map).ToList();
                foreach (var field in fields)
                {
                    if (fieldStr.Length == 0)
                        fieldStr = "pro" + field.map + " as '" + field.name + "'";
                    else
                        fieldStr += ",pro" + field.map + " as '" + field.name + "'";

                    if (fieldName.Length == 0)
                        fieldName = "{name:'" + field.name + "',mapping:" + (field.map + 1) + ",display:'" + field.description + "'}";
                    else
                        fieldName += ",{name:'" + field.name + "',mapping:" + (field.map + 1) + ",display:'" + field.description + "'}";
                }
                fieldName = "[{name:'date',mapping:0}," + fieldName + "]";

                sql = string.Format(sql, fieldStr, defineCode);
                entity.Database.Connection.Open();
                using (entity.Database.Connection)
                {
                    System.Data.IDbCommand commond = entity.Database.Connection.CreateCommand();
                    commond.CommandText = sql;
                    IDataReader reader = commond.ExecuteReader();

                    IList<string[]> indexData = new List<string[]>();
                    string dataStr = "";
                    while (reader.Read())
                    {
                        //var data = new List<string>();
                        //data.Add(DateTime.Parse(reader["date"] + "").ToString("yyyy-MM-dd"));

                        //data.AddRange(fields.Select(filed => reader[filed.name] + ""));

                        //indexData.Add(data.ToArray());
                        string fstr = "'" + DateTime.Parse(reader["date"] + "").ToString("yyyyMMdd") + "'";
                        foreach (var f in fields)
                        {
                            fstr += "," + reader[f.name];
                        }
                        fstr = "[" + fstr + "]";

                        if (dataStr.Length == 0)
                            dataStr = fstr;
                        else
                            dataStr += "," + fstr;
                    }
                    dataStr = "[" + dataStr + "]";
                    string result = string.Format("{{name:'{2}',fields:{0},data:{1}}}", fieldName, dataStr, define.name);

                    //entity.Database.Connection.Close();

                    return result;
                }
            }
        }



        public IList<customobject> GetDataByCode(string codes)
        {
            var codeList = codes.Split(',');
            IList<customobject> resultList = new List<customobject>();
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                //var list = entity.related_object_define
                //    .Where(p => codeList.Contains(p.code))
                //    .Select(p => new customobject
                //    {
                //        date=DateTime.Now,
                //        code = p.code,
                //        name = p.name,
                //        price = p.value,
                //        yestclose = p.last_value
                //    }).ToList();
             
                string[] idArray = codes.Split(',');
                string idsStr = string.Empty;
                foreach (string id in idArray)
                {
                    if (idsStr.Length == 0)
                        idsStr = "'" + id + "'";
                    else
                        idsStr += ",'" + id + "'";
                }

                string sql = string.Format(@"SELECT code,
                        description as name,
                        value as price,
                        last_value as yestclose,    
                        '{1}' as date ,
                        0 as state,
                        0 as open ,
                        0 as low ,
                        0 as updown ,
                        0 as volume ,
                        0 as turnover ,
                        0 as high ,
                        0 as percent 
                    FROM `related_object_define` where code in ({0})", idsStr, DateTime.Now.ToString("yyyy-MM-dd"));
                return entity.Database.SqlQuery<customobject>(sql).ToList();
                //return list;
            }
        }
    }
}
