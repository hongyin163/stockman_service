using StockMan.MySqlAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Service.Rds
{
    public static class CommonService
    {
        public static void ClearTable(string tableName)
        {
            using (var entity = new StockManDBEntities())
            {
                try
                {
                    entity.Database.ExecuteSqlCommand(string.Format("DELETE FROM '{0}'", tableName));
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
