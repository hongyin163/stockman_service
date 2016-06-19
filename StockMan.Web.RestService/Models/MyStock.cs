using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace StockMan.Web.RestService.Models
{
    /// <summary>
    /// 自选股模型
    /// </summary>
    public class Stock
    {
        public string id { get; set; }
        public string date { get; set; }
        public string code { get; set; }
        public string symbol { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> yestclose { get; set; }
        public string state { get; set; }
        public bool inhand { get; set; }
        public Nullable<int> sort { get; set; }
    }

    public class MyStock
    {
        public string user_id { get; set; }
        public decimal version { get; set; }
        //public IList<string> codes { get; set; }
        public IList<Stock> stocks { get; set; }
    }
}