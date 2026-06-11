using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Engines;
using C8.eServices.Mvc.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class SchedulingController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        private NotificationEngine notificationEngine;
        private ComplaintWorkflowEngine workflowEngine;
        private IdentityManager identityManager;

        public SchedulingController()
        {
            notificationEngine = new NotificationEngine(db);
            workflowEngine = new ComplaintWorkflowEngine(db);
            identityManager = new IdentityManager(db);
        }

        [DecryptParameter]
        public ActionResult ManageSchedule(int referenceId, string referenceType)
        {
            if (referenceId == 0) throw new Exception("Invalid reference ID.");



            var vm = new GenericSchedulingViewModel
            {
                ReferenceId = referenceId,
                ReferenceType = referenceType
            };

            var TimeSlots = db.TimeSlots.Where(r => r.IsActive).ToList();
            var RolesList = TimeSlots.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            vm.AvailableTimeSlots = RolesList;

            if (referenceType == "Complaint")
            {
                var complaint = db.TenantComplaints
                    .Include(t => t.ComplaintCategory)
                    .Include(t => t.ComplaintType)
                    .FirstOrDefault(t => t.Id == referenceId);

                if (complaint == null) return HttpNotFound();

                vm.ReferenceNumber = complaint.CaseReferenceNumber;
                vm.TargetName = complaint.RespondentFirstName + " " + complaint.RespondentSurname;
                vm.HeaderTitle = "Complaint Investigation Scheduling";

                vm.ExistingSchedules = db.InspectionSchedules
                    .Include(r => r.DateToSchedule)
                    .Include(r => r.TimeSlot)
                    .Where(x => x.TenantComplaintId == referenceId && !x.IsDeleted && !x.IsInspected)
                    .ToList();
            }
            // Add PropertyLease or HumanSettlement branches here later when transitioning them over

            return View(vm);
        }

        [HttpPost]
        public ActionResult ManageSchedule(GenericSchedulingViewModel vm, DateTime DateToSchedule, params string[] SelectedRoles)
        {
            if (SelectedRoles == null)
            {
                Session["Display"] = "display";
                Session["MessageBody"] = "Please select hours available for the day!";
                Session["MessageTitle"] = "Invalid hours available";
                return RedirectToAction("ManageSchedule", new { q = new AesCrypto().Encrypt("referenceId=" + vm.ReferenceId + "&referenceType=" + vm.ReferenceType) });
            }

            try
            {


                var Dates = db.DateToSchedules
                    .Where(x => (vm.ReferenceType == "Complaint" && x.TenantComplaintId == vm.ReferenceId) && x.ShecduleDate == DateToSchedule.Date)
                    .GroupBy(r => r.ShecduleDate)
                    .Select(x => x.FirstOrDefault())
                    .ToList();

                if (Dates.Count == 0)
                {
                    DateToSchedule dateTo = new DateToSchedule();
                    dateTo.ShecduleDate = DateToSchedule.Date;
                    
                    if (vm.ReferenceType == "Complaint")
                        dateTo.TenantComplaintId = vm.ReferenceId;
                        
                    db.DateToSchedules.Add(dateTo);
                    db.SaveChanges();
                    
                    Dates = db.DateToSchedules
                        .Where(x => (vm.ReferenceType == "Complaint" && x.TenantComplaintId == vm.ReferenceId) && x.ShecduleDate == DateToSchedule.Date)
                        .GroupBy(r => r.ShecduleDate)
                        .Select(x => x.FirstOrDefault())
                        .ToList();
                }

                foreach (var item in Dates)
                {
                    foreach (var item2 in SelectedRoles)
                    {
                        var thisItem = Convert.ToInt32(item2);
                        var ispsch = db.InspectionSchedules
                            .OrderByDescending(x => x.Id)
                            .FirstOrDefault(x => x.DateToScheduleId == item.Id && x.TimeSlotId == thisItem && !x.IsDeleted);

                        if (ispsch == null)
                        {
                            InspectionSchedule apd = new InspectionSchedule();
                            
                            if (vm.ReferenceType == "Complaint")
                                apd.TenantComplaintId = vm.ReferenceId;

                            apd.DateToScheduleId = item.Id;
                            apd.TimeSlotId = thisItem;
                            
                            db.InspectionSchedules.Add(apd);
                            db.SaveChanges();
                        }
                    }
                }

                Session["TimeslotScheduledSession"] = string.Format($"Timeslot(s) has been scheduled for reference {vm.ReferenceNumber}");

                if (vm.ReferenceType == "Complaint")
                {
                    var complaint = db.TenantComplaints.Find(vm.ReferenceId);
                    var awaitingStatus = db.Status.FirstOrDefault(s => s.Key == ComplaintStatusKeys.AwaitingInvestigation);
                    if (awaitingStatus != null && complaint != null)
                    {
                        complaint.StatusId = awaitingStatus.Id;
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("ManageSchedule", new { q = new AesCrypto().Encrypt("referenceId=" + vm.ReferenceId + "&referenceType=" + vm.ReferenceType) });
            }
            catch (Exception ex)
            {
                // Handle Exception
            }

            return RedirectToAction("ManageSchedule", new { q = new AesCrypto().Encrypt("referenceId=" + vm.ReferenceId + "&referenceType=" + vm.ReferenceType) });
        }

        [HttpPost]
        public ActionResult FinalizeComplaintSchedule(int referenceId)
        {
            var complaint = db.TenantComplaints.Find(referenceId);
            
            // Get the first schedule for notification purposes
            var schedule = db.InspectionSchedules
                .Include(r => r.DateToSchedule)
                .Include(r => r.TimeSlot)
                .FirstOrDefault(x => x.TenantComplaintId == referenceId && !x.IsDeleted);

            // Re-create a ComplaintInvestigation object in memory just to pass to the existing notification engine
            if (schedule != null)
            {
                var investigation = new ComplaintInvestigation
                {
                    TenantComplaintId = complaint.Id,
                    AppointmentDate = schedule.DateToSchedule?.ShecduleDate
                };

                TimeSpan parsedTime;
                if (TimeSpan.TryParse(schedule.TimeSlot.Name, out parsedTime))
                {
                    investigation.AppointmentTime = parsedTime;
                }
                else if (DateTime.TryParse(schedule.TimeSlot.Name, out DateTime parsedDateTime))
                {
                    investigation.AppointmentTime = parsedDateTime.TimeOfDay;
                }

                var systemUser = identityManager.CurrentUser(User);
                Customer customer = null;
                if (systemUser != null)
                {
                    var systemUserId = systemUser.Id;
                    customer = db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                    investigation.ScheduledById = customer?.Id;
                }
                investigation.DateScheduled = DateTime.Now;

                // Save to database so ConfirmAppointment works
                db.ComplaintInvestigations.Add(investigation);
                db.SaveChanges();

                if (customer != null)
                {
                    workflowEngine.RoundRobinMarkFinished(customer.Id, ResponsibilityTypeKeys.ComplaintInvestigation, complaint.Id);
                }

                notificationEngine.SendAppointmentNotification(complaint, investigation);
            }
            
            return RedirectToAction("Details", "Complaints", new { q = new AesCrypto().Encrypt("id=" + referenceId) });
        }

        public ActionResult ApproveTimeSlot(int Id)
        {
            if (Id == 0) throw new Exception("Invalid schedule");
            
            var schedule = db.InspectionSchedules.Find(Id);
            if (schedule != null)
            {
                schedule.IsApproved = true;
                db.Entry(schedule).State = EntityState.Modified;
                
                // Set others to IsDeleted = true
                var unselected = db.InspectionSchedules.Where(s => s.DateToScheduleId == schedule.DateToScheduleId && s.Id != Id);
                foreach (var unsel in unselected)
                {
                    unsel.IsDeleted = true;
                    db.Entry(unsel).State = EntityState.Modified;
                }
                
                db.SaveChanges();
            }
            
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteSchedule(int Id)
        {
            if (Id == 0) throw new Exception("Invalid schedule");
            
            var schedule = db.InspectionSchedules.Find(Id);
            if (schedule != null)
            {
                schedule.IsDeleted = true;
                db.Entry(schedule).State = EntityState.Modified;
                db.SaveChanges();
            }
            
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
