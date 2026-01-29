using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using C8.eServices.Mvc.Controllers;

namespace C8.eServices.Mvc.Helpers
{
    public static class WorkAllocationHumanHelper
    {
        public static int MinimunWorkAllocatedUser(eServicesDbContext core, int PreferredComplexAreaId, string[] Roles)
        {
            try
            {
                var rrs = core.UserWorkAllocations.Include(r => r.SystemUser).Where(r => Roles.Contains(r.Roles) && r.RRActive && r.IsActive && !r.IsDeleted && r.PreferredComplexAreaId == PreferredComplexAreaId).ToList();
                var UserId = 0;
                var min = 0;
                foreach (var Q in rrs)
                {
                    var index = rrs.FindIndex(r => r.SystemUserId == Q.SystemUserId);
                    var Clerk = core.Customers.FirstOrDefault(d => d.SystemUserId == Q.SystemUser.Id);
                    var work = core.RoundRobinQueues.Include(r => r.Status).Where(d => d.ClerkId == Clerk.Id && d.Status.Key == StatusKeys.Submitted).ToList().Count();
                    if (index == 0 )
                    {
                        min = work;
                        UserId = Q.SystemUser.Id;
                    }
                    else if (work < min)
                    {
                        min = work;
                        UserId = Q.SystemUser.Id;
                    }
                }
                if (UserId == 0) return UserId;
                var ClerkId = core.Customers.FirstOrDefault(r => r.SystemUserId == UserId).Id;
                return ClerkId;
            }
            catch (Exception)
            {
                return 0;
                //throw;
            }
        }

        public static void FinishAllPreviousWork(int ApplicationId)
        {
            try
            {
                var core = new eServicesDbContext();
                var workundone = core.RoundRobinQueues.Where(a => a.HumanSettlementApplicationId == ApplicationId && a.EndTaskDateTime == null).ToList();
                if (workundone.Count > 0)
                    foreach (var work in workundone)
                    {
                        work.EndTaskDateTime = DateTime.Now;
                        work.StatusId = core.Status.FirstOrDefault(d => d.Key == StatusKeys.Archived).Id;
                        core.Entry(work).State = EntityState.Modified;
                        core.SaveChanges();
                    }
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }
        }

