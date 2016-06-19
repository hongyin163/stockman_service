using System;
using System.Collections.Generic;

namespace StockMan.Index
{
    public class IndexData:List<double>
    {
        public IndexData()
        {
        }
        public IndexData(DateTime date)
        {
            this.date = date;
        }
        public DateTime date { get; set; }
    }
}