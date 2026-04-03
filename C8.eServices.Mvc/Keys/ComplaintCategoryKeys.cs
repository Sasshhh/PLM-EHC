namespace C8.eServices.Mvc.Keys
{
    public class ComplaintCategoryKeys
    {
        public const string Administration = "complaint_cat_administration";
        public const string NuisanceAndBehavioural = "complaint_cat_nuisance_behavioural";
        public const string ParkingAndVehicle = "complaint_cat_parking_vehicle";
        public const string PetAndAnimals = "complaint_cat_pet_animals";
        public const string PropertyUsage = "complaint_cat_property_usage";
        public const string SafetyAndSecurity = "complaint_cat_safety_security";
        public const string Other = "complaint_cat_other";
    }

    public class ComplaintTypeKeys
    {
        // Administration
        public const string SubLetting = "complaint_type_subletting";
        public const string ViolationOfRules = "complaint_type_violation_rules";

        // Nuisance and Behavioural
        public const string NoiseDisturbance = "complaint_type_noise";
        public const string OdorsAndFumes = "complaint_type_odors";
        public const string AggressiveBehaviour = "complaint_type_aggressive";
        public const string Children = "complaint_type_children";

        // Parking and Vehicle
        public const string ObstructiveParking = "complaint_type_obstructive_parking";
        public const string UnauthorisedParking = "complaint_type_unauthorised_parking";
        public const string CarWash = "complaint_type_car_wash";

        // Pet and Animals
        public const string NuisancePets = "complaint_type_nuisance_pets";
        public const string UnapprovedPets = "complaint_type_unapproved_pets";

        // Property Usage
        public const string UntidyAreas = "complaint_type_untidy";
        public const string Neglected = "complaint_type_neglected";
        public const string UnapprovedAlterations = "complaint_type_unapproved_alterations";
        public const string Misuse = "complaint_type_misuse";

        // Safety & Security
        public const string NeglectingSecurityRules = "complaint_type_security_neglect";
        public const string OtherSafety = "complaint_type_other_safety";

        // Other
        public const string OtherType = "complaint_type_other";
    }

    public class ComplaintStatusKeys
    {
        public const string Submitted = "complaint_status_submitted";
        public const string AwaitingAppointment = "complaint_status_awaiting_appointment";
        public const string AwaitingInvestigation = "complaint_status_awaiting_investigation";
        public const string Resolved = "complaint_status_resolved";
        public const string Referred = "complaint_status_referred";
        public const string Unresolved = "complaint_status_unresolved";
    }

    public class ComplaintOutcomeKeys
    {
        public const string Resolved = "Resolved";
        public const string Referral = "Referral";
        public const string Unresolved = "Unresolved";
    }
}
