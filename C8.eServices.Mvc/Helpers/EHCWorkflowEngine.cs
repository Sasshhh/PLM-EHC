using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Globalization;
using C8.eServices.Mvc.Helpers;
using System.Web.Mvc;
using MoreLinq;

namespace C8.eServices.Mvc.Helpers
{
    public static class EHCWorkflowEngine
    {
        public static List<Customer> ToAllocateBackOffice(eServicesDbContext core, PropertyLeaseApplication application)
        {
            Customer backOffice = new Customer();
            PreferredComplexArea complex_1 = core.PreferredComplexAreas.Include(c => c.LettingOfficer).FirstOrDefault(a => a.Id == application.PreferredComplexAreaId);
            PreferredComplexArea complex_2 = core.PreferredComplexAreas.Include(c => c.LettingOfficer).FirstOrDefault(a => a.Id == application.PreferredComplexArea2Id);
            List<Customer> lettingOfficers = new List<Customer>();
            if (complex_1 != null && complex_1.LettingOfficer != null)
                lettingOfficers.Add(complex_1.LettingOfficer);
            //lettingOfficers.Add(complex_2.LettingOfficer);
            return lettingOfficers;
        }

        public static Customer GetBackOfficeId(eServicesDbContext core, int Id, bool LF)
        {
            Customer clerk = new Customer();
            var appu = core.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == Id);
            var matched = core.MatchedUnits.FirstOrDefault(x => x.Id == appu.MatchedID);

