using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Collections.Generic;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Controllers
{
    [AllowAnonymous]
    public class RealEstateController : Controller
    {
        private readonly eServicesDbContext db = new eServicesDbContext();
        private readonly BaseHelper _base = new BaseHelper();
        private SystemUser _systemUser;
        private Customer _customer;

        private void Initialise()
        {
            _base.Initialise(db);
            _systemUser = _base.SystemUser;
            _customer = _base.Customer;

            if (_systemUser == null)
            {
                _systemUser = db.SystemUsers.FirstOrDefault(o => o.IsActive && !o.IsDeleted);
            }
            if (_customer == null)
            {
                _customer = db.Customers.FirstOrDefault(c => c.IsActive && !c.IsDeleted);
            }

            if (_customer != null && _customer.Status == null)
            {
                _customer.Status = db.Status.Find(_customer.StatusId);
            }
        }

        // GET: RealEstate/Inbox
        public ActionResult Inbox()
        {
            Initialise();
            if (_customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch pending applications for this customer
            var pendingStatusKeys = new[] { StatusKeys.Submitted, StatusKeys.InProgress, StatusKeys.Query };
            var applications = db.RE_Applications
                .Include(a => a.Status)
                .Where(a => a.CustomerId == _customer.Id && a.IsActive && !a.IsDeleted && pendingStatusKeys.Contains(a.Status.Key))
                .ToList();

            return View(applications);
        }

        // GET: RealEstate/Capture
        public ActionResult Capture()
        {
            Initialise();
            if (_customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if profile is active/approved
            // if (_customer.Status.Key != StatusKeys.CustomerActive)
            // {
            //     object obj = new { customerId = _customer.Id, agentId = 0 };
            //     return RedirectToAction("Index3", "Profile", SecureActionLinkExtension.Encrypt(obj));
            // }

            // Populate Dropdowns
            PopulateCaptureViewBags();

            var model = new RE_CaptureViewModel
            {
                Application = new RE_Application
                {
                    SystemUserId = _systemUser.Id,
                    CustomerId = _customer.Id,
                    EntityEmail = _systemUser.EmailAddress,
                    EntityMobile = _systemUser.MobileNumber,
                    EntityRegisteredAddress = _customer.PhysicalAddress1 + " " + _customer.PhysicalAddress2,
                    EntityRegisteredPostalCode = Convert.ToString(_customer.PhysicalAddressCode),
                    BankName = "",
                    BankAccountType = "Savings",
                    BankAccountName = _customer.FullName,
                    BankAccountNumber = "",
                    BankBranchCode = "",
                    ErfFarmNumber = "",
                    PropertyAddress = "",
                    TownshipSuburbFarmName = "",
                    PropertyPostalCode = ""
                }
            };

            return View(model);
        }

        // POST: RealEstate/Capture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Capture(RE_CaptureViewModel model)
        {
            Initialise();
            if (_customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            HttpPostedFileBase file_Id = Request.Files["file_Id"];
            HttpPostedFileBase file_Address = Request.Files["file_Address"];
            HttpPostedFileBase file_Cipc = Request.Files["file_Cipc"];
            HttpPostedFileBase file_Sars = Request.Files["file_Sars"];
            HttpPostedFileBase file_Profile = Request.Files["file_Profile"];
            HttpPostedFileBase file_References = Request.Files["file_References"];
            HttpPostedFileBase file_Letters = Request.Files["file_Letters"];
            HttpPostedFileBase file_Locality = Request.Files["file_Locality"];
            HttpPostedFileBase file_Zoning = Request.Files["file_Zoning"];
            HttpPostedFileBase file_Fee = Request.Files["file_Fee"];
            HttpPostedFileBase file_Experience = Request.Files["file_Experience"];
            HttpPostedFileBase file_Financials = Request.Files["file_Financials"];
            HttpPostedFileBase file_BusinessPlan = Request.Files["file_BusinessPlan"];
            HttpPostedFileBase file_Mbd4 = Request.Files["file_Mbd4"];

            // Retrieve optional supporting other files
            var otherFilesList = new List<HttpPostedFileBase>();
            for (int i = 0; i < Request.Files.Count; i++)
            {
                if (Request.Files.GetKey(i) == "file_Other")
                {
                    var file = Request.Files[i];
                    if (file != null && file.ContentLength > 0)
                    {
                        otherFilesList.Add(file);
                    }
                }
            }

            // Remove server-generated fields from model validation
            ModelState.Remove("Application.ApplicationReferenceNumber");

            // Perform manual validation on uploaded documents
            bool isIndividual = model.Application.ApplicantType == "Individual";

            // Enforce input field validations based on requirements
            if (!isIndividual && (string.IsNullOrEmpty(model.Application.CompanyRegistrationNumber) || !System.Text.RegularExpressions.Regex.IsMatch(model.Application.CompanyRegistrationNumber, @"^\d{4}/\d{6}/\d{2}$")))
            {
                ModelState.AddModelError("Application.CompanyRegistrationNumber", "Company Registration Number must follow the format YYYY/NNNNNN/NN.");
            }
            if (string.IsNullOrEmpty(model.Application.TaxReferenceNumber) || !System.Text.RegularExpressions.Regex.IsMatch(model.Application.TaxReferenceNumber, @"^\d{10}$"))
            {
                ModelState.AddModelError("Application.TaxReferenceNumber", "SARS Tax Reference Number must be exactly 10 digits.");
            }
            if (string.IsNullOrEmpty(model.Application.EntityRegisteredPostalCode) || !System.Text.RegularExpressions.Regex.IsMatch(model.Application.EntityRegisteredPostalCode, @"^\d{4}$"))
            {
                ModelState.AddModelError("Application.EntityRegisteredPostalCode", "Postal Code must be exactly 4 digits.");
            }
            if (string.IsNullOrEmpty(model.Application.PropertyPostalCode) || !System.Text.RegularExpressions.Regex.IsMatch(model.Application.PropertyPostalCode, @"^\d{4}$"))
            {
                ModelState.AddModelError("Application.PropertyPostalCode", "Property Postal Code must be exactly 4 digits.");
            }
            if (!isIndividual && !string.IsNullOrEmpty(model.Application.VatRegistrationNumber))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.Application.VatRegistrationNumber, @"^4\d{9}$"))
                {
                    ModelState.AddModelError("Application.VatRegistrationNumber", "VAT Registration Number must be a 10 digit number starting with 4.");
                }
            }
            if (!isIndividual && !string.IsNullOrEmpty(model.Application.EntityTelephone))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.Application.EntityTelephone, @"^0\d{9}$"))
                {
                    ModelState.AddModelError("Application.EntityTelephone", "Telephone Number must be exactly 10 digits starting with 0.");
                }
            }
            if (string.IsNullOrEmpty(model.Application.EntityMobile) || !System.Text.RegularExpressions.Regex.IsMatch(model.Application.EntityMobile, @"^0\d{9}$"))
            {
                ModelState.AddModelError("Application.EntityMobile", "Mobile Number must be exactly 10 digits starting with 0.");
            }
            if (!isIndividual && !string.IsNullOrEmpty(model.Application.EntityFax))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.Application.EntityFax, @"^0\d{9}$"))
                {
                    ModelState.AddModelError("Application.EntityFax", "Fax Number must be exactly 10 digits starting with 0.");
                }
            }

            if (file_Id == null || file_Id.ContentLength == 0)
                ModelState.AddModelError("file_Id", "Applicant ID Document is required.");
            if (file_Address == null || file_Address.ContentLength == 0)
                ModelState.AddModelError("file_Address", "Proof of Address is required.");
            if (!isIndividual && (file_Cipc == null || file_Cipc.ContentLength == 0))
                ModelState.AddModelError("file_Cipc", "CIPC Compliance Document is required for Entity applications.");
            if (file_Sars == null || file_Sars.ContentLength == 0)
                ModelState.AddModelError("file_Sars", "SARS Tax Clearance Certificate is required.");
            if (!isIndividual && (file_Profile == null || file_Profile.ContentLength == 0))
                ModelState.AddModelError("file_Profile", "Company Profile is required for Entity applications.");
            if (file_References == null || file_References.ContentLength == 0)
                ModelState.AddModelError("file_References", "Contactable References document is required.");
            if (file_Letters == null || file_Letters.ContentLength == 0)
                ModelState.AddModelError("file_Letters", "Supporting/Motivational Letter is required.");
            if (file_Fee == null || file_Fee.ContentLength == 0)
                ModelState.AddModelError("file_Fee", "Proof of Application Fee Payment is required.");
            if (file_Experience == null || file_Experience.ContentLength == 0)
                ModelState.AddModelError("file_Experience", "Facilities Management Experience description is required.");
            if (file_Financials == null || file_Financials.ContentLength == 0)
                ModelState.AddModelError("file_Financials", "3-years Audited Financial Statements (or equivalent) is required.");
            if (file_BusinessPlan == null || file_BusinessPlan.ContentLength == 0)
                ModelState.AddModelError("file_BusinessPlan", "Business Plan is required.");
            if (file_Mbd4 == null || file_Mbd4.ContentLength == 0)
                ModelState.AddModelError("file_Mbd4", "Declaration of Interest (Form MBD 4) is required.");

            // Parse and validate SelectedUnitsJson list
            List<RE_SelectedUnit> selectedUnitsList = null;
            if (!string.IsNullOrEmpty(model.Application.SelectedUnitsJson))
            {
                try
                {
                    selectedUnitsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RE_SelectedUnit>>(model.Application.SelectedUnitsJson);
                }
                catch { }
            }

            if (selectedUnitsList != null && selectedUnitsList.Any())
            {
                // Remove standard properties from validation if we are in multiple units mode
                ModelState.Remove("Application.SelectedFacilityId");
                ModelState.Remove("Application.SelectedFacilityUnitId");
                ModelState.Remove("Application.SelectedUnitCount");

                decimal totalRental = 0;
                foreach (var selUnit in selectedUnitsList)
                {
                    var facilityUnit = db.RE_FacilityUnits
                        .Include(fu => fu.FacilityCategory)
                        .FirstOrDefault(fu => fu.Id == selUnit.UnitId && fu.IsActive && !fu.IsDeleted);

                    if (facilityUnit == null)
                    {
                        ModelState.AddModelError("", "One of the selected units is invalid.");
                    }
                    else if (selUnit.Qty > facilityUnit.MaxUnits)
                    {
                        ModelState.AddModelError("", string.Format("Quantity for {0} exceeds maximum limit of {1}.", facilityUnit.UnitType, facilityUnit.MaxUnits));
                    }
                    else
                    {
                        totalRental += facilityUnit.UnitSize * facilityUnit.FacilityCategory.TariffPerSqm * selUnit.Qty;
                    }
                }

                if (ModelState.IsValid)
                {
                    // Map primary fields for backward compatibility
                    var firstUnit = selectedUnitsList.First();
                    var firstDbUnit = db.RE_FacilityUnits.Find(firstUnit.UnitId);
                    model.Application.SelectedFacilityId = firstDbUnit?.FacilityId;
                    model.Application.SelectedFacilityUnitId = firstUnit.UnitId;
                    model.Application.SelectedUnitCount = firstUnit.Qty;
                    model.Application.CalculatedMonthlyRental = totalRental;
                }
            }
            else
            {
                // Fallback to validating single-unit dropdowns
                if (model.Application.SelectedFacilityId == null)
                    ModelState.AddModelError("Application.SelectedFacilityId", "Please select a Facility.");
                if (model.Application.SelectedFacilityUnitId == null)
                    ModelState.AddModelError("Application.SelectedFacilityUnitId", "Please select a Unit Type.");
                if (model.Application.SelectedUnitCount == null || model.Application.SelectedUnitCount <= 0 || model.Application.SelectedUnitCount > 4)
                    ModelState.AddModelError("Application.SelectedUnitCount", "Number of units must be between 1 and 4.");

                if (ModelState.IsValid)
                {
                    try
                    {
                        var facilityUnit = db.RE_FacilityUnits
                            .Include(fu => fu.FacilityCategory)
                            .FirstOrDefault(fu => fu.Id == model.Application.SelectedFacilityUnitId && fu.IsActive && !fu.IsDeleted);

                        if (facilityUnit == null)
                        {
                            ModelState.AddModelError("Application.SelectedFacilityUnitId", "Invalid Facility Unit selected.");
                        }
                        else if (model.Application.SelectedUnitCount > facilityUnit.MaxUnits)
                        {
                            ModelState.AddModelError("Application.SelectedUnitCount", string.Format("Quantity exceeds maximum limit of {0} for this unit.", facilityUnit.MaxUnits));
                        }
                        else
                        {
                            decimal calculatedRental = facilityUnit.UnitSize * facilityUnit.FacilityCategory.TariffPerSqm * model.Application.SelectedUnitCount.Value;
                            model.Application.CalculatedMonthlyRental = calculatedRental;
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Failed to validate unit type: " + ex.Message);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                PopulateCaptureViewBags();
                return View(model);
            }

            try
            {
                // Generate Reference Number
                string referenceNumber = GenerateReferenceNumber();
                    model.Application.ApplicationReferenceNumber = referenceNumber;
                    model.Application.SystemUserId = _systemUser.Id;
                    model.Application.CustomerId = _customer.Id;
                    model.Application.StatusId = db.Status.FirstOrDefault(s => s.Key == StatusKeys.AwaitingApplicationFeeValidation).Id;
                    model.Application.DepartmentId = db.ApplicationEntities.FirstOrDefault(ae => ae.Key == ApplicationEntityKeys.RealEstateDevelopment)?.Id;
                    model.Application.IsActive = true;
                    model.Application.IsDeleted = false;
                    model.Application.IsLocked = false;
                    model.Application.CreatedBySystemUserId = _systemUser.Id;
                    model.Application.CreatedDateTime = DateTime.Now;
                    model.Application.ModifiedBySystemUserId = _systemUser.Id;
                    model.Application.ModifiedDateTime = DateTime.Now;

                    db.RE_Applications.Add(model.Application);
                    db.SaveChanges();

                    int appId = model.Application.Id;

                    // Reference Type Key for Real Estate Application
                    int refTypeId = db.ReferenceTypes.FirstOrDefault(r => r.Key == ReferenceTypeKeys.RealEstateApplication)?.Id ?? 16;

                    // Save the files and link them
                    SaveAndLinkFile(file_Id, "Applicant ID Document", appId, 1, refTypeId);
                    SaveAndLinkFile(file_Address, "Proof of Address", appId, 54, refTypeId);
                    if (!isIndividual && file_Cipc != null)
                        SaveAndLinkFile(file_Cipc, "CIPC Compliance Document", appId, 18, refTypeId);
                    SaveAndLinkFile(file_Sars, "SARS Tax Clearance Certificate", appId, 23, refTypeId);
                    if (!isIndividual && file_Profile != null)
                        SaveAndLinkFile(file_Profile, "Company Profile", appId, 18, refTypeId);
                    SaveAndLinkFile(file_References, "Contactable References", appId, 18, refTypeId);
                    SaveAndLinkFile(file_Letters, "Supporting/Motivational Letter", appId, 18, refTypeId);
                    if (file_Locality != null && file_Locality.ContentLength > 0)
                        SaveAndLinkFile(file_Locality, "Locality Plan of Property", appId, 18, refTypeId);
                    if (file_Zoning != null && file_Zoning.ContentLength > 0)
                        SaveAndLinkFile(file_Zoning, "Copy of Zoning Certificate", appId, 18, refTypeId);
                    SaveAndLinkFile(file_Fee, "Proof of Application Fee Payment", appId, 18, refTypeId);
                    SaveAndLinkFile(file_Experience, "Facilities Management Experience", appId, 18, refTypeId);
                    SaveAndLinkFile(file_Financials, "3-years Audited Financial Statements", appId, 59, refTypeId);
                    SaveAndLinkFile(file_BusinessPlan, "Business Plan", appId, 18, refTypeId);
                    SaveAndLinkFile(file_Mbd4, "Declaration of Interest (Form MBD 4)", appId, 18, refTypeId);

                    // Save and link other supporting documents
                    foreach (var otherFile in otherFilesList)
                    {
                        SaveAndLinkFile(otherFile, "Other Supporting Document: " + Path.GetFileName(otherFile.FileName), appId, 18, refTypeId);
                    }

                    // Assign Verify Payment task in Round Robin Queue
                    RealEstateWorkAllocationHelper.AssignRealEstateTask(db, appId, ResponsibilityTypeKeys.RealEstateVerifyPayment);

                    // Notify customer via Email
                    try
                    {
                        var email = new Email();
                        string subject = "Real Estate Development: Lease Application Captured Successfully";
                        string body = string.Format("Dear {0},<br/><br/>Your lease application for space has been captured successfully. Your Reference Number is: <strong>{1}</strong>.<br/><br/>Your application will now be processed for evaluation.", _systemUser.FullName, referenceNumber);
                        email.GenerateEmail(_systemUser.EmailAddress, subject, body, _customer.Id.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, _systemUser.FullName);
                    }
                    catch (Exception ex)
                    {
                        EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    }

                    TempData["SuccessMessage"] = "Application submitted successfully! Reference Number: " + referenceNumber;
                    return RedirectToAction("MyApplications");
                }
                catch (Exception ex)
                {
                    var fullError = ex.ToString();
                    if (ex.InnerException != null)
                    {
                        fullError += "\nINNER EXCEPTION:\n" + ex.InnerException.ToString();
                        if (ex.InnerException.InnerException != null)
                        {
                            fullError += "\nDOUBLE INNER:\n" + ex.InnerException.InnerException.ToString();
                        }
                    }
                    try
                    {
                        var path = System.IO.Path.Combine(HttpRuntime.AppDomainAppPath, "Logs", "Db_Save_Error_Detail.txt");
                        System.IO.File.WriteAllText(path, fullError);
                    }
                    catch { }
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    ModelState.AddModelError("", "An error occurred while saving your application. Please try again.");
                }

            // If we reach here, validation failed. Repopulate view bags.
            PopulateCaptureViewBags();

            return View(model);
        }

        // GET: RealEstate/MyApplications
        public ActionResult MyApplications()
        {
            Initialise();
            if (_customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var applications = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.CCC)
                .Where(a => a.CustomerId == _customer.Id && a.IsActive && !a.IsDeleted)
                .ToList();

            return View(applications);
        }

        [HttpGet]
        public JsonResult GetApplicationDetails(int id)
        {
            Initialise();
            if (_customer == null)
            {
                return Json(new { success = false, message = "Unauthorized access." }, JsonRequestBehavior.AllowGet);
            }

            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.CCC)
                .Include(a => a.SelectedFacility)
                .Include(a => a.SelectedFacilityUnit)
                .Include(a => a.SelectedFacilityUnit.FacilityCategory)
                .FirstOrDefault(a => a.Id == id && a.CustomerId == _customer.Id && a.IsActive && !a.IsDeleted);

            if (app == null)
            {
                return Json(new { success = false, message = "Application not found." }, JsonRequestBehavior.AllowGet);
            }

            var data = new
            {
                app.Id,
                app.ApplicationReferenceNumber,
                app.ApplicantType,
                app.EntityName,
                app.CompanyRegistrationNumber,
                app.VatRegistrationNumber,
                app.TaxReferenceNumber,
                app.EntityRegisteredAddress,
                app.EntityRegisteredPostalCode,
                app.AuthorizedRepresentativeName,
                app.AuthorizedRepresentativeCapacity,
                app.EntityTelephone,
                app.EntityMobile,
                app.EntityFax,
                app.EntityEmail,
                app.BankName,
                app.BankAccountType,
                app.BankAccountName,
                app.BankAccountNumber,
                app.BankBranchCode,
                app.PurposeOfLease,
                CCCName = app.CCC != null ? app.CCC.CCCName : "N/A",
                app.ErfFarmNumber,
                app.PropertyAddress,
                app.TownshipSuburbFarmName,
                app.PropertyPostalCode,
                app.FacilityOutdoorAdvertising,
                app.FacilityTelecommunications,
                app.FacilityInformalTrading,
                app.FacilityTaxiRankTrading,
                app.FacilityVocationalSkills,
                app.FacilityComputerTraining,
                app.FacilityIndustrialPark,
                app.FacilityBusinessHub,
                app.FacilityAutomotiveHub,
                app.FacilityAgriPark,
                app.FacilityIncubationFarm,
                StatusName = app.Status != null ? app.Status.Name : "Submitted",
                app.SelectedFacilityId,
                SelectedFacilityName = app.SelectedFacility != null ? app.SelectedFacility.Name : "N/A",
                app.SelectedFacilityUnitId,
                SelectedUnitType = app.SelectedFacilityUnit != null ? app.SelectedFacilityUnit.UnitType : "N/A",
                SelectedUnitSize = app.SelectedFacilityUnit != null ? app.SelectedFacilityUnit.UnitSize : 0,
                SelectedUnitTariff = app.SelectedFacilityUnit != null && app.SelectedFacilityUnit.FacilityCategory != null ? app.SelectedFacilityUnit.FacilityCategory.TariffPerSqm : 0,
                app.SelectedUnitCount,
                app.CalculatedMonthlyRental
            };

            return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFacilitiesByCCC(int cccId)
        {
            var facilities = db.RE_Facilities
                .Where(f => f.CCCId == cccId && f.IsActive && !f.IsDeleted)
                .Select(f => new { f.Id, f.Name })
                .OrderBy(f => f.Name)
                .ToList();
            return Json(facilities, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUnitsByFacility(int facilityId)
        {
            var units = db.RE_FacilityUnits
                .Include(u => u.FacilityCategory)
                .Where(u => u.FacilityId == facilityId && u.IsActive && !u.IsDeleted)
                .Select(u => new
                {
                    u.Id,
                    u.UnitType,
                    u.UnitSize,
                    Tariff = u.FacilityCategory.TariffPerSqm,
                    u.MaxUnits,
                    CategoryName = u.FacilityCategory.Name
                })
                .OrderBy(u => u.UnitType)
                .ToList();
            return Json(units, JsonRequestBehavior.AllowGet);
        }

        private void SaveAndLinkFile(HttpPostedFileBase file, string documentName, int appId, int documentTypeId, int referenceTypeId)
        {
            if (file == null || file.ContentLength == 0) return;

            byte[] contentData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                contentData = binaryReader.ReadBytes(file.ContentLength);
            }

            var dbFile = new C8.eServices.Mvc.Models.File
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                Content = contentData,
                FileSize = file.ContentLength,
                IsActive = true,
                IsDeleted = false,
                CreatedDateTime = DateTime.Now
            };

            db.Files.Add(dbFile);
            db.SaveChanges();

            var doc = new Document
            {
                CustomerId = _customer.Id,
                RealEstateApplicationId = appId,
                FileId = dbFile.Id,
                DocumentName = documentName,
                ReferenceId = appId,
                ReferenceTypeId = referenceTypeId,
                LocationTypeId = db.LocationTypes.FirstOrDefault(l => l.Key == LocationTypeKeys.Database)?.Id ?? 3,
                StatusId = db.Status.FirstOrDefault(s => s.Key == StatusKeys.DocumentUploaded).Id,
                DocumentCheckListId = db.DocumentCheckLists.FirstOrDefault(d => d.DocumentTypeId == documentTypeId)?.Id ?? 1,
                IsActive = true,
                IsDeleted = false,
                CreatedDateTime = DateTime.Now
            };

            db.Documents.Add(doc);
            db.SaveChanges();
        }

        private string GenerateReferenceNumber()
        {
            var counterSetting = db.AppSettings.FirstOrDefault(o => o.Key == "red_daily_sequence_counter");
            var limiterSetting = db.AppSettings.FirstOrDefault(o => o.Key == "red_daily_sequence_limiter");

            if (counterSetting == null || limiterSetting == null)
            {
                throw new Exception("RED sequence counter app settings are missing in the database.");
            }

            int limiter = Convert.ToInt32(limiterSetting.Value);
            int currentSeq = Convert.ToInt32(counterSetting.Value);

            if (counterSetting.ModifiedDateTime.HasValue && counterSetting.ModifiedDateTime.Value.Date == DateTime.Today)
            {
                currentSeq++;
            }
            else
            {
                currentSeq = 1;
            }

            string nextVal = currentSeq.ToString().PadLeft(limiter, '0');

            counterSetting.Value = nextVal;
            counterSetting.ModifiedDateTime = DateTime.Now;
            db.Entry(counterSetting).State = EntityState.Modified;
            db.SaveChanges();

            return string.Format("DPRE-{0}-{1}", DateTime.Now.ToString("yyyyMMdd"), nextVal);
        }

        private void PopulateCaptureViewBags()
        {
            ViewBag.CCCList = db.CCCs.Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.CCCName)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CCCName })
                .ToList();

            ViewBag.BankAccountTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Savings", Text = "Savings Account" },
                new SelectListItem { Value = "Cheque/Current", Text = "Cheque / Current Account" },
                new SelectListItem { Value = "Transmission", Text = "Transmission Account" }
            };

            ViewBag.ApplicantTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Individual", Text = "Individual" },
                new SelectListItem { Value = "Close Corporation", Text = "Close Corporation" },
                new SelectListItem { Value = "Company (PTY LTD)/Partnership", Text = "Company (PTY LTD) / Partnership" },
                new SelectListItem { Value = "NPO/NGO", Text = "NPO / NGO" },
                new SelectListItem { Value = "Government Entity (Organ of State)", Text = "Government Entity (Organ of State)" }
            };

            ViewBag.PurposesOfLease = new List<SelectListItem>
            {
                new SelectListItem { Value = "Retail", Text = "Retail Units (Shops, kiosks, stalls, etc.)" },
                new SelectListItem { Value = "Office/Professional", Text = "Offices/Professional Units" }
            };
        }

        // --- UC 13: Customer Confirms or Proposes Slot ---
        public ActionResult SelectInspectionSlot(int id)
        {
            Initialise();
            if (_customer == null) return RedirectToAction("Login", "Account");

            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SelectedFacility)
                .Include(a => a.SelectedFacilityUnit)
                .FirstOrDefault(a => a.Id == id && a.CustomerId == _customer.Id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();
            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectInspectionSlot(int id, string actionType, DateTime? proposedDate, string proposedTime)
        {
            Initialise();
            if (_customer == null) return RedirectToAction("Login", "Account");

            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.CustomerId == _customer.Id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (actionType == "Confirm")
            {
                // Accept the scheduled slot
                app.InspectionStatus = "Confirmed";
                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Applicant accepted/confirmed scheduled inspection slot.");
                TempData["SuccessMessage"] = "Inspection slot confirmed successfully.";
            }
            else if (actionType == "Propose")
            {
                if (!proposedDate.HasValue || string.IsNullOrEmpty(proposedTime))
                {
                    TempData["ErrorMessage"] = "Proposed Date and Time are required.";
                    return RedirectToAction("SelectInspectionSlot", new { id = id });
                }

                // Customer proposes new slot
                app.InspectionDate = proposedDate;
                app.InspectionTime = proposedTime;
                app.InspectionStatus = "Proposed";
                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Applicant proposed new inspection slot: {0:yyyy-MM-dd} at {1}.", proposedDate.Value, proposedTime));
                TempData["SuccessMessage"] = "Proposed slot submitted. Property officer will review.";
            }

            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Inbox");
        }

        // --- UC 18: Request Permission to Occupy ---
        public ActionResult RequestPto(int id)
        {
            Initialise();
            if (_customer == null) return RedirectToAction("Login", "Account");

            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SelectedFacility)
                .Include(a => a.SelectedFacilityUnit)
                .FirstOrDefault(a => a.Id == id && a.CustomerId == _customer.Id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            // Verify eligibility
            var allowedStatuses = new[] { StatusKeys.ReConcludedApproved, StatusKeys.ReAwaitingInspection, StatusKeys.ReAwaitingAgreementConclusion };
            if (!allowedStatuses.Contains(app.Status.Key))
            {
                TempData["ErrorMessage"] = "Please note that you do not have applications with eligible status.";
                return RedirectToAction("MyApplications");
            }

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestPto(int id, string purpose, DateTime? startDate, DateTime? endDate, bool? acceptIndemnity)
        {
            Initialise();
            if (_customer == null) return RedirectToAction("Login", "Account");

            var app = db.RE_Applications
                .Include(a => a.Status)
                .FirstOrDefault(a => a.Id == id && a.CustomerId == _customer.Id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            if (string.IsNullOrEmpty(purpose) || !startDate.HasValue || !endDate.HasValue || acceptIndemnity != true)
            {
                TempData["ErrorMessage"] = "Purpose, Dates and Acceptance of Indemnity are required.";
                return RedirectToAction("RequestPto", new { id = id });
            }

            if (startDate.Value < DateTime.Today)
            {
                TempData["ErrorMessage"] = "The start date cannot fall in the past.";
                return RedirectToAction("RequestPto", new { id = id });
            }

            if (endDate.Value <= startDate.Value)
            {
                TempData["ErrorMessage"] = "The end date must be after the start date.";
                return RedirectToAction("RequestPto", new { id = id });
            }

            double totalDays = (endDate.Value - startDate.Value).TotalDays;
            if (totalDays > 366) // 12 months limit
            {
                TempData["ErrorMessage"] = "The requested PTO period cannot exceed the maximum allowable period of 12 months.";
                return RedirectToAction("RequestPto", new { id = id });
            }

            // Documents upload
            HttpPostedFileBase fileInsurance = Request.Files["fileInsurance"];
            HttpPostedFileBase fileFitout = Request.Files["fileFitout"];
            HttpPostedFileBase fileHealthSafety = Request.Files["fileHealthSafety"];

            if (fileInsurance == null || fileInsurance.ContentLength == 0 ||
                fileFitout == null || fileFitout.ContentLength == 0 ||
                fileHealthSafety == null || fileHealthSafety.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "All supporting documents (Certificate of Insurance, Fit-out plans, Health & Safety Clearance) are mandatory.";
                return RedirectToAction("RequestPto", new { id = id });
            }

            // Reference Type Key for Real Estate Application
            int refTypeId = db.ReferenceTypes.FirstOrDefault(r => r.Key == ReferenceTypeKeys.RealEstateApplication)?.Id ?? 16;

            SaveAndLinkFile(fileInsurance, "PTO: Certificate of Insurance", id, 18, refTypeId);
            SaveAndLinkFile(fileFitout, "PTO: Fit-out plans", id, 18, refTypeId);
            SaveAndLinkFile(fileHealthSafety, "PTO: Health & Safety Clearance", id, 18, refTypeId);

            // Generate PTO Ref: PTO-yyyyMMdd-####
            string yearStr = DateTime.Now.ToString("yyyyMMdd");
            var counterSetting = db.AppSettings.FirstOrDefault(o => o.Key == "red_daily_sequence_counter");
            string nextVal = counterSetting != null ? counterSetting.Value : "001";
            app.PtoReferenceNumber = string.Format("PTO-{0}-{1}", yearStr, nextVal);

            app.PtoPurposeOfOccupation = purpose;
            app.PtoStartDate = startDate;
            app.PtoEndDate = endDate;
            app.PtoAcceptedIndemnity = true;

            var targetStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ReAwaitingPtoReview);
            if (targetStatus != null)
            {
                app.StatusId = targetStatus.Id;
                app.PtoStatus = targetStatus.Name;
            }

            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Applicant requested early Permission to Occupy.");
            TempData["SuccessMessage"] = "PTO request submitted successfully! Ref: " + app.PtoReferenceNumber;
            return RedirectToAction("MyApplications");
        }

        private int SaveFile(HttpPostedFileBase file)
        {
            byte[] contentData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                contentData = binaryReader.ReadBytes(file.ContentLength);
            }

            var dbFile = new C8.eServices.Mvc.Models.File
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                Content = contentData,
                FileSize = file.ContentLength,
                IsActive = true,
                IsDeleted = false,
                CreatedDateTime = DateTime.Now
            };

            db.Files.Add(dbFile);
            db.SaveChanges();
            return dbFile.Id;
        }

        // --- UC 23: Tenant Sign Lease/User Agreement ---
        public ActionResult LeaseAgreements()
        {
            Initialise();
            if (_customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.CCC)
                .Where(a => a.CustomerId == _customer.Id && a.IsActive && !a.IsDeleted)
                .ToList();

            return View(apps);
        }

        public ActionResult SignLeaseAgreement(int id)
        {
            Initialise();
            if (_customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.CustomerId == _customer.Id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignLeaseAgreement(int id, string dummyParam = "")
        {
            Initialise();
            if (_customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.CustomerId == _customer.Id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            HttpPostedFileBase signedLeaseFile = Request.Files["signedLeaseFile"];

            if (signedLeaseFile == null || signedLeaseFile.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Signed Lease/User Agreement document upload is mandatory.";
                return RedirectToAction("SignLeaseAgreement", new { id = id });
            }

            int? fileId = SaveFile(signedLeaseFile);
            app.LeaseAgreementSignedFileId = fileId;
            app.LeaseAgreementTenantSignatureDate = DateTime.Now;

            var status = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ReAwaitingAgreementConclusionOutcome);
            if (status != null) app.StatusId = status.Id;

            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            // Link signed file in Documents table too
            int refTypeId = db.ReferenceTypes.FirstOrDefault(r => r.Key == ReferenceTypeKeys.RealEstateApplication)?.Id ?? 16;
            SaveAndLinkFile(signedLeaseFile, "Signed Lease/User Agreement", id, 18, refTypeId);

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Tenant signed and uploaded the User Agreement.");
            TempData["SuccessMessage"] = "Lease agreement signed and uploaded successfully! Pending final authorization.";
            return RedirectToAction("MyApplications");
        }
    }

    public class RE_SelectedUnit
    {
        public int UnitId { get; set; }
        public string UnitType { get; set; }
        public int Qty { get; set; }
        public decimal Size { get; set; }
        public decimal Tariff { get; set; }
        public decimal LineTotal { get; set; }
    }
}
