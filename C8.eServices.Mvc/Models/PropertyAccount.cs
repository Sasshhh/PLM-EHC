using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class PropertyAccount : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }
        [ForeignKey("PropertyId")]
        public Property Property { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Account Type")]
        public int AccountTypeId { get; set; }
        [ForeignKey("AccountTypeId")]
        public AccountType AccountType { get; set; }

        [Column(Order = 12)]
        [MaxLength(30)]
        [Display(Name = "Account No")]
        public string AccountNumber { get; set; }

        [Column(Order = 13)]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string BilledFirstName { get; set; }

        [Column(Order = 14)]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string BilledLastName { get; set; }

        [Column(Order = 15)]
        [MaxLength(100)]
        [Display(Name = "ID No")]
        public string BilledIdentificationNumber { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        public string AccountHolder { get { return BilledFirstName + ' ' + BilledLastName; } }
        /// <summary>
        /// L.M.20160802 - Compares property account objects
        ///  </summary>
        /// <param name="propertyaccount">The Property Account to compare</param>
        /// <returns></returns>
        public bool IsEqualsTo(PropertyAccount propertyaccount)
        {
            try
            {
                var propAccount = new PropertyAccount();
                return propAccount.BilledFirstName != propertyaccount.BilledFirstName ||
                       propAccount.BilledLastName != propertyaccount.BilledLastName ||
                       propAccount.BilledIdentificationNumber != propertyaccount.BilledIdentificationNumber ||
                       propAccount.AccountNumber != propertyaccount.AccountNumber;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
