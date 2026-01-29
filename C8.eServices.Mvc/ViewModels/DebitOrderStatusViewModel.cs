using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace C8.eServices.Mvc.ViewModels
{
    public class DebitOrderStatusViewModel
    {
        public Customer Customer { get; set; }
        public List<BankAccount> CurrentBankAccounts { get; set; }
        public List<BankAccountType> BankAccountTypes { get; set; }
        public List<DebitOrder> DebitOrders { get; set; }
        public List<DebitOrderStatus> DebitOrderStatuses { get; set; }
        public BankAccount BankAccount { get; set; }
        public DebitOrder DebitOrder { get; set; }
        public DebitOrderStatus DebitOrderStatus { get; set; }
    }
}