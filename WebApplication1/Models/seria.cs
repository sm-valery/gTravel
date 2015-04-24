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
    
    public partial class seria
    {
        public seria()
        {
            this.ConditionSerias = new HashSet<ConditionSeria>();
            this.Contracts = new HashSet<Contract>();
            this.RiskSerias = new HashSet<RiskSeria>();
            this.Tarifs = new HashSet<Tarif>();
            this.AgentSerias = new HashSet<AgentSeria>();
        }
    
        public System.Guid SeriaId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Nullable<int> IsMulti { get; set; }
        public string formname { get; set; }
        public Nullable<System.Guid> DefaultCurrencyId { get; set; }
        public Nullable<int> AutoNumber { get; set; }
        public string numberformat { get; set; }
        public Nullable<System.Guid> DefaultTerritoryId { get; set; }
        public string PrintFunction { get; set; }
    
        public virtual ICollection<ConditionSeria> ConditionSerias { get; set; }
        public virtual ICollection<Contract> Contracts { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual ICollection<RiskSeria> RiskSerias { get; set; }
        public virtual ICollection<Tarif> Tarifs { get; set; }
        public virtual ICollection<AgentSeria> AgentSerias { get; set; }
    }
}
