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
    
    public partial class object_user_map : EntityBase
    {
        public string object_code { get; set; }
        public string object_type { get; set; }
        public string user_id { get; set; }
    
        public virtual users users { get; set; }
    }
}