            if (matched.ApplicationAllocatedPropertyId != null)
            {
                var unit = core.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == matched.ApplicationAllocatedPropertyId);
                var complex = core.PreferredComplexAreas.FirstOrDefault(x => x.Id == unit.OfferedComplexId);
                if (LF)
                {
                    clerk = core.Customers.FirstOrDefault(x => x.Id == complex.LettingOfficerId);
                }
                else
                {
                    clerk = core.Customers.FirstOrDefault(x => x.Id == complex.LettingOfficerId);
                }
            }
            else
            {
                var unit = core.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == matched.UnitsEkurhuleniHousingCompanyId);
                var complex = core.PreferredComplexAreas.FirstOrDefault(x => x.Id == unit.PreferredComplexAreaId);
                if (LF)
                {
                    clerk = core.Customers.FirstOrDefault(x => x.Id == complex.LettingOfficerId);
                }
                else
                {
                    clerk = core.Customers.FirstOrDefault(x => x.Id == complex.LettingOfficerId);
                }
            }

            return clerk ?? new Customer { Id = 0 };
        }

        public static Customer GetHousingSupervisorId(eServicesDbContext core, int Id)
        {
            Customer clerk = new Customer();
            var appu = core.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == Id);
            var matched = core.MatchedUnits.FirstOrDefault(x => x.Id == appu.MatchedID);

            if (matched.ApplicationAllocatedPropertyId != null)
            {
                var unit = core.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == matched.ApplicationAllocatedPropertyId);
                var complex = core.PreferredComplexAreas.FirstOrDefault(x => x.Id == unit.OfferedComplexId);
                clerk = core.Customers.FirstOrDefault(x => x.Id == complex.HousingSuperId);
            }
            else
            {
                var unit = core.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == matched.UnitsEkurhuleniHousingCompanyId);
                var complex = core.PreferredComplexAreas.FirstOrDefault(x => x.Id == unit.PreferredComplexAreaId);
                clerk = core.Customers.FirstOrDefault(x => x.Id == complex.HousingSuperId);
            }

            return clerk ?? new Customer { Id = 0 };
        }

        public static Customer GetMaintenanceManagerId(eServicesDbContext core, int Id)
        {
            Customer clerk = new Customer();
            var appu = core.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == Id);
            var matched = core.MatchedUnits.FirstOrDefault(x => x.Id == appu.MatchedID);

            if (matched.ApplicationAllocatedPropertyId != null)
            {
                var unit = core.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == matched.ApplicationAllocatedPropertyId);
                var complex = core.PreferredComplexAreas.FirstOrDefault(x => x.Id == unit.OfferedComplexId);
                clerk = core.Customers.FirstOrDefault(x => x.Id == complex.MaintenanceManagerId);
            }
            else
            {
                var unit = core.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == matched.UnitsEkurhuleniHousingCompanyId);
                var complex = core.PreferredComplexAreas.FirstOrDefault(x => x.Id == unit.PreferredComplexAreaId);
                clerk = core.Customers.FirstOrDefault(x => x.Id == complex.MaintenanceManagerId);
            }

            return clerk ?? new Customer { Id = 0 };
        }

        public static Customer GetPropertyFacilitiesManagerId(eServicesDbContext core, int Id)
        {
            // Property & Facilities Manager is a system-wide one-to-one role
            // (like Revenue Manager, CEO, Property Manager) — not per-complex.
            // Read directly from the AppSetting rather than the complex table.
            try
            {
                var setting = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.PropertyFacilitiesManager);
                if (setting != null && int.TryParse(setting.Value, out int customerId) && customerId > 0)
                {
                    var customer = core.Customers.FirstOrDefault(x => x.Id == customerId);
                    if (customer != null) return customer;
                }
            }
            catch { }

            return new Customer { Id = 0 };
        }

        #region UC031 / UC032 deposit refund routing

        /// <summary>Creates the UC031 status and task type when the database does not have them yet (older upgrade script).</summary>
        public static void EnsureRefundProcessingMasterData(eServicesDbContext db)
        {
            bool changed = false;
            if (!db.Status.Any(x => x.Key == StatusKeys.AwaitingDepositRefundProcessing))
            {
                var like = db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRefundResponse);
                if (like != null)
                {
                    db.Status.Add(new C8.eServices.Mvc.Models.Status
                    {
                        Name = "Awaiting Deposit Refund Processing",
                        Description = "Refund started by the CSO; Client Services Supervisor to process deductions",
                        Key = StatusKeys.AwaitingDepositRefundProcessing,
                        StatusTypeId = like.StatusTypeId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDateTime = DateTime.Now
                    });
                    changed = true;
                }
            }
            if (!db.ResponsibilityTypes.Any(x => x.Key == ResponsibilityTypeKeys.DepositRefundProcessing))
            {
                db.ResponsibilityTypes.Add(new ResponsibilityType
                {
                    Name = "Deposit Refund Processing",
                    Description = "Client Services Supervisor processes the deposit refund / deductions (UC031)",
                    Key = ResponsibilityTypeKeys.DepositRefundProcessing,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDateTime = DateTime.Now
                });
                changed = true;
            }
            // Exit inspection stages (application status while the lease is on s_awaiting_exit_inspec).
            var exitLike = db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingExitInspection);
            if (exitLike != null)
            {
                foreach (var st in new[] {
                    new { Key = StatusKeys.AwaitingExitInspectionSlotSelection, Name = "Awaiting Applicant to Select Exit Inspection Slots", Desc = "Exit inspection slots published; waiting for the tenant to select one" },
                    new { Key = StatusKeys.ExitInspectionScheduled, Name = "Exit Inspection Scheduled", Desc = "Tenant selected an exit inspection slot; supervisor to conduct the exit inspection" } })
                {
                    if (!db.Status.Any(x => x.Key == st.Key))
                    {
                        db.Status.Add(new C8.eServices.Mvc.Models.Status { Name = st.Name, Description = st.Desc, Key = st.Key, StatusTypeId = exitLike.StatusTypeId, IsActive = true, IsDeleted = false, CreatedDateTime = DateTime.Now });
                        changed = true;
                    }
                }
            }
            if (changed) db.SaveChanges();
        }

        /// <summary>Latest (current) lease row of an application.</summary>
        public static LeaseDetails CurrentLease(eServicesDbContext db, int appId)
        {
            return db.LeaseDetails.Include(x => x.Status).Where(x => x.PropertyLeaseApplicationId == appId && !x.IsDeleted).OrderByDescending(x => x.Id).FirstOrDefault();
        }

        /// <summary>True when the application's current lease is waiting for an exit inspection.</summary>
        public static bool IsExitInspection(eServicesDbContext db, int appId)
        {
            var lease = CurrentLease(db, appId);
            return lease != null && lease.Status != null && lease.Status.Key == StatusKeys.AwaitingExitInspection;
        }

        /// <summary>
        /// Exit inspection stage for a lease on s_awaiting_exit_inspec, from the application status:
        /// "schedule" (supervisor publishes slots), "select" (tenant must pick a slot), "conduct" (slot picked).
        /// </summary>
        public static string ExitInspectionStage(string applicationStatusKey)
        {
            if (applicationStatusKey == StatusKeys.AwaitingExitInspectionSlotSelection) return "select";
            if (applicationStatusKey == StatusKeys.ExitInspectionScheduled) return "conduct";
            return "schedule";
        }

        private static int AppSettingInt(eServicesDbContext db, string key)
        {
            int value;
            int.TryParse(db.AppSettings.Where(x => x.Key == key).Select(x => x.Value).FirstOrDefault(), out value);
            return value;
        }

        /// <summary>Client Services Supervisor of the tenant's complex (PreferredComplexAreas.HousingSuperId), else AppSettings u_housing_super_visor.</summary>
        public static int RefundSupervisorId(eServicesDbContext db, int appId)
        {
            try
            {
                var sup = GetHousingSupervisorId(db, appId);
                if (sup != null && sup.Id != 0) return sup.Id;
            }
            catch { }
            return AppSettingInt(db, AppSettingKeys.HousingSupervisor);
        }

        public static int RefundRevenueManagerId(eServicesDbContext db) { return AppSettingInt(db, AppSettingKeys.RevenueManager); }
        public static int RefundCeoId(eServicesDbContext db) { return AppSettingInt(db, AppSettingKeys.CEO); }

        /// <summary>CSO / Letting Officer of the tenant's complex, else AppSettings u_letting_officer.</summary>
        public static int RefundCsoId(eServicesDbContext db, int appId)
        {
            try
            {
                var lo = GetBackOfficeId(db, appId, true);
                if (lo != null && lo.Id != 0) return lo.Id;
            }
            catch { }
            return AppSettingInt(db, AppSettingKeys.LettingOfficer);
        }

        /// <summary>Opens one refund task (round-robin row) for the given user on the application's latest lease row and e-mails them.</summary>
        public static void OpenRefundTask(eServicesDbContext db, int appId, string responsibilityKey, int clerkId)
        {
            if (clerkId == 0) throw new Exception("No user is set up for the refund step '" + responsibilityKey + "'. Check the complex supervisor / AppSettings.");
            var resp = db.ResponsibilityTypes.FirstOrDefault(x => x.Key == responsibilityKey);
            if (resp == null) throw new Exception("Missing ResponsibilityTypes row " + responsibilityKey + ".");
            var submittedId = db.Status.First(x => x.Key == StatusKeys.Submitted).Id;
            var lease = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == appId && !x.IsDeleted);
            db.RoundRobinQueues.Add(new RoundRobinQueue
            {
                PropertyLeaseApplicationId = appId,
                LeaseDetailsId = lease != null ? lease.Id : (int?)null,
                ResponsibilityTypeId = resp.Id,
                CurrentTaskDateTime = DateTime.Now,
                ClerkId = clerkId,
                StatusId = submittedId
            });
            db.SaveChanges();
            BackOfficeNotification(db, appId, clerkId, resp.Name);
        }

        /// <summary>Plain e-mail (and SMS where a number exists) to each customer id: tenant or staff. Failures are logged, never thrown.</summary>
        public static void NotifyRefundParties(eServicesDbContext db, int appId, IEnumerable<int> customerIds, string subject, string message)
        {
            var app = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == appId);
            foreach (var id in customerIds.Where(x => x != 0).Distinct())
            {
                try
                {
                    var c = db.Customers.Include(x => x.SystemUser).FirstOrDefault(x => x.Id == id);
                    if (c == null) continue;
                    var email = !string.IsNullOrEmpty(c.EmailAddress) ? c.EmailAddress : (c.SystemUser != null ? c.SystemUser.EmailAddress : null);
                    if (string.IsNullOrEmpty(email)) continue;
                    var name = (c.FirstName + " " + c.LastName).Trim();
                    var body = message + (app != null ? "<br/>Application reference: " + app.ApplicationReferenceNumber : "");
                    new Email().GenerateEmailSMS(message, c.CellPhoneNumber, appId, id, body, email, subject, body, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, name);
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }
            }
        }

        #endregion

        public static bool BackOfficeNotification(eServicesDbContext db, int RCSAppID, int CustomerID, string QueueName)
        {
            if (CustomerID == 0) return false;
            try
            {
                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.BONewCaseLoaded).FirstOrDefault();
                Email SendMail = new Email();
                var RCSApplication = db.PropertyLeaseApplications.Where(x => x.Id == RCSAppID).FirstOrDefault();
                var BackOfficeClerk = db.Customers.Where(x => x.Id == CustomerID).FirstOrDefault();
                string attorneyemail = BackOfficeClerk.EmailAddress;
                string attorneyname = BackOfficeClerk.FirstName + " " + BackOfficeClerk.LastName;
                string emailbody = getemailbody.Description;
                emailbody = emailbody.Replace("{0}", QueueName);
                emailbody = emailbody.Replace("{1}", RCSApplication.ApplicationReferenceNumber);
                var systemusermobilenum = BackOfficeClerk.CellPhoneNumber;
                var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + emailbody;
                SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum, RCSApplication.Id, CustomerID, emailbody, attorneyemail, "PLM-Online Application", emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                return true;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }
        }

        public static int EHCRoundRobin(
            eServicesDbContext db,
            int RCSAppID,
            bool RiskAssessment,
            bool ValidateDepositPayment,
            bool InviteToClientTraining,
            bool UnitInspections,
            bool UpdateTenantDetails,
            bool GenerateLeaseAgreement,
            bool LeaseAgreementValidation,
            bool DebitOrderValidation,
            bool ShechuleInspectionSlots,
            bool MaintananceJobSheet,
            bool Terminations,
            bool TerminationValidation,
            bool CommitteeOutcomes,
            bool VacatingConfirmation,
            bool RecomendForRenewal,
            bool SecondRenewalRecommendation,
            bool LeaseRenewalRevenue,
            bool RenewalRiskAssessment,
            bool UnitMaintenance,
            bool AgreemrntValidateRevenue,
            bool PropertyFacilitiesManagerReview, // Renamed from 'six'
            bool AgreementApprovalCEO,
            bool eight,
            int DepartmentID,
            bool AcknowlegeRefund,
            bool IssueRefundsCollection,
            int RefundAppID,
            bool? IsAwaitingRefundResponse = null,
            bool? IsAwaitingDocUploadingForMigratedApps = null,
            bool? IsAwaitingRefundAuthorisation = null)
        {
            var applicationUserRoles = db.Roles.ToList();
            var dates = new string[2];
            dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            var startDate = DateTime.Parse(dates[0]).Date;
            var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            var oneDayTime = endDate - startDate;
            var SubmittedId_Rpt = db.Status.FirstOrDefault(x => x.Key == StatusKeys.Submitted).Id;
            var RoundRobingQueue = db.RoundRobinQueues.Where(x => x.StatusId == SubmittedId_Rpt && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();

            var AssignedToUser = 0;
            var responsibilityTypes = db.ResponsibilityTypes.ToList();

            if (RiskAssessment)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                List<Customer> LettingOfficers = ToAllocateBackOffice(db, RcsApplication).DistinctBy(a => a.SystemUserId).ToList();

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RiskAssessment).FirstOrDefault();
                if (LettingOfficers.Any())
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    foreach (var boUser in LettingOfficers)
                    {
                        var roundRobinQueue = new RoundRobinQueue
                        {
                            PropertyLeaseApplicationId = RcsApplication.Id,
                            ResponsibilityTypeId = ResponsibilityTypeId.Id,
                            CurrentTaskDateTime = DateTime.Now,
                            ClerkId = boUser.Id,
                            StatusId = StatusId
                        };
                        db.RoundRobinQueues.Add(roundRobinQueue);
                        db.SaveChanges();
                        BackOfficeNotification(db, RCSAppID, boUser.Id, ResponsibilityTypeId.Name);
                    }
                }
            }
            else if (ValidateDepositPayment)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.ValidateDepositPayment).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (InviteToClientTraining)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var UserId = GetBackOfficeId(db, RCSAppID, false);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.InviteToClientTraining).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (ShechuleInspectionSlots)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var UserId = GetHousingSupervisorId(db, RCSAppID);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.ScheduleInspectionSlots).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (UnitInspections)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var UserId = GetHousingSupervisorId(db, RCSAppID);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Inspections).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (MaintananceJobSheet)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var UserId = GetMaintenanceManagerId(db, RCSAppID);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.MaintenanceManager).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.MaintananceJobSheet).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (UpdateTenantDetails)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var UserId = GetBackOfficeId(db, RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CaptureLeaseDetails).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (GenerateLeaseAgreement)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var UserId = GetBackOfficeId(db, RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (LeaseAgreementValidation)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueManager).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseAgreementValidation).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (DebitOrderValidation)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.DebitOrderVAlidation).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (Terminations)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                // UC023: Use RevenueOfficer as the default termination clerk; GetBackOfficeId may throw if no ApplicantUnit exists
                Customer backOfficeUser = null;
                try { backOfficeUser = GetBackOfficeId(db, RCSAppID, true); } catch { }
                var revenueOfficerSetting = db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.RevenueOfficer);
                var StoredUser = revenueOfficerSetting != null ? Convert.ToInt16(revenueOfficerSetting.Value) : 0;
                var activeDirectoryOn = (backOfficeUser != null && backOfficeUser.Id != 0) ? backOfficeUser.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Terminations).FirstOrDefault();
                if (activeDirectoryOn != 0 && ResponsibilityTypeId != null)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        LeaseDetailsId = findItem != null ? findItem.Id : (int?)null,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (TerminationValidation)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.TerminationValidation).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        LeaseDetailsId = findItem.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (CommitteeOutcomes)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CommitteeOutcomes).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (VacatingConfirmation)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var UserId = GetBackOfficeId(db, RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.VacatingConfirmation).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        LeaseDetailsId = findItem.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (RecomendForRenewal)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var UserId = GetBackOfficeId(db, RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseRenewals).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        LeaseDetailsId = findItem.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (SecondRenewalRecommendation)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SecondLeaseRenewal).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        LeaseDetailsId = findItem.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (LeaseRenewalRevenue)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueManager).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseRenewalRevenue).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        LeaseDetailsId = findItem.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (RenewalRiskAssessment)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalRiskAssessment).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        LeaseDetailsId = findItem.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (UnitMaintenance)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var UserId = GetMaintenanceManagerId(db, RCSAppID);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.MaintenanceManager).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.UnitMaintenanance).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (AgreemrntValidateRevenue)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueManager).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseAgreementValidation).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (AgreementApprovalCEO)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementApproval).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (PropertyFacilitiesManagerReview)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var UserId = GetPropertyFacilitiesManagerId(db, RCSAppID);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyFacilitiesManager).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.PropertyFacilitiesManagerReview).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (IssueRefundsCollection)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var UserId = GetBackOfficeId(db, RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.DepositRefund).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        LeaseDetailsId = findItem.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (IsAwaitingRefundResponse != null && IsAwaitingRefundResponse.Value == true)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                // UC032: the refund response is the Revenue Manager's step (Refund > Update Refund Response).
                // It used to go to the complex's Letting Officer, so the Revenue Manager's list stayed empty.
                int revenueManager = 0;
                int.TryParse(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueManager).Select(x => x.Value).FirstOrDefault(), out revenueManager);
                var activeDirectoryOn = revenueManager;
                if (activeDirectoryOn == 0)
                {
                    var UserId = GetBackOfficeId(db, RCSAppID, true);
                    var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                    activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;
                }

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.DepositRefundResponse).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        LeaseDetailsId = findItem.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (IsAwaitingDocUploadingForMigratedApps != null && IsAwaitingDocUploadingForMigratedApps.Value == true)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var UserId = GetBackOfficeId(db, RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AwaitingDocUploadingForMigratedApps).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }
            else if (IsAwaitingRefundAuthorisation != null && IsAwaitingRefundAuthorisation.Value == true)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.CEO).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RefundAuthorisation).FirstOrDefault();
                if (activeDirectoryOn != 0 && ResponsibilityTypeId != null)
                {
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        LeaseDetailsId = findItem.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }

            return AssignedToUser;
        }
    }
}
