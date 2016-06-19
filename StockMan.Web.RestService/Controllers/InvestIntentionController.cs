using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using StockMan.MySqlAccess;
using StockMan.EntityModel;
using StockMan.Facade.Models;
using System.Web;
using StockMan.Web.RestService.Filters;
namespace StockMan.Web.RestService.Controllers
{
    //[IdentityBasicAuthentication]
    public class InvestIntentionController : ApiController
    {
        private StockManDBEntities db = new StockManDBEntities();

        // GET: api/InvestIntention
        public IQueryable<user_investintention> GetUser_InvestIntention()
        {
            return db.user_investintention;
        }

        // GET: api/InvestIntention/5
        [ResponseType(typeof(user_investintention))]
        public IHttpActionResult GetUser_InvestIntention(string id)
        {
            user_investintention user_InvestIntention = db.user_investintention.Find(id);
            if (user_InvestIntention == null)
            {
                return NotFound();
            }

            return Ok(user_InvestIntention);
        }

        // PUT: api/InvestIntention/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUser_InvestIntention(string id, user_investintention user_InvestIntention)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user_InvestIntention.code)
            {
                return BadRequest();
            }

            db.Entry(user_InvestIntention).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!User_InvestIntentionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/InvestIntention
        [ResponseType(typeof(user_investintention))]
        public IHttpActionResult PostUser_InvestIntention(user_investintention user_InvestIntention)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (User_InvestIntentionExists(user_InvestIntention.code))
            {
                db.Entry(user_InvestIntention).State = EntityState.Modified;
            }
            else
            {
                db.user_investintention.Add(user_InvestIntention);
            }

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {

                throw;

            }

            return Ok();
        }

        // DELETE: api/InvestIntention/5
        [ResponseType(typeof(user_investintention))]
        public IHttpActionResult DeleteUser_InvestIntention(string id)
        {
            user_investintention user_InvestIntention = db.user_investintention.Find(id);
            if (user_InvestIntention == null)
            {
                return NotFound();
            }

            db.user_investintention.Remove(user_InvestIntention);
            db.SaveChanges();

            return Ok(user_InvestIntention);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool User_InvestIntentionExists(string id)
        {
            return db.user_investintention.Count(e => e.code == id) > 0;
        }

        public IQueryable<sys_investdata> GetInvestItem()
        {
            return db.sys_investdata;

            //if (HttpContext.Current.Cache.Get(id) == null)
            //{
            //    var result = db.Sys_InvestData
            //        .Where(p => p.type == id)
            //        .Select(p => new IntentionItem
            //        {
            //            code = p.code,
            //            value = p.value,
            //            name = p.name,
            //            state = 0
            //        }).ToList();
            //    HttpContext.Current.Cache.Insert(id, result);
            //    return Ok<IList<IntentionItem>>(result);
            //}
            //else
            //{
            //    return Ok<IList<IntentionItem>>((IList<IntentionItem>)HttpContext.Current.Cache.Get(id));
            //}



            //if (id == "tech")
            //{
            //    if (HttpContext.Current.Cache.Get("tech") == null)
            //    {
            //        var result = db.IndexDefinition.Select(p => new IntentionItem
            //            {
            //                code = p.code,
            //                name = p.name,
            //                state = 0
            //            }).ToList();
            //        HttpContext.Current.Cache.Insert("tech", result);
            //        return Ok<IList<IntentionItem>>(result);
            //    }
            //    else
            //    {
            //        return Ok<IList<IntentionItem>>((IList<IntentionItem>)HttpContext.Current.Cache.Get("tech"));
            //    }


            //}
            //else if (id == "cate")
            //{
            //    if (HttpContext.Current.Cache.Get("cate") == null)
            //    {
            //        var result = db.StockCategory.Select(p => new IntentionItem
            //           {
            //               code = p.code,
            //               name = p.name,
            //               state = 0
            //           }).ToList();
            //        HttpContext.Current.Cache.Insert("cate", result);
            //        return Ok<IList<IntentionItem>>(result);
            //    }
            //    else
            //    {
            //        return Ok<IList<IntentionItem>>((IList<IntentionItem>)HttpContext.Current.Cache.Get("cate"));
            //    }

            //}
            //else if (id == "learn")
            //{
            //    IList<IntentionItem> list = new List<IntentionItem>();
            //    list.Add(new IntentionItem { code = "0", name = "自己看书", state = 0 });
            //    list.Add(new IntentionItem { code = "1", name = "听牛人讲解", state = 0 });
            //    list.Add(new IntentionItem { code = "2", name = "无师自通", state = 0 });
            //    return Ok<IList<IntentionItem>>(list);
            //}
            //else if (id == "tool")
            //{
            //    IList<IntentionItem> list = new List<IntentionItem>();
            //    list.Add(new IntentionItem { code = "0", name = "大智慧", state = 0 });
            //    list.Add(new IntentionItem { code = "1", name = "同花顺", state = 0 });
            //    list.Add(new IntentionItem { code = "2", name = "益萌", state = 0 }); 
            //    list.Add(new IntentionItem { code = "3", name = "腾讯自选股", state = 0 });
            //    list.Add(new IntentionItem { code = "4", name = "东方财富通", state = 0 });
            //    list.Add(new IntentionItem { code = "5", name = "和讯", state = 0 });
            //    list.Add(new IntentionItem { code = "6", name = "券商软件", state = 0 });
            //    list.Add(new IntentionItem { code = "7", name = "全民股神", state = 0 });
            //    list.Add(new IntentionItem { code = "8", name = "雪球", state = 0 });
            //    list.Add(new IntentionItem { code = "9", name = "骑牛", state = 0 });
            //    list.Add(new IntentionItem { code = "7", name = "牛股宝", state = 0 });               
            //    list.Add(new IntentionItem { code = "9", name = "牛股王", state = 0 });
            //    list.Add(new IntentionItem { code = "10", name = "好股互动", state = 0 });
            //    list.Add(new IntentionItem { code = "11", name = "顾优模拟炒股", state = 0 });
            //    list.Add(new IntentionItem { code = "11", name = "股票雷达", state = 0 });
            //    list.Add(new IntentionItem { code = "11", name = "妙财量化选股", state = 0 });
            //    list.Add(new IntentionItem { code = "12", name = "其他", state = 0 });
            //    return Ok<IList<IntentionItem>>(list);
            //}
            //return Ok();
        }

        public IList<sys_investdata> GetInvestByType([FromUri] string type)
        {
            var key = "investcate-" + type;
            if (HttpContext.Current.Cache.Get(key) == null)
            {
                var list = db.sys_investdata.Where(p => p.type == type).ToList();
                HttpContext.Current.Cache.Insert(key, list);
                return list;
            }
            else
            {
                return (IList<sys_investdata>)HttpContext.Current.Cache.Get(key);
            }
        }
    }
}