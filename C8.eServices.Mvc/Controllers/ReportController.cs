using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Services.Protocols;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.DataAccessLayer.eBilling6Months;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Microsoft.AspNet.Identity;
using Microsoft.SqlServer.ReportExecution;
using OfficeOpenXml;
using Microsoft.AspNet.Identity.EntityFramework;
using PdfSharp.Pdf.Security;
using Document = iTextSharp.text.Document;
using PdfDocument = PdfSharp.Pdf.PdfDocument;
using PdfReader = PdfSharp.Pdf.IO.PdfReader;
using Microsoft.Reporting.WebForms;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        //eServicesDbContext _eServices = new eServicesDbContext();
        //eBilling6MonthsDbContext _eBilling6Months = new eBilling6MonthsDbContext();
        //private ValueAssistDbContext _valueAssist = new ValueAssistDbContext();
        //CesarDbContext _Cesar = new CesarDbContext();

        public ReportController()
       : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }
        public ReportController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }
        public ReportController(eServicesDbContext db)
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
        public byte[] Buffer { get; set; }

        public class IncentivePolicyStats
        {
            public int NoOfApplication { get; set; }
            public int NoOfSubmittedApplications { get; set; }
            public int NoOfInProgressApplications { get; set; }
            public int NoOfApplicationsInQuery { get; set; }
            public int NoOfSubmittedForAssessment { get; set; }
            public int NoOfApplicationApprovedAssessment { get; set; }
            public int NoOfApplicationDeclinedAssessment { get; set; }
        }

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

        #region Report Online Bills GET
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult OnlineBills()
        {
            try
            {
                Initialise();
                if (Customer == null)
                    return RedirectToAction("Index", "Profile");

                var isAgent = Customer != null && Customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent;

                ViewBag.Message = LinkedAccountHelper.LinkedAccountNotification(false);
                ViewBag.IsAgent = isAgent;
                return View();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #region Report Back Office Doc View
        // [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DocView()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        [EncryptedActionParameter]
        public ActionResult GenerateDocView()
        {
            //NetworkCredential nwc = new NetworkCredential("Ekurhuleni\nataliec", "Louella@2016");
            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            AppSetting genBillLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenBillLink);
            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value, AdDom.Value);

            WebClient client = new WebClient();
            client.Credentials = nwc;
            //string yearD = _billDate.Substring(0, 4);
            //string monthD = _billDate.Substring(5, 2);
            //string accNo = accountNumber.Replace("z", string.Empty); // to take the z away from the account number after forcing it into a string -- see LoadOnlineBils


            //string yearN = yearD;
            //string month = monthD;
            string reportURL = genBillLink.Value;
            return File(client.DownloadData(reportURL), "application/pdf");
        }

        public ActionResult EHCTerminationReport()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    // Initialize the PLMList and its billViewModels property
                    PLMList vmlist = new PLMList();
                    vmlist.billViewModels = new List<BillViewModel>();

                    var plma = cxt.PropertyLeaseApplications.Include(x => x.Status).Include(x => x.PreferredComplexArea).Include(x => x.PreferredComplexArea2).Include(x => x.HumanEHCOptions).OrderByDescending(x => x.Id).ToList();
                    var LeaseDetail = cxt.LeaseDetails.OrderByDescending(x => x.CreatedDateTime).Where(x => x.IsRenewed == false && x.TerminationNotice< DateTime.Now ).ToList() ;
                    foreach (var item in plma)
                    {
                        BillViewModel vm = new BillViewModel(); // Create a new instance of BillViewModel for each item

                        vm.PropertyLeaseApplication = item;
                        vm.LeaseDetail = cxt.LeaseDetails.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id);
                        vm.MatchedUnit = cxt.MatchedUnits.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id && x.IsAccepted == true);


                        if (vm.MatchedUnit != null && vm.LeaseDetail != null)
                        {
                            vm.ApplicationAllocatedProperty = cxt.ApplicationAllocatedProperty.OrderByDescending(x => x.CreatedDateTime).Include(x => x.OfferedComplex).FirstOrDefault(x => x.Id == vm.MatchedUnit.ApplicationAllocatedPropertyId);

                            if (vm.ApplicationAllocatedProperty != null)
                            {
                                vmlist.billViewModels.Add(vm);
                            }
                        }
                    }
                    return View(vmlist);


                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult EHCTerminationReport(int? User, DateTime? startDate, DateTime? endDate)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                PLMList vmlist = new PLMList();
                vmlist.billViewModels = new List<BillViewModel>();


                var plma = cxt.PropertyLeaseApplications.Include(x => x.Status).Include(x => x.PreferredComplexArea).Include(x => x.PreferredComplexArea2).Include(x => x.HumanEHCOptions).OrderByDescending(x => x.Id).ToList();

                foreach (var item in plma)
                {
                    BillViewModel vm = new BillViewModel(); // Create a new instance of BillViewModel for each item

                    vm.PropertyLeaseApplication = item;
                    //vm.LeaseDetail = cxt.LeaseDetails.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id);
                    //vm.MatchedUnit = cxt.MatchedUnits.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id && x.IsAccepted == true);

                    vmlist.billViewModels.Add(vm);

                }



                return View(vmlist);
            }
        }

        public ActionResult EHCApplicationReport()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    // Initialize the PLMList and its billViewModels property
                    PLMList vmlist = new PLMList();
                    vmlist.billViewModels = new List<BillViewModel>();

                    var plma = cxt.PropertyLeaseApplications.Include(x=>x.Status).Include(x=>x.PreferredComplexArea).Include(x => x.PreferredComplexArea2).Include(x => x.HumanEHCOptions).OrderByDescending(x => x.Id).ToList();

                    foreach (var item in plma)
                    {
                        BillViewModel vm = new BillViewModel(); // Create a new instance of BillViewModel for each item

                        vm.PropertyLeaseApplication = item;
                        //vm.LeaseDetail = cxt.LeaseDetails.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id);
                        //vm.MatchedUnit = cxt.MatchedUnits.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id && x.IsAccepted == true);

                        vmlist.billViewModels.Add(vm);
                   
                    }

                    return View(vmlist);


                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult EHCApplicationReport(int? User, DateTime? startDate, DateTime? endDate)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                PLMList vmlist = new PLMList();
                vmlist.billViewModels = new List<BillViewModel>();


                var plma = cxt.PropertyLeaseApplications.Include(x => x.Status).Include(x => x.PreferredComplexArea).Include(x => x.PreferredComplexArea2).Include(x => x.HumanEHCOptions).OrderByDescending(x => x.Id).ToList();

                foreach (var item in plma)
                {
                    BillViewModel vm = new BillViewModel(); // Create a new instance of BillViewModel for each item

                    vm.PropertyLeaseApplication = item;
                    //vm.LeaseDetail = cxt.LeaseDetails.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id);
                    //vm.MatchedUnit = cxt.MatchedUnits.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id && x.IsAccepted == true);

                    vmlist.billViewModels.Add(vm);

                }

        

                return View(vmlist);
            }
        }


        public ActionResult EHCNewlyAllocatedReport()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    // Initialize the PLMList and its billViewModels property
                    PLMList vmlist = new PLMList();
                    vmlist.billViewModels = new List<BillViewModel>();

                    var plma = cxt.PropertyLeaseApplications.OrderByDescending(x=>x.Id).ToList();

                    foreach (var item in plma)
                    {
                        BillViewModel vm = new BillViewModel(); // Create a new instance of BillViewModel for each item

                        vm.PropertyLeaseApplication = item;
                        vm.LeaseDetail = cxt.LeaseDetails.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id);
                        vm.MatchedUnit = cxt.MatchedUnits.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id && x.IsAccepted == true);

                       
                        if (vm.MatchedUnit != null && vm.LeaseDetail != null)
                        {
                            vm.ApplicationAllocatedProperty = cxt.ApplicationAllocatedProperty.OrderByDescending(x => x.CreatedDateTime).Include(x=>x.OfferedComplex).FirstOrDefault(x => x.Id == vm.MatchedUnit.ApplicationAllocatedPropertyId);

                            if (vm.ApplicationAllocatedProperty != null)
                            {
                                vmlist.billViewModels.Add(vm);
                            }
                        }
                    }

                    return View(vmlist);


                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
   
        [HttpPost]
        public ActionResult EHCNewlyAllocatedReport(int? User, DateTime? startDate, DateTime? endDate)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                PLMList vmlist = new PLMList();
                vmlist.billViewModels = new List<BillViewModel>();

              
                var plma = cxt.PropertyLeaseApplications
    .Where(x => x.CreatedDateTime >= startDate && x.CreatedDateTime <= endDate)
    .OrderByDescending(x => x.Id)
    .ToList();

                foreach (var item in plma)
                {
                    BillViewModel vm = new BillViewModel(); // Create a new instance of BillViewModel for each item

                    vm.PropertyLeaseApplication = item;
                    vm.LeaseDetail = cxt.LeaseDetails.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id);
                    vm.MatchedUnit = cxt.MatchedUnits.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.Id && x.IsAccepted == true);


                    if (vm.MatchedUnit != null && vm.LeaseDetail != null)
                    {
                        vm.ApplicationAllocatedProperty = cxt.ApplicationAllocatedProperty.OrderByDescending(x => x.CreatedDateTime).Include(x => x.OfferedComplex).FirstOrDefault(x => x.Id == vm.MatchedUnit.ApplicationAllocatedPropertyId);

                        if (vm.ApplicationAllocatedProperty != null)
                        {
                            vmlist.billViewModels.Add(vm);
                        }
                    }
                }

                return View(vmlist);
            }
        }

        #region Report Load Online Bills
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult LoadOnlineBills(int pageNumber, int pageSize, string searchText, int? numberOfMonths)
        {
            using (var eServicesContext = new eServicesDbContext())
            {
                Initialise();
                var billingHistories = new List<BillingHistory>();
                var isAgent = false;
                var rows = new List<object>();
                var total = 0;
                var skip = (pageNumber - 1) * pageSize;

                if (Customer == null)
                {
                    ViewBag.IsAgent = false;
                }
                else
                {
                    using (var eBilling6MonthsContext = new eBilling6MonthsDbContext())
                    {
                        try
                        {
                            var accountNumbers = string.Empty;
                            var linkedAccounts = new List<LinkedAccount>();
                            var customerBillingHistory = new List<BillingHistory>();

                            var linkedAccountsModels = eServicesContext.LinkedAccounts.Include(a => a.Account)
                                .Include(a => a.Customer)
                                .Include(a => a.Customer.Status)
                                .Where(a => a.CustomerId == Customer.Id
                                 && a.Status.Key == StatusKeys.LinkedAccountActive
                                 && a.Customer.Status.Key == StatusKeys.CustomerActive).ToList();

                            if (Customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent)
                            {
                                isAgent = true;
                                linkedAccountsModels.AddRange(GetAgentCustomerLinkedAccounts());
                            }

                            if (!string.IsNullOrEmpty(searchText))
                            {
                                linkedAccounts = linkedAccountsModels.OrderBy(l => l.Id).Where(l => l.Account.AccountNumber.Contains(searchText))
                                                 .ToList();
                            }
                            else
                            {
                                linkedAccounts = linkedAccountsModels.OrderBy(l => l.Id).ToList();
                            }

                            accountNumbers = linkedAccounts.Aggregate(accountNumbers, (current, account) => current + (account.Account.AccountNumber + ','));

                            //RMS bills
                            //billingHistories.AddRange(
                            //    eServicesContext.Database.SqlQuery<BillingHistory>(
                            //        "pr_REPORT_GetCustomerRmsBillingForArray @AccountNumbers,@CustomerId",
                            //        new SqlParameter("@AccountNumbers", accountNumbers),
                            //        new SqlParameter("@CustomerId", Customer.Id)).ToList());

                            //COINS bills
                            //billingHistories.AddRange(
                            //    eBilling6MonthsContext.Database.SqlQuery<BillingHistory>(
                            //        "pr_REPORT_BillingHistorySummaryForArray @AccountNumbers",
                            //        new SqlParameter("@AccountNumbers", accountNumbers)).ToList());
                            //List[BillingHistory] CustBillList = new List[BillingHistory];

                            var bInfoList = new List<BillInfo>();
                            using (var billcxt = new BillingDbContext())
                            {
                                foreach (var la in linkedAccounts)
                                {
                                    bInfoList.AddRange(billcxt.BillInfos.Where(x => x.AccountNumber.Trim() == la.Account.AccountNumber.ToString()).ToList());
                                }

                                foreach (var bi in bInfoList)
                                {
                                    billingHistories.Add(new BillingHistory()
                                    {
                                        AccountNumber = long.Parse(bi.AccountNumber),
                                        BillDate = bi.RunDateConvert,
                                        IdentificationNumber = bi.Id.ToString(),
                                        BalanceBroughtForward = decimal.Round(bi.TotalConvert, 2, MidpointRounding.AwayFromZero),
                                        Total = decimal.Round(bi.TotalConvert, 2, MidpointRounding.AwayFromZero)
                                    });
                                }
                            }

                            total = billingHistories.Count();

                            // Defaults 6 Months for each Account Number
                            //numberOfMonths = numberOfMonths ?? 6;
                            //billingHistories =
                            //    billingHistories.GroupBy(row => row.AccountNumber)
                            //        .SelectMany(acc => acc.OrderBy(row => row.BillDate).Take((int)numberOfMonths))
                            //        .ToList();

                            foreach (var billingHistory in billingHistories)
                            {
                                if (string.IsNullOrEmpty(billingHistory.IdentificationNumber))
                                {
                                    var linkedAccount =
                                        linkedAccounts.FirstOrDefault(
                                            la => la.Account.AccountNumber == billingHistory.AccountNumber.ToString(CultureInfo.InvariantCulture));
                                    if (null != linkedAccount)
                                        billingHistory.IdentificationNumber = linkedAccount.Customer.IdentificationNumber;
                                }
                                customerBillingHistory.Add(billingHistory);
                            }

                            billingHistories = customerBillingHistory.OrderByDescending(l => l.BillDate)
                                               .Skip(skip).Take(pageSize).ToList();

                            foreach (var history in billingHistories)
                            {
                                var date = history.BillDate.ToString("yyyy-MM-dd");
                                var amount = decimal.Round(history.Total, 2, MidpointRounding.AwayFromZero);
                                var accountNo = history.AccountNumber.ToString();
                                long accNo = 0;

                                // If a number does not come in padded.
                                if (Int64.TryParse(accountNo, out accNo))
                                {
                                    // JK.20190425 - Demo
                                    accountNo = string.Format("{0:D11}", accNo);
                                    accountNo = accNo.ToString();
                                }




                                var downloadLink =
                                    HttpUtility.UrlEncode(
                                        SecureActionLinkExtension.Encrypt(
                                            string.Format("accountNumber={0}??_billDate={1}",
                                                "z" + accountNo, date))); // add z before accountNo to force it to be a string

                                rows.Add(new
                                {
                                    //history.AccountNumber,
                                    AccountNumber = accountNo,
                                    history.IdentificationNumber,
                                    BillDate = date,
                                    BillAmount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero),
                                    DownloadLink = downloadLink,
                                    isAgent
                                });
                            }

                            ViewBag.IsAgent = isAgent;
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }

                var json = Json(new { total, rows }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = Int32.MaxValue;
                return json;
            }
        }
        #endregion
        #region Report Load Online Bills API
        public JsonResult LoadBillsList(string AccountNo, int Sysid)
        {
            using (var eServicesContext = new eServicesDbContext())
            {
                var customer = eServicesContext.Customers.FirstOrDefault(c => c.SystemUserId == Sysid);
                Initialise();
                var billingHistories = new List<BillingHistory>();
                var isAgent = false;
                var rows = new List<object>();
                var total = 0;
                var searchText = string.Empty;
                if (customer == null)
                {
                }
                else
                {
                    using (var eBilling6MonthsContext = new eBilling6MonthsDbContext())
                    {
                        try
                        {
                            var accountNumbers = string.Empty;
                            var linkedAccounts = new List<LinkedAccount>();
                            var customerBillingHistory = new List<BillingHistory>();

                            var linkedAccountsModels = eServicesContext.LinkedAccounts.Include(a => a.Account)
                                .Include(a => a.Customer)
                                .Include(a => a.Customer.Status)
                                .Where(a => a.CustomerId == customer.Id
                                 && a.Status.Key == StatusKeys.LinkedAccountActive
                                 && a.Customer.Status.Key == StatusKeys.CustomerActive
                                  && a.Account.AccountNumber == AccountNo).ToList();
                            var custtype = eServicesContext.CustomerTypes.FirstOrDefault(o => o.Key == CustomerTypeKeys.ManagingAgent);

                            if (customer.CustomerTypeId == custtype.Id)
                            {
                                isAgent = true;
                                linkedAccountsModels.AddRange(GetAgentCustomerLinkedAccounts());
                            }

                            linkedAccounts = linkedAccountsModels.OrderBy(l => l.Id).ToList();


                            accountNumbers = linkedAccounts.Aggregate(accountNumbers, (current, account) => current + (account.Account.AccountNumber + ','));

                            var bInfoList = new List<BillInfo>();
                            using (var billcxt = new BillingDbContext())
                            {
                                foreach (var la in linkedAccounts)
                                {
                                    bInfoList.AddRange(billcxt.BillInfos.Where(x => x.AccountNumber.Trim() == la.Account.AccountNumber.ToString()).ToList());
                                }

                                foreach (var bi in bInfoList)
                                {
                                    billingHistories.Add(new BillingHistory()
                                    {
                                        AccountNumber = long.Parse(bi.AccountNumber),
                                        BillDate = bi.RunDateConvert,
                                        IdentificationNumber = bi.Id.ToString(),
                                        BalanceBroughtForward = decimal.Round(bi.TotalConvert, 2, MidpointRounding.AwayFromZero),
                                        Total = decimal.Round(bi.TotalConvert, 2, MidpointRounding.AwayFromZero)
                                    });
                                }
                            }

                            total = billingHistories.Count();

                            foreach (var billingHistory in billingHistories)
                            {
                                if (string.IsNullOrEmpty(billingHistory.IdentificationNumber))
                                {
                                    var linkedAccount =
                                        linkedAccounts.FirstOrDefault(
                                            la => la.Account.AccountNumber == billingHistory.AccountNumber.ToString(CultureInfo.InvariantCulture));
                                    if (null != linkedAccount)
                                        billingHistory.IdentificationNumber = linkedAccount.Customer.IdentificationNumber;
                                }
                                customerBillingHistory.Add(billingHistory);
                            }

                            billingHistories = customerBillingHistory.OrderByDescending(l => l.BillDate).ToList();

                            foreach (var history in billingHistories)
                            {
                                var date = history.BillDate.ToString("yyyy-MM-dd");
                                var amount = decimal.Round(history.Total, 2, MidpointRounding.AwayFromZero);
                                var accountNo = history.AccountNumber.ToString();
                                long accNo = 0;

                                // If a number does not come in padded.
                                if (Int64.TryParse(accountNo, out accNo))
                                {
                                    // JK.20190425 - Demo
                                    accountNo = string.Format("{0:D11}", accNo);
                                    accountNo = accNo.ToString();
                                }




                                var downloadLink =
                                    HttpUtility.UrlEncode(
                                        SecureActionLinkExtension.Encrypt(
                                            string.Format("accountNumber={0}??_billDate={1}",
                                                "z" + accountNo, date))); // add z before accountNo to force it to be a string

                                rows.Add(new
                                {
                                    //history.AccountNumber,
                                    AccountNumber = accountNo,
                                    history.IdentificationNumber,
                                    BillDate = date,
                                    BillAmount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero),
                                    DownloadLink = downloadLink,
                                    isAgent
                                });
                            }

                            ViewBag.IsAgent = isAgent;
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }

                var json = Json(new { total, rows }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = Int32.MaxValue;
                return json;
            }
        }
        #endregion
        public List<LinkedAccount> GetAgentCustomerLinkedAccounts()
        {
            using (var context = new eServicesDbContext())
            {
                var linkedAccounts = new List<LinkedAccount>();
                var agentCustomers = context.AgentCustomers.Where(ac => ac.Agent.CustomerId == Customer.Id)
                    .ToList();

                foreach (var customer in agentCustomers)
                {
                    var customerLinkedAccounts = context.LinkedAccounts.Include(la => la.Account)
                        .Include(a => a.Customer)
                        .Include(la => la.Status)
                        .Include(la => la.Customer.Status)
                        .Where(la => la.CustomerId == customer.CustomerId
                                     && la.Status.Key == StatusKeys.LinkedAccountActive
                                     && la.Customer.Status.Key == StatusKeys.CustomerActive)
                        .ToList();

                    linkedAccounts.AddRange(customerLinkedAccounts);
                }
                return linkedAccounts;
            }
        }


        public List<LinkedAccount> GetAgentCustomerLinkedAccounts(Customer customer)
        {
            using (var context = new eServicesDbContext())
            {
                var linkedAccounts = new List<LinkedAccount>();
                var agentCustomers = context.AgentCustomers.Where(ac => ac.Agent.CustomerId == customer.Id)
                    .ToList();

                foreach (var cust in agentCustomers)
                {
                    var customerLinkedAccounts = context.LinkedAccounts.Include(la => la.Account)
                        .Include(a => a.Customer)
                        .Include(la => la.Status)
                        .Include(la => la.Customer.Status)
                        .Where(la => la.CustomerId == cust.CustomerId
                                     && la.Status.Key == StatusKeys.LinkedAccountActive
                                     && la.Customer.Status.Key == StatusKeys.CustomerActive)
                        .ToList();

                    linkedAccounts.AddRange(customerLinkedAccounts);
                }
                return linkedAccounts;
            }
        }

        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public List<BillingHistory> GetRmsGeneratedBillsForRange(string accountNumber, DateTime startDateTime, DateTime endDateTime)
        {
            using (var context = new eServicesDbContext())
            {
                var rmsBills = new List<BillingHistory>();
                try
                {
                    if (accountNumber != null)
                    {
                        var _rmsBills =
                            context.BillDirectories.AsEnumerable().Where(
                                b =>
                                    b.AccountNumber == accountNumber &&
                                    Convert.ToInt64(b.BillDate) >= long.Parse(startDateTime.ToString("yyyyMMdd")) &&
                                    Convert.ToInt64(b.BillDate) <= long.Parse(endDateTime.ToString("yyyyMMdd")))
                                .ToList();

                        if (_rmsBills.Any())
                        {
                            rmsBills.AddRange(from rmsBill in _rmsBills
                                              where System.IO.File.Exists(rmsBill.BillLocation)
                                              let billDate = DateTime.ParseExact(rmsBill.BillDate, "yyyyMMdd", CultureInfo.InvariantCulture)
                                              select new BillingHistory
                                              {
                                                  Year = billDate.Year,
                                                  BillDate = billDate,
                                                  AccountNumber = Convert.ToInt64(rmsBill.AccountNumber),
                                                  DownloadLink = rmsBill.BillLocation
                                              });
                        }
                    }

                }
                catch (Exception ex)
                {
                    SecurityHelper.LogError(ex, "Error");
                }
                return rmsBills;
            }

        }

        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public List<BillingHistory> GetRmsGeneratedBills(LinkedAccount linkedAccount)
        {
            using (var context = new eServicesDbContext())
            {
                var rmsBills = new List<BillingHistory>();
                var account = linkedAccount.Account;

                try
                {
                    if (account != null)
                    {
                        var _rmsBills = context.BillDirectories.Where(b => b.AccountNumber == account.AccountNumber).ToList();

                        if (_rmsBills.Any())
                        {
                            foreach (var rmsBill in _rmsBills)
                            {
                                if (System.IO.File.Exists(rmsBill.BillLocation))
                                {
                                    var billDate = DateTime.ParseExact(rmsBill.BillDate, "yyyyMMdd", CultureInfo.InvariantCulture);

                                    var billingHistory = new BillingHistory
                                    {
                                        Year = billDate.Year,
                                        BillDate = billDate,
                                        AccountNumber = Convert.ToInt64(rmsBill.AccountNumber),
                                        IdentificationNumber = linkedAccount.Customer.IdentificationNumber,
                                        AccountType = account.AccountType,
                                        DownloadLink = rmsBill.BillLocation
                                    };

                                    rmsBills.Add(billingHistory);
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }

                return rmsBills;
            }
        }

        public int ResendAutomatedCopyAccounts(DateTime StartDateTime, DateTime EndDateTime)
        {
            Email email = new Email();
            FileStream stream;
            var count = 0;
            using (eBilling6MonthsDbContext _eBilling6Months = new eBilling6MonthsDbContext())
            {
                var billDateAccounts =
                    _eBilling6Months.Database.SqlQuery<QueueArchive>(
                        "SELECT * FROM QueueArchive WHERE BillDate BETWEEN '" + StartDateTime + "'AND '" + EndDateTime +
                        "'").ToList();

                foreach (var billDateAccount in billDateAccounts)
                {
                    var _billArchive = _eBilling6Months.Database.SqlQuery<BillsArchive>(
                        "SELECT * FROM BillsArchive WHERE QueueId = @QueueId",
                            new SqlParameter("@QueueId", billDateAccount.Id)).FirstOrDefault();

                    // Render arguments
                    byte[] result = null;
                    string mimeType = null;

                    string generatedBillFilename = string.Format("{0}-{1}.pdf", billDateAccount.AccountNumber,
                       Convert.ToDateTime(billDateAccount.BillDate).ToString("yyyyMMdd"));
                    var remotepath = _billArchive.Path.Substring(2) + "\\" + _billArchive.Filename;
                    string fileL = @"\\10.10.9.206" + remotepath;

                    #region Get the file document for streaming to browser.
                    //using (stream = System.IO.File.Open(fileL, FileMode.Open))
                    //{
                    //    // Used to allow the user to download the file.
                    //    var cd = new System.Net.Mime.ContentDisposition()
                    //    {
                    //        FileName = generatedBillFilename,
                    //        // always prompt the user for downloading, set to true if you want 
                    //        // the browser to try to show the file inline
                    //        Inline = false
                    //    };

                    ////        memory.Close();
                    ////    }

                    ////    stream.Close();
                    ////}
                    #endregion 
                    // JK.20190630a - Changed account numebr to int to accomodate change in the EKU database.
                    GenerateBill(billDateAccount.AccountNumber.ToString(), Convert.ToDateTime(billDateAccount.BillDate).ToString("yyyy-MM-dd"), false, true);

                    email.GenerateEmail(billDateAccount.EmailAddressTo,
                        billDateAccount.EmailSubject,
                        string.Format(
                            "<html><p>Please see attached statement(s) for account <b>{0}</b></p><p>{1}</p><p>You will need Adobe Reader to open this attachment. If you do not have Adobe Reader installed please download and install from here <a href='http://get.adobe.com/reader'>http://get.adobe.com/reader</a>.</p><p>Should you have any queries relating to your statement, please send an email with your account number in the subject line and a description of the problem to <a href='mailto:jayan@xetgroup.com'>jayan@xetgroup.com</a>.</p></html>",
                            billDateAccount.AccountNumber, ""),
                        string.Format("{0}", billDateAccount.AccountNumber),
                        true,
                        AppSettingKeys.EservicesDefaultEmailTemplate,
                        ccList: "",
                        bccList: "",
                        attachment: Buffer,
                        attachmentName: generatedBillFilename);

                    count++;

                }
            }
            return count;
        }

        public ActionResult EmailBills()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult EmailBills([Bind(Include = "AccountNumber,StartDateTime,EndDateTime,EmailTo,EmailCc,Message")] EmailBillViewModel emailBillViewModel, IEnumerable<HttpPostedFileBase> files, bool isResendCopyAccount = false)
        {
            Initialise();
            var total = 0;
            if (isResendCopyAccount && null != emailBillViewModel.StartDateTime &&
                null != emailBillViewModel.EndDateTime)
            {
                try
                {
                    total = ResendAutomatedCopyAccounts((DateTime)emailBillViewModel.StartDateTime,
                        (DateTime)emailBillViewModel.EndDateTime);
                }
                catch (Exception ex)
                {
                    SecurityHelper.LogError(ex, "Error");
                }
                if (total > 0)
                {
                    ViewBag.Message = total + " Billing accounts has been resent successfully.";
                }
                else
                {
                    ViewBag.Warning = "No billing accounts found for the specified date range.";
                }

            }
            else
            {
                if (ModelState.IsValid)
                {
                    var accountType = Request.Form["radAccountType"];
                    bool IsRMS = accountType.Contains("RMS");
                    var attachments = new List<byte[]>();
                    var fileNames = new List<String>();
                    var body = string.Empty;
                    const int numberOfMonths = 12;

                    //var isAccountValid = _valueAssist.Accounts.Any(o => o.AccountNumber == emailBillViewModel.AccountNumber && o.IsRMSAccount == IsRMS);
                    Email email = new Email();

                    //if (isAccountValid)
                    //{
                    if (IsRMS)
                    {
                        foreach (HttpPostedFileBase file in files)
                        {
                            MemoryStream target = new MemoryStream();
                            //Create Font for our watermark
                            var bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);

                            file.InputStream.CopyTo(target);

                            var targetCopyAccount = AddWatermark(target.ToArray(), bf);

                            byte[] attachment = targetCopyAccount;

                            attachments.Add(attachment);

                            fileNames.Add(string.Format("{0}-{1}", emailBillViewModel.AccountNumber,
                                file.FileName));

                            body = body + file.FileName;
                        }

                        email.GenerateEmail(emailBillViewModel.EmailTo,
                            string.Format("eThekwini Metro Bills [{0}]", emailBillViewModel.AccountNumber),
                            string.Format(
                                    "<html><p>Please see attached statement(s) for account <b>{0}</b></p><p>{1}</p><p>You will need Adobe Reader to open this attachment(s). If you do not have Adobe Reader installed please download and install from here <a href='http://get.adobe.com/reader'>http://get.adobe.com/reader</a>.</p></html>",
                                emailBillViewModel.AccountNumber, emailBillViewModel.Message),
                            SystemUser.Id.ToString(),
                            true,
                            AppSettingKeys.EservicesDefaultEmailTemplate,
                            attachments: attachments,
                            fileNames: fileNames);
                        //email.GenerateEmail(emailBillViewModel.EmailTo,
                        //    string.Format("eThekwini Metro Bill: {0}", emailBillViewModel.AccountNumber),
                        //    string.Format(
                        //        "<html><p>Please see attached statement(s) for account <b>{0}</b></p><p>{1}</p><p>You will need Adobe Reader to open this attachment. If you do not have Adobe Reader installed please download and install from here <a href='http://get.adobe.com/reader'>http://get.adobe.com/reader</a>.</p><p>Should you have any queries relating to your statement, please send an email with your account number in the subject line and a description of the problem to <a href='mailto:revline@durban.gov.za'>revline@durban.gov.za</a> or contact the Call Centre on 031 324 5000.</p></html>",
                        //        emailBillViewModel.AccountNumber, emailBillViewModel.Message),
                        //    string.Format("{0}", emailBillViewModel.AccountNumber),
                        //    true,
                        //    AppSettingKeys.EservicesDefaultEmailTemplate,
                        //    ccList: emailBillViewModel.EmailCc,
                        //    attachments: attachments,
                        //    fileNames: fileNames);
                        // attachmentName: string.Format("{0}.pdf", emailBillViewModel.AccountNumber));

                        ViewBag.Message = "RMS Billing accounts have been emailed.";
                    }
                    else
                    {
                        using (eBilling6MonthsDbContext _eBilling6Months = new eBilling6MonthsDbContext())
                        {
                            var billingHistories =
                            _eBilling6Months.Database.SqlQuery<BillingHistory>("pr_REPORT_BillingHistorySummaryForRange @AccountNumber, @StartDateTime, @EndDateTime",
                            new SqlParameter("@AccountNumber", emailBillViewModel.AccountNumber),
                            new SqlParameter("@StartDateTime", emailBillViewModel.StartDateTime),
                            new SqlParameter("@EndDateTime", emailBillViewModel.EndDateTime)).ToList();

                            //Add RMS bills for range
                            var rmsGeneratedBills = GetRmsGeneratedBillsForRange(emailBillViewModel.AccountNumber,
                                (DateTime)emailBillViewModel.StartDateTime, (DateTime)emailBillViewModel.EndDateTime);

                            if (rmsGeneratedBills.Any() || billingHistories.Any())
                            {
                                //COINS
                                var coinsBills = billingHistories.GroupBy(b => b.AccountNumber)
                                    .SelectMany(
                                        acc => acc.OrderByDescending(row => row.BillDate).Take(numberOfMonths))
                                    .ToList();
                                foreach (var bill in coinsBills)
                                {
                                    // JK.20190630a - changed to accomodate varchar in EKU db.
                                    GenerateBill(emailBillViewModel.AccountNumber, bill.BillDate.ToString(), true);
                                    attachments.Add(Buffer);
                                    if (Buffer != null)
                                    {
                                        fileNames.Add(string.Format("{0}-{1}.pdf", bill.AccountNumber,
                                            bill.BillDate.ToString("yyyyMMdd")));
                                        body = body +
                                               string.Format("<li>Bill Date: {0}</li>", bill.BillDate.ToString("yyyy-MM-dd"));
                                    }

                                }
                                //RMS
                                var rmsBills = rmsGeneratedBills.GroupBy(b => b.AccountNumber)
                                    .SelectMany(
                                        acc => acc.OrderByDescending(row => row.BillDate).Take(numberOfMonths))
                                    .ToList();
                                foreach (var bill in rmsBills)
                                {
                                    GetRmsBill(bill.DownloadLink);
                                    attachments.Add(Buffer);
                                    fileNames.Add(string.Format("{0}-{1}.pdf", bill.AccountNumber,
                                        bill.BillDate.ToString("yyyyMMdd")));
                                    body = body +
                                           string.Format("<li>Bill Date: {0}</li>", bill.BillDate.ToString("yyyy-MM-dd"));
                                }

                                email.GenerateEmail(emailBillViewModel.EmailTo,
                                    string.Format("eThekwini Metro Bills [{0}]", emailBillViewModel.AccountNumber),
                                    string.Format(
                                        "<html><p>Please see attached statement for the following bill dates:<br/>" + body +
                                        "<p>You will need Adobe Reader to open this attachment. If you do not have Adobe Reader installed please download and install from here <a href='http://get.adobe.com/reader'>http://get.adobe.com/reader</a>.</p></html>",
                                        "", "", emailBillViewModel.Message),
                                    SystemUser.Id.ToString(),
                                    true,
                                    AppSettingKeys.EservicesDefaultEmailTemplate,
                                    attachments: attachments,
                                    fileNames: fileNames);

                                ViewBag.Message = "Billing accounts have been emailed.";
                            }
                            else
                            {
                                ViewBag.Warning = "No billing accounts found for the specified date range.";
                            }
                        }
                    }
                    //}
                    //else
                    //{
                    //ViewBag.Warning = "Account number not found/Incorrect account type.";
                    //}

                }
            }
            return View(emailBillViewModel);
        }

        // GET: /Report/
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        [EncryptedActionParameter]
        public ActionResult GetRmsBill(string generatedBillLocation)
        {
            string mimeType = null;
            byte[] result = { };
            //Create Font for our watermark
            var bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);

            try
            {
                // JK.20140624a - Included the securing of pdfs rendered. Uses the PDFsharp library, must be included in the hosting server's GAC.
                if (!string.IsNullOrEmpty(generatedBillLocation) && System.IO.File.Exists(generatedBillLocation))
                {
                    // Get the file document to be emailed.
                    using (var stream = System.IO.File.Open(generatedBillLocation, FileMode.Open, FileAccess.Read))
                    {
                        if (mimeType == null)
                            mimeType = MimeMapping.GetMimeMapping(generatedBillLocation);

                        using (var memory = new MemoryStream())
                        {
                            stream.CopyTo(memory);
                            result = memory.GetBuffer();
                            //If copy account, add Watermark
                            Buffer = AddWatermark(result.ToArray(), bf);
                            memory.Close();
                        }
                        //stream.Close();
                    }


                }
            }
            catch (Exception ex)
            {
                SecurityHelper.LogError(ex, "Error");
            }
            return File(result, mimeType);
        }
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum Type
        {
            WORD,
            EXCEL,
            PDF
        }
        #region ReturnValues
        public string MimeType { get; set; }
        public byte[] Content { get; set; }
        public string FileExtension
        {
            get
            {
                string extension;
                switch (ReportType)
                {
                    case Type.EXCEL:
                        extension = "xls";
                        break;

                    case Type.WORD:
                        extension = "doc";
                        break;

                    default:
                        extension = "pdf";
                        break;
                }

                return extension;
            }
        }
        #endregion
        #region InternalValues
        private string Path { get; set; }
        private string DataSetName { get; set; }
        private IEnumerable DataSetSource { get; set; }
        private Type ReportType { get; set; }
        #endregion

        public ActionResult getpdfReportORG(string cusAcc, string StateYear, string stateMonth)
        {

            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            AppSetting genBillLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenBillLink);
            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string accNo = cusAcc;
            string yearN = StateYear;
            string month = stateMonth;
            string reportURL = "http://10.1.2.212/ReportServer_SOLARERP/Pages/ReportViewer.aspx?%2fCustomer_Bill%2fCustomer_Bill_View&rs:Command=Render&rs:Format=PDF&customer=" + accNo + "&year=" + yearN + "&month=" + month;
            return File(client.DownloadData(reportURL), "application/pdf");


            //LocalReport lr = new LocalReport
            //{
            //    ReportPath = "http://10.1.2.212/ReportServer_SOLARERP?%2fCustomer_Bill%2fCustomer_Bill_View&rs:Command=Render"
            //};

            //ReportDataSource reportDataSource = new ReportDataSource(DataSetName, DataSetSource);
            //lr.DataSources.Add(reportDataSource);

            //string deviceInfo = $@"<DeviceInfo>
            //                          <OutputFormat>PDF</OutputFormat>
            //                          <PageWidth>8.5in</PageWidth>
            //                          <PageHeight>11in</PageHeight>
            //                          <MarginTop>0.5in</MarginTop>
            //                          <MarginLeft>1in</MarginLeft>
            //                          <MarginRight>1in</MarginRight>
            //                          <MarginBottom>0.5in</MarginBottom>
            //                       </DeviceInfo>";

            //string mimeType;
            //string encoding;
            //string fileNameExtension;
            //Microsoft.Reporting.WebForms.Warning[] warnings;
            //string[] streams;

            //var content = lr.Render(
            //                    ReportType.ToString(),
            //                    deviceInfo,
            //                    out mimeType,
            //                    out encoding,
            //                    out fileNameExtension,
            //                    out streams,
            //                    out warnings);

            //MimeType = mimeType;

            //return File(content, report.MimeType, $"ReportDataSet_{DateTime.Now.ToString("yyyy-MM-dd-HH'h'mm")}.{report.FileExtension}");
        }

        #region Request Access to Account Number POST
        [HttpPost]
        //[Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult getpdfReport(string cusAcc, string StateYear, string stateMonth)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {

                    AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
                    AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
                    AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
                    AppSetting genBillLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenBillLink);
                    NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value);
                    WebClient client = new WebClient();
                    client.Credentials = nwc;
                    string accNo = cusAcc;
                    string yearN = StateYear;
                    string month = stateMonth;
                    string reportURL = genBillLink.Value + accNo + "&year=" + yearN + "&month=" + month;
                    File(client.DownloadData(reportURL), "application/pdf");

                    return Json(new { isValid = true }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                }

            }

        }
        #endregion
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult GeneratePDF(string cusAcc, string StateYear, string stateMonth)
        {
            //NetworkCredential nwc = new NetworkCredential("Ekurhuleni\nataliec", "Louella@2016");
            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            AppSetting genBillLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenBillLink);
            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value, AdDom.Value);

            WebClient client = new WebClient();
            client.Credentials = nwc;
            string yearD = StateYear;
            string monthD = stateMonth;
            string accNo = cusAcc;
            string fileName = "" + yearD + "-" + monthD + "_" + accNo + "_Statement.pdf";
            string yearN = yearD;
            string month = monthD;
            string reportURL = genBillLink.Value + accNo + "&year=" + yearN + "&month=" + month;
            return File(client.DownloadData(reportURL), "application/pdf", fileName);
        }
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        [EncryptedActionParameter]
        public ActionResult GenerateBill(string accountNumber, string _billDate, bool isCopy = false, bool isResend = false)
        {
            //NetworkCredential nwc = new NetworkCredential("Ekurhuleni\nataliec", "Louella@2016");
            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            AppSetting genBillLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenBillLink);

            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value, AdDom.Value);

            WebClient client = new WebClient();
            client.Credentials = nwc;
            string yearD = _billDate.Substring(0, 4);
            string monthD = _billDate.Substring(5, 2);
            string accNo = accountNumber.Replace("z", string.Empty); // to take the z away from the account number after forcing it into a string -- see LoadOnlineBils


            string yearN = yearD;
            string month = monthD;
            string reportURL = genBillLink.Value + accNo + "&year=" + yearN + "&month=" + month;
            return File(client.DownloadData(reportURL), "application/pdf");
        }
        //// GET: /Report/
        //[Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        //[EncryptedActionParameter]
        //public ActionResult GenerateBill1(int accountNumber, string _billDate, bool isCopy = false, bool isResend = false)
        //{
        //    var _accountNumber = accountNumber.ToString();
        //    Initialise();

        //    //getpdfReport(accountNo, yearD, monthD)
        //    using (eServicesDbContext _eServices = new eServicesDbContext())
        //    {
        //        try
        //        {
        //            _eServices.Logs.Add(new Log()
        //            {
        //                LogEntry = "Reports Started...",
        //                LogTypeId = 1,
        //                ReferenceTypeId = 1,
        //                IsActive = true,
        //                IsDeleted = false
        //            });
        //            _eServices.SaveChanges();

        //            var billDate = Convert.ToDateTime(_billDate);
        //            LinkedAccount linkedAccount = null;

        //            if (!isResend)
        //            {
        //                linkedAccount =
        //                    _eServices.LinkedAccounts.Include(l => l.Account)
        //                        .FirstOrDefault(
        //                            la => la.Account.AccountNumber == _accountNumber && la.CustomerId == CustomerId);

        //                if (CustomerId > 0)
        //                {

        //                    if (Customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent)
        //                    {
        //                        var agentCustomers =
        //                            _eServices.AgentCustomers.Where(ac => ac.Agent.CustomerId == Customer.Id)
        //                                .ToList();

        //                        if (linkedAccount == null && agentCustomers.Any())
        //                        {
        //                            linkedAccount =
        //                                GetAgentCustomerLinkedAccounts()
        //                                    .FirstOrDefault(
        //                                        la =>
        //                                            la.Account.AccountNumber == _accountNumber &&
        //                                            agentCustomers.Any(a => a.CustomerId == la.CustomerId));
        //                        }

        //                    }

        //                }
        //                //linkedAccount = _eServices.LinkedAccounts.Include(a => a.Account).FirstOrDefault(a => a.Account.AccountNumber == accountNumber);

        //                if (SystemUser != null) //&& linkedAccount != null
        //                {
        //                    //Checks if current customer profile belongs to the system user
        //                    if (
        //                        !SecurityHelper.VerifySystemUserOwnership(SystemUser.Id, linkedAccount,
        //                            ViewCodeKeys.DownloadBills))
        //                    {
        //                        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
        //                        return RedirectToActionPermanent("Index", "Error");
        //                    }
        //                }
        //            }
        //            // Check if current customer has linked accounts, if false log as milicious activity
        //            //if (linkedAccount == null)
        //            //{
        //            //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
        //            //    return RedirectToActionPermanent("Index", "Error");
        //            //}

        //            // event varaibles                
        //            bool fireAgain = false;
        //            string format = "PDF";
        //            var appSettings = new AppSettingController();

        //            var reportServerUrl = appSettings.GetAppSetting(AppSettingKeys.ReportServerUrl);
        //            var reportPath = appSettings.GetAppSetting(AppSettingKeys.ReportPath);
        //            var reportServerUsername = appSettings.GetAppSetting(AppSettingKeys.ReportServerUsername);
        //            var reportServerPassword = appSettings.GetAppSetting(AppSettingKeys.ReportServerPassword);
        //            var reportServerDomain = appSettings.GetAppSetting(AppSettingKeys.ReportServerDomain);
        //            var generatedBillFileLocation = appSettings.GetAppSetting(AppSettingKeys.GeneratedBillFileLocation);
        //            const bool useIntegratedAuthentication = false;

        //            if (reportServerUrl == null || reportPath == null || reportServerUsername == null || reportServerPassword == null || reportServerDomain == null || generatedBillFileLocation == null)
        //                throw new Exception("Error: Missing application settings for reporting server configuration.");

        //            string generatedBillFilename = string.Format("{0}-{1}.pdf", _accountNumber, billDate.ToString("yyyyMMdd"));
        //            string generatedBillLocation = string.Format(@"{0}\{1}", HostingEnvironment.MapPath("~/" + generatedBillFileLocation.Value), generatedBillFilename);
        //            //string generatedBillLocation = string.Format(@"{0}\{1}", Server.MapPath(Url.Content("~/" + generatedBillFileLocation.Value)), generatedBillFilename);

        //            ReportExecutionService rs = new ReportExecutionService();

        //            if (useIntegratedAuthentication)
        //                rs.Credentials = CredentialCache.DefaultCredentials;
        //            else
        //            {
        //                CredentialCache myCache = new CredentialCache();
        //                myCache.Add(new Uri(reportServerUrl.Value), "Basic",
        //                    new NetworkCredential(reportServerUsername.Value, CreateSecurePassword(reportServerPassword.Value),
        //                        reportServerDomain.Value));
        //                rs.Credentials = myCache;
        //            }

        //            rs.Url = reportServerUrl.Value + @"/ReportExecution2005.asmx";

        //            // Render arguments
        //            Warning[] warnings = null;
        //            byte[] result = null;
        //            string historyId = null;
        //            string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";
        //            string encoding;
        //            string mimeType;
        //            string extension;
        //            string[] streamIDs = null;
        //            //Create Font for our watermark
        //            var bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);

        //            // Prepare report parameter.
        //            ParameterValue[] parameters = new ParameterValue[3];
        //            parameters[0] = new ParameterValue();
        //            parameters[0].Name = "customer";
        //            parameters[0].Value = _accountNumber.ToString();
        //            parameters[1] = new ParameterValue();
        //            parameters[1].Name = "year";
        //            parameters[1].Value = billDate.Year.ToString();
        //            parameters[2] = new ParameterValue();
        //            parameters[2].Name = "month";
        //            parameters[2].Value = billDate.Month.ToString();

        //            _eServices.Logs.Add(new Log()
        //            {
        //                LogEntry = string.Format("{0}, {1}, {2}", _accountNumber, billDate.Year, billDate.Month),
        //                LogTypeId = 1,
        //                ReferenceTypeId = 1,
        //                IsActive = true,
        //                IsDeleted = false
        //            });
        //            _eServices.SaveChanges();

        //            ExecutionInfo execInfo = new ExecutionInfo();
        //            ExecutionHeader execHeader = new ExecutionHeader();

        //            rs.Timeout = 300000;
        //            rs.ExecutionHeaderValue = execHeader;
        //            execInfo = rs.LoadReport(reportServerUrl.Value + reportPath.Value, historyId);
        //            rs.SetExecutionParameters(parameters, "en-us");
        //            String SessionId = rs.ExecutionHeaderValue.ExecutionID;

        //            try
        //            {
        //                result = rs.Render(format, devInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);
        //                execInfo = rs.GetExecutionInfo();

        //                // Clear contents in GeneratedBills folder
        //                //ClearGeneratedBills(string.Format(@"{0}", Server.MapPath(Url.Content("~/" + generatedBillFileLocation.Value))));

        //                // Write the contents of the report to an MHTML file.
        //                try
        //                {
        //                    //using (var stream = System.IO.File.Create(generatedBillLocation, result.Length))
        //                    //{
        //                    //    stream.Write(result, 0, result.Length);
        //                    //    stream.Close();
        //                    //}

        //                    System.IO.File.Create(generatedBillLocation).Close();
        //                    System.IO.File.WriteAllBytes(generatedBillLocation, result);

        //                    // JK.20140624a - Included the securing of pdfs rendered. Uses the PDFsharp library, must be included in the hosting server's GAC.
        //                    if (!string.IsNullOrEmpty(generatedBillLocation) && System.IO.File.Exists(generatedBillLocation))
        //                    {
        //                        // Open an existing document. Providing an unrequired password is ignored.
        //                        PdfDocument document = PdfReader.Open(generatedBillLocation, "no password");

        //                        PdfSecuritySettings securitySettings = document.SecuritySettings;

        //                        // Setting one of the passwords automatically sets the security level to 
        //                        // PdfDocumentSecurityLevel.Encrypted128Bit.
        //                        securitySettings.OwnerPassword = "9ar?rqKG4v_XYFe;KhMc:cSUX5bdn73a";

        //                        // Restrict some rights.
        //                        securitySettings.PermitAccessibilityExtractContent = false;
        //                        securitySettings.PermitAnnotations = false;
        //                        securitySettings.PermitAssembleDocument = false;
        //                        securitySettings.PermitExtractContent = false;
        //                        securitySettings.PermitFormsFill = false;
        //                        securitySettings.PermitFullQualityPrint = false;
        //                        securitySettings.PermitModifyDocument = false;
        //                        securitySettings.PermitPrint = true;

        //                        // Save the document...
        //                        document.Save(generatedBillLocation);

        //                        // Get the file document for streaming to browser.
        //                        using (var stream = System.IO.File.Open(generatedBillLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
        //                        {
        //                            // Used to allow the user to download the file.
        //                            var cd = new ContentDisposition()
        //                            {
        //                                FileName = generatedBillFilename,
        //                                // always prompt the user for downloading, set to true if you want 
        //                                // the browser to try to show the file inline
        //                                Inline = false
        //                            };

        //                            if (mimeType == null)
        //                                mimeType = MimeMapping.GetMimeMapping(generatedBillLocation);
        //                            //Response.AppendHeader( "Content-Disposition", cd.ToString() );

        //                            using (var memory = new MemoryStream())
        //                            {
        //                                stream.CopyTo(memory);
        //                                result = memory.GetBuffer();
        //                                //If copy account, add Watermark
        //                                Buffer = isCopy ? AddWatermark(result.ToArray(), bf) : result;
        //                                memory.Close();
        //                            }
        //                            //stream.Close();
        //                        }

        //                        _eServices.Logs.Add(new Log()
        //                        {
        //                            LogEntry = string.Format("{0}", "Reporting Stopping..."),
        //                            LogTypeId = 1,
        //                            ReferenceTypeId = 1,
        //                            IsActive = true,
        //                            IsDeleted = false
        //                        });
        //                        _eServices.SaveChanges();

        //                        return File(result, mimeType, generatedBillFilename);
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    _eServices.Logs.Add(new Log()
        //                    {
        //                        LogEntry = string.Format("{0}", e.ToString()),
        //                        LogTypeId = 1,
        //                        ReferenceTypeId = 1,
        //                        IsActive = true,
        //                        IsDeleted = false
        //                    });
        //                    _eServices.SaveChanges();
        //                    throw e;
        //                }

        //            }
        //            catch (SoapException e)
        //            {
        //                _eServices.Logs.Add(new Log()
        //                {
        //                    LogEntry = string.Format("{0}", e.ToString()),
        //                    LogTypeId = 1,
        //                    ReferenceTypeId = 1,
        //                    IsActive = true,
        //                    IsDeleted = false
        //                });
        //                _eServices.SaveChanges();
        //                throw e;
        //            }


        //            return null;
        //        }
        //        catch (Exception e)
        //        {
        //            _eServices.Logs.Add(new Log()
        //            {
        //                LogEntry = string.Format("{0}", e.ToString()),
        //                LogTypeId = 1,
        //                ReferenceTypeId = 1,
        //                IsActive = true,
        //                IsDeleted = false
        //            });
        //            _eServices.SaveChanges();
        //            throw e;
        //        }
        //    }
        //}

        /// <summary>
        /// Get RMS generated bills from drop off location.
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="billDate"></param>
        /// <param name="generatedBillLocation"></param>
        /// <returns></returns>
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult RenderRmsGeneratedBill(string accountNumber, string _billDate, string generatedBillLocation)
        {
            //Initialise();

            //using (eServicesDbContext _eServices = new eServicesDbContext())
            //{
            //    //If bill location is null or empty, then is COINS bill => Generate bill from eBilling
            //    // JK.20190630a - changed the parameters to accomodate the EKU db.
            //    if (string.IsNullOrEmpty(generatedBillLocation)) return GenerateBill(int.Parse(accountNumber), _billDate);

            //    //FileStream stream;
            //    string format = "PDF";
            //    var billDate = Convert.ToDateTime(_billDate);

            //    LinkedAccount linkedAccount = null;
            //    linkedAccount =
            //        _eServices.LinkedAccounts.Include(l => l.Account)
            //            .FirstOrDefault(la => la.Account.AccountNumber == accountNumber && la.CustomerId == CustomerId);

            //    if (Customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent && linkedAccount == null)
            //    {
            //        var agentCustomers = _eServices.AgentCustomers.Where(ac => ac.Agent.CustomerId == Customer.Id)
            //        .ToList();

            //        if (agentCustomers.Any())
            //        {
            //            linkedAccount =
            //            GetAgentCustomerLinkedAccounts()
            //                .FirstOrDefault(la => la.Account.AccountNumber == accountNumber && agentCustomers.Any(a => a.CustomerId == la.CustomerId));
            //        }
            //    }

            //    if (SystemUser != null && linkedAccount != null)
            //    {
            //        //Checks if current customer profile belongs to the system user
            //        if (!SecurityHelper.VerifySystemUserOwnership(SystemUser.Id, linkedAccount, ViewCodeKeys.DownloadBills))
            //        {
            //            SecurityHelper.LogError(new Exception("Malicious Activity"), null);
            //            return RedirectToAction("Index", "Error");
            //        }
            //    }

            //    // JK.20190619a - Place this check back for security.
            //    // Check if current customer has linked accounts, if false log as milicious activity
            //    //if (linkedAccount == null)
            //    //{
            //    //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
            //    //    return RedirectToActionPermanent("Index", "Error");
            //    //}

            //    // Render arguments
            //    byte[] result = null;
            //    string mimeType = null;

            //    string generatedBillFilename = string.Format("{0}-{1}.pdf", accountNumber, billDate.ToString("yyyyMMdd"));

            //    // Get the file document for streaming to browser.
            //    using (var stream = System.IO.File.Open(@generatedBillLocation, FileMode.Open))
            //    {
            //        // Used to allow the user to download the file.
            //        var cd = new ContentDisposition()
            //        {
            //            FileName = generatedBillFilename,
            //            // always prompt the user for downloading, set to true if you want 
            //            // the browser to try to show the file inline
            //            Inline = false
            //        };

            //        if (mimeType == null)
            //            mimeType = MimeMapping.GetMimeMapping(generatedBillLocation);

            //        Response.ContentType = "application/pdf";
            //        Response.AppendHeader("Content-Disposition", cd.ToString());

            //        using (var memory = new MemoryStream())
            //        {
            //            stream.CopyTo(memory);
            //            result = memory.GetBuffer();
            //            Buffer = result;

            //            memory.Close();
            //        }

            //        stream.Close();
            //    }

            //    return File(result, mimeType, generatedBillFilename);

            //}
            return null;
        }

        private void ClearGeneratedBills(string locationPath)
        {
            DirectoryInfo directory = new DirectoryInfo(locationPath);

            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
        }

        private SecureString CreateSecurePassword(string pwd)
        {
            SecureString s = new SecureString();
            foreach (char c in pwd)
            {
                s.AppendChar(c);
            }
            return s;
        }

        /// <summary>
        /// L.M.20170320- Adds WaterMark to a PDF document
        /// </summary>
        /// <param name="bytes">PDF Doc in bytes</param>
        /// <param name="bf">Font </param>
        /// <returns></returns>
        private static byte[] AddWatermark(byte[] bytes, BaseFont bf)
        {
            try
            {
                using (var _context = new eServicesDbContext())
                {
                    var watermark =
                        _context.AppSettings.FirstOrDefault(app => app.Key == AppSettingKeys.CopyAccountWatermark && app.IsActive && !app.IsDeleted);
                    if (null == watermark)
                        throw new Exception(string.Format("Invalid/ missing application setting key: {0}", AppSettingKeys.CopyAccountWatermark));

                    using (var ms = new MemoryStream(10 * 1024))
                    {
                        var reader = new iTextSharp.text.pdf.PdfReader(bytes);
                        iTextSharp.text.pdf.PdfReader.unethicalreading = true;
                        using (var stamper = new PdfStamper(reader, ms))
                        {
                            int times = reader.NumberOfPages;
                            for (int i = 1; i <= times; i++)
                            {
                                var dc = stamper.GetOverContent(i);
                                PdfHelper.AddWaterMark(dc, watermark.Value, bf, 48, 35, new BaseColor(70, 70, 255), reader.GetPageSizeWithRotation(i));
                            }
                            stamper.Close();
                        }
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.SecurityHelper.LogError(ex, "Error");
            }
            return null;
        }


        /// <summary>
        /// LM.20180111 - Exports partial view 
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="title">Report title</param>
        /// <param name="reportKey">The report Key</param>
        /// <param name="format">The format type</param>
        /// <param name="startDate">The Start Date</param>
        /// <param name="endDate">The End Date</param>
        /// <param name="referenceId"></param>
        /// <returns>Document</returns>
        public ActionResult AsMsOffice(string searchText, string title, string reportKey, string format, string startDate, string endDate, int referenceId = 0)
        {
            try
            {
                var partialAsString = "";
                switch (reportKey)
                {
                    case ReportTypeKeys.CustomerProfile:
                        {
                            var customerProfiles = GetCustomerProfiles(searchText, referenceId, startDate, endDate);

                            #region Print

                            if (!string.IsNullOrEmpty(format))
                            {
                                partialAsString = RenderPartialViewToString(title, "_CustomerProfilePartial",
                                    customerProfiles);
                                var data = partialAsString;
                                if (format == "PDF")
                                {
                                    using (MemoryStream stream = new System.IO.MemoryStream())
                                    {
                                        StringReader sr = new StringReader(partialAsString);
                                        Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 10f);
                                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                                        pdfDoc.Open();
                                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                                        pdfDoc.Close();
                                        return File(stream.ToArray(), "application/pdf",
                                            title + DateTime.Now.ToString("MMddyyyy_HHmmss") + ".pdf");
                                    }
                                }
                                else
                                {
                                    data = HttpUtility.UrlDecode(data);
                                    BuildCurrentResponse(format, data, title);
                                }
                            }

                            #endregion
                        }
                        break;
                    case ReportTypeKeys.CustomerLinkedAccount:
                        {
                            var linkedAccounts = GetCustomerLinkedAccounts(searchText, referenceId, startDate, endDate);

                            #region Print

                            if (!string.IsNullOrEmpty(format))
                            {
                                partialAsString = RenderPartialViewToString(title, "_CustomerLinkedAccountPartial",
                                    linkedAccounts);
                                var data = partialAsString;
                                if (format == "PDF")
                                {
                                    using (MemoryStream stream = new System.IO.MemoryStream())
                                    {
                                        StringReader sr = new StringReader(partialAsString);
                                        Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 10f);
                                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                                        pdfDoc.Open();
                                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                                        pdfDoc.Close();
                                        return File(stream.ToArray(), "application/pdf",
                                            title + DateTime.Now.ToString("MMddyyyy_HHmmss") + ".pdf");
                                    }
                                }
                                else
                                {
                                    data = HttpUtility.UrlDecode(data);
                                    BuildCurrentResponse(format, data, title);
                                }

                                #endregion
                            }

                        }
                        break;
                    case ReportTypeKeys.CustomerQuery:
                        {
                            var queries = LoadCustomerQueries(searchText, referenceId, startDate, endDate);

                            #region Print

                            if (!string.IsNullOrEmpty(format))
                            {
                                partialAsString = RenderPartialViewToString(title, "_CustomerQueryPartial",
                                    queries);
                                var data = partialAsString;
                                if (format == "PDF")
                                {
                                    using (MemoryStream stream = new System.IO.MemoryStream())
                                    {
                                        StringReader sr = new StringReader(partialAsString);
                                        Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 10f);
                                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                                        pdfDoc.Open();
                                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                                        pdfDoc.Close();
                                        return File(stream.ToArray(), "application/pdf",
                                            title + DateTime.Now.ToString("MMddyyyy_HHmmss") + ".pdf");
                                    }
                                }
                                else
                                {
                                    data = HttpUtility.UrlDecode(data);
                                    BuildCurrentResponse(format, data, title);
                                }

                                #endregion
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                SecurityHelper.LogError(ex, "Error");
            }

            return null;
        }

        /// <summary>
        /// Load customer queries
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="queryStatus"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<Query> LoadCustomerQueries(string searchText, int? queryStatus, string startDate, string endDate)
        {
            var customerQueries = new List<Query>();
            var db = new eServicesDbContext();

            const string goLiveDate = "2019/07/01";
            var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            try
            {
                var statusResolved = db.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerQueryResolved && s.IsActive && !s.IsDeleted);
                if (null == statusResolved) throw new Exception(string.Format("Missing/ invalid status key -{0}", StatusKeys.CustomerQueryResolved));

                var queryModel =
                    db.Queries.Include(s => s.Status)
                        .Where(q => !(bool)q.IsLocked && !q.IsDeleted &&
                                    (DbFunctions.TruncateTime(q.CreatedDateTime) >= startDateTime &&
                                     DbFunctions.TruncateTime(q.CreatedDateTime) <= endDateTime))
                        .OrderByDescending(q => q.Id).ToList();

                if (queryStatus != null)
                {
                    var statusFilter = Convert.ToInt32(queryStatus);
                    queryModel = queryModel.Where(q => q.StatusId == statusFilter).ToList();
                }

                int parsedId;
                int.TryParse(searchText, out parsedId);

                if (!string.IsNullOrEmpty(searchText))
                {
                    customerQueries = queryModel.Where(c => (c.FullName ?? "").Contains(searchText)
                                                    || (c.ReferenceNumber ?? "").Contains(searchText)
                                                    || (c.Email ?? "").Contains(searchText)
                                                    || (c.Comment ?? "").Contains(searchText)
                                                    || (c.ResolutionComment ?? "").Contains(searchText)
                                                    || (c.Id == parsedId)).ToList();
                }
                else
                {
                    customerQueries = queryModel.ToList();
                }
            }
            catch (Exception ex)
            {
                SecurityHelper.LogError(ex, "Error");
            }
            return customerQueries;
        }
        /// <summary>
        /// Load customers
        /// </summary>
        /// <param name="searchText">Search String</param>
        /// <param name="referenceId">The Customer Status Type</param>
        /// <returns></returns>
        public List<Customer> GetCustomerProfiles(string searchText, int referenceId, string startDate, string endDate)
        {
            var customers = new List<Customer>();
            var rows = new List<object>();
            using (var context = new eServicesDbContext())
            {
                const string goLiveDate = "2019/07/01";
                var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
                var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);

                try
                {
                    var customerModels = context.Customers.Include(c => c.CustomerType).Include(c => c.Status)
                        .Include(c => c.SystemUser)
                        .Where(c => c.SystemUserId != null
                                    && c.IsActive && !c.IsDeleted &&
                                    DbFunctions.TruncateTime(c.CreatedDateTime) >= startDateTime &&
                                    DbFunctions.TruncateTime(c.CreatedDateTime) <= endDateTime);

                    var profilePending = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                    var profileActive = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);

                    if (profilePending == null) throw new Exception("Invalid status");
                    if (profileActive == null) throw new Exception("Invalid status");
                    customerModels = referenceId == 0
                        ? customerModels.Where(c => c.StatusId == profilePending.Id)
                        : customerModels.Where(c => c.StatusId == profileActive.Id);

                    if (searchText != "###" && !string.IsNullOrEmpty(searchText))
                    {
                        customers = customerModels.GroupJoin(
                            context.Entities,
                            customer => customer.Id,
                            entity => entity.CustomerId,
                            (customer, entity) => new { Customer = customer, Entity = entity })
                            .SelectMany(
                                x => x.Entity.DefaultIfEmpty(),
                                (x, y) => new { Customer = x.Customer, Entity = y })
                            .Where(ce => (ce.Customer.FirstName ?? "").Contains(searchText)
                                         || (ce.Customer.LastName ?? "").Contains(searchText)
                                         || (ce.Customer.IdentificationNumber ?? "").Contains(searchText)
                                         || (ce.Customer.EmailAddress ?? "").Contains(searchText)
                                         || (ce.Customer.CustomerType.Name ?? "").Contains(searchText)
                                         || (ce.Entity.EntityRegistrationNumber ?? "").Contains(searchText))
                            .Select(c => c.Customer).Include(c => c.SystemUser)
                            .Include(c => c.Status).Include(c => c.CustomerType)
                            .ToList();
                    }
                    else
                    {
                        customers = customerModels.ToList();
                    }
                    foreach (var customer in customers)
                    {
                        if (customer.SystemUser == null) throw new Exception("Invalid System user");
                        var isEmailConfirmed = UserManager.FindByName(customer.SystemUser.UserName).EmailConfirmed;
                        var status = (isEmailConfirmed) ? "Active" : "Pending";
                        var identificationNumber = "";

                        switch (customer.CustomerType.Key)
                        {
                            case CustomerTypeKeys.Entity:
                                var entity = context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id);
                                if (entity != null)
                                    identificationNumber = entity.EntityRegistrationNumber;
                                break;
                            case CustomerTypeKeys.ManagingAgent:
                                var agent = context.Agents.FirstOrDefault(a => a.CustomerId == customer.Id);

                                if (agent != null)
                                {
                                    var entityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);

                                    identificationNumber = (entityAgent != null)
                                        ? context.Entities.Find(entityAgent.EntityId).EntityRegistrationNumber
                                        : context.Entities.First(e => e.CustomerId == agent.CustomerId).EntityRegistrationNumber;
                                }
                                break;
                            default:
                                identificationNumber = customer.IdentificationNumber;
                                break;
                        }
                        customer.IdentificationNumber = identificationNumber;

                        //rows.Add(new
                        //{
                        //    FullName = customer.FirstName + " " + customer.LastName,
                        //    IDNumber = identificationNumber,
                        //    EmailAddress = customer.SystemUser.EmailAddress,
                        //    CustomerType = customer.CustomerType.Name,
                        //    ProfileStatus = customer.Status.Name,
                        //    AccountStatus = status,
                        //});
                    }
                }
                catch (Exception ex)
                {
                    SecurityHelper.LogError(ex, "Error");
                }
            }
            return customers;
        }

        /// <summary>
        /// Load Customer LinkedAccounts
        /// </summary>
        /// <param name="searchText">Search String</param>
        /// <param name="referenceId">The Linked Account Status Type</param>
        /// <param name="startDate">Start Date </param>
        /// <param name="endDate">End Date</param>
        /// <returns></returns>
        public List<LinkedAccount> GetCustomerLinkedAccounts(string searchText, int referenceId, string startDate, string endDate)
        {
            var customerLinkedAcounts = new List<LinkedAccount>();
            try
            {
                using (var eServicesDbContext = new eServicesDbContext())
                {
                    const string goLiveDate = "2019/07/01";
                    var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
                    var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);

                    var linkedAccountPending = eServicesDbContext.Status.FirstOrDefault(s => s.Key == StatusKeys.LinkedAccountPending);
                    var documentsPending = eServicesDbContext.Status.FirstOrDefault(s => s.Key == StatusKeys.DocumentPending);
                    var linkedAccountActive = eServicesDbContext.Status.FirstOrDefault(s => s.Key == StatusKeys.LinkedAccountActive);
                    var linkedEmailPendingActivation = eServicesDbContext.Status.FirstOrDefault(s => s.Key == StatusKeys.PendingEmailVerification);

                    if (linkedAccountPending == null) throw new Exception("Invalid status");
                    if (documentsPending == null) throw new Exception("Invalid status");
                    if (linkedAccountActive == null) throw new Exception("Invalid status");
                    if (linkedEmailPendingActivation == null) throw new Exception("Invalid status");

                    var linkedAccountModels = eServicesDbContext.LinkedAccounts.Include(l => l.Account)
                        .Include(l => l.Customer)
                        .Include(l => l.Customer.CustomerType)
                        .Where(
                            l =>
                                l.IsActive && !l.IsDeleted &&
                                (DbFunctions.TruncateTime(l.CreatedDateTime) >= startDateTime && DbFunctions.TruncateTime(l.CreatedDateTime) <= endDateTime));

                    switch (referenceId)
                    {
                        case 0:
                            linkedAccountModels = linkedAccountModels.Where(l => l.StatusId == linkedAccountPending.Id);
                            break;
                        case 1:
                            linkedAccountModels = linkedAccountModels.Where(l => l.StatusId == documentsPending.Id);
                            break;
                        case 2:
                            linkedAccountModels = (from linkedaccount in linkedAccountModels
                                                   join linkedEmail in eServicesDbContext.LinkedEmails
                                                  on linkedaccount.Id equals linkedEmail.LinkedAccountId
                                                   where linkedEmail.Status.Key == linkedEmailPendingActivation.Key
                                                   select linkedaccount).Include(l => l.Customer).Include(l => l.Account)
                                              .Include(l => l.Customer.CustomerType);
                            break;
                        default:
                            linkedAccountModels = linkedAccountModels.Where(l => l.StatusId == linkedAccountActive.Id);
                            break;
                    }

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        customerLinkedAcounts = (from linkedaccount in linkedAccountModels
                                                 join customer in eServicesDbContext.Customers
                                                     on linkedaccount.CustomerId equals customer.Id
                                                     into custacc
                                                 from y in custacc.DefaultIfEmpty()
                                                 join entity in eServicesDbContext.Entities
                                                     on linkedaccount.CustomerId equals entity.CustomerId
                                                     into custEntity
                                                 from x in custEntity.DefaultIfEmpty()
                                                 where (y.IdentificationNumber ?? "").Contains(searchText) ||
                                                       (x.EntityRegistrationNumber ?? "").Contains(searchText) ||
                                                       (linkedaccount.Account.AccountNumber ?? "").Contains(searchText)
                                                 select linkedaccount).Include(l => l.Customer.CustomerType)
                                          .Include(l => l.Account).ToList();
                    }
                    else
                    {
                        customerLinkedAcounts = linkedAccountModels.ToList();
                    }

                    foreach (var linkedaccount in customerLinkedAcounts)
                    {
                        var identificationNumber = "";

                        switch (linkedaccount.Customer.CustomerType.Key)
                        {
                            case CustomerTypeKeys.Entity:
                                var entity = eServicesDbContext.Entities.FirstOrDefault(e => e.CustomerId == linkedaccount.CustomerId);
                                if (entity != null)
                                    identificationNumber = entity.EntityRegistrationNumber;
                                break;
                            case CustomerTypeKeys.ManagingAgent:
                                var agent = eServicesDbContext.Agents.FirstOrDefault(a => a.CustomerId == linkedaccount.CustomerId);

                                if (agent != null)
                                {
                                    var entityAgent = eServicesDbContext.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);

                                    identificationNumber = (entityAgent != null)
                                        ? eServicesDbContext.Entities.Find(entityAgent.EntityId).EntityRegistrationNumber
                                        : eServicesDbContext.Entities.First(e => e.CustomerId == agent.CustomerId).EntityRegistrationNumber;
                                }
                                break;
                            default:
                                identificationNumber = linkedaccount.Customer.IdentificationNumber;
                                break;
                        }
                        linkedaccount.Customer.IdentificationNumber = identificationNumber;
                    }
                }

            }
            catch (Exception ex)
            {
                SecurityHelper.LogError(ex, "Error");
            }
            return customerLinkedAcounts;
        }
        /// <summary>
        /// LM.20150708a - Adds response header, specify content type 
        ///               and writes data for output  
        /// </summary>
        /// <param name="format">output format</param>
        /// <param name="data"> data to be exported</param>
        /// <param name="filename">predifined filename</param>
        public void BuildCurrentResponse(string format, string data, string filename)
        {
            try
            {
                System.Web.HttpContext.Current.Response.Clear();
                if (format == "Excel")
                {
                    System.Web.HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=" + filename + DateTime.Now.ToString("MMddyyyy_HHmmss") + ".xls");
                    System.Web.HttpContext.Current.Response.Charset = "";
                    System.Web.HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
                }
                else
                {
                    System.Web.HttpContext.Current.Response.AddHeader("content-disposition",
                        "attachment;filename=" + filename + DateTime.Now.ToString("MMddyyyy_HHmmss") + ".doc");
                    System.Web.HttpContext.Current.Response.Charset = "";
                    System.Web.HttpContext.Current.Response.ContentType = "application/msword";
                }

                string style = @"<style> td { mso-number-format:\@; } table { border:solid 0.1pt #CCCCCC; } tr { border:solid 0.1pt #CCCCCC; } th { border:solid 0.1pt #CCCCCC; } </style> ";
                // Style is added dynamically
                Response.Write(style);
                System.Web.HttpContext.Current.Response.Write(data);
                System.Web.HttpContext.Current.Response.Flush();
                System.Web.HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// LM.20150708 - Converts dynamic partial view, 
        ///               uses action to search for view if viewname is not specified
        /// </summary>
        /// <param name="reportTitle">tile of the report</param>
        /// <param name="viewName">name of partial view(view)</param>
        /// <param name="model">list of objects</param>
        /// <returns>string</returns>
        protected string RenderPartialViewToString(string reportTitle, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;
            ViewBag.ReportTitle = reportTitle;
            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        public HttpResponseMessage GetPDFChatBot(string cusAcc, string StateYear, string stateMonth)
        {
            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            AppSetting genBillLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenBillLink);
            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value, AdDom.Value);
            // NetworkCredential nwc = new NetworkCredential("Jaclyne", "wetland12!@", "Ekurhuleni");

            byte[] result = null;


            WebClient client = new WebClient();
            client.Credentials = nwc;
            string accNo = cusAcc;
            string yearN = StateYear;
            string month = stateMonth;
            string billFileName = yearN + "-" + month + "_" + cusAcc + "_Statement.pdf";

            string reportURL = genBillLink.Value + accNo + "&year=" + yearN + "&month=" + month;
            //return File(client.DownloadData(reportURL), "application/pdf");
            //return reportURL;

            result = client.DownloadData(reportURL);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new MemoryStream(result);
            //Stream stream = new MemoryStream(byteArray);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = yearN + "-" + month + "_" + cusAcc + "_Statement.pdf"
            };


            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            return response;
            // return File(stream.GetBuffer(), "application/pdf",billFileName);
            //return reportURL;
        }

        //___________________________________________Report Dashboard_______________________________________________________//

        // [Authorize(Roles = "Administrators" + "," + "Super Administrators")]
        public ActionResult IndexReport()
        {

            return View();
        }
        // [Authorize(Roles = "Administrators" + "," + "Super Administrators")]
        public ActionResult ReportDashboard()
        {

            return View();
        }
        //  [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult GetConsumption(string meterType, string accountNumber, string startDate, string endDate)
        {
            if (meterType == null && accountNumber == null && startDate == null && endDate == null)
                return Json(string.Empty);

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
                            "{ \"name\" : \"startDate\", \"value\" : \"" + Convert.ToDateTime(startDate).ToString("yyyyMMdd") + "\" },  " +
                            "{ \"name\" : \"endDate\", \"value\" : \"" + Convert.ToDateTime(endDate).ToString("yyyyMMdd") + "\" } ] } } ";

            WebHelper webHelper = new C8.eServices.Mvc.Helpers.WebHelper();
            var assetUrl = new AppSetting();

            using (var cxt = new eServicesDbContext())
            {
                assetUrl = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.MeterConsumptionUrl);
            }

            Uri url = new Uri(string.Format(assetUrl.Value + HttpUtility.UrlEncode(json)));

            string response = webHelper.Get(url, json);
            dynamic jo = JObject.Parse(response);
            string consumption = "[";
            string reading, d;
            string meterNo = jo.assetProperties.meterProperties[1].meterIdentifiers.meterNumber;
            DateTime date;

            foreach (dynamic mc in jo.assetProperties.meterProperties[1].meterConsumption)
            {
                d = mc["date"].ToString();
                date = Convert.ToDateTime(d.Substring(0, 4) + "-" + d.Substring(4, 2) + "-" + d.Substring(6, 2));
                reading = mc["consumption"].ToString();
                consumption += "{ \"label\": \"" + meterNo + ": " + date.ToString("yyyy-MM-dd") + "\", \"y\": " + reading + "},";
            }
            consumption = consumption.Substring(0, consumption.Length - 1);
            consumption += "]";

            if (response != null)
            {
                //Handle your reponse here
                return Json(consumption);
            }
            else
            {
                //No Response from the server
                System.Diagnostics.Debug.WriteLine(response);
                return Json("No data available.");
            }
        }
        // [Authorize(Roles = "Administrators" + "," + "Super Administrators")]
        //[EncryptedActionParameter]
        public ActionResult GenerateDateRangeReport(string startDate, string endDate)
        {
            //NetworkCredential nwc = new NetworkCredential("Ekurhuleni\nataliec", "Louella@2016");
            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            AppSetting genDateRangeReport = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenDateRangelink);

            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value, AdDom.Value);

            WebClient client = new WebClient();
            client.Credentials = nwc;
            DateTime StartDate = Convert.ToDateTime(startDate);
            DateTime EndDate = Convert.ToDateTime(endDate);
            var ExeTime = DateTime.Now;
            string fileName = "Siyakhokha Statistics Comparision Report_" + ExeTime + ".xls";
            //string accNo = accountNumber.Replace("z", string.Empty); // to take the z away from the account number after forcing it into a string -- see LoadOnlineBils
            string reportURL = genDateRangeReport.Value + StartDate + "&EndDate=" + EndDate;
            return File(client.DownloadData(reportURL), "application/ms-excel", fileName);
        }
        // [Authorize(Roles = "Administrators" + "," + "Super Administrators")]
        //[EncryptedActionParameter]
        public ActionResult GenerateDetailedReport()
        {
            //NetworkCredential nwc = new NetworkCredential("Ekurhuleni\nataliec", "Louella@2016");
            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            AppSetting genDetailedLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenDetailedlink);

            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value, AdDom.Value);
            var ExeTime = DateTime.Now;
            string fileName = "Siyakhokha Statistics Report_" + ExeTime + ".xls";
            WebClient client = new WebClient();
            client.Credentials = nwc;
            //string accNo = accountNumber.Replace("z", string.Empty); // to take the z away from the account number after forcing it into a string -- see LoadOnlineBils
            string reportURL = genDetailedLink.Value;
            return File(client.DownloadData(reportURL), "ms-excel", fileName);
        }
        public JsonResult LoadReportData()
        {
            using (var eServicesContext = new eServicesDbContext())
            {
                var cesarContext = new CesarDbContext();
                var cesarBulkContext = new CesarBulkDBContext();
                var ekuContext = new BillingDbContext();
                //Initialise();
                var userLogins = new List<SystemUserLogTime>();
                var userReg = new List<SystemUser>();
                var userLinkedAccount = new List<LinkedAccount>();
                var userENotifications = new List<EmailQueueProcessed>();
                var userSNotifications = new List<SmsQueueProcessed>();
                var userEStatementNotifications = new List<EmailQueueProcessedBulk>();
                var userSStatementNotifications = new List<SmsQueueProcessedBulk>();
                var userStatementLoaded = new List<BillInfo>();
                var userMasterpass = new List<MasterpassTransaction>();
                var userIEFT = new List<InstantEFT>();

                var userMasterpassT = new List<MasterpassTransaction>();
                var userIEFTT = new List<InstantEFT>();
                //var userDebitOrdersT = new List<DebitOrder>();
                //var userDebitOrders = new List<DebitOrder>();
                var rows = new List<object>();

                var userLoginsModelsresult = eServicesContext.Database
    .SqlQuery<UserLoginsReport>("sp_eku_User_Logins_Report").OrderBy(o=>o.Logins_Month)
    .ToList();
                var userRegModelsresult = eServicesContext.Database
 .SqlQuery<UserRegistrationReports>("sp_eku_Registration_Report").OrderBy(o => o.Registration_Month)
 .ToList();
                var userLinkedAccountModelsresult = eServicesContext.Database
 .SqlQuery<UserLinkedAccountReport>("sp_eku_Linked_Accounts_Report").OrderBy(o => o.Linked_Account_Month)
 .ToList();
                var userENotificationsModelsresult = eServicesContext.Database
 .SqlQuery<UserENotificationReport>("sp_eku_ENotification_Report").OrderBy(o => o.Notification_Month)
 .ToList();
                var userSNotificationsModelsresult = eServicesContext.Database
 .SqlQuery<UserSNotificationReport>("sp_eku_SNotification_Report").OrderBy(o => o.SMS_Month)
 .ToList();
                var userEStatementNotificationsModelsresult = eServicesContext.Database
 .SqlQuery<UserEStatementNotificationReport>("sp_eku_EStatementNotification_Report").OrderBy(o => o.Estatement_Month)
 .ToList();
                var userSStatementNotificationsModelsresult = eServicesContext.Database
 .SqlQuery<UserSStatementNotificationReport>("sp_eku_SStatementNotification_Report").OrderBy(o => o.SStatment_Month)
 .ToList();
                var userStatementLoadedModelsresult = eServicesContext.Database
 .SqlQuery<UserStatementLoadedReport>("sp_eku_Statements_Loaded_Report").OrderBy(o => o.State_Month)
 .ToList();
                var userMasterpassModelsresult = eServicesContext.Database
 .SqlQuery<UserMasterpassReport>("sp_eku_master_pass_report").OrderBy(o => o.Masterpass_Month)
 .ToList();
                var userMasterpassTModelsresult = eServicesContext.Database
 .SqlQuery<UserMasterpassTReport>("sp_eku_master_passT_report").OrderBy(o => o.Masterpass_Month)
 .ToList();
                var userIEFTModelsresult = eServicesContext.Database
 .SqlQuery<UserIEFTReport>("sp_eku_eft_Report").OrderBy(o => o.EFT_Month)
 .ToList();
                var userIEFTTModelsresult = eServicesContext.Database
 .SqlQuery<UserIEFTTReport>("sp_eku_eftT_Report").OrderBy(o => o.EFT_Month)
 .ToList();
                //               var userDebitOrdersModelsresult = eServicesContext.Database
                //.SqlQuery<UserRegistrationReports>("sp_eku_Registration_Report")
                //.ToList();
                //               var userDebitOrdersTModelsresult = eServicesContext.Database
                //.SqlQuery<UserRegistrationReports>("sp_eku_Registration_Report")
                //.ToList();
                //var userLoginsModels = eServicesContext.SystemUserLogTimes
                //    .Where(a => a. == Customer.Id
                //     && a.Status.Key == StatusKeys.LinkedAccountActive
                //     && a.Customer.Status.Key == StatusKeys.CustomerActive).ToList();


                for (int j=0; j< userLoginsModelsresult.Count;j++)
                {
                    DateTime ddte = new DateTime(userLoginsModelsresult[j].Logins_Year, userLoginsModelsresult[j].Logins_Month, 1);
                    string datMonth = ddte.ToString("MMMM");
                    int webV = userLoginsModelsresult[j].Total_Logins;
                    int RegV = userRegModelsresult[j].Total_Registered;
                    int LMAV = userLinkedAccountModelsresult[j].Linked_Account_Total;
                    var amount = decimal.Round(userMasterpassModelsresult[j].Masterpass_Total, 2, MidpointRounding.AwayFromZero);
                    decimal amount2 = 0;
                    if(j<userIEFTModelsresult.Count)
                    {
                        amount2 = decimal.Round(userIEFTModelsresult[j].EFT_Total, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        amount2 = 0;
                    } 
                    var amountTot = decimal.Round(amount + amount2, 2, MidpointRounding.AwayFromZero);
                    //var accountNo = history.AccountNumber.ToString();
                    //long accNo = 0;
                    rows.Add(new
                    {
                        //history.AccountNumber,
                        Month = datMonth,
                        Webvisits= webV,
                        RegisteredCustomers = RegV,
                        Linked = LMAV,
                        AmountProcessed = amountTot
                    });
                }
                var total = 0;

                //using (var eBilling6MonthsContext = new eBilling6MonthsDbContext())
                //{
                //    try
                //    {
                //        var accountNumbers = string.Empty;
                //        var linkedAccounts = new List<LinkedAccount>();
                //        var customerBillingHistory = new List<BillingHistory>();

                //        var linkedAccountsModels = eServicesContext.LinkedAccounts.Include(a => a.Account)
                //            .Include(a => a.Customer)
                //            .Include(a => a.Customer.Status)
                //            .Where(a => a.CustomerId == Customer.Id
                //             && a.Status.Key == StatusKeys.LinkedAccountActive
                //             && a.Customer.Status.Key == StatusKeys.CustomerActive).ToList();

                //        if (Customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent)
                //        {
                //            isAgent = true;
                //            linkedAccountsModels.AddRange(GetAgentCustomerLinkedAccounts());
                //        }

                //        if (!string.IsNullOrEmpty(searchText))
                //        {
                //            linkedAccounts = linkedAccountsModels.OrderBy(l => l.Id).Where(l => l.Account.AccountNumber.Contains(searchText))
                //                             .ToList();
                //        }
                //        else
                //        {
                //            linkedAccounts = linkedAccountsModels.OrderBy(l => l.Id).ToList();
                //        }

                //        accountNumbers = linkedAccounts.Aggregate(accountNumbers, (current, account) => current + (account.Account.AccountNumber + ','));

                //        //RMS bills
                //        //billingHistories.AddRange(
                //        //    eServicesContext.Database.SqlQuery<BillingHistory>(
                //        //        "pr_REPORT_GetCustomerRmsBillingForArray @AccountNumbers,@CustomerId",
                //        //        new SqlParameter("@AccountNumbers", accountNumbers),
                //        //        new SqlParameter("@CustomerId", Customer.Id)).ToList());

                //        //COINS bills
                //        //billingHistories.AddRange(
                //        //    eBilling6MonthsContext.Database.SqlQuery<BillingHistory>(
                //        //        "pr_REPORT_BillingHistorySummaryForArray @AccountNumbers",
                //        //        new SqlParameter("@AccountNumbers", accountNumbers)).ToList());
                //        //List[BillingHistory] CustBillList = new List[BillingHistory];

                //        var bInfoList = new List<BillInfo>();
                //        using (var billcxt = new BillingDbContext())
                //        {
                //            foreach (var la in linkedAccounts)
                //            {
                //                bInfoList.AddRange(billcxt.BillInfos.Where(x => x.AccountNumber.Trim() == la.Account.AccountNumber.ToString()).ToList());
                //            }

                //            foreach (var bi in bInfoList)
                //            {
                //                billingHistories.Add(new BillingHistory()
                //                {
                //                    AccountNumber = long.Parse(bi.AccountNumber),
                //                    BillDate = bi.RunDateConvert,
                //                    IdentificationNumber = bi.Id.ToString(),
                //                    BalanceBroughtForward = decimal.Round(bi.TotalConvert, 2, MidpointRounding.AwayFromZero),
                //                    Total = decimal.Round(bi.TotalConvert, 2, MidpointRounding.AwayFromZero)
                //                });
                //            }
                //        }

                //        total = billingHistories.Count();

                //        // Defaults 6 Months for each Account Number
                //        //numberOfMonths = numberOfMonths ?? 6;
                //        //billingHistories =
                //        //    billingHistories.GroupBy(row => row.AccountNumber)
                //        //        .SelectMany(acc => acc.OrderBy(row => row.BillDate).Take((int)numberOfMonths))
                //        //        .ToList();

                //        foreach (var billingHistory in billingHistories)
                //        {
                //            if (string.IsNullOrEmpty(billingHistory.IdentificationNumber))
                //            {
                //                var linkedAccount =
                //                    linkedAccounts.FirstOrDefault(
                //                        la => la.Account.AccountNumber == billingHistory.AccountNumber.ToString(CultureInfo.InvariantCulture));
                //                if (null != linkedAccount)
                //                    billingHistory.IdentificationNumber = linkedAccount.Customer.IdentificationNumber;
                //            }
                //            customerBillingHistory.Add(billingHistory);
                //        }

                //        billingHistories = customerBillingHistory.OrderByDescending(l => l.BillDate)
                //                           .Skip(skip).Take(pageSize).ToList();

                //        foreach (var history in billingHistories)
                //        {
                //            var date = history.BillDate.ToString("yyyy-MM-dd");
                //            var amount = decimal.Round(history.Total, 2, MidpointRounding.AwayFromZero);
                //            var accountNo = history.AccountNumber.ToString();
                //            long accNo = 0;
                //            rows.Add(new
                //            {
                //                //history.AccountNumber,
                //                AccountNumber = accountNo,
                //                history.IdentificationNumber,
                //                BillDate = date,
                //                BillAmount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero),
                //                DownloadLink = downloadLink,
                //                isAgent
                //            });
                //        }

                //        ViewBag.IsAgent = isAgent;
                //  }
                //    catch (Exception ex)
                //     {
                //        throw;
                //    }
            //}

            var json = Json(new { rows }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = Int32.MaxValue;
            return json;
        }
    }

        //public ActionResult MakeChart()
        //{
        //    return this.View(new Employees());
        //}

        //public ActionResult ShowChart()
        //{
        //    //Employees newEmp = new Employees();
        //    //using (var context = new eServicesDbContext())
        //    //{
        //    //    List<SystemUser> res = context.SystemUsers.Where(o => o.IsActive && !o.IsDeleted && o.CreatedDateTime != null).ToList();
        //    //    newEmp.Ids = res.ToString().ToArray();
        //    //        }
        //    return this.View(new Employees());
        //}
    }
}