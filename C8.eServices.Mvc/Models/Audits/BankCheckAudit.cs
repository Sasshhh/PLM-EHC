using C8.eServices.Mvc.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class BankCheckAudit : BaseModelAudit
    {


        [Column(Order = 10)]
        [Display(Name = "Transaction Completed")]
        [Required]
        public bool TransactionCompleted { get; set; }

        ////    "StatusCode": "D",
        [Column(Order = 11)]
        [Display(Name = "Transaction Status")]
        [MaxLength(11)]
        [Required]
        public string StatusCode { get; set; }
        //[Column(Order = 12)]
        //[Display(Name = "Bank Name")]
        //public int BankId { get; set; }
        //[ForeignKey("BankId")]
        //public Bank Bank { get; set; }



        ////    "AccountNumber": "*******9397",
        private string _bankAccountNumber = string.Empty;
        [Column(Order = 13)]
        [Display(Name = "Account Number")]
        [MaxLength(64)]
        [Required]
        public string AVSBankAccountNumber { get { return _bankAccountNumber; } set { _bankAccountNumber = value; } }

        // Hides the bank account number and only shows the last 4 digits.
        public string AVSHiddenBankAccountNumber
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

        public string ShowAVSBankAccountNumber
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
        ////    "BranchCode": "198765",

        [Column(Order = 14)]
        [Display(Name = "Branch Code")]
        [MaxLength(8)]
        [Required]
        public string AVSBranchCode { get; set; }

        ////    "BankAccountType": "Current/Cheque Account",

        [Column(Order = 15)]
        [Display(Name = "Account Type")]
        [Required]
        public string BankAccountTypeId { get; set; }



        ////    "ClientID": "950223*******",
        [Column(Order = 16)]
        [Display(Name = "ClientID")]
        public string ClientID { get; set; }

        ////    "Initials": "A",
        [Column(Order = 17)]
        [Display(Name = "Initials")]
        public string Initials { get; set; }

        ////    "LastName": "KADER",
        [Column(Order = 18)]
        [Display(Name = "LastName")]
        public string LastName { get; set; }

        ////    "AccountFound": "U",
        [Column(Order = 19)]
        [Display(Name = "AccountFound")]
        public string AccountFound { get; set; }

        ////    "AccountOpen": "U",
        [Column(Order = 20)]
        [Display(Name = "AccountOpen")]
        public string AccountOpen { get; set; }

        ////    "AccountLength": "U",
        [Column(Order = 21)]
        [Display(Name = "AccountLength")]
        public string AccountLength { get; set; }

        ////    "AccountTypeCorrect": "U",
        [Column(Order = 22)]
        [Display(Name = "AccountTypeCorrect")]
        public string AccountTypeCorrect { get; set; }

        ////    "IDNumberMatch": "U",
        [Column(Order = 23)]
        [Display(Name = "Customer")]
        public string IDNumberMatch { get; set; }

        ////    "InitialsMatch": "U",
        [Column(Order = 24)]
        [Display(Name = "InitialsMatch")]
        public string InitialsMatch { get; set; }

        ////    "NameMatch": "U",
        [Column(Order = 25)]
        [Display(Name = "NameMatch")]
        public string AVSCliNameMatchentID { get; set; }

        ////    "AccountDebitsAll": "U",
        [Column(Order = 26)]
        [Display(Name = "AccountDebitsAll")]
        public string AccountDebitsAll { get; set; }

        ////    "AccountCreditsAll": "U",
        [Column(Order = 27)]
        [Display(Name = "AccountCreditsAll")]
        public string AccountCreditsAll { get; set; }

        ////    "AccountPhoneMatch": "U",
        [Column(Order = 28)]
        [Display(Name = "Account Phone Match")]
        public string AccountPhoneMatch { get; set; }

        ////    "AllRecordsPassed": "U",
        [Column(Order = 29)]
        [Display(Name = "AllRecordsPassed")]
        public string AllRecordsPassed { get; set; }

        ////    "TransactionsID": 84439,
        [Column(Order = 30)]
        [Display(Name = "TransactionsID")]
        public string TransactionsID { get; set; }

        ////    "JobID": 84439,
        [Column(Order = 31)]
        [Display(Name = "JobID")]
        public string JobID { get; set; }

        ////    "FirstResponse": "00001"
        [Column(Order = 32)]
        [Display(Name = "First Response")]
        public string FirstResponse { get; set; }


    }
}