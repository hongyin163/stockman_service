using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StockMan.Web.RestService.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            //var client = new RedisClient(ConfigurationManager.AppSettings["RedisHost"] + "", int.Parse(ConfigurationManager.AppSettings["RedisPort"] + ""));

            return View();
        }
    }
}