        public static void BackOfficeNotification(int RCSAppID, int CustomerID, string QueueName)
        {
            var core = new eServicesDbContext();
            try
            {
                var getemailbody = core.EmailContentTypes.Where(x => x.Key == EmailContentKeys.BONewCaseLoaded).FirstOrDefault();
                Email SendMail = new Email();
                var PLMApplication = core.HumanSettlementApplications.Where(x => x.Id == RCSAppID).FirstOrDefault();
                var BackOfficeClerk = core.Customers.Where(x => x.Id == CustomerID).FirstOrDefault();
                string attorneyemail =  BackOfficeClerk.EmailAddress;
                string attorneyname = string.Format("{0} {1}", BackOfficeClerk.FirstName, BackOfficeClerk.LastName);
                string emailbody = getemailbody.Description;
                emailbody = emailbody.Replace("{0}", QueueName);
                emailbody = emailbody.Replace("{1}", PLMApplication.ApplicationReferenceNumber);
                var systemusermobilenum = BackOfficeClerk.CellPhoneNumber;
                var ActivityTrackerMessageEmail = string.Format("{0} {1}", core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description, emailbody);
                SendMail.GenerateEmailSMS2(ActivityTrackerMessageEmail, systemusermobilenum, PLMApplication.Id, CustomerID, emailbody, attorneyemail, "PLM-Online Application", emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }
        }
        public static void RoundRobinMarkJobAsFinished(eServicesDbContext core, int ApplicationId,  int ResposibilityId, int ClerkId, bool? isFromAdmin = false)
        {
            try
            {
                var findItem = new RoundRobinQueue();
                if (isFromAdmin.Value == true)
                {
                    findItem = core.RoundRobinQueues.OrderByDescending(x => x.Id).FirstOrDefault(x => x.HumanSettlementApplicationId == ApplicationId && x.ResponsibilityTypeId == ResposibilityId && x.EndTaskDateTime == null);
                }
                else 
                {
                    findItem = core.RoundRobinQueues.OrderByDescending(x => x.Id).FirstOrDefault(x => x.HumanSettlementApplicationId == ApplicationId && x.ResponsibilityTypeId == ResposibilityId && x.ClerkId == ClerkId && x.EndTaskDateTime == null);
                }
                findItem.StatusId = core.Status.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                findItem.EndTaskDateTime = DateTime.Now;
                core.SaveChanges();
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }
        }
        public static void AgreementOfLeaseRenewal(int ApplicationId, bool Recommendation, bool _2ndRecommendation, bool RecommendationReview, bool RenewlAcceptance, bool DocsUploadRenewal)
        {
            var core = new eServicesDbContext();
            var RcsApplication = core.HumanSettlementApplications.Include(x => x.PreferredComplexArea).Include(x => x.PreferredComplexArea2).Include(x => x.Status).Where(r => r.Id == ApplicationId).FirstOrDefault();
            var responsibilityTypes = core.ResponsibilityTypes.ToList();
            int PreferredComplexAreaId = RcsApplication.PreferredComplexArea == null ? RcsApplication.PreferredComplexArea2.Id : RcsApplication.PreferredComplexArea.Id;
            //Regional Manager Senior Housing Specialist, Housing Liaison Officer, Caretaker
            try
            {
                if (Recommendation)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Housing Liaison Officer" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalFirstRecommendation).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (_2ndRecommendation)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Senior Housing Specialist" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Renewal2ndRecommendation).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (RecommendationReview)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Regional Manager" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalReview).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (RenewlAcceptance)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Housing Liaison Officer" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementRenewalAcceptance).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (DocsUploadRenewal)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Housing Liaison Officer" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalUploadDocs).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }

            }
            catch (Exception IO)
            {
                LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //     throw;
            }
        }
        public static void AgreementOfLease(int ApplicationId, bool GenerateAgreement, bool AgreementReview, bool AgreementApproval, bool SignantureAgreement, bool FinalizedAgreement, bool UpdateAgreement)
        {
            var core = new eServicesDbContext();
            var RcsApplication = core.HumanSettlementApplications.Include(x => x.PreferredComplexArea).Include(x => x.PreferredComplexArea2).Include(x => x.Status).Where(r => r.Id == ApplicationId).FirstOrDefault();
            var responsibilityTypes = core.ResponsibilityTypes.ToList();
            int PreferredComplexAreaId = RcsApplication.PreferredComplexArea == null ? RcsApplication.PreferredComplexArea2.Id : RcsApplication.PreferredComplexArea.Id;
            //Regional Manager Senior Housing Specialist, Housing Liaison Officer, Caretaker
            try
            {
                if (GenerateAgreement)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Housing Liaison Officer" });
                    var ResponsibilityTypeId = responsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement);
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (AgreementReview)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Senior Housing Specialist" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementReview).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (AgreementApproval)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Regional Manager" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementApproval).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (SignantureAgreement)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Housing Liaison Officer" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementSignature).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (FinalizedAgreement)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Housing Liaison Officer" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.FinalizeAgreement).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (UpdateAgreement)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Housing Liaison Officer" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.UpdateAgreement).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }

            }
            catch (Exception IO)
            {
                LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //     throw;
            }
        }
        public static void AgreementTerminations(int ApplicationId, bool ServeNotice, bool AccountValidation, bool ConfirmVacated)
        {
            var core = new eServicesDbContext();
            var RcsApplication = core.HumanSettlementApplications.Include(x => x.PreferredComplexArea).Include(x => x.PreferredComplexArea2).Include(x => x.Status).Where(r => r.Id == ApplicationId).FirstOrDefault();
            var responsibilityTypes = core.ResponsibilityTypes.ToList();
            int PreferredComplexAreaId = RcsApplication.PreferredComplexArea == null ? RcsApplication.PreferredComplexArea2.Id : RcsApplication.PreferredComplexArea.Id;
            //Regional Manager Senior Housing Specialist, Housing Liaison Officer, Caretaker
            try
            {
                if (ServeNotice)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Housing Liaison Officer" });
                    var ResponsibilityTypeId = responsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.NoticedAgreements);
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (AccountValidation)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Senior Housing Specialist" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.PendingTerminationreview).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (ConfirmVacated)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Housing Liaison Officer" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.PendingTakeOff).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
            }
            catch (Exception IO)
            {
                LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //     throw;
            }
        }
        public static void InspectionsPrePost(int ApplicationId, bool PostInspection, bool AccountValidation, bool ConfirmVacated)
        {
            var core = new eServicesDbContext();
            var RcsApplication = core.HumanSettlementApplications.Include(x => x.PreferredComplexArea).Include(x => x.PreferredComplexArea2).Include(x => x.Status).Where(r => r.Id == ApplicationId).FirstOrDefault();
            var responsibilityTypes = core.ResponsibilityTypes.ToList();
            int PreferredComplexAreaId = RcsApplication.PreferredComplexArea == null ? RcsApplication.PreferredComplexArea2.Id : RcsApplication.PreferredComplexArea.Id;
            //Regional Manager Senior Housing Specialist, Housing Liaison Officer, Caretaker
            try
            {
                if (PostInspection)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Housing Liaison Officer"/*, "Caretaker"*/ });
                    var ResponsibilityTypeId = responsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.PostInspection);
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (AccountValidation)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Senior Housing Specialist" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementReview).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
                if (ConfirmVacated)
                {
                    var WorkAllocationUser = MinimunWorkAllocatedUser(core, PreferredComplexAreaId, new string[] { "Regional Manager" });
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementApproval).FirstOrDefault();
                    var statusList = core.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    if (WorkAllocationUser != 0)
                    {
                        var roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.HumanSettlementApplicationId = RcsApplication.Id;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.ClerkId = WorkAllocationUser;
                        roundRobinQueue.StatusId = StatusId;
                        core.RoundRobinQueues.Add(roundRobinQueue);
                        FinishAllPreviousWork(RcsApplication.Id);
                        core.SaveChanges();
                        BackOfficeNotification(RcsApplication.Id, WorkAllocationUser, ResponsibilityTypeId.Name);
                    }
                    else
                    {
                        //Default User for all faulted work
                    }
                }
            }
            catch (Exception IO)
            {
                LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //     throw;
            }
        }

        #region Error_Handling
        public static void LogSystemError(string logEntry, string logType, string referenceType)
        {
            #region Text Error Log
            using (var context = new eServicesDbContext())
            {
                try
                {
                    int rType = context.ReferenceTypes.FirstOrDefault(o => o.Key == referenceType).Id;
                    int lType = context.LogTypes.FirstOrDefault(o => o.Key == logType).Id;

                    var txtErrorLog = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", 1, rType, logEntry, lType, true, false, DateTime.Now);
                    WriteToTextFile("System_Error_Log.txt", txtErrorLog);

                    context.Logs.Add(new Log()
                    {
                        ReferenceId = 2,
                        ReferenceTypeId = rType,
                        LogEntry = logEntry,
                        LogTypeId = lType,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDateTime = DateTime.Now
                    });
                    context.SaveChanges();
                }
                catch (Exception noConn)
                {
                    LogDBCError(logEntry, noConn.Message);
                }
            }
            #endregion
        }
        public static void LogDBCError(string logEntry, string exceptionEntry)
        {
            #region Text Error Log
            var txtErrorLog = string.Format("ReferenceId: {0} ReferenceTypeId: {1} LogEntry: {2} LogTypeId: {3} Active: {4} Deleted: {5} SystemError: {6} CreatedDateTime: {7}", 1, 4, exceptionEntry, 2, true, false, logEntry, DateTime.Now);
            //var root = HttpContext.Current.Server.MapPath("~/Logs/");
            //string fileName = "DBC_Error_Log.txt";
            //var path = System.IO.Path.Combine(root, fileName);
            //path = System.IO.Path.GetFullPath(path);
            //System.IO.File.WriteAllText(path, txtErrorLog);
            WriteToTextFile("DBC_Error_Log.txt", txtErrorLog);
            #endregion
        }
        public static void WriteToTextFile(string textFileName, string textValue)
        {
            try
            {
                var root = HttpContext.Current.Server.MapPath("~/Logs/");
                string fileName = FormatTextFileName(textFileName);
                var path = System.IO.Path.Combine(root, fileName);
                path = System.IO.Path.GetFullPath(path);
                System.IO.File.WriteAllText(path, textValue);
            }
            catch (Exception)
            {

            }

        }
        public static string FormatTextFileName(string textFileName)
        {
            if (textFileName.ToLower().Contains(".txt"))
            {
                return textFileName;
            }
            else
            {
                return textFileName + ".txt";
            }
        }
        #endregion

    }
}