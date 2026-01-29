using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.ViewModels;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using C8.eServices.Mvc.Helpers;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using System.Web.Routing;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Data.Entity.Core.Objects;
using C8.eServices.Mvc.ApiServices;
using Newtonsoft.Json.Linq;

namespace C8.eServices.Mvc.Controllers
{

    public class DepartmentsApprovalsController : Controller
    {
        #region Db Conns
            private eServicesDbContext db = new eServicesDbContext();
            BaseHelper _base = new BaseHelper();
            private static int _loginId;
            public DepartmentsApprovalsController()
             : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
            {
                //allow alphanumeric usernames  
                //UserManager.UserValidator = new UserValidator<SystemIdentityUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
            }

            public DepartmentsApprovalsController(UserManager<SystemIdentityUser> userManager)
            {
                UserManager = userManager;
                var provider = new DpapiDataProtectionProvider("eServices");
                UserManager.UserValidator = new UserValidator<SystemIdentityUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
                UserManager.UserTokenProvider = new DataProtectorTokenProvider<SystemIdentityUser>(
                                                provider.Create("EmailConfirmation"))
                {
                    TokenLifespan = TimeSpan.FromDays(14)
                };
            }

            public DepartmentsApprovalsController(eServicesDbContext context)
            {
                eServicesDbContext _context = new eServicesDbContext();
                UserManager =
                    new UserManager<SystemIdentityUser>(
                        new UserStore<SystemIdentityUser>(_context));
            }

