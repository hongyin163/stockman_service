using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Web.RestService.Filters;
using StockMan.Facade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using data = StockMan.EntityModel;
using System.Configuration;
using StockMan.Service.Cache;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
namespace StockMan.Web.RestService.Controllers
{
    [IdentityBasicAuthentication]
    public class ObjectController : ApiController
    {
        IStockCategoryService categoryService = new StockCategoryService();
        ICustomObjectService objectService = new CustomObjectService();
        IRelatedDataService relateService = new RelatedDataService();
        IStockService stockService = new StockService();
        private IUserDataVersionService versionService = new UserDataVersionService();
        // GET api/cycle
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/category/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/category
        public void Post([FromBody]string value)
        {
        }

        // PUT api/category/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/category/5
        public void Delete(int id)
        {
        }
        /// <summary>
        /// 获取自定义指数（行业）
        /// </summary>
        /// <param name="id">分类，2,行业，3大盘，4其他</param>
        /// <returns></returns>
        public IList<CustomObject> GetObjectList(string id)
        {
            switch (id)
            {

                case "2"://行业

                    return categoryService.GetCategoryList("tencent").Select(p => new StockMan.Facade.Models.CustomObject
                    {
                        code = p.code,
                        type = id,
                        name = p.name,
                        price = 0,
                        yestclose = 0,
                        sort = 1,
                    }).ToList();
                case "4"://关联数据

                    return relateService.FindAll().Select(p => new StockMan.Facade.Models.CustomObject
                    {
                        code = p.code,
                        type = id,
                        name = p.name,
                        price = 0,
                        yestclose = 0,
                        sort = 2
                    }).ToList();

                default:
                    var list = objectService.GetObjectList(id);
                    var l = list.Select(p => new StockMan.Facade.Models.CustomObject
                    {
                        code = p.code,
                        type = id,
                        name = p.name,
                        price = p.price,
                        yestclose = p.yestclose,
                        sort = 0
                    });

                    return l.ToList();
            }


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        public IHttpActionResult GetMyObjectList()
        {
            var user_id = this.User.Identity.Name;
            var list = objectService.GetMyObject(user_id);

            IList<CustomObject> l = list.Select(p => new CustomObject
            {
                code = p.code,
                name = p.name,
                type = p.type,
                price = p.price,
                yestclose = p.yestclose
            }).ToList();

            //decimal version = versionService.GetUserDataVersion(p1, data.DataVersionCode.my_category.ToString());

            return Ok<MyObject>(new MyObject
            {
                user_id = user_id,
                //group_code = p2,
                objects = l
                //,
                //version = version

            });
        }
        public IHttpActionResult PostMyObject(MyObject myCategory)
        {
            if (myCategory == null
                || string.IsNullOrEmpty(myCategory.user_id)
                || myCategory.objects == null
                || myCategory.objects.Count <= 0)
                return BadRequest("参数不合规");

            var list = myCategory.objects.Select(p => new data.object_user_map
            {
                object_code = p.code,
                user_id = myCategory.user_id,
                object_type = p.type
            }).ToList();
            try
            {
                objectService.AddMyObject(list);
                //versionService.UpdateUserDataVersion(myCategory.user_id, data.DataVersionCode.my_object.ToString(), myCategory.version);
                return Ok<Message>(new Message() { success = true });

            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }
        }
        /// <summary>
        /// 获取类别数据
        /// </summary>
        /// <param name="id">类别code的字符串，逗号连接</param>
        /// <returns></returns>
        public IHttpActionResult PostData(string[][] id)
        {
            var p1 = id;// Newtonsoft.Json.JsonConvert.DeserializeObject<string[][]>(ids);

            List<string> objectList = new List<string>();
            List<string> categoryList = new List<string>();
            List<string> ecoList = new List<string>();
            foreach (string[] item in p1)
            {
                //行业
                if (item[0] == "2")
                {
                    categoryList.Add(item[1]);
                }
                else if (item[0] == "3")
                {
                    objectList.Add(item[1]);
                }
                else if (item[0] == "4")
                {
                    ecoList.Add(item[1]);
                }
            }
            List<CustomObject> result = new List<CustomObject>();
            //自定义对象
            var cache_objs = CacheHelper.Get<PriceInfo>(objectList.Select(p => "3_" + p).ToArray());
            if (cache_objs != null && cache_objs.Length > 0)
            {
                var t1 = cache_objs.Select(p => new CustomObject
                 {
                     code = p.code,
                     type = "3",
                     price = p.price,
                     yestclose = p.yestclose
                 }).ToList();
                result.AddRange(t1);
            }
            else
            {
                var list1 = objectService.GetDataByCode(string.Join(",", objectList));
                var result1 = list1.Select(p => new CustomObject
                          {
                              code = p.code,
                              type = "3",
                              price = p.price,
                              yestclose = p.yestclose,
                              name = p.name
                          }).ToList();
                result.AddRange(result1);
            }
            //行业
            var cache_cate = CacheHelper.Get<PriceInfo>(categoryList.Select(p => "2_" + p).ToArray());
            if (cache_cate != null && cache_cate.Length > 0)
            {
                var t2 = cache_cate.Select(p => new CustomObject
                {
                    code = p.code,
                    type = "2",
                    price = p.price,
                    yestclose = p.yestclose
                }).ToList();
                result.AddRange(t2);
            }
            else
            {
                string cateStr = string.Join(",", categoryList);
                var list2 = categoryService.GetCategoryByCode(cateStr);

                var result2 = list2.Select(p => new CustomObject
                {
                    code = p.code,
                    type = "2",
                    price = p.price,
                    yestclose = p.yestclose,
                    name = p.name
                }).ToList();
                result.AddRange(result2);
            }

            //经济指数

            string ecoStr = string.Join(",", ecoList);
            var list3 = relateService.GetDataByCode(ecoStr);

            var result3 = list3.Select(p => new CustomObject
            {
                code = p.code,
                type = "4",
                price = p.price,
                yestclose = p.yestclose,
                name = p.name
            }).ToList();
            result.AddRange(result3);

            return Ok(result);
        }


        public IHttpActionResult GetDataByCode(string id)
        {
            var list1 = objectService.GetDataByCode(id);
            var result1 = list1.Select(p => new PriceInfo
            {
                code = p.code,
                price = p.price ?? 0,
                yestclose = p.yestclose ?? 0,
                percent = p.percent ?? 0,
                volume = p.volume ?? 0,
                high = p.high ?? 0,
                low = p.low ?? 0,
                open = p.open ?? 0,
                updown = p.updown ?? 0,
                turnover = p.turnover ?? 0,
                date = p.date
            }).ToList();

            return Ok(result1);
        }
        /// <summary>
        /// 获取类别数据
        /// </summary>
        /// <param name="p1">类别code</param>
        /// <param name="p2">类型，day，week，month</param>
        /// <returns></returns>
        public IHttpActionResult GetPriceInfo(string p1, string p2)
        {
            if (string.IsNullOrEmpty(p1) || string.IsNullOrEmpty(p2))
                return BadRequest("参数不合规");

            data.TechCycle cycle;
            if (!Enum.TryParse(p2, true, out cycle))
            {
                return BadRequest("参数不合规");
            }

            var list = objectService.GetPriceInfo(p1, cycle);

            var result = list.Select(p => new PriceInfo
            {
                date = p.date,
                code = p.code,
                price = p.price ?? 0,
                yestclose = p.yestclose ?? 0,
                high = p.high ?? 0,
                low = p.low ?? 0,
                open = p.open ?? 0,
                percent = p.percent ?? 0,
                updown = p.updown ?? 0,
                volume = p.volume ?? 0
            }).ToList();
            return Ok(result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1">code，编码</param>
        /// <param name="p2">周期，day,week,month</param>
        /// <param name="p3">数据类型，1，个股，2，行业，3，大盘</param>
        /// <returns></returns>
        public IHttpActionResult GetData(string p1, string p2, string p3)
        {
            if (string.IsNullOrEmpty(p1) || string.IsNullOrEmpty(p2))
                return BadRequest("参数不合规");

            data.TechCycle cycle;
            if (!Enum.TryParse(p2, true, out cycle))
            {
                return BadRequest("参数不合规");
            }
            string result = string.Empty;

            if (p3 == "3")
            {
                switch (p2.ToLower())
                {
                    case "day":
                        result = objectService.GetData<data.data_object_day_latest>(p1);
                        break;
                    case "week":
                        result = objectService.GetData<data.data_object_week_latest>(p1);
                        break;
                    case "month":
                        result = objectService.GetData<data.data_object_month_latest>(p1);
                        break;

                }
            }
            else if (p3 == "2")
            {
                switch (p2.ToLower())
                {
                    case "day":
                        result = objectService.GetData<data.data_category_day_latest>(p1);
                        break;
                    case "week":
                        result = objectService.GetData<data.data_category_week_latest>(p1);
                        break;
                    case "month":
                        result = objectService.GetData<data.data_category_month_latest>(p1);
                        break;

                }

            }
            else if (p3 == "1")
            {
                switch (p2.ToLower())
                {
                    case "day":
                        result = objectService.GetData<data.data_stock_day_latest>(p1);
                        break;
                    case "week":
                        result = objectService.GetData<data.data_stock_week_latest>(p1);
                        break;
                    case "month":
                        result = objectService.GetData<data.data_stock_month_latest>(p1);
                        break;

                }

            }

            return Ok(result);
        }

        /// <summary>
        /// 获取K线数据，旧版本
        /// </summary>
        /// <param name="p1">code，编码</param>
        /// <param name="p2">周期，day,week,month</param>
        /// <param name="p3">数据类型，1，个股，2，行业，3，大盘</param>
        /// <returns>{current:[],history:[]}</returns>
        public IHttpActionResult GetAllData(string p1, string p2, string p3)
        {

            if (string.IsNullOrEmpty(p1) || string.IsNullOrEmpty(p2))
                return BadRequest("参数不合规");

            data.TechCycle cycle;
            if (!Enum.TryParse(p2, true, out cycle))
            {
                return BadRequest("参数不合规");
            }

            string current = getCurrentDataFromCache(p1, p2, p3);
            string history = getLastDataFromCache(p1, p2, p3);// CacheHelper.Get<string>(string.Format("{0}_{1}_{2}", p3, p1, p2.ToLower()));
            //日线不处理
            //周线，当前价格+current计算得当前周
            //月线，当前价格+current计算得当前月
            switch (p2.ToLower())
            {
                case "day":
                    //return Ok(string.Format("{{current:{0},history:{1}}}", current, history));
                    break;
                case "week":
                case "month":
                    var dayPriceStr = getCurrentDataFromCache(p1, "day", p3);
                    if (!string.IsNullOrEmpty(dayPriceStr))
                    {
                        PriceInfo currentPrice = JsonConvert.DeserializeObject<PriceInfo>(dayPriceStr);

                        PriceInfo currentCycle = null;
                        if (!string.IsNullOrEmpty(current))
                        {
                            currentCycle = JsonConvert.DeserializeObject<PriceInfo>(current);

                            currentCycle.price = currentPrice.price;
                            currentCycle.high = currentPrice.high >= currentCycle.high ? currentPrice.high : currentCycle.high;
                            currentCycle.low = currentPrice.low <= currentCycle.low ? currentPrice.low : currentCycle.low;
                            currentCycle.volume = currentCycle.volume + currentPrice.volume;
                            currentCycle.turnover = currentCycle.turnover + currentPrice.volume;
                            if (currentCycle.yestclose.HasValue)
                            {
                                currentCycle.percent = Math.Round(((currentPrice.price - currentCycle.yestclose ?? 0) * 100) / currentCycle.yestclose.Value, 2);
                            }
                            else
                            {
                                currentCycle.percent = 0;
                            }
                        }
                        else
                        {
                            currentCycle = currentPrice;
                        }
                        current = JsonConvert.SerializeObject(currentCycle);
                    }
                    break;
            }
            if (string.IsNullOrEmpty(current))
                current = "null";
            if (string.IsNullOrEmpty(history))
                history = "[]";
            return Ok(string.Format("{{current:{0},history:{1}}}", current, history));
        }
        /// <summary>
        /// 获取K线数据
        /// </summary>
        /// <param name="p1">code，编码</param>
        /// <param name="p2">周期，day,week,month</param>
        /// <param name="p3">数据类型，1，个股，2，行业，3，大盘</param>
        /// <returns>{current:[],history:[]}</returns>
        public IHttpActionResult GetKData(string p1, string p2, string p3)
        {

            if (string.IsNullOrEmpty(p1) || string.IsNullOrEmpty(p2))
                return BadRequest("参数不合规");

            data.TechCycle cycle;
            if (!Enum.TryParse(p2, true, out cycle))
            {
                return BadRequest("参数不合规");
            }

            string current = getCurrentDataFromCache(p1, p2, p3);
            string history = getLastDataFromCache(p1, p2, p3);// CacheHelper.Get<string>(string.Format("{0}_{1}_{2}", p3, p1, p2.ToLower()));
            //日线不处理
            //周线，当前价格+current计算得当前周
            //月线，当前价格+current计算得当前月
            switch (p2.ToLower())
            {
                case "day":
                    //return Ok(string.Format("{{current:{0},history:{1}}}", current, history));
                    break;
                case "week":
                case "month":
                    var dayPriceStr = getCurrentDataFromCache(p1, "day", p3);
                    if (!string.IsNullOrEmpty(dayPriceStr))
                    {
                        PriceInfo currentPrice = JsonConvert.DeserializeObject<PriceInfo>(dayPriceStr);

                        PriceInfo currentCycle = null;
                        if (!string.IsNullOrEmpty(current))
                        {
                            currentCycle = JsonConvert.DeserializeObject<PriceInfo>(current);

                            currentCycle.price = currentPrice.price;
                            currentCycle.high = currentPrice.high >= currentCycle.high ? currentPrice.high : currentCycle.high;
                            currentCycle.low = currentPrice.low <= currentCycle.low ? currentPrice.low : currentCycle.low;
                            currentCycle.volume = currentCycle.volume + currentPrice.volume;
                            currentCycle.turnover = currentCycle.turnover + currentPrice.volume;
                            if (currentCycle.yestclose.HasValue)
                            {
                                currentCycle.percent = Math.Round(((currentPrice.price - currentCycle.yestclose ?? 0) * 100) / currentCycle.yestclose.Value, 2);
                            }
                            else
                            {
                                currentCycle.percent = 0;
                            }
                        }
                        else
                        {
                            currentCycle = currentPrice;
                        }
                        current = JsonConvert.SerializeObject(currentCycle);
                    }
                    break;
            }
            if (string.IsNullOrEmpty(current))
                current = "null";
            if (string.IsNullOrEmpty(history))
                history = "[]";
            return new StringToJsonResult(string.Format("{{\"current\":{0},\"history\":{1}}}", current, history),this.Request);
        }
        /// <summary>
        /// 返回日周月三线的数据
        /// </summary>
        /// <param name="p1">code</param>
        /// <param name="p2">类别</param>
        /// <returns></returns>
        public IHttpActionResult GetKDataAllCycle(string p1, string p2)
        {

            if (string.IsNullOrEmpty(p1) || string.IsNullOrEmpty(p2))
                return BadRequest("参数不合规");

            List<string> result = new List<string>();
            string[] cycles = new string[3] { "day", "week", "month" };
            foreach (var cycle in cycles)
            {
                string current = getCurrentDataFromCache(p1, cycle, p2);
                string history = getLastDataFromCache(p1, cycle, p2);// CacheHelper.Get<string>(string.Format("{0}_{1}_{2}", p3, p1, p2.ToLower()));
                //日线不处理
                //周线，当前价格+current计算得当前周
                //月线，当前价格+current计算得当前月
                switch (cycle.ToLower())
                {
                    case "day":
                        //return Ok(string.Format("{{current:{0},history:{1}}}", current, history));
                        break;
                    case "week":
                    case "month":
                        var dayPriceStr = getCurrentDataFromCache(p1, "day", p2);
                        if (!string.IsNullOrEmpty(dayPriceStr))
                        {
                            PriceInfo currentPrice = JsonConvert.DeserializeObject<PriceInfo>(dayPriceStr);

                            PriceInfo currentCycle = null;
                            if (!string.IsNullOrEmpty(current))
                            {
                                currentCycle = JsonConvert.DeserializeObject<PriceInfo>(current);

                                currentCycle.price = currentPrice.price;
                                currentCycle.high = currentPrice.high >= currentCycle.high ? currentPrice.high : currentCycle.high;
                                currentCycle.low = currentPrice.low <= currentCycle.low ? currentPrice.low : currentCycle.low;
                                currentCycle.volume = currentCycle.volume + currentPrice.volume;
                                currentCycle.turnover = currentCycle.turnover + currentPrice.volume;
                                if (currentCycle.yestclose.HasValue)
                                {
                                    currentCycle.percent = Math.Round(((currentPrice.price - currentCycle.yestclose ?? 0) * 100) / currentCycle.yestclose.Value, 2);
                                }
                                else
                                {
                                    currentCycle.percent = 0;
                                }
                            }
                            else
                            {
                                currentCycle = currentPrice;
                            }
                            current = JsonConvert.SerializeObject(currentCycle);
                        }
                        break;
                }
                if (string.IsNullOrEmpty(current))
                    current = "null";
                if (string.IsNullOrEmpty(history))
                    history = "[]";
                result.Add(string.Format("\"{0}\":{{\"current\":{1},\"history\":{2}}}", cycle, current, history));
            }
            return new StringToJsonResult("{" + String.Join(",", result) + "}", this.Request);
        }

        public IHttpActionResult GetStringToJson()
        {
            return new StringToJsonResult("{\"a\":\"1\"}", this.Request);
        }
        public IHttpActionResult GetKDataRandom()
        {
            var code = stockService.GetCodeRandom();
            var name = stockService.GetStocksByIds(code).First().name;

            string history = getLastDataFromCache(code, "day", "1");// CacheHelper.Get<string>(string.Format("{0}_{1}_{2}", p3, p1, p2.ToLower()));

            if (string.IsNullOrEmpty(history))
                history = "[]";
            return Ok(string.Format("{{name:\"{0}\",data:{1}}}", name, history));
        }
        public IHttpActionResult GetKDataNiu()
        {
            var code = stockService.GetCodeRandom();
            var name = stockService.GetStocksByIds(code).First().name;
            DateTime start = new DateTime(2014, 10, 1);
            DateTime end = new DateTime(2015, 7, 9);

            string history = stockService.GetData<data.data_stock_day_latest>(code, start, end); ;// CacheHelper.Get<string>(string.Format("{0}_{1}_{2}", p3, p1, p2.ToLower()));

            if (string.IsNullOrEmpty(history))
                history = "[]";

            if (history == "[]")
            {
                code = stockService.GetCodeRandom();
                name = stockService.GetStocksByIds(code).First().name;
                history = stockService.GetData<data.data_stock_day_latest>(code, start, end); ;// CacheHelper.Get<string>(string.Format("{0}_{1}_{2}", p3, p1, p2.ToLower()));
            }
            return Ok(string.Format("{{name:\"{0}\",data:{1}}}", name, history));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1">类别编码</param>
        /// <param name="p2">对象编码</param>
        /// <returns></returns>
        public IHttpActionResult GetCurrentData(string p1, string p2)
        {
            string str = getObjectInfoFromCache(p1, p2);
            string mediaType = this.Request.Headers.Accept.First().MediaType;
            if (mediaType == "application/json")
            {
                return new StringToJsonResult(str, this.Request);
            }
            return Ok(str);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1">编码</param>
        /// <param name="p2">周期</param>
        /// <param name="p3">数据类型</param>
        /// <returns></returns>
        private string getLastDataFromCache(string code, string cycle, string dataType)
        {
            return CacheHelper.Get(string.Format("{0}_{1}_{2}_history", dataType, code, cycle).ToLower());
        }

        private string getCurrentDataFromCache(string code, string cycle, string dataType)
        {
            return CacheHelper.Get(string.Format("{0}_{1}_{2}_current", dataType, code, cycle.ToLower()));

        }

        private string getObjectInfoFromCache(string dataType, string code)
        {
            return CacheHelper.Get(string.Format("{0}_{1}", dataType, code));

        }


    }



}