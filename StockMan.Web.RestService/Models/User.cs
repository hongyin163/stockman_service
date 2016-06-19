using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockMan.Web.RestService.Models
{
    public class User
    {
        public string id { get; set; }

        public string name { get; set; }

        public string email { get; set; }

        public string phone { get; set; }

        public decimal exp { get; set; }

        public decimal points { get; set; }

        public string password { get; set; }
    }
}