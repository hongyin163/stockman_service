using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Facade.Models
{
    public class MyStrategy
    {
        public string user_id { get; set; }
        public IList<String> strategy { get; set; }
    }
}
