using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Facade.Models
{
    public class UserConfigItem
    {
        //{"receive":true,"email":"","phone":"","tech":"T0005"}
        public string receive { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string tech { get; set; }
    }
}
