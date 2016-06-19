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
    public class StockCategoryService : ServiceBase<stockcategory>, IStockCategoryService
    {
        public void UpdateCategoryPrice(IList<PriceInfo> spiList)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {

                string sql = string.Empty;
                foreach (PriceInfo p in spiList)
                {
                    // code = item.code,
                    //date = today,
                    //price = index,
                    //high = 0,
                    //low = 0,
                    //yestclose = yestclose,
                    //volume = vol,
                    //turnover = turnover
                    sql += string.Format(@"UPDATE `stockcategory` SET                      
                            `price` = '{0}',
                            `yestclose` ='{1}',
                            `volume` = '{2}',
                            `turnover` = '{3}',                           
                            `high` = '{4}',                           
                            `low` ='{5}',                         
                            `date` = '{6}',
                            `percent` = '{7}'
                            WHERE code = '{8}';", p.price, p.yestclose, p.volume, p.turnover,
                                                 p.high, p.low, p.date.ToString("yyyy-MM-dd"),p.percent, p.code);

                }
                entity.Database.ExecuteSqlCommand(sql);
            }
        }
        public void AddCategoryGroup(stock_category_group group)
        {
            if (group == null)
                return;
            if (string.IsNullOrEmpty(group.code))
                throw new Exception("行情分类编码不能为空");

            using (StockManDBEntities entity = new StockManDBEntities())
            {
                if (entity.stock_category_group.Count(p => p.code == group.code) > 0)
                    return;
                entity.stock_category_group.Add(group);
                entity.SaveChanges();
            }
         
        }


        public IList<stockcategory> GetCategoryList(string group_code)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.stockcategory
                    .Where(p => p.group_code == group_code)
                    .ToList();
            }
        }


        public void AddMyCategory(stockcategory_user_map data)
        {
            if (data == null
                || string.IsNullOrEmpty(data.cate_code)
                || string.IsNullOrEmpty(data.user_id))
                throw new Exception("参数不合规");
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                entity.stockcategory_user_map.Add(data);
                entity.SaveChanges();
            }
        }


        public void AddMyCategory(IList<stockcategory_user_map> cate_user)
        {
            if (cate_user == null
                || cate_user.Count() == 0
                || cate_user.Count(p => string.IsNullOrEmpty(p.cate_code) || string.IsNullOrEmpty(p.user_id)) > 0
                )
                throw new Exception("参数不合规");

            string user_id = cate_user[0].user_id;
            string group = cate_user[0].group_code;
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var oldList = entity.stockcategory_user_map
                    .Where(p => p.user_id == user_id && p.group_code == group)
                    .ToList();
                //新增
                var addList = cate_user.Except(oldList, new Category_User_Comparer());
                foreach (var add in addList)
                {
                    entity.stockcategory_user_map.Add(add);
                }
              
                //删除
                var delList = oldList.Except(cate_user, new Category_User_Comparer());
                foreach (var del in delList)
                {
                    entity.stockcategory_user_map.Remove(del);
                }
              

                entity.SaveChanges();
            }
        }


        public IList<stockcategory> GetMyCategory(string user_id, string group_code)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                //var list = entity.stockcategory_user_map.Include("stockcategory")
                //     .Where(p => p.user_id == user_id && p.group_code == group_code)
                //     .Select(p => p.stockcategory).ToList();
                return null;
            }
        }

        public void UpdateMyCateVersion(string user_id, decimal version)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var list = entity.userdataversion
                      .Where(p => p.user_id == user_id && p.code == "my_category")
                      .FirstOrDefault();
                if (list != null)
                {
                    list.version = version;
                    entity.SaveChanges();
                }
            }
        }

        public IList<stockcategory> GetCategoryByCode(string codes)
        {
            var codeList = codes.Split(',');
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var list = entity.stockcategory
                      .Where(p => codeList.Contains(p.code))
                      .ToList();
                return list;
            }
        }

        public IList<PriceInfo> GetPriceInfo(string code, TechCycle type)
        {
            string sql = string.Format(@"SELECT code
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
                  FROM Data_Category_{0}_Latest where object_code='{1}'", type, code);

            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<PriceInfo>(sql).ToList();
            }
        }
        public IList<PriceInfo> GetPriceInfo(string categoryCode, TechCycle type, DateTime date)
        {
            string sql = string.Format(@"SELECT code
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
                  FROM Data_Category_{0}_Latest where object_code='{1}' and date='{2}'", type, categoryCode, date.ToString("yyyy-MM-dd"));

            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<PriceInfo>(sql).ToList();
            }
        }


        public void AddPriceInfo(string categoryCode, PriceInfo info, TechCycle cycle)
        {

            using (var entity = new StockManDBEntities())
            {
                switch (cycle)
                {
                    case TechCycle.day:
                        entity.data_category_day_latest.Add(new data_category_day_latest()
                        {
                            object_code = categoryCode,
                            code = categoryCode + "_" + info.date.ToString("yyyyMMdd"),
                            date = info.date,
                            high = info.high,
                            low = info.low,
                            open = info.open,
                            price = info.price,
                            yestclose = info.yestclose,
                            volume = info.volume
                        });
                        break;
                    case TechCycle.week:
                        entity.data_category_week_latest.Add(new data_category_week_latest()
                        {
                            object_code =  categoryCode,
                            code = categoryCode + "_" + info.date.ToString("yyyyMMdd"),
                            date = info.date,
                            high = info.high,
                            low = info.low,
                            open = info.open,
                            price = info.price,
                            yestclose = info.yestclose,
                            volume = info.volume
                        });
                        break;
                    case TechCycle.month:
                        entity.data_category_month_latest.Add(new data_category_month_latest()
                        {
                            object_code = categoryCode,
                            code = categoryCode + "_" + info.date.ToString("yyyyMMdd"),
                            date = info.date,
                            high = info.high,
                            low = info.low,
                            open = info.open,
                            price = info.price,
                            yestclose = info.yestclose,
                            volume = info.volume
                        });
                        break;
                }
                entity.SaveChanges();
            }
        }

        #region oldCode
        /*
        public void AddPriceByDay(IList<PriceInfo> priceList)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                IList<Data_Category_Day_Latest> list = new List<Data_Category_Day_Latest>();
                foreach (PriceInfo p in priceList)
                {
                    string code = p.code + "_" + p.date.ToString("yyyyMMdd");
                    if (entity.Data_Category_Day_Latest.Count(s => s.code == code) <= 0)
                    {
                        list.Add(new Data_Category_Day_Latest
                        {
                            code = code,
                            date = p.date,//new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(p.date.Substring(6))),
                            price = p.price,
                            yestclose = p.yestclose,
                            high = p.high,
                            low = p.low,
                            open = p.open,
                            percent = p.percent,
                            object_code = p.code,
                            updown = p.updown,
                            volume = p.volume
                        });
                    }
                }


                entity.Data_Category_Day_Latest.AddRange(list);
                try
                {
                    entity.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void AddPriceByWeek(IList<PriceInfo> priceList)
        {
            throw new NotImplementedException();
        }

        public void AddPriceByMonth(IList<PriceInfo> priceList)
        {
            throw new NotImplementedException();
        } */
        #endregion
    }

    public class Category_User_Comparer : IEqualityComparer<stockcategory_user_map>
    {

        public bool Equals(stockcategory_user_map x, stockcategory_user_map y)
        {
            if (x.cate_code == y.cate_code && x.group_code == y.group_code)
                return true;
            return false;
        }

        public int GetHashCode(stockcategory_user_map obj)
        {
            return obj.cate_code.GetHashCode();
        }
    }
}
