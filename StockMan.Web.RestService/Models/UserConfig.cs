using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace StockMan.Web.RestService.Models
{
    public class UserConfig
    {
        [Key]
        public string code { get; set; }
        public string config { get; set; }
    }
}