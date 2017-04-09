using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Facade.Models
{
    public class TradeRecord
    {
        public string code { get; set; }
        public Nullable<int> count { get; set; }
        public int direact { get; set; }
        public string stock_code { get; set; }
        public string stock_name { get; set; }
        public decimal? price { get; set; }
        public DateTime? date { get; set; }
    }
}
