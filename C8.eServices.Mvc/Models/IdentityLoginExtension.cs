using C8.eServices.Mvc.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models
{
    public static class IdentityLoginExtensions
    {
        public static async Task<SystemIdentityUser> FindByUserNameOrEmailAsync
            (this UserManager<SystemIdentityUser> userManager, string usernameOrEmail, string password)
        {
            var username = usernameOrEmail;
            if (usernameOrEmail.Contains("@"))
            {
                var userForEmail = await userManager.FindByEmailAsync(usernameOrEmail);
                if (userForEmail != null)
                {
                    username = userForEmail.UserName;
                }
            }

            return await userManager.FindAsync(username, password);
        }
    }
}