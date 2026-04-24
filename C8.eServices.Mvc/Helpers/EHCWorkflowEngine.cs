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
            bool seven,
            bool eight,
            int DepartmentID,
            bool AcknowlegeRefund,
            bool IssueRefundsCollection,
            int RefundAppID,
            bool? IsAwaitingRefundResponse = null,
            bool? IsAwaitingDocUploadingForMigratedApps = null)
        {
            var applicationUserRoles = db.Roles.ToList();
            var dates = new string[2];
            dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            var startDate = DateTime.Parse(dates[0]).Date;
            var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            var oneDayTime = endDate - startDate;
            var RoundRobingQueue = db.RoundRobinQueues.Where(x => x.IsActive == true && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();

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
                var activeDirectoryOn2 = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseAgreementValidation).FirstOrDefault();
                if (activeDirectoryOn != 0 && activeDirectoryOn2 != 0)
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

                    var roundRobinQueue2 = new RoundRobinQueue
                    {
                        PropertyLeaseApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn2,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue2);
                    db.SaveChanges();
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                    BackOfficeNotification(db, RCSAppID, activeDirectoryOn2, ResponsibilityTypeId.Name);
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
                var UserId = GetBackOfficeId(db, RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Terminations).FirstOrDefault();
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
                var UserId = GetBackOfficeId(db, RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

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

            return AssignedToUser;
        }
    }
}
