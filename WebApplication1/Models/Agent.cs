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
    
    public partial class Agent
    {
        public Agent()
        {
            this.AgentUsers = new HashSet<AgentUser>();
            this.TarifPlanAgents = new HashSet<TarifPlanAgent>();
            this.AgentSerias = new HashSet<AgentSeria>();
            this.Agent1 = new HashSet<Agent>();
        }
    
        public System.Guid AgentId { get; set; }
        public string Name { get; set; }
        public string AgentContractNum { get; set; }
        public Nullable<System.DateTime> AgentContractDate { get; set; }
        public Nullable<System.Guid> ParentId { get; set; }
    
        public virtual ICollection<AgentUser> AgentUsers { get; set; }
        public virtual ICollection<TarifPlanAgent> TarifPlanAgents { get; set; }
        public virtual ICollection<AgentSeria> AgentSerias { get; set; }
        public virtual ICollection<Agent> Agent1 { get; set; }
        public virtual Agent Agent2 { get; set; }
    }
}
