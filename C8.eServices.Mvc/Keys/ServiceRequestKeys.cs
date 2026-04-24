namespace C8.eServices.Mvc.Keys
{
    public static class ServiceRequestCategoryKeys
    {
        public const string MaintenanceRequest = "sr_cat_maintenance";
        public const string SpecialisedCareAndSupport = "sr_cat_specialised_care";
        public const string CompensationAndDisputes = "sr_cat_compensation_disputes";
    }

    public static class ServiceRequestPriorityKeys
    {
        public const string EmergencyCritical = "sr_priority_emergency_critical";
        public const string High = "sr_priority_high";
        public const string Medium = "sr_priority_medium";
        public const string Low = "sr_priority_low";
    }

    public static class ServiceRequestStatusKeys
    {
        public const string Open = "sr_status_open";
        public const string InProgress = "sr_status_in_progress";
        public const string Resolved = "sr_status_resolved";
        public const string Closed = "sr_status_closed";
        public const string Deleted = "sr_status_deleted";
    }
}
