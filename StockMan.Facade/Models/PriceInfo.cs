using System;

namespace StockMan.Facade.Models
{
    public class PriceInfo
    {
        public string code { get; set; }
        public DateTime date { get; set; }
        public decimal? open { get; set; }
        public decimal price { get; set; }
        public decimal? yestclose { get; set; }
        public decimal? high { get; set; }
        public decimal? low { get; set; }
        public decimal? percent { get; set; }
        public decimal? updown { get; set; }
        public decimal? volume { get; set; }
        public decimal? turnover { get; set; }
    }
}