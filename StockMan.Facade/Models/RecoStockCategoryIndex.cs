using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockMan.Facade.Models
{
    public class RecoStockCategoryIndex
    {
        public RecoStockCategoryIndex()
        {

        }

        public string code { get; set; }
        public string cate_code { get; set; }
        public string index_code { get; set; }
        public string object_code { get; set; }
        public Nullable<int> day { get; set; }
        public Nullable<int> week { get; set; }
        public Nullable<int> month { get; set; }
        public Nullable<int> last_day { get; set; }
        public Nullable<int> last_week { get; set; }
        public Nullable<int> last_month { get; set; }
        public string cate_name { get; set; }
        public string index_name { get; set; }
        public string object_name { get; set; }
        public Nullable<decimal> pe { get; set; }
        public Nullable<decimal> pb { get; set; }
        public Nullable<decimal> mv { get; set; }
        public Nullable<decimal> fv { get; set; }
        public Nullable<decimal> price { get; set; }

    }
}
