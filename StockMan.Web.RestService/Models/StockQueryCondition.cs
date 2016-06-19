using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockMan.Web.RestService.Models
{
    public class StockQueryCondition
    {
        public string pe { get; set; }//市盈率
        public string pb { get; set; }//市净率
        public string mv { get; set; }//~161.20总市值
        public string fv { get; set; }//~161.20流通市值
        public string tech { get; set; }//技术，逗号分隔
        public string cate { get; set; }//行业，逗号分隔

    }
}