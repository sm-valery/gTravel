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
    
    public partial class Risk
    {
        public Risk()
        {
            this.ContractRisks = new HashSet<ContractRisk>();
            this.RiskSerias = new HashSet<RiskSeria>();
        }
    
        public System.Guid RiskId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<ContractRisk> ContractRisks { get; set; }
        public virtual ICollection<RiskSeria> RiskSerias { get; set; }
    }
}
