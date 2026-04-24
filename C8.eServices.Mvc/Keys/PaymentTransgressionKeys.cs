namespace C8.eServices.Mvc.Keys
{
    public static class PaymentTransgressionCategoryKeys
    {
        public const string Financial = "payment_transgression_cat_financial";
        public const string Other = "payment_transgression_cat_other";
    }

    public static class PaymentTransgressionTypeKeys
    {
        public const string PaymentMisconduct = "payment_transgression_type_payment_misconduct";
        public const string LevyArrears = "payment_transgression_type_levy_arrears";
        public const string ViolationOfLeaseAgreement = "payment_transgression_type_violation_lease";
        public const string Other = "payment_transgression_type_other";
    }

    public static class PaymentTransgressionSeverityKeys
    {
        public const string Level1Minor = "payment_transgression_severity_level1_minor";
        public const string Level2Moderate = "payment_transgression_severity_level2_moderate";
        public const string Level3Major = "payment_transgression_severity_level3_major";
    }

    public static class PaymentTransgressionStatusKeys
    {
        public const string Submitted = "payment_transgression_status_submitted";
        public const string LetterGenerated = "payment_transgression_status_letter_generated";
        public const string LetterSent = "payment_transgression_status_letter_sent";
        public const string Closed = "payment_transgression_status_closed";
    }

    public static class PaymentTransgressionLetterTypes
    {
        public const string PaymentTransgressionNotice = "Payment Transgression Notice";
        public const string WrittenWarningLetter = "Written Warning Letter";
        public const string FinalWrittenWarningLetter = "Final Written Warning Letter";
    }
}
