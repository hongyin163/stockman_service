using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMan.Message.Task.Interface;
using StockMan.Service.Rds;
using StockMan.Service.Interface.Rds;
using StockMan.EntityModel;
using Newtonsoft.Json;
using StockMan.MySqlAccess;

namespace StockMan.Message.TaskInstance
{
    public class RecoStockForUserTask : Message.Task.ITask
    {
        private const string CODE = "T0006";
        IUserService userService = new UserService();
        public void Excute(string message)
        {
            var msg = JsonConvert.DeserializeObject<UserInfo>(message);
            var u = userService.Find(msg.id);

            this.Log().Info(string.Format("计算用户：{0}", u.id));

            //获取推荐的个股
            this.Log().Info("推荐个股");
            var stockList = GetRecomentStock(u);
            //生成推荐的消息
            string recoReuslt = string.Empty;
            //if (stockList.Count > 0)
            //{
            //    recoReuslt = GetRecomendStockMsg("综合推荐", stockList, "");
            //}
            //获取推荐的月线金叉股票
            var dayCross = GetRecomentCrossStock(u, "day");
            //if (dayCross.Count > 0)
            //{
            //    recoReuslt += GetRecomendStockMsg("日线金叉", dayCross, "day");
            //}
            var weekCross = GetRecomentCrossStock(u, "week");
            //if (weekCross.Count > 0)
            //{
            //    recoReuslt += GetRecomendStockMsg("周线金叉", weekCross, "week");
            //}
            var monthCross = GetRecomentCrossStock(u, "month");
            //if (monthCross.Count > 0)
            //{
            //    recoReuslt += GetRecomendStockMsg("月线金叉", monthCross, "month");
            //}

            //获取推荐的行业
            var cateList = GetRecomentCategory();
            //if (cateList.Count > 0)
            //{
            //    recoReuslt += GetRecomendCateMsg(cateList);
            //}

            //this.Log().Info(recoReuslt);
            //生成消息Json数据
            //string recoHit = "";// GetRecoHit(stockList);

            //生成自选股状态消息
            //string stateReuslt = GetMyStockState(u);
            //this.Log().Info(stateReuslt);

            //type=0文本消息，1,个股推荐消息
            //if ((recoReuslt + stateReuslt).Length > 0)
            //{
            //    messageList.Add(new data.user_message
            //    {
            //        code = u.id + "_" + DateTime.Now.ToString("yyyyMMdd"),
            //        content = recoReuslt + stateReuslt,
            //        createtime = DateTime.Now,
            //        hint = recoHit,
            //        state = 0,
            //        title = "",
            //        user_id = u.id,
            //        type = 1,
            //        notice_state = 0
            //    });
            //}
        }

        public string GetCode()
        {
            return CODE;
        }

        public void Send(IMessageSender sender)
        {
            int pageSize = 10, pageIndex = 0;
            IList<users> userList = userService.GetUserWithDataList(pageSize, pageIndex);
            while (userList.Count() > 0) 
            {
                foreach (var user in userList)
                {
                    UserInfo info = new UserInfo
                    {
                        id = user.id
                    };
                    sender.Send(JsonConvert.SerializeObject(info));
                }

                userList = userService.GetUserWithDataList(pageSize,++pageIndex);
            }
        }


