using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockMan.Web.RestService.Models
{
    public class MyObject
    {
        public string user_id { get; set; }
        public string group_code { get; set; }
        public decimal version { get; set; }
        public IList<CustomObject> objects { get; set; }
    }

    public class MyCategory
    {
        public string user_id { get; set; }
        public string group_code { get; set; }
        public decimal version { get; set; }
        public IList<StockCategory> categorys { get; set; }
    }
}