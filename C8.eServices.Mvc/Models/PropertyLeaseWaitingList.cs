using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{

    public class PropertyLeaseWaitingList : BaseModel
    {
        public int PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public virtual PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        public int PreferredComplexId { get; set; }
        // LINK 1: Map the ID to the Complex Object
        [ForeignKey("PreferredComplexId")]
        public virtual PreferredComplexArea PreferredComplexArea { get; set; }

        public int PreferredTypologyId { get; set; }
        // LINK 2: Map the ID to the Typology Object
        [ForeignKey("PreferredTypologyId")]
        public virtual HumanEHCOptions HumanEHCOption { get; set; }

        // CRITICAL: We sort by this.
        public DateTime DateAdded { get; set; }

        // Status: "Waiting", "Offered", "Matched", "Declined_Offer"
        public string QueueStatus { get; set; }

        public int? OfferedUnitId { get; set; }
    }

}