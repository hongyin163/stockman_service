using StockMan.MySqlAccess;
using StockMan.Service.Cache;
using StockMan.Web.RestService.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace StockMan.Web.RestService.Controllers
{
    public class ValuesController : ApiController
    {
        [HttpGet]
        public IHttpActionResult UserName()
        {
            return Ok(this.User.Identity.Name);
        }
        // GET api/values
        public IEnumerable<string> Get()
        {
            StockManDBEntities entity = new StockManDBEntities();
            var stock = entity.stock.Find("1300332");
            return new string[] { stock.name };
        }

        // GET api/values/5
        public string Get(string id)
        {
            StockManDBEntities entity = new StockManDBEntities();
            var stock = entity.stock.Find(id);
            return stock.name;
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
        [HttpGet]
        public IHttpActionResult GetCacheData(string id)
        {
            return Ok(CacheHelper.Get(id));
        }
    }
}