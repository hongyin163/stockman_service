
using StockMan.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Jobs.Biz.Model
{
    public class IndexTask
    {
        /// <summary>
        /// 对象类型，category,object,stock
        /// </summary>
        public ObjectType type { get; set; }
        /// <summary>
        /// 对象标识
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 周期类型，day,week,month
        /// </summary>
        public TechCycle cycle { get; set; }
      
    }
}
