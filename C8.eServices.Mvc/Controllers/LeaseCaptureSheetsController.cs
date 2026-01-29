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
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Globalization;
using C8.eServices.Mvc.ApiServices;
using System.Web.Routing;
using System.Data.Entity.Core.Objects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Controllers
{
    public class LeaseCaptureSheetsController : Controller
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
            var leaseCaptureSheets = db.LeaseCaptureSheets.Include(l => l.CreatedBySystemUser).Include(l => l.ModifiedBySystemUser);
            return View(leaseCaptureSheets.ToList());
        }

        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaseCaptureSheet leaseCaptureSheet = db.LeaseCaptureSheets.Find(id);
            if (leaseCaptureSheet == null)
            {
                return HttpNotFound();
            }
            return View(leaseCaptureSheet);
        }


        public ActionResult Capture(/*int? id*/)
        {

            LeaseDetails lease = new LeaseDetails();
            //var matchedUnit = db.MatchedUnits.FirstOrDefault(x => x.Id == id) ?? null;
            //var propertyLease = db.PropertyLeaseApplications.FirstOrDefault(x => x.ApplicationReferenceNumber == matchedUnit.LeaseRefrence) ?? null;
            //var units = db.Units.FirstOrDefault(x => x.Id == matchedUnit.unitID && !x.IsDeleted) ?? null;
            //var applicantUnit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLease.Id) ?? null;

            LeaseCaptureAddressContact leaseCaptureAddress = new LeaseCaptureAddressContact();
            LeaseCaptureSheet leaseCaptureSheet = new LeaseCaptureSheet();
            TenantViewModel model = new TenantViewModel
            {
                LeaseCaptureAddressContact = leaseCaptureAddress,
                LeaseCaptureSheet= leaseCaptureSheet
                //,Lease = lease,
                //Unit = units,
                //ApplicantUnit = applicantUnit,
                //PropertyLeaseApplication = propertyLease
            };
            //ViewBag.VAT = units.PropertyPrice * 0.15;
            //ViewBag.Total = (units.PropertyPrice * 0.15) + units.PropertyPrice;
            ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Capture(TenantViewModel tenant)
        {
            PropertyLeaseApplication property = new PropertyLeaseApplication();
            LeaseDetails lease = new LeaseDetails();
            LeaseCaptureAddressContact leaseCaptureAddress = new LeaseCaptureAddressContact();
            LeaseCaptureSheet leaseCaptureSheet = new LeaseCaptureSheet();
            property = tenant.PropertyLeaseApplication;
            lease = tenant.Lease;
            leaseCaptureAddress = tenant.LeaseCaptureAddressContact;
            leaseCaptureSheet = tenant.LeaseCaptureSheet;
            using (var es = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var AppSettings = es.AppSettings;
                    AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.PLMSequence);
                    var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.PLMSequenceLimit);
                    var BatchCounter = query.Value;

                    ////lease.leaseApplicationRef = tenant.PropertyLeaseApplication.ApplicationReferenceNumber;
                    leaseCaptureSheet.LeaseReferenceNo = "PLE" + DateTime.Now.ToString("ddMMyy") + BatchCounter;




                    int limiter = 0;
                    limiter = Convert.ToInt16(SeqLimit.Value);
                    if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                    {
                        var lastRef = query.Value;
                        BatchCounter = lastRef.ToString();
                        int nextSeq = Convert.ToInt16(query.Value) + 1;
                        string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                        query.Value = nextVal;
                        es.Entry(query).State = EntityState.Modified;
                        es.SaveChanges();
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
                        es.Entry(query).State = EntityState.Modified;
                        es.SaveChanges();

                    }

                    lease.PurchaserTypeId = tenant.Lease.PurchaserTypeId;

                    //lease.buildingName = tenant.Unit.UnitBuildingName;
                    //lease.SpaceUnitNo = tenant.Unit.UnitBuildingName;//To be a number, I dunoif it wss captured or not

                    //lease.LeaStreet = tenant.Unit.Address; //Address
                    //lease.LeaSuburb = tenant.Unit.Address; //surburb
                    //                                       //Geo-location =!!!

                    //lease.StartDate = tenant.Lease.StartDate;
                    //lease.PeriodInMonths = tenant.Lease.PeriodInMonths;
                    //lease.EndDate = tenant.Lease.EndDate;
                    //lease.RenewalNotice = tenant.Lease.RenewalNotice;
                    //lease.TerminationNotice = tenant.Lease.TerminationNotice;
                    //lease.DepositeAmount = tenant.Lease.DepositeAmount;
                    //lease.RentalAmount = tenant.Lease.RentalAmount;
                    //lease.VATAmount = (lease.RentalAmount * 0.15);
                    //lease.TotalIncludingVAT = (lease.RentalAmount + lease.VATAmount);

                    //lease.StatementDate = tenant.Lease.StatementDate;
                    //lease.EscalationDate = tenant.Lease.EscalationDate;
                    //lease.Email = tenant.Lease.Email;
                    //lease.SMS = tenant.Lease.SMS;
                    //lease.Postal = tenant.Lease.Postal;

                    //if (lease.TenantType == "t_Person")
                    //{
                    //    //property.ResAddress = tenant.PropertyLeaseApplication.ResAddress;
                    //    //property.ResSuburb = tenant.PropertyLeaseApplication.ResSuburb;
                    //    //property.ResPostal = tenant.PropertyLeaseApplication.ResPostal;

                    //    //lease.FirstNames = tenant.Lease.FirstNames;
                    //    //lease.LastName = tenant.Lease.LastName;
                    //    lease.IDNo = tenant.Lease.IDNo;
                    //    lease.TypeOfActivities = "N/A";
                    //    lease.ComplianceDetails = "N/A";
                    //}
                    //else if (lease.TenantType == "t_company")
                    //{
                    //    property.ContactPName = tenant.PropertyLeaseApplication.ContactPName;
                    //    property.ContactPSurname = tenant.PropertyLeaseApplication.ContactPSurname;
                    //    property.Capacity = tenant.PropertyLeaseApplication.Capacity;
                    //    property.CPIdNumber = tenant.PropertyLeaseApplication.CPIdNumber;

                    //    property.CPCellNo = tenant.PropertyLeaseApplication.CPCellNo;
                    //    property.CPWorkNo = tenant.PropertyLeaseApplication.CPWorkNo;
                    //    property.CPHomeNo = tenant.PropertyLeaseApplication.CPHomeNo;
                    //    property.CPEmail1 = tenant.PropertyLeaseApplication.CPEmail1;
                    //    property.CPEmail2 = tenant.PropertyLeaseApplication.CPEmail2;

                    //    lease.TypeOfActivities = tenant.Lease.TypeOfActivities;
                    //    lease.ComplianceDetails = tenant.Lease.ComplianceDetails;
                    //    lease.FirstNames = "N/A";
                    //    lease.LastName = "N/A";
                    //    lease.IDNo = "N/A";

                    //}
                    //es.PropertyLeaseApplications.Add(property);
                    //es.Entry(property).State = EntityState.Modified;
                    //es.SaveChanges();
                    //es.LeaseDetails.Add(lease);
                    
                    es.LeaseCaptureSheets.Add(leaseCaptureSheet);
                    es.SaveChanges();
                    leaseCaptureAddress.LeaseCaptureSheetId = leaseCaptureSheet.Id;
                    es.LeaseCaptureAddressContacts.Add(leaseCaptureAddress);
                    es.SaveChanges();



                    return RedirectToAction("LeaseList", "LeaseDetails");
                }
                catch(Exception e)
                {
                }
                ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");

            }


            return RedirectToAction("LeaseList", "LeaseDetails");

        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaseCaptureSheet leaseCaptureSheet = db.LeaseCaptureSheets.Find(id);
            if (leaseCaptureSheet == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", leaseCaptureSheet.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", leaseCaptureSheet.ModifiedBySystemUserId);
            return View(leaseCaptureSheet);
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,LeaseReferenceNo,TenantCode,PropertyCode,HighLevelPropertyCode,SolarAccountNumber,PropertyOfficerName,Signiture,TenantName,TenantType,CompantRegistrationNo,VAtRegistrationNo,IDNo,BuildingNumberName,ContactName,ContactSurname,RepresentativeCO,LeaNegContactName,LeaNegContactSurname,LeaNegRepresentativeCO,CAA,Lattitude,Longitude,REDReferenceNo,CientAdvertisingSignReferenceNo,CityPanningreferenceNo,BuildingName,UsageOfPremises,AreaLeased,LeaseClassification,AdminOfficerCollection,LeaseStartDate,LeaseEndDate,DateOfFirstRentalEscalation,CommencementRental,LegalActionTaken,DepositPaid,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] LeaseCaptureSheet leaseCaptureSheet)
        {
            if (ModelState.IsValid)
            {
                db.Entry(leaseCaptureSheet).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", leaseCaptureSheet.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", leaseCaptureSheet.ModifiedBySystemUserId);
            return View(leaseCaptureSheet);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaseCaptureSheet leaseCaptureSheet = db.LeaseCaptureSheets.Find(id);
            if (leaseCaptureSheet == null)
            {
                return HttpNotFound();
            }
            return View(leaseCaptureSheet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LeaseCaptureSheet leaseCaptureSheet = db.LeaseCaptureSheets.Find(id);
            db.LeaseCaptureSheets.Remove(leaseCaptureSheet);
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
