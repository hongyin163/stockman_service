using System;
using System.Collections.Generic;

namespace StockMan.EntityModel
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
        public IndexData(string date)
        {
            this.date = DateTime.ParseExact(date, "yyyyMMdd", null);
        }
        public DateTime date { get; set; }
    }

 

}