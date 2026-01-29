using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Keys
{
    public class EmailContentKeys
    {

        public const string ApplicationFeeRejection = "plm_app_fee_reject";

        public const string ApplicationFeeApproved = "plm_app_fee_approve";


        public const string RiskAssessmentRejectUnaffordability = "plm_on_risk_reject_unaffordability";
        public const string RiskAssessmentRejectDocuments = "plm_on_risk_reject_documents";
        public const string RiskAssessmentRejectITC = "plm_on_risk_reject_itc";
        public const string ApplicationFeeUploaded = "plm_Application_payment_uploaded";

        public const string ApplicationCapturedSuccessfully = "rcs_email_captured_successfully";
        public const string RefundProofPaymentRejected = "rcs_email_refund_pop_rejected";
        public const string ApplicationDocsApproved = "rcs_email_documents_approved";
        public const string ApplicationDocsRejected = "rcs_email_documents_rejected";
        public const string ApplicationProofOfPaymentApproved = "rcs_email_Application_payment_approved";
        public const string ApplicationProofOfPaymentRejected = "rcs_email_Application_payment_rejected";
        public const string AssessmentProofOfPaymentApproved = "rcs_email_Assessment_payment_approved";
        public const string AssessmentProofOfPaymentRejected = "rcs_email_Assessment_payment_rejected";
        public const string AssessmentFigureUploaded = "rcs_email_Assessment_Uploaded";
        public const string RCCIssued = "rcs_email_rcc_issued";
        public const string RefundSuccessBankAccount = "rcs_email_refund_bank";
        public const string RefundSuccessCollection = "rcs_email_refund_collection";
        public const string RefundApplicationRejected = "rcs_email_refund_rejected";
        public const string RefundDocsApproved = "rcs_email_refund_docs_approved";
        public const string RefundDocsRejected = "rcs_email_refund_docs_rejected";

        public const string ConveyancerSendsNewMessage = "rcs_conveyancer_sends_new_message";
        public const string BOSendsNewMessage = "rcs_bo_sends_new_message";

        public const string NotifyBOOfNewBOMessage = "rcs_bo_sends_new_message_to_bo";
        public const string AssessmentFiguresFullyPaid = "rcs_email_assessment_uploaded_figures_fully_paid";

        public const string RefundConveyancerSendsNewMessage = "rcs_refund_conveyancer_sends_new_message";
        public const string RefundBOSendsNewMessage = "rcs_refund_bo_sends_new_message";

        public const string RefundNotifyBOOfNewBOMessage = "rcs_refund_bo_sends_new_message_to_bo";

        public const string BONewCaseLoaded = "rcs_bo_new_case_loaded";
        public const string BONewCaseReAllocated = "rcs_bo_new_case_redistributed";

        //PLM e-mails
        public const string ApplicationCaptureEmail = "plm_applicant_capture_application";
        public const string ApplicationAssignedToRiskAssessment = "plm_assigned_to_risk_assessment";
        public const string ApplicationRiskAssessmentApproved = "plm_awaiting_lease_agreement_on_risk";
        public const string InActionGenerateLeaseAgreement = "plm_bo_generate_lease_agreement";
        public const string GenerateDebitOrderAuthorityForm = "plm_applicant_generate_debit_order";
        public const string BackOfficeApproveDebitOrder = "plm_bo_approve_debit_order";
        public const string DebitOrderRegister = "plm_debit_order_register";
        public const string ReDebitOrderRegister = "plm_debit_order_register_re";
        public const string DebitOrderRejectByBO = "plm_debit_order_reject_bo";
        public const string Rejectproperty = "plm_reject_property";
        public const string Accetproperty = "plm_accept_property";
        public const string RemoveWaitingListOnSecondReject = "plm_waiting_list_remove_on_reject";
        public const string BackOfficeRenewalNotification = "plm_backoffice_renewal_";
        public const string ApplicantRenewalNotification = "plm_applicant_renewal";
        public const string UnitInspectionScheduleMail = "plm_unit_ispection_mail";
        public const string UnitInspectionScheduleByApplicant = "plm_unit_ispection_at_applicant_confirm";
        public const string InviteTenantForTraining = "plm_invite_tenant_for_training";
        public const string AcceptanceLetter = "plm_acceptance_letter_submit";
        public const string EvictionGranted = "plm_eviction_approved";
        public const string ServeNotice = "plm_serve_notice";
        public const string ApplicationRiskAssessmentRejected = "plm_on_risk_reject";
        public const string ReallocatedForAssessment = "reallocated_for_assessment";
        public const string TerminationApprovedForTenant = "plm_on_tenant_notice_approved";
        public const string TerminationRejectedForTenant = "plm_on_tenant_notice_reject";
        public const string TerminationRejectedFromLO = "plm_on_termination_rejected";
        public const string TerminationAccepted = "plm_on_termination_accepted";
        public const string EvictionCapture = "plm_on_first_eviction";
        public const string ApplicationUpForRenewal = "plm_up_for_renewal";
        public const string RenewalApprovedRecommendation = "plm_approve_renewal_recommendations";
        public const string RenewalRejectRecommendation = "plm_rejected_renewal_recommendations";
        public const string AtUnitMatchApplication = "plm_at_matched_to_unit";
        public const string DepositPaymentApprove = "plm_deposite_approve";
        public const string DepositPaymentReject = "plm_deposite_reject";

        public const string WaitingListReEntery = "plm_waiting_list_reentry";
        public const string RequestTimeSlots = "plm_request_slots";
        public const string plm_re_list_to_queue = "plm_re_list_to_queue";
        public const string plm_remove_from_queue = "plm_remove_from_queue";
        public const string plm_awaiting_debit_order = "plm_awaiting_debit_order";


        public const string AwaitingApplicantsSignature = "plm_awaiting_applicants_signature";
        public const string TransferSuccessful = "plm_successful_agreement_transfer";
        public const string AgreementRenewalApproved = "plm_agreement_renewal_approved";
        public const string AgreementRenewalRejected = "plm_agreement_renewal_rejected";
        public const string AgreementRenewalAccepted = "plm_agreement_renewal_accepted_by_leasee";
        public const string CustomerAgreementRenewalRejected = "plm_agreement_renewal_rejected_by_leasee";
        public const string DocumentUploadRenewal = "plm_agreement_renewal_upload_";

        public const string AnnualRentalEscalation = "plm_rental_escalation_msg";
        public const string WarningLetter = "plm_warning_letter_msg";

        public const string UpdateRefundResponseRefundDue = "plm_update_refund_response_refund_due";
        public const string UpdateRefundResponseNoRefundDue = "plm_update_refund_response_norefund_due";

        public const string RenewalRejectByCustomer = "plm_renewal_reject_by_customer";
    }
}