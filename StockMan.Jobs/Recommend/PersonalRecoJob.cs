using Quartz;
using StockMan.MySqlAccess;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data = StockMan.EntityModel;
using facade = StockMan.Facade.Models;
using StockMan.Common;
using StockMan.EntityModel.dto;
using Newtonsoft.Json;
using StockMan.EntityModel;
using System.Net.Mail;
using StockMan.Service.Common;
namespace StockMan.Jobs.Recommend
{
    public class PersonalRecoJob : IJob
    {
        //IStockCategoryService cateService = new StockCategoryService();
        //IStockService stockService = new StockService();
        //IIndexService indexService = new IndexService();
        IUserService userService = new UserService();
        public void Execute(IJobExecutionContext context)
        {
            //每个行业，每个技术，最多推荐5只股票。
            //IList<data.stockcategory> cateList = cateService.GetCategoryList("tencent");
            //IList<data.indexdefinition> indexList = indexService.FindAll().Where(p => p.state == 1).ToList();

            IList<data.users> userList = GetUserList();
            IList<data.user_message> messageList = new List<data.user_message>();
            foreach (var u in userList)
            {
                this.Log().Info(string.Format("计算用户：{0}", u.id));

                //获取推荐的个股
                this.Log().Info("推荐个股");
                var stockList = GetRecomentStock(u);
                //生成推荐的消息
                string recoReuslt = string.Empty;
                if (stockList.Count > 0)
                {
                    recoReuslt = GetRecomendStockMsg("综合推荐", stockList, "");
                }
                //获取推荐的月线金叉股票
                var dayCross = GetRecomentCrossStock(u, "day");
                if (dayCross.Count > 0)
                {
                    recoReuslt += GetRecomendStockMsg("日线金叉", dayCross, "day");
                }
                var weekCross = GetRecomentCrossStock(u, "week");
                if (weekCross.Count > 0)
                {
                    recoReuslt += GetRecomendStockMsg("周线金叉", weekCross, "week");
                }
                var monthCross = GetRecomentCrossStock(u, "month");
                if (monthCross.Count > 0)
                {
                    recoReuslt += GetRecomendStockMsg("月线金叉", monthCross, "month");
                }

                //获取推荐的行业
                var cateList = GetRecomentCategory();
                if (cateList.Count > 0)
                {
                    recoReuslt += GetRecomendCateMsg(cateList);
                }

                this.Log().Info(recoReuslt);
                //生成消息Json数据
                string recoHit = "";// GetRecoHit(stockList);

                //生成自选股状态消息
                string stateReuslt = GetMyStockState(u);
                this.Log().Info(stateReuslt);

                //type=0文本消息，1,个股推荐消息
                if ((recoReuslt + stateReuslt).Length > 0)
                {
                    messageList.Add(new data.user_message
                    {
                        code = u.id + "_" + DateTime.Now.ToString("yyyyMMdd"),
                        content = recoReuslt + stateReuslt,
                        createtime = DateTime.Now,
                        hint = recoHit,
                        state = 0,
                        title = "",
                        user_id = u.id,
                        type = 1,
                        notice_state = 0
                    });
                }

            }

            SaveUserMessage(messageList);

            SendStateNotification();

        }

