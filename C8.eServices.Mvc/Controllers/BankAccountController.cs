using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class BankAccountController : Controller
    {

        public BankAccountController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }

        public BankAccountController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }

        public BankAccountController(eServicesDbContext db)
        {
            UserManager =
            new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(db));
        }

        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }


        #region Report Init

        private void Initialise()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    IdentityManager = new IdentityManager(context);

                    if (User != null && User.Identity.IsAuthenticated)
                    {
                        IdentityManager.CurrentUser(User);
                        SystemUser = IdentityManager.CurrentUser(User);
                    }

                    if (SystemUser != null)
                    {
                        Customer =
                            context.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                                .Include(o => o.CustomerType)
                                .Include(o => o.Country)
                                .Include(o => o.IdentificationType)
                                .Include(o => o.TitleType)
                                .FirstOrDefault();

                        if (Customer != null)
                        {
                            Entity =
                                context.Entities.Where(o => o.CustomerId == Customer.Id)
                                    .Include(o => o.EntityType)
                                    .FirstOrDefault();
                            CustomerId = Customer.Id;
                        }

                    }

                    if (Customer != null)
                    {
                        Agent = context.Agents.FirstOrDefault(o => o.CustomerId == Customer.Id);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        #endregion

        // GET: BankAccount
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.None)]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult Index()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);
                    if (Customer == null)
                        return RedirectToAction("Index", "Profile");

                    var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
                    ViewBag.UserN = cust.FullName;
                   ViewBag.UserType = cust.CustomerTypeId;
                    TempData["AccHold"] = ViewBag.UserN;
               

                    cust.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", cust.Id));

                    ViewBag.CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).Include(o => o.BankAccountType).Include(o => o.Bank).ToList<BankAccount>();
                    ViewBag.BankAccountTypes = new SelectList(cxt.BankAccountTypes.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "Name");
                    ViewBag.Banks = new SelectList(cxt.Banks.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "Name");
                    ViewBag.UniBankCodes = new SelectList(cxt.Banks.Where(o => o.IsActive && !o.IsDeleted).ToList(), "UniBranchCode", "UniBranchCode");
                    foreach (var ba in ViewBag.CurrentBankAccounts)
                    {
                        ba.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", ba.Id)));
                    }

                    DebitOrderViewModel dovm = new DebitOrderViewModel()
                    {
                        Customer = cust,
                        CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList<BankAccount>(),
                        BankAccountTypes = cxt.BankAccountTypes.Where(o => o.IsActive == true && o.IsDeleted == false).ToList<BankAccountType>()
                    };

                    return View(dovm);
                }
                catch (Exception x)
                {

                    throw;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult Index(DebitOrderViewModel vm)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);

                    var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
                    cust.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", cust.Id));
                    var encryptedAccount = new TripleDesCrypto().Encrypt(vm.BankAccount.BankAccountNumber);
                    ViewBag.UserN = cust.FullName;
                    ViewBag.UserType = cust.CustomerTypeId;
                    DebitOrderViewModel dovm = new DebitOrderViewModel()
                    {
                        Customer = cust,
                        CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList<BankAccount>(),
                        BankAccountTypes = cxt.BankAccountTypes.Where(o => o.IsActive == true && o.IsDeleted == false).ToList<BankAccountType>()
                    };
                    var  BAccNoConfirmed= vm.BankAccount.BankAccountNumber;
                    string DuplicateDetected = "";
                    var BankResults =cxt.BankAccounts.Where(x => x.BankAccountNumber== encryptedAccount).ToList();
                    var BankResults1 = cxt.BankAccounts.Where(x => x.BankAccountNumber == encryptedAccount && x.IsAccountHolder==true && x.IsActive && !x.IsDeleted).ToList();
                    if(BankResults1.Count != 0)
                    {
                        DuplicateDetected = "Bank Account already exists";
                    }
                    if (vm.BankAccount.Id == 0 && BankResults1.Count == 0)
                    {

                        vm.BankAccount.CustomerId = cust.Id;
                        vm.BankAccount.BankAccountNumber = encryptedAccount;
                       // vm.BankAccount.IsAccountHolder = dbCheck;
                        //vm.BankAccount.AccountStatus = dbStatus;
                        cxt.BankAccounts.Add(vm.BankAccount);
                        cxt.SaveChanges();
                        TempData["Success"] = true;
                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Bank account successfully created.";
                        dovm.BankAccount = cxt.BankAccounts.SingleOrDefault(o => o.Id == vm.BankAccount.Id && o.IsActive && !o.IsDeleted);
                    }
                    else
                    {
                        var ba = cxt.BankAccounts.SingleOrDefault(o => o.Id == vm.BankAccount.Id && o.IsActive && !o.IsDeleted);
                        if (ba != null && ba.CustomerId != cust.Id)
                        {
                            TempData["Success"] = false;
                            ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Bank account already created by another user.";
                            return View(dovm);
                        }

                        if (vm.BankAccount.BankAccountNumber.Contains("*"))
                            vm.BankAccount.BankAccountNumber = ba.BankAccountNumber;

                        if (ModelState.IsValid)
                        {
                            ba.AccountHolder = vm.BankAccount.AccountHolder;
                            ba.BankAccountNumber =encryptedAccount;
                            ba.BankId = vm.BankAccount.BankId;
                            ba.BankAccountTypeId = vm.BankAccount.BankAccountTypeId;
                            ba.IsAccountHolder = vm.BankAccount.IsAccountHolder;
                            ba.AccountStatus = vm.BankAccount.AccountStatus;
                            cxt.Entry(ba).State = EntityState.Modified;
                            cxt.SaveChanges();

                        }
                        
                        TempData["Success"] = true;
                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "-"+DuplicateDetected+" - Bank account successfully updated.";
                        dovm.BankAccount = new BankAccount();
                    }

                    ViewBag.CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).Include(o => o.BankAccountType).Include(o => o.Bank).ToList<BankAccount>();
                    ViewBag.BankAccountTypes = new SelectList(cxt.BankAccountTypes.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "Name");
                    ViewBag.Banks = new SelectList(cxt.Banks.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "Name");
                    ViewBag.UniBankCodes = new SelectList(cxt.Banks.Where(o => o.IsActive && !o.IsDeleted).ToList(), "UniBranchCode", "UniBranchCode");
                    cxt.BankAccountTypes.Where(o => o.IsActive == true && o.IsDeleted == false).ToList<BankAccountType>();

                    return View(dovm);
                }
                catch (Exception x)
                {

                    throw;
                }
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadAccount(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    BankAccount ba = cxt.BankAccounts.Include(o => o.BankAccountType).Include(o => o.Bank).SingleOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);

                    var json = Json(new { data = ba }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }

                return null;
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadAccounts(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    List<BankAccount> list = cxt.BankAccounts.Include(o => o.BankAccountType).Include(o => o.Bank).Where(o => o.CustomerId == Id && o.IsActive && !o.IsDeleted).ToList();

                    foreach (var ba in list)
                    {
                        ba.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", ba.Id));
                    }

                    var json = Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }

                return null;
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteAccount(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    var ba = cxt.BankAccounts.SingleOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);
                    ba.IsDeleted = true;
                    cxt.Entry(ba).State = EntityState.Modified;
                    cxt.SaveChanges();

                    var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id && o.IsActive && !o.IsDeleted);

                    DebitOrderViewModel dovm = new DebitOrderViewModel()
                    {
                        Customer = cust,
                        CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList<BankAccount>(),
                        BankAccountTypes = cxt.BankAccountTypes.Where(o => o.IsActive == true && o.IsDeleted == false).ToList<BankAccountType>()
                    };

                    return RedirectToAction("Index", "BankAccount");
                }

                return null;
            }
        }
        private void AVSCheck(string AccHold, string AccNo, string Branch, string Type, string BusType)
            //private void AVSCheck()
        {
            var assetID = new AppSetting();
            var assetKEY = new AppSetting();
            var assetUrl = new AppSetting();
            using (var cxt1 = new eServicesDbContext())
            {
                try
                {
                    assetID = cxt1.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AVSCustomerID);
                    assetKEY = cxt1.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AVSCustomerKey);
                    assetUrl = cxt1.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AVSEndUrl);
                }
                catch (Exception e)
                {
                    if (assetID == null)
                    {
                        e = new Exception("- AVSCustomerID is empty");

                    }
                    else if (assetKEY == null)
                    {
                        e = new Exception("- AVSCustomerKey is empty");

                    }
                    else if (assetUrl == null)
                    {
                        e = new Exception("- AVSEndUrl is empty");

                    }
                    throw e;
                }
            }

            string messages = "";
            string result = string.Empty;
            WebResponse response;
            String CustomerID = assetID.Value.ToString();
            String API_KEY = assetKEY.Value.ToString();
            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(API_KEY + ":" + CustomerID));
            Encoding enc = System.Text.Encoding.GetEncoding(1252);

            using (var cxt = new eServicesDbContext())
            {

                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);


                var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
                var agent = cxt.Agents.SingleOrDefault(a => a.CustomerId == cust.Id);
                Entity custEntity = new Entity();

                if (Agent != null)
                {

                    var isEntityAgent = cxt.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);

                    if (isEntityAgent != null)
                        custEntity = cxt.Entities.Find(isEntityAgent.EntityId);
                }
                else
                {
                    custEntity = cxt.Entities.FirstOrDefault(o => o.CreatedBySystemUserId == SystemUser.Id);
                }


                var FisrtName = cust.FirstName;
                var CusInitials = "";
                var CustomerLastName = "";

                var CustomerId = "";
                // var CusInitials = FisrtName.Substring(0,1);
                string IsBus = BusType;
                if(IsBus=="true")
                {
                    Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                    string init = initials.Replace(cust.FirstName, "$1");
                    CusInitials = init;
                    CustomerLastName = custEntity.EntityName;

                    CustomerId = custEntity.EntityRegistrationNumber;
                }
                else
                {
                    Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                    string init = initials.Replace(cust.FirstName, "$1");
                    CusInitials = init;
                   CustomerLastName = cust.LastName;

 CustomerId = cust.IdentificationNumber;
                }
                
                //sandbox
                // var Branch = "250655";
                // var AccNo = "63548929003";
                // var Type ="1";
               
                  string endpoint = assetUrl.Value.ToString() + "Branch=" + Branch + "&Account=" + AccNo + "&Initials=" + CusInitials + "&Type=" + Type + "&IDNo=" + CustomerId + "&LastName=" + CustomerLastName + "&Phone=";
                //string endpoint = "https://www.imagin8.co.za/api/json/avsr/SubmitFile?"+"Branch=" + Branch + "&Account=" + AccNo + "&Initials=" + CusInitials + "&Type=" + Type + "&IDNo=" + CustomerId + "&LastName=" + CustomerLastName + "&Phone=";


                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = (sender, x509Certificate, chain, sslPolicyErrors) => true;

                    string authInfo = API_KEY + ":" + CustomerID;
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

                    WebRequest request = WebRequest.Create(endpoint);
                    request.ContentType = "application/json";
                    request.Method = "GET";
                    request.PreAuthenticate = true;
                    request.Headers["Authorization"] = "Basic " + authInfo;

                    try
                    {
                        response = (WebResponse)request.GetResponse();
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                            JObject jo = JObject.Parse(result);
                            string jobID = jo["Response"]["Transaction"]["JobID"].Value<string>();
                            //payment.Code = jo["code"].Value<string>();
                           // CheckDetails(jobID, authInfo);
                           CheckDetails(jobID, authInfo, AccHold);
                            //CheckDetails("84439", authInfo, AccHold);
                        }
                    }
                    catch (WebException ex)
                    {
                        using (var stream = ex.Response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            System.Diagnostics.Debug.WriteLine(reader.ReadToEnd());
                        }
                    }
                }
                catch (WebException ex)
                {
                    messages = ("Exception: " + ex.Message);
                }
            }
        }

        private void CheckDetails(string jobID, string authInfo , string AccHolder)
             //private void CheckDetails(string jobID, string authInfo)
        {
            var assetCheckUrl = new AppSetting();
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    assetCheckUrl = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AVSCheckUrl);
                }
                catch( Exception e)
                {
                    if(assetCheckUrl == null)
                    {
                     e = new Exception("- AVSCheckUrl is empty");
                        
                    }
                    throw e;
                }
            }
            //sandbox
            string endpoint =assetCheckUrl.Value.ToString();
            //string endpoint = "https://www.imagin8.co.za/api/json/avsr/CheckJobID?JobID=";
            WebResponse response;
            string result = string.Empty;
            string messages = "";
            using (var cxt = new eServicesDbContext())
            {

                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);
                var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
                //initialise to Failed to cater for Blank Status
                ViewBag.AccStatus = "Account Failed Verification";
                ViewBag.AccStatusCode = "Verification Failed";
                try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (sender, x509Certificate, chain, sslPolicyErrors) => true;

                

                WebRequest request = WebRequest.Create(endpoint+ jobID);
                request.ContentType = "application/json";
                request.Method = "GET";
                request.PreAuthenticate = true;
                request.Headers["Authorization"] = "Basic " + authInfo;

                try
                {
                    response = (WebResponse)request.GetResponse();
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                        JObject jo = JObject.Parse(result);
                            bool TransComRes = jo["Response"]["Transaction"]["TransactionCompleted"].Value<bool>();
                            string StatusCodeRes = jo["Response"]["Transaction"]["StatusCode"].Value<string>();
                            string BranchCodeRes = jo["Response"]["Transaction"]["BranchCode"].Value<string>();
                            string AccountNumberRes = jo["Response"]["Transaction"]["AccountNumber"].Value<string>();
                            string BankAccountTypeRes = jo["Response"]["Transaction"]["BankAccountType"].Value<string>();
                            string ClientIDRes = jo["Response"]["Transaction"]["ClientID"].Value<string>();
                            string InitialsRes = jo["Response"]["Transaction"]["Initials"].Value<string>();
                            string LastNameRes = jo["Response"]["Transaction"]["LastName"].Value<string>();
                            string AccountFoundRes = jo["Response"]["Transaction"]["AccountFound"].Value<string>();
                            string AccountOpenRes = jo["Response"]["Transaction"]["AccountOpen"].Value<string>();
                            string AccountLengthRes = jo["Response"]["Transaction"]["AccountLength"].Value<string>();
                            string AccountTypeCorrectRes = jo["Response"]["Transaction"]["AccountTypeCorrect"].Value<string>();
                            string IDNumberMatchRes = jo["Response"]["Transaction"]["IDNumberMatch"].Value<string>();
                            string InitialsMatchRes = jo["Response"]["Transaction"]["InitialsMatch"].Value<string>();
                            string NameMatchRes = jo["Response"]["Transaction"]["NameMatch"].Value<string>();
                            string AccountDebitsAllRes = jo["Response"]["Transaction"]["AccountDebitsAll"].Value<string>();
                            string AccountCreditsAllRes = jo["Response"]["Transaction"]["AccountCreditsAll"].Value<string>();
                            string AccountPhoneMatchRes = jo["Response"]["Transaction"]["AccountPhoneMatch"].Value<string>();
                            string AllPassed = jo["Response"]["Transaction"]["AllRecordsPassed"].Value<string>();
                            string TransactionsIDRes = jo["Response"]["Transaction"]["TransactionsID"].Value<string>();
                            string CheckJobIDRes = jo["Response"]["Transaction"]["JobID"].Value<string>();
                            string FirstResponseRes = jo["Response"]["Transaction"]["FirstResponse"].Value<string>();
                            var avcontext = new eServicesDbContext();
                            //ced.Logs.Add(new Log { LogEntry = ex.ToString(), LogTypeId = 1, ReferenceTypeId = 1, ReferenceId = 1 });
                            avcontext.BankChecks.Add(new BankCheck {
                                TransactionCompleted = TransComRes,
                                StatusCode = StatusCodeRes,
                                AVSBranchCode = BranchCodeRes,
                                AVSBankAccountNumber = AccountNumberRes,
                                BankAccountTypeId= BankAccountTypeRes,
                                ClientID= ClientIDRes,
                                Initials=InitialsRes,
                                LastName=LastNameRes,
                                AccountFound=AccountFoundRes,
                                AccountOpen=AccountOpenRes,
                                AccountLength=AccountLengthRes,
                                AccountTypeCorrect=AccountTypeCorrectRes,
                                IDNumberMatch=IDNumberMatchRes,
                                InitialsMatch=InitialsMatchRes,
                                AVSCliNameMatchentID=NameMatchRes,
                                AccountDebitsAll=AccountDebitsAllRes,
                                AccountCreditsAll=AccountCreditsAllRes,
                                AccountPhoneMatch=AccountPhoneMatchRes,
                                AllRecordsPassed=AllPassed,
                                TransactionsID=TransactionsIDRes,
                                JobID=CheckJobIDRes,
                                FirstResponse=FirstResponseRes
                            });
                            avcontext.SaveChanges();
                            //payment.Code = jo["code"].Value<string>();
                            //add to avs check table
                            ViewBag.IsAccHold = "True";
                            var CustomerCheck = cust.FullName;
                            //TO CHECK FOR SPECIAL CHRACTERS
                          //if(CustomerCheck==AccHolder)
                          //  {
                          //      ViewBag.IsAccHold = "True";
                          //  }
                          //  else
                          //  {
                          //      ViewBag.IsAccHold = "false";
                          //  }
                            //CheckDetails(jobID, authInfo);
                        //    ViewBag.AccStatus = "";
                        //ViewBag.AccStatusCode = AllPassed;
                        Session["AccountHoldCheck"]=ViewBag.IsAccHold[0];
                        Session["BAVSAccStatus"] = AllPassed;
                        // adding or condition to cater for typos on name so accounts dont continously fail
                        if (AllPassed=="U")
                        {
                            ViewBag.AccStatus = "Account pending verification - Please Note this takes between 1-24 hours";
                                ViewBag.AccStatusCode = "Unverified";
                        }
                        else if(AllPassed == "Y" || (AllPassed == "N" && AccountFoundRes=="Y" && AccountOpenRes=="Y"  && AccountTypeCorrectRes=="Y" && IDNumberMatchRes=="Y")) //&& AccountLengthRes=="Y" removed due to failure on certain accounts
                            {
                            ViewBag.AccStatus = "Account Verified";
                                ViewBag.AccStatusCode = "Verified";
                            }
                        else 
                        {
                            ViewBag.AccStatus = "Account Failed Verification";
                                ViewBag.AccStatusCode = "Verification Failed";
                            }
                    }
                }
                catch (WebException ex)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        System.Diagnostics.Debug.WriteLine(reader.ReadToEnd());
                    }
                }
              
            }
            catch (WebException ex)
            {
                messages = ("Exception: " + ex.Message);
                }
            }
        }
        //Acchold=' + BAccHold + 'AccNo=' + BAccNo + 'Branch=' + BBranch + 'Type=' + batype,
        public ActionResult getSub(string AccHold, string AccNo, string Branch, string Type, string BusType)
        //public ActionResult getSub(int Type1) // cascade dropdowns Cat & SubCat
        {
            string[] result = {"","","",""};
            string result1 = "";
            try
            {
               AVSCheck(AccHold, AccNo, Branch, Type, BusType);
                //AVSCheck();
                result[0] = ViewBag.AccStatus;
                result[1] = ViewBag.AccStatusCode;
                result[2] = ViewBag.IsAccHold;
                ////int Type = Convert.ToInt16(Type1);
                //var query = db.Sub_Category.Where(i => i.FK_CategoryID == Type1).OrderBy(i => i.Sub_Category1).Select(s => new { s.Sub_Category1, s.Sub_CategoryID });
                //JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                //result = javaScriptSerializer.Serialize(query);
              

            }
            catch (Exception ex)
            {

                    var ced = new eServicesDbContext();
                    ced.Logs.Add(new Log { LogEntry = ex.ToString(), LogTypeId = 1, ReferenceTypeId = 1, ReferenceId = 1 });  
                ced.SaveChanges();
                var e = ex.Message;
                result1 = e;
                result[3] = e;
                throw ex;
                
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        //ReCheck AVS
        private void ReCheckDetails(string jobID)
        //private void CheckDetails(string jobID, string authInfo)
        {
            var assetCheckUrl = new AppSetting();
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    assetCheckUrl = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AVSCheckUrl);
                }
                catch (Exception e)
                {
                    if (assetCheckUrl == null)
                    {
                        e = new Exception("- AVSCheckUrl is empty");

                    }
                    throw e;
                }
            }
            //sandbox
            string endpoint = assetCheckUrl.Value.ToString();
            //string endpoint = "https://www.imagin8.co.za/api/json/avsr/CheckJobID?JobID=";
            WebResponse response;
            string result = string.Empty;
            string messages = "";
            using (var cxt = new eServicesDbContext())
            {

                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);
                var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = (sender, x509Certificate, chain, sslPolicyErrors) => true;

                    string authInfo = "89B4036F-9908-4764-B2F4-847BC5AC8168" + ":" + "10190";
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

                    WebRequest request = WebRequest.Create(endpoint + jobID);
                    request.ContentType = "application/json";
                    request.Method = "GET";
                    request.PreAuthenticate = true;
                    request.Headers["Authorization"] = "Basic " + authInfo;

                    try
                    {
                        response = (WebResponse)request.GetResponse();
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                            JObject jo = JObject.Parse(result);
                            bool TransComRes = jo["Response"]["Transaction"]["TransactionCompleted"].Value<bool>();
                            string StatusCodeRes = jo["Response"]["Transaction"]["StatusCode"].Value<string>();
                            string BranchCodeRes = jo["Response"]["Transaction"]["BranchCode"].Value<string>();
                            string AccountNumberRes = jo["Response"]["Transaction"]["AccountNumber"].Value<string>();
                            string BankAccountTypeRes = jo["Response"]["Transaction"]["BankAccountType"].Value<string>();
                            string ClientIDRes = jo["Response"]["Transaction"]["ClientID"].Value<string>();
                            string InitialsRes = jo["Response"]["Transaction"]["Initials"].Value<string>();
                            string LastNameRes = jo["Response"]["Transaction"]["LastName"].Value<string>();
                            string AccountFoundRes = jo["Response"]["Transaction"]["AccountFound"].Value<string>();
                            string AccountOpenRes = jo["Response"]["Transaction"]["AccountOpen"].Value<string>();
                            string AccountLengthRes = jo["Response"]["Transaction"]["AccountLength"].Value<string>();
                            string AccountTypeCorrectRes = jo["Response"]["Transaction"]["AccountTypeCorrect"].Value<string>();
                            string IDNumberMatchRes = jo["Response"]["Transaction"]["IDNumberMatch"].Value<string>();
                            string InitialsMatchRes = jo["Response"]["Transaction"]["InitialsMatch"].Value<string>();
                            string NameMatchRes = jo["Response"]["Transaction"]["NameMatch"].Value<string>();
                            string AccountDebitsAllRes = jo["Response"]["Transaction"]["AccountDebitsAll"].Value<string>();
                            string AccountCreditsAllRes = jo["Response"]["Transaction"]["AccountCreditsAll"].Value<string>();
                            string AccountPhoneMatchRes = jo["Response"]["Transaction"]["AccountPhoneMatch"].Value<string>();
                            string AllPassed = jo["Response"]["Transaction"]["AllRecordsPassed"].Value<string>();
                            string TransactionsIDRes = jo["Response"]["Transaction"]["TransactionsID"].Value<string>();
                            string CheckJobIDRes = jo["Response"]["Transaction"]["JobID"].Value<string>();
                            string FirstResponseRes = jo["Response"]["Transaction"]["FirstResponse"].Value<string>();
                            var avcontext = new eServicesDbContext();
                            //ced.Logs.Add(new Log { LogEntry = ex.ToString(), LogTypeId = 1, ReferenceTypeId = 1, ReferenceId = 1 });
                            //avcontext.BankChecks.Add(new BankCheck
                            //{
                            //    TransactionCompleted = TransComRes,
                            //    StatusCode = StatusCodeRes,
                            //    AVSBranchCode = BranchCodeRes,
                            //    AVSBankAccountNumber = AccountNumberRes,
                            //    BankAccountTypeId = BankAccountTypeRes,
                            //    ClientID = ClientIDRes,
                            //    Initials = InitialsRes,
                            //    LastName = LastNameRes,
                            //    AccountFound = AccountFoundRes,
                            //    AccountOpen = AccountOpenRes,
                            //    AccountLength = AccountLengthRes,
                            //    AccountTypeCorrect = AccountTypeCorrectRes,
                            //    IDNumberMatch = IDNumberMatchRes,
                            //    InitialsMatch = InitialsMatchRes,
                            //    AVSCliNameMatchentID = NameMatchRes,
                            //    AccountDebitsAll = AccountDebitsAllRes,
                            //    AccountCreditsAll = AccountCreditsAllRes,
                            //    AccountPhoneMatch = AccountPhoneMatchRes,
                            //    AllRecordsPassed = AllPassed,
                            //    TransactionsID = TransactionsIDRes,
                            //    JobID = CheckJobIDRes,
                            //    FirstResponse = FirstResponseRes
                            //});
                            //avcontext.SaveChanges();
                            //payment.Code = jo["code"].Value<string>();
                            //add to avs check table
                            ViewBag.IsAccHold = "True";
                            var CustomerCheck = cust.FullName;
                            //TO CHECK FOR SPECIAL CHRACTERS
                            //if(CustomerCheck==AccHolder)
                            //  {
                            //      ViewBag.IsAccHold = "True";
                            //  }
                            //  else
                            //  {
                            //      ViewBag.IsAccHold = "false";
                            //  }
                            //CheckDetails(jobID, authInfo);
                            ViewBag.AccStatus = "";
                            ViewBag.AccStatusCode = AllPassed;
                            Session["AccountHoldCheck"] = ViewBag.IsAccHold[0];
                            Session["BAVSAccStatus"] = AllPassed;
                            // adding or condition to cater for typos on name so accounts dont continously fail
                            if (AllPassed == "U")
                            {
                                ViewBag.AccStatus = "Account pending verification - Please Note this takes between 1-24 hours";
                                ViewBag.AccStatusCode = "Unverified";
                            }
                            else if (AllPassed == "Y" || (AllPassed == "N" && AccountFoundRes == "Y" && AccountOpenRes == "Y" && AccountTypeCorrectRes == "Y" && IDNumberMatchRes == "Y")) //&& AccountLengthRes=="Y" removed due to failure on certain accounts
                            {
                                ViewBag.AccStatus = "Account Verified";
                                ViewBag.AccStatusCode = "Verified";
                            }
                            else
                            {
                                ViewBag.AccStatus = "Account Failed Verification";
                                ViewBag.AccStatusCode = "Verification Failed";
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        using (var stream = ex.Response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            System.Diagnostics.Debug.WriteLine(reader.ReadToEnd());
                        }
                    }

                }
                catch (WebException ex)
                {
                    messages = ("Exception: " + ex.Message);
                }
            }
        }
        public ActionResult getSubRecheck(string AccHold)
        //public ActionResult getSub(int Type1) // cascade dropdowns Cat & SubCat
        {
            string[] result = { "", "", "", "" };
            string result1 = "";
            try
            {
                ReCheckDetails("289598");
                result[0] = ViewBag.AccStatus;
                result[1] = ViewBag.AccStatusCode;
                result[2] = ViewBag.IsAccHold;
                ////int Type = Convert.ToInt16(Type1);
                //var query = db.Sub_Category.Where(i => i.FK_CategoryID == Type1).OrderBy(i => i.Sub_Category1).Select(s => new { s.Sub_Category1, s.Sub_CategoryID });
                //JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                //result = javaScriptSerializer.Serialize(query);


            }
            catch (Exception ex)
            {

                var ced = new eServicesDbContext();
                ced.Logs.Add(new Log { LogEntry = ex.ToString(), LogTypeId = 1, ReferenceTypeId = 1, ReferenceId = 1 });
                ced.SaveChanges();
                var e = ex.Message;
                result1 = e;
                result[3] = e;
                throw ex;

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}