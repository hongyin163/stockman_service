using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMan.Model
{
    public interface IIndex
    {
        decimal[] Calculate(IList<Stock> list, Stock stock);
    }
}
