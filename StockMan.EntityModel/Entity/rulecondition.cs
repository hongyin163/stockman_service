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
    
    public partial class rulecondition : EntityBase
    {
        public string code { get; set; }
        public string category_code { get; set; }
        public string category_name { get; set; }
        public string object_code { get; set; }
        public string object_name { get; set; }
        public string index_code { get; set; }
        public string index_name { get; set; }
        public string rule_code { get; set; }
        public Nullable<int> sort { get; set; }
    
        public virtual rule rule { get; set; }
    }
}
