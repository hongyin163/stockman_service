//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StockMan.EntityModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class data_stock_month_latest : EntityBase
    {
        public string code { get; set; }
        public string object_code { get; set; }
        public System.DateTime date { get; set; }
        public Nullable<decimal> open { get; set; }
        public Nullable<decimal> low { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> updown { get; set; }
        public Nullable<decimal> turnoverrate { get; set; }
        public Nullable<decimal> yestclose { get; set; }
        public Nullable<decimal> pe { get; set; }
        public Nullable<decimal> pb { get; set; }
        public Nullable<decimal> fv { get; set; }
        public Nullable<decimal> mv { get; set; }
        public Nullable<decimal> surgedprice { get; set; }
        public Nullable<decimal> declineprice { get; set; }
        public Nullable<decimal> volume { get; set; }
        public Nullable<decimal> turnover { get; set; }
        public Nullable<decimal> high { get; set; }
        public Nullable<decimal> percent { get; set; }
    }
}
