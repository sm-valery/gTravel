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
    
    public partial class ContractCondition
    {
        public System.Guid ContractCondId { get; set; }
        public Nullable<System.Guid> ConditionId { get; set; }
        public Nullable<System.Guid> Contractid { get; set; }
        public string Val_c { get; set; }
        public Nullable<decimal> Val_n { get; set; }
        public Nullable<bool> Val_l { get; set; }
    
        public virtual Condition Condition { get; set; }
        public virtual Contract Contract { get; set; }
    }
}