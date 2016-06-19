using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using StockMan.MySqlAccess;
using dm = StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Facade.Models;
using StockMan.Web.RestService.Filters;
using StockMan.Service.Cache;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Data;
namespace StockMan.Web.RestService.Controllers
{
    [IdentityBasicAuthentication]
    public class StockController : ApiController
    {
        //private StockManDBEntities db = new StockManDBEntities();
        private IStockService service = new StockService();
        private IUserDataVersionService versionService = new UserDataVersionService();
        // GET api/Stock
        public IEnumerable<Stock> GetStocks(string id)
        {
            var results = CacheHelper.Get(id.Split(',').Select(s => "1_" + s).ToArray());
            if (results != null && results.Length > 0)
            {
                return results
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Select(p => JsonConvert.DeserializeObject<Stock>(p));
            }
            else
            {
                var list = service.GetStocksByIds(id);
                return list.Select(p => new Stock
                {
                    name = p.name,
                    code = p.code,
                    price = p.price,
                    yestclose = p.yestclose,
                    symbol = p.symbol,
                    volume = p.volume,
                    turnover = p.turnover,
                    high = p.high,
                    updown = p.updown,
                    low = p.low,
                    turnoverrate = p.turnoverrate,
                    pe = p.pe,
                    pb = p.pb,
                    fv = p.fv,
                    mv = p.mv,
                    percent = p.percent
                });
            }
        }

        /// <summary>
        /// 获取K线数据,旧版本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IHttpActionResult GetStockInfo(string id)
        {
            var results = CacheHelper.Get(id.Split(',').Select(s => "1_" + s).ToArray());
            if (results != null && results.Length > 0)
            {
                return Ok(string.Format("[{0}]", string.Join(",", results)));
            }
            else
            {
                var list = service.GetStocksByIds(id);
                var result = list.Select(p => new Stock
                {
                    name = p.name,
                    code = p.code,
                    price = p.price,
                    yestclose = p.yestclose,
                    symbol = p.symbol,
                    volume = p.volume,
                    turnover = p.turnover,
                    high = p.high,
                    updown = p.updown,
                    low = p.low,
                    turnoverrate = p.turnoverrate,
                    pe = p.pe,
                    pb = p.pb,
                    fv = p.fv,
                    mv = p.mv,
                    percent = p.percent
                });
                return Ok<IEnumerable<Stock>>(result);
            }
        }
        /// <summary>
        /// 获取K线数据，新版本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IHttpActionResult GetPrice(string id) {

