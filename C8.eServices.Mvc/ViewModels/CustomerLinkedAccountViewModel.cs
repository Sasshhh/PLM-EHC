using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using C8.eServices.Mvc.Models;
using System;

namespace C8.eServices.Mvc.ViewModels
{
    public class CustomerLinkedAccountViewModel
    {
        public int Id { get; set; }
        public Customer Customer { get; set; }
        public LinkedAccount LinkedAccount { get; set; }
        public Agent Agent { get; set; }
        public Entity Entity { get; set; }
        //[Required]
        //[RegularExpression("^[0-9]{11}$", ErrorMessage = "Invalid account number")]
        [Display(Name = "Account Number ")]
        public string AccountNumber { get; set; }

        [Display(Name = "Account Number ")]
        public string AccountNumberDescription { get; set; }

        public string CustomerEmail { get; set; }

        public LinkedEmail LinkedEmail { get; set; }

        public LinkedMobile LinkedMobile { get; set; }

        //public List<LinkedEmail> LinkedEmails { get; set; }

        public int ? AgentId { get; set; }
        public int CustomerId { get; set; }
        
        public int? AgentCustomerId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string LinkedAccountType { get; set; }

        public string RecipientType { get; set; }

        public bool isRms { get; set; }

        public Status Status { get; set; }
        //public string Status { get; set; }

        public DateTime CreatedDateTime { get; set; } 

        //[Display(Name = "Email Bill")]
        //public bool EmailBill { get; set; }
        public int DocumentId { get; set; }
        public bool UnlinkAccount { get; set; }
        public bool IsEntityAgent { get; set; }
        public DocumentsViewModel Document { get; set; }
        public List<AssociatedAccounts> AssociatedAccs { get; set; }
        public List<Note> Notes { get; set; } 
    }

    public class AssociatedAccounts
    {
        public string AccountNo { get; set; }

        public string ReferenceId { get; set; }
    }
}