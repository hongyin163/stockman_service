using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockMan.MySqlAccess;
using StockMan.EntityModel;
using StockMan.Service.Interface;
using StockMan.Service.Interface.Rds;

namespace StockMan.Service.Rds
{
    public class UserService : ServiceBase<users>, IUserService
    {

        public sys_userconfig GetUserConfig(string userId)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.sys_userconfig.Find(userId);
            }
        }

        public void SaveUserConfig(sys_userconfig config)
        {
            if (string.IsNullOrEmpty(config.code))
                throw new Exception("参数异常");

            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var query = entity.sys_userconfig.Find(config.code);
                if (query == null)
                {
                    entity.sys_userconfig.Add(config);
                }
                else
                {
                    query.config = config.config;
                }

                entity.SaveChanges();
            }
        }

        public long GetUserCount()
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.users.Count();
            }
        }


        public IList<users> GetUserWithDataList(int pageSize, int pageIndex)
        {
            int skip = pageIndex * pageSize;
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.users
                    .Include("stock_user_map")
                    .Include("object_user_map")
                    .Include("index_user_map")
                    .OrderBy(p=>p.id)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();
            }
        }

        public users GetUserWithDataList(string id)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.users
                    .Include("stock_user_map")
                    .Include("object_user_map")
                    .Include("index_user_map")
                    .FirstOrDefault(p => p.id == id);
                
            }
        }
    }
}
