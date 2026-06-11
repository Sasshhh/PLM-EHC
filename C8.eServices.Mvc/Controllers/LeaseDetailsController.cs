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
using Microsoft.AspNet.Identity;
using Microsoft.Ajax.Utilities;
using System.Runtime.Remoting.Contexts;
using System.Web.UI.WebControls;
using System.Runtime.Remoting.Lifetime;
using System.Drawing;
using iTextSharp.text.pdf;
using System.IO;
namespace C8.eServices.Mvc.Controllers
{
    public class LeaseDetailsController : Controller
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
       
        [DecryptParameter]
        public ActionResult ManageTenant(int? refNo, string IsManage)
        {
            ViewBag.ManageLeaseId = refNo;
            LeaseDetails leaseDetails = db.LeaseDetails.Where(x => x.Id == refNo).FirstOrDefault();
            List<PropertyResident> NewResidents = null;
            List<PropertyResident> OldResidents = null;
            TenantViewModel model = new TenantViewModel();
            try
            {
                NewResidents = db.PropertyResidents.Include(v => v.Status).Where(x => x.StatusId != db.Status.FirstOrDefault(r => r.Key == StatusKeys.DeactiveOccupant).Id && (x.RenewalLeaseId == leaseDetails.Id || x.LeaseDetailsId == leaseDetails.Id)).ToList() ?? null;
                OldResidents = db.PropertyResidents.Include(v => v.Status).Where(x => x.StatusId != db.Status.FirstOrDefault(r => r.Key == StatusKeys.ActiveOccupant).Id && (x.RenewalLeaseId == leaseDetails.Id || x.LeaseDetailsId == leaseDetails.Id)).ToList() ?? null;
            }
            catch (Exception)
            {
            }
            var temp = String.Empty;
            if (Session["IsManage"] != null) temp = Session["IsManage"].ToString();
            if (String.IsNullOrEmpty(IsManage) && !String.IsNullOrEmpty(temp)) IsManage = temp;
            Session["IsManage"] = null;
            ViewBag.IsManage = IsManage;
            model.PropertyResidentList = NewResidents;
            model.OldResidents = OldResidents;
            model.Lease = leaseDetails;
            if (Session["AddOccupantsSession"] != null)
            {
                var value = Session["AddOccupantsSession"].ToString();
                Session["AddOccupantsSession"] = null;
                ViewBag.AddOccupantsSession = value;
            }
            Session["AddOccupantsSession"] = null;
            return View(model);
        }

