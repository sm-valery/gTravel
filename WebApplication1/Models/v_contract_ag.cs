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
    
    public partial class v_contract_ag
    {
        public string Name1 { get; set; }
        public string Address { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string Pasport { get; set; }
        public System.Guid ContractId { get; set; }
        public System.Guid currencyid { get; set; }
        public Nullable<decimal> contractnumber { get; set; }
        public System.Guid seriaid { get; set; }
        public Nullable<System.DateTime> date_begin { get; set; }
        public Nullable<System.DateTime> date_end { get; set; }
        public Nullable<int> date_diff { get; set; }
        public Nullable<System.Guid> Holder_SubjectId { get; set; }
        public Nullable<System.Guid> ContractStatusId { get; set; }
        public Nullable<decimal> period_multi { get; set; }
        public string period_multi_type { get; set; }
        public string UserId { get; set; }
        public Nullable<System.DateTime> date_out { get; set; }
        public string contractnumberformat { get; set; }
    }
}
