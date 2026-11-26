using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;

namespace C8.eServices.Mvc.Controllers
{
    /// <summary>
    /// DPRE UC05 Scenarios 2-4: one shared form for Outdoor Advertising (DPRE-OA), Telecommunications (DPRE-TEL)
    /// and Concession/Leasehold (DPRE-LH: Social, Agricultural, Servitude), with Save and Continue.
    /// After submission these applications follow the same back-office workflow as Business Hub (DPRE-Q48 assumption).
    /// </summary>
    public partial class RealEstateController
    {
        /// <summary>A supporting document on the site form.</summary>
        public class RE_SiteDocument
        {
            public string Key { get; set; }
            public string Name { get; set; }
            public int DocumentTypeId { get; set; }
            public bool Multiple { get; set; }
            public string Hint { get; set; }
            public Func<string, bool, bool, bool> Applies { get; set; }     // (formCode, isIndividual, hasVat) => shown
            public Func<string, bool, bool, bool> Mandatory { get; set; }   // (formCode, isIndividual, hasVat) => required
        }

        private static readonly string[] CompanyOrCc = { "Company", "Close Corporation" };

        /// <summary>
        /// Supporting documents per form (SDS UC05 Scenario 2 Step 23, Scenario 3 Step 11, Scenario 4 Step 11).
        /// ASSUMPTIONS: proof of the application fee is required at submission for every type, as for Business Hub (DPRE-Q39);
        /// person-only documents apply to Individuals and entity documents to the other applicant types (DPRE-Q40);
        /// proof of income stays optional because it applies only to salaried persons or pensioners.
        /// </summary>
        public static readonly List<RE_SiteDocument> SiteDocuments = new List<RE_SiteDocument>
        {
            new RE_SiteDocument { Key = "file_Motivation", Name = "Motivation for the need to lease the property", DocumentTypeId = 18,
                Applies = (f, ind, vat) => f != "LH", Mandatory = (f, ind, vat) => f != "LH" },
            new RE_SiteDocument { Key = "file_RegCert", Name = "Company/CC/NPO/NGO Registration Certificate", DocumentTypeId = 18,
                Applies = (f, ind, vat) => !ind, Mandatory = (f, ind, vat) => !ind },
            new RE_SiteDocument { Key = "file_Directors", Name = "List of Directors or Members", DocumentTypeId = 18, Hint = "Company or Close Corporation",
                Applies = (f, ind, vat) => !ind, Mandatory = (f, ind, vat) => false },
            new RE_SiteDocument { Key = "file_Vat", Name = "VAT Registration Certificate", DocumentTypeId = 18, Hint = "Required when a VAT number is given",
                Applies = (f, ind, vat) => true, Mandatory = (f, ind, vat) => vat },
            new RE_SiteDocument { Key = "file_Id", Name = "Copy of ID Document", DocumentTypeId = 1,
                Applies = (f, ind, vat) => ind, Mandatory = (f, ind, vat) => ind },
            new RE_SiteDocument { Key = "file_Income", Name = "Proof of Income", DocumentTypeId = 59, Hint = "Salaried persons or pensioners",
                Applies = (f, ind, vat) => ind, Mandatory = (f, ind, vat) => false },
            new RE_SiteDocument { Key = "file_Residence", Name = "Proof of Residence", DocumentTypeId = 54,
                Applies = (f, ind, vat) => ind, Mandatory = (f, ind, vat) => ind },
            new RE_SiteDocument { Key = "file_Locality", Name = "Locality Plan of the property or erf", DocumentTypeId = 18, Hint = "Obtainable from DPRE",
                Applies = (f, ind, vat) => true, Mandatory = (f, ind, vat) => true },
            new RE_SiteDocument { Key = "file_Zoning", Name = "Copy of the Zoning Certificate", DocumentTypeId = 18, Hint = "Obtainable from DPRE",
                Applies = (f, ind, vat) => true, Mandatory = (f, ind, vat) => true },
            new RE_SiteDocument { Key = "file_Plans", Name = "Plans/Drawings (supporting annexures)", DocumentTypeId = 18, Multiple = true,
                Applies = (f, ind, vat) => f == "TEL", Mandatory = (f, ind, vat) => f == "TEL" },
            new RE_SiteDocument { Key = "file_Fee", Name = "Proof of Application Fee Payment", DocumentTypeId = 18,
                Applies = (f, ind, vat) => true, Mandatory = (f, ind, vat) => true },
        };

