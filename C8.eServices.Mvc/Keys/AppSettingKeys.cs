using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Keys
{
    public class AppSettingKeys
    {
        public const string SharePointUsername = "as_share_point_username";
        public const string SharePointPassword = "as_share_point_password";
        public const string SharePointUrl = "as_share_point_url";
        public const string SharePointSite = "as_share_point_site";
        public const string SharePointDomain = "as_share_point_domain";
        public const string SharePointLibrary = "as_share_point_library";
        public const string ReportServerUrl = "eservices_report_server_url";
        public const string ReportPath = "eservices_report_path";
        public const string ReportServerUsername = "eservices_report_server_username";
        public const string ReportServerPassword = "eservices_report_server_password";
        public const string ReportServerDomain = "eservices_report_server_domain";
        public const string GeneratedBillFileLocation = "eservices_generated_bill_file_location";
        public const string GeneratedRmsBillsLocation = "eservices_generated_rms_bills_root_location";
        public const string SortedRmsBillsLocation = "eservices_sorted_rms_bills_location";
        public const string CustomerHelperEmails = "as_customer_help_emails";
        public const string RevlineHelperEmail = "as_revline_help_emails";
        public const string PropertyMarketValue = "as_va_property_market_value";
        public const string PropertyRatingCategoryCode = "as_va_property_rating_category_code"; 
        public const string EservicesApplication = "eservices_applicationID";
        public const string EservicesEmailAccount = "eservices_email_account";
        public const string EservicesDefaultEmailTemplate = "eservices_default_email_template";     
        public const string RatesRebateEmailAccount = "rates_rebate_default_email_template";
        public const string LinkedAccountsAssociatedEmailTemplate = "linked_accounts_associated_email_template";
        public const string LinkedAccountsUnassociatedEmailTemplate = "linked_accounts_unassociated_email_template";
        public const string CopyAccountWatermark = "as_copy_account_watermark";
        public const string LinkedAccountsActiveMessage = "linked_accounts_active_message";
        public const string LinkedAccountsPendingAccountApprovalMessage = "linked_accounts_pending_account_approval_message";
        public const string LinkedAccountsPendingAccountApprovalAndEmailActivationMessage = "linked_accounts_pending_account_and_email_activation_message";
        public const string LinkedEmailsPendingEmailActivationMessage = "linked_emails_pending_email_activation_message";
        public const string LinkedEmailsPendingEmailActivationAndLinkedAccountApprovalMessage = "linked_emails_pending_email_activation_and_linked_account_approval_message";
        public const string IpAddressRange = "eservices_customer_centres_ip_range";
        public const string UpdateNotification = "eservices_update_notification";
        public const string IncentivePolicyDocumentLocation = "incentive_policy_document_location";
        public const string IncentivePolicyDefaultEmailTemplate = "incentive_policy_default_email_template";
        public const string UploadNetworkDirectory = "eservices_upload_network_directory";
        public const string UploadHtmlDirectory = "upload_html_directory";
        public const string IncentivePolicyTermsAndConditions = "incentive_policy_ts_cs";

        // API Services
        public const string MeterConsumptionUrl = "meter_consumption_url";
        public const string AcbBusinessAccountNumber = "acb_business_account_number";
        public const string SupportEmailList = "as_support_email_list";
        public const string AVSCustomerID = "avs_customer_id";
        public const string AVSCustomerKey = "avs_customer_api_key";
        public const string AVSEndUrl = "avs_first_check_url";
        public const string AVSCheckUrl = "avs_check_14_day_url";

        // Venus/ Solar Intergration
        public const string CustomerAccountsApi = "api_customer_accounts";
        public const string AccountBalanceApi = "api_account_balance";
        public const string AccountBalanceApiUsername = "api_account_balance_username";
        public const string AccountBalanceApiPassword = "api_account_balance_password";
        public const string AttorneyDetails = "api_attorney_details";


        // Masterpass Integration
        public const string MasterpassUrl = "as_masterpass_url";
        public const string MasterpassUsername = "as_masterpass_username";
        public const string MasterpassPassword = "as_masterpass_password";

        // SMPP Settings
        public const string SmppHost = "as_smpp_host";
        public const string SmppUsername = "as_smpp_username";
        public const string SmppPassword = "as_smpp_password";
        public const string SmppPort = "as_smpp_port";

        // Masterpass Daily File
        public const string MasterpassDailyUrl = "as_masterpass_daily_url";
        public const string MasterpassDailyPath = "as_masterpass_daily_path_url";
        public const string MasterpassDailyStarttime = "as_masterpass_daily_starttime";
        public const string MasterpassDailyEndtime = "as_masterpass_daily_endtime";
        public const string MasterpassDailyFilename = "as_masterpass_daily_filename";
        public const string MasterpassDailyExecutiontime = "as_masterpass_daily_executiontime";
        public const string MasterpassDailyTest = "as_masterpass_test";

        // Masterpass Daily File
        public const string MasterpassHourlyPath = "as_masterpass_hourly_path";
        public const string MasterpassHourlyFilename = "as_masterpass_hourly_filename";

        // Payment History
        public const string PaymenthistoryPollingDirectory = "as_paymenthistory_polling_directory";

        //Recurring Debit Orders
        public const string RecurringDebitOrdersStrikeDayEdit = "as_recurring_debit_orders_strikeday_edit";
        public const string RecurringDebitOrdersStrikeDayRemove = "as_recurring_debit_orders_strikeday_remove";

        //Sequence Counters
        public const string DebitOrderBatchSequence = "debit_order_daily_sequence_counter";
        public const string EFTBatchSequence = "eft_daily_sequence_counter";
        public const string DebitOrderBatchSequenceLimit = "debit_order_daily_sequence_limiter";
        public const string EFTBatchSequenceLimit = "eft_daily_sequence_limiter";
        public const string RCSSequence = "rcs_daily_sequence_counter";
        public const string PLMSequence = "plm_daily_sequence_counter";
        public const string PLMSequenceLimit = "plm_daily_sequence_limiter";
        public const string RCSSequenceLimit = "rcs_daily_sequence_limiter";
        public const string RCSRefundSequence = "rcs_refund_daily_sequence_counter";
        public const string RCSRefundSequenceLimit = "rcs_refund_daily_sequence_limiter";
        public const string EHCReferenceDailyCounter = "ehc_daily_sequence_counter";
        public const string EHCReferenceDailyLimit = "ehc_daily_sequence_limiter";
        public const string REDReferenceDailyCounter = "red_daily_sequence_counter";
        public const string REDReferenceDailyLimit = "red_daily_sequence_limiter";
        public const string hsd_daily_sequence_counter = "hsd_daily_sequence_counter";
        public const string hsd_daily_sequence_limiter = "hsd_daily_sequence_limiter";


        //App Config toggle Settings
        public const string AccountLock = "account_lock_out";
        public const string DOMultiplePaymentsDaily = "debit_order_multiple_payments_daily";
        public const string DOMultiplePaymentsMonthly= "debit_order_multiple_payments_monthly";
        public const string DOMultiplePaymentsAmount= "debit_order_multiple_payments_amount";

        //Ad Account for Reports
        public const string AdUserName = "ad_user_name";
        public const string AdPassword = "ad_password";
        public const string adDomain = "ad_domain";
        public const string adGenBillLink = "ad_bill_Link";
        public const string adGenDateRangelink = "ad_date_range_report";
        public const string adGenDetailedlink = "ad_detailed_report";


        //Payment Gateway Keys
        public const string RCSApplicationFeeAmt = "as_rcs_app_fee";

        public const string RCSPaymentGateway = "as_payment_gateway_environment";

        //ad login keys
        //ActiveDirectory
        public const string ActiveDirectoryActive = "active_directory_active";
        public const string activeDirectoryDomain = "active_directory_domain";
        public const string ADUserCreds = "ad_admin_user_creds";


        //Lawtrust Keys
        public const string ws02key = "lt_ws_key";

        public const string ws02Secret = "lt_ws_secret";

        public const string Ws02gentokenendpoint = "lt_ws_gentoken_endpoint";

        public const string Ws02genlinkendpoint = "lt_ws_genlink_endpoint";


        //Solar WS02 Keys
        public const string Solarws02key = "sol_ws_key";

        public const string Solarws02Secret = "sol_ws_secret";

        public const string SolarWs02gentokenendpoint = "sol_ws_gentoken_endpoint";



        //Lims Prod Keys

        public const string limsWso2key = "lims_Wso_Key";

        public const string limsWso2Secret = "lims_Wso_Secret";

        public const string limsWs02gentokenendpoint = "lims_ws_gentoken_endpoint";

        public const string limsWs02endpoint = "lims_ws_endpoint";


        //Attorneydetails api
        public const string AttorneydetailsWso2key = "Attorneydetails_Wso_Key";

        public const string AttorneydetailsWso2Secret = "Attorneydetails_Wso_Secret";

        public const string AttorneydetailsWs02gentokenendpoint = "Attorneydetails_ws_gentoken_endpoint";

        public const string AttorneydetailsWs02endpoint = "Attorneydetails_ws_endpoint";

        //Municipal Service api
        public const string MunservWso2key = "Munserv_Wso_Key";

        public const string MunservWso2Secret = "Munserv_Wso_Secret";

        public const string MunservWs02gentokenendpoint = "Munserv_ws_gentoken_endpoint";

        public const string MunservWs02endpoint = "Munserv_ws_endpoint";



        //Generate RCC api
        public const string GenerateRccWso2key = "GenerateRcc_Wso_Key";

        public const string GenerateRccWso2Secret = "GenerateRcc_Wso_Secret";

        public const string GenerateRccWs02gentokenendpoint = "GenerateRcc_ws_gentoken_endpoint";

        public const string GenerateRccWs02endpoint = "GenerateRcc_ws_endpoint";


        //Payment Details api
        public const string PaymentDetailsWso2key = "PaymentDetails_Wso_Key";

        public const string PaymentDetailsWso2Secret = "PaymentDetails_Wso_Secret";

        public const string PaymentDetailsWs02gentokenendpoint = "PaymentDetails_ws_gentoken_endpoint";

        public const string PaymentDetailsWs02endpoint = "PaymentDetails_ws_endpoint";


        //Property owner Details api
        public const string PropertyOwnerDetailsWso2key = "PropertyOwnerDetails_Wso_Key";

        public const string PropertyOwnerDetailsWso2Secret = "PropertyOwnerDetails_Wso_Secret";

        public const string PropertyOwnerDetailsWs02gentokenendpoint = "PropertyOwnerDetails_ws_gentoken_endpoint";

        public const string PropertyOwnerDetailsWs02endpoint = "PropertyOwnerDetails_ws_endpoint";


        //Standnumber api
        public const string StandnumberWso2key = "Standnumber_Wso_Key";

        public const string StandnumberWso2Secret = "Standnumber_Wso_Secret";

        public const string StandnumberWs02gentokenendpoint = "Standnumber_ws_gentoken_endpoint";

        public const string StandnumberWs02endpoint = "Standnumber_ws_endpoint";

        //Acccount Balance api
        public const string AcccountBalanceWso2key = "AcccountBalance_Wso_Key";

        public const string AcccountBalanceWso2Secret = "AcccountBalance_Wso_Secret";

        public const string AcccountBalanceWs02gentokenendpoint = "AcccountBalance_ws_gentoken_endpoint";

        public const string AcccountBalanceWs02endpoint = "AcccountBalance_ws_endpoint";

        //Figures api
        public const string FiguresWso2key = "Figures_Wso_Key";

        public const string FiguresWso2Secret = "Figures_Wso_Secret";

        public const string FiguresWs02gentokenendpoint = "Figures_ws_gentoken_endpoint";

        public const string FiguresWs02endpoint = "Figures_ws_endpoint";

        //lawtrust test account
        public const string Lawtrsuttestaccountswitch = "lt_test_switch";

        //PLM


        public const string DownloadDomain = "key_DomainForDownload";
        public const string PublicDownloadDomain = "key_PublicDownloadDomain";
        public const string URLAutDomain = "key_URLAutDomain";
        public const string Owner_Is_Objector = "Objector_is_Owner";
        public const string systemDomain = "as_system_domain";


        public const string Lease1TemplateKey = "lt_Lease1TemplateKey";
        public const string DebitOrderAuthority = "lt_debit_order_authority";

        public const string TenantSequenceCounter = "tenant_sequence";
        public const string PropertyLeaseManagementDefaultEmailTempate = "email_default_plm_email";
        public const string EHCFirstStayInMonths = "ehc_fist_stay_on_premises";


        //User Id
        public const string PropertyManager = "r_property_manager";
        public const string RevenueManager = "r_revenue_manager";
        public const string RevenueOfficer = "r_revenue_officer";
        public const string CommunityDevelopmentOfficer = "r_community_development_officer";
        public const string ClientServicesOfficer = "u_client_services_officer";
        public const string HousingSupervisor = "u_housing_super_visor";
        public const string LettingOfficer = "u_letting_officer";
        public const string MaintenanceManager = "u_maintenance_manager";
        public const string PropertyFacilitiesManager = "u_property_facilities_manager";


        public const string WaitingListSorting = "waiting_list_sorting_";

        public const string DO_TEMP_PDF = "pdf_location_path_do";
        public const string LA_TEMP_PDF = "pdf_location_path_la";
        public const string waiting_list_notification_in_munites = "waiting_list_notification_in_munites";
        public const string renewal_notification_in_minutes = "renewal_notification_in_minutes";


        public const string USER_CHECK_RELATIONSHIP = "USER_CHECK_RELATIONSHIP";
        public const string AOL = "HSAOL";


        public const string SamsClientKey = "c_samsClientKey";
        public const string SamsClientSecret = "c_samsClientSecret";
        public const string SamsTokenEndpoint = "c_samsTokenEndpoint";

        public const string RefundProcessEmail = "refund_email";

    }
}