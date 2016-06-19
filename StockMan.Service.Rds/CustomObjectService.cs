using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.MySqlAccess;
using StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using System.Data;
using StockMan.EntityModel.dto;

namespace StockMan.Service.Rds
{
    public class CustomObjectService : ServiceBase<customobject>, ICustomObjectService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>

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
            
                  FROM Data_Object_{0}_Latest where object_code='{1}'", type, code);

            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<PriceInfo>(sql).ToList();
            }
        }

        public void UpdateObjectInfo(IList<ObjectInfo> spiList)
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
                foreach (ObjectInfo p in spiList)
                {
                    //SELECT `customobject`.`code`,
                    //    `customobject`.`name`,
                    //    `customobject`.`state`,
                    //    `customobject`.`date`,1
                    //    `customobject`.`open`,1
                    //    `customobject`.`low`,1
                    //    `customobject`.`price`,1
                    //    `customobject`.`updown`,1
                    //    `customobject`.`yestclose`,1
                    //    `customobject`.`volume`,1
                    //    `customobject`.`turnover`,1
                    //    `customobject`.`high`,1
                    //    `customobject`.`percent`1
                    //FROM `stockmandb`.`customobject`;
                    sql += string.Format(@"UPDATE `customobject` SET                      
                            `price` = '{0}',
                            `yestclose` ='{1}',
                            `volume` = '{2}',
                            `turnover` = '{3}',
                            `high` = '{4}',
                            `updown` = '{5}',
                            `low` ='{6}',        
                            `open` ='{7}',                 
                            `percent` = '{8}',
                            `date`='{9}'
                            WHERE code = '{10}';", p.price, p.yestclose, p.volume, p.turnover,
                                                 p.high, p.updown, p.low, p.open,  p.percent,p.date, p.object_code);

                }
                entity.Database.ExecuteSqlCommand(sql);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IList<customobject> GetObjectList(string category_code)
        {

            string sql = @"SELECT  code
                  ,name
                  ,state
                  ,date
                  ,open
                  ,low
                  ,price
                  ,updown
                  ,yestclose
                  ,volume
                  ,turnover
                  ,high
                  ,percent
              FROM customobject a inner join CustomObject_Category_Map b on a.code=b.object_code
              where b.cate_code=@p0";

            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<customobject>(sql, category_code).ToList();
            }

        }

        public IList<MyCycleObject> GetMyObject(string userId)
        {
            //三类，大盘，行业，经济数据
//            string sql = @"SELECT  code
//                  ,name
//                  ,state
//                  ,date
//                  ,open
//                  ,low
//                  ,price
//                  ,updown
//                  ,yestclose
//                  ,volume
//                  ,turnover
//                  ,high
//                  ,percent
//              FROM customobject a inner join Object_User_Map b on a.code=b.object_code
//              where b.user_id='0'";
            string sql = @"select a.code,a.name,a.price,a.yestclose,b.object_type as 'type'  from customobject a
                        inner join object_user_map b on a.code=b.object_code and b.object_type='3' and b.user_id=@p0
                        union
                        select a.code,a.name,a.price,a.yestclose,b.object_type as 'type'  from stockcategory a
                        inner join object_user_map b on a.code=b.object_code and b.object_type='2' and b.user_id=@p0
                        union 
                        select a.code,a.name,a.value as 'price',a.last_value as 'yestclose',b.object_type as 'type' from related_object_define a 
                        inner join object_user_map b on a.code =b.object_code and b.object_type='4' and b.user_id=@p0";
            using (var entity = new StockManDBEntities())
            {
                return entity.Database.SqlQuery<MyCycleObject>(sql, userId).ToList();
            }
        }


        public void AddMyObject(List<object_user_map> cate_user)
        {
            if (cate_user == null
              || cate_user.Count() == 0
              || cate_user.Count(p => string.IsNullOrEmpty(p.object_code) || string.IsNullOrEmpty(p.user_id)) > 0
              )
                throw new Exception("参数不合规");

            string user_id = cate_user[0].user_id;
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var oldList = entity.object_user_map
                    .Where(p => p.user_id == user_id)
                    .ToList();
                //新增
                var addList = cate_user.Except(oldList, new Object_User_Comparer());
                foreach (var item in addList)
                {
                    entity.object_user_map.Add(item);
                }

                //删除
                var delList = oldList.Except(cate_user, new Object_User_Comparer());

                foreach (var del in delList)
                {
                    entity.object_user_map.Remove(del);
                }


                entity.SaveChanges();
            }
        }


        public IList<customobject> GetDataByCode(string codes)
        {
            string[] idArray = codes.Split(',');
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
                string sql = string.Format(@" SELECT `code`,
                                                    `name`,
                                                    `state`,
                                                    `date`,
                                                    `open`,
                                                    `low`,
                                                    `price`,
                                                    `updown`,
                                                    `yestclose`,
                                                    `volume`,
                                                    `turnover`,
                                                    `high`,
                                                    `percent`
                                             FROM `customobject` WHERE code in ({0});", idsStr);

                return entity.Database.SqlQuery<customobject>(sql).ToList();

            }
        }
    }
    public class Object_User_Comparer : IEqualityComparer<object_user_map>
    {

        public bool Equals(object_user_map x, object_user_map y)
        {
            if (x.object_code == y.object_code && x.object_type == y.object_type)
                return true;
            return false;
        }

        public int GetHashCode(object_user_map obj)
        {
            return obj.object_code.GetHashCode();
        }
    }
}
