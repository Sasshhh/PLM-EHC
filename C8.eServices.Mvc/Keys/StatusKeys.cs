using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Keys
{
    public class StatusKeys
    {
        // Rates Rebate.
        public const string RatesRebateApplicationPending = "s_rates_rebate_application_pending";
        public const string RatesRebateApplicationQuery = "s_rates_rebate_application_query";
        public const string RatesRebateApplicationReceived = "s_rates_rebate_application_received";
        public const string RatesRebateInProgress = "s_rates_rebate_in_progress";
        public const string RatesRebateApplicationApproved = "s_rates_rebate_application_approved";
        public const string RatesRebateApplicationSubmitted = "s_rates_rebate_application_submitted";
        public const string RatesRebateApplicationDeclined = "s_rates_rebate_application_declined";

        // Rates Rebate Property.
        public const string RatesRebatePropertyPending = "s_rates_rebate_property_pending";
        public const string RatesRebatePropertyConflict = "s_rates_rebate_property_conflict";
        public const string RatesRebatePropertyVerified = "s_rates_rebate_property_verified";
        public const string RatesRebatePropertyConflictWithValueAssist = "s_conflict";

        // Property.
        public const string PropertyPending = "s_property_pending";
        public const string PropertyConflict = "s_property_conflict";
        public const string PropertyVerified = "s_property_verified";

        // Additional Property Owners.
        public const string RatesRebateAdditionalPropertyOwnersPending = "s_additional_property_owners_pending";
        public const string RatesRebateAdditionalPropertyOwnersConfict = "s_additional_property_owners_conflict";
        public const string RatesRebateAdditionalPropertyOwnersVerified = "s_additional_property_owners_verified";

        // Property Account.
        public const string RatesRebatePropertyAccountPending = "s_rates_rebate_property_account_pending";
        public const string RatesRebatePropertyAccountConflict = "s_rates_rebate_property_account_conflict";
        public const string RatesRebatePropertyAccountVerified = "s_rates_rebate_property_account_verified";

        // Documents.
        public const string DocumentUploaded = "s_document_uploaded";
        public const string DocumentVerified = "s_document_verified";
        public const string DocumentPending = "s_document_pending";

        public const string DocumentApproved = "s_document_approved";
        public const string DocumentRejected = "s_document_rejected";

        // Customer.
        public const string CustomerPendingApproval = "s_customer_pending";
        public const string CustomerActive = "s_customer_active";
        public const string CustomerPendingDocuments = "s_customer_pending_document";
        
        // Linked Account.
        public const string LinkedAccountPending = "s_linked_account_pending";
        public const string LinkedAccountActive = "s_linked_account_active";
        public const string LinkedAccountUnlinked = "s_linked_account_unlinked";

        // Agent.
        public const string AgentPending = "s_agent_pending";
        public const string AgentActive = "s_agent_active";

        // Entity.
        public const string EntityPending = "s_entity_pending";
        public const string EntityActive = "s_entity_active";

        // Account.
        public const string AccountActive = "s_account_active";
        public const string AccountPending = "s_account_pending";

        // System User.
        public const string SystemUserAccountActive = "s_system_user_account_active";

        // Email
        public const string PendingEmailVerification = "s_email_verification_pending";
        public const string EmailVerified = "s_email_verified";

        //Customer Query
        public const string CustomerQueryPending = "s_customer_query_pending";
        public const string CustomerQueryInProgress = "s_customer_query_inprogress";
        public const string CustomerQueryResolved = "s_customer_query_resolved";

        //Incentive Policy Property 
        public const string IncentivePolicyPropertyPending = "s_incentive_policy_property_pending";
        public const string IncentivePolicyPropertyVerified = "s_incentive_policy_property_verified";

        //Incentive Policy Application
        public const string IncentivePolicyApplicationSubmitted = "s_incentive_policy_application_submitted";
        public const string IncentivePolicyApplicationReSubmitted = "s_incentive_policy_application_resubmitted";
        public const string IncentivePolicyApplicationInQuery = "s_incentive_policy_application_inquery";
        public const string IncentivePolicyApplicationPending = "s_incentive_policy_application_pending";
        public const string IncentivePolicyApplicationApproved = "s_incentive_policy_application_approved";
        public const string IncentivePolicyApplicationDeclined = "s_incentive_policy_application_declined";
        public const string IncentivePolicyApplicationProcessing = "s_incentive_policy_application_processing";

        // Debit Order
        public const string DebitOrderSent = "s_debit_order_sent";
        public const string DebitOrderPending = "s_debit_order_pending";
        public const string DebitOrderSuccess = "s_debit_order_success";
        public const string DebitOrderFailed = "s_debit_order_failed";

        // Support Query
        public const string SupportQueryPending = "s_support_query_pending";
        public const string SupportQueryClosed = "s_support_query_closed";
        public const string SupportQueryResolved = "s_support_query_resolved";

        // Linked Mobile
        public const string LinkedMobilePending = "s_linked_mobile_pending";
        public const string LinkedMobileActive = "s_linked_mobile_active";
        public const string LinkedMobileUnlinked = "s_linked_mobile_unlinked";

        // Masterpass
        public const string MasterpassNoticationRecieved = "s_masterpass_notication_recieved";
        public const string MasterpassNoticationProcessed = "s_masterpass_notication_processed";

        //SMS
        public const string SMSSent = "s_sms_sent";
        public const string SMSPending = "s_sms_pending";
        public const string SMSSuccess = "s_sms_success";
        public const string SMSFailed = "s_sms_failed";

        //RCS
        public const string Approved = "s_rcs_approved";
        public const string ReAssignDep = "Rcs_Re-Assign";
        public const string ReAllocate = "Rcs_Re-Allocate";

        //PLM

        //RCS Application Statuses

        public const string RiskAssesmentRejectedUnaffordability = "s_riskassessmentrejected_unaffordability";
        public const string RiskAssesmentRejectedITCCheck = "s_riskassessmentrejected_itccheck";
        
        public const string Saved  = "s_rcs_Saved";
        public const string Submitted = "s_rcs_Submitted";
        public const string InProgress  = "s_rcs_InProgress";
        public const string Query = "s_rcs_Query ";
        public const string AwaitingAssessmentPayment = "s_rcs_AwaitingAssessmentPayment";
        public const string AssessmentPaymentSuccessful = "s_rcs_AssessmentPaymentSuccessful";
        public const string BackOffice = "s_rcs_BackOffice";
        public const string GuaranteeOutstanding  = "s_rcs_GuaranteeOutstanding";
        public const string awaited = "s_rcs_awaited";
        public const string GuaranteePaid  = "s_rcs_GuaranteePaid";
        public const string Cancelled = "s_rcs_Cancelled";
        public const string Archived = "s_rcs_Archived";
        public const string RCCIssued = "s_rcs_RCCIssued";
        public const string Disprove = "s_rcs_Disprove";
        public const string Rejected = "s_rcs_Rejected";
        public const string ReuploadApplicationDocs = "s_reupload_application_docs";

        public const string PendingDocumentsApproval = "s_rcs_PendingDocumentsApproval";

        public const string PendingApplicationFeePaymentValidation = "s_rcs_pending_applicationfee_payment_validation";
        public const string UploadPOPApplicationFee = "s_upload_pop_assessment_fee";

        public const string ViewAssessmentFigure = "s_view_assessment_rcfigure";

        public const string PendingAssessmentFeePaymentValidation = "s_rcs_pending_assessment_payment_validation";
       public const string UploadAssessmentFeePayment = "s_upload_pop_second_assessment_fee";
        public const string AssessmentFeePaymentApproved = "s_rcs_assessment_payment_approved";

        public const string ViewRCCCertificate = "s_view_rcc_certifcate";
        public const string RCSApplicationCompleted = "s_rcs_application_completed";
        

        //refund status keys
        public const string RefundRequestAvailable = "s_refund_request_available";
        public const string RefundAwaitingUpload = "s_upload_refund_municipal_statement";
        public const string PendingRefundDocumentValidation = "s_pending_refund_document_validation";

        public const string RefundDocumentApproved = "s_refund_document_approved";
        public const string RefundDocumentRejected = "s_refund_document_rejected";
        public const string RefundRequestInProgress = "s_refund_request_inprogress";


        public const string RefundPaid = "s_refund_paid";
        public const string RefundReadyForCollection = "s_refund_ready_for_collection";
        public const string RefundRejected = "s_refund_rejected";


        public const string AwaitingRiskAssessment = "s_awaiting_risk_assessment";
        public const string AwaitingUnitOffers = "s_awaiting_unit_offers";

        public const string AwaitingDepartmentResponse = "s_awaiting_department_response";

        public const string AwaitingDepartmentInputs = "s_awaiting_department_inputs";


        public const string AwaitingDepositPaid = "s_debit_order_sent";


        public const string AwaitingLeaseAgreement = "a_awaiting_lease_agreement";
        public const string AwaitingLeaseAgreementReview = "s_awaiting_agreement_review_";
        public const string AwaitingLeaseAgreementApproval = "s_awaiting_agreement_approval_";
        public const string AwaitingDepartmentComments = "s_awaiting_department_comments";

        public const string LeaseAgreementGenerated = "a_lease_generated";
    
    
        public const string NotAllDepartmentsApproved = "s_not_all_departments_approved";
        public const string AllDepartmentsApproved = "s_all_departments_approved";
        public const string AwaitingHoDResponse = "s_awaiting_hod_response";
        public const string ApprovedByHoD = "s_approved_by_hod";
        //public const string AwaitingLeaseAgreement = "s_awaiting_lease_agreement";
       // public const string AwaitingLeaseAgreement = "s_awaiting_lease_agreement";
        public const string DepartmentRejected = "s_department_rejected";
        public const string DepartmentApproved = "s_department_approved";

        public const string RecomendedByDFC = "s_recomended_by_dfc";
        public const string RejectedByDFC = "s_reject_recomended_by_dfc";

        public const string TerminatedLease = "l_terminated_lease";
        public const string ActiveLease = "l_active_lease";

        //Lease Renewal statuses
        public const string AwaitingTenantAcceptance = "s_awaiting_tenant_acceptance";
        public const string LeaseTerminatedDueToComplaints = "s_lease_terminate_complaints";
        public const string AwaitingRenewalTenantLease = "s_awaiting_renewal_tenant_lease";
        public const string TerminateAtEndOfPeriod = "s_terminate_lease_at_end";
        public const string AwaitingRenewalDocuments = "s_awaiting_renewal_docs";


        //Lease Termination
        public const string TerminationLeaseByTenant = "s_termination_review";
        public const string AwaitingterminantionApproval = "s_awaiting_termination_approval";
        public const string AwaitingREACApproval = "s_approved_awaiting_reac_review";
        public const string TerminationRejected = "s_termination_rejected";
        public const string AwaitingTenantAccountBalanceReview = "s_awaiting_tenant_acc_review";
        public const string AwaitingPropertyEviction = "s_awiting_property_eviction";
        public const string AwaitingCommitteEviction = "s_awaiting_committee_eviction";
        public const string TerminationDateIssued = "s_termination_issued";
        public const string AwaitingVacatingConfirm = "s_awaiting_vacating_confirmation";
        public const string ApplicantVacated = "s_applicant_vacated_unit";
        public const string ApplicantNotVacated  = "s_applicant_not_vacated_unit";

        
        public const string AwaitingExitInspection = "s_awaiting_exit_inspec";
        public const string AwaitingMaintananceJobSheet = "s_customer_query_pending";
        public const string AwaitingUnitInspection = "s_additional_property_owners_pending";
        public const string CreditScoreRejected = "s_credit_score_rejected";
        public const string UnitInhabitable = "s_inhabitable_unit";
        public const string UnitUninhabitable = "s_Uninhabitable_unit";

        public const string DeactivateLeaseNewCaptured = "s_deactivated_lease";
        public const string ActiveOccupant = "s_active_occupant";
        public const string DeactiveOccupant = "s_deactive_occupant";
        public const string InAwaitingWaitingListReEntry = "s_awaiting_waiting_list_reentry";
        public const string InAwaitingAvailableUnitsAtReEntry = "s_awaiting_available_unit_at_re_entry";
        public const string ApplicationDiscardedNoUnitAvailable = "s_application_discarded_no_available_unit";
        public const string ApplicationUpForRenewalAtThreeMonths = "s_awaiting_application_lease_renrewal";
        public const string InAwaitingPropertyManagersReview = "s_in_awaiting_property_managers_review";
        public const string InAwaitingRevenueManagersReview = "s_in_awaiting_revenue_managers_review";
        public const string AwitingInspectionSchedule = "s_awaiting_inspection_schedule";
        public const string AwaitingInspectionScheduleSlots = "s_awaiting_inspection_schedule_slots";
        public const string AwaitingManagersSignature = "s_awaiting_managers_signature";
        public const string AwaitingApplicantssignature = "s_awaiting_applicants_signature";

        //update tenant record
        public const string AwaitingTenantDocuments = "s_awaiting_tenant_documents";
        public const string AwaitingTenantUpdateDetails = "s_awaiting_tenant_update_record";

        //Eviction      
        public const string IllegalActivities = "s_application_evicted_for_illegal_activities";
        public const string NonPayment = "s_application_evicted_for_non_payment";
        public const string NonCompliance = "s_application_evicted_for_non_compliance"; 
        public const string Subletting = "s_application_evicted_due_to_subletting";
        public const string EvictionGranted = "s_application_eviction_granted";
        public const string EvictionNotGranted = "s_application_eviction_not_granted";

        //termination
        public const string LeaseNotRenewed = "s_lease_not_renewed";
        public const string TenantDeceased = "s_termination_due_to_tenant_deceased";
        public const string EndOfLeaseTerm = "s_end_of_lease_term";
        public const string TenantNotice = "s_tenant_notice";
        public const string TerminationApproved = "s_termination_approved_ss";
        public const string TerminationReject = "s_termination_rejected_ss";



        public const string TenantEvictionApproved = "s_tenant_eviction_approved";
        public const string ApplicationCompleted = "s_application_completed";
        public const string ExtendedLeasingPeriod = "s_extended_leasing_period";


        public const string AwaitingFinalAgreementOfLease = "s_awaiting_final_agreement_of_lease";
        public const string SignedAgreementOfLeaseUploaded = "s_signed_agreement_of_lease_uploaded";
        public const string AwaitingAgreementUpdate = "s_awaiting_agreement_lease_update";


        public const string AwaitingRecommendationReview = "s_awaiting_recommendation_review";
        public const string AwaitingRecommendationApproval = "s_awaiting_recommendation_approval_";
        public const string AgreementOfLeaseReview = "s_awaiting_agreement_of_lease_review__";
        public const string AgreementRenewal = "s_awaiting_agreement_renewal__";
        public const string UploadDocumentRenewal = "s_renewal_documents_uploaded";

        public const string AwaitingTerminationReview = "s_awaiting_agreement_termination_rev";
        public const string SuccessfullyRenewed = "s_renewal_agree_success";

        public const string RequestRequirementChange = "s_awaiting_requirements_application_chnage";

        public const string AwaitingRefundResponse = "s_awaiting_refund_response";
        public const string VertedApplication = "s_verted_awaiting_unit";

        public const string DocumentUploadNotRiskAssessment = "s_document_upload_not_risk_assessment";

        public const string AwaitingApplicationFeeUpload = "s_awaiting_application_fee_upload";
        public const string AwaitingApplicationFeeValidation = "s_awaiting_application_fee_validation";
        public const string PreUnitInspection = "Pre-Unit Inspection";
        public const string ExitUnitInspection = "Exit-Unit Inspection";

        // Tenant Training & Examination (Added: 2025-01-30)
        public const string AwaitingOnlineTraining = "s_awaiting_online_training";
        public const string TrainingInProgress = "s_training_in_progress";
        public const string TrainingCompleted = "s_training_completed";
        public const string ExaminationPassed = "s_examination_passed";
        public const string ExaminationFailed = "s_examination_failed";

    }
}