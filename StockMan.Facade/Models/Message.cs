using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockMan.Facade.Models
{
    public class Message
    {
        public string code { get; set; }
        public bool success { get; set; }
        public string content { get; set; }
    }
}