        private static string FormTitleFor(string formCode)
        {
            switch (formCode)
            {
                case "OA": return "Outdoor Advertising Lease Application";
                case "TEL": return "Telecommunications Lease Application";
                default: return "Concession/Leasehold Lease Application";
            }
        }

        private void PopulateSiteViewBags()
        {
            ViewBag.CCCList = db.CCCs.Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.CCCName)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CCCName })
                .ToList();
            ViewBag.SiteDocuments = SiteDocuments;
        }

        private RE_SiteApplicationViewModel BuildSiteViewModel(RE_LeaseLookups.LeaseApplicationType type, RE_Application app, RE_ApplicationSiteDetail site)
        {
            var vm = new RE_SiteApplicationViewModel
            {
                Application = app,
                Site = site,
                LeaseType = type.Name,
                FormCode = type.RefCode,
                FormTitle = FormTitleFor(type.RefCode),
                ReferencePrefix = "DPRE-" + type.RefCode
            };
            if (app != null && app.Id > 0)
            {
                vm.UploadedDocuments = db.Documents
                    .Where(d => d.RealEstateApplicationId == app.Id && d.IsActive && !d.IsDeleted)
                    .Select(d => d.DocumentName)
                    .ToList();
            }
            return vm;
        }

        private RE_Application LoadOwnDraft(int id)
        {
            return db.RE_Applications.Include(a => a.Status)
                .FirstOrDefault(a => a.Id == id && a.CustomerId == _customer.Id && !a.IsDeleted && a.Status.Key == StatusKeys.ReApplicationDraft);
        }

        // GET: RealEstate/CaptureSite?leaseType=Outdoor%20Advertising   or   /RealEstate/CaptureSite?draftId=12
        public ActionResult CaptureSite(string leaseType, int? draftId)
        {
            if (!Request.IsAuthenticated) return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            Initialise();
            if (_customer == null) return RedirectToAction("Login", "Account");

            RE_Application app;
            RE_ApplicationSiteDetail site;
            if (draftId.HasValue)
            {
                app = LoadOwnDraft(draftId.Value);
                if (app == null) return HttpNotFound();
                site = db.RE_ApplicationSiteDetails.FirstOrDefault(s => s.RE_ApplicationId == app.Id) ?? new RE_ApplicationSiteDetail();
                leaseType = app.LeaseType;
                ClearDraftPlaceholders(app);
            }
            else
            {
                app = new RE_Application
                {
                    LeaseType = leaseType,
                    SystemUserId = _systemUser.Id,
                    CustomerId = _customer.Id,
                    EntityEmail = _systemUser.EmailAddress,
                    EntityMobile = _systemUser.MobileNumber,
                    EntityRegisteredAddress = (_customer.PhysicalAddress1 + " " + _customer.PhysicalAddress2).Trim(),
                    EntityRegisteredPostalCode = Convert.ToString(_customer.PhysicalAddressCode)
                };
                site = new RE_ApplicationSiteDetail
                {
                    FirstName = _customer.FirstName,
                    Surname = _customer.LastName,
                    PostalAddress = (_customer.PostalAddress1 + " " + _customer.PostalAddress2).Trim()
                };
            }

            var type = RE_LeaseLookups.FindLeaseApplicationType(leaseType);
            if (type == null) return RedirectToAction("LeaseApplications");
            if (type.RefCode == "BUS") return RedirectToAction("Capture", new { leaseType = type.Name });

            PopulateSiteViewBags();
            return View(BuildSiteViewModel(type, app, site));
        }

        // POST: RealEstate/CaptureSite   submitAction = "draft" (Save and Continue) or "submit"
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]   // signature pad posts a data URL; every other field is encoded on output
        public ActionResult CaptureSite(RE_SiteApplicationViewModel model, string submitAction)
        {
            if (!Request.IsAuthenticated) return RedirectToAction("Login", "Account");
            Initialise();
            if (_customer == null) return RedirectToAction("Login", "Account");

            var type = RE_LeaseLookups.FindLeaseApplicationType(model.LeaseType);
            if (type == null || type.RefCode == "BUS") return RedirectToAction("LeaseApplications");

            bool isDraft = submitAction == "draft";
            var posted = model.Application ?? new RE_Application();
            var postedSite = model.Site ?? new RE_ApplicationSiteDetail();

            // Load the existing draft (if any) so nothing outside the form is overwritten.
            RE_Application app = null;
            RE_ApplicationSiteDetail site = null;
            if (posted.Id > 0)
            {
                app = LoadOwnDraft(posted.Id);
                if (app == null) return HttpNotFound();
                site = db.RE_ApplicationSiteDetails.FirstOrDefault(s => s.RE_ApplicationId == app.Id);
            }

            // Server-generated / not-on-form fields are not validated from the post.
            foreach (var key in ModelState.Keys.ToList())
                if (key.StartsWith("Application.") || key.StartsWith("Site."))
                    ModelState.Remove(key);

            NormaliseSitePost(posted, postedSite, type.RefCode);
            bool isIndividual = posted.ApplicantType == "Individual/Natural Person";
            bool hasVat = !string.IsNullOrWhiteSpace(posted.VatRegistrationNumber);

            var existingDocs = app != null
                ? db.Documents.Where(d => d.RealEstateApplicationId == app.Id && d.IsActive && !d.IsDeleted).Select(d => d.DocumentName).ToList()
                : new List<string>();

            if (!isDraft)
            {
                ValidateSiteApplication(posted, postedSite, type.RefCode, isIndividual);
                foreach (var doc in SiteDocuments.Where(d => d.Applies(type.RefCode, isIndividual, hasVat) && d.Mandatory(type.RefCode, isIndividual, hasVat)))
                {
                    bool uploaded = PostedFiles(doc.Key).Any() || existingDocs.Any(n => n == doc.Name || n.StartsWith(doc.Name + ":"));
                    if (!uploaded) ModelState.AddModelError(doc.Key, doc.Name + " is required.");
                }
                foreach (var key in SiteDocuments.Select(d => d.Key))
                    foreach (var f in PostedFiles(key))
                        if (f.ContentLength > 10 * 1024 * 1024)
                            ModelState.AddModelError(key, Path.GetFileName(f.FileName) + " is larger than 10 MB.");
            }
            else if (posted.CCCId <= 0)
            {
                // A draft still needs a CCA because RE_Applications.CCCId is mandatory.
                ModelState.AddModelError("Application.CCCId", "Select a Customer Care Area before saving.");
            }

            if (!ModelState.IsValid)
            {
                PopulateSiteViewBags();
                var vm = BuildSiteViewModel(type, posted, postedSite);
                if (app != null) vm.UploadedDocuments = existingDocs;
                return View(vm);
            }

            try
            {
                var now = DateTime.Now;
                var deptId = db.ApplicationEntities.FirstOrDefault(ae => ae.Key == ApplicationEntityKeys.RealEstateDevelopment)?.Id;
                var draftStatusId = db.Status.Where(s => s.Key == StatusKeys.ReApplicationDraft).Select(s => (int?)s.Id).FirstOrDefault();
                if (isDraft && draftStatusId == null)
                    throw new Exception("Status re_application_draft is missing. Run DatabaseScripts/RE_ApplicationSiteDetails_Create.sql.");

                bool isNewApp = app == null;
                if (isNewApp)
                {
                    app = new RE_Application
                    {
                        SystemUserId = _systemUser.Id,
                        CustomerId = _customer.Id,
                        DepartmentId = deptId,
                        CreatedBySystemUserId = _systemUser.Id,
                        CreatedDateTime = now,
                        // Placeholder until submission; the real reference is issued on Submit.
                        ApplicationReferenceNumber = "DRAFT-" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant()
                    };
                }
                CopyApplicationFields(posted, app);
                FillStorageDefaults(app);
                app.LeaseType = type.Name;
                app.ModifiedBySystemUserId = _systemUser.Id;
                app.ModifiedDateTime = now;

                string referenceNumber = null;
                if (isDraft)
                {
                    app.StatusId = draftStatusId.Value;
                }
                else
                {
                    referenceNumber = GenerateReferenceNumber(type.RefCode);
                    app.ApplicationReferenceNumber = referenceNumber;
                    app.StatusId = db.Status.First(s => s.Key == StatusKeys.AwaitingApplicationFeeValidation).Id;
                }

                if (isNewApp) db.RE_Applications.Add(app);
                db.SaveChanges();

                if (site == null)
                {
                    site = new RE_ApplicationSiteDetail
                    {
                        RE_ApplicationId = app.Id,
                        DepartmentId = deptId,
                        CreatedBySystemUserId = _systemUser.Id,
                        CreatedDateTime = now
                    };
                    db.RE_ApplicationSiteDetails.Add(site);
                }
                CopySiteFields(postedSite, site);
                site.FormCode = type.RefCode;
                site.ModifiedBySystemUserId = _systemUser.Id;
                site.ModifiedDateTime = now;
                if (site.TermsAccepted && site.TermsAcceptedDateTime == null) site.TermsAcceptedDateTime = now;
                if (!isDraft) site.SubmittedDateTime = now;
                db.SaveChanges();

                // Files uploaded with a draft are kept, so the applicant does not upload them again.
                int refTypeId = db.ReferenceTypes.FirstOrDefault(r => r.Key == ReferenceTypeKeys.RealEstateApplication)?.Id ?? 16;
                foreach (var doc in SiteDocuments)
                {
                    var files = PostedFiles(doc.Key).ToList();
                    for (int i = 0; i < files.Count; i++)
                    {
                        var name = doc.Multiple || files.Count > 1 ? doc.Name + ": " + Path.GetFileName(files[i].FileName) : doc.Name;
                        SaveAndLinkFile(files[i], name, app.Id, doc.DocumentTypeId, refTypeId);
                    }
                }
                foreach (var other in PostedFiles("file_Other"))
                    SaveAndLinkFile(other, "Other Supporting Document: " + Path.GetFileName(other.FileName), app.Id, 18, refTypeId);

                if (isDraft)
                {
                    TempData["SuccessMessage"] = "Your application has been saved. You can continue it later from My Applications.";
                    return RedirectToAction("CaptureSite", new { draftId = app.Id });
                }

                RealEstateWorkAllocationHelper.AssignRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateVerifyPayment);

                try
                {
                    var email = new Email();
                    string subject = "Real Estate Development: " + type.Name + " Lease Application Submitted";
                    string body = string.Format("Dear {0},<br/><br/>Your {1} lease application has been submitted successfully. Your Reference Number is: <strong>{2}</strong>.<br/><br/>Status: Application Submitted, Pending Review.",
                        HttpUtility.HtmlEncode(_systemUser.FullName), HttpUtility.HtmlEncode(type.Name), referenceNumber);
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
                EventLogHelper.LogSystemError(ex.ToString(), LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                ModelState.AddModelError("", "An error occurred while saving your application. Please try again.");
                PopulateSiteViewBags();
                return View(BuildSiteViewModel(type, posted, postedSite));
            }
        }

        private IEnumerable<HttpPostedFileBase> PostedFiles(string key)
        {
            for (int i = 0; i < Request.Files.Count; i++)
            {
                if (Request.Files.GetKey(i) != key) continue;
                var f = Request.Files[i];
                if (f != null && f.ContentLength > 0) yield return f;
            }
        }

        /// <summary>Fills the mandatory RE_Applications columns from the type-specific answers so all types share one table.</summary>
        private static void NormaliseSitePost(RE_Application app, RE_ApplicationSiteDetail site, string formCode)
        {
            if (app.ApplicantType == "Individual/Natural Person")
            {
                var full = ((site.FirstName ?? "") + " " + (site.Surname ?? "")).Trim();
                if (!string.IsNullOrEmpty(full)) app.EntityName = full;
            }

            switch (formCode)
            {
                case "OA":
                    app.PurposeOfLease = "Outdoor Advertising: " + (site.SignType == "Other" ? site.SignTypeOther : site.SignType);
                    if (site.SignLocationType == RE_LeaseLookups.SignLocationTypes[2])
                    {
                        // Road reserve: the location is described by street names rather than an erf.
                        app.PropertyAddress = string.Format("Road reserve, {0} (nearest intersection {1})", site.RoadReserveStreetName, site.RoadReserveNearestIntersection);
                    }
                    if (string.IsNullOrWhiteSpace(app.TownshipSuburbFarmName)) app.TownshipSuburbFarmName = site.Town;
                    break;
                case "TEL":
                    app.PurposeOfLease = "Telecommunications: " + (site.InstallationType == "Other" ? site.InstallationTypeOther : site.InstallationType);
                    break;
                default:
                    app.PurposeOfLease = site.ProposedUse != null && site.ProposedUse.Length > 100 ? site.ProposedUse.Substring(0, 100) : site.ProposedUse;
                    break;
            }
        }

        private void ValidateSiteApplication(RE_Application app, RE_ApplicationSiteDetail site, string formCode, bool isIndividual)
        {
            Action<bool, string, string> req = (ok, key, msg) => { if (!ok) ModelState.AddModelError(key, msg); };
            Func<string, bool> has = s => !string.IsNullOrWhiteSpace(s);

            // Section A: property / site
            req(app.CCCId > 0, "Application.CCCId", "Select the Customer Care Area.");
            if (formCode == "OA")
            {
                req(has(site.SignLocationType), "Site.SignLocationType", "Select where the sign will be erected.");
                req(has(site.SignType), "Site.SignType", "Select the sign type.");
                req(site.SignType != "Other" || has(site.SignTypeOther), "Site.SignTypeOther", "Specify the sign type.");
                req(site.SignIlluminated.HasValue, "Site.SignIlluminated", "Indicate whether the sign will be illuminated.");
                req(has(site.SignElevations), "Site.SignElevations", "Select the number of elevations.");
                req(site.SignElevations != "Other" || has(site.SignElevationsOther), "Site.SignElevationsOther", "Indicate the number of elevations.");
                req(has(site.Town), "Site.Town", "Select the town where the sign will be erected.");
                if (site.SignLocationType == RE_LeaseLookups.SignLocationTypes[2])
                {
                    req(has(site.RoadReserveStreetName), "Site.RoadReserveStreetName", "Street Name is required.");
                    req(has(site.RoadReserveNearestIntersection), "Site.RoadReserveNearestIntersection", "Name of Nearest Intersecting Street is required.");
                    req(has(site.RoadReserveAdjacentProperty), "Site.RoadReserveAdjacentProperty", "Description of Adjacent Property is required.");
                }
                else
                {
                    req(has(app.PropertyAddress), "Application.PropertyAddress", "Physical Address of the property is required.");
                }
                if (site.SignLocationType == RE_LeaseLookups.SignLocationTypes[1])
                    req(has(site.FarmPortionDescription), "Site.FarmPortionDescription", "Farm Portion/Agricultural Holding Description is required.");
            }
            else
            {
                req(has(app.PropertyAddress), "Application.PropertyAddress", "Physical Address is required.");
                req(has(site.Zoning), "Site.Zoning", "Zoning is required.");
                req(site.ProposedLeaseYears.HasValue && site.ProposedLeaseYears >= 0 && site.ProposedLeaseYears <= 99,
                    "Site.ProposedLeaseYears", "Proposed Lease Term (years) is required (0 to 99).");
            }
            req(has(app.ErfFarmNumber), "Application.ErfFarmNumber", "ERF/Farm Number is required.");
            req(has(app.TownshipSuburbFarmName), "Application.TownshipSuburbFarmName", "Township/Suburb is required.");
            req(has(app.PropertyPostalCode) && Regex.IsMatch(app.PropertyPostalCode, @"^\d{4}$"), "Application.PropertyPostalCode", "Postal Code must be exactly 4 digits.");

            if (formCode == "TEL")
            {
                req(has(site.SiteName), "Site.SiteName", "Site Name is required.");
                req(site.Latitude.HasValue && site.Latitude >= RE_LeaseLookups.EkurhuleniMinLat && site.Latitude <= RE_LeaseLookups.EkurhuleniMaxLat,
                    "Site.Latitude", "Latitude must be in decimal degrees within Ekurhuleni (between -26.55 and -25.90).");
                req(site.Longitude.HasValue && site.Longitude >= RE_LeaseLookups.EkurhuleniMinLng && site.Longitude <= RE_LeaseLookups.EkurhuleniMaxLng,
                    "Site.Longitude", "Longitude must be in decimal degrees within Ekurhuleni (between 27.95 and 28.75).");
                req(has(site.InstallationType), "Site.InstallationType", "Select the telecommunications installation.");
                req(site.InstallationType != "Other" || has(site.InstallationTypeOther), "Site.InstallationTypeOther", "Specify the installation.");
                req((site.AreaOnStructureSqm ?? 0) > 0 || (site.AreaGroundSqm ?? 0) > 0, "Site.AreaGroundSqm", "Enter the area to be leased (on the structure, at ground level, or both).");
                req(!site.ProposedLeaseMonths.HasValue || (site.ProposedLeaseMonths >= 0 && site.ProposedLeaseMonths <= 11), "Site.ProposedLeaseMonths", "Months must be between 0 and 11.");
                req((site.ProposedLeaseYears ?? 0) + (site.ProposedLeaseMonths ?? 0) > 0, "Site.ProposedLeaseYears", "Enter the proposed lease term.");
                req(site.OptionToRenew.HasValue, "Site.OptionToRenew", "Indicate whether you want an option to renew.");
                req(site.CurrentlySublet.HasValue, "Site.CurrentlySublet", "Indicate whether the site is currently sublet.");
                req(site.CurrentlySublet != true || (site.CurrentCoLocators ?? 0) > 0, "Site.CurrentCoLocators", "Indicate the number of co-locators/sub-lessees.");
                req(site.WillBeSublet.HasValue, "Site.WillBeSublet", "Indicate whether the site will be sublet.");
                req(site.WillBeSublet != true || (site.AnticipatedCoLocators ?? 0) > 0, "Site.AnticipatedCoLocators", "Indicate the number of anticipated co-locators/sub-lessees.");
            }
            if (formCode == "LH")
            {
                req(has(site.ProposedUse), "Site.ProposedUse", "Define and explain the proposed use.");
                req((site.TotalExtentSqm ?? 0) > 0, "Site.TotalExtentSqm", "Total Extent (size) required must be greater than 0.");
            }

            // Section B: applicant
            req(has(app.ApplicantType), "Application.ApplicantType", "Select the Applicant Type.");
            req(app.ApplicantType != "Other" || has(site.ApplicantTypeOther), "Site.ApplicantTypeOther", "Specify the applicant type.");
            if (isIndividual)
            {
                req(has(site.FirstName), "Site.FirstName", "First Name is required.");
                req(has(site.Surname), "Site.Surname", "Surname is required.");
                if (site.IdentificationType == "Passport Number")
                    req(has(app.CompanyRegistrationNumber), "Application.CompanyRegistrationNumber", "Passport Number is required.");
                else
                    req(has(app.CompanyRegistrationNumber) && Regex.IsMatch(app.CompanyRegistrationNumber, @"^\d{13}$"),
                        "Application.CompanyRegistrationNumber", "Enter a valid 13-digit RSA ID number.");
            }
            else
            {
                req(has(app.EntityName), "Application.EntityName", "Name of the Business/Entity is required.");
                req(has(app.CompanyRegistrationNumber), "Application.CompanyRegistrationNumber", "Registration Number is required.");
                req(has(app.AuthorizedRepresentativeName), "Application.AuthorizedRepresentativeName", "Full Name of Authorised Representative is required.");
            }
            req(has(site.IdentificationType), "Site.IdentificationType", "Select the Identification Type.");
            if (has(app.VatRegistrationNumber))
                req(Regex.IsMatch(app.VatRegistrationNumber, @"^4\d{9}$"), "Application.VatRegistrationNumber", "VAT Registration Number must be 10 digits starting with 4.");
            req(has(app.EntityMobile) && Regex.IsMatch(app.EntityMobile, @"^0\d{9}$"), "Application.EntityMobile", "Mobile Number must be 10 digits starting with 0.");
            if (has(app.EntityTelephone))
                req(Regex.IsMatch(app.EntityTelephone, @"^0\d{9}$"), "Application.EntityTelephone", "Telephone Number must be 10 digits starting with 0.");
            req(has(app.EntityEmail) && Regex.IsMatch(app.EntityEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"), "Application.EntityEmail", "A valid E-mail Address is required.");
            req(has(app.EntityRegisteredAddress), "Application.EntityRegisteredAddress", "Registered/Residential Address is required.");
            req(has(site.PostalAddress), "Site.PostalAddress", "Postal Address is required.");

            // Terms and submission
            req(site.TermsAccepted, "Site.TermsAccepted", "Please tick the checkbox or abandon the application.");
            req(has(site.SignatoryName), "Site.SignatoryName", "Full Name of Applicant/Authorised Representative is required.");
            req(has(site.SignatoryCapacity), "Site.SignatoryCapacity", "Capacity is required.");
            req(has(site.SignatureImage) && site.SignatureImage.StartsWith("data:image/png;base64,") && site.SignatureImage.Length < 400000,
                "Site.SignatureImage", "Please sign in the signature box.");
        }

        private static readonly string[] ApplicationFormFields =
        {
            "ApplicantType", "EntityName", "CompanyRegistrationNumber", "VatRegistrationNumber", "TaxReferenceNumber",
            "EntityRegisteredAddress", "EntityRegisteredPostalCode", "AuthorizedRepresentativeName", "AuthorizedRepresentativeCapacity",
            "EntityTelephone", "EntityMobile", "EntityFax", "EntityEmail", "PurposeOfLease", "CCCId",
            "ErfFarmNumber", "PropertyAddress", "TownshipSuburbFarmName", "PropertyPostalCode"
        };

        private static void CopyApplicationFields(RE_Application from, RE_Application to)
        {
            foreach (var name in ApplicationFormFields)
            {
                var p = typeof(RE_Application).GetProperty(name);
                var v = p.GetValue(from);
                if (v is string) v = ((string)v).Trim();
                p.SetValue(to, v);
            }
        }

        // RE_Applications has NOT NULL / [Required] columns that a draft may not have filled in yet.
        private const string DraftPlaceholder = "-";
        private static readonly string[] RequiredApplicationText = { "ApplicantType", "PurposeOfLease", "ErfFarmNumber", "PropertyAddress", "TownshipSuburbFarmName", "PropertyPostalCode" };
        private static readonly string[] NotNullApplicationText = { "BankName", "BankAccountType", "BankAccountName", "BankAccountNumber", "BankBranchCode" };

        /// <summary>
        /// ASSUMPTION (DPRE-Q42): banking details are not asked at application stage (POPIA); they are captured once the lease is approved.
        /// The NOT NULL bank columns are stored empty.
        /// </summary>
        private static void FillStorageDefaults(RE_Application app)
        {
            foreach (var name in RequiredApplicationText)
            {
                var p = typeof(RE_Application).GetProperty(name);
                if (string.IsNullOrWhiteSpace((string)p.GetValue(app))) p.SetValue(app, DraftPlaceholder);
            }
            foreach (var name in NotNullApplicationText)
            {
                var p = typeof(RE_Application).GetProperty(name);
                if (p.GetValue(app) == null) p.SetValue(app, "");
            }
        }

        private static void ClearDraftPlaceholders(RE_Application app)
        {
            foreach (var name in RequiredApplicationText)
            {
                var p = typeof(RE_Application).GetProperty(name);
                if ((string)p.GetValue(app) == DraftPlaceholder) p.SetValue(app, null);
            }
        }

        private static readonly string[] SiteSystemFields = { "Id", "RE_ApplicationId", "FormCode", "TermsAcceptedDateTime", "SubmittedDateTime" };

        private static void CopySiteFields(RE_ApplicationSiteDetail from, RE_ApplicationSiteDetail to)
        {
            var props = typeof(RE_ApplicationSiteDetail).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            foreach (var p in props)
            {
                if (!p.CanWrite || SiteSystemFields.Contains(p.Name) || p.Name == "Application") continue;
                var v = p.GetValue(from);
                if (v is string) v = ((string)v).Trim();
                p.SetValue(to, v);
            }
        }
    }
}
