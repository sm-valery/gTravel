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
    
    public partial class ContractRisk
    {
        public System.Guid ContractRiskId { get; set; }
        public System.Guid ContractId { get; set; }
        public Nullable<System.Guid> RiskId { get; set; }
        public Nullable<decimal> InsSum { get; set; }
        public Nullable<decimal> InsPrem { get; set; }
        public Nullable<decimal> BaseTarif { get; set; }
        public Nullable<decimal> InsFee { get; set; }
    
        public virtual Risk Risk { get; set; }
        public virtual Contract Contract { get; set; }
    }
}
