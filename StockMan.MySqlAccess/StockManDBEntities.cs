using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.EntityModel;
using System.Data;
namespace StockMan.MySqlAccess
{
    public partial class StockManDBEntities : DbContext
    {

        public DataTable ExecuteTable(string sql)
        {
            var entity = this;
            entity.Database.Connection.Open();
            using (entity.Database.Connection)
            {
                System.Data.IDbCommand commond = entity.Database.Connection.CreateCommand();
                commond.CommandText = sql;
                IDataReader reader = commond.ExecuteReader();
                DataTable dt = new DataTable();
                for (var n = 0; n < reader.FieldCount; n++)
                {
                    dt.Columns.Add(reader.GetName(n));
                }

                while (reader.Read())
                {
                    var row = dt.NewRow();
                    var count = reader.FieldCount;
                    for (var i = 0; i < count; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    dt.Rows.Add(row);
                }
                entity.Database.Connection.Close();
                return dt;
            }
        }

    }

}