        private void SendStateNotification()
        {
            //检查MACD趋势，如果趋势是下跌，提示大盘较弱
            //检查动能指标，如果出现红柱，红柱别昨天大，大盘渐强

            //看短线指标见底和见顶指标，kdj和rsi，在底部和顶部提示风险，如果大盘弱，提示轻仓操作，大盘强，提示增加仓位。
            string msg = "您的自选股状态和推荐已更新";
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                string sql = "select ga from tech_object_ga_day where f_code='0000001' order by date desc limit 5";
                IList<double> gas = entity.Database.SqlQuery<double>(sql).ToList();
                if (gas[0] > 0)
                {
                    if (gas[0] > gas[1])
                        msg = "中线较强";
                    else
                        msg = "中线渐弱";
                }
                else
                {
                    if (gas[0] > gas[1])
                        msg = "中线渐强";
                    else
                        msg = "中线较弱";
                }

                sql = "select j from tech_object_kdj_day where f_code='0000001' order by date desc limit 5";
                IList<double> kdjs = entity.Database.SqlQuery<double>(sql).ToList();
                sql = "select r1 from tech_object_rsi_day where f_code='0000001' order by date desc limit 5";
                IList<double> rsis = entity.Database.SqlQuery<double>(sql).ToList();

                if (kdjs[0] > 80 && rsis[0] > 80)
                {
                    msg += ",短线见顶";
                }
                else if (kdjs[0] < 20 && rsis[0] < 20)
                {
                    msg += ",短线见底";
                }
                else
                {
                    if (rsis[0] > rsis[1] && kdjs[0] > kdjs[1])
                    {
                        msg += ",短线渐强";
                    }
                    else if (rsis[0] < rsis[1] && kdjs[0] < kdjs[1])
                    {
                        msg += ",短线渐弱";
                    }
                }

                if ((msg.Contains("中线渐弱") || msg.Contains("中线较弱")) && msg.Contains("短线见顶"))
                {
                    msg += ",轻仓,注意风险。";
                }
                else if ((msg.Contains("中线渐弱") || msg.Contains("中线较弱")) && msg.Contains("短线见底"))
                {
                    msg += ",轻仓,谨慎抢反弹。";
                }
                else if ((msg.Contains("中线渐弱") || msg.Contains("中线较弱")) && msg.Contains("短线渐强"))
                {
                    msg += ",逐步减仓,降低成本。";
                }
                else if ((msg.Contains("中线渐弱") || msg.Contains("中线较弱")) && msg.Contains("短线渐弱"))
                {
                    msg += ",考虑空仓。";
                }
                else if ((msg.Contains("中线渐强") || msg.Contains("中线较强")) && msg.Contains("短线见底"))
                {
                    msg += ",注意回调机会。";
                }
                else if ((msg.Contains("中线渐强") || msg.Contains("中线较强")) && msg.Contains("短线渐弱"))
                {
                    msg += ",控制仓位，注意风险。";
                }
                else if ((msg.Contains("中线渐强") || msg.Contains("中线较强")) && msg.Contains("短线渐强"))
                {
                    msg += "乐观，逢高减持。";
                }
                else
                {
                    msg += ",注意风险！";
                }
            }
            try
            {
                PushHelper.Push("慢牛分析，仅供参考", msg);
            }
            catch (Exception ex)
            {
                this.Log().Info("推送消息异常：" + ex.Message);
            }
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