        private List<reco_user_stock> GetRecomentStock(users u)
        {
            var stockList = new List<reco_user_stock>();
            var myCateList = u.object_user_map.Where(p => p.object_type == "2").ToList();
            if (myCateList.Count == 0)
            {
                myCateList.AddRange(new object_user_map[]{
                    new object_user_map
                    {
                        user_id = u.id,
                        object_code = "0000001",
                        object_type = "3"
                    }, new object_user_map
                    {
                        user_id = u.id,
                        object_code = "1399001",
                        object_type = "3"
                    }, new object_user_map
                    {
                        user_id = u.id,
                        object_code = "1399006",
                        object_type = "3"
                    }
                });
            }

            using (StockManDBEntities entity = new StockManDBEntities())
            {
                foreach (var cate in myCateList)
                {
                    //foreach (var index in u.index_user_map)
                    //{

                    var tmpCode = string.Format("{0}_{1}_", u.id, cate.object_code);
                    string sql = string.Format(@"SELECT CONCAT('{0}',`object_code`) as `code`,'{1}' as user_id,`cate_code`,''as `index_code`,`object_code`,'0' as `day`,'0' as `week`,'0' as `month`,'0' as `last_day`,'0' as `last_week`,'0' as `last_month`
                            ,`cate_name`,'' as `index_name`,`object_name`,`pe`,`pb`,`mv`,`fv`,`price`,`yestclose` 
                            FROM reco_stock_category_rank a
                            where a.cate_code=@p0 order by a.rank desc limit 1", tmpCode, u.id);

                    var stocksTemp = entity.Database.SqlQuery<reco_user_stock>(sql, cate.object_code).ToList();
                    stockList.AddRange(stocksTemp);

                    //}
                }
                //存储推荐的股票
                entity.Database.ExecuteSqlCommand("truncate table reco_user_stock");
                //存储推荐的股票
                var log = log4net.LogManager.GetLogger("stock");
                foreach (var stock in stockList)
                {
                    log.Info(stock.code);
                    if (!entity.reco_user_stock.Any(p => p.code == stock.code))
                        entity.reco_user_stock.Add(stock);
                }
                entity.SaveChanges();
            }
            return stockList;
        }


        private List<reco_user_stock> GetRecomentCrossStock(users u, string cycle)
        {
            var stockList = new List<reco_user_stock>();
            var myCateList = u.object_user_map.Where(p => p.object_type == "2").ToList();
            if (myCateList.Count == 0)
            {
                myCateList.AddRange(new object_user_map[]{
                    new object_user_map
                    {
                        user_id = u.id,
                        object_code = "0000001",
                        object_type = "3"
                    }, new object_user_map
                    {
                        user_id = u.id,
                        object_code = "1399001",
                        object_type = "3"
                    }, new object_user_map
                    {
                        user_id = u.id,
                        object_code = "1399006",
                        object_type = "3"
                    }
                });
            }
            IList<string> indexList = u.index_user_map.Select(p => p.index_code).ToList();
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                foreach (var cate in myCateList)
                {

                    //stockList = entity.reco_stock_category_tag
                    //     .Where(p => p.cate_code == cate.object_code && indexList.Contains(p.index_code) && p.cycle == cycle)
                    //     .Select(p => new reco_user_stock
                    //     {
                    //         cate_code = p.cate_code,
                    //         index_code = p.index_code,
                    //         object_code = p.object_code,
                    //         cate_name = p.cate_name,
                    //         index_name = p.index_name,
                    //         object_name = p.object_name,
                    //         code = p.cate_code + "_" + p.index_name + "_" + p.object_code + "_" + p.cycle,
                    //         day = p.day,
                    //         week = p.week,
                    //         month = p.month,
                    //         last_day = p.last_day,
                    //         last_week = p.last_week,
                    //         last_month = p.last_month,
                    //         price = p.price,
                    //         yestclose = p.yestclose,
                    //         pe = p.pe,
                    //         fv = p.fv,
                    //         mv = p.mv,
                    //         pb = p.pb,
                    //         user_id = u.id
                    //     }).ToList();

                    var query = from p in entity.reco_stock_category_tag
                                where p.cate_code == cate.object_code && indexList.Contains(p.index_code) && p.cycle == cycle
                                select p;
                    var list = query.ToList();
                    stockList = list.Select(p => new reco_user_stock
                    {
                        cate_code = p.cate_code,
                        index_code = p.index_code,
                        object_code = p.object_code,
                        cate_name = p.cate_name,
                        index_name = p.index_name,
                        object_name = p.object_name,
                        code = p.cate_code + "_" + p.index_name + "_" + p.object_code + "_" + p.cycle,
                        day = p.day,
                        week = p.week,
                        month = p.month,
                        last_day = p.last_day,
                        last_week = p.last_week,
                        last_month = p.last_month,
                        price = p.price,
                        yestclose = p.yestclose,
                        pe = p.pe,
                        fv = p.fv,
                        mv = p.mv,
                        pb = p.pb,
                        user_id = u.id
                    }).ToList();

                }

            }
            return stockList;
        }
        private List<stockcategory> GetRecomentCategory()
        {

            using (StockManDBEntities entity = new StockManDBEntities())
            {

                string sql = string.Format(@"SELECT  a.object_code as code  FROM stockmandb.reco_object_rank a
                                        inner join stockcategory b on a.object_code=b.code
                                         where a.category_code='2' order by rank desc limit 2;");

                var temp = entity.Database.SqlQuery<string>(sql);

                if (temp.Count() > 0)
                {
                    return entity.stockcategory.Where(p => temp.Contains(p.code)).ToList();
                }
                return new List<stockcategory>();
            }
        }


    }
    public class UserInfo
    {
        public string id { get; set; }
    }
}
