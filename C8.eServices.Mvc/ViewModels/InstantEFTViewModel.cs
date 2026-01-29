using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    public class InstantEFTViewModel
    {
        public Customer Customer { get; set; }
        public List<BankAccount> CurrentBankAccounts { get; set; }
        public List<BankAccountType> BankAccountTypes { get; set; }
        public List<InstantEFT> InstantEFTs { get; set; }
        public BankAccount BankAccount { get; set; }
        public InstantEFT InstantEFT { get; set; }
    }
}