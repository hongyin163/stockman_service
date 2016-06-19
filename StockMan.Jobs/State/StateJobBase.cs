using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.MySqlAccess;

namespace StockMan.Jobs.State
{
    public class StateJobBase
    {
        IIndexService indexService = new IndexService();
        protected IList<IndexData> GetLastIndexData(stock stock, IndexDefinitionInfo define)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string sql = @"select * from " + define.table_name + " where f_code='" + stock.code + "' order by code desc limit 50";

                entity.Database.Connection.Open();
                using (entity.Database.Connection)
                {
                    System.Data.IDbCommand commond = entity.Database.Connection.CreateCommand();
                    commond.CommandText = sql;
                    IDataReader reader = commond.ExecuteReader();

                    IList<IndexData> list = new List<IndexData>();
                    while (reader.Read())
                    {

                        IndexData id = new IndexData();
                        id.date = DateTime.Parse(reader["date"] + "");
                        foreach (var filed in define.fields)
                        {
                            id.Add(double.Parse(reader[filed.name] + ""));
                        }

                        list.Add(id);
                    }
                    entity.Database.Connection.Close();
                    return list;
                }
            }
        }
        protected IList<IndexDefinitionInfo> index_definition_infos(TechCycle cycle)
        {
            var list = indexService.FindAll();
            this.Log().Info(list.Count + "");
            IList<IndexDefinitionInfo> defineList = list.Select(define => new IndexDefinitionInfo
            {
                code = define.code,
                name = define.name,
                fields = JsonConvert.DeserializeObject<IList<Field>>(define.fields),
                parameter =define.parameter,
                table_name = "Tech_Stock_" + define.table_name + "_" + cycle.ToString(),
                algorithm_script = define.algorithm_script
            }).ToList();
            return defineList;
        }

    }
}