        public UserManager<SystemIdentityUser> UserManager { get; private set; }
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
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
                }
            
        }
        #endregion
        #region ActivityTracker
        public bool ActivityTrackerAudit(int RCSAppID, string ActivityTrackerMessage, int CustomerID)
        {
            try
            {
                var RCSHistoryLog = new RCSApplicationHistoryLog
                {
                    RCSApplicationStatusId = RCSAppID,
                    AuditAction = ActivityTrackerMessage,
                    UserId = CustomerID,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                db.SaveChanges();
                return true;
            }
            catch(Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }
        
            //return "test";
        }
        //public bool ActivityTrackerAuditDepartments(int DepartmentID, string ActivityTrackerMessage, int CustomerID)
        //{
        //    try
        //    {
        //        var RCSHistoryLog = new RCSApplicationHistoryLog
        //        {
        //            department = RCSAppID,
        //            Action = ActivityTrackerMessage,
        //            UserId = CustomerID,
        //            CreatedDateTime = DateTime.Now,
        //            IsActive = true
        //        };
        //        db.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
        //        db.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception IO)
        //    {
        //        return false;
        //    }

        //    //return "test";
        //}

        public bool ActivityTrackerAuditRefunds(int RefundAppID,int RCSAppID, string ActivityTrackerMessage, int CustomerID)
        {
            try
            {
                var RCSHistoryLog = new PLMApplicationHistortyLog
                {
                    PropertyLeaseApplicationId= RCSAppID,
                    AuditAction = ActivityTrackerMessage,
                    UserId = CustomerID,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.PLMApplicationHistortyLogs.Add(RCSHistoryLog);
                db.SaveChanges();
                return true;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }

            //return "test";
        }
        #endregion
        #region BO Doc Evaluation
        public ActionResult BODocDashboard()
        {
            Initialise();
            var userID = Customer.Id;
            object rcsApps = null;

            var Keys = db.Status;

            if (User.IsInRole("Clerks"))
            {
                rcsApps = db.RCSApplicationStatus.Where(x => x.ClerkId == userID && x.IsDeleted == false && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation )).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status);
            }

            return View(rcsApps);
        }
        //Sashen Acknowledgement Dashboard
       
        public ActionResult AcknowledgementDashboard()
        {
            Initialise();
            var userID = Customer.Id;
            //object rcsApps = null;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if(User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
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
                 if (User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                     rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                }
                else
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                     rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }
                //rrq= rrq.Where(x=>x.Clerk.SystemUser.CCC == 8)
                var list = rrq.Select(x => x.RCSApplicationStatusId).ToList();
                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation )).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();
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
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).OrderByDescending(x=>x.Id).FirstOrDefault();
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
            else
            {
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                var list = rrq.Select(x => x.RCSApplicationStatusId).ToList();
                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation )).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();

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
           
            //    rcsApps = db.RCSApplicationStatus.Where(x => x.ClerkId == userID && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation || x.Status.Key == StatusKeys.PendingAssessmentFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status);
           
        }


        //View all cases
        public ActionResult viewallcases()
        {
            Initialise();
            var userID = Customer.Id;
            //object rcsApps = null;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
           
              
                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                //rrq= rrq.Where(x=>x.Clerk.SystemUser.CCC == 8)
                var list = rrq.Select(x => x.RCSApplicationStatusId).ToList();
                var rcsApps = db.RCSApplicationStatus.Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).Include(d => d.CCC).ToList();
                
              

                return View(rcsApps);

            
          

            //    rcsApps = db.RCSApplicationStatus.Where(x => x.ClerkId == userID && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation || x.Status.Key == StatusKeys.PendingAssessmentFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status);

        }



        //End all view cases







        [DecryptParameter]
        public ActionResult AcknowledgementReAllocate(int? id)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();

            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();



            CaptureController c = new CaptureController();
            var SystUserId = c.RoundRobinRedistribution(true, false, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);
            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId && x.IsActive && x.IsDeleted == false).FirstOrDefault();


            if (SystUserId == 0)
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData["AcknowledgementRedistributionTitle"] = "Error when re-distributing case to a new user";
                TempData["AcknowledgementRedistribution"] = "Please note case cannot be redistributed using round robin, as no other users are available.";

            }
            else
            {
               
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData["AcknowledgementRedistributionTitle"] = "Application Redistribution Successful";
                TempData["AcknowledgementRedistribution"] = "Please note case was successfully redistributed using round robin to " + ClerkId.FullName;

            }
            return RedirectToAction("AcknowledgementDashboard");
        }

        public ActionResult RefundAcknowledgementDashboard()
        {
            Initialise();
            var userID = Customer.Id;
            List<RefundApplication> rcsApps = null;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
            {

                var Message = TempData["RefundAcknowledgement"];
                if (Message != null)
                {
                    ViewBag.MessageTitle = TempData["RefundAcknowledgementTitle"].ToString();
                    ViewBag.Message = Message;
                }

                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;

                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                if (User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                }
                else
                {
                     rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }
               
                //rrq= rrq.Where(x=>x.Clerk.SystemUser.CCC == 8)
                var list = rrq.Select(x => x.RefundApplicationId).ToList();
                rcsApps = db.RefundApplications.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.PendingRefundDocumentValidation )).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();

                foreach (var item in rcsApps)
                {

                    var rrqName = db.RoundRobinQueues.Where(x => x.RefundApplicationId == item.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == db.Status.Where(o=>o.Key == StatusKeys.Submitted).FirstOrDefault().Id).Include(x => x.Clerk).OrderByDescending(x => x.Id).FirstOrDefault();
                    item.Data = rrqName.Clerk.UserFullName;
                    //Add Round robin queue id to viewmodel
                    //dont forget to add the code to the dashboard view for the button
                    item.RoundRobinQueueId = Convert.ToString(rrqName.Id);
                

                 
                }

                return View(rcsApps);

            }
            else
            {
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;

                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                var list = rrq.Select(x => x.RefundApplicationId).ToList();
                rcsApps = db.RefundApplications.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.PendingRefundDocumentValidation )).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();


                return View(rcsApps);
            }

            //    rcsApps = db.RCSApplicationStatus.Where(x => x.ClerkId == userID && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation || x.Status.Key == StatusKeys.PendingAssessmentFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status);

        }
        [DecryptParameter]
        public ActionResult RefundAcknowledgementReAllocate(int? id)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            var rcsApps = db.RefundApplications.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RefundApplicationId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();


            CaptureController c = new CaptureController();
            var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, true, false);

            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
            //foreach (var item in rrqList)
            //{
            //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}


            if (SystUserId == 0)
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var failuremessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_failed" && x.IsDeleted == false).FirstOrDefault();
                TempData["RefundAcknowledgementTitle"] = failuremessage.Title;
                TempData["RefundAcknowledgement"] = failuremessage.Body;
             
            }
            else
            {
                var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var successmessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_successful" && x.IsDeleted == false).FirstOrDefault();
                TempData["RefundAcknowledgementTitle"] = successmessage.Title;
                TempData["RefundAcknowledgement"] = successmessage.Body + ClerkId.FullName;
              
            }

            return RedirectToAction("RefundAcknowledgementDashboard");
        }
        public ActionResult IssueRefundDashboard()
        {
            Initialise();
            var userID = Customer.Id;
            List<RefundApplication> rcsApps = null;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (Request.IsAuthenticated && (User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator")))
            {
                var Message = TempData["IssueRefundCollection"];
                if (Message != null)
                {
                    ViewBag.MessageTitle = TempData["IssueRefundCollectionTitle"].ToString();
                    ViewBag.Message = Message;
                }

                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;

                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                if (User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
                {
                   rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                }
                else
                {
                   rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }

            
                //rrq= rrq.Where(x=>x.Clerk.SystemUser.CCC == 8)
                var list = rrq.Select(x => x.RefundApplicationId).ToList();
                rcsApps = db.RefundApplications.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.RefundDocumentApproved)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();

                foreach(var item in rcsApps)
                {
                    var rrqName = db.RoundRobinQueues.Where(x => x.RefundApplicationId == item.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == db.Status.Where(o => o.Key == StatusKeys.Submitted).FirstOrDefault().Id).Include(x => x.Clerk).OrderByDescending(x => x.Id).FirstOrDefault();
                    item.Data = rrqName.Clerk.UserFullName;
                    //Add Round robin queue id to viewmodel
                    //dont forget to add the code to the dashboard view for the button
                    item.RoundRobinQueueId = Convert.ToString(rrqName.Id);


                }
                return View(rcsApps);

            }
            else
            {
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;

                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                var list = rrq.Select(x => x.RefundApplicationId).ToList();
                rcsApps = db.RefundApplications.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.RefundDocumentApproved)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();


                return View(rcsApps);
            }

            //    rcsApps = db.RCSApplicationStatus.Where(x => x.ClerkId == userID && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation || x.Status.Key == StatusKeys.PendingAssessmentFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status);

        }
        [DecryptParameter]
        public ActionResult IssueRefundReAllocate(int? id)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            var rcsApps = db.RefundApplications.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RefundApplicationId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();


            CaptureController c = new CaptureController();
            var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, true);

            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
            //foreach (var item in rrqList)
            //{
            //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}


            if (SystUserId == 0)
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var failuremessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_failed" && x.IsDeleted == false).FirstOrDefault();
                TempData["IssueRefundCollectionTitle"] = failuremessage.Title;
                TempData["IssueRefundCollection"] = failuremessage.Body;

            }
            else
            {
                var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var successmessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_successful" && x.IsDeleted == false).FirstOrDefault();
                TempData["IssueRefundCollectionTitle"] = successmessage.Title;
                TempData["IssueRefundCollection"] = successmessage.Body + ClerkId.FullName;

            }

            return RedirectToAction("IssueRefundDashboard");
        }
        #region Issue Refunds
        public ActionResult IssueRefund(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer.Id;

            var Message = TempData["ApplicationFeeValidation"];
            if (Message != null)
            {
                ViewBag.MessageTitle = "Error when re-distributing case to a new user";
                ViewBag.Message = TempData["ApplicationFeeValidation"].ToString();
            }



            RefundApplication refundApps = null;



            refundApps = context.RefundApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.RCSApplicationStatus).Include(d => d.Status).FirstOrDefault();


            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.RefundPaid || x.Key == RCSActionTypeKeys.RefundApplicationRejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");
            }
            else
            {

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.RefundPaid || x.Key == RCSActionTypeKeys.RefundApplicationRejected).OrderBy(x => x.Name), "Key", "Name");

            }
            try
            {
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == refundApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                var documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSRefund);
                if (documentReferenceType == null)
                    throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                        ReferenceTypeKeys.RCSUpload));

                var documentCheckLists = new List<DocumentCheckList>();


                var MunicipalAccStatement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundMunicipalStatement);
                var RefundConveyancerBankingDetails = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundConveyancerBankingDetails);
                var RefundDeedSearch = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundDeedSearch);
                var RefundMeterReading = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundMeterReading);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalAccStatement.Id && dcl.ReferenceTypeId == documentReferenceType.Id));
                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RefundConveyancerBankingDetails.Id && dcl.ReferenceTypeId == documentReferenceType.Id));
                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RefundDeedSearch.Id && dcl.ReferenceTypeId == documentReferenceType.Id));
                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RefundMeterReading.Id && dcl.ReferenceTypeId == documentReferenceType.Id));
                var watermeterreading = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundWaterMeterReading);
                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == watermeterreading.Id && dcl.ReferenceTypeId == documentReferenceType.Id));
                var addDocumentType = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);
              

                var addDoc = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == documentReferenceType.Id);
                if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
                {
                    documentCheckLists.Add(addDoc);
                }
                var vm = new DepartmentsApprovalViewModel();
                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.RCSApplicationStatusId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.RefundApplicationId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var dvm = new DocumentsViewModel
                {
                    ApplicationId = application.Id,
                    CustomerId = customer.Id,
                    Documents = customerDocuments,
                    IsUploadView = false,
                    DocumentCheckLists = documentCheckLists.ToList(),
                };

                Entity entity = null;
                Agent agent = null;



                vm.Customer = customer;

                vm.CustomerDocuments = documents;
                vm.Notes = notes;
                vm.Document = dvm;
                vm.RefundApplication = refundApps;

                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;

                var obj = new
                {
                    IdentificationNumber = vm.Customer.IdentificationNumber,
                    FullName = vm.Customer.FullName,
                    EmailAddress = vm.Customer.SystemUser == null ? vm.Customer.EmailAddress : vm.Customer.SystemUser.EmailAddress,
                    SystemUserId = vm.Customer.SystemUser == null ? 0 : vm.Customer.SystemUserId,
                    CustomerId = vm.Customer.Id
                };

                ViewBag.CustomerModel = obj;
                ViewBag.CustomerTypeId = new SelectList(context.Status.Include(x => x.StatusType).Where(x => x.StatusType.Key == StatusTypeKeys.DocumentUpload).ToList(), "Key", "Name");
                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View(refundApps);
        }


        [HttpPost]
        public ActionResult IssueRefund(int? id, string ApprovalStatusddl, DepartmentsApprovalViewModel DAVM)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var refundApps = db.RefundApplications.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            var rcscustomer = db.Customers.Where(x => x.Id == refundApps.CustomerId).FirstOrDefault();
            if (ApprovalStatusddl == RCSActionTypeKeys.RefundPaid)
            {
                //CaptureController c = new CaptureController();
                //var SystUserId = c.RoundRobinCCC(false, false, false, false, false, false, false, false, 0, 0, false, true, refundApps.Id);

                ////sash move out of round robin code
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;
                var rrq = db.RoundRobinQueues.Where(x => x.RefundApplicationId == refundApps.Id && x.ResponsibilityTypeId == ResponsibilityTypeId).ToList();
                foreach (var item in rrq)
                {
                    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }


                refundApps.StatusId = Keys.Where(x => x.Key == StatusKeys.RefundPaid).FirstOrDefault().Id;
                db.SaveChanges();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundPaidToConveyancerBankAcc).Description.ToString();

                var Result = ActivityTrackerAuditRefunds(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                //var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.RefundSuccessBankAccount).FirstOrDefault();
                //Email SendMail = new Email();
                //string attorneyemail = rcscustomer.EmailAddress;
                //string attorneyname = rcscustomer.FirstName + " " + rcscustomer.LastName;
                //string emailbody = getemailbody.Description + refundApps.ApplicationReferenceNumber;
                ////string emailbody = "Rates Clearance Clerk Processed Refund, Refund Paid To Conveyancer Bank Account. RCS Refund Application Reference Number: " + refundApps.ApplicationReferenceNumber;
                //var ActivityTrackerEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundEmailIssueRefund).Description.ToString() + " " + emailbody;
                //SendMail.GenerateRefundEmail(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), Customer.Id, ActivityTrackerEmail, attorneyemail, "RCS-Online Application",
                //              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                //var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                //var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                //var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundIssueCollection).Description.ToString();
                //var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundIssueCollection).Description.ToString();

                //var Result2 = ActivityTrackerAuditRefunds(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), ActivityTrackerMessage2, ClerkId.Id);
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.RefundReadyForCollection)
            {
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;
                var rrq = db.RoundRobinQueues.Where(x => x.RefundApplicationId == refundApps.Id && x.ResponsibilityTypeId == ResponsibilityTypeId).ToList();
                foreach (var item in rrq)
                {
                    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                var departapprovals = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == id).ToList();

                refundApps.StatusId = Keys.Where(x => x.Key == StatusKeys.RefundReadyForCollection).FirstOrDefault().Id;
                db.SaveChanges();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundReadyForCollection).Description.ToString();

                var Result = ActivityTrackerAuditRefunds(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.RefundSuccessCollection).FirstOrDefault();
                Email SendMail = new Email();
                string attorneyemail = refundApps.Customer.EmailAddress;
                string attorneyname = refundApps.Customer.FirstName + " " + refundApps.Customer.LastName;
                string emailbody = getemailbody.Description + refundApps.ApplicationReferenceNumber;
                //string emailbody = "Rates Clearance Clerk Processed Refund, Refund Ready For Collection. RCS Refund Application Reference Number: " + refundApps.ApplicationReferenceNumber;


                var ActivityTrackerEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundEmailIssueRefund).Description.ToString() + " " + emailbody;

                SendMail.GenerateRefundEmail(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), Customer.Id, ActivityTrackerEmail, attorneyemail, "RCS - Refund Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);


                //SendMail.GenerateEmail(attorneyemail, "RCS-Online Application",
                //              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                //foreach (var item in departapprovals)
                //{
                //    item.StatusId = rcsApps.StatusId;
                //    db.SaveChanges();
                //}
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.RefundApplicationRejected)
            {
                var departapprovals = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == id).ToList();
           
                refundApps.StatusId = Keys.Where(x => x.Key == StatusKeys.RefundRejected).FirstOrDefault().Id;
                db.SaveChanges();
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;
                var rrq = db.RoundRobinQueues.Where(x => x.RefundApplicationId == refundApps.Id && x.ResponsibilityTypeId == ResponsibilityTypeId).ToList();
                foreach (var item in rrq)
                {
                    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundApplicationRejected).Description.ToString();

              

                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.RefundApplicationRejected).FirstOrDefault();
                Email SendMail = new Email();
                string attorneyemail = refundApps.Customer.EmailAddress;
                string attorneyname = refundApps.Customer.FirstName + " " + refundApps.Customer.LastName;
                string emailbody =getemailbody.Description + refundApps.ApplicationReferenceNumber;
                //string emailbody = "Your Refund Application Has Been Rejected, RCS Refund Application Reference Number: " + refundApps.ApplicationReferenceNumber;
                if (DAVM.Comment != null && DAVM.Comment != "")
                {
                    string RejectionReason = "Reason: " + DAVM.Comment + ".";
                    ActivityTrackerMessage += RejectionReason;
                    emailbody = getemailbody.Description + refundApps.ApplicationReferenceNumber + ". Reason: " + DAVM.Comment;

                }
                else
                {
                    emailbody = getemailbody.Description + refundApps.ApplicationReferenceNumber;

                }
                var Result = ActivityTrackerAuditRefunds(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);


                var ActivityTrackerEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundEmailAcknowledgement).Description.ToString() + " " + emailbody;

                SendMail.GenerateRefundEmail(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), Customer.Id, ActivityTrackerEmail, attorneyemail, "RCS- Refund Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                //SendMail.GenerateEmail(attorneyemail, "RCS-Online Application",
                //              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                //foreach (var item in departapprovals)
                //{
                //    item.StatusId = rcsApps.StatusId;
                //    db.SaveChanges();
                //}
            }

            else if (ApprovalStatusddl == RCSActionTypeKeys.ReAllocate)
            {
               
                var rcsApps = db.RefundApplications.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            
                int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;
                var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RefundApplicationId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();


                CaptureController c = new CaptureController();
                var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, true);

                //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                //foreach (var item in rrqList)
                //{
                //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                //    db.Entry(item).State = EntityState.Modified;
                //    db.SaveChanges();
                //}


                if (SystUserId == 0)
                {
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                    var failuremessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_failed" && x.IsDeleted == false).FirstOrDefault();
                    TempData["IssueRefundCollectionTitle"] = failuremessage.Title;
                    TempData["IssueRefundCollection"] = failuremessage.Body;

                }
                else
                {
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                    var successmessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_successful" && x.IsDeleted == false).FirstOrDefault();
                    TempData["IssueRefundCollectionTitle"] = successmessage.Title;
                    TempData["IssueRefundCollection"] = successmessage.Body + ClerkId.FullName;

                }


            }

            return RedirectToAction("IssueRefundDashboard");
        }
        #endregion
        #region Refunds docs
        public ActionResult RefundDocumentVerification(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer.Id;

            var Message = TempData["ApplicationFeeValidation"];
            if (Message != null)
            {
                ViewBag.MessageTitle = "Error when re-distributing case to a new user";
                ViewBag.Message = TempData["ApplicationFeeValidation"].ToString();
            }


            var Message2 = TempData["RefundDocumentTitle"];
            var Title2 = TempData["RefundDocumentBody"];
            if (Message2 != null && Title2 != null)
            {
                ViewBag.MessageTitle2 = TempData["RefundDocumentTitle"].ToString();
                ViewBag.Message2 = TempData["RefundDocumentBody"].ToString();
            }



            RefundApplication refundApps = null;



            refundApps = context.RefundApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d=>d.RCSApplicationStatus).Include(d => d.Status).FirstOrDefault();


            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.RefundDocsRejected ||  x.Key == RCSActionTypeKeys.RefundApplicationRejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");

            }
            else
            {

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.RefundDocsRejected || x.Key == RCSActionTypeKeys.RefundApplicationRejected).OrderBy(x => x.Name), "Key", "Name");

            }
            try
            {
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == refundApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                var documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSRefund);
                if (documentReferenceType == null)
                    throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                        ReferenceTypeKeys.RCSUpload));

                var documentCheckLists = new List<DocumentCheckList>();


                var MunicipalAccStatement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundMunicipalStatement);
                var RefundConveyancerBankingDetails = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundConveyancerBankingDetails);
                var RefundDeedSearch = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundDeedSearch);
                var RefundMeterReading = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundMeterReading);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalAccStatement.Id && dcl.ReferenceTypeId == documentReferenceType.Id));
                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RefundConveyancerBankingDetails.Id && dcl.ReferenceTypeId == documentReferenceType.Id));
                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RefundDeedSearch.Id && dcl.ReferenceTypeId == documentReferenceType.Id));
                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RefundMeterReading.Id && dcl.ReferenceTypeId == documentReferenceType.Id));
                var addDocumentType = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);
                var watermeterreading = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundWaterMeterReading);
                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == watermeterreading.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                var addDoc = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == documentReferenceType.Id);
                if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
                {
                    documentCheckLists.Add(addDoc);
                }
                var vm = new DepartmentsApprovalViewModel();
                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.RCSApplicationStatusId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.RefundApplicationId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var dvm = new DocumentsViewModel
                {
                    ApplicationId = application.Id,
                    CustomerId = customer.Id,
                    Documents = customerDocuments,
                    IsUploadView = false,
                    DocumentCheckLists = documentCheckLists.ToList(),
                };

                Entity entity = null;
                Agent agent = null;



                vm.Customer = customer;

                vm.CustomerDocuments = documents;
                vm.Notes = notes;
                vm.Document = dvm;
                vm.RefundApplication = refundApps;

                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;

                var obj = new
                {
                    IdentificationNumber = vm.Customer.IdentificationNumber,
                    FullName = vm.Customer.FullName,
                    EmailAddress = vm.Customer.SystemUser == null ? vm.Customer.EmailAddress : vm.Customer.SystemUser.EmailAddress,
                    SystemUserId = vm.Customer.SystemUser == null ? 0 : vm.Customer.SystemUserId,
                    CustomerId = vm.Customer.Id
                };

                ViewBag.CustomerModel = obj;
                ViewBag.CustomerTypeId = new SelectList(context.Status.Include(x => x.StatusType).Where(x => x.StatusType.Key == StatusTypeKeys.DocumentUpload).ToList(), "Key", "Name");
                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View(refundApps);
        }


        [HttpPost]
        public ActionResult RefundDocumentVerification(int? id, string ApprovalStatusddl, DepartmentsApprovalViewModel DAVM)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var refundApps = db.RefundApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Status).Include(x => x.RCSApplicationStatus).Include(x => x.RCSApplicationStatus.CCC).FirstOrDefault();
            var rcscustomer = db.Customers.Where(x => x.Id == refundApps.CustomerId).FirstOrDefault();
            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                CaptureController c = new CaptureController();
                var SystUserId = c.RoundRobinCCC(false, false, false, false, false, false, false, false, 0, 0, false, true, refundApps.Id);

                if(SystUserId != 0)
                {
                    ////sash move out of round robin code
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;
                    var rrq = db.RoundRobinQueues.Where(x => x.RefundApplicationId == refundApps.Id && x.ResponsibilityTypeId == ResponsibilityTypeId).ToList();
                    foreach (var item in rrq)
                    {
                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }


                    refundApps.StatusId = Keys.Where(x => x.Key == StatusKeys.RefundDocumentApproved).FirstOrDefault().Id;
                    db.SaveChanges();
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundApplicationAcknowledgementDcoumentsApproved).Description.ToString();

                    var Result = ActivityTrackerAuditRefunds(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                    var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.RefundDocsApproved).FirstOrDefault();
                    Email SendMail = new Email();
                    string attorneyemail = rcscustomer.EmailAddress;
                    string attorneyname = rcscustomer.FirstName + " " + rcscustomer.LastName;
                    string emailbody = getemailbody.Description + refundApps.ApplicationReferenceNumber;
                    //string emailbody = "Your Documents have been approved for RCS Refund Application Reference Number: " + refundApps.ApplicationReferenceNumber;
                    var ActivityTrackerEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundEmailAcknowledgement).Description.ToString() + " " + emailbody;
                    SendMail.GenerateRefundEmail(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), Customer.Id, ActivityTrackerEmail, attorneyemail, "RCS-Online Application",
                                  emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                    var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundIssueCollection).Description.ToString();

                    var Result2 = ActivityTrackerAuditRefunds(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), ActivityTrackerMessage2, ClerkId.Id);
                }
                else
                {


                    var ResponsibilityType = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault();

                    var UserMessage = db.RCSUserMessages.FirstOrDefault(x => x.Key == RCSUserMessageKeys.DepartmentNotSetup);
                    UserMessage.Body = UserMessage.Body.Replace("{0}", ResponsibilityType.Name);
                    UserMessage.Body = UserMessage.Body.Replace("{1}", refundApps.RCSApplicationStatus.CCC.CCCName);
                    //ViewBag.DepartmentBody = UserMessage.Body;
                    //ViewBag.DepartmentTitle = UserMessage.Title;
                    TempData["RefundDocumentTitle"] = UserMessage.Title;
                    TempData["RefundDocumentBody"] = UserMessage.Body;
                    AesCrypto AES = new AesCrypto();
                    var q = AES.Encrypt("id=" + id.ToString());
                    return RedirectToAction("RefundDocumentVerification", new { q = q });
                }

               
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.RefundDocsRejected)
            {
                var departapprovals = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == id).ToList();

                refundApps.StatusId = Keys.Where(x => x.Key == StatusKeys.RefundDocumentRejected).FirstOrDefault().Id;
                db.SaveChanges();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundApplicationAcknowledgementDcoumentsRejected).Description.ToString();

                var Result = ActivityTrackerAuditRefunds(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.RefundDocsRejected).FirstOrDefault();
                Email SendMail = new Email();
                string attorneyemail = refundApps.Customer.EmailAddress;
                string attorneyname = refundApps.Customer.FirstName + " " + refundApps.Customer.LastName;
                string emailbody = getemailbody.Description + refundApps.ApplicationReferenceNumber;
                //string emailbody = "Your Documents have been been Rejected, please re-upload documents for RCS Refund Application Reference Number: " + refundApps.ApplicationReferenceNumber;


                var ActivityTrackerEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundEmailAcknowledgement).Description.ToString() + " " + emailbody;

                SendMail.GenerateRefundEmail(refundApps.Id,Convert.ToInt16(refundApps.RCSApplicationStatusId), Customer.Id, ActivityTrackerEmail, attorneyemail, "RCS - Refund Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);


                //SendMail.GenerateEmail(attorneyemail, "RCS-Online Application",
                //              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                //foreach (var item in departapprovals)
                //{
                //    item.StatusId = rcsApps.StatusId;
                //    db.SaveChanges();
                //}
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.RefundApplicationRejected)
            {
                var departapprovals = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == id).ToList();

                refundApps.StatusId = Keys.Where(x => x.Key == StatusKeys.RefundRejected).FirstOrDefault().Id;
                db.SaveChanges();

               


                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.RefundApplicationRejected).FirstOrDefault();
                Email SendMail = new Email();
                string attorneyemail = refundApps.Customer.EmailAddress;
                string attorneyname = refundApps.Customer.FirstName + " " + refundApps.Customer.LastName;
                string emailbody = "";
                var ActivityTrackerMessage1 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundApplicationRejected).Description.ToString();

               
                if (DAVM.Comment != null && DAVM.Comment != "")
                {
                    string RejectionReason = "Reason: " + DAVM.Comment + ".";
                    ActivityTrackerMessage1 +=  RejectionReason; 
                     emailbody = getemailbody.Description + refundApps.ApplicationReferenceNumber + ". Reason: "+DAVM.Comment;

                }
                else
                {
                     emailbody = getemailbody.Description + refundApps.ApplicationReferenceNumber ;

                }
                var Result = ActivityTrackerAuditRefunds(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), ActivityTrackerMessage1, Customer.Id);

                //string emailbody = "Your refund application has been rejected reference number: " + refundApps.ApplicationReferenceNumber;
                var ActivityTrackerEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundEmailAcknowledgement).Description.ToString() + " " + emailbody;

                SendMail.GenerateRefundEmail(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), Customer.Id, ActivityTrackerEmail, attorneyemail, "RCS- Refund Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                //SendMail.GenerateEmail(attorneyemail, "RCS-Online Application",
                //              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                //foreach (var item in departapprovals)
                //{
                //    item.StatusId = rcsApps.StatusId;
                //    db.SaveChanges();
                //}
            }
     
            else if (ApprovalStatusddl == RCSActionTypeKeys.ReAllocate)
            {
             
                var rcsApps = db.RefundApplications.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
             
                int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;
                var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RefundApplicationId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();


                CaptureController c = new CaptureController();
                var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, true, false);

                //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                //foreach (var item in rrqList)
                //{
                //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                //    db.Entry(item).State = EntityState.Modified;
                //    db.SaveChanges();
                //}


                if (SystUserId == 0)
                {
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                    var failuremessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_failed" && x.IsDeleted == false).FirstOrDefault();
                    TempData["RefundAcknowledgementTitle"] = failuremessage.Title;
                    TempData["RefundAcknowledgement"] = failuremessage.Body;

                }
                else
                {
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                    var successmessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_successful" && x.IsDeleted == false).FirstOrDefault();
                    TempData["RefundAcknowledgementTitle"] = successmessage.Title;
                    TempData["RefundAcknowledgement"] = successmessage.Body + ClerkId.FullName;

                }



            }

            return RedirectToAction("RefundAcknowledgementDashboard");
        }
        #endregion

        [DecryptParameter]
        public ActionResult BOValidation(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer.Id;


            RCSApplicationStatus rcsApps = null;

            if (User.IsInRole("Clerks"))
            {
                rcsApps = context.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).FirstOrDefault();
            }

            ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

            try
            {
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.LinkedAccounts));

                var documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSUpload);
                if (documentReferenceType == null)
                    throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                        ReferenceTypeKeys.RCSUpload));
                //var referenceTypeId = 6;
                var documentCheckLists = new List<DocumentCheckList>();

                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfPayment);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                //var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

                //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

                //var AuthorityToActAttorney = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceTypeId));


                //var ProofOfProperty = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceTypeId));


                //var MunicipalStatement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

                //var MunicipalCheckList = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));

                ////this code will change
                //var SellerID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceTypeId));

                //var PurchaserID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceTypeId));

                ////var PurchaserSalesAgreement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

                ////documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));

                ////var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                ////                        .Include(d => d.ReferenceType)
                ////                        .Where(dc => dc.ApplicationId == application.Id &&
                ////                        dc.ReferenceTypeId == documentReferenceType.Id);
                //var addDocumentType = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

                //var addDoc = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId);
                //if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
                //{
                //    documentCheckLists.Add(addDoc);
                //}
                var vm = new DepartmentsApprovalViewModel();
                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.RCSApplicationStatusId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.RCSApplicationStatusId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var dvm = new DocumentsViewModel
                {
                    ApplicationId = application.Id,
                    CustomerId = customer.Id,
                    Documents = customerDocuments,
                    IsUploadView = false,
                    DocumentCheckLists = documentCheckLists.ToList(),
                };

                Entity entity = null;
                Agent agent = null;



                vm.Customer = customer;

                vm.CustomerDocuments = documents;
                vm.Notes = notes;
                vm.Document = dvm;
                vm.RCSApplicationStatus = rcsApps;

                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;

                var obj = new
                {
                    IdentificationNumber = vm.Customer.IdentificationNumber,
                    FullName = vm.Customer.FullName,
                    EmailAddress = vm.Customer.SystemUser == null ? vm.Customer.EmailAddress : vm.Customer.SystemUser.EmailAddress,
                    SystemUserId = vm.Customer.SystemUser == null ? 0 : vm.Customer.SystemUserId,
                    CustomerId = vm.Customer.Id
                };

                ViewBag.CustomerModel = obj;
                ViewBag.CustomerTypeId = new SelectList(context.Status.Include(x => x.StatusType).Where(x => x.StatusType.Key == StatusTypeKeys.DocumentUpload).ToList(), "Key", "Name");
                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View(rcsApps);
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult BOValidation(int? id, string ApprovalStatusddl)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId && x.IsDeleted == false).FirstOrDefault();
            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                rcsApps.StatusId = Keys.Where(x => x.Key == StatusKeys.PendingDocumentsApproval).FirstOrDefault().Id;
                db.SaveChanges();

              
                Email SendMail = new Email();
                string attorneyemail = rcscustomer.EmailAddress;
                string attorneyname = rcscustomer.FirstName + " " + rcscustomer.LastName;
                string emailbody = "Your Documents have been approved for RCS Application Reference Number: " + rcsApps.ApplicationReferenceNumber;
                SendMail.GenerateEmail(attorneyemail, "RCS-Online Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
            }
            else
            {
                rcsApps.StatusId = Keys.Where(x => x.Key == StatusKeys.UploadPOPApplicationFee).FirstOrDefault().Id;
                db.SaveChanges();
                Email SendMail = new Email();
                string attorneyemail = rcsApps.Customer.EmailAddress;
                string attorneyname = rcsApps.Customer.FirstName + " " + rcsApps.Customer.LastName;
                string emailbody = "Your Documents have been Rejected, please re-upload required documents for RCS Application Reference Number: " + rcsApps.ApplicationReferenceNumber;
                SendMail.GenerateEmail(attorneyemail, "RCS-Online Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                //foreach (var item in departapprovals)
                //{
                //    item.StatusId = rcsApps.StatusId;
                //    db.SaveChanges();
                //}
            }

            return RedirectToAction("BODocDashboard");
        }

        //Sashens
        [DecryptParameter]
        public ActionResult ApplicationFeeValidation(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer.Id;
            var Message = TempData["ApplicationFeeValidation"];
            if (Message != null)
            {
                ViewBag.MessageTitle = "Error when re-distributing case to a new user";
                ViewBag.Message = TempData["ApplicationFeeValidation"].ToString();
            }

            

            RCSApplicationStatus rcsApps = null;

                rcsApps = context.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d=>d.TransferInformation).Include(d => d.Status).FirstOrDefault();
            PaymentDetailsApi api = new PaymentDetailsApi();
            var paymentDetails= api.GetPaymentDetails(rcsApps.TransferInformation.RatesNumber);

            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved ||  x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");

            }
            else
            {

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

            }
            try
            {
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.LinkedAccounts));

                var documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSUpload);
                if (documentReferenceType == null)
                    throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                        ReferenceTypeKeys.RCSUpload));
                //var referenceTypeId = 6;
                var documentCheckLists = new List<DocumentCheckList>();

                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfPayment);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                var ApplicationStage = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;
                var vm = new DepartmentsApprovalViewModel();
                vm.OnlinePaymentHistory = db.AssessmentPaymentTransactions.Where(x => x.RCSApplicationStatusId == id && x.IsActive == true && x.ResponsibilityTypeId == ApplicationStage).ToList();


                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.RCSApplicationStatusId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.RCSApplicationStatusId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var dvm = new DocumentsViewModel
                {
                    ApplicationId = application.Id,
                    CustomerId = customer.Id,
                    Documents = customerDocuments,
                    IsUploadView = false,
                    DocumentCheckLists = documentCheckLists.ToList(),
                };

                Entity entity = null;
                Agent agent = null;



                vm.Customer = customer;

                vm.CustomerDocuments = documents;
                vm.Notes = notes;
                vm.Document = dvm;
                vm.RCSApplicationStatus = rcsApps;
                List<PaymentDetailsList> Orderded = new List<PaymentDetailsList>();
                try
                {
                    Orderded = paymentDetails.PaymentDetailsList.OrderByDescending(x => x.ConvertedDate).ToList();
                    paymentDetails.PaymentDetailsList = Orderded;
                }
                catch (Exception IO)
                {
                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }
                vm.paymentDetails = paymentDetails;
                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;

                var obj = new
                {
                    IdentificationNumber = vm.Customer.IdentificationNumber,
                    FullName = vm.Customer.FullName,
                    EmailAddress = vm.Customer.SystemUser == null ? vm.Customer.EmailAddress : vm.Customer.SystemUser.EmailAddress,
                    SystemUserId = vm.Customer.SystemUser == null ? 0 : vm.Customer.SystemUserId,
                    CustomerId = vm.Customer.Id
                };

                ViewBag.CustomerModel = obj;
                ViewBag.CustomerTypeId = new SelectList(context.Status.Include(x => x.StatusType).Where(x => x.StatusType.Key == StatusTypeKeys.DocumentUpload).ToList(), "Key", "Name");
                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View(rcsApps);
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult ApplicationFeeValidation(int? id, string ApprovalStatusddl)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                rcsApps.StatusId = Keys.Where(x => x.Key == StatusKeys.PendingDocumentsApproval).FirstOrDefault().Id;
                db.SaveChanges();

                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;
                var StatusId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == rcsApps.Id && x.StatusId == StatusId).ToList();
                foreach (var item in rrqList)
                {
                    item.CurrentTaskDateTime = DateTime.Now;
                    //item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }

                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AcknowledgementAppFeeApproved).Description.ToString()/* + DecisionType*/;
                var RCSHistoryLog = new RCSApplicationHistoryLog
                {
                    RCSApplicationStatusId = rcsApps.Id,
                    AuditAction = ActivityTrackerMessage,
                    UserId = Customer.Id,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                db.SaveChanges();

                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.ApplicationProofOfPaymentApproved).FirstOrDefault();
                Email SendMail = new Email();

                string attorneyemail = rcscustomer.EmailAddress;
                string attorneyname = rcscustomer.FirstName + " " + rcscustomer.LastName;
                string emailbody = getemailbody.Description + rcsApps.ApplicationReferenceNumber;
                //string emailbody = "Your proof of payment documents for RCS application fee has been verified for Application Reference Number: " + rcsApps.ApplicationReferenceNumber;
                SendMail.GenerateEmail(attorneyemail, "RCS-Online Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AcknowledgementAppFeeEmail).Description.ToString() +" "+ emailbody;
                var RCSHistoryLog2 = new RCSApplicationHistoryLog
                {
                    RCSApplicationStatusId = rcsApps.Id,
                    AuditAction = ActivityTrackerMessage2,
                    UserId = Customer.Id,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.RCSApplicationHistoryLogs.Add(RCSHistoryLog2);
                db.SaveChanges();
                var ActivityTrackerMessage3 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SentToAcknowledgementDocuments).Description.ToString()/* + DecisionType*/;
                var RCSHistoryLog3 = new RCSApplicationHistoryLog
                {
                    RCSApplicationStatusId = rcsApps.Id,
                    AuditAction = ActivityTrackerMessage3,
                    UserId = Customer.Id,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.RCSApplicationHistoryLogs.Add(RCSHistoryLog3);
                db.SaveChanges();
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                rcsApps.StatusId = Keys.Where(x => x.Key == StatusKeys.UploadPOPApplicationFee).FirstOrDefault().Id;
                db.SaveChanges();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AcknowledgementAppFeeRejected).Description.ToString()/* + DecisionType*/;
                var RCSHistoryLog = new RCSApplicationHistoryLog
                {
                    RCSApplicationStatusId = rcsApps.Id,
                    AuditAction = ActivityTrackerMessage,
                    UserId = Customer.Id,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                db.SaveChanges();
                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.ApplicationProofOfPaymentRejected).FirstOrDefault();
                Email SendMail = new Email();
                string attorneyemail = rcsApps.Customer.EmailAddress;
                string attorneyname = rcsApps.Customer.FirstName + " " + rcsApps.Customer.LastName;
                string emailbody = getemailbody.Description + rcsApps.ApplicationReferenceNumber;
                //string emailbody = "Your proof of payment documents for RCS application fee has been rejected , please re-upload required documents for RCS Application Reference Number: " + rcsApps.ApplicationReferenceNumber;
                SendMail.GenerateEmail(attorneyemail, "RCS-Online Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AcknowledgementAppFeeEmail).Description.ToString() + " " + emailbody;
                var RCSHistoryLog2 = new RCSApplicationHistoryLog
                {
                    RCSApplicationStatusId = rcsApps.Id,
                    AuditAction = ActivityTrackerMessage2,
                    UserId = Customer.Id,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.RCSApplicationHistoryLogs.Add(RCSHistoryLog2);
                db.SaveChanges();
                //foreach (var item in departapprovals)
                //{
                //    item.StatusId = rcsApps.StatusId;
                //    db.SaveChanges();
                //}
            }
            else if( ApprovalStatusddl == RCSActionTypeKeys.ReAllocate)
            {
                int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;
                var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();



                CaptureController c = new CaptureController();
                var SystUserId = c.RoundRobinRedistribution(true, false, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false,false);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId && x.IsActive && x.IsDeleted==false).FirstOrDefault();

                //if(ClerkId != null)
                //{
                //    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString()/* + DecisionType*/;
                //    var RCSHistoryLog = new RCSApplicationHistoryLog
                //    {
                //        RCSApplicationStatusId = rcsApps.Id,
                //        Action = ActivityTrackerMessage,
                //        UserId = Customer.Id,
                //        CreatedDateTime = DateTime.Now,
                //        IsActive = true
                //    };
                //    db.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                //    db.SaveChanges();
                //}
            
                //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                //foreach (var item in rrqList)
                //{
                //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                //    db.Entry(item).State = EntityState.Modified;
                //    db.SaveChanges();
                //}
                if (SystUserId == 0)
                {
                    AesCrypto AES = new AesCrypto();
                    var q = AES.Encrypt("id="+id.ToString());

                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString()/* + DecisionType*/;
                        var RCSHistoryLog = new RCSApplicationHistoryLog
                        {
                            RCSApplicationStatusId = rcsApps.Id,
                            AuditAction = ActivityTrackerMessage,
                            UserId = Customer.Id,
                            CreatedDateTime = DateTime.Now,
                            IsActive = true
                        };
                        db.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                        db.SaveChanges();
                    
                    TempData["ApplicationFeeValidation"] = "Please note case cannot be redistributed using round robin, as no other users are available.";
                    return RedirectToAction("ApplicationFeeValidation", new { q = q });
                }
                else
                {
                    if (ClerkId != null)
                    {
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() +" "+ ClerkId.FullName;
                        var RCSHistoryLog = new RCSApplicationHistoryLog
                        {
                            RCSApplicationStatusId = rcsApps.Id,
                            AuditAction = ActivityTrackerMessage,
                            UserId = Customer.Id,
                            CreatedDateTime = DateTime.Now,
                            IsActive = true
                        };
                        db.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                        db.SaveChanges();
                    }
                    //var ClerkId2 = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                    TempData["ApplicationFeeValidationRedistribution"] = "Please note case was successfully redistributed using round robin to "+ClerkId.FullName;
                }

           

            }

            return RedirectToAction("AcknowledgementDashboard");
        }

        [DecryptParameter]
        public ActionResult BOAssessmentFeeValidation(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer.Id;


            RCSApplicationStatus rcsApps = null;

          
                rcsApps = context.RCSApplicationStatus.Where(x => x.Id == id).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).FirstOrDefault();
    

            ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

            try
            {
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                var documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSUpload);
                if (documentReferenceType == null)
                    throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                        ReferenceTypeKeys.RCSUpload));
                //var referenceTypeId = 6;
                var documentCheckLists = new List<DocumentCheckList>();

                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                //var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

                //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

                //var AuthorityToActAttorney = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceTypeId));


                //var ProofOfProperty = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceTypeId));


                //var MunicipalStatement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

                //var MunicipalCheckList = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));

                ////this code will change
                //var SellerID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceTypeId));

                //var PurchaserID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceTypeId));

                ////var PurchaserSalesAgreement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

                ////documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));

                ////var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                ////                        .Include(d => d.ReferenceType)
                ////                        .Where(dc => dc.ApplicationId == application.Id &&
                ////                        dc.ReferenceTypeId == documentReferenceType.Id);
                //var addDocumentType = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

                //var addDoc = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId);
                //if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
                //{
                //    documentCheckLists.Add(addDoc);
                //}
                var vm = new DepartmentsApprovalViewModel();
                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.RCSApplicationStatusId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.RCSApplicationStatusId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var dvm = new DocumentsViewModel
                {
                    ApplicationId = application.Id,
                    CustomerId = customer.Id,
                    Documents = customerDocuments,
                    IsUploadView = false,
                    DocumentCheckLists = documentCheckLists.ToList(),
                };

                Entity entity = null;
                Agent agent = null;



                vm.Customer = customer;

                vm.CustomerDocuments = documents;
                vm.Notes = notes;
                vm.Document = dvm;
                vm.RCSApplicationStatus = rcsApps;

                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;

                var obj = new
                {
                    IdentificationNumber = vm.Customer.IdentificationNumber,
                    FullName = vm.Customer.FullName,
                    EmailAddress = vm.Customer.SystemUser == null ? vm.Customer.EmailAddress : vm.Customer.SystemUser.EmailAddress,
                    SystemUserId = vm.Customer.SystemUser == null ? 0 : vm.Customer.SystemUserId,
                    CustomerId = vm.Customer.Id
                };

                ViewBag.CustomerModel = obj;
                ViewBag.CustomerTypeId = new SelectList(context.Status.Include(x => x.StatusType).Where(x => x.StatusType.Key == StatusTypeKeys.DocumentUpload).ToList(), "Key", "Name");
                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View(rcsApps);
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult BOAssessmentFeeValidation(int? id, string ApprovalStatusddl)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                rcsApps.StatusId = Keys.Where(x => x.Key == StatusKeys.AssessmentFeePaymentApproved).FirstOrDefault().Id;
                rcsApps.CheckAssessmentFigureExpiry = false;
                db.SaveChanges();
            }
            else
            {
                rcsApps.StatusId = Keys.Where(x => x.Key == StatusKeys.UploadAssessmentFeePayment).FirstOrDefault().Id;
                rcsApps.CheckAssessmentFigureExpiry = true;
                db.SaveChanges();

                //foreach (var item in departapprovals)
                //{
                //    item.StatusId = rcsApps.StatusId;
                //    db.SaveChanges();
                //}
            }

            return RedirectToAction("BODocDashboard");
        }
       
        //Sashens
        [DecryptParameter]
        public ActionResult 
            AssessmentFeeValidation(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer.Id;
       
            var Message = TempData["AssessmentFeeRedistributionTitle"];
            var Title = TempData["AssessmentFeeRedistribution"];
            if (Message != null && Title != null)
            {
                ViewBag.MessageTitle = TempData["AssessmentFeeRedistributionTitle"].ToString();
                ViewBag.Message = TempData["AssessmentFeeRedistribution"].ToString();
            }

            var Message2 = TempData["AssessmentFeeTitle"];
            var Title2 = TempData["AssessmentFeeBody"];
            if (Message2 != null && Title2 != null)
            {
                ViewBag.MessageTitle2 = TempData["AssessmentFeeTitle"].ToString();
                ViewBag.Message2 = TempData["AssessmentFeeBody"].ToString();
            }

          
            RCSApplicationStatus rcsApps = null;


            rcsApps = context.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.TransferInformation).Include(d => d.Status).FirstOrDefault();

            PaymentDetailsApi api = new PaymentDetailsApi();
            var paymentDetails = api.GetPaymentDetails(rcsApps.TransferInformation.RatesNumber);

            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved ||  x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");

            }
            else
            {

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

            }
            try
            {
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                var documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSUpload);
                if (documentReferenceType == null)
                    throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                        ReferenceTypeKeys.RCSUpload));
                //var referenceTypeId = 6;
                var documentCheckLists = new List<DocumentCheckList>();

                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                //var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

                //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

                //var AuthorityToActAttorney = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceTypeId));


                //var ProofOfProperty = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceTypeId));


                //var MunicipalStatement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

                //var MunicipalCheckList = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));

                ////this code will change
                //var SellerID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceTypeId));

                //var PurchaserID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceTypeId));

                ////var PurchaserSalesAgreement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

                ////documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));

                ////var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                ////                        .Include(d => d.ReferenceType)
                ////                        .Where(dc => dc.ApplicationId == application.Id &&
                ////                        dc.ReferenceTypeId == documentReferenceType.Id);
                //var addDocumentType = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

                //var addDoc = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId);
                //if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
                //{
                //    documentCheckLists.Add(addDoc);
                //}
               var ApplicationStage = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
                var vm = new DepartmentsApprovalViewModel();

                vm.OnlinePaymentHistory = db.AssessmentPaymentTransactions.Where(x => x.RCSApplicationStatusId == id && x.IsActive == true && x.ResponsibilityTypeId == ApplicationStage).ToList();
                

                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.RCSApplicationStatusId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.RCSApplicationStatusId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var dvm = new DocumentsViewModel
                {
                    ApplicationId = application.Id,
                    CustomerId = customer.Id,
                    Documents = customerDocuments,
                    IsUploadView = false,
                    DocumentCheckLists = documentCheckLists.ToList(),
                };

                Entity entity = null;
                Agent agent = null;



                vm.Customer = customer;
                List<PaymentDetailsList> Orderded = new List<PaymentDetailsList>();
                try
                {
                    Orderded = paymentDetails.PaymentDetailsList.OrderByDescending(x => x.ConvertedDate).ToList();
                    paymentDetails.PaymentDetailsList = Orderded;
                }
                catch(Exception IO)
                {
                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }
                vm.paymentDetails = paymentDetails;
                //var dd = paymentDetails.PaymentDetailsList.OrderByDescending(x => x.ConvertedDate);
                vm.CustomerDocuments = documents;
                vm.Notes = notes;
                vm.Document = dvm;
                vm.RCSApplicationStatus = rcsApps;

                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;

                var obj = new
                {
                    IdentificationNumber = vm.Customer.IdentificationNumber,
                    FullName = vm.Customer.FullName,
                    EmailAddress = vm.Customer.SystemUser == null ? vm.Customer.EmailAddress : vm.Customer.SystemUser.EmailAddress,
                    SystemUserId = vm.Customer.SystemUser == null ? 0 : vm.Customer.SystemUserId,
                    CustomerId = vm.Customer.Id
                };

                ViewBag.CustomerModel = obj;
                ViewBag.CustomerTypeId = new SelectList(context.Status.Include(x => x.StatusType).Where(x => x.StatusType.Key == StatusTypeKeys.DocumentUpload).ToList(), "Key", "Name");
                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View(rcsApps);
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult AssessmentFeeValidation(int? id, string ApprovalStatusddl)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(x=>x.CCC).Include(x=>x.Customer).FirstOrDefault();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                CaptureController c = new CaptureController();
                var SystUserId = c.RoundRobinCCC(false, false, true, false, false, false, false, false, rcsApps.Id, 0,false,false,0);

                if(SystUserId != 0)
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;

                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrq = db.RoundRobinQueues.Where(x => x.RCSApplicationStatusId == rcsApps.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                    foreach (var item in rrq)
                    {
                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }


                    rcsApps.StatusId = Keys.Where(x => x.Key == StatusKeys.AssessmentFeePaymentApproved).FirstOrDefault().Id;
                    rcsApps.CheckAssessmentFigureExpiry = false;
                    db.SaveChanges();

                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresPOPApproved).Description.ToString();

                    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                    var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.AssessmentProofOfPaymentApproved).FirstOrDefault();
                    Email SendMail = new Email();
                    string attorneyemail = rcsApps.Customer.EmailAddress;
                    string attorneyname = rcsApps.Customer.FirstName + " " + rcsApps.Customer.LastName;
                    string emailbody = getemailbody.Description + rcsApps.ApplicationReferenceNumber;
                    var systemusermobilenum = db.SystemUsers.Where(x => x.Id == rcsApps.Customer.SystemUserId).FirstOrDefault().MobileNumber;
                    //string emailbody = "Your proof of payment documents for RCS assesment figures has been verified for Application Reference Number: " + rcsApps.ApplicationReferenceNumber;
                    var ActivityTrackerMessageEmail2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SubmitFiguresEmail).Description.ToString() + " " + emailbody;

                    SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail2, systemusermobilenum, rcsApps.Id, Customer.Id, emailbody, attorneyemail, "RCS-Online Application",
                                  emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();

                    var ActivityTrackerSendToRCC = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SentToRCC).Description.ToString();

                    var ResultSentToRCC = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerSendToRCC, ClerkId.Id);
                }
                else
                {
                 

                    var ResponsibilityType = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault();

                    var UserMessage = db.RCSUserMessages.FirstOrDefault(x => x.Key == RCSUserMessageKeys.DepartmentNotSetup);
                    UserMessage.Body = UserMessage.Body.Replace("{0}", ResponsibilityType.Name);
                    UserMessage.Body = UserMessage.Body.Replace("{1}", rcsApps.CCC.CCCName);
                    //ViewBag.DepartmentBody = UserMessage.Body;
                    //ViewBag.DepartmentTitle = UserMessage.Title;
                    TempData["AssessmentFeeTitle"] = UserMessage.Title;
                    TempData["AssessmentFeeBody"] = UserMessage.Body;
                          AesCrypto AES = new AesCrypto();
                    var q = AES.Encrypt("id=" + id.ToString());
                    return RedirectToAction("AssessmentFeeValidation", new { q = q });
                 
                }
               
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                rcsApps.StatusId = Keys.Where(x => x.Key == StatusKeys.UploadAssessmentFeePayment).FirstOrDefault().Id;
                rcsApps.CheckAssessmentFigureExpiry = true;
                db.SaveChanges();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresPOPRejected).Description.ToString();



                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.AssessmentProofOfPaymentRejected).FirstOrDefault();
                Email SendMail = new Email();
                string attorneyemail = rcsApps.Customer.EmailAddress;
                string attorneyname = rcsApps.Customer.FirstName + " " + rcsApps.Customer.LastName;
                string emailbody = getemailbody.Description + rcsApps.ApplicationReferenceNumber;
                 var systemusermobilenum = db.SystemUsers.Where(x => x.Id == rcsApps.Customer.SystemUserId).FirstOrDefault().MobileNumber;
                //string emailbody = "Your proof of payment documents for RCS assessment figures has been rejected , please re-upload required documents for RCS Application Reference Number: " + rcsApps.ApplicationReferenceNumber;

                var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SubmitFiguresEmail).Description.ToString()+" "+ emailbody;

                SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum, rcsApps.Id, Customer.Id, emailbody, attorneyemail, "RCS-Online Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
            }

            else if (ApprovalStatusddl == RCSActionTypeKeys.ReAllocate)
            {
                CaptureController c = new CaptureController();

                int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;

                var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();

              

                var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);

                //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                //foreach (var item in rrqList)
                //{
                //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                //    db.Entry(item).State = EntityState.Modified;
                //    db.SaveChanges();
                //}
                if (SystUserId == 0)
                {
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                    TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
                    TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;
                    AesCrypto AES = new AesCrypto();
                    var q = AES.Encrypt("id=" + id.ToString());
                    return RedirectToAction("AssessmentFeeValidation", new {q=q});
                }
                else
                {
                   

                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                    TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; ;
                    TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;
                    return RedirectToAction("AssessmentFiguresDashboard");
                }
 
            }



            return RedirectToAction("AssessmentFiguresDashboard");
        }
        //documents validation
        [DecryptParameter]
        public ActionResult BOEval(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer.Id;


            RCSApplicationStatus rcsApps = null;

            if (User.IsInRole("Clerks"))
            {
                rcsApps = context.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).FirstOrDefault();
            }

            ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

            try
            {
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                var documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSUpload);
                if (documentReferenceType == null)
                    throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                        ReferenceTypeKeys.RCSUpload));
            
                var documentCheckLists = new List<DocumentCheckList>();

                //var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

                //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

                var AuthorityToActAttorney = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                var ProofOfProperty = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                var MunicipalStatement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

                var MunicipalCheckList = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == documentReferenceType.Id);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                //this code will change
                var SellerID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                var PurchaserID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                //var PurchaserSalesAgreement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));

                //var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                //                        .Include(d => d.ReferenceType)
                //                        .Where(dc => dc.ApplicationId == application.Id &&
                //                        dc.ReferenceTypeId == documentReferenceType.Id);
                var addDocumentType = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

                var addDoc = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == documentReferenceType.Id);
                if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
                {
                    documentCheckLists.Add(addDoc);
                }
                var vm = new DepartmentsApprovalViewModel();
                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.RCSApplicationStatusId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.RCSApplicationStatusId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var dvm = new DocumentsViewModel
                {
                    ApplicationId = application.Id,
                    CustomerId = customer.Id,
                    Documents = customerDocuments,
                    IsUploadView = false,
                    DocumentCheckLists = documentCheckLists.ToList(),
                };

                Entity entity = null;
                Agent agent = null;

               

                vm.Customer = customer;
              
                vm.CustomerDocuments = documents;
                vm.Notes = notes;
                vm.Document = dvm;
                vm.RCSApplicationStatus = rcsApps;

                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;

                var obj = new
                {
                    IdentificationNumber = vm.Customer.IdentificationNumber,
                    FullName = vm.Customer.FullName,
                    EmailAddress = vm.Customer.SystemUser == null ? vm.Customer.EmailAddress : vm.Customer.SystemUser.EmailAddress,
                    SystemUserId = vm.Customer.SystemUser == null ? 0 : vm.Customer.SystemUserId,
                    CustomerId = vm.Customer.Id
                };

                ViewBag.CustomerModel = obj;
                ViewBag.CustomerTypeId = new SelectList(context.Status.Include(x => x.StatusType).Where(x=>x.StatusType.Key == StatusTypeKeys.DocumentUpload).ToList(), "Key", "Name");
                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View(rcsApps);
        }
      [DecryptParameter]
        public ActionResult AssessmentFiguresReAllocate(int? id)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();


            CaptureController c = new CaptureController();
            var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);

            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
            //foreach (var item in rrqList)
            //{
            //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
            if (SystUserId == 0)
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
                TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;

            }
            else
            {
                var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString()+" "+ClerkId.FullName;

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; ;
                TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;

            }

            return RedirectToAction("AssessmentFiguresDashboard");
        }

        [DecryptParameter]
        public ActionResult ManualReAllocate(int? id, int? rrqID, string ResponsibilityType, string ViewName, string TitleName, string BodyName)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;

            var plmApps = db.PropertyLeaseApplications.Where(x => x.Id == id).FirstOrDefault();
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            var rcscustomer = db.Customers.Where(x => x.Id == plmApps.CustomerId).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeName = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault();
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == rrqID).FirstOrDefault();
            var RoleName = db.Roles.Where(x => x.Name == ResponsibilityTypeName.Name).FirstOrDefault().Id;

            

            var UsersList = GetUsersInRole(RoleName).Where(x => x.RoundRobinIsActive == true).ToList();
            
            
            var vm = new ManualReAllocationViewModel();
            vm.CurrentFullName = rrq.Clerk.FullName;
            vm.RCSApplicationId = plmApps.Id;
            vm.PropertyLeaseApplicationId=plmApps.Id;
            vm.CurrentAssignedUserName = rrq.Clerk.UserFullName;
            vm.RoundRobinQueueId = rrq.Id;
            vm.ResponsibilityType = Convert.ToString(ResponsibilityTypeId);
            vm.ViewName = ViewName;
            vm.TitleName = TitleName;
            vm.BodyName = BodyName;
            //vm.CCCTypes = new SelectList(CCCs, "Id", "Name");
            ViewBag.BOUsers = new SelectList(UsersList, "SystemUser.Id", "SystemUser.FullName");
            var test = "";
            //CaptureController c = new CaptureController();
            //var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);

            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
            //foreach (var item in rrqList)
            //{
            //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
            //if (SystUserId == 0)
            //{
            //    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

            //    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

            //    TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
            //    TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;

            //}
            //else
            //{
            //    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
            //    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

            //    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

            //    TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; ;
            //    TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;

            //}

            return View(vm);
        }

        [HttpPost]
        public ActionResult ManualReAllocate(int? id, ManualReAllocationViewModel vm)
        {
            Initialise();
            //var userID = Customer.Id;

            //var Keys = db.Status;

            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == vm.RCSApplicationId && x.IsDeleted == false).FirstOrDefault();
            var plmApps = db.PropertyLeaseApplications.Where(x => x.Id == vm.RCSApplicationId && x.IsDeleted == false).FirstOrDefault();
            //var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            //int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            ////var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault().Id;
            ////var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId).FirstOrDefault();
            //var RoleName = db.Roles.Where(x => x.Name == "Acknowledge RCS Application").FirstOrDefault().Id;
            //var UsersList = GetUsersInRole(RoleName).Where(x => x.RoundRobinIsActive == true && x.CCCId == rcsApps.CCCId).ToList();


            //var vm = new ManualReAllocationViewModel();
            //vm.CurrentFullName = rrq.Clerk.FullName;
            //vm.CurrentAssignedUserName = rrq.Clerk.UserFullName;
            ////vm.CCCTypes = new SelectList(CCCs, "Id", "Name");
            //ViewBag.CCCTypes = new SelectList(UsersList, "Id", "Name");
            //CaptureController c = new CaptureController();
            //var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);

            var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == vm.RoundRobinQueueId).FirstOrDefault();
            if (rrqList != null)
            {
                rrqList.StatusId = db.Status.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                db.Entry(rrqList).State = EntityState.Modified;
                db.SaveChanges();
            }


           
            var ClerkId = db.Customers.Where(x => x.SystemUserId == vm.NewBackOfficeUser && x.IsDeleted == false).FirstOrDefault();
            var statusList = db.Status.ToList();
            var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;




            var roundRobinQueue = new RoundRobinQueue
            {
                RCSApplicationStatusId = rcsApps.Id,
                ResponsibilityTypeId = Convert.ToInt32(vm.ResponsibilityType),
                CurrentTaskDateTime = DateTime.Now,
                ClerkId = ClerkId.Id,
                StatusId = StatusId


            };
            db.RoundRobinQueues.Add(roundRobinQueue);
            db.SaveChanges();
            var Title = vm.TitleName;
            var Body = vm.BodyName;
            if (roundRobinQueue.Id == 0)
            {
                //var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                //var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;

            }
            else
            {
               
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; ;
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;

            }

            return RedirectToAction(vm.ViewName);
        }


        [DecryptParameter]
        public ActionResult ManualReAllocateDepartments(int? id, int? rrqID, string ResponsibilityType, string ViewName, string TitleName, string BodyName)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;

            var Department = db.DepartmentsApprovals.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == Department.RCSApplicationStatusId && x.IsDeleted == false).FirstOrDefault();



            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeName = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault();
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == rrqID).FirstOrDefault();
            var RoleName = db.Roles.Where(x => x.Name == ResponsibilityTypeName.Name).FirstOrDefault().Id;



            var UsersList = GetUsersInRole(RoleName).Where(x => x.RoundRobinIsActive == true && x.CCCId == rcsApps.CCCId).ToList();


            var vm = new ManualReAllocationViewModel();
            vm.CurrentFullName = rrq.Clerk.FullName;
            vm.RCSApplicationId = rcsApps.Id;
            vm.DepartmentId = Department.Id;
            vm.CurrentAssignedUserName = rrq.Clerk.UserFullName;
            vm.RoundRobinQueueId = rrq.Id;
            vm.ResponsibilityType = Convert.ToString(ResponsibilityTypeId);
            vm.ViewName = ViewName;
            vm.TitleName = TitleName;
            vm.BodyName = BodyName;
            //vm.CCCTypes = new SelectList(CCCs, "Id", "Name");
            ViewBag.BOUsers = new SelectList(UsersList, "SystemUser.Id", "SystemUser.FullName");
            var test = "";
            //CaptureController c = new CaptureController();
            //var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);

            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
            //foreach (var item in rrqList)
            //{
            //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
            //if (SystUserId == 0)
            //{
            //    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

            //    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

            //    TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
            //    TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;

            //}
            //else
            //{
            //    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
            //    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

            //    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

            //    TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; ;
            //    TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;

            //}

            return View(vm);
        }

        [HttpPost]
        public ActionResult ManualReAllocateDepartments(int? id, ManualReAllocationViewModel vm)
        {
            Initialise();
            //var userID = Customer.Id;

            //var Keys = db.Status;

            var Department = db.DepartmentsApprovals.Where(x => x.Id == vm.DepartmentId && x.IsDeleted == false).FirstOrDefault();
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == vm.RCSApplicationId && x.IsDeleted == false).FirstOrDefault();


            
            //var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            //int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            ////var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault().Id;
            ////var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId).FirstOrDefault();
            //var RoleName = db.Roles.Where(x => x.Name == "Acknowledge RCS Application").FirstOrDefault().Id;
            //var UsersList = GetUsersInRole(RoleName).Where(x => x.RoundRobinIsActive == true && x.CCCId == rcsApps.CCCId).ToList();


            //var vm = new ManualReAllocationViewModel();
            //vm.CurrentFullName = rrq.Clerk.FullName;
            //vm.CurrentAssignedUserName = rrq.Clerk.UserFullName;
            ////vm.CCCTypes = new SelectList(CCCs, "Id", "Name");
            //ViewBag.CCCTypes = new SelectList(UsersList, "Id", "Name");
            //CaptureController c = new CaptureController();
            //var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);

            var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == vm.RoundRobinQueueId).FirstOrDefault();
            if (rrqList != null)
            {
                rrqList.StatusId = db.Status.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                db.Entry(rrqList).State = EntityState.Modified;
                db.SaveChanges();
            }



            var ClerkId = db.Customers.Where(x => x.SystemUserId == vm.NewBackOfficeUser && x.IsDeleted == false).FirstOrDefault();
            var statusList = db.Status.ToList();
            var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;




            var roundRobinQueue = new RoundRobinQueue
            {
                RCSApplicationStatusId = rcsApps.Id,
                ResponsibilityTypeId = Convert.ToInt32(vm.ResponsibilityType),
                CurrentTaskDateTime = DateTime.Now,
                ClerkId = ClerkId.Id,
                StatusId = StatusId


            };
            db.RoundRobinQueues.Add(roundRobinQueue);
            db.SaveChanges();
            var Title = vm.TitleName;
            var Body = vm.BodyName;
            if (roundRobinQueue.Id == 0)
            {
                //var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                //var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;

            }
            else
            {

                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; ;
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;

            }

            return RedirectToAction(vm.ViewName);
        }


        [DecryptParameter]
        public ActionResult ManualReAllocateRefund(int? id, int? rrqID, string ResponsibilityType, string ViewName, string TitleName, string BodyName)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            var refundApps = db.RefundApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x=>x.RCSApplicationStatus).Include(x=>x.RCSApplicationStatus.CCC).FirstOrDefault();

            //var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            //var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeName = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault();
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == rrqID).FirstOrDefault();
            var RoleName = db.Roles.Where(x => x.Name == ResponsibilityTypeName.Name).FirstOrDefault().Id;



            var UsersList = GetUsersInRole(RoleName).Where(x => x.RoundRobinIsActive == true && x.CCCId == refundApps.RCSApplicationStatus.CCCId).ToList();


            var vm = new ManualReAllocationViewModel();
            vm.CurrentFullName = rrq.Clerk.FullName;
            vm.RCSApplicationId = Convert.ToInt16(refundApps.RCSApplicationStatusId);
            vm.RefundId = refundApps.Id;
            vm.CurrentAssignedUserName = rrq.Clerk.UserFullName;
            vm.RoundRobinQueueId = rrq.Id;
            vm.ResponsibilityType = Convert.ToString(ResponsibilityTypeId);
            vm.ViewName = ViewName;
            vm.TitleName = TitleName;
            vm.BodyName = BodyName;
            //vm.CCCTypes = new SelectList(CCCs, "Id", "Name");
            ViewBag.BOUsers = new SelectList(UsersList, "SystemUser.Id", "SystemUser.FullName");
            var test = "";
            //CaptureController c = new CaptureController();
            //var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);

            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
            //foreach (var item in rrqList)
            //{
            //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
            //if (SystUserId == 0)
            //{
            //    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

            //    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

            //    TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
            //    TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;

            //}
            //else
            //{
            //    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
            //    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

            //    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

            //    TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; ;
            //    TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;

            //}

            return View(vm);
        }

        [HttpPost]
        public ActionResult ManualReAllocateRefund(int? id, ManualReAllocationViewModel vm)
        {
            Initialise();
            //var userID = Customer.Id;

            //var Keys = db.Status;
            var refundApps = db.RefundApplications.Where(x => x.Id == vm.RefundId && x.IsDeleted == false).Include(x => x.RCSApplicationStatus).Include(x => x.RCSApplicationStatus.CCC).FirstOrDefault();

            //var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == vm.RCSApplicationId && x.IsDeleted == false).FirstOrDefault();
            ////var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            //int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            ////var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault().Id;
            ////var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId).FirstOrDefault();
            //var RoleName = db.Roles.Where(x => x.Name == "Acknowledge RCS Application").FirstOrDefault().Id;
            //var UsersList = GetUsersInRole(RoleName).Where(x => x.RoundRobinIsActive == true && x.CCCId == rcsApps.CCCId).ToList();


            //var vm = new ManualReAllocationViewModel();
            //vm.CurrentFullName = rrq.Clerk.FullName;
            //vm.CurrentAssignedUserName = rrq.Clerk.UserFullName;
            ////vm.CCCTypes = new SelectList(CCCs, "Id", "Name");
            //ViewBag.CCCTypes = new SelectList(UsersList, "Id", "Name");
            //CaptureController c = new CaptureController();
            //var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);

            var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == vm.RoundRobinQueueId).FirstOrDefault();
            if (rrqList != null)
            {
                rrqList.StatusId = db.Status.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                db.Entry(rrqList).State = EntityState.Modified;
                db.SaveChanges();
            }



            var ClerkId = db.Customers.Where(x => x.SystemUserId == vm.NewBackOfficeUser && x.IsDeleted == false).FirstOrDefault();
            var statusList = db.Status.ToList();
            var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;




            var roundRobinQueue = new RoundRobinQueue
            {
                RefundApplicationId = refundApps.Id,
                ResponsibilityTypeId = Convert.ToInt32(vm.ResponsibilityType),
                CurrentTaskDateTime = DateTime.Now,
                ClerkId = ClerkId.Id,
                StatusId = StatusId


            };
            db.RoundRobinQueues.Add(roundRobinQueue);
            db.SaveChanges();
            var Title = vm.TitleName;
            var Body = vm.BodyName;
            if (roundRobinQueue.Id == 0)
            {
                //var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                //var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;

            }
            else
            {

                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                var Result = ActivityTrackerAuditRefunds(refundApps.Id, Convert.ToInt16(refundApps.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; 
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;

            }

            return RedirectToAction(vm.ViewName);
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult ManualReAllocate2(int? id)
        {
            Initialise();
            //var userID = Customer.Id;

            //var Keys = db.Status;

            //var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            ////var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            ////int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            //////var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault().Id;
            //////var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId).FirstOrDefault();
            ////var RoleName = db.Roles.Where(x => x.Name == "Acknowledge RCS Application").FirstOrDefault().Id;
            ////var UsersList = GetUsersInRole(RoleName).Where(x => x.RoundRobinIsActive == true && x.CCCId == rcsApps.CCCId).ToList();


            ////var vm = new ManualReAllocationViewModel();
            ////vm.CurrentFullName = rrq.Clerk.FullName;
            ////vm.CurrentAssignedUserName = rrq.Clerk.UserFullName;
            //////vm.CCCTypes = new SelectList(CCCs, "Id", "Name");
            ////ViewBag.CCCTypes = new SelectList(UsersList, "Id", "Name");
            ////CaptureController c = new CaptureController();
            ////var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);

            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == Convert.ToInt32(vm.RoundRobinQueueId)).FirstOrDefault();
            //if (rrqList != null)
            //{
            //    rrqList.StatusId = db.Status.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(rrqList).State = EntityState.Modified;
            //    db.SaveChanges();
            //}



            //var ClerkId = db.Customers.Where(x => x.SystemUserId == vm.NewBackOfficeUser && x.IsDeleted == false).FirstOrDefault();
            //var statusList = db.Status.ToList();
            //var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;




            //var roundRobinQueue = new RoundRobinQueue
            //{
            //    RCSApplicationStatusId = rcsApps.Id,
            //    ResponsibilityTypeId = Convert.ToInt32(vm.ResponsibilityType),
            //    CurrentTaskDateTime = DateTime.Now,
            //    ClerkId = ClerkId.Id,
            //    StatusId = StatusId


            //};
            //db.RoundRobinQueues.Add(roundRobinQueue);
            //db.SaveChanges();
            //if (roundRobinQueue.Id == 0)
            //{
            //    //var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

            //    //var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

            //    TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
            //    TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;

            //}
            //else
            //{

            //    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

            //    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

            //    //TempData["AssessmentFeeRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; ;
            //    //TempData["AssessmentFeeRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;

            //}

            return View();
        }
        [DecryptParameter]
        public ActionResult DocumentVerification(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer.Id;

            var Message = TempData["ApplicationFeeValidation"];
            if (Message != null)
            {
                ViewBag.MessageTitle = "Error when re-distributing case to a new user";
                ViewBag.Message = TempData["ApplicationFeeValidation"].ToString();
            }



            RCSApplicationStatus rcsApps = null;

        
          
                rcsApps = context.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).FirstOrDefault();
           

            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved ||  x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");

            }
            else
            {

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

            }
            try
            {
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                var documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSUpload);
                if (documentReferenceType == null)
                    throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                        ReferenceTypeKeys.RCSUpload));

                var documentCheckLists = new List<DocumentCheckList>();


                //var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

                //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

                var AuthorityToActAttorney = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                //var ProofOfProperty = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                var MunicipalStatement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

                var MunicipalCheckList = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == documentReferenceType.Id);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                //this code will change
                var SellerID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == documentReferenceType.Id));
                var DeedSearch = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DeedSearch);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                var PurchaserID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                try
                {
                    var MunicipalAccInfo = context.MunicipalAccountInformations.Where(x => x.Id == rcsApps.MunicipalAccountInformationId).FirstOrDefault();
                    var ElectricityInfo = context.ElectricityMeterInformations.Where(x => x.RCSApplicationStatusId == rcsApps.Id).ToList();
                    var WaterInfo = context.WaterMeterInformations.Where(x => x.RCSApplicationStatusId == rcsApps.Id).ToList();
                    if (MunicipalAccInfo != null)
                    {
                        if (MunicipalAccInfo.PrepaidElectricityMeterNo == null && ElectricityInfo.Count > 0)
                        {
                            var ElectricityMeterReading = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ElectricityMeterReading);

                            documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ElectricityMeterReading.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                        }

                        if (WaterInfo.Count > 0)
                        {
                            var WaterMeterReading = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.WaterMeterReading);

                            documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == WaterMeterReading.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                        }

                    }

                }
                catch (Exception IO)
                {
                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }



                //var ElectricityMeterReading = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ElectricityMeterReading);

                //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ElectricityMeterReading.Id && dcl.ReferenceTypeId == referenceTypeId));


                try
                {
                    var purchasers = context.PurchaserInformations.Include(x => x.PurchaserTypes).Where(x => x.RCSApplicationStatusId == rcsApps.Id).FirstOrDefault();


                    if (purchasers != null)
                    {
                        if (purchasers.PurchaserTypes.Key == PurchaserTypeKeys.Company || purchasers.PurchaserTypes.Key == PurchaserTypeKeys.CloseCorporation)
                        {
                            var PurchaserSalesAgreement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

                            documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                        }
                    }


                    var walkin = context.WalkInApplicantDetails.Include(x => x.ActOnBehalfType).Where(x => x.RCSApplicationStatusId == rcsApps.Id).FirstOrDefault();
                    if (walkin != null)
                    {
                        if (walkin.ActOnBehalfType.Key == ActOnBehalfTypeKeys.ExecutorofEstate)
                        {
                            var ExecutorOfestate = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ExecutorofEstate);

                            documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ExecutorOfestate.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                        }
                        if (walkin.ActOnBehalfType.Key == ActOnBehalfTypeKeys.ExecutorofEstate)
                        {

                        }
                    }
                    else
                    {

                    }
                }
                catch (Exception IO)
                {
                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }


                var addDocumentType = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

                var addDoc = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == documentReferenceType.Id);
                if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
                {
                    documentCheckLists.Add(addDoc);
                }
                var vm = new DepartmentsApprovalViewModel();
                var documents = context.Documents.Where(d => d.RCSApplicationStatusId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d =>d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.RCSApplicationStatusId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var dvm = new DocumentsViewModel
                {
                    ApplicationId = application.Id,
                    CustomerId = customer.Id,
                    Documents = customerDocuments,
                    IsUploadView = false,
                    DocumentCheckLists = documentCheckLists.ToList(),
                };

                Entity entity = null;
                Agent agent = null;



                vm.Customer = customer;

                vm.CustomerDocuments = documents;
                vm.Notes = notes;
                vm.Document = dvm;
                vm.RCSApplicationStatus = rcsApps;

                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;

                var obj = new
                {
                    IdentificationNumber = vm.Customer.IdentificationNumber,
                    FullName = vm.Customer.FullName,
                    EmailAddress = vm.Customer.SystemUser == null ? vm.Customer.EmailAddress : vm.Customer.SystemUser.EmailAddress,
                    SystemUserId = vm.Customer.SystemUser == null ? 0 : vm.Customer.SystemUserId,
                    CustomerId = vm.Customer.Id
                };

                ViewBag.CustomerModel = obj;
                ViewBag.CustomerTypeId = new SelectList(context.Status.Include(x => x.StatusType).Where(x => x.StatusType.Key == StatusTypeKeys.DocumentUpload).ToList(), "Key", "Name");
                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View(rcsApps);
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult DocumentVerification(int? id, string ApprovalStatusddl, DepartmentsApprovalViewModel dvm)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                CaptureController c = new CaptureController();
                var SystUserId = c.RoundRobinCCC(false, true, false, false, false, false,false,false,rcsApps.Id,0, false, false,0);
                if(SystUserId != 0)
                {
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();

                    var ArchivedStatus = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                    //sash move out of round robin code
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;
                    var rrq = db.RoundRobinQueues.Where(x => x.RCSApplicationStatusId == rcsApps.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId != ArchivedStatus).ToList();
                    foreach (var item in rrq)
                    {
                        item.StatusId = ArchivedStatus;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }


                    rcsApps.StatusId = Keys.Where(x => x.Key == StatusKeys.BackOffice).FirstOrDefault().Id;
                    db.SaveChanges();
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AcknowledgementDocumentsApproved).Description.ToString()/* + DecisionType*/;
                    var RCSHistoryLog = new RCSApplicationHistoryLog
                    {
                        RCSApplicationStatusId = rcsApps.Id,
                        AuditAction = ActivityTrackerMessage,
                        UserId = Customer.Id,
                        CreatedDateTime = DateTime.Now,
                        IsActive = true
                    };
                    db.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                    db.SaveChanges();

                    var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.ApplicationDocsApproved).FirstOrDefault();

                    Email SendMail = new Email();
                    string attorneyemail = rcscustomer.EmailAddress;
                    string attorneyname = rcscustomer.FirstName + " " + rcscustomer.LastName;
                    string emailbody = getemailbody.Description + rcsApps.ApplicationReferenceNumber;
                    //string emailbody = "Your Documents have been approved for RCS Application Reference Number: " + rcsApps.ApplicationReferenceNumber;
                    SendMail.GenerateEmail(attorneyemail, "RCS-Online Application",
                                  emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);



                    var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AcknowledgementDocumentsEmail).Description.ToString() + " " + emailbody;
                    var RCSHistoryLog2 = new RCSApplicationHistoryLog
                    {
                        RCSApplicationStatusId = rcsApps.Id,
                        AuditAction = ActivityTrackerMessage2,
                        UserId = Customer.Id,
                        CreatedDateTime = DateTime.Now,
                        IsActive = true
                    };
                    db.RCSApplicationHistoryLogs.Add(RCSHistoryLog2);
                    db.SaveChanges();


                    var ActivityTrackerMessage3 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SentToSubmitFigures).Description.ToString() /*+ " " + ClerkId.FullName*/;

                    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage3, ClerkId.Id);


                }
                else
                {
                    RCSApplicationStatus rcsApps2 = null;



                    rcsApps2 = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d=>d.CCC).Include(d => d.Status).FirstOrDefault();

                    var ResponsibilityType = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault();

                    var UserMessage = db.RCSUserMessages.FirstOrDefault(x => x.Key == RCSUserMessageKeys.DepartmentNotSetup);
                    UserMessage.Body = UserMessage.Body.Replace("{0}", ResponsibilityType.Name);
                    UserMessage.Body = UserMessage.Body.Replace("{1}", rcsApps2.CCC.CCCName);
                    ViewBag.DepartmentBody = UserMessage.Body;
                    ViewBag.DepartmentTitle = UserMessage.Title;
                   

                    if (User.IsInRole("Area Manager"))
                    {
                        ViewBag.ApprovalStatus = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");

                    }
                    else
                    {

                        ViewBag.ApprovalStatus = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

                    }
                    try
                    {
                        var customer = db.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                                       .Include(s => s.CustomerType).FirstOrDefault(s => s.Id == rcsApps2.CustomerId);
                        if (customer == null) throw new Exception("Invalid Customer");

                        var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                        if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                        var documentReferenceType = db.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSUpload);
                        if (documentReferenceType == null)
                            throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                                ReferenceTypeKeys.RCSUpload));

                        var documentCheckLists = new List<DocumentCheckList>();


                        //var idDocumentType = _db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

                        //documentCheckLists.Add(_db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

                        var AuthorityToActAttorney = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

                        documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                        //var ProofOfProperty = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

                        //documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                        var MunicipalStatement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

                        var MunicipalCheckList = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == documentReferenceType.Id);

                        documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                        var BankConfirmationLetter = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankConfirmationLetter);

                        documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankConfirmationLetter.Id && dcl.ReferenceTypeId == documentReferenceType.Id));



                        //this code will change
                        var SellerID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

                        documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                        var PurchaserID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

                        documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                        try
                        {
                            var MunicipalAccInfo = db.MunicipalAccountInformations.Where(x => x.Id == rcsApps2.MunicipalAccountInformationId).FirstOrDefault();
                            var ElectricityInfo = db.ElectricityMeterInformations.Where(x => x.RCSApplicationStatusId == rcsApps2.Id).ToList();
                            var WaterInfo = db.WaterMeterInformations.Where(x => x.RCSApplicationStatusId == rcsApps2.Id).ToList();
                            if (WaterInfo != null && ElectricityInfo != null)
                            {
                                if (ElectricityInfo.Count > 0)
                                {
                                    var ElectricityMeterReading = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ElectricityMeterReading);

                                    documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ElectricityMeterReading.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                                }

                                if (WaterInfo.Count > 0)
                                {
                                    var WaterMeterReading = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.WaterMeterReading);

                                    documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == WaterMeterReading.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                                }

                            }

                        }
                        catch (Exception IO)
                        {
                            EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                        }



                        //var ElectricityMeterReading = _db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ElectricityMeterReading);

                        //documentCheckLists.Add(_db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ElectricityMeterReading.Id && dcl.ReferenceTypeId == referenceTypeId));


                        try
                        {
                            var purchasers = db.PurchaserInformations.Include(x => x.PurchaserTypes).Where(x => x.RCSApplicationStatusId == rcsApps2.Id).FirstOrDefault();


                            if (purchasers != null)
                            {
                                if (purchasers.PurchaserTypes.Key == PurchaserTypeKeys.Company || purchasers.PurchaserTypes.Key == PurchaserTypeKeys.CloseCorporation)
                                {
                                    var PurchaserSalesAgreement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

                                    documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                                }
                            }


                            var walkin = db.WalkInApplicantDetails.Include(x => x.ActOnBehalfType).Where(x => x.RCSApplicationStatusId == rcsApps2.Id).FirstOrDefault();
                            if (walkin != null)
                            {
                                if (walkin.ActOnBehalfType.Key == ActOnBehalfTypeKeys.ExecutorofEstate)
                                {
                                    var ExecutorOfestate = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ExecutorofEstate);

                                    documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ExecutorOfestate.Id && dcl.ReferenceTypeId == documentReferenceType.Id));


                                }
                                if (walkin.ActOnBehalfType.Key == ActOnBehalfTypeKeys.ExecutorofEstate)
                                {

                                }
                            }
                            else
                            {

                            }
                        }
                        catch (Exception)
                        {

                        }


                        var addDocumentType = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

                        var addDoc = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == documentReferenceType.Id);
                        if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
                        {
                            documentCheckLists.Add(addDoc);
                        }
                        var vm = new DepartmentsApprovalViewModel();
                        var documents = db.Documents.Where(d => d.RCSApplicationStatusId == id).ToList();
                        var notes = db.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                        var customerDocuments = db.Documents.Include(d => d.File).Include(d => d.Status)
                                                        .Where(d =>d.ReferenceType.Id == documentReferenceType.Id
                                    && d.IsActive
                                    && d.IsDeleted == false && d.RCSApplicationStatusId == id).ToList();

                        foreach (var doc in customerDocuments)
                        {
                            doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                            doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                        }

                        var documentvm = new DocumentsViewModel
                        {
                            ApplicationId = application.Id,
                            CustomerId = customer.Id,
                            Documents = customerDocuments,
                            IsUploadView = false,
                            DocumentCheckLists = documentCheckLists.ToList(),
                        };

                        Entity entity = null;
                        Agent agent = null;



                        vm.Customer = customer;

                        vm.CustomerDocuments = documents;
                        vm.Notes = notes;
                        vm.Document = documentvm;
                        vm.RCSApplicationStatus = rcsApps2;

                        ViewBag.ReferenceTypeId = documentReferenceType.Id;
                        ViewBag.ApplicationId = application.Id;

                        var obj = new
                        {
                            IdentificationNumber = vm.Customer.IdentificationNumber,
                            FullName = vm.Customer.FullName,
                            EmailAddress = vm.Customer.SystemUser == null ? vm.Customer.EmailAddress : vm.Customer.SystemUser.EmailAddress,
                            SystemUserId = vm.Customer.SystemUser == null ? 0 : vm.Customer.SystemUserId,
                            CustomerId = vm.Customer.Id
                        };

                        ViewBag.CustomerModel = obj;
                        ViewBag.CustomerTypeId = new SelectList(db.Status.Include(x => x.StatusType).Where(x => x.StatusType.Key == StatusTypeKeys.DocumentUpload).ToList(), "Key", "Name");
                        return View(vm);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }


            }
            else if(ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                var departapprovals = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == id).ToList();

                rcsApps.StatusId = Keys.Where(x => x.Key == StatusKeys.Rejected).FirstOrDefault().Id;
                db.SaveChanges();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AcknowledgementDocumentsRejected).Description.ToString();

               var Result =  ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.ApplicationDocsRejected).FirstOrDefault();


                Email SendMail = new Email();
                string attorneyemail = rcsApps.Customer.EmailAddress;
                string attorneyname = rcsApps.Customer.FirstName + " " + rcsApps.Customer.LastName;
                 var systemusermobilenum = db.SystemUsers.Where(x => x.Id == rcsApps.Customer.SystemUserId).FirstOrDefault().MobileNumber;
                string emailbody = getemailbody.Description + rcsApps.ApplicationReferenceNumber;
                //string emailbody = "Your Documents have been been Rejected, please re-upload application documents for RCS Application Reference Number: " + rcsApps.ApplicationReferenceNumber;
                var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AcknowledgementDocumentsEmail).Description.ToString() + " " +emailbody;

                SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum,rcsApps.Id, Customer.Id, emailbody, attorneyemail, "RCS-Online Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                //foreach (var item in departapprovals)
                //{
                //    item.StatusId = rcsApps.StatusId;
                //    db.SaveChanges();
                //}
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.ReAllocate)
            {
                int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;
                var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();


                CaptureController c = new CaptureController();
                var SystUserId = c.RoundRobinRedistribution(true, false, false, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                //foreach (var item in rrqList)
                //{
                //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                //    db.Entry(item).State = EntityState.Modified;
                //    db.SaveChanges();
                //}
                if (SystUserId == 0)
                {
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                    AesCrypto AES = new AesCrypto();
                    var q = AES.Encrypt("id=" + id.ToString());
                    TempData["ApplicationFeeValidation"] = "Please note case cannot be redistributed using round robin, as no other users are available.";
                    return RedirectToAction("ApplicationFeeValidation", new { q = q });
                }
                else
                {
                    if(ClerkId != null)
                    {
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                        var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                    }

                    TempData["ApplicationFeeValidationRedistribution"] = "Please note case was successfully redistributed using round robin to " + ClerkId.FullName;
                }



            }

            return RedirectToAction("AcknowledgementDashboard");
        }

        #endregion

        #region Assessment Figures
        //Sashen Assesment figure dashboard
        public ActionResult AssessmentFiguresDashboard()
        {
            Initialise();
            var userID = Customer.Id;
           

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            //Add role for sectional head
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
            {

                
                var Message = TempData["AssessmentFeeRedistributionTitle"];
                var Title = TempData["AssessmentFeeRedistribution"];
                if (Message != null && Title != null)
                {
                    ViewBag.MessageTitle = TempData["AssessmentFeeRedistributionTitle"].ToString();
                    ViewBag.Message = TempData["AssessmentFeeRedistribution"].ToString();
                }
                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                if (User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                }
                else
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }
                  
                //rrq= rrq.Where(x=>x.Clerk.SystemUser.CCC == 8)
                var list = rrq.Select(x => x.RCSApplicationStatusId).ToList();
                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.BackOffice || x.Status.Key == StatusKeys.PendingAssessmentFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();
                var ResponsibilityTypeID = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault();

                foreach (var item in rcsApps)
                {

                    var rrqName = db.RoundRobinQueues.Where(x => x.RCSApplicationStatusId == item.Id && x.ResponsibilityTypeId == ResponsibilityTypeID.Id).Include(x => x.Clerk).OrderByDescending(x => x.Id).FirstOrDefault();
                    item.Data = rrqName.Clerk.UserFullName;
                    //Add Round robin queue id to viewmodel
                    //dont forget to add the code to the dashboard view for the button
                    item.RoundRobinQueueId = Convert.ToString(rrqName.Id);
                    if (item.Status.Key == StatusKeys.BackOffice)
                    {

                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.SubmitFigures).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).FirstOrDefault();
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
                        }
                        else if (diffdays > TurnAroundDays)
                        {
                            item.ColorCode = OverdueColor;
                        }
                    }

                    else if (item.Status.Key == StatusKeys.PendingAssessmentFeePaymentValidation)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.AssessmentFiguresPaymentValidation).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).FirstOrDefault();
                        DateTime StartDateFinal = Convert.ToDateTime(StartDate.CurrentTaskDateTime);

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
            else
            {
                //rcsApps = db.RCSApplicationStatus.Where(x => x.Status.Key == StatusKeys.BackOffice).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status);

                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;

                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                var list = rrq.Select(x => x.RCSApplicationStatusId).ToList();
                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.BackOffice || x.Status.Key == StatusKeys.PendingAssessmentFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();
                foreach (var item in rcsApps)
                {
                    if (item.Status.Key == StatusKeys.BackOffice)
                    {

                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.SubmitFigures).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).FirstOrDefault();
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
                        }
                        else if (diffdays > TurnAroundDays)
                        {
                            item.ColorCode = OverdueColor;
                        }
                    }

                    else if (item.Status.Key == StatusKeys.PendingAssessmentFeePaymentValidation)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.AssessmentFiguresPaymentValidation).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).FirstOrDefault();
                        DateTime StartDateFinal = Convert.ToDateTime(StartDate.CurrentTaskDateTime);

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

        //Suhail Department Dashboards
        public ActionResult SundryDepartmentDashboard()
        {
            Initialise();
            var userID = Customer.Id;


            var Keys = db.Status;

            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;


            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
            {
                var Message = TempData["SundryRedistributionTitle"];
                var Title = TempData["SundryRedistribution"];
                if (Message != null && Title != null)
                {
                    ViewBag.MessageTitle = TempData["SundryRedistributionTitle"].ToString();
                    ViewBag.Message = TempData["SundryRedistribution"].ToString();
                }






                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();


                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                if (User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
                {
                     rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                }
                else
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }

              


                var list = rrq.Select(x => x.DepartmentApprovalId).ToList();


                var rcsApps = db.DepartmentsApprovals.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.Submitted)).Include(d => d.CreatedBySystemUser).Include(d => d.Status).Include(d => d.RCSApplicationStatus).ToList();


                var ResponsibilityTypeID = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault();


                foreach (var item in rcsApps)
                {

                    var rrqName = db.RoundRobinQueues.Where(x => x.DepartmentApprovalId == item.Id && x.ResponsibilityTypeId == ResponsibilityTypeID.Id).Include(x => x.Clerk).OrderByDescending(x => x.Id).FirstOrDefault();
                    item.Data = rrqName.Clerk.UserFullName;
                    item.RoundRobinQueueId = Convert.ToString(rrqName.Id);


                    if (item.Status.Key == StatusKeys.Submitted)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.SundryAccount).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.DepartmentApprovalId == item.Id).FirstOrDefault();
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



                }

                return View(rcsApps);

            }
            else
            {
                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.StatusId == SubmittedId && x.ResponsibilityTypeId == ResponsibilityTypeId).ToList();
                var list = rrq.Select(x => x.DepartmentApprovalId).ToList();
                var rcsApps = db.DepartmentsApprovals.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.Submitted)).Include(d => d.CreatedBySystemUser).Include(d => d.Status).Include(d => d.RCSApplicationStatus).ToList();
                foreach (var item in rcsApps)
                {
                    if (item.Status.Key == StatusKeys.Submitted)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.SundryAccount).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.DepartmentApprovalId == item.Id).FirstOrDefault();
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




                }
                return View(rcsApps);
            }
        }
        [DecryptParameter]
        public ActionResult SundryDepartmentReAllocate(int? id)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            var rcsApps = db.DepartmentsApprovals.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            //var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();


            CaptureController c = new CaptureController();
            var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, true, 0, rcsApps.Id, rrq.Id, false, false);


            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
            //foreach (var item in rrqList)
            //{
            //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
            if (SystUserId == 0)
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var failuremessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_failed" && x.IsDeleted == false).FirstOrDefault();
                TempData["SundryRedistributionTitle"] = failuremessage.Title;
                TempData["SundryRedistribution"] = failuremessage.Body;

            }
            else
            {
                var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var successmessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_successful" && x.IsDeleted == false).FirstOrDefault();
                TempData["SundryRedistributionTitle"] = successmessage.Title;
                TempData["SundryRedistribution"] = successmessage.Body + ClerkId.FullName;

            }

            return RedirectToAction("SundryDepartmentDashboard");
        }
        public ActionResult BillingDepartmentDashboard()
        {
            Initialise();
            var userID = Customer.Id;


            var Keys = db.Status;

            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin")||User.IsInRole("Back Office System Administrator") )
            {

                var Message = TempData["BillingRedistributionTitle"];
                var Title = TempData["BillingRedistribution"];
                if (Message != null && Title != null)
                {
                    ViewBag.MessageTitle = TempData["BillingRedistributionTitle"].ToString();
                    ViewBag.Message = TempData["BillingRedistribution"].ToString();
                }



                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();

                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                if (User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();

                }
                else
                {
                   rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();

                }




                var list = rrq.Select(x => x.DepartmentApprovalId).ToList();

               
                var rcsApps = db.DepartmentsApprovals.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.Submitted)).Include(d => d.CreatedBySystemUser).Include(d => d.Status).Include(d => d.RCSApplicationStatus).ToList();



                var ResponsibilityTypeID = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault();

                foreach (var item in rcsApps)
                {
                    var rrqName = db.RoundRobinQueues.Where(x => x.DepartmentApprovalId == item.Id && x.ResponsibilityTypeId == ResponsibilityTypeID.Id).Include(x => x.Clerk).OrderByDescending(x => x.Id).FirstOrDefault();
                    item.Data = rrqName.Clerk.UserFullName;
                    item.RoundRobinQueueId = Convert.ToString(rrqName.Id);
                    if (item.Status.Key == StatusKeys.Submitted)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.Billing).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.DepartmentApprovalId == item.Id).FirstOrDefault();
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



                }

                return View(rcsApps);

            }
            else
            {
                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.StatusId == SubmittedId && x.ResponsibilityTypeId == ResponsibilityTypeId).ToList();
                var list = rrq.Select(x => x.DepartmentApprovalId).ToList();
                var rcsApps = db.DepartmentsApprovals.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.Submitted)).Include(d => d.CreatedBySystemUser).Include(d => d.Status).Include(d => d.RCSApplicationStatus).ToList();
                foreach (var item in rcsApps)
                {
                    if (item.Status.Key == StatusKeys.Submitted)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.Billing).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.DepartmentApprovalId == item.Id).FirstOrDefault();
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




                }
                return View(rcsApps);
            }
        }
        [DecryptParameter]
        public ActionResult BillingReAllocate(int? id)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            var rcsApps = db.DepartmentsApprovals.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            //var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();


            CaptureController c = new CaptureController();
            var SystUserId = c.RoundRobinRedistribution(false, false, false, true, false, false, false, false, 0, rcsApps.Id, rrq.Id, false, false);
         
            
            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
            //foreach (var item in rrqList)
            //{
            //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
            if (SystUserId == 0)
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var failuremessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_failed" && x.IsDeleted == false).FirstOrDefault();
                TempData["BillingRedistributionTitle"] = failuremessage.Title ;
                TempData["BillingRedistribution"] = failuremessage.Body;

            }
            else
            {
                var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var successmessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_successful" && x.IsDeleted == false).FirstOrDefault();
                TempData["BillingRedistributionTitle"] = successmessage.Title;
                TempData["BillingRedistribution"] = successmessage.Body + ClerkId.FullName;

            }

            return RedirectToAction("BillingDepartmentDashboard");
        }


        public ActionResult CreditDepartmentDashboard()
        {
            Initialise();
            var userID = Customer.Id;


            var Keys = db.Status;

            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;


            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault().Id;

            if(Request.IsAuthenticated && (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator")))
            {
                var Message = TempData["CreditControlRedistributionTitle"];
                var Title = TempData["CreditControlRedistribution"];
                if (Message != null && Title != null)
                {
                    ViewBag.MessageTitle = TempData["CreditControlRedistributionTitle"].ToString();
                    ViewBag.Message = TempData["CreditControlRedistribution"].ToString();
                }




                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();

                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                if (User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
                {
                     rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                }
                else
                {
                     rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }


          


                var list = rrq.Select(x => x.DepartmentApprovalId).ToList();


                var rcsApps = db.DepartmentsApprovals.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.Submitted)).Include(d => d.CreatedBySystemUser).Include(d => d.Status).Include(d => d.RCSApplicationStatus).ToList();


                var ResponsibilityTypeID = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault();


                foreach (var item in rcsApps)
                {
                    var rrqName = db.RoundRobinQueues.Where(x => x.DepartmentApprovalId == item.Id && x.ResponsibilityTypeId == ResponsibilityTypeID.Id).Include(x => x.Clerk).OrderByDescending(x => x.Id).FirstOrDefault();
                    item.Data = rrqName.Clerk.UserFullName;
                    item.RoundRobinQueueId = Convert.ToString(rrqName.Id);
                    if (item.Status.Key == StatusKeys.Submitted)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.CreditControl).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.DepartmentApprovalId == item.Id).FirstOrDefault();
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



                }

                return View(rcsApps);

            }
            else
            {

                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.StatusId == SubmittedId && x.ResponsibilityTypeId == ResponsibilityTypeId).ToList();
                var list = rrq.Select(x => x.DepartmentApprovalId).ToList();
                var rcsApps = db.DepartmentsApprovals.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.Submitted)).Include(d => d.CreatedBySystemUser).Include(d => d.Status).Include(d => d.RCSApplicationStatus).ToList();
                foreach (var item in rcsApps)
                {
                    if (item.Status.Key == StatusKeys.Submitted)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.CreditControl).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.DepartmentApprovalId == item.Id).FirstOrDefault();
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




                }
                return View(rcsApps);
            }
        }
        [DecryptParameter]
        public ActionResult CreditReAllocate(int? id)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            var rcsApps = db.DepartmentsApprovals.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            //var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();


            CaptureController c = new CaptureController();
            var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, true, false, 0, rcsApps.Id, rrq.Id, false, false);


            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
            //foreach (var item in rrqList)
            //{
            //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
            if (SystUserId == 0)
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var failuremessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_failed" && x.IsDeleted == false).FirstOrDefault();
                TempData["CreditControlRedistributionTitle"] = failuremessage.Title;
                TempData["CreditControlRedistribution"] = failuremessage.Body;

            }
            else
            {
                var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var successmessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_successful" && x.IsDeleted == false).FirstOrDefault();
                TempData["CreditControlRedistributionTitle"] = successmessage.Title;
                TempData["CreditControlRedistribution"] = successmessage.Body + ClerkId.FullName;

            }

            return RedirectToAction("CreditDepartmentDashboard");
        }
        public ActionResult RatesDepartmentDashboard()
        {
            Initialise();
            var userID = Customer.Id;
            //object rcsApps = null;

            var Keys = db.Status;


            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
            {


                var Message = TempData["RatesRedistributionTitle"];
                var Title = TempData["RatesRedistribution"];
                if (Message != null && Title != null)
                {
                    ViewBag.MessageTitle = TempData["RatesRedistributionTitle"].ToString();
                    ViewBag.Message = TempData["RatesRedistribution"].ToString();
                }





                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id && x.IsDeleted == false).FirstOrDefault();


                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                if (User.IsInRole("Support Admin")|| User.IsInRole("Back Office System Administrator"))
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();


                }
                else
                {
                     rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();


                }



                var list = rrq.Select(x => x.DepartmentApprovalId).ToList();


                var rcsApps = db.DepartmentsApprovals.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.Submitted)).Include(d => d.CreatedBySystemUser).Include(d => d.Status).Include(d => d.RCSApplicationStatus).ToList();



                var ResponsibilityTypeID = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault();

                foreach (var item in rcsApps)
                {
                    var rrqName = db.RoundRobinQueues.Where(x => x.DepartmentApprovalId == item.Id && x.ResponsibilityTypeId == ResponsibilityTypeID.Id).Include(x => x.Clerk).OrderByDescending(x => x.Id).FirstOrDefault();
                    item.Data = rrqName.Clerk.UserFullName;
                    item.RoundRobinQueueId = Convert.ToString(rrqName.Id);

                    if (item.Status.Key == StatusKeys.Submitted)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.rates).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.DepartmentApprovalId == item.Id).FirstOrDefault();
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



                }

                return View(rcsApps);

            }
            else
            {
                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.StatusId == SubmittedId && x.ResponsibilityTypeId == ResponsibilityTypeId).ToList();
                var list = rrq.Select(x => x.DepartmentApprovalId).ToList();
                var rcsApps = db.DepartmentsApprovals.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.Submitted)).Include(d => d.CreatedBySystemUser).Include(d => d.Status).Include(d => d.RCSApplicationStatus).ToList();
                foreach (var item in rcsApps)
                {
                    if (item.Status.Key == StatusKeys.Submitted)
                    {
                        var Flag = db.Flags.Where(x => x.Key == FlagKeys.rates).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.DepartmentApprovalId == item.Id).FirstOrDefault();
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




                }

                return View(rcsApps);
            }
        }
        [DecryptParameter]
        public ActionResult RatesReAllocate(int? id)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            var rcsApps = db.DepartmentsApprovals.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            //var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();


            CaptureController c = new CaptureController();
            var SystUserId = c.RoundRobinRedistribution(false, false, false, false, true, false, false, false, 0, rcsApps.Id, rrq.Id, false, false);


            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
            //foreach (var item in rrqList)
            //{
            //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
            if (SystUserId == 0)
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var failuremessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_failed" && x.IsDeleted == false).FirstOrDefault();
                TempData["RatesRedistributionTitle"] = failuremessage.Title;
                TempData["RatesRedistribution"] = failuremessage.Body;

            }
            else
            {
                var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                var successmessage = db.RCSUserMessages.Where(x => x.Key == "r_rcs_redistribution_successful" && x.IsDeleted == false).FirstOrDefault();
                TempData["RatesRedistributionTitle"] = successmessage.Title;
                TempData["RatesRedistribution"] = successmessage.Body + ClerkId.FullName;

            }

            return RedirectToAction("RatesDepartmentDashboard");
        }
        //Department Flags 
        [DecryptParameter]
        public ActionResult SundriesFlag(int id)
        {
            Initialise();
            var userID = Customer.Id;

           




            return View();
        }
        
        [DecryptParameter]
        [HttpPost]
        public ActionResult SundriesFlag(int id, string Issue)
        {
            Initialise();
            var Keys = db.Status;
            int i = 0;
            int DepartmentsApprovalsID = id;
            var DepartmentsApprovals =  db.DepartmentsApprovals.Where(x => x.Id == DepartmentsApprovalsID).ToList().FirstOrDefault();
            var rcsapp = db.RCSApplicationStatus.Where(x => x.Id == DepartmentsApprovals.RCSApplicationStatusId).ToList().FirstOrDefault();
            DepartmentsApprovals.FailureReason = "Issue Resolved at Sundry Accounts";
            DepartmentsApprovals.Comment = Issue;

            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Where(x => x.DepartmentApprovalId == DepartmentsApprovalsID && x.ResponsibilityTypeId == ResponsibilityTypeId).FirstOrDefault();
            rrq.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;

            db.SaveChanges();

            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SundriesResolved).Description.ToString();

            var Result = ActivityTrackerAudit(rcsapp.Id, ActivityTrackerMessage, Customer.Id);

            List< DepartmentsApproval > DepartmentList = new List<DepartmentsApproval>();
            DepartmentList = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == DepartmentsApprovals.RCSApplicationStatusId).ToList();


            foreach (var department in DepartmentList)
            {
                if(department.Comment=="Yes")
                {
                    i++;

                }

                else
                {
                    i = i;
                }



            }
            if (i == DepartmentList.Count())
            {
                var Keyss = db.Status;
               rcsapp.StatusId= Keyss.Where(x => x.Key == StatusKeys.BackOffice).FirstOrDefault().Id;
                db.SaveChanges();
                var ActivityTracker = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AllDepartmentIssuesResolved).Description.ToString();

                var Result1 = ActivityTrackerAudit(rcsapp.Id, ActivityTracker, Customer.Id);

                return RedirectToAction("SundryDepartmentDashboard");
            }





            return RedirectToAction("SundryDepartmentDashboard");
        }

        [DecryptParameter]
        public ActionResult BillingFlag(int id)
        {
            Initialise();
            var userID = Customer.Id;






            return View();
        }
        [DecryptParameter]
        [HttpPost]
        public ActionResult BillingFlag(int id, string Issue)
        {
            Initialise();
            var Keys = db.Status;
            int i = 0;
            int DepartmentsApprovalsID = id;
            var DepartmentsApprovals = db.DepartmentsApprovals.Where(x => x.Id == DepartmentsApprovalsID).ToList().FirstOrDefault();
            var rcsapp = db.RCSApplicationStatus.Where(x => x.Id == DepartmentsApprovals.RCSApplicationStatusId).ToList().FirstOrDefault();
            DepartmentsApprovals.FailureReason = "Issue Resolved at Billing";
            DepartmentsApprovals.Comment = Issue;

            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Where(x => x.DepartmentApprovalId == DepartmentsApprovalsID && x.ResponsibilityTypeId == ResponsibilityTypeId).FirstOrDefault();
           
            rrq.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;

            db.SaveChanges();
            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.BillingResolved).Description.ToString();

            var Result = ActivityTrackerAudit(rcsapp.Id, ActivityTrackerMessage, Customer.Id);

            List<DepartmentsApproval> DepartmentList = new List<DepartmentsApproval>();
            DepartmentList = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == DepartmentsApprovals.RCSApplicationStatusId).ToList();


            foreach (var department in DepartmentList)
            {
                if (department.Comment == "Yes")
                {
                    i++;

                }

                else
                {
                    i = i;
                }



            }
            if (i == DepartmentList.Count())
            {
                var Keyss = db.Status;
                rcsapp.StatusId = Keyss.Where(x => x.Key == StatusKeys.BackOffice).FirstOrDefault().Id;
                db.SaveChanges();
                var ActivityTracker = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AllDepartmentIssuesResolved).Description.ToString();

                var Result1 = ActivityTrackerAudit(rcsapp.Id, ActivityTracker, Customer.Id);

                return RedirectToAction("BillingDepartmentDashboard");
            }





            return RedirectToAction("BillingDepartmentDashboard");
        }
        [DecryptParameter]
        public ActionResult RatesFlag(int id)
        {
            Initialise();
            var userID = Customer.Id;






            return View();
        }
        [DecryptParameter]
        [HttpPost]
        public ActionResult RatesFlag(int id, string Issue)
        {
            Initialise();
            var Keys = db.Status;
            int i = 0;
            int DepartmentsApprovalsID = id;
            var DepartmentsApprovals = db.DepartmentsApprovals.Where(x => x.Id == DepartmentsApprovalsID).ToList().FirstOrDefault();
            var rcsapp = db.RCSApplicationStatus.Where(x => x.Id == DepartmentsApprovals.RCSApplicationStatusId).ToList().FirstOrDefault();
            DepartmentsApprovals.FailureReason = "Issue Resolved at Rates";
            DepartmentsApprovals.Comment = Issue;

            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Where(x => x.DepartmentApprovalId == DepartmentsApprovalsID && x.ResponsibilityTypeId == ResponsibilityTypeId).FirstOrDefault();
            rrq.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            db.SaveChanges();
            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RatesResolved).Description.ToString();

            var Result = ActivityTrackerAudit(rcsapp.Id, ActivityTrackerMessage, Customer.Id);

            List<DepartmentsApproval> DepartmentList = new List<DepartmentsApproval>();
            DepartmentList = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == DepartmentsApprovals.RCSApplicationStatusId).ToList();


            foreach (var department in DepartmentList)
            {
                if (department.Comment == "Yes")
                {
                    i++;

                }

                else
                {
                    i = i;
                }



            }
            if (i == DepartmentList.Count())
            {
                var Keyss = db.Status;
                rcsapp.StatusId = Keyss.Where(x => x.Key == StatusKeys.BackOffice).FirstOrDefault().Id;
                db.SaveChanges();
                var ActivityTracker = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AllDepartmentIssuesResolved).Description.ToString();

                var Result1 = ActivityTrackerAudit(rcsapp.Id, ActivityTracker, Customer.Id);

                return RedirectToAction("RatesDepartmentDashboard");
            }





            return RedirectToAction("RatesDepartmentDashboard");
        }

        [DecryptParameter]
        public ActionResult CreditFlag(int id)
        {
            Initialise();
            var userID = Customer.Id;






            return View();
        }
        [DecryptParameter]
        [HttpPost]
        public ActionResult CreditFlag(int id, string Issue)
        {
            Initialise();
            var Keys = db.Status;
            int i = 0;
            int DepartmentsApprovalsID = id;
            var DepartmentsApprovals = db.DepartmentsApprovals.Where(x => x.Id == DepartmentsApprovalsID).ToList().FirstOrDefault();
            var rcsapp = db.RCSApplicationStatus.Where(x => x.Id == DepartmentsApprovals.RCSApplicationStatusId).ToList().FirstOrDefault();
            DepartmentsApprovals.FailureReason = "Issue Resolved at Credit Control";
            DepartmentsApprovals.Comment = Issue;

            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Where(x => x.DepartmentApprovalId == DepartmentsApprovalsID && x.ResponsibilityTypeId == ResponsibilityTypeId).FirstOrDefault();
            rrq.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;

            db.SaveChanges();
            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.CreditControlResolved).Description.ToString();

            var Result = ActivityTrackerAudit(rcsapp.Id, ActivityTrackerMessage, Customer.Id);


            List<DepartmentsApproval> DepartmentList = new List<DepartmentsApproval>();
            DepartmentList = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == DepartmentsApprovals.RCSApplicationStatusId).ToList();


            foreach (var department in DepartmentList)
            {
                if (department.Comment == "Yes")
                {
                    i++;

                }

                else
                {
                    i = i;
                }



            }
            if (i == DepartmentList.Count())
            {
                var Keyss = db.Status;
                rcsapp.StatusId = Keyss.Where(x => x.Key == StatusKeys.BackOffice).FirstOrDefault().Id;
                db.SaveChanges();
                var ActivityTracker = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AllDepartmentIssuesResolved).Description.ToString();

                var Result1 = ActivityTrackerAudit(rcsapp.Id, ActivityTracker, Customer.Id);

                return RedirectToAction("CreditDepartmentDashboard");
            }





            return RedirectToAction("CreditDepartmentDashboard");
        }


        //Sashen Assesment figure dashboard
        [DecryptParameter]
        public ActionResult AssessmentFigures(int id)
        {
            Initialise();
            var userID = Customer.Id;
            var Message2 = TempData["AssessmentFiguresTitle"];
            var Title2 = TempData["AssessmentFiguresBody"];
            if (Message2 != null && Title2 != null)
            {
                ViewBag.MessageTitle2 = TempData["AssessmentFiguresTitle"].ToString();
                ViewBag.Message2 = TempData["AssessmentFiguresBody"].ToString();
            }


            var documentCheckLists = new List<DocumentCheckList>();
            var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

            var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCUploadAssessment).FirstOrDefault();
            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();
            var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == id).Include(x=>x.Customer).Include(x => x.TransferInformation).FirstOrDefault();
            Customer ConveyancerCustomerId = RCSApplication.Customer;

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                CustomerId = ConveyancerCustomerId.Id,
                ApplicationId = (int)application.Id,
                Application = application,
                RcsApplicationId = id,
                ReferenceTypeId = (int)referenceType.Id,
                ReferenceType = referenceType,
                ReferenceId = ConveyancerCustomerId.Id,
                IsUploadView = true,
                Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.CustomerId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList()
            };
            dvm.DocumentCheckLists = documentCheckLists;
            SolarApiServices api = new SolarApiServices();
            var DebtorNotes = api.getDebtorNotes(RCSApplication.TransferInformation.RatesNumber);

            List<DebtorsNoteList> Orderded = new List<DebtorsNoteList>();
            try
            {
                Orderded = DebtorNotes.NoteList.OrderByDescending(x => x.ConvertedDate).ToList();
                DebtorNotes.NoteList = Orderded;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }
            dvm.DebtorsNote = DebtorNotes;
            //var Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id &&o.CustomerId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList();

            foreach (var customerDocument in dvm.Documents)
            {
                //if (SystemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(SystemUserId, customerDocument, ViewCodeKeys.UpdateCustomerDocuments))
                //{
                //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                //    return RedirectToAction("Index", "Error");
                //}
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
                //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, ConveyancerCustomerId.Id, ConveyancerCustomerId.Id, application.Id, id))));
            }

            return View(dvm);
        }
        //Sashen Assesment figure dashboard
        [DecryptParameter]
        [HttpPost]
        public ActionResult AssessmentFigures(int id, string amount, string AssessmentFigureExpiryDate,string BalanceDueDate, int RcsApplicationId, string Sundries, string Rates, string CreditControl, string Billing)
        {
            Initialise();
            CaptureController cc = new CaptureController();
       


            var checkflag = false;
            var applicationUserRoles = db.Roles.ToList();
            var SundriesUsers = (List<SystemIdentityUser>)null;
            var DepartmentUsers = (List<SystemIdentityUser>)null;
            #region department checks
            if (Sundries == "Yes")
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == id);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Sundry Account").FirstOrDefault().Id;
                SundriesUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();

                if(SundriesUsers.Count()>0)
                {
                    var getkeys = db.Status;
                    DepartmentsApproval depApprovals = new DepartmentsApproval();
                    var ApprovrcsType = db.Status.Where(x => x.Key == StatusKeys.PendingDocumentsApproval).FirstOrDefault();
                    depApprovals.StatusId = ApprovrcsType.Id;
                    //depApprovals.DepartmentId = department.Id;
                    depApprovals.CapturedDate = DateTime.Now;
                    depApprovals.FailureReason = "Failure at " + "Sundry Account";
                    var statusid = getkeys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    depApprovals.StatusId = Convert.ToInt32(statusid);
                    depApprovals.RCSApplicationStatusId = Convert.ToInt32(id);
                    db.DepartmentsApprovals.Add(depApprovals);
                    db.SaveChanges();
                    var SystemUserId = cc.RoundRobinCCC(false, false, false, false, false, false, false, true, id, depApprovals.Id, false, false, 0);
                    checkflag = true;
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystemUserId).FirstOrDefault();

                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.Sundries).Description.ToString();

                    var Result = ActivityTrackerAudit(id, ActivityTrackerMessage, ClerkId.Id);
                }
                else
                {
                    var ResponsibilityType = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault();

                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityType.Id,
                        CCCId = Convert.ToInt16(RcsApplication.CCCId),
                        CCCName = RcsApplication.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();

                

                
                    var UserMessage = db.RCSUserMessages.FirstOrDefault(x => x.Key == RCSUserMessageKeys.DepartmentNotSetup);
                    UserMessage.Body = UserMessage.Body.Replace("{0}", ResponsibilityType.Name);
                    UserMessage.Body = UserMessage.Body.Replace("{1}", RcsApplication.CCC.CCCName);
                    //ViewBag.DepartmentBody = UserMessage.Body;
                    //ViewBag.DepartmentTitle = UserMessage.Title;
                    TempData["AssessmentFiguresTitle"] = UserMessage.Title;
                    TempData["AssessmentFiguresBody"] = UserMessage.Body;
                    AesCrypto AES = new AesCrypto();
                    var q = AES.Encrypt("id=" + id.ToString());
                    return RedirectToAction("AssessmentFigures", new { q = q });
                }

             

            }
            if (Billing == "Yes")
            {

                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == id);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Billing").FirstOrDefault().Id;
                DepartmentUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();

                if (DepartmentUsers.Count() > 0)
                {
                    var getkeys = db.Status;
                    DepartmentsApproval depApprovals = new DepartmentsApproval();
                    var ApprovrcsType = db.Status.Where(x => x.Key == StatusKeys.PendingDocumentsApproval).FirstOrDefault();
                    depApprovals.StatusId = ApprovrcsType.Id;
                    //depApprovals.DepartmentId = department.Id;
                    depApprovals.CapturedDate = DateTime.Now;
                    depApprovals.FailureReason = "Failure at " + "Billing";
                    var statusid = getkeys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    depApprovals.StatusId = Convert.ToInt32(statusid);
                    depApprovals.RCSApplicationStatusId = Convert.ToInt32(id);
                    db.DepartmentsApprovals.Add(depApprovals);
                    db.SaveChanges();
                    var SystemUserId = cc.RoundRobinCCC(false, false, false, true, false, false, false, false, id, depApprovals.Id, false, false, 0);
                    checkflag = true;
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystemUserId).FirstOrDefault();

                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.Billing).Description.ToString();

                    var Result = ActivityTrackerAudit(id, ActivityTrackerMessage, ClerkId.Id);
                }
                else
                {


                    var ResponsibilityType = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault();

                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityType.Id,
                        CCCId = Convert.ToInt16(RcsApplication.CCCId),
                        CCCName = RcsApplication.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();
                    var UserMessage = db.RCSUserMessages.FirstOrDefault(x => x.Key == RCSUserMessageKeys.DepartmentNotSetup);
                    UserMessage.Body = UserMessage.Body.Replace("{0}", ResponsibilityType.Name);
                    UserMessage.Body = UserMessage.Body.Replace("{1}", RcsApplication.CCC.CCCName);
                    //ViewBag.DepartmentBody = UserMessage.Body;
                    //ViewBag.DepartmentTitle = UserMessage.Title;
                    TempData["AssessmentFiguresTitle"] = UserMessage.Title;
                    TempData["AssessmentFiguresBody"] = UserMessage.Body;
                    AesCrypto AES = new AesCrypto();
                    var q = AES.Encrypt("id=" + id.ToString());
                    return RedirectToAction("AssessmentFigures", new { q = q });
                }


            

            }
            if (Rates == "Yes")
            {

                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == id);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Rates").FirstOrDefault().Id;
                DepartmentUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();

                if(DepartmentUsers.Count() > 0)
                {

                    var getkeys = db.Status;
                    DepartmentsApproval depApprovals = new DepartmentsApproval();
                    var ApprovrcsType = db.Status.Where(x => x.Key == StatusKeys.PendingDocumentsApproval).FirstOrDefault();
                    depApprovals.StatusId = ApprovrcsType.Id;
                    //depApprovals.DepartmentId = department.Id;
                    depApprovals.CapturedDate = DateTime.Now;
                    depApprovals.FailureReason = "Failure at " + "Rates";
                    var statusid = getkeys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    depApprovals.StatusId = Convert.ToInt32(statusid);
                    depApprovals.RCSApplicationStatusId = Convert.ToInt32(id);
                    db.DepartmentsApprovals.Add(depApprovals);
                    db.SaveChanges();
                    var SystemUserId = cc.RoundRobinCCC(false, false, false, false, true, false, false, false, id, depApprovals.Id, false, false, 0);
                    checkflag = true;
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystemUserId).FirstOrDefault();

                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.Rates).Description.ToString();

                    var Result = ActivityTrackerAudit(id, ActivityTrackerMessage, ClerkId.Id);


                }



             else
                {


                    var ResponsibilityType = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault();
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityType.Id,
                        CCCId = Convert.ToInt16(RcsApplication.CCCId),
                        CCCName = RcsApplication.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false
                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();
                    var UserMessage = db.RCSUserMessages.FirstOrDefault(x => x.Key == RCSUserMessageKeys.DepartmentNotSetup);
                    UserMessage.Body = UserMessage.Body.Replace("{0}", ResponsibilityType.Name);
                    UserMessage.Body = UserMessage.Body.Replace("{1}", RcsApplication.CCC.CCCName);
                    //ViewBag.DepartmentBody = UserMessage.Body;
                    //ViewBag.DepartmentTitle = UserMessage.Title;
                    TempData["AssessmentFiguresTitle"] = UserMessage.Title;
                    TempData["AssessmentFiguresBody"] = UserMessage.Body;
                    AesCrypto AES = new AesCrypto();
                    var q = AES.Encrypt("id=" + id.ToString());
                    return RedirectToAction("AssessmentFigures", new { q = q });
                }
            }
            if (CreditControl == "Yes")
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == id);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Credit Control").FirstOrDefault().Id;
                DepartmentUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();

                if(DepartmentUsers.Count() > 0)
                {
                    var getkeys = db.Status;
                    DepartmentsApproval depApprovals = new DepartmentsApproval();
                    var ApprovrcsType = db.Status.Where(x => x.Key == StatusKeys.PendingDocumentsApproval).FirstOrDefault();
                    depApprovals.StatusId = ApprovrcsType.Id;
                    //depApprovals.DepartmentId = department.Id;
                    depApprovals.CapturedDate = DateTime.Now;
                    depApprovals.FailureReason = "Failure at " + "Credit Control";
                    var statusid = getkeys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    depApprovals.StatusId = Convert.ToInt32(statusid);
                    depApprovals.RCSApplicationStatusId = Convert.ToInt32(id);
                    db.DepartmentsApprovals.Add(depApprovals);
                    db.SaveChanges();
                    var SystemUserId = cc.RoundRobinCCC(false, false, false, false, false, false, true, false, id, depApprovals.Id, false, false, 0);
                    checkflag = true;
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystemUserId).FirstOrDefault();

                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.CreditControl).Description.ToString();

                    var Result = ActivityTrackerAudit(id, ActivityTrackerMessage, ClerkId.Id);

                }



             else
                {


                    var ResponsibilityType = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault();
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityType.Id,
                        CCCId = Convert.ToInt16(RcsApplication.CCCId),
                        CCCName = RcsApplication.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();
                    var UserMessage = db.RCSUserMessages.FirstOrDefault(x => x.Key == RCSUserMessageKeys.DepartmentNotSetup);
                    UserMessage.Body = UserMessage.Body.Replace("{0}", ResponsibilityType.Name);
                    UserMessage.Body = UserMessage.Body.Replace("{1}", RcsApplication.CCC.CCCName);
                    //ViewBag.DepartmentBody = UserMessage.Body;
                    //ViewBag.DepartmentTitle = UserMessage.Title;
                    TempData["AssessmentFiguresTitle"] = UserMessage.Title;
                    TempData["AssessmentFiguresBody"] = UserMessage.Body;
                    AesCrypto AES = new AesCrypto();
                    var q = AES.Encrypt("id=" + id.ToString());
                    return RedirectToAction("AssessmentFigures", new { q = q });
                }                               
                
            }

            if (checkflag == true)
            {
                var Keyss = db.Status;
                var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id).FirstOrDefault();
                var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();


                rcsApps.StatusId = Keyss.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                db.SaveChanges();

                return RedirectToAction("AssessmentFiguresDashboard");

            }
            #endregion






            var userID = Customer.Id;

            var documentCheckLists = new List<DocumentCheckList>();
            var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

            var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCUploadAssessment).FirstOrDefault();
            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();



            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var rcsApplication = db.RCSApplicationStatus.Include(x=>x.Customer).Where(x => x.Id == id).FirstOrDefault();
            Customer ConveyancerCustomerId = rcsApplication.Customer;



            var Docs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList docCheckList = new DocumentCheckList();
            if (Docs != null)
            {
               
                docCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == Docs.DocumentCheckListId && c.IsActive && !c.IsDeleted);
              
            }

           

            var doc = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == docCheckList.Id && o.IsActive && !o.IsDeleted).ToList();

            var isChecked = false;



           





            if (AssessmentFigureExpiryDate == null || AssessmentFigureExpiryDate == ""  ||  amount == null || amount == "" )
            {
                isChecked = true;
                ViewBag.Error = "No Rates Clearance Figures Available.";
              
            }
            //else if ((amount == "0.00" || amount == "0") && doc.Count > 0)
            //{
            //    isChecked = true;
            //    ViewBag.Error = "Capture The Total Assessment Figure";
               
            //}
            else if (doc.Count == 0)
            {
                isChecked = true;
                ViewBag.Error = "Rates Clearance Figures Document";
               
            }
            
            var Keys = db.Status;
            bool FiguresFullyPaid = false;
            if (!isChecked)
            {
                var depApprovals = db.RCSApplicationStatus.Where(x => x.Id == id).Include(o => o.Customer).FirstOrDefault();


                if (amount != null && amount != "")
                {
                  
                    var replace = (amount).Replace(',', '.');
                    var ConvertedAmount = Convert.ToDecimal(replace, CultureInfo.InvariantCulture);
                    depApprovals.amount = ConvertedAmount;

                    if(ConvertedAmount > 0)
                    {
                        depApprovals.AssessmentFiguresFullyPaid = false;
                        depApprovals.AssessmentFigureAmountOutstanding = ConvertedAmount;
                        depApprovals.AssessmentFigureTotalAmount = ConvertedAmount;
                    }
                    else
                    {
                        depApprovals.AssessmentFiguresFullyPaid = true;
                        FiguresFullyPaid = true;
                        depApprovals.AssessmentFigureAmountOutstanding = 0;
                        depApprovals.AssessmentFigureTotalAmount = 0;
                    }
                
                    depApprovals.AssessmentFigureAmountPaid = 0;
                

                 

                    //depApprovals.amount = decimal.Parse((amount).Replace(',', '.').ToString(), CultureInfo.InvariantCulture);


                }
                DateTime FormattedAssessmentFigureExpiryDate = new DateTime();
                if (AssessmentFigureExpiryDate.Length == 8)
                {
                    var Year = AssessmentFigureExpiryDate.Substring(0, 4);
                    var Month = AssessmentFigureExpiryDate.Substring(4, 2);
                    var Day = AssessmentFigureExpiryDate.Substring(6, 2);
                    var DateFinal = Year + '-' + Month + '-' + Day;
                    FormattedAssessmentFigureExpiryDate = Convert.ToDateTime(DateFinal);
                }
                depApprovals.AssessmentFiguresEndDate = FormattedAssessmentFigureExpiryDate;
                if (BalanceDueDate.Length == 8)
                {
                    var Year = BalanceDueDate.Substring(0, 4);
                    var Month = BalanceDueDate.Substring(4, 2);
                    var Day = BalanceDueDate.Substring(6, 2);
                    var DateFinal = Year + '-' + Month + '-' + Day;
                    FormattedAssessmentFigureExpiryDate = Convert.ToDateTime(DateFinal);
                    depApprovals.AssessmentFigureExpiryDate = FormattedAssessmentFigureExpiryDate;
                }
            
                if (FiguresFullyPaid == false)
                {
                    depApprovals.StatusId = Keys.Where(x => x.Key == StatusKeys.ViewAssessmentFigure).FirstOrDefault().Id;
                }
                else
                {
                    depApprovals.StatusId = Keys.Where(x => x.Key == StatusKeys.PendingAssessmentFeePaymentValidation).FirstOrDefault().Id;

                }

                depApprovals.AssessmentFigureUploadDate = DateTime.Now;
                depApprovals.CheckAssessmentFigureExpiry = true;
                depApprovals.AssessmentFiguresExpired = false;
                db.Entry(depApprovals).State = EntityState.Modified;
                db.SaveChanges();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresSentToConveyancer).Description.ToString();

                var Result = ActivityTrackerAudit(depApprovals.Id, ActivityTrackerMessage, Customer.Id);

           

                if(FiguresFullyPaid ==false)
                {
                    var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.AssessmentFigureUploaded).FirstOrDefault();
                    Email SendMail = new Email();
                    string attorneyemail = depApprovals.Customer.EmailAddress;
                    string attorneyname = depApprovals.Customer.FirstName + " " + depApprovals.Customer.LastName;
                    string emailbody = getemailbody.Description;
                    var systemusermobilenum = db.SystemUsers.Where(x => x.Id == depApprovals.Customer.SystemUserId).FirstOrDefault().MobileNumber;
                    //string emailbody = "Rates Clearance figures sent, please login to your RCS application and view your assessment figures. ";
                    var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SubmitFiguresEmail).Description.ToString() + " " + emailbody;

                    SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum, depApprovals.Id, Customer.Id, emailbody, attorneyemail, "RCS- New Online Application Submission",
                                  emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                }
                else
                {
                    depApprovals.CheckAssessmentFigureExpiry = false;
                    db.Entry(depApprovals).State = EntityState.Modified;
                    db.SaveChanges();
                    var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.AssessmentFiguresFullyPaid).FirstOrDefault();
                    Email SendMail = new Email();
                    string attorneyemail = depApprovals.Customer.EmailAddress;
                    string attorneyname = depApprovals.Customer.FirstName + " " + depApprovals.Customer.LastName;
                    string emailbody = getemailbody.Description;
                    var systemusermobilenum = db.SystemUsers.Where(x => x.Id == depApprovals.Customer.SystemUserId).FirstOrDefault().MobileNumber;
                    //string emailbody = "Rates Clearance figures sent, please login to your RCS application and view your assessment figures. ";
                    var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SubmitFiguresEmail).Description.ToString() + " " + emailbody;

                    SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum, depApprovals.Id, Customer.Id, emailbody, attorneyemail, "RCS- New Online Application Submission",
                                  emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);


                    var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresCredit).Description.ToString();

                    var Result2 = ActivityTrackerAudit(depApprovals.Id, ActivityTrackerMessage2, Customer.Id);

                }





            }
            else
            {

                var dvm = new DocumentsViewModel
                {
                    CustomerId = ConveyancerCustomerId.Id,
                    ApplicationId = (int)application.Id,
                    Application = application,
                    RcsApplicationId = id,
                    ReferenceTypeId = (int)referenceType.Id,
                    ReferenceType = referenceType,
                    ReferenceId = ConveyancerCustomerId.Id,
                    IsUploadView = true,
                    Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList()
                };
                dvm.DocumentCheckLists = documentCheckLists;
                var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.TransferInformation).Include(d => d.Status).FirstOrDefault();

                SolarApiServices api = new SolarApiServices();
                var DebtorNotes = api.getDebtorNotes(rcsApps.TransferInformation.RatesNumber);

                List<DebtorsNoteList> Orderded = new List<DebtorsNoteList>();
                try
                {
                    Orderded = DebtorNotes.NoteList.OrderByDescending(x => x.ConvertedDate).ToList();
                    DebtorNotes.NoteList = Orderded;
                }
                catch (Exception IO)
                {
                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }
                dvm.DebtorsNote = DebtorNotes;
                var docChecklistID = dvm.DocumentCheckLists[0].Id;
                foreach (var customerDocument in dvm.Documents)
                {
                    
                    if (customerDocument.File != null)
                        customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                    var docCheckList1 =
                         db.DocumentCheckLists.Include(d => d.DocumentType)
                             .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                    customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList1.DocumentType.Name,
                        customerDocument.DocumentName);
                   
                    customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
                }
                foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
                {

                    documentCheckList.DataList = new List<string>();
                    documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, ConveyancerCustomerId.Id, ConveyancerCustomerId.Id, application.Id, id))));
                }


                return View(dvm);
            }

            return RedirectToAction("AssessmentFiguresDashboard");
        }

        //Api call to generate assessment figures
        public JsonResult GenerareAssessmentFigures(string RCSId)
        {
            object result = null;

            try
            {
                if (RCSId.Trim() == "")
                {
                    result = null;
                }
                else
                {
                    //try
                    //{

                    //}
                    //catch(Exception eo)
                    //{
                
                    //    AssessmentFigurePayload errPayload = new AssessmentFigurePayload();
                    //    errPayload.Status = "Error";
                    //    errPayload.AccountNumber = "1 "+eo.Message;
                    //    result = errPayload;
                    
                    //return Json(result, JsonRequestBehavior.AllowGet);
                    // }

                    //try
                    //{

                    //}
                    //catch (Exception eo)
                    //{

                    //    AssessmentFigurePayload errPayload = new AssessmentFigurePayload();
                    //    errPayload.Status = "Error";
                    //    errPayload.AccountNumber = "2 "+eo.Message;
                    //    result = errPayload;

                    //    return Json(result, JsonRequestBehavior.AllowGet);
                    //}
                    int ids = Convert.ToInt32(RCSId);
                    var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == ids).Include(x => x.TransferInformation).FirstOrDefault();
                    var MuniAccNo = RCSApplication.TransferInformation.RatesNumber;

                    SolarCalculateAssessmentFigureApi p = new SolarCalculateAssessmentFigureApi();
                    var details = p.calculateAssessmentFigures(MuniAccNo.ToString());

                    SolarAccountBalanceApi sab = new SolarAccountBalanceApi();
                    var accountdetails = sab.GetAccountBalance(MuniAccNo.ToString());
                    dynamic datas= JObject.Parse(accountdetails);
                    //DateTime io = Convert.ToDateTime("hshshsh");
                    dynamic data = JObject.Parse(details);
                    AssessmentFigurePayload afp = new AssessmentFigurePayload();
                    data = data.SolarERP.payload;

                    foreach (var item in data.StatusMessages)
                    {
                        afp.Status = item;

                        
                    }

                    if (afp.Status != "Success")
                    {
                        result = afp;
                    }
                    else
                    {


                        string txt = (data.OutstandingAmount);
                        var outstandingamountString = (txt).Replace(',', '.');
                        afp.OutstandingAmount = Convert.ToString(outstandingamountString);

                        if (afp.OutstandingAmount != null || afp.OutstandingAmount != "")
                        {
                            var replace = (afp.OutstandingAmount).Replace(',', '.');
                            var Converted = Convert.ToDecimal(afp.OutstandingAmount, CultureInfo.InvariantCulture);

                            afp.FormattedOutstandingAmount = Converted;
                        }

                        afp.VerifyAccountBillingCycle = Convert.ToString(data.VerifyAccountBillingCycle);
                        afp.BillingCycleNumber = Convert.ToString(data.BillingCycleNumber);
                        afp.BillingCycleChange = Convert.ToString(data.BillingCycleChange);
                        afp.BalanceDueDate = Convert.ToString(data.BalanceDueDate);
                        afp.AssessmentFigureExpiryDate = Convert.ToString(data.AssessmentFigureExpiryDate);
                        if (afp.AssessmentFigureExpiryDate.Length == 8)
                        {
                            var Year = afp.AssessmentFigureExpiryDate.Substring(0, 4);
                            var Month = afp.AssessmentFigureExpiryDate.Substring(4, 2);
                            var Day = afp.AssessmentFigureExpiryDate.Substring(6, 2);
                            var DateFinal = Year + '-' + Month + '-' + Day;
                            afp.FormattedAssessmentFigureExpiryDate = Convert.ToDateTime(DateFinal);
                        }
                        afp.date = afp.FormattedAssessmentFigureExpiryDate;
                        afp.CalculationMonths = Convert.ToString(data.CalculationMonths);
                        afp.AccountBalance = Convert.ToString(datas.AccountBalance);
                        var DebitCreditFlag = Convert.ToString(datas.DtCrFlag);
                        if (DebitCreditFlag == "D")
                        {
                            afp.DtCrFlag = "Debit;";
                        }
                        else
                            if (DebitCreditFlag == "C")
                        {
                            afp.DtCrFlag = "Credit";
                        }
                        afp.PaymentBy = Convert.ToString(datas.PaymentBy);



                        result = afp;
                    }


                    //var propertyinfo = db.CustomerAccounts.Where(x => x.AccountNo == MuniAccNo).FirstOrDefault();
                    //result = propertyinfo;

                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                var e = ex.Message;
                AssessmentFigurePayload errPayload = new AssessmentFigurePayload();
                errPayload.Status = "Error";
                errPayload.AccountNumber = e;
                result = errPayload;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public RCSApplicationStatus GenerareAssessmentFigures2(string RCSId)
        {
            RCSApplicationStatus rcsapp = new RCSApplicationStatus();
            object result = null;

            try
            {
                if (RCSId.Trim() == "")
                {
                    result = null;
                }
                else
                {
                    //try
                    //{

                    //}
                    //catch(Exception eo)
                    //{

                    //    AssessmentFigurePayload errPayload = new AssessmentFigurePayload();
                    //    errPayload.Status = "Error";
                    //    errPayload.AccountNumber = "1 "+eo.Message;
                    //    result = errPayload;

                    //return Json(result, JsonRequestBehavior.AllowGet);
                    // }

                    //try
                    //{

                    //}
                    //catch (Exception eo)
                    //{

                    //    AssessmentFigurePayload errPayload = new AssessmentFigurePayload();
                    //    errPayload.Status = "Error";
                    //    errPayload.AccountNumber = "2 "+eo.Message;
                    //    result = errPayload;

                    //    return Json(result, JsonRequestBehavior.AllowGet);
                    //}
                    int ids = Convert.ToInt32(RCSId);
                    var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == ids).Include(x => x.TransferInformation).FirstOrDefault();
                    var MuniAccNo = RCSApplication.TransferInformation.RatesNumber;

                    SolarCalculateAssessmentFigureApi p = new SolarCalculateAssessmentFigureApi();
                    var details = p.calculateAssessmentFigures(MuniAccNo.ToString());

                    SolarAccountBalanceApi sab = new SolarAccountBalanceApi();
                    var accountdetails = sab.GetAccountBalance(MuniAccNo.ToString());
                    dynamic datas = JObject.Parse(accountdetails);
                    //DateTime io = Convert.ToDateTime("hshshsh");
                    dynamic data = JObject.Parse(details);
                    AssessmentFigurePayload afp = new AssessmentFigurePayload();
                    data = data.SolarERP.payload;

                    foreach (var item in data.StatusMessages)
                    {
                        afp.Status = item;


                    }

                    if (afp.Status != "Success")
                    {
                        result = afp;
                    }
                    else
                    {


                        string txt = (data.OutstandingAmount);
                        var outstandingamountString = (txt).Replace(',', '.');
                        afp.OutstandingAmount = Convert.ToString(outstandingamountString);

                        if (afp.OutstandingAmount != null || afp.OutstandingAmount != "")
                        {
                            var replace = (afp.OutstandingAmount).Replace(',', '.');
                            var Converted = Convert.ToDecimal(afp.OutstandingAmount, CultureInfo.InvariantCulture);

                            afp.FormattedOutstandingAmount = Converted;
                        }

                        afp.VerifyAccountBillingCycle = Convert.ToString(data.VerifyAccountBillingCycle);
                        afp.BillingCycleNumber = Convert.ToString(data.BillingCycleNumber);
                        afp.BillingCycleChange = Convert.ToString(data.BillingCycleChange);
                        afp.BalanceDueDate = Convert.ToString(data.BalanceDueDate);
                      
                        if (afp.BalanceDueDate.Length == 8)
                        {
                            var Year = afp.BalanceDueDate.Substring(0, 4);
                            var Month = afp.BalanceDueDate.Substring(4, 2);
                            var Day = afp.BalanceDueDate.Substring(6, 2);
                            var DateFinal = Year + '-' + Month + '-' + Day;
                            afp.FormattedAssessmentFigureExpiryDate = Convert.ToDateTime(DateFinal);
                        }
                        afp.AssessmentFigureExpiryDate = Convert.ToString(data.AssessmentFigureExpiryDate);
                        if (afp.AssessmentFigureExpiryDate.Length == 8)
                        {
                            var Year = afp.AssessmentFigureExpiryDate.Substring(0, 4);
                            var Month = afp.AssessmentFigureExpiryDate.Substring(4, 2);
                            var Day = afp.AssessmentFigureExpiryDate.Substring(6, 2);
                            var DateFinal = Year + '-' + Month + '-' + Day;
                            afp.FormattedAssessmentFigureEndDate = Convert.ToDateTime(DateFinal);
                        }


                        afp.date = afp.FormattedAssessmentFigureExpiryDate;
                        afp.CalculationMonths = Convert.ToString(data.CalculationMonths);
                        afp.AccountBalance = Convert.ToString(datas.AccountBalance);

                        rcsapp.AssessmentFigureExpiryDate = afp.FormattedAssessmentFigureExpiryDate;
                        rcsapp.AssessmentFiguresEndDate = afp.FormattedAssessmentFigureEndDate;





                        var DebitCreditFlag = Convert.ToString(datas.DtCrFlag);
                        if (DebitCreditFlag == "D")
                        {
                            afp.DtCrFlag = "Debit;";
                        }
                        else
                            if (DebitCreditFlag == "C")
                        {
                            afp.DtCrFlag = "Credit";
                        }
                        afp.PaymentBy = Convert.ToString(datas.PaymentBy);



                        result = rcsapp;
                    }


                    //var propertyinfo = db.CustomerAccounts.Where(x => x.AccountNo == MuniAccNo).FirstOrDefault();
                    //result = propertyinfo;

                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                var e = ex.Message;
                AssessmentFigurePayload errPayload = new AssessmentFigurePayload();
                errPayload.Status = "Error";
                errPayload.AccountNumber = e;
                result = errPayload;
            }
           

            return rcsapp;
        }

        public JsonResult CheckAccountBalancce(string RCSId)
        {
            object result = null;

            try
            {
                if (RCSId.Trim() == "")
                {
                    result = null;
                }
                else
                {
                    int ids = Convert.ToInt32(RCSId);
                    var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == ids).Include(x => x.TransferInformation).FirstOrDefault();
                    var MuniAccNo = RCSApplication.TransferInformation.RatesNumber;

                   

                    SolarAccountBalanceApi sab = new SolarAccountBalanceApi();
                    var accountdetails = sab.GetAccountBalance(MuniAccNo.ToString());
                    dynamic datas = JObject.Parse(accountdetails);
                    datas = datas.SolarERP.payload;

                    AssessmentFigurePayload afp = new AssessmentFigurePayload();

                   
                    afp.AccountBalance = Convert.ToString(datas.AccountBalance);
                    var DebitCreditFlag = Convert.ToString(datas.DrCrFlag);
                    if (DebitCreditFlag == "D")
                    {
                        afp.DtCrFlag = "Debit";
                    }
                    else
                        if (DebitCreditFlag == "C")
                    {
                        afp.DtCrFlag = "Credit";
                    }
                    afp.PaymentBy = Convert.ToString(datas.PaymentBy);



                    result = afp;

                    //var propertyinfo = db.CustomerAccounts.Where(x => x.AccountNo == MuniAccNo).FirstOrDefault();
                    //result = propertyinfo;

                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                var e = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UploadAssessmentFigureDashboard()
            {
                Initialise();
                var userID = Customer.Id;
                object rcsApps = null;

                var Keys = db.Status;

                if (User.IsInRole("Clerks"))
                {
                    rcsApps = db.RCSApplicationStatus.Where(x => x.Status.Key == StatusKeys.BackOffice).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status);
                }

                return View(rcsApps);
            }

            public ActionResult UploadAssessmentFigure(int id)
            {
                Initialise();
                var userID = Customer.Id;

                var documentCheckLists = new List<DocumentCheckList>();
                var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

                documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

                var referenceType = db.ReferenceTypes.Where(x=>x.Key == ReferenceTypeKeys.RCUploadAssessment).FirstOrDefault();
                var application = db.Applications.Where(x=>x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();



                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();
                var dvm = new DocumentsViewModel
                {
                    CustomerId = userID,
                    ApplicationId = (int)application.Id,
                    Application = application,
                    RcsApplicationId = id,
                    ReferenceTypeId = (int)referenceType.Id,
                    ReferenceType = referenceType,
                    ReferenceId = Customer.Id,
                    IsUploadView = true,
                    Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == Customer.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList()
                };
                dvm.DocumentCheckLists = documentCheckLists;



                foreach (var customerDocument in dvm.Documents)
                {
                    //if (SystemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(SystemUserId, customerDocument, ViewCodeKeys.UpdateCustomerDocuments))
                    //{
                    //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                    //    return RedirectToAction("Index", "Error");
                    //}
                    if (customerDocument.File != null)
                        customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                    var docCheckList =
                         db.DocumentCheckLists.Include(d => d.DocumentType)
                             .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                    customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                        customerDocument.DocumentName);
                    //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
                    //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
                    customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
                }

                foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
                {

                    documentCheckList.DataList = new List<string>();
                    documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, Customer.Id, Customer.Id, application.Id, id))));
                }
         
                return View(dvm);
            }

        [HttpPost]
        public ActionResult UploadAssessmentFigure(int id, string amount, int RcsApplicationId, string Sundries, string Rates, string CreditControl, string Billing)
        {
            Initialise();
            var userID = Customer.Id;

            var documentCheckLists = new List<DocumentCheckList>();
            var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

            var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCUploadAssessment).FirstOrDefault();
            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();



            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            var Docs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == Customer.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList docCheckList = new DocumentCheckList();
            if(Docs!=null)
            {
                //if (Docs.File != null)
                //    Docs.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", Docs.FileId));

                docCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == Docs.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                //Docs.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                //    Docs.DocumentName);
                //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
                //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
                //Docs.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", Docs.Id)));
            }

            //foreach (var customerDocument in Docs)
            //{
            //    //if (SystemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(SystemUserId, customerDocument, ViewCodeKeys.UpdateCustomerDocuments))
            //    //{
            //    //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
            //    //    return RedirectToAction("Index", "Error");
            //    //}
            //    if (customerDocument.File != null)
            //        customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

            //    var docCheckList =
            //         db.DocumentCheckLists.Include(d => d.DocumentType)
            //             .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
            //    customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
            //        customerDocument.DocumentName);
            //    //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
            //    //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
            //    customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            //}

            
            var doc = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == Customer.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == docCheckList.Id && o.IsActive && !o.IsDeleted).ToList();

            var isChecked = false;

            if ((amount == "0.00" || amount == "0") && doc.Count == 0)
            {
                isChecked = true;
                ViewBag.Error = "Please Attach Assessment Figure Document And Capture The Total Assessment Figure";
                //return View(dvm);

                //ViewBag.Error = "Please Enter Amount For Conveyancer To Pay";
                //return View("");
            }
            else if ((amount == "0.00" || amount == "0") && doc.Count > 0)
            {
                isChecked = true;
                ViewBag.Error = "Capture The Total Assessment Figure";
                //return View(dvm);
            }
            else if (doc.Count == 0)
            {
                isChecked = true;
                ViewBag.Error = "Please Attach Assessment Figures Document";
                //return View(dvm);
            }
            //Initialise();
            //var userID = Customer.Id;
            var Keys = db.Status;
           if(!isChecked)
            { 
            var depApprovals = db.RCSApplicationStatus.Where(x => x.Id == id).Include(o => o.Customer).FirstOrDefault();


            if (amount != null & amount != "")
            {
                var test = (amount).Replace(',', '.');
                var ttt = Convert.ToDecimal(amount, CultureInfo.InvariantCulture);
                depApprovals.amount = ttt;
                //depApprovals.amount = decimal.Parse((amount).Replace(',', '.').ToString(), CultureInfo.InvariantCulture);


            }
                Email SendMail = new Email();
                string attorneyemail = depApprovals.Customer.EmailAddress;
                string attorneyname = depApprovals.Customer.FirstName + " " + depApprovals.Customer.LastName;
                string emailbody = "Rates Clearance figures sent";
                SendMail.GenerateEmail(attorneyemail, "RCS- New Online Application Submission",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                depApprovals.StatusId = Keys.Where(x => x.Key == StatusKeys.ViewAssessmentFigure).FirstOrDefault().Id;
            db.Entry(depApprovals).State = EntityState.Modified;
            db.SaveChanges();





        }
        else
        {
                var dvm = new DocumentsViewModel
                {
                    CustomerId = userID,
                    ApplicationId = (int)application.Id,
                    Application = application,
                    RcsApplicationId = id,
                    ReferenceTypeId = (int)referenceType.Id,
                    ReferenceType = referenceType,
                    ReferenceId = Customer.Id,
                    IsUploadView = true,
                    Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == Customer.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList()
                };
                dvm.DocumentCheckLists = documentCheckLists;
                var docChecklistID = dvm.DocumentCheckLists[0].Id;
                foreach (var customerDocument in dvm.Documents)
                {
                    //if (SystemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(SystemUserId, customerDocument, ViewCodeKeys.UpdateCustomerDocuments))
                    //{
                    //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                    //    return RedirectToAction("Index", "Error");
                    //}
                    if (customerDocument.File != null)
                        customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                    var docCheckList1 =
                         db.DocumentCheckLists.Include(d => d.DocumentType)
                             .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                    customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList1.DocumentType.Name,
                        customerDocument.DocumentName);
                    //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
                    //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
                    customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
                }
                foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
                {

                    documentCheckList.DataList = new List<string>();
                    documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, Customer.Id, Customer.Id, application.Id, id))));
                }
              
                //dvm = null;
                return View(dvm);
    }

            return RedirectToAction("UploadAssessmentFigureDashboard");
            }


        #endregion

        #region RCC Certificate
            public ActionResult RCCCertificateDashboard()
            {
                Initialise();
                var userID = Customer.Id;
                object rcsApps = null;

                var Keys = db.Status;

                if (User.IsInRole("Clerks"))
                {
                    rcsApps = db.RCSApplicationStatus.Where(x => x.Status.Key == StatusKeys.AssessmentFeePaymentApproved && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status);
                }

                return View(rcsApps);
            }

            public ActionResult UploadRCCCertificate(int id)
            {
                Initialise();
                var userID = Customer.Id;

                var documentCheckLists = new List<DocumentCheckList>();
                var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCCCertificate);

                documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

                var referenceType = db.ReferenceTypes.Where(x=>x.Key == ReferenceTypeKeys.RCCCertificateUpload).FirstOrDefault();
                var application = db.Applications.Where(x=>x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();



                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();
                var dvm = new DocumentsViewModel
                {
                    CustomerId = userID,
                    ApplicationId = (int)application.Id,
                    Application = application,
                    RcsApplicationId = id,
                    ReferenceTypeId = (int)referenceType.Id,
                    ReferenceType = referenceType,
                    ReferenceId = Customer.Id,
                    IsUploadView = true,
                    Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == Customer.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList()
                };
                dvm.DocumentCheckLists = documentCheckLists;



                foreach (var customerDocument in dvm.Documents)
                {
                    //if (SystemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(SystemUserId, customerDocument, ViewCodeKeys.UpdateCustomerDocuments))
                    //{
                    //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                    //    return RedirectToAction("Index", "Error");
                    //}
                    if (customerDocument.File != null)
                        customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                    var docCheckList =
                         db.DocumentCheckLists.Include(d => d.DocumentType)
                             .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                    customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                        customerDocument.DocumentName);
                    //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
                    //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
                    customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
                }

                foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
                {

                    documentCheckList.DataList = new List<string>();
                    documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, referenceType.Id, Customer.Id, Customer.Id, application.Id, id))));
                }
         
                return View(dvm);
            }

            [HttpPost]
            public ActionResult UploadRCCCertificate(int id , int RcsApplicationId)
            {
                Initialise();
                var userID = Customer.Id;

            var documentCheckLists = new List<DocumentCheckList>();
            var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCCCertificate);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

            var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCCCertificateUpload).FirstOrDefault();
            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();



            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                CustomerId = userID,
                ApplicationId = (int)application.Id,
                Application = application,
                RcsApplicationId = id,
                ReferenceTypeId = (int)referenceType.Id,
                ReferenceType = referenceType,
                ReferenceId = Customer.Id,
                IsUploadView = true,
                Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == Customer.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList()
            };
            dvm.DocumentCheckLists = documentCheckLists;



            foreach (var customerDocument in dvm.Documents)
            {
                //if (SystemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(SystemUserId, customerDocument, ViewCodeKeys.UpdateCustomerDocuments))
                //{
                //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                //    return RedirectToAction("Index", "Error");
                //}
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
                //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, referenceType.Id, Customer.Id, Customer.Id, application.Id, id))));
            }


            var docChecklistID = dvm.DocumentCheckLists[0].Id;
            var doc = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == Customer.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == docChecklistID && o.IsActive && !o.IsDeleted).ToList();


            if ( doc.Count == 0)
            {

                ViewBag.Error = "Please Attach Rates Clearance Certificate For Application";
                return View(dvm);

                //ViewBag.Error = "Please Enter Amount For Conveyancer To Pay";
                //return View("");
            }

            var getcustomer = db.RCSApplicationStatus.Where(x => x.Id == id).Include(o => o.Customer).FirstOrDefault();
            Email SendMail = new Email();
            string attorneyemail = getcustomer.Customer.EmailAddress;
            string attorneyname = getcustomer.Customer.FullName;
            string emailbody = "Rates Clearance certificate Issued for application number: "+getcustomer.ApplicationReferenceNumber;
            SendMail.GenerateEmail(attorneyemail, "RCS- New Online Application Submission",
                          emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);




            RefundApplication refundApplication = new RefundApplication();
            var Keys = db.Status;
            refundApplication.StatusId = Keys.Where(x => x.Key == StatusKeys.RefundRequestAvailable).FirstOrDefault().Id;

            var depApprovals = db.RCSApplicationStatus.Include(x=>x.TransferInformation).Where(x => x.Id == id).FirstOrDefault();
                depApprovals.StatusId = Keys.Where(x => x.Key == StatusKeys.ViewRCCCertificate).FirstOrDefault().Id;
                db.SaveChanges();

         
            refundApplication.CustomerId = depApprovals.CustomerId;
            int limiter = 0;
            var AppSettings = db.AppSettings.ToList();
            AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSRefundSequence);
            var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSRefundSequenceLimit);
            var BatchCounter = query.Value;
            limiter = Convert.ToInt16(SeqLimit.Value);
            if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
            {
                var lastRef = query.Value;
                BatchCounter = lastRef.ToString();
                int nextSeq = Convert.ToInt16(query.Value) + 1;
                string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                query.Value = nextVal;
                db.Entry(query).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                int nextSeq = 1;
                string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                BatchCounter = nextVal;
                nextSeq = 2;
                nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                query.Value = nextVal;
                query.ModifiedDateTime = DateTime.Now.Date;
                db.Entry(query).State = EntityState.Modified;
                db.SaveChanges();

            }
            
                var refs = depApprovals.TransferInformation.RatesNumber;
                var RefNum = refs + "RCC" + DateTime.Now.ToString("ddMMyyyy") + BatchCounter;

          
            refundApplication.ApplicationReferenceNumber = RefNum;
            refundApplication.RCSApplicationStatusId = depApprovals.Id;
            db.RefundApplications.Add(refundApplication);
            db.SaveChanges();

            return RedirectToAction("RCCCertificateDashboard");
            }
        //sashen RC Certificate
        public ActionResult RCCertificateDashboard()
        {
            Initialise();
            var userID = Customer.Id;
            

            var Keys = db.Status;

            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))

            {

                var Message = TempData["RCCRedistributionTitle"];
                var Title = TempData["RCCRedistribution"];
                if (Message != null && Title != null)
                {
                    ViewBag.MessageTitle = TempData["RCCRedistributionTitle"].ToString();
                    ViewBag.Message = TempData["RCCRedistribution"].ToString();
                }
                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id).FirstOrDefault();
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault().Id;

                List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                if (User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                }
                else
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }
                   
                //rrq= rrq.Where(x=>x.Clerk.SystemUser.CCC == 8)
                var list = rrq.Select(x => x.RCSApplicationStatusId).ToList();


                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.AssessmentFeePaymentApproved)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();
                var ResponsibilityTypeID = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault();

                foreach (var item in rcsApps)
                {
                    var rrqName = db.RoundRobinQueues.Where(x => x.RCSApplicationStatusId == item.Id && x.ResponsibilityTypeId == ResponsibilityTypeID.Id).Include(x => x.Clerk).OrderByDescending(x => x.Id).FirstOrDefault();
                    item.Data = rrqName.Clerk.UserFullName;
                    item.RoundRobinQueueId = Convert.ToString(rrqName.Id);

                    var Flag = db.Flags.Where(x => x.Key == FlagKeys.IssueRCC).FirstOrDefault();
                        var OnTargetColor = Flag.OnTargetColor;
                        var OverdueColor = Flag.OverdueColor;
                        var RunningLateColor = Flag.RunningLateColor;
                        var TurnAroundDays = Flag.TurnAroundDays;
                        var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).FirstOrDefault();
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
                return View(rcsApps);
            }
            else
            {

                //    rcsApps = db.RCSApplicationStatus.Where(x => x.ClerkId == userID && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation || x.Status.Key == StatusKeys.PendingAssessmentFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status);
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault().Id;

                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                var list = rrq.Select(x => x.RCSApplicationStatusId).ToList();
                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.AssessmentFeePaymentApproved)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();

                foreach (var item in rcsApps)
                {

                    var Flag = db.Flags.Where(x => x.Key == FlagKeys.IssueRCC).FirstOrDefault();
                    var OnTargetColor = Flag.OnTargetColor;
                    var OverdueColor = Flag.OverdueColor;
                    var RunningLateColor = Flag.RunningLateColor;
                    var TurnAroundDays = Flag.TurnAroundDays;
                    var StartDate = rrq.Where(x => x.RCSApplicationStatusId == item.Id).FirstOrDefault();
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
                return View(rcsApps);
            }
        }
        //sashen RC Certificate
        [DecryptParameter]
        public ActionResult RCCertificateReAllocate(int? id)
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            var rcsApps = db.RCSApplicationStatus.Where(x => x.Id == id).FirstOrDefault();
            var rcscustomer = db.Customers.Where(x => x.Id == rcsApps.CustomerId).FirstOrDefault();
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == rcsApps.Id && x.StatusId == SubmittedId).FirstOrDefault();


            CaptureController c = new CaptureController();
            var SystUserId = c.RoundRobinRedistribution(false, false, true, false, false, false, false, false, rcsApps.Id, 0, rrq.Id, false, false);

            //var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
            //foreach (var item in rrqList)
            //{
            //    item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
            if (SystUserId == 0)
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData["RCCRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title; ;
                TempData["RCCRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;
               
            }
            else
            {
                var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData["RCCRedistributionTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body;
                TempData["RCCRedistribution"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body;

            }

            return RedirectToAction("RCCertificateDashboard");
        }
        [DecryptParameter]
        public ActionResult RCCertificate(int id)
        {
            Initialise();
            var userID = Customer.Id;

            var documentCheckLists = new List<DocumentCheckList>();
            var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCCCertificate);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

            var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCCCertificateUpload).FirstOrDefault();
            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();

            var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == id).Include(x => x.Customer).FirstOrDefault();
            Customer ConveyancerCustomerId = RCSApplication.Customer;

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                CustomerId = ConveyancerCustomerId.Id,
                ApplicationId = (int)application.Id,
                Application = application,
                RcsApplicationId = id,
                ReferenceTypeId = (int)referenceType.Id,
                ReferenceType = referenceType,
                ReferenceId = ConveyancerCustomerId.Id,
                IsUploadView = true,
                Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList()
            };
            dvm.DocumentCheckLists = documentCheckLists;



            foreach (var customerDocument in dvm.Documents)
            {
                //if (SystemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(SystemUserId, customerDocument, ViewCodeKeys.UpdateCustomerDocuments))
                //{
                //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                //    return RedirectToAction("Index", "Error");
                //}
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
                //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, referenceType.Id, ConveyancerCustomerId.Id, ConveyancerCustomerId.Id, application.Id, id))));
            }

            return View(dvm);
        }
        public String RefNum()
        {
            int limiter = 0;
            var AppSettings = db.AppSettings.ToList();
            AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSSequence);
            var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSSequenceLimit);
            var BatchCounter = query.Value;
            limiter = Convert.ToInt16(SeqLimit.Value);
            if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
            {
                var lastRef = query.Value;
                BatchCounter = lastRef.ToString();
                int nextSeq = Convert.ToInt16(query.Value) + 1;
                string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                query.Value = nextVal;
                db.Entry(query).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                int nextSeq = 1;
                string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                BatchCounter = nextVal;
                nextSeq = 2;
                nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                query.Value = nextVal;
                query.ModifiedDateTime = DateTime.Now.Date;
                db.Entry(query).State = EntityState.Modified;
                db.SaveChanges();

            }
            return query.Value;
        }
  
        public JsonResult CaptureDocChecker2(int id)
        {

            var documentCheckLists = new List<DocumentCheckList>();
            //var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

            //documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));


            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));



            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();




            var rcsApplication = db.RCSApplicationStatus.Include(x => x.Customer).Where(x => x.Id == id).FirstOrDefault();
            Customer ConveyancerCustomerId = rcsApplication.Customer;

            var AuthorityToActAttorney = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

            var Docs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList docCheckList = new DocumentCheckList();
            if (Docs != null)
            {

                docCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.DocumentTypeId == AuthorityToActAttorney.Id && c.IsActive && !c.IsDeleted);

            }



            var AuthoritytoActAttorney = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == docCheckList.Id && o.IsActive && !o.IsDeleted).ToList();




            var MunicipalStatement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);


            var MunciChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceType.Id);

            var MunicipalStatementDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList MunicipalStatementdocCheckList = new DocumentCheckList();
            if (MunicipalStatementDocs != null)
            {

                MunicipalStatementdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == MunciChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var MunicipalStatementDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == MunicipalStatementdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();


            //required
            var BankConfirmationLetter = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankConfirmationLetter);
            var BankConfirmationLetterChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankConfirmationLetter.Id && dcl.ReferenceTypeId == referenceType.Id);

            var BankConfirmationLetterDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList BankConfirmationLetterdocCheckList = new DocumentCheckList();
            if (BankConfirmationLetterDocs != null)
            {

                BankConfirmationLetterdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == BankConfirmationLetterChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var BankConfirmationLetterDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == BankConfirmationLetterdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();


            var DeedSearch = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DeedSearch);

            var DeedSearchLetterChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceType.Id);

            var DeedSearchLetterDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList DeedSearchLetterdocCheckList = new DocumentCheckList();
            if (DeedSearchLetterDocs != null)
            {

                DeedSearchLetterdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == DeedSearchLetterChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var DeedSearchLetterDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o =>  o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == DeedSearchLetterdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();

            var SellerID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);
            var SellerChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceType.Id);

            var SellerDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList SellerdocCheckList = new DocumentCheckList();
            if (SellerDocs != null)
            {

                SellerdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == SellerChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var SellerDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == SellerdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();


            var PurchaserID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);
            var PurchaserChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceType.Id);

            var PurchaserDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o =>o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList PurchaserdocCheckList = new DocumentCheckList();
            if (PurchaserDocs != null)
            {

                PurchaserdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == PurchaserChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var PurchaserDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == PurchaserdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();





            var isChecked = false;


            if (AuthoritytoActAttorney.Count < 1)
            {
                isChecked = true;
            }

            if (MunicipalStatementDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (BankConfirmationLetterDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (DeedSearchLetterDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (SellerDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (PurchaserDocsFinal.Count < 1)
            {
                isChecked = true;
            }

            string result = "";
            if (isChecked == false)
            {
                //if no docs are less than 1
                result = "false";
            }
            else
            {

                result = "true";
                //query.Value = nextVal;
                //query.ModifiedDateTime = DateTime.Now.Date;
                //db.Entry(query).State = EntityState.Modified;
                //db.SaveChanges();

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult StatusCheck(int Stat)
        {

            var documentCheckLists = new List<DocumentCheckList>();
            //var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

            //documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));


            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));



            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();


            var statusId = db.Status.FirstOrDefault(o => o.Key == StatusKeys.AwaitingAssessmentPayment).Id;


            var rcsApplication = db.RCSApplicationStatus.Include(x => x.Customer).FirstOrDefault(x => x.Id == Stat && x.StatusId == statusId);
            var isChecked = false;
            if (rcsApplication != null)
            {
                isChecked = true;
            }





            string result = "";
            if (isChecked == false)
            {
                //if no docs are less than 1
                result = "false";
            }
            else
            {

                result = "true";
                //query.Value = nextVal;
                //query.ModifiedDateTime = DateTime.Now.Date;
                //db.Entry(query).State = EntityState.Modified;
                //db.SaveChanges();

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public string CaptureDocChecker(int id)
        {

            var documentCheckLists = new List<DocumentCheckList>();
            //var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

            //documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

         
            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
     

          
            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();



           
            var rcsApplication = db.RCSApplicationStatus.Include(x => x.Customer).Where(x => x.Id == id).FirstOrDefault();
            Customer ConveyancerCustomerId = rcsApplication.Customer;

            var AuthorityToActAttorney = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

            var Docs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o =>  o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList docCheckList = new DocumentCheckList();
            if (Docs != null)
            {

                docCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c=>c.DocumentTypeId== AuthorityToActAttorney.Id && c.IsActive && !c.IsDeleted);

            }



            var AuthoritytoActAttorney = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == docCheckList.Id && o.IsActive && !o.IsDeleted).ToList();

        


            var MunicipalStatement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

  
           var MunciChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceType.Id);

            var MunicipalStatementDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList MunicipalStatementdocCheckList = new DocumentCheckList();
            if (MunicipalStatementDocs != null)
            {

                MunicipalStatementdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == MunciChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var MunicipalStatementDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == MunicipalStatementdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();


            //required
            var BankConfirmationLetter = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankConfirmationLetter);
            var BankConfirmationLetterChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankConfirmationLetter.Id && dcl.ReferenceTypeId == referenceType.Id);

            var BankConfirmationLetterDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList BankConfirmationLetterdocCheckList = new DocumentCheckList();
            if (BankConfirmationLetterDocs != null)
            {

                BankConfirmationLetterdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == BankConfirmationLetterChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var BankConfirmationLetterDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == BankConfirmationLetterdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();


            var DeedSearch = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DeedSearch);

            var DeedSearchLetterChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceType.Id);

            var DeedSearchLetterDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList DeedSearchLetterdocCheckList = new DocumentCheckList();
            if (DeedSearchLetterDocs != null)
            {

                DeedSearchLetterdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == DeedSearchLetterChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var DeedSearchLetterDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o =>  o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == DeedSearchLetterdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();

            var SellerID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);
            var SellerChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceType.Id);

            var SellerDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList SellerdocCheckList = new DocumentCheckList();
            if (SellerDocs != null)
            {

                SellerdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == SellerChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var SellerDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == SellerdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();


            var PurchaserID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);
            var PurchaserChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceType.Id);

            var PurchaserDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList PurchaserdocCheckList = new DocumentCheckList();
            if (PurchaserDocs != null)
            {

                PurchaserdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == PurchaserChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var PurchaserDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == PurchaserdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();





            var isChecked = false;


            if(AuthoritytoActAttorney.Count <1)
            {
                isChecked = true;
            }

            if (MunicipalStatementDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (BankConfirmationLetterDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (DeedSearchLetterDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (SellerDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (PurchaserDocsFinal.Count < 1)
            {
                isChecked = true;
            }

            string result = "";
            if (isChecked == false)
            {
                //if no docs are less than 1
                result = "false";
            }
            else
            {

                result = "true";
                //query.Value = nextVal;
                //query.ModifiedDateTime = DateTime.Now.Date;
                //db.Entry(query).State = EntityState.Modified;
                //db.SaveChanges();

            }
            return result;
        }

        public JsonResult RefundDocChecker(int id)
        {

            var documentCheckLists = new List<DocumentCheckList>();
            //var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

            //documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));


            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSRefund));



            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();




            var rcsApplication = db.RefundApplications.Include(x => x.Customer).Include(r => r.RCSApplicationStatus).FirstOrDefault(x => x.Id == id);
            Customer ConveyancerCustomerId = rcsApplication.Customer;



            var bankletter = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundConveyancerBankingDetails);

            var bankletterDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();


            var bankletterChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == bankletter.Id && dcl.ReferenceTypeId == referenceType.Id);

            DocumentCheckList bankletterdocCheckList = new DocumentCheckList();
            if (bankletterDocs != null)
            {

                bankletterdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == bankletterChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var bankletterdDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.DocumentCheckListId == bankletterdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();

            var DeedSearch = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundDeedSearch);


            var DeedSearchLetterChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceType.Id);

            var DeedSearchLetterDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList DeedSearchLetterdocCheckList = new DocumentCheckList();
            if (DeedSearchLetterDocs != null)
            {

                DeedSearchLetterdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == DeedSearchLetterChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var DeedSearchLetterDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.DocumentCheckListId == DeedSearchLetterdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();



            var MunicipalAccStatement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundMunicipalStatement);


            var MunicipalAccStatementLetterChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalAccStatement.Id && dcl.ReferenceTypeId == referenceType.Id);

            var MunicipalAccStatementLetterDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList MunicipalAccStatementdocCheckList = new DocumentCheckList();
            if (MunicipalAccStatementLetterDocs != null)
            {

                MunicipalAccStatementdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == MunicipalAccStatementLetterChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var MunicipalAccStatementLetterDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.DocumentCheckListId == MunicipalAccStatementdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalAccStatement.Id && dcl.ReferenceTypeId == referenceType.Id));

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceType.Id));

 

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == bankletter.Id && dcl.ReferenceTypeId == referenceType.Id));


            var isChecked = false;


     

            if (DeedSearchLetterDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (bankletterdDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (MunicipalAccStatementLetterDocsFinal.Count < 1)
            {
                isChecked = true;
            }
          

            string result = "";
            if (isChecked == false)
            {
                //if no docs are less than 1
                result = "false";
            }
            else
            {

                result = "true";
                //query.Value = nextVal;
                //query.ModifiedDateTime = DateTime.Now.Date;
                //db.Entry(query).State = EntityState.Modified;
                //db.SaveChanges();

            }
            return Json(result, JsonRequestBehavior.AllowGet);
         
        }

        public bool RefundDocChecker2(int id)
        {

            var documentCheckLists = new List<DocumentCheckList>();
            //var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

            //documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));


            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSRefund));



            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();




            var rcsApplication = db.RefundApplications.Include(x => x.Customer).Include(r => r.RCSApplicationStatus).FirstOrDefault(x => x.Id == id);
            Customer ConveyancerCustomerId = rcsApplication.Customer;



            var bankletter = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundConveyancerBankingDetails);

            var bankletterDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();


            var bankletterChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == bankletter.Id && dcl.ReferenceTypeId == referenceType.Id);

            DocumentCheckList bankletterdocCheckList = new DocumentCheckList();
            if (bankletterDocs != null)
            {

                bankletterdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == bankletterChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var bankletterdDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.DocumentCheckListId == bankletterdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();

            var DeedSearch = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundDeedSearch);


            var DeedSearchLetterChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceType.Id);

            var DeedSearchLetterDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList DeedSearchLetterdocCheckList = new DocumentCheckList();
            if (DeedSearchLetterDocs != null)
            {

                DeedSearchLetterdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == DeedSearchLetterChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var DeedSearchLetterDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.DocumentCheckListId == DeedSearchLetterdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();



            var MunicipalAccStatement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundMunicipalStatement);


            var MunicipalAccStatementLetterChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalAccStatement.Id && dcl.ReferenceTypeId == referenceType.Id);

            var MunicipalAccStatementLetterDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList MunicipalAccStatementdocCheckList = new DocumentCheckList();
            if (MunicipalAccStatementLetterDocs != null)
            {

                MunicipalAccStatementdocCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == MunicipalAccStatementLetterChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var MunicipalAccStatementLetterDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RefundApplicationId == id && o.DocumentCheckListId == MunicipalAccStatementdocCheckList.Id && o.IsActive && !o.IsDeleted).ToList();

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalAccStatement.Id && dcl.ReferenceTypeId == referenceType.Id));

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceType.Id));



            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == bankletter.Id && dcl.ReferenceTypeId == referenceType.Id));


            var isChecked = false;




            if (DeedSearchLetterDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (bankletterdDocsFinal.Count < 1)
            {
                isChecked = true;
            }
            if (MunicipalAccStatementLetterDocsFinal.Count < 1)
            {
                isChecked = true;
            }


            bool result = false;
            if (isChecked == false)
            {
                //if no docs are less than 1
                result = false;
            }
            else
            {

                result = true;
                //query.Value = nextVal;
                //query.ModifiedDateTime = DateTime.Now.Date;
                //db.Entry(query).State = EntityState.Modified;
                //db.SaveChanges();

            }
            return result;

        }
        //sashen RC Certificate
        [DecryptParameter]
        [HttpPost]
        public ActionResult RCCertificate(int id, int RcsApplicationId)
        {
            Initialise();
            var userID = Customer.Id;
          
            var documentCheckLists = new List<DocumentCheckList>();
            var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCCCertificate);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

            var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCCCertificateUpload).FirstOrDefault();
            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();

            var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == id).Include(x => x.Customer).FirstOrDefault();
            Customer ConveyancerCustomerId = RCSApplication.Customer;


            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            var Docs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
           DocumentCheckList docCheckList = new DocumentCheckList();
            if (Docs != null)
            {
                //if (Docs.File != null)
                //    Docs.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", Docs.FileId));

                docCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == Docs.DocumentCheckListId && c.IsActive && !c.IsDeleted);
            
            }


            var doc = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == ConveyancerCustomerId.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.DocumentCheckListId == docCheckList.Id && o.IsActive && !o.IsDeleted).ToList();
            var isChecked = false;


       

            if (doc.Count == 0)
            {
                isChecked = true;
                ViewBag.Error = "Please Attach Rates Clearance Certificate For Application";
                //return View(dvm);

                //ViewBag.Error = "Please Enter Amount For Conveyancer To Pay";
                //return View("");
            }

            if (!isChecked)
            {


                //RefundApplication refundApplication = new RefundApplication();
                var Keys = db.Status;
                //refundApplication.StatusId = Keys.Where(x => x.Key == StatusKeys.RefundRequestAvailable).FirstOrDefault().Id;

                var depApprovals = db.RCSApplicationStatus.Include(x => x.TransferInformation).Where(x => x.Id == id).FirstOrDefault();
                depApprovals.StatusId = Keys.Where(x => x.Key == StatusKeys.ViewRCCCertificate).FirstOrDefault().Id;
                db.SaveChanges();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RCCIssued).Description.ToString();

                var Result = ActivityTrackerAudit(depApprovals.Id, ActivityTrackerMessage, Customer.Id);

                var getcustomer = db.RCSApplicationStatus.Where(x => x.Id == id).Include(o => o.Customer).FirstOrDefault();
                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.RCCIssued).FirstOrDefault();
                var systemusermobilenum = db.SystemUsers.Where(x => x.Id == getcustomer.Customer.SystemUserId).FirstOrDefault().MobileNumber;
                Email SendMail = new Email();
                string attorneyemail = getcustomer.Customer.EmailAddress;
                string attorneyname = getcustomer.Customer.FullName;
                string emailbody = getemailbody.Description + getcustomer.ApplicationReferenceNumber;
                //string emailbody = "Rates clearance certificate issued for application number: " + getcustomer.ApplicationReferenceNumber;
                var ActivityTrackerEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RCCEmail).Description.ToString()+" "+ emailbody;

                SendMail.GenerateEmailSMS(ActivityTrackerEmail, systemusermobilenum, depApprovals.Id, Customer.Id, emailbody, attorneyemail, "RCS- New Online Application Submission",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);


                //refundApplication.CustomerId = depApprovals.CustomerId;
                //int limiter = 0;
                //var AppSettings = db.AppSettings.ToList();
                //AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSRefundSequence);
                //var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSRefundSequenceLimit);
                //var BatchCounter = query.Value;
                //limiter = Convert.ToInt16(SeqLimit.Value);
                //if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                //{
                //    var lastRef = query.Value;
                //    BatchCounter = lastRef.ToString();
                //    int nextSeq = Convert.ToInt16(query.Value) + 1;
                //    string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                //    query.Value = nextVal;
                //    db.Entry(query).State = EntityState.Modified;
                //    db.SaveChanges();
                //}
                //else
                //{
                //    int nextSeq = 1;
                //    string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                //    BatchCounter = nextVal;
                //    nextSeq = 2;
                //    nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                //    query.Value = nextVal;
                //    query.ModifiedDateTime = DateTime.Now.Date;
                //    db.Entry(query).State = EntityState.Modified;
                //    db.SaveChanges();

                //}




                //refundApplication.ApplicationReferenceNumber = RefNum;
                //refundApplication.RCSApplicationStatusId = depApprovals.Id;
                //db.RefundApplications.Add(refundApplication);
                //db.SaveChanges();
                //RefundApplication refundApplication = new RefundApplication();
                //refundApplication.StatusId = Keys.Where(x => x.Key == StatusKeys.RefundRequestAvailable).FirstOrDefault().Id;
                //refundApplication.CustomerId = depApprovals.CustomerId;
                //refundApplication.CustomerId = depApprovals.CustomerId;
                //int limiter = 0;
                //var AppSettings = db.AppSettings.ToList();
                //AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSRefundSequence);
                //var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSRefundSequenceLimit);
                //var BatchCounter = query.Value;
                //limiter = Convert.ToInt16(SeqLimit.Value);
                //if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                //{
                //    var lastRef = query.Value;
                //    BatchCounter = lastRef.ToString();
                //    int nextSeq = Convert.ToInt16(query.Value) + 1;
                //    string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                //    query.Value = nextVal;
                //    db.Entry(query).State = EntityState.Modified;
                //    db.SaveChanges();
                //}
                //else
                //{
                //    int nextSeq = 1;
                //    string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                //    BatchCounter = nextVal;
                //    nextSeq = 2;
                //    nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                //    query.Value = nextVal;
                //    query.ModifiedDateTime = DateTime.Now.Date;
                //    db.Entry(query).State = EntityState.Modified;
                //    db.SaveChanges();

                //}
                //var refs = depApprovals.TransferInformation.RatesNumber;
                //var RefNum = refs + "RCC" + DateTime.Now.ToString("ddMMyyyy") + BatchCounter;
                //refundApplication.ApplicationReferenceNumber = "125663";
                //var refs = depApprovals.TransferInformation.RatesNumber;
                //var RefNum = refs + "RCC" + DateTime.Now.ToString("ddMMyyyy") + BatchCounter;
                //refundApplication.RCSApplicationStatusId = depApprovals.Id;
                //db.RefundApplications.Add(refundApplication);
                //db.SaveChanges();
                //var refn = RefNum();
                //RefundApplication rf = new RefundApplication
                //{
                //    RCSApplicationStatusId = depApprovals.Id,
                //    StatusId = Keys.Where(x => x.Key == StatusKeys.RefundRequestAvailable).FirstOrDefault().Id,
                //    CustomerId = depApprovals.CustomerId,
                //    ApplicationReferenceNumber = "2100000902RCC230920200003",
                //    IsActive = true,
                //    IsDeleted = false,
                //    IsLocked = false,
                //    CreatedDateTime = DateTime.Now,
                //    ModifiedDateTime = DateTime.Now

                //};
                //db.RefundApplications.Add(rf);
                //db.SaveChanges();
                return RedirectToAction("RCCertificateDashboard");

            }
            else
            {

                var dvm = new DocumentsViewModel
                {
                    CustomerId = ConveyancerCustomerId.Id,
                    ApplicationId = (int)application.Id,
                    Application = application,
                    RcsApplicationId = id,
                    ReferenceTypeId = (int)referenceType.Id,
                    ReferenceType = referenceType,
                    ReferenceId = ConveyancerCustomerId.Id,
                    IsUploadView = true,
                    Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == Customer.Id && o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList()
                };
                dvm.DocumentCheckLists = documentCheckLists;



                foreach (var customerDocument in dvm.Documents)
                {
                    //if (SystemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(SystemUserId, customerDocument, ViewCodeKeys.UpdateCustomerDocuments))
                    //{
                    //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                    //    return RedirectToAction("Index", "Error");
                    //}
                    if (customerDocument.File != null)
                        customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                    var docCheckList1 =
                         db.DocumentCheckLists.Include(d => d.DocumentType)
                             .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                    customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList1.DocumentType.Name,
                        customerDocument.DocumentName);
                    //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
                    //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
                    customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
                }

                foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
                {

                    documentCheckList.DataList = new List<string>();
                    documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, referenceType.Id, ConveyancerCustomerId.Id, ConveyancerCustomerId.Id, application.Id, id))));
                }

                return View(dvm);
            }






        }

        #endregion

        #region Department Approval Index
        public ActionResult Index()
        {
            Initialise();
            var userID = Customer.Id;

            object departmentsApprovals = null;
            var DepartmentName = "";

                if (User.IsInRole("Legal"))
                {
                    departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSDepartment.Key == RCSDepartmentTypeKeys.LegalSection && x.Status.Key == StatusKeys.Submitted || x.Status.Key == StatusKeys.ReAllocate || x.Status.Key == StatusKeys.ReAssignDep && x.AssignedToCustomerId == userID).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    DepartmentName = "Legal";
                }

                else if (User.IsInRole("Credit Control"))
                {
                    departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSDepartment.Key == RCSDepartmentTypeKeys.CreditControlSection && x.Status.Key == StatusKeys.Submitted || x.Status.Key == StatusKeys.ReAllocate || x.Status.Key == StatusKeys.ReAssignDep && x.AssignedToCustomerId == userID).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    DepartmentName = "Credit Control";
                }

                else if (User.IsInRole("Billing"))
                {
                    departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSDepartment.Key == RCSDepartmentTypeKeys.BillingSection && x.Status.Key == StatusKeys.Submitted || x.Status.Key == StatusKeys.ReAllocate || x.Status.Key == StatusKeys.ReAssignDep && x.AssignedToCustomerId == userID).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    DepartmentName = "Billing";
                }

                else if (User.IsInRole("Endowment"))
                {
                    departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSDepartment.Key == RCSDepartmentTypeKeys.EndowmentSection && x.Status.Key == StatusKeys.Submitted || x.Status.Key == StatusKeys.ReAllocate || x.Status.Key == StatusKeys.ReAssignDep && x.AssignedToCustomerId == userID).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    DepartmentName = "Endowment";
                }
            else if (User.IsInRole("Sundry Account"))
            {
                departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSDepartment.Key == RCSDepartmentTypeKeys.SundryAccountSection && x.Status.Key == StatusKeys.Submitted || x.Status.Key == StatusKeys.ReAllocate || x.Status.Key == StatusKeys.ReAssignDep && x.AssignedToCustomerId == userID).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                DepartmentName = "Sundry Account";
            }
            else if (User.IsInRole("Accounts Management"))
                {
                    departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSDepartment.Key == RCSDepartmentTypeKeys.SundryAccountSection && x.Status.Key == StatusKeys.Submitted || x.Status.Key == StatusKeys.ReAllocate || x.Status.Key == StatusKeys.ReAssignDep && x.AssignedToCustomerId == userID).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    DepartmentName = "Accounts Management";
                }

                else if (User.IsInRole("Clerks"))
                {
                    departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatus.ApplicationReferenceNumber != null).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    DepartmentName = "Back Office";
                }
                ViewBag.DepartmentName = DepartmentName;
                return View(departmentsApprovals);
            }
        #endregion

        #region Load Details
            public ActionResult LoadDetails()
            {
                List<DepartmentsApproval> departmentsApproval = new List<DepartmentsApproval>();

                if (User.IsInRole("Clerks"))
                {
                    int id = Convert.ToInt32(TempData["id"]);
                    departmentsApproval = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == id).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status).ToList();
                }
                else
                {
                    int id = Convert.ToInt32(TempData["id"]);
                    departmentsApproval = db.DepartmentsApprovals.Where(x => x.Id == id).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status).ToList();
                }

                ViewBag.ApprovalStatus = new SelectList(db.RCSTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Id", "Name");

                var json = Json(new { data = departmentsApproval }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = Int32.MaxValue;

                return json;
            }
        #endregion

        #region Find Approvals Cases By User/Role
        public ActionResult ApprovalsLoad()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var userID = Customer.Id;

                    object departmentsApprovals = null;
                    if (User.IsInRole("Legal"))
                    {
                        departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSDepartment.Key == RCSDepartmentTypeKeys.LegalSection && x.Status.Key != StatusKeys.BackOffice && x.AssignedToCustomerId == userID).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    }

                    else if (User.IsInRole("Credit Control"))
                    {
                        departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSDepartment.Key == RCSDepartmentTypeKeys.CreditControlSection && x.Status.Key != StatusKeys.BackOffice && x.AssignedToCustomerId == userID).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    }

                    else if (User.IsInRole("Billing"))
                    {
                        departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSDepartment.Key == RCSDepartmentTypeKeys.BillingSection && x.Status.Key != StatusKeys.BackOffice && x.AssignedToCustomerId == userID).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    }

                    else if (User.IsInRole("Endowment"))
                    {
                        departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSDepartment.Key == RCSDepartmentTypeKeys.EndowmentSection && x.Status.Key != StatusKeys.BackOffice && x.AssignedToCustomerId == userID).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    }

                    else if (User.IsInRole("Accounts Management"))
                    {
                        departmentsApprovals = db.DepartmentsApprovals.Where(x => x.RCSDepartment.Key == RCSDepartmentTypeKeys.SundryAccountSection && x.Status.Key != StatusKeys.BackOffice && x.AssignedToCustomerId == userID).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    }

                    else if (User.IsInRole("Clerks"))
                    {
                        departmentsApprovals = db.DepartmentsApprovals.Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                    }


                    var json = Json(new { data = departmentsApprovals }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }

        #endregion

        #region Details
        [DecryptParameter]
        public ActionResult Details(int? refNo, int? id)
            {
                List<RCSDepartmentType> DepartmentList = new List<RCSDepartmentType>();
                DepartmentList = db.RCSDepartmentTypes.ToList();
                List<DepartmentsApproval> allDepApprovalsInApplication = new List<DepartmentsApproval>();
                List<RCSDepartmentType> ddDeps = new List<RCSDepartmentType>();

                ddDeps = DepartmentList;
                var departmentsApproval = (IEnumerable<DepartmentsApproval>)null;

                TempData["id"] = id;

                if (User.IsInRole("Legal"))
                {                    
                    departmentsApproval = db.DepartmentsApprovals.Where(x => x.Id == id && x.RCSDepartment.Key == RCSDepartmentTypeKeys.LegalSection).Include(d => d.RCSApplicationStatus).Include(d => d.AssignedToCustomer).Include(d => d.RCSDepartment).Include(d => d.Status).ToList();
                }

                else if (User.IsInRole("Credit Control"))
                {
                    departmentsApproval = db.DepartmentsApprovals.Where(x => x.Id == id && x.RCSDepartment.Key == RCSDepartmentTypeKeys.CreditControlSection).Include(d => d.RCSApplicationStatus).Include(d => d.AssignedToCustomer).Include(d => d.RCSDepartment).Include(d => d.Status).ToList();
                }

                else if (User.IsInRole("Billing"))
                {
                    departmentsApproval = db.DepartmentsApprovals.Where(x => x.Id == id && x.RCSDepartment.Key == RCSDepartmentTypeKeys.BillingSection).Include(d => d.RCSApplicationStatus).Include(d => d.AssignedToCustomer).Include(d => d.RCSDepartment).Include(d => d.Status).ToList();
                }

                else if (User.IsInRole("Endowment"))
                {
                    departmentsApproval = db.DepartmentsApprovals.Where(x => x.Id == id && x.RCSDepartment.Key == RCSDepartmentTypeKeys.EndowmentSection).Include(d => d.RCSApplicationStatus).Include(d => d.AssignedToCustomer).Include(d => d.RCSDepartment).Include(d => d.Status).ToList();
                }

                else if (User.IsInRole("Accounts Management"))
                {
                    departmentsApproval = db.DepartmentsApprovals.Where(x => x.Id == id && x.RCSDepartment.Key == RCSDepartmentTypeKeys.SundryAccountSection).Include(d => d.RCSApplicationStatus).Include(d => d.AssignedToCustomer).Include(d => d.RCSDepartment).Include(d => d.Status).ToList();
                }

                else if (User.IsInRole("Clerks"))
                {
                    List<DepartmentsApproval> depApproval = new List<DepartmentsApproval>();

                    TempData["id"] = refNo;
                    departmentsApproval = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == refNo).Include(d => d.RCSApplicationStatus).Include(d => d.AssignedToCustomer).Include(d => d.RCSDepartment).Include(d => d.Status).ToList();
                }


            if (departmentsApproval.Count() > 0)
                {
                    var RCSApplicationStatusId = departmentsApproval.FirstOrDefault().RCSApplicationStatusId;
                    allDepApprovalsInApplication = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == RCSApplicationStatusId).Include(d => d.RCSApplicationStatus).ToList();
                }

                //Dont allow to to re-allocate to a department if all fails where selected.
                if (allDepApprovalsInApplication.Count == DepartmentList.Count)
                {
                    ddDeps = null;
                }
                //Dont allow to to re-allocate to same department selected && not another department that didnt fail on a check.
                else
                {
                    foreach (var item in DepartmentList.ToList())
                    {
                        foreach (var item2 in allDepApprovalsInApplication)
                        {
                            if (item.Id == item2.DepartmentId)
                            {
                                ddDeps.Remove(item);
                                break;
                            }
                        }
                    }
                }

                if (!User.IsInRole("Clerks"))
                {
                    ViewBag.ApprovalStatus = new SelectList(db.RCSActionTypes.Where(x => x.Key != RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                }
                else
                {
                    ViewBag.ApprovalStatus = new SelectList(db.RCSActionTypes.Where(x=>x.Key != RCSActionTypeKeys.Disprove && x.Key != RCSActionTypeKeys.ReAllocate && x.Key != RCSActionTypeKeys.ReAssignDep && x.Key != RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
                }


                if (ddDeps != null && ddDeps.Count > 0)
                {
                    ViewBag.ReAssignDep = new SelectList(ddDeps, "Key", "Name");
                }

                if (refNo == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                if (departmentsApproval == null)
                {
                    return HttpNotFound();
                }

                if (User.IsInRole("Clerks"))
                {
                //Check if the RCS Application has all the approvals so that BO can approve otherwise hide the submit btn
                    var statusKey = db.Status;
                    var BOAllAppsApproved = allDepApprovalsInApplication.Where(o => o.StatusId == statusKey.Where(x => x.Key == StatusKeys.BackOffice).FirstOrDefault().Id);
                    var BOAllAppsDisproved = allDepApprovalsInApplication.Where(o => o.StatusId == statusKey.Where(x => x.Key == StatusKeys.Disprove).FirstOrDefault().Id);

                    if (BOAllAppsApproved.Count() == allDepApprovalsInApplication.Count())
                    {
                        ViewBag.BOAllAppsApproved = true;
                        ViewBag.ApprovalStatus = null;
                    }
                    else if (BOAllAppsDisproved.Count() == allDepApprovalsInApplication.Count())
                    {
                        ViewBag.BOAllAppsApproved = null;
                    }
                    else
                    {
                        ViewBag.BO = true;
                    }
                }

                return View(departmentsApproval);
            }

        [DecryptParameter]
            [HttpPost]
            public ActionResult Details(Int32 id, string ApprovalStatusddl, string ReAssignddl, string Comment)
            {
                var applicationUserRoles = db.Roles.ToList();
                var DepartmentApprovals = db.DepartmentsApprovals.Where(x=>x.Id == id && x.IsDeleted == false).ToList();
                DepartmentApprovals.FirstOrDefault().Comment = Comment;
                db.SaveChanges();   
                var ReAllocateddl = 0;
                var custId = 0;
                var AccountsManagement = applicationUserRoles.Where(x => x.Name == "Accounts Management").FirstOrDefault().Id;
                var Legal = applicationUserRoles.Where(x => x.Name == "Legal").FirstOrDefault().Id;
                var CreditControl = applicationUserRoles.Where(x => x.Name == "Credit Control").FirstOrDefault().Id;
                var Billing = applicationUserRoles.Where(x => x.Name == "Billing").FirstOrDefault().Id;
                var Endowment = applicationUserRoles.Where(x => x.Name == "Endowment").FirstOrDefault().Id;


                List<SystemIdentityUser> users = null;

                if (RCSActionTypeKeys.ReAssignDep == ApprovalStatusddl)
                {
                    var RCSDepartmentTypes = db.RCSDepartmentTypes.Where(x => x.Key == ReAssignddl).FirstOrDefault();
                    var reAssDep = applicationUserRoles.Where(x => x.Name == RCSDepartmentTypes.Name).FirstOrDefault().Id;

                    users = GetUsersInRole(reAssDep).Where(x => x.RoundRobinIsActive != false).ToList();
                    ReAllocateddl = CaptureController.AssigedToRR(users, DepartmentApprovals);
                }
               
                else if(RCSActionTypeKeys.Approved == ApprovalStatusddl || RCSActionTypeKeys.ReAllocate == ApprovalStatusddl)
                {
                    if (User.IsInRole("Legal"))
                    {
                        users = GetUsersInRole(Legal).Where(x => x.RoundRobinIsActive != false).ToList();
                        ReAllocateddl = CaptureController.AssigedToRR(users, DepartmentApprovals);
                    }

                    else if (User.IsInRole("Credit Control"))
                    {
                        users = GetUsersInRole(CreditControl).Where(x => x.RoundRobinIsActive != false).ToList();
                        ReAllocateddl = CaptureController.AssigedToRR(users, DepartmentApprovals);
                    }

                    else if (User.IsInRole("Billing"))
                    {
                        users = GetUsersInRole(Billing).Where(x => x.RoundRobinIsActive != false).ToList();
                        ReAllocateddl = CaptureController.AssigedToRR(users, DepartmentApprovals);
                    }

                    else if (User.IsInRole("Endowment"))
                    {
                        users = GetUsersInRole(Endowment).Where(x => x.RoundRobinIsActive != false).ToList();
                        ReAllocateddl = CaptureController.AssigedToRR(users, DepartmentApprovals);
                    }

                    else if (User.IsInRole("Accounts Management"))
                    {
                        users = GetUsersInRole(AccountsManagement).Where(x => x.RoundRobinIsActive != false).ToList();
                        ReAllocateddl = CaptureController.AssigedToRR(users, DepartmentApprovals);
                    }
                }

                if (ReAllocateddl != 0)
                {
                    custId = db.Customers.Where(x => x.SystemUserId == ReAllocateddl && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                }

                CaptureController c = new CaptureController();
                c.ApproveOrReject(id, ApprovalStatusddl, ReAssignddl, custId);

                return RedirectToAction("Index","DepartmentsApprovals");
            }
        #endregion

        #region Create
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber");
            return View();
        }

        // POST: DepartmentsApprovals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,RCSApplicationStatusId,Department,FailureReason,ApprovalStatus,Comment,AssignedTo,CapturedDate,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] DepartmentsApproval departmentsApproval)
        {
            if (ModelState.IsValid)
            {
                db.DepartmentsApprovals.Add(departmentsApproval);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsApproval.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsApproval.ModifiedBySystemUserId);
            ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber", departmentsApproval.RCSApplicationStatusId);
            return View(departmentsApproval);
        }
        #endregion

        #region Edit

        public ActionResult Edit(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DepartmentsApproval departmentsApproval = db.DepartmentsApprovals.Find(id);
                if (departmentsApproval == null)
                {
                    return HttpNotFound();
                }
                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsApproval.CreatedBySystemUserId);
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsApproval.ModifiedBySystemUserId);
                ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber", departmentsApproval.RCSApplicationStatusId);
                return View(departmentsApproval);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit([Bind(Include = "Id,RCSApplicationStatusId,Department,FailureReason,ApprovalStatus,Comment,AssignedTo,CapturedDate,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] DepartmentsApproval departmentsApproval)
            {
                if (ModelState.IsValid)
                {
                    db.Entry(departmentsApproval).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsApproval.CreatedBySystemUserId);
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsApproval.ModifiedBySystemUserId);
                ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber", departmentsApproval.RCSApplicationStatusId);
                return View(departmentsApproval);
            }
        #endregion

        #region GetUsersInRole
        public IEnumerable<SystemIdentityUser> GetUsersInRole(string roleId)
        {
            return UserManager.Users.Include(o=>o.SystemUser).Where(o => o.Roles.Any(s => s.RoleId == roleId)).ToList();
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
