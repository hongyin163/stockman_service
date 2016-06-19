using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Http;
using StockMan.MySqlAccess;
using dm = StockMan.EntityModel;
using StockMan.Service.Interface.Rds;
using StockMan.Service.Rds;
using StockMan.Facade.Models;
using System.Web;
using System.Web.Http.Results;
using StockMan.Web.RestService.Filters;
using System.Web.Http;
using System.Web.Http.OData;
namespace StockMan.Web.RestService.Controllers
{
    [IdentityBasicAuthentication]
    public class CommonController : ApiController
    {
        IUserDataVersionService service = new UserDataVersionService();

        public IHttpActionResult GetDataVersion(string p1, string p2)
        {
            var v = service.GetUserDataVersion(p1, p2);
            return Ok<decimal>(v);
        }

        public IHttpActionResult GetRequest(string p1, string p2)
        {
            byte[] bytes = Convert.FromBase64String(p1);

            string url = Encoding.Default.GetString(bytes);

            String jsonString = GetJsonData(url);
            String cb = p2;
            cb = HttpContext.Current.Request["callback"];
            //String responseString = "";
            //if (!String.IsNullOrEmpty(cb))
            //{
            //    responseString = cb + "(" + jsonString + ")";
            //}
            //else
            //{
            //    responseString = jsonString;
            //}
            //return 

            return Ok(jsonString);
        }
        public string GetJsonData(string reqUrl)
        {
            var url = HttpContext.Current.Server.UrlDecode(reqUrl);

            var web = WebRequest.CreateHttp(url);
            web.Proxy = new WebProxy("proxy1.bj.petrochina", 8080);
            var rsp = web.GetResponse();
            var stream = rsp.GetResponseStream();

            if (stream != null)
            {
                var sr = new StreamReader(stream, System.Text.Encoding.Default);
                var content = sr.ReadToEnd();

                return content;
            }
            return string.Empty;
        }

        public string GetTimeout(int id)
        {
            Thread.Sleep(id);
            return "";
        }
        [HttpPost]
        public IHttpActionResult ReplySuggest(dm.sys_goodidea sys_GoodIdea)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (StockManDBEntities db = new StockManDBEntities())
            {

                var item = db.sys_goodidea.FirstOrDefault(p => p.code == sys_GoodIdea.code);
                if (item == null)
                {
                    db.sys_goodidea.Add(sys_GoodIdea);
                }
                else
                {
                    item.description = sys_GoodIdea.description;
                }


                try
                {
                    db.SaveChanges();
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return Ok();
        }


        public IHttpActionResult GetUserCount()
        {
            IUserService service = new UserService();
            return Ok(service.GetUserCount());
        }
        [EnableQuery]
        public IQueryable<dm.users> Get()
        {  
            StockManDBEntities db = new StockManDBEntities();
            return db.users;
        }
    }
}
