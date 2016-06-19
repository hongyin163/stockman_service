using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockMan.Facade.Models
{
    public class IntentionItem
    {
        public string code { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public int state { get; set; }
    }
}