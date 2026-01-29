using C8.eServices.Mvc.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class SidEftTransaction : BaseModel
    {
        // 1. SID_MERCHANT
        [Column(Order = 20)]
        [Display(Name = "Merchant")]
        public string Merchant { get; set; }
        // 2. SID_CURRENCY
        [Column(Order = 21)]
        [Display(Name = "Currency")]
        public string Currency { get; set; }
        // 3. SID_COUNTRY
        [Column(Order = 22)]
        [Display(Name = "Country")]
        public string Country { get; set; }
        // 4. SID_REFERENCE
        [Column(Order = 23)]
        [Display(Name = "Reference")]
        public string Reference { get; set; }
        // 5. SID_DEFAULT_REFERENCE(if applicable)
        [Column(Order = 24)]
        [Display(Name = "DefaultReference")]
        public string DefaultReference { get; set; }
        // 6. SID_AMOUNT
        [Column(Order = 25)]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
        // 7. SID_CUSTOM_01(if applicable)
        [Column(Order = 26)]
        [Display(Name = "Custom 01")]
        public string Custom01 { get; set; }
        // 8. SID_CUSTOM_02(if applicable)
        [Column(Order = 27)]
        [Display(Name = "Custom 02")]
        public string Custom02 { get; set; }
        // 9. SID_CUSTOM_03(if applicable)
        [Column(Order = 28)]
        [Display(Name = "Custom 03")]
        public string Custom03 { get; set; }
        // 10. SID_CUSTOM_04(if applicable)
        [Column(Order = 29)]
        [Display(Name = "Custom 04")]
        public string Custom04 { get; set; }
        // 11. SID_CUSTOM_05(if applicable)
        [Column(Order = 30)]
        [Display(Name = "Custom 05")]
        public string Custom05 { get; set; }
        // 12. SID_SECRET_KEY
        [Column(Order = 31)]
        [Display(Name = "Secret Key")]
        public string SecretKey { get; set; }
        // SID_CONSISTENT
        [Column(Order = 32)]
        [Display(Name = "Consistent")]
        public string Consistent {
            get
            {
                string hashed = string.Empty;
                string data = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", Merchant, Currency, Country, Reference, 
                    DefaultReference, Amount.ToString(), Custom01, Custom02, Custom03, Custom04, Custom05, SecretKey);

                hashed = HashHelper.ComputeSha512Hash(data);

                return hashed;
            }
        }
    }
}