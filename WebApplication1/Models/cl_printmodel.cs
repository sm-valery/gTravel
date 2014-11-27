using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gTravel.Models
{
    public class cl_printmodel
    {
        public Contract contract { get; set; }
        public string entryport { get; set; }
        public string exitport { get; set; }
        public DateTime? entrydate { get; set; }
        public DateTime? exitdate { get; set; }
        public string route { get; set; }
    }
}