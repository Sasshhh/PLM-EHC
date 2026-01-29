using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Keys
{
    public class QueryTypeKeys
    {
        public const string Login = "qt_login";
        public const string PasswordRecovery = "qt_password_recovery";
        public const string InformationMigration = "qt_information_migration";
        public const string AccountsMigration = "qt_accounts_migration";
        public const string EmailsMigration = "qt_emails_migration";
        public const string MultipleLogins = "qt_multiple_logins";
        public const string ConvertEntity = "qt_convert_entity";
        public const string ConvertManagingAgent = "qt_convert_managing agent";
        public const string ConvertIndividual = "qt_convert_individual";
        public const string IncorrectMigration = "qt_incorrect_migration";
        public const string IncorrectAccounts = "qt_incorrect_accounts";
        public const string IncorrectEmails = "qt_incorrect_emails";
        public const string Other = "qt_other";
        public const string Suggestion = "qt_suggestion";
        public const string Compliment = "qt_compliment";
        public const string Complaint = "qt_complaint";
        public const string ProfileApproval = "qt_customer_approval";

        public const string CustomerQuery = "st_customer_query_status";
        public const string DeactiveAccount = "qt_deactivate_account";
        
    }
}