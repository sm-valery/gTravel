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
    
    public partial class v_contract_agent
    {
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
        public string SeriaCode { get; set; }
        public Nullable<System.Guid> AgentId { get; set; }
        public Nullable<decimal> Percent { get; set; }
        public Nullable<decimal> InsPremRur { get; set; }
        public string stat_code { get; set; }
        public Nullable<decimal> sum_fee { get; set; }
    }
}
