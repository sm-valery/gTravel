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
    
    public partial class DocRel
    {
        public System.Guid DocRelId { get; set; }
        public System.Guid DocId { get; set; }
        public System.Guid ParentId { get; set; }
        public string RelType { get; set; }
    }
}