        private string GetMyStockState1(users u)
        {
            if (u.stock_user_map == null || u.stock_user_map.Count == 0)
                return "自选股买卖点:未上传自选股。";
            if (u.index_user_map == null || u.index_user_map.Count == 0)
                return "自选股买卖点:未上传关注的技术指标。";
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var indexList = entity.indexdefinition.ToList();
                string style = @"<style type='text/css'>
                            table.gridtable {
                                width:100%;
	                            font-family: verdana,arial,sans-serif;
	                            font-size:11px;
	                            color:#333333;
	                            border-width: 1px;
	                            border-color: #666666;
	                            border-collapse: collapse;
                                margin:3px 0px;
                            }
                            table.gridtable th{
	                            border-width: 1px;
	                            padding: 3px;
	                            border-style: solid;
	                            border-color: #666666;
	                            background-color: #dedede;
                            }
                            table.gridtable caption {	
                                border-top-width: 1px;   
                                border-left-width: 1px;  
                                border-right-width: 1px;  
                                border-bottom-width: 0px;                       
	                            padding: 3px;
	                            border-style: solid;
	                            border-color: #666666;
	                            background-color: #dedede;
                                font-weight:bold;
                            }
                            table.gridtable td {
	                            border-width: 1px;
	                            padding: 3px;
	                            border-style: solid;
	                            border-color: #666666;
                            }
                            .up{ color:#cc0000;}
                            .down{color:#006030;}
                            .changeup{color:#fff;background-color:#cc0000 !important;}
                            .changedown{color:#fff;background-color:#006030 !important;}
                            </style>";
                var table = "<table class='gridtable'><caption>自选股状态变化</caption>{0}</table>";

                StringBuilder trBuilder = new StringBuilder("<tr><th></th><th>指标</th><th>日</th><th>周</th><th>月</th></tr>");



                foreach (var map in u.stock_user_map)
                {

                    var stock = entity.stock.FirstOrDefault(p => p.code == map.stock_code);
                    if (stock == null)
                    {
                        this.Log().Info("系统不存在该股票:" + map.stock_code);
                        continue;
                    }
                    string th = string.Format("<tr><td rowspan='{0}'>{1}</td></tr>", u.index_user_map.Count() + 1, stock.name);
                    StringBuilder trstrBuild = new StringBuilder();
                    foreach (var tech in u.index_user_map)
                    {
                        trstrBuild.Append("<tr>");

                        string stateSql = @"select `code`,`category_code`,
                                                `object_code`,`index_code`,`day`,`week`,`month`,`date`,
                                                `last_day`,`last_week`,`last_month` from objectstate a where a.object_code=@p0 and a.index_code=@p1";
                        var objstate = entity.Database.SqlQuery<objectstate>(stateSql, map.stock_code, tech.index_code).FirstOrDefault();

                        trstrBuild.Append("<td >");
                        trstrBuild.Append(indexList.FirstOrDefault(p => p.code == tech.index_code).name);
                        trstrBuild.Append("</td>");
                        if (objstate != null)
                        {
                            if (objstate.day > objstate.last_day)
                                trstrBuild.Append("<td class='changeup'>转涨*</td>");
                            else if (objstate.day < objstate.last_day)
                                trstrBuild.Append("<td class='changedown'>转跌</td>");
                            else if (objstate.day > 0)
                                trstrBuild.Append("<td ><span class='up'>上涨</span> 不变</td>");
                            else if (objstate.day < 0)
                                trstrBuild.Append("<td ><span class='down'>下跌</span> 不变</td>");
                            else
                                trstrBuild.Append("<td ></td>");

                            if (objstate.week > objstate.last_week)
                                trstrBuild.Append("<td class='changeup'>转涨*</td>");
                            else if (objstate.week < objstate.last_week)
                                trstrBuild.Append("<td class='changedown'>转跌</td>");
                            else if (objstate.week == 1)
                                trstrBuild.Append("<td ><span class='up'>上涨</span> 不变</td>");
                            else if (objstate.week == -1)
                                trstrBuild.Append("<td ><span class='down'>下跌</span> 不变</td>");
                            else
                                trstrBuild.Append("<td ></td>");

                            if (objstate.month > objstate.last_month)
                                trstrBuild.Append("<td class='changeup'>转涨*</td>");
                            else if (objstate.month < objstate.last_month)
                                trstrBuild.Append("<td class='changedown'>转跌</td>");
                            else if (objstate.month > 0)
                                trstrBuild.Append("<td ><span class='up'>上涨</span> 不变</td>");
                            else if (objstate.month < 0)
                                trstrBuild.Append("<td ><span class='down'>下跌</span> 不变</td>");
                            else
                                trstrBuild.Append("<td ></td>");
                        }
                        trstrBuild.Append("</tr>");


                    }

                    trBuilder.Append(th);
                    trBuilder.Append(trstrBuild.ToString());

                }
                string stateReuslt = style + string.Format(table, trBuilder.ToString());
                return stateReuslt;
            }
        }

        private string GetMyStockState(users u)
        {
            if (u.stock_user_map == null || u.stock_user_map.Count == 0)
                return "<b>自选股买卖点</b>:<br/>未上传自选股。";
            if (u.index_user_map == null || u.index_user_map.Count == 0)
                return "<b>自选股买卖点</b>:<br/>未上传关注的技术指标。";

            string style = @"<style type='text/css'>                           
                            .up{ color:#cc0000;}
                            .down{color:#006030;}                         
                            </style>";
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                var indexList = entity.indexdefinition.ToList();

                StringBuilder trBuilder = new StringBuilder();


                foreach (var map in u.stock_user_map)
                {

                    var stock = entity.stock.FirstOrDefault(p => p.code == map.stock_code);
                    if (stock == null)
                    {
                        this.Log().Info("系统不存在该股票:" + map.stock_code);
                        continue;
                    }
                    StringBuilder trstrBuild = new StringBuilder();
                    foreach (var tech in u.index_user_map)
                    {

                        string stateSql = @"select `code`,`category_code`,
                                                `object_code`,`index_code`,`day`,`week`,`month`,`date`,
                                                `last_day`,`last_week`,`last_month` from objectstate a where a.object_code=@p0 and a.index_code=@p1";
                        var objstate = entity.Database.SqlQuery<objectstate>(stateSql, map.stock_code, tech.index_code).FirstOrDefault();

                        if (objstate != null)
                        {
                            if (objstate.day == objstate.last_day
                                && objstate.week == objstate.last_week
                                && objstate.month == objstate.last_month)
                            {
                                continue;
                            }
                            var techName = indexList.FirstOrDefault(p => p.code == tech.index_code).name;

                            trstrBuild.Append(techName + ":<br/>");
                            if (objstate.day == 1 && objstate.day > objstate.last_day)
                                trstrBuild.Append("日:<span class='up'>买入</span><br/>");
                            else if (objstate.day < objstate.last_day)
                                trstrBuild.Append("日:<span class='down'>卖出</span><br/>");

                            if (objstate.week == 1 && objstate.week > objstate.last_week)
                                trstrBuild.Append("周:<span class='up'>买入</span><br/>");
                            else if (objstate.week < objstate.last_week)
                                trstrBuild.Append("周:<span class='down'>卖出</span><br/>");

                            if (objstate.month == 1 && objstate.month > objstate.last_month)
                                trstrBuild.Append("月:<span class='up'>买入</span><br/>");
                            else if (objstate.month < objstate.last_month)
                                trstrBuild.Append("月:<span class='down'>卖出</span><br/>");

                        }

                    }
                    if (trstrBuild.Length > 0)
                    {
                        trBuilder.Append("<p>" + stock.name + ":<br/>" + trstrBuild.ToString() + "</p>");
                    }
                }

                if (trBuilder.Length > 0)
                {
                    return style + "<b>自选股买卖点</b>:<br/>" + trBuilder.ToString();
                }
                else
                {
                    return "<b>自选股买卖点</b>:无<br/>";
                }
            }
        }
        private static string GetRecoHit(List<reco_user_stock> stockList)
        {
            string recoMsg;

            var cateDic = new Dictionary<string, IList<string>>();
            var indexDic = new Dictionary<string, IList<string>>();

            foreach (var item in stockList)
            {
                if (!cateDic.ContainsKey(item.cate_code))
                {
                    cateDic.Add(item.cate_code, new List<string>());
                }

                if (cateDic[item.cate_code].Count(p => p == item.object_code) <= 0)
                {
                    cateDic[item.cate_code].Add(item.object_code);
                }
                if (!indexDic.ContainsKey(item.index_code))
                {
                    indexDic.Add(item.index_code, new List<string>());
                }

                if (indexDic[item.index_code].Count(p => p == item.object_code) <= 0)
                {
                    indexDic[item.index_code].Add(item.object_code);
                }

            }
            //{0000:['','',],333:'',''}

            StringBuilder cateStrBuilder = new StringBuilder();
            foreach (var k in cateDic.Keys)
            {
                if (cateStrBuilder.Length == 0)
                {
                    //cateStr = k + ":" + JsonConvert.SerializeObject(cateDic[k].ToArray());
                    cateStrBuilder.Append(k);
                    cateStrBuilder.Append(":");
                    cateStrBuilder.Append(JsonConvert.SerializeObject(cateDic[k].ToArray()));
                }
                else
                {
                    //cateStr += "," + k + ":" + JsonConvert.SerializeObject(cateDic[k].ToArray());
                    cateStrBuilder.Append(",");
                    cateStrBuilder.Append(k);
                    cateStrBuilder.Append(":");
                    cateStrBuilder.Append(JsonConvert.SerializeObject(cateDic[k].ToArray()));
                }
            }
            string cateStr = string.Format("{{{0}}}", cateStrBuilder.ToString());


            StringBuilder indexStrBuild = new StringBuilder();
            foreach (var k in indexDic.Keys)
            {
                if (indexStrBuild.Length == 0)
                {
                    //indexStr = k + ":" + JsonConvert.SerializeObject(indexDic[k].ToArray());
                    indexStrBuild.Append(k);
                    indexStrBuild.Append(":");
                    indexStrBuild.Append(JsonConvert.SerializeObject(indexDic[k].ToArray()));
                }
                else
                {
                    //indexStr += "," + k + ":" + JsonConvert.SerializeObject(indexDic[k].ToArray());
                    indexStrBuild.Append(",");
                    indexStrBuild.Append(k);
                    indexStrBuild.Append(":");
                    indexStrBuild.Append(JsonConvert.SerializeObject(indexDic[k].ToArray()));
                }
            }
            string indexStr = string.Format("{{{0}}}", indexStrBuild.ToString());

            recoMsg = string.Format("{{cate:{0},tech:{1}}}", cateStr, indexStr);
            return recoMsg;
        }

        private string GetRecomendStockMsg(string title, List<reco_user_stock> stockList, string cycle)
        {
            if (stockList.Count <= 0)
                return string.Empty;

            //string msgFormat = "<table class='gridtable'><tr><th>个股推荐</th></tr><tr><td>{0}</td></tr></table>";
            string msgFormat = "<b>" + title + "</b>:{0}<br/>";
            string msg = "";
            foreach (var stock in stockList)
            {
                if (msg.Length == 0)
                    msg += string.Format("<stock code='{0}' cycle='{2}' tech='{3}'>{1}</stock>", stock.object_code, stock.object_name, cycle, stock.index_code);
                else
                    msg += "," + string.Format("<stock code='{0}' cycle='{2}' tech='{3}'>{1}</stock>", stock.object_code, stock.object_name, cycle, stock.index_code);
            }

            return string.Format(msgFormat, msg);
        }
        private string GetRecomendCateMsg(List<stockcategory> cateList)
        {
            if (cateList.Count <= 0)
                return string.Empty;

            string msgFormat = "<b>行业推荐</b>:{0}<br/>";
            string msg = "";
            foreach (var cate in cateList)
            {
                if (msg.Length == 0)
                    msg += string.Format("<cate code='{0}'>{1}</cate>", cate.code, cate.name);
                else
                    msg += "," + string.Format("<cate code='{0}'>{1}</cate>", cate.code, cate.name);
            }
            return string.Format(msgFormat, msg);
        }

        private IList<data.users> GetUserList()
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                return entity.users
                    .Include("stock_user_map")
                    .Include("object_user_map")
                    .Include("index_user_map").ToList();
            }
        }

        private void SaveUserMessage(IList<data.user_message> messageList)
        {
            using (StockManDBEntities entity = new StockManDBEntities())
            {
                foreach (var msg in messageList)
                {
                    if (!entity.user_message.Any(p => p.code == msg.code))
                    {
                        entity.user_message.Add(msg);
                    }
                }
                entity.SaveChanges();
            }
        }
    }
}
