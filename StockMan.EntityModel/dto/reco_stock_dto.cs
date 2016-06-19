using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.EntityModel.dto
{
    public class reco_cate_index_stock_dto
    {
        public string object_code { get; set; }
        public string object_name { get; set; }
        public string cate_code { get; set; }
        public string cate_name { get; set; }
        public string index_code { get; set; }
        public string index_name { get; set; }
        public decimal pe { get; set; }
        public decimal pb { get; set; }
        public decimal price { get; set; }
        public decimal mv { get; set; }
    }
}


