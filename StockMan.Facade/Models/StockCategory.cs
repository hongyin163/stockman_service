using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockMan.Facade.Models
{
    public class StockCategory
    {
        public string code { get; set; }
        public string name { get; set; }
        public string group_code { get; set; }
        public string type { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> yestclose { get; set; }
        public string state { get; set; }
        public bool inhand { get; set; }
        public Nullable<int> sort { get; set; }
    }
}