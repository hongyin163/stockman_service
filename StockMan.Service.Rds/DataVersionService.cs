using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.MySqlAccess;
using StockMan.EntityModel;
using StockMan.Service.Interface.Rds;

namespace StockMan.Service.Rds
{
    public class UserDataVersionService : ServiceBase<userdataversion>, IUserDataVersionService
    {

        public decimal GetUserDataVersion(string user_id, string version_code)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var verson = entity.userdataversion
                    .Where(p => p.user_id == user_id && p.code == version_code)
                    .FirstOrDefault();
                if (verson != null)
                    return verson.version ?? 0;
                return 0;
            }
        }

        public void UpdateUserDataVersion(string user_id, string version_code,decimal ver)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var verson = entity.userdataversion
                    .Where(p => p.user_id == user_id && p.code == version_code)
                    .FirstOrDefault();
                if (verson != null)
                {
                    verson.version = ver;
                    verson.update_time = DateTime.Now;
                }
                else
                {
                    userdataversion v=new userdataversion();
                    v.code = version_code;
                    v.user_id = user_id;
                    v.version = ver;
                    v.update_time = DateTime.Now;
                    entity.userdataversion.Add(v);
                }
                entity.SaveChanges();
            }
        }
    }
}
