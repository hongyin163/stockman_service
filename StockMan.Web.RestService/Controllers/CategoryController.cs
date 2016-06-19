using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using data = StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Facade.Models;
using StockMan.Web.RestService.Filters;

namespace StockMan.Web.RestService.Controllers
{
     [IdentityBasicAuthentication]
    public class CategoryController : ApiController
    {
        IStockCategoryService categoryService = new StockCategoryService();
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
        /// 获取股票分类（行业）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<StockCategory> GetCategoryList(string id)
        {
            var list = categoryService.GetCategoryList(id);
            var l = list.Select(p => new StockMan.Facade.Models.StockCategory
            {
                code = p.code,
                group_code = p.group_code,
                name = p.name,
                price = 10,
                yestclose = 13
            });

            return l.ToList();
        }
        public IHttpActionResult GetMyCategoryList(string p1, string p2)
        {
            var list = categoryService.GetMyCategory(p1, p2);

            IList<StockCategory> l = list.Select(p => new StockCategory
             {
                 code = p.code,
                 name = p.name,
                 group_code = p.group_code
             }).ToList();
            decimal version = versionService.GetUserDataVersion(p1, data.DataVersionCode.my_category.ToString());

            return Ok<MyCategory>(new MyCategory
            {
                user_id = p1,
                group_code = p2,
                categorys = l,
                version = version

            });
        }
        public IHttpActionResult PostMyCategory(MyCategory myCategory)
        {
            if (myCategory == null
                || string.IsNullOrEmpty(myCategory.user_id)
                || string.IsNullOrEmpty(myCategory.group_code)
                || myCategory.categorys == null
                || myCategory.categorys.Count <= 0)
                return BadRequest("参数不合规");

            var list = myCategory.categorys.Select(p => new data.stockcategory_user_map
            {
                cate_code = p.code,
                user_id = myCategory.user_id,
                group_code = myCategory.group_code
            }).ToList();
            try
            {
                categoryService.AddMyCategory(list);
                versionService.UpdateUserDataVersion(myCategory.user_id, data.DataVersionCode.my_category.ToString(), myCategory.version);
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
        public IHttpActionResult GetData(string id)
        {
            var list = categoryService.GetCategoryByCode(id);

            var result = list.Select(p => new StockCategory
            {
                code = p.code,
                price = p.price,
                yestclose = p.yestclose,
                group_code = p.group_code,
                name = p.name
            }).ToList();
            return Ok(result);
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

            var list = categoryService.GetPriceInfo(p1, cycle);

            var result = list.Select(p => new PriceInfo
            {
                date = p.date,
                code = p.code,
                price = p.price??0,
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


    

    }
}
