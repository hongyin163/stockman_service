using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.EntityModel.dto
{
    public class StockQueryResult
    {
        public string code { get; set; }
        public string name { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> yestclose { get; set; }
        public string cate { get; set; }
        public string tech { get; set; }
        public int? day { get; set; }
        public int? week { get; set; }
        public int? month { get; set; }
        public int? last_day { get; set; }
        public int? last_week { get; set; }
        public int? last_month { get; set; }
    }
    public class StockCrossQueryResult : StockQueryResult
    {
        public string tag { get; set; }
        public string cycle { get; set; }
    }

    public class StockQueryCount
    {
        public string cate_code { get; set; }
        public int? count { get; set; }
    }
}
