using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockMan.Facade.Models
{
    public class MyRule
    {
        public string code { get; set; }
        public string name { get; set; }
        public string user_id { get; set; }
        public int state { get; set; }
        public string description { get; set; }
        public IList<Condition> conditions { get; set; }
    }

    public class Condition
    {
        public string code { get; set; }
        public string category_code { get; set; }
        public string object_code { get; set; }
        public string index_code { get; set; }
        public string category_name { get; set; }
        public string object_name { get; set; }
        public string index_name { get; set; }
        public string rule_code { get; set; }
        public int sort { get; set; }
    }
}