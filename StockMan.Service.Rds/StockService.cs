using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using StockMan.MySqlAccess;
using StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.EntityModel.dto;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Data;
namespace StockMan.Service.Rds
{
    public class StockService : ServiceBase<stock>, IStockService
    {
        /// <summary>
        /// 同步自选股
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="stocks"></param>
        /// <returns></returns>
        public bool SyncMyStock(string userId, IList<stock_user_map> stockCodeList)
        {

            int count = stockCodeList.Count(p => string.IsNullOrEmpty(p.user_id) || string.IsNullOrEmpty(p.stock_code));
            if (count > 0)
            {
                throw new Exception("缺失属性user_id和stock_code");
            }
            //新增
            IList<string> newCodeList = stockCodeList.Select(p => p.stock_code).ToList();

            using (StockManDBEntities entity = new StockManDBEntities())
            {

                var oldMapList = (from t in entity.stock_user_map
                                  where t.user_id == userId
                                  select t).ToList();

                var oldCodeList = (from t in oldMapList
                                   select t.stock_code).ToList();
                var addCodeList = newCodeList.Except(oldCodeList).ToList();
                var deleteCodeList = oldCodeList.Except(newCodeList).ToList();
                var updateCodeLIst = newCodeList.Intersect(oldCodeList).ToList();

                List<stock_user_map> addMapList = stockCodeList.Where(p => addCodeList.Contains(p.stock_code)).ToList();

                foreach (var addmap in addMapList)
                {
                    entity.stock_user_map.Add(addmap);
                }


                List<stock_user_map> updateMapList = stockCodeList.Where(p => updateCodeLIst.Contains(p.stock_code)).ToList();
                stock_user_map oldMap = null;
                foreach (var map in updateMapList)
                {
                    oldMap = oldMapList.First(p => p.stock_code == map.stock_code);
                    oldMap.inhand = map.inhand;
                    oldMap.sort = map.sort;
                    oldMap.group_name = map.group_name;
                }

                List<stock_user_map> removeMapList = oldMapList.Where(p => deleteCodeList.Contains(p.stock_code)).ToList();

                foreach (var r in removeMapList)
                {
                    entity.stock_user_map.Remove(r);
                }

                entity.SaveChanges();

                return true;
            }
        }

