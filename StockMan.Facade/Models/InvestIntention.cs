using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace StockMan.Facade.Models
{
    public class InvestIntention
    {
        [Key]
        public string code { get; set; }
        public string user_id { get; set; }
        public string trade { get; set; }
        public string market { get; set; }
        public string investamount { get; set; }
        public string job { get; set; }
        public string investmethod { get; set; }
        public string learnmethod { get; set; }
        public string apps { get; set; }
        public string infosource { get; set; }
        public string book { get; set; }
    }
}