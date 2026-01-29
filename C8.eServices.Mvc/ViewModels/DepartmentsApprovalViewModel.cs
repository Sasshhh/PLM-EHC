using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace C8.eServices.Mvc.ViewModels
{
    public class DepartmentsApprovalViewModel
    {
        public SystemUser CurrentSystemUser { get; set; }
        public HumanSettlementLeaseMaster HumanSettlementLeaseMaster { get; set; }
        public HumanSettlementLeaseDetails HumanSettlementLeaseDetails { get; set; }
        public HumanSettlementApplication HumanSettlementApplication { get; set; }
        public WaitingQueueChangeRequest WaitingQueueChangeRequest { get; set; }
        public PropertyLeaseActionComments PropertyLeaseActionComments { get; set; }
        public List<PropertyLeaseActionComments> PropertyLeaseActionCommentList { get; set; }
        public List<HumanSettlementApplication> HumanSettlementApplicationtList { get; set; }
        public List<InspectionSchedule> InspectionScheduleList { get; set; }
        public PropertyManager PropertyManager { get; set; }
        public DebitOrderRegistration DebitOrderRegistration { get; set; }
        public Customer Customer { get; set; }
        public LeaseTermination LeaseTermination { get; set; }
        public Units Units { get; set; }
        public DocumentsViewModel DocumentsViewModel { get; set; }
        public DocumentsViewModel DocumentsViewModelTemplate { get; set; }
        public MeetingRequest MeetingRequest { get; set; }
        public Entity Entity { get; set; }

        public List<Attachments>  Attachments { get; set; }
        public DepartmentalComments DepartmentalComments { get; set; }
        public CommitteeOutcome CommitteeOutcome { get; set; }
        public RiskAssessmentOutcome RiskAssessmentOutcome { get; set; }


        public List<AssessmentPaymentTransaction> OnlinePaymentHistory { get; set; }

        public PaymentDetails paymentDetails { get; set; }
        public List<CustomerType> CustomerTypes { get; set; }

        [Display(Name = "Customer")]
        public List<Customer> Customers { get; set; }

        public List<Document> CustomerDocuments { get; set; }
        public List<HSRenewalAction> HSRenewalActions { get; set; }


        public List<Note> Notes { get; set; }

        public DocumentsViewModel Document { get; set; }
        public RCSApplicationStatus RCSApplicationStatus { get; set; }
        public List<LeaseDetails> LeaseDetailsList { get; set; }
        public PropertyLeaseApplication PropertyLeaseApplications { get; set; }
        public UnitsEkurhuleniHousingCompany EkurhuleniHousingCompanies { get; set; }
        public ConductUnitInspection ConductUnitInspection { get; set; }
        public LeaseDetails LeaseDetails { get; set; }
        public RefundApplication RefundApplication { get; set; }
        public SystemUser SystemUser { get; set; }
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }
        public List<PropertyLeaseApplication> PropertyLeaseApplicationList { get; set; }
        public HoD HoD { get; set; }
        public ApplicationDepartment ApplicationDepartment { get; set; }
        public List<ApplicationDepartment> ApplicationDepartmentListProperty { get; set; }
        public List<ApplicationDepartment> ApplicationDepartmentList { get; set; }
        public IEnumerable<string> SelectedFeatures { get; set; }
        public IEnumerable<SelectListItem> Features { get; set; }
        public List<PreferredComplexArea> Data { get; set; }
        public List<SelectListItem> Coplexes { get; set; }
        public bool ShowUpdateLink { get; set; }
        public string Comment { get; set; }

        // Used to return back to view that has the partial customer view in it.
        public int ViewId { get; set; }

        public string DocName { get; set; }
        public string DocDesc { get; set; }

        public bool DocumentsVerified { get; set; }

        public string Message { get; set; }
        public bool _Make_tenant { get; set; }

        public string _Data { get; set; }
        public string OTP { get; set; }

        public bool Render { get; set; }
        public bool Search { get; set; }
        public bool Review { get; set; }
        public DocumentsViewModel TerminationLetterDocumentsViewModel { get; set; }
        public DocumentsViewModel DocumentsViewModelExitInterviewForm { get; set; }
        public DocumentsViewModel DocumentsViewModelRefundDeposits { get; set; }
        public List<PropertyLeaseActionComments> WarningLetterReasons { get; set; }
    }
}