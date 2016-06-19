using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.EntityModel.dto
{
    public class MyCycleObject
    {
        public string code { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> yestclose { get; set; }
    }
}
