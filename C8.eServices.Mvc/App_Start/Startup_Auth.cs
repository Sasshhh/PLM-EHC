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
        // The Client ID is used by the application to uniquely identify itself to Microsoft identity platform.
        string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];

        string clientSecret = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];

        // RedirectUri is the URL where the user will be redirected to after they sign in.
        string redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];

        // Tenant is the tenant ID (e.g. contoso.onmicrosoft.com, or 'common' for multi-tenant)
        static string tenant = System.Configuration.ConfigurationManager.AppSettings["Tenant"];

        static string authorityUrl = System.Configuration.ConfigurationManager.AppSettings["Authority"];

        // Authority is the URL for authority, composed by Microsoft identity platform endpoint and the tenant name (e.g. https://login.microsoftonline.com/contoso.onmicrosoft.com/v2.0)
        string authority = String.Format(System.Globalization.CultureInfo.InvariantCulture, authorityUrl, tenant);

        Uri url = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

        string redirectUriLOGOUT = System.Configuration.ConfigurationManager.AppSettings["logout"];

        

        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            var Backslashpath = url.LocalPath.Split('\\').ToList().Count;
            if (url.LocalPath.Contains("Workspace"))
            {
                clientId = System.Configuration.ConfigurationManager.AppSettings["InternalClientId"];
                clientSecret = System.Configuration.ConfigurationManager.AppSettings["InternalClientSecret"];
                redirectUri = System.Configuration.ConfigurationManager.AppSettings["InternalRedirectUri"];
            } 
            else
            {
                clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
                clientSecret = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];
                redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];
            }


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

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AuthenticationType = OpenIdConnectAuthenticationDefaults.AuthenticationType,
                SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType(),

                // Sets the ClientId, authority, RedirectUri as obtained from web.config
                ClientId = clientId,
                ClientSecret = clientSecret,
                Authority = authority,
                RedirectUri = redirectUri,
                PostLogoutRedirectUri = redirectUriLOGOUT,
                // PostLogoutRedirectUri is the page that users will be redirected to after sign-out. In this case, it is using the home page
                //PostLogoutRedirectUri = redirectUri,
                Scope = OpenIdConnectScope.OpenId,
                SaveTokens = true,

                // ResponseType is set to request the id_token - which contains basic information about the signed-in user
                ResponseType = OpenIdConnectResponseType.Code,

                // ValidateIssuer set to false to allow personal and work accounts from any organization to sign in to your application
                // To only allow users from a single organizations, set ValidateIssuer to true and 'tenant' setting in web.config to the tenant name
                // To allow users from only a list of specific organizations, set ValidateIssuer to true and use ValidIssuers parameter
                TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false // This is a simplification
                },

                Configuration = new OpenIdConnectConfiguration
                {
                    AuthorizationEndpoint = string.Format("{0}/oauth2/authorize", authorityUrl),
                    TokenEndpoint = string.Format("{0}/oauth2/token", authorityUrl),
                    UserInfoEndpoint = string.Format("{0}/oauth2/userinfo", authorityUrl)
                },

                // OpenIdConnectAuthenticationNotifications configures OWIN to send notification of failed authentications to OnAuthenticationFailed method
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = OnAuthenticationFailed,

                    AuthorizationCodeReceived = async (notification) =>
                    {
                        using (var client = new HttpClient())
                        {
                            string url = new HumanSettlementApplicationController().GetAppUrl();
                            // JK.20210826a - Used to change the unique identifier claim for the users.
                            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimsIdentity.DefaultNameClaimType;

                            var configuration = await notification.Options.ConfigurationManager.GetConfigurationAsync(notification.Request.CallCancelled);
                            var request = new HttpRequestMessage(HttpMethod.Post, configuration.TokenEndpoint);
                            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                            {
                                {OpenIdConnectParameterNames.ClientId, notification.Options.ClientId},
                                {OpenIdConnectParameterNames.ClientSecret, notification.Options.ClientSecret},
                                {OpenIdConnectParameterNames.Code, notification.ProtocolMessage.Code},
                                {OpenIdConnectParameterNames.GrantType, "authorization_code"},
                                {OpenIdConnectParameterNames.ResponseType, "code"},
                                {OpenIdConnectParameterNames.RedirectUri, notification.Options.RedirectUri}
                            });

                            HttpResponseMessage response = null;

                            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                            response = await client.SendAsync(request, notification.Request.CallCancelled);
                            response.EnsureSuccessStatusCode();

                            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());

                            // use the access token to retrieve claims from userinfo
                            var uiclient = new HttpClient();
                            var uiresponse = await uiclient.GetUserInfoAsync(new UserInfoRequest
                            {
                                Address = string.Format("{0}/oauth2/userinfo", authorityUrl),
                                Token = payload.Value<string>(OpenIdConnectParameterNames.AccessToken)
                            });

                            if (uiresponse.IsError) throw new Exception(uiresponse.Error);
                            var uiclaims = uiresponse.Claims;

                            // create new identity
                            var id = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie, ClaimTypes.GivenName, ClaimTypes.Role);
                            id.AddClaims(uiclaims);
                            id.AddClaim(new Claim(OpenIdConnectParameterNames.AccessToken, payload.Value<string>(OpenIdConnectParameterNames.AccessToken)));
                            id.AddClaim(new Claim(OpenIdConnectParameterNames.ExpiresIn, DateTime.Now.AddSeconds(payload.Value<double>(OpenIdConnectParameterNames.ExpiresIn)).ToLocalTime().ToString()));
                            id.AddClaim(new Claim(OpenIdConnectParameterNames.RefreshToken, payload.Value<string>(OpenIdConnectParameterNames.RefreshToken)));
                            id.AddClaim(new Claim(OpenIdConnectParameterNames.IdToken, payload.Value<string>(OpenIdConnectParameterNames.IdToken)));
                            id.AddClaim(new Claim(OpenIdConnectParameterNames.TokenType, payload.Value<string>(OpenIdConnectParameterNames.TokenType)));
                            id.AddClaim(new Claim(OpenIdConnectParameterNames.Scope, payload.Value<string>(OpenIdConnectParameterNames.Scope)));
                            id.AddClaim(new Claim(ClaimTypes.Authentication, "true"));
                            id.AddClaim(new Claim(ClaimTypes.NameIdentifier, uiclaims.SingleOrDefault(d => d.Type == "sub").Value));

                            // Map claims from WSO2.
                            foreach (var c in uiclaims)
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format("{0}: {1}", c.Type, c.Value));
                                switch (c.Type)
                                {
                                    case "sub":
                                        id.AddClaim(new Claim(ClaimTypes.Name, c.Value));
                                        break;
                                    case "groups":
                                        var roles = c.Value.Split(',');
                                        var role = string.Empty;

                                        foreach (var r in roles)
                                        {
                                            if (r.Contains("/")) role = r.Substring(r.IndexOf("/") + 1);
                                            if (r.Contains("_")) role = role.Replace("_", " ");
                                            id.AddClaim(new Claim(ClaimTypes.Role, role));
                                        }
                                        break;
                                    case "email":
                                        id.AddClaim(new Claim(ClaimTypes.NameIdentifier, c.Value));
                                        id.AddClaim(new Claim(ClaimTypes.Email, c.Value));
                                        break;
                                    default:
                                        break;
                                }
                            }

                            AuthenticationProperties props = new AuthenticationProperties();
                            notification.AuthenticationTicket = new AuthenticationTicket(id, props);

                            ;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Handle failed authentication requests by redirecting the user to the home page with an error in the query string
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            context.HandleResponse();
            context.Response.Redirect("/?errormessage=" + context.Exception.Message);
            return Task.FromResult(0);
        }
    }
}