using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockMan.Facade.Models
{
    public class IndexDefinition
    {
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string fields { get; set; }
        public string table_name { get; set; }
        public string algorithm_script { get; set; }
        public string chart_config { get; set; }
        public string parameter { get; set; }
        public int? state { get; set; }
        public decimal version { get; set; }
        public int? sort { get; set; }
        public string group_code { get; set; }
        public string group_name { get; set; }
    }

    public class IndexDefineGroup
    {
        public string code { get; set; }
        public string group_name { get; set; }
    }

    public class MyIndex
    {
        public string user_id { get; set; }
        public decimal version { get; set; }
        public IList<IndexDefinition> indexs { get; set; }
    }
}