            var results = CacheHelper.Get(id.Split(',').Select(s => "1_" + s).ToArray());
            if (results != null && results.Length > 0)
            {
                return new StringToJsonResult(string.Format("[{0}]", string.Join(",", results)),this.Request);
            }
            else
            {
                var list = service.GetStocksByIds(id);
                var result = list.Select(p => new Stock
                {
                    name = p.name,
                    code = p.code,
                    price = p.price,
                    yestclose = p.yestclose,
                    symbol = p.symbol,
                    volume = p.volume,
                    turnover = p.turnover,
                    high = p.high,
                    updown = p.updown,
                    low = p.low,
                    turnoverrate = p.turnoverrate,
                    pe = p.pe,
                    pb = p.pb,
                    fv = p.fv,
                    mv = p.mv,
                    percent = p.percent
                });
                return Ok<IEnumerable<Stock>>(result);
            }
        }
        // GET api/Stock/5
        public Stock GetStock(string id)
        {
            var stock = service.Find(id);
            if (stock == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return new Stock
            {
                code = stock.code,
                name = stock.name,
                price = stock.price,
                symbol = stock.symbol,
                yestclose = stock.yestclose
            };
        }


        // PUT api/Stock/5
        public IHttpActionResult PutStock(Stock stock)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            service.Update(new dm.stock()
            {

                code = stock.code,
                name = stock.name,
                price = stock.price,
                symbol = stock.symbol,
                yestclose = stock.yestclose

            });
            return Ok();
            //db.Entry(stock).State = EntityState.Modified;

            //try
            //{
            //    db.SaveChanges();
            //}
            //catch (DbUpdateConcurrencyException ex)
            //{
            //    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            //}

            //return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/Stock
        public IHttpActionResult PostStock(Stock stock)
        {
            if (ModelState.IsValid)
            {
                service.Add(new dm.stock()
                {
                    code = stock.code,
                    name = stock.name,
                    price = stock.price,
                    symbol = stock.symbol,
                    yestclose = stock.yestclose

                });

                return Ok<Stock>(stock);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        // DELETE api/Stock/5
        public IHttpActionResult DeleteStock(string id)
        {
            service.Delete(new dm.stock
            {
                code = id
            });

            return Ok();
            //Stock stock = db.Stock.Find(id);
            //if (stock == null)
            //{
            //    return Request.CreateResponse(HttpStatusCode.NotFound);
            //}

            //db.Stock.Remove(stock);

            //try
            //{
            //    db.SaveChanges();
            //}
            //catch (DbUpdateConcurrencyException ex)
            //{
            //    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            //}

            //return Request.CreateResponse(HttpStatusCode.OK, stock);
        }
        /// <summary>
        /// 同步上传
        /// </summary>
        /// <param name="myStock"></param>
        /// <returns></returns>
        [HttpOptions]
        [HttpPost]
        public IHttpActionResult PostMyStocks(MyStock myStock)
        {
            IList<dm.stock_user_map> list = (from t in myStock.stocks
                                             select new dm.stock_user_map
                                          {
                                              group_name = "default",
                                              user_id = myStock.user_id,
                                              stock_code = t.code,
                                              inhand = t.inhand,
                                              sort = t.sort
                                          }).ToList();

            service.SyncMyStock(myStock.user_id, list);
            versionService.UpdateUserDataVersion(myStock.user_id, dm.DataVersionCode.my_stock.ToString(), myStock.version);

            return Ok<Message>(new Message { success = true });
        }
        /// <summary>
        /// 同步下载
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IHttpActionResult GetMyStocks(string id)
        {
            var list = service.GetMyStock(id);

            //IList<string> codeList = list.Select(p => p.).ToList();

            var query = from t in list
                        select new Stock
                        {
                            code = t.stock_code,
                            symbol = t.stock.symbol,
                            name = t.stock.name,
                            sort = t.sort,
                            inhand = t.inhand ?? true
                        };
            decimal version = versionService.GetUserDataVersion(id, dm.DataVersionCode.my_stock.ToString());

            var myStock = new MyStock()
            {
                user_id = id,
                stocks = query.ToList(),
                version = version
            };

            return Ok<MyStock>(myStock);
        }
        /// <summary>
        /// 根据股票类别，获取股票
        /// </summary>
        /// <param name="p1">股票类别</param>
        /// <param name="p2">页码</param>
        /// <param name="p3">页数</param>
        /// <returns></returns>
        public IHttpActionResult GetStockByCategory(string p1, int p2, int p3)
        {
            var list = service.GetStockByCategory(p1, p2, p3);

            var stocks = list.Select(p => new Stock()
             {
                 name = p.name,
                 price = p.price,
                 code = p.code,
                 yestclose = p.yestclose
             });
            return Ok(stocks);
        }

        protected override void Dispose(bool disposing)
        {
            service.Dispose();
            base.Dispose(disposing);
        }

        [HttpPost]
        public IHttpActionResult FindStockBy(StockQueryCondition condition)
        {

            //需要和用户关注行业和技术关联

            var query = service.FindStockFromPoolBy(new dm.dto.StockQueryCondition
            {
                user_id = condition.user_id,
                price = condition.price,
                pe = condition.pe,
                pb = condition.pb,
                mv = condition.mv,
                fv = condition.fv,
                cate = condition.cate,
                tech = condition.tech
            });

            if (condition.user_id == "guest")
            {
                return Ok(query.Take(5).ToList());
            }
            return Ok(query);
        }


        [HttpPost]
        public IHttpActionResult FindStockRankBy(StockQueryCondition condition)
        {

            //需要和用户关注行业和技术关联

            var query = service.FindStockFromRankPoolBy(new dm.dto.StockQueryCondition
            {
                user_id = condition.user_id,
                price = condition.price,
                pe = condition.pe,
                pb = condition.pb,
                mv = condition.mv,
                fv = condition.fv,
                cate = condition.cate
            });

            if (condition.user_id == "guest")
            {
                return Ok(query.Take(5).ToList());
            }
            return Ok(query);
        }
        [HttpPost]
        public IHttpActionResult GetRecoCateCount(StockQueryCondition condition)
        {
            var query = service.FindStockCountFromRankPoolBy(new dm.dto.StockQueryCondition
            {
                user_id = condition.user_id,
                price = condition.price,
                pe = condition.pe,
                pb = condition.pb,
                mv = condition.mv,
                fv = condition.fv,
                cate = condition.cate
            });

            return Ok(query);

        }
        /// <summary>
        /// 返回推荐的总数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult GetRecoTotalCount(StockQueryCondition condition)
        {
            //string[] codeList = id.Split('$');

            //string[] cateList = codeList[0].Split(',');

            //string[] indexList = codeList[1].Split(',');

            //using (StockManDBEntities db = new StockManDBEntities())
            //{
            //    //var sql = @"select cate_code, count(cate_code) as count from reco_category_count where cate_code in ('013500','013200','','012400') and index_code in ('T0001','T0003','T0002') group by cate_code";
            //    var total = db.reco_category_count.Where(p => cateList.Contains(p.cate_code)).Sum(p=>p.count);

            //    return Ok(total);
            //}


            var query = service.FindStockCountFromRankPoolBy(new dm.dto.StockQueryCondition
            {
                user_id = condition.user_id,
                price = condition.price,
                pe = condition.pe,
                pb = condition.pb,
                mv = condition.mv,
                fv = condition.fv,
                cate = condition.cate
            });

            return Ok(query.Sum(p => p.count));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">code编码，000323,132323,123123</param>
        /// <returns></returns>
        public IHttpActionResult GetPriceByIds(string id)
        {
            //返回当前所有编码的个股价格信息
            //格式{123123：{}，12123123:{}}
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult FindCrossStock(StockQueryCondition condition)
        {
            //需要和用户关注行业和技术关联

            var query = service.FindCrossStockBy(new dm.dto.StockQueryCondition
            {
                user_id = condition.user_id,
                price = condition.price,
                pe = condition.pe,
                pb = condition.pb,
                mv = condition.mv,
                fv = condition.fv,
                cate = condition.cate,
                tech = condition.tech,
                cycle = condition.cycle
            });

            //if (condition.user_id == "guest")
            //{
            //    return Ok(query.Take(5).ToList());
            //}
            return Ok(query);
        }
        [HttpPost]
        public IHttpActionResult GetCrossStockCount(StockQueryCondition condition)
        {
            var query = service.FindCrossStockCountBy(new dm.dto.StockQueryCondition
            {
                user_id = condition.user_id,
                price = condition.price,
                pe = condition.pe,
                pb = condition.pb,
                mv = condition.mv,
                fv = condition.fv,
                cate = condition.cate,
                tech = condition.tech
            });

            return Ok(query.Sum(p => p.count));
        }


        [HttpPost]
        public IHttpActionResult FindStateStock(StockQueryCondition condition)
        {
            //需要和用户关注行业和技术关联

            var query = service.FindStateStockBy(new dm.dto.StockQueryCondition
            {
                user_id = condition.user_id,
                price = condition.price,
                pe = condition.pe,
                pb = condition.pb,
                mv = condition.mv,
                fv = condition.fv,
                cate = condition.cate,
                tech = condition.tech,
                cycle = condition.cycle
            });

            //if (condition.user_id == "guest")
            //{
            //    return Ok(query.Take(5).ToList());
            //}
            return Ok(query);
        }
        [HttpPost]
        public IHttpActionResult GetStateStockCount(StockQueryCondition condition)
        {
            var query = service.FindStateStockCountBy(new dm.dto.StockQueryCondition
            {
                user_id = condition.user_id,
                price = condition.price,
                pe = condition.pe,
                pb = condition.pb,
                mv = condition.mv,
                fv = condition.fv,
                cate = condition.cate,
                tech = condition.tech
            });

            return Ok(query.Sum(p => p.count));
        }
        public IHttpActionResult GetStockGroup(string id)
        {
            string[] ids = id.Split(',');

            var datas = service.GetStockGroup(ids);
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonTextWriter jtw = new JsonTextWriter(sw);
            jtw.WriteStartObject();

            foreach (DataRow row in datas.Rows)
            {

                jtw.WritePropertyName(row["stock_code"] + "");
                jtw.WriteStartObject();
                jtw.WritePropertyName("code");
                jtw.WriteValue(row["cate_code"] + "");
                jtw.WritePropertyName("name");
                jtw.WriteValue(row["cate_name"] + "");

                var p = CacheHelper.Get<PriceInfo>(string.Format("{0}_{1}", 2, row["cate_code"]));
                if (p != null)
                {
                    jtw.WritePropertyName("price");
                    jtw.WriteValue(p.price);
                    jtw.WritePropertyName("yestclose");
                    jtw.WriteValue(p.yestclose);
                }
                jtw.WriteEndObject();

            }
            //foreach (string key in datas.Keys)
            //{
            //    jtw.WritePropertyName(key);
            //    jtw.WriteValue(datas[key]);
            //}
            jtw.WriteEndObject();
            jtw.Flush();
            jtw.Close();

            return Ok(sb.ToString());
        }
    }
}