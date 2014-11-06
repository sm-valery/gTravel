using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gTravel.Models
{
    public class cl_import_contract
    {
        public int contract_number { get; set; }
        public string contract_number_str { get; set; }
        public DateTime? date_out { get; set; }
        public DateTime? date_begin { get; set; }
        public DateTime? date_end { get; set; }
        public string entry_out { get; set; }
        public string entry_in { get; set; }
        public DateTime? date_flyout { get; set; }
        public string SubjName { get; set; }
        public DateTime? dateofbirth { get; set; }
        public string pasport { get; set; }
        public string gender { get; set; }
        public string placeofbirth { get; set; }
        public DateTime? passportvaliddate { get; set; }

    }
}