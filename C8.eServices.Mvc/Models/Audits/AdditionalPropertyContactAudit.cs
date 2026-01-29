using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models.Audits
{
    public class AdditionalPropertyContactAudit:BaseModelAudit
    {
        public int IncentivePolicyPropertyId { get; set; }
        [ForeignKey("IncentivePolicyPropertyId")]
        public IncentivePolicyProperty IncentivePolicyProperty { get; set; }

        public int TitleTypeId { get; set; }
        [ForeignKey("TitleTypeId")]
        public TitleType TitleType { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public string Position { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string CellPhoneNumber { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Busines Phone Number")]
        public string BusinessPhoneNumber { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Business EmailAddress")]
        public string EmailAddress { get; set; }
    }
}
