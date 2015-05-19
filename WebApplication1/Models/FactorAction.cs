using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gTravel.Models
{
    public partial class Factor
    {

        public string action { get; set; }

 
        public void before_save()
        {
            if (this.FactorType.Trim().ToLower() != "territory")
                this.TerritoryId = null;

            if (this.RiskId == Guid.Empty)
                this.RiskId = null;

        }
    }
}