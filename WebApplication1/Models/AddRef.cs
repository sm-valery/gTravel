//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace gTravel.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class AddRef
    {
        public AddRef()
        {
            this.Conditions = new HashSet<Condition>();
        }
    
        public System.Guid AddRefsId { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public Nullable<decimal> OrderNum { get; set; }
    
        public virtual ICollection<Condition> Conditions { get; set; }
    }
}
