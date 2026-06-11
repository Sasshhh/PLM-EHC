using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Keys
{
    public class RCSActionTypeKeys
    {
        public const string Approved = "Rcs_Approval";
        public const string ReAssignDep = "Rcs_Re-Assign";
        public const string ReAllocate = "Rcs_Re-Allocate";
        public const string Disprove = "Rcs_Disprove";
        public const string Rejected = "Rcs_Rejected";

        public const string RejectedITC = "Rcs_Rejected_itc";

        public const string RejectedDocuments = "Rcs_Rejected_documents";

        public const string RejectedUnaffordability = "Rcs_Rejected_unaffordability";




        public const string Uninhabitable = "Rcs_Uninhabitable";
        public const string Inhabitable = "Rcs_Inhabitable";
        public const string Habitable = "plm_habitable";
        public const string HabitableMinorDefects = "plm_habitable_minor_defects";
        public const string NotHabitable = "plm_not_habitable";


        public const string RefundDocsRejected = "rcs_refund_docs_rejected";
        public const string RefundApplicationRejected = "rcs_refund_application_rejected";


        public const string RefundReadyForCollection = "rcs_refund_ready_for_collection";
        public const string RefundPaid = "rcs_refund_paid";
        public const string PlmNo = "plm_no";
        public const string PlmYes = "plm_yes";
        public const string Approve24Months = "plm_24_months_renew";
        public const string Approve12Months = "plm_12_months_renew";
        public const string Vacated = "plm_vacated";
        public const string NotVacated = "plm_not_vacated";
        public const string NonPayment = "plm_non_payment";
        public const string NonCompliance = "plm_non_compliance";
        public const string Subletting = "plm_subletting";
        public const string IllegalActivities = "plm_illegal_activities";
        public const string LeaseNotRenuewed = "plm_lease_not_renewed";
        public const string EndOfLeasePeriod60M = "plm_end_oflease_term_60_m";
        public const string TenantNotice = "plm_tenant_notice";
        public const string TenantDeceased = "plm_tenant_deceased";
        public const string NotRenew = "plm_not_renew";
        public const string Seconded = "plm_seconded";
        public const string NotSeconded = "plm_not_seconded";
        public const string Other = "plm_other";

        public const string RefundDue = "plm_refund_due";
        public const string NoRefundDue = "plm_no_refund_due";

        public const string ReallocateApplication = "r_re_allocate_application";

        // UC022: Lease Agreement Rejection Sub-Types
        public const string NotSupportedDueToTenant = "plm_not_supported_tenant";
        public const string NotSupportedDueToCSO = "plm_not_supported_cso";

        // UC023-S3 — Legal Referral (Revenue Manager)
        public const string LegalReferral = "plm_legal_referral";
    }
}