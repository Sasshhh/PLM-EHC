using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.ApiServices;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Helpers.Abstract;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace C8.eServices.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        private readonly IStoredProcedure _storedProcedure;
        #region Init
        public HomeController()
        {
            _storedProcedure = new StoredProcedure();
        }
        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }
        private void Initialise()
        {
            try
            {
                IdentityManager = new IdentityManager(db);

                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    SystemUser = IdentityManager.CurrentUser(User);
                }

                if (SystemUser != null)
                {
                    Customer =
                        db.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                            .Include(o => o.CustomerType)
                            .Include(o => o.Country)
                            .Include(o => o.IdentificationType)
                            .Include(o => o.TitleType)
                            .FirstOrDefault();

                    if (Customer != null)
                    {
                        Entity =
                            db.Entities.Where(o => o.CustomerId == Customer.Id)
                                .Include(o => o.EntityType)
                                .FirstOrDefault();
                        CustomerId = Customer.Id;
                    }

                }

                if (Customer != null)
                {
                    Agent = db.Agents.FirstOrDefault(o => o.CustomerId == Customer.Id);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion
        #region Audit Logs
        public ActionResult TableAuditLogs()
        {
            Initialise();
            var loggedIn = Customer.Id;
            //IEnumerable<Audit> records = _storedProcedure.AuditRecords();
            if (User.IsInRole("Area Manager"))
            {
                ViewBag.AuditUser = new SelectList(db.SystemUsers.Where(d => d.IsActive == true && d.IsDeleted == false), "Id", "UserName").OrderBy(x => x.Text);
            }
            else
            {
                ViewBag.AuditUser = new SelectList(db.SystemUsers.Where(d => d.IsActive == true && d.IsDeleted == false), "Id", "UserName").OrderBy(x => x.Text);

            }
            List<string> rrq = new List<string> { "ActivityTrackerMessages","Agents","ApplicantJoints","ApplicantUnits","ApplicationDepartments","ApplicationRoles","Applications","ApplicationUserRoles","AppSettings","AppUserRoles","ClerkRegistrations","ClerkRoles","CommitteeOutcomes","CompanyDirectors",
                        "ConductUnitInspections","Customers","DateToSchedules","DebitOrderRegistrations","DepartmentalComments","DocumentReferences","Documents","Entities","Files","HoDs","InspectionSchedules","LeaseCaptureAddressContacts","LeaseCaptureSheets", "LeaseDetails","LeaseTerminations","Logs","MatchedUnits",
                        "MeetingRequests","PLMApplicationHistortyLogs","PropertyLeaseActionComments","PropertyLeaseAgreementMasters","PropertyLeaseApplications","PropertyLeaseRenewalOffers","PropertyManagers","PropertyResidents","RiskAssessmentOutcomes","RoundRobinQueues","ScheduledInspections","SystemUsers",
                        "UnitsEkurhuleniHousingCompanies","WaitingListQues", "UserWorkAllocations","HumanSettlementAgreementMasters","HumanSettlementApplications", "HumanSettlementLeaseDetails" };
            DateTime date = Convert.ToDateTime("2022-08-10");
            var CaseHistoryLog = db.Audits.Include(r => r.AuditBySystemUser).OrderBy(r => r.PrimaryKey).Where(x => x.AuditDateTime.Year > date.Year).ToList();
            ViewBag.TableNames = new SelectList(CaseHistoryLog.Where(r => rrq.Contains(r.TableName)).GroupBy(r => r.TableName).Select(rr => rr.First()), "TableName", "TableName").OrderBy(x => x.Text);
            Session["TableNames"]= new SelectList(CaseHistoryLog.GroupBy(r => r.TableName).Select(rr => rr.First()), "TableName", "TableName").OrderBy(x => x.Text);
            ViewBag.ShowAudits = false;
            return View();
        }
        [HttpPost]
        public ActionResult TableAuditLogs(int? User, DateTime? startDate, DateTime? endDate, string Table, List<string> TableNames)
        {
            eServicesDbContext core = new eServicesDbContext();

            BaseHelper _base = new BaseHelper();
            _base.Initialise(core);
            var loggedIn = _base.SystemUser.Id;
            int Id = 0;
            if (User != null) Id = (Int32)User;
            if (endDate != null)
            {
                TimeSpan ts = new TimeSpan(23, 59, 59);
                endDate = endDate + ts;
            }
            if (User != null)
            {
                var CustomerID = core.Customers.Where(x => x.SystemUserId == User).FirstOrDefault();
                User = CustomerID?.Id;
            }
            DateTime date = Convert.ToDateTime("2022-01-01");
            var CaseHistoryLog = new List<Audit>();

            if (Table == "") Table = null;

            CaseHistoryLog = core.Audits.Include(r => r.AuditBySystemUser).OrderBy(r => r.PrimaryKey).Where(x => x.AuditDateTime.Year > date.Year).ToList();
            List<string> rrq = new List<string> { "ActivityTrackerMessages","Agents","ApplicantJoints","ApplicantUnits","ApplicationDepartments","ApplicationRoles","Applications","ApplicationUserRoles","AppSettings","AppUserRoles","ClerkRegistrations","ClerkRoles","CommitteeOutcomes","CompanyDirectors",
                        "ConductUnitInspections","Customers","DateToSchedules","DebitOrderRegistrations","DepartmentalComments","DocumentReferences","Documents","Entities","Files","HoDs","InspectionSchedules","LeaseCaptureAddressContacts","LeaseCaptureSheets", "LeaseDetails","LeaseTerminations","Logs","MatchedUnits",
                        "MeetingRequests","PLMApplicationHistortyLogs","PropertyLeaseActionComments","PropertyLeaseAgreementMasters","PropertyLeaseApplications","PropertyLeaseRenewalOffers","PropertyManagers","PropertyResidents","RiskAssessmentOutcomes","RoundRobinQueues","ScheduledInspections","SystemUsers",
                        "UnitsEkurhuleniHousingCompanies","WaitingListQues", "UserWorkAllocations","HumanSettlementAgreementMasters","HumanSettlementApplications", "HumanSettlementLeaseDetails" };
            ViewBag.TableNames = new SelectList(CaseHistoryLog.GroupBy(r => r.TableName).Select(rr => rr.First()), "TableName", "TableName").OrderBy(x => x.Text);
            ViewBag.ShowAudits = false;

            if (User != null)
            {
                CaseHistoryLog = CaseHistoryLog.Where(d => (d.AuditBySystemUserId == User || d.AuditBySystemUserId == Id)).ToList();
                ViewBag.ShowAudits = true;
            }
            if (startDate != null)
            {
                CaseHistoryLog = CaseHistoryLog.Where(d => d.AuditDateTime >= startDate).ToList();
                ViewBag.ShowAudits = true;
            }
            if (endDate != null)
            {
                CaseHistoryLog = CaseHistoryLog.Where(d => d.AuditDateTime <= endDate).ToList();
                ViewBag.ShowAudits = true;
            }
            if ((Table != null) || (Table == ""))
            {
                CaseHistoryLog = CaseHistoryLog.Where(d => d.TableName == Table).ToList();
                ViewBag.ShowAudits = true;
            }

            ViewData["CaseHistoryLog"] = CaseHistoryLog;
            ViewBag.AuditUser = new SelectList(core.SystemUsers.Where(d => d.IsActive == true && d.IsDeleted == false), "Id", "UserName").OrderBy(x => x.Text); ;
            return View();
        }
        #endregion
        #region Audit Logs
        public ActionResult AuditLogsIndex()
        {
            Initialise();
            var loggedIn = Customer.Id;
            var users = db.SystemUsers.Where(d => d.IsActive == true && d.IsDeleted == false);
            var UserKey = db.Customers.Include(r => r.Department).FirstOrDefault(x => x.Id == loggedIn).Department?.Key;
            if (UserKey != null) users = users.Include(o => o.Department).Where(r => r.Department.Key == UserKey);
            if (User.IsInRole("Area Manager"))
            {
                //var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == loggedIn && x.IsDeleted == false).FirstOrDefault();
                //if(CCCClerk != null)
                //{
                //    ViewBag.AuditUser = new SelectList(db.SystemUsers.Where(d => d.IsActive == true && d.IsDeleted == false && d.CCCId == CCCClerk.Id), "Id", "UserName").OrderBy(x => x.Text);

                //}
                //else
                //{
                //    ViewBag.AuditUser = new SelectList(db.SystemUsers.Where(d => d.IsActive == true && d.IsDeleted == false), "Id", "UserName").OrderBy(x => x.Text);

                //}

                ViewBag.AuditUser = new SelectList(users, "Id", "UserName").OrderBy(x => x.Text);

            }
            else
            {
                ViewBag.AuditUser = new SelectList(users, "Id", "UserName").OrderBy(x => x.Text);

            }
            var HistoryLog = db.PropertyLeaseApplications.Where(x => x.ApplicationReferenceNumber != null).ToList();
            var rrq = HistoryLog.Select(x => x.Id).ToList();
            var HSDLogs = db.HumanSettlementApplications.Where(x => x.ApplicationReferenceNumber != null).ToList();
            var hsd = HSDLogs.Select(x => x.Id).ToList();
            var CaseHistoryLog = db.PLMApplicationHistortyLogs.Where(d => d.IsActive == true && d.IsDeleted == false && (rrq.Contains((int)d.PropertyLeaseApplicationId) || hsd.Contains((int)d.HumanSettlementApplicationId)))
                .Include(d => d.PropertyLeaseApplication).Include(d => d.User).ToList();
            if (CaseHistoryLog != null)
            {
                ViewData["CaseHistoryLog"] = CaseHistoryLog;
            }
            return View();
        }
        [HttpPost]
        public ActionResult AuditLogsIndex(int? User, DateTime? startDate, DateTime? endDate)
        {
            Initialise();
            var loggedIn = SystemUser.Id;
            //User = Customer.Id;
            if (endDate != null)
            {
                TimeSpan ts = new TimeSpan(23, 59, 59);
                endDate = endDate + ts;
            }
            if (User != null)
            {
                var CustomerID = db.Customers.Where(x => x.SystemUserId == User).FirstOrDefault().Id;
                User = CustomerID;
            }
     
            var HistoryLog = db.PropertyLeaseApplications.Where(x => x.ApplicationReferenceNumber != null).ToList();
            var rrq = HistoryLog.Select(x => x.Id).ToList();
            var HSDLogs = db.HumanSettlementApplications.Where(x => x.ApplicationReferenceNumber != null).ToList();
            var hsd = HSDLogs.Select(x => x.Id).ToList();
            var CaseHistoryLog = db.PLMApplicationHistortyLogs
                .Where(d => d.IsActive == true && d.IsDeleted == false && (rrq.Contains((int)d.PropertyLeaseApplicationId) || hsd.Contains((int)d.HumanSettlementApplicationId)))
                .Include(d => d.PropertyLeaseApplication).Include(d => d.User).ToList();
            if (User != null)
            {
                CaseHistoryLog = CaseHistoryLog.Where(d => d.UserId == User).ToList();
            }
            if (startDate != null)
            {
                CaseHistoryLog = CaseHistoryLog.Where(d => d.CreatedDateTime >= startDate).ToList();
            }
            if (endDate != null)
            {
                CaseHistoryLog = CaseHistoryLog.Where(d => d.CreatedDateTime <= endDate).ToList();
            }

            ViewData["CaseHistoryLog"] = CaseHistoryLog;

            ViewBag.AuditUser = new SelectList(db.SystemUsers.Where(d => d.IsActive == true && d.IsDeleted == false), "Id", "UserName").OrderBy(x => x.Text); ;

            return View();
        }
        #endregion


        #region Audit Logs Login
        public ActionResult UserLogsIndex()
        {
            Initialise();
            var loggedIn = SystemUser.Id;
            ViewBag.AuditUser = new SelectList(db.SystemUsers.Where(d => d.IsActive == true && d.IsDeleted == false), "Id", "UserName");

            var UserHistoryLog = db.SystemUserLogTimes.Where(s => s.SystemUserId == loggedIn).Include(d => d.SystemUser).OrderByDescending(x => x.Id).ToList().ToList();
            if (UserHistoryLog != null)
            {
                ViewData["UserHistoryLog"] = UserHistoryLog;
            }
            return View();
        }
        #endregion
        #region Home Index
        //
        // GET: /Home/
        public ActionResult Index()
        {
            try
            {
                Session["payments"] = null;

                // Test code.
                //This is not needed
                //var ca = CustomerAccountApi.GetAccounts("6911305037089");
                //EmailHelper client = new EmailHelper();
                //client.Recipient = "jayan.kistasami@calc8.co.za";
                //client.Subject = "Test " + DateTime.Now.ToString();
                //client.Body = "the quick brown fox.";
                //client.SendEmail();

                //using (var cxt = new eServicesDbContext())
                //{
                //    var ca = cxt.CustomerAccounts.FirstOrDefault(o => o.AccountNo == 1800678685);
                //    System.Diagnostics.Debug.WriteLine(ca.IDNo);
                //}                             

                // Used to display an update message for eservices.
                using (var cxt = new eServicesDbContext())
                {
                    //var ban = new BankAccount() { BankAccountNumber = "62099807411" };
                    //System.Diagnostics.Debug.WriteLine(ban.BankAccountNumber + Environment.NewLine + ban.HiddenBankAccountNumber);

                    //ban = cxt.BankAccounts.First();
                    //System.Diagnostics.Debug.WriteLine(ban.BankAccountNumber + Environment.NewLine + ban.HiddenBankAccountNumber);

                    var eServicesUpdate = cxt.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.UpdateNotification);
                    if (eServicesUpdate == null) throw new Exception("Invalid appsetting");

                    ViewBag.ShowUpdate = eServicesUpdate.IsActive;
                    ViewBag.UpdateMessage = eServicesUpdate.Value;
                    ViewBag.UpdateTime = eServicesUpdate.ModifiedDateTime?.ToString("yyyy/MM/dd HH:mm") ?? DateTime.Now.ToString("yyyy/MM/dd HH:mm");
                    return View();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #region FAQ Index
        public ActionResult FAQ()
        {
            return View();
        }
        #endregion
        public ActionResult ContactUs()
        {
            return View();
        }

        #region Home Index2
        public ActionResult Index2()
        {
            ViewBag.Status = "Email address activated";
            return View("Index");
        }


        #endregion

        #region Intro Index
        public ActionResult Introduction()
        {
            return View();
        }
        #endregion


        #region RightsandUsage Index
        public ActionResult RightsandUsage()
        {
            return View();
        }
        #endregion

        #region UsernamePassword Index
        public ActionResult UsernamePassword()
        {
            return View();
        }
        #endregion


        #region InforandPrivacy Index
        public ActionResult InfoandPrivacy()
        {
            return View();
        }
        #endregion

        #region Unavailability Index
        public ActionResult Unavailability()
        {
            return View();
        }
        #endregion
        #region Comming Soon
        public ActionResult UnderConstructionComingSoon()
        {
            return View();
        }
        #endregion


        public ActionResult fileuploadapitest()
        {
            var online = CheckForInternetConnection();
            if (online==false)
            {
                ViewBag.NetConnection = false;
            }

            else
            {
                ViewBag.NetConnection =true;
            }
            return View();
        }

        [HttpPost]
        public ActionResult fileuploadapitest(string link)
        {

            LawTrustApi law = new LawTrustApi();

            link = law.wso2genlink();
            
            return RedirectToAction("signhub", "Home", new { link = link });
        }


        //public ActionResult solarfileuploadapitest()
        //{
        //    var online = CheckForInternetConnection();
        //    if (online == false)
        //    {
        //        ViewBag.NetConnection = false;
        //    }

        //    else
        //    {
        //        ViewBag.NetConnection = true;
        //    }
        //    return View();
        //}

        //[HttpPost]
        public ActionResult solarfileuploadapitest(int id,string link)
        {
            Initialise();
            string email = "";
            var getdets = db.AppSettings.Where(x => x.Key == AppSettingKeys.Lawtrsuttestaccountswitch).FirstOrDefault();
            string lawtrusttestswitch = getdets.Value;
            if (getdets.Value == "on")
            {
                 email = "suhail.dada@xetgroup.com";
            }
            else
            {
                 email = Customer.EmailAddress;
            }
            LawTrustApi law = new LawTrustApi();
            var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == id).Include(x => x.TransferInformation).FirstOrDefault();
            string AccountNo = RCSApplication.TransferInformation.RatesNumber;
            link = law.wso2gendocandlink(AccountNo,email);

            return RedirectToAction("signhub", "Home", new { link = link });
        }

        //public ActionResult testupload() //old way to catch file from frontend
        //{
        //    LawTrustApi law = new LawTrustApi();


        //    string link = law.lawtrustintegrration();

        //    return RedirectToAction("signhub", "Home", new { link = link });
        //}



        [HttpPost]
        public ActionResult SetViewBag(string value)
        {
            ViewBag.RoundRobinQueue = null;
            ViewBag.RoundRobinQueueMessage = null;
            return new EmptyResult();
        }




        [ValidateInput(false)]
        public ActionResult signhub(string link)
        {
            //string con = link.Substring(1);
            //string cons = con.Remove(con.Length - 1, 1);

            string con = link.Substring(1);
            string cons = con.Remove(con.Length - 1, 1);


            ViewBag.link = cons;
            return View();
        }
        #region Account Internet Connectivity
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion
        public ActionResult RCSHome()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Help()
        {
            return View();
        }


        public ActionResult ClearViewBags()
        {
            ViewBag.NoticeSuccess = null;
            ViewBag.PostSuccess = null;
            ViewBag.RoundRobinQueue = null;
            ViewData.Clear();
            return Json(true, JsonRequestBehavior.AllowGet);
        }

    }
}
