using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockMan.Web.RestService.Models
{
    public class GoodIdea
    {
        public string code { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int up { get; set; }
        public int down { get; set; }
    }
}