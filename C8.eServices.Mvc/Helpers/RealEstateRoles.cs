using System.Linq;
using System.Security.Principal;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Helpers
{
    /// <summary>
    /// Real Estate (DPRE) role names in one place.
    /// Decided with Sash 27 Nov 2026: in Real Estate the Property Officer does all the work the system calls
    /// "Property Manager" work. This applies to Real Estate screens and tasks only: Property Officer is NOT an alias
    /// of the EHC Property Manager (= CEO) role, so Property Officers never see EHC housing screens.
    /// The DPRE Head of Department and DPRE Evaluation Committee roles take over from the borrowed roles
    /// (Back Office System Administrator, Area Manager) as soon as they have members; until then the old roles still work.
    /// </summary>
    public static class RealEstateRoles
    {
        public const string PropertyOfficer = "Property Officer";
        public const string HeadOfDepartment = "DPRE Head of Department";
        public const string EvaluationCommittee = "DPRE Evaluation Committee";
        public const string FinanceAdministrator = "Finance Administrator";
        public const string DepartmentalRepresentative = "Departmental Representative";

        /// <summary>Roles allowed to do Real Estate "Property Manager" work, in routing order.</summary>
        public static readonly string[] PropertyManagerWork = { PropertyOfficer, "Property Manager", "Property Managers" };
        public static readonly string[] HeadOfDepartmentWork = { HeadOfDepartment, "Back Office System Administrator" };
        public static readonly string[] CommitteeWork = { EvaluationCommittee, "Area Manager", "Area Managers" };

        public static bool CanDoPropertyManagerWork(IPrincipal user)
        {
            return user != null && PropertyManagerWork.Any(user.IsInRole);
        }

        /// <summary>
        /// Roles a Real Estate task is routed to, tried in order. The first role with an active clerk gets the task.
        /// </summary>
        public static string[] RoutingRoles(string responsibilityTypeKey)
        {
            if (responsibilityTypeKey == ResponsibilityTypeKeys.RealEstateVerifyPayment) return new[] { FinanceAdministrator };
            if (responsibilityTypeKey == ResponsibilityTypeKeys.RealEstateRiskAssessment) return PropertyManagerWork;
            if (responsibilityTypeKey == ResponsibilityTypeKeys.RealEstateCommitteeReview) return CommitteeWork;
            if (responsibilityTypeKey == ResponsibilityTypeKeys.RealEstateHODAuthorisation) return HeadOfDepartmentWork;
            return new[] { "Back Office System Administrator" };
        }

        /// <summary>The role new committee members get: the DPRE role once it exists in the database, otherwise Area Manager.</summary>
        public static string CommitteeRoleForNewMembers(eServicesDbContext db)
        {
            return db.Roles.Any(r => r.Name == EvaluationCommittee) ? EvaluationCommittee : "Area Manager";
        }
    }
}