        [DecryptParameter]
        public ActionResult UploadOccupantsDoc(int? Id, string IsManage)
        {
            if (Id == null) throw new Exception("Invalid Application");
           
            Initialise();
            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
            var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
            Session["IsManage"] = IsManage;
            return RedirectToAction("AddOccupants", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(
                        new
                        {
                            referenceId = Customer.Id,
                            customerId = Customer.Id,
                            referenceTypeId = referenceType.Id,
                            applicationId = application.Id,
                            agentId = application.Id,
                            returnUrl = "",
                            rcsappId = Id
                        })));
        }

        public ActionResult ReturnToView()
        {
            return View();
        }


        public ActionResult LeaseManageTenants()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var CustomerId = Customer.Id;
                    var Apps = db.PropertyLeaseApplications.Where(x => x.CustomerId == CustomerId).ToList();
                    var rrq = Apps.Select(x => x.Id);
                    var rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false&&rrq.Contains(x.PropertyLeaseApplicationId) && x.StatusId != (db.Status.FirstOrDefault(r => r.Key == StatusKeys.DeactivateLeaseNewCaptured).Id)).Include(r => r.CreatedBySystemUser).Include(r => r.PurchaserType).Include(r => r.Status).Include(r => r.ModifiedBySystemUser).ToList();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        public JsonResult ActivatePerson(int? id)
        {
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ValidateDuplicates(int? Id, string IDNo)
        {
            bool resul = false;
            var findItem = db.PropertyResidents.FirstOrDefault(x => x.LeaseDetailsId == Id && x.IDNo == IDNo);
            if (findItem != null) resul = true;
            return Json(resul, JsonRequestBehavior.AllowGet);
        }

        [DecryptParameter]
        public ActionResult ApplicationUpdateTenantLeaseDetails(int? id)
        {
           

            LeaseDetails lease = new LeaseDetails();
            PropertyResident propertyResident = new PropertyResident();
            var propertyLease = db.PropertyLeaseApplications.Include(r=>r.HumanEHCOptions).Include(r=>r.PreferredComplexArea).FirstOrDefault(x => x.Id == id) ?? null;
            var findItem = db.LeaseDetails.Include(r=>r.PropertyLeaseApplication).OrderByDescending(x=>x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == id && x.IsRenewed != true) ?? null;
            var matchedUnit = db.MatchedUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLease.Id) ?? null;

            if(findItem != null)
            {
                var LeaseAgreementReview = db.LeaseReviewComments.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLease.Id && x.LeaseDetailsId == findItem.Id);
                var actionComment = db.propertyLeaseActionComments.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLease.Id);

                if (actionComment != null && !string.IsNullOrEmpty(actionComment.RejectReason) && actionComment.RejectReason.Contains("Not Supported"))
                {
                    ViewBag.LeaseAgreementReview = actionComment.RejectReason;
                }
                else if (LeaseAgreementReview != null)
                {
                    ViewBag.LeaseAgreementReview = LeaseAgreementReview.Comment;
                }
            }

            Units units = new Units();
            //UnitsEkurhuleniHousingCompany ekurhuleniHousingCompany = new UnitsEkurhuleniHousingCompany();
            ApplicationAllocatedProperty applicationAllocatedProperty = new ApplicationAllocatedProperty();
            ApplicantUnit applicantUnit = new ApplicantUnit();
            try
            {
                units = db.Units.FirstOrDefault(x => x.Id == matchedUnit.UnitsId && !x.IsDeleted) ?? null;
                //ekurhuleniHousingCompany = db.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == matchedUnit.UnitsEkurhuleniHousingCompanyId && !x.IsDeleted) ?? null;
                applicationAllocatedProperty = db.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == matchedUnit.ApplicationAllocatedPropertyId && !x.IsDeleted) ?? null;
                applicantUnit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLease.Id) ?? null;
            }
            catch (Exception e)
            {
  
            }
            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == id).FirstOrDefault();

            // Get or create PropertyLeaseAgreementMaster for banking details
            var propertyLeaseAgreementMaster = db.propertyLeaseAgreementMasters
                .FirstOrDefault(x => x.PropertyLeaseApplicationId == id) 
                ?? new PropertyLeaseAgreementMaster { PropertyLeaseApplicationId = id.Value };

            eServicesDbContext context = new eServicesDbContext();

            TenantViewModel model = new TenantViewModel
            {
                PropertyResident = propertyResident,
                Lease = findItem == null ? lease : findItem,
                Unit = units,
                ApplicantUnit = applicantUnit,
                PropertyLeaseApplication = propertyLease,
                PropertyLeaseAgreementMaster = propertyLeaseAgreementMaster,
                //UnitsEkurhuleniHousingCompany = ekurhuleniHousingCompany,
                ApplicationAllocatedProperty = applicationAllocatedProperty
            };
            double vat = applicationAllocatedProperty.MonthlyRentalAmount * 0.15;
            ViewBag.PropertyLeaseApplicationId = propertyLease.Id;
            ViewBag.UnitId = applicationAllocatedProperty.Id;
            ViewBag.TotalIncludeVat = (applicationAllocatedProperty.MonthlyRentalAmount + vat).ToString("C");
            ViewBag.VAT = (vat).ToString("C");
            ViewBag.PropertyDeposite = (applicationAllocatedProperty.RequiedDepositAmount).ToString("C");
            ViewBag.PropertyPrice = (applicationAllocatedProperty.MonthlyRentalAmount).ToString("C");
            ViewBag.EHCMonths = Convert.ToInt16(db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EHCFirstStayInMonths).Value);
            ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Key == PurchaserTypeKeys.NaturalPerson), "Id", "Name");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.DOB = propertyLease.DOB.ToString().Substring(0, 10);

            if (findItem != null)
            {
                ViewBag.Rental = findItem.RentalAmount;
                ViewBag.Water = findItem.Water;
                ViewBag.Sewer = findItem.Sewerage;
                ViewBag.Refuse = findItem.Refuse;
                ViewBag.TolatCharges = findItem.TotalMonthlyCharges;
                ViewBag.DepositRequired = findItem.DepositeAmount;
                ViewBag.DepositHeld = findItem.DepositeAmount;
            }
            else
            {
                ViewBag.Rental = applicationAllocatedProperty.MonthlyRentalAmount;
                ViewBag.Water = applicationAllocatedProperty.SpaceUnitSize;
                ViewBag.Sewer = applicationAllocatedProperty.SpaceUnitSize;
                ViewBag.Refuse = applicationAllocatedProperty.MonthlyRentalAmount;
                ViewBag.TolatCharges = applicationAllocatedProperty.SpaceUnitSize;
                ViewBag.DepositRequired = applicationAllocatedProperty.RequiedDepositAmount;
                ViewBag.DepositHeld = applicationAllocatedProperty.RequiedDepositAmount;
            }



            return View(model);

            //Documents Upload End


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplicationUpdateTenantLeaseDetails(TenantViewModel tenant, int UnitId)
        {
            PropertyLeaseApplication property = null;
            LeaseDetails lease = new LeaseDetails();
            eServicesDbContext context = new eServicesDbContext();
            property = db.PropertyLeaseApplications.Where(x => x.Id == tenant.PropertyLeaseApplication.Id).FirstOrDefault();
            lease = tenant.Lease;
            using (var es =new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var AppSettings = es.AppSettings;
                    AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.PLMSequence);
                    var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.PLMSequenceLimit);
                    var BatchCounter = query.Value;


                    lease.leaseApplicationRef = tenant.PropertyLeaseApplication.ApplicationReferenceNumber;
                    lease.LeaseReferenceNo = tenant.PropertyLeaseApplication.ApplicationReferenceNumber /*"PLM-LE" + DateTime.Now.ToString("ddMMyy") + BatchCounter*/;
                    int limiter = 0;
                    limiter = Convert.ToInt16(SeqLimit.Value);

                    var ThisUnit = es.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == UnitId);
                    lease.PurchaserTypeId = tenant.Lease.PurchaserTypeId;
                    lease.buildingName = tenant.ApplicationAllocatedProperty.BuildingName;
                    lease.SpaceUnitNo = tenant.ApplicationAllocatedProperty.SpaceUnitNumber;//To be a number, I dunoif it wss captured or not

                    lease.LeaAddress = tenant.ApplicationAllocatedProperty.Address; //Address
                    lease.LeaPostal = tenant.ApplicationAllocatedProperty.Postal; //Address
                    lease.LeaSuburb = tenant.ApplicationAllocatedProperty.Township; //surburb
                                                           //Geo - location = !!

                    var str = lease.StoreRooms == 0 || lease.StoreRooms == null ? lease.STR = false : lease.STR = true;
                    var spp = lease.ShadePortParking == 0 || lease.ShadePortParking == null ? lease.SPP = false : lease.SPP = true;
                    var opp = lease.OpenParking == 0 || lease.OpenParking == null ? lease.OPP = false : lease.OPP = true;
                    var elc = lease.Electricity == 0 || lease.Electricity == null ? lease.ELEC = false : lease.ELEC = true;
                    var wtr = lease.Water == 0 || lease.Water == null ? lease.WTR = false : lease.WTR = true;
                    var sec = lease.SecurityFee == 0 || lease.SecurityFee == null ? lease.SEC = false : lease.SEC = true;

                    lease.StartDate = tenant.Lease.StartDate;
                    lease.PeriodInMonths = 36;
                    lease.EndDate = tenant.Lease.EndDate;
                    lease.RenewalNotice = tenant.Lease.RenewalNotice;
                    lease.TerminationNotice = tenant.Lease.TerminationNotice;
                    lease.DepositeAmount = (int)ThisUnit.RequiedDepositAmount;
                    lease.RentalAmount = (int)ThisUnit.MonthlyRentalAmount;
                    lease.VATAmount = (int)((double)ThisUnit.MonthlyRentalAmount * 0.15);
                    lease.TotalIncludingVAT = (lease.RentalAmount + lease.VATAmount);
                    lease.Email = tenant.Lease.Email;
                    lease.SMS = tenant.Lease.SMS;
                    lease.Postal = tenant.Lease.Postal;
                    lease.PropertyLeaseApplicationId = property.Id;
                    lease.StatusId = es.Status.Where(x => x.Key == StatusKeys.ActiveLease).ToList().FirstOrDefault().Id;
                    lease.IsNew = true;
                    lease.DetailsUpdated = true;

                    var appToUpdate = es.PropertyLeaseApplications.FirstOrDefault(x => x.Id == property.Id);
                    if (appToUpdate != null)
                    {
                        appToUpdate.HasDSTV = tenant.PropertyLeaseApplication.HasDSTV;
                        appToUpdate.DSTVActivationFee = tenant.PropertyLeaseApplication.DSTVActivationFee;
                        appToUpdate.DSTVMonthlyLevy = tenant.PropertyLeaseApplication.DSTVMonthlyLevy;
                        appToUpdate.AccessCardDeposit = tenant.PropertyLeaseApplication.AccessCardDeposit;
                        appToUpdate.KeyDeposit = tenant.PropertyLeaseApplication.KeyDeposit;
                        es.Entry(appToUpdate).State = EntityState.Modified;
                    }

                    es.SaveChanges();


                    if (lease.PurchaserTypeId == (db.PurchaserType.Where(x=>x.Key==PurchaserTypeKeys.NaturalPerson).FirstOrDefault().Id))
                    {
                        property.ResAddress = tenant.PropertyLeaseApplication.ResAddress;
                        property.ResSuburb = tenant.PropertyLeaseApplication.ResSuburb;
                        property.ResPostal = tenant.PropertyLeaseApplication.ResPostal;

                        lease.FirstNames = tenant.Lease.FirstNames;
                        lease.LastName = tenant.Lease.LastName;
                        lease.IDNo = tenant.Lease.IDNo;
                        lease.TypeOfActivities = "N/A";
                        lease.ComplianceDetails = "N/A";
                    }
                    else if (lease.PurchaserTypeId == (db.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.Company).FirstOrDefault().Id))
                    {
                        property.ContactPName = tenant.PropertyLeaseApplication.ContactPName;
                        property.ContactPSurname = tenant.PropertyLeaseApplication.ContactPSurname;
                        property.Capacity = tenant.PropertyLeaseApplication.Capacity;
                        property.CPIdNumber = tenant.PropertyLeaseApplication.CPIdNumber;

                        property.CPCellNo = tenant.PropertyLeaseApplication.CPCellNo;
                        property.CPWorkNo = tenant.PropertyLeaseApplication.CPWorkNo;
                        property.CPHomeNo = tenant.PropertyLeaseApplication.CPHomeNo;
                        property.CPEmail1 = tenant.PropertyLeaseApplication.CPEmail1;
                        property.CPEmail2 = tenant.PropertyLeaseApplication.CPEmail2;

                        lease.TypeOfActivities = tenant.Lease.TypeOfActivities;
                        lease.ComplianceDetails = tenant.Lease.ComplianceDetails;
                        lease.FirstNames = "N/A";
                        lease.LastName = "N/A";
                        lease.IDNo = "N/A";

                    }
                      var findItem = db.LeaseDetails
        .Include(x => x.PropertyLeaseApplication)
        .Include(x => x.Status)
        .Include(x => x.PurchaserType)
        .Include(x => x.SystemUser)
        .OrderByDescending(x => x.Id)
        .FirstOrDefault(x => x.PropertyLeaseApplicationId == property.Id && x.IsRenewed == false);
    

                    MatchingHelper.ChangeApplicationStatus(es, es.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingLeaseAgreement).Id, property.Id);

                    if (findItem != null)
                    {
                        lease.Id = findItem.Id;
                        //lease.PropertyLeaseApplication = findItem.PropertyLeaseApplication;
                        //lease.PropertyLeaseApplicationId = findItem.PropertyLeaseApplicationId;
                        //lease.Status = findItem.Status;
                        //lease.StatusId = findItem.StatusId;
                        //lease = findItem;  // This line should be removed or changed.
                    }
                    var result = findItem == null ? MatchingHelper.SaveLeaseDetaisInfo(es, lease) : MatchingHelper.SaveLeaseDetaisInfo(es, lease);

                    // FIX: Create/update PropertyLeaseAgreementMaster AFTER lease save
                    // so that LeaseDetailsId is available. This prevents a duplicate record
                    // being created later by MatchingHelper.PropertyLeaseMasterData which
                    // queries by BOTH PropertyLeaseApplicationId AND LeaseDetailsId.
                    var existingMaster = es.propertyLeaseAgreementMasters
                        .FirstOrDefault(x => x.PropertyLeaseApplicationId == property.Id && x.LeaseDetailsId == lease.Id);
                    // Fallback: also check for orphan record with no LeaseDetailsId
                    if (existingMaster == null)
                    {
                        existingMaster = es.propertyLeaseAgreementMasters
                            .FirstOrDefault(x => x.PropertyLeaseApplicationId == property.Id && (x.LeaseDetailsId == null || x.LeaseDetailsId == 0));
                    }

                    if (existingMaster == null)
                    {
                        // Create new PropertyLeaseAgreementMaster record with LeaseDetailsId
                        var agreementMaster = new PropertyLeaseAgreementMaster
                        {
                            PropertyLeaseApplicationId = property.Id,
                            LeaseDetailsId = lease.Id,
                            HasDSTV = tenant.PropertyLeaseApplication.HasDSTV,
                            DSTVActivationFee = tenant.PropertyLeaseApplication.DSTVActivationFee ?? 0,
                            DSTVMonthlyLevy = tenant.PropertyLeaseApplication.DSTVMonthlyLevy ?? 0,
                            AccessCardDeposit = tenant.PropertyLeaseApplication.AccessCardDeposit ?? 0,
                            KeyDeposit = tenant.PropertyLeaseApplication.KeyDeposit ?? 0,
                            CommencementDay = tenant.Lease.StartDate?.ToString("dd"),
                            CreatedDateTime = DateTime.Now,
                            CreatedBySystemUserId = SystemUser.Id
                        };

                        es.propertyLeaseAgreementMasters.Add(agreementMaster);
                    }
                    else
                    {
                        // Update existing PropertyLeaseAgreementMaster record
                        existingMaster.LeaseDetailsId = lease.Id; // ensure LeaseDetailsId is set
                        existingMaster.HasDSTV = tenant.PropertyLeaseApplication.HasDSTV;
                        existingMaster.DSTVActivationFee = tenant.PropertyLeaseApplication.DSTVActivationFee ?? 0;
                        existingMaster.DSTVMonthlyLevy = tenant.PropertyLeaseApplication.DSTVMonthlyLevy ?? 0;
                        existingMaster.AccessCardDeposit = tenant.PropertyLeaseApplication.AccessCardDeposit ?? 0;
                        existingMaster.KeyDeposit = tenant.PropertyLeaseApplication.KeyDeposit ?? 0;
                        existingMaster.CommencementDay = tenant.Lease.StartDate?.ToString("dd");
                        existingMaster.ModifiedDateTime = DateTime.Now;
                        existingMaster.ModifiedBySystemUserId = SystemUser.Id;

                        es.Entry(existingMaster).State = EntityState.Modified;
                    }
                    es.SaveChanges();

                    //var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == property.Id && x.IsRenewed == false) ?? null;

                    //MatchingHelper.ChangeApplicationStatus(es, es.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingLeaseAgreement).Id, property.Id);
                    //if(findItem != null)
                    //{
                    //    lease.Id = findItem.Id;
                    //}
                    //var result = findItem == null ? MatchingHelper.SaveLeaseDetaisInfo(es, lease) : MatchingHelper.SaveLeaseDetaisInfo(es, findItem = lease);
                    if (tenant.WaterSessionList != null && tenant.WaterSessionList != "")
                    {
                        List<PropertyResident> WaterMeterInfo = JsonConvert.DeserializeObject<List<PropertyResident>>(tenant.WaterSessionList);
                        if (WaterMeterInfo.Count > 0)
                        {
                            foreach (var item in WaterMeterInfo)
                            {
                                var TenantSequenceCounter = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.TenantSequenceCounter);
                                PropertyResident propertyT = new PropertyResident();
                                propertyT.IDNo = item.IDNo;
                                propertyT.FirstNames = item.FirstNames;
                                propertyT.LastName = item.LastName;
                                propertyT.PropertyLeaseApplicationId = tenant.PropertyLeaseApplication.Id;
                                propertyT.LeaseDetailsId = lease.Id;
                                propertyT.StatusId = es.Status.FirstOrDefault(x => x.Key == StatusKeys.ActiveOccupant).Id;
                                propertyT.OccupantReferenceNo = "LE" + lease.Id + DateTime.Now.ToString("ddMMyy") + propertyT.Id + TenantSequenceCounter.Value;

                                if (TenantSequenceCounter.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                                {
                                    TenantSequenceCounter.Value = (Convert.ToInt16(TenantSequenceCounter.Value) + 1).ToString();
                                    es.Entry(TenantSequenceCounter).State = EntityState.Modified;
                                    es.SaveChanges();
                                }
                                else
                                {
                                    TenantSequenceCounter.Value = (1).ToString();
                                    es.Entry(TenantSequenceCounter).State = EntityState.Modified;
                                    es.SaveChanges();
                                }
                                es.PropertyResidents.Add(propertyT);
                                es.SaveChanges();
                            }
                        }
                    }

                    var custmusers = es.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = es.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantCapturedLease).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(es, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                    PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CaptureLeaseDetails).FirstOrDefault();
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)property.Id, null, ResponsibilityTypeId.Id, custmusers.Id);

                    cc.EHCRoundRobin(property.Id, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);


                    var prefx = es.PropertyLeaseApplications.Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault().ApplicationReferenceNumber.Substring(0, 3);
                    if (prefx == "EHC")
                    {
                        //MatchingHelper.ChangeApplicationStatus(es, es.Status.FirstOrDefault(x => x.Key == StatusKeys.RatesRebatePropertyAccountConflict).Id, lease.PropertyLeaseApplicationId);
                        var msg = es.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantRecordUpdated).Description.ToString();
                        MatchingHelper.ActivityTrackerAudit(es, lease.PropertyLeaseApplicationId, msg, custmusers.Id);
                    }

                    var referenceType = es.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                    var application = es.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                }
                catch(Exception IO)
                {
                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }
                ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                Session["UpdateLeaseDetailsSession"] = string.Format($"Lease details captured successfully for application reference ,{property.ApplicationReferenceNumber}");
            }
            return RedirectToAction("UpdateLeaseDetails", "PropertyLeaseApplication");
        }


        [DecryptParameter]
        public ActionResult UpdateTenantLeaseAgreement(int? id)
        {


            LeaseDetails lease = new LeaseDetails();
            PropertyResident propertyResident = new PropertyResident();
            var propertyLease = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == id) ?? null;
            var matchedUnit = db.MatchedUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLease.Id) ?? null;
            Units units = new Units();
            ApplicantUnit applicantUnit = new ApplicantUnit();
            try
            {
                units = db.Units.FirstOrDefault(x => x.Id == matchedUnit.UnitsId && !x.IsDeleted) ?? null;
                applicantUnit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLease.Id) ?? null;
            }
            catch (Exception e)
            {

            }
            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == id).FirstOrDefault();
            eServicesDbContext context = new eServicesDbContext();

            TenantViewModel model = new TenantViewModel
            {
                PropertyResident = propertyResident,
                Lease = lease,
                Unit = units,
                ApplicantUnit = applicantUnit,
                PropertyLeaseApplication = propertyLease
            };
            double vat = units.PropertyPrice * 0.15;
            ViewBag.PropertyLeaseApplicationId = propertyLease.Id;
            ViewBag.UnitId = units.Id;
            ViewBag.TotalIncludeVat = (units.PropertyPrice + vat).ToString("C");
            ViewBag.VAT = (vat).ToString("C");
            ViewBag.PropertyDeposite = (units.PropertyDeposit).ToString("C");
            ViewBag.PropertyPrice = (units.PropertyPrice).ToString("C");
            ViewBag.EHCMonths = Convert.ToInt16(db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EHCFirstStayInMonths).Value);
            ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && (x.Id == 5 || x.Id == 1)), "Id", "Name");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");

            return View(model);

            //Documents Upload End


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateTenantLeaseAgreement(TenantViewModel tenant, int UnitId)
        {
            PropertyLeaseApplication property = null;
            LeaseDetails lease = new LeaseDetails();
            eServicesDbContext context = new eServicesDbContext();
            property = db.PropertyLeaseApplications.Where(x => x.Id == tenant.PropertyLeaseApplication.Id).FirstOrDefault();

            lease = tenant.Lease;
            using (var es = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var AppSettings = es.AppSettings;
                    AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.PLMSequence);
                    var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.PLMSequenceLimit);
                    var BatchCounter = query.Value;


                    lease.leaseApplicationRef = tenant.PropertyLeaseApplication.ApplicationReferenceNumber;
                    lease.LeaseReferenceNo = tenant.PropertyLeaseApplication.ApplicationReferenceNumber /*"PLM-LE" + DateTime.Now.ToString("ddMMyy") + BatchCounter*/;
                    int limiter = 0;
                    limiter = Convert.ToInt16(SeqLimit.Value);
                 

                    var ThisUnit = es.Units.FirstOrDefault(x => x.Id == UnitId);
                    lease.PurchaserTypeId = tenant.Lease.PurchaserTypeId;
                    lease.buildingName = tenant.Unit.UnitBuildingName;
                    lease.SpaceUnitNo = tenant.Unit.UnitBuildingName;//To be a number, I dunoif it wss captured or not

                    lease.LeaAddress = tenant.Unit.Address; //Address
                    lease.LeaPostal = tenant.Unit.Postal; //Address
                    lease.LeaSuburb = tenant.Unit.Surburb; //surburb
                                                           //Geo - location = !!!

                    lease.StartDate = tenant.Lease.StartDate;
                    lease.PeriodInMonths = 12;
                    lease.EndDate = tenant.Lease.EndDate;
                    lease.RenewalNotice = tenant.Lease.RenewalNotice;
                    lease.TerminationNotice = tenant.Lease.TerminationNotice;
                    lease.DepositeAmount = (int)ThisUnit.PropertyDeposit;
                    lease.RentalAmount = (int)ThisUnit.PropertyPrice;
                    lease.VATAmount = (int)((double)ThisUnit.PropertyPrice * 0.15);
                    lease.TotalIncludingVAT = (lease.RentalAmount + lease.VATAmount);
                    lease.Email = tenant.Lease.Email;
                    lease.SMS = tenant.Lease.SMS;
                    lease.Postal = tenant.Lease.Postal;
                    lease.PropertyLeaseApplicationId = property.Id;
                    lease.StatusId = es.Status.Where(x => x.Key == StatusKeys.ActiveLease).ToList().FirstOrDefault().Id;
                    lease.IsNew = true;

                   


                    if (lease.PurchaserTypeId == (db.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.NaturalPerson).FirstOrDefault().Id))
                    {
                        property.ResAddress = tenant.PropertyLeaseApplication.ResAddress;
                        property.ResSuburb = tenant.PropertyLeaseApplication.ResSuburb;
                        property.ResPostal = tenant.PropertyLeaseApplication.ResPostal;

                        lease.FirstNames = tenant.Lease.FirstNames;
                        lease.LastName = tenant.Lease.LastName;
                        lease.IDNo = tenant.Lease.IDNo;
                        lease.TypeOfActivities = "N/A";
                        lease.ComplianceDetails = "N/A";
                    }
                    else if (lease.PurchaserTypeId == (db.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.Company).FirstOrDefault().Id))
                    {
                        property.ContactPName = tenant.PropertyLeaseApplication.ContactPName;
                        property.ContactPSurname = tenant.PropertyLeaseApplication.ContactPSurname;
                        property.Capacity = tenant.PropertyLeaseApplication.Capacity;
                        property.CPIdNumber = tenant.PropertyLeaseApplication.CPIdNumber;

                        property.CPCellNo = tenant.PropertyLeaseApplication.CPCellNo;
                        property.CPWorkNo = tenant.PropertyLeaseApplication.CPWorkNo;
                        property.CPHomeNo = tenant.PropertyLeaseApplication.CPHomeNo;
                        property.CPEmail1 = tenant.PropertyLeaseApplication.CPEmail1;
                        property.CPEmail2 = tenant.PropertyLeaseApplication.CPEmail2;

                        lease.TypeOfActivities = tenant.Lease.TypeOfActivities;
                        lease.ComplianceDetails = tenant.Lease.ComplianceDetails;
                        lease.FirstNames = "N/A";
                        lease.LastName = "N/A";
                        lease.IDNo = "N/A";

                    }

                    MatchingHelper.ChangeApplicationStatus(es, es.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantDocuments).Id, property.Id);
                    MatchingHelper.SaveLeaseDetaisInfo(es, lease);
                    if (tenant.WaterSessionList != null && tenant.WaterSessionList != "")
                    {
                        List<PropertyResident> WaterMeterInfo = JsonConvert.DeserializeObject<List<PropertyResident>>(tenant.WaterSessionList);
                        if (WaterMeterInfo.Count > 0)
                        {
                            foreach (var item in WaterMeterInfo)
                            {
                                var TenantSequenceCounter = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.TenantSequenceCounter);
                                PropertyResident propertyT = new PropertyResident();
                                propertyT.IDNo = item.IDNo;
                                propertyT.FirstNames = item.FirstNames;
                                propertyT.LastName = item.LastName;
                                propertyT.PropertyLeaseApplicationId = tenant.PropertyLeaseApplication.Id;
                                propertyT.LeaseDetailsId = lease.Id;
                                propertyT.StatusId = es.Status.FirstOrDefault(x => x.Key == StatusKeys.ActiveOccupant).Id;
                                propertyT.OccupantReferenceNo = "LE" + lease.Id + DateTime.Now.ToString("ddMMyy") + propertyT.Id + TenantSequenceCounter.Value;

                                if (TenantSequenceCounter.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                                {
                                    TenantSequenceCounter.Value = (Convert.ToInt16(TenantSequenceCounter.Value) + 1).ToString();
                                    es.Entry(TenantSequenceCounter).State = EntityState.Modified;
                                    es.SaveChanges();
                                }
                                else
                                {
                                    TenantSequenceCounter.Value = (1).ToString();
                                    es.Entry(TenantSequenceCounter).State = EntityState.Modified;
                                    es.SaveChanges();
                                }
                                es.PropertyResidents.Add(propertyT);
                                es.SaveChanges();
                            }
                        }
                    }

                    var custmusers = es.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = es.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantCapturedLease).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(es, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                    var prefx = es.PropertyLeaseApplications.Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault().ApplicationReferenceNumber.Substring(0, 3);
                    if (prefx == "EHC")
                    {
                        var msg = es.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantRecordUpdated).Description.ToString();
                        MatchingHelper.ActivityTrackerAudit(es, lease.PropertyLeaseApplicationId, msg, custmusers.Id);
                    }

                    var referenceType = es.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                    var application = es.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                }
                catch (Exception e)
                { }
                ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
            }
            return RedirectToAction("Inbox", "PropertyLeaseApplication");
        }



        #region Tenant Risk Assessment
        public ActionResult TenantLeaseApplications()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    //MatchingHelper.MatchUnitParallelProcessor(db);

                    var rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false && x.StatusId == db.Status.Where(b => b.Key == StatusKeys.AwaitingRiskAssessment).FirstOrDefault().Id).Include(r => r.CreatedBySystemUser).Include(r=>r.PurchaserType).Include(r => r.ModifiedBySystemUser).Include(r => r.Status).ToList();


                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));

                    }
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        #endregion

        #region Lease Renewal OnLoad
        [DecryptParameter]
        public ActionResult TenantLeaseRenewal(int? id)
        {


            LeaseDetails lease = null;
            PropertyResident propertyResident = new PropertyResident();
            var propertyLease = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == id) ?? null;
            var matchedUnit = db.MatchedUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLease.Id) ?? null;
            Units units = new Units();
            ApplicantUnit applicantUnit = new ApplicantUnit();
            try
            {
                units = db.Units.FirstOrDefault(x => x.Id == matchedUnit.UnitsId && !x.IsDeleted) ?? null;
                applicantUnit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLease.Id) ?? null;


            }
            catch (Exception e)
            {


            }

            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == id).FirstOrDefault();
            eServicesDbContext context = new eServicesDbContext();

            lease = db.LeaseDetails.Where(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.StatusId != (db.Status.Where(r=>r.Key== StatusKeys.DeactivateLeaseNewCaptured).FirstOrDefault().Id))
                .Include(r=>r.CreatedBySystemUser)
                .Include(r=>r.PurchaserType)
                .Include(r=>r.Status).FirstOrDefault();

            if (lease.PurchaserTypeId==db.PurchaserType.Where(x=>x.Key==PurchaserTypeKeys.NaturalPerson).FirstOrDefault().Id)
            {
                ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && (x.Id == lease.PurchaserTypeId)), "Id", "Name");
            }
            else
            {
                ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && (x.Id == lease.PurchaserTypeId)), "Id", "Name");
            }

            ViewBag.Deposite = units.PropertyDeposit.ToString("C");
            ViewBag.Vat = (units.PropertyPrice*0.15).ToString("C");
            ViewBag.Total = ((units.PropertyPrice * 0.15) +units.PropertyPrice).ToString("C");
            ViewBag.rent = units.PropertyPrice.ToString("C");


            //Documents Upload Start
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


            var ApplicationStage = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;


            var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.PropertyLeaseApplicationId == id).ToList();
            var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
            var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                            .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                        && d.IsActive
                        && d.IsDeleted == false && d.PropertyLeaseApplicationId == id).ToList();

            foreach (var doc in customerDocuments)
            {
                doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

            }

            var docdets = db.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading);
            var attachments = db.Attachments.Where(x => x.PropertyLeaseApplicationId == id && x.DocumentTypeId == docdets.Id).ToList();


            var vm = new DepartmentsApprovalViewModel
            {
                Attachments = attachments,
                PropertyLeaseApplications = rcsApps,
                DocName = docdets.Name,
                DocDesc = docdets.Description


            };

            Entity entity = null;
            Agent agent = null;



            vm.Customer = customer;


            //var dd = paymentDetails.PaymentDetailsList.OrderByDescending(x => x.ConvertedDate);

            vm.PropertyLeaseApplications = rcsApps;

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

            //start
            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

            if (referenceType == null) throw new Exception("Invalid reference type.");


            // Always defaults ID document to application. 
            var documentCheckLists2 = new List<DocumentCheckList>();

            //var idDocumentType = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

            //documentCheckLists2.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
            //required
            var AuthorityToActAttorney = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

            documentCheckLists2.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceType.Id));


            //var ProofOfProperty = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

            //documentCheckLists2.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceType.Id));

            //required
            var MunicipalStatement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

            var MunicipalCheckList = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceType.Id);

            documentCheckLists2.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceType.Id));


            //required
            var BankConfirmationLetter = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankConfirmationLetter);

            documentCheckLists2.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankConfirmationLetter.Id && dcl.ReferenceTypeId == referenceType.Id));

            //
            var DeedSearch = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DeedSearch);

            documentCheckLists2.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceType.Id));
            //required
            //this code will change
            var SellerID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

            documentCheckLists2.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceType.Id));
            //required
            var PurchaserID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

            documentCheckLists2.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceType.Id));




            var authorityDocumentType = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Authority);


            // If there are additional documents need for an application.
            var addDocumentType = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                //ReturnUrl = returnUrl,
                // CustomerId = (agentId == null) ? customerId : referenceId,
                CustomerId = customer.Id,
                ApplicationId = (int)application.Id,
                Application = application,
                PropertyLeaseApplicationId = rcsApps.Id,
                RcsApplicationId = rcsApps.Id,
                ReferenceTypeId = (int)referenceType.Id,
                ReferenceType = referenceType,
                ReferenceId = customer.Id,
                IsUploadView = true,
                Documents = db.Documents.Include(o => o.File).Where(o => o.ReferenceId == customer.Id && o.ReferenceTypeId == referenceType.Id && o.PropertyLeaseApplicationId == rcsApps.Id && o.IsActive && !o.IsDeleted).ToList()
            };

            var addDoc = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            if (documentCheckLists2.All(chk => chk.Id != addDoc.Id))
            {
                documentCheckLists2.Add(addDoc);
            }
            //documentCheckLists2.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id && dcl.ApplicationId == applicationId));
            dvm.DocumentCheckLists = documentCheckLists2;

            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceType.Id,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }

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
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceType.Id, customer.Id, application.Id, rcsApps.Id))));
            }

            //end


            //return View(dvm);


            TenantViewModel model = new TenantViewModel
            {
                PropertyResident = propertyResident,
                Lease = lease,
                Unit = units,
                ApplicantUnit = applicantUnit,
                PropertyLeaseApplication = propertyLease
            };
            try
            {
                //ViewBag.VAT = units.PropertyPrice * 0.15;
                //ViewBag.Total = (units.PropertyPrice * 0.15) + units.PropertyPrice;
            }
            catch (Exception)
            {


            }

           
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");

            model.DocumentsViewModel = dvm;
            return View(model);

            //Documents Upload End


        }
        #endregion

        #region Lease Renewal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TenantLeaseRenewal(TenantViewModel tenant)
        {
            PropertyLeaseApplication property = new PropertyLeaseApplication();
            LeaseDetails lease = tenant.Lease;
            eServicesDbContext context = new eServicesDbContext();
            property = db.PropertyLeaseApplications.Where(x => x.ApplicationReferenceNumber == tenant.PropertyLeaseApplication.ApplicationReferenceNumber).FirstOrDefault();

            lease = tenant.Lease;
            using (var es = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var AppSettings = es.AppSettings;
                    AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.PLMSequence);
                    var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.PLMSequenceLimit);
                    var BatchCounter = query.Value;

                    lease.leaseApplicationRef = tenant.PropertyLeaseApplication.ApplicationReferenceNumber;
                    lease.LeaseReferenceNo = tenant.PropertyLeaseApplication.ApplicationReferenceNumber;
                    int limiter = 0;
                    limiter = Convert.ToInt16(SeqLimit.Value);

                    lease.PurchaserTypeId = tenant.Lease.PurchaserTypeId;
                    lease.buildingName = tenant.Unit.UnitBuildingName;
                    lease.SpaceUnitNo = tenant.Unit.UnitBuildingName;
                    lease.LeaPostal = tenant.Unit.Postal;
                    lease.LeaSuburb = tenant.Unit.Postal;
                    lease.LeaAddress = tenant.Unit.Address;
                    lease.Email = tenant.Lease.Email;
                    lease.SMS = tenant.Lease.SMS;
                    lease.Postal = tenant.Lease.Postal;
                    lease.IsNew = true;
                    lease.StatusId = db.Status.Where(x => x.Key == StatusKeys.ActiveLease).ToList().FirstOrDefault().Id;

                    if (lease.PurchaserTypeId == (db.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.NaturalPerson).ToList().FirstOrDefault().Id))
                    {
                        property.ResAddress = tenant.PropertyLeaseApplication.ResAddress;
                        property.ResSuburb = tenant.PropertyLeaseApplication.ResSuburb;
                        property.ResPostal = tenant.PropertyLeaseApplication.ResPostal;

                        lease.FirstNames = tenant.Lease.FirstNames;
                        lease.LastName = tenant.Lease.LastName;
                        lease.IDNo = tenant.Lease.IDNo;
                        lease.TypeOfActivities = "N/A";
                        lease.ComplianceDetails = "N/A";
                    }
                    else if (lease.PurchaserTypeId == (db.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.Company).ToList().FirstOrDefault().Id))
                    {
                        property.ContactPName = tenant.PropertyLeaseApplication.ContactPName;
                        property.ContactPSurname = tenant.PropertyLeaseApplication.ContactPSurname;
                        property.Capacity = tenant.PropertyLeaseApplication.Capacity;
                        property.CPIdNumber = tenant.PropertyLeaseApplication.CPIdNumber;

                        property.CPCellNo = tenant.PropertyLeaseApplication.CPCellNo;
                        property.CPWorkNo = tenant.PropertyLeaseApplication.CPWorkNo;
                        property.CPHomeNo = tenant.PropertyLeaseApplication.CPHomeNo;
                        property.CPEmail1 = tenant.PropertyLeaseApplication.CPEmail1;
                        property.CPEmail2 = tenant.PropertyLeaseApplication.CPEmail2;

                        lease.TypeOfActivities = tenant.Lease.TypeOfActivities;
                        lease.ComplianceDetails = tenant.Lease.ComplianceDetails;
                        lease.FirstNames = "N/A";
                        lease.LastName = "N/A";
                        lease.IDNo = "N/A";
                    }
                    es.LeaseDetails.Add(lease);
                    es.SaveChanges();

                    MatchingHelper.DeactivateDuplicateLeaseRecords(es);
                    var leasesOld = es.LeaseDetails.Where(x => x.PropertyLeaseApplicationId == property.Id && x.Id != lease.Id && x.StatusId != (es.Status.FirstOrDefault(r => r.Key == StatusKeys.DeactivateLeaseNewCaptured).Id)).ToList();
                    if (leasesOld.Count > 0)
                    {
                        foreach (var Item in leasesOld)
                        {
                            MatchingHelper.ChangeLeaseStatusII(es, es.Status.FirstOrDefault(x => x.Key == StatusKeys.DeactivateLeaseNewCaptured).Id, Item.Id);
                            MatchingHelper.ReferenceOldOccupants(es, es.Status.FirstOrDefault(x => x.Key == StatusKeys.DeactivateLeaseNewCaptured).Id, Item.Id, lease.Id);
                            MatchingHelper.MarkLeaseAsOld(es, Item.Id);
                        }
                    }

                    if (tenant.WaterSessionList != null && tenant.WaterSessionList != "")
                    {
                        List<PropertyResident> WaterMeterInfo = JsonConvert.DeserializeObject<List<PropertyResident>>(tenant.WaterSessionList);
                        if (WaterMeterInfo.Count > 0)
                        {
                            foreach (var item in WaterMeterInfo)
                            {
                                var TenantSequenceCounter = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.TenantSequenceCounter);
                                PropertyResident propertyT = new PropertyResident();
                                propertyT.IDNo = item.IDNo;
                                propertyT.FirstNames = item.FirstNames;
                                propertyT.LastName = item.LastName;
                                propertyT.PropertyLeaseApplicationId = tenant.PropertyLeaseApplication.Id;
                                propertyT.LeaseDetailsId = lease.Id;
                                propertyT.StatusId = es.Status.FirstOrDefault(x => x.Key == StatusKeys.ActiveOccupant).Id;
                                propertyT.OccupantReferenceNo = "LE" + lease.Id + DateTime.Now.ToString("ddMMyy") + propertyT.Id+ TenantSequenceCounter.Value;

                                if (TenantSequenceCounter.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                                {
                                    TenantSequenceCounter.Value = (Convert.ToInt16(TenantSequenceCounter.Value) + 1).ToString();
                                    context.Entry(TenantSequenceCounter).State = EntityState.Modified;
                                    context.SaveChanges();
                                }
                                else
                                {
                                    TenantSequenceCounter.Value = (1).ToString();
                                    context.Entry(TenantSequenceCounter).State = EntityState.Modified;
                                    context.SaveChanges();
                                }

                                es.PropertyResidents.Add(propertyT);
                                es.SaveChanges();
                            }
                        }
                    }

                    return RedirectToAction("Tenants");
                }
                catch (Exception e)
                {

                }
                ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");

            }


            return RedirectToAction("Tenants", "LeaseDetails");

        }
        #endregion

        #region Renewals List
        public ActionResult Renewals()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    List<LeaseDetails> rCSApplicationStatus = new List<LeaseDetails>();
                    // MatchingHelper.RenewalNotificationAtEndOfTime(cxt);
                    List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();

                    var Keys = db.Status;
                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    int UserId = Customer.Id;

                    if (User.IsInRole("Revenue Manager"))
                    {
                        var activeDirectoryOn = Convert.ToInt32(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueManager).FirstOrDefault()?.Value ?? "0");
                        // UC021: RM signs the renewal agreement — queue key is RenewalRMSign
                        var rmSignRespId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalRMSign).FirstOrDefault()?.Id ?? 0;
                        int AwaitingRenewalRMSignature = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRenewalRMSignature)?.Id ?? 0;

                        rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser)
                            .Where(x => x.ResponsibilityTypeId == rmSignRespId && x.Clerk.Id == activeDirectoryOn && x.StatusId == SubmittedId).ToList();
                        var list = rrq.Where(x => x.LeaseDetailsId.HasValue).Select(x => x.LeaseDetailsId.Value).ToList();

                        rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false && list.Contains(x.Id) && x.StatusId == AwaitingRenewalRMSignature)
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Status)
                            .Include(r => r.ModifiedBySystemUser).ToList();
                    }
                    else if (User.IsInRole("Property Manager"))
                    {
                        var activeDirectoryOn = Convert.ToInt32(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault()?.Value ?? "0");
                        // UC021: CEO/Director signs the renewal agreement — queue key is RenewalCEOSign
                        var ceoSignRespId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalCEOSign).FirstOrDefault()?.Id ?? 0;
                        int AwaitingRenewalCEOSignature = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRenewalCEOSignature)?.Id ?? 0;

                        rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser)
                            .Where(x => x.ResponsibilityTypeId == ceoSignRespId && x.Clerk.Id == activeDirectoryOn && x.StatusId == SubmittedId).ToList();
                        var allLeaseIds = rrq.Where(x => x.LeaseDetailsId.HasValue).Select(x => x.LeaseDetailsId.Value).ToList();

                        rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false && allLeaseIds.Contains(x.Id) && x.StatusId == AwaitingRenewalCEOSignature)
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Status)
                            .Include(r => r.ModifiedBySystemUser).ToList();
                    }
                    else if ((User.IsInRole("Lease Official")) || (User.IsInRole("Letting Officer")) || (User.IsInRole("Client Services Officer")))
                    {
                        var CustomerId = Customer.Id;
                        // UC021: CSO manages the renewal queue — key is RenewalTenantSign
                        var tenantSignRespId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalTenantSign).FirstOrDefault()?.Id ?? 0;
                        // Also include old LeaseRenewals key so pre-existing queues still appear
                        var legacyRespId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseRenewals).FirstOrDefault()?.Id ?? 0;

                        int ApplicationUpForRenewalAtThreeMonths = db.Status.FirstOrDefault(r => r.Key == StatusKeys.ApplicationUpForRenewalAtThreeMonths)?.Id ?? 0;
                        int AwaitingLeaseRenewalAgreementConclusion = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingLeaseRenewalAgreementConclusion)?.Id ?? 0;
                        int AwaitingRenewalAgreementGeneration = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRenewalAgreementGeneration)?.Id ?? 0;
                        int AwaitingRenewalTenantSignature = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRenewalTenantSignature)?.Id ?? 0;
                        int AwaitingRenewalLeaseCapture = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRenewalLeaseCapture)?.Id ?? 0;
                        int LeaseRenewalAgreementConcluded = db.Status.FirstOrDefault(r => r.Key == StatusKeys.LeaseRenewalAgreementConcluded)?.Id ?? 0;

                        rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser)
                            .Where(x => (x.ResponsibilityTypeId == tenantSignRespId || x.ResponsibilityTypeId == legacyRespId)
                                     && x.Clerk.Id == CustomerId && x.StatusId == SubmittedId).ToList();
                        var list = rrq.Where(x => x.LeaseDetailsId.HasValue).Select(x => x.LeaseDetailsId.Value).ToList();

                        rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false && list.Contains(x.Id)
                            && (x.StatusId == ApplicationUpForRenewalAtThreeMonths
                             || x.StatusId == AwaitingLeaseRenewalAgreementConclusion
                             || x.StatusId == AwaitingRenewalAgreementGeneration
                             || x.StatusId == AwaitingRenewalTenantSignature
                             || x.StatusId == AwaitingRenewalLeaseCapture
                             || x.StatusId == LeaseRenewalAgreementConcluded))
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Status)
                            .Include(r => r.ModifiedBySystemUser).ToList();
                    }
                    if (Session["LeaseRenewalSession"] != null)
                    {
                        var value = Session["LeaseRenewalSession"].ToString();
                        Session["LeaseRenewalSession"] = null;
                        ViewBag.LeaseRenewalSession = value;
                    }
                    Session["LeaseRenewalSession"] = null;
                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        #endregion



        public ActionResult TenantLeaseRenewalOffer()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    if(!User.IsInRole("Customers")) return RedirectToAction("Login", "Account");

                    int AwaitingRenewalDocuments = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRenewalDocuments).Id;
                    int AwaitingTenantAcceptance = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingTenantAcceptance).Id;
                    int AwaitingRenewalTenantSignature = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRenewalTenantSignature).Id;

                    var rCSApplicationStatus = db.LeaseDetails.Include(r=>r.PropertyLeaseApplication).Where(x => x.IsDeleted == false && x.PropertyLeaseApplication.CustomerId == Customer.Id && (x.StatusId == AwaitingRenewalDocuments || x.StatusId == AwaitingTenantAcceptance || x.StatusId == AwaitingRenewalTenantSignature))
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.Status)
                        .Include(r => r.ModifiedBySystemUser).ToList();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }



        #region Lease List
        public ActionResult LeaseList()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).Include(r => r.Status).Where(x => x.CustomerId == Customer.Id).ToList();
                    if (Session["Display"] != null)
                    {

                        if (Session["ApplicationRefNo"] != null)
                        {
                            Session["Display"] = "True";
                            var message = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.PLMSuccessMessage).FirstOrDefault();

                            if (Session["Display"].ToString() == "True")
                            {
                                ViewBag.Display = "True";
                                ViewBag.MessageTitle3 = message.Title + Session["ApplicationRefNo"];

                                ViewBag.MessageBody3 = Session["MessageBody"].ToString();
                                Session["MessageBody"] = null;
                            }
                            Session["Display"] = null;
                            Session["ApplicationRefNo"] = null;
                        }
                    }

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        #endregion

        #region Tenants List
        public ActionResult Tenants()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();                    
                    // MatchingHelper.RenewalNotificationAtEndOfTime(cxt);

                    var rmStatusId = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRenewalRMSignature)?.Id;
                    var ceoStatusId = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRenewalCEOSignature)?.Id;
                    List<LeaseDetails> rCSApplicationStatus = new List<LeaseDetails>();

                    if (User.IsInRole("Revenue Manager"))
                    {
                        var rec = db.LeaseDetails.OrderByDescending(x => x.Id)
                        .Where(x => x.IsDeleted == false && x.StatusId == rmStatusId)
                        .Include(r => r.CreatedBySystemUser).Include(r => r.PurchaserType).Include(r => r.Status).Include(r => r.ModifiedBySystemUser).ToList();
                        rCSApplicationStatus.AddRange(rec);
                    }
                    else if (User.IsInRole("CEO") || User.IsInRole("Property Manager") || User.IsInRole("Director"))
                    {
                        var rec = db.LeaseDetails.OrderByDescending(x => x.Id)
                        .Where(x => x.IsDeleted == false && x.StatusId == ceoStatusId)
                        .Include(r => r.CreatedBySystemUser).Include(r => r.PurchaserType).Include(r => r.Status).Include(r => r.ModifiedBySystemUser).ToList();
                        rCSApplicationStatus.AddRange(rec);
                    }
                    else
                    {
                        var property= db.PropertyLeaseApplications.Where(x=>x.CustomerId==CustomerId).ToList();
                        foreach (var item in property)
                        {
                            var rec = db.LeaseDetails.OrderByDescending(x => x.Id)
                            .Where(x => x.IsDeleted == false && x.StatusId != (db.Status.FirstOrDefault(r => r.Key == StatusKeys.DeactivateLeaseNewCaptured).Id) && x.PropertyLeaseApplicationId == item.Id)
                            .Include(r => r.CreatedBySystemUser).Include(r => r.PurchaserType).Include(r => r.Status).Include(r => r.ModifiedBySystemUser).ToList();
                            foreach(var item2 in rec)
                            {
                                rCSApplicationStatus.Add(item2);
                            }
                        }
                    }
                    
                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        #endregion

        public ActionResult LeaseTerminated()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    // MatchingHelper.RenewalNotificationAtEndOfTime(cxt);
                    var Keys = cxt.Status;
                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    int UserId = Customer.Id;
                    List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                    List<LeaseDetails> rCSApplicationStatus = new List<LeaseDetails>();

                    if (User.IsInRole("Housing Supervisor") || User.IsInRole("Client Services Officer") || User.IsInRole("Letting Officer"))
                    {
                        var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.VacatingConfirmation).FirstOrDefault().Id;

                        rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.StatusId == SubmittedId).ToList();

                        var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();
                        var list2 = rrq.Select(x => x.LeaseDetailsId).ToList();

                        var AwaitingRiskAssessment = cxt.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingRiskAssessment).Id;

                        var rCSApplicationStatuss = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id) && x.StatusId == AwaitingRiskAssessment)
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.Customer)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.ModifiedBySystemUser)
                            .Include(r => r.HumanEHCOptions)
                            .Include(r => r.Status).ToList();

                        int EvictionGranted = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionGranted).Id;
                        int TerminationDateIssued = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminationDateIssued).Id;
                        int TenantEvictionApproved = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.TenantEvictionApproved).Id;
                        int AwaitingVacatingConfirm = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingVacatingConfirm).Id;
                        int TerminationApproved = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminationApproved).Id;

                        rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false && list2.Contains(x.Id) && (x.StatusId == TerminationDateIssued || x.StatusId == TerminationApproved || x.StatusId == EvictionGranted || x.StatusId == AwaitingVacatingConfirm))
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Status)
                            .Include(r => r.ModifiedBySystemUser).ToList();
                    }

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    if (Session["ConfirmVacatingAppicantSession"] != null)
                    {
                        var value = Session["ConfirmVacatingAppicantSession"].ToString();
                        Session["ConfirmVacatingAppicantSession"] = null;
                        ViewBag.ConfirmVacatingAppicantSession = value;
                    }
                    Session["ConfirmVacatingAppicantSession"] = null;
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }
                return RedirectToAction("Login", "Account");
            }
        }


        [DecryptParameter]
        public ActionResult LeaseRenewalValidation(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer;

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


            PropertyLeaseApplication rcsApps = null;
            LeaseDetails lease = null;

            lease = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.ModifiedBySystemUser)
              .Include(r => r.Status)
              .Include(r => r.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();

            var AccountBal = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.IsActive && !x.IsDeleted);
            var matched = db.MatchedUnits.FirstOrDefault(x => x.Id == AccountBal.MatchedID && x.IsAccepted && !x.IsDeleted) ?? null;
            var unit = db.Units.FirstOrDefault(x => x.Id == matched.UnitsId && !x.IsDeleted && x.IsTaken && x.IsActive) ?? null;
            ViewBag.AmountDue = AccountBal.OutstandingDepopsitAmount;

            if (lease.PeriodInMonths >= 24)
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approve24Months || x.Key == RCSActionTypeKeys.Approve12Months || x.Key == RCSActionTypeKeys.NotRenew).OrderBy(x => x.Name), "Key", "Name");
            }
            else
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approve12Months || x.Key == RCSActionTypeKeys.NotRenew).OrderBy(x => x.Name), "Key", "Name");
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

                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                var ApplicationStage = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
   

                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.PropertyLeaseApplicationId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.PropertyLeaseApplicationId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var docdets = db.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading);
                var attachments = db.Attachments.Where(x => x.PropertyLeaseApplicationId == id && x.DocumentTypeId == docdets.Id).ToList();

                var referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                if (referenceType == null) throw new Exception("Invalid reference type.");
                DocumentsViewModel dvm = new DocumentsViewModel();
                MatchingHelper.DocumentRenewalLetter(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, true);


                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description,
                    DocumentsViewModel = dvm


                };

                Entity entity = null;
                Agent agent = null;
                vm.Customer = customer;
                vm.PropertyLeaseApplications = rcsApps;
                ViewBag.RejectComment = "";
                ViewBag.LeaseId = lease.Id;
                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;
                ViewBag.PurchaserTypes = db.PurchaserType;

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
                ViewBag.MigratedAppRenewalAllowed = ((rcsApps.IsMigrated && !rcsApps.IsFullyMigrated) ? "Application cannot do renewal because migrated record not completed, please complete migration process to start renewal" : null);
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
        public ActionResult LeaseRenewalValidation(int? id, string ApprovalStatusddl, string RejectComment)
        {

            using (var _conx = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var userID = Customer;
                    var Keys = _conx.Status;
                    var lease = _conx.LeaseDetails.Where(x => x.Id == id).Include(r=>r.PurchaserType).Include(r=>r.Status).FirstOrDefault();
                    var Months = 0;

                    PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                    {
                        PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId,
                        RejectReason = RejectComment,
                        LeaseRenewalValidation = true,
                        CreatedDateTime = DateTime.Now,
                        ClerkId = Customer.Id
                    };
                    _conx.propertyLeaseActionComments.Add(comments);
                    _conx.SaveChanges();
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseRenewals).FirstOrDefault();
                    PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();

                    var UserId = Customer.Id;

                    if (ApprovalStatusddl == RCSActionTypeKeys.Approve24Months)
                    {
                        MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.InAwaitingPropertyManagersReview).Id, (int)lease.Id);
                        //MatchingHelper.ChangeLeaseEndDate(_conx, 24, (int)lease.Id);
                        var custmusers = _conx.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        var ActivityTrackerMessage = _conx.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeasePassRenewalValidation).Description.ToString()+" : "+lease.LeaseReferenceNo ;
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                        int emailboodyId = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                        EmailHelper.CustomerEmailNotification(_conx, lease.PropertyLeaseApplicationId, emailboodyId);
                        Months = 24;
                    }
                    if (ApprovalStatusddl == RCSActionTypeKeys.Approve12Months)
                    {
                        MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.InAwaitingPropertyManagersReview).Id, (int)lease.Id);
                        MatchingHelper.ChangeLeaseEndDate(_conx, 12, (int)lease.Id);
                        var custmusers = _conx.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        var ActivityTrackerMessage = _conx.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeasePassRenewalValidation).Description.ToString()+" : "+lease.LeaseReferenceNo ;
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                        int emailboodyId = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                        EmailHelper.CustomerEmailNotification(_conx, lease.PropertyLeaseApplicationId, emailboodyId);
                        Months = 12;
                    }
                    if (ApprovalStatusddl == RCSActionTypeKeys.NotRenew)
                    {
                        MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.InAwaitingPropertyManagersReview).Id, (int)lease.Id);
                        var custmusers = _conx.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        var ActivityTrackerMessage = _conx.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeaseFailRenewalValidation).Description.ToString() + " : " + lease.LeaseReferenceNo;
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                        int emailboodyId = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                        EmailHelper.CustomerEmailNotification(_conx, lease.PropertyLeaseApplicationId, emailboodyId);
                    }

                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)lease.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, UserId);
                    //                                                        1      2      3      4       5     6     7       8      9      10     11    12     13     14     15     16     17     18    19     20     21     22     23   24    25    26   27                                                       
                    cc.EHCRoundRobin((int)lease.PropertyLeaseApplicationId, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, 1, false, false, 1);

                    Session["LeaseRenewalSession"] = string.Format($"Lease renewal processed successfully for application reference ,{lease.LeaseReferenceNo}");
                    var Offer = new PropertyLeaseRenewalOffer
                    {
                        PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId,
                        MonthsOffer = Months
                    };
                    if (Months != 0) _conx.PropertyLeaseRenewalOffers.Add(Offer);
                    _conx.SaveChanges();
                    return RedirectToAction("Renewals");
                }
                catch (Exception e)
                {

                    throw;
                }
            }

        }

        // ─── Phase 4 – CSO recommends for renewal (new multi-step flow) ──────────

        [DecryptParameter]
        public ActionResult RecommendForRenewal(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer;

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

            PropertyLeaseApplication rcsApps = null;
            LeaseDetails lease = null;

            lease = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.ModifiedBySystemUser)
              .Include(r => r.Status)
              .Include(r => r.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();

            var AccountBal = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.IsActive && !x.IsDeleted);
            var matched = db.MatchedUnits.FirstOrDefault(x => x.Id == AccountBal.MatchedID && x.IsAccepted && !x.IsDeleted) ?? null;
            var unit = db.Units.FirstOrDefault(x => x.Id == matched.UnitsId && !x.IsDeleted && x.IsTaken && x.IsActive) ?? null;
            ViewBag.AmountDue = AccountBal.OutstandingDepopsitAmount;

            if (lease.PeriodInMonths >= 24)
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approve24Months || x.Key == RCSActionTypeKeys.Approve12Months || x.Key == RCSActionTypeKeys.NotRenew).OrderBy(x => x.Name), "Key", "Name");
            }
            else
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approve12Months || x.Key == RCSActionTypeKeys.NotRenew).OrderBy(x => x.Name), "Key", "Name");
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

                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                var ApplicationStage = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
   

                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.PropertyLeaseApplicationId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.PropertyLeaseApplicationId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var docdets = db.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading);
                var attachments = docdets != null 
                    ? db.Attachments.Where(x => x.PropertyLeaseApplicationId == id && x.DocumentTypeId == docdets.Id).ToList()
                    : new List<Attachments>();

                var referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                DocumentsViewModel dvm = new DocumentsViewModel
                {
                    Documents = new List<Document>(),
                    DocumentCheckLists = new List<DocumentCheckList>()
                };

                if (referenceType != null && application != null)
                {
                    try
                    {
                        MatchingHelper.DocumentRenewalLetter(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, true);
                    }
                    catch (Exception dvmEx)
                    {
                        EventLogHelper.LogSystemError(dvmEx.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    }
                }


                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    DocName = docdets?.Name,
                    DocDesc = docdets?.Description,
                    DocumentsViewModel = dvm
                };

                vm.Customer = customer;
                vm.PropertyLeaseApplications = rcsApps;
                ViewBag.RejectComment = "";
                ViewBag.LeaseId = lease.Id;
                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;
                ViewBag.PurchaserTypes = db.PurchaserType;

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
                ViewBag.MigratedAppRenewalAllowed = ((rcsApps.IsMigrated && !rcsApps.IsFullyMigrated) ? "Application cannot do renewal because migrated record not completed, please complete migration process to start renewal" : null);

                // ── UC018: Load all evaluation data ──────────────────────────
                var appRef = rcsApps.ApplicationReferenceNumber;

                // Occupants / Residents
                ViewBag.Residents = context.PropertyResidents
                    .Include(r => r.Status)
                    .Where(x => x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted)
                    .ToList();

                // Complaints against this tenancy (matched by OfficialNumber = appRef)
                ViewBag.Complaints = context.TenantComplaints
                    .Include(c => c.Status)
                    .Include(c => c.ComplaintCategory)
                    .Where(c => c.OfficialNumber == appRef && c.IsActive && !c.IsDeleted)
                    .OrderByDescending(c => c.CreatedDateTime)
                    .ToList();

                // Payment Transgressions (rental arrears/disputes) — TenancyReferenceNumber or OfficialNumber
                ViewBag.Transgressions = context.PaymentTransgressions
                    .Include(p => p.Status)
                    .Include(p => p.Category)
                    .Include(p => p.Severity)
                    .Where(p => p.TenancyReferenceNumber == appRef || p.OfficialNumber == appRef)
                    .OrderByDescending(p => p.CreatedDateTime)
                    .ToList();

                // Service Requests (maintenance)
                ViewBag.ServiceRequests = context.ServiceRequests
                    .Include(s => s.Status)
                    .Include(s => s.Category)
                    .Where(s => s.CreatedByCustomerId == rcsApps.CustomerId && s.IsActive && !s.IsDeleted)
                    .OrderByDescending(s => s.CreatedDateTime)
                    .ToList();

                // Risk Assessment / ITC/TPN
                ViewBag.RiskAssessment = context.RiskAssessmentOutcomes
                    .Include(r => r.Status)
                    .Where(r => r.PropertyLeaseApplicationId == rcsApps.Id)
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefault();

                // BR21: 72-month cap check
                ViewBag.Is72MonthCapOk = MatchingHelper.IsEligibleForRenewal_72MonthCap(context, rcsApps.Id);

                // BR39: ≥95% on-time payments / no active disputes
                ViewBag.IsPaymentCompliant = MatchingHelper.IsEligibleForRenewal_PaymentCompliance(context, rcsApps.Id);

                // Lease duration in months from original start
                ViewBag.LeaseDurationMonths = lease.PeriodInMonths ?? 0;

                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult RecommendForRenewal(int? id, string ApprovalStatusddl, string RejectComment)
        {
            using (var _conx = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var lease = _conx.LeaseDetails.Include(r => r.Status).FirstOrDefault(x => x.Id == id);

                    PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                    {
                        PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId,
                        RejectReason = RejectComment,
                        LeaseRenewalValidation = true,
                        CreatedDateTime = DateTime.Now,
                        ClerkId = Customer.Id
                    };
                    _conx.propertyLeaseActionComments.Add(comments);
                    _conx.SaveChanges();

                    if (ApprovalStatusddl == RCSActionTypeKeys.NotRenew)
                    {
                        MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.LeaseRenewalRejected).Id, (int)lease.Id);
                        var atMsg = string.Format("Lease renewal not recommended by CSO: {0}", lease.LeaseReferenceNo);
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, atMsg, Customer.Id);
                        Session["LeaseRenewalSession"] = string.Format("Lease renewal declined for {0}.", lease.LeaseReferenceNo);
                        return RedirectToAction("Renewals");
                    }

                    if (!MatchingHelper.IsEligibleForRenewal_72MonthCap(_conx, (int)lease.PropertyLeaseApplicationId))
                    {
                        Session["LeaseRenewalSession"] = string.Format("Renewal not permitted - lease has exceeded the 72-month cap for {0}.", lease.LeaseReferenceNo);
                        return RedirectToAction("Renewals");
                    }

                    if (!MatchingHelper.IsEligibleForRenewal_PaymentCompliance(_conx, (int)lease.PropertyLeaseApplicationId))
                    {
                        Session["LeaseRenewalSession"] = string.Format("Renewal not permitted - active payment transgressions or complaints exist for {0}.", lease.LeaseReferenceNo);
                        return RedirectToAction("Renewals");
                    }

                    int months = ApprovalStatusddl == RCSActionTypeKeys.Approve24Months ? 24 : 12;

                    // Store proposed months and pre-calculated dates in PropertyLeaseRenewalOffer.
                    // LeaseDetails date fields are NOT changed here - they are only updated
                    // in UpdatePropertyLeaseDates() when the tenant accepts via LeaseOfferValidation.
                    LeaseRenewalService.CreateOrUpdateOffer(
                        _conx,
                        propertyLeaseApplicationId: (int)lease.PropertyLeaseApplicationId,
                        leaseId: (int)lease.Id,
                        months: months,
                        csoOutcome: ApprovalStatusddl,
                        csoComment: RejectComment,
                        csoSystemUserId: Customer.Id);

                    var leaseRenewalsResp = _conx.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.LeaseRenewals);
                    MatchingHelper.RoundRobinMarkJobAsFinished(_conx, (int)lease.PropertyLeaseApplicationId, null, leaseRenewalsResp.Id, Customer.Id);

                    MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalReviewOutcome).Id, (int)lease.Id);

                    var atMessage = string.Format("Lease renewal recommended for Revenue Manager review ({0} months): {1}", months, lease.LeaseReferenceNo);
                    MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, atMessage, Customer.Id);

                    MatchingHelper.RoundRobinRenewal(_conx, (int)lease.PropertyLeaseApplicationId, ResponsibilityTypeKeys.LeaseRenewalRevenue, (int)lease.Id, AppSettingKeys.RevenueManager);

                    Session["LeaseRenewalSession"] = string.Format("Renewal recommendation submitted for Revenue Manager review: {0}.", lease.LeaseReferenceNo);
                    return RedirectToAction("Renewals");
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        // ─── Phase 6 – Revenue Manager review ────────────────────────────────────

                private void LoadEvaluationData(eServicesDbContext context, LeaseDetails lease, PropertyLeaseApplication rcsApps)
        {
            var appRef = rcsApps.ApplicationReferenceNumber;
            ViewBag.Residents = context.PropertyResidents.Include(r => r.Status).Where(x => x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted).ToList();
            ViewBag.Complaints = context.TenantComplaints.Include(c => c.Status).Include(c => c.ComplaintCategory).Where(c => c.OfficialNumber == appRef && c.IsActive && !c.IsDeleted).OrderByDescending(c => c.CreatedDateTime).ToList();
            ViewBag.Transgressions = context.PaymentTransgressions.Include(p => p.Status).Include(p => p.Category).Include(p => p.Severity).Where(p => p.TenancyReferenceNumber == appRef || p.OfficialNumber == appRef).OrderByDescending(p => p.CreatedDateTime).ToList();
            ViewBag.ServiceRequests = context.ServiceRequests.Include(s => s.Status).Include(s => s.Category).Where(s => s.CreatedByCustomerId == rcsApps.CustomerId && s.IsActive && !s.IsDeleted).OrderByDescending(s => s.CreatedDateTime).ToList();
            ViewBag.RiskAssessment = context.RiskAssessmentOutcomes.Include(r => r.Status).Where(r => r.PropertyLeaseApplicationId == lease.PropertyLeaseApplicationId && r.IsActive && !r.IsDeleted).OrderByDescending(r => r.CreatedDateTime).FirstOrDefault();
        }

        [DecryptParameter]
        public ActionResult RevenueManagerRenewalReview(int? id)
        {
            try
            {
                eServicesDbContext context = new eServicesDbContext();
                Initialise();
                PropertyLeaseApplication rcsApps = null;
                LeaseDetails lease = null;

                lease = db.LeaseDetails.OrderByDescending(x=>x.Id).Where(x => x.IsDeleted == false && x.IsNew).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.Status)
                  .Include(r => r.PurchaserType)
                  .Where(x => x.Id == id).FirstOrDefault();

                rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                  .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

                var referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                DocumentsViewModel dvm = new DocumentsViewModel
                {
                    Documents = new List<Document>(),
                    DocumentCheckLists = new List<DocumentCheckList>()
                };

                if (referenceType != null && application != null)
                {
                    try
                    {
                        MatchingHelper.DocumentRenewalLetter(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, false);
                    }
                    catch (Exception dvmEx)
                    {
                        EventLogHelper.LogSystemError(dvmEx.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    }
                }

                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    PropertyLeaseApplications = rcsApps,
                    DocumentsViewModel = dvm
                };
                var Actions = context.RCSActionTypes.ToList();

                var p = context.propertyLeaseActionComments.Include(r => r.CreatedBySystemUser).Include(r => r.Clerk).Where(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.LeaseRenewalValidation).FirstOrDefault();
                var q = context.PropertyLeaseRenewalOffers.Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).FirstOrDefault()??null;
                var f = "";
                if (q != null)
                {
                    f = (q.MonthsOffer == 12) ? Actions.FirstOrDefault(x => x.Key == RCSActionTypeKeys.Approve12Months).Name : Actions.FirstOrDefault(x => x.Key == RCSActionTypeKeys.Approve24Months).Name;
                }
                ViewBag.OfficerName = p != null && p.Clerk != null ? p.Clerk.FirstName + " " + p.Clerk.LastName : "Unknown Officer";
                ViewBag.ActionComment = q == null ? Actions.FirstOrDefault(x=>x.Key==RCSActionTypeKeys.NotRenew).Name+ ": " + (p != null ? p.RejectReason : "") : f + ": " + (p != null ? p.RejectReason : "");
                ViewBag.DateCaptured = p != null ? p.CreatedDateTime : DateTime.Now;

                ViewBag.RejectComment = "";
                ViewBag.PropertyComment = ""; LoadEvaluationData(context, lease, rcsApps); return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult RevenueManagerRenewalReview(int? id, string ApprovalStatusddl, string RejectComment)
        {
            using (var _conx = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var lease = _conx.LeaseDetails.Include(r => r.Status).FirstOrDefault(x => x.Id == id);
                    var revResp = _conx.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.LeaseRenewalRevenue);

                    PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                    {
                        PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId,
                        RejectReason = RejectComment,
                        RevenueReviewValidation = true,
                        CreatedDateTime = DateTime.Now,
                        ClerkId = Customer.Id
                    };
                    _conx.propertyLeaseActionComments.Add(comments);
                    _conx.SaveChanges();

                    // Record RM decision on offer trail (no LeaseDetails changes)
                    LeaseRenewalService.RecordRMDecision(
                        _conx,
                        propertyLeaseApplicationId: (int)lease.PropertyLeaseApplicationId,
                        outcome: ApprovalStatusddl,
                        comment: RejectComment,
                        rmSystemUserId: Customer.Id);

                    if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                    {
                        var stAwaiting = _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalOutcome);
                        if (stAwaiting != null) MatchingHelper.ChangeLeaseStatusII(_conx, stAwaiting.Id, (int)lease.Id);

                        if (revResp != null) MatchingHelper.RoundRobinMarkJobAsFinished(_conx, (int)lease.PropertyLeaseApplicationId, null, revResp.Id, Customer.Id);

                        var atMsg = string.Format("Lease renewal approved by Revenue Manager, pending Director decision: {0}", lease.LeaseReferenceNo);
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, atMsg, Customer.Id);

                        MatchingHelper.RoundRobinRenewal(_conx, (int)lease.PropertyLeaseApplicationId, ResponsibilityTypeKeys.LeaseRenewalCEOApproval, (int)lease.Id, AppSettingKeys.PropertyManager);

                        Session["LeaseRenewalSession"] = string.Format("Renewal approved and submitted for Property Manager decision: {0}.", lease.LeaseReferenceNo);
                    }
                    else
                    {
                        var stRejected = _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.LeaseRenewalRejected);
                        if (stRejected != null) MatchingHelper.ChangeLeaseStatusII(_conx, stRejected.Id, (int)lease.Id);

                        if (revResp != null) MatchingHelper.RoundRobinMarkJobAsFinished(_conx, (int)lease.PropertyLeaseApplicationId, null, revResp.Id, Customer.Id);

                        var atMsg = string.Format("Lease renewal rejected by Revenue Manager: {0}. Reason: {1}", lease.LeaseReferenceNo, RejectComment);
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, atMsg, Customer.Id);

                        var emailboody = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement);
                        if (emailboody != null) EmailHelper.CustomerEmailNotification(_conx, (int)lease.PropertyLeaseApplicationId, emailboody.Id);

                        Session["LeaseRenewalSession"] = string.Format("Renewal rejected for {0}.", lease.LeaseReferenceNo);
                    }

                    return RedirectToAction("Renewals");
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        // ─── Phase 7 – Director/CEO decision ──────────────────────────────────────

        [DecryptParameter]
        public ActionResult CEORenewalDecision(int? id)
        {
            try
            {
                eServicesDbContext context = new eServicesDbContext();
                Initialise();
                PropertyLeaseApplication rcsApps = null;
                LeaseDetails lease = null;
                PropertyManager propertyManager = new PropertyManager();
                
                lease = db.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.IsDeleted == false && x.IsNew).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.Status)
                  .Include(r => r.PurchaserType)
                  .Where(x => x.Id == id).FirstOrDefault();

                rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                  .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                var revComment = context.propertyLeaseActionComments.Include(r => r.CreatedBySystemUser).Include(r => r.Clerk).Where(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.RevenueReviewValidation).FirstOrDefault();
                if (revComment != null)
                {
                    propertyManager.PropertyComments = revComment.RejectReason;
                    propertyManager.Approved = true;
                }
                else
                {
                    propertyManager.PropertyComments = "No Comment";
                }

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

                var referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                DocumentsViewModel dvm = new DocumentsViewModel
                {
                    Documents = new List<Document>(),
                    DocumentCheckLists = new List<DocumentCheckList>()
                };

                if (referenceType != null && application != null)
                {
                    try
                    {
                        MatchingHelper.DocumentRenewalLetter(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, false);
                    }
                    catch (Exception dvmEx)
                    {
                        EventLogHelper.LogSystemError(dvmEx.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    }
                }

                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    PropertyLeaseApplications = rcsApps,
                    PropertyManager = propertyManager,
                    DocumentsViewModel = dvm
                };

                ViewBag.Approved = "Recomendation Seconded By Revenue Manager";

                var Actions = context.RCSActionTypes.ToList();

                var p = context.propertyLeaseActionComments.Include(r => r.CreatedBySystemUser).Include(r => r.Clerk).Where(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.LeaseRenewalValidation).FirstOrDefault();
                var q = context.PropertyLeaseRenewalOffers.Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).FirstOrDefault() ?? null;
                var f = "";
                if (q != null)
                {
                    f = (q.MonthsOffer == 12) ? Actions.FirstOrDefault(x => x.Key == RCSActionTypeKeys.Approve12Months).Name : Actions.FirstOrDefault(x => x.Key == RCSActionTypeKeys.Approve24Months).Name;
                }
                
                ViewBag.OfficerName = p != null && p.Clerk != null ? p.Clerk.FirstName + " " + p.Clerk.LastName : "Unknown Officer";
                ViewBag.ActionComment = q == null ? Actions.FirstOrDefault(x => x.Key == RCSActionTypeKeys.NotRenew).Name + ": " + (p != null ? p.RejectReason : "") : f + ": " + (p != null ? p.RejectReason : "");
                ViewBag.DateCaptured = p != null ? p.CreatedDateTime : DateTime.Now;

                ViewBag.RejectComment = "";
                ViewBag.PropertyComment = ""; LoadEvaluationData(context, lease, rcsApps); return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult CEORenewalDecision(int? id, string ApprovalStatusddl, string RejectComment)
        {
            using (var _conx = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var lease = _conx.LeaseDetails.Include(r => r.Status).FirstOrDefault(x => x.Id == id);
                    var ceoResp = _conx.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.LeaseRenewalCEOApproval);

                    PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                    {
                        PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId,
                        RejectReason = RejectComment,
                        PropertyReviewValidation = true,
                        CreatedDateTime = DateTime.Now,
                        ClerkId = Customer.Id
                    };
                    _conx.propertyLeaseActionComments.Add(comments);
                    _conx.SaveChanges();

                    // Record CEO/Director decision on offer trail (no LeaseDetails changes)
                    LeaseRenewalService.RecordCEODecision(
                        _conx,
                        propertyLeaseApplicationId: (int)lease.PropertyLeaseApplicationId,
                        outcome: ApprovalStatusddl,
                        comment: RejectComment,
                        ceoSystemUserId: Customer.Id);

                    if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                    {
                        var stApproved = _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.LeaseRenewalApproved);
                        if (stApproved != null) MatchingHelper.ChangeLeaseStatusII(_conx, stApproved.Id, (int)lease.Id);

                        var stAwaiting = _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantAcceptance);
                        if (stAwaiting != null) MatchingHelper.ChangeLeaseStatusII(_conx, stAwaiting.Id, (int)lease.Id);

                        if (ceoResp != null) MatchingHelper.RoundRobinMarkJobAsFinished(_conx, (int)lease.PropertyLeaseApplicationId, null, ceoResp.Id, Customer.Id);

                        var atMsg = string.Format("Lease renewal approved by Director - awaiting tenant acceptance: {0}", lease.LeaseReferenceNo);
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, atMsg, Customer.Id);

                        var emailboody = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationUpForRenewal);
                        if (emailboody != null) EmailHelper.CustomerEmailNotification(_conx, (int)lease.PropertyLeaseApplicationId, emailboody.Id);

                        Session["LeaseRenewalSession"] = string.Format("Renewal approved - notification sent to Tenant to accept/decline: {0}.", lease.LeaseReferenceNo);
                    }
                    else
                    {
                        var stRejected = _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.LeaseRenewalRejected);
                        if (stRejected != null) MatchingHelper.ChangeLeaseStatusII(_conx, stRejected.Id, (int)lease.Id);

                        if (ceoResp != null) MatchingHelper.RoundRobinMarkJobAsFinished(_conx, (int)lease.PropertyLeaseApplicationId, null, ceoResp.Id, Customer.Id);

                        var atMsg = string.Format("Lease renewal rejected by Director: {0}. Reason: {1}", lease.LeaseReferenceNo, RejectComment);
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, atMsg, Customer.Id);

                        var emailboody = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement);
                        if (emailboody != null) EmailHelper.CustomerEmailNotification(_conx, (int)lease.PropertyLeaseApplicationId, emailboody.Id);

                        Session["LeaseRenewalSession"] = string.Format("Renewal rejected by Director for {0}.", lease.LeaseReferenceNo);
                    }

                    return RedirectToAction("Renewals");
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        // ─── Phase 8 – CSO concludes renewal agreement ────────────────────────────

        [DecryptParameter]
        public ActionResult ConcludeRenewalAgreement(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();

            var lease = db.LeaseDetails.Include(r => r.Status).Include(r => r.PurchaserType)
                .FirstOrDefault(x => x.Id == id);
            var rcsApps = db.PropertyLeaseApplications.Include(r => r.Customer).Include(r => r.Status)
                .FirstOrDefault(x => x.Id == lease.PropertyLeaseApplicationId);

            var offer = db.PropertyLeaseRenewalOffers.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);
            if (offer != null)
            {
                lease.MonthsOffered = offer.MonthsOffer;
            }

            ViewBag.LeaseId = lease.Id;
            var vm = new DepartmentsApprovalViewModel { LeaseDetails = lease, PropertyLeaseApplications = rcsApps };
            vm.Customer = context.Customers.Include(s => s.SystemUser).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
            return View(vm);
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult ConcludeRenewalAgreement(int? id, string confirm)
        {
            using (var _conx = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var lease = _conx.LeaseDetails.Include(r => r.Status).FirstOrDefault(x => x.Id == id);

                    // DO NOT call AddMonthsToDates here - that was a double-extension bug.
                    // LeaseDetails date fields are extended ONLY when the CSO concludes
                    // the offer (calls UpdatePropertyLeaseDates).

                    var offer = _conx.PropertyLeaseRenewalOffers.FirstOrDefault(x => x.PropertyLeaseApplicationId == lease.PropertyLeaseApplicationId);
                    if (offer != null)
                    {
                        MatchingHelper.UpdatePropertyLeaseDates(_conx, offer.MonthsOffer, lease.Id);
                    }

                    MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.LeaseRenewalAgreementConcluded).Id, (int)lease.Id);

                    var leaseRenewalsResp = _conx.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.LeaseRenewals);
                    MatchingHelper.RoundRobinMarkJobAsFinished(_conx, (int)lease.PropertyLeaseApplicationId, null, leaseRenewalsResp.Id, Customer.Id);

                    var atMsg = string.Format("Lease renewal agreement concluded by CSO: {0}", lease.LeaseReferenceNo);
                    MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, atMsg, Customer.Id);

                    int emailboodyId = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationUpForRenewal).Id;
                    EmailHelper.CustomerEmailNotification(_conx, (int)lease.PropertyLeaseApplicationId, emailboodyId);

                    Session["LeaseRenewalSession"] = string.Format("Renewal agreement concluded for {0}.", lease.LeaseReferenceNo);
                    return RedirectToAction("Renewals");
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        // ─── UC021 — Capture Renewal Lease Details ────────────────────────────────
        [DecryptParameter]
        public ActionResult CaptureRenewalLeaseDetails(int? id)
        {
            Initialise();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var lease = db.LeaseDetails.Include(x => x.PropertyLeaseApplication).FirstOrDefault(x => x.Id == id);
            if (lease == null) return HttpNotFound();

            var propertyLease = db.PropertyLeaseApplications.Include(r => r.HumanEHCOptions).Include(r => r.PreferredComplexArea).FirstOrDefault(x => x.Id == lease.PropertyLeaseApplicationId);
            var matchedUnit = db.MatchedUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLease.Id);
            
            Units units = new Units();
            ApplicationAllocatedProperty applicationAllocatedProperty = new ApplicationAllocatedProperty();
            ApplicantUnit applicantUnit = new ApplicantUnit();
            if (matchedUnit != null)
            {
                units = db.Units.FirstOrDefault(x => x.Id == matchedUnit.UnitsId && !x.IsDeleted);
                applicationAllocatedProperty = db.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == matchedUnit.ApplicationAllocatedPropertyId && !x.IsDeleted);
                applicantUnit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLease.Id);
            }

            var propertyLeaseAgreementMaster = db.propertyLeaseAgreementMasters
                .FirstOrDefault(x => x.PropertyLeaseApplicationId == lease.PropertyLeaseApplicationId) 
                ?? new PropertyLeaseAgreementMaster { PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId };

            // Pre-fill some defaults if not already set by offer
            var offer = db.PropertyLeaseRenewalOffers.FirstOrDefault(x => x.PropertyLeaseApplicationId == lease.PropertyLeaseApplicationId);
            int monthsOffer = offer != null ? offer.MonthsOffer : 24; // Default to 24 months (BR17)
            
            if (!lease.StartDate.HasValue) lease.StartDate = lease.EndDate?.AddDays(1) ?? DateTime.Now.Date;
            if (!lease.EndDate.HasValue && lease.StartDate.HasValue) lease.EndDate = lease.StartDate.Value.AddMonths(monthsOffer).AddDays(-1);

            TenantViewModel model = new TenantViewModel
            {
                PropertyResident = new PropertyResident(),
                Lease = lease,
                Unit = units,
                ApplicantUnit = applicantUnit,
                PropertyLeaseApplication = propertyLease,
                PropertyLeaseAgreementMaster = propertyLeaseAgreementMaster,
                ApplicationAllocatedProperty = applicationAllocatedProperty
            };

            double vat = applicationAllocatedProperty != null ? applicationAllocatedProperty.MonthlyRentalAmount * 0.15 : 0;
            ViewBag.PropertyLeaseApplicationId = propertyLease.Id;
            ViewBag.UnitId = applicationAllocatedProperty?.Id ?? 0;
            ViewBag.TotalIncludeVat = applicationAllocatedProperty != null ? (applicationAllocatedProperty.MonthlyRentalAmount + vat).ToString("C") : "R0.00";
            ViewBag.VAT = (vat).ToString("C");
            ViewBag.PropertyDeposite = applicationAllocatedProperty != null ? (applicationAllocatedProperty.RequiedDepositAmount).ToString("C") : "R0.00";
            ViewBag.PropertyPrice = applicationAllocatedProperty != null ? (applicationAllocatedProperty.MonthlyRentalAmount).ToString("C") : "R0.00";
            
            var ehcMonthsSetting = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EHCFirstStayInMonths);
            ViewBag.EHCMonths = ehcMonthsSetting != null ? Convert.ToInt16(ehcMonthsSetting.Value) : 12;
            ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Key == PurchaserTypeKeys.NaturalPerson), "Id", "Name");
            ViewBag.DOB = propertyLease.DOB.ToString().Substring(0, 10);
            
            ViewBag.Rental = lease.RentalAmount;
            ViewBag.Water = lease.Water;
            ViewBag.Sewer = lease.Sewerage;
            ViewBag.Refuse = lease.Refuse;
            ViewBag.TolatCharges = lease.TotalMonthlyCharges;
            ViewBag.DepositRequired = lease.DepositeAmount;
            ViewBag.DepositHeld = lease.DepositeAmount;

            // Store period for the view
            ViewBag.PeriodInMonths = monthsOffer;

            return View("~/Views/LeaseDetails/CaptureRenewalLeaseDetails.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CaptureRenewalLeaseDetails(TenantViewModel model)
        {
            Initialise();
            var lease = db.LeaseDetails.Include(x => x.PropertyLeaseApplication).FirstOrDefault(x => x.Id == model.Lease.Id);
            if (lease == null) return HttpNotFound();

            // 1. Update lease details
            lease.StartDate = model.Lease.StartDate;
            lease.EndDate = model.Lease.EndDate;
            lease.PeriodInMonths = model.Lease.PeriodInMonths;
            lease.RenewalNotice = model.Lease.RenewalNotice;
            lease.TerminationNotice = model.Lease.TerminationNotice;
            
            lease.RentalAmount = model.Lease.RentalAmount;
            lease.VATAmount = model.Lease.VATAmount;
            lease.TotalIncludingVAT = model.Lease.TotalIncludingVAT;
            
            lease.DepositeAmount = model.Lease.DepositeAmount;
            lease.InitialDepositPremises = (double?)model.Lease.DepositeAmount; // Sync them
            
            lease.ApplicantComment = model.Lease.ApplicantComment;
            
            // 2. Generate Master Data for the Renewal
            MatchingHelper.PropertyLeaseRenewalMasterData(db, lease, lease.PropertyLeaseApplication);

            // 3. Change Status
            MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalAgreementGeneration).Id, lease.Id);

            db.SaveChanges();

            Session["LeaseRenewalSession"] = string.Format("Renewal lease details captured successfully for {0}. Please generate the Lease Renewal Agreement now and send it to the tenant for signature.", lease.LeaseReferenceNo);
            return RedirectToAction("Renewals");
        }

        [DecryptParameter]
        public ActionResult GenerateRenewalLeaseAgreement(int rcsAppId)
        {
            var cxt = new eServicesDbContext();
            try
            {
                Initialise();
                var leaseInfo = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.HumanEHCOptions).Include(r => r.PurchaserType).Include(r => r.Status)
                  .Where(x => x.Id == rcsAppId).FirstOrDefault();

                var docdets = cxt.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading);
                var attachments = cxt.Attachments.Where(x => x.PropertyLeaseApplicationId == rcsAppId && x.DocumentTypeId == docdets.Id).ToList();

                ViewBag.ApprovalStatus = new SelectList(cxt.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

                var vm = new DepartmentsApprovalViewModel()
                {
                    Attachments = attachments,
                    PropertyLeaseApplications = leaseInfo,
                    DocName = docdets != null ? docdets.Name : "",
                    DocDesc = docdets != null ? docdets.Description : ""
                };
                
                var lease = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == leaseInfo.Id);
                ViewBag.LeaseId = lease != null ? lease.Id : (int?)null;
                
                ViewBag.PropertyLeaseAppliactionId = leaseInfo.Id;
                return View("~/Views/LeaseDetails/GenerateRenewalLeaseAgreement.cshtml", vm);
            }
            catch (Exception io)
            {
                EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }

            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DecryptParameter]
        public ActionResult GenerateRenewalLeaseAgreement(int rcsAppId, string ApprovalStatusddl, string Reason)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var leaseInfo = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.HumanEHCOptions).Include(r => r.PurchaserType).Include(r => r.Status)
                      .Where(x => x.Id == rcsAppId).FirstOrDefault();

                    var lease = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == leaseInfo.Id);

                    switch (ApprovalStatusddl)
                    {
                        case RCSActionTypeKeys.Approved:
                            // Change status to Awaiting Renewal Tenant Signature
                            MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalTenantSignature).Id, rcsAppId);

                            var sysUserId = (int?)Session["SystemUserId"] ?? 0;
                            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement).FirstOrDefault();
                            if (ResponsibilityTypeId != null)
                                MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsAppId, null, ResponsibilityTypeId.Id, sysUserId);

                            // Do not assign to Letting Officer. The Tenant will sign from their customer portal inbox.
                            
                            var custmusers = cxt.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                            var ActivityTrackerMessage = cxt.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.PropertyLeaseAgreementGenerated).Description.ToString();
                            MatchingHelper.ActivityTrackerAudit(cxt, rcsAppId, ActivityTrackerMessage, custmusers.Id);

                            // Send e-mail and SMS notification to tenant to sign renewal
                            var emailContent = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement);
                            if (emailContent != null)
                            {
                                EmailHelper.CustomerEmailNotification(cxt, (int)rcsAppId, emailContent.Id);
                            }

                            Session["LeaseRenewalSession"] = string.Format($"Renewal lease agreement generated and sent to tenant successfully for application reference {leaseInfo.ApplicationReferenceNumber}");
                            break;
                        case RCSActionTypeKeys.Rejected:
                            MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalLeaseCapture).Id, (Int32)leaseInfo.Id);
                            var ResponsibilityTypeIdReject = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement).FirstOrDefault();
                            if (ResponsibilityTypeIdReject != null)
                                MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsAppId, null, ResponsibilityTypeIdReject.Id, Customer.Id);

                            MatchingHelper.ActivityTrackerAudit(db, leaseInfo.Id, "Renewal lease agreement generation rejected, sent back to capture", Customer.Id);

                            LeaseReviewComment lrc = new LeaseReviewComment();
                            lrc.PropertyLeaseApplicationId = leaseInfo.Id;
                            lrc.RCSActionTypeId = cxt.RCSActionTypes.FirstOrDefault(x => x.Key == RCSActionTypeKeys.Rejected).Id;
                            lrc.Comment = Reason;
                            lrc.LeaseDetailsId = lease.Id;
                            cxt.LeaseReviewComments.Add(lrc);
                            cxt.SaveChanges();

                            Session["LeaseRenewalSession"] = ("Renewal agreement rejected for application with reference number: " + leaseInfo.ApplicationReferenceNumber + ", Reason: " + Reason);
                            break;
                    }

                    return RedirectToAction("Renewals");
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        [DecryptParameter]
        public void pdfGenerateRenewalLeaseAgreement(int? LeaseId)
        {
            if (LeaseId == null) throw new Exception("Invalid Lease ID.");
            Initialise();

            var lease = db.LeaseDetails.Include(x => x.PropertyLeaseApplication).FirstOrDefault(x => x.Id == LeaseId);
            if (lease == null) throw new Exception("Lease not found.");

            var application = lease.PropertyLeaseApplication;
            var master = db.propertyLeaseAgreementMasters
                .FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted);

            if (master == null) throw new Exception("Lease Agreement Master Data not found.");

            string pdfTemplate = Server.MapPath("~/PDFTemplates/Revised Lease Agreement_v2.pdf"); // Reusing same template as instructed
            string newFile = Server.MapPath($"~/Content/Lease_Agreements/Renewal_Agreement_{application.ApplicationReferenceNumber}_{DateTime.Now.Ticks}.pdf");

            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, Response.OutputStream);
            AcroFields pdfFormFields = pdfStamper.AcroFields;

            // Using existing SetFieldWithFontSize pattern
            SetFieldWithFontSize(pdfFormFields, "CommencementDate", master.CommencementDate ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "LandlordAddress", master.LandlordAddress ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "AgentName", master.RepresentedBy ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "FullNames", master.ApplicantFullName ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "IdentityNumber", master.ApplicantIdentityNumber ?? application.IDNo ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "UnitNumber", master.UnitNumber ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "UnitBlock", master.BlockNumber ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "Rent", master.MonthlyUnitRental.ToString("F2"), 9.0f);
            SetFieldWithFontSize(pdfFormFields, "Deposit", master.InitialDepositPremises.ToString("F2"), 9.0f);
            SetFieldWithFontSize(pdfFormFields, "CreditCheckFee", master.CreditCheckFee == 0 ? "N/A" : master.CreditCheckFee.ToString("F2"), 9.0f);
            SetFieldWithFontSize(pdfFormFields, "AmountRent", master.UnitRentalAmountPM.ToString("F2"), 9.0f);
            SetFieldWithFontSize(pdfFormFields, "DateEnd", master.EndDate ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "TerminationDate", master.PenaltyMonth ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "Bedrooms", master.BedRooms.ToString(), 9.0f);
            SetFieldWithFontSize(pdfFormFields, "AmountDeposit", master.InitialDepositAmonunt.ToString("F2"), 9.0f);
            SetFieldWithFontSize(pdfFormFields, "MonthRent", master.UnitRentalDate ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "IncreaseDate", master.RentalIncreaseDate ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "DayRent", master.UnitRentalDay ?? "", 9.0f);
            SetFieldWithFontSize(pdfFormFields, "RentLeaseAdminFee", master.LeaseAdministrationFee.ToString("F2"), 9.0f);
            SetFieldWithFontSize(pdfFormFields, "Sewerage", master._sewerage.ToString("F2"), 9.0f);
            SetFieldWithFontSize(pdfFormFields, "Water", master._water.ToString("F2"), 9.0f);
            SetFieldWithFontSize(pdfFormFields, "Refuse", master._refuse.ToString("F2"), 9.0f);

            // Optional Charges
            if (master.OPP == true)
            {
                SetFieldWithFontSize(pdfFormFields, "OpenParkingRent", master.OPenParkingBayRental.ToString("F2"), 9.0f);
                SetFieldWithFontSize(pdfFormFields, "OpenParkingBay", master.OPenParkingBayNumber ?? "", 9.0f);
            }
            if (master.SPP == true)
            {
                SetFieldWithFontSize(pdfFormFields, "ShadeParkingRent", master.ShadePortBayRental.ToString("F2"), 9.0f);
                SetFieldWithFontSize(pdfFormFields, "ShadeParkingBay", master.ShadePortBayNumber ?? "", 9.0f);
            }
            SetFieldWithFontSize(pdfFormFields, "DateRent", master.UnitRentalDate ?? "", 9.0f);

            // Occupants
            if (!string.IsNullOrEmpty(master.OccupantONE)) { SetFieldWithFontSize(pdfFormFields, "Persons_name1", master.OccupantONE, 9.0f); SetFieldWithFontSize(pdfFormFields, "Persons_ID1", master.OccupantONEIdentityNo ?? "", 9.0f); }
            if (!string.IsNullOrEmpty(master.OccupantTWO)) { SetFieldWithFontSize(pdfFormFields, "Persons_name2", master.OccupantTWO, 9.0f); SetFieldWithFontSize(pdfFormFields, "Persons_ID2", master.OccupantTWOIdentityNo ?? "", 9.0f); }
            if (!string.IsNullOrEmpty(master.OccupantTHREE)) { SetFieldWithFontSize(pdfFormFields, "Persons_name3", master.OccupantTHREE, 9.0f); SetFieldWithFontSize(pdfFormFields, "Persons_ID3", master.OccupantTHREEIdentityNo ?? "", 9.0f); }
            if (!string.IsNullOrEmpty(master.OccupantFOUR)) { SetFieldWithFontSize(pdfFormFields, "Persons_name4", master.OccupantFOUR, 9.0f); SetFieldWithFontSize(pdfFormFields, "Persons_ID4", master.OccupantFOURIdentityNo ?? "", 9.0f); }
            if (!string.IsNullOrEmpty(master.OccupantFIVE)) { SetFieldWithFontSize(pdfFormFields, "Persons_name5", master.OccupantFIVE, 9.0f); SetFieldWithFontSize(pdfFormFields, "Persons_ID5", master.OccupantFIVEIdentityNo ?? "", 9.0f); }

            SetFieldWithFontSize(pdfFormFields, "TotalPersons", master.PeopleAllowedOnPremises.ToString(), 9.0f);

            // Tenant Initials for T&C agreement (14 fields)
            string tenantInitials = "";
            if (application != null && !string.IsNullOrEmpty(application.FirstName) && !string.IsNullOrEmpty(application.LastName))
                tenantInitials = application.FirstName.Substring(0, 1).ToUpper() + application.LastName.Substring(0, 1).ToUpper();
            SetFieldWithFontSize(pdfFormFields, "undefined", tenantInitials, 9.0f);
            for (int i = 2; i <= 14; i++)
            {
                SetFieldWithFontSize(pdfFormFields, $"undefined_{i}", tenantInitials, 9.0f);
            }

            // Add Signatures if they exist
            if (master.RenewalTenantSigned && !string.IsNullOrEmpty(master.RenewalTenantSignature))
            {
                var base64Data = master.RenewalTenantSignature.Substring(master.RenewalTenantSignature.IndexOf(",") + 1);
                var imgBytes = Convert.FromBase64String(base64Data);
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imgBytes);
                img.ScaleAbsolute(120, 40);
                var fieldPositions = pdfFormFields.GetFieldPositions("TenantSignature");
                if (fieldPositions != null && fieldPositions.Count > 0)
                {
                    var rect = fieldPositions[0].position;
                    img.SetAbsolutePosition(rect.Left, rect.Bottom);
                    PdfContentByte overContent = pdfStamper.GetOverContent(fieldPositions[0].page);
                    overContent.AddImage(img);
                }
            }

            if (!string.IsNullOrEmpty(master.RenewalWitness1Signature))
            {
                SetFieldWithFontSize(pdfFormFields, "Witness1Name", master.RenewalWitness1Name ?? "", 9.0f);
                var base64Data = master.RenewalWitness1Signature.Substring(master.RenewalWitness1Signature.IndexOf(",") + 1);
                var imgBytes = Convert.FromBase64String(base64Data);
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imgBytes);
                img.ScaleAbsolute(120, 40);
                var fieldPositions = pdfFormFields.GetFieldPositions("Witness1Signature");
                if (fieldPositions != null && fieldPositions.Count > 0)
                {
                    var rect = fieldPositions[0].position;
                    img.SetAbsolutePosition(rect.Left, rect.Bottom);
                    PdfContentByte overContent = pdfStamper.GetOverContent(fieldPositions[0].page);
                    overContent.AddImage(img);
                }
            }

            if (master.RenewalRevenueManagerSigned && !string.IsNullOrEmpty(master.RenewalRevenueManagersSignature))
            {
                var base64Data = master.RenewalRevenueManagersSignature.Substring(master.RenewalRevenueManagersSignature.IndexOf(",") + 1);
                var imgBytes = Convert.FromBase64String(base64Data);
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imgBytes);
                img.ScaleAbsolute(120, 40);
                var fieldPositions = pdfFormFields.GetFieldPositions("RevenueManagerSignature");
                if (fieldPositions != null && fieldPositions.Count > 0)
                {
                    var rect = fieldPositions[0].position;
                    img.SetAbsolutePosition(rect.Left, rect.Bottom);
                    PdfContentByte overContent = pdfStamper.GetOverContent(fieldPositions[0].page);
                    overContent.AddImage(img);
                }
            }

            if (master.RenewalPropertyManagerSigned && !string.IsNullOrEmpty(master.RenewalPropertyManagersSignature))
            {
                var base64Data = master.RenewalPropertyManagersSignature.Substring(master.RenewalPropertyManagersSignature.IndexOf(",") + 1);
                var imgBytes = Convert.FromBase64String(base64Data);
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imgBytes);
                img.ScaleAbsolute(120, 40);
                var fieldPositions = pdfFormFields.GetFieldPositions("PropertyManagerSignature");
                if (fieldPositions != null && fieldPositions.Count > 0)
                {
                    var rect = fieldPositions[0].position;
                    img.SetAbsolutePosition(rect.Left, rect.Bottom);
                    PdfContentByte overContent = pdfStamper.GetOverContent(fieldPositions[0].page);
                    overContent.AddImage(img);
                }
            }

            pdfStamper.FormFlattening = true;
            pdfStamper.Close();
            pdfReader.Close();

            // Only transition status if this is the first time generating it
            if (lease.StatusId == db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalAgreementGeneration).Id)
            {
                MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalTenantSignature).Id, lease.Id);
                // Assign to Tenant via RoundRobin if we wanted to, but usually tenant just accesses via their portal
            }

            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename=Renewal_Agreement_{application.ApplicationReferenceNumber}.pdf");
            Response.End();
        }

        private void SetFieldWithFontSize(AcroFields fields, string fieldName, string value, float fontSize)
        {
            try
            {
                fields.SetField(fieldName, value ?? "");
                fields.SetFieldProperty(fieldName, "textsize", fontSize, null);
                fields.SetFieldProperty(fieldName, "textfont", "Helvetica", null);
                fields.RegenerateField(fieldName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set field '{fieldName}': {ex.Message}");
            }
        }

        // ─── UC021 — Renewal Tenant Signature ─────────────────────────────────────
        [DecryptParameter]
        public ActionResult RenewalLeaseAgreementValidation(int? id)
        {
            Initialise();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var lease = db.LeaseDetails.Include(x => x.PropertyLeaseApplication).FirstOrDefault(x => x.Id == id);
            if (lease == null) return HttpNotFound();

            var propertyLease = db.PropertyLeaseApplications.Include(r => r.CreatedBySystemUser)
                .Include(r => r.Customer).Include(r => r.Status).Include(r => r.PurchaserType)
                .FirstOrDefault(x => x.Id == lease.PropertyLeaseApplicationId);

            var vm = new DepartmentsApprovalViewModel()
            {
                PropertyLeaseApplications = propertyLease,
                DocName = "Lease Renewal Agreement",
                RiskAssessmentOutcome = new RiskAssessmentOutcome()
            };
            ViewBag.LeaseId = lease.Id;
            // Must be set so the view renders the Approval Status dropdown.
            // Without this, the @if(ViewBag.ApprovalStatus != null) guard hides the entire
            // dropdown — ApprovalStatusddl is never posted and the controller does nothing.
            ViewBag.ApprovalStatus = new SelectList(
                db.RCSActionTypes
                  .Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected)
                  .OrderBy(x => x.Name),
                "Key", "Name");
            return View("~/Views/LeaseDetails/RenewalLeaseAgreementValidation.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RenewalLeaseAgreementValidation(int? id, string ApprovalStatusddl, string Comment)
        {
            Initialise();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var lease = db.LeaseDetails.FirstOrDefault(x => x.Id == id);
            if (lease == null) return HttpNotFound();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalRMSignature).Id, lease.Id);
                
                int sysUserId = (int?)Session["SystemUserId"] ?? 0;
                // Mark Tenant Sign job as finished
                var tenantResp = db.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RenewalTenantSign);
                if (tenantResp != null) MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)lease.PropertyLeaseApplicationId, null, tenantResp.Id, sysUserId);

                // Assign to Revenue Manager via Round Robin
                MatchingHelper.RoundRobinRenewal(db, (int)lease.PropertyLeaseApplicationId, ResponsibilityTypeKeys.RenewalRMSign, lease.Id, AppSettingKeys.RevenueManager);

                Session["LeaseRenewalSession"] = string.Format("Renewal agreement signed by tenant for {0}.", lease.LeaseReferenceNo);
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                // Send back to CSO to capture details again or cancel
                MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalLeaseCapture).Id, lease.Id);
                MatchingHelper.AddCommentOnRejectAgreement(db, Comment, (int)lease.PropertyLeaseApplicationId);
                Session["LeaseRenewalSession"] = string.Format("Renewal agreement rejected by tenant for {0}.", lease.LeaseReferenceNo);
            }

            return RedirectToAction("Renewals");
        }

        [HttpPost]
        public JsonResult SaveRenewalTenantSignatureImage()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.InputStream)) body = reader.ReadToEnd();
                var payload = JsonConvert.DeserializeObject<dynamic>(body);
                int? LeaseId = (int?)payload.LeaseId;
                string SignatureImageData = (string)payload.SignatureImageData;

                if (LeaseId == null || string.IsNullOrEmpty(SignatureImageData)) return Json(new { success = false, message = "Invalid parameters." });
                Initialise();

                var lease = db.LeaseDetails.FirstOrDefault(x => x.Id == LeaseId);
                var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x => x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted);
                if (master == null) return Json(new { success = false, message = "Master lease not found." });

                master.RenewalTenantSignature = SignatureImageData;
                master.RenewalTenantSigned = true;
                master.RenewalTenantSignatureDate = DateTime.Now;
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public JsonResult SaveRenewalWitness1SignatureImage()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.InputStream)) body = reader.ReadToEnd();
                var payload = JsonConvert.DeserializeObject<dynamic>(body);
                int? LeaseId = (int?)payload.LeaseId;
                string SignatureImageData = (string)payload.SignatureImageData;
                string WitnessName = (string)payload.WitnessName;

                if (LeaseId == null || string.IsNullOrEmpty(SignatureImageData) || string.IsNullOrEmpty(WitnessName)) return Json(new { success = false, message = "Invalid parameters." });
                Initialise();

                var lease = db.LeaseDetails.FirstOrDefault(x => x.Id == LeaseId);
                var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x => x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted);
                if (master == null) return Json(new { success = false, message = "Master lease not found." });

                master.RenewalWitness1Signature = SignatureImageData;
                master.RenewalWitness1Name = WitnessName;
                master.RenewalWitness1SignatureDate = DateTime.Now;
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public JsonResult CheckRenewalSignatureStatus(int LeaseId)
        {
            try
            {
                Initialise();
                var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x => x.LeaseDetailsId == LeaseId && x.IsActive && !x.IsDeleted);
                if (master == null) return Json(new { success = false, message = "Master lease not found." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    tenantSigned = master.RenewalTenantSigned,
                    tenantSignatureData = master.RenewalTenantSignature,
                    witness1Signed = !string.IsNullOrEmpty(master.RenewalWitness1Signature),
                    witness1SignatureData = master.RenewalWitness1Signature,
                    witness1Name = master.RenewalWitness1Name
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        // ─── UC021 — Renewal Revenue Manager Validation ──────────────────────────
        [DecryptParameter]
        public ActionResult RenewalRevenueManagerValidation(int? id)
        {
            Initialise();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var lease = db.LeaseDetails.Include(x => x.PropertyLeaseApplication).FirstOrDefault(x => x.Id == id);
            if (lease == null) return HttpNotFound();

            var propertyLease = db.PropertyLeaseApplications.Include(r => r.CreatedBySystemUser)
                .Include(r => r.Customer).Include(r => r.Status).Include(r => r.PurchaserType)
                .FirstOrDefault(x => x.Id == lease.PropertyLeaseApplicationId);

            var vm = new DepartmentsApprovalViewModel()
            {
                PropertyLeaseApplications = propertyLease,
                DocName = "Lease Renewal Agreement",
                RiskAssessmentOutcome = new RiskAssessmentOutcome()
            };
            ViewBag.LeaseId = lease.Id;
            return View("~/Views/LeaseDetails/RenewalRevenueManagerValidation.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RenewalRevenueManagerValidation(int? id, string ApprovalStatusddl, string Comment)
        {
            Initialise();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var lease = db.LeaseDetails.FirstOrDefault(x => x.Id == id);
            if (lease == null) return HttpNotFound();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalCEOSignature).Id, lease.Id);
                int sysUserId = (int?)Session["SystemUserId"] ?? 0;
                // Mark Revenue Manager job as finished
                var rmResp = db.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RenewalRMSign);
                if (rmResp != null) MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)lease.PropertyLeaseApplicationId, null, rmResp.Id, sysUserId);

                // Assign to CEO via Round Robin
                MatchingHelper.RoundRobinRenewal(db, (int)lease.PropertyLeaseApplicationId, ResponsibilityTypeKeys.RenewalCEOSign, lease.Id, AppSettingKeys.PropertyManager);

                Session["LeaseRenewalSession"] = string.Format("Renewal agreement signed by Revenue Manager for {0}.", lease.LeaseReferenceNo);
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalLeaseCapture).Id, lease.Id);
                MatchingHelper.AddCommentOnRejectAgreement(db, Comment, (int)lease.PropertyLeaseApplicationId);
                Session["LeaseRenewalSession"] = string.Format("Renewal agreement rejected by Revenue Manager for {0}.", lease.LeaseReferenceNo);
            }

            return RedirectToAction("Renewals");
        }

        [HttpPost]
        public JsonResult SaveRenewalRevenueManagerSignatureImage()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.InputStream)) body = reader.ReadToEnd();
                var payload = JsonConvert.DeserializeObject<dynamic>(body);
                int? LeaseId = (int?)payload.LeaseId;
                string SignatureImageData = (string)payload.SignatureImageData;

                if (LeaseId == null || string.IsNullOrEmpty(SignatureImageData)) return Json(new { success = false, message = "Invalid parameters." });
                Initialise();

                var lease = db.LeaseDetails.FirstOrDefault(x => x.Id == LeaseId);
                var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x => x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted);
                if (master == null) return Json(new { success = false, message = "Master lease not found." });

                master.RenewalRevenueManagersSignature = SignatureImageData;
                master.RenewalRevenueManagerSigned = true;
                master.RenewalRevenueManagerSignatureDate = DateTime.Now;
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ─── UC021 — Renewal Property Manager (CEO) Validation ──────────────────
        [DecryptParameter]
        public ActionResult RenewalPropertyManagerValidation(int? id)
        {
            Initialise();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var lease = db.LeaseDetails.Include(x => x.PropertyLeaseApplication).FirstOrDefault(x => x.Id == id);
            if (lease == null) return HttpNotFound();

            var propertyLease = db.PropertyLeaseApplications.Include(r => r.CreatedBySystemUser)
                .Include(r => r.Customer).Include(r => r.Status).Include(r => r.PurchaserType)
                .FirstOrDefault(x => x.Id == lease.PropertyLeaseApplicationId);

            var vm = new DepartmentsApprovalViewModel()
            {
                PropertyLeaseApplications = propertyLease,
                DocName = "Lease Renewal Agreement",
                RiskAssessmentOutcome = new RiskAssessmentOutcome()
            };
            ViewBag.LeaseId = lease.Id;
            return View("~/Views/LeaseDetails/RenewalPropertyManagerValidation.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RenewalPropertyManagerValidation(int? id, string ApprovalStatusddl, string Comment)
        {
            Initialise();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var lease = db.LeaseDetails.FirstOrDefault(x => x.Id == id);
            if (lease == null) return HttpNotFound();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                // CONCLUDE THE RENEWAL
                var offer = db.PropertyLeaseRenewalOffers.FirstOrDefault(x => x.PropertyLeaseApplicationId == lease.PropertyLeaseApplicationId);
                if (offer != null)
                {
                    MatchingHelper.UpdatePropertyLeaseDates(db, offer.MonthsOffer, lease.Id);
                }

                MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.LeaseRenewalAgreementConcluded).Id, lease.Id);
                
                var ceoResp = db.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RenewalCEOSign);
                int sysUserId = (int?)Session["SystemUserId"] ?? 0;
                if (ceoResp != null) MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)lease.PropertyLeaseApplicationId, null, ceoResp.Id, sysUserId);

                Session["LeaseRenewalSession"] = string.Format("Renewal agreement fully signed and concluded for {0}.", lease.LeaseReferenceNo);
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalLeaseCapture).Id, lease.Id);
                MatchingHelper.AddCommentOnRejectAgreement(db, Comment, (int)lease.PropertyLeaseApplicationId);
                Session["LeaseRenewalSession"] = string.Format("Renewal agreement rejected by Director for {0}.", lease.LeaseReferenceNo);
            }

            return RedirectToAction("Renewals");
        }

        [HttpPost]
        public JsonResult SaveRenewalPropertyManagerSignatureImage()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.InputStream)) body = reader.ReadToEnd();
                var payload = JsonConvert.DeserializeObject<dynamic>(body);
                int? LeaseId = (int?)payload.LeaseId;
                string SignatureImageData = (string)payload.SignatureImageData;

                if (LeaseId == null || string.IsNullOrEmpty(SignatureImageData)) return Json(new { success = false, message = "Invalid parameters." });
                Initialise();

                var lease = db.LeaseDetails.FirstOrDefault(x => x.Id == LeaseId);
                var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x => x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted);
                if (master == null) return Json(new { success = false, message = "Master lease not found." });

                master.RenewalPropertyManagersSignature = SignatureImageData;
                master.RenewalPropertyManagerSigned = true;
                master.RenewalPropertyManagerSignatureDate = DateTime.Now;
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [DecryptParameter]
        public ActionResult ConductExitInspection(int? LeseId)
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


            LeaseDetails lease = null;
            LeaseTermination termination = null;
            PropertyLeaseApplication rcsApps = null;

            lease = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.ModifiedBySystemUser)
              .Include(r => r.PurchaserType).Include(r => r.Status)
              .Where(x => x.Id == LeseId).FirstOrDefault();

            termination = db.LeaseTerminations.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.ModifiedBySystemUser).Include(r => r.Status)
              .Where(x => x.LeaseDetailsId == LeseId).FirstOrDefault();

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status).Include(x => x.PurchaserType)
              .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();


            int id = rcsApps.Id;

            //rcsApps = context.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.TransferInformation).Include(d => d.Status).FirstOrDefault();

            //PaymentDetailsApi api = new PaymentDetailsApi();
            //var paymentDetails = api.GetPaymentDetails(rcsApps.TransferInformation.RatesNumber);

            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");

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


                var ApplicationStage = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;


                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.PropertyLeaseApplicationId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.PropertyLeaseApplicationId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var docdets = db.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading);
                //var attachments = db.Attachments.Where(x => x.PropertyLeaseApplicationId == id && x.DocumentTypeId == docdets.Id).ToList();
                List<Attachments> attachments = new List<Attachments>();
                var atth = db.Attachments.FirstOrDefault();
                attachments.Add(atth);

                var vm = new DepartmentsApprovalViewModel
                {
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description


                };

                Entity entity = null;
                Agent agent = null;



                vm.Customer = customer;


                //var dd = paymentDetails.PaymentDetailsList.OrderByDescending(x => x.ConvertedDate);

                vm.PropertyLeaseApplications = rcsApps;
                vm.LeaseDetails = lease;
                vm.LeaseTermination = termination;

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

                ViewBag.RejectComment = "";
                ViewBag.CustomerModel = obj;
                ViewBag.CustomerTypeId = new SelectList(context.Status.Include(x => x.StatusType).Where(x => x.StatusType.Key == StatusTypeKeys.DocumentUpload).ToList(), "Key", "Name");

                //start
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                bool IsUpload = true;
                var returnUrl = "";
                MatchingHelper.DocumentConductConductExitInspection(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, returnUrl, rcsApps.Id, IsUpload);

                DocumentsViewModel dvmTemplate = new DocumentsViewModel();
                MatchingHelper.DocumentGetConductExitInspectionTemplate(dvmTemplate, db, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, false);
                vm.DocumentsViewModelTemplate = dvmTemplate;

                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();

                if (application.Key == ApplicationKeys.RatesRebate)
                {
                    ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                    ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                    var nav = new NavigationProperty
                    {
                        CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                        PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                        RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                        AgentId = agent == null ? 0 : agent.Id,
                        ApplicationId = application.Id,
                        ReferenceTypeId = referenceType.Id,
                        ReferenceId = referenceType.Id,
                        Step = ViewCodeKeys.StepFive
                    };

                    ViewBag.NavigationParameters = nav;
                }

                vm.DocumentsViewModel = dvm;
                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult ConductExitInspection(DepartmentsApprovalViewModel vm, int? id, string ApprovalStatusddl, int? LeseId, string RejectComment)
        {
            using (var _context = new eServicesDbContext())
            {
                Initialise();
               
                var userID = Customer.Id;
                var Keys = _context.Status;
                var LeaseApplication = _context.LeaseDetails.Where(x => x.Id == LeseId && x.IsDeleted == false).FirstOrDefault();
                var TerminationRecord = _context.LeaseTerminations.Where(x => x.LeaseDetailsId == LeaseApplication.Id && !x.IsDeleted).FirstOrDefault();

                PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                {
                    PropertyLeaseApplicationId = LeaseApplication.PropertyLeaseApplicationId,
                    RejectReason = RejectComment,
                    ExitInspection = true
                };
                _context.propertyLeaseActionComments.Add(comments);
                _context.SaveChanges();

                if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                {
                    LeaseApplication.EndDate = TerminationRecord.TerminationDate;
                    LeaseApplication.TerminationNotice = TerminationRecord.TerminationDate.AddMonths(-3);
                    _context.Entry(LeaseApplication).State = EntityState.Modified;
                    _context.SaveChanges();

                    CaptureController c = new CaptureController();
                    MatchingHelper.ChangeLeaseStatusII(_context, _context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingPropertyEviction).Id, (int)LeaseApplication.Id);

                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ConductExitInspectionApprove).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(_context, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                }
                else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
                {
                    MatchingHelper.ChangeLeaseStatusII(_context, _context.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminationRejected).Id, (int)LeaseApplication.Id);

                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ConductExitInspectionReject).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(_context, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                }
                return RedirectToAction("PropertyLeaseApplicationTerminations", "PropertyLeaseApplication");
            }
        }

        #region Lease Offer Acceptance OnLoad
        [DecryptParameter]
        public ActionResult LeaseOfferValidation(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer;

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


            PropertyLeaseApplication rcsApps = null;
            LeaseDetails lease = null;

            lease = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.ModifiedBySystemUser)
              .Include(r => r.Status)
              .Include(r=>r.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();

            var offer = db.PropertyLeaseRenewalOffers.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);
            if (offer != null)
            {
                lease.MonthsOffered = offer.MonthsOffer;
            }

            if ((lease.Status.Key == StatusKeys.AwaitingRenewalDocuments) ||(lease.Status.Key == StatusKeys.TerminateAtEndOfPeriod) || (rcsApps.Status.Key == StatusKeys.TerminateAtEndOfPeriod))
            {
                Session["View"] = "TenantLeaseRenewalOffer";
                Session["Controller"] = "leaseDetails";
                return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = rcsApps.Id, appId = lease.Id });
            }

            ViewBag.Id = lease.Id;
            ViewBag.PropId = rcsApps.Id;

            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");

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

                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                var ApplicationStage = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;


                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.PropertyLeaseApplicationId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.PropertyLeaseApplicationId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var docdets = db.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading);
                var attachments = db.Attachments.Where(x => x.PropertyLeaseApplicationId == id && x.DocumentTypeId == docdets.Id).ToList();


                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description


                };

                Entity entity = null;
                Agent agent = null;



                vm.Customer = customer;


                vm.PropertyLeaseApplications = rcsApps;

                ViewBag.LeaseId = db.LeaseDetails.Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).OrderByDescending(x => x.Id).FirstOrDefault().Id;
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
        #endregion

        #region Lease Offer Acceptance
        [DecryptParameter]
        [HttpPost]
        public ActionResult LeaseOfferValidation(int? id, string ApprovalStatusddl, string RejectComment)
        {
            using(var cxt = new eServicesDbContext())
            {
                Initialise();
                var userID = Customer;
                var Keys = db.Status;
                var rcsApps = db.LeaseDetails.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();

                var pp = cxt.PropertyLeaseRenewalOffers.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.PropertyLeaseApplicationId);

                if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                {
                    if (rcsApps.PurchaserTypeId == (db.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.NaturalPerson).FirstOrDefault().Id))
                    {
                        CaptureController c = new CaptureController();
                        MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalDocuments).Id, (int)rcsApps.Id);
                        
                    }
                    else if (rcsApps.PurchaserTypeId == (db.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.Company).FirstOrDefault().Id))
                    {
                        CaptureController c = new CaptureController();
                        MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepartmentInputs).Id, (int)rcsApps.Id);
                    }
                    else
                    {
                        CaptureController c = new CaptureController();
                        MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalDocuments).Id, (int)rcsApps.Id);
                    }

                    if (pp != null)
                    {
                        MatchingHelper.AcceptRenewalOfferPeriod(cxt, pp.Id);
                    }
                    Session["LeaseOfferValidationSession"] = string.Format($"Lease offer accepted for application reference ,{rcsApps.LeaseReferenceNo}");
                    return RedirectToAction("AcceptLeaseRenewal", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("Id=" + rcsApps.Id.ToString()) });

                }
                else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
                {
                    PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                    {
                        PropertyLeaseApplicationId = rcsApps.PropertyLeaseApplicationId,
                        RejectRenewalReasonCustomer = RejectComment
                    };
                    cxt.propertyLeaseActionComments.Add(comments);
                    cxt.SaveChanges();

                    MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminateAtEndOfPeriod).Id, (int)rcsApps.Id);
                    db.SaveChanges();

                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresPOPRejected).Description.ToString();
                    ActivityTrackerMessage = "Renewal offer rejected by applicant, due to reason: "+RejectComment+".";
                    MatchingHelper.ActivityTrackerAudit(db, rcsApps.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    var em2 = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RenewalRejectByCustomer);
                    var em3 = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == "plm_renewal_reject_by_customer");

                    int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RenewalRejectByCustomer).Id;
                    var BackOffice = MatchingHelper.GetBackOfficeId(db, (int)rcsApps.PropertyLeaseApplicationId, true);
                    EmailHelper.BackOfficeNotification(cxt, rcsApps.PropertyLeaseApplicationId, BackOffice.Id, emailboodyId, RejectComment);
                }
                return RedirectToAction("Tenants");
            }
        }
        #endregion

        #region Lease Termination OnLoad
        [DecryptParameter]
        public ActionResult LeaseTerminationValidation(int? id)
        {
          
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            var userID = Customer;

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


            PropertyLeaseApplication rcsApps = null;
            LeaseDetails lease = null;

            lease = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.ModifiedBySystemUser)
              .Include(r => r.Status)
              .Include(r => r.PurchaserType)
              .Include(r => r.PropertyLeaseApplication)
              .Where(x => x.Id == id).FirstOrDefault();

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();

            ViewBag.NoticeDate = rcsApps.ServeNoticeDate;
            ViewBag.PropertyLeaseApplicationId = rcsApps.Id;

            ViewBag.Id = lease.Id;
            ViewBag.PropId = rcsApps.Id;

            if (!string.IsNullOrEmpty(lease.NoticeDate.ToString()))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.LeaseNotRenuewed || x.Key == RCSActionTypeKeys.EndOfLeasePeriod60M || x.Key == RCSActionTypeKeys.TenantNotice || x.Key == RCSActionTypeKeys.TenantDeceased).OrderBy(x => x.Name), "Key", "Name");
            }
            else
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.LeaseNotRenuewed || x.Key == RCSActionTypeKeys.EndOfLeasePeriod60M || x.Key == RCSActionTypeKeys.TenantDeceased).OrderBy(x => x.Name), "Key", "Name");
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




                DocumentsViewModel dvm = new DocumentsViewModel();

                MatchingHelper.DocumentTerminationValidation(dvm, context, customer.Id, customer.Id, documentReferenceType.Id, application.Id, "", rcsApps.Id, true);
                




                var documentCheckLists = new List<DocumentCheckList>();

                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                var ApplicationStage = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;


                var documents = context.Documents.Where(d => d.CustomerId == customer.Id && d.PropertyLeaseApplicationId == id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.PropertyLeaseApplicationId == id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }

                var docdets = db.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading);
                var attachments = db.Attachments.Where(x => x.PropertyLeaseApplicationId == id && x.DocumentTypeId == docdets.Id).ToList();


                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description,
                    PropertyLeaseActionComments = new PropertyLeaseActionComments()
                };

                Entity entity = null;
                Agent agent = null;

                vm.DocumentsViewModel = dvm;

                //getting Termination letter and prrof of banking details
                DocumentsViewModel terminationletterVm = new DocumentsViewModel();
                MatchingHelper.DocumentTerminationLetterAndProofBanking(terminationletterVm, context, customer.Id, customer.Id, (int)documentReferenceType.Id, (int)application.Id, "", rcsApps.Id, true,false);
                vm.TerminationLetterDocumentsViewModel = terminationletterVm;

                vm.Customer = customer;
                vm.PropertyLeaseApplications = rcsApps;

                LeaseTermination leaseTermination = new LeaseTermination();
                leaseTermination.TerminationDate = lease.NoticeDate !=null ? lease.NoticeDate.Value : DateTime.Now;
                vm.LeaseTermination = leaseTermination;

                ViewBag.LeaseId = db.LeaseDetails.Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).OrderByDescending(x => x.Id).FirstOrDefault().Id;
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

                var propertyLeaseActionComments = context.propertyLeaseActionComments.Include(r => r.CreatedBySystemUser).Include(r => r.Clerk).Where(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.LeaseTerminationLetter).OrderByDescending(p=>p.CreatedDateTime).FirstOrDefault();
                ViewBag.ReasonComments = propertyLeaseActionComments != null ? propertyLeaseActionComments.RejectReason : null;
                ViewBag.TerminationAppRenewalAllowed = ((rcsApps.IsMigrated && !rcsApps.IsFullyMigrated) ? "Application cannot be terminated because migrated record not completed, please complete migration process to start termination" : null);
                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View(rcsApps);
        }
        #endregion

        public JsonResult validateTenantNotice(int Id)
        {
            var p = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == Id);
            bool result = p.ServeNoticeDate != null ? true : false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Lease Termination
        [DecryptParameter]
        [HttpPost]
        public ActionResult LeaseTerminationValidation(DepartmentsApprovalViewModel vm, int? id, string ApprovalStatusddl)
        {

            using (var _context = new eServicesDbContext())
            {
                Initialise();
                var userID = Customer;
                var Keys = _context.Status;
                var LeaseApplication = _context.LeaseDetails.Include(r=>r.PropertyLeaseApplication).Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
                var LeaseId = vm.LeaseDetails.Id;
                
                var awaitingAppraisalStatusId = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTerminationAppraisal).Id;

                LeaseTermination termination = new LeaseTermination();
                termination.LeaseDetailsId = LeaseId;
                termination.LeaseReferenceNumber = vm.LeaseDetails.LeaseReferenceNo;
                termination.PropertyLeaseApplicationId = LeaseApplication.PropertyLeaseApplicationId;
                termination.TerminationDate = vm.LeaseTermination.TerminationDate;
                termination.StatusId = awaitingAppraisalStatusId;

                // Capture expanded CSO fields
                termination.ClauseReference = Request.Form["ClauseReference"];
                termination.NoticePeriod = Request.Form["NoticePeriod"];
                termination.OfficialNumber = Request.Form["OfficialNumber"];
                termination.TerminationReferenceNumber = MatchingHelper.GenerateTerminationReference(_context);

                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Terminations).FirstOrDefault(); 

                var User2 = MatchingHelper.GetBackOfficeId(db, (int)LeaseApplication.PropertyLeaseApplicationId, true);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var UserId = User2.Id != 0 ? User2.Id : activeDirectoryOn;
                PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();

                if (ApprovalStatusddl == RCSActionTypeKeys.TenantNotice)
                {
                    termination.ReasonForTermination = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.TenantNotice).Description;
                    MatchingHelper.ChangeLeaseStatusII(_context, awaitingAppraisalStatusId, LeaseId);
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)LeaseApplication.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, UserId);
                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantNotice).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(_context, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                }

                if (ApprovalStatusddl == RCSActionTypeKeys.LeaseNotRenuewed)
                {
                    MatchingHelper.ChangeLeaseStatusII(_context, awaitingAppraisalStatusId, LeaseId);
                    termination.ReasonForTermination = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.LeaseNotRenewed).Description;
                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.LeaseNotRenewed).Description.ToString() ;
                    MatchingHelper.ActivityTrackerAudit(_context, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                }

                if (ApprovalStatusddl == RCSActionTypeKeys.TenantDeceased)
                {
                    MatchingHelper.ChangeLeaseStatusII(_context, awaitingAppraisalStatusId, LeaseId);
                    termination.ReasonForTermination = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.TenantDeceased).Description;
                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantDeceased).Description.ToString() ;
                    MatchingHelper.ActivityTrackerAudit(_context, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                }

                if (ApprovalStatusddl==RCSActionTypeKeys.EndOfLeasePeriod60M)
                {
                    MatchingHelper.ChangeLeaseStatusII(_context, awaitingAppraisalStatusId, LeaseId);
                    termination.ReasonForTermination = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.EndOfLeaseTerm).Description;
                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EndOfLeaseTerm).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(_context, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                }
                Session["LeaseTerminationLOSession"] = string.Format($"Lease termination processed successfully for application ref ,{LeaseApplication.LeaseReferenceNo}");
                //                                                                    1     2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17      18   19
                cc.EHCRoundRobin((int)LeaseApplication.PropertyLeaseApplicationId, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);


                _context.LeaseTerminations.Add(termination);
                _context.SaveChanges();


                if (User.IsInRole("Customers"))
                {
                    return RedirectToAction("Inbox", "propertyLeaseApplication");
                }
                if ((User.IsInRole("Lease Official")) || (User.IsInRole("Letting Officer")) || (User.IsInRole("Client Services Officer")))
                {
                    return RedirectToAction("Termination", "propertyLeaseApplication");
                }

                return View();
            }
          

        }
        #endregion

        #region Tenant upload renewal Doccuments for Risk Assessment
        [DecryptParameter]
        public ActionResult AcceptLeaseRenewal(int? Id)
        {
            var lease = db.LeaseDetails.Where(x => x.Id == Id).FirstOrDefault();
            using (var es = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                    var application   = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    var customerId    = Customer.Id;
                    var plmAppId      = lease.PropertyLeaseApplicationId;

                    // ── Pre-populate renewal document slots from original application docs ──
                    // Maps each original DocumentType key → its renewal counterpart.
                    // For each pair: if the tenant already has a renewal doc in that slot,
                    // skip it (idempotent). Otherwise clone the most-recent original doc
                    // into the renewal slot so the upload page shows current docs.
                    var docMapping = new Dictionary<string, string>
                    {
                        { DocumentTypeKeys.IdentityDocument,  DocumentTypeKeys.RenewalIdentityDocument  },
                        { DocumentTypeKeys.PayslipPensionGrant,DocumentTypeKeys.RenewalProofOfIncome    },
                        { DocumentTypeKeys.BankStatement,     DocumentTypeKeys.RenewalBankStatement     },
                        { DocumentTypeKeys.ProofOfEmployment, DocumentTypeKeys.RenewalProofOfEmployment },
                        { DocumentTypeKeys.Affidavit,         DocumentTypeKeys.RenewalAffidavit         },
                        { DocumentTypeKeys.ProofOfAddress,    DocumentTypeKeys.RenewalProofOfAddress    },
                    };

                    var statusId = es.Status.FirstOrDefault(s => s.Key == StatusKeys.ActiveLease)?.Id
                                ?? es.Status.OrderBy(s => s.Id).First().Id;

                    foreach (var map in docMapping)
                    {
                        // Resolve original checklist
                        var origDocType = es.DocumentTypes.FirstOrDefault(dt => dt.Key == map.Key);
                        if (origDocType == null) continue;
                        var origChecklist = es.DocumentCheckLists
                            .FirstOrDefault(dcl => dcl.DocumentTypeId == origDocType.Id
                                               && dcl.ReferenceTypeId == referenceType.Id);
                        if (origChecklist == null) continue;

                        // Resolve renewal checklist
                        var renewalDocType = es.DocumentTypes.FirstOrDefault(dt => dt.Key == map.Value);
                        if (renewalDocType == null) continue;
                        var renewalChecklist = es.DocumentCheckLists
                            .FirstOrDefault(dcl => dcl.DocumentTypeId == renewalDocType.Id
                                               && dcl.ReferenceTypeId == referenceType.Id);
                        if (renewalChecklist == null) continue;

                        // Skip if renewal slot already has an active doc (tenant already uploaded)
                        bool renewalSlotFilled = es.Documents.Any(d =>
                            d.DocumentCheckListId      == renewalChecklist.Id &&
                            d.PropertyLeaseApplicationId == plmAppId &&
                            d.ReferenceId              == customerId &&
                            d.IsActive && !d.IsDeleted);
                        if (renewalSlotFilled) continue;

                        // Grab the most-recent original doc for this slot (include the File bytes)
                        var origDoc = es.Documents
                            .Include(d => d.File)
                            .Where(d => d.DocumentCheckListId       == origChecklist.Id &&
                                        d.PropertyLeaseApplicationId == plmAppId &&
                                        d.ReferenceId               == customerId &&
                                        d.IsActive && !d.IsDeleted)
                            .OrderByDescending(d => d.Id)
                            .FirstOrDefault();
                        if (origDoc == null) continue;

                        // Deep-copy the file bytes into a brand-new File row so the
                        // renewal copy is 100% independent — deleting it (or the original)
                        // has zero impact on the other record.
                        int? newFileId = null;
                        if (origDoc.FileId != null && origDoc.File != null && origDoc.File.Content != null)
                        {
                            var newFile = new Models.File
                            {
                                FileName    = origDoc.File.FileName,
                                ContentType = origDoc.File.ContentType,
                                Content     = origDoc.File.Content.ToArray(), // byte-for-byte copy
                                FileSize    = origDoc.File.FileSize,
                                IsActive    = true,
                                IsDeleted   = false,
                                CreatedDateTime = DateTime.Now,
                            };
                            es.Files.Add(newFile);
                            es.SaveChanges(); // flush to get the new File.Id
                            newFileId = newFile.Id;
                        }

                        // Clone the Document row pointing to the new File
                        var renewalDoc = new Document
                        {
                            CustomerId                 = origDoc.CustomerId,
                            ReferenceTypeId            = origDoc.ReferenceTypeId,
                            ReferenceId                = origDoc.ReferenceId,
                            LocationTypeId             = origDoc.LocationTypeId,
                            DocumentLocation           = origDoc.DocumentLocation,
                            DocumentName               = origDoc.DocumentName,
                            StatusId                   = statusId,
                            DocumentCheckListId        = renewalChecklist.Id,
                            FileId                     = newFileId,          // new independent File row
                            PropertyLeaseApplicationId = origDoc.PropertyLeaseApplicationId,
                            IsActive                   = true,
                            IsDeleted                  = false,
                            IsLocked                   = false,
                            CreatedDateTime            = DateTime.Now,
                        };
                        es.Documents.Add(renewalDoc);
                    }
                    es.SaveChanges();
                    // ─────────────────────────────────────────────────────────────────────

                    return RedirectToAction("IndexTenants", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(
                        new
                        {
                            referenceId   = customerId,
                            customerId    = customerId,
                            referenceTypeId = referenceType.Id,
                            applicationId = application.Id,
                            agentId       = application.Id,
                            rcsappId      = plmAppId
                        })));
                }
                catch (Exception e)
                {
                    EventLogHelper.LogSystemError(e.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }
            }
            return RedirectToAction("Tenants");
        }

        
        #endregion


        [DecryptParameter]
        public ActionResult ApplincantDetails(int? refNo)
        {
            if (refNo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var vm = new ApplicationDetailsViewModel();
            try
            {
                eServicesDbContext _context = new eServicesDbContext();
                //var vm = new ApplicationDetailsViewModel();

                LeaseDetails leaseDetails = null;
                List<LeaseDetails> leaseDetailsListHistory = null;
                PropertyLeaseApplication propertyLeaseApplication = null;
                List<PropertyLeaseApplication> propertyLeaseApplicationList = null;
                MatchedUnits matchedUnits = null;
                PreferredComplexArea preferredComplexArea1 = null;
                PreferredComplexArea preferredComplexArea2 = null;
                List<PLMApplicationHistortyLog> pLMApplicationHistortyLogs = null;
                EnvisagedUsage envisagedUsage = null;
                HumanEHCOptions humanEHCOptions = null;
                PurchaserType purchaserType = null;
                IncomeSource incomeSource = null;
                List<DepartmentalComments> DepartmentalComments = null;
                CommitteeOutcome committeeOutcome = null;
                List<HoD> HeadofDepartment = null;
                OccupationType occupationType = null;
                Units units = null;
                IdentificationType identification = null;
                CompanyType companyType = null;
                ApplicationAllocatedProperty ekurhuleniHousingCompany = null;
                List<CommitteeOutcome> committeeList = null;
                List<PropertyResident> propertyResident = null;
                ApplicationAllocatedProperty applicationAllocatedProperty = null;

                leaseDetails = _context.LeaseDetails
                    .Include(r => r.PurchaserType)
                    .Include(r=>r.Status).Where(x => x.Id == refNo).FirstOrDefault();

                leaseDetailsListHistory = _context.LeaseDetails
                    .Include(r => r.PurchaserType)
                    .Include(r => r.Status).Where(x => x.PropertyLeaseApplicationId == leaseDetails.PropertyLeaseApplicationId).ToList();

                propertyLeaseApplication = _context.PropertyLeaseApplications
                    .Include(r => r.PurchaserType)
                    .Include(r => r.HumanEHCOptions)
                    .Include(r => r.IdentificationType)
                    .Include(r => r.IncomeSource)
                    .Include(r => r.Status)
                    .Where(x => x.Id == leaseDetails.PropertyLeaseApplicationId).FirstOrDefault();

                propertyLeaseApplicationList = _context.PropertyLeaseApplications
                    .Include(r => r.PurchaserType)
                    .Include(r => r.HumanEHCOptions).Include(r => r.IdentificationType)
                    .Include(r => r.Status)
                    .Where(x => x.IsActive && x.CustomerId == propertyLeaseApplication.CustomerId).ToList();

                purchaserType = _context.PurchaserType
                    .Where(x => x.Id == leaseDetails.PurchaserTypeId).FirstOrDefault();


               ViewBag.StartDate=Convert.ToDateTime(leaseDetails.StartDate).ToLongDateString();
               ViewBag.EndDate = Convert.ToDateTime(leaseDetails.EndDate).ToLongDateString();
               ViewBag.TerminationNotice = Convert.ToDateTime(leaseDetails.TerminationNotice).ToLongDateString();
               ViewBag.RenewalNotice = Convert.ToDateTime(leaseDetails.RenewalNotice).ToLongDateString();

                



                if (purchaserType.Id == (_context.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.Company).FirstOrDefault().Id))
                {
                    ViewBag.TenantType = "Company";
                    ViewBag.ApplicationType = "Company";
                    ViewBag.ApplicationType = "Company";

                    matchedUnits = _context.MatchedUnits.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).FirstOrDefault();
                    if (matchedUnits != null)
                    {
                        units = _context.Units.Where(x => x.Id == matchedUnits.UnitsId).FirstOrDefault();
                        ekurhuleniHousingCompany = _context.ApplicationAllocatedProperty.Include(r => r.HumanEHCOption).Where(x => x.Id == matchedUnits.ApplicationAllocatedPropertyId).FirstOrDefault();
                        applicationAllocatedProperty = _context.ApplicationAllocatedProperty.Include(r => r.HumanEHCOption).Where(x => x.Id == matchedUnits.ApplicationAllocatedPropertyId).FirstOrDefault();
                    }
                    int cop = Convert.ToInt32(propertyLeaseApplication.CompanyType);
                    companyType = _context.companyTypes.Where(x => x.Id == cop).FirstOrDefault();
                    occupationType = _context.OccupationTypes.Where(x => x.Id == propertyLeaseApplication.BType).FirstOrDefault();
                    envisagedUsage = _context.envisagedUsages.Where(x => x.Id == propertyLeaseApplication.BUsage).FirstOrDefault();
                    DepartmentalComments = _context.DepartmentalComments.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).ToList();
                    committeeList = _context.committeeOutcomes.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).ToList();
                    HeadofDepartment = _context.HoDs.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).ToList();

                }
                else if (purchaserType.Id == (_context.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.NaturalPerson).FirstOrDefault().Id))
                {
                    ViewBag.ApplicationType = "Individual";
                    ViewBag.TenantType = "Individual";
                    ViewBag.ApplicationType = "Individual";

                    matchedUnits = _context.MatchedUnits.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).FirstOrDefault();
                    if (matchedUnits != null)
                    {
                        units = _context.Units.Where(x => x.Id == matchedUnits.UnitsId).FirstOrDefault();
                        ekurhuleniHousingCompany = _context.ApplicationAllocatedProperty.Include(r => r.HumanEHCOption).Where(x => x.Id == matchedUnits.ApplicationAllocatedPropertyId).FirstOrDefault();
                        applicationAllocatedProperty = _context.ApplicationAllocatedProperty.Include(r => r.HumanEHCOption).Where(x => x.Id == matchedUnits.ApplicationAllocatedPropertyId).FirstOrDefault();
                    }
                    propertyResident = db.PropertyResidents
                        .Include(v => v.Status)
                        .Where(x => x.IsActive == true && x.StatusId != db.Status
                        .FirstOrDefault(r => r.Key == StatusKeys.DeactiveOccupant).Id && (x.RenewalLeaseId == leaseDetails.Id || x.LeaseDetailsId == leaseDetails.Id))
                        .ToList() ?? null;

                    identification = _context.IdentificationTypes.Where(x => x.Id == propertyLeaseApplication.IdentificationTypeId).FirstOrDefault();
                    preferredComplexArea1 = _context.PreferredComplexAreas.Where(x => x.Id == propertyLeaseApplication.PreferredComplexAreaId).FirstOrDefault();
                    preferredComplexArea2 = _context.PreferredComplexAreas.Where(x => x.Id == propertyLeaseApplication.PreferredComplexArea2Id).FirstOrDefault();
                    pLMApplicationHistortyLogs = _context.PLMApplicationHistortyLogs.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).ToList();
                    humanEHCOptions = _context.humanEHCOptions.Where(x => x.Id == propertyLeaseApplication.HumanEHCOptionsId).FirstOrDefault();
                    incomeSource = _context.IncomeSources.Where(x => x.Id == propertyLeaseApplication.IncomeSourceId).FirstOrDefault();
                }

                CultureInfo zar = new CultureInfo("en-ZA");

                ViewBag.Water = applicationAllocatedProperty.Water.ToString("C",zar);
                ViewBag.Sewer = applicationAllocatedProperty.Sewer.ToString("C", zar);
                ViewBag.Refuse = applicationAllocatedProperty.Refuse.ToString("C", zar);
                ViewBag.TotalMonthlyCharges = applicationAllocatedProperty.TotalMonthlyCharges.ToString("C", zar);
                //ViewBag.DepositHeld = applicationAllocatedProperty.DepositHeld.ToString("C", zar);
                ViewBag.DepositRequired = applicationAllocatedProperty.RequiedDepositAmount.ToString("C", zar);
                ViewBag.PropertyPrice = applicationAllocatedProperty.MonthlyRentalAmount.ToString("C", zar);

                ViewBag.GrossIncome = propertyLeaseApplication.GrossIncome.GetValueOrDefault().ToString("C", zar);
                ViewBag.NetIncome = propertyLeaseApplication.NetIncome.GetValueOrDefault().ToString("C", zar);
                if (propertyLeaseApplication.SecondApplicant)
                {
                    ViewBag.SecAppGrossIncome = ((decimal)propertyLeaseApplication.SecAppGrossIncome).ToString("C", zar);
                    ViewBag.SecAppNetIncome = ((decimal)propertyLeaseApplication.SecAppNetIncome).ToString("C", zar);
                }
                ViewBag.TotalCombinedIncome = propertyLeaseApplication.TotalCombinedIncome.GetValueOrDefault().ToString("C", zar);
                ViewBag.LeaseDetailsId = refNo;
                ViewBag.SecondApplicant = propertyLeaseApplication.SecondApplicant;

                leaseDetails.Data = leaseDetails.Id.ToString();
                //push to viewmodel
                vm.ApplicationAllocatedProperty = ekurhuleniHousingCompany;
                vm.ApplicationAllocatedProperty = applicationAllocatedProperty;
                vm.LeaseDetails = leaseDetails;
                vm.PropertyLeaseApplication = propertyLeaseApplication;
                vm.PropertyLeaseApplicationList = propertyLeaseApplicationList;
                vm.LeaseDetailsList = leaseDetailsListHistory;
                vm.PurchaserType = purchaserType;
                vm.MatchedUnits = matchedUnits;
                vm.CompanyType = companyType;
                vm.OccupationType = occupationType;
                vm.EnvisagedUsage = envisagedUsage;
                vm.DepartmentalCommentsList = DepartmentalComments;
                vm.CommitteeOutcomeList = committeeList;
                vm.hodList = HeadofDepartment;
                vm.PropertyResidentList = propertyResident == null ? ViewBag.propertyResident = "false" : propertyResident;
                vm.IdentificationType = identification;
                vm.PreferredComplexArea = preferredComplexArea1;
                vm.PreferredComplexAreaSecondOpt = preferredComplexArea2;
                vm.PLMApplicationHistortyLogList = pLMApplicationHistortyLogs;
                vm.HumanEHCOptions = humanEHCOptions;
                vm.IncomeSource = incomeSource;

                var application = _context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                var referenceType = _context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();
                var customer = _context.Customers.Include(s => s.SystemUser).Include(s => s.Status).Include(s => s.CustomerType).FirstOrDefault(c => c.Id == propertyLeaseApplication.CustomerId);

                //Required ViewModels For displaying
                var dvm = new DocumentsViewModel();
                var dvmUnitInspection = new DocumentsViewModel();
                var dvmMaintananceJobSheet = new DocumentsViewModel();
                var dvmTenantLease = new DocumentsViewModel();
                var dvmRisk = new DocumentsViewModel();
                var dvmExitInspection = new DocumentsViewModel();
                var dmvTenantRiskAssessment = new DocumentsViewModel();
                var dvmEvictionCommittee = new DocumentsViewModel();
                var dvmPropertyEviction = new DocumentsViewModel();
                var dvmTenantAccoutValidation = new DocumentsViewModel();
                var dvmLeaseAgreement = new DocumentsViewModel();
                var dvmLeaseWarningLetter = new DocumentsViewModel();
                if (application == null) throw new Exception("Invalid application.");
                if (referenceType == null) throw new Exception("Invalid reference type.");

                MatchingHelper.DocumentCaptureApplication(dvm, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentCaptureTenantLease(dvmTenantLease, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentConductConductExitInspection(dvmExitInspection, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentConductMaintanaceJobSheet(dvmMaintananceJobSheet, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentConductUnitInspection(dvmUnitInspection, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentEvictionCommitteeOutcome(dvmEvictionCommittee, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentPropertyEvictionValidation(dvmPropertyEviction, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentRiskAssessmentOutcome(dvmRisk, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentTenantAccountValidation(dvmTenantAccoutValidation, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentTenantRiskAssessment(dmvTenantRiskAssessment, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentUploadFinalLeaseAgreement(dvmLeaseAgreement, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                //MatchingHelper.DocumentTenantRiskAssessment(dvmLeaseAgreement, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentWarningLetter(dvmLeaseWarningLetter, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);

                vm.Document = dvm;
                vm.DocumentUnitInspection = dvmUnitInspection;
                vm.DocumentMaintananceJobSheet = dvmMaintananceJobSheet;
                vm.DocumentTenantLease = dvmTenantLease;
                vm.DocumentRiskAssessment = dvmRisk;
                vm.DocumentExitInspection = dvmExitInspection;
                vm.DocumentTenantRiskAssessment = dmvTenantRiskAssessment;
                vm.DocumentEvictionCommittee = dvmEvictionCommittee;
                vm.DocumentPropertyEviction = dvmPropertyEviction;
                vm.DocumentTenantAccoutValidation = dvmTenantAccoutValidation;
                vm.DocumentLeaseAgreement = dvmLeaseAgreement;

                var propertyLeaseActionComments = _context.propertyLeaseActionComments.Include(r => r.CreatedBySystemUser).Include(r => r.Clerk).Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id && x.LeaseWarningLetter).ToList();

                var documentLeaseWarningLetterDetails = new DepartmentsApprovalViewModel
                {
                    DocumentsViewModel = dvmLeaseWarningLetter,
                    WarningLetterReasons = propertyLeaseActionComments
                };
                vm.DocumentLeaseWarningLetterDetails = documentLeaseWarningLetterDetails;
                return View(vm);
            }
            catch (Exception Io)
            {
                
            }
            return View();

            
        }



        [DecryptParameter]
        public ActionResult Details(int? refNo)
        {
            if (refNo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var vm = new ApplicationDetailsViewModel();
            try
            {
                PropertyLeaseApplication propertyLeaseApplication = db.PropertyLeaseApplications.Where(x => x.Id == refNo).ToList().FirstOrDefault();
                MatchedUnits matchedUnits = db.MatchedUnits.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).ToList().FirstOrDefault();
                //Units units = db.Units.Where(x => x.Id == matchedUnits.unitID).ToList().FirstOrDefault();
                PreferredComplexArea preferredComplexArea1 = db.PreferredComplexAreas.Where(x => x.Key == propertyLeaseApplication.PrefArea).ToList().FirstOrDefault();
                PreferredComplexArea preferredComplexArea2 = db.PreferredComplexAreas.Where(x => x.Key == propertyLeaseApplication.PrefAreaOption2).ToList().FirstOrDefault();
                List<PLMApplicationHistortyLog> pLMApplicationHistortyLogs = db.PLMApplicationHistortyLogs.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).ToList();
                //OccupationType occupationType = db.OccupationTypes.Where(x => x.Id == units.UnitTypeId).ToList().FirstOrDefault();
                EnvisagedUsage envisagedUsage = db.envisagedUsages.Where(x => x.Id == propertyLeaseApplication.Id).ToList().FirstOrDefault();
                HumanEHCOptions humanEHCOptions = db.humanEHCOptions.Where(x => x.Id == propertyLeaseApplication.HumanEHCOptionsId).ToList().FirstOrDefault();
                PurchaserType purchaserType = db.PurchaserType.Where(x => x.Id == propertyLeaseApplication.PurchaserTypeId).ToList().FirstOrDefault();
                IncomeSource incomeSource = db.IncomeSources.Where(x => x.Id == propertyLeaseApplication.IncomeSourceId).ToList().FirstOrDefault();
                var ApplicationType = db.PurchaserType.Where(x => x.Id == propertyLeaseApplication.PurchaserTypeId).ToList().FirstOrDefault().Description;



                vm.PropertyLeaseApplication = propertyLeaseApplication;
                vm.MatchedUnits = matchedUnits;
                //vm.Units = units;
                vm.PreferredComplexArea = preferredComplexArea1;
                vm.PLMApplicationHistortyLogList = pLMApplicationHistortyLogs;
                //vm.OccupationType = occupationType;
                vm.EnvisagedUsage = envisagedUsage;
                vm.HumanEHCOptions = humanEHCOptions;
                vm.IncomeSource = incomeSource;
                //vm.PreferredComplexArea = preferredComplexArea1;
                if (ApplicationType == "Person")
                {
                    ViewBag.ApplicationType = "Individual";
                }
                else if (ApplicationType == "Company")
                {
                    ViewBag.ApplicationType = "Company";
                }

            }
            catch (Exception e)
            {

                #region RCS Models
                RCSApplicationStatus rcsapp = db.RCSApplicationStatus.Where(x => x.Id == refNo && x.IsDeleted == false).ToList().FirstOrDefault();
                var TransferInformationID = rcsapp.TransferInformationId;
                var MunicipalAccountInformationID = rcsapp.MunicipalAccountInformationId;
                TransferInformation TransferInformation = db.TransferInformations.Include(x => x.TransferTypes).Where(x => x.Id == TransferInformationID).ToList().FirstOrDefault();
                SellerInformation sellerInformation = db.SellerInformations.Where(x => x.RCSApplicationStatusId == refNo).ToList().FirstOrDefault();
                MunicipalAccountInformation MunicipalAccountInformation = db.MunicipalAccountInformations.Where(x => x.Id == MunicipalAccountInformationID).ToList().FirstOrDefault();
                WalkInApplicantDetails walkInApplicantDetails = db.WalkInApplicantDetails.Include(x => x.ActOnBehalfType).Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList().FirstOrDefault();


                if (walkInApplicantDetails != null)
                {
                    ViewBag.walkIn = "walkIn";
                }
                if (TransferInformation.TransferTypeName == "tt_SectionalTitle")
                {
                    ViewBag.sectionaltitle = "ST";
                }
                ConveyancingAttorneyDetail conveyancingAttorneyDetail = db.ConveyancingAttorneyDetails.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList().FirstOrDefault();


                var CaseHistoryLog = db.RCSApplicationHistoryLogs.Where(d => d.RCSApplicationStatusId == rcsapp.Id && d.IsActive == true && d.IsDeleted == false).Include(d => d.RCSApplicationStatus).Include(d => d.User).ToList();

                vm.ElectricityMeterList = db.ElectricityMeterInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();
                vm.WaterMeterList = db.WaterMeterInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();
                vm.PurchaserList = db.PurchaserInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();

                vm.ConveyancingAttorneyDetail = conveyancingAttorneyDetail;
                vm.TransferInformation = TransferInformation;
                vm.SellerInformation = sellerInformation;
                vm.MunicipalAccountInformation = MunicipalAccountInformation;
                vm.WalkInApplicant = walkInApplicantDetails;
                vm.RCSApplicationHistoryLogs = CaseHistoryLog;



                var documentReferenceType = db.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSRefund);
                if (documentReferenceType == null)
                    throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                        ReferenceTypeKeys.RCSUpload));

                var documentCheckLists = new List<DocumentCheckList>();

                var referenceTypeId = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload)).Id;

                var AuthorityToActAttorney = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

                documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceTypeId));

                var MunicipalStatement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

                var MunicipalCheckList = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);

                documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));



                var BankConfirmationLetter = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankConfirmationLetter);

                documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankConfirmationLetter.Id && dcl.ReferenceTypeId == referenceTypeId));

                var DeedSearch = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DeedSearch);

                documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceTypeId));

                //this code will change
                var SellerID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

                documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceTypeId));

                var PurchaserID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

                documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceTypeId));


                var AdditonalDocCap = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

                documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditonalDocCap.Id && dcl.ReferenceTypeId == referenceTypeId));

                try
                {
                    //var MunicipalAccInfo = db.MunicipalAccountInformations.Where(x => x.Id == rcsapp.MunicipalAccountInformationId).FirstOrDefault();
                    var ElectricityInfo = db.ElectricityMeterInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();
                    var WaterInfo = db.WaterMeterInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();
                    if (WaterInfo != null && ElectricityInfo != null)
                    {
                        if (ElectricityInfo.Count > 0)
                        {
                            var ElectricityMeterReading = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ElectricityMeterReading);

                            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ElectricityMeterReading.Id && dcl.ReferenceTypeId == referenceTypeId));


                        }

                        if (WaterInfo.Count > 0)
                        {
                            var WaterMeterReading = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.WaterMeterReading);

                            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == WaterMeterReading.Id && dcl.ReferenceTypeId == referenceTypeId));


                        }

                    }

                }
                catch (Exception)
                {

                }

                try
                {
                    var purchasers = db.PurchaserInformations.Include(x => x.PurchaserTypes).Where(x => x.RCSApplicationStatusId == rcsapp.Id).FirstOrDefault();


                    if (purchasers != null)
                    {
                        if (purchasers.PurchaserTypes.Key == PurchaserTypeKeys.Company || purchasers.PurchaserTypes.Key == PurchaserTypeKeys.CloseCorporation)
                        {
                            var PurchaserSalesAgreement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

                            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));


                        }
                    }


                    var walkin = db.WalkInApplicantDetails.Include(x => x.ActOnBehalfType).Where(x => x.RCSApplicationStatusId == rcsapp.Id).FirstOrDefault();
                    if (walkin != null)
                    {
                        if (walkin.ActOnBehalfType.Key == ActOnBehalfTypeKeys.ExecutorofEstate)
                        {
                            var ExecutorOfestate = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ExecutorofEstate);

                            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ExecutorOfestate.Id && dcl.ReferenceTypeId == referenceTypeId));


                        }
                        if (walkin.ActOnBehalfType.Key == ActOnBehalfTypeKeys.ExecutorofEstate)
                        {

                        }
                    }
                    else
                    {

                    }

                    var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

                    documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

                }


                catch (Exception)
                {

                }

                var documents = db.Documents.Where(d => d.RCSApplicationStatusId == rcsapp.Id).ToList();
                var notes = db.Notes.Where(n => n.ReferenceId == rcsapp.CustomerId).ToList();
                var customerDocuments = db.Documents.Include(d => d.File).Include(d => d.Status)
                                                .Where(d => d.RCSApplicationStatusId == rcsapp.Id
                            && d.IsActive
                            && d.IsDeleted == false && d.RCSApplicationStatusId == rcsapp.Id).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

                }
                var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                var dvm = new DocumentsViewModel
                {
                    ApplicationId = application.Id,
                    CustomerId = rcsapp.CustomerId,
                    Documents = customerDocuments,
                    IsUploadView = false,
                    DocumentCheckLists = documentCheckLists.ToList(),
                };

                vm.Document = dvm;
                #endregion
            }

            return View(vm);
        }
        [DecryptParameter]
         public ActionResult Delete(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var property = context.PropertyResidents.Where(x => x.Id == id).FirstOrDefault();
            property.StatusId = context.Status.FirstOrDefault(x => x.Key == StatusKeys.DeactiveOccupant).Id;
            context.Entry(property).State = EntityState.Modified;
            context.SaveChanges();

            if(property.RenewalLeaseId!=null)
                return RedirectToAction("ManageTenant", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("refNo=" + property.RenewalLeaseId.ToString()) });
            else
                return RedirectToAction("ManageTenant", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("refNo=" + property.LeaseDetailsId.ToString()) });
        }
        [DecryptParameter]
         public ActionResult Activate(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var property = context.PropertyResidents.Where(x => x.Id == id).FirstOrDefault();
            property.StatusId = context.Status.FirstOrDefault(x => x.Key == StatusKeys.ActiveOccupant).Id;
            context.Entry(property).State = EntityState.Modified;
            context.SaveChanges();

            if(property.RenewalLeaseId!=null)
                return RedirectToAction("ManageTenant", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("refNo=" + property.RenewalLeaseId.ToString()) });
            else
                return RedirectToAction("ManageTenant", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("refNo=" + property.LeaseDetailsId.ToString()) });
        }

        public ActionResult AddOccupant(TenantViewModel tenant)
        {
            eServicesDbContext context = new eServicesDbContext();
            PropertyResident prp = new PropertyResident();
            var lease = context.LeaseDetails.Where(x => x.Id == tenant.Lease.Id).ToList().FirstOrDefault();
            prp = tenant.PropertyResident;
           
            try
            {
                var AppSettings = context.AppSettings;
                var TenantSequenceCounter = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.TenantSequenceCounter);

                prp.FirstNames = tenant.PropertyResident.FirstNames;
                prp.LastName = tenant.PropertyResident.LastName;
                prp.IDNo = tenant.PropertyResident.IDNo;
                prp.FirstNames = tenant.PropertyResident.FirstNames;

                prp.PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId;
                prp.LeaseDetailsId = lease.Id;
                prp.StatusId = context.Status.FirstOrDefault(x => x.Key == StatusKeys.ActiveOccupant).Id;
                prp.OccupantReferenceNo = "LE" + lease.Id + DateTime.Now.ToString("ddMMyy") + prp.Id+ TenantSequenceCounter.Value;

                if (TenantSequenceCounter.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                {
                    TenantSequenceCounter.Value = (Convert.ToInt16(TenantSequenceCounter.Value) + 1).ToString();
                    context.Entry(TenantSequenceCounter).State = EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    TenantSequenceCounter.Value = (1).ToString();
                    context.Entry(TenantSequenceCounter).State = EntityState.Modified;
                    context.SaveChanges();
                }

                context.PropertyResidents.Add(prp);
                context.SaveChanges();
                Session["AddOccupantsSession"] = string.Format($"Occupant has been saved successfully");
                return RedirectToAction("ManageTenant", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("refNo=" + lease.Id.ToString()) });
            }
            catch (Exception e)
            {
            }
            return View();         
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LeaseDetails leaseDetails = db.LeaseDetails.Find(id);
            db.LeaseDetails.Remove(leaseDetails);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [DecryptParameter]
        public ActionResult SendWarningLetter(int? id)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();

            PropertyLeaseApplication rcsApps = null;
            LeaseDetails lease = null;

            lease = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.ModifiedBySystemUser)
              .Include(r => r.Status)
              .Include(r => r.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();

            try
            {
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                var referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                if (referenceType == null) throw new Exception("Invalid reference type.");
                DocumentsViewModel dvm = new DocumentsViewModel();
                MatchingHelper.DocumentWarningLetter(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, true);

                var propertyLeaseActionComments = context.propertyLeaseActionComments.Include(r => r.CreatedBySystemUser).Include(r => r.Clerk).Where(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.LeaseWarningLetter).ToList();

                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    PropertyLeaseApplications = rcsApps,
                    DocumentsViewModel = dvm,
                    WarningLetterReasons = propertyLeaseActionComments
                };
                vm.Customer = customer;
                vm.PropertyLeaseApplications = rcsApps;
                ViewBag.LeaseId = lease.Id;
                ViewBag.ApplicationId = application.Id;

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
        public ActionResult SendWarningLetter(int? id, string docRandId, string WarningComment)
        {

            using (var _conx = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var userID = Customer;
                    var lease = _conx.LeaseDetails.Where(x => x.Id == id).Include(r => r.PurchaserType).Include(r => r.Status).FirstOrDefault();
                    PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                    {
                        PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId,
                        WarningReason = WarningComment,
                        LeaseWarningLetter = true,
                        CreatedDateTime = DateTime.Now,
                        ClerkId = Customer.Id,
                        WarningDocRefId = Convert.ToInt32(docRandId)
                    };
                    _conx.propertyLeaseActionComments.Add(comments);
                    _conx.SaveChanges();

                    var UserId = Customer.Id;
                    var custmusers = _conx.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _conx.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UploadWarningLetter).Description.ToString() + " : " + lease.LeaseReferenceNo;
                    MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    int emailboodyId = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.WarningLetter).Id;
                    EmailHelper.CustomerEmailNotification(_conx, lease.PropertyLeaseApplicationId, emailboodyId, "EHC Warning letter");

                    Session["SendWarningLetterSession"] = string.Format($"Warning letter sent successfully for application reference ,{lease.LeaseReferenceNo}");
                    return RedirectToAction("AllPropertyLeases", "PropertyLeaseApplication");
                }
                catch (Exception e)
                {

                    throw;
                }
            }

        }

        //Terminations by Customer and now entertained by LO
        public ActionResult TerminationQueued()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var Keys = cxt.Status;
                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    int UserId = Customer.Id;
                    List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Terminations).FirstOrDefault().Id;

                    rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.StatusId == SubmittedId).ToList();

                    var list = rrq.Select(x => x.LeaseDetailsId).ToList();


                    var rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false && list.Contains(x.Id))
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.Status).ToList();
                    if (Session["Display"] != null)
                    {
                        ViewBag.Display = "True";
                        ViewBag.MessageTitle3 = "Success!";
                        ViewBag.MessageBody3 = Session["MessageBody"].ToString();

                        Session["Display"] = null;
                        Session["ApplicationRefNo"] = null;
                        Session["MessageTitle"] = null;
                        Session["MessageBody"] = null;
                    }
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
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







