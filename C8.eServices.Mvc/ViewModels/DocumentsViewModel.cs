using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    public class DocumentsViewModel
    {
        public int ReferenceId { get; set; }
        public int ReferenceTypeId { get; set; }
        public List<AssessmentPaymentTransaction> OnlinePaymentHistory { get; set; }
        public int ApplicationId { get; set; }
        public int ? CustomerId { get; set; }
        public int StatusId { get; set; }

        public int RcsApplicationId { get; set; }
        public int RefundApplicationId { get; set; }
        public int PropertyLeaseApplicationId { get; set; }
        public int HumanSettlementApplicationId { get; set; }

        public int AllocatedUnitMaintenanceId { get; set; }
        public string ApplicationReferenceNumber { get; set; }
        public string ReturnUrl { get; set; }

        public bool IsUploadView { get; set; }

        public ReferenceType ReferenceType { get; set; }
        public Application Application { get; set; }
        public RCSApplicationStatus RCSApplicationStatus { get; set; }
        public List<DocumentCheckList> DocumentCheckLists { get; set; }
        public List<DocumentCheckList> RiskAssessmentDocumentCheckLists { get; set; }
        public List<DocumentCheckList> UnitInspectionDocumentCheckLists { get; set; }
        public List<DocumentCheckList> MaintananceJobSheetDocumentCheckLists { get; set; }
        public List<DocumentCheckList> TenantLeaseDocumentCheckLists { get; set; }
        public List<DocumentCheckList> ChecklistUploadTemplates { get; set; }

        public List<Document> Documents { get; set; }

        public DebtorsNote DebtorsNote { get; set; }

        public decimal Amount { get; set; }

        public DepartmentalComments Departmental { get; set; }

        public int LeaseDetailsId { get; set; }
        public int docRandId { get; set; }
    }
}