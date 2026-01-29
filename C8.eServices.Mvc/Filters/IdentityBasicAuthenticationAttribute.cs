using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace C8.eServices.Mvc.Filters
{
    public class IdentityBasicAuthenticationAttribute : BasicAuthenticationAttribute
    {
        protected override async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
        {
            UserManager<SystemIdentityUser> userManager = CreateUserManager();

            cancellationToken.ThrowIfCancellationRequested(); // Unfortunately, UserManager doesn't support CancellationTokens.
            SystemIdentityUser user = await userManager.FindByUserNameOrEmailAsync( userName, password );

            //Uncomment below code to bypass login with user's password 
            //SystemIdentityUser user = await userManager.FindByNameAsync( userName.Trim() );

            if (user == null)
            {
                // No user with userName/password exists.
                return null;
            }

            // Create a ClaimsIdentity with all the claims for this user.
            cancellationToken.ThrowIfCancellationRequested(); // Unfortunately, IClaimsIdenityFactory doesn't support CancellationTokens.
            ClaimsIdentity identity = await userManager.ClaimsIdentityFactory.CreateAsync(userManager, user, "Basic");
            return new ClaimsPrincipal(identity);
        }

        private static UserManager<SystemIdentityUser> CreateUserManager()
        {
            return new UserManager<SystemIdentityUser>( new UserStore<SystemIdentityUser>( new eServicesDbContext() ) );
        }
    }
}