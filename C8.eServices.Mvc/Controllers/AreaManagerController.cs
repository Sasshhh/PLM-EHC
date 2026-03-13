using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Org.BouncyCastle.Asn1.Ocsp;

namespace C8.eServices.Mvc.Controllers
{
    public class AreaManagerController : Controller
    {
        //private eServicesDbContext db = new eServicesDbContext();

        #region ApplicationUserRole Init
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();

        public AreaManagerController()
        {
            IdentityManager = new IdentityManager(db);
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            UserManager = new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(db));
        }

        public IdentityManager IdentityManager { get; set; }
        public RoleManager<IdentityRole> RoleManager { get; set; }
        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        #endregion
        #region Init



        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }


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
                                  .Include(o => o.Status)
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
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }

        #endregion
        // GET: AreaManager

        [Authorize(Roles = "Area Manager")]
        public ActionResult Index()
        {
            Initialise();
            var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();
            List<ClerkRegistration> clerkRegistrations = new List<ClerkRegistration>();
            if(CCCClerk != null)
            {
                clerkRegistrations = db.ClerkRegistrations.Where(x => x.Status.Key == StatusKeys.AccountPending && x.CCCId == CCCClerk.Id).Include(c => c.CCCType).Include(c => c.CreatedBySystemUser).Include(c => c.Status).Include(c => c.ModifiedBySystemUser).Include(c => c.NotificationType).ToList();

            }
            else
            {

                ViewBag.Message = "Please contact system administrator to get your account mapped to a CCC";
                ViewBag.MessageTitle = "Area Manager Not Mapped to CCC";
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                ViewBag.MessageTitle = "User Created";

            }
            return View(clerkRegistrations.ToList());
        }
        [DecryptParameter]
        public ActionResult ManualReAllocate(int? id, int? rrqID, string ResponsibilityType, string ViewName, string TitleName, string BodyName)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var plmApps = db.PropertyLeaseApplications.Where(x => x.Id == id).FirstOrDefault();
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            //var rcscustomer = db.Customers.Where(x => x.Id == plmApps.CustomerId).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var findItem = db.RoundRobinQueues.Include(r => r.Clerk).Include(r => r.ResponsibilityType).FirstOrDefault(x => x.StatusId == SubmittedId && x.PropertyLeaseApplicationId == plmApps.Id);
            var bouserid = db.Customers.FirstOrDefault(x => x.Id == findItem.ClerkId).SystemUserId;
            var userrole = db.ApplicationUserRoles.Include(r => r.IdentityRole).OrderByDescending(x => x.Id).FirstOrDefault(x => x.SystemUserId == bouserid);
            var ResponsibilityTypeName = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault();
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == rrqID).FirstOrDefault();
            var RoleName = db.Roles.Where(x => x.Name == userrole.IdentityRole.Name).FirstOrDefault().Id;
            PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
            var UsersList = cc.GetUsersInRole(RoleName).Where(x => x.RoundRobinIsActive == true).ToList();

            var vm = new ManualReAllocationViewModel();
            vm.CurrentFullName = rrq.Clerk.FullName;
            vm.PropertyLeaseApplicationId = (int)id;
            vm.CurrentAssignedUserName = rrq.Clerk.UserFullName;
            vm.RoundRobinQueueId = rrq.Id;
            vm.ResponsibilityType = Convert.ToString(findItem.ResponsibilityType.Id);
            vm.ViewName = ViewName;
            vm.TitleName = TitleName;
            vm.BodyName = BodyName;
            ViewBag.BOUsers = new SelectList(UsersList, "SystemUser.Id", "SystemUser.FullName");
            return View(vm);
        }

        [HttpPost]
        public ActionResult ManualReAllocate(int? id, ManualReAllocationViewModel vm, int UserId)
        {
            Initialise();

            var plmApps = db.PropertyLeaseApplications.Where(x => x.Id == vm.PropertyLeaseApplicationId && x.IsDeleted == false).FirstOrDefault();
            var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == vm.RoundRobinQueueId).FirstOrDefault();

            if (rrqList != null)
            {
                rrqList.StatusId = db.Status.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                rrqList.EndTaskDateTime = DateTime.Now;
                db.Entry(rrqList).State = EntityState.Modified;
                db.SaveChanges();
            }

            var ClerkId = db.Customers.Where(x => x.SystemUserId == UserId && x.IsDeleted == false).FirstOrDefault();
            var statusList = db.Status.ToList();
            var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

            var roundRobinQueue = new RoundRobinQueue
            {
                PropertyLeaseApplicationId = plmApps.Id,
                ResponsibilityTypeId = Convert.ToInt32(vm.ResponsibilityType),
                CurrentTaskDateTime = DateTime.Now,
                ClerkId = ClerkId.Id,
                StatusId = StatusId,
                AssignedFrom = rrqList.Id
            };
            db.RoundRobinQueues.Add(roundRobinQueue);
            db.SaveChanges();
            PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();

            int rtype = Convert.ToInt32(vm.ResponsibilityType);
            var ResponsibilityType = db.ResponsibilityTypes.FirstOrDefault(x => x.Id == rtype);
            cc.BackOfficeNotification(plmApps.Id, ClerkId.Id, ResponsibilityType.Name);


            var Title = vm.TitleName;
            var Body = vm.BodyName;

            if (roundRobinQueue.Id == 0)
            {
                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;
            }
            else
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                var Result =cc.ActivityTrackerAudit(plmApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; ;
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;
            }

            return RedirectToAction(vm.ViewName);
        }

        public ActionResult EHCRoundRobbinDashboard()
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
            {
                var Message = TempData["ApplicationFeeValidationRedistribution"];
                if (Message != null)
                {
                    ViewBag.MessageTitle = "Application Redistribution Successful";
                    ViewBag.Message = TempData["ApplicationFeeValidationRedistribution"].ToString();
                }

                var Message2 = TempData["AcknowledgementRedistribution"];
                var Title2 = TempData["AcknowledgementRedistributionTitle"];
                if (Message2 != null && Title2 != null)
                {
                    ViewBag.MessageTitle = TempData["AcknowledgementRedistribution"].ToString();
                    ViewBag.Message = TempData["AcknowledgementRedistribution"].ToString();
                }
                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();
                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                if (User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x =>/* x.ResponsibilityTypeId == ResponsibilityTypeId && */x.StatusId == SubmittedId && x.HumanSettlementApplicationId == null).ToList();
                }
                else
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x =>/* x.ResponsibilityTypeId == ResponsibilityTypeId && */x.StatusId == SubmittedId && x.HumanSettlementApplicationId == null).ToList();
                    //rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }
                //rrq= rrq.Where(x=>x.Clerk.SystemUser.CCC == 8)
                var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                var plmApps = db.PropertyLeaseApplications.Where(x => x.IsActive && list.Contains(x.Id))
                    .Include(r => r.Status)
                    .Include(r => r.SystemUser)
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.HumanEHCOptions)
                    .Include(r => r.Customer).ToList();

                foreach (var Item in plmApps)
                {
                    var rrqplm = db.RoundRobinQueues.Include(r => r.Clerk).Where(x => x.PropertyLeaseApplicationId == Item.Id && x.StatusId == SubmittedId).FirstOrDefault();
                    Item.Data = rrqplm.Clerk.UserFullName;
                    Item.RoundRobinQueueId = rrqplm.Id;
                }

                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();
                var ResponsibilityTypeID = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault();
                foreach (var item in rcsApps)
                {
                    var rrqName = db.RoundRobinQueues.Where(x => x.RCSApplicationStatusId == item.Id && x.ResponsibilityTypeId == ResponsibilityTypeID.Id).Include(x => x.Clerk).OrderByDescending(x => x.Id).FirstOrDefault();
                    item.Data = rrqName.Clerk.UserFullName;
                    item.RoundRobinQueueId = Convert.ToString(rrqName.Id);
                    if (item.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation)
                    {


                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.ApplicationFeeValidation).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).OrderByDescending(x => x.Id).FirstOrDefault();
                        DateTime StartDateFinal = Convert.ToDateTime(StartDate.CreatedDateTime);


                        TimeSpan diff2 = (DateTime.Now.Subtract(StartDateFinal));

                        var diffdays = diff2.Days;
                        if (diffdays < TurnAroundDays)
                        {
                            item.ColorCode = OnTargetColor;
                        }
                        else if (diffdays == TurnAroundDays)
                        {
                            item.ColorCode = RunningLateColor;
                            //@*background - color: #e6cc11;*@
                        }
                        else if (diffdays > TurnAroundDays)
                        {
                            item.ColorCode = OverdueColor;
                        }
                    }

                    else if (item.Status.Key == StatusKeys.PendingDocumentsApproval)
                    {
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).OrderByDescending(x => x.Id).FirstOrDefault();
                        DateTime StartDateFinal = Convert.ToDateTime(StartDate.CurrentTaskDateTime);
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.DocumentValidation).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        TimeSpan diff2 = (DateTime.Now.Subtract(StartDateFinal));

                        var diffdays = diff2.Days;
                        if (diffdays < TurnAroundDays)
                        {
                            item.ColorCode = OnTargetColor;
                        }
                        else if (diffdays == TurnAroundDays)
                        {
                            item.ColorCode = RunningLateColor;
                        }
                        else if (diffdays > TurnAroundDays)
                        {
                            item.ColorCode = OverdueColor;
                        }
                    }


                }

                return View(plmApps);

            }
            else
            {
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                var list = rrq.Select(x => x.RCSApplicationStatusId).ToList();
                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();

                foreach (var item in rcsApps)
                {

                    if (item.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.ApplicationFeeValidation).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).OrderByDescending(x => x.Id).FirstOrDefault();

                        DateTime StartDateFinal = Convert.ToDateTime(StartDate.CreatedDateTime);


                        TimeSpan diff2 = (DateTime.Now.Subtract(StartDateFinal));

                        var diffdays = diff2.Days;
                        if (diffdays < TurnAroundDays)
                        {
                            item.ColorCode = OnTargetColor;
                        }
                        else if (diffdays == TurnAroundDays)
                        {
                            item.ColorCode = RunningLateColor;
                            //@*background - color: #e6cc11;*@
                        }
                        else if (diffdays > TurnAroundDays)
                        {
                            item.ColorCode = OverdueColor;
                        }
                    }

                    else if (item.Status.Key == StatusKeys.PendingDocumentsApproval)
                    {
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).OrderByDescending(x => x.Id).FirstOrDefault();

                        DateTime StartDateFinal = Convert.ToDateTime(StartDate.CurrentTaskDateTime);
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.DocumentValidation).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        TimeSpan diff2 = (DateTime.Now.Subtract(StartDateFinal));

                        var diffdays = diff2.Days;
                        if (diffdays < TurnAroundDays)
                        {
                            item.ColorCode = OnTargetColor;
                        }
                        else if (diffdays == TurnAroundDays)
                        {
                            item.ColorCode = RunningLateColor;
                        }
                        else if (diffdays > TurnAroundDays)
                        {
                            item.ColorCode = OverdueColor;
                        }
                    }


                }
                return View(rcsApps);
            }


        }

        public ActionResult EHCComplexAreaDashboard()
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
            {
                var Message = TempData["ApplicationFeeValidationRedistribution"];
                if (Message != null)
                {
                    ViewBag.MessageTitle = "Application Redistribution Successful";
                    ViewBag.Message = TempData["ApplicationFeeValidationRedistribution"].ToString();
                }

                var Message2 = TempData["AcknowledgementRedistribution"];
                var Title2 = TempData["AcknowledgementRedistributionTitle"];
                if (Message2 != null && Title2 != null)
                {
                    ViewBag.MessageTitle = TempData["AcknowledgementRedistribution"].ToString();
                    ViewBag.Message = TempData["AcknowledgementRedistribution"].ToString();
                }
                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();
                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                List<PreferredComplexArea> prr = new List<PreferredComplexArea>();
                if (User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                    prr = db.PreferredComplexAreas.Include(x => x.HousingSuper).Include(x => x.LettingOfficer).ToList();
                }
                else
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                    List<string> rr = new List<string> { PrefferedComplexKeys.Airportpark, PrefferedComplexKeys.AirportparkStaff, PrefferedComplexKeys.Delville,
                    PrefferedComplexKeys.DelvilleStaff, PrefferedComplexKeys.PharoePark, PrefferedComplexKeys.ChrisHani, PrefferedComplexKeys.ChrisHaniStaff, PrefferedComplexKeys.NewDelville, PrefferedComplexKeys.BlockE};

                    prr = db.PreferredComplexAreas.Include(x => x.HousingSuper).Include(x => x.LettingOfficer).Where(x => x.Key != "ekurhuleni_complex").ToList();
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }

                return View(prr);

            }

            return View();

        }

        [Authorize]
        public ActionResult UserWorkAllocation()
        {
            Initialise();
            MatchingHelper.addUsersToWorkAllocation(db);
            var userID = Customer.Id;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
            {
                var Message = TempData["ApplicationFeeValidationRedistribution"];
                if (Message != null)
                {
                    ViewBag.MessageTitle = "Application Redistribution Successful";
                    ViewBag.Message = TempData["ApplicationFeeValidationRedistribution"].ToString();
                }

                var Message2 = TempData["AcknowledgementRedistribution"];
                var Title2 = TempData["AcknowledgementRedistributionTitle"];
                if (Message2 != null && Title2 != null)
                {
                    ViewBag.MessageTitle = TempData["AcknowledgementRedistribution"].ToString();
                    ViewBag.Message = TempData["AcknowledgementRedistribution"].ToString();
                }
                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();
                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                List<PreferredComplexArea> prr = new List<PreferredComplexArea>();
                if (User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                    prr = db.PreferredComplexAreas.Include(r=>r.RegionType).Include(r=>r.CCCType).Where(x => x.Key.Contains("h_")).ToList();
                }
                else
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                    List<string> rr = new List<string> { PrefferedComplexKeys.Airportpark, PrefferedComplexKeys.AirportparkStaff, PrefferedComplexKeys.Delville,
                    PrefferedComplexKeys.DelvilleStaff, PrefferedComplexKeys.PharoePark, PrefferedComplexKeys.ChrisHani, PrefferedComplexKeys.ChrisHaniStaff, PrefferedComplexKeys.NewDelville, PrefferedComplexKeys.BlockE};

                    prr = prr = db.PreferredComplexAreas.Where(x => x.Key.Contains("h_")).ToList();
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }

                return View(prr);

            }

            return View();

        }

        [DecryptParameter]
        public ActionResult EditArea(int? id, string ViewName, string TitleName, string BodyName)
        {
            Initialise();
            var userID = Customer.Id;
            var area = db.PreferredComplexAreas.Include(r => r.LettingOfficer).Include(r => r.HousingSuper).FirstOrDefault(x => x.Id == id);

            var CSORole = db.Roles.Where(x => x.Name == "Client Services Officer").FirstOrDefault();
            var RoleName = CSORole != null ? CSORole.Id : db.Roles.Where(x => x.Name == "Letting Officer").FirstOrDefault().Id;
            PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();

            var CSOUsersList = cc.GetUsersInRole(RoleName).Where(x => x.RoundRobinIsActive == true).ToList();

            var vm = new ManualReAllocationViewModel();
            vm.PreferredComplexArea = area;
            vm.CurrentFullName = area.Name;
            vm.PropertyLeaseApplicationId = (int)id;
            vm.CurrentAssignedUserName = area.Name;
            vm.ViewName = ViewName;
            vm.TitleName = TitleName;
            vm.BodyName = BodyName;
            ViewBag.CSOUsers = new SelectList(CSOUsersList, "SystemUser.Id", "SystemUser.FullName");
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditArea(int? id, ManualReAllocationViewModel vm)
        {
            Initialise(); 

            var area = db.PreferredComplexAreas.Include(r => r.LettingOfficer).Include(r => r.HousingSuper).FirstOrDefault(x => x.Id == vm.PropertyLeaseApplicationId);

            var csoCustomerId = db.Customers.FirstOrDefault(x => x.SystemUserId == vm.LettingOfficerId).Id;
            area.LettingOfficerId = csoCustomerId;
            area.HousingSuperId = csoCustomerId;
            area.ModifiedDateTime = DateTime.Now;
            db.Entry(area).State = EntityState.Modified;
            db.SaveChanges();

            var Title = vm.TitleName;
            var Body = vm.BodyName;
            return RedirectToAction(vm.ViewName);
        }

        [Authorize]
        [DecryptParameter]
        public ActionResult EditWorkAllocation(int? id, string ViewName, string TitleName, string BodyName)
        {
            Initialise();
            var core = new eServicesDbContext();
            var userID = Customer.Id;
            var area = core.PreferredComplexAreas.Include(r => r.RegionType).Include(r => r.CCCType).FirstOrDefault(x => x.Id == id);
            var Regions = core.RegionTypes.ToList();
            var CCCs = core.CCCTypes.ToList();
            var deletedusers = core.UserWorkAllocations.Where(d => d.PreferredComplexAreaId == area.Id && d.IsActive).ToList().Select(r => r.SystemUserId);
            var users = new List<SystemUser>();
            string[] Roles = { "Senior Housing Specialist", "Regional Manager", "Housing Liaison Officer", "Caretaker" };
            PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
            foreach (var role in Roles)
            {
                var RoleName = db.Roles.Where(x => x.Name == role).FirstOrDefault().Id;
                var rr = cc.GetUsersInRole(RoleName).ToList();
                foreach(var user in rr)
                {
                    var rrq = core.SystemUsers.FirstOrDefault(r => r.Id == user.SystemUserId && !deletedusers.Contains(r.Id));
                    if (rrq != null)
                    {
                        rrq.Data = string.Format("{0}, {1}", rrq.UserFullName, user.ServiceNo);
                        users.Add(rrq);
                    }
                }
            }
            var rrUsers = core.UserWorkAllocations
               .Include(r => r.PreferredComplexArea.RegionType)
               .Include(r => r.SystemUser)
               .Include(r => r.PreferredComplexArea.CCCType).Where(x => x.PreferredComplexAreaId == id && !x.IsDeleted).ToList();

            var vm = new ManualReAllocationViewModel();
            vm.UserWorkAllocation_L = rrUsers;
            vm.PreferredComplexArea = area;
            vm.CurrentFullName = area.Name;
            vm.PropertyLeaseApplicationId = (int)id;
            vm.CurrentAssignedUserName = area.Name;
            vm.ViewName = ViewName;
            vm.TitleName = TitleName;
            vm.BodyName = BodyName;
            ViewBag.CCCddl = new SelectList(CCCs.OrderBy(d=>d.Name), "Id", "Name");
            ViewBag.Regionddl = new SelectList(Regions.OrderBy(d => d.Name), "Id", "Name");
            ViewBag.Users = new SelectList(users.OrderBy(d => d.Data), "Id", "Data") ?? null; 


            return View(vm);
        }

        [Authorize]
        public ActionResult FindUserRoles(int UserId)
        {
            var core = new eServicesDbContext();
            var names = string.Empty;

            var useruser = core.SystemUsers.Find(UserId);
            var user = UserManager.FindByName(useruser.UserName);
            var userId = user.Id;
            // get user roles
            List<string> rolesArray = UserManager.GetRoles(userId).ToList();
            string RolesList = "";
            int counter = 0;
            foreach (var Roles in rolesArray)
            {
                if (!string.IsNullOrEmpty(names))
                {
                    names += ", ";
                }
                else
                {
                    counter++;
                    names += Roles;
                }
            
            }


            var obj = new
            {
                status = !string.IsNullOrEmpty(names) ? "Success" : "Failure",
                names = string.Format("{0}", names)
            };

            return Json(obj);
        }
        public ActionResult UpdateAreaRegionCCC(int PreferredComplexAreaId, string CurrentAssignedUserName, int RegionType, int CCCType)
        {
            var core = new eServicesDbContext();
            var Area = core.PreferredComplexAreas.FirstOrDefault(r => r.Id == PreferredComplexAreaId);
            Area.Name = CurrentAssignedUserName;
            Area.RegionTypeId = RegionType;
            Area.CCCTypeId = CCCType;
            core.Entry(Area).State = EntityState.Modified;
            core.SaveChanges();
            return RedirectToAction("EditWorkAllocation", "AreaManager", new { q = new AesCrypto().Encrypt("id=" + PreferredComplexAreaId + "&ViewName=EHCComplexAreaDashboard" + "&TitleName=EHCComplexAreaDashboardTitle&BodyName=EHCComplexAreaDashboard") });
        }

        public ActionResult AddAreaRRUser(int PreferredComplexAreaId, string Roles, int UserId)
        {
            var core = new eServicesDbContext();
            var find = core.UserWorkAllocations.FirstOrDefault(r => r.SystemUserId == UserId && r.PreferredComplexAreaId == PreferredComplexAreaId) ?? null;
            if (find != null)
            {
                //activate
                find.IsActive = true;
                find.IsDeleted = false;
                find.IsLocked = false;
                find.RRActive = false;
                find.Roles = Roles;
                core.Entry(find).State = EntityState.Modified;
                core.SaveChanges();
            }
            else
            {
                var wa = new UserWorkAllocation
                {
                    Roles = Roles,
                    PreferredComplexAreaId = PreferredComplexAreaId,
                    SystemUserId = UserId,
                    IsActive = true
                };
                core.UserWorkAllocations.Add(wa);
                core.SaveChanges();
            }

            var area = core.PreferredComplexAreas.Include(r => r.RegionType).Include(r => r.CCCType).FirstOrDefault(x => x.Id == PreferredComplexAreaId);
            var Regions = core.RegionTypes.ToList();
            var CCCs = core.CCCTypes.ToList();
            ViewBag.CCCddl = new SelectList(CCCs.OrderBy(d => d.Name), "Id", "Name");
            ViewBag.Regionddl = new SelectList(Regions.OrderBy(d => d.Name), "Id", "Name");
            ViewBag.Users = new SelectList(GetSystemUsers(area).OrderBy(d => d.Data), "Id", "Data");
            var vm = new ManualReAllocationViewModel();
            GetViewModel(vm, PreferredComplexAreaId);
            return View("EditWorkAllocation", vm);
        }



        [Authorize]
        [HttpPost]
        public ActionResult EditWorkAllocation(int? id, ManualReAllocationViewModel vm)
        {
            Initialise(); 

            var area = db.PreferredComplexAreas.Include(r => r.LettingOfficer).Include(r => r.HousingSuper).FirstOrDefault(x => x.Id == vm.PropertyLeaseApplicationId);

            var csoCustomerId = db.Customers.FirstOrDefault(x => x.SystemUserId == vm.LettingOfficerId).Id;
            area.LettingOfficerId = csoCustomerId;
            area.HousingSuperId = csoCustomerId;
            area.ModifiedDateTime = DateTime.Now;
            db.Entry(area).State = EntityState.Modified;
            db.SaveChanges();

            var Title = vm.TitleName;
            var Body = vm.BodyName;
            return RedirectToAction(vm.ViewName);
        }

        [Authorize]
        [DecryptParameter]
        public ActionResult EditWorkAllocationAction(int PreferredComplexAreaId, int UserId, string Action)
        {
            Initialise();
            var core = new eServicesDbContext();
            var complex = core.UserWorkAllocations.Include(t=>t.SystemUser).Where(r => r.PreferredComplexAreaId == PreferredComplexAreaId && r.SystemUserId == UserId).FirstOrDefault();
            switch (Action)
            {
                case ViewCodeKeys.OFF:
                    complex.IsActive = false;
                    complex.RRActive = false;
                    var user = complex.SystemUser;
                    break;
                case ViewCodeKeys.ON:
                    complex.IsActive = true;
                    complex.RRActive = true;
                    complex.IsDeleted = false;
                    break;
                case ViewCodeKeys.DELETE:
                    complex.IsActive = false;
                    complex.RRActive = false;
                    complex.IsDeleted = true;
                    break;
            }
            core.Entry(complex).State = EntityState.Modified;
            core.SaveChanges();

            var area = core.PreferredComplexAreas.Include(r => r.RegionType).Include(r => r.CCCType).FirstOrDefault(x => x.Id == PreferredComplexAreaId);
            var Regions = core.RegionTypes.ToList();
            var CCCs = core.CCCTypes.ToList();
            ViewBag.CCCddl = new SelectList(CCCs.OrderBy(d => d.Name), "Id", "Name");
            ViewBag.Regionddl = new SelectList(Regions.OrderBy(d => d.Name), "Id", "Name");
            ViewBag.Users = new SelectList(GetSystemUsers(area).OrderBy(d => d.Data), "Id", "Data");
            var vm = new ManualReAllocationViewModel();
            GetViewModel(vm, PreferredComplexAreaId);
            return View("EditWorkAllocation", vm);
        }

        public List< SystemUser> GetSystemUsers( PreferredComplexArea area)
        {
            var core = new eServicesDbContext();
            var users = new List<SystemUser>();
            string[] Roles2 = { "Senior Housing Specialist", "Regional Manager", "Housing Liaison Officer", "Caretaker" };
            PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
            var deletedusers = core.UserWorkAllocations.Where(d => d.PreferredComplexAreaId == area.Id && d.IsActive).ToList().Select(r => r.SystemUserId);
            foreach (var role in Roles2)
            {
                var RoleName = core.Roles.Where(x => x.Name == role).FirstOrDefault().Id;
                var rr = cc.GetUsersInRole(RoleName).ToList();
                foreach (var user in rr)
                {
                    var rrq = core.SystemUsers.FirstOrDefault(r => r.Id == user.SystemUserId && !deletedusers.Contains(r.Id));
                    if (rrq != null)
                    {
                        rrq.Data = string.Format("{0}, {1}", rrq.UserFullName, user.ServiceNo);
                        users.Add(rrq);
                    }
                }
            }
            return users;
        }

        public static ManualReAllocationViewModel GetViewModel(ManualReAllocationViewModel vm,int PreferredComplexAreaId)
        {
            var core = new eServicesDbContext();
            var rrUsers = core.UserWorkAllocations
                .Include(r => r.PreferredComplexArea.RegionType)
                .Include(r => r.SystemUser)
                .Include(r => r.PreferredComplexArea.CCCType).Where(x => x.PreferredComplexAreaId == PreferredComplexAreaId && !x.IsDeleted).ToList();
            var area = core.PreferredComplexAreas.Include(r => r.RegionType).Include(r => r.CCCType).FirstOrDefault(x => x.Id == PreferredComplexAreaId);

            vm.UserWorkAllocation_L = rrUsers;
            vm.PreferredComplexArea = area;
            vm.CurrentFullName = area.Name;
            vm.PropertyLeaseApplicationId = (int)PreferredComplexAreaId;
            vm.CurrentAssignedUserName = area.Name;
            vm.ViewName = "EHCComplexAreaDashboard";
            vm.TitleName = "EHCComplexAreaDashboard";
            vm.BodyName = "EHCComplexAreaDashboard";
            return vm;
        }

        [DecryptParameter]
        public ActionResult EditRCPUser(int? Id, string RoleId)
        { 
            Initialise();
            PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
            var role = db.Roles.FirstOrDefault(x => x.Id == RoleId);
            var cust = db.Customers.FirstOrDefault(x => x.SystemUserId == Id);

            var userrole = db.ApplicationUserRoles.FirstOrDefault(x => x.SystemUserId == (int)Id).RoleId;
            var usersinrole = db.ApplicationUserRoles.Include(r => r.SystemUser).Where(x => (bool)x.IsDeleted != true && x.RoleId == userrole).ToList();
            var rrq = usersinrole.Select(x => x.SystemUserId).ToList();
            var Users = db.SystemUsers.Where(x => rrq.Contains(x.Id)).ToList();

            var User = db.SystemUsers.FirstOrDefault(x => x.Id == Id);
            ViewBag.UserName = User.UserFullName;
            ViewBag.Role = role.Name;
            ViewBag.RoleId = RoleId;
            ViewBag.RemoveId = cust.SystemUserId;
            ViewBag.BOUsers = new SelectList(Users, "Id", "FullName");
            return View();
        }

        [HttpPost]
        public ActionResult EditRCPUser(int? RemoveId, int UserId, string Role)
        {
            using (var core = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var ClerkId = core.Customers.FirstOrDefault(x => x.SystemUserId == UserId);
                    var User = db.Customers.Where(x => x.SystemUserId == (int)RemoveId).FirstOrDefault();
                    var modify = core.AppSettings.Where(x => x.Value == User.Id.ToString() && (x.Key == AppSettingKeys.CommunityDevelopmentOfficer || x.Key == AppSettingKeys.RevenueManager || x.Key == AppSettingKeys.RevenueOfficer || x.Key == AppSettingKeys.PropertyManager)).FirstOrDefault();
                    modify.Value = ClerkId.Id.ToString();
                    core.Entry(modify).State = EntityState.Modified;
                    core.SaveChanges();
                    return RedirectToAction("RCPUserManagement");
                }
                catch (Exception)
                {
                    return RedirectToAction("_Error");
                }
              
            }
        }
        [DecryptParameter]
        public ActionResult EditIsActive(int? id, string ViewName, string TitleName, string BodyName)
        { 
            var findItem = db.PreferredComplexAreas.FirstOrDefault(x => x.Id == id);
            var result = findItem.IsActive == true ? MatchingHelper.ModifyComplexAvailability(db, (int)id, false) : MatchingHelper.ModifyComplexAvailability(db, (int)id, true);
            if (findItem.Key.Contains("h_")) return RedirectToAction("UserWorkAllocation");
            return RedirectToAction("EHCComplexAreaDashboard");
        }

        public ActionResult RoundRobbinActiveWorkQueues()
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
            {
                var Message = TempData["ApplicationFeeValidationRedistribution"];
                if (Message != null)
                {
                    ViewBag.MessageTitle = "Application Redistribution Successful";
                    ViewBag.Message = TempData["ApplicationFeeValidationRedistribution"].ToString();
                }

                var Message2 = TempData["AcknowledgementRedistribution"];
                var Title2 = TempData["AcknowledgementRedistributionTitle"];
                if (Message2 != null && Title2 != null)
                {
                    ViewBag.MessageTitle = TempData["AcknowledgementRedistribution"].ToString();
                    ViewBag.Message = TempData["AcknowledgementRedistribution"].ToString();
                }
                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();
                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                List<Customer> prr = new List<Customer>();

                if (User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.StatusId == SubmittedId && x.HumanSettlementApplicationId == null).ToList();
                    var list = rrq.Select(x => x.ClerkId).ToList();

                    prr = db.Customers.Where(x => list.Contains(x.Id)).ToList();

                    foreach (var Item in prr)
                    {
                        Item.Data = (db.RoundRobinQueues.Where(x => x.ClerkId == Item.Id && x.StatusId == SubmittedId).Count()).ToString();

                        var user = UserManager.FindByName(Item.SystemUser.UserName);
                        var userId = user.Id;
                        // get user roles
                        List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                        string RolesList = "";
                        int counter = 0;
                        foreach (var Roles in rolesArray)
                        {
                            if (counter > 0)
                            {
                                RolesList += ", ";
                            }
                            counter++;
                            RolesList += Roles;
                        }
                        Item.ColorCode = RolesList;
                    }

                }
                else
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.StatusId == SubmittedId && x.HumanSettlementApplicationId == null).ToList();
                    var list = rrq.Select(x => x.ClerkId).ToList();

                    prr = db.Customers.Where(x => list.Contains(x.Id)).ToList();

                    foreach (var Item in prr)
                    {
                        Item.Data = (db.RoundRobinQueues.Where(x => x.ClerkId == Item.Id && x.StatusId == SubmittedId).Count()).ToString();

                        var user = UserManager.FindByName(Item.SystemUser.UserName);
                        var userId = user.Id;
                        // get user roles
                        List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                        string RolesList = "";
                        int counter = 0;
                        foreach (var Roles in rolesArray)
                        {
                            if (counter > 0)
                            {
                                RolesList += ", ";
                            }
                            counter++;
                            RolesList += Roles;
                        }
                        Item.ColorCode = RolesList;
                    }
                }

                return View(prr);

            }

            return View();

        }

        public ActionResult RCPUserManagement()
        {
            Initialise();
            if (User.IsInRole("Back Office System Administrator") || User.IsInRole("Support Admin")|| User.IsInRole("Area Manager"))
            {
                var applicationName = "";
                var rrpUsers = db.AppSettings.Where(x => (x.Key == AppSettingKeys.CommunityDevelopmentOfficer || x.Key == AppSettingKeys.RevenueManager || x.Key == AppSettingKeys.RevenueOfficer || x.Key == AppSettingKeys.PropertyManager)).ToList();
                var rrq = rrpUsers.Select(x => int.Parse(x.Value)).ToList();
                var UserInfo = db.Customers.Where(x => rrq.Contains(x.Id)).ToList();
                List<SystemUser> users = new List<SystemUser>();
               var application = db.Applications.Where(x=>x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();
                applicationName = application.Name;
                foreach (var Item in UserInfo)
                {
                    var userrole = db.ApplicationUserRoles.Include(r=>r.IdentityRole).FirstOrDefault(x => x.SystemUserId == (int)Item.SystemUserId);
                    var user = db.SystemUsers.FirstOrDefault(x => x.Id == Item.SystemUserId);
                    user.Data = userrole.IdentityRole.Name;
                    user.Designation = userrole.IdentityRole.Id;
                    users.Add(user);
                }
                ViewBag.ApplicationName = applicationName;
                return View(users);
            }
            return View("_Error");
        }
        //
        [DecryptParameter]
        public ActionResult EditRRUser(int Id, string data)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    if (Id == 0) throw new Exception("Invalid User");
                    var Users = context.SystemUsers.Where(x => x.Id == Id);
                    ViewBag.Name = Users.FirstOrDefault().UserFullName;
                    var userrole = context.ApplicationUserRoles.FirstOrDefault(x => x.SystemUserId ==(int)Id).RoleId;
                    var usersinrole = context.ApplicationUserRoles.Include(r => r.SystemUser).Where(x => (bool)x.IsDeleted != true && x.RoleId == userrole).ToList();
                    var rrq = usersinrole.Select(x => x.SystemUserId).ToList();
                    var userlist = context.Customers.Where(x => rrq.Contains((int)x.SystemUserId)).ToList();
                    ViewBag.TypeofProperty = new SelectList(userlist.ToList(), "Id", "UserFullName");
                    ViewBag.Id = Id;
                    var Message = TempData["RoundRobinRedistributionTitle"];
                    var Title = TempData["RoundRobinRedistribution"];
                    if (Message != null && Title != null)
                    {
                        ViewBag.MessageTitle = TempData["RoundRobinRedistributionTitle"].ToString();
                        ViewBag.Message = TempData["RoundRobinRedistribution"].ToString();
                    }
                    return View(Users);
                }
                catch
                {
                    return View("_Error");
                }
            }
        }


        [HttpPost]
        [DecryptParameter]
        public ActionResult EditRRUser(int Id, bool ReAllocateCases, int AssignToId)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    if (ReAllocateCases == true)
                    {
                        var Keys = context.Status;
                        int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                        var Clerk = context.Customers.Where(x => x.SystemUserId == Id).FirstOrDefault();
                        var rrqList = context.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.ResponsibilityType).Include(x => x.Clerk.SystemUser).Include(x => x.LeaseDetails).Include(x => x.PropertyLeaseApplication).Where(x => x.ClerkId == Clerk.Id && x.StatusId == SubmittedId).ToList();
                        AesCrypto AES = new AesCrypto();
                        var q = AES.Encrypt("id=" + Id + "&" + "data=" + rrqList);
                        int SystUserId = Id;
                        foreach (var item in rrqList)
                        {
                            DepartmentsApprovalsController DA = new DepartmentsApprovalsController();
                            CaptureController c = new CaptureController();
                            switch (item.ResponsibilityType.Key)
                            {
                                
                                case (ResponsibilityTypeKeys.RiskAssessment):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.ValidateDepositPayment):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.InviteToClientTraining):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.ScheduleInspectionSlots):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.Inspections):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.MaintananceJobSheet):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.CaptureLeaseDetails):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.GenerateLeaseAgreement):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.LeaseAgreementValidation):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.DebitOrderVAlidation):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.Terminations):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.TerminationValidation):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.CommitteeOutcomes):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.VacatingConfirmation):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.LeaseRenewals):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.SecondLeaseRenewal):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.LeaseRenewalRevenue):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.RenewalRiskAssessment):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.UnitMaintenanance):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUser(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        break;
                                    }
                            }
                        }
                    }

                    // Update it with the values from the view model
                    //user.RoundRobinIsActive = rrIsActive;
                    //UserManager.Update(user);

                    return RedirectToAction("RoundRobbinActiveWorkQueues");
                }
                catch
                {
                    return View("_Error");
                }
            }
        }

        [Authorize(Roles = "Back Office System Administrator, Super Administrators")]
        public ActionResult AdminInbox()
        {
            Initialise();
            List<ClerkRegistration> clerkRegistrations = new List<ClerkRegistration>();
            var CurrentUserModule = db.ApplicationEntities.Find(SystemUser.DepartmentId).Key;
            clerkRegistrations = db.ClerkRegistrations.Where(x => x.Status.Key == StatusKeys.AccountPending)
                .Include(c => c.Department)
                .Include(c => c.CCCType)
                .Include(c => c.CreatedBySystemUser)
                .Include(c => c.Status)
                .Include(c => c.ModifiedBySystemUser)
                .Include(c => c.NotificationType).ToList();

            if (CurrentUserModule != null) clerkRegistrations = clerkRegistrations.Where(d => d.Department?.Key == CurrentUserModule).ToList();

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                ViewBag.MessageTitle = "User Created";

            }
            return View(clerkRegistrations.ToList());
        }
        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        public ActionResult Redirect()
        {
            Initialise();


          
            if (User.IsInRole("Back Office System Administrator"))
            {
                RedirectToAction("AdminInbox");
            }
            else
            {
                RedirectToAction("Index");
            }

            return View("Index");
        }



        // GET: AreaManager/Details/5
        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClerkRegistration clerkRegistration = db.ClerkRegistrations.Find(id);
            if (clerkRegistration == null)
            {
                return HttpNotFound();
            }
            return View(clerkRegistration);
        }

        // GET: AreaManager/Create
        //public ActionResult Create()
        //{
        //    ViewBag.CCCTypeId = new SelectList(db.CCCTypes, "Id", "Name");
        //    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
        //    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
        //    ViewBag.NotificationTypeId = new SelectList(db.NotificationTypes, "Id", "Name");
        //    return View();
        //}

        //// POST: AreaManager/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "Id,FirstName,LastName,UserName,EmployeeNumber,RoleArray,EmailAddress,SystemUserTypeId,StatusId,IsPasswordReset,IsTemporaryPassword,IdentificationNumber,MobileNumber,Code,NotificationTypeId,CCCTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] ClerkRegistration clerkRegistration)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.ClerkRegistrations.Add(clerkRegistration);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.CCCTypeId = new SelectList(db.CCCTypes, "Id", "Name", clerkRegistration.CCCTypeId);
        //    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.CreatedBySystemUserId);
        //    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.ModifiedBySystemUserId);
        //    ViewBag.NotificationTypeId = new SelectList(db.NotificationTypes, "Id", "Name", clerkRegistration.NotificationTypeId);
        //    return View(clerkRegistration);
        //}

        // GET: AreaManager/Edit/5

        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        public ActionResult Edit(int? id)
        {
            using (var context = new eServicesDbContext())
            { 
                if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
   
            ClerkRegistration clerkRegistration = db.ClerkRegistrations.Find(id);
            if (clerkRegistration == null)
            {
                return HttpNotFound();
            }
                var vm = new InternalRegisterViewModel();
                vm.ClerkRegID = clerkRegistration.Id;
                vm.FirstName = clerkRegistration.FirstName;
                vm.LastName = clerkRegistration.LastName;
                vm.IdentificationNumber = clerkRegistration.IdentificationNumber;
                vm.UserName = clerkRegistration.UserName;
                vm.MobileNumber = clerkRegistration.MobileNumber;
                vm.ConfirmMobileNumber = clerkRegistration.MobileNumber;
                vm.EmailAddress = clerkRegistration.EmailAddress;
                vm.ConfirmEmailAddress = clerkRegistration.EmailAddress;
                vm.EmployeeNumber = clerkRegistration.EmployeeNumber;
                ViewBag.CCCTypes = new SelectList(db.CCCTypes, "Id", "Name", clerkRegistration.CCCTypeId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.ModifiedBySystemUserId);
            ViewBag.NotificationTypeId = new SelectList(db.NotificationTypes, "Id", "Name", clerkRegistration.NotificationTypeId);
                var currentRoleApplication = context.ApplicationRoles.Where(ar => ar.IsActive && ar.IsDeleted == false
                                && ar.Application.Key == ApplicationKeys.RatesClearanceSystem).Select(cr => cr.RoleId);
                //var roles = context.Roles.Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Issue Refunds Collection" || r.Name == "Acknowledge Refund Application" || r.Name == "Area Manager" || r.Name == "Internal Registration" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Rates Clearance Senior Clerk" || r.Name == "Rates" || r.Name == "Billing" || r.Name == "Sundry Account" || r.Name == "Credit Control" || r.Name == "Acknowledge RCS Application" || r.Name == "Rates Clearance Sectional Head" || r.Name == "Support Admin" || r.Name == "Support Clerk" || r.Name == "Back Office System Administrator").ToList();
                var roles = context.Roles
                                        .Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Property Manager" || r.Name == "Housing Supervisor" || r.Name == "Finance Administrator" || r.Name == "Revenue Manager" || r.Name == "Community Development Officer"
                                        || r.Name == "Letting Officer" || r.Name == "Revenue Officer" || r.Name == "Back Office System Administrator" || r.Name == "Area Manager" || r.Name == "Senior Manager" || r.Name == "Senior specialist" || r.Name == "Housing liaison officer"
                                        || r.Name == "Maintenance Supervisor" || r.Name == "Caretaker").ToList(); vm.AdUser = clerkRegistration.IsActiveDirectoryUser;
                vm.Features = new SelectList(roles, "Name", "Name");
                var RolesArrayTemp = context.ClerkRoles.Where(x => x.ClerkRegistrationId == clerkRegistration.Id);
                List<string> RolesArray = new List<string>();
                foreach (var item in RolesArrayTemp)
                {
                    RolesArray.Add(item.RoleName);
                }
                var RolesList2 = roles.Select(x => new SelectListItem()
                {
                    Selected = RolesArray.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                });
                vm.Features = RolesList2;
                vm.CCC = clerkRegistration.CCCTypeId;
                return View(vm);
            }
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(InternalRegisterViewModel model, string[] SelectedRoles)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    //SelectedRoles[0] = "Back Office System Administrator";
                    var currentRoleApplication = context.ApplicationRoles.Where(ar => ar.IsActive && ar.IsDeleted == false
                                && ar.Application.Key == ApplicationKeys.RatesClearanceSystem).Select(cr => cr.RoleId);
                    //var roles = context.Roles.Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Issue Refunds Collection" || r.Name == "Acknowledge Refund Application" || r.Name == "Area Manager" || r.Name == "Internal Registration" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Rates Clearance Senior Clerk" || r.Name == "Rates" || r.Name == "Billing" || r.Name == "Sundry Account" || r.Name == "Credit Control" || r.Name == "Acknowledge RCS Application" || r.Name == "Rates Clearance Sectional Head" || r.Name == "Support Admin" || r.Name == "Support Clerk"|| r.Name == "Back Office System Administrator").ToList();
                    var roles = context.Roles
                                            .Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Property Manager" || r.Name == "Housing Supervisor" || r.Name == "Finance Administrator" || r.Name == "Revenue Manager" || r.Name == "Community Development Officer"
                                            || r.Name == "Letting Officer" || r.Name == "Revenue Officer" || r.Name == "Back Office System Administrator" || r.Name == "Area Manager" || r.Name == "Senior Manager" || r.Name == "Senior specialist" || r.Name == "Housing liaison officer"
                                            || r.Name == "Maintenance Supervisor" || r.Name == "Caretaker").ToList();

                    var SelectedCCC = context.CCCs.Where(x => x.CCCTypeId == model.CCC).FirstOrDefault();
                    var RolesArrayTemp = context.ClerkRoles.Where(x => x.ClerkRegistrationId == model.ClerkRegID);
                    List<string> RolesArray = new List<string>();
                    foreach (var item in RolesArrayTemp)
                    {
                        RolesArray.Add(item.RoleName);
                    }
                    var RolesList2 = roles.Select(x => new SelectListItem()
                    {
                        Selected = RolesArray.Contains(x.Name),
                        Text = x.Name,
                        Value = x.Name
                    });
                    model.Features = RolesList2;

                    ViewBag.CCCTypes = new SelectList(db.CCCTypes, "Id", "Name", model.CCC);
                    _base.Initialise(context);
                    if (ModelState.IsValid)
                    {
                        var randomPassword = GeneratePassword(10);
                        var email = new Email();
                        var identityManager = new IdentityManager();
                  
                        var applicationUserRole = new ApplicationUserRole
                        {
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false,
                            CCCId=SelectedCCC.Id,
                            ApplicationId = context.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault().Id,
                            RoleId = RoleManager.FindByName(SelectedRoles[0]).Id
                        };

                     

                        var defaultCustomer = context.CustomerTypes.FirstOrDefault(c => c.Key == CustomerTypeKeys.Individual);
                        var defaultIdentification = context.IdentificationTypes.FirstOrDefault(id => id.Key == IdentificationTypeKey.SouthAfricanID);
                        var defaultTitleType = context.TitleTypes.FirstOrDefault(t => t.Key == TitleTypeKeys.Mister);
                        var defaultStatus = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);

                        if (defaultCustomer == null) throw new Exception("Invalid customer type");
                        if (defaultIdentification == null) throw new Exception("Invalid identification");
                        if (defaultTitleType == null) throw new Exception("Invalid title type");
                        if (defaultStatus == null) throw new Exception("Invalid status");

                        var customer = new Customer()
                        {
                            CustomerTypeId = defaultCustomer.Id,
                            IdentificationTypeId = defaultIdentification.Id,
                            CountryOfIssueTypeId = null,
                            IdentificationNumber = model.IdentificationNumber,
                            TitleTypeId = defaultTitleType.Id,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            CellPhoneNumber = model.MobileNumber,
                            Gender = null,
                            IsDeceased = false,
                            EmailAddress = model.EmailAddress,
                            PhysicalAddressCode = 0000,
                            PostalAddressCode = 0000,
                            StatusId = defaultStatus.Id,
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false
                        };
                        var firstName = model.FirstName;
                        var surname = model.LastName;
                        var username = model.UserName;
                        var emailAddress = model.EmailAddress;
                        var serviceNo = model.EmployeeNumber;
                        var usernameAssigned = context.SystemUsers.Any(u => u.UserName.ToLower() == username.ToLower()
                            && u.IsActive && u.IsDeleted == false);
                        var emailAssigned = context.SystemUsers.Any(u => u.EmailAddress.ToLower() == emailAddress.ToLower()
                            && u.IsActive && u.IsDeleted == false);
                        var response = "";
                        var Statuses = context.Status.Where(x => x.Key == StatusKeys.AccountPending).FirstOrDefault();
                        if (!emailAssigned)
                        {
                            emailAssigned = context.ClerkRegistrations.Include(x => x.Status).Any(u => u.EmailAddress.ToLower() == model.EmailAddress.ToLower() && u.IsActive && u.IsDeleted == false &&u.Id !=model.ClerkRegID && u.StatusId == Statuses.Id);
                        }
                        //if (!mobileAssigned)
                        //{
                        //    mobileAssigned = context.ClerkRegistrations.Include(x => x.Status).Any(u => u.MobileNumber == model.MobileNumber && u.IsActive && u.IsDeleted == false && u.StatusId == Statuses.Id);
                        //}
                        if (!usernameAssigned)
                        {
                            usernameAssigned = context.ClerkRegistrations.Include(x => x.Status).Any(u => u.UserName.ToLower() == model.UserName.ToLower() && u.IsActive && u.IsDeleted == false && u.Id != model.ClerkRegID && u.StatusId == Statuses.Id);
                        }
                        if (!usernameAssigned && emailAssigned)
                        {
                            response = "Email address registered. Please use an alternative email address";
                        }
                        else if (usernameAssigned && !emailAssigned)
                        {
                            response = "Username registered. Please choose a unique username.";
                        }
                        else if (usernameAssigned && emailAssigned)
                        {
                            response = "Username and Email address registered. Please use an alternative email address and a unique username.";
                        }
                        else
                        {
                            // JK.20140724a - Passing values from the ViewModel to the Model.
                            var user = new SystemIdentityUser
                            {
                                UserName = username,
                                Email = emailAddress,
                                EmailConfirmed = true,
                                PhoneNumber = model.MobileNumber,
                                isInternalUser = true,
                                isActiveDirectoryUser = model.AdUser,
                                RoundRobinIsActive=true,
                                CCCId = SelectedCCC.Id,
                                ServiceNo=serviceNo,
                                SystemUser = new SystemUser()
                                {
                                    FirstName = firstName,
                                    LastName = surname,
                                    UserName = username,
                                    MobileNumber = model.MobileNumber,
                                    IdentificationNumber = model.IdentificationNumber,
                                    EmailAddress = emailAddress,
                                    IsPasswordReset = model.AdUser,
                                    CCCId = SelectedCCC.Id,
                                    ServiceNo = serviceNo,
                                    IsActive = true,
                                    IsDeleted = false,
                                    IsLocked = false,
                                    ModifiedDateTime = DateTime.Now
                                }
                            };

                            // JK.20140724a - Custom profile information.

                            // Send email to User with Username and Temp Password
                            //var result = identityManager.CreateUser(user, randomPassword);
                            var result = await UserManager.CreateAsync(user, randomPassword);


                            foreach (var item in SelectedRoles)
                            {
                                identityManager.AddUserToRole(user.Id, item.ToString());
                            }

                            applicationUserRole.SystemUserId = user.SystemUserId;
                            customer.SystemUserId = user.SystemUserId;

                            context.ApplicationUserRoles.Add(applicationUserRole);
                            context.Customers.Add(customer);
                            context.SaveChanges();


                            foreach (var item in SelectedRoles)
                            {
                                var appUserRole = new AppUserRole
                                {
                                    IsActive = true,
                                    IsDeleted = false,
                                    IsLocked = false,
                                    RoleId = RoleManager.FindByName(item).Id,
                                    ApplicationUserRoleId = applicationUserRole.Id
                                };

                                context.AppUserRoles.Add(appUserRole);
                                context.SaveChanges();
                            }
                         
                            if (result.Succeeded)
                            {
                                string role = "";
                                foreach (var item in SelectedRoles)
                                {
                                    if(role == "")
                                    {
                                        role = role + item.ToString();
                                    }
                                    else
                                    {
                                        role = role + ", " + item.ToString() ;
                                    }
                                    
                                }

                                var applicationAccess = context.Applications.Find(applicationUserRole.ApplicationId);
                                const string emailSubject = "Property Lease Management System: User Registration";
                                string emailBody = "";
                                if (model.AdUser == false)
                                {
                                     emailBody = "<b>You have been successfully added onto Property Lease Management System.</b><br/><br/>" +
                                   "<b>Login Details:</b><br/>" +
                                   "Username: " + user.UserName + "<br/>" +
                                   "Temporary Password: " + randomPassword + "<br/>" +
                                   "Application Access: " + applicationAccess.Name + "<br/>" +
                                   "Role: " + role + "<br/><br/>" +
                                   "<b> Please change temporary password on your first login.</b>";

                                }
                                else
                                {
                                     emailBody = "<b>You have been successfully added onto Property Lease Management System.</b><br/><br/>" +
                                   "<b>Login Details:</b><br/>" +
                                   "Active Directory Username: " + user.UserName + "<br/>" +
                                   "Password: " + "Please use your own Active Directory password" + "<br/>" +
                                   "Application Access: " + applicationAccess.Name + "<br/>" +
                                   "Role: " + role + "<br/><br/>";

                                }


                                email.GenerateEmail(user.Email, emailSubject, emailBody, user.SystemUserId.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.SystemUser.FullName);
                                var clerkreg = context.ClerkRegistrations.Find(model.ClerkRegID);
                                clerkreg.StatusId = context.Status.Where(x => x.Key == StatusKeys.AccountActive).FirstOrDefault().Id;
                                context.Entry(clerkreg).State = EntityState.Modified;
                                context.SaveChanges();
                                response = "Success";
                                //ViewBag.EmailAddress = model.EmailAddress == null || model.EmailAddress.Trim() == string.Empty ? model.MobileNumber : model.EmailAddress;
                            }
                            else
                            {
                                AddErrors(result);
                            }
                            ViewBag.Response = response;
                            //return View();
                            
                            ViewBag.EmailAddress = model.EmailAddress == null || model.EmailAddress.Trim() == string.Empty ? model.MobileNumber : model.EmailAddress;
                            if (model.AdUser == false)
                            {
                                TempData["Message"] = "Username: " + user.UserName + " was successfully created, a temporary password has been emailed.";

                            }
                            else
                            {
                                TempData["Message"] = "Username: " + user.UserName + " was successfully created, active directory login has been enabled for user.";
                              
                            }
                            if (User.IsInRole("Back Office System Administrator"))
                            {
                                return RedirectToAction("AdminInbox");
                            }
                                return RedirectToAction("Index");
                        }
                        ModelState.AddModelError("", response);
                    }
                }
                catch(Exception IO)
                {
                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }

            //db.Entry(clerkRegistration).State = EntityState.Modified;
            //db.SaveChanges();
            return View(model);

            //ViewBag.CCCTypeId = new SelectList(db.CCCTypes, "Id", "Name", clerkRegistration.CCCTypeId);
            //ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.CreatedBySystemUserId);
            //ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.ModifiedBySystemUserId);
            //ViewBag.NotificationTypeId = new SelectList(db.NotificationTypes, "Id", "Name", clerkRegistration.NotificationTypeId);
            //return View();
        }
        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        public ActionResult WaitingListQueueManagement()
        {
            Initialise();
            //MatchingHelper.ModifyWaitinglistPositions(db);
            MatchingHelper.MarkLeaseInfoAsCompleted(db);
            using (var core = new eServicesDbContext())
            {
                List<TemporaryDisplayModel> WaitingListQueueData = new List<TemporaryDisplayModel>();
                var Queue = core.waitingListQues.OrderBy(r=>r.Position).Include(r => r.PropertyLeaseApplication).Where(x => x.Position != null).ToList();
                var rrq = Queue.Select(x => x.PropertyLeaseApplicationId).ToList();
                var test = Queue.Select(x => new { x.PropertyLeaseApplicationId, x.QueueDate }).ToList();
                var findItem = core.PropertyLeaseApplications.Include(r => r.PreferredComplexArea).Include(r => r.PreferredComplexArea2).Include(r => r.HumanEHCOptions).Where(x => rrq.Contains(x.Id) && x.IsActive).ToList();
                foreach (var p in findItem)
                {
                    var data = new TemporaryDisplayModel();
                    var q = Queue.Where(x => x.PropertyLeaseApplicationId == p.Id).FirstOrDefault();
                    data.WaitingListQueueId = q.Id;
                    data.Order = (int)q.Position;
                    data.WaitingListQueueDate = q.QueueDate.ToString().Substring(0, 10);
                    data.IsRelisted = q.IsReListed;
                    data.Position = "Waiting List No. " + ((int)q.Position).ToString();
                    data.ApplicantFullName = p.FirstName + " " + p.LastName;
                    data.ApplicationReferenceNo = p.ApplicationReferenceNumber;
                    data.BedRooms = p.HumanEHCOptions.Name;
                    data.PropertyLeaseApplicationId = (int)q.PropertyLeaseApplicationId;
                    data.ComplexAreas = p.PreferredComplexArea.Name == p.PreferredComplexArea2.Name ? p.PreferredComplexArea.Name : p.PreferredComplexArea.Name + ", " + p.PreferredComplexArea2.Name;
                    WaitingListQueueData.Add(data);
                }
                ViewBag.QueueSum = WaitingListQueueData.Count();
                //WaitingListQueueData.OrderBy(x => x.Order);
                return View(WaitingListQueueData);
            }
        }

        public ActionResult CancelWaitingListQueueItem(int Id)
        {
            bool result = MatchingHelper.RemoveWaitingListQueueItem(db, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ValidateWaitingListSorting()
        {
            var val = db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting).Value;
            bool result = Convert.ToBoolean(Convert.ToInt16(db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting).Value));
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult MoveWaitingListQueueItemPosition(int WaitingListQueueId, int Queue, string ApplicationReferenceNo, int PropertyLeaseApplicationId, int? MoveToPosition, string ApplicantFullName, string Position)
        {
            try
            {
                int p = (int)db.waitingListQues.FirstOrDefault(x => x.Id == WaitingListQueueId).Position;
                TemporaryDisplayModel vm = new TemporaryDisplayModel()
                {
                    WaitingListQueueId = WaitingListQueueId,
                    Queue = Queue,
                    Position = Position,
                    ApplicationReferenceNo = ApplicationReferenceNo,
                    MoveToPosition = (int)MoveToPosition,
                    PropertyLeaseApplicationId = PropertyLeaseApplicationId,
                    ApplicantFullName = ApplicantFullName,
                    ItemPosition = p
                };
                return View(vm);
            }
            catch (Exception IO)
            {
                throw;
            }
        }

        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveWaitingListQueueItemPosition(TemporaryDisplayModel vm)
        {
            using (var core = new eServicesDbContext())
            {
                MatchingHelper.MoveToPositionWaitingListQueue(core, vm.WaitingListQueueId, vm.MoveToPosition);
                return RedirectToAction("WaitingListQueueManagement");
            }
        }
        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult RemoveWaitingListQueueItem(int WaitingListQueueId, int Queue, string ApplicationReferenceNo, int PropertyLeaseApplicationId, int? MoveToPosition, string ApplicantFullName, string Position)
        {
            try
            {
                int p = (int)db.waitingListQues.FirstOrDefault(x => x.Id == WaitingListQueueId).Position;
                TemporaryDisplayModel vm = new TemporaryDisplayModel()
                {
                    WaitingListQueueId = WaitingListQueueId,
                    Queue = Queue,
                    Position = Position,
                    ApplicationReferenceNo = ApplicationReferenceNo,
                    MoveToPosition = (int)MoveToPosition,
                    PropertyLeaseApplicationId = PropertyLeaseApplicationId,
                    ApplicantFullName = ApplicantFullName,
                    ItemPosition = p
                };
                ViewBag.ApprovalStatus = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
                return View(vm);
            }
            catch (Exception IO)
            {
                throw;
            }
        }

        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveWaitingListQueueItem(TemporaryDisplayModel vm)
        {
            try
            {
                using (var core = new eServicesDbContext())
                {
                    MatchingHelper.RemoveApplicationFromWaitingList(core, vm.WaitingListQueueId);
                    return RedirectToAction("WaitingListQueueManagement");
                }
            }
            catch (Exception Io)
            {

                return View("_Error");
            }
        }
        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult ModifyApplicationUnitRequiremets(int WaitingListQueueId, int Queue, string ApplicationReferenceNo, int PropertyLeaseApplicationId, int? MoveToPosition, string ApplicantFullName, string Position)
        {
            try
            {
                int p = (int)db.waitingListQues.FirstOrDefault(x => x.Id == WaitingListQueueId).Position;
                var pp = db.PropertyLeaseApplications.Include(r => r.PreferredComplexArea).Include(r => r.PreferredComplexArea2).Include(r => r.HumanEHCOptions).Where(x => x.Id == PropertyLeaseApplicationId).FirstOrDefault();
                TemporaryDisplayModel vm = new TemporaryDisplayModel()
                {
                    WaitingListQueueId = WaitingListQueueId,
                    Queue = Queue,
                    Position = Position,
                    ApplicationReferenceNo = ApplicationReferenceNo,
                    MoveToPosition = (int)MoveToPosition,
                    PropertyLeaseApplicationId = PropertyLeaseApplicationId,
                    ApplicantFullName = ApplicantFullName,
                    ItemPosition = p,
                    ComplexAreas = pp.PreferredComplexAreaId != pp.PreferredComplexArea2Id ? pp.PreferredComplexArea.Name + ", " + pp.PreferredComplexArea2.Name : pp.PreferredComplexArea.Name,
                    BedRooms = pp.HumanEHCOptions.Name
                };
                List<string> rrq = new List<string> { PrefferedComplexKeys.Airportpark, PrefferedComplexKeys.AirportparkStaff, PrefferedComplexKeys.Delville,
                    PrefferedComplexKeys.DelvilleStaff, PrefferedComplexKeys.PharoePark, PrefferedComplexKeys.ChrisHani, PrefferedComplexKeys.ChrisHaniStaff, PrefferedComplexKeys.NewDelville, PrefferedComplexKeys.BlockE};

                ViewBag.ComplexOptionOne = new SelectList(db.PreferredComplexAreas.Where(x => x.IsActive && x.Key != "ekurhuleni_complex").ToList(), "Id", "Name");
                ViewBag.ComplexOptionTwo = new SelectList(db.PreferredComplexAreas.Where(x => x.IsActive && x.Key != "ekurhuleni_complex").ToList(), "Id", "Name");
                ViewBag.HumanEHCOptions = new SelectList(db.humanEHCOptions.Where(x => x.IsActive).ToList(), "Id", "Name");
                return View(vm);
            }
            catch (Exception IO)
            {
                throw;
            }
        }

        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModifyApplicationUnitRequiremets(TemporaryDisplayModel vm, int ComplexOptionOneId, int ComplexOptionTwoId, int HumanEHCOptionsId)
        {
            using (var core = new eServicesDbContext())
            {
                var p = core.PropertyLeaseApplications.Where(x => x.Id == vm.PropertyLeaseApplicationId).FirstOrDefault();
                p.PreferredComplexAreaId = ComplexOptionOneId;
                p.PreferredComplexArea2Id = ComplexOptionTwoId;
                p.HumanEHCOptionsId = HumanEHCOptionsId;
                core.Entry(p).State = EntityState.Modified;
                core.SaveChanges();
                return RedirectToAction("WaitingListQueueManagement");
            }
        }

        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        public ActionResult AddApplicationInWaitingListQueue()
        {
            try
            {
                var matched = db.MatchedUnits.Where(x=>x.PropertyLeaseApplicationId!=null).ToList();
                var rq = matched.Select(x => x.PropertyLeaseApplicationId).ToList();
                var wait = db.waitingListQues.Where(x => x.IsMatched &&x.Position==null&& x.PropertyLeaseApplicationId != null).ToList();
                var w = wait.Select(x => x.PropertyLeaseApplicationId).ToList();
                var position = db.waitingListQues.Where(x =>  x.Position != null&& x.PropertyLeaseApplicationId != null).ToList();
                var p = position.Select(x => x.PropertyLeaseApplicationId).ToList();
                var list = new List<int> ();
                foreach(var Item in rq)
                {
                    list.Add((int)Item);
                }
                foreach(var Item in w)
                {
                    list.Add((int)Item);
                }
                foreach(var Item in p)
                {
                    list.Add((int)Item);
                }
                var q = db.PropertyLeaseApplications.Where(x => !list.Contains(x.Id)).ToList();
                ViewBag.PropertyLeaseApplicationId = new SelectList(q,"Id", "ApplicationReferenceNumber");


                //int p = (int)db.waitingListQues.FirstOrDefault(x => x.Id == WaitingListQueueId).Position;
                //var pp = db.PropertyLeaseApplications.Include(r => r.PreferredComplexArea).Include(r => r.PreferredComplexArea2).Include(r => r.HumanEHCOptions).Where(x => x.Id == PropertyLeaseApplicationId).FirstOrDefault();
                //TemporaryDisplayModel vm = new TemporaryDisplayModel()
                //{
                //    WaitingListQueueId = WaitingListQueueId,
                //    Queue = Queue,
                //    Position = Position,
                //    ApplicationReferenceNo = ApplicationReferenceNo,
                //    MoveToPosition = (int)MoveToPosition,
                //    PropertyLeaseApplicationId = PropertyLeaseApplicationId,
                //    ApplicantFullName = ApplicantFullName,
                //    ItemPosition = p,
                //    ComplexAreas = pp.PreferredComplexAreaId != pp.PreferredComplexArea2Id ? pp.PreferredComplexArea.Name + ", " + pp.PreferredComplexArea2.Name : pp.PreferredComplexArea.Name,
                //    BedRooms = pp.HumanEHCOptions.Name
                //};

                List<string> rrq = new List<string> { PrefferedComplexKeys.Airportpark, PrefferedComplexKeys.AirportparkStaff, PrefferedComplexKeys.Delville,
                    PrefferedComplexKeys.DelvilleStaff, PrefferedComplexKeys.PharoePark, PrefferedComplexKeys.ChrisHani, PrefferedComplexKeys.ChrisHaniStaff, PrefferedComplexKeys.NewDelville, PrefferedComplexKeys.BlockE};

                ViewBag.ComplexOptionOne = new SelectList(db.PreferredComplexAreas.Where(x => x.IsActive && rrq.Contains(x.Key)).ToList(), "Id", "Name");
                ViewBag.ComplexOptionTwo = new SelectList(db.PreferredComplexAreas.Where(x => x.IsActive && rrq.Contains(x.Key)).ToList(), "Id", "Name");
                ViewBag.HumanEHCOptions = new SelectList(db.humanEHCOptions.Where(x => x.IsActive).ToList(), "Id", "Name");
                return View(/*vm*/);
            }
            catch (Exception IO)
            {
                return View("_Error");
            }
        }

        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddApplicationInWaitingListQueue(int Id)
        {
            try
            {
                Initialise();
                using (var core = new eServicesDbContext())
                {
                    var Keys = core.Status.ToList();
                    var await = Keys.Where(x => x.Key == StatusKeys.AwaitingUnitOffers).FirstOrDefault().Id;
                    var p = core.PropertyLeaseApplications.Where(x => x.Id == Id).FirstOrDefault();
                    var wait = core.waitingListQues.Where(x => x.PropertyLeaseApplicationId == Id).FirstOrDefault();
                    if (wait != null)
                    {
                        wait.IsMatched = false;
                        core.Entry(wait).State = EntityState.Modified;
                        core.SaveChanges();

                        MatchingHelper.AddWaitingListPosition(core, wait.Id);

                        p.StatusId = await;
                        core.Entry(p).State = EntityState.Modified;
                        core.SaveChanges();
                    }
                    else
                    {
                        var QueueItem = new WaitingListQue
                        {
                            CreatedBySystemUserId = SystemUser.Id,
                            PropertyLeaseApplicationId = Id,
                            QueueDate = DateTime.Now,
                            IsMatched = false
                        };
                        core.waitingListQues.Add(QueueItem);
                        core.SaveChanges();

                        MatchingHelper.AddWaitingListPosition(core, QueueItem.Id);

                        p.StatusId = await;
                        core.Entry(p).State = EntityState.Modified;
                        core.SaveChanges();
                    }

                    //var p = core.PropertyLeaseApplications.Where(x => x.Id == vm.PropertyLeaseApplicationId).FirstOrDefault();
                    //p.PreferredComplexAreaId = ComplexOptionOneId;
                    //p.PreferredComplexArea2Id = ComplexOptionTwoId;
                    //p.HumanEHCOptionsId = HumanEHCOptionsId;
                    //core.Entry(p).State = EntityState.Modified;
                    //core.SaveChanges();
                    return RedirectToAction("WaitingListQueueManagement");
                }
            }
            catch (Exception)
            {
                return View("_Error");
            }
          
        }

        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        public ActionResult AddUser(ClerkRegistration clerkRegistration)
        {
            using (var context = new eServicesDbContext())
            {
                Initialise();
                var vm = new InternalRegisterViewModel();
                vm.ClerkRegID = clerkRegistration.Id;
                vm.FirstName = clerkRegistration.FirstName;
                vm.LastName = clerkRegistration.LastName;
                vm.IdentificationNumber = clerkRegistration.IdentificationNumber;
                vm.UserName = clerkRegistration.UserName;
                vm.MobileNumber = clerkRegistration.MobileNumber;
                vm.ConfirmMobileNumber = clerkRegistration.MobileNumber;
                vm.EmailAddress = clerkRegistration.EmailAddress;
                vm.ConfirmEmailAddress = clerkRegistration.EmailAddress;
                vm.EmployeeNumber = clerkRegistration.EmployeeNumber;
                ViewBag.CCCTypes = new SelectList(db.CCCTypes.OrderBy(r=>r.Name).Where(x => x.IsActive), "Id", "Name");
                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.CreatedBySystemUserId);
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.ModifiedBySystemUserId);
                ViewBag.NotificationTypeId = new SelectList(db.NotificationTypes, "Id", "Name", clerkRegistration.NotificationTypeId);
                ViewBag.DepartmentId = new SelectList(SystemUser.DepartmentId != null ? db.ApplicationEntities.Where(a => a.Id == SystemUser.DepartmentId) : db.ApplicationEntities.OrderByDescending(r => r.Id), "Id", "Name");
                var currentRoleApplication = context.ApplicationRoles.Where(ar => ar.IsActive && ar.IsDeleted == false                                                                                                                                                                                                                                                                                                                              //"Area Manager,Back Office System Administrator"
                                && ar.Application.Key == ApplicationKeys.RatesClearanceSystem).Select(cr => cr.RoleId);
                //var roles = context.Roles.Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Issue Refunds Collection" || r.Name == "Acknowledge Refund Application" || r.Name == "Area Manager" || r.Name == "Internal Registration" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Rates Clearance Senior Clerk" || r.Name == "Rates" || r.Name == "Billing" || r.Name == "Sundry Account" || r.Name == "Credit Control" || r.Name == "Acknowledge RCS Application" || r.Name == "Rates Clearance Sectional Head" || r.Name == "Support Admin" || r.Name == "Support Clerk" || r.Name == "Back Office System Administrator").ToList();
                var roles = context.Roles
                                        .Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Property Manager" || r.Name == "Housing Supervisor" || r.Name == "Finance Administrator" || r.Name == "Revenue Manager" || r.Name == "Community Development Officer"
                                        || r.Name == "Letting Officer" || r.Name == "Revenue Officer" || r.Name == "Back Office System Administrator" || r.Name == "Area Manager" || r.Name == "Senior Manager" || r.Name == "Senior specialist" || r.Name == "Housing liaison officer"
                                        || r.Name == "Maintenance Supervisor" || r.Name == "Regional Manager" || r.Name == "Caretaker" || r.Name == "Senior Housing Specialist").ToList();
                vm.AdUser = clerkRegistration.IsActiveDirectoryUser;
                vm.Features = new SelectList(roles, "Name", "Name");
                var RolesArrayTemp = context.ClerkRoles.Where(x => x.ClerkRegistrationId == clerkRegistration.Id);
                List<string> RolesArray = new List<string>();
                foreach (var item in RolesArrayTemp)
                {
                    RolesArray.Add(item.RoleName);
                }
                var RolesList2 = roles.Select(x => new SelectListItem()
                {
                    Selected = RolesArray.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                });
                vm.Features = RolesList2;
                vm.CCC = clerkRegistration.CCCTypeId;
                ViewBag.Data = "";
                return View(vm);
            }
        }

        [Authorize(Roles = "Area Manager,Back Office System Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser(InternalRegisterViewModel model, string[] SelectedRoles)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    //SelectedRoles[0] = "Back Office System Administrator";
                    var currentRoleApplication = context.ApplicationRoles.Where(ar => ar.IsActive && ar.IsDeleted == false
                                && ar.Application.Key == ApplicationKeys.RatesClearanceSystem).Select(cr => cr.RoleId);
                    //var roles = context.Roles.Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Issue Refunds Collection" || r.Name == "Acknowledge Refund Application" || r.Name == "Area Manager" || r.Name == "Internal Registration" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Rates Clearance Senior Clerk" || r.Name == "Rates" || r.Name == "Billing" || r.Name == "Sundry Account" || r.Name == "Credit Control" || r.Name == "Acknowledge RCS Application" || r.Name == "Rates Clearance Sectional Head" || r.Name == "Support Admin" || r.Name == "Support Clerk"|| r.Name == "Back Office System Administrator").ToList();
                    var roles = context.Roles
                        .Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Property Manager" || r.Name == "Housing Supervisor" || r.Name == "Finance Administrator" || r.Name == "Revenue Manager" || r.Name == "Community Development Officer"
                        || r.Name == "Letting Officer" || r.Name == "Revenue Officer" || r.Name == "Back Office System Administrator" || r.Name == "Area Manager" || r.Name == "Senior Manager" || r.Name == "Senior specialist" || r.Name == "Housing liaison officer"
                        || r.Name == "Maintenance Supervisor" || r.Name == "Regional Manager" || r.Name == "Caretaker" || r.Name == "Senior Housing Specialist").ToList();
                    var EdenvaleId = context.CCCTypes.FirstOrDefault(x => x.Key == CCCTypeKeys.Edenvale).Id;
                    var EkurhuleniHousingCompanyId = context.ApplicationEntities.FirstOrDefault(r => r.Key == ApplicationEntityKeys.EkurhuleniHousingCompany).Id;
                    var SelectedCCC = context.CCCs.Where(x => x.CCCTypeId == model.CCC).FirstOrDefault();
                    var RolesArrayTemp = context.ClerkRoles.Where(x => x.ClerkRegistrationId == model.ClerkRegID);
                    List<string> RolesArray = new List<string>();
                    foreach (var item in RolesArrayTemp)
                    {
                        RolesArray.Add(item.RoleName);
                    }
                    var RolesList2 = roles.Select(x => new SelectListItem()
                    {
                        Selected = RolesArray.Contains(x.Name),
                        Text = x.Name,
                        Value = x.Name
                    });
                    model.Features = RolesList2;

                    ViewBag.CCCTypes = new SelectList(db.CCCTypes, "Id", "Name", model.CCC);
                    ViewBag.DepartmentId = new SelectList(db.ApplicationEntities.OrderByDescending(r => r.Id), "Id", "Name", model.DepartmentId);


                    _base.Initialise(context);
                    if (ModelState.IsValid)
                    {
                        var randomPassword = GeneratePassword(10);
                        var email = new Email();
                        var identityManager = new IdentityManager();

                        var applicationUserRole = new ApplicationUserRole
                        {
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false,
                            CCCId = (EkurhuleniHousingCompanyId == model.DepartmentId) ? EdenvaleId : SelectedCCC.Id,
                            ApplicationId = context.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault().Id,
                            RoleId = RoleManager.FindByName(SelectedRoles[0]).Id,
                            DepartmentId  = model.DepartmentId
                        };



                        var defaultCustomer = context.CustomerTypes.FirstOrDefault(c => c.Key == CustomerTypeKeys.Individual);
                        var defaultIdentification = context.IdentificationTypes.FirstOrDefault(id => id.Key == IdentificationTypeKey.SouthAfricanID);
                        var defaultTitleType = context.TitleTypes.FirstOrDefault(t => t.Key == TitleTypeKeys.Mister);
                        var defaultStatus = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);

                        if (defaultCustomer == null) throw new Exception("Invalid customer type");
                        if (defaultIdentification == null) throw new Exception("Invalid identification");
                        if (defaultTitleType == null) throw new Exception("Invalid title type");
                        if (defaultStatus == null) throw new Exception("Invalid status");

                        var customer = new Customer()
                        {
                            CustomerTypeId = defaultCustomer.Id,
                            IdentificationTypeId = defaultIdentification.Id,
                            CountryOfIssueTypeId = null,
                            IdentificationNumber = model.IdentificationNumber,
                            TitleTypeId = defaultTitleType.Id,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            CellPhoneNumber = model.MobileNumber,
                            Gender = null,
                            IsDeceased = false,
                            EmailAddress = model.EmailAddress,
                            PhysicalAddressCode = 0000,
                            PostalAddressCode = 0000,
                            StatusId = defaultStatus.Id,
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false,
                            DepartmentId=model.DepartmentId
                        };
                        var firstName = model.FirstName;
                        var surname = model.LastName;
                        var username = model.UserName;
                        var emailAddress = model.EmailAddress;
                        var serviceNo = model.EmployeeNumber;
                        var usernameAssigned = context.SystemUsers.Any(u => u.UserName.ToLower() == username.ToLower()
                            && u.IsActive && u.IsDeleted == false);
                        var emailAssigned = context.SystemUsers.Any(u => u.EmailAddress.ToLower() == emailAddress.ToLower()
                            && u.IsActive && u.IsDeleted == false);
                        var response = "";
                        var Statuses = context.Status.Where(x => x.Key == StatusKeys.AccountPending).FirstOrDefault();
                        if (!emailAssigned)
                        {
                            emailAssigned = context.ClerkRegistrations.Include(x => x.Status).Any(u => u.EmailAddress.ToLower() == model.EmailAddress.ToLower() && u.IsActive && u.IsDeleted == false && u.Id != model.ClerkRegID && u.StatusId == Statuses.Id);
                        }
                        //if (!mobileAssigned)
                        //{
                        //    mobileAssigned = context.ClerkRegistrations.Include(x => x.Status).Any(u => u.MobileNumber == model.MobileNumber && u.IsActive && u.IsDeleted == false && u.StatusId == Statuses.Id);
                        //}
                        if (!usernameAssigned)
                        {
                            usernameAssigned = context.ClerkRegistrations.Include(x => x.Status).Any(u => u.UserName.ToLower() == model.UserName.ToLower() && u.IsActive && u.IsDeleted == false && u.Id != model.ClerkRegID && u.StatusId == Statuses.Id);
                        }
                        if (!usernameAssigned && emailAssigned)
                        {
                            response = "Email address registered. Please use an alternative email address";
                        }
                        else if (usernameAssigned && !emailAssigned)
                        {
                            response = "Username registered. Please choose a unique username.";
                        }
                        else if (usernameAssigned && emailAssigned)
                        {
                            response = "Username and Email address registered. Please use an alternative email address and a unique username.";
                        }
                        else
                        {
                            // JK.20140724a - Passing values from the ViewModel to the Model.
                            var user = new SystemIdentityUser
                            {
                                UserName = username,
                                Email = emailAddress,
                                EmailConfirmed = true,
                                PhoneNumber = model.MobileNumber,
                                isInternalUser = true,
                                isActiveDirectoryUser = model.AdUser,
                                RoundRobinIsActive = true,
                                CCCId = SelectedCCC.Id,
                                ServiceNo = serviceNo,
                                SystemUser = new SystemUser()
                                {
                                    FirstName = firstName,
                                    LastName = surname,
                                    UserName = username,
                                    MobileNumber = model.MobileNumber,
                                    //IdentificationNumber = model.IdentificationNumber,
                                    EmailAddress = emailAddress,
                                    IsPasswordReset = model.AdUser,
                                    CCCId = SelectedCCC.Id,
                                    ServiceNo = serviceNo,
                                    IsActive = true,
                                    IsDeleted = false,
                                    IsLocked = false,
                                    ModifiedDateTime = DateTime.Now,
                                   DepartmentId=model.DepartmentId
                                }
                            };

                            // JK.20140724a - Custom profile information.

                            // Send email to User with Username and Temp Password
                            //var result = identityManager.CreateUser(user, randomPassword);
                            var result = await UserManager.CreateAsync(user, randomPassword);


                            foreach (var item in SelectedRoles)
                            {
                                identityManager.AddUserToRole(user.Id, item.ToString());
                            }

                            applicationUserRole.SystemUserId = user.SystemUserId;
                            customer.SystemUserId = user.SystemUserId;

                            context.ApplicationUserRoles.Add(applicationUserRole);
                            context.Customers.Add(customer);
                            context.SaveChanges();
                            var Keys = db.Status.ToList();

                            var clerkRegistration = new ClerkRegistration()
                            {
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                IdentificationNumber = model.IdentificationNumber,
                                UserName = model.UserName,
                                EmployeeNumber = model.EmployeeNumber,
                                RoleArray = "Clerks",
                                CCCTypeId = model.CCC,
                                CCCId = SelectedCCC.Id,
                                IsActiveDirectoryUser = model.AdUser,
                                //IsActiveDirectoryUser = model.AdUser,
                                EmailAddress = model.EmailAddress,
                                MobileNumber = model.MobileNumber,
                                StatusId = Keys.Where(x => x.Key == StatusKeys.AccountPending).FirstOrDefault().Id,
                                IsActive = true,
                                IsDeleted = false,
                                IsLocked = false,
                                CreatedDateTime = DateTime.Now,
                                ModifiedDateTime = DateTime.Now,
                                IsPasswordReset = true,
                                NotificationTypeId = 3,
                                DepartmentId = model.DepartmentId
                            };
                            context.ClerkRegistrations.Add(clerkRegistration);
                            context.SaveChanges();

                            model.ClerkRegID = clerkRegistration.Id;

                            foreach (var item in SelectedRoles)
                            {
                                var appUserRole = new AppUserRole
                                {
                                    IsActive = true,
                                    IsDeleted = false,
                                    IsLocked = false,
                                    RoleId = RoleManager.FindByName(item).Id,
                                    ApplicationUserRoleId = applicationUserRole.Id,
                                    DepartmentId = model.DepartmentId
                                };

                                context.AppUserRoles.Add(appUserRole);
                                context.SaveChanges();
                            }

                            if (result.Succeeded)
                            {
                                string role = "";
                                foreach (var item in SelectedRoles)
                                {
                                    if (role == "")
                                    {
                                        role = role + item.ToString();
                                    }
                                    else
                                    {
                                        role = role + ", " + item.ToString();
                                    }

                                }

                                var applicationAccess = context.Applications.Find(applicationUserRole.ApplicationId);
                                const string emailSubject = "Property Lease Management System: User Registration";
                                string emailBody = "";
                                if (model.AdUser == false)
                                {
                                    emailBody = "<b>You have been successfully added onto Property Lease Management System.</b><br/><br/>" +
                                  "<b>Login Details:</b><br/>" +
                                  "Username: " + user.UserName + "<br/>" +
                                  "Temporary Password: " + randomPassword + "<br/>" +
                                  "Application Access: " + applicationAccess.Name + "<br/>" +
                                  "Role: " + role + "<br/><br/>" +
                                  "<b> Please change temporary password on your first login.</b>";

                                }
                                else
                                {
                                    emailBody = "<b>You have been successfully added onto Property Lease Management System.</b><br/><br/>" +
                                  "<b>Login Details:</b><br/>" +
                                  "Active Directory Username: " + user.UserName + "<br/>" +
                                  "Password: " + "Please use your own Active Directory password" + "<br/>" +
                                  "Application Access: " + applicationAccess.Name + "<br/>" +
                                  "Role: " + role + "<br/><br/>";

                                }


                                email.GenerateEmail(user.Email, emailSubject, emailBody, user.SystemUserId.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.SystemUser.FullName);
                                try
                                {
                                    var clerkreg = context.ClerkRegistrations.Find(model.ClerkRegID) ?? null;
                                    if (clerkreg != null)
                                    {
                                        clerkreg.StatusId = context.Status.Where(x => x.Key == StatusKeys.AccountActive).FirstOrDefault().Id;
                                        context.Entry(clerkreg).State = EntityState.Modified;
                                        context.SaveChanges();
                                    }
                                }
                                catch (Exception)
                                {

                                }
                                
                                
                                response = "Success";
                                //ViewBag.EmailAddress = model.EmailAddress == null || model.EmailAddress.Trim() == string.Empty ? model.MobileNumber : model.EmailAddress;
                            }
                            else
                            {
                                AddErrors(result);
                            }
                            ViewBag.Response = response;
                            //return View();

                            ViewBag.EmailAddress = (model.EmailAddress == null || model.EmailAddress.Trim() == string.Empty) ? model.MobileNumber : model.EmailAddress;
                            if (model.AdUser == false)
                            {
                                TempData["Message"] = "Username: " + user.UserName + " was successfully created, a temporary password has been emailed.";

                            }
                            else
                            {
                                TempData["Message"] = "Username: " + user.UserName + " was successfully created, active directory login has been enabled for user.";

                            }
                            if (User.IsInRole("Back Office System Administrator"))
                            {
                                return RedirectToAction("AdminInbox");
                            }
                            return RedirectToAction("Index");
                        }
                        ModelState.AddModelError("", response);
                    }
                }
                catch (Exception IO)
                {
                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }

            //db.Entry(clerkRegistration).State = EntityState.Modified;
            //db.SaveChanges();
            return View(model);

            //ViewBag.CCCTypeId = new SelectList(db.CCCTypes, "Id", "Name", clerkRegistration.CCCTypeId);
            //ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.CreatedBySystemUserId);
            //ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.ModifiedBySystemUserId);
            //ViewBag.NotificationTypeId = new SelectList(db.NotificationTypes, "Id", "Name", clerkRegistration.NotificationTypeId);
            return View();
        }

        public JsonResult FindAdUserINformation(string username)
        {
            ADLogin Ad = new ADLogin();
            ClerkRegistration clerk = Ad.ValidateUser2(username);
            ViewBag.Data = clerk.UserName != null ? clerk : null;
            return Json(clerk, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FindDepartmentKey(int? Key)
        {
            var result = db.ApplicationEntities.FirstOrDefault(r => r.Id == Key)?.Key;
            if (result != ApplicationEntityKeys.EkurhuleniHousingCompany) ViewBag.CCCTypes = new SelectList(db.CCCTypes.OrderBy(r=>r.Name).Where(x => x.IsActive), "Id", "Name");
            if (result == ApplicationEntityKeys.EkurhuleniHousingCompany) ViewBag.CCCTypes = new SelectList(db.CCCTypes.OrderBy(r => r.Name).Where(x => x.Key == CCCTypeKeys.Edenvale), "Id", "Name");
            return Json(result, JsonRequestBehavior.AllowGet);
        }







        // GET: AreaManager/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ClerkRegistration clerkRegistration = db.ClerkRegistrations.Find(id);
        //    if (clerkRegistration == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(clerkRegistration);
        //}

        // POST: AreaManager/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    ClerkRegistration clerkRegistration = db.ClerkRegistrations.Find(id);
        //    db.ClerkRegistrations.Remove(clerkRegistration);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        #region Generate User Password
        public string GeneratePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var res = new StringBuilder();
            var rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        #endregion
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        //For HSD
        public ActionResult HSDRoundRobbinDashboard()
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
            {
                var Message = TempData["ApplicationFeeValidationRedistribution"];
                if (Message != null)
                {
                    ViewBag.MessageTitle = "Application Redistribution Successful";
                    ViewBag.Message = TempData["ApplicationFeeValidationRedistribution"].ToString();
                }

                var Message2 = TempData["AcknowledgementRedistribution"];
                var Title2 = TempData["AcknowledgementRedistributionTitle"];
                if (Message2 != null && Title2 != null)
                {
                    ViewBag.MessageTitle = TempData["AcknowledgementRedistribution"].ToString();
                    ViewBag.Message = TempData["AcknowledgementRedistribution"].ToString();
                }
                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();
                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                if (User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.StatusId == SubmittedId && x.HumanSettlementApplicationId != null).ToList();
                }
                else
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.StatusId == SubmittedId && x.HumanSettlementApplicationId != null).ToList();
                }
                var list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();

                var plmApps = db.HumanSettlementApplications.Where(x => x.IsActive && list.Contains(x.Id))
                    .Include(r => r.Status)
                    .Include(r => r.SystemUser)
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.Customer).ToList();

                foreach (var Item in plmApps)
                {
                    var rrqplm = db.RoundRobinQueues.Include(r => r.Clerk).Where(x => x.HumanSettlementApplicationId == Item.Id && x.StatusId == SubmittedId).FirstOrDefault();
                    Item.Data = rrqplm.Clerk.UserFullName;
                    Item.RoundRobinQueueId = Convert.ToString(rrqplm.Id);
                }

                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();
                var ResponsibilityTypeID = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault();
                foreach (var item in rcsApps)
                {
                    var rrqName = db.RoundRobinQueues.Where(x => x.RCSApplicationStatusId == item.Id && x.ResponsibilityTypeId == ResponsibilityTypeID.Id).Include(x => x.Clerk).OrderByDescending(x => x.Id).FirstOrDefault();
                    item.Data = rrqName.Clerk.UserFullName;
                    item.RoundRobinQueueId = Convert.ToString(rrqName.Id);
                    if (item.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation)
                    {


                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.ApplicationFeeValidation).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).OrderByDescending(x => x.Id).FirstOrDefault();
                        DateTime StartDateFinal = Convert.ToDateTime(StartDate.CreatedDateTime);


                        TimeSpan diff2 = (DateTime.Now.Subtract(StartDateFinal));

                        var diffdays = diff2.Days;
                        if (diffdays < TurnAroundDays)
                        {
                            item.ColorCode = OnTargetColor;
                        }
                        else if (diffdays == TurnAroundDays)
                        {
                            item.ColorCode = RunningLateColor;
                            //@*background - color: #e6cc11;*@
                        }
                        else if (diffdays > TurnAroundDays)
                        {
                            item.ColorCode = OverdueColor;
                        }
                    }

                    else if (item.Status.Key == StatusKeys.PendingDocumentsApproval)
                    {
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).OrderByDescending(x => x.Id).FirstOrDefault();
                        DateTime StartDateFinal = Convert.ToDateTime(StartDate.CurrentTaskDateTime);
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.DocumentValidation).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        TimeSpan diff2 = (DateTime.Now.Subtract(StartDateFinal));

                        var diffdays = diff2.Days;
                        if (diffdays < TurnAroundDays)
                        {
                            item.ColorCode = OnTargetColor;
                        }
                        else if (diffdays == TurnAroundDays)
                        {
                            item.ColorCode = RunningLateColor;
                        }
                        else if (diffdays > TurnAroundDays)
                        {
                            item.ColorCode = OverdueColor;
                        }
                    }


                }

                return View(plmApps);

            }
            else
            {
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                var list = rrq.Select(x => x.RCSApplicationStatusId).ToList();
                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();

                foreach (var item in rcsApps)
                {

                    if (item.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.ApplicationFeeValidation).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).OrderByDescending(x => x.Id).FirstOrDefault();

                        DateTime StartDateFinal = Convert.ToDateTime(StartDate.CreatedDateTime);


                        TimeSpan diff2 = (DateTime.Now.Subtract(StartDateFinal));

                        var diffdays = diff2.Days;
                        if (diffdays < TurnAroundDays)
                        {
                            item.ColorCode = OnTargetColor;
                        }
                        else if (diffdays == TurnAroundDays)
                        {
                            item.ColorCode = RunningLateColor;
                            //@*background - color: #e6cc11;*@
                        }
                        else if (diffdays > TurnAroundDays)
                        {
                            item.ColorCode = OverdueColor;
                        }
                    }

                    else if (item.Status.Key == StatusKeys.PendingDocumentsApproval)
                    {
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).OrderByDescending(x => x.Id).FirstOrDefault();

                        DateTime StartDateFinal = Convert.ToDateTime(StartDate.CurrentTaskDateTime);
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.DocumentValidation).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        TimeSpan diff2 = (DateTime.Now.Subtract(StartDateFinal));

                        var diffdays = diff2.Days;
                        if (diffdays < TurnAroundDays)
                        {
                            item.ColorCode = OnTargetColor;
                        }
                        else if (diffdays == TurnAroundDays)
                        {
                            item.ColorCode = RunningLateColor;
                        }
                        else if (diffdays > TurnAroundDays)
                        {
                            item.ColorCode = OverdueColor;
                        }
                    }


                }
                return View(rcsApps);
            }


        }
        [DecryptParameter]
        public ActionResult HsdManualReAllocate(int? id, int? rrqID, string ResponsibilityType, string ViewName, string TitleName, string BodyName)
        {
            Initialise();
            var Keys = db.Status;
            var plmApps = db.HumanSettlementApplications.Where(x => x.Id == id).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var findItem = db.RoundRobinQueues.Include(r => r.Clerk).Include(r => r.ResponsibilityType).FirstOrDefault(x => x.StatusId == SubmittedId && x.HumanSettlementApplicationId == plmApps.Id);
            var bouserid = db.Customers.FirstOrDefault(x => x.Id == findItem.ClerkId).SystemUserId;

            var Users = db.SystemUsers.Where(x => x.Id == bouserid);

            var user = UserManager.FindByName(Users.FirstOrDefault().UserName);
            var userId = user.Id;
            // get user roles
            var roleName = UserManager.GetRoles(userId).FirstOrDefault();


            var roleId = db.Roles.Where(x => x.Name == roleName).FirstOrDefault().Id;
            PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
            var UsersList = cc.GetUsersInRole(roleId).Where(x => x.RoundRobinIsActive == true).ToList();

            var ResponsibilityTypeName = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault();
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == rrqID).FirstOrDefault();

            var vm = new ManualReAllocationViewModel();
            vm.CurrentFullName = rrq.Clerk.FullName;
            vm.HumanSettlementApplicationId = (int)id;
            vm.CurrentAssignedUserName = rrq.Clerk.UserFullName;
            vm.RoundRobinQueueId = rrq.Id;
            vm.ResponsibilityType = Convert.ToString(findItem.ResponsibilityType.Id);
            vm.ViewName = ViewName;
            vm.TitleName = TitleName;
            vm.BodyName = BodyName;
            ViewBag.BOUsers = new SelectList(UsersList, "SystemUser.Id", "SystemUser.FullName");
            return View(vm);
        }
        [HttpPost]
        public ActionResult HsdManualReAllocate(int? id, ManualReAllocationViewModel vm, int UserId)
        {
            Initialise();

            var plmApps = db.HumanSettlementApplications.Where(x => x.Id == vm.HumanSettlementApplicationId && x.IsDeleted == false).FirstOrDefault();
            var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == vm.RoundRobinQueueId).FirstOrDefault();

            if (rrqList != null)
            {
                rrqList.StatusId = db.Status.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                rrqList.EndTaskDateTime = DateTime.Now;
                db.Entry(rrqList).State = EntityState.Modified;
                db.SaveChanges();
            }

            var ClerkId = db.Customers.Where(x => x.SystemUserId == UserId && x.IsDeleted == false).FirstOrDefault();
            var statusList = db.Status.ToList();
            var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

            var roundRobinQueue = new RoundRobinQueue
            {
                HumanSettlementApplicationId = plmApps.Id,
                ResponsibilityTypeId = Convert.ToInt32(vm.ResponsibilityType),
                CurrentTaskDateTime = DateTime.Now,
                ClerkId = ClerkId.Id,
                StatusId = StatusId,
                AssignedFrom = rrqList.Id
            };
            db.RoundRobinQueues.Add(roundRobinQueue);
            db.SaveChanges();
            HumanSettlementApplicationController cc = new HumanSettlementApplicationController();

            int rtype = Convert.ToInt32(vm.ResponsibilityType);
            var ResponsibilityType = db.ResponsibilityTypes.FirstOrDefault(x => x.Id == rtype);

            var Title = vm.TitleName;
            var Body = vm.BodyName;

            if (roundRobinQueue.Id == 0)
            {
                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;
            }
            else
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                var Result = cc.ActivityTrackerAudit(plmApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; ;
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;
            }

            return RedirectToAction("HSDRoundRobbinDashboard");
        }

        //For HSD
        public ActionResult RoundRobbinHsdActiveWorkQueues()
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
            {
                var Message = TempData["ApplicationFeeValidationRedistribution"];
                if (Message != null)
                {
                    ViewBag.MessageTitle = "Application Redistribution Successful";
                    ViewBag.Message = TempData["ApplicationFeeValidationRedistribution"].ToString();
                }

                var Message2 = TempData["AcknowledgementRedistribution"];
                var Title2 = TempData["AcknowledgementRedistributionTitle"];
                if (Message2 != null && Title2 != null)
                {
                    ViewBag.MessageTitle = TempData["AcknowledgementRedistribution"].ToString();
                    ViewBag.Message = TempData["AcknowledgementRedistribution"].ToString();
                }
                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();
                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                List<Customer> prr = new List<Customer>();

                if (User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.StatusId == SubmittedId && x.HumanSettlementApplicationId != null).ToList();
                    var list = rrq.Select(x => x.ClerkId).ToList();

                    prr = db.Customers.Where(x => list.Contains(x.Id)).ToList();

                    foreach (var Item in prr)
                    {
                        Item.Data = (db.RoundRobinQueues.Where(x => x.ClerkId == Item.Id && x.StatusId == SubmittedId).Count()).ToString();

                        var user = UserManager.FindByName(Item.SystemUser.UserName);
                        var userId = user.Id;
                        // get user roles
                        List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                        string RolesList = "";
                        int counter = 0;
                        foreach (var Roles in rolesArray)
                        {
                            if (counter > 0)
                            {
                                RolesList += ", ";
                            }
                            counter++;
                            RolesList += Roles;
                        }
                        Item.ColorCode = RolesList;
                    }

                }
                else
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.StatusId == SubmittedId && x.HumanSettlementApplicationId == null).ToList();
                    var list = rrq.Select(x => x.ClerkId).ToList();

                    prr = db.Customers.Where(x => list.Contains(x.Id)).ToList();

                    foreach (var Item in prr)
                    {
                        Item.Data = (db.RoundRobinQueues.Where(x => x.ClerkId == Item.Id && x.StatusId == SubmittedId).Count()).ToString();

                        var user = UserManager.FindByName(Item.SystemUser.UserName);
                        var userId = user.Id;
                        // get user roles
                        List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                        string RolesList = "";
                        int counter = 0;
                        foreach (var Roles in rolesArray)
                        {
                            if (counter > 0)
                            {
                                RolesList += ", ";
                            }
                            counter++;
                            RolesList += Roles;
                        }
                        Item.ColorCode = RolesList;
                    }
                }

                return View(prr);

            }

            return View();

        }
        [DecryptParameter]
        public ActionResult EditHsdRRUser(int Id, string data)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    if (Id == 0) throw new Exception("Invalid User");
                    var Users = context.SystemUsers.Where(x => x.Id == Id);
                    ViewBag.Name = Users.FirstOrDefault().UserFullName;
                    
                    var user = UserManager.FindByName(Users.FirstOrDefault().UserName);
                    var userId = user.Id;
                    // get user roles
                    var roleName = UserManager.GetRoles(userId).FirstOrDefault();


                    var roleId = db.Roles.Where(x => x.Name == roleName).FirstOrDefault().Id;
                    PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
                    var UsersList = cc.GetUsersInRole(roleId).Where(x => x.RoundRobinIsActive == true).ToList();
                    var rrq = UsersList.Select(x => x.SystemUserId).ToList();
                    var userlist = context.Customers.Where(x => rrq.Contains((int)x.SystemUserId)).ToList();
                    ViewBag.TypeofProperty = new SelectList(userlist.ToList(), "Id", "UserFullName");
                    ViewBag.Id = Id;
                    var Message = TempData["RoundRobinRedistributionTitle"];
                    var Title = TempData["RoundRobinRedistribution"];
                    if (Message != null && Title != null)
                    {
                        ViewBag.MessageTitle = TempData["RoundRobinRedistributionTitle"].ToString();
                        ViewBag.Message = TempData["RoundRobinRedistribution"].ToString();
                    }

                    return View(Users);
                }
                catch
                {
                    return View("_Error");
                }
            }
        }
        [HttpPost]
        [DecryptParameter]
        public ActionResult EditHsdRRUser(int Id, bool ReAllocateCases, int AssignToId)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    if (ReAllocateCases == true)
                    {
                        var Keys = context.Status;
                        int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                        var Clerk = context.Customers.Where(x => x.SystemUserId == Id).FirstOrDefault();
                        var rrqList = context.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.ResponsibilityType).Include(x => x.Clerk.SystemUser).Include(x => x.LeaseDetails).Include(x => x.HumanSettlementApplication).Where(x => x.ClerkId == Clerk.Id && x.StatusId == SubmittedId).ToList();
                        AesCrypto AES = new AesCrypto();
                        var q = AES.Encrypt("id=" + Id + "&" + "data=" + rrqList);
                        int SystUserId = Id;
                        foreach (var item in rrqList)
                        {
                            DepartmentsApprovalsController DA = new DepartmentsApprovalsController();
                            CaptureController c = new CaptureController();
                            switch (item.ResponsibilityType.Key)
                            {

                                case (ResponsibilityTypeKeys.RiskAssessment):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.ValidateDepositPayment):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.InviteToClientTraining):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.ScheduleInspectionSlots):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.Inspections):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.MaintananceJobSheet):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.CaptureLeaseDetails):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.GenerateLeaseAgreement):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.LeaseAgreementValidation):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.DebitOrderVAlidation):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.Terminations):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.TerminationValidation):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.CommitteeOutcomes):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.VacatingConfirmation):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.LeaseRenewals):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.SecondLeaseRenewal):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.LeaseRenewalRevenue):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.RenewalRiskAssessment):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.UnitMaintenanance):
                                    {
                                        bool result = MatchingHelper.ReallocateCaseToNewUserHsd(context, item, AssignToId);
                                        if (result)
                                        {
                                            var ClerkId = context.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                            item.EndTaskDateTime = DateTime.Now;
                                            context.Entry(item).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.PropertyLeaseApplicationId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = context.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditHsdRRUser", new { q = q });
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                    return RedirectToAction("RoundRobbinHsdActiveWorkQueues");
                }
                catch
                {
                    return View("_Error");
                }
            }
        }
    }
}
