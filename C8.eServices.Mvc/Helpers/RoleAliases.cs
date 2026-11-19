using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace C8.eServices.Mvc.Helpers
{
    /// <summary>
    /// Old and new names for the same EHC role. A user who has any role in a group is treated
    /// as having every role in that group, so [Authorize(Roles=...)], User.IsInRole and the menus
    /// give both names the same access while the old names are being replaced.
    /// To add a pair, add a new group below.
    /// </summary>
    public static class RoleAliases
    {
        public static readonly string[][] Groups =
        {
            new[] { "CEO", "Property Manager" },
            new[] { "Client Services Officer", "Letting Officer" },
            new[] { "Client Services Supervisor", "Housing Supervisor" },
        };

        /// <summary>Adds the alias role claims to every claims identity of the principal (idempotent).</summary>
        public static void Apply(IPrincipal principal)
        {
            var cp = principal as ClaimsPrincipal;
            if (cp == null) return;
            foreach (var identity in cp.Identities)
            {
                if (identity == null || !identity.IsAuthenticated) continue;
                var roleType = identity.RoleClaimType ?? ClaimTypes.Role;
                var have = new HashSet<string>(identity.FindAll(roleType).Select(c => c.Value), StringComparer.OrdinalIgnoreCase);
                foreach (var group in Groups)
                {
                    if (!group.Any(have.Contains)) continue;
                    foreach (var role in group)
                    {
                        if (have.Add(role))
                            identity.AddClaim(new Claim(roleType, role));
                    }
                }
            }
        }
    }
}