        /// <summary>
        /// 获取我的自选股
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IList<stock_user_map> GetMyStock(string userId)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var list = entity.stock_user_map
                      .Include("stock")
                      .Where(p => p.user_id == userId)
                      .ToList();
                return list;
            }
        }


        public decimal GetMyStockVersion(string user_id)
        {
            IUserDataVersionService service = new UserDataVersionService();
            return service.GetUserDataVersion(user_id, "my_stock");
        }

        public void UpdateMyStockVersion(string user_id, decimal version)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var list = entity.userdataversion
                      .Where(p => p.user_id == user_id && p.code == "my_stock")
                      .FirstOrDefault();
                if (list != null)
                {
                    list.version = version;
                    entity.SaveChanges();
                }
            }
        }


        public IList<stock> GetStockByCategory(string cateCode)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var query = from c in entity.stock_category_map
                            join s in entity.stock on c.stock_code equals s.code
                            where c.cate_code == cateCode
                            select s;

                return query.ToList();
            }
        }

        public IList<stock> GetStockByCategory(string cateCode, int pageNum, int pageSize)
        {
            var skip = (pageNum - 1) * pageSize;
            var take = pageSize;
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var query = (from c in entity.stock_category_map
                             join s in entity.stock on c.stock_code equals s.code
                             where c.cate_code == cateCode
                             orderby s.percent descending
                             select s).Skip(skip).Take(take);

                return query.ToList();
            }
        }

        public void UpdateStockPrice(string code, StockInfo sprice)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                //var stock = entity.stock.FirstOrDefault(p => p.code == code);
                //stock.price = sprice.price;
                //stock.yestclose = sprice.yestclose;
                //stock.volume = sprice.volume;
                //stock.mv = sprice.mv;
                //stock.pb = sprice.pb;
                //stock.pe = sprice.pe;
                //stock.low = sprice.low;
                //stock.high = sprice.high;
                //stock.fv = sprice.fv;
                //stock.turnover = sprice.turnover;
                //stock.turnoverrate = sprice.turnoverrate;
                //stock.updown = sprice.updown;
                //stock.percent = sprice.percent;

                string sql = @"UPDATE `stock`
                            SET                      
                            `price` = @price,
                            `yestclose` = @yestclose,
                            `volume` = @volume,
                            `turnover` = @turnover,
                            `open` = @open,
                            `high` = @high,
                            `updown` = @updown,
                            `low` = @low,
                            `turnoverrate` = @turnoverrate,
                            `pe` = @pe,
                            `pb` = @pb,
                            `fv` = @fv,
                            `mv` = @mv,
                            `percent` = @percent
                            `date` = @date
                            WHERE code = @code;
                            ";


                entity.Database.ExecuteSqlCommand(sql,
                    new MySqlParameter("@price", sprice.price),
                    new MySqlParameter("@yestclose", sprice.yestclose),
                    new MySqlParameter("@volume", sprice.volume),
                    new MySqlParameter("@turnover", sprice.turnover),
                    new MySqlParameter("@high", sprice.high),
                    new MySqlParameter("@updown", sprice.updown),
                    new MySqlParameter("@low", sprice.low),
                    new MySqlParameter("@turnoverrate", sprice.turnoverrate),
                    new MySqlParameter("@pe", sprice.pe),
                    new MySqlParameter("@pb", sprice.pb),
                    new MySqlParameter("@fv", sprice.fv),
                    new MySqlParameter("@mv", sprice.mv),
                    new MySqlParameter("@percent", sprice.percent),
                    new MySqlParameter("@open", sprice.open),
                     new MySqlParameter("@date", sprice.date),
                    new MySqlParameter("@code", code)
                );
            }
        }
        public void UpdateStockPrice(IList<StockInfo> spiList)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                //var stock = entity.stock.FirstOrDefault(p => p.code == code);
                //stock.price = sprice.price;
                //stock.yestclose = sprice.yestclose;
                //stock.volume = sprice.volume;
                //stock.mv = sprice.mv;
                //stock.pb = sprice.pb;
                //stock.pe = sprice.pe;
                //stock.low = sprice.low;
                //stock.high = sprice.high;
                //stock.fv = sprice.fv;
                //stock.turnover = sprice.turnover;
                //stock.turnoverrate = sprice.turnoverrate;
                //stock.updown = sprice.updown;
                //stock.percent = sprice.percent;
                string sql = string.Empty;
                foreach (StockInfo p in spiList)
                {
                    sql += string.Format(@"UPDATE `stock` SET                      
                            `price` = '{0}',
                            `yestclose` ='{1}',
                            `volume` = '{2}',
                            `turnover` = '{3}',                           
                            `high` = '{4}',
                            `updown` = '{5}',
                            `low` ='{6}',
                            `turnoverrate` = '{7}',
                            `pe` = '{8}',
                            `pb` ='{9}',
                            `fv` = '{10}',
                            `mv` = '{11}',
                            `percent` = '{12}',
                            `open` = '{13}',
                            `date` = '{14}'
                            WHERE code = '{15}';", p.price, p.yestclose, p.volume, p.turnover,
                                                 p.high, p.updown, p.low, p.turnoverrate,
                                                 p.pe, p.pb, p.fv, p.mv, p.percent, p.open, p.date, p.stock_code);

                }
                entity.Database.ExecuteSqlCommand(sql);
            }
        }

        /*
        public void AddPriceByDay(IList<PriceInfo> price)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                IList<Data_Stock_Day_Latest> list = new List<Data_Stock_Day_Latest>();
                foreach (PriceInfo p in price)
                {
                    string code = p.code + "_" + p.date.ToString("yyyyMMdd");
                    if (entity.Data_Stock_Day_Latest.Count(s => s.code == code) <= 0)
                    {
                        list.Add(new Data_Stock_Day_Latest
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


                entity.Data_Stock_Day_Latest.AddRange(list);
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

        public void AddPriceByWeek(IList<PriceInfo> price)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {

                IList<Data_Stock_Week_Latest> list = new List<Data_Stock_Week_Latest>();

                DateTime endDate = DateTime.Now.AddDays(DayOfWeek.Friday - DateTime.Now.DayOfWeek);

                DateTime startDate = endDate.AddDays(-5);

                foreach (PriceInfo p in price)
                {
                    string startCode = p.code + "_" + startDate.ToString("yyyyMMdd");
                    string endCode = p.code + "_" + endDate.ToString("yyyyMMdd");
                    string code = p.code + "_" + p.date.ToString("yyyyMMdd");

                    var countRow = entity.Database.SqlQuery<int>(
                        "select count(0) from Data_Stock_Week_Latest a where a.code between @p0 and @p1", startCode,
                        endCode);

                    if (countRow.First() <= 0)
                    {
                        list.Add(new Data_Stock_Week_Latest
                        {
                            code = code,
                            date = p.date,// new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(p.date.Substring(6))),
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
                    else
                    {
                        entity.Database.ExecuteSqlCommand("delete from Data_Stock_Week_Latest where code between @p0 and @p1", startCode, endCode);

                        list.Add(new Data_Stock_Week_Latest
                        {
                            code = code,
                            date = p.date,// new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(p.date.Substring(6))),
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
                entity.Data_Stock_Week_Latest.AddRange(list);
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

        public void AddPriceByMonth(IList<PriceInfo> price)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                DateTime nextMonthData = DateTime.Now.AddMonths(1);
                DateTime endDate = new DateTime(nextMonthData.Year, nextMonthData.Month, 1);
                DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                IList<Data_Stock_Month_Latest> list = new List<Data_Stock_Month_Latest>();
                foreach (PriceInfo p in price)
                {
                    string startCode = p.code + "_" + startDate.ToString("yyyyMMdd");
                    string endCode = p.code + "_" + endDate.ToString("yyyyMMdd");
                    string code = p.code + "_" + p.date.ToString("yyyyMMdd");

                    var countRow = entity.Database.SqlQuery<int>("select count(0) from Data_Stock_Month_Latest a where a.code between @p0 and @p1", startCode, endCode);

                    if (countRow.First() <= 0)
                    {
                        list.Add(new Data_Stock_Month_Latest
                        {
                            code = code,
                            date = p.date,// new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(p.date.Substring(6))),
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
                    else
                    {
                        entity.Database.ExecuteSqlCommand("delete from Data_Stock_Month_Latest where code between @p0 and @p1", startCode, endCode);

                        list.Add(new Data_Stock_Month_Latest
                        {
                            code = code,
                            date = p.date,
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
                entity.Data_Stock_Month_Latest.AddRange(list);
                entity.SaveChanges();
            }
        }
        */
        public void AddPriceSyncLog(pricesynclog log)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var t = entity.pricesynclog
                    .FirstOrDefault(p => p.batch == log.batch && p.stock_code == log.stock_code);

                if (t == null)
                {
                    log.state = 0;
                    entity.pricesynclog.Add(log);
                    entity.SaveChanges();
                }
                else
                {
                    t.state = 0;
                    entity.SaveChanges();
                }
            }
        }


        public IList<pricesynclog> GetPriceSyncLog(Guid batch, int state)
        {
            string batchId = batch.ToString();
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var query = from p in entity.pricesynclog
                            //join s in entity.stock on p.stock_code equals s.code
                            where p.batch == batchId && p.state == state
                            select p;

                return query.ToList();
            }
        }

        public void UpdatePriceSyncLog(Guid batch, string stock_code, int state)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string sql = @"UPDATE PriceSyncLog
                           SETstate = @state
                         WHERE batch=@batch and stock_code=@stock_code";
                entity.Database.ExecuteSqlCommand(sql,
                    new SqlParameter("@batch", batch),
                    new SqlParameter("@state", state),
                    new SqlParameter("@stock_code", stock_code));
            }
        }

        public IList<PriceInfo> GetStockPriceDayInfo(stock data)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var query = from t in entity.data_stock_day_latest
                            where t.object_code == data.code
                            select new PriceInfo
                            {
                                date = t.date,
                                code = t.object_code,
                                price = t.price ?? 0,
                                open = t.open ?? 0,
                                high = t.high ?? 0,
                                low = t.low ?? 0,
                                updown = t.updown ?? 0,
                                volume = t.volume ?? 0,
                                yestclose = t.yestclose ?? 0
                            };
                return query.ToList();
            }
        }
        public IList<PriceInfo> GetStockPriceWeekInfo(stock data)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var query = from t in entity.data_stock_week_latest
                            where t.object_code == data.code
                            select new PriceInfo
                            {
                                date = t.date,
                                code = t.object_code,
                                price = t.price ?? 0,
                                open = t.open ?? 0,
                                high = t.high ?? 0,
                                low = t.low ?? 0,
                                updown = t.updown ?? 0,
                                volume = t.volume ?? 0,
                                yestclose = t.yestclose ?? 0
                            };
                return query.ToList();
            }
        }
        public IList<PriceInfo> GetStockPriceMonthInfo(stock data)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var query = from t in entity.data_stock_month_latest
                            where t.object_code == data.code
                            select new PriceInfo
                            {
                                date = t.date,
                                code = t.object_code,
                                price = t.price ?? 0,
                                open = t.open ?? 0,
                                high = t.high ?? 0,
                                low = t.low ?? 0,
                                updown = t.updown ?? 0,
                                volume = t.volume ?? 0,
                                yestclose = t.yestclose ?? 0
                            };
                return query.ToList();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IList<stock> GetStocksByIds(string ids)
        {
            string[] idArray = ids.Split(',');
            string idsStr = string.Empty;
            foreach (string id in idArray)
            {
                if (idsStr.Length == 0)
                    idsStr = "'" + id + "'";
                else
                    idsStr += ",'" + id + "'";
            }
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string sql = string.Format(@"  SELECT code,
                                    symbol,
                                    spell,
                                    name,
                                    price,
                                    yestclose,
                                    volume,
                                    turnover,                                   
                                    high,
                                    updown,
                                    low,
                                    turnoverrate,
                                    pe,
                                    pb,
                                    fv,
                                    mv,
                                    percent,
                                    date,
                                    open
                                FROM `stock` where code in ({0})", idsStr);

                return entity.Database.SqlQuery<stock>(sql).ToList();

            }
        }

        public IList<PriceInfo> GetStockPriceByDate(string categoryCode, TechCycle cycle, DateTime datetime)
        {
            using (var entity = new StockManDBEntities())
            {
                string sql = @"SELECT a.date ,a.code ,a.price ,a.open ,a.high,a.low ,a.updown ,a.volume,a.yestclose,a.turnover,a.percent
                          FROM Data_Stock_{0}_Latest  a
                          inner join Stock_Category_Map b on a.object_code=b.stock_code
                          where b.cate_code='{1}' and date='{2}'";

                sql = string.Format(sql, cycle.ToString(), categoryCode, datetime.ToString("yyyy-MM-dd"));

                return entity.Database.SqlQuery<PriceInfo>(sql).ToList();

            }
        }

        public IList<PriceInfo> GetPriceInfo(string code, TechCycle cycle)
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
                  FROM Data_Stock_{0}_Latest where object_code='{1}' order by date", cycle, code);

            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<PriceInfo>(sql).ToList();
            }
        }

        public IList<StockQueryResult> FindStockBy(StockQueryCondition condition)
        {


            //if (string.IsNullOrEmpty(condition.user_id) || condition.user_id == "guest")
            return FindStockFromPoolBy(condition);
            /*
            string conditonSql = string.Format("`user_id`='{0}' ", condition.user_id);
            if (!string.IsNullOrEmpty(condition.cate))
            {
                var cateStr = string.Empty;
                string[] cates = condition.cate.Split(',');
                foreach (string ca in cates)
                {
                    if (cateStr.Length == 0)
                        cateStr = "'" + ca + "'";
                    else
                        cateStr += ",'" + ca + "'";
                }
                conditonSql += " and `cate_code` in (" + cateStr + ")";
            }
            if (!string.IsNullOrEmpty(condition.tech))
            {
                var techStr = string.Empty;
                string[] techs = condition.tech.Split(',');
                foreach (string te in techs)
                {
                    if (techStr.Length == 0)
                        techStr = "'" + te + "'";
                    else
                        techStr += ",'" + te + "'";
                }
                conditonSql += " and `index_code` in (" + techStr + ")";
            }

            string peMin, peMax, pbMin, pbMax, mvMin, mvMax, priceMin, priceMax;

            if (!string.IsNullOrEmpty(condition.pe) && condition.pe != "-1")
            {
                peMin = condition.pe.Split('-')[0];
                peMax = condition.pe.Split('-')[1];
                conditonSql += string.Format(" and `pe` between '{0}' and '{1}'", peMin, peMax);
            }
            if (!string.IsNullOrEmpty(condition.pb) && condition.pb != "-1")
            {
                pbMin = condition.pb.Split('-')[0];
                pbMax = condition.pb.Split('-')[1];
                conditonSql += string.Format(" and `pb` between '{0}' and '{1}'", pbMin, pbMax);
            }
            if (!string.IsNullOrEmpty(condition.mv) && condition.mv != "-1")
            {
                mvMin = condition.mv.Split('-')[0];
                mvMax = condition.mv.Split('-')[1];
                conditonSql += string.Format(" and `mv` between '{0}' and '{1}'", mvMin, mvMax);
            }
            if (!string.IsNullOrEmpty(condition.price) && condition.price != "-1")
            {
                priceMin = condition.price.Split('-')[0];
                priceMax = condition.price.Split('-')[1];
                conditonSql += string.Format(" and `price` between '{0}' and '{1}'", priceMin, priceMax);
            }





            using (var entity = new StockManDBEntities())
            {
                string sql = @"SELECT distinct
                            `object_code` as 'code',
                            `object_name` as 'name',
                            `price`,
                            `yestclose`,
                            `day`,
                            `week`,
                            `month`,
                            `last_day`,
                            `last_week`,
                            `last_month`,
                            `cate_name` as 'cate',
                            `index_name` as 'tech'                          
                        FROM `reco_user_stock` where " + conditonSql;

                return entity.Database.SqlQuery<StockQueryResult>(sql).ToList();

            }*/

        }


        public IList<StockQueryResult> FindStockFromPoolBy(StockQueryCondition condition)
        {
            //            string sql = string.Format(@"select a.code,a.name,a.price,a.yestclose,a.percent,b.cate_code as cate,o.index_code as tech 
            //                                from stock a
            //                                inner join Stock_Category_Map b on a.code=b.stock_code
            //                                --inner join StockCategory c on b.cate_code=c.code
            //                                inner join ObjectState o on a.code=o.object_code
            //                                where o.index_code in ({0}) and b.cate_code in ({1}) and 
            //                                (
            //                                (o.month =1 and o.week=1 and o.day=1)or 
            //                                (o.month=1 and o.week=1 and o.day<>1)or
            //                                (o.month=1 and o.week<>1 and o.day<>1)
            //                                )", condition.tech, condition.cate); ;
            string sql = @"select distinct a.object_code as code,a.object_name as name,a.price,a.yestclose,a.cate_name as cate,a.index_name as tech,a.day,a.week,a.month,a.last_day,a.last_week,a.last_month
                                from reco_stock_category_index a
                                where 1=1 {0} ";

            string conditonSql = "";
            if (!string.IsNullOrEmpty(condition.cate))
            {
                var cateStr = string.Empty;
                string[] cates = condition.cate.Split(',');
                foreach (string ca in cates)
                {
                    if (cateStr.Length == 0)
                        cateStr = "'" + ca + "'";
                    else
                        cateStr += ",'" + ca + "'";
                }
                conditonSql += " and a.`cate_code` in (" + cateStr + ")";
            }
            if (!string.IsNullOrEmpty(condition.tech))
            {
                var techStr = string.Empty;
                string[] techs = condition.tech.Split(',');
                foreach (string te in techs)
                {
                    if (techStr.Length == 0)
                        techStr = "'" + te + "'";
                    else
                        techStr += ",'" + te + "'";
                }
                conditonSql += " and a.`index_code` in (" + techStr + ")";
            }

            string peMin, peMax, pbMin, pbMax, mvMin, mvMax, priceMin, priceMax;

            if (!string.IsNullOrEmpty(condition.pe) && condition.pe != "-1")
            {
                peMin = condition.pe.Split('-')[0];
                peMax = condition.pe.Split('-')[1];
                conditonSql += string.Format(" and a.`pe` between '{0}' and '{1}'", peMin, peMax);
            }
            if (!string.IsNullOrEmpty(condition.pb) && condition.pb != "-1")
            {
                pbMin = condition.pb.Split('-')[0];
                pbMax = condition.pb.Split('-')[1];
                conditonSql += string.Format(" and a.`pb` between '{0}' and '{1}'", pbMin, pbMax);
            }
            if (!string.IsNullOrEmpty(condition.mv) && condition.mv != "-1")
            {
                mvMin = condition.mv.Split('-')[0];
                mvMax = condition.mv.Split('-')[1];
                conditonSql += string.Format(" and a.`mv` between '{0}' and '{1}'", mvMin, mvMax);
            }
            if (!string.IsNullOrEmpty(condition.price) && condition.price != "-1")
            {
                priceMin = condition.price.Split('-')[0];
                priceMax = condition.price.Split('-')[1];
                conditonSql += string.Format(" and a.`price` between '{0}' and '{1}'", priceMin, priceMax);
            }


            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<StockQueryResult>(string.Format(sql, conditonSql)).ToList();
            }
        }

        public IList<StockQueryResult> FindStockFromRankPoolBy(StockQueryCondition condition)
        {

            string sql = @"select distinct a.object_code as code,a.object_name as name,a.price,a.yestclose,a.cate_name as cate
                                from reco_stock_category_rank a 
                                where 1=1 {0} order by rank desc";

            string conditonSql = "";
            if (!string.IsNullOrEmpty(condition.cate))
            {
                var cateStr = string.Empty;
                string[] cates = condition.cate.Split(',');
                foreach (string ca in cates)
                {
                    if (cateStr.Length == 0)
                        cateStr = "'" + ca + "'";
                    else
                        cateStr += ",'" + ca + "'";
                }
                conditonSql += " and a.`cate_code` in (" + cateStr + ")";
            }


            string peMin, peMax, pbMin, pbMax, mvMin, mvMax, priceMin, priceMax;

            if (!string.IsNullOrEmpty(condition.pe) && condition.pe != "-1")
            {
                peMin = condition.pe.Split('-')[0];
                peMax = condition.pe.Split('-')[1];
                conditonSql += string.Format(" and a.`pe` between '{0}' and '{1}'", peMin, peMax);
            }
            if (!string.IsNullOrEmpty(condition.pb) && condition.pb != "-1")
            {
                pbMin = condition.pb.Split('-')[0];
                pbMax = condition.pb.Split('-')[1];
                conditonSql += string.Format(" and a.`pb` between '{0}' and '{1}'", pbMin, pbMax);
            }
            if (!string.IsNullOrEmpty(condition.mv) && condition.mv != "-1")
            {
                mvMin = condition.mv.Split('-')[0];
                mvMax = condition.mv.Split('-')[1];
                conditonSql += string.Format(" and a.`mv` between '{0}' and '{1}'", mvMin, mvMax);
            }
            if (!string.IsNullOrEmpty(condition.price) && condition.price != "-1")
            {
                priceMin = condition.price.Split('-')[0];
                priceMax = condition.price.Split('-')[1];
                conditonSql += string.Format(" and a.`price` between '{0}' and '{1}'", priceMin, priceMax);
            }


            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<StockQueryResult>(string.Format(sql, conditonSql)).ToList();
            }
        }


        public IList<StockQueryCount> FindStockCountFromRankPoolBy(StockQueryCondition condition)
        {

            string sql = @"select count( a.cate_code) as count,a.cate_code
                                from reco_stock_category_rank a 
                                where 1=1 {0} 
                                group by a.cate_code";

            string conditonSql = "";
            if (!string.IsNullOrEmpty(condition.cate))
            {
                var cateStr = string.Empty;
                string[] cates = condition.cate.Split(',');
                foreach (string ca in cates)
                {
                    if (cateStr.Length == 0)
                        cateStr = "'" + ca + "'";
                    else
                        cateStr += ",'" + ca + "'";
                }
                conditonSql += " and a.`cate_code` in (" + cateStr + ")";
            }


            string peMin, peMax, pbMin, pbMax, mvMin, mvMax, priceMin, priceMax;

            if (!string.IsNullOrEmpty(condition.pe) && condition.pe != "-1")
            {
                peMin = condition.pe.Split('-')[0];
                peMax = condition.pe.Split('-')[1];
                conditonSql += string.Format(" and a.`pe` between '{0}' and '{1}'", peMin, peMax);
            }
            if (!string.IsNullOrEmpty(condition.pb) && condition.pb != "-1")
            {
                pbMin = condition.pb.Split('-')[0];
                pbMax = condition.pb.Split('-')[1];
                conditonSql += string.Format(" and a.`pb` between '{0}' and '{1}'", pbMin, pbMax);
            }
            if (!string.IsNullOrEmpty(condition.mv) && condition.mv != "-1")
            {
                mvMin = condition.mv.Split('-')[0];
                mvMax = condition.mv.Split('-')[1];
                conditonSql += string.Format(" and a.`mv` between '{0}' and '{1}'", mvMin, mvMax);
            }
            if (!string.IsNullOrEmpty(condition.price) && condition.price != "-1")
            {
                priceMin = condition.price.Split('-')[0];
                priceMax = condition.price.Split('-')[1];
                conditonSql += string.Format(" and a.`price` between '{0}' and '{1}'", priceMin, priceMax);
            }


            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<StockQueryCount>(string.Format(sql, conditonSql)).ToList();
            }
        }

        public IList<StockCrossQueryResult> FindCrossStockBy(StockQueryCondition condition)
        {

            string sql = @"select distinct a.object_code as code,a.object_name as name,a.price,a.yestclose,a.cate_name as cate,a.tag_name as tag,a.index_code as tech,
                                a.day,a.week,a.month,a.last_day,a.last_week,a.last_month,a.cycle
                                from reco_stock_category_tag a
                                where 1=1 {0} ";

            string conditonSql = "";
            if (!string.IsNullOrEmpty(condition.cate))
            {
                var cateStr = string.Empty;
                string[] cates = condition.cate.Split(',');
                foreach (string ca in cates)
                {
                    if (cateStr.Length == 0)
                        cateStr = "'" + ca + "'";
                    else
                        cateStr += ",'" + ca + "'";
                }
                conditonSql += " and a.`cate_code` in (" + cateStr + ")";
            }
            if (!string.IsNullOrEmpty(condition.tech))
            {
                var techStr = string.Empty;
                string[] techs = condition.tech.Split(',');
                foreach (string te in techs)
                {
                    if (techStr.Length == 0)
                        techStr = "'" + te + "'";
                    else
                        techStr += ",'" + te + "'";
                }
                conditonSql += " and a.`index_code` in (" + techStr + ")";
            }
            if (!string.IsNullOrEmpty(condition.cycle))
            {
                conditonSql += " and a.`cycle` = '" + condition.cycle + "'";
            }
            string peMin, peMax, pbMin, pbMax, mvMin, mvMax, priceMin, priceMax;

            if (!string.IsNullOrEmpty(condition.pe) && condition.pe != "-1")
            {
                peMin = condition.pe.Split('-')[0];
                peMax = condition.pe.Split('-')[1];
                conditonSql += string.Format(" and a.`pe` between '{0}' and '{1}'", peMin, peMax);
            }
            if (!string.IsNullOrEmpty(condition.pb) && condition.pb != "-1")
            {
                pbMin = condition.pb.Split('-')[0];
                pbMax = condition.pb.Split('-')[1];
                conditonSql += string.Format(" and a.`pb` between '{0}' and '{1}'", pbMin, pbMax);
            }
            if (!string.IsNullOrEmpty(condition.mv) && condition.mv != "-1")
            {
                mvMin = condition.mv.Split('-')[0];
                mvMax = condition.mv.Split('-')[1];
                conditonSql += string.Format(" and a.`mv` between '{0}' and '{1}'", mvMin, mvMax);
            }
            if (!string.IsNullOrEmpty(condition.price) && condition.price != "-1")
            {
                priceMin = condition.price.Split('-')[0];
                priceMax = condition.price.Split('-')[1];
                conditonSql += string.Format(" and a.`price` between '{0}' and '{1}'", priceMin, priceMax);
            }


            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<StockCrossQueryResult>(string.Format(sql, conditonSql)).ToList();
            }
        }

        public IList<StockQueryCount> FindCrossStockCountBy(StockQueryCondition condition)
        {
            string sql = @"select  count(a.cycle) as count,a.cycle
                                from reco_stock_category_tag a
                                where 1=1 {0} 
                                group by a.cycle";

            string conditonSql = "";
            if (!string.IsNullOrEmpty(condition.cate))
            {
                var cateStr = string.Empty;
                string[] cates = condition.cate.Split(',');
                foreach (string ca in cates)
                {
                    if (cateStr.Length == 0)
                        cateStr = "'" + ca + "'";
                    else
                        cateStr += ",'" + ca + "'";
                }
                conditonSql += " and a.`cate_code` in (" + cateStr + ")";
            }
            if (!string.IsNullOrEmpty(condition.tech))
            {
                var techStr = string.Empty;
                string[] techs = condition.tech.Split(',');
                foreach (string te in techs)
                {
                    if (techStr.Length == 0)
                        techStr = "'" + te + "'";
                    else
                        techStr += ",'" + te + "'";
                }
                conditonSql += " and a.`index_code` in (" + techStr + ")";
            }

            string peMin, peMax, pbMin, pbMax, mvMin, mvMax, priceMin, priceMax;

            if (!string.IsNullOrEmpty(condition.pe) && condition.pe != "-1")
            {
                peMin = condition.pe.Split('-')[0];
                peMax = condition.pe.Split('-')[1];
                conditonSql += string.Format(" and a.`pe` between '{0}' and '{1}'", peMin, peMax);
            }
            if (!string.IsNullOrEmpty(condition.pb) && condition.pb != "-1")
            {
                pbMin = condition.pb.Split('-')[0];
                pbMax = condition.pb.Split('-')[1];
                conditonSql += string.Format(" and a.`pb` between '{0}' and '{1}'", pbMin, pbMax);
            }
            if (!string.IsNullOrEmpty(condition.mv) && condition.mv != "-1")
            {
                mvMin = condition.mv.Split('-')[0];
                mvMax = condition.mv.Split('-')[1];
                conditonSql += string.Format(" and a.`mv` between '{0}' and '{1}'", mvMin, mvMax);
            }
            if (!string.IsNullOrEmpty(condition.price) && condition.price != "-1")
            {
                priceMin = condition.price.Split('-')[0];
                priceMax = condition.price.Split('-')[1];
                conditonSql += string.Format(" and a.`price` between '{0}' and '{1}'", priceMin, priceMax);
            }


            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<StockQueryCount>(string.Format(sql, conditonSql)).ToList();
            }

        }


        public IList<StockCrossQueryResult> FindStateStockBy(StockQueryCondition condition)
        {

            string sql = @"select distinct a.object_code as code,a.object_name as name,a.price,a.yestclose,a.cate_name as cate,a.index_name as tag,a.index_code as tech,
                                a.day,a.week,a.month,a.last_day,a.last_week,a.last_month,a.cycle
                                from reco_stock_category_state a
                                where 1=1 {0} ";

            string conditonSql = "";
            if (!string.IsNullOrEmpty(condition.cate))
            {
                var cateStr = string.Empty;
                string[] cates = condition.cate.Split(',');
                foreach (string ca in cates)
                {
                    if (cateStr.Length == 0)
                        cateStr = "'" + ca + "'";
                    else
                        cateStr += ",'" + ca + "'";
                }
                conditonSql += " and a.`cate_code` in (" + cateStr + ")";
            }
            if (!string.IsNullOrEmpty(condition.tech))
            {
                var techStr = string.Empty;
                string[] techs = condition.tech.Split(',');
                foreach (string te in techs)
                {
                    if (techStr.Length == 0)
                        techStr = "'" + te + "'";
                    else
                        techStr += ",'" + te + "'";
                }
                conditonSql += " and a.`index_code` in (" + techStr + ")";
            }
            if (!string.IsNullOrEmpty(condition.cycle))
            {
                conditonSql += " and a.`cycle` = '" + condition.cycle + "'";
            }
            string peMin, peMax, pbMin, pbMax, mvMin, mvMax, priceMin, priceMax;

            if (!string.IsNullOrEmpty(condition.pe) && condition.pe != "-1")
            {
                peMin = condition.pe.Split('-')[0];
                peMax = condition.pe.Split('-')[1];
                conditonSql += string.Format(" and a.`pe` between '{0}' and '{1}'", peMin, peMax);
            }
            if (!string.IsNullOrEmpty(condition.pb) && condition.pb != "-1")
            {
                pbMin = condition.pb.Split('-')[0];
                pbMax = condition.pb.Split('-')[1];
                conditonSql += string.Format(" and a.`pb` between '{0}' and '{1}'", pbMin, pbMax);
            }
            if (!string.IsNullOrEmpty(condition.mv) && condition.mv != "-1")
            {
                mvMin = condition.mv.Split('-')[0];
                mvMax = condition.mv.Split('-')[1];
                conditonSql += string.Format(" and a.`mv` between '{0}' and '{1}'", mvMin, mvMax);
            }
            if (!string.IsNullOrEmpty(condition.price) && condition.price != "-1")
            {
                priceMin = condition.price.Split('-')[0];
                priceMax = condition.price.Split('-')[1];
                conditonSql += string.Format(" and a.`price` between '{0}' and '{1}'", priceMin, priceMax);
            }


            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<StockCrossQueryResult>(string.Format(sql, conditonSql)).ToList();
            }
        }

        public IList<StockQueryCount> FindStateStockCountBy(StockQueryCondition condition)
        {
            string sql = @"select  count(a.cycle) as count,a.cycle
                                from reco_stock_category_state a
                                where 1=1 {0} 
                                group by a.cycle";

            string conditonSql = "";
            if (!string.IsNullOrEmpty(condition.cate))
            {
                var cateStr = string.Empty;
                string[] cates = condition.cate.Split(',');
                foreach (string ca in cates)
                {
                    if (cateStr.Length == 0)
                        cateStr = "'" + ca + "'";
                    else
                        cateStr += ",'" + ca + "'";
                }
                conditonSql += " and a.`cate_code` in (" + cateStr + ")";
            }
            if (!string.IsNullOrEmpty(condition.tech))
            {
                var techStr = string.Empty;
                string[] techs = condition.tech.Split(',');
                foreach (string te in techs)
                {
                    if (techStr.Length == 0)
                        techStr = "'" + te + "'";
                    else
                        techStr += ",'" + te + "'";
                }
                conditonSql += " and a.`index_code` in (" + techStr + ")";
            }

            string peMin, peMax, pbMin, pbMax, mvMin, mvMax, priceMin, priceMax;

            if (!string.IsNullOrEmpty(condition.pe) && condition.pe != "-1")
            {
                peMin = condition.pe.Split('-')[0];
                peMax = condition.pe.Split('-')[1];
                conditonSql += string.Format(" and a.`pe` between '{0}' and '{1}'", peMin, peMax);
            }
            if (!string.IsNullOrEmpty(condition.pb) && condition.pb != "-1")
            {
                pbMin = condition.pb.Split('-')[0];
                pbMax = condition.pb.Split('-')[1];
                conditonSql += string.Format(" and a.`pb` between '{0}' and '{1}'", pbMin, pbMax);
            }
            if (!string.IsNullOrEmpty(condition.mv) && condition.mv != "-1")
            {
                mvMin = condition.mv.Split('-')[0];
                mvMax = condition.mv.Split('-')[1];
                conditonSql += string.Format(" and a.`mv` between '{0}' and '{1}'", mvMin, mvMax);
            }
            if (!string.IsNullOrEmpty(condition.price) && condition.price != "-1")
            {
                priceMin = condition.price.Split('-')[0];
                priceMax = condition.price.Split('-')[1];
                conditonSql += string.Format(" and a.`price` between '{0}' and '{1}'", priceMin, priceMax);
            }


            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<StockQueryCount>(string.Format(sql, conditonSql)).ToList();
            }

        }
        public void AddStockToCategory(string stockCode, string categoryCode)
        {
            using (var entity = new StockManDBEntities())
            {
                if (entity.stock_category_map.Any(p => p.stock_code == stockCode && p.cate_code == categoryCode))
                    return;

                string sql = @"insert into stock_category_map(stock_code,cate_code,stock_name)values(@p0,@p1,@p2)";

                var stock = entity.stock.Find(stockCode);
                if (stock != null)
                    entity.Database.SqlQuery<PriceInfo>(sql, stockCode, categoryCode, stock.name).ToList();
            }
        }


        public DataTable GetStockGroup(string[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new DataTable();

            string sql = @"SELECT a.stock_code ,b.code as cate_code,b.name as cate_name FROM stockmandb.stock_category_map  a
                        inner join stockmandb.stockcategory b on a.cate_code=b.code
                        inner join stockmandb.stock_category_group c on b.group_code=c.code
                        where a.stock_code in ({0}) and  c.code='tencent'";

            string idsStr = string.Empty;
            foreach (string id in ids)
            {
                if (idsStr.Length == 0)
                    idsStr = "'" + id + "'";
                else
                    idsStr += ",'" + id + "'";
            }

            sql = string.Format(sql, idsStr);
            var dic = new Dictionary<string, Dictionary<string, string>>();
            using (var entity = new StockManDBEntities())
            {
                DataTable dt = entity.ExecuteTable(sql);

                return dt;
            }
        }

        public string GetCodeRandom()
        {
            using (var entity = new StockManDBEntities())
            {
                //DateTime start = new DateTime(DateTime.Now.Year - 4, 1, 1);
                //DateTime end = new DateTime(DateTime.Now.Year, 1, 1);
                var codes = entity.stock_category_map
                    .Where(p => p.cate_code == "1399300")
                    .Select(p => p.stock_code).ToArray();

                var r = new Random();
                var index = r.Next(0, codes.Length - 1);
                return codes[index];
            }
        }

    }

}
