using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Helpers
{
    /// <summary>
    /// "CEO" and "Property Manager" are the same business role (the old and the new name). A user in either
    /// role gets the other one as well, so every [Authorize(Roles=...)], User.IsInRole(...) check and menu
    /// treats them the same, whichever name a page was written against.
    /// </summary>
    public static class RoleEquivalence
    {
        public const string CEO = "CEO";
        public const string PropertyManager = "Property Manager";

        // Old and new names for the same role. To add a pair, add a group here.
        private static readonly string[][] Groups =
        {
            new[] { CEO, PropertyManager },
            new[] { "Client Services Officer", "Letting Officer" },
            new[] { "Client Services Supervisor", "Housing Supervisor" },
        };

        /// <summary>Adds the equivalent role claims to the signed-in identity (called on every request).</summary>
        public static void Apply(IPrincipal user)
        {
            var principal = user as ClaimsPrincipal;
            if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated) return;
            var identity = principal.Identity as ClaimsIdentity;
            if (identity == null) return;

            foreach (var group in Groups)
            {
                if (!group.Any(r => identity.HasClaim(identity.RoleClaimType, r))) continue;
                foreach (var role in group)
                {
                    if (!identity.HasClaim(identity.RoleClaimType, role))
                        identity.AddClaim(new Claim(identity.RoleClaimType, role));
                }
            }
        }

        public static bool IsCeoOrPropertyManager(IPrincipal user)
        {
            return user != null && (user.IsInRole(CEO) || user.IsInRole(PropertyManager));
        }

        /// <summary>
        /// Work-queue owners for the current user. CEO / Property Manager work is routed to the users in AppSettings
        /// r_property_manager and s_ceo; anyone in the CEO / Property Manager role sees both, so the two names see
        /// the same queues.
        /// </summary>
        public static List<int> QueueClerkIds(eServicesDbContext db, IPrincipal user, int userId)
        {
            var ids = new List<int> { userId };
            if (IsCeoOrPropertyManager(user))
            {
                foreach (var key in new[] { AppSettingKeys.PropertyManager, AppSettingKeys.CEO })
                {
                    int id;
                    if (int.TryParse(db.AppSettings.Where(x => x.Key == key).Select(x => x.Value).FirstOrDefault(), out id) && id != 0)
                        ids.Add(id);
                }
            }
            return ids.Distinct().ToList();
        }
    }
}
