using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Web.Services.Description;
using C8.eServices.Mvc.ViewModels;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Helpers
{
    public class ADLogin
    {
    
            private eServicesDbContext db = new eServicesDbContext();

            public bool Valid { get; private set; }
            public string UniqueName { get; private set; }
            public string EmailAddress { get; private set; }

            public List<string> Validate(string domain, bool searchUid, LoginViewModel model)
            {
                List<string> userInfo = new List<string>();
                try
                {
                    using (var principalContext = new PrincipalContext(ContextType.Domain, domain, model.UserName, model.Password))
                    {
                        Valid = principalContext.ValidateCredentials(model.UserName, model.Password);
                        UniqueName = model.UserName;

                        if (searchUid && Valid)
                        {
                            using (var userPrincipal = new UserPrincipal(principalContext))
                            {
                                userPrincipal.SamAccountName = model.UserName;
                                using (var principalSearcher = new PrincipalSearcher())
                                {
                                    principalSearcher.QueryFilter = userPrincipal;
                                    Principal searchResult = principalSearcher.FindOne();
                                    if (searchResult != null)
                                    {
                                        DirectoryEntry directoryEntry = (DirectoryEntry)searchResult.GetUnderlyingObject();
                                        if (directoryEntry.Properties.Contains("mail"))
                                        {
                                            userInfo.Add(directoryEntry.Properties["mail"].Value.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Valid = false;
                }
                return userInfo.ToList();
            }


            public bool ValidateUser(string UserName, string Password)
            {
                try
                {
                    var activeDirectoryDomain = db.AppSettings.Where(x => x.Key == AppSettingKeys.activeDirectoryDomain).FirstOrDefault().Value;

                    UserName = UserName.Trim();
                    using (var context = new PrincipalContext(ContextType.Domain, activeDirectoryDomain))
                    {
                        return context.ValidateCredentials(UserName, Password);
                    }
                }
                catch
                {
                    //TODO: Inject logging and log exception
                    return false;
                }
            }

        public ClerkRegistration ValidateUser2(string UserName)
        {
            List<string> userInfo = new List<string>();
            ClerkRegistration cr = new ClerkRegistration();
            string user2 = "SolarTest06";
            
            try
            {
                var activeDirectoryDomain = db.AppSettings.Where(x => x.Key == AppSettingKeys.activeDirectoryDomain).FirstOrDefault().Value;
                var aduser = db.AppSettings.Where(x => x.Key == AppSettingKeys.ADUserCreds).FirstOrDefault();

                UserName = UserName.Trim();
                using (var context = new PrincipalContext(ContextType.Domain, activeDirectoryDomain, aduser.Description, aduser.Value))
                {
                    using (var userPrincipal = new UserPrincipal(context))
                    {
                        userPrincipal.SamAccountName = UserName;
                        using (var principalSearcher = new PrincipalSearcher())
                        {
                            principalSearcher.QueryFilter = userPrincipal;
                            Principal searchResult = principalSearcher.FindOne();
                            if (searchResult != null)
                            {
                                DirectoryEntry directoryEntry = (DirectoryEntry)searchResult.GetUnderlyingObject();
                                cr.UserName = UserName;
                                cr.IsActiveDirectoryUser = true;
                                if (directoryEntry.Properties.Contains("mail"))
                                {
                                    userInfo.Add(directoryEntry.Properties["mail"].Value.ToString());
                                    cr.EmailAddress = directoryEntry.Properties["mail"].Value.ToString();
                                }
                                if (directoryEntry.Properties.Contains("sn"))
                                {
                                    userInfo.Add(directoryEntry.Properties["sn"].Value.ToString());
                                    cr.LastName = directoryEntry.Properties["sn"].Value.ToString();
                                }
                                if (directoryEntry.Properties.Contains("givenname"))
                                {
                                    userInfo.Add(directoryEntry.Properties["givenname"].Value.ToString());
                                    cr.FirstName = directoryEntry.Properties["givenname"].Value.ToString();
                                }
                                if (directoryEntry.Properties.Contains("cn"))
                                {
                                    userInfo.Add(directoryEntry.Properties["cn"].Value.ToString());
                                }
                                if (directoryEntry.Properties.Contains("telephoneNumber"))
                                {
                                    userInfo.Add(directoryEntry.Properties["telephoneNumber"].Value.ToString());
                                }
                                if (directoryEntry.Properties.Contains("mobile"))
                                {
                                    userInfo.Add(directoryEntry.Properties["mobile"].Value.ToString());
                                    cr.MobileNumber = directoryEntry.Properties["mobile"].Value.ToString();
                                }

                            }
                        }
                    }

                    return cr;
                }



            
            }
            catch
            {
                //TODO: Inject logging and log exception
                return cr;
            }
        }
        public bool ValidateAdUserName(string domain, bool searchUid, string SearchFor, string UserName, string Password)
            {
                bool UserNameExists = false;
                var principalContext = new PrincipalContext(ContextType.Domain, domain, UserName.Trim(), Password);
                try
                {
                    using (var userPrincipal = new UserPrincipal(principalContext))
                    {
                        userPrincipal.SamAccountName = SearchFor;
                        using (var principalSearcher = new PrincipalSearcher())
                        {
                            principalSearcher.QueryFilter = userPrincipal;
                            Principal searchResult = principalSearcher.FindOne();
                            if (searchResult != null)
                            {
                                DirectoryEntry directoryEntry = (DirectoryEntry)searchResult.GetUnderlyingObject();
                                if (directoryEntry.Properties.Contains("sAMAccountName"))
                                {
                                    UserNameExists = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var e = ex.Message;
                    Valid = false;
                }
                return UserNameExists;
            }
        }
   
}