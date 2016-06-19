using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockMan.Facade.Models
{
    public class StockQueryResult
    {
        public string code { get; set; }
        public string name { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> yestclose { get; set; }      
        public string cate { get; set; }
        public string tech { get; set; }
    }
}