//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.IdentityModel.Protocols.OpenIdConnect;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.IdentityModel.Tokens;

//public class Startup
//{
//    public void ConfigureServices(IServiceCollection services)
//    {
//        // Add authentication services
//        services.AddAuthentication(options =>
//        {
//            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//        })
//        .AddCookie()
//        .AddOpenIdConnect(options =>
//        {
//            options.ClientId = Configuration["ClientId"];
//            options.ClientSecret = Configuration["ClientSecret"];
//            options.Authority = Configuration["Authority"];
//            options.CallbackPath = Configuration["RedirectUri"];
//            options.ResponseType = OpenIdConnectResponseType.Code;
//            options.TokenValidationParameters = new TokenValidationParameters
//            {
//                ValidateIssuer = false, // This is a simplification
//            };
//            options.Events = new OpenIdConnectEvents
//            {
//                OnAuthenticationFailed = context =>
//                {
//                    context.HandleResponse();
//                    context.Response.Redirect("/?errormessage=" + context.Exception.Message);
//                    return Task.CompletedTask;
//                },
//                OnAuthorizationCodeReceived = async context =>
//                {
//                    var client = new HttpClient();
//                    var appUrl = new HumanSettlementApplicationController().GetAppUrl();
//                    var discoveryDocument = await client.GetDiscoveryDocumentAsync(Configuration["Authority"]);
//                    var tokenResponse = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
//                    {
//                        Address = discoveryDocument.TokenEndpoint,
//                        ClientId = context.Options.ClientId,
//                        ClientSecret = context.Options.ClientSecret,
//                        Code = context.ProtocolMessage.Code,
//                        RedirectUri = context.Options.RedirectUri,
//                    });
//                    if (tokenResponse.IsError) throw new Exception(tokenResponse.Error);
//                    var userInfoResponse = await client.GetUserInfoAsync(new UserInfoRequest
//                    {
//                        Address = discoveryDocument.UserInfoEndpoint,
//                        Token = tokenResponse.AccessToken
//                    });
//                    if (userInfoResponse.IsError) throw new Exception(userInfoResponse.Error);
//                    var claims = userInfoResponse.Claims;
//                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.GivenName, ClaimTypes.Role);
//                    identity.AddClaims(claims);
//                    identity.AddClaim(new Claim(OpenIdConnectParameterNames.AccessToken, tokenResponse.AccessToken));
//                    identity.AddClaim(new Claim(OpenIdConnectParameterNames.ExpiresIn, DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToLocalTime().ToString()));
//                    identity.AddClaim(new Claim(OpenIdConnectParameterNames.RefreshToken, tokenResponse.RefreshToken));
//                    identity.AddClaim(new Claim(OpenIdConnectParameterNames.IdToken, tokenResponse.IdentityToken));
//                    identity.AddClaim(new Claim(OpenIdConnectParameterNames.TokenType, tokenResponse.TokenType));
//                    identity.AddClaim(new Claim(OpenIdConnectParameterNames.Scope, tokenResponse.Scope));
//                    identity.AddClaim(new Claim(ClaimTypes.Authentication, "true"));
//                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, claims.SingleOrDefault(d => d.Type == "sub").Value));
//                    // Map claims from WSO2.
//                    foreach (var claim in claims)
//                    {
//                        System.Diagnostics.Debug.WriteLine($"{claim.Type}: {claim.Value}");
//                        switch (claim.Type)
//                        {
//                            case "sub":
//                                identity.AddClaim(new Claim(ClaimTypes.Name, claim.Value));
//                                break;
//                            case "groups":
//                                var roles = claim.Value.Split(',');
//                                var role = string.Empty;
//                                foreach (var r in roles)
//                                {
//                                    if (r.Contains("/")) role = r.Substring(r.IndexOf("/") + 1);
//                                    if (r.Contains("_")) role = role.Replace("_", " ");
//                                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
//                                }
//                                break;
//                            case "email":
//                                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, claim.Value));
//                                identity.AddClaim(new Claim(ClaimTypes.Email, claim.Value));
//                                break;
//                            default:
//                                break;
//                        }
//                    }
//                    var authenticationProperties = new AuthenticationProperties();
//                    context.Principal = new ClaimsPrincipal(identity);
//                    context.Properties = authenticationProperties;
//                }
//            };
//        });
//    }

//    public void Configure(IApplicationBuilder app)
//    {
//        // Configure the app to use authentication
//        app.UseAuthentication();
//        // Other middleware and configuration...
//    }
//}
