using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.EntityModel.dto
{
    public class UserConfigItem
    {
        //{"receive":true,"email":"","phone":"","tech":"T0005"}
        public bool receive { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string tech { get; set; }
    }
}
