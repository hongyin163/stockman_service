//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StockMan.Web.RestService
{
    using System;
    using System.Collections.Generic;
    
    public partial class StockCategory
    {
        public string code { get; set; }
        public string name { get; set; }
        public string group_code { get; set; }
    
        public virtual Stock_Category_Group Stock_Category_Group { get; set; }
    }
}