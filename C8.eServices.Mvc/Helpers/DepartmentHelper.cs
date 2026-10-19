using System;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Helpers
{
    /// <summary>
    /// Resolves the ApplicationEntities row that represents a department.
    ///
    /// Department ids are NOT stable across environments. In this database EHC is
    /// ap_ekurhuleni_housing_company = 4, Real Estate = 3 and Human Settlement = 2;
    /// there is no row with Id 1 at all. Code that hard-coded "DepartmentId = 1"
    /// therefore failed with
    ///     FK_dbo.&lt;table&gt;_dbo.ApplicationEntities_DepartmentId
    /// on every insert. Always resolve by [Key], never by a literal id.
    /// </summary>
    public static class DepartmentHelper
    {
        /// <summary>Resolve a department id from its ApplicationEntities [Key].</summary>
        public static int? ResolveByKey(eServicesDbContext context, string applicationEntityKey)
        {
            if (context == null || string.IsNullOrWhiteSpace(applicationEntityKey))
                return null;
            try
            {
                return context.ApplicationEntities
                    .Where(e => e.Key == applicationEntityKey)
                    .Select(e => (int?)e.Id)
                    .FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>The EHC (Ekurhuleni Housing Company) department id for this database.</summary>
        public static int? EhcDepartmentId(eServicesDbContext context)
        {
            return ResolveByKey(context, ApplicationEntityKeys.EkurhuleniHousingCompany);
        }

        /// <summary>
        /// The ApplicationEntities [Key] of the signed-in user's department, e.g.
        /// "ap_ekurhuleni_housing_company". Returns an empty string when unknown.
        ///
        /// Views call this on every menu block, so it must be cheap: the value is
        /// taken from Session (set at login) and otherwise resolved once per request
        /// and cached in HttpContext.Items. SecurityHelper.UserDepartment() opens a
        /// fresh DbContext on every call, which is far too expensive here.
        /// </summary>
        public static string CurrentDepartmentKey()
        {
            var http = HttpContext.Current;
            if (http == null) return string.Empty;

            const string cacheKey = "__plm_department_key";
            if (http.Items.Contains(cacheKey))
                return (string)http.Items[cacheKey] ?? string.Empty;

            string key = string.Empty;
            try
            {
                // set by AccountController on successful login
                if (http.Session != null && http.Session["DepartmentEntity"] != null)
                    key = Convert.ToString(http.Session["DepartmentEntity"]);

                if (string.IsNullOrEmpty(key) &&
                    http.User != null && http.User.Identity != null && http.User.Identity.IsAuthenticated)
                {
                    using (var db = new eServicesDbContext())
                    {
                        var name = http.User.Identity.Name;
                        key = (from u in db.SystemUsers
                               join e in db.ApplicationEntities on u.DepartmentId equals e.Id
                               where u.UserName == name
                               select e.Key).FirstOrDefault() ?? string.Empty;
                    }
                }
            }
            catch (Exception)
            {
                key = string.Empty;
            }

            http.Items[cacheKey] = key;
            return key;
        }

        /// <summary>
        /// The PLM landing page: RCSHome, on RCS_Layout, with the lease and Ekurhuleni
        /// map banner. It is also the site's default route. Every PLM department lands
        /// here - Real Estate reuses the EHC page until it has one of its own - so no
        /// PLM user is ever sent to Home/Index, the municipal Siyakhokha portal page.
        /// </summary>
        public const string LandingController = "Home";
        public const string LandingAction = "RCSHome";

        /// <summary>
        /// True when the signed-in user belongs to a PLM department (EHC, Real Estate,
        /// Human Settlement or a combination), and so should see the PLM layouts rather
        /// than the municipal portal. Anonymous visitors return false.
        /// </summary>
        public static bool IsPlmUser()
        {
            var http = HttpContext.Current;
            if (http == null || http.User == null || http.User.Identity == null ||
                !http.User.Identity.IsAuthenticated)
                return false;

            var key = CurrentDepartmentKey();
            return key == ApplicationEntityKeys.EkurhuleniHousingCompany
                || key == ApplicationEntityKeys.RealEstateDevelopment
                || key == ApplicationEntityKeys.HumanSettlmentDevelopment
                || key == ApplicationEntityKeys.HSD_RED
                || key == ApplicationEntityKeys.EHC_HSD
                || key == ApplicationEntityKeys.RED_HSD
                || key == ApplicationEntityKeys.AllEntities;
        }

        /// <summary>
        /// True when the signed-in user belongs to one of the given department keys.
        ///
        /// A user whose department cannot be determined is treated as EHC, because EHC
        /// is the original single-department branch and thousands of its users pre-date
        /// the DepartmentId column. That keeps existing EHC staff working while still
        /// hiding another department's menus from them.
        /// </summary>
        public static bool IsInDepartment(params string[] applicationEntityKeys)
        {
            if (applicationEntityKeys == null || applicationEntityKeys.Length == 0) return false;
            var key = CurrentDepartmentKey();
            if (string.IsNullOrEmpty(key))
                key = ApplicationEntityKeys.EkurhuleniHousingCompany;
            return applicationEntityKeys.Any(k =>
                string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Cross-department administrators who legitimately see every department.
        /// Deliberately narrow: "Back Office System Administrator" is NOT included,
        /// because both EHC and Real Estate use that role name.
        /// </summary>
        public static bool IsCrossDepartmentAdmin()
        {
            var http = HttpContext.Current;
            if (http == null || http.User == null) return false;
            return http.User.IsInRole("Super Administrators")
                || http.User.IsInRole("Administrators")
                || http.User.IsInRole("Support Admin");
        }

        /// <summary>True when the current user may see Real Estate (DPRE) functionality.</summary>
        public static bool CanSeeRealEstate()
        {
            return IsCrossDepartmentAdmin()
                || IsInDepartment(ApplicationEntityKeys.RealEstateDevelopment);
        }

        /// <summary>True when the current user may see EHC functionality.</summary>
        public static bool CanSeeEhc()
        {
            return IsCrossDepartmentAdmin()
                || IsInDepartment(ApplicationEntityKeys.EkurhuleniHousingCompany);
        }

        /// <summary>
        /// The department the signed-in user belongs to, falling back to EHC when the
        /// user has no department or the value does not resolve to a real row.
        /// Use this when stamping a department onto a record the user is creating.
        /// </summary>
        public static int? CurrentOrEhc(eServicesDbContext context)
        {
            if (context == null) return null;
            try
            {
                var user = HttpContext.Current != null ? HttpContext.Current.User : null;
                if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
                {
                    var name = user.Identity.Name;
                    var deptId = context.SystemUsers
                        .Where(u => u.UserName == name)
                        .Select(u => u.DepartmentId)
                        .FirstOrDefault();

                    if (deptId.HasValue &&
                        context.ApplicationEntities.Any(e => e.Id == deptId.Value))
                        return deptId;
                }
            }
            catch (Exception)
            {
                // fall through to the EHC default
            }
            return EhcDepartmentId(context);
        }

    }
}
