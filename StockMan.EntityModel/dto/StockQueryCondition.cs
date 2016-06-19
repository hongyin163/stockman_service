using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.EntityModel.dto
{
    public class StockQueryCondition
    {
        public string user_id { get; set; }
        public string pe { get; set; }//市盈率
        public string pb { get; set; }//市净率
        public string mv { get; set; }//~161.20总市值
        public string fv { get; set; }//~161.20流通市值
        public string price { get; set; }
        public string tech { get; set; }//技术，逗号分隔
        public string cate { get; set; }//行业，逗号分隔
        public string cycle { get; set; }
    }
}
