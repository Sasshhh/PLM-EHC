using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Filters;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Ionic.Zip;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.SqlServer.ReportExecution;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PdfReader = PdfSharp.Pdf.IO.PdfReader;
using C8.eServices.Mvc.Controllers;
using System.Text;

namespace C8.eServices.Mvc.Controllers
{
    [IdentityBasicAuthentication] // Enable authentication via an ASP.NET Identity user name and password
    [Authorize] // Require some form of authentication
    public class MobileController : ApiController
    {
        private int _systemUserId = -1;
        private eServicesDbContext context;

        public IdentityManager IdentityManager { get; set; }
        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        #region Init Identity
        public MobileController()
        {
            context = new eServicesDbContext();

            IdentityManager = new IdentityManager(context);
            UserManager =
                new UserManager<SystemIdentityUser>(
                new UserStore<SystemIdentityUser>(context));
        }
        #endregion

        #region Customer Details
        // GET api/<controller>
        [HttpGet]
        [Route("api/mobile/customer")]
        public IHttpActionResult Customer()
        {
            try
            {
                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    _systemUserId = IdentityManager.CurrentUser(User).Id;
                }

                using (var dbContext = new eServicesDbContext())
                {
                    var customer = dbContext.Customers.FirstOrDefault(c => c.SystemUserId == _systemUserId);

                    if (customer == null)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            Content = new StringContent("Customer ID Not Found."),
                            ReasonPhrase = "Customer ID Not Found"
                        };

                        throw new HttpResponseException(resp);
                    }

                    var json = Json(customer, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Objects
                    });

