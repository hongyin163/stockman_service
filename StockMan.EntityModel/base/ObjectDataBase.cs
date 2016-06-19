using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.EntityModel
{
    public class ObjectDataBase : EntityBase
    {
        public ObjectDataBase(){}
        public string code { get; set; }
        public string object_code { get; set; }
        public System.DateTime date { get; set; }
        public Nullable<decimal> open { get; set; }
        public Nullable<decimal> low { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> updown { get; set; }
        public Nullable<decimal> turnoverrate { get; set; }
        public Nullable<decimal> yestclose { get; set; }
        public Nullable<decimal> pe { get; set; }
        public Nullable<decimal> pb { get; set; }
        public Nullable<decimal> fv { get; set; }
        public Nullable<decimal> mv { get; set; }
        public Nullable<decimal> surgedprice { get; set; }
        public Nullable<decimal> declineprice { get; set; }
        public Nullable<decimal> volume { get; set; }
        public Nullable<decimal> turnover { get; set; }
        public Nullable<decimal> high { get; set; }
        public Nullable<decimal> percent { get; set; }
    }
}
