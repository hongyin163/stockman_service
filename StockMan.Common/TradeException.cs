using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Common
{
    public class TradeException : Exception
    {
        public TradeException(string msg)
            : base(msg)
        {

        }
    }
}
