using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using C8.eServices.Mvc.Models;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using IdentityModel.Client;
using System.Linq;
using C8.eServices.Mvc.DataAccessLayer;
using System.Web.Helpers;
using C8.eServices.Mvc.Controllers;
using System.Web;

namespace C8.eServices.Mvc
{
    public partial class Startup
    {

        //string url = new HumanSettlementApplicationController().GetAppUrl();
        // IAM/OpenIdConnect fields removed - IAM server (10.2.2.163:9443) is decommissioned and no longer in use.
        // Local cookie-based login is the only active authentication mechanism.
        // string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
        // string clientSecret = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];
        // string redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];
        // static string tenant = System.Configuration.ConfigurationManager.AppSettings["Tenant"];
        // static string authorityUrl = System.Configuration.ConfigurationManager.AppSettings["Authority"];
        // string authority = String.Format(System.Globalization.CultureInfo.InvariantCulture, authorityUrl, tenant);
        // Uri url = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        // string redirectUriLOGOUT = System.Configuration.ConfigurationManager.AppSettings["logout"];

        

        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // IAM credential selection removed - see commented fields above.

            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(eServicesDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, SystemIdentityUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});

            // IAM/OpenIdConnect authentication disabled - IAM server (10.2.2.163:9443) is decommissioned.
            // Local cookie-based login (UseCookieAuthentication above) is the only active authentication mechanism.
        }
    }
}