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
    
    public partial class sys_goodidea : EntityBase
    {
        public string code { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Nullable<int> up { get; set; }
        public Nullable<int> down { get; set; }
        public Nullable<int> type { get; set; }
        public Nullable<System.DateTime> createtime { get; set; }
        public Nullable<int> state { get; set; }
        public string createuser { get; set; }
    }
}
