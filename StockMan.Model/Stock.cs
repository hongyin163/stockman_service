using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Model
{
    public class Stock : EntityBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string symbol
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public string code
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }
       
    }
}
