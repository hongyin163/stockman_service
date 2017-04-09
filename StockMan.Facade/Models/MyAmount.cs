using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Facade.Models
{
    public class MyAmount
    {
        public string id { get; set; }
        public decimal? amount { get; set; }
        public decimal? mv { get; set; }
        public bool init { get; set; }
    }
}
