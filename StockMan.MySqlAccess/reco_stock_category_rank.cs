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
    
    public partial class reco_stock_category_rank : EntityBase
    {
        public string code { get; set; }
        public string cate_code { get; set; }
        public string object_code { get; set; }
        public string cate_name { get; set; }
        public string object_name { get; set; }
        public Nullable<decimal> pe { get; set; }
        public Nullable<decimal> pb { get; set; }
        public Nullable<decimal> mv { get; set; }
        public Nullable<decimal> fv { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> yestclose { get; set; }
        public Nullable<int> rank { get; set; }
    }
}