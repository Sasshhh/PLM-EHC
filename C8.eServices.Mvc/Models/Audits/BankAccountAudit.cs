using C8.eServices.Mvc.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class BankAccountAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        [Display(Name = "Account Holder")]
        [MaxLength(128)]
        [Required]
        public string AccountHolder { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Bank Name")]
        [Required]
        public int BankId { get; set; }
        [ForeignKey("BankId")]
        public Bank Bank { get; set; }

        private string _bankAccountNumber = string.Empty;
        [Column(Order = 12)]
        [Display(Name = "Account Number")]
        [MaxLength(64)]
        [Required]
        public string BankAccountNumber { get { return _bankAccountNumber; } set { _bankAccountNumber = value; } }

        // Hides the bank account number and only shows the last 4 digits.
        public string HiddenBankAccountNumber
        {
            get
            {
                var hban = "";
                var decrypted = string.Empty;

                if (!_bankAccountNumber.All(char.IsDigit))
                    decrypted = new TripleDesCrypto().Decrypt(_bankAccountNumber);
                else
                    decrypted = _bankAccountNumber;

                for (var i = 0; i < decrypted.Length - 4; i++)
                    hban += "*";

                hban += decrypted.Substring(decrypted.Length - 4);

                return hban;
            }
        }

        public string ShowBankAccountNumber
        {
            get
            {
                var decrypted = string.Empty;

                if (!_bankAccountNumber.All(char.IsDigit))
                    decrypted = new TripleDesCrypto().Decrypt(_bankAccountNumber);
                else
                    decrypted = _bankAccountNumber;

                return decrypted;
            }
        }

        [Column(Order = 13)]
        [Display(Name = "Branch Code")]
        [MaxLength(8)]
        [Required]
        public string BranchCode { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Bank Account Type")]
        [Required]
        public int BankAccountTypeId { get; set; }
        [ForeignKey("BankAccountTypeId")]
        public BankAccountType BankAccountType { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Is Account Holder")]
        [Required]
        public bool IsAccountHolder { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Account Status")]
       
        public string AccountStatus { get; set; }

        [Column(Order = 18)]
        [Display(Name = "Account Type")]
        public bool IsBusiness { get; set; }
    }
}