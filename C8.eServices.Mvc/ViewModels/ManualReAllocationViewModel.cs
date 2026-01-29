using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace C8.eServices.Mvc.ViewModels
{
    public class ManualReAllocationViewModel
    {


        public string CurrentAssignedUserName { get; set; }
        public string CurrentFullName { get; set; }

        public string ResponsibilityType { get; set; }
        public string TitleName { get; set; }
        public string BodyName { get; set; }
        public string Roles { get; set; }

        public List<UserWorkAllocation> UserWorkAllocation_L { get; set; }

        public int RoundRobinQueueId { get; set; }
        public int RCSApplicationId { get; set; }
        public int PropertyLeaseApplicationId { get; set; }
        public int HumanSettlementApplicationId { get; set; }
        public int DepartmentId { get; set; }
        public int RefundId { get; set; }

        public string ViewName { get; set; }
        //public string ResponsibilityType { get; set; }

        public Customer Customer { get; set; }

        public Entity Entity { get; set; }

        public UserWorkAllocation UserWorkAllocation { get; set; }
        public List<AssessmentPaymentTransaction> OnlinePaymentHistory { get; set; }

        public PreferredComplexArea PreferredComplexArea { get; set; }
        public PaymentDetails paymentDetails { get; set; }
        public List<CustomerType> CustomerTypes { get; set; }

        [Display(Name = "Customer")]
        public List<Customer> Customers { get; set; }

        public List<Document> CustomerDocuments { get; set; }
        public List<Note> Notes { get; set; }

        public DocumentsViewModel Document { get; set; }
        public RCSApplicationStatus RCSApplicationStatus { get; set; }
        public RefundApplication RefundApplication { get; set; }
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        public bool ShowUpdateLink { get; set; }
        public string Comment { get; set; }

        // Used to return back to view that has the partial customer view in it.
        public int ViewId { get; set; }

        public bool DocumentsVerified { get; set; }

        [Required]
        [Display(Name = "CCC")]
        public int NewBackOfficeUser { get; set; }
        public int LettingOfficerId { get; set; }
        public int ClerkRegID { get; set; }

        public bool AdUser { get; set; }

        public List<ApplicationUserRole> UserRole { get; set; }

        public IEnumerable<string> SelectedFeatures { get; set; }

        public IEnumerable<SelectListItem> CCCTypes { get; set; }
    }
}