                    return json;
                }
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response.StatusCode);
            }
        }
        #endregion

        #region Entity Details
        // GET api/<controller>
        [HttpGet]
        [Route("api/mobile/entity")]
        public IHttpActionResult Entity()
        {
            try
            {
                Entity entity = null;

                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    _systemUserId = IdentityManager.CurrentUser(User).Id;
                }

                using (var dbContext = new eServicesDbContext())
                {
                    var customer = dbContext.Customers.FirstOrDefault(c => c.SystemUserId == _systemUserId);

                    if (customer == null)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            Content = new StringContent("Customer ID Not Found."),
                            ReasonPhrase = "Customer ID Not Found"
                        };

                        throw new HttpResponseException(resp);
                    }
                    else
                        entity = dbContext.Entities.FirstOrDefault(e => e.CustomerId == customer.Id);

                    if (entity == null)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            Content = new StringContent("Business Entity Not Found."),
                            ReasonPhrase = "Entity Not Found"
                        };

                        throw new HttpResponseException(resp);
                    }

                    var json = Json(entity, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Objects
                    });

                    return json;
                }
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response.StatusCode);
            }
        }
        #endregion

        #region Customer Details Via ID No.
        // POST api/<controller>
        [HttpPost]
        [Route("api/mobile/customer")]
        public IHttpActionResult Customer([FromBody] IdentificationViewModel id)
        {
            try
            {
                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    _systemUserId = IdentityManager.CurrentUser(User).Id;
                }

                using (var dbContext = new eServicesDbContext())
                {
                    var customer = dbContext.Customers.FirstOrDefault(c => c.IdentificationNumber == id.IdentificationNumber);

                    if (customer == null)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            Content = new StringContent("Customer ID Not Found."),
                            ReasonPhrase = "Customer ID No. Not Found"
                        };

                        throw new HttpResponseException(resp);
                    }

                    var json = Json(customer, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Objects
                    });

                    return json;
                }
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response.StatusCode);
            }
        }
        #endregion

        #region Customer Details Via Email Address.
        // POST api/<controller>
        [HttpPost]
        [Route("api/mobile/customer")]
        public IHttpActionResult Customer([FromBody] EmailViewModel email)
        {
            try
            {
                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    _systemUserId = IdentityManager.CurrentUser(User).Id;
                }

                using (var dbContext = new eServicesDbContext())
                {
                    var customer = dbContext.Customers.FirstOrDefault(c => c.EmailAddress == email.EmailAddress);

                    if (customer == null)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            Content = new StringContent("Customer with email address not found."),
                            ReasonPhrase = "Customer with email address not found."
                        };
                                                
                        throw new HttpResponseException(resp);
                    }

                    var json = Json(customer, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Objects
                    });

                    return json;
                }
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response.StatusCode);
            }
        }
        #endregion
        
        #region Register new user and create partial customer.
        // POST api/<controller>
        [HttpPost]
        [Route("api/mobile/register")]
        public async Task<IHttpActionResult> Register([FromBody] RegisterViewModelApi model)
        {
            try
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound);

                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    _systemUserId = IdentityManager.CurrentUser(User).Id;
                }

                using (var context = new eServicesDbContext())
                {
                    if (ModelState.IsValid)
                    {
                        var usernameAssigned = context.SystemUsers.Any(u => u.UserName.ToLower() ==
                            model.UserName.ToLower() && u.IsActive && u.IsDeleted == false);
                        var emailAssigned = context.SystemUsers.Any(u => u.EmailAddress.ToLower() ==
                            model.EmailAddress.ToLower() && u.IsActive && u.IsDeleted == false);
                        var response = "";

                        if (usernameAssigned)
                        {
                            resp.Content = new StringContent("The username entered is already taken. | ");
                            resp.ReasonPhrase = "The username entered is already taken. | ";
                        }
                        else if (emailAssigned)
                        {
                            resp.Content = new StringContent("The email address entered is already in use. | ");
                            resp.ReasonPhrase = "The email address entered is already in use. | ";
                        }
                        else
                        {
                            // JK.20140724a - Passing values from the ViewModel to the Model.
                            var user = new SystemIdentityUser { UserName = model.UserName, Email = model.EmailAddress };
                            var identityManager = new IdentityManager();


                            // JK.20140724a - Custom profile information.
                            user.SystemUser = new SystemUser
                            {
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                UserName = model.UserName,
                                Designation = model.Designation,
                                CompanyName = model.CompanyName,
                                EmailAddress = model.EmailAddress,
                                IsActive = true,
                                IsDeleted = false,
                                IsLocked = false,
                                CreatedDateTime = DateTime.Now,
                                ModifiedDateTime = DateTime.Now,
                                IsPasswordReset = true
                            };

                            model.Password = PasswordGenerator.GeneratePassword(true, true, true, true, false, 6);

                            var result = await UserManager.CreateAsync(user, model.Password);
                            identityManager.AddUserToRole(user.Id, "Customers");

                            var provider = new DpapiDataProtectionProvider("SampleAppName");
                            UserManager.UserTokenProvider = new DataProtectorTokenProvider<SystemIdentityUser>(
                                provider.Create("SampleTokenName"));

                            if (result.Succeeded)
                            {
                                //Confirmation email
                                var confirmationEmail = new Email();
                                var emailConfirmationCode =
                                    await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                                emailConfirmationCode = HttpUtility.UrlEncode(emailConfirmationCode);
                                const string subject = "Siyakhokha: Email Confirmation";
                                var body = string.Format("Thank you for your registration, please click on the below link to complete your registration: <br/><a href=\"{0}\" title=\"User Email Confirm\">{0}</a><br/><br/> Your username and system generated password is below:<br/>Username: {1}<br/>Password:{2}", Url.Link("Default", new { controller = "Account", action = "ConfirmEmail", q = SecureActionLinkExtension.EncryptToString(new { id = user.Id, token = emailConfirmationCode }) }), user.UserName, model.Password);
                                confirmationEmail.GenerateEmail(user.SystemUser.EmailAddress, subject, body, user.SystemUserId.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.SystemUser.FullName);

                                // Create the customer.
                                var cust = new Customer()
                                {
                                    FirstName = model.FirstName,
                                    LastName = model.LastName,
                                    IdentificationNumber = model.IdentificationNumber,
                                    CellPhoneNumber = model.CellPhoneNumber,
                                    WorkPhoneNumber = model.WorkPhoneNumber,
                                    EmailAddress = model.EmailAddress,
                                    SystemUserId = user.SystemUserId,
                                    TitleTypeId = context.TitleTypes.SingleOrDefault(tt => tt.Key == TitleTypeKeys.Mister).Id,
                                    CustomerTypeId = context.CustomerTypes.SingleOrDefault(ct => ct.Key == CustomerTypeKeys.Individual).Id,
                                    IdentificationTypeId = context.IdentificationTypes.SingleOrDefault(it => it.Key == IdentificationTypeKey.SouthAfricanID).Id,
                                    StatusId = context.Status.SingleOrDefault(s => s.Key == StatusKeys.CustomerActive).Id,
                                    IsDeceased = false,
                                    IsActive = true,
                                    IsDeleted = false,
                                    IsLocked = false
                                };

                                context.Customers.Add(cust);
                                context.SaveChanges();

                                response = "Success";

                                var json = Json(response, new JsonSerializerSettings()
                                {
                                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                    Formatting = Formatting.Indented,
                                    TypeNameHandling = TypeNameHandling.Objects
                                });

                                return json;
                            }
                            else
                            {
                                foreach (var error in result.Errors)
                                {
                                    resp.ReasonPhrase += error + " | ";
                                    resp.Content = new StringContent(resp.ReasonPhrase);
                                }
                            }
                        }
                    }
                }

                throw new HttpResponseException(resp);
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response);
            }
        }
        #endregion
        
        #region Customer Linked Account Details
        // GET api/<controller>
        [HttpGet]
        [Route("api/mobile/accounts")]
        public IHttpActionResult Accounts()
        {
            try
            {
                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    _systemUserId = IdentityManager.CurrentUser(User).Id;
                }

                using (var dbContext = new eServicesDbContext())
                {
                    var customer = dbContext.Customers.FirstOrDefault(c => c.SystemUserId == _systemUserId);

                    if (customer == null)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            Content = new StringContent("Customer ID Not Found"),
                            ReasonPhrase = "Customer ID Not Found"
                        };

                        throw new HttpResponseException(resp);
                    }

                    var query = dbContext.LinkedAccounts.Include(x => x.Account).Include(x => x.Customer)
                        .Include(x => x.Customer.Status)
                        .Include(x => x.Status)
                        .Where(x => x.CustomerId == customer.Id && x.Customer.IsActive
                        && x.Customer.Status.Key == StatusKeys.CustomerActive && x.IsActive && x.IsDeleted == false);

                    var cList = query.ToList();

                    foreach (var acc in cList)
                    {
                        //Checks if current customer profile belongs to the system user
                        if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, acc, ViewCodeKeys.ViewCustomerLinkedAccount))
                        {
                            SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                            throw new Exception("Malicious Activity");
                        }

                        acc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt($"Id={acc.Id}"));
                    }

                    var json = Json(cList, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Objects
                    });

                    return json;
                }
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response.StatusCode);
            }
        }
        #endregion


        #region Number of Bills
        // POST api/<controller>
        [HttpPost]
        [Route("api/mobile/numberofbills")]
        public IHttpActionResult NumberOfBills([FromBody] BillViewModel bill)
        {
            try
            {
                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    _systemUserId = IdentityManager.CurrentUser(User).Id;
                }

                using (var dbContext = new eServicesDbContext())
                {
                    var customer = dbContext.Customers.Include(c => c.CustomerType)
                   .FirstOrDefault(c => c.SystemUserId == _systemUserId);

                    if (customer == null)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            ReasonPhrase = "Customer ID Not Found"
                        };

                        throw new HttpResponseException(resp);
                    }

                    var linkedAccount = dbContext.LinkedAccounts.Include(x => x.Account).Include(x => x.Customer)
                        .Include(x => x.Customer.Status)
                        .Include(x => x.Status)
                        .FirstOrDefault(x => x.CustomerId == customer.Id && x.Customer.IsActive && x.Account.AccountNumber == bill.AccountNumber
                                    && x.Customer.Status.Key == StatusKeys.CustomerActive && x.IsActive && x.IsDeleted == false);

                    if (linkedAccount == null)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            ReasonPhrase = "Linked Account Not Found"
                        };

                        throw new HttpResponseException(resp);
                    }

                    //Checks if current customer profile belongs to the system user
                    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, linkedAccount, ViewCodeKeys.ViewCustomerLinkedAccount))
                    {
                        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                        var resp = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                        {
                            ReasonPhrase = "Malicious Activity"
                        };

                        throw new HttpResponseException(resp);
                    }

                    linkedAccount.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt($"Id={linkedAccount.Id}"));

                    var billingHistories = new List<BillingHistory>();
                    var eBilling6Months = new eBilling6MonthsDbContext();
                    var accountNumber = Convert.ToInt64(bill.AccountNumber);

                    //COINS accounts
                    var coinsBills = eBilling6Months.Database.SqlQuery<BillingHistory>(
                            "pr_REPORT_BillingHistorySummaryForArray @AccountNumbers",
                            new SqlParameter("@AccountNumbers", linkedAccount.Account.AccountNumber))
                            .ToList();

                    //RMS accounts
                    var rmsBills = dbContext.Database.SqlQuery<BillingHistory>(
                        "pr_REPORT_GetCustomerRmsBillingForArray @AccountNumbers,@CustomerId",
                        new SqlParameter("@AccountNumbers", linkedAccount.Account.AccountNumber),
                        new SqlParameter("@CustomerId", customer.Id))
                        .Where(b => b.IdentificationNumber.Equals(customer.IdentificationNumber))
                        .ToList();

                    billingHistories.AddRange(coinsBills);
                    billingHistories.AddRange(rmsBills);

                    var generatedBills =
                        billingHistories.Where(
                            b =>
                                b.BillDate.Year == bill.Year && b.BillDate.Month == bill.Month &&
                                b.AccountNumber == accountNumber).ToList();

                    var json = Json(generatedBills.Count, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Objects
                    });

                    return json;
                }
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response.StatusCode);
            }
        }
        #endregion

        #region Report Load Online Bills
        // POST api/<controller>
        [HttpGet]
        [Route("api/mobile/billlist")]
        public IHttpActionResult BillList(string AccountNo)
        {
           
            try
            {
                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    _systemUserId = IdentityManager.CurrentUser(User).Id;
                }

                    var BillListGen = new ReportController().LoadBillsList(AccountNo, _systemUserId);


                    var json = Json(BillListGen, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Objects
                    });

                    return json;
                
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response.StatusCode);
            }
        }
        #endregion

        #region meter consumption
        // POST api/<controller>
        [HttpGet]
        [Route("api/mobile/meterconsumption")]
        public HttpResponseMessage MeterConsumption(string meterType, string accountNumber, string startDate, string endDate)
        {

            try
            {
                var errorMessage = "";
                string returnData = string.Empty;

                if (meterType == null && accountNumber == null && startDate == null && endDate == null)
                {
                    errorMessage = "All Parameter Values are Null";
                   
                    string jsonReturn = errorMessage;
                    if (!string.IsNullOrEmpty(jsonReturn))
                    {
                        var responseMess = this.Request.CreateResponse(HttpStatusCode.BadRequest);
                        responseMess.Content = new StringContent(jsonReturn, Encoding.UTF8, "application/json");
                        return responseMess;
                    }
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                }
                        

                    // TODO: Move to AppSettings.
                    string json =
                        "{ " +
                            "\"serviceProperties\" : { " +
                                "\"systemCode\" : \"XET\", \"key\" : \"1234\" }, " +
                            "\"serviceRequest\" : { " +
                                "\"assetType\" : \"meter\", " +
                                "\"searchType\" : \"meterConsumption\", " +
                                "\"searchCriteria\" : [ " +
                                    "{ \"name\" : \"meterType\", \"value\" : \"" + meterType + "\" }, " +
                                    "{ \"name\" : \"accountNumber\", \"value\" : \"" + accountNumber + "\" },  " +
                                    "{ \"name\" : \"startDate\", \"value\" : \"" +startDate + "\" },  " +
                                    "{ \"name\" : \"endDate\", \"value\" : \"" + endDate + "\" } ] } } ";

                    WebHelper webHelper = new C8.eServices.Mvc.Helpers.WebHelper();
                    var assetUrl = new AppSetting();

                    using (var cxt = new eServicesDbContext())
                    {
                        assetUrl = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.MeterConsumptionUrl);
                    }

                    Uri url = new Uri(string.Format(assetUrl.Value + HttpUtility.UrlEncode(json)));

                    string response = webHelper.Get(url, json);
                    dynamic jo = Newtonsoft.Json.Linq.JObject.Parse(response);
                string consumption = "[";
                    //string consumption = "[";
                    string reading, d;
                    string meterNo = jo.assetProperties.meterProperties[1].meterIdentifiers.meterNumber;
                    DateTime date;
             
                    foreach (dynamic mc in jo.assetProperties.meterProperties[1].meterConsumption)
                    {
                    errorMessage = "Success";
                        d = mc["date"].ToString();
                        date = Convert.ToDateTime(d.Substring(0, 4) + "-" + d.Substring(4, 2) + "-" + d.Substring(6, 2));
                        reading = mc["consumption"].ToString();
                        //consumption += "{ \"meterNumber\":" + meterNo + ",\"Date\": " + date.ToString("yyyy-MM-dd") + ", \"Consumption\": " + reading + "},";
                        consumption += "{\r\n      " +
                             "\"MeterNo\": \"" + meterNo + "\",\r\n  " +
                                "\"Date\": \"" + date.ToString("yyyy-MM-dd") + "\",\r\n  " +
                                "\"Consumption\": \"" + reading + "\",\r\n  " +
                                "\"PayloadMsg\": \"" + errorMessage + "\"\r\n    },";
                }
                    consumption = consumption.Substring(0, consumption.Length - 1);
                    consumption += "]";

                    if (response != null)
                    {
                    errorMessage = "";
                   //Handle your reponse here
                   // return Json(consumption);


                    string jsonReturn = consumption;
                    if (!string.IsNullOrEmpty(jsonReturn))
                    {
                        var responseMess = this.Request.CreateResponse(HttpStatusCode.OK);
                        responseMess.Content = new StringContent(jsonReturn, Encoding.UTF8, "application/json");
                        return responseMess;
                    }
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                    else
                    {
                        //No Response from the server
                        System.Diagnostics.Debug.WriteLine(response);
                    //return Json("No data available.");
                    string jsonReturn = "No data available.";
                    if (!string.IsNullOrEmpty(jsonReturn))
                    {
                        var responseMess = this.Request.CreateResponse(HttpStatusCode.NotFound);
                        responseMess.Content = new StringContent(jsonReturn, Encoding.UTF8, "application/json");
                        return responseMess;
                    }
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response.StatusCode);
            }
        }
        #endregion


        #region Generate Bill - Coins/Rms
        // POST api/<controller>
        [HttpPost]
        [Route("api/mobile/generatebill")]
        public HttpResponseMessage GenerateBill([FromBody] BillViewModel bill)
        {
            try
            {
                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    _systemUserId = IdentityManager.CurrentUser(User).Id;
                }

                using (var dbContext = new eServicesDbContext())
                {
                    var customer = dbContext.Customers.Include(c => c.CustomerType)
                   .FirstOrDefault(c => c.SystemUserId == _systemUserId);

                    if (customer == null)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            ReasonPhrase = "Customer ID Not Found"
                        };

                        throw new HttpResponseException(resp);
                    }

                    var linkedAccount = dbContext.LinkedAccounts.Include(x => x.Account).Include(x => x.Customer)
                        .Include(x => x.Customer.Status)
                        .Include(x => x.Status)
                        .FirstOrDefault(x => x.CustomerId == customer.Id && x.Customer.IsActive && x.Account.AccountNumber == bill.AccountNumber
                                    && x.Customer.Status.Key == StatusKeys.CustomerActive && x.IsActive && x.IsDeleted == false);

                    if (linkedAccount == null)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            ReasonPhrase = "Linked Account Not Found"
                        };

                        throw new HttpResponseException(resp);
                    }

                    //Checks if current customer profile belongs to the system user
                    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, linkedAccount, ViewCodeKeys.ViewCustomerLinkedAccount))
                    {
                        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                        var resp = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                        {
                            ReasonPhrase = "Malicious Activity"
                        };

                        throw new HttpResponseException(resp);
                    }

                    linkedAccount.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt($"Id={linkedAccount.Id}"));

                    var billingHistories = new List<BillingHistory>();
                    var eBilling6Months = new eBilling6MonthsDbContext();
                    var accountNumber = Convert.ToInt64(bill.AccountNumber);

                    //COINS accounts
                    var coinsBills = eBilling6Months.Database.SqlQuery<BillingHistory>(
                            "pr_REPORT_BillingHistorySummaryForArray @AccountNumbers",
                            new SqlParameter("@AccountNumbers", linkedAccount.Account.AccountNumber))
                            .ToList();

                    //RMS accounts
                    var rmsBills = dbContext.Database.SqlQuery<BillingHistory>(
                        "pr_REPORT_GetCustomerRmsBillingForArray @AccountNumbers,@CustomerId",
                        new SqlParameter("@AccountNumbers", linkedAccount.Account.AccountNumber),
                        new SqlParameter("@CustomerId", customer.Id))
                        .Where(b => b.IdentificationNumber.Equals(customer.IdentificationNumber))
                        .ToList();

                    billingHistories.AddRange(coinsBills);
                    billingHistories.AddRange(rmsBills);

                    var generatedBills =
                        billingHistories.Where(
                            b =>
                                b.BillDate.Year == bill.Year && b.BillDate.Month == bill.Month &&
                                b.AccountNumber == accountNumber).ToList();

                    if (!generatedBills.Any())
                        return null;

                    var fileResults = new List<GenerateBillViewModel>();
                    var response = Request.CreateResponse(HttpStatusCode.OK);

                    foreach (var genBill in generatedBills)
                    {
                        if (genBill.AccountType == "RMS")
                        {
                            fileResults.Add(RenderRmsGeneratedBill(bill.AccountNumber,
                                genBill.BillDate.ToString(CultureInfo.InvariantCulture),
                                genBill.DownloadLink));
                        }
                        else
                        {
                            fileResults.Add(RenderCoinsGeneratedBill(bill.AccountNumber,
                               genBill.BillDate.ToString(CultureInfo.InvariantCulture), customer));
                        }
                    }

                    if (fileResults.Count() == 1)
                    {
                        response.Content = new ByteArrayContent(fileResults[0].FileContent);
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/text");
                        response.Content.Headers.ContentLength = fileResults[0].FileContent.Length;
                        ContentDispositionHeaderValue contentDisposition = null;
                        if (ContentDispositionHeaderValue.TryParse("attachment; filename=" + fileResults[0].FileName, out contentDisposition))
                        {
                            response.Content.Headers.ContentDisposition = contentDisposition;
                        }
                    }
                    else
                    {
                        if (bill.Index > 0)
                        {
                            response.Content = new ByteArrayContent(fileResults[0].FileContent);
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/text");
                            response.Content.Headers.ContentLength = fileResults[0].FileContent.Length;
                            ContentDispositionHeaderValue contentDisposition = null;
                            if (ContentDispositionHeaderValue.TryParse("attachment; filename=" + fileResults[bill.Index].FileName, out contentDisposition))
                            {
                                response.Content.Headers.ContentDisposition = contentDisposition;
                            }
                        }

                        //TODO: JK.20180626a - Remove zip code, replaced by the session code.
                        //using (var zip = new ZipFile())
                        //{
                        //    foreach (var fileResult in fileResults)
                        //    {
                        //        zip.AddFile(fileResult.BillLocation, Path.GetFileNameWithoutExtension(fileResult.BillLocation));
                        //    }

                        //    var pushStreamContent = new PushStreamContent((stream, content, context) =>
                        //    {
                        //        zip.Save(stream);
                        //        stream.Close();
                        //    }, "application/zip");

                        //    response.Content = pushStreamContent;
                        //}
                    }

                    return response;
                }
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response.StatusCode);
            }
        }
        #endregion

        #region Render RMS Bill
        private GenerateBillViewModel RenderRmsGeneratedBill(string accountNumber, string bill_date, string generatedBillLocation)
        {
            try
            {
                FileStream stream;
                var billDate = Convert.ToDateTime(bill_date);

                // Render arguments
                byte[] result = null;
                string mimeType = null;

                string generatedBillFilename = $"{accountNumber}-{billDate.ToString("yyyyMMdd")}.pdf";

                // Get the file document for streaming to browser.
                using (stream = System.IO.File.Open(@generatedBillLocation, FileMode.Open))
                {
                    // Used to allow the user to download the file.
                    var cd = new ContentDisposition()
                    {
                        FileName = generatedBillFilename,
                        // always prompt the user for downloading, set to true if you want 
                        // the browser to try to show the file inline
                        Inline = false
                    };

                    if (mimeType == null)
                        mimeType = MimeMapping.GetMimeMapping(generatedBillLocation);

                    //Response.ContentType = "application/pdf";
                    //Response.AppendHeader( "Content-Disposition", cd.ToString() );

                    using (var memory = new MemoryStream())
                    {
                        stream.CopyTo(memory);
                        result = memory.GetBuffer();

                        memory.Close();
                    }

                    stream.Close();
                }

                return new GenerateBillViewModel { FileContent = result, FileName = generatedBillFilename, BillLocation = generatedBillLocation };
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response.StatusCode);
            }
        }
        #endregion

        #region Render Coins Bill
        private GenerateBillViewModel RenderCoinsGeneratedBill(string accountNumber, string bill_date, Customer customer)
        {
            try
            {
                using (var dbContext = new eServicesDbContext())
                {
                    var billDate = Convert.ToDateTime(bill_date);

                    var linkedAccount = dbContext.LinkedAccounts.Include(l => l.Account)
                    .FirstOrDefault(
                    la => la.Account.AccountNumber == accountNumber && la.CustomerId == customer.Id);

                    if (customer.Id > 0)
                    {
                        if (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent)
                        {
                            var agentCustomers = dbContext.AgentCustomers.Where(ac => ac.Agent.CustomerId == customer.Id)
                                                 .ToList();

                            if (linkedAccount == null && agentCustomers.Any())
                            {
                                linkedAccount =
                                    new ReportController().GetAgentCustomerLinkedAccounts(customer)
                                        .FirstOrDefault(
                                            la =>
                                                la.Account.AccountNumber == accountNumber &&
                                                agentCustomers.Any(a => a.CustomerId == la.CustomerId));
                            }

                        }
                    }

                    if (_systemUserId != -1)
                    {
                        //Checks if current customer profile belongs to the system user
                        if (
                            !SecurityHelper.VerifySystemUserOwnership(_systemUserId, linkedAccount,
                                ViewCodeKeys.DownloadBills))
                        {
                            SecurityHelper.LogError(new Exception("Malicious Activity"), null);

                            var resp = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                            {
                                ReasonPhrase = "Malicious Activity"
                            };

                            throw new HttpResponseException(resp);
                        }
                    }

                    // event variables              
                    var appSettings = new AppSettingController();

                    var reportServerUrl = appSettings.GetAppSetting(AppSettingKeys.ReportServerUrl);
                    var reportPath = appSettings.GetAppSetting(AppSettingKeys.ReportPath);
                    var reportServerUsername = appSettings.GetAppSetting(AppSettingKeys.ReportServerUsername);
                    var reportServerPassword = appSettings.GetAppSetting(AppSettingKeys.ReportServerPassword);
                    var reportServerDomain = appSettings.GetAppSetting(AppSettingKeys.ReportServerDomain);
                    var generatedBillFileLocation = appSettings.GetAppSetting(AppSettingKeys.GeneratedBillFileLocation);

                    if (reportServerUrl == null || reportPath == null || reportServerUsername == null ||
                        reportServerPassword == null || reportServerDomain == null || generatedBillFileLocation == null)
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            ReasonPhrase = "Missing application settings for reporting server configuration."
                        };

                        throw new HttpResponseException(resp);
                    }

                    var generatedBillFilename = $"{accountNumber}-{billDate.ToString("yyyyMMdd")}.pdf";
                    var generatedBillLocation = $@"{HostingEnvironment.MapPath("~/" + generatedBillFileLocation.Value)}\{generatedBillFilename}";

                    var rs = new ReportExecutionService
                    {
                        Credentials = CredentialCache.DefaultCredentials,
                        Url = reportServerUrl.Value + @"/_vti_bin/ReportServer/ReportExecution2005.asmx"
                    };

                    // Render arguments
                    Warning[] warnings;
                    byte[] result;
                    const string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";
                    string encoding;
                    string mimeType;
                    string extension;
                    string[] streamIDs;

                    // Prepare report parameter.
                    var parameters = new ParameterValue[3];
                    parameters[0] = new ParameterValue
                    {
                        Name = "ACCOUNTNO",
                        Value = accountNumber
                    };
                    parameters[1] = new ParameterValue
                    {
                        Name = "BILLDATE",
                        Value = billDate.ToShortDateString()
                    };

                    new ExecutionInfo();
                    var execHeader = new ExecutionHeader();

                    rs.ExecutionHeaderValue = execHeader;
                    rs.LoadReport(reportServerUrl.Value + reportPath.Value, null);
                    rs.SetExecutionParameters(parameters, "en-us");

                    result = rs.Render("PDF", devInfo, out extension, out encoding, out mimeType, out warnings,
                        out streamIDs);
                    rs.GetExecutionInfo();

                    System.IO.File.Create(generatedBillLocation).Close();
                    System.IO.File.WriteAllBytes(generatedBillLocation, result);

                    // JK.20140624a - Included the securing of pdfs rendered. Uses the PDFsharp library, must be included in the hosting server's GAC.
                    if (!string.IsNullOrEmpty(generatedBillLocation) && System.IO.File.Exists(generatedBillLocation))
                    {
                        // Open an existing document. Providing an unrequired password is ignored.
                        var document = PdfReader.Open(generatedBillLocation, "no password");

                        var securitySettings = document.SecuritySettings;

                        // Setting one of the passwords automatically sets the security level to 
                        // PdfDocumentSecurityLevel.Encrypted128Bit.
                        securitySettings.OwnerPassword = "9ar?rqKG4v_XYFe;KhMc:cSUX5bdn73a";

                        // Restrict some rights.
                        securitySettings.PermitAccessibilityExtractContent = false;
                        securitySettings.PermitAnnotations = false;
                        securitySettings.PermitAssembleDocument = false;
                        securitySettings.PermitExtractContent = false;
                        securitySettings.PermitFormsFill = false;
                        securitySettings.PermitFullQualityPrint = false;
                        securitySettings.PermitModifyDocument = false;
                        securitySettings.PermitPrint = true;

                        // Save the document...
                        document.Save(generatedBillLocation);

                        // Get the file document for streaming to browser.
                        using (
                            var stream = System.IO.File.Open(generatedBillLocation, FileMode.Open, FileAccess.Read,
                                FileShare.Read))
                        {
                            if (mimeType == null)
                                MimeMapping.GetMimeMapping(generatedBillLocation);

                            using (var memory = new MemoryStream())
                            {
                                stream.CopyTo(memory);
                                result = memory.GetBuffer();
                                memory.Close();
                            }
                        }

                        return new GenerateBillViewModel { FileContent = result, FileName = generatedBillFilename, BillLocation = generatedBillLocation };
                    }
                }
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(ex.Response.StatusCode);
            }

            return null;
        }
        #endregion


    }
}