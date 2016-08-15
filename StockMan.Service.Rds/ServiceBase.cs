using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using StockMan.Service.Interface;
using System.Configuration;
using StockMan.MySqlAccess;
using StockMan.EntityModel;
using System.Data.Entity;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Data;
namespace StockMan.Service.Rds
{
    public class ServiceBase<T> : IEntityService<T>
        where T : EntityBase
    {

        public ServiceBase()
        {

        }
        public virtual T Add(T data)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                entity.Set<T>().Add(data);
                try
                {
                    entity.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    throw ex;
                }
                return data;
            }
        }

        public virtual void Update(T data)
        {
            using (StockManDBEntities db = new StockManDBEntities())
            {
                db.Entry<T>(data).State = EntityState.Modified;
                db.SaveChanges();
            }

        }

        public virtual void Delete(T data)
        {
            using (StockManDBEntities db = new StockManDBEntities())
            {
                Type type = data.GetType();
                PropertyInfo[] members = type.GetProperties();
                List<string> ids = new List<string>();
                foreach (PropertyInfo m in members)
                {
                    var temp = m.GetCustomAttribute<KeyAttribute>();
                    if (temp != null)
                    {
                        object obj = m.GetValue(data);
                        ids.Add(obj.ToString());
                        continue;
                    }
                }
                T entity = db.Set<T>().Find(ids.ToArray());
                if (entity != default(T))
                {
                    db.Set<T>().Remove(entity);
                    db.SaveChanges();
                }
            }
        }

        public virtual T Find(string id)
        {
            using (StockManDBEntities db = new StockManDBEntities())
            {
                T entity = db.Set<T>().Find(id);
                return entity;
            }
        }

        public void Dispose()
        {
            //if (Context != null)
            //    Context.Dispose();
            //if (Client != null)
            //    Client.Dispose();
        }

        public virtual IList<T> FindAll()
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.Set<T>().ToList();
            }
        }


        public virtual void AddRange(IList<T> datas)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                foreach (var data in datas)
                {
                    entity.Set<T>().Add(data);
                }

                try
                {
                    entity.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    throw ex;
                }
            }
        }

        public virtual void DeleteRange(IList<T> datas)
        {
            using (StockManDBEntities db = new StockManDBEntities())
            {
                IList<T> list = new List<T>();
                foreach (T data in datas)
                {
                    Type type = data.GetType();
                    PropertyInfo[] members = type.GetProperties();
                    List<string> ids = new List<string>();
                    foreach (PropertyInfo m in members)
                    {
                        var temp = m.GetCustomAttribute<KeyAttribute>();
                        if (temp != null)
                        {
                            object obj = m.GetValue(data);
                            ids.Add(obj.ToString());
                            continue;
                        }
                    }
                    T entity = db.Set<T>().Find(ids.ToArray());
                    list.Add(entity);
                }

                if (list.Count > 0)
                {
                    foreach (var l in list)
                    {
                        db.Set<T>().Remove(l);
                    }

                    db.SaveChanges();
                }
            }
        }

        public void AddPriceByDay<TM>(IList<PriceInfo> priceList, bool check = true) where TM : ObjectDataBase, new()
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string tableName = typeof(TM).Name;
                //IList<TM> list = new List<TM>();
                foreach (PriceInfo p in priceList)
                {
                    if (!p.open.HasValue || p.open.Value <= 0)
                        continue;

                    string code = p.code + "_" + p.date.ToString("yyyyMMdd");
                    if (check && entity.Set<TM>().Count(s => s.code == code) > 0)
                    {
                        entity.Entry(new TM()
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
                            volume = p.volume,
                            turnover = p.turnover
                        }).State = EntityState.Modified;
                    }
                    else
                    {
                        entity.Set<TM>().Add(new TM()
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
                            volume = p.volume,
                            turnover = p.turnover
                        });

                    }

                }

                //foreach (var l in list)
                //{
                //    entity.Set<TM>().Add(l);
                //}

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

        public void AddPriceByWeek<TM>(IList<PriceInfo> price, bool check = true) where TM : ObjectDataBase, new()
        {
            using (var entity = new StockManDBEntities())
            {

                string tableName = typeof(TM).Name;

                //IList<TM> list = new List<TM>();

                DateTime endDate = DateTime.Now.AddDays(DayOfWeek.Friday - DateTime.Now.DayOfWeek + 1);

                DateTime startDate = endDate.AddDays(-5);

                //DateTime lastStartDate = startDate.AddDays(-7);
                //DateTime lastEndDate = startDate;
                foreach (PriceInfo p in price)
                {
                    if (!p.open.HasValue || p.open.Value <= 0)
                        continue;
                    string startCode = p.code + "_" + startDate.ToString("yyyyMMdd");
                    string endCode = p.code + "_" + endDate.ToString("yyyyMMdd");
                    string code = p.code + "_" + p.date.ToString("yyyyMMdd");

                    //string lastStartCode = p.code + "_" + lastStartDate.ToString("yyyyMMdd");
                    //string lastEndCode = p.code + "_" + lastEndDate.ToString("yyyyMMdd");
                    //判断是否有记录，有的话，不插入

                    if (check)
                    {
                        if (entity.Set<TM>().Count(s => s.code == code) > 0)
                        {
                            continue;
                        }

                        //从日线里寻找开盘价，收盘价，最高价，最低价
                        decimal? open, close, high, low, updown, volume, turnover, percent = 0, yestclose;
                        string statisSql = string.Format("select code,date,open,price,yestclose,high,low,percent,updown,volume,turnover from {0} a where a.code between @p0 and @p1", tableName.Replace("week", "day"));

                        var racentPrice = entity.Database.SqlQuery<PriceInfo>(statisSql, startCode, endCode).OrderBy(pi => pi.date).ToList();
                        if (racentPrice.Count == 0)
                            continue;
                        if (racentPrice.Count > 1)
                        {
                            //var lastWeekPrice = entity.Database.SqlQuery<PriceInfo>(statisSql, lastStartCode, lastEndCode).OrderBy(pi => pi.date).ToList();
                            //decimal? yestclose = 0;
                            //if (lastWeekPrice.Count > 0)
                            //    yestclose = lastWeekPrice[lastWeekPrice.Count - 1].price;

                            yestclose = racentPrice.First().yestclose;
                            open = racentPrice.First().open;
                            close = racentPrice.Last().price;
                            high = racentPrice.Max(pp => pp.high);
                            low = racentPrice.Max(pp => pp.low);
                            updown = close - yestclose;
                            volume = racentPrice.Sum(pp => pp.volume);
                            turnover = racentPrice.Sum(pp => pp.turnover);
                            if (yestclose != 0)
                            {
                                percent = (close - yestclose) / yestclose;
                            }
                            else
                            {
                                percent = 0;
                            }

                        }
                        else
                        {
                            open = racentPrice[0].open;
                            close = racentPrice[0].price;
                            high = racentPrice[0].high;
                            low = racentPrice[0].low;
                            updown = racentPrice[0].updown;
                            volume = racentPrice[0].volume;
                            turnover = racentPrice[0].turnover;
                            percent = racentPrice[0].percent;
                            yestclose = racentPrice[0].yestclose;
                        }
                        //判断是否当前周，如果是当前周，删除数据，添加新的数据
                        var countRow = entity.Database.SqlQuery<int>(
                            string.Format("select count(0) from {0} a where a.code between @p0 and @p1", tableName)
                            , startCode,
                            endCode);

                        if (countRow.First() > 0)
                        {
                            entity.Database.ExecuteSqlCommand(string.Format("delete from {0} where code between @p0 and @p1", tableName), startCode, endCode);
                        }

                        entity.Set<TM>().Add(new TM()
                        {
                            code = code,
                            date = p.date,// new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(p.date.Substring(6))),
                            price = close,
                            yestclose = yestclose,
                            high = high,
                            low = low,
                            open = open,
                            percent = percent,
                            object_code = p.code,
                            updown = updown,
                            volume = volume,
                            turnover = turnover
                        });
                    }
                    else
                    {

                        entity.Set<TM>().Add(new TM()
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
                            turnover = p.turnover,
                            volume = p.volume
                        });
                    }



                }
                //foreach (var l in list)
                //{
                //    entity.Set<TM>().Add(l);
                //}

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

        public void AddPriceByMonth<TM>(IList<PriceInfo> price, bool check = true) where TM : ObjectDataBase, new()
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string tableName = typeof(TM).Name;

                DateTime nextMonthData = DateTime.Now.AddMonths(1);
                DateTime endDate = new DateTime(nextMonthData.Year, nextMonthData.Month, 1);
                DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                //IList<TM> list = new List<TM>();
                foreach (PriceInfo p in price)
                {
                    if (!p.open.HasValue || p.open.Value <= 0)
                        continue;
                    string startCode = p.code + "_" + startDate.ToString("yyyyMMdd");
                    string endCode = p.code + "_" + endDate.ToString("yyyyMMdd");
                    string code = p.code + "_" + p.date.ToString("yyyyMMdd");
                    //判断是否有记录，有的话，不插入
                    if (check)
                    {

                        if (entity.Set<TM>().Count(s => s.code == code) > 0)
                        {
                            continue;
                        }
                        //从日线里寻找开盘价，收盘价，最高价，最低价
                        decimal? open, close, high, low, updown, volume, turnover, percent = 0, yestclose;
                        string statisSql = string.Format("select code,date,open,price,yestclose,high,low,percent,updown,volume,turnover from {0} a where a.code between @p0 and @p1", tableName.Replace("month", "week"));

                        var racentPrice = entity.Database.SqlQuery<PriceInfo>(statisSql, startCode, endCode).OrderBy(pi => pi.date).ToList();
                        if (racentPrice.Count == 0)
                            continue;
                        if (racentPrice.Count > 1)
                        {
                            yestclose = racentPrice.First().yestclose;
                            open = racentPrice.First().open;
                            close = racentPrice.Last().price;
                            high = racentPrice.Max(pp => pp.high);
                            low = racentPrice.Max(pp => pp.low);
                            updown = close - open;
                            volume = racentPrice.Sum(pp => pp.volume);
                            turnover = racentPrice.Sum(pp => pp.turnover);
                            if (yestclose != 0)
                            {
                                percent = (close - yestclose) / yestclose;
                            }
                            else
                            {
                                percent = 0;
                            }
                        }
                        else
                        {
                            open = racentPrice[0].open;
                            close = racentPrice[0].price;
                            high = racentPrice[0].high;
                            low = racentPrice[0].low;
                            updown = racentPrice[0].updown;
                            volume = racentPrice[0].volume;
                            turnover = racentPrice[0].turnover;
                            percent = racentPrice[0].percent;
                            yestclose = racentPrice[0].yestclose;
                        }



                        var countRow = entity.Database.SqlQuery<int>(string.Format("select count(0) from {0} a where a.code between @p0 and @p1", tableName), startCode, endCode);

                        if (countRow.First() > 0)
                        {
                            entity.Database.ExecuteSqlCommand(string.Format("delete from {0} where code between @p0 and @p1", tableName), startCode, endCode);

                        }

                        entity.Set<TM>().Add(new TM()
                        {
                            code = code,
                            date = p.date,// new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(p.date.Substring(6))),
                            price = close,
                            yestclose = yestclose,
                            high = high,
                            low = low,
                            open = open,
                            percent = percent,
                            object_code = p.code,
                            updown = updown,
                            volume = volume,
                            turnover = turnover
                        });
                    }
                    else
                    {
                        entity.Set<TM>().Add(new TM()
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
                            volume = p.volume,
                            turnover = p.turnover
                        });
                    }

                }
                //foreach (var l in list)
                //{
                //    entity.Set<TM>().Add(l);
                //}

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

        /// <summary>
        /// 获取某周期的数据
        /// </summary>
        /// <typeparam name="TM"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetData<TM>(string code) where TM : ObjectDataBase, new()
        {
            //e[0],//日期
            //e[1],//开盘 
            //e[2],//收盘
            //e[3],//最高
            //e[4],//最低   
            //e[5],//成交量
            //e[6],//涨跌额
            //e[7],//涨跌幅

            string tableName = typeof(TM).Name;
            string sql = string.Format(@"SELECT 
                      date
                      ,open
                      ,price
                      ,high
                      ,low
                      ,volume
                      ,updown 
                      ,percent                  
                      ,yestclose                      
                      ,turnover
                  FROM {0} where object_code='{1}' order by date", tableName, code);

            using (var entity = new StockManDBEntities())
            {
                entity.Database.Connection.Open();
                using (entity.Database.Connection)
                {
                    System.Data.IDbCommand commond = entity.Database.Connection.CreateCommand();
                    commond.CommandText = sql;
                    IDataReader reader = commond.ExecuteReader();

                    string dataStr = "";
                    while (reader.Read())
                    {
                        string fstr = "\"" + DateTime.Parse(reader["date"] + "").ToString("yyyyMMdd") + "\"";
                        for (var i = 1; i < reader.FieldCount; i++)
                        {
                            fstr += "," + reader[i];
                        }
                        fstr = "[" + fstr + "]";

                        if (dataStr.Length == 0)
                            dataStr = fstr;
                        else
                            dataStr += "," + fstr;
                    }
                    dataStr = "[" + dataStr + "]";
                    entity.Database.Connection.Close();
                    return dataStr;
                }
            }
        }

        public string GetData<TM>(string code, DateTime start, DateTime end) where TM : ObjectDataBase, new()
        {
            //e[0],//日期
            //e[1],//开盘 
            //e[2],//收盘
            //e[3],//最高
            //e[4],//最低   
            //e[5],//成交量
            //e[6],//涨跌额
            //e[7],//涨跌幅

            string tableName = typeof(TM).Name;
            string sql = string.Format(@"SELECT 
                      date
                      ,open
                      ,price
                      ,high
                      ,low
                      ,volume
                      ,updown 
                      ,percent                  
                      ,yestclose                      
                      ,turnover
                  FROM {0} where object_code='{1}' and date between '{2}' and '{3}' order by date", tableName, code, start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));

            using (var entity = new StockManDBEntities())
            {
                entity.Database.Connection.Open();
                using (entity.Database.Connection)
                {
                    System.Data.IDbCommand commond = entity.Database.Connection.CreateCommand();
                    commond.CommandText = sql;
                    IDataReader reader = commond.ExecuteReader();

                    string dataStr = "";
                    while (reader.Read())
                    {
                        string fstr = "\"" + DateTime.Parse(reader["date"] + "").ToString("yyyyMMdd") + "\"";
                        for (var i = 1; i < reader.FieldCount; i++)
                        {
                            fstr += "," + reader[i];
                        }
                        fstr = "[" + fstr + "]";

                        if (dataStr.Length == 0)
                            dataStr = fstr;
                        else
                            dataStr += "," + fstr;
                    }
                    dataStr = "[" + dataStr + "]";
                    entity.Database.Connection.Close();
                    return dataStr;
                }
            }
        }

        public PriceInfo GetCurrentData(ObjectType objType, TechCycle cycle, string code)
        {


            string tableName = string.Format("data_{0}_day_latest", objType.ToString());
            switch (cycle)
            {
                case TechCycle.day:
                    if (objType == ObjectType.Stock)
                        tableName = "stock";
                    else if (objType == ObjectType.Object)
                        tableName = "customobject";
                    else if (objType == ObjectType.Category)
                        tableName = "stockcategory";
                    string statisSql = string.Format("select code,open,price,yestclose,high,low,percent,updown,volume,turnover from {0} a where a.code = @p0", tableName);

                    using (var entity = new StockManDBEntities())
                    {
                        var racentPrice = entity.Database.SqlQuery<PriceInfo>(statisSql, code);

                        //return getDataByRange(code, tableName, endDate, startDate);
                        return racentPrice.FirstOrDefault();
                    }
                //break;
                case TechCycle.week:
                    DateTime endDate = DateTime.Now.AddDays(DayOfWeek.Sunday - DateTime.Now.DayOfWeek);
                    DateTime startDate = endDate.AddDays(-7);
                    return getDataByRange(code, tableName, endDate, startDate);

                //break;
                case TechCycle.month:

                    DateTime nextMonthData = DateTime.Now.AddMonths(1);
                    DateTime endDate0 = new DateTime(nextMonthData.Year, nextMonthData.Month, 1);
                    DateTime startDate0 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    return getDataByRange(code, tableName, endDate0, startDate0);
                    //break;
            }


            return null;
        }

        public string GetHistoryData(ObjectType objType, TechCycle cycle, string code)
        {
            //e[0],//日期
            //e[1],//开盘 
            //e[2],//收盘
            //e[3],//最高
            //e[4],//最低   
            //e[5],//成交量
            //e[6],//涨跌额
            //e[7],//涨跌幅
            using (var entity = new StockManDBEntities())
            {
                string tableName = string.Format("data_{0}_{1}_latest", objType.ToString(), cycle);
                string where = string.Empty;
                switch (cycle)
                {
                    case TechCycle.day:
                        //where = string.Format("and date < (select max(date) from {0} where object_code='{1}')", tableName, code);
                        break;
                    case TechCycle.week:
                        DateTime endDate = DateTime.Now.AddDays(DayOfWeek.Sunday - DateTime.Now.DayOfWeek);
                        DateTime startDate = endDate.AddDays(-7);
                        //return getDataByRange(code, tableName, endDate, startDate);
                        where = string.Format(" and date < '{0}'", startDate.ToString("yyyy-MM-dd"));
                        break;
                    case TechCycle.month:

                        DateTime startDate0 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);

                        where = string.Format(" and date <= '{0}'", startDate0.ToString("yyyy-MM-dd"));
                        break;
                }
                string sql = string.Format(@"SELECT * FROM ( SELECT  
                      date
                      ,open
                      ,price
                      ,high
                      ,low
                      ,volume
                      ,updown 
                      ,percent                  
                      ,yestclose                      
                      ,turnover
                  FROM {0} where object_code='{1}' {2} order by date desc limit 150) a order by a.date ASC", tableName, code, where);


                entity.Database.Connection.Open();
                using (entity.Database.Connection)
                {
                    System.Data.IDbCommand commond = entity.Database.Connection.CreateCommand();
                    commond.CommandText = sql;
                    IDataReader reader = commond.ExecuteReader();

                    string dataStr = "";
                    while (reader.Read())
                    {
                        string fstr = string.Format("\"{0}\"", DateTime.Parse(reader["date"] + "").ToString("yyyyMMdd"));
                        for (var i = 1; i < reader.FieldCount; i++)
                        {
                            if (string.IsNullOrEmpty(reader[i] + ""))
                            {
                                fstr += "," + "0";
                            }
                            else
                            {
                                fstr += "," + reader[i];
                            }

                        }
                        fstr = "[" + fstr + "]";

                        if (dataStr.Length == 0)
                            dataStr = fstr;
                        else
                            dataStr += "," + fstr;
                    }
                    dataStr = "[" + dataStr + "]";
                    entity.Database.Connection.Close();
                    return dataStr;
                }
            }
        }

        public DateTime GetLatestDate(ObjectType objType, string code)
        {
            using (var entity = new StockManDBEntities())
            {
                string tableName = string.Format("data_{0}_{1}_latest", objType.ToString(), TechCycle.day);
                string where = string.Empty;

                string sql = string.Format(@"SELECT date FROM {0} where object_code='{1}' {2} order by date desc limit 1", tableName, code, where);

                entity.Database.Connection.Open();
                using (entity.Database.Connection)
                {
                    System.Data.IDbCommand commond = entity.Database.Connection.CreateCommand();
                    commond.CommandText = sql;
                    object val = commond.ExecuteScalar();

                    entity.Database.Connection.Close();

                    if (string.IsNullOrEmpty(val + ""))
                        return DateTime.Now;
                    return DateTime.Parse(val + "");
                }
            }
        }
        protected PriceInfo getDataByRange(string code, string tableName, DateTime endDate, DateTime startDate)
        {
            using (var entity = new StockManDBEntities())
            {
                string startCode = code + "_" + startDate.ToString("yyyyMMdd");
                string endCode = code + "_" + endDate.ToString("yyyyMMdd");
                //从日线里寻找开盘价，收盘价，最高价，最低价
                decimal? open, close, high, low, updown, volume, turnover, percent = 0, yestclose;
                string statisSql = string.Format("select code,date,open,price,yestclose,high,low,percent,updown,volume,turnover from {0} a where a.code between @p0 and @p1", tableName);

                var racentPrice = entity.Database.SqlQuery<PriceInfo>(statisSql, startCode, endCode).OrderBy(pi => pi.date).ToList();
                if (racentPrice.Count == 0)
                    return null;
                if (racentPrice.Count > 1)
                {
                    open = racentPrice.First().open;
                    close = racentPrice.Last().price;
                    high = racentPrice.Max(pp => pp.high);
                    low = racentPrice.Max(pp => pp.low);
                    updown = close - open;
                    volume = racentPrice.Sum(pp => pp.volume);
                    turnover = racentPrice.Sum(pp => pp.turnover);
                    if (open != 0)
                    {
                        percent = (close - open) / open;
                    }
                    yestclose = racentPrice.First().yestclose;
                }
                else
                {
                    open = racentPrice[0].open;
                    close = racentPrice[0].price;
                    high = racentPrice[0].high;
                    low = racentPrice[0].low;
                    updown = racentPrice[0].updown;
                    volume = racentPrice[0].volume;
                    turnover = racentPrice[0].turnover;
                    percent = racentPrice[0].percent;
                    yestclose = racentPrice[0].yestclose;
                }

                return new PriceInfo
                {
                    open = open,
                    price = close,
                    high = high,
                    low = low,
                    updown = updown,
                    volume = volume,
                    turnover = turnover,
                    percent = percent,
                    yestclose = yestclose
                };
            }
        }


    }


    public class PriceDataServiceBase<T> : ServiceBase<T>
        where T : EntityBase
    {

    }
}
