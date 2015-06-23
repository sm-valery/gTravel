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
            this.ContractAgents = new HashSet<ContractAgent>();
            this.ContractFactors = new HashSet<ContractFactor>();
            this.ContractRisks = new HashSet<ContractRisk>();
            this.Subjects = new HashSet<Subject>();
            this.ContractConditions = new HashSet<ContractCondition>();
        }
    
        public System.Guid ContractId { get; set; }
        public System.Guid currencyid { get; set; }
        public Nullable<decimal> contractnumber { get; set; }
        public System.Guid seriaid { get; set; }
        public Nullable<System.DateTime> date_begin { get; set; }
        public Nullable<System.DateTime> date_end { get; set; }
        public Nullable<int> date_diff { get; set; }
        public Nullable<System.Guid> Holder_SubjectId { get; set; }
        public Nullable<System.Guid> ContractStatusId { get; set; }
        public string period_multi_type { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> date_out { get; set; }
        public string contractnumberformat { get; set; }
        public Nullable<int> tripduration { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual ICollection<Contract_territory> Contract_territory { get; set; }
        public virtual ContractStatu ContractStatu { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual seria seria { get; set; }
        public virtual ICollection<ContractAgent> ContractAgents { get; set; }
        public virtual ICollection<ContractFactor> ContractFactors { get; set; }
        public virtual ICollection<ContractRisk> ContractRisks { get; set; }
        public virtual ICollection<Subject> Subjects { get; set; }
        public virtual ICollection<ContractCondition> ContractConditions { get; set; }
    }
}
