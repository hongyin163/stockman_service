using System;

namespace StockMan.Web.RestService.Models
{
    public class ObjectState
    {
        public string code { get; set; }
        public string category_code { get; set; }
        public string object_code { get; set; }
        public string index_code { get; set; }
        public int? day { get; set; }
        public int? week { get; set; }
        public int? month { get; set; }
        public DateTime? date { get; set; }
        public int? last_day { get; set; }
        public int? last_week { get; set; }
        public int? last_month { get; set; }
    }
}