using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;

namespace C8.eServices.Mvc.Controllers
{
    public class UnitsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
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
                              .Include(o => o.Status)
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
        public ActionResult Index()
        {
            var units = db.Units.Include(u => u.CreatedBySystemUser).Include(r=>r.HumanEHCOptions).Include(r => r.PreferredComplexArea).Include(u => u.ModifiedBySystemUser);
            return View(units.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Units units = db.Units.Find(id);
            if (units == null)
            {

                return HttpNotFound();
            }
            ViewBag.Occuupation = new SelectList(db.OccupationTypes.Where(x=>x.Id==id).FirstOrDefault().Propertytype.ToList());
            ViewBag.ComplexArea = new SelectList(db.PreferredComplexAreas.Where(x => x.Id == id).FirstOrDefault().Name.ToList());
            ViewBag.Settlement = new SelectList(db.humanEHCOptions.Where(x => x.Id == id).FirstOrDefault().Name.ToList());


            return View(units);
        }

        public ActionResult AddUnit()
        {

            try
            {
                Initialise();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                //var applicationProp = db.PropertyLeaseApplications.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Where(x => x.Id == id).FirstOrDefault();
                //ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                //ViewBag.FirstName = applicationProp.FirstName;
                //ViewBag.RfNo = applicationProp.ApplicationReferenceNumber;
                //ViewBag.DateCreated = applicationProp.CreatedDateTime.Value;
                //ViewBag.Sat = applicationProp.FirstName;
                Units units = new Units();
                units.AgentLastName = systemusers.LastName;
                units.AgentName = systemusers.FirstName;


                var dvm = new DepartmentsApprovalViewModel
                {
                    Units = units
                    //PropertyLeaseApplication = applicationProp,
                    //CommitteeOutcome = hd
                };
                
                ViewBag.UnitType = new SelectList(db.OccupationTypes.OrderBy(x => x.Propertytype), "Id", "Propertytype");
                ViewBag.ComplexArea = new SelectList(db.PreferredComplexAreas.OrderBy(x => x.Name).Where(x => x.IsDeleted != true), "Id", "Name");
                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.CommitteeDFC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                ViewBag.CommitteeREAC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Name == "REAC"), "Key", "Name");
                ViewBag.RecommendDFC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "dfc"), "Id", "Description");
                ViewBag.RecommendREAC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "reac"), "Id", "Description");
                ViewBag.date = DateTime.Now.Date;
                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                return View(dvm);
            }
            catch(Exception e)
            {

            }
            return View();
            ViewBag.EnvisageUsage = new SelectList(db.envisagedUsages.OrderBy(x => x.UssageName).ToList(), "Id", "UssageName");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.Occuupation = new SelectList(db.OccupationTypes.OrderBy(x=>x.Propertytype), "Id", "Propertytype");
            ViewBag.ComplexArea = new SelectList(db.PreferredComplexAreas.OrderBy(x => x.Name), "Id", "Name");
            ViewBag.Settlement = new SelectList(db.humanEHCOptions.OrderBy(x => x.Name), "Id", "Name");


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUnit(DepartmentsApprovalViewModel unitvalues)
        {
           
            try
            {
                eServicesDbContext context = new eServicesDbContext();
                Units Add = new Units();
                Add = unitvalues.Units;
                Initialise();
                var CustomerId = Customer;
                var custmusersI = context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var Systemusers = context.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userId = User.Identity.GetUserId();
                Add.AgentName = unitvalues.Units.AgentName;
                Add.AgentLastName = unitvalues.Units.AgentLastName;
                Add.AgentIDNo = unitvalues.Units.AgentIDNo;
                Add.AgentEmail = unitvalues.Units.AgentEmail;
                Add.AgentCell = unitvalues.Units.AgentCell;

                Add.UnitBuildingName = unitvalues.Units.UnitBuildingName;
                Add.PreferredComplexAreaId = unitvalues.Units.PreferredComplexAreaId;
                Add.OccupationTypeId = unitvalues.Units.OccupationTypeId;

                Add.Address = unitvalues.Units.Address;
                Add.Surburb = unitvalues.Units.Surburb;
                Add.BedroomCount = unitvalues.Units.BedroomCount;

                Add.PropertyPrice = unitvalues.Units.PropertyPrice;
                Add.PropertyDeposit = unitvalues.Units.PropertyDeposit;
                Add.LettingRequirements = unitvalues.Units.LettingRequirements;

                db.Units.Add(Add);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {

            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", unitvalues.Units.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", unitvalues.Units.ModifiedBySystemUserId);
            ViewBag.PropertyType = new SelectList(db.OccupationTypes, "Propertytype", "Propertytype");
            ViewBag.Complex_Area = new SelectList(db.PreferredComplexAreas, "Name", "Name");
            ViewBag.HumanEHC = new SelectList(db.humanEHCOptions, "Name", "Name");
            return View();
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Units units = db.Units.Find(id);
            if (units == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", units.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", units.ModifiedBySystemUserId);
            return View(units);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UnitTypeId,HumanEHC,PropertyType,Complex_Area,PropertyPrice,PropertyDeposit,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Units units)
        {
            if (ModelState.IsValid)
            {
                db.Entry(units).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", units.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", units.ModifiedBySystemUserId);
            return View(units);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Units units = db.Units.Find(id);
            if (units == null)
            {
                return HttpNotFound();
            }
            return View(units);
        }

        // POST: Units/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Units units = db.Units.Find(id);
            db.Units.Remove(units);
            db.SaveChanges();
            return RedirectToAction("Index");
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
