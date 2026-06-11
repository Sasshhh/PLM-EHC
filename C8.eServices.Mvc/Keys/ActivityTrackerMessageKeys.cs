using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Keys
{
    public class ActivityTrackerMessageKeys
    {
        public const string ConevayncerCapturesNewApplication = "at_capture_new_rcs_app";
        public const string EmailNewApplication = "at_email_new_rcs_app";
        public const string ConevayncerApplicationFeeSiyakhokha = "a_all_applications";
        public const string ConevayncerApplicationFeeExternalPayment = "a_all_applications";
        public const string ConevayncerUploadsApplicationDocument = "at_upload_app_docs";
        public const string ConevayncerUploadsApplicationFeeProofOfPayment = "at_upload_pop_application_fee";
        public const string ApplicationSentForAcknowledgementAppFee = "at_acknowledgement_app_fee";

        public const string AcknowledgementAppFeeApproved = "at_acknowledgement_app_fee_approved";
        public const string AcknowledgementAppFeeRejected = "at_acknowledgement_app_fee_rejected";
        public const string AcknowledgementAppFeeEmail = "at_acknowledgement_app_fee_email";

        public const string SentToAcknowledgementDocuments = "at_acknowledgement_documents";
        public const string AcknowledgementDocumentsApproved = "at_acknowledgement_documents_approved";
        public const string AcknowledgementDocumentsRejected = "at_acknowledgement_documents_rejected";
        public const string AcknowledgementDocumentsEmail = "at_acknowledgement_documents_email";

        public const string ApplicationFeeRejected = "at_applicationfee_reject";
        public const string ApplicationFeeApproved = "at_applicationfee_approve";

        public const string RedistributionSuccessful = "at_successful_redistribution";
        public const string RedistributionUnsuccessful = "at_unsuccessful_redistribution";

        public const string SentToSubmitFigures = "at_submit_figures";
        public const string AssessmentFiguresSentToConveyancer = "at_submit_figures_sent";
        public const string SubmitFiguresEmail = "at_submit_figures_email";


        public const string ExternalPaymentAssessmentFiguresPaymentOption = "at_assessment_figures_external_payment";
        public const string SiyakhokhaPayPortalAssessmentFiguresPaymentOption = "at_assessment_figures_siyakhokha_payment_portal";
        public const string AssessmentFiguresPaymentPOP = "at_assessment_figures_pop";
        public const string AssessmentFiguresPaymentPOPSentToClerk = "at_submit_figures_pop_validation";


        public const string AssessmentFiguresPOPApproved = "at_assessment_figures_pop_approved";
        public const string AssessmentFiguresPOPRejected = "at_assessment_figures_pop_rejected";

        public const string SentToRCC = "at_sent_to_rcc";
        public const string RCCIssued = "at_rcc_sent";
        public const string RCCEmail = "at_rcc_email";

        public const string Sundries = "at_sent_to_sundries";
        public const string Billing = "at_sent_to_billing";
        public const string Rates = "at_sent_to_rates";
        public const string CreditControl = "at_sent_to_creditcontrol";

        public const string SundriesResolved = "at_resolved_sundries";
        public const string BillingResolved = "at_resolved_billing";
        public const string RatesResolved = "at_resolved_rates";
        public const string CreditControlResolved = "at_resolved_creditcontrol";

        public const string AllDepartmentIssuesResolved = "at_department_issues_resolved";

        //refund keys
        public const string RefundConevayncerCapturesNewApplication = "at_capture_new_refund_app";
        public const string RefundEmailNewApplication = "at_email_new_refund_app";
        //public const string ConevayncerApplicationFeeSiyakhokha = "a_all_applications";
        //public const string ConevayncerApplicationFeeExternalPayment = "a_all_applications";
        public const string RefundConevayncerUploadsApplicationDocument = "at_upload_refund_docs";
        //public const string ConevayncerUploadsApplicationFeeProofOfPayment = "at_upload_pop_application_fee";
        public const string RefundApplicationSentForAcknowledgementApplication = "at_refund_acknowledgement";


        public const string RefundApplicationAcknowledgementDcoumentsApproved = "at_refund_acknowledgement_documents_approved";

        public const string RefundApplicationAcknowledgementDcoumentsRejected = "at_refund_acknowledgement_documents_rejected";

        public const string RefundApplicationRejected = "at_refund_reject_application";

        public const string RefundEmailAcknowledgement = "at_refund_email_acknowledgement";
        public const string RefundIssueCollection = "at_refund_collection";

        public const string RefundEmailIssueRefund = "at_refund_email_issuerefund";
        public const string RefundPaidToConveyancerBankAcc = "at_refund_paid";
        public const string RefundReadyForCollection = "at_refund_ready_for_collection";

        //public const string RefundApplicationSentForAcknowledgementApplication = "at_acknowledgement_app_fee";
        public const string RCCViewedByConveyancer = "at_rcc_viewed";
        public const string RCSApplicationCompleted = "at_rcs_application_completed";

        public const string RatesClearanceSystem = "a_rates_clearance_system";

        public const string NewMessageSentOnChat = "at_new_chat_message";
        public const string NotifyConevayncerOfNewMessage = "at_new_message_notification_conveyancer";
        public const string NotifyBOOfNewMessage = "at_new_message_notification_bo";
        public const string AssessmentFiguresCredit = "at_submit_figures_credit_validation";

        //property application keys
        public const string ApplicationCapture = "at_capture_of_application";
        public const string ApplicationDepartmentInputs = "at_departmental_inputs";
        public const string ApplicationDepartmentalApprove = "at_department_approve";
        public const string ApplicationDepartmentalReject = "at_department_reject";
        public const string AllDepartmentsApproved = "at_all_department_approve";
        public const string NotAllDepartmentsApproved = "at_not_all_department_approve";
        public const string DFCApprovedApplication = "at_dfc_approve_application";
        public const string DFCRejectedApplication = "at_dfc_reject_application";
        public const string REACRecomendTender = "at_reac_recomend_tender_process";
        public const string REACRecomendHOD = "at_reac_recoment_hod_approval";
        public const string HoDApprovedAplication = "at_hod_approval";
        public const string HoDRejectedApplication = "at_hod_reject";

        public const string SubmitForRiskAssessmrnt = "at_submit_for_risk_assessment";
        public const string AcceptCreditScore = "at_risk_threshold_above_min";
        public const string RejectCreditScore = "at_risk_threshold_below_min";
        public const string ApplicationMatchedUnit = "at_unit_match_found";
        public const string ApplicantRejectUnit = "at_applicant_reject_unit";
        public const string ApplicantAcceptUnit = "at_applicant_accept_unit";

        public const string ApplicantUploadsDocuments = "at_applicant_upload";
        public const string ReallocatedForRiskAssessment = "at_reallocated_risk_assessment";
        public const string PropertyLeaseAgreementGenerated = "at_lease_agreement_generate";
        public const string ApplicantDebitOrderGenerated = "at_debit_order_generate";
        public const string ApplicantDebitOrderApproved = "at_debit_order_approve";
        public const string ApplicantDebitOrderRejected = "at_debit_order_rejected";
        public const string TenantRecordUpdated = "at_record_update_tenant";
        public const string UnitInspectionApproved = "at_unit_inspec_approve";
        public const string UnitInspectionRejected = "at_unit_inspec_reject";
        public const string LeaseAgreementApproved = "at_leae_agreement_approve";
        public const string TenantCapturedLease = "at_lease_capture";
        public const string UnitInspectionFailedInhabitable = "at_inspection_habitable_unit";
        public const string UnitInspectionFailedUninhabitable = "at_inspection_uninhabitable_unit";
        public const string ApplicationLeasePassRenewalValidation = "at_lease_validation_approve";
        public const string ApplicationLeaseFailRenewalValidation = "at_lease_validation_reject";
        public const string TenantRiskRejectRenewal = "at_tenant_risk_reject";
        public const string TenantRiskApproveRenewal = "at_tenant_risk_approve";

        public const string TerminateThisLeaseApplication = "at_terminate_lease";
        public const string EvictionCommitteeActionsTermination = "at_committee_approve_terminate";
        public const string ConductExitInspectionApprove = "at_exit_inspection_approve";
        public const string ConductExitInspectionReject = "at_exit_inspection_reject";
        public const string TenantAccountValidation = "at_account_validation";
        public const string ProopertyEvictionAprove = "at_property_eviction_approve";
        public const string AcceptMatchedUnit = "at_matched_unit_accept";
        public const string RejectMatchedUnit = "at_matched_unit_reject";
        public const string TenantNotice = "at_tenant_notice_letting_o";
        public const string LeaseNotRenewed = "at_lease_not_renewed_letting_o";
        public const string EndOfLeaseTerm = "at_lease_end_of_lease_term_letting_o";
        public const string TerminationApproved = "at_termination_approved_ss";
        public const string TerminationRejected= "at_termination_rejected_ss";
        public const string TenantDeceased = "at_termination_tenant_deceased";
        public const string EvictionCapture = "at_capture_eviction";
        public const string ApproveDeposit = "at_deposite_approve";
        public const string RejectDeposit = "at_deposite_reject";
        public const string WaitingListReEntry = "at_re_entry_waiting_list";
        public const string WaitingListExit = "at_exit_waiting_list";
        public const string at_change_application_status = "at_change_application_status";
        public const string at_change_lease_application_status = "at_change_lease_application_status";


        public const string AgreementOfLeaseRejected = "at_reject_agreement_of_lease";
        public const string ApproveAgreementOfLease = "at_approve_agreement_of_lease";
        public const string SignAgreementByApplicant = "at_sign_agreement_of_lease";
        public const string TransferredSuccessfully = "at_transferred_successfully";
        public const string AgreementRenewalApproved = "at_agreement_renewal_approved";
        public const string AgreementRenewalRecjected = "at_agreement_renewal_reject";
        public const string CustomerAgreementRenewalRejected = "at_agreement_renewal_rejected_by_leasee";
        public const string CustomerAgreementRenewalAccepted = "at_agreement_renewal_accepted_by_leasee";
        public const string UploadDocumentRenewal = "at_upload_renewal_documets";
        public const string AnnualRentalEscalation = "at_rental_escalation_msg";
        public const string RequirementCnhange = "at_requirement_change";
        public const string FirstApplicant = "first_applicant";
        public const string SpouseApplicant = "second_applicant";
        public const string UploadWarningLetter = "at_upload_warning_letter";
        //public const string Name = "Key";
        //public const string Name = "Key";
        //public const string Name = "Key";
        //public const string Name = "Key";
        //public const string Name = "Key";
        //public const string Name = "Key";
        //public const string Name = "Key";
        //public const string Name = "Key";
        //public const string Name = "Key";

        public const string AwaitingRefundResponse = "at_awaiting_refund_response";
        public const string UpdateRefundResponse = "at_update_refund_response";
        public const string AdminLeaseCapture = "at_admin_lease_capture";

        public const string UnitOfferToApplicant = "at_unit_offer_tenant";
        public const string IsMigrated = "at_is_migrated";
        public const string FullyMigrated = "at_fully_migrated";

        // Tenant Training Keys
        public const string TenantInvitedToTraining = "at_tenant_invited_to_training";

        // UC022: Lease Agreement Rejection
        public const string LeaseAgreementRejectedToTenant = "at_lease_agreement_rejected_tenant";
        public const string LeaseAgreementRejectedToCSO = "at_lease_agreement_rejected_cso";

        // BR19 — Lease Signing Expiry (30-day deadline)
        public const string LeaseSigningExpired = "at_lease_signing_expired";

        // BR09 — Unit Offer Expiry (30-day deadline)
        public const string UnitOfferExpired = "at_unit_offer_expired";

        // UC023 — Manage Lease Termination
        public const string TerminationCSOReviewSupported = "at_termination_cso_supported";
        public const string TerminationCSOReviewNotSupported = "at_termination_cso_not_supported";
        public const string TerminationRMApproved = "at_termination_rm_approved";
        public const string TerminationRMRejected = "at_termination_rm_rejected";
        public const string TerminationRMReferredLegal = "at_termination_rm_referred_legal";
        public const string TerminationCSOInitiated = "at_termination_cso_initiated";

        // UC024 — Eviction CEO Authorization
        public const string EvictionCEOApproved = "at_eviction_ceo_approved";
        public const string EvictionCEORejected = "at_eviction_ceo_rejected";
        public const string EvictionNoticeGenerated = "at_eviction_notice_generated";

        // UC025 — Serve Eviction Notice & Proof of Service
        public const string EvictionNoticeServed = "at_eviction_notice_served";
        public const string ProofOfServiceCaptured = "at_proof_of_service_captured";

        // UC026 — Manage Disputes
        public const string DisputeRegistered = "at_dispute_registered";
        public const string DisputeReviewed = "at_dispute_reviewed";
        public const string DisputeReferredLegal = "at_dispute_referred_legal";
        public const string DisputeResolved = "at_dispute_resolved";
        public const string DisputeNotResolved = "at_dispute_not_resolved";
        public const string DisputeClosedCEO = "at_dispute_closed_ceo";
        public const string DisputeRejectedCEO = "at_dispute_rejected_ceo";

    }
}