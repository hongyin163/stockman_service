using StockMan.MySqlAccess;
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
        // GET api/values
        public IEnumerable<string> Get()
        {
            StockManDBEntities entity = new StockManDBEntities();
            var stock = entity.stock.Find("1300332");
            return new string[] { stock.name };
        }

        // GET api/values/5
        public string Get(int id)
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
    }
}