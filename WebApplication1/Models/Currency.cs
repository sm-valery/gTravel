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
    
    public partial class Currency
    {
        public Currency()
        {
            this.Contracts = new HashSet<Contract>();
        }
    
        public System.Guid CurrencyId { get; set; }
        public string name { get; set; }
        public string code { get; set; }
    
        public virtual ICollection<Contract> Contracts { get; set; }
    }
}
