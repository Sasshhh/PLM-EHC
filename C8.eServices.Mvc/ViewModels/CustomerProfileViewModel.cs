using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    public class CustomerProfileViewModel
    {

        [Display(Name = "Are you a Managing Agent?")]
        public bool IsManagingAgent { get; set; }

        [Display(Name = "Is same as physical address?")]
        public bool IsSameAddress { get; set; }

        public Agent Agent { get; set; }

        public Customer Customer { get; set; }

        public Entity Entity { get; set; }

        public List<CustomerType> CustomerTypes { get; set; }

        [Display(Name = "Customer")]
        public List<Customer> Customers { get; set; }

        public List<Document> CustomerDocuments { get; set; }
        public List<Note> Notes { get; set; }

        public DocumentsViewModel Document { get; set; }

        public bool ShowUpdateLink { get; set; }

        // Used to return back to view that has the partial customer view in it.
        public int ViewId { get; set; }

        public bool DocumentsVerified { get; set; }
    }
}