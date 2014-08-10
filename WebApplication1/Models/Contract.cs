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
    
    public partial class Contract
    {
        public Contract()
        {
            this.Contract_territory = new HashSet<Contract_territory>();
            this.ContractConditions = new HashSet<ContractCondition>();
        }
    
        public System.Guid ContractId { get; set; }
        public System.Guid currencyid { get; set; }
        public decimal contractnumber { get; set; }
        public System.Guid seriaid { get; set; }
        public Nullable<System.DateTime> date_begin { get; set; }
        public Nullable<System.DateTime> date_end { get; set; }
        public Nullable<int> date_diff { get; set; }
    
        public virtual Currency Currency { get; set; }
        public virtual seria seria { get; set; }
        public virtual ICollection<Contract_territory> Contract_territory { get; set; }
        public virtual ICollection<ContractCondition> ContractConditions { get; set; }
    }
}
