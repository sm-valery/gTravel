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
    
    public partial class Territory
    {
        public Territory()
        {
            this.Contract_territory = new HashSet<Contract_territory>();
            this.Tarifs = new HashSet<Tarif>();
        }
    
        public System.Guid TerritoryId { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<Contract_territory> Contract_territory { get; set; }
        public virtual ICollection<Tarif> Tarifs { get; set; }
    }
}
