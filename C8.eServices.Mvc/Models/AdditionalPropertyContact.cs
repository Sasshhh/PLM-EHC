using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models
{
   public class AdditionalPropertyContact: BaseModel
    {
        [Column(Order=2)]
        public int IncentivePolicyPropertyId { get; set; }
        [ForeignKey("IncentivePolicyPropertyId")]
        public IncentivePolicyProperty IncentivePolicyProperty { get; set; }

        [Column(Order=3)]
        public int TitleTypeId { get; set; }
        [ForeignKey("TitleTypeId")]
        public TitleType TitleType { get; set; }

        [Column(Order=4)]
        [Display(Name="Full Name")]
        public string FullName { get; set; }

        [Column(Order=5)]
        [Display(Name= "Position In Business")]
        public string Position { get; set; }

        [Column(Order=6)]
        [Display(Name="Cell Number")]
        [DataType(DataType.PhoneNumber)]
        public string CellPhoneNumber { get; set; }

        [Column(Order=7)]
        [DataType(DataType.PhoneNumber)]
        [Display(Name="Busines Phone Number")]
        public string BusinessPhoneNumber { get; set; }

        [Column(Order=8)]
        [DataType(DataType.EmailAddress)]
        [Display(Name="Business Email Address")]
        public string EmailAddress { get; set; }

        [Display(Name = "Name")]
        public string ContactFullName
        {
            get { return string.Format("{0} {1}", TitleType.Name, FullName); }

        }

  
        [Display(Name = "Contacts")]
        public string ContactNumbers
        {
            get
            {
                string contact = string.Empty;
                if (!string.IsNullOrEmpty(EmailAddress)) contact = string.Format("(Email) {0} | ", EmailAddress);
                if (!string.IsNullOrEmpty(CellPhoneNumber)) contact += string.Format("(C) {0} | ", CellPhoneNumber);
                if (!string.IsNullOrEmpty(BusinessPhoneNumber)) contact += string.Format("(W) {0} | ", BusinessPhoneNumber);
                return contact;
            }
        }
    }
}
