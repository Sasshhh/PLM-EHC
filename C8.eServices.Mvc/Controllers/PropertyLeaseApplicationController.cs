using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Globalization;
using System.Web.Routing;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using iTextSharp.text.pdf;
using System.Net.Sockets;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using C8.eServices.Mvc.ApiServices;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.ViewModels;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Helpers.UnitEngine.Abstract;
using C8.eServices.Mvc.Helpers.UnitEngine.Concrete;
using Microsoft.Ajax.Utilities;
using System.Data.Common;
using System.Text.RegularExpressions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.Runtime.Remoting.Lifetime;
using System.Web.Services.Discovery;
using Microsoft.SharePoint.Client;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography.Xml;
using Org.BouncyCastle.Ocsp;
using static System.Net.Mime.MediaTypeNames;
using System.Web.UI.WebControls;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class PropertyLeaseApplicationController : Controller
    {

        public string encp = "spgencpassp";
        private eServicesDbContext db = new eServicesDbContext();
        private static Random random = new Random();
        BaseHelper _base = new BaseHelper();
        private readonly IUnitAllocation unitAllocationService;

        public PropertyLeaseApplicationController()
        {
            UserStore<SystemIdentityUser> store = new UserStore<SystemIdentityUser>(db);
            UserManager = new UserManager<SystemIdentityUser>(store);
            unitAllocationService = new UnitAllocation(db);
            Initialise();
        }
        #region Init
        public UserManager<SystemIdentityUser> UserManager { get; private set; }
        public PropertyLeaseApplicationController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
            Initialise();
        }

        public PropertyLeaseApplicationController(eServicesDbContext context)
        {

            UserManager =
            new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(context));
        }

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

        #endregion


        public bool ActivityTrackerAudit(int PropertyId, string ActivityTrackerMessage, int CustomerID)
        {
            try
            {
                PLMApplicationHistortyLog newlog = new PLMApplicationHistortyLog();

                newlog.PropertyLeaseApplicationId = PropertyId;
                newlog.AuditAction = ActivityTrackerMessage;
                newlog.UserId = CustomerID;
                newlog.CreatedDateTime = DateTime.Now;
                newlog.IsActive = true;
                newlog.IsDeleted = false;
                newlog.IsLocked = false;

                db.PLMApplicationHistortyLogs.Add(newlog);
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

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [DecryptParameter]
        public bool pdfDenerateDebitOrderAuthority(int? ApplicationId)
        {
            var application = db.PropertyLeaseApplications.Where(x => x.Id == ApplicationId).FirstOrDefault();
            var DebitOrderRecord = db.DebitOrderRegistrations.OrderByDescending(r=>r.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id);

            var template = db.AppSettings.FirstOrDefault(r => r.Key == AppSettingKeys.DO_TEMP_PDF).Value;

            string pdfTemplate = "";
            string IP = System.Web.HttpContext.Current.Request.UserHostAddress;
            var get_server = IP == "::1" ? pdfTemplate = Server.MapPath("~/PDFTemplates/DO_AuthorityForm.pdf") : pdfTemplate = Server.MapPath(template);

            var timestamp2 = DateTime.Now.ToString("ddMMyyyyHHmmss");
            string folderName = Server.MapPath("~/Templates");
            string pathString = System.IO.Path.Combine(folderName, timestamp2);
            System.IO.Directory.CreateDirectory(pathString);
            string nFolderName = null;
            string newFile = nFolderName = folderName;
            Random rnd = new Random();
            int randomNum = rnd.Next(1, 51);
            newFile = nFolderName + "\\" + timestamp2+"_"+ application.IDNo+ "DEBITORDER.pdf";
            var filename = application.ApplicationReferenceNumber + ".DEBITORDER.pdf";
            var sign = application.FirstName.Substring(0,1) + application.LastName.Substring(0,1) + " " + application.IDNo;

            var revUser = db.SystemUsers.FirstOrDefault(x => x.Id == DebitOrderRecord.RevenueOfficerId);
            var sign_revenue = revUser == null ? null : revUser.FirstName.Substring(0, 1) + revUser.LastName.Substring(0, 1) + " " + revUser.ServiceNo;
            var leaUser = db.SystemUsers.FirstOrDefault(x => x.Id == DebitOrderRecord.LeasingOfficerId);
            var sign_lease = leaUser == null ? null : leaUser.FirstName.Substring(0, 1) + leaUser.LastName.Substring(0, 1) + " " + leaUser.IdentificationNumber;

            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
            AcroFields pdfFormFields = pdfStamper.AcroFields;

            if (application != null)
            {
                pdfFormFields.SetField("Name_of_Applicant", application.FirstName.ToString()+" "+ application.LastName.ToString());
                pdfFormFields.SetField("Unit_no", DebitOrderRecord.UnitNumber.ToString());
                pdfFormFields.SetField("Reference_no", application.ApplicationReferenceNumber.ToString());
                pdfFormFields.SetField("ID_no", application.IDNo.ToString());
                pdfFormFields.SetField("Email", DebitOrderRecord.Email.ToString());
                pdfFormFields.SetField("Cell_no", DebitOrderRecord.CellNo.ToString());
                pdfFormFields.SetField("Bank_Name_&_number", DebitOrderRecord.BankName.ToString()+", "+ DebitOrderRecord.BankNumber.ToString());
                pdfFormFields.SetField("Rent_amount", Convert.ToDouble(DebitOrderRecord.RentalAmount).ToString("c"));
                pdfFormFields.SetField("Debit_check_action_date", DebitOrderRecord.DebitCheckActionDate.ToString().Substring(0,10));
                pdfFormFields.SetField("Tenant_signature", sign);
                var leaseng = DebitOrderRecord.LeasingSignature == true ? pdfFormFields.SetField("Leasing_Officer", DebitOrderRecord.LeasingOfficer.ToString()) && pdfFormFields.SetField("leasing_officer_Signature", sign_lease.ToString()) : true;
                var debit = DebitOrderRecord.RevenueSignature == true ? pdfFormFields.SetField("Revenue_Officer", DebitOrderRecord.RevenueOfficer.ToString()) && pdfFormFields.SetField("revenue_officer__signature", sign_revenue.ToString()) : true;
                var result = DebitOrderRecord.KeyNo != null ? pdfFormFields.SetField("Key_no", application.FirstName.ToString()) : true;
                var result2 = DebitOrderRecord.MenterNumber != null ? pdfFormFields.SetField("Meter_no", application.FirstName.ToString()) : true;
            }
            pdfStamper.FormFlattening = true;
            pdfStamper.Close();
            string ReportURL = newFile;
            byte[] temp = System.IO.File.ReadAllBytes(ReportURL);

            Response.Clear();
            MemoryStream ms = new MemoryStream(temp);
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename="+ filename);
            Response.Buffer = true;
            ms.WriteTo(Response.OutputStream);
            Response.End();

            return true;
        }



        #region Revised Lease Agreement v1 Helper Methods

        /// <summary>
        /// Generates tenant initials from first name and last name (e.g., "John Doe" -> "JD")
        /// </summary>
        private string GetTenantInitials(PropertyLeaseApplication application)
        {
            if (string.IsNullOrEmpty(application?.FirstName) || string.IsNullOrEmpty(application?.LastName))
                return "";

            return application.FirstName.Substring(0, 1).ToUpper() +
                   application.LastName.Substring(0, 1).ToUpper();
        }

        /// <summary>
        /// Gets the signing location from the preferred complex area (e.g., "Airport Park")
        /// </summary>
        private string GetSignedAtLocation(PropertyLeaseApplication application)
        {
            if (application?.PreferredComplexAreaId.HasValue == true)
            {
                var complex = db.PreferredComplexAreas.Find(application.PreferredComplexAreaId.Value);
                return complex?.Name ?? "Ekurhuleni";
            }
            return "Ekurhuleni";
        }

        /// <summary>
        /// Retrieves building name from the matched unit's ApplicationAllocatedProperty
        /// </summary>
        private string GetBuildingName(int applicationId)
        {
            var matchedUnit = db.MatchedUnits
                .FirstOrDefault(x => x.PropertyLeaseApplicationId == applicationId && x.IsActive && !x.IsDeleted);

            if (matchedUnit?.ApplicationAllocatedPropertyId.HasValue == true)
            {
                var unit = db.ApplicationAllocatedProperty.Find(matchedUnit.ApplicationAllocatedPropertyId.Value);
                return unit?.BuildingName ?? "";
            }
            return "";
        }

        /// <summary>
        /// Calculates total monthly charges (rent + water + refuse + sewerage + optional parking/storeroom)
        /// </summary>
        private double CalculateTotalMonthlyCharges(PropertyLeaseAgreementMaster master)
        {
            double total = 0;

            // Base charges
            total += master.UnitRentalAmountPM;
            total += master._water;
            total += master._refuse;
            total += master._sewerage;

            // Optional services
            if (master.SPP == true) total += master.ShadePortParking;
            if (master.OPP == true) total += master.OpenParking;
            if (master.STR == true) total += master.StoreRooms;

            return total;
        }

        /// <summary>
        /// Sets a PDF form field value with specified font size (more aggressive approach)
        /// </summary>
        private void SetFieldWithFontSize(AcroFields fields, string fieldName, string value, float fontSize)
        {
            try
            {
                // Set the value first
                fields.SetField(fieldName, value ?? "");

                // Try multiple approaches to set font size
                fields.SetFieldProperty(fieldName, "textsize", fontSize, null);
                fields.SetFieldProperty(fieldName, "textfont", "Helvetica", null);

                // Force regenerate appearance
                fields.RegenerateField(fieldName);
            }
            catch (Exception ex)
            {
                // Log but don't fail - field might not exist or be read-only
                System.Diagnostics.Debug.WriteLine($"Failed to set field '{fieldName}': {ex.Message}");
            }
        }

        #endregion

        #region Revised Lease Agreement v1 PDF Generation

        /// <summary>1
        /// Generates PDF for the REVISED Lease Agreement v1 template (85 fields)
        /// This REPLACES the original pdfDeneratePropertyLeaseAgreement() method.
        /// The original method body has been updated to use the new template.
        /// </summary>
        [DecryptParameter]
        public void pdfDeneratePropertyLeaseAgreement(int? ApplicationId)
        {
            if (ApplicationId == null) throw new Exception("Invalid Application.");

            var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == ApplicationId);
            if (application == null) throw new Exception("Application not found.");

            var lease = db.LeaseDetails.OrderByDescending(x => x.Id)
                .FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IsNew && x.IsActive && !x.IsDeleted);
            if (lease == null) throw new Exception("Invalid Property Lease.");

            var master = db.propertyLeaseAgreementMasters
                .FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted);
            if (master == null) throw new Exception("Invalid Lease Agreement.");

            // Template path handling (same pattern as original for consistency)
            // TODO: Add AppSetting key "LA_TEMP_PDF_REVISED" for production deployment
            var templateSetting = db.AppSettings.FirstOrDefault(r => r.Key == "LA_TEMP_PDF_REVISED");
            var template = templateSetting?.Value ?? "";

            string pdfTemplate = "";
            string IP = System.Web.HttpContext.Current.Request.UserHostAddress;

            pdfTemplate = IP == "::1"
                ? Server.MapPath("~/PDFTemplates/Revised Lease Agreement_v2.pdf")
                : Server.MapPath(template);

            var timestamp2 = DateTime.Now.ToString("ddMMyyyyHHmmss");
            string folderName = Server.MapPath("~/Templates");
            string pathString = System.IO.Path.Combine(folderName, timestamp2);
            System.IO.Directory.CreateDirectory(pathString);

            string newFile = folderName + "\\" + timestamp2 + "_" + (application?.IDNo ?? "") + "_RevisedLeaseAgreement.pdf";
            var filename = (application?.ApplicationReferenceNumber ?? "Lease") + "_REVISED_v1.pdf";

            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
            AcroFields pdfFormFields = pdfStamper.AcroFields;

            // Force iTextSharp to regenerate field appearances with our font settings
            pdfStamper.AcroFields.GenerateAppearances = true;

            if (application != null && master != null)
            {
                // ========================================================================
                // CATEGORY 1: EXISTING FIELDS (33 fields)
                // ========================================================================
                SetFieldWithFontSize(pdfFormFields, "AgentName", master.RepresentedBy ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "FullNames", master.ApplicantFullName ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "IdentityNumber", master.ApplicantIdentityNumber ?? application.IDNo ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "UnitNumber", master.UnitNumber ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "UnitBlock", master.BlockNumber ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Rent", master.MonthlyUnitRental.ToString("F2"), 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Deposit", master.InitialDepositPremises.ToString("F2"), 9.0f);
                SetFieldWithFontSize(pdfFormFields, "CreditCheckFee", master.CreditCheckFee == 0 ? "N/A" : master.CreditCheckFee.ToString("F2"), 9.0f);
                SetFieldWithFontSize(pdfFormFields, "AmountRent", master.UnitRentalAmountPM.ToString("F2"), 9.0f);
                SetFieldWithFontSize(pdfFormFields, "AmountWater", master._water.ToString("F2"), 9.0f);
                SetFieldWithFontSize(pdfFormFields, "AmountElectricity", master.ELEC == true ? master.Electricity.ToString("F2") : "Prepaid", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "AmountRefuse", master._refuse.ToString("F2"), 9.0f);
                SetFieldWithFontSize(pdfFormFields, "AmountSewerage", master._sewerage.ToString("F2"), 9.0f);
                SetFieldWithFontSize(pdfFormFields, "ParkingBay", master.CarportParkingBayNumber ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Storeroom", master.STR == true ? master.StoreRooms.ToString("F2") : "N/A", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "CommencementDate", master.CommencementDate ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "SignedDay", master.TenantSignDay ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "SignedMonth", master.TenantSignDate ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "SignedDay2", master.ManagersSignDay ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "SignedMonth2", master.ManagersSignDate ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "LeaseAdministrationFee", master.LeaseAdministrationFee.ToString("F2"), 9.0f);

                // ========================================================================
                // CATEGORY 2: NEW MAPPINGS WITH EXISTING DATA (18 fields)
                // ========================================================================
                SetFieldWithFontSize(pdfFormFields, "Surname", application.LastName ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "CellNumber", application.CellNo ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "WorkNumber", application.WorkNo ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Salary", application.GrossIncome?.ToString("F2") ?? "0.00", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "TenantFullName", $"{application.FirstName} {application.LastName}", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "BuildingName", GetBuildingName(application.Id), 9.0f);
                SetFieldWithFontSize(pdfFormFields, "UnitAddress", lease.LeaAddress ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "AmountTOTAL", CalculateTotalMonthlyCharges(master).ToString("F2"), 9.0f);

                string signedLocation = GetSignedAtLocation(application);
                SetFieldWithFontSize(pdfFormFields, "SignedAt", signedLocation, 9.0f);
                SetFieldWithFontSize(pdfFormFields, "SignedAt2", signedLocation, 9.0f);

                // ========================================================================
                // SUBSIDIES: Set to 0 (9 fields)
                // ========================================================================
                SetFieldWithFontSize(pdfFormFields, "RentSubsidy", "0.00", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "DepositSubsidy", "0.00", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "keySubsidy", "0.00", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "AccessSubsidy", "0.00", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "LeaseAdministrationSubsidy", "0.00", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "KeyDeposit", "0.00", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "AccessCard", "N/A", 9.0f);

                // ========================================================================
                // DSTV: Set to 0/NO (5 fields)
                // ========================================================================
                SetFieldWithFontSize(pdfFormFields, "DSTV", "NO", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "DSTVFee", "0.00", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "AmountDSTV", "0.00", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "DstvMonthlyFee", "0.00", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "DSTVActivationFee", "0.00", 9.0f);

                // ========================================================================
                // EMPLOYER & BANKING: Placeholder (2 fields)
                // ========================================================================
                SetFieldWithFontSize(pdfFormFields, "Employer", "To Be Captured", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "BankingDetails", "To Be Provided", 9.0f);

                // ========================================================================
                // OCCUPANTS: 3 occupants with expanded details (15 fields)
                // ========================================================================
                // Occupant 1
                SetFieldWithFontSize(pdfFormFields, "Occupant1Name", master.OccupantONE ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant1ID", master.OccupantONEIdentityNo ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant1Relationship", master.OccupantONE != null ? "Family Member" : "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant1Contact", master.OccupantONE != null ? application.CellNo ?? "" : "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant1Salary", master.OccupantONE != null ? "0.00" : "", 9.0f);

                // Occupant 2
                SetFieldWithFontSize(pdfFormFields, "Occupant2Name", master.OccupantTWO ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant2ID", master.OccupantTWOIdentityNo ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant2Relationship", master.OccupantTWO != null ? "Family Member" : "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant2Contact", master.OccupantTWO != null ? application.CellNo ?? "" : "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant2Salary", master.OccupantTWO != null ? "0.00" : "", 9.0f);

                // Occupant 3
                SetFieldWithFontSize(pdfFormFields, "Occupant3Name", master.OccupantTHREE ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant3ID", master.OccupantTHREEIdentityNo ?? "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant3Relationship", master.OccupantTHREE != null ? "Family Member" : "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant3Contact", master.OccupantTHREE != null ? application.CellNo ?? "" : "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Occupant3Salary", master.OccupantTHREE != null ? "0.00" : "", 9.0f);

                // ========================================================================
                // UNDEFINED FIELDS: Tenant initials for T&C agreement (14 fields)
                // ========================================================================
                string tenantInitials = GetTenantInitials(application);
                SetFieldWithFontSize(pdfFormFields, "undefined", tenantInitials, 9.0f);
                for (int i = 2; i <= 14; i++)
                {
                    SetFieldWithFontSize(pdfFormFields, $"undefined_{i}", tenantInitials, 9.0f);
                }

                // ========================================================================
                // WITNESSES: Witness1 rendered as signature image below, others left blank
                // ========================================================================
                SetFieldWithFontSize(pdfFormFields, "Witness2", "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Witness3", "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Witness4", "", 9.0f);

                // ========================================================================
                // HEADING COLUMNS: Leave blank (4 fields)
                // ========================================================================
                SetFieldWithFontSize(pdfFormFields, "Subject", "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Description", "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Item", "", 9.0f);
                SetFieldWithFontSize(pdfFormFields, "Item_2", "", 9.0f);

                // ========================================================================
                // SIGNATURES: Render as images (3 signature fields)
                // ========================================================================

                // Tenant Signature
                if (!string.IsNullOrEmpty(master.TenantSignature) && master.TenantSignature.Contains(","))
                {
                    try
                    {
                        string base64Data = master.TenantSignature.Substring(master.TenantSignature.IndexOf(',') + 1);
                        byte[] sigBytes = Convert.FromBase64String(base64Data);
                        iTextSharp.text.Image sigImage = iTextSharp.text.Image.GetInstance(sigBytes);

                        var positions = pdfFormFields.GetFieldPositions("TenantSignature");
                        if (positions != null && positions.Count > 0)
                        {
                            var sigPos = positions[0];
                            iTextSharp.text.Rectangle rect = sigPos.position;
                            sigImage.ScaleToFit(rect.Width, rect.Height);
                            sigImage.SetAbsolutePosition(rect.Left, rect.Bottom);
                            PdfContentByte cb = pdfStamper.GetOverContent(sigPos.page);
                            cb.AddImage(sigImage);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error if needed, but don't fail PDF generation
                        System.Diagnostics.Debug.WriteLine($"Failed to render tenant signature: {ex.Message}");
                    }
                }

                // Property Manager Signature
                if (!string.IsNullOrEmpty(master.PropertyManagersSignature) && master.PropertyManagersSignature.Contains(","))
                {
                    try
                    {
                        string base64Data = master.PropertyManagersSignature.Substring(master.PropertyManagersSignature.IndexOf(',') + 1);
                        byte[] sigBytes = Convert.FromBase64String(base64Data);
                        iTextSharp.text.Image sigImage = iTextSharp.text.Image.GetInstance(sigBytes);

                        var positions = pdfFormFields.GetFieldPositions("PropertyManagerSignature");
                        if (positions != null && positions.Count > 0)
                        {
                            var sigPos = positions[0];
                            iTextSharp.text.Rectangle rect = sigPos.position;
                            sigImage.ScaleToFit(rect.Width, rect.Height);
                            sigImage.SetAbsolutePosition(rect.Left, rect.Bottom);
                            PdfContentByte cb = pdfStamper.GetOverContent(sigPos.page);
                            cb.AddImage(sigImage);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to render property manager signature: {ex.Message}");
                    }
                }

                // Revenue Manager Signature
                if (!string.IsNullOrEmpty(master.RevenueManagersSignature) && master.RevenueManagersSignature.Contains(","))
                {
                    try
                    {
                        string base64Data = master.RevenueManagersSignature.Substring(master.RevenueManagersSignature.IndexOf(',') + 1);
                        byte[] sigBytes = Convert.FromBase64String(base64Data);
                        iTextSharp.text.Image sigImage = iTextSharp.text.Image.GetInstance(sigBytes);

                        var positions = pdfFormFields.GetFieldPositions("RevenueManagerSignature");
                        if (positions != null && positions.Count > 0)
                        {
                            var sigPos = positions[0];
                            iTextSharp.text.Rectangle rect = sigPos.position;
                            sigImage.ScaleToFit(rect.Width, rect.Height);
                            sigImage.SetAbsolutePosition(rect.Left, rect.Bottom);
                            PdfContentByte cb = pdfStamper.GetOverContent(sigPos.page);
                            cb.AddImage(sigImage);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to render revenue manager signature: {ex.Message}");
                    }
                }

                // Witness 1 Signature
                if (!string.IsNullOrEmpty(master.Witness1Signature) && master.Witness1Signature.Contains(","))
                {
                    try
                    {
                        string base64Data = master.Witness1Signature.Substring(master.Witness1Signature.IndexOf(',') + 1);
                        byte[] sigBytes = Convert.FromBase64String(base64Data);
                        iTextSharp.text.Image sigImage = iTextSharp.text.Image.GetInstance(sigBytes);

                        var positions = pdfFormFields.GetFieldPositions("Witness1");
                        if (positions != null && positions.Count > 0)
                        {
                            var sigPos = positions[0];
                            iTextSharp.text.Rectangle rect = sigPos.position;
                            sigImage.ScaleToFit(rect.Width, rect.Height);
                            sigImage.SetAbsolutePosition(rect.Left, rect.Bottom);
                            PdfContentByte cb = pdfStamper.GetOverContent(sigPos.page);
                            cb.AddImage(sigImage);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to render witness 1 signature: {ex.Message}");
                    }
                }
            }

            // Flatten the form (make it non-editable)
            pdfStamper.FormFlattening = true;
            pdfStamper.Close();

            // Send PDF to browser
            string ReportURL = newFile;
            byte[] temp = System.IO.File.ReadAllBytes(ReportURL);

            Response.Clear();
            MemoryStream ms = new MemoryStream(temp);
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + filename);
            Response.Buffer = true;
            ms.WriteTo(Response.OutputStream);
            Response.End();
        }

        #endregion

        [DecryptParameter]
        public void NewOldpdfDeneratePropertyLeaseAgreement(int? ApplicationId)
        {
            if (ApplicationId == null) throw new Exception("Invalid Application.");

            var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == ApplicationId);
            if (application == null) throw new Exception("Application not found.");

            var lease = db.LeaseDetails.OrderByDescending(x => x.Id)
                .FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IsNew && x.IsActive && !x.IsDeleted);
            if (lease == null) throw new Exception("Invalid Property Lease.");

            var master = db.propertyLeaseAgreementMasters
                .FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted);
            if (master == null) throw new Exception("Invalid Lease Agreement.");

            var templateSetting = db.AppSettings.FirstOrDefault(r => r.Key == AppSettingKeys.LA_TEMP_PDF);
            var template = templateSetting?.Value ?? "";

            string pdfTemplate = "";
            string IP = System.Web.HttpContext.Current.Request.UserHostAddress;

            pdfTemplate = IP == "::1"
                ? Server.MapPath("~/PDFTemplates/LA_Template.pdf")
                : Server.MapPath(template);

            var timestamp2 = DateTime.Now.ToString("ddMMyyyyHHmmss");

            string folderName = Server.MapPath("~/Templates");
            string pathString = System.IO.Path.Combine(folderName, timestamp2);
            System.IO.Directory.CreateDirectory(pathString);

            string nFolderName = folderName;

            string newFile = nFolderName + "\\" + timestamp2 + "_" + (application?.IDNo ?? "") + "_PLMLeaseAgreement.pdf";

            var filename = (application?.ApplicationReferenceNumber ?? "Lease") + ".LEASEAGREEMENT.pdf";

            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));

            AcroFields pdfFormFields = pdfStamper.AcroFields;

            if (application != null && master != null)
            {
                pdfFormFields.SetField("AgentName", master.RepresentedBy ?? "");
                pdfFormFields.SetField("FullNames", master.ApplicantFullName ?? "");
                pdfFormFields.SetField("IdentityNumber", master.ApplicantIdentityNumber ?? application.IDNo ?? "");

                pdfFormFields.SetField("UnitNumber", master.UnitNumber ?? "");

                pdfFormFields.SetField("LeasePreparation", master.PreparationFee.ToString());

                pdfFormFields.SetField("CreditCheckFee",
                    master.CreditCheckFee == 0 ? "N/A" : master.CreditCheckFee.ToString());

                pdfFormFields.SetField("CalculatedAsFollows", master.CalculatedAsFolllows.ToString());
                pdfFormFields.SetField("InitialDepositPremises", master.InitialDepositPremises.ToString());
                pdfFormFields.SetField("InitialDepositTContribution", master.DepositTenantContribution.ToString());
                pdfFormFields.SetField("RentalUnit", master.MonthlyUnitRental.ToString());

                pdfFormFields.SetField("ShadePortParking",
                    master.SPP == true ? master.ShadePortParking.ToString() : "N/A");

                pdfFormFields.SetField("OpenParkingBay",
                    master.OPP == true ? master.OpenParking.ToString() : "N/A");

                pdfFormFields.SetField("StoreRooms",
                    master.STR == true ? master.StoreRooms.ToString() : "N/A");

                pdfFormFields.SetField("Electricity",
                    master.ELEC == true ? master.Electricity.ToString() : "Prepaid");

                pdfFormFields.SetField("SecurityFees",
                    master.SEC == true ? master.SecurityFee.ToString() : "N/A");

                pdfFormFields.SetField("Water",
                    master.WTR == true ? master.Water.ToString() : "N/A");

                pdfFormFields.SetField("Refuse", master.Refuse.ToString());
                pdfFormFields.SetField("Sewerage", master.Sewerage.ToString());

                pdfFormFields.SetField("BedRooms", master.BedRooms.ToString());
                pdfFormFields.SetField("FloorNumber", master.FloorNumber ?? "");
                pdfFormFields.SetField("Block", master.BlockNumber ?? "");

                pdfFormFields.SetField("Day", master.Day ?? "");
                pdfFormFields.SetField("Date", master.CommencementDate ?? "");
                pdfFormFields.SetField("EndDate", master.EndDate ?? "");

                pdfFormFields.SetField("Month", master.NoPenaltyMonth ?? "");
                pdfFormFields.SetField("RentalDue", master.RentalDueUntill ?? "");
                pdfFormFields.SetField("MonthOfLastDay", master.PenaltyMonth ?? "");

                pdfFormFields.SetField("InitialDepositeAmount", master.InitialDepositAmonunt.ToString());
                pdfFormFields.SetField("LeaseAdministrationFee", master.LeaseAdministrationFee.ToString());

                pdfFormFields.SetField("UnitRentalAmount", master.UnitRentalAmountPM.ToString());

                pdfFormFields.SetField("Day2", master.UnitRentalDay ?? "");
                pdfFormFields.SetField("Date2", master.UnitRentalDate ?? "");
                pdfFormFields.SetField("IncreaseDate", master.RentalIncreaseDate ?? "");

                pdfFormFields.SetField("CarportParkingBay", master.CarportParkingBayNumber ?? "");
                pdfFormFields.SetField("OpenParkingBayNumber", master.OPenParkingBayNumber ?? "");

                pdfFormFields.SetField("OpenParkingBayRental",
                    master.OPenParkingBayRental == 0 ? "N/A" : master.OPenParkingBayRental.ToString());

                pdfFormFields.SetField("ShadePortParkingBayNumber", master.ShadePortBayNumber ?? "");

                pdfFormFields.SetField("ShadePortRental",
                    master.ShadePortBayRental == 0 ? "N/A" : master.ShadePortBayRental.ToString());

                pdfFormFields.SetField("_Of1July", master._Of1July ?? "");
                pdfFormFields.SetField("IncreaseDayParking", master.ParkingIncreaseDay ?? "");
                pdfFormFields.SetField("IncreaseMonthParking", master.ParkingIncreaseMonth ?? "");

                pdfFormFields.SetField("Water", master._water.ToString());
                pdfFormFields.SetField("Refuse", master._refuse.ToString());
                pdfFormFields.SetField("Sewerage", master._sewerage.ToString());

                pdfFormFields.SetField("NumberOfOccupants", master.PeopleAllowedOnPremises.ToString());
                pdfFormFields.SetField("LandlordAddress", master.LandlordAddress ?? "");
                pdfFormFields.SetField("TenantSignDate", master.TenantSignDate ?? "");
                pdfFormFields.SetField("TenantSignDay", master.TenantSignDay ?? "");
                pdfFormFields.SetField("TenantsWitness1", master.TenantWitnessONE ?? "");
                pdfFormFields.SetField("TenantsWitness2", master.TenantWitnessTWO ?? "");
                pdfFormFields.SetField("ManagersSignDate", master.ManagersSignDate ?? "");
                pdfFormFields.SetField("ManagersSignDay", master.ManagersSignDay ?? "");
                pdfFormFields.SetField("ManagersWitness1", master.ManagersWitnessONE ?? "");
                pdfFormFields.SetField("ManagersWitness2", master.ManagersWitnessTWO ?? "");
                pdfFormFields.SetField("SignatureMainLessee", master.SignatureMainLessee ?? "");
                pdfFormFields.SetField("SignatureOFSpouse", master.SignatureOfSpouse ?? "");

                if (!string.IsNullOrEmpty(master.TenantSignature) && master.TenantSignature.Contains(","))
                {
                    try
                    {
                        string base64Data = master.TenantSignature.Substring(master.TenantSignature.IndexOf(',') + 1);
                        byte[] sigBytes = Convert.FromBase64String(base64Data);
                        iTextSharp.text.Image sigImage = iTextSharp.text.Image.GetInstance(sigBytes);

                        AcroFields.FieldPosition sigPos = null;
                        var positions = pdfFormFields.GetFieldPositions("TenantsSignature");
                        if (positions != null && positions.Count > 0)
                            sigPos = positions[0];

                        if (sigPos != null)
                        {
                            iTextSharp.text.Rectangle rect = sigPos.position;
                            sigImage.ScaleToFit(rect.Width, rect.Height);
                            sigImage.SetAbsolutePosition(rect.Left, rect.Bottom);
                            PdfContentByte cb = pdfStamper.GetOverContent(sigPos.page);
                            cb.AddImage(sigImage);
                        }
                    }
                    catch { }
                }
            }
            pdfStamper.FormFlattening = true;
            pdfStamper.Close();

            string ReportURL = newFile;
            byte[] temp = System.IO.File.ReadAllBytes(ReportURL);

            Response.Clear();

            MemoryStream ms = new MemoryStream(temp);
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + filename);

            Response.Buffer = true;

            ms.WriteTo(Response.OutputStream);
            Response.End();
        }

        [DecryptParameter]
        public void oldpdfDeneratePropertyLeaseAgreement(int? ApplicationId)
        {
            if (ApplicationId == null) throw new Exception("Invalid Application.");
            var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == ApplicationId);
            var lease = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IsNew && x.IsActive && !x.IsDeleted);
            if (lease == null) throw new Exception("Invalid Property Lease.");
            var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted);
            if (master == null) throw new Exception("Invalid Lease Agreement.");


            var template = db.AppSettings.FirstOrDefault(r => r.Key == AppSettingKeys.LA_TEMP_PDF).Value;

            string pdfTemplate = "";
            string IP = System.Web.HttpContext.Current.Request.UserHostAddress;
            var get_server = IP == "::1" ? pdfTemplate = Server.MapPath("~/PDFTemplates/LA_Template.pdf") : pdfTemplate = Server.MapPath(template);
           
            var timestamp2 = DateTime.Now.ToString("ddMMyyyyHHmmss");
            string folderName = Server.MapPath("~/Templates");
            string pathString = System.IO.Path.Combine(folderName, timestamp2);
            System.IO.Directory.CreateDirectory(pathString);
            string nFolderName = null;
            string newFile = nFolderName = folderName;
            Random rnd = new Random();
            int randomNum = rnd.Next(1, 51);
            newFile = nFolderName + "\\" + timestamp2 + "_" + application.IDNo + "_PLMLeaseAgreement.pdf";
            var filename = application.ApplicationReferenceNumber + ".LEASEAGREEMENT.pdf";

            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
            AcroFields pdfFormFields = pdfStamper.AcroFields;

            if (application != null)
            {
                pdfFormFields.SetField("AgentName", master.RepresentedBy.ToString());
                pdfFormFields.SetField("FullNames", master.ApplicantFullName.ToString());
                pdfFormFields.SetField("IdentityNumber", master.ApplicantIdentityNumber.ToString());
                pdfFormFields.SetField("UnitNumber", master.UnitNumber.ToString());
                pdfFormFields.SetField("LeasePreparation", master.PreparationFee.ToString());
                pdfFormFields.SetField("CreditCheckFee", master.CreditCheckFee.ToString() == "0" ? "N/A" : master.CreditCheckFee.ToString());
                pdfFormFields.SetField("CalculatedAsFollows", master.CalculatedAsFolllows.ToString());
                pdfFormFields.SetField("InitialDepositPremises", master.InitialDepositPremises.ToString());
                pdfFormFields.SetField("InitialDepositTContribution", master.DepositTenantContribution.ToString());
                pdfFormFields.SetField("RentalUnit", master.MonthlyUnitRental.ToString());
                var shad = master.SPP == true ? pdfFormFields.SetField("ShadePortParking", master.ShadePortParking.ToString()) : pdfFormFields.SetField("ShadePortParking", "N/A");
                var open = master.OPP == true ? pdfFormFields.SetField("OpenParkingBay", master.OpenParking.ToString()) : pdfFormFields.SetField("OpenParkingBay", "N/A");
                var strm = master.STR == true ? pdfFormFields.SetField("StoreRooms", master.StoreRooms.ToString()) : pdfFormFields.SetField("StoreRooms", "N/A");
                var eltr = master.ELEC == true ? pdfFormFields.SetField("Electricity", master.Electricity.ToString()) : pdfFormFields.SetField("Electricity", "Prepaid");
                var secr = master.SEC == true ? pdfFormFields.SetField("SecurityFees", master.SecurityFee.ToString()) : pdfFormFields.SetField("SecurityFees", "N/A");
                var watr = master.WTR == true ? pdfFormFields.SetField("Water", master.Water.ToString()) : pdfFormFields.SetField("Water", "N/A");
                pdfFormFields.SetField("Refuse", master.Refuse.ToString());
                pdfFormFields.SetField("Sewerage", master.Sewerage.ToString());
                pdfFormFields.SetField("UnitNumber", master.UnitNumber.ToString());
                pdfFormFields.SetField("BedRooms", master.BedRooms.ToString());
                pdfFormFields.SetField("FloorNumber", master.FloorNumber.ToString());
                pdfFormFields.SetField("Block", master.BlockNumber.ToString());
                pdfFormFields.SetField("Day", master.Day.ToString());
                pdfFormFields.SetField("Date", master.CommencementDate.ToString());
                pdfFormFields.SetField("EndDate", master.EndDate.ToString());
                pdfFormFields.SetField("Month", master.NoPenaltyMonth.ToString());
                pdfFormFields.SetField("RentalDue", master.RentalDueUntill.ToString());
                pdfFormFields.SetField("MonthOfLastDay", master.PenaltyMonth.ToString());
                pdfFormFields.SetField("InitialDepositeAmount", master.InitialDepositAmonunt.ToString());
                pdfFormFields.SetField("LeaseAdministrationFee", master.LeaseAdministrationFee.ToString());
                pdfFormFields.SetField("UnitRentalAmount", master.UnitRentalAmountPM.ToString());
                pdfFormFields.SetField("Day2", master.UnitRentalDay.ToString());
                pdfFormFields.SetField("Date2", master.UnitRentalDate.ToString());
                pdfFormFields.SetField("IncreaseDate", master.RentalIncreaseDate.ToString());
                pdfFormFields.SetField("CarportParkingBay", master.CarportParkingBayNumber.ToString());
                pdfFormFields.SetField("OpenParkingBayNumber", master.OPenParkingBayNumber.ToString());
                pdfFormFields.SetField("OpenParkingBayRental", master.OPenParkingBayRental.ToString() == "0" ? "N/A" : master.OPenParkingBayRental.ToString());
                pdfFormFields.SetField("ShadePortParkingBayNumber", master.ShadePortBayNumber.ToString());
                pdfFormFields.SetField("ShadePortRental", master.ShadePortBayRental.ToString() == "0" ? "N/A" : master.ShadePortBayRental.ToString());
                pdfFormFields.SetField("_Of1July", master._Of1July.ToString());
                pdfFormFields.SetField("IncreaseDayParking", master.ParkingIncreaseDay.ToString());
                pdfFormFields.SetField("IncreaseMonthParking", master.ParkingIncreaseMonth.ToString());
                pdfFormFields.SetField("Water", master._water.ToString());
                pdfFormFields.SetField("Refuse", master._refuse.ToString());
                pdfFormFields.SetField("Sewerage", master._sewerage.ToString());
                pdfFormFields.SetField("NumberOfOccupants", master.PeopleAllowedOnPremises.ToString());
                pdfFormFields.SetField("LandlordAddress", master.LandlordAddress.ToString());
                var Oc_One = master.OccupantONE == null ? true : pdfFormFields.SetField("Occupant1", master.OccupantONE.ToString()) && pdfFormFields.SetField("OccupantIDNO1", master.OccupantONEIdentityNo.ToString());
                var Oc_Two = master.OccupantTWO == null ? true : pdfFormFields.SetField("Occupant2", master.OccupantTWO.ToString()) && pdfFormFields.SetField("OccupantIDNO2", master.OccupantTWOIdentityNo.ToString());
                var Oc_Thr = master.OccupantTHREE == null ? true : pdfFormFields.SetField("Occupant3", master.OccupantTHREE.ToString()) && pdfFormFields.SetField("OccupantIDNO3", master.OccupantTHREEIdentityNo.ToString());
                var Oc_Fou = master.OccupantFOUR == null ? true : pdfFormFields.SetField("Occupant4", master.OccupantFOUR.ToString()) && pdfFormFields.SetField("OccupantIDNO4", master.OccupantFOURIdentityNo.ToString());
                var Oc_Fiv = master.OccupantFIVE == null ? true : pdfFormFields.SetField("Occupant5", master.OccupantFIVE.ToString()) && pdfFormFields.SetField("OccupantIDNO5", master.OccupantFIVEIdentityNo.ToString());
                var Oc_Six = master.OccupantSIX == null ? true : pdfFormFields.SetField("Occupant6", master.OccupantSIX.ToString()) && pdfFormFields.SetField("OccupantIDNO6", master.OccupantSIXIdentityNo.ToString());
                var sign_t = master.TenantSigned == true ? pdfFormFields.SetField("TenantSignDate", master.TenantSignDate.ToString()) && pdfFormFields.SetField("TenantSignDay", master.TenantSignDay.ToString()) && pdfFormFields.SetField("TenantsSignature", master.TenantSignature.ToString()) : true;
                var sign_p = master.PropertyManagerSigned == true ? pdfFormFields.SetField("PropertyManagersSignature", master.PropertyManagersSignature.ToString()): true;
                var m_sndt = master.PropertyManagerSigned == true && master.RevenueManagerSigned == true ? pdfFormFields.SetField("ManagersSignDate", master.ManagersSignDate.ToString()) && pdfFormFields.SetField("ManagersSignDay", master.ManagersSignDay.ToString()) : true;
                var sign_r = master.RevenueManagerSigned == true ? pdfFormFields.SetField("RevenueManagersSignature", master.RevenueManagersSignature.ToString()) : true;
                var t_wtn1 = master.TenantWitnessONE == null ? true : pdfFormFields.SetField("TenantsWitness1", master.TenantWitnessONE.ToString());
                var t_wtn2 = master.TenantWitnessTWO == null ? true : pdfFormFields.SetField("TenantsWitness2", master.TenantWitnessTWO.ToString());
                var m_wtn1 = master.ManagersWitnessONE == null ? true : pdfFormFields.SetField("ManagersWitness1", master.ManagersWitnessONE.ToString());
                var m_wtn2 = master.ManagersWitnessTWO == null ? true : pdfFormFields.SetField("ManagersWitness2", master.ManagersWitnessTWO.ToString());
                var m_lesee = master.MainLesseeSigned == true ? pdfFormFields.SetField("SignatureMainLessee", master.SignatureMainLessee.ToString()) : true;
                var spousee = master.SpouseSigned == true ? pdfFormFields.SetField("SignatureOFSpouse", master.SignatureOfSpouse.ToString()):true;

            }
            pdfStamper.FormFlattening = true;
            pdfStamper.Close();
            string ReportURL = newFile;
            byte[] temp = System.IO.File.ReadAllBytes(ReportURL);

            Response.Clear();
            MemoryStream ms = new MemoryStream(temp);
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + filename);
            Response.Buffer = true;
            ms.WriteTo(Response.OutputStream);
            Response.End();
        }


        public async Task<ActionResult> Test2()
        {
            return RedirectToAction("pdfDenerateDebitOrderAuthority", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("ApplicationId=" + 116) });


            var leaseInfo = db.PropertyLeaseApplications.FirstOrDefault();
            var refNumber2 = await CreatePdfAsync(leaseInfo, "DebitOrderAuthority", "DebitOrderAuthority");
           
            var refNumber = await CreatePdfAsync(leaseInfo, "Lease1", "Lease1");
            ViewBag.linkRef = refNumber;
            return View();
        }


        public ActionResult Lease1(int? id)
        {
            var temp = db.AppSettings.FirstOrDefault(x => x.Key == Keys.AppSettingKeys.Lease1TemplateKey);
            if (temp == null) throw new Exception("Invalid template");
            string tempVal = temp.Value;

            var leaseInfo = db.PropertyLeaseApplications.Find(id);
            var applicantunit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == id);
            //var matchedunit = db.MatchedUnits.FirstOrDefault(x => x.Id == applicantunit.MatchedID);
            //var unit = db.Units.FirstOrDefault(x => x.Id == matchedunit.unitID);

            //var leasedetails = db.LeaseDetails.FirstOrDefault(x => x.leaseApplicationRef == leaseInfo.ApplicationReferenceNumber);
            //tempVal = tempVal.Replace("*e1*", leaseInfo.IDNo);
            //tempVal = tempVal.Replace("*e2*", leaseInfo.IDNo);


            //tempVal = tempVal.Replace("*e3*", Convert.ToDateTime(leasedetails.StartDate).ToString("dd/MM/yyyy")); 
            //tempVal = tempVal.Replace("*e4*", Convert.ToDateTime(leasedetails.StartDate).ToString("dd/MM/yyyy"));
            //tempVal = tempVal.Replace("*e5*", Convert.ToString(unit.PropertyDeposit));


            ViewBag.Test = tempVal;
            return View();
        }

        public ActionResult DebitOrderAuthority(int? id)
        {
            var temp = db.AppSettings.FirstOrDefault(x => x.Key == Keys.AppSettingKeys.DebitOrderAuthority);
            if (temp == null) throw new Exception("Invalid template");
            string tempVal = temp.Value;

            var leaseInfo = db.PropertyLeaseApplications.Include(x=>x.Customer).FirstOrDefault(x=>x.Id == id);
            var add = leaseInfo.BStreet + " " + leaseInfo.BSuburb;
            tempVal = tempVal.Replace("e1", leaseInfo.Customer.UserFullName);
            tempVal = tempVal.Replace("e2", leaseInfo.ApplicationReferenceNumber);
            tempVal = tempVal.Replace("e3", add);
            tempVal = tempVal.Replace("e4", leaseInfo.BSuburb);
            tempVal = tempVal.Replace("e5", leaseInfo.BAddress);
            tempVal = tempVal.Replace("e6", "Absa");
            tempVal = tempVal.Replace("e7", "Sandton");
            tempVal = tempVal.Replace("e8", "409656");
            tempVal = tempVal.Replace("e9", "Cheque");
            tempVal = tempVal.Replace("e10", "40090893626");
            tempVal = tempVal.Replace("e11", leaseInfo.Customer.UserFullName);

            tempVal = tempVal.Replace("e12", "Boksburg CCC");
            tempVal = tempVal.Replace("e13", "17");
            tempVal = tempVal.Replace("e14", "02");
            tempVal = tempVal.Replace("e15", "2022");
            tempVal = tempVal.Replace("e16", leaseInfo.Customer.UserFullName);

            tempVal = tempVal.Replace("e17", leaseInfo.Customer.UserFullName);
            ViewBag.Test = tempVal;
            return View();
        }


        public async Task<string> CreatePdfAsync(PropertyLeaseApplication agricultureHolding, string Pdf_Name_FromDb, string templateType)
        {
            string randomRefString = string.Empty;
            //string EmailCC = string.Empty;
            //string CCCNO = string.Empty;
            string ReferenceVal = string.Empty;
            //string ReferenceObj = string.Empty;
            int viewID = agricultureHolding.Id;
            //get pdf name from appsettings
            var FindDownloadDOom = db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.DownloadDomain);
            var FindPublicDomain = db.AppSettings.FirstOrDefault(v => v.Key == AppSettingKeys.PublicDownloadDomain);
            //FindDownloadDOom.Value = "http://localhost:3450/";

            string URLAutho = "";
            string formName = agricultureHolding.IDNo + "_" + templateType;
            //string PDFName = "AgricultureHoldingsObjectionsPDF";
            string controllerName = "PropertyLeaseApplication";
            Attachments fileUploadModel = new Attachments();
            try
            {
                string PDFString = "";
                        switch (templateType)
                {
                    case "Lease1":
                         PDFString = GenerateViewToPDF(viewID, "TEST", Pdf_Name_FromDb, FindDownloadDOom.Value, "PropertyLeaseApplication", "Lease1", templateType);
                        fileUploadModel.DocumentTypeId = db.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading).Id;
                        break;
                    case "Lease2":
                        PDFString = GenerateViewToPDF(viewID, "TEST", Pdf_Name_FromDb, FindDownloadDOom.Value, "PropertyLeaseApplication", "Lease1", templateType);
                        fileUploadModel.DocumentTypeId = db.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading).Id;
                        break;
                    case "DebitOrderAuthority":
                         PDFString = GenerateViewToPDF(viewID, "TEST", Pdf_Name_FromDb, FindDownloadDOom.Value, "PropertyLeaseApplication", "DebitOrderAuthority", templateType);
                        fileUploadModel.DocumentTypeId = db.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundWaterMeterReading).Id;
                        break;
                    default:
                         PDFString = GenerateViewToPDF(viewID, "TEST", Pdf_Name_FromDb, FindDownloadDOom.Value, "PropertyLeaseApplication", "Lease1", templateType);
                        break;
                }
                //string PDFString = GenerateViewToPDF(viewID, "TEST", Pdf_Name_FromDb, FindDownloadDOom.Value, "PropertyLeaseApplication", "Lease1", templateType);
                var values = PDFString.Split('|');
                var fullpathtofile = values[1];
                var mimetype = values[0];
                byte[] FileByte = Encoding.ASCII.GetBytes(values[2]);
                Int64 Length = Convert.ToInt64(values[3]);
                // var filecontents = System.IO.File.ReadAllBytes(fullpathtofile);


              
                fileUploadModel.FileName = Pdf_Name_FromDb + formName;
                fileUploadModel.FilePath = fullpathtofile;
                fileUploadModel.PropertyLeaseApplicationId = agricultureHolding.Id;
       
                if (agricultureHolding.IDNo != null && agricultureHolding.IDNo != "")
                {
                    fileUploadModel.ReferenceIDNO = formName;
                    ReferenceVal = fileUploadModel.ReferenceIDNO;
                }
                else
                {
                    randomRefString = RandomString(7);
                    fileUploadModel.ReferenceIDNO = randomRefString + formName;
                    ReferenceVal = fileUploadModel.ReferenceIDNO;
                }

                db.Attachments.Add(fileUploadModel);


                //await db.SaveChangesAsync();
                int saveChange = await db.SaveChangesAsync();
                if (saveChange > 0) return fileUploadModel.FilePath;
                else return "Failed";
            }
            catch (Exception ex)
            {
                db.Logs.Add(new Log()
                {
                    LogTypeId = 1,
                    LogEntry = ex.Message.ToString(),
                    ReferenceId = 0,
                    ReferenceTypeId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                });
                db.SaveChanges();
                return "Failed";
            }
        }

        
        [EncryptedActionParameter]
        public ActionResult UpdateTenantRecord(int rcsAppId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    return RedirectToAction("ApplicationUpdateTenantLeaseDetails", "LeaseDetails", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + rcsAppId.ToString()) });

                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }


        public ActionResult UploadTemplates()
        {
            Initialise();
            var rcsApps = db.PropertyLeaseApplications.OrderByDescending(x => x.Id).FirstOrDefault();
            var customer = Customer;
            var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var dvm = new DocumentsViewModel();
            MatchingHelper.DocumentUploadTemplates(dvm, db, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, true);
            var vm = new DepartmentsApprovalViewModel
            {
                DocumentsViewModel = dvm
            };

            return View(vm);
        }

        public ActionResult GetUploadedTemplates()
        {
            Initialise();
            var rcsApps = db.PropertyLeaseApplications.OrderByDescending(x => x.Id).FirstOrDefault();
            var customer = Customer;
            var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var dvm = new DocumentsViewModel();
            MatchingHelper.DocumentUploadTemplates(dvm, db, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, false);
            var vm = new DepartmentsApprovalViewModel
            {
                DocumentsViewModel = dvm
            };

            return View(vm);
        }


        [DecryptParameter]
        public ActionResult ConductUnitInspection(int? id)
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

            PropertyLeaseApplication rcsApps = null;
            LeaseDetails leaseDetails = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status).Include(x => x.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();

            leaseDetails = db.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.IsDeleted == false)
                .Include(r => r.CreatedBySystemUser)
                .Include(r => r.PurchaserType)
                .Include(r => r.ModifiedBySystemUser)
                .Include(r => r.Status)
                .Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).FirstOrDefault()??null;


            ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Habitable || x.Key == RCSActionTypeKeys.HabitableMinorDefects || x.Key == RCSActionTypeKeys.NotHabitable).OrderBy(x => x.Name), "Key", "Name");

            ViewBag.UnitInspectionComment = null;


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
                List<Attachments> attachments = new List<Attachments>();
                var atth = db.Attachments.FirstOrDefault();
                attachments.Add(atth);

                var vm = new DepartmentsApprovalViewModel
                {
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    LeaseDetails=leaseDetails,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description


                };

                Entity entity = null;
                Agent agent = null;

                vm.Customer = customer;

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

                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                DocumentsViewModel dvmTemplate = new DocumentsViewModel();

                bool IsUpload = true;
                var returnUrl = "";
                MatchingHelper.DocumentConductUnitInspection(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, returnUrl, rcsApps.Id, IsUpload);
                MatchingHelper.DocumentGetConductUnitInspectionTemplate(dvmTemplate, db, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, false);

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
                vm.DocumentsViewModelTemplate = dvmTemplate;


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
        public ActionResult ConductUnitInspection(Int32? id, String ApprovalStatusddl, String UnitInspectionComment)
        {
            Initialise();
            PropertyLeaseApplication rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();


          var leaseApp = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);
            var AppUnit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.IsActive && !x.IsDeleted);
            var Matched = db.MatchedUnits.FirstOrDefault(x => x.Id == AppUnit.MatchedID);
            //var Unit = db.Units.FirstOrDefault(x => x.Id == Matched.UnitsId);
            //var Unit = db.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == Matched.UnitsEkurhuleniHousingCompanyId);
            var app = db.ApplicantUnits.Include(r => r.Matched).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id).Matched;

            var Unit = db.ApplicationAllocatedProperty.FirstOrDefault(r => r.Id == app.ApplicationAllocatedPropertyId);


            ResponsibilityType ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Inspections).FirstOrDefault();
            Customer User =  GetBackOfficeId(db, (Int32)id, false);
            short activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
            Int32 UserId = User.Id != 0 ? User.Id : activeDirectoryOn;


            if (ApprovalStatusddl == RCSActionTypeKeys.Habitable)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantUpdateDetails).Id, (Int32)id);
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (Int32)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);
                EHCRoundRobin(rcsApps.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                //customer email here
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UnitInspectionApproved).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, Customer.Id);
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.HabitableMinorDefects)
            {
                EHCRoundRobin((Int32)id, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                //customer email here


                AllocatedUnitMaintenanceEHC UM = new AllocatedUnitMaintenanceEHC();
                UM.ApplicationAllocatedPropertyId = Unit.Id;
                UM.PropertyLeaseApplicationId = rcsApps.Id;
                //UM.LeaseDetailsId = leaseApp.Id;
                UM.RCSActionTypeId = db.RCSActionTypes.FirstOrDefault(x => x.Key == RCSActionTypeKeys.HabitableMinorDefects).Id;
                UM.StatusId = db.Status.FirstOrDefault(x => x.Key == StatusKeys.Submitted).Id;
                UM.InspectionType = StatusKeys.PreUnitInspection;
                db.allocatedUnitMaintenanceEHCs.Add(UM);
                db.SaveChanges();


                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantUpdateDetails).Id, (Int32)id);
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (Int32)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);
                EHCRoundRobin(rcsApps.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                Nullable<Int32> ApplicationAllocatedPropertyId = db.ApplicantUnits.Include(r => r.Matched).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id).Matched.ApplicationAllocatedPropertyId;
                var findItem = db.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == ApplicationAllocatedPropertyId);
                findItem.Inspection = true;
                db.Entry(findItem).State = EntityState.Modified;
                db.SaveChanges();

             

                //customer email here
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UnitInspectionFailedInhabitable).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, Customer.Id);
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.NotHabitable)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.CustomerQueryPending).Id, (Int32)id);
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (Int32)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);
                //                      1      2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17     18   19
                EHCRoundRobin((Int32)id, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                //customer email here

                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UnitInspectionFailedUninhabitable).Description.ToString() + ", Reason: "+ UnitInspectionComment;
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, Customer.Id);

                var findItem = db.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == app.ApplicationAllocatedPropertyId);
                findItem.Inspection = true;
                db.Entry(findItem).State = EntityState.Modified;
                db.SaveChanges();

                AllocatedUnitMaintenanceEHC UM = new AllocatedUnitMaintenanceEHC();
                UM.ApplicationAllocatedPropertyId = Unit.Id;
                UM.PropertyLeaseApplicationId = rcsApps.Id;
                //UM.LeaseDetailsId = leaseApp.Id;
                UM.RCSActionTypeId = db.RCSActionTypes.FirstOrDefault(x => x.Key == RCSActionTypeKeys.NotHabitable).Id;
                UM.StatusId = db.Status.FirstOrDefault(x => x.Key == StatusKeys.Submitted).Id;
                UM.InspectionType = StatusKeys.PreUnitInspection;
                db.allocatedUnitMaintenanceEHCs.Add(UM);
                db.SaveChanges();

                //var emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RiskAssessmentRejectITC).Id;
                //EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);



            }

            MatchingHelper.MarkInspectionDatesAsInspected(db, (Int32)id);

            var ConductInspec = new ConductUnitInspection()
            {
                PropertyLeaseApplicationId = rcsApps.Id,
                InspectionComment = UnitInspectionComment
            };
            db.conductUnitInspections.Add(ConductInspec);
            db.SaveChanges();
            Session["ConductUnitInspectionSession"] = string.Format($"Conduct unit inspection processed successfully for application reference ,{rcsApps.ApplicationReferenceNumber}");
            return RedirectToAction("PropertyLeaseInspections");
        }



        [DecryptParameter]
        public ActionResult ConductExitInspection(int? id)
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

            PropertyLeaseApplication rcsApps = null;
            LeaseDetails leaseDetails = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status).Include(x => x.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();

            leaseDetails = db.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.IsDeleted == false)
                .Include(r => r.CreatedBySystemUser)
                .Include(r => r.PurchaserType)
                .Include(r => r.ModifiedBySystemUser)
                .Include(r => r.Status)
                .Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).FirstOrDefault()??null;


            ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Habitable || x.Key == RCSActionTypeKeys.HabitableMinorDefects || x.Key == RCSActionTypeKeys.NotHabitable).OrderBy(x => x.Name), "Key", "Name");

            ViewBag.UnitInspectionComment = null;


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
                List<Attachments> attachments = new List<Attachments>();
                var atth = db.Attachments.FirstOrDefault();
                attachments.Add(atth);

                var vm = new DepartmentsApprovalViewModel
                {
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    LeaseDetails=leaseDetails,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description


                };

                Entity entity = null;
                Agent agent = null;

                vm.Customer = customer;

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

                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                DocumentsViewModel dvmTemplate = new DocumentsViewModel();
                DocumentsViewModel dvmExitInterviewForm = new DocumentsViewModel();

                bool IsUpload = true;
                var returnUrl = "";
                MatchingHelper.DocumentConductConductExitInspection(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, returnUrl, rcsApps.Id, IsUpload);
                MatchingHelper.DocumentGetConductExitInspectionTemplate(dvmTemplate, db, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, false);
                // For Exit Inter Form
                MatchingHelper.DocumentExitInterviewForm(dvmExitInterviewForm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, returnUrl, rcsApps.Id, IsUpload);
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
                vm.DocumentsViewModelTemplate = dvmTemplate;
                vm.DocumentsViewModelExitInterviewForm = dvmExitInterviewForm;

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
        public ActionResult ConductExitInspection(int? id, string ApprovalStatusddl, string UnitInspectionComment)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            ResponsibilityType ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Inspections).FirstOrDefault();

            var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();
            var leaseApp = db.LeaseDetails.OrderByDescending(x=>x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);
            var AppUnit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.IsActive && !x.IsDeleted);
            var Matched = db.MatchedUnits.FirstOrDefault(x => x.Id == AppUnit.MatchedID);
            //var Unit = db.Units.FirstOrDefault(x => x.Id == Matched.UnitsId);
            //var Unit = db.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == Matched.UnitsEkurhuleniHousingCompanyId);
            var app = db.ApplicantUnits.Include(r => r.Matched).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id).Matched;

            var Unit = db.ApplicationAllocatedProperty.FirstOrDefault(r => r.Id == app.ApplicationAllocatedPropertyId);

            //var Units = db.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == Matched.UnitsEkurhuleniHousingCompanyId);

            if (ApprovalStatusddl == RCSActionTypeKeys.Habitable)
            {
                //MatchingHelper.MarkUnitAsAvailable(db, Unit.Id);
                //MatchingHelper.MarkAplicationAsDeleted(db, AppUnit.Id);

                EHCRoundRobin((int)rcsApps.Id, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingVacatingConfirm).Id, (int)id);
                MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingVacatingConfirm).Id, (int)leaseApp.Id);
       

                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ConductExitInspectionApprove).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (Int32)rcsApps.Id, null, ResponsibilityTypeId.Id, userID);

            }

            if ((ApprovalStatusddl == RCSActionTypeKeys.HabitableMinorDefects))
            {
                //MatchingHelper.MarkUnitAsAvailable(db, Unit.Id);
                //MatchingHelper.MarkUnitAsInspection(db, Unit.Id);
                //MatchingHelper.MarkAplicationAsDeleted(db, AppUnit.Id);


                EHCRoundRobin((int)rcsApps.Id, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingVacatingConfirm).Id, (int)id);
                MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingVacatingConfirm).Id, (int)leaseApp.Id);


             MatchingHelper.RoundRobinMarkJobAsFinished(db, (Int32)rcsApps.Id, null, ResponsibilityTypeId.Id, userID);

              
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ConductExitInspectionReject).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
 
            }
            if ((ApprovalStatusddl == RCSActionTypeKeys.NotHabitable))
            {
                EHCRoundRobin((int)rcsApps.Id, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                var findItem = db.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == app.ApplicationAllocatedPropertyId);
                findItem.Inspection = true;
                db.Entry(findItem).State = EntityState.Modified;
                db.SaveChanges();

                AllocatedUnitMaintenanceEHC UM = new AllocatedUnitMaintenanceEHC();
                UM.ApplicationAllocatedPropertyId = Unit.Id;
                UM.PropertyLeaseApplicationId = rcsApps.Id;
                UM.LeaseDetailsId = leaseApp.Id;
                UM.RCSActionTypeId = db.RCSActionTypes.FirstOrDefault(x => x.Key == RCSActionTypeKeys.NotHabitable).Id;
                UM.StatusId = db.Status.FirstOrDefault(x => x.Key == StatusKeys.Submitted).Id;
                UM.InspectionType = StatusKeys.ExitUnitInspection;
                db.allocatedUnitMaintenanceEHCs.Add(UM);
                db.SaveChanges();


                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingMaintananceJobSheet).Id, (int)id);
              //  MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingMaintananceJobSheet).Id, (int)leaseApp.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ConductExitInspectionReject).Description.ToString() + ", Reason: " + UnitInspectionComment; ;
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);

            }
            //new code configure first

            var ConductInspec = new ConductUnitInspection()
            {
                PropertyLeaseApplicationId = rcsApps.Id,
                InspectionComment = UnitInspectionComment
            };
            db.conductUnitInspections.Add(ConductInspec);
            db.SaveChanges();

            Session["ConductUnitInspectionSession"] = string.Format($"Conduct exit unit inspection processed successfully for application reference ,{rcsApps.ApplicationReferenceNumber}");
            return RedirectToAction("PropertyLeaseInspections");
        }


        [DecryptParameter]
        public ActionResult UnitMaintenance(int? id)
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


            PropertyLeaseApplication rcsApps = null;
            LeaseDetails leaseDetails = null;
            ConductUnitInspection conduct = null;
            UnitsEkurhuleniHousingCompany maintenance = null;

            maintenance = db.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == id);
            var matched = db.MatchedUnits.OrderByDescending(x => x.Id).FirstOrDefault(x => x.UnitsEkurhuleniHousingCompanyId == maintenance.Id && x.IsAccepted);
            var appunit = matched != null ? db.ApplicantUnits.FirstOrDefault(x => x.MatchedID == matched.Id) : null;

            //var lease = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == leaseInfo.Id);
            //var AppUnit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == leaseInfo.Id && x.IsActive && !x.IsDeleted);
            //var match = db.MatchedUnits.FirstOrDefault(x => x.Id == AppUnit.MatchedID);
            //var Units = db.ApplicationAllocatedProperty.Include(o => o.OfferedComplex).FirstOrDefault(x => x.Id == match.ApplicationAllocatedPropertyId);


            rcsApps = appunit != null ? db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status).Include(x => x.PurchaserType)
              .Where(x => x.Id == appunit.PropertyLeaseApplicationId).FirstOrDefault() : null;

            leaseDetails = rcsApps != null ? db.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.IsDeleted == false && x.IsNew)
                .Include(r => r.CreatedBySystemUser)
                .Include(r => r.PurchaserType)
                .Include(r => r.ModifiedBySystemUser)
                .Include(r => r.Status)
                .Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).FirstOrDefault() : null;

            ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
            
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
                List <Attachments> attachments = new List<Attachments>();
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
                vm.PropertyLeaseApplications = rcsApps;
                vm.LeaseDetails = leaseDetails;
                vm.ConductUnitInspection = conduct;
                vm.EkurhuleniHousingCompanies = maintenance;

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

                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                bool IsUpload = true;
                var returnUrl = "";
                MatchingHelper.DocumentConductMaintanaceJobSheet(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, returnUrl, rcsApps.Id, IsUpload);


                DocumentsViewModel dvmTemplate = new DocumentsViewModel();
                MatchingHelper.DocumentGetConductMaintananceJobSheetTemplate(dvmTemplate, db, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, false);
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
            return View(rcsApps);
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult UnitMaintenance(int? id, string ApprovalStatusddl)
        {
            Initialise();

            var userID = Customer.Id;
            var Keys = db.Status;
            var maintenance = db.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == id);
            var matched = db.MatchedUnits.OrderByDescending(x => x.Id).FirstOrDefault(x => x.UnitsEkurhuleniHousingCompanyId == maintenance.Id && x.IsAccepted);
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == matched.PropertyLeaseApplicationId && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                MatchingHelper.MarkUnitAsInpected(db, maintenance.Id);
                var User =  GetBackOfficeId(db, rcsApps.Id, false);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var UserId = User.Id != 0 ? User.Id : activeDirectoryOn;

                maintenance.Inspection = false;
                db.Entry(maintenance).State = EntityState.Modified;
                db.SaveChanges();

                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.UnitMaintenanance).FirstOrDefault();
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);

            }
            return RedirectToAction("PropertyLeaseInspections");
        }

        [DecryptParameter]
        public ActionResult MaintenanceJobSheet(int? id)
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


            PropertyLeaseApplication rcsApps = null;
            LeaseDetails leaseDetails = null;
            ConductUnitInspection conduct = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status).Include(x => x.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();

            leaseDetails = db.LeaseDetails.OrderByDescending(x=>x.Id).Where(x => x.IsDeleted == false && x.IsNew)
                .Include(r => r.CreatedBySystemUser)
                .Include(r => r.PurchaserType)
                .Include(r => r.ModifiedBySystemUser)
                .Include(r => r.Status)
                .Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).FirstOrDefault() ?? null;

            conduct = db.conductUnitInspections.OrderByDescending(x => x.Id).Include(r => r.CreatedBySystemUser).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);


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
                List <Attachments> attachments = new List<Attachments>();
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
                vm.PropertyLeaseApplications = rcsApps;
                vm.LeaseDetails = leaseDetails;
                vm.ConductUnitInspection = conduct;

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

                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                bool IsUpload = true;
                var returnUrl = "";

                var UM = db.allocatedUnitMaintenanceEHCs.Include(x => x.RCSActionType).OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);


                if (UM.InspectionType == StatusKeys.PreUnitInspection)
                {
                    MatchingHelper.DocumentConductMaintanaceJobSheet(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, returnUrl, rcsApps.Id, IsUpload);


                }
                else if (UM.InspectionType == StatusKeys.ExitUnitInspection)
                {
                    MatchingHelper.DocumentConductExitMaintanaceJobSheet(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, returnUrl, rcsApps.Id, IsUpload);

                }



                DocumentsViewModel dvmTemplate = new DocumentsViewModel();
                MatchingHelper.DocumentGetConductMaintananceJobSheetTemplate(dvmTemplate, db, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, false);
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
            return View(rcsApps);
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult MaintenanceJobSheet(int? id, string ApprovalStatusddl)
        {
            Initialise();

            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();
            CaptureController c = new CaptureController();


            var UM2 = db.allocatedUnitMaintenanceEHCs.Include(x => x.RCSActionType).OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);


            if (UM2.InspectionType == StatusKeys.PreUnitInspection)
            {
                if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                {
                    Nullable<Int32> ApplicationAllocatedPropertyId = db.ApplicantUnits.Include(r => r.Matched).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id).Matched.ApplicationAllocatedPropertyId;
                    var findItem = db.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == ApplicationAllocatedPropertyId);
                    findItem.Inspection = false;
                    db.Entry(findItem).State = EntityState.Modified;
                    db.SaveChanges();

                    var User = GetBackOfficeId(db, rcsApps.Id, false);
                    var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                    var UserId = User.Id != 0 ? User.Id : activeDirectoryOn;

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.MaintananceJobSheet).FirstOrDefault();
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);

                    var UM = db.allocatedUnitMaintenanceEHCs.Include(x => x.RCSActionType).OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.InspectionType == StatusKeys.PreUnitInspection);
                    UM.UnitMaintenanceCompleted = true;
                    UM.StatusId = db.Status.FirstOrDefault(x => x.Key == StatusKeys.Archived).Id;
                    db.SaveChanges();

                    if (UM.RCSActionType.Key == RCSActionTypeKeys.NotHabitable)
                    {
                        MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantUpdateDetails).Id, (Int32)id);
                        MatchingHelper.RoundRobinMarkJobAsFinished(db, (Int32)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);
                        EHCRoundRobin(rcsApps.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                        //customer email here
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UnitInspectionApproved).Description.ToString();
                        MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, Customer.Id);
                        //MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id, (int)id);

                        //EHCRoundRobin(rcsApps.Id, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                        Session["MaintenanceJobSheetSession"] = string.Format($"Job sheet approved for application reference ,{rcsApps.ApplicationReferenceNumber} , Application sent back to schedule inspection dates");

                    }
                    else if (UM.RCSActionType.Key == RCSActionTypeKeys.HabitableMinorDefects)
                    {
                        Session["MaintenanceJobSheetSession"] = string.Format($"Job sheet approved for application reference ,{rcsApps.ApplicationReferenceNumber} , due to minor defects there are no changes to application process flow.");

                    }

                }
                else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
                {
                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.CustomerQueryPending).Id, (int)id);
                    db.SaveChanges();
                    Session["MaintenanceJobSheetSession"] = string.Format($"Job sheet rejected for application reference ,{rcsApps.ApplicationReferenceNumber}");
                }
            }
            else if (UM2.InspectionType == StatusKeys.ExitUnitInspection)
            {
                if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                {
                    Nullable<Int32> ApplicationAllocatedPropertyId = db.ApplicantUnits.Include(r => r.Matched).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id).Matched.ApplicationAllocatedPropertyId;
                    var findItem = db.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == ApplicationAllocatedPropertyId);
                    findItem.Inspection = false;
                    db.Entry(findItem).State = EntityState.Modified;


                    var User = GetBackOfficeId(db, rcsApps.Id, false);
                    var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                    var UserId = User.Id != 0 ? User.Id : activeDirectoryOn;

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.MaintananceJobSheet).FirstOrDefault();
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);

                    var UM = db.allocatedUnitMaintenanceEHCs.Include(x => x.RCSActionType).OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.InspectionType == StatusKeys.ExitUnitInspection);
                    UM.UnitMaintenanceCompleted = true;
                    UM.StatusId = db.Status.FirstOrDefault(x => x.Key == StatusKeys.Archived).Id;
                    db.SaveChanges();

                    if (UM.RCSActionType.Key == RCSActionTypeKeys.NotHabitable)
                    {
                        int AwaitingExitInspection = db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingExitInspection).Id;
                        //EHCRoundRobin((int)LeaseApplication.PropertyLeaseApplicationId, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                        EHCRoundRobin((int)rcsApps.Id, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                        MatchingHelper.ChangeApplicationStatus(db, AwaitingExitInspection, rcsApps.Id);
                        Session["MaintenanceJobSheetSession"] = string.Format($"Job sheet approved for application reference ,{rcsApps.ApplicationReferenceNumber} , Application sent back to conduct exit inspection");

                    }
                    else if (UM.RCSActionType.Key == RCSActionTypeKeys.HabitableMinorDefects)
                    {
                        Session["MaintenanceJobSheetSession"] = string.Format($"Job sheet approved for application reference ,{rcsApps.ApplicationReferenceNumber} , due to minor defects there are no changes to application process flow.");

                    }

                    //                        1      2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17      18   19
                }
                else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
                {
                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.CustomerQueryPending).Id, (int)id);
                    db.SaveChanges();
                    Session["MaintenanceJobSheetSession"] = string.Format($"Job sheet rejected for application reference ,{rcsApps.ApplicationReferenceNumber}");
                }
            }


            return RedirectToAction("PropertyLeaseInspections");
        }


















        [DecryptParameter]
        public ActionResult WaitingListReEntryConfirmation(int? id)
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

            PropertyLeaseApplication rcsApps = null;
            LeaseDetails leaseDetails = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status).Include(x => x.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();

            leaseDetails = db.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.IsDeleted == false)
                .Include(r => r.CreatedBySystemUser)
                .Include(r => r.PurchaserType)
                .Include(r => r.ModifiedBySystemUser)
                .Include(r => r.Status)
                .Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).FirstOrDefault() ?? null;


            ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Habitable || x.Key == RCSActionTypeKeys.HabitableMinorDefects || x.Key == RCSActionTypeKeys.NotHabitable).OrderBy(x => x.Name), "Key", "Name");

            ViewBag.UnitInspectionComment = null;


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
                List<Attachments> attachments = new List<Attachments>();
                var atth = db.Attachments.FirstOrDefault();
                attachments.Add(atth);

                var vm = new DepartmentsApprovalViewModel
                {
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    LeaseDetails = leaseDetails,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description


                };

                Entity entity = null;
                Agent agent = null;

                vm.Customer = customer;

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

                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                DocumentsViewModel dvmTemplate = new DocumentsViewModel();

                bool IsUpload = true;
                var returnUrl = "";
                MatchingHelper.DocumentConductUnitInspection(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, returnUrl, rcsApps.Id, IsUpload);
                MatchingHelper.DocumentGetConductUnitInspectionTemplate(dvmTemplate, db, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, false);

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
                vm.DocumentsViewModelTemplate = dvmTemplate;


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
        public ActionResult WaitingListReEntryConfirmation(int? id, string ApprovalStatusddl, string UnitInspectionComment)
        {
            Initialise();
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();
            if (ApprovalStatusddl == RCSActionTypeKeys.PlmYes)
            {
                //customer email here
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.WaitingListReEntry).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                MatchingHelper.SaveToWaitingListQueue(db, (int)rcsApps.Id, rcsApps.CustomerId);
                //Send e-mail and SMS notification
                int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.plm_re_list_to_queue).Id;
                EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
            }
            if (ApprovalStatusddl == RCSActionTypeKeys.PlmNo)
            {

                //customer email here
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.WaitingListExit).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, rcsApps.Id, ActivityTrackerMessage, Customer.Id);

                //Send e-mail and SMS notification
                int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.plm_remove_from_queue).Id;
                EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
            }

            return RedirectToAction("PropertyLeaseInspections");
        }




















        [DecryptParameter]
        public ActionResult AcceptanceLetter(int rcsAppId)
        {
            eServicesDbContext cxt = new eServicesDbContext();
            try
            {
                Initialise();
                var appUnit = cxt.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsAppId && x.IsActive);
                var matched = cxt.MatchedUnits.FirstOrDefault(x => x.Id == appUnit.MatchedID && x.IsAccepted);
                var appstatus = db.PropertyLeaseApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == rcsAppId);
                if (appstatus.Status.Key == StatusKeys.AwaitingDepositPaid) return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = rcsAppId, appId = "" });

                var units = cxt.Units.FirstOrDefault(x => x.Id == matched.UnitsId && x.IsTaken);
                var Unit = cxt.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == matched.UnitsEkurhuleniHousingCompanyId && x.IsTaken);

                return View(units);

            }
            catch (Exception io)
            {
                EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }

            return RedirectToAction("Login", "Account");
        }
        [DecryptParameter]
        [HttpPost]
        public ActionResult AcceptanceLetter(int? rcsAppId)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                var userID = Customer.Id;
                var Keys = cxt.Status;
                var rcsApps = cxt.PropertyLeaseApplications.Where(x => x.Id == rcsAppId && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();
            
                int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.AcceptanceLetter).Id;
                EmailHelper.CustomerEmailNotification(cxt, rcsApps.Id, emailboodyId);
                MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepositPaid).Id, rcsApps.Id);
                return RedirectToAction("Inbox");
            }
        }


        [DecryptParameter]
        public ActionResult ClientTraining(int id)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    PropertyLeaseApplication rcsApps = null;

                    rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.HumanEHCOptions).Include(r => r.Status).Include(r => r.PurchaserType)
                      .Where(x => x.Id == id).FirstOrDefault();
                    //var rCSApplicationStatus = db.RCSApplicationStatus.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).Where(x => x.CustomerId == Customer.Id).Where(x => x.Id == rcsAppId).FirstOrDefault();

                    //var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    //var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    //int num = 5;

                    //return RedirectToAction("ProofOfPaymentAssessment", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = rcsAppId })));
                    MeetingRequest mr = new MeetingRequest();
                    var vm = new DepartmentsApprovalViewModel
                    {
                        
                        PropertyLeaseApplications = rcsApps,
                        MeetingRequest = mr
                            


                    };
                    return View(vm);
                    MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingLeaseAgreement).Id, id);

                    return RedirectToAction("PropertyLeaseApplications");

                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        public Int32 getLoggedInUser()
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                return 1172;
                return SystemUser.Id;
            }
        }


        [DecryptParameter]
        [HttpPost]
        public ActionResult ClientTraining(int? id, string ApprovalStatusddl, DepartmentsApprovalViewModel dvm)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                var userID = Customer.Id;
                var Keys = cxt.Status;
                var rcsApps = cxt.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();

                // Create MeetingRequest
                MeetingRequest meeting = dvm.MeetingRequest;
                meeting.PropertyLeaseApplicationId = (int)id;
                meeting.CustomerId = userID;
                cxt.MeetingRequests.Add(meeting);
                cxt.SaveChanges();

                // ✅ CREATE TENANT TRAINING RECORD HERE!
                var existingTraining = cxt.TenantTrainings.FirstOrDefault(t => t.PropertyLeaseApplicationId == id && !t.IsDeleted);

                if (existingTraining == null)
                {
                    // Generate unique token
                    var token = Guid.NewGuid().ToString();
                    var expiryDate = DateTime.Now.AddDays(30); // 30 days to complete

                    var training = new TenantTraining
                    {
                        PropertyLeaseApplicationId = (int)id,
                        InvitationToken = token,
                        TokenExpiryDate = expiryDate,
                        InvitationSentDate = DateTime.Now,
                        CurrentSlideNumber = 0,
                        IsTrainingCompleted = false,
                        IsExamPassed = false,
                        ExamAttempts = 0,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false
                    };

                    cxt.TenantTrainings.Add(training);
                    cxt.SaveChanges();
                }

                // Send email
                int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InviteTenantForTraining).Id;
                EmailHelper.CustomerEmailNotification(cxt, rcsApps.Id, emailboodyId);

                // ✅ REMOVE THE OLD STATUS LINE - KEEP ONLY THE NEW ONE
                // MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id, rcsApps.Id);

                // Change status to AwaitingOnlineTraining
                MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingOnlineTraining).Id, rcsApps.Id);

                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.CommunityDevelopmentOfficer).FirstOrDefault().Value);
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.InviteToClientTraining).FirstOrDefault();
                MatchingHelper.RoundRobinMarkJobAsFinished(cxt, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, activeDirectoryOn);

                Session["ClientTrainingInviteSession"] = string.Format($"Tenant has been invited successfully for application reference ,{rcsApps.ApplicationReferenceNumber}");
                return RedirectToAction("PropertyLeaseTenantTraining");
            }
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult ClientTrainingOLD(int? id, string ApprovalStatusddl, DepartmentsApprovalViewModel dvm)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                var userID = Customer.Id;
                var Keys = cxt.Status;
                var rcsApps = cxt.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();
                MeetingRequest meeting = dvm.MeetingRequest;
                meeting.PropertyLeaseApplicationId = (int)id;
                meeting.CustomerId = userID;
                cxt.MeetingRequests.Add(meeting);
                cxt.SaveChanges();

                int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InviteTenantForTraining).Id;
                EmailHelper.CustomerEmailNotification(cxt, rcsApps.Id, emailboodyId);
                //MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id, rcsApps.Id);
                MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingOnlineTraining).Id, rcsApps.Id);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.CommunityDevelopmentOfficer).FirstOrDefault().Value);
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.InviteToClientTraining).FirstOrDefault();
                MatchingHelper.RoundRobinMarkJobAsFinished(cxt, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, activeDirectoryOn);

                Session["ClientTrainingInviteSession"] = string.Format($"Tenant has been invited successfully for application reference ,{rcsApps.ApplicationReferenceNumber}");
                return RedirectToAction("PropertyLeaseTenantTraining");
            }
            
        }

        [DecryptParameter]
        public ActionResult GenerateLeaseAgreement(Int32 rcsAppId)
             //public async Task<ActionResult> GenerateLeaseAgreement(int rcsAppId)
        {
            var cxt = new eServicesDbContext();
           
                try
                {
                    Initialise();
                    var leaseInfo = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.HumanEHCOptions).Include(r => r.PurchaserType).Include(r => r.Status)
                      .Where(x => x.Id == rcsAppId).FirstOrDefault();

                    var lease = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == leaseInfo.Id);
                    var AppUnit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == leaseInfo.Id && x.IsActive && !x.IsDeleted);
                    var match = db.MatchedUnits.FirstOrDefault(x => x.Id == AppUnit.MatchedID);
                    //var Unit = db.Units.Include(o=>o.PreferredComplexArea).FirstOrDefault(x => x.Id == match.UnitsId && x.IsTaken);
                    //var Units = db.UnitsEkurhuleniHousingCompany.Include(o=>o.PreferredComplexArea).FirstOrDefault(x => x.Id == match.UnitsEkurhuleniHousingCompanyId);
                    var Units = db.ApplicationAllocatedProperty.Include(o => o.OfferedComplex).FirstOrDefault(x => x.Id == match.ApplicationAllocatedPropertyId);

                    MatchingHelper.PropertyLeaseMasterData(db, Units, lease, leaseInfo);



                    //if (leaseInfo.HousingType == HousingTypeKeys.Company)
                    //{
                    //    var refNumber = await CreatePdfAsync(leaseInfo, "Lease1", "Lease1");
                    //}
                    //else
                    //{
                    //    var refNumber = await CreatePdfAsync(leaseInfo, "Lease2", "Lease1");
                    //}

                    //start of code for post

                    var docdets = cxt.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading);
                    var attachments = cxt.Attachments.Where(x => x.PropertyLeaseApplicationId == rcsAppId && x.DocumentTypeId == docdets.Id).ToList();
                    //MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.LeaseAgreementGenerated).Id, rcsAppId);

                    //var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);

                    //var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement).FirstOrDefault();
                    //MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsAppId, null, ResponsibilityTypeId.Id, Customer.Id);



                    //var custmusers = cxt.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    //var ActivityTrackerMessage = cxt.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.PropertyLeaseAgreementGenerated).Description.ToString();
                    //MatchingHelper.ActivityTrackerAudit(cxt, rcsAppId, ActivityTrackerMessage, custmusers.Id);

                    ////Send e-mail and SMS notification
                    //int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                    //EmailHelper.CustomerEmailNotification(cxt, rcsAppId, emailboodyId);
                    ////end of code for post

                    ViewBag.ApprovalStatus = new SelectList(cxt.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

                    var vm = new DepartmentsApprovalViewModel()
                    {
                        Attachments = attachments,
                        PropertyLeaseApplications = leaseInfo,
                        DocName = docdets.Name,
                        DocDesc = docdets.Description
                    };
                    ViewBag.PropertyLeaseAppliactionId = leaseInfo.Id;
                    return View(vm);
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
        public ActionResult GenerateLeaseAgreement(int rcsAppId, string ApprovalStatusddl, string Reason)
        //public async Task<ActionResult> GenerateLeaseAgreement(int rcsAppId)
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
                    var AppUnit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == leaseInfo.Id && x.IsActive && !x.IsDeleted);
                    var match = db.MatchedUnits.FirstOrDefault(x => x.Id == AppUnit.MatchedID);
                    //var Unit = db.Units.Include(o=>o.PreferredComplexArea).FirstOrDefault(x => x.Id == match.UnitsId && x.IsTaken);
                    //var Units = db.UnitsEkurhuleniHousingCompany.Include(o=>o.PreferredComplexArea).FirstOrDefault(x => x.Id == match.UnitsEkurhuleniHousingCompanyId);
                    var Units = db.ApplicationAllocatedProperty.Include(o => o.OfferedComplex).FirstOrDefault(x => x.Id == match.ApplicationAllocatedPropertyId);

                    MatchingHelper.PropertyLeaseMasterData(db, Units, lease, leaseInfo);



                    //if (leaseInfo.HousingType == HousingTypeKeys.Company)
                    //{
                    //    var refNumber = await CreatePdfAsync(leaseInfo, "Lease1", "Lease1");
                    //}
                    //else
                    //{
                    //    var refNumber = await CreatePdfAsync(leaseInfo, "Lease2", "Lease1");
                    //}
                    var docdets = cxt.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading);
                    var attachments = cxt.Attachments.Where(x => x.PropertyLeaseApplicationId == rcsAppId && x.DocumentTypeId == docdets.Id).ToList();
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement).FirstOrDefault();
                    switch (ApprovalStatusddl)
                    {
                        case RCSActionTypeKeys.Approved:
                            //start of code for post

                         
                            MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.LeaseAgreementGenerated).Id, rcsAppId);

                            var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);

                          
                            MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsAppId, null, ResponsibilityTypeId.Id, Customer.Id);



                            var custmusers = cxt.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                            var ActivityTrackerMessage = cxt.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.PropertyLeaseAgreementGenerated).Description.ToString();
                            MatchingHelper.ActivityTrackerAudit(cxt, rcsAppId, ActivityTrackerMessage, custmusers.Id);

                            //Send e-mail and SMS notification
                            int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                            EmailHelper.CustomerEmailNotification(cxt, rcsAppId, emailboodyId);
                            //end of code for post
                            Session["LeaseAgreementReview"] = string.Format($"Risk assessment reallocated for application reference ,{leaseInfo.ApplicationReferenceNumber}");
                            break;
                        case RCSActionTypeKeys.Rejected:
                            MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantUpdateDetails).Id, (Int32)leaseInfo.Id);
                            MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsAppId, null, ResponsibilityTypeId.Id, Customer.Id);

                            EHCRoundRobin(leaseInfo.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                            //customer email here
                           MatchingHelper.ActivityTrackerAudit(db, leaseInfo.Id, "Lease agreement review rejected, application sent back to update tenant lease details to update fields used on Lease Agreement", Customer.Id);


                            LeaseReviewComment lrc = new LeaseReviewComment();
                            lrc.PropertyLeaseApplicationId = leaseInfo.Id;
                            lrc.RCSActionTypeId = cxt.RCSActionTypes.FirstOrDefault(x => x.Key == RCSActionTypeKeys.Rejected).Id;
                            lrc.Comment = Reason;
                            lrc.LeaseDetailsId = lease.Id;
                            cxt.LeaseReviewComments.Add(lrc);
                            cxt.SaveChanges();

                            var test = Customer.Id;
                            var test2 = SystemUser.Id;
                            Session["LeaseAgreementReview"] = ("Lease agreement review rejected by BO: "+ SystemUser.UserFullName+", for application with reference number: "+ leaseInfo.ApplicationReferenceNumber+ ", Reason: " + Reason);
                            break;
                    }



                    return RedirectToAction("PropertyLeaseAgreements", "PropertyLeaseApplication");

                    var vm = new DepartmentsApprovalViewModel()
                    {
                        Attachments = attachments,
                        PropertyLeaseApplications = leaseInfo,
                        DocName = docdets.Name,
                        DocDesc = docdets.Description
                    };
                    ViewBag.PropertyLeaseAppliactionId = leaseInfo.Id;
                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }


        [DecryptParameter]
        public ActionResult DebitOrderRegistration(int rcsAppId, string Data)
        {
            try
            {
                var rcsApp = db.PropertyLeaseApplications.Include(r=>r.PurchaserType).Include(r=>r.CreatedBySystemUser).Include(r=>r.Customer).Include(r=>r.IdentificationType).FirstOrDefault(x => x.Id == rcsAppId && !x.IsDeleted);
                var matchedUnits = db.MatchedUnits.Where(x => x.PropertyLeaseApplicationId == rcsApp.Id).ToList().FirstOrDefault();
            
                var units = db.Units.Where(x => x.Id == 0).FirstOrDefault() ?? null;

                //var unit = db.UnitsEkurhuleniHousingCompany.Include(r=>r.PreferredComplexArea).Where(x => x.Id == matchedUnits.UnitsEkurhuleniHousingCompanyId).FirstOrDefault() ?? null;
                var unit = db.ApplicationAllocatedProperty.Include(r => r.OfferedComplex).Where(x => x.Id == matchedUnits.ApplicationAllocatedPropertyId).FirstOrDefault() ?? null;
                var debitorder = db.DebitOrderRegistrations.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApp.Id)??null;
                if (debitorder != null)
                {
                    if (debitorder.Rejected == true)
                    {
                        ViewBag.RejectedDebitOrder = "true";
                        ViewBag.RejectedDebitOrderInfo = debitorder.RejectionComment;
                        Session["ReCapure"] = "true";
                    }
                }
                var vm = new DepartmentsApprovalViewModel
                {
                    DebitOrderRegistration = debitorder,
                    Units = units,
                    PropertyLeaseApplications = rcsApp,
                };
                ViewBag.UnitsID = unit.Id;
                ViewBag.RentalAmount = unit.MonthlyRentalAmount.ToString("C");
                ViewBag.Applicant = String.Format("{0} {1}", rcsApp.FirstName, rcsApp.LastName);
                ViewBag.UNITNo = String.Format("{0}, {1}", unit.SpaceUnitNumber.ToUpper(), unit.OfferedComplex.Name.ToUpper());
                ViewBag.Data = Data;
                return View(vm);
            }
            catch (Exception IO)
            {
                return View("_Error");
            }
        }

        [HttpPost]
        public ActionResult DebitOrderRegistration(DepartmentsApprovalViewModel model, string Data, int UnitsID)
        {
            using ( var context = new eServicesDbContext())
            {
                try
                {
                    var findItem = context.DebitOrderRegistrations.OrderByDescending(r => r.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == model.PropertyLeaseApplications.Id) ?? null;
                    //var units = context.UnitsEkurhuleniHousingCompany.Include(r => r.PreferredComplexArea).FirstOrDefault(x => x.Id == UnitsID);
                    var units = context.ApplicationAllocatedProperty.Include(r => r.OfferedComplex).FirstOrDefault(x => x.Id == UnitsID);
                    if (findItem != null)
                    {
                        var propertyapplication = context.PropertyLeaseApplications.FirstOrDefault(x => x.Id == model.PropertyLeaseApplications.Id);
                        var debit = findItem;
                        debit = model.DebitOrderRegistration;
                        debit.Email = debit.Email != null ? debit.Email : propertyapplication.PurEmail;
                        debit.BankName = model.DebitOrderRegistration.BankName;
                        debit.BankNumber = model.DebitOrderRegistration.BankNumber;
                        debit.UnitNumber = units.SpaceUnitNumber.ToUpper() + ", " + units.OfferedComplex.Name.ToUpper();
                        debit.Email = model.DebitOrderRegistration.Email;
                        debit.CellNo = model.DebitOrderRegistration.CellNo;
                        debit.PropertyLeaseApplicationId = propertyapplication.Id;
                        debit.TenantSignature = true;
                        debit.DebitCheckActionDate = DateTime.Now;
                        debit.RentalAmount = units.MonthlyRentalAmount.ToString();
                        MatchingHelper.UpdateDebitOrder(context, model.DebitOrderRegistration.Id, debit);

                        int emailboodyId = context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ReDebitOrderRegister).Id;
                        EmailHelper.CustomerEmailNotification(context, propertyapplication.Id, emailboodyId);
                    }
                    else
                    {
                        var propertyapplication = context.PropertyLeaseApplications.FirstOrDefault(x => x.Id == model.PropertyLeaseApplications.Id);
                        var unit = context.Units.FirstOrDefault(x => x.Id == UnitsID);
                       
                        DebitOrderRegistration debit = new DebitOrderRegistration();
                        debit = model.DebitOrderRegistration;
                        debit.UnitNumber = units.SpaceUnitNumber.ToUpper() + ", " + units.OfferedComplex.Name.ToUpper();
                        debit.PropertyLeaseApplicationId = propertyapplication.Id;
                        debit.Email = debit.Email != null ? debit.Email : propertyapplication.PurEmail;
                        debit.TenantSignature = true;
                        debit.DebitCheckActionDate = DateTime.Now;
                        debit.RentalAmount = units.MonthlyRentalAmount.ToString();
                        context.DebitOrderRegistrations.Add(debit);
                        context.SaveChanges();

                        //Send e-mail and SMS notification
                        int emailboodyId = context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.DebitOrderRegister).Id;
                        EmailHelper.CustomerEmailNotification(context, propertyapplication.Id, emailboodyId);
                    }
                    
                    return RedirectToAction("GenerateDebitOrder", "PropertyLeaseApplication", new { q = Data });
                }
                catch (Exception IO)
                {

                    throw;
                }
                
            }
        }

        [EncryptedActionParameter]

        public async Task<ActionResult> GenerateDebitOrder(int rcsAppId)
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

                    var leased = db.LeaseDetails.OrderByDescending(x=>x.Id).Where(x => !x.IsDeleted && x.IsNew).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.ModifiedBySystemUser)
                     .Include(r => r.PurchaserType).Include(r => r.Status)
                      .Where(x => x.PropertyLeaseApplicationId == leaseInfo.Id).FirstOrDefault();

                    if (leaseInfo.Status.Key == StatusKeys.IncentivePolicyApplicationProcessing) return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = leaseInfo.Id, appId = "" });

                    var refNumber = await CreatePdfAsync(leaseInfo, "DebitOrderAuthority", "DebitOrderAuthority");

                    var docdets = cxt.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundWaterMeterReading);
                    var attachments = cxt.Attachments.Where(x => x.PropertyLeaseApplicationId == rcsAppId && x.DocumentTypeId == docdets.Id).ToList();
                    MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.IncentivePolicyApplicationProcessing).Id, rcsAppId);

                    EHCRoundRobin(leaseInfo.Id, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                    //Send e-mail and SMS notification
                    int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.GenerateDebitOrderAuthorityForm).Id;
                    EmailHelper.CustomerEmailNotification(db, rcsAppId, emailboodyId);

                    var vm = new DepartmentsApprovalViewModel()
                    {
                        LeaseDetails=leased,
                        Attachments = attachments,
                        PropertyLeaseApplications = leaseInfo,
                        DocName = docdets.Name,
                        DocDesc = docdets.Description
                    };

                    ViewBag.PropertyLeaseAppliactionId = leaseInfo.Id;

                    //not sure what message should be recorded here
                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantDebitOrderGenerated).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(db, rcsAppId, ActivityTrackerMessage, custmusers.Id);
                    //Test2();    
                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        [EncryptedActionParameter]
        public ActionResult UploadAssessmentFee(int rcsAppId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                      .Where(x => x.Id == rcsAppId).FirstOrDefault();



                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    int num = 5;

                    return RedirectToAction("ProofOfPaymentAssessment", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = rcsAppId })));



                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        [EncryptedActionParameter]
        public ActionResult UploadApplicationFee(int rcsAppId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                      .Where(x => x.Id == rcsAppId).FirstOrDefault();



                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    int num = 5;

                    return RedirectToAction("ProofOfApplicationFeePayment", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = rcsAppId })));



                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }


        public string GenerateViewToPDF(int id, string formName, string fileName, string urlDomain, string controllerName, string viewName, string LeaseType)
        {
            string pdfname = "";
            try
            {
                //string Pdf_Name_FromDb = db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.AgriKey).Value.ToString();
                var FindDownloadDOom = db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.DownloadDomain);
                //public domain
                var FindPublicDomain = db.AppSettings.FirstOrDefault(v => v.Key == AppSettingKeys.PublicDownloadDomain);
                var myUniqueFileName = string.Format(@"{0}.pdf", Guid.NewGuid());

                string Date = Convert.ToString(DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + DateTime.Now.Millisecond + DateTime.Now.Minute);
                
                switch (LeaseType)
                {
                    case "Lease1":
                        pdfname = fileName + "Lease1" + myUniqueFileName;
                        break;
                    case "DebitOrderAuthority":
                        pdfname = fileName + "DebitOrderAuthority" + myUniqueFileName;
                        break;
                    default:
                        pdfname = fileName + "LeaseDefault" + myUniqueFileName;
                        break;
                }
                var root = System.Web.Hosting.HostingEnvironment.MapPath("~/PDFFiles");
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                // var filePath = Path.Combine(Server.MapPath("~/PDFFiles/"), pdfname)


                //var filePath = FindDownloadDOom.Value + "PDFFiles/" + pdfname;
                var filePath = FindPublicDomain.Value + "PDFFiles/" + pdfname;

                //AppSetting pdfDomain = db.AppSettings.FirstOrDefault(o => o.Key == AppSSettingKeys.PDFDomain);
                string pdfDomain = FindDownloadDOom.Value;
                // uncomment below link and comment out below that
                urlDomain = pdfDomain;
                //  urlDomain = "localhost:3456/";
                //controllerName = "CloudBasedForms";
                //viewName = "OnlineApplications";
                //url="localhost:3456/CloudBasedForms/OnlineApplications"


                var url = string.Format("{0}/{1}/{2}/{3}", urlDomain, controllerName, viewName, id);
                //   var url = string.Format("{0}/{1}/{2}/{3}", null, controllerName, viewName, id);

                var actionPDF = new Rotativa.UrlAsPdf(url)
                {
                    FileName = fileName,
                    //SaveOnServerPath = path, // JK.20200404a - Deprecated, save bytes as below.
                    PageSize = Rotativa.Options.Size.A4,
                    PageOrientation = Rotativa.Options.Orientation.Portrait,
                    PageMargins = { Left = 1, Right = 1, Top = 2 }
                };

                byte[] applicationPDFData = actionPDF.BuildFile(ControllerContext);
                var length = actionPDF.BuildFile(ControllerContext).Length;
                // JK.20200404a - SaveOnServerPath is deprecated, have to save bytes manually.
                System.IO.File.WriteAllBytes(path, applicationPDFData);

                var fullpathtofile = path;
                var mimetype = "application/pdf";
                var filecontents = System.IO.File.ReadAllBytes(fullpathtofile);
                string FormatedFile = string.Format("{0}|{1}|{2}|{3}", mimetype, filePath, applicationPDFData, length);

                var document = new Document();
                //try
                //{
                //    //HttpPostedFileBase file = filecontents;
                //Models.File oFile = new Models.File();

                //Stream fileContent = filecontents.InputStream;


                ////Stream fileContent = file.InputStream;
                //oFile.FileName = fileName;
                //oFile.ContentType = mimetype;
                //oFile.Content = new byte[filecontents.Length];
                ////fileContent.Read(oFile.Content, 0, (int)length);
                //oFile.FileSize = (Int64)length;

                //    db.Files.Add(oFile);
                //    db.SaveChanges();
                //    var dbLocationType = db.LocationTypes.FirstOrDefault(l => l.Key == LocationTypeKeys.Database);
                //    var status = db.Status.SingleOrDefault(o => o.Key == StatusKeys.DocumentUploaded);
                //    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                //    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                //    document = new Document()
                //    {
                //        CustomerId = 45,
                //        ReferenceTypeId = referenceType.Id,
                //        ReferenceId = 45,
                //        DocumentCheckListId = 27/*Convert.ToInt32(documentCheckListIds[i])*/,
                //        LocationTypeId = dbLocationType.Id,
                //        StatusId = status.Id,
                //        PropertyLeaseApplicationId = 1,
                //        DocumentLocation = string.Format("{0}", "eServicesDb"),
                //        DocumentName = oFile.FileName,
                //        FileId = oFile.Id,
                //        IsActive = true,
                //        IsDeleted = false
                //    };
                //    db.Documents.Add(document);
                //    db.SaveChanges();


                //    //if (document.Id > 0)
                //    //{
                //    //    db.DocumentReferences.Add(new DocumentReference()
                //    //    {
                //    //        DocumentId = document.Id,
                //    //        ReferenceId = referenceId,
                //    //        ApplicationId = applicationId,
                //    //        ReferenceTypeId = referenceTypeId,
                //    //        IsActive = true,
                //    //        IsDeleted = false
                //    //    });

                //    //    //db.SaveChanges();
                //    //    success = true;
                //    //}

                //    db.SaveChanges();


                //    //    var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == rcsappid);
                //    //    if (RcsApplication != null)
                //    //    {
                //    //        DepartmentsApprovalsController dac = new DepartmentsApprovalsController();
                //    //        //var docresult = dac.CaptureDocChecker(RcsApplication.Id);
                //    //        //if (docresult== "false")
                //    //        //{
                //    //        //}
                //    //        if (RcsApplication.Status.Key == StatusKeys.AwaitingDepositPaid)
                //    //        {
                //    //            RcsApplication.StatusId = db.Status.FirstOrDefault(o => o.Key == StatusKeys.PendingAssessmentFeePaymentValidation).Id;

                //    //            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ConevayncerUploadsApplicationFeeProofOfPayment).Description.ToString()/* + DecisionType*/;
                //    //            var RCSHistoryLog = new RCSApplicationHistoryLog
                //    //            {
                //    //                PropertyLeaseApplicationId = RcsApplication.Id,
                //    //                AuditAction = ActivityTrackerMessage,
                //    //                UserId = customerId,
                //    //                CreatedDateTime = DateTime.Now,
                //    //                IsActive = true
                //    //            };
                //    //            db.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                //    //            db.SaveChanges();
                //    //        }


                


                //}


                ////}
                //catch (Exception ex)
                //{
                //    SecurityHelper.LogError(new Exception(
                //        $"Document Error: {ex.Message}  Inner Exception: {ex.InnerException}"), null);
                //}
                //return new FileContentResult(filecontents, mimetype);
                return FormatedFile;
            }
            catch (Exception x)
            {
                db.Logs.Add(new Log()
                {
                    LogTypeId = 1,
                    LogEntry = x.Message.ToString(),
                    ReferenceId = 0,
                    ReferenceTypeId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                });
                db.SaveChanges();
                //throw x;
            }

            return null;
        }




        public ActionResult Modashow()
        {
            return View();
        }
        #region PaymentGateway
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string pgMerchantId, string voteNumber, string pgMerchantReference,
            string pgMerchantDescription, string Amount, string pgEmail, string pgMobile,
            string customerFirstName, string customerLastName, string returnUrl, string adhocRef1,
            string adhocRef2, string adhocRef3, string adhocRef4, string adhocRef5)
        {
            returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
            returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
            string amt = Amount.Replace('.', ',');
            decimal conAmt = Convert.ToDecimal(amt);
            returnUrl = returnUrl + "/ReturnBackUrl";
            //Format  parameters into Single Delimited String
            string enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}",
                pgMerchantId, voteNumber, pgMerchantReference, pgMerchantDescription, Amount,
                pgEmail, pgMobile, customerFirstName, customerLastName, returnUrl, adhocRef1, adhocRef2, adhocRef3, adhocRef4, adhocRef5);
            //Encrypt the Single String to and Encrypted string e
            var e = new AesCrypto(encp).Encrypt(enc);

            AppSetting PGDomain = db.AppSettings.FirstOrDefault(o => o.Key == PaymentGatewayKeys.PGDomain);


            var segment = string.Join("q?", e);
            var escapedSegment = Uri.EscapeDataString(segment);
            var baseFormat = PGDomain.Value + "PaymentGateway/PaygateTest?q=" + e;
            //+"https://www.google.co.za/maps/search/{0}/"
            var url = string.Format(baseFormat, escapedSegment);
            // {e} is your encrypted String
            //return RedirectToAction("PayGate", new { q = e });
            return Redirect(baseFormat);
        }

        [DecryptParameter]
        public ActionResult Index2(int RcsApplicationId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    //var refno = Session["saveRef"].ToString();
                    //Session["saveRef"] = "";

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                        .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                        .Where(x => x.CustomerId == Customer.Id).FirstOrDefault();

                    var PropertyLeaseApp = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Status)
                        .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                        .Where(x => x.Id == RcsApplicationId).FirstOrDefault();

                    //var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.CustomerProfile).FirstOrDefault();
                    //var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                    //int num = 5;

                    //var statuses = cxt.Status.ToList();
                    //if (rCSApplicationStatus.StatusId == statuses.FirstOrDefault(o => o.Key == StatusKeys.AwaitingAssessmentPayment).Id)
                    //{
                    //    rCSApplicationStatus.StatusId = statuses.FirstOrDefault(o => o.Key == StatusKeys.UploadPOPApplicationFee).Id;

                    //    db.Entry(rCSApplicationStatus).State = EntityState.Modified;
                    //    db.SaveChanges();
                    //}

                    Session["Display"] = "True";
                    Session["ApplicationRefNo"] = PropertyLeaseApp.ApplicationReferenceNumber;
                    if (PropertyLeaseApp.Status.Key==StatusKeys.AwaitingRiskAssessment)
                    {
                        Session["MessageBody"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.PLMSuccessMessage).FirstOrDefault().Body + PropertyLeaseApp.ApplicationReferenceNumber;
                        Session["MessageTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.PLMSuccessMessage).FirstOrDefault().Title + PropertyLeaseApp.ApplicationReferenceNumber;
                    }
                    else if (PropertyLeaseApp.Status.Key == StatusKeys.AwaitingApplicationFeeUpload)
                    {
                        var message =
 "You have successfully uploaded the required documents.\n\n" +
 "Please pay the application fee of R250 via EFT:\n" +
 "Bank: ABSA\n" +
 "Account Name: GREATER GERMISTON HOUSING CORP PHASE 2\n" +
 "Account Number: 405620225\n" +
 "Company Reg No: 2000/007937/07\n" +
 "Branch: PRESIDENT GERMISTON\n" +
 "Branch Code: 334542\n" +
 "SWIFT: ABSAZAJJ\n\n" +
 "After payment, go to PLM Applications > Inbox, search your ref number, select Upload Application Fee, and upload your proof.\n" +
 "Application Reference Number: " + PropertyLeaseApp.ApplicationReferenceNumber;

                        // New names requested
                        Session["ApplicationFeeBody"] = message;
                        Session["ApplicationFeeTitle"] = "Awaiting Application Fee Payment";

                        // Keep old keys for backward compatibility with other views/logic
                        Session["MessageBody"] = message;
                        Session["MessageTitle"] = "Awaiting Application Fee Payment";

                    }
                    else {

                        Session["MessageBody"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.PLMReuploadMessage).FirstOrDefault().Body + PropertyLeaseApp.ApplicationReferenceNumber;
                        Session["MessageTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.PLMReuploadMessage).FirstOrDefault().Title + PropertyLeaseApp.ApplicationReferenceNumber;
                    }
                    
                    //if (Session["ApplicationRefNo"] != null)
                    //{
                    //    ViewBag.MessageBody = Session["ApplicationRefNo"].ToString();
                    //    ViewBag.MessageTitle = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.SuccessMessage).FirstOrDefault().Name;
                    //}
                    //if (Session["ReceiptValues"] != null)
                    //{
                    //    ViewBag.Value = Session["ReceiptValues"];
                    //}
                    //string ss = "true";
                    //AesCrypto aes = new AesCrypto();
                    //var test = aes.Encrypt(ss);
                    return RedirectToAction("Inbox"/*, new { param = test}*/);
                    return RedirectToAction("Login", "Account");
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
        public ActionResult UpdateTenanntDocuments(int? Id)
        {
            var lease = db.LeaseDetails.Where(x => x.PropertyLeaseApplicationId == Id).LastOrDefault();
            using (var es = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    return RedirectToAction("IndexTenants", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(
                        new
                        {
                            referenceId = Customer.Id,
                            customerId = Customer.Id,
                            referenceTypeId = 12,
                            applicationId = application.Id,
                            agentId = application.Id,
                            rcsappId = lease.PropertyLeaseApplicationId
                        })));
                }
                catch (Exception e)
                {

                }
            }
            return View();
        }

        [DecryptParameter]
        public ActionResult PropertyDetails(int? Id)
        {
            try
            {
                var units = db.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == Id && !x.IsDeleted && !x.IsTaken) ?? null;
                return View(units);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult Properties()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {

                    Initialise();

                    var rrq = new List<int>();
                    var Key = cxt.PreferredComplexAreas.ToList();
                    rrq.Add(Key.FirstOrDefault(r => r.Key == PrefferedComplexKeys.AirportparkStaff).Id);
                    rrq.Add(Key.FirstOrDefault(r => r.Key == PrefferedComplexKeys.ChrisHaniStaff).Id);
                    rrq.Add(Key.FirstOrDefault(r => r.Key == PrefferedComplexKeys.DelvilleStaff).Id);
                    var rCSApplicationStatus = db.UnitsEkurhuleniHousingCompany.Include(r => r.PreferredComplexArea).Include(r => r.HumanEHCOption).Where(x => x.IsDeleted == false && !x.IsTaken && x.PreferredComplexArea.LettingOfficerId == Customer.Id).ToList();
                    if (User.IsInRole("Client Services Officer"))
                    {
                        rCSApplicationStatus = db.UnitsEkurhuleniHousingCompany.Include(r => r.PreferredComplexArea).Include(r => r.HumanEHCOption).Where(x => x.IsDeleted == false && !x.IsTaken && x.PreferredComplexArea.LettingOfficerId == Customer.Id).ToList();
                    }
                    else
                    {
                        rCSApplicationStatus = db.UnitsEkurhuleniHousingCompany.Include(r => r.PreferredComplexArea).Include(r => r.HumanEHCOption).Where(x => x.IsDeleted == false && !x.IsTaken && !rrq.Contains((int)x.PreferredComplexAreaId)).ToList();
                    }
                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = item.PropertyPrice.ToString("C");
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
        public ActionResult EHCPropertyUnitMaintanance()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    List<UnitsEkurhuleniHousingCompany> rCSApplicationStatus = new List<UnitsEkurhuleniHousingCompany>();

                    if (User.IsInRole("Client Services Officer"))
                    {
                        var Areas = db.PreferredComplexAreas.Where(x => x.LettingOfficerId == Customer.Id).ToList();
                        var List = Areas.Select(x => x.Id);
                        rCSApplicationStatus = db.UnitsEkurhuleniHousingCompany.Where(x => x.IsDeleted == false && x.Inspection).Include(r => r.HumanEHCOption).Include(r => r.PreferredComplexArea).ToList();
                    }

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = item.PropertyPrice.ToString("C");

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
        public ActionResult PropertyUnitMaintanance()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    List<UnitsEkurhuleniHousingCompany> rCSApplicationStatus = new List<UnitsEkurhuleniHousingCompany>();
                    
                    if (User.IsInRole("Client Services Officer"))
                    {
                        var Areas = db.PreferredComplexAreas.Where(x => x.LettingOfficerId == Customer.Id).ToList();
                        var List = Areas.Select(x => x.Id);
                        rCSApplicationStatus = db.UnitsEkurhuleniHousingCompany.Where(x => x.IsDeleted == false && x.Inspection).Include(r=>r.HumanEHCOption).Include(r => r.PreferredComplexArea).ToList();
                    }

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = item.PropertyPrice.ToString("C");
                        
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
        public ActionResult AddProperty()
        {
            try
            {
                Initialise();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();

                Units units = new Units();
                units.AgentLastName = systemusers.LastName;
                units.AgentName = systemusers.FirstName;
                var dvm = new DepartmentsApprovalViewModel
                {
                    Units = units
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
            catch (Exception e)
            {

            }
            return View();
            ViewBag.EnvisageUsage = new SelectList(db.envisagedUsages.OrderBy(x => x.UssageName).ToList(), "Id", "UssageName");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.Occuupation = new SelectList(db.OccupationTypes.OrderBy(x => x.Propertytype), "Id", "Propertytype");
            ViewBag.ComplexArea = new SelectList(db.PreferredComplexAreas.OrderBy(x => x.Name), "Id", "Name");
            ViewBag.Settlement = new SelectList(db.humanEHCOptions.OrderBy(x => x.Name), "Id", "Name");


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProperty(DepartmentsApprovalViewModel unitvalues)
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
                Add.OccupationTypeId = 10;

                Add.UnitBuildingName = unitvalues.Units.UnitBuildingName;
                Add.PreferredComplexAreaId = unitvalues.Units.PreferredComplexAreaId;
               
                Add.Address = unitvalues.Units.Address;
                Add.Surburb = unitvalues.Units.Surburb;
                Add.BedroomCount = unitvalues.Units.BedroomCount;

                Add.PropertyPrice = unitvalues.Units.PropertyPrice;
                Add.PropertyDeposit = unitvalues.Units.PropertyDeposit;
                Add.LettingRequirements = unitvalues.Units.LettingRequirements;

                db.Units.Add(Add);
                db.SaveChanges();
                return RedirectToAction("AddProperty");
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

        public ActionResult EvictionsLeaseManagement()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    
                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.PurchaserType).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();

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
        public ActionResult CommitteeEvictionsOutcomes()
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

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CommitteeOutcomes).FirstOrDefault().Id;

                    rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId &&x.ClerkId== UserId && x.StatusId == SubmittedId).ToList();

                    var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                    int NonCompliance = db.Status.FirstOrDefault(x => x.Key == StatusKeys.NonCompliance).Id;
                    int NonPayment = db.Status.FirstOrDefault(x => x.Key == StatusKeys.NonPayment).Id;
                    int Subletting = db.Status.FirstOrDefault(x => x.Key == StatusKeys.Subletting).Id;
                    int IllegalActivities = db.Status.FirstOrDefault(x => x.Key == StatusKeys.IllegalActivities).Id;

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id) && (x.StatusId == NonCompliance || x.StatusId == NonPayment || x.StatusId == Subletting || x.StatusId == IllegalActivities))
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.Customer)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.HumanEHCOptions)
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
        
        public ActionResult UpdateLeaseDetails()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var Keys = cxt.Status.ToList();
                    var activeDirectoryOn = Customer.Id;

                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CaptureLeaseDetails).FirstOrDefault().Id;

                    rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId && x.ClerkId == activeDirectoryOn && x.EndTaskDateTime==null).ToList();
                    var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                    int AwaitingTenantUpdateDetails = db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingTenantUpdateDetails).Id;

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id) && x.StatusId == AwaitingTenantUpdateDetails)
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.Customer)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.HumanEHCOptions)
                        .Include(r => r.Status).ToList();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    if (Session["UpdateLeaseDetailsSession"] != null)
                    {
                        var value = Session["UpdateLeaseDetailsSession"].ToString();
                        Session["UpdateLeaseDetailsSession"] = null;
                        ViewBag.UpdateLeaseDetailsSession = value;
                    }
                    Session["UpdateLeaseDetailsSession"] = null;

                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        //uifiuiuf

        public ActionResult _Error(int Id, int? appId)
        {
            var plmApp = db.PropertyLeaseApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == Id);
            var lease = appId != null ? db.LeaseDetails.Include(r => r.Status).FirstOrDefault(x => x.Id == appId) : null;
            var status = appId == null ? plmApp.Status : lease.Status;

            var mpt = "";
            var In = " is In ";
            var dddd = "true";
            var end = "you are unable to re-submit on the previous  application step.";
            if (appId != null && ((status.Key == StatusKeys.AwaitingRiskAssessment) || (status.Key == StatusKeys.AwaitingRenewalDocuments) || (status.Key == StatusKeys.ApplicationUpForRenewalAtThreeMonths) || (status.Key == StatusKeys.InAwaitingPropertyManagersReview) || (status.Key == StatusKeys.InAwaitingRevenueManagersReview) || (status.Key == StatusKeys.AwaitingTenantAcceptance)))
            {
                end = "you are unable to action this lease while in review.";
            }

            Session["MessageTitle"] = "Invalid Application Status";
            Session["MessageBody"] = "Your application with reference no. " + plmApp.ApplicationReferenceNumber + In + status.Name.Replace("In ", mpt) + " status, " + end;
            Session["Display"] = dddd;
            Session["ApplicationRefNo"] = dddd;
            return RedirectToAction("Inbox", "PropertyLeaseApplication");

            if ((Session["Controller"].ToString() == null || Session["Controller"].ToString() == "") && (Session["View"].ToString() == null || Session["View"].ToString() == ""))
            {
                return RedirectToAction("Inbox", "PropertyLeaseApplication");
            }
            else
            {
                var Controller = Session["Controller"].ToString();
                var View = Session["View"].ToString();
                Session["Controller"] = null;
                Session["View"] = null;
                return RedirectToAction(View, Controller);
            }
        }

        public static string GetPublicIp()
        {
            string address = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                address = stream.ReadToEnd();
            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);


            var host = Dns.GetHostEntry(Dns.GetHostName());
            var Ip2 = String.Empty;
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Ip2 = ip.ToString();
                    return address + "/" + Ip2+"/"+ System.Web.HttpContext.Current.Request.UserHostAddress;
                }
            }
            return "";
            
        }

        public ActionResult Inbox()
        
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    MatchingHelper.WaitingListNotificationAtOneYear(cxt);
                    MatchingHelper.MatchUnitParallelProcessor(cxt);
                    MatchingHelper.RenewalNotificationAtEndOfTime(cxt);
                   //MatchingHelper.ValidateWaitingListTime(cxt);

                    List<int> rrq = new List<int>();
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingRiskAssessment).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.awaited).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.ReuploadApplicationDocs).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.Approved).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingAssessmentPayment).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingDepositPaid).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.ViewRCCCertificate).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.IncentivePolicyApplicationPending).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.ReuploadApplicationDocs).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingTenantDocuments).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.InAwaitingWaitingListReEntry).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.LeaseAgreementGenerated).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwitingInspectionSchedule).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.UploadAssessmentFeePayment).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingApplicationFeeValidation).Id);
                    rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingApplicationFeeUpload).Id);
                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && rrq.Contains((int)x.StatusId))
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.Customer)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.HumanEHCOptions)
                        .Include(r => r.Status).Where(x => x.CustomerId == Customer.Id).ToList();



                    // --- Application Fee modal initialization (new keys with legacy fallback)
                    string applicationFeeBody = null;
                    string applicationFeeTitle = null;

                    if (Session["ApplicationFeeBody"] != null)
                    {
                        applicationFeeBody = Session["ApplicationFeeBody"].ToString();
                        Session["ApplicationFeeBody"] = null;
                    }
                    else if (Session["MessageBody"] != null)
                    {
                        // legacy fallback
                        applicationFeeBody = Session["MessageBody"].ToString();
                        Session["MessageBody"] = null;
                    }

                    if (Session["ApplicationFeeTitle"] != null)
                    {
                        applicationFeeTitle = Session["ApplicationFeeTitle"].ToString();
                        Session["ApplicationFeeTitle"] = null;
                    }
                    else if (Session["MessageTitle"] != null)
                    {
                        // legacy fallback
                        applicationFeeTitle = Session["MessageTitle"].ToString();
                        Session["MessageTitle"] = null;
                    }

                    if (!string.IsNullOrEmpty(applicationFeeBody))
                    {
                        ViewBag.ApplicationFeeBody = applicationFeeBody;
                        ViewBag.ApplicationFeeTitle = applicationFeeTitle ?? "Notification";

                        // ensure modal display logic in view will run
                        ViewBag.Display = "True";

                        // clear any request-scoped flags to avoid duplicate messages
                        Session["Display"] = null;
                        Session["ApplicationRefNo"] = null;
                    }

                    if (Session["Display"] != null)
                    {
                        if (Session["ApplicationRefNo"] != null)
                        {
                            Session["Display"] = "True";
                            var message = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.PLMSuccessMessage).FirstOrDefault();

                            if (Session["Display"].ToString() == "True")
                            {
                                var refr = Session["ApplicationRefNo"].ToString();
                         
                                ViewBag.Display = "True";
                                ViewBag.MessageTitle3 = Session["MessageTitle"].ToString();
                                ViewBag.MessageBody3 = Session["MessageBody"].ToString();
                                Session["MessageBody"] = null;
                            }
                            Session["Display"] = null;
                            Session["ApplicationRefNo"] = null;
                            Session["MessageTitle"] = null;
                        }
                    }
                    if (Session["DepositPaidDocSession"] != null)
                    {
                        var value = Session["DepositPaidDocSession"].ToString();
                        Session["DepositPaidDocSession"] = null;
                        ViewBag.DepositPaidDocumentSession = value;
                    }
                    Session["DepositPaidDocSession"] = null;
                    if (Session["LeaseAgreementSignedSession"] != null)
                    {
                        var value = Session["LeaseAgreementSignedSession"].ToString();
                        Session["LeaseAgreementSignedSession"] = null;
                        ViewBag.DepositPaidDocumentSession = value;
                    }
                    Session["LeaseAgreementSignedSession"] = null;
                    




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


        public ActionResult AllPropertyLeases()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var rCSApplicationStatus = cxt.LeaseDetails.OrderByDescending(r=>r.Id).Include(r=>r.Status).Include(r=>r.PropertyLeaseApplication).Include(r=>r.SystemUser).Include(r=>r.PurchaserType).ToList();
                    List<LeaseDetailsViewModel> vm = new List<LeaseDetailsViewModel>();

                    var application = cxt.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    var referenceType = cxt.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                    var WarningLetter = cxt.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.WarningLetter);
                    int documentCheckListId = cxt.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == WarningLetter.Id && dcl.ReferenceTypeId == referenceType.Id).Id;
                    foreach (var item in rCSApplicationStatus)
                    {
                        var rr = new LeaseDetailsViewModel();
                        var mm = cxt.MatchedUnits.OrderByDescending(r=>r.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.PropertyLeaseApplicationId && x.IsAccepted)?.UnitsEkurhuleniHousingCompanyId??null;
                        rr.EkurhulreniEHCUnitId = mm != null ? mm.ToString() : 1000.ToString();
                        rr.StatusName = item.Status.Name;
                        rr.TenantType = item.PurchaserType.Name;
                        rr.TenantFullName = item.FirstNames + " " + item.LastName;
                        rr.ReferenceNumber = item.leaseApplicationRef;
                        rr.CreatedDateTime = item.CreatedDateTime;
                        rr.Id = item.Id;
                        rr.PropertyLeaseApplicationId = item.PropertyLeaseApplicationId;
                        rr.CompletionStatus = item.Completed == true ? "Completed" : "In Progress";

                        var customer = cxt.Customers.Include(s => s.SystemUser).Include(s => s.Status).Include(s => s.CustomerType).FirstOrDefault(c => c.Id == item.PropertyLeaseApplication.CustomerId);
                        rr.WarningLetterCount = MatchingHelper.WarningLetterCount(cxt, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", item.PropertyLeaseApplicationId, documentCheckListId);
                        vm.Add(rr);
                    }
                    if (Session["SendWarningLetterSession"] != null)
                    {
                        var value = Session["SendWarningLetterSession"].ToString();
                        Session["SendWarningLetterSession"] = null;
                        ViewBag.SendWarningLetterSession = value;
                    }
                    Session["SendWarningLetterSession"] = null;
                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        [DecryptParameter]
        public ActionResult UnitDetails(int Id)
        {
            var unit = db.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == Id && !x.IsDeleted) ?? null;
            UnitViewModel model = new UnitViewModel
            {
                UnitsEkurhuleniHousingCompany = unit ?? null,
                UnitDescription = db.humanEHCOptions.FirstOrDefault(x => x.Id == unit.HumanEHCOptionId)?.Name ?? "No Description Available"
            };
            return View(model);
        }
        public List<int> ReturnStatusList()
        {
            var Key = db.Status.ToList();
            var rr = new List<int>
                    {
                    Key.FirstOrDefault(x => x.Key == StatusKeys.TerminateAtEndOfPeriod).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.EndOfLeaseTerm).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.TerminatedLease).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.TerminationApproved).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.TerminationDateIssued).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.TerminationLeaseByTenant).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.LeaseTerminatedDueToComplaints).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.AwaitingterminantionApproval).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.EvictionGranted).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.AwaitingCommitteEviction).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.AwaitingPropertyEviction).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.TenantEvictionApproved).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.ApplicationUpForRenewalAtThreeMonths).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantAccountBalanceReview).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.InAwaitingPropertyManagersReview).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.InAwaitingRevenueManagersReview).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRiskAssessment).Id,
                    Key.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalDocuments).Id,
                    };
            return rr;
        }

        public ActionResult ServeNotice()
        {
            using (var core = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    MatchingHelper.WaitingListNotificationAtOneYear(core);
                    MatchingHelper.MatchUnitParallelProcessor(core);
                    MatchingHelper.RenewalNotificationAtEndOfTime(core);

                    var p = core.PropertyLeaseApplications.Where(x => x.CustomerId == Customer.Id).ToList();
                    var rrq = p.Select(x => x.Id).ToList();

                    var rr = ReturnStatusList();

                    var rCSApplicationStatus = core.LeaseDetails
                        .Include(r => r.PurchaserType)
                        .Include(r => r.PropertyLeaseApplication)
                        .Include(r => r.Status)
                        .Where(x => rrq.Contains(x.PropertyLeaseApplicationId) && !rr.Contains((int)x.StatusId) && x.IsActive && x.NoticeDate == null).ToList();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    if (Session["TenantServeNoticeSession"] != null)
                    {
                        var value = Session["TenantServeNoticeSession"].ToString();
                        Session["TenantServeNoticeSession"] = null;
                        ViewBag.TenantServeNoticemsgSession = value;
                    }
                    Session["TenantServeNoticeSession"] = null;
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult DepartmentalInput()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var Customerid = Customer;
                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                    var userID = User.Identity.GetUserId();
                    var COEDepart = db.DepartmentsCoEs.Where(x => x.RepresentedBy == userID).FirstOrDefault().DepartmentName;
                   
                    var PropertyApplications = db.ApplicationDepart.Where(x => x.IsDeleted == false && x.DepartmentType == COEDepart && x.PropertyLeaseApplicationId != null).Include(r => r.Status).ToList();
                    var LeaseApplications = db.ApplicationDepart.Where(x => x.IsDeleted == false && x.DepartmentType == COEDepart && x.LeaseDetailsId != null).Include(r => r.Status).Include(r => r.PurchaserType).ToList();
            
                   
                    foreach (var item in PropertyApplications)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    foreach (var item in LeaseApplications)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    var vm = new DepartmentsApprovalViewModel
                    {
                        ApplicationDepartmentListProperty = PropertyApplications,
                        ApplicationDepartmentList = LeaseApplications
                    };


                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }


        public ActionResult DepartmentAffected()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && x.StatusId == db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingDepartmentInputs).Id
                        || x.StatusId == db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingRiskAssessment).Id)
                        .Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r=>r.PurchaserType).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();

                    var LeaseApplications = db.LeaseDetails.Where(x => x.IsDeleted == false && x.StatusId == db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingDepartmentInputs).Id
                        || x.StatusId == db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingRiskAssessment).Id)
                        .Include(r => r.CreatedBySystemUser).Include(r => r.ModifiedBySystemUser).Include(r => r.PurchaserType).Include(r => r.Status).ToList();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    foreach (var item in LeaseApplications)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    var vm = new DepartmentsApprovalViewModel
                    {
                        PropertyLeaseApplicationList = rCSApplicationStatus,
                        LeaseDetailsList = LeaseApplications
                    };


                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        [Authorize(Roles = "Client Services Officer,Community Development Officer,Property Manager,Revenue Manager,RevenueOfficer")]
        [HttpGet]
        public ActionResult PropertyTenantCommunication()
        {
            try
            {
               
                 
                Initialise();
                var vm = new DepartmentsApprovalViewModel();
                if (User.IsInRole("Client Services Officer"))
                {
                    var UserId = Customer.Id;
                    var ComplexNames = db.PreferredComplexAreas.Where(x => x.LettingOfficerId == UserId && x.Key != "ekurhuleni_complex" && x.IsActive).ToList();
                    var cc = ComplexNames.Select(x => x.Id).ToList();
                    var mat = db.MatchedUnits.Include(r => r.UnitsEkurhuleniHousingCompany).Where(x => x.UnitsEkurhuleniHousingCompany.IsTaken && cc.Contains((int)x.UnitsEkurhuleniHousingCompany.PreferredComplexAreaId)).ToList();
                    var m = mat.Select(x => x.Id).ToList();
                    var rec = db.ApplicantUnits.Include(r => r.PropertyLeaseApplication).Where(x => m.Contains(x.MatchedID) && x.PropertyLeaseApplicationId != null).ToList();
                    var rrq = rec.Select(x => x.PropertyLeaseApplicationId).ToList();
                    var list = db.PropertyLeaseApplications.Where(x => rrq.Contains(x.Id)).ToList().Select(x => x.SystemUserId);
                    var users = db.SystemUsers.Where(x => list.Contains(x.Id)).ToList();


                    vm.Features = new SelectList(ComplexNames.OrderBy(r => r.Name), "Id", "Name");
                    vm.Data = ComplexNames;
                    ViewBag.Users = new SelectList(users.OrderBy(x=>x.UserFullName), "Id", "UserFullName").ToList();
                    ViewBag.Features = new SelectList(ComplexNames, "Id", "Name");
                    ViewBag.Complexes = ComplexNames;

                }
                else if (User.IsInRole("Community Development Officer") || (User.IsInRole("Property Manager")) || (User.IsInRole("Revenue Manager")) || (User.IsInRole("Revenue Officer")))
                {
                    var UserId = Customer.Id;
                    var ComplexNames = db.PreferredComplexAreas.Where(x => x.IsActive && x.Key != "ekurhuleni_complex").ToList();
                    var cc = ComplexNames.Select(x => x.Id).ToList();
                    var mat = db.MatchedUnits.Include(r => r.UnitsEkurhuleniHousingCompany).Where(x => x.UnitsEkurhuleniHousingCompany.IsTaken).ToList();
                    var m = mat.Select(x => x.Id).ToList();
                    var rec = db.ApplicantUnits.Where(x => m.Contains(x.MatchedID) && x.PropertyLeaseApplicationId != null).ToList();
                    var rrq = rec.Select(x => x.PropertyLeaseApplicationId).ToList();
                    var list = db.PropertyLeaseApplications.Where(x => rrq.Contains(x.Id)).ToList().Select(x => x.SystemUserId);
                    var users = db.SystemUsers.Where(x => list.Contains(x.Id)).ToList();


                    vm.Features = new SelectList(ComplexNames.OrderBy(r=>r.Name), "Id", "Name");
                    vm.Data = ComplexNames;
                    ViewBag.Users = new SelectList(users.OrderBy(x => x.UserFullName), "Id", "UserFullName").ToList();
                    ViewBag.Features = new SelectList(ComplexNames, "Id", "Name");
                }
                ViewBag.SMS = false;
                ViewBag.EMAIL = false;
                ViewBag.POSTAL = false;
                var success = "";
                if (Session["success"] != null && Session["empt"] != null)
                {
                    success = Session["success"].ToString();
                    ViewBag.success = success;
                    ViewBag.True = Session["empt"].ToString();
                    Session["success"] = null;
                    Session["empt"] = null;
                }
                return View(vm);
            }
            catch (Exception Io)
            {

                
            }
            return View();
        }

        [Authorize(Roles = "Client Services Officer,Community Development Officer,Property Manager,Revenue Manager,RevenueOfficer")]
        [HttpPost]
        public ActionResult PropertyTenantCommunication(DepartmentsApprovalViewModel vm, string CommunicationType, string Title, string MessageBody, string Title2, string MessageBody2, bool EMAIL, bool SMS, bool POSTAL, int? User, List<PreferredComplexArea> Features, params string[] SelectedRoles)
        {
            try
            {
                Initialise();
                var success = "";
                if (CommunicationType == "Individual")
                {
                    if (User != null)
                    {
                        var rr = db.SystemUsers.FirstOrDefault(x => x.Id == Customer.SystemUserId);
                        var user = UserManager.FindByName(rr.UserName);
                        var userId = user.Id;
                        string role = UserManager.GetRoles(userId).FirstOrDefault();
                        var users = db.SystemUsers.Where(x => x.Id == User).ToList();
                        var p = db.PropertyLeaseApplications.Where(x => x.SystemUserId == User).ToList();
                        var messagebody = MessageBody;
                        messagebody += "<br/><br/>";
                        messagebody += "<b>PLM Management</b><br/>";
                        messagebody += "<b>" + role + "</b><br/>";
                        messagebody += rr.FullName + "</b></b>";
                        messagebody += "Cell: " + rr.MobileNumber;
                        messagebody += "<br/><br/>";
                        messagebody += "Email: " + rr.EmailAddress;

                        Email email = new Email();
                        var result = email.GenerateBulkEmails(users, Title.ToUpper(), messagebody, SMS, EMAIL, POSTAL, AppSettingKeys.EservicesDefaultEmailTemplate, p, Customer.Id, false, "Tenats", null);

                        success = "Communication sent to " + users.FirstOrDefault().FullName + " successfully. " + result;
                        Session["empt"] = "success";
                    }
                    else
                    {
                        success = "Communication not sent, invalid complex.";
                        Session["empt"] = "empty";
                    }
                }
                else if (CommunicationType == "PerComplex")
                {

                    if (SelectedRoles != null)
                    {
                        var rr = db.SystemUsers.FirstOrDefault(x => x.Id == Customer.SystemUserId);
                        var user = UserManager.FindByName(rr.UserName);
                        var userId = user.Id;
                        string role = UserManager.GetRoles(userId).FirstOrDefault();
                        List<int> dd = new List<int>();
                        foreach (var I in SelectedRoles)
                        {
                            dd.Add(Convert.ToInt16(I));
                        }
                        var UserId = Customer.Id;
                        var ComplexNames = db.PreferredComplexAreas.Where(x => dd.Contains(x.Id)).ToList();
                        var cc = ComplexNames.Select(x => x.Id).ToList();
                        var mat = db.MatchedUnits.Include(r => r.UnitsEkurhuleniHousingCompany).Where(x => x.UnitsEkurhuleniHousingCompany.IsTaken && cc.Contains((int)x.UnitsEkurhuleniHousingCompany.PreferredComplexAreaId)).ToList();
                        var m = mat.Select(x => x.Id).ToList();
                        var rec = db.ApplicantUnits.Include(r => r.PropertyLeaseApplication).Where(x => m.Contains(x.MatchedID) && x.PropertyLeaseApplicationId != null).ToList();
                        var rrq = rec.Select(x => x.PropertyLeaseApplicationId).ToList();
                        var p = db.PropertyLeaseApplications.Where(x => rrq.Contains(x.Id)).ToList();
                        var list = p.Select(x => x.SystemUserId);
                        var users = db.SystemUsers.Where(x => list.Contains(x.Id)).ToList();


                        int counter = 0;
                        var names = "";
                        foreach (var Roles in ComplexNames)
                        {
                            if (counter > 0)
                            {
                                names += ", ";
                            }
                            counter++;
                            names += Roles.Name.ToUpper();
                        }

                        var messagebody = names;
                        messagebody += "<br/><br/>";
                        messagebody += MessageBody;
                        messagebody += "<br/><br/>";
                        messagebody += "<b>PLM Management</b><br/>";
                        messagebody += "<b>" + role + "</b><br/>";
                        messagebody += rr.FullName + "</b></b>";
                        messagebody += "Cell: " + rr.MobileNumber;
                        messagebody += "<br/><br/>";
                        messagebody += "Email: " + rr.EmailAddress;

                        Email email = new Email();
                        var result =email.GenerateBulkEmails(users, Title.ToUpper(), messagebody, SMS, EMAIL, POSTAL, AppSettingKeys.EservicesDefaultEmailTemplate, p, Customer.Id, true, "Tenats", null);
                        vm.Features = new SelectList(ComplexNames, "Id", "Name");
                        ViewBag.Users = new SelectList(users, "Id", "UserFullName").ToList();
                        ViewBag.SMS = false;
                        ViewBag.EMAIL = false;
                        ViewBag.POSTAL = false;
                        success = "Communication sent to selected group(s) successfully. "+result;
                        Session["empt"] = "success";
                    }
                    else
                    {
                        success = "Communication not sent, invalid complex.";
                        Session["empt"] = "empty";
                    }
                }
                ViewBag.success = success;
                Session["success"] = success;
                return RedirectToAction("PropertyTenantCommunication");
            }
            catch (Exception Io)
            {


            }
            return View();
        }

        public ActionResult ApplicantRiskAssessment()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    //MatchingHelper.MatchUnitParallelProcessor(cxt);
                    var Keys = cxt.Status;
                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    int UserId = Customer.Id;
                    List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RiskAssessment).FirstOrDefault().Id;
                    var ResponsibilityTypeId2 = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalRiskAssessment).FirstOrDefault().Id;

                    rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => (x.ResponsibilityTypeId == ResponsibilityTypeId || x.ResponsibilityTypeId == ResponsibilityTypeId2) && x.ClerkId == UserId && x.EndTaskDateTime == null).ToList();

                    var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();
                    var list2 = rrq.Select(x => x.LeaseDetailsId).ToList();

                    var AwaitingRiskAssessment = cxt.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingRiskAssessment).Id;

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id) && x.StatusId == AwaitingRiskAssessment)
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.Customer)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.HumanEHCOptions)
                        .Include(r => r.Status).ToList();

                    var LeaseApplications = db.LeaseDetails.Where(x => x.IsDeleted == false && list2.Contains(x.Id) && x.StatusId == AwaitingRiskAssessment)
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.Status).ToList();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    foreach (var item in LeaseApplications)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    var vm = new DepartmentsApprovalViewModel
                    {
                        PropertyLeaseApplicationList = rCSApplicationStatus,
                        LeaseDetailsList = LeaseApplications
                    };

                    if (Session["ConductRiskAssessmentSession"] != null)
                    {
                        var value = Session["ConductRiskAssessmentSession"].ToString();
                        Session["ConductRiskAssessmentSession"] = null;
                        ViewBag.ConductRiskAssessmentSession = value;
                    }
                    Session["ConductRiskAssessmentSession"] = null;
                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }




        public ActionResult Vetted()
        {
            IEnumerable<PropertyLeaseApplication> apps = db.PropertyLeaseApplications.Include(r => r.CreatedBySystemUser)
                        .Include(r => r.Customer)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.HumanEHCOptions)
                        .Include(c => c.Status).Where(a => a.Status.Key == StatusKeys.VertedApplication);

            return View(apps);
        }

        [HttpGet]
        [Authorize]
        [DecryptParameter]
        public ActionResult AllocateOrMatchUnit(Int32 Id)
        {
            Initialise();
            PropertyLeaseApplication app = db.PropertyLeaseApplications.Include(r => r.CreatedBySystemUser)
                        .Include(r => r.Customer)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r=>r.PreferredComplexArea)
                           .Include(r => r.PreferredComplexArea2)
                        .Include(r => r.HumanEHCOptions)
                        .Include(c => c.Status).FirstOrDefault(a => a.Status.Key == StatusKeys.VertedApplication && a.Id == Id);
            CaptureViewModel captureViewModel = new CaptureViewModel();
            captureViewModel.PropertyLeaseApplication = app;
            MatchedUnits matchedUnits = db.MatchedUnits
                   .OrderByDescending(a => a.Id)
                   .Include(c => c.ApplicationAllocatedProperty)
                   .FirstOrDefault(a => a.PropertyLeaseApplicationId == Id);

            if (matchedUnits != null)
                captureViewModel.ApplicationAllocatedProperties = matchedUnits.ApplicationAllocatedProperty;
            IEnumerable<Int32> systemIdentityUsers = IdentityManager.FindUsersInRole("Client Services Officer").Select(a => a.SystemUserId);
            IEnumerable<Customer> customerObjects = db.Customers.Where(a => systemIdentityUsers.Contains((Int32)a.SystemUserId) && (Int32)a.SystemUserId != SystemUser.Id).ToList();

            ViewBag.Id = Id;
            ViewBag.LettingOfficer = new SelectList(customerObjects, "Id", "FullName");
            ViewBag.HumanEHCOption = new SelectList(db.PreferredComplexAreas.OrderBy(a => a.Name).Where(a => !a.Key.Equals("ekurhuleni_complex")), "Id", "Name");
            ViewBag.ApprovalStatus = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReallocateApplication).OrderBy(x => x.Name), "Key", "Name");

            return View(captureViewModel);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AllocateOrMatchUnit(CaptureViewModel capture, Int32 Id)
        {
            try
            {
                using (eServicesDbContext context= new eServicesDbContext())
                {

                    Initialise();
                    ApplicationAllocatedProperty collection = capture.ApplicationAllocatedProperties;
                    ApplicationAllocatedProperty collection1 = context.ApplicationAllocatedProperty.FirstOrDefault(a => a.SolarReference == collection.SolarReference && a.SpaceUnitNumber == collection.SpaceUnitNumber);

                    if (collection1 != null)
                    {
                        collection1.RequiedDepositAmount = collection.RequiedDepositAmount;
                        collection1.MonthlyRentalAmount = collection.MonthlyRentalAmount;
                        collection1.LettingRequirements = collection.LettingRequirements;
                        collection1.StreetName = collection.StreetName;
                        collection1.Township = collection.Township;
                        collection1.Postal = collection.Postal;
                        collection1.IsTaken = true;
                        unitAllocationService.Update(collection1);
                    }
                    else
                    {
                        collection.AllocatedByUserId = SystemUser.Id;
                        collection.PropertyLeaseApplicationId = Id;
                        unitAllocationService.Save(collection);
                    }
                    

                    MatchedUnits matchedUnit = new MatchedUnits
                    {
                        ApplicationAllocatedPropertyId = collection.Id,
                        PropertyLeaseApplicationId = Id,
                        IsAccepted = false,
                        RejectedProperty = false,
                    };
                    unitAllocationService.Save(matchedUnit);

                    MatchingHelper.ChangeApplicationStatus(context, (Int32)context.Status.FirstOrDefault(x => x.Key == StatusKeys.awaited)?.Id, Id);
                    String ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantUploadsDocuments).Description.ToString();
                    //Send e-mail and SMS notification
                    Int32 emailboodyId = context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                    EmailHelper.CustomerEmailNotification(context, Id, emailboodyId);
                }
                return View("Vetted", "PropertyLeaseApplication");
            }
            catch (Exception)
            {
                return View("Vetted", "PropertyLeaseApplication");
                throw;
            }
            


            return View();
        }


        [DecryptParameter]
        public ActionResult PropertyReviewValidation(int? Id)
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
                  .Where(x => x.Id == Id).FirstOrDefault();

                rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                  .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Seconded || x.Key == RCSActionTypeKeys.NotSeconded).OrderBy(x => x.Name), "Key", "Name");

                var referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                if (referenceType == null) throw new Exception("Invalid reference type.");
                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                DocumentsViewModel dvm = new DocumentsViewModel();
                MatchingHelper.DocumentRenewalLetter(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, false);
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
                ViewBag.OfficerName = p.Clerk.FirstName + " " + p.Clerk.LastName;
                ViewBag.ActionComment = q == null ? Actions.FirstOrDefault(x=>x.Key==RCSActionTypeKeys.NotRenew).Name+ ": " + p.RejectReason : f + ": " + p.RejectReason;
                ViewBag.DateCaptured = p.CreatedDateTime;

                ViewBag.RejectComment = "";
                ViewBag.PropertyComment = "";
                return View(vm);
            }
            catch (Exception)
            {
                throw;
            }
            return View();
        }

        [HttpPost]
        [DecryptParameter]
        public ActionResult PropertyReviewValidation(int Id, string PropertyComment,string ApprovalStatus, string RejectComment)
        {
            using (var _conx = new eServicesDbContext())
            {
                var lease = _conx.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.Id == Id && x.IsNew).Include(r => r.PurchaserType).Include(r => r.Status).FirstOrDefault();
                Initialise();

                PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                {
                    PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId,
                    RejectReason = RejectComment,
                    LeaseRenewalValidation = true
                };
                _conx.propertyLeaseActionComments.Add(comments);
                _conx.SaveChanges();

                if (ApprovalStatus == RCSActionTypeKeys.Seconded)
                {
                    MatchingHelper.SavePropertyManagersResponse(_conx, lease.PropertyLeaseApplicationId, RejectComment, true);
                    MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.InAwaitingRevenueManagersReview).Id, (int)lease.Id);
                    var custmusers = _conx.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _conx.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeasePassRenewalValidation).Description.ToString() + " : " + lease.LeaseReferenceNo;
                    MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    int emailboodyId = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                    //EmailHelper.CustomerEmailNotification(_conx, lease.PropertyLeaseApplicationId, emailboodyId);
                }
                if (ApprovalStatus == RCSActionTypeKeys.NotSeconded)
                {
                    MatchingHelper.SavePropertyManagersResponse(_conx, lease.PropertyLeaseApplicationId, RejectComment, false);
                    MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.InAwaitingRevenueManagersReview).Id, (int)lease.Id);
                    var custmusers = _conx.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _conx.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeaseFailRenewalValidation).Description.ToString() + " : " + lease.LeaseReferenceNo;
                    MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    int emailboodyId = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                    //EmailHelper.CustomerEmailNotification(_conx, lease.PropertyLeaseApplicationId, emailboodyId);
                }
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SecondLeaseRenewal).FirstOrDefault();
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)lease.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, activeDirectoryOn);

                //                                                     1      2      3      4       5     6     7       8      9      10     11    12     13     14     15     16     17     18    19     20     21     22     23   24    25    26   27                                                       
                EHCRoundRobin((int)lease.PropertyLeaseApplicationId, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, 1, false, false, 1);
                Session["LeaseRenewalSession"] = string.Format($"Property Review Lease renewal processed successfully for application reference ,{lease.LeaseReferenceNo}");
                return RedirectToAction("Renewals", "LeaseDetails");
            }
        }

        [DecryptParameter]
        public ActionResult RevenueReviewValidation(int? Id)
        {
            try
            {
                eServicesDbContext context = new eServicesDbContext();
                Initialise();
                PropertyLeaseApplication rcsApps = null;
                LeaseDetails lease = null;
                PropertyManager propertyManager = null;
                
                lease = db.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.IsDeleted == false && x.IsNew).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.Status)
                  .Include(r => r.PurchaserType)
                  .Where(x => x.Id == Id).FirstOrDefault();

                rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                  .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");
                propertyManager = db.PropertyManagers.OrderByDescending(x => x.CreatedDateTime).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                if (propertyManager.PropertyComments == null || propertyManager.PropertyComments == "")
                {
                    propertyManager.PropertyComments = "No Comment";
                }

                var referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                if (referenceType == null) throw new Exception("Invalid reference type.");
                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                DocumentsViewModel dvm = new DocumentsViewModel();
                MatchingHelper.DocumentRenewalLetter(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, false);

                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    PropertyLeaseApplications = rcsApps,
                    PropertyManager = propertyManager,
                    DocumentsViewModel = dvm
                };
                if (propertyManager.Approved == true)
                {
                    ViewBag.Approved = "Recomendation Seconded By Property Manager";
                }
                else
                {
                    ViewBag.Approved = "Recomendation Not Seconded By Property Manager";
                }

                var Actions = context.RCSActionTypes.ToList();

                var p = context.propertyLeaseActionComments.Include(r => r.CreatedBySystemUser).Include(r => r.Clerk).Where(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.LeaseRenewalValidation).FirstOrDefault();
                var q = context.PropertyLeaseRenewalOffers.Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).FirstOrDefault() ?? null;
                var f = "";
                if (q != null)
                {
                    f = (q.MonthsOffer == 12) ? Actions.FirstOrDefault(x => x.Key == RCSActionTypeKeys.Approve12Months).Name : Actions.FirstOrDefault(x => x.Key == RCSActionTypeKeys.Approve24Months).Name;
                }
                if (p.ClerkId == null)
                {
                    var match = context.MatchedUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.IsAccepted);
                    var comp = context.UnitsEkurhuleniHousingCompany.Include(r => r.PreferredComplexArea).Where(x => x.Id == match.UnitsEkurhuleniHousingCompanyId).FirstOrDefault().PreferredComplexAreaId;
                    var User = context.PreferredComplexAreas.Include(c=>c.LettingOfficer).FirstOrDefault(x => x.Id == comp).LettingOfficer;
                    ViewBag.OfficerName = User.FirstName + " " + User.LastName;
                }
                else
                {
                    ViewBag.OfficerName = p.Clerk.FirstName + " " + p.Clerk.LastName;
                }
                ViewBag.ActionComment = q == null ? Actions.FirstOrDefault(x => x.Key == RCSActionTypeKeys.NotRenew).Name + ": " + p.RejectReason : f + ": " + p.RejectReason;
                ViewBag.DateCaptured = p.CreatedDateTime;

                ViewBag.RejectComment = "";
                ViewBag.PropertyComment = "";
                return View(vm);
            }
            catch (Exception)
            {
                throw;
            }
            return View();
        }

        [HttpPost]
        [DecryptParameter]
        public ActionResult RevenueReviewValidation(int Id, string ApprovalStatus, string RejectComment)
        {
            using (var _conx = new eServicesDbContext())
            {
                Initialise();
                var lease = _conx.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.Id == Id && x.IsNew).Include(r => r.PurchaserType).Include(r => r.Status).FirstOrDefault();
                PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                {
                    PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId,
                    RejectReason = RejectComment,
                    RevenueReviewValidation = true
                };
                _conx.propertyLeaseActionComments.Add(comments);
                _conx.SaveChanges();

                var _re = _conx.PropertyLeaseRenewalOffers.Where(x => x.PropertyLeaseApplicationId == lease.PropertyLeaseApplicationId).FirstOrDefault() ?? null;

                if (ApprovalStatus == RCSActionTypeKeys.Approved)
                {
                    if (_re != null)
                    {
                        MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantAcceptance).Id, (int)lease.Id);
                        var custmusers = _conx.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        var ActivityTrackerMessage = _conx.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeasePassRenewalValidation).Description.ToString() + " : " + lease.LeaseReferenceNo;
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                        int emailboodyId = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RenewalApprovedRecommendation).Id;
                        EmailHelper.CustomerEmailNotification(_conx, lease.PropertyLeaseApplicationId, emailboodyId);
                    }
                    else
                    {
                        MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminateAtEndOfPeriod).Id, (int)lease.Id);
                        var custmusers = _conx.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        var ActivityTrackerMessage = _conx.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeaseFailRenewalValidation).Description.ToString() + " : " + lease.LeaseReferenceNo;
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                        int emailboodyId = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RenewalRejectRecommendation).Id;
                        EmailHelper.CustomerEmailNotification(_conx, lease.PropertyLeaseApplicationId, emailboodyId);
                    }
                    Session["LeaseRenewalSession"] = string.Format($"Revenue Review Lease renewal approved for application reference ,{lease.LeaseReferenceNo}");
                }

                if (ApprovalStatus == RCSActionTypeKeys.Rejected)
                {
                    if (_re != null)
                    {
                        MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminateAtEndOfPeriod).Id, (int)lease.Id);
                        var custmusers = _conx.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        var ActivityTrackerMessage = _conx.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeaseFailRenewalValidation).Description.ToString() + " : " + lease.LeaseReferenceNo;
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                        int emailboodyId = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RenewalRejectRecommendation).Id;
                        EmailHelper.CustomerEmailNotification(_conx, lease.PropertyLeaseApplicationId, emailboodyId);
                    }
                    else
                    {
                        MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantAcceptance).Id, (int)lease.Id);
                        var custmusers = _conx.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        var ActivityTrackerMessage = _conx.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeasePassRenewalValidation).Description.ToString() + " : " + lease.LeaseReferenceNo;
                        MatchingHelper.ActivityTrackerAudit(_conx, lease.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                        int emailboodyId = _conx.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RenewalApprovedRecommendation).Id;
                        EmailHelper.CustomerEmailNotification(_conx, lease.PropertyLeaseApplicationId, emailboodyId);

                        var Offer = new PropertyLeaseRenewalOffer
                        {
                            PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId,
                            MonthsOffer = 12
                        };
                        _conx.PropertyLeaseRenewalOffers.Add(Offer);
                        _conx.SaveChanges();

                        lease.MonthsOffered = 12;
                        _conx.Entry(lease).State = EntityState.Modified;
                        _conx.SaveChanges();

                    }
                    Session["LeaseRenewalSession"] = string.Format($"Revenue Review Lease renewal rejected for application reference ,{lease.LeaseReferenceNo}");
                }

                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseRenewalRevenue).FirstOrDefault();
                var UserId = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueManager).FirstOrDefault().Value);
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)lease.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, UserId);
                return RedirectToAction("Renewals", "LeaseDetails");

            }
        }
        [Authorize(Roles = "Revenue Officer,Client Services Officer")]
        public ActionResult PropertyLeaseApplicationTerminations()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    List<LeaseDetails> rCSApplicationStatus = new List<LeaseDetails>();
                    int UserId = Customer.Id;
                    int TerminationLeaseByTenant = db.Status.FirstOrDefault(r => r.Key == StatusKeys.TerminationLeaseByTenant).Id;
                    int AwaitingTenantAccountBalanceReview = db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingTenantAccountBalanceReview).Id;
                    int TenantNotice = db.Status.FirstOrDefault(r => r.Key == StatusKeys.TenantNotice).Id;
                    int LeaseNotRenewed = db.Status.FirstOrDefault(r => r.Key == StatusKeys.LeaseNotRenewed).Id;
                    int EndOfLeaseTerm = db.Status.FirstOrDefault(r => r.Key == StatusKeys.EndOfLeaseTerm).Id;

                    var Keys = cxt.Status;
                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.TerminationValidation).FirstOrDefault().Id;

                    


                    if (User.IsInRole("Revenue Officer"))
                    {
                        rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                        var list = rrq.Select(x => x.LeaseDetailsId).ToList();

                        rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false&&list.Contains(x.Id) && ( x.StatusId == AwaitingTenantAccountBalanceReview || x.StatusId == EndOfLeaseTerm || x.StatusId == LeaseNotRenewed || x.StatusId == TenantNotice))
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Status)
                            .Include(r => r.ModifiedBySystemUser).ToList();
                    }
                    else if (User.IsInRole("Client Services Officer"))
                    {
                        rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.StatusId == SubmittedId).ToList();
                        var list = rrq.Select(x => x.LeaseDetailsId).ToList();

                        rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false && list.Contains(x.Id) && x.IsNew && x.StatusId == (db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingExitInspection).Id))
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Status)
                            .Include(r => r.ModifiedBySystemUser).ToList();
                    }
                    else
                    {
                        rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false && x.IsNew &&
                    (x.StatusId == (db.Status.FirstOrDefault(r => r.Key == StatusKeys.TerminationLeaseByTenant).Id)
                    || x.StatusId == (db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingPropertyEviction).Id)
                    || x.StatusId == (db.Status.FirstOrDefault(r => r.Key == StatusKeys.TerminationLeaseByTenant).Id)
                    || x.StatusId == (db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingCommitteEviction).Id)
                    || x.StatusId == (db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingTenantAccountBalanceReview).Id)
                    || x.StatusId == (db.Status.FirstOrDefault(r => r.Key == StatusKeys.AwaitingExitInspection).Id)))
                            .Include(r => r.CreatedBySystemUser).Include(r => r.PurchaserType).Include(r => r.Status).Include(r => r.ModifiedBySystemUser).ToList();
                    }
                    
                    

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    if (Session["BOAccountValidationSession"] != null)
                    {
                        var value = Session["BOAccountValidationSession"].ToString();
                        Session["BOAccountValidationSession"] = null;
                        ViewBag.BOAccountValidationTSession = value;
                    }
                    Session["BOAccountValidationSession"] = null;
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult PropertyLeaseApplications()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    MatchingHelper.MatchUnitParallelProcessor(db);

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.PurchaserType)
                        .Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();

                    MatchingHelper.WaitingListNotificationAtOneYear(cxt);
                    MatchingHelper.RenewalNotificationAtEndOfTime(cxt);

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

        public ActionResult MyPropertyLeaseApplications()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    MatchingHelper.MatchUnitParallelProcessor(db);

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && x.CustomerId == Customer.Id).Include(r => r.PurchaserType)
                        .Include(r => r.CreatedBySystemUser).Include(r => r.PurchaserType).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();

                    MatchingHelper.WaitingListNotificationAtOneYear(cxt);
                    MatchingHelper.RenewalNotificationAtEndOfTime(cxt);

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

        public ActionResult PropertyLeaseTenantTraining()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    MatchingHelper.MatchUnitParallelProcessor(db);
                    var Keys = cxt.Status;
                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    int UserId = Customer.Id;
                    List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.InviteToClientTraining).FirstOrDefault().Id;

                    rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId== UserId && x.StatusId == SubmittedId).ToList();

                    var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                    var AssessmentFeePaymentApproved = db.Status.FirstOrDefault(i => i.Key == StatusKeys.AssessmentFeePaymentApproved).Id;

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false &&list.Contains(x.Id) && x.StatusId == AssessmentFeePaymentApproved)
                                                .Include(r => r.CreatedBySystemUser)
                                                .Include(r => r.Customer).Include(r => r.PurchaserType)
                                                .Include(r => r.ModifiedBySystemUser)
                                                .Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();

                    MatchingHelper.WaitingListNotificationAtOneYear(cxt);
                    MatchingHelper.RenewalNotificationAtEndOfTime(cxt);

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                
                    foreach (var item in rCSApplicationStatus)
                    {
                       item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    if (Session["ClientTrainingInviteSession"] != null)
                    {
                        var value = Session["ClientTrainingInviteSession"].ToString();
                        Session["ClientTrainingInviteSession"] = null;
                        ViewBag.ClientTrainingInvitationSession = value;
                    }
                    Session["ClientTrainingInviteSession"] = null;
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult PropertyLeaseAgreements()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    List<PropertyLeaseApplication> rCSApplicationStatus = new List<PropertyLeaseApplication>();
                    List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                    List<PropertyLeaseAgreementMaster> MasterApplication = new List<PropertyLeaseAgreementMaster>();

                    int UserId = Customer.Id;
                    var Keys = db.Status;
                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    int AwaitingManagersSignature = db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingManagersSignature).Id;

                    if (User.IsInRole("Revenue Manager"))
                    {
                        var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseAgreementValidation).FirstOrDefault().Id;
                        var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueManager).FirstOrDefault().Value);

                        rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.Id == activeDirectoryOn && x.StatusId == SubmittedId).ToList();
                        var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                        MasterApplication = db.propertyLeaseAgreementMasters.Where(x => x.RevenueManagerSigned == false && list.Contains(x.PropertyLeaseApplicationId)).ToList();
                        var list2 = MasterApplication.Select(x => x.PropertyLeaseApplicationId).ToList();

                        rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list2.Contains(x.Id) && x.StatusId == AwaitingManagersSignature)
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.Customer).Include(r => r.PurchaserType)
                            .Include(r => r.ModifiedBySystemUser)
                            .Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();
                    }

                    if (User.IsInRole("Property Manager"))
                    {
                        var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseAgreementValidation).FirstOrDefault().Id;
                        var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);

                        rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.Id == activeDirectoryOn && x.StatusId == SubmittedId).ToList();
                        var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                        MasterApplication = db.propertyLeaseAgreementMasters.Where(x => x.PropertyManagerSigned == false && list.Contains(x.PropertyLeaseApplicationId)).ToList();
                        var ListMaster = MasterApplication.Select(x => x.PropertyLeaseApplicationId).ToList();

                        rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && ListMaster.Contains(x.Id) && x.StatusId == AwaitingManagersSignature)
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.Customer).Include(r => r.PurchaserType)
                            .Include(r => r.ModifiedBySystemUser)
                            .Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();
                    }

                    if ((User.IsInRole("Lease Official")) || (User.IsInRole("Client Services Officer")))
                    {
                        var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement).FirstOrDefault().Id;
                        var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                        int AwaitingLeaseAgreement = db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingLeaseAgreement).Id;

                        rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.Id == UserId && x.StatusId == SubmittedId).ToList();
                        var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                        rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id) && x.StatusId == AwaitingLeaseAgreement)
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.Customer).Include(r => r.PurchaserType)
                            .Include(r => r.ModifiedBySystemUser)
                            .Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();
                    }

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    if (rCSApplicationStatus != null)
                    {
                        foreach (var item in rCSApplicationStatus)
                        {
                            item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                        }
                    }

                    if (Session["ManagerLeaseAgreementValidationSession"] != null)
                    {
                        var value = Session["ManagerLeaseAgreementValidationSession"].ToString();
                        Session["ManagerLeaseAgreementValidationSession"] = null;
                        ViewBag.ManagerLeaseAgreementValidationSession = value;
                    }
                    Session["ManagerLeaseAgreementValidationSession"] = null;

                  
                    if (Session["LeaseAgreementReview"] != null)
                    {
                        var value = Session["LeaseAgreementReview"].ToString();
                        Session["LeaseAgreementReview"] = null;
                        ViewBag.LeaseAgreementReview = value;
                    }
                    Session["LeaseAgreementReview"] = null;




                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult PropertyLeaseApplicationFee()
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
                    var rCSApplicationStatus = new List<PropertyLeaseApplication>();

                    if (User.IsInRole("Revenue Officer"))
                    {
                        var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.ValidateDepositPayment).FirstOrDefault().Id;

                        rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.StatusId == SubmittedId).ToList();

                        var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();
                        var PendingAssessmentFeePaymentValidation = db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingApplicationFeeValidation).Id;

                        rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && x.StatusId == PendingAssessmentFeePaymentValidation)
                           .Include(r => r.CreatedBySystemUser)
                           .Include(r => r.Customer).Include(r => r.PurchaserType)
                           .Include(r => r.ModifiedBySystemUser)
                           .Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();
                    }

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    if (Session["BODepositVerifySession"] != null)
                    {
                        var value = Session["BODepositVerifySession"].ToString();
                        Session["BODepositVerifySession"] = null;
                        ViewBag.BODepositVerificationSession = value;
                    }
                    Session["BODepositVerifySession"] = null;

                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult PropertyLeaseDeposit()
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
                    var rCSApplicationStatus = new List<PropertyLeaseApplication>();

                    if (User.IsInRole("Revenue Officer"))
                    {
                        var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.ValidateDepositPayment).FirstOrDefault().Id;

                        rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.StatusId == SubmittedId).ToList();

                        var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();
                        var PendingAssessmentFeePaymentValidation = db.Status.FirstOrDefault(i => i.Key == StatusKeys.PendingAssessmentFeePaymentValidation).Id;

                        rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id) && x.StatusId == PendingAssessmentFeePaymentValidation)
                           .Include(r => r.CreatedBySystemUser)
                           .Include(r => r.Customer).Include(r => r.PurchaserType)
                           .Include(r => r.ModifiedBySystemUser)
                           .Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();
                    }

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                
                    foreach (var item in rCSApplicationStatus)
                    {
                       item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    if (Session["BODepositVerifySession"] != null)
                    {
                        var value = Session["BODepositVerifySession"].ToString();
                        Session["BODepositVerifySession"] = null;
                        ViewBag.BODepositVerificationSession = value;
                    }
                    Session["BODepositVerifySession"] = null;

                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult PropertyLeaseDebitOrder()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var Keys = cxt.Status;
                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                    var rCSApplicationStatus = new List<PropertyLeaseApplication>();

                    if (User.IsInRole("Revenue Officer"))
                    {
                        var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.DebitOrderVAlidation).FirstOrDefault().Id;

                        rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();

                        var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();
                        var IncentivePolicyApplicationProcessing = db.Status.FirstOrDefault(i => i.Key == StatusKeys.IncentivePolicyApplicationProcessing).Id;

                        rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id) && x.StatusId == IncentivePolicyApplicationProcessing)
                           .Include(r => r.CreatedBySystemUser)
                           .Include(r => r.Customer).Include(r => r.PurchaserType)
                           .Include(r => r.ModifiedBySystemUser)
                           .Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();
                    }

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                
                    foreach (var item in rCSApplicationStatus)
                    {
                       item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    if (Session["DebitOrderValidationSession"] != null)
                    {
                        var value = Session["DebitOrderValidationSession"].ToString();
                        Session["DebitOrderValidationSession"] = null;
                        ViewBag.DebitOrderValidationSession = value;
                    }
                    Session["DebitOrderValidationSession"] = null;

                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult PropertyLeaseInspections()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var Keys = cxt.Status.ToList();
                    var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.CommunityDevelopmentOfficer).FirstOrDefault().Value);
                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    var UserId = Customer.Id;
                    List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();

                    var ResponsibilityTypeId = cxt.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Inspections).FirstOrDefault().Id;
                    var ResponsibilityTypeId2 = cxt.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.ScheduleInspectionSlots).FirstOrDefault().Id;
                    var ResponsibilityTypeId3 = cxt.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.MaintananceJobSheet).FirstOrDefault().Id;
                    var ResponsibilityTypeId4 = cxt.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.UnitMaintenanance).FirstOrDefault().Id;

                    rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ClerkId == UserId && x.StatusId == SubmittedId && (x.ResponsibilityTypeId == ResponsibilityTypeId || x.ResponsibilityTypeId == ResponsibilityTypeId2 || x.ResponsibilityTypeId == ResponsibilityTypeId3 || x.ResponsibilityTypeId == ResponsibilityTypeId4)).ToList();

                    var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                    int AwaitingExitInspection = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingExitInspection).Id;
                    int CustomerQueryPending = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.CustomerQueryPending).Id;
                    int UnitInhabitable = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.UnitInhabitable).Id;
                    int RatesRebateAdditionalPropertyOwnersPending = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.RatesRebateAdditionalPropertyOwnersPending).Id;
                    int AwaitingInspectionScheduleSlots = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id;
                    int EvictionGranted = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionGranted).Id;



                    //var UM = db.allocatedUnitMaintenanceEHCs.Include(x=>x.Status).Where(x=>x.Status.Key == StatusKeys.Submitted).ToList();
                    //var UMList = UM.Select(x => x.PropertyLeaseApplicationId);
                    //.Select(x => x.PropertyLeaseApplicationId). ||UMList.Contains(x.Id)
                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id) && ((x.StatusId == AwaitingExitInspection || x.StatusId == CustomerQueryPending || x.StatusId == UnitInhabitable || x.StatusId == RatesRebateAdditionalPropertyOwnersPending || x.StatusId == AwaitingInspectionScheduleSlots || x.StatusId == EvictionGranted) ) )
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.Customer)
                    .Include(r => r.PurchaserType)
                    .Include(r => r.ModifiedBySystemUser)
                    .Include(r => r.HumanEHCOptions)
                    .Include(r => r.Status).ToList();

                    //var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id))
                    //    .Include(r => r.CreatedBySystemUser)
                    //    .Include(r => r.Customer)
                    //    .Include(r => r.PurchaserType)
                    //    .Include(r => r.ModifiedBySystemUser)
                    //    .Include(r => r.HumanEHCOptions)
                    //    .Include(r => r.Status).ToList();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                
                    foreach (var item in rCSApplicationStatus)
                    {
                       item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    
                    if (Session["UnitInspectionScheduledSession"] != null)
                    {
                        var value = Session["UnitInspectionScheduledSession"].ToString();
                        Session["UnitInspectionScheduledSession"] = null;
                        ViewBag.UnitInspectionScheduledSession = value;
                    }
                    Session["UnitInspectionScheduledSession"] = null;

                    if (Session["ConductUnitInspectionSession"] != null)
                    {
                        var value = Session["ConductUnitInspectionSession"].ToString();
                        Session["ConductUnitInspectionSession"] = null;
                        ViewBag.UnitInspectionScheduledSession = value;
                    }
                    Session["ConductUnitInspectionSession"] = null;

                    if (Session["MaintenanceJobSheetSession"] != null)
                    {
                        var value = Session["MaintenanceJobSheetSession"].ToString();
                        Session["MaintenanceJobSheetSession"] = null;
                        ViewBag.MaintenanceJobSheetSession = value;
                    }
                    Session["MaintenanceJobSheetSession"] = null;

                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }


        public ActionResult PropertyLeaseTerminated()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false
                    && x.StatusId == db.Status.FirstOrDefault(i => i.Key == StatusKeys.TerminationDateIssued).Id)
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.Status).ToList();

                    //MatchingHelper.WaitingListNotificationAtOneYear(cxt);
                    //MatchingHelper.RenewalNotificationAtEndOfTime(cxt);

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


        [HttpGet]
        public ActionResult ReturnBackUrl(string q)
        {
            //Create an object of the Encryption Class
            AesCrypto aes = new AesCrypto(encp);
            //use enryption class obj to call the Decryption Method and pass the Encryption String Value (q)
            var decrypted = aes.Decrypt(q);
            // once Decrypted you will need to Split the Single string back into the individual Variables/parameters
            var values = decrypted.Split('|');
            var statuses = db.Status;
            var Appref = "";
            try
            {
                if (values[13] != null && values[15] != null)
                {
                    int RCSAppID = Convert.ToInt16(values[15]);
                    RCSApplicationStatus RCSApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).Where(x => x.Id == RCSAppID).FirstOrDefault();
                    Appref = RCSApplication.ApplicationReferenceNumber;
                    if (RCSApplication != null)
                    {
                        if (values[13] == "Success" || values[13] == "Test")
                        {
                            if (RCSApplication.StatusId == statuses.FirstOrDefault(o => o.Key == StatusKeys.AwaitingAssessmentPayment).Id)
                            {

                                CaptureController c = new CaptureController();
                                var SystUserId = c.RoundRobinCCC(true, false, false, false, false, false, false, false, RCSApplication.Id, 0, false, false, 0);

                                if (SystUserId != 0)
                                {
                                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                    RCSApplication.StatusId = db.Status.FirstOrDefault(o => o.Key == StatusKeys.PendingApplicationFeePaymentValidation).Id;

                                    RCSApplication.ApplicationFeeAmount = values[7];
                                    RCSApplication.ApplicationFeePaymentOnline = true;
                                    RCSApplication.ApplicationFeePayMethod = values[8];

                                    RCSApplication.ApplicationFeeReceiptNumber = values[4];
                                    RCSApplication.ApplicationFeePaymentDate = values[5];
                                    RCSApplication.ApplicationFeeStatus = values[14];


                                    db.Entry(RCSApplication).State = EntityState.Modified;
                                    db.SaveChanges();

                                    var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationSentForAcknowledgementAppFee).Description.ToString()/* + DecisionType*/;
                                    var RCSHistoryLog2 = new RCSApplicationHistoryLog
                                    {
                                        RCSApplicationStatusId = RCSApplication.Id,
                                        AuditAction = ActivityTrackerMessage2,
                                        UserId = ClerkId.Id,
                                        CreatedDateTime = DateTime.Now,
                                        IsActive = true
                                    };
                                    db.RCSApplicationHistoryLogs.Add(RCSHistoryLog2);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var ResponsibilityType = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault();

                                    var UserMessage = db.RCSUserMessages.FirstOrDefault(x => x.Key == RCSUserMessageKeys.DepartmentNotSetup);
                                    UserMessage.Body = UserMessage.Body.Replace("{0}", ResponsibilityType.Name);
                                    UserMessage.Body = UserMessage.Body.Replace("{1}", RCSApplication.CCC.CCCName);
                                    var obj2 = new
                                    {
                                        status = "Department Failure",
                                        title = UserMessage.Title,
                                        body = UserMessage.Body
                                    };


                                    RCSApplication.StatusId = statuses.FirstOrDefault(o => o.Key == StatusKeys.ReuploadApplicationDocs).Id;

                                    db.Entry(RCSApplication).State = EntityState.Modified;
                                    db.SaveChanges();
                                    return RedirectToAction("Index", "RCSApplication");
                                }


                                RCSApplication.StatusId = statuses.FirstOrDefault(o => o.Key == StatusKeys.PendingApplicationFeePaymentValidation).Id;

                                db.Entry(RCSApplication).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            Session["ApplicationRefNo"] = Appref;

                            Session["ReceiptValues"] = values;

                            return RedirectToAction("Index", "RCSApplication");
                        }
                        //   if(RCSApplication.StatusId == statuses.FirstOrDefault(o => o.Key == StatusKeys.PendingDocumentsApproval).Id)
                    }
                }
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View();
        }

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


            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status).Include(r => r.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();


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
                var vm = new DepartmentsApprovalViewModel();

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
         
              
                //var dd = paymentDetails.PaymentDetailsList.OrderByDescending(x => x.ConvertedDate);
                vm.CustomerDocuments = documents;
                vm.Notes = notes;
                vm.Document = dvm;
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

                CultureInfo za = new CultureInfo("en-ZA");
                ViewBag.DepositAmount = context.ApplicantUnits.FirstOrDefault(r => r.PropertyLeaseApplicationId == id).OutstandingDepopsitAmount.ToString("C", za);

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
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Status).Include(x => x.Customer).Include(x=>x.PurchaserType).FirstOrDefault();
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.ValidateDepositPayment).FirstOrDefault();
            var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);


            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                CaptureController c = new CaptureController();

                if (rcsApps.PurchaserType.Key == PurchaserTypeKeys.Company)
                {
                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.RatesRebatePropertyAccountConflict).Id, (int)id);
                }
                else if (rcsApps.PurchaserType.Key == PurchaserTypeKeys.NaturalPerson)
                {
                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AssessmentFeePaymentApproved).Id, (int)id);
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, activeDirectoryOn);
                    EHCRoundRobin(rcsApps.Id, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);


                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApproveDeposit).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(db, rcsApps.Id, ActivityTrackerMessage, custmusers.Id);
                    int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.DepositPaymentApprove).Id;
                    EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                }
                Session["BODepositVerifySession"] = string.Format($"Deposit payment accepted application successfully for application ref ,{rcsApps.ApplicationReferenceNumber}");
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepositPaid).Id, (int)id);
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, activeDirectoryOn);


                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RejectDeposit).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, rcsApps.Id, ActivityTrackerMessage, custmusers.Id);
                int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.DepositPaymentReject).Id;
                EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                Session["BODepositVerifySession"] = string.Format($"Deposit payment rejected application successfully for application ref ,{rcsApps.ApplicationReferenceNumber}");
            }

            return RedirectToAction("PropertyLeaseDeposit");
        }


        [DecryptParameter]
        public ActionResult
ApplicationFeeValidation(int? id)
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


            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status).Include(r => r.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();


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

                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ApplicationFeePOP);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

                var ApplicationStage = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
                var vm = new DepartmentsApprovalViewModel();

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


                //var dd = paymentDetails.PaymentDetailsList.OrderByDescending(x => x.ConvertedDate);
                vm.CustomerDocuments = documents;
                vm.Notes = notes;
                vm.Document = dvm;
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

                CultureInfo za = new CultureInfo("en-ZA");
    
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
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Status).Include(x => x.Customer).Include(x => x.PurchaserType).FirstOrDefault();
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.ValidateDepositPayment).FirstOrDefault();
            var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);


            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                CaptureController c = new CaptureController();

                if (rcsApps.PurchaserType.Key == PurchaserTypeKeys.Company)
                {
                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.RatesRebatePropertyAccountConflict).Id, (int)id);
                }
                else if (rcsApps.PurchaserType.Key == PurchaserTypeKeys.NaturalPerson)
                {
                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRiskAssessment).Id, (int)id);
                    //MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, activeDirectoryOn);
                    //EHCRoundRobin(rcsApps.Id, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                    
                    EHCRoundRobin(rcsApps.Id, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);


                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationFeeApproved).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(db, rcsApps.Id, ActivityTrackerMessage, custmusers.Id);
                    int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationFeeApproved).Id;
                    EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                }
                Session["BODepositVerifySession"] = string.Format($"Application Fee payment accepted application successfully for application ref ,{rcsApps.ApplicationReferenceNumber}");
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingApplicationFeeUpload).Id, (int)id);
                //MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, activeDirectoryOn);


                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationFeeRejected).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, rcsApps.Id, ActivityTrackerMessage, custmusers.Id);
                int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationFeeRejection).Id;
                EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                Session["BODepositVerifySession"] = string.Format($"Application fee payment rejected application successfully for application ref ,{rcsApps.ApplicationReferenceNumber}");
            }

            return RedirectToAction("PropertyLeaseDeposit");
        }


        [DecryptParameter]
        public ActionResult LeaseAgreementValidation(int? id)
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
            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.PurchaserType).Include(r => r.Status)
              .Where(x => x.Id == id).FirstOrDefault();

            if ((rcsApps.Status.Key == StatusKeys.AwaitingManagersSignature) || (rcsApps.Status.Key == StatusKeys.AwaitingTenantUpdateDetails)) return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = rcsApps.Id, appId = "" });

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
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description


                };

                vm.Customer = customer;

                vm.PropertyLeaseApplications = rcsApps;
                ViewBag.DebitApproval = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.PropertyLeaseAppliactionId = rcsApps.Id;
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
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult LeaseAgreementValidation(int? id, string ApprovalStatusddl, string Comment)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).Include(x=>x.PurchaserType).FirstOrDefault();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                CaptureController c = new CaptureController();

                if(rcsApps.PurchaserType.Key == PurchaserTypeKeys.Company)
                {
                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepositPaid).Id, (int)id);

                }
                else if(rcsApps.PurchaserType.Key == PurchaserTypeKeys.NaturalPerson)
                {

                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingManagersSignature).Id, (int)id);

                    EHCRoundRobin(rcsApps.Id, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                    //Send e-mail and SMS notification
                    //int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                    //EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                }
                //not sure what message should be recorded here
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.LeaseAgreementApproved).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
                Session["LeaseAgreementSignedSession"] = string.Format($"Lease agreement approved by candidate for application reference ,{rcsApps.ApplicationReferenceNumber}");
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantUpdateDetails).Id, (int)id);
                EHCRoundRobin(rcsApps.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);


                MatchingHelper.AddCommentOnRejectAgreement(db, Comment, (int)id);

                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresPOPRejected).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
                Session["LeaseAgreementSignedSession"] = string.Format($"Lease agreement rejected by candidate for application reference ,{rcsApps.ApplicationReferenceNumber}");
            }
            return RedirectToAction("Inbox");
        }

        public ActionResult Redirect()
        {
            return Json(true, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ValidateDocumentUpload()
        {
            var uploaded = Convert.ToString(Session["DocumentUploadedCapture"]);
            Session["DocumentUploadedCapture"] = null;
            ViewBag.DocumentUploadedCapture = uploaded;
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLIMSProperties(string IDNo )
        {

            var res = Lims10.LIMSApi(IDNo);
            //var uploaded = Convert.ToString(Session["DocumentUploadedCapture"]);
            //Session["DocumentUploadedCapture"] = null;
            //ViewBag.DocumentUploadedCapture = uploaded;
            return Json(res, JsonRequestBehavior.AllowGet);
        }



        [DecryptParameter]
        public ActionResult CaptureCommitteeOutcome(int? id)
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
            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.PurchaserType).Include(r => r.Status)
              .Where(x => x.Id == id).FirstOrDefault();

            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.NonPayment || x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");
            }
            else
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.CommitteeDate = null;
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

                // Documents required variables
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();

                //Required ViewModels For displaying
                var dvm = new DocumentsViewModel();
             
                if (application == null) throw new Exception("Invalid application.");
                if (referenceType == null) throw new Exception("Invalid reference type.");

                MatchingHelper.DocumentEvictionOutcomeSupportingDoc(dvm, context, customer.Id, customer.Id, referenceType.Id, application.Id, "", rcsApps.Id, true);

                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);
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
                vm.DocumentsViewModel = dvm.DocumentCheckLists == null ? null : dvm;
                vm.Customer = customer;

                vm.PropertyLeaseApplications = rcsApps;
                ViewBag.DebitApproval = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.PropertyLeaseAppliactionId = rcsApps.Id;
                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;
                ViewBag.DocumentUploaded = false;
                ViewBag.DocumentUploadedCapture = "";

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
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult CaptureCommitteeOutcome(int? id, string ApprovalStatusddl, string CommitteeDate)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).Include(x => x.PurchaserType).FirstOrDefault();
            var this_lease = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);

            var User2 = GetBackOfficeId(db, (int)id, true);
            var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
            var UserId = User2.Id != 0 ? User2.Id : activeDirectoryOn;
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CommitteeOutcomes).FirstOrDefault();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                CaptureController c = new CaptureController();
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionGranted).Id, (int)id);
                MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionGranted).Id, (int)this_lease.Id);
                MatchingHelper.Commiteedate(db, rcsApps.Id, CommitteeDate);
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)id, null, ResponsibilityTypeId.Id, UserId);

                int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.EvictionGranted).Id;
                EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);

                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.LeaseAgreementApproved).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);

                //                      1      2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17     18   19
                EHCRoundRobin((int)id, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                Session["MessageBody"] = "Eviction Granted, Outcome Captured Successfully";
                Session["OutcomeCapturedSuccessfully"] = "true";
                Session["Display"] = "true";
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionNotGranted).Id, (int)id);
                MatchingHelper.ChangeLeaseStatusII(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionNotGranted).Id, (int)this_lease.Id);
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)id, null, ResponsibilityTypeId.Id, UserId);

                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresPOPRejected).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);

                Session["MessageBody"] = "Eviction Not Granted, Outcome Captured Successfully";
                Session["OutcomeCapturedSuccessfully"] = "true";
                Session["Display"] = "true";
            }
            
            return RedirectToAction("CommitteeEvictionsOutcomes", "PropertyLeaseApplication");
        }
         [DecryptParameter]
        public ActionResult CaptureEvictionDetails(int? id)
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
            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.PurchaserType).Include(r => r.Status)
              .Where(x => x.Id == id).FirstOrDefault();

            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.NonPayment || x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");
            }
            else
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.NonPayment || x.Key == RCSActionTypeKeys.NonCompliance || x.Key == RCSActionTypeKeys.Subletting || x.Key == RCSActionTypeKeys.IllegalActivities).OrderBy(x => x.Name), "Key", "Name");
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

                // Documents required variables
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();

                //Required ViewModels For displaying
                var dvm = new DocumentsViewModel();
             
                if (application == null) throw new Exception("Invalid application.");
                if (referenceType == null) throw new Exception("Invalid reference type.");

                MatchingHelper.DocumentPropertyEvictionValidation(dvm, context, customer.Id, customer.Id, referenceType.Id, application.Id, "", rcsApps.Id, true);


                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);


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
                vm.DocumentsViewModel = dvm.DocumentCheckLists == null ? null : dvm;
                vm.Customer = customer;

                vm.PropertyLeaseApplications = rcsApps;
                ViewBag.DebitApproval = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.PropertyLeaseAppliactionId = rcsApps.Id;
                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;
                ViewBag.DocumentUploaded = false;
                ViewBag.DocumentUploadedCapture = "";

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
        }

       

        [DecryptParameter]
        [HttpPost]
        public ActionResult CaptureEvictionDetails(int? id, string ApprovalStatusddl)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).Include(x => x.PurchaserType).FirstOrDefault();
            var ActionKey = db.RCSActionTypes.FirstOrDefault(x => x.Key == ApprovalStatusddl);
            var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);



            if (ApprovalStatusddl == RCSActionTypeKeys.NonPayment)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.NonPayment).Id, (int)id);
            }
            if (ApprovalStatusddl == RCSActionTypeKeys.NonCompliance)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.NonCompliance).Id, (int)id);
            }
            if (ApprovalStatusddl == RCSActionTypeKeys.Subletting)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.Subletting).Id, (int)id);
            }
            if (ApprovalStatusddl == RCSActionTypeKeys.IllegalActivities)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.IllegalActivities).Id, (int)id);
            }
            //                      1      2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17     18   19
            EHCRoundRobin((int)id, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);


            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EvictionCapture).Description.Replace("{#}", ActionKey.Name).ToString();
            MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
            int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.EvictionCapture).Id;
            EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);

            Session["EvictionDetailsSession"] = string.Format($"Eviction details taken successfully for application reference ,{rcsApps.ApplicationReferenceNumber}");
            return RedirectToAction("ApplicationEviction", "PropertyLeaseApplication");
        }





        [DecryptParameter]
        public ActionResult PropertyManagerLeaseAgreementValidation(int? id)
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
            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.PurchaserType).Include(r => r.Status)
              .Where(x => x.Id == id).FirstOrDefault();

            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");
            }
            else
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
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

                // Documents required variables
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();

                //Required ViewModels For displaying
                var dvm = new DocumentsViewModel();
             
                if (application == null) throw new Exception("Invalid application.");
                if (referenceType == null) throw new Exception("Invalid reference type.");

                var findItem = context.propertyLeaseAgreementMasters.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);
                if(findItem.RevenueManagerSigned==true) MatchingHelper.DocumentUploadFinalLeaseAgreement(dvm, context, customer.Id, customer.Id, referenceType.Id, application.Id, "", rcsApps.Id, true);


                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);


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
                vm.DocumentsViewModel = dvm.DocumentCheckLists == null ? null : dvm;
                vm.Customer = customer;

                vm.PropertyLeaseApplications = rcsApps;
                ViewBag.DebitApproval = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.PropertyLeaseAppliactionId = rcsApps.Id;
                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;

                var master = db.propertyLeaseAgreementMasters.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);
                ViewBag.PropertyManagerSigned = master?.PropertyManagerSigned ?? false;
                ViewBag.RevenueManagerSigned = master?.RevenueManagerSigned ?? false;
                ViewBag.BothManagersSigned = (master?.PropertyManagerSigned ?? false) && (master?.RevenueManagerSigned ?? false);

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
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult PropertyManagerLeaseAgreementValidation(int? id, string ApprovalStatusddl)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).Include(x => x.PurchaserType).FirstOrDefault();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                CaptureController c = new CaptureController();

                if (rcsApps.PurchaserType.Key == PurchaserTypeKeys.Company)
                {
                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepositPaid).Id, (int)id);
                }
                else if (rcsApps.PurchaserType.Key == PurchaserTypeKeys.NaturalPerson)
                {
                    //Send e-mail and SMS notification
                    int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.plm_awaiting_debit_order).Id;
                    EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                }
                //not sure what message should be recorded here
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.LeaseAgreementApproved).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
                Session["ManagerLeaseAgreementValidationSession"] = string.Format($"Signed lease agreement approved for application reference ,{rcsApps.ApplicationReferenceNumber}");
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantUpdateDetails).Id, (int)id);
                EHCRoundRobin(rcsApps.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);


                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresPOPRejected).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
                Session["ManagerLeaseAgreementValidationSession"] = string.Format($"Signed lease agreement rejected for application reference ,{rcsApps.ApplicationReferenceNumber}");
            }
            return RedirectToAction("PropertyLeaseAgreements");
        }





        [DecryptParameter]
        public ActionResult RevenueManagerLeaseAgreementValidation(int? id)
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
            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.PurchaserType).Include(r => r.Status)
              .Where(x => x.Id == id).FirstOrDefault();

            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReAllocate).OrderBy(x => x.Name), "Key", "Name");
            }
            else
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
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

                // Documents required variables
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();

                //Required ViewModels For displaying
                var dvm = new DocumentsViewModel();

                if (application == null) throw new Exception("Invalid application.");
                if (referenceType == null) throw new Exception("Invalid reference type.");

                var findItem = context.propertyLeaseAgreementMasters.OrderByDescending(x=>x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);
                if (findItem.PropertyManagerSigned == true) MatchingHelper.DocumentUploadFinalLeaseAgreement(dvm, context, customer.Id, customer.Id, referenceType.Id, application.Id, "", rcsApps.Id, true);
                if (findItem.RevenueManagerSigned == true) MatchingHelper.DocumentUploadFinalLeaseAgreement(dvm, context, customer.Id, customer.Id, referenceType.Id, application.Id, "", rcsApps.Id, true);


                var ProofOfPayment = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);


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
                vm.DocumentsViewModel = dvm.DocumentCheckLists == null ? null : dvm;
                vm.Customer = customer;

                vm.PropertyLeaseApplications = rcsApps;
                ViewBag.DebitApproval = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.PropertyLeaseAppliactionId = rcsApps.Id;
                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;

                var master = db.propertyLeaseAgreementMasters.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);
                ViewBag.PropertyManagerSigned = master?.PropertyManagerSigned ?? false;
                ViewBag.RevenueManagerSigned = master?.RevenueManagerSigned ?? false;
                ViewBag.BothManagersSigned = (master?.PropertyManagerSigned ?? false) && (master?.RevenueManagerSigned ?? false);

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
        }

        [DecryptParameter]
        [HttpPost]
        public ActionResult RevenueManagerLeaseAgreementValidation(int? id, string ApprovalStatusddl)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).Include(x => x.PurchaserType).FirstOrDefault();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                CaptureController c = new CaptureController();

                if (rcsApps.PurchaserType.Key == PurchaserTypeKeys.Company)
                {
                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepositPaid).Id, (int)id);


                }
                else if (rcsApps.PurchaserType.Key == PurchaserTypeKeys.NaturalPerson)
                {

                    //MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.IncentivePolicyApplicationPending).Id, (int)id);

                    //Send e-mail and SMS notification
                    int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.plm_awaiting_debit_order).Id;
                    EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                }
                //not sure what message should be recorded here
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.LeaseAgreementApproved).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
                Session["ManagerLeaseAgreementValidationSession"] = string.Format($"Signed lease agreement approved for application reference ,{rcsApps.ApplicationReferenceNumber}");
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantUpdateDetails).Id, (int)id);
                EHCRoundRobin(rcsApps.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);



                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresPOPRejected).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
                Session["ManagerLeaseAgreementValidationSession"] = string.Format($"Signed lease agreement rejected for application reference ,{rcsApps.ApplicationReferenceNumber}");
            }
            return RedirectToAction("PropertyLeaseAgreements");
        }


        [DecryptParameter]
        public ActionResult DebitOrderValidation(int? id)
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


            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.PurchaserType).Include(r => r.Status)
              .Where(x => x.Id == id).FirstOrDefault();

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
                ViewBag.DebitApproval = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");

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

                var dvm = new DocumentsViewModel();
                MatchingHelper.DocumentDebitOrder(dvm, context, customer.Id, customer.Id, documentReferenceType.Id, application.Id, "", rcsApps.Id, true);

                var debit = db.DebitOrderRegistrations.Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).FirstOrDefault();
                ViewBag.RejectComment = "";
                var vm = new DepartmentsApprovalViewModel
                {
                    DebitOrderRegistration = debit,
                    PropertyLeaseApplications = rcsApps,
                    DocumentsViewModel = dvm
                };

                Entity entity = null;
                Agent agent = null;



                vm.Customer = customer;


                //var dd = paymentDetails.PaymentDetailsList.OrderByDescending(x => x.ConvertedDate);

                vm.PropertyLeaseApplications = rcsApps;
                ViewBag.Encryption = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("ApplicationId=" + application.Id.ToString());
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
                ViewBag.PropertyLeaseAppliactionId = rcsApps.Id;
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
        public ActionResult DebitOrderValidation(int? id, string ApprovalStatusddl, string RejectComment)
        {
            Initialise();
            var userID = Customer.Id;
            var Keys = db.Status;
            var rcsApps = db.PropertyLeaseApplications.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();
            var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.DebitOrderVAlidation).FirstOrDefault();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                CaptureController c = new CaptureController();
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicationCompleted).Id, (int)id);

                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, activeDirectoryOn);

                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantDebitOrderApproved).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);

                //Send e-mail and SMS notification
                int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.BackOfficeApproveDebitOrder).Id;
                EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);

                var debit = db.DebitOrderRegistrations.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);
                debit.Rejected = false;
                db.Entry(debit).State = EntityState.Modified;
                db.SaveChanges();
                Session["DebitOrderValidationSession"] = string.Format($"Debit Order approved for application reference ,{rcsApps.ApplicationReferenceNumber}");
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                var debit = db.DebitOrderRegistrations.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);
                debit.Rejected = true;
                debit.RejectionComment = RejectComment;
                db.Entry(debit).State = EntityState.Modified;
                db.SaveChanges();

                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantDebitOrderRejected).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.IncentivePolicyApplicationPending).Id, (int)id);

                MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, userID);

                //Send e-mail and SMS notification
                int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.DebitOrderRejectByBO).Id;
                EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                Session["DebitOrderValidationSession"] = string.Format($"Debit Order rejected for application reference ,{rcsApps.ApplicationReferenceNumber}");
            }
            return RedirectToAction("PropertyLeaseDebitOrder");
        }

        public JsonResult ApplicantSignLeaseAgreement(int? Id)
        {
            if (Id == null) throw new Exception("Invalid Application");
            Initialise();
            var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == Id);
            var lease = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IsNew && x.IsActive && !x.IsDeleted);
            if (lease == null) throw new Exception("Invalid Application Lease");
            var IIndApplicant = db.ApplicantJoints.FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IDNo != null);
            MatchingHelper.ApplicantSignLeaseAgreement(db, application, lease, IIndApplicant);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveTenantSignatureImage()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.InputStream))
                    body = reader.ReadToEnd();

                var payload = JsonConvert.DeserializeObject<dynamic>(body);
                int? ApplicationId = (int?)payload.ApplicationId;
                string SignatureImageData = (string)payload.SignatureImageData;

                if (ApplicationId == null || string.IsNullOrEmpty(SignatureImageData))
                    return Json(new { success = false, message = "Invalid parameters." });

                Initialise();
                var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == ApplicationId);
                if (application == null)
                    return Json(new { success = false, message = "Application not found." });

                var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x =>
                    x.PropertyLeaseApplicationId == application.Id && x.IsActive && !x.IsDeleted);
                if (master == null)
                    return Json(new { success = false, message = "Lease agreement record not found." });

                master.TenantSignature = SignatureImageData;
                master.TenantSigned = true;
                master.TenantSignDate = DateTime.Now.ToString("dd MMMM yyyy");

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SaveWitness1SignatureImage()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.InputStream))
                    body = reader.ReadToEnd();

                var payload = JsonConvert.DeserializeObject<dynamic>(body);
                int? ApplicationId = (int?)payload.ApplicationId;
                string SignatureImageData = (string)payload.SignatureImageData;
                string WitnessName = (string)payload.WitnessName;

                if (ApplicationId == null || string.IsNullOrEmpty(SignatureImageData) || string.IsNullOrEmpty(WitnessName))
                    return Json(new { success = false, message = "Invalid parameters." });

                Initialise();
                var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == ApplicationId);
                if (application == null)
                    return Json(new { success = false, message = "Application not found." });

                var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x =>
                    x.PropertyLeaseApplicationId == application.Id && x.IsActive && !x.IsDeleted);
                if (master == null)
                    return Json(new { success = false, message = "Lease agreement record not found." });

                master.Witness1Signature = SignatureImageData;
                master.Witness1Name = WitnessName;
                master.Witness1SignatureDate = DateTime.Now;

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult CheckSignatureStatus(int applicationId)
        {
            try
            {
                Initialise();
                var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == applicationId);
                if (application == null)
                    return Json(new { success = false, message = "Application not found." }, JsonRequestBehavior.AllowGet);

                var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x =>
                    x.PropertyLeaseApplicationId == application.Id && x.IsActive && !x.IsDeleted);
                if (master == null)
                    return Json(new { success = false, message = "Lease agreement record not found." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    tenantSigned = master.TenantSigned,
                    tenantSignatureData = master.TenantSignature,
                    witness1Signed = !string.IsNullOrEmpty(master.Witness1Signature),
                    witness1SignatureData = master.Witness1Signature,
                    witness1Name = master.Witness1Name
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SavePropertyManagerSignatureImage()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.InputStream))
                    body = reader.ReadToEnd();

                var payload = JsonConvert.DeserializeObject<dynamic>(body);
                int? ApplicationId = (int?)payload.ApplicationId;
                string SignatureImageData = (string)payload.SignatureImageData;

                if (ApplicationId == null || string.IsNullOrEmpty(SignatureImageData))
                    return Json(new { success = false, message = "Invalid parameters." });

                Initialise();
                var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == ApplicationId);
                if (application == null)
                    return Json(new { success = false, message = "Application not found." });

                var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x =>
                    x.PropertyLeaseApplicationId == application.Id && x.IsActive && !x.IsDeleted);
                if (master == null)
                    return Json(new { success = false, message = "Lease agreement record not found." });

                master.PropertyManagersSignature = SignatureImageData;
                master.PropertyManagerSigned = true;
                master.PropertyManagerSignatureDate = DateTime.Now;

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SaveRevenueManagerSignatureImage()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.InputStream))
                    body = reader.ReadToEnd();

                var payload = JsonConvert.DeserializeObject<dynamic>(body);
                int? ApplicationId = (int?)payload.ApplicationId;
                string SignatureImageData = (string)payload.SignatureImageData;

                if (ApplicationId == null || string.IsNullOrEmpty(SignatureImageData))
                    return Json(new { success = false, message = "Invalid parameters." });

                Initialise();
                var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == ApplicationId);
                if (application == null)
                    return Json(new { success = false, message = "Application not found." });

                var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x =>
                    x.PropertyLeaseApplicationId == application.Id && x.IsActive && !x.IsDeleted);
                if (master == null)
                    return Json(new { success = false, message = "Lease agreement record not found." });

                master.RevenueManagersSignature = SignatureImageData;
                master.RevenueManagerSigned = true;
                master.RevenueManagerSignatureDate = DateTime.Now;

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public JsonResult PropertyManagerSignLeaseAgreement(int? Id)
        {
            if (Id == null) throw new Exception("Invalid Application");
            Initialise();
            var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == Id);
            var lease = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IsNew && x.IsActive && !x.IsDeleted);
            var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);
            var User = db.Customers.Include(r => r.SystemUser).FirstOrDefault(x => x.Id == activeDirectoryOn);
            if (lease == null) throw new Exception("Invalid Application Lease");
            MatchingHelper.PropertyManagerSignLeaseAgreement(db, application, lease, User);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RevenueManagergnLeaseAgreement(int? Id)
        {
            if (Id == null) throw new Exception("Invalid Application");
            Initialise();
            var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == Id);
            var lease = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IsNew && x.IsActive && !x.IsDeleted);
            var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueManager).FirstOrDefault().Value);
            var User = db.Customers.Include(r=>r.SystemUser).FirstOrDefault(x => x.Id == activeDirectoryOn);
            if (lease == null) throw new Exception("Invalid Application Lease");
            MatchingHelper.RevenueManagerSignLeaseAgreement(db, application, lease, User);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RevenueSignDebitOrder(int? id)
        {
            if (id == null) throw new Exception("Invalid Application");
            Initialise();
            var application = db.PropertyLeaseApplications.FirstOrDefault(x=>x.Id==id);
            var dbto = db.DebitOrderRegistrations.OrderByDescending(r=>r.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id);
            MatchingHelper.RevenueSignDebitOrderAuthorityForm(db, SystemUser, dbto);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LeaseOfficialSignDebitOrder(int? id)
        {
            if (id == null) throw new Exception("Invalid Application");
            Initialise();
            var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == id);
            var dbto = db.DebitOrderRegistrations.OrderByDescending(r => r.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id);

            var User = GetBackOfficeId(db, application.Id, true);
            var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
            
            var activeDirectoryOn = User.Id != 0 ? User : db.Customers.FirstOrDefault(x => x.Id == StoredUser);
            MatchingHelper.LeaseOfficialSignDebitOrderAuthorityForm(db, User, dbto);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public int EHCRoundRobinArchived(int ClerkId, int? PropertyId, int? LeaseId, int ResponsibilityId)
        {
            int result = 0;
            if (PropertyId != null || PropertyId != 0)
            {

            }
            else if (LeaseId != null || LeaseId != 0)
            {

            }
            return result;
        }

        public static Customer GetBackOfficeId(eServicesDbContext core, int Id, bool LF)
        {
            Customer UserId = new Customer();
            ApplicantUnit AppUnit = core.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == Id);
            if (AppUnit != null)
            {
                MatchedUnits Match = AppUnit != null ? core.MatchedUnits.FirstOrDefault(x => x.Id == AppUnit.MatchedID) : null;
                if (Match != null)
                {
                    ApplicationAllocatedProperty allocatedUnit = core.ApplicationAllocatedProperty.FirstOrDefault(a => a.Id == Match.ApplicationAllocatedPropertyId);
                    PreferredComplexArea preferredComplexArea = allocatedUnit != null ? core.PreferredComplexAreas.FirstOrDefault(x => x.Id == allocatedUnit.OfferedComplexId) : null;

                    if (preferredComplexArea != null)
                        _ = (LF && preferredComplexArea != null) ?
                            UserId = core.Customers.FirstOrDefault(x => x.Id == preferredComplexArea.LettingOfficerId)
                            : UserId = core.Customers.FirstOrDefault(x => x.Id == preferredComplexArea.LettingOfficerId);
                }
            }
            return UserId;
        }
        public static List< Customer > ToAllocateBackOffice(eServicesDbContext core, PropertyLeaseApplication application)
        {
            Customer backOffice = new Customer();
            PreferredComplexArea complex_1 = core.PreferredComplexAreas.Include(c=>c.LettingOfficer).FirstOrDefault(a => a.Id == application.PreferredComplexAreaId);
            PreferredComplexArea complex_2 = core.PreferredComplexAreas.Include(c => c.LettingOfficer).FirstOrDefault(a => a.Id == application.PreferredComplexArea2Id);
            List<Customer> lettingOfficers = new List<Customer>();
            lettingOfficers.Add(complex_1.LettingOfficer);
            //lettingOfficers.Add(complex_2.LettingOfficer);
            return lettingOfficers;
        }

        //                                              1                       2                           3                       4                       5                           6                           7                               8                           9                              10                   11                   12                       13                        14                         15                          16                              17                        18                       19                         20                   21         22          23          24                    25                     26                         27                                       
        public int EHCRoundRobin(int RCSAppID, bool RiskAssessment, bool ValidateDepositPayment, bool InviteToClientTraining,bool UnitInspections, bool UpdateTenantDetails,bool GenerateLeaseAgreement, bool LeaseAgreementValidation,bool DebitOrderValidation, bool ShechuleInspectionSlots, bool MaintananceJobSheet, bool Terminations, bool TerminationValidation, bool CommitteeOutcomes, bool VacatingConfirmation, bool RecomendForRenewal,bool SecondRenewalRecommendation, bool LeaseRenewalRevenue, bool RenewalRiskAssessment, bool UnitMaintenance, bool AgreemrntValidateRevenue, bool six, bool seven, bool eight, int DepartmentID, bool AcknowlegeRefund, bool IssueRefundsCollection, int RefundAppID, bool? IsAwaitingRefundResponse = null, bool? IsAwaitingDocUploadingForMigratedApps = null)
        {
            Initialise();
            var applicationUserRoles = db.Roles.ToList();
            var dates = new string[2];
            dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            var startDate = DateTime.Parse(dates[0]).Date;
            var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            var oneDayTime = endDate - startDate;
            var RoundRobingQueue = db.RoundRobinQueues.Where(x => x.IsActive == true && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();
            var AccountsManagementUsers = (List<SystemIdentityUser>)null;
            var SubmitFiguresUsers = (List<SystemIdentityUser>)null;
            var IssueRCCUsers = (List<SystemIdentityUser>)null;
            var BillingUsers = (List<SystemIdentityUser>)null;
            var RateUsers = (List<SystemIdentityUser>)null;
            var BOUsers = (List<SystemIdentityUser>)null;
            var CreditControlUsers = (List<SystemIdentityUser>)null;
            var SundriesUsers = (List<SystemIdentityUser>)null;
            var RefundUsers = (List<SystemIdentityUser>)null;

            var AssignedToUser = 0;
            var responsibilityTypes = db.ResponsibilityTypes.ToList();

            if (RiskAssessment)
            {
                PropertyLeaseApplication RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                List<Customer> LettingOfficers = ToAllocateBackOffice(db, RcsApplication).DistinctBy(a => a.SystemUserId).ToList();
                ResponsibilityType ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RiskAssessment).FirstOrDefault();

                if (LettingOfficers.Count >= 1)
                {
                    List<Status> statusList = db.Status.ToList();
                    Int32 StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    RoundRobinQueue roundRobinQueue = new RoundRobinQueue();
                    roundRobinQueue.PropertyLeaseApplicationId = RcsApplication.Id;
                    roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                    roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                    roundRobinQueue.StatusId = StatusId;

                    foreach(Customer boUser in LettingOfficers)
                    {
                        roundRobinQueue.ClerkId = boUser.Id;
                        db.RoundRobinQueues.Add(roundRobinQueue);
                        db.SaveChanges();
                        BackOfficeNotification(RCSAppID, boUser.Id, ResponsibilityTypeId.Name);
                    }
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (ValidateDepositPayment)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);

                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.ValidateDepositPayment).FirstOrDefault();
                if (activeDirectoryOn != null)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (InviteToClientTraining)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);

                var appUnit = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == RCSAppID);
                var matchedUnit = appUnit != null ? db.MatchedUnits.FirstOrDefault(x => x.Id == appUnit.MatchedID) : null;
                var allocatedProperty = matchedUnit != null ? db.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == matchedUnit.ApplicationAllocatedPropertyId) : null;
                var offeredComplex = allocatedProperty != null ? db.PreferredComplexAreas.Include(x => x.LettingOfficer).FirstOrDefault(x => x.Id == allocatedProperty.OfferedComplexId) : null;
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = (offeredComplex != null && offeredComplex.LettingOfficerId.HasValue)
                    ? offeredComplex.LettingOfficerId.Value
                    : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.InviteToClientTraining).FirstOrDefault();
                if (activeDirectoryOn != null)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (ShechuleInspectionSlots)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);

                var UserId = GetBackOfficeId(db, RCSAppID, false);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.ScheduleInspectionSlots).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (UnitInspections)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);

                var UserId =  GetBackOfficeId(db,  RCSAppID, false);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Inspections).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (MaintananceJobSheet)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);

                var UserId = GetBackOfficeId(db,  RCSAppID, false);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.MaintananceJobSheet).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (UpdateTenantDetails)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var UserId = GetBackOfficeId(db,  RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CaptureLeaseDetails).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.
            }
            else if (GenerateLeaseAgreement)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var UserId = GetBackOfficeId(db,  RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }
                // JK.20140724a - Custom profile information.
            }
            else if (LeaseAgreementValidation)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);

                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueManager).FirstOrDefault().Value);
                var activeDirectoryOn2 = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseAgreementValidation).FirstOrDefault();
                if (activeDirectoryOn != 0 && activeDirectoryOn2 != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                    BackOfficeNotification(RCSAppID, activeDirectoryOn2, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,PropertyLeaseDeposit
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (DebitOrderValidation)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);

                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.DebitOrderVAlidation).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (Terminations)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var UserId =  GetBackOfficeId(db,  RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Terminations).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (TerminationValidation)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var UserId =GetBackOfficeId(db,  RCSAppID, false);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.TerminationValidation).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (CommitteeOutcomes)
             {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var UserId = GetBackOfficeId(db,  RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CommitteeOutcomes).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (VacatingConfirmation)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var UserId = GetBackOfficeId(db,  RCSAppID, false);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.VacatingConfirmation).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (RecomendForRenewal)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var UserId = GetBackOfficeId(db,  RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseRenewals).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (SecondRenewalRecommendation)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var UserId = GetBackOfficeId(db,  RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SecondLeaseRenewal).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (LeaseRenewalRevenue)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var findItem = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var UserId = GetBackOfficeId(db, RCSAppID, true);
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (RenewalRiskAssessment)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var leaseInfo = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalRiskAssessment).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue();
                    roundRobinQueue.PropertyLeaseApplicationId = RcsApplication.Id;
                    roundRobinQueue.LeaseDetailsId = leaseInfo.Id;
                    roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                    roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                    roundRobinQueue.ClerkId = activeDirectoryOn;
                    roundRobinQueue.StatusId = StatusId;
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();


                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }
            else if (UnitMaintenance)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var appu = db.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplication.Id);
                var unit = db.Units.FirstOrDefault(x => x.Id == appu.Matched.UnitsId);
                var UserId =GetBackOfficeId(db, RCSAppID, false);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id != 0 ? UserId.Id : StoredUser;

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.UnitMaintenanance).FirstOrDefault();
                if (activeDirectoryOn != 0)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    PropertyLeaseApplicationId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                

                // JK.20140724a - Custom profile information.

            }
            else if (IsAwaitingRefundResponse !=null && IsAwaitingRefundResponse.Value == true)
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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }


                // JK.20140724a - Custom profile information.

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
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
            }

            return AssignedToUser;
        }


        //public ActionResult BulkReAllocate(int? id, int? rrqID, string ResponsibilityType, string ViewName, string TitleName, string BodyName)
        //{

        //    Initialise();
        //    if (id != null)
        //    {


        //        var Keys = db.Status;
        //        int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

        //        var SysUserID = SystemUser.Id;
        //        var Clerk = db.Customers.Include(x => x.SystemUser).Where(x => x.Id == id).FirstOrDefault();



        //        var user = UserManager.FindByName(Clerk.SystemUser.UserName);
        //        var userId = user.Id;

        //        var test = UserManager.IsInRole(userId, "Revenue Manager");
        //        if (UserManager.IsInRole(userId, "Revenue Manager"))
        //        {
        //            var RevenueManager = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueManager).FirstOrDefault().Value);
        //            var RevnueManagerCust = db.Customers.FirstOrDefault(x => x.Id == RevenueManager);
        //            if (Clerk.Id != RevnueManagerCust.Id)
        //            {
        //                var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ClerkId == Clerk.Id).FirstOrDefault();

        //                if (rrqList != null)
        //                {
        //                    rrqList.StatusId = db.Status.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
        //                    rrqList.EndTaskDateTime = DateTime.Now;
        //                    db.Entry(rrqList).State = EntityState.Modified;
        //                    db.SaveChanges();
        //                }

        //                var ClerkId = db.Customers.Where(x => x.SystemUserId == vm.NewBackOfficeUser && x.IsDeleted == false).FirstOrDefault();
        //                var statusList = db.Status.ToList();
        //                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

        //                var roundRobinQueue = new RoundRobinQueue
        //                {
        //                    PropertyLeaseApplicationId = plmApps.Id,
        //                    ResponsibilityTypeId = Convert.ToInt32(vm.ResponsibilityType),
        //                    CurrentTaskDateTime = DateTime.Now,
        //                    ClerkId = ClerkId.Id,
        //                    StatusId = StatusId,
        //                    AssignedFrom = rrqList.Id
        //                };
        //                db.RoundRobinQueues.Add(roundRobinQueue);
        //                db.SaveChanges();

        //                int rtype = Convert.ToInt32(vm.ResponsibilityType);
        //                var ResponsibilityType = db.ResponsibilityTypes.FirstOrDefault(x => x.Id == rtype);
        //                BackOfficeNotification(plmApps.Id, ClerkId.Id, ResponsibilityType.Name);


        //                var Title = vm.TitleName;
        //                var Body = vm.BodyName;
        //            }
        //            else
        //            {

        //            }



        //        }



        //        var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.ResponsibilityType).Include(x => x.Clerk.SystemUser).Include(x => x.RefundApplication).Include(x => x.RCSApplicationStatus).Where(x => x.ClerkId == Clerk.Id && x.StatusId == SubmittedId).ToList();
        //        AesCrypto AES = new AesCrypto();
        //        var q = AES.Encrypt("id=" + id);
        //        foreach (var item in rrqList)
        //        {

        //            DepartmentsApprovalsController DA = new DepartmentsApprovalsController();
        //            CaptureController c = new CaptureController();
        //            switch (item.ResponsibilityType.Key)
        //            {
        //                case (ResponsibilityTypeKeys.AcknowledgeRCSApplication):
        //                    {
        //                        var SystUserId = c.RoundRobinRedistribution(true, false, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);
        //                        //var SystUserId = c.RoundRobinCCC(true, false, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, false, false, 0);
        //                        if (SystUserId == 0)
        //                        {
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

        //                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

        //                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
        //                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


        //                            return RedirectToAction("EditRRUser", new { q = q });
        //                        }
        //                        else
        //                        {
        //                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);


        //                        }
        //                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
        //                        db.Entry(item).State = EntityState.Modified;
        //                        db.SaveChanges();
        //                        break;
        //                    }
        //                case (ResponsibilityTypeKeys.SubmitFigures):
        //                    {
        //                        var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);

        //                        if (SystUserId == 0)
        //                        {
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

        //                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

        //                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
        //                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

        //                            return RedirectToAction("EditRRUser", new { q = q });
        //                        }
        //                        else
        //                        {
        //                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

        //                        }

        //                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
        //                        db.Entry(item).State = EntityState.Modified;
        //                        db.SaveChanges();
        //                        break;
        //                    }
        //                case (ResponsibilityTypeKeys.IssueCertificates):
        //                    {
        //                        var SystUserId = c.RoundRobinRedistribution(false, false, true, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);

        //                        if (SystUserId == 0)
        //                        {
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
        //                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

        //                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
        //                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

        //                            return RedirectToAction("EditRRUser", new { q = q });
        //                        }
        //                        else
        //                        {
        //                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

        //                        }

        //                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
        //                        db.Entry(item).State = EntityState.Modified;
        //                        db.SaveChanges();
        //                        break;
        //                    }
        //                case (ResponsibilityTypeKeys.Billing):
        //                    {
        //                        var SystUserId = c.RoundRobinRedistribution(false, false, false, true, false, false, false, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

        //                        if (SystUserId == 0)
        //                        {
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
        //                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

        //                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
        //                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

        //                            return RedirectToAction("EditRRUser", new { q = q });
        //                        }
        //                        else
        //                        {
        //                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

        //                        }

        //                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
        //                        db.Entry(item).State = EntityState.Modified;
        //                        db.SaveChanges();
        //                        break;
        //                    }
        //                case (ResponsibilityTypeKeys.CreditControl):
        //                    {
        //                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, true, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

        //                        if (SystUserId == 0)
        //                        {
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
        //                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

        //                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
        //                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

        //                            return RedirectToAction("EditRRUser", new { q = q });
        //                        }
        //                        else
        //                        {
        //                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

        //                        }

        //                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
        //                        db.Entry(item).State = EntityState.Modified;
        //                        db.SaveChanges();
        //                        break;
        //                    }
        //                case (ResponsibilityTypeKeys.SundryAccount):
        //                    {
        //                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, true, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

        //                        if (SystUserId == 0)
        //                        {
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
        //                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

        //                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
        //                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

        //                            return RedirectToAction("EditRRUser", new { q = q });
        //                        }
        //                        else
        //                        {
        //                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

        //                        }

        //                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
        //                        db.Entry(item).State = EntityState.Modified;
        //                        db.SaveChanges();
        //                        break;
        //                    }
        //                case (ResponsibilityTypeKeys.Rates):
        //                    {
        //                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, true, false, false, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

        //                        if (SystUserId == 0)
        //                        {
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
        //                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

        //                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
        //                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

        //                            return RedirectToAction("EditRRUser", new { q = q });
        //                        }
        //                        else
        //                        {
        //                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

        //                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

        //                        }

        //                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
        //                        db.Entry(item).State = EntityState.Modified;
        //                        db.SaveChanges();
        //                        break;
        //                    }

        //                case (ResponsibilityTypeKeys.AcknowledgeRefundApplication):
        //                    {
        //                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, Convert.ToInt16(item.RefundApplicationId), 0, item.Id, true, false);

        //                        if (SystUserId == 0)
        //                        {
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

        //                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
        //                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

        //                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
        //                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

        //                            return RedirectToAction("EditRRUser", new { q = q });
        //                        }
        //                        else
        //                        {
        //                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

        //                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

        //                        }

        //                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
        //                        db.Entry(item).State = EntityState.Modified;
        //                        db.SaveChanges();
        //                        break;
        //                    }

        //                case (ResponsibilityTypeKeys.IssueRefundCollection):
        //                    {
        //                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, Convert.ToInt16(item.RefundApplicationId), 0, item.Id, false, true);

        //                        if (SystUserId == 0)
        //                        {
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

        //                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
        //                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

        //                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
        //                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

        //                            return RedirectToAction("EditRRUser", new { q = q });
        //                        }
        //                        else
        //                        {
        //                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
        //                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

        //                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

        //                        }

        //                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
        //                        db.Entry(item).State = EntityState.Modified;
        //                        db.SaveChanges();
        //                        break;
        //                    }
        //            }






        //        }

        //    }



        //}

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
            var findItem = db.RoundRobinQueues.Include(r=>r.Clerk).Include(r=>r.ResponsibilityType).FirstOrDefault(x => x.StatusId == SubmittedId && x.PropertyLeaseApplicationId == plmApps.Id);
            var bouserid = db.Customers.FirstOrDefault(x => x.Id == findItem.ClerkId).SystemUserId;
            var userrole = db.ApplicationUserRoles.Include(r=>r.IdentityRole).OrderByDescending(x=>x.Id).FirstOrDefault(x => x.SystemUserId == bouserid);
            var ResponsibilityTypeName = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault();
            var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityType).FirstOrDefault().Id;
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == rrqID).FirstOrDefault();
            var RoleName = db.Roles.Where(x => x.Name == userrole.IdentityRole.Name).FirstOrDefault().Id;

            var UsersList = GetUsersInRole(RoleName).Where(x => x.RoundRobinIsActive == true).ToList();

            var vm = new ManualReAllocationViewModel();
            vm.CurrentFullName = rrq.Clerk.FullName;
            vm.PropertyLeaseApplicationId = (int)id;
            vm.CurrentAssignedUserName = rrq.Clerk.UserFullName;
            vm.RoundRobinQueueId = rrq.Id;
            vm.ResponsibilityType = Convert.ToString(findItem.ResponsibilityType.Id);
            vm.ViewName = ViewName;
            vm.TitleName = TitleName;
            vm.BodyName = BodyName;
            ViewBag.BOUsers = new SelectList(UsersList, "SystemUser.Id", "SystemUser.FullName");
            return View(vm);
        }

        [HttpPost]
        public ActionResult ManualReAllocate(int? id, ManualReAllocationViewModel vm)
        {
            Initialise();

            var plmApps = db.PropertyLeaseApplications.Where(x => x.Id == vm.PropertyLeaseApplicationId && x.IsDeleted == false).FirstOrDefault();
            var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == vm.RoundRobinQueueId).FirstOrDefault();

            if (rrqList != null)
            {
                rrqList.StatusId = db.Status.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                rrqList.EndTaskDateTime = DateTime.Now;
                db.Entry(rrqList).State = EntityState.Modified;
                db.SaveChanges();
            }

            var ClerkId = db.Customers.Where(x => x.SystemUserId == vm.NewBackOfficeUser && x.IsDeleted == false).FirstOrDefault();
            var statusList = db.Status.ToList();
            var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

            var roundRobinQueue = new RoundRobinQueue
            {
                PropertyLeaseApplicationId = plmApps.Id,
                ResponsibilityTypeId = Convert.ToInt32(vm.ResponsibilityType),
                CurrentTaskDateTime = DateTime.Now,
                ClerkId = ClerkId.Id,
                StatusId = StatusId,
                AssignedFrom = rrqList.Id
            };
            db.RoundRobinQueues.Add(roundRobinQueue);
            db.SaveChanges();

            int rtype = Convert.ToInt32(vm.ResponsibilityType);
            var ResponsibilityType = db.ResponsibilityTypes.FirstOrDefault(x => x.Id == rtype);
            BackOfficeNotification(plmApps.Id, ClerkId.Id, ResponsibilityType.Name);


            var Title = vm.TitleName;
            var Body = vm.BodyName; // commment

            if (roundRobinQueue.Id == 0)
            {
                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Title;
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionFailMessage).FirstOrDefault().Body;
            }
            else
            {
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;
                var Result = ActivityTrackerAudit(plmApps.Id, ActivityTrackerMessage, Customer.Id);

                TempData[Title] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Title; ;
                TempData[Body] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RedistrubutionSuccessMessage).FirstOrDefault().Body + ClerkId.FullName;
            }

            return RedirectToAction(vm.ViewName);
        }

        public ActionResult EHCRoundRobbinDashboard()
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
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
                if (User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x =>/* x.ResponsibilityTypeId == ResponsibilityTypeId && */x.StatusId == SubmittedId).ToList();
                }
                else
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }
                //rrq= rrq.Where(x=>x.Clerk.SystemUser.CCC == 8)
                var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();
                
                var plmApps = db.PropertyLeaseApplications.Where(x => x.IsActive && list.Contains(x.Id))
                    .Include(r=>r.Status)
                    .Include(r=>r.SystemUser)
                    .Include(r=>r.CreatedBySystemUser)
                    .Include(r=>r.HumanEHCOptions)
                    .Include(r=>r.Customer).ToList();

                foreach(var Item in plmApps)
                {
                    var rrqplm = db.RoundRobinQueues.Include(r => r.Clerk).Where(x => x.PropertyLeaseApplicationId == Item.Id && x.StatusId == SubmittedId).FirstOrDefault();
                    Item.Data = rrqplm.Clerk.UserFullName;
                    Item.RoundRobinQueueId = rrqplm.Id;
                }   

                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();
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

                return View(plmApps);

            }
            else
            {
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                var rrq = db.RoundRobinQueues.Where(x => x.ClerkId == Customer.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == SubmittedId).ToList();
                var list = rrq.Select(x => x.RCSApplicationStatusId).ToList();
                var rcsApps = db.RCSApplicationStatus.Where(x => list.Contains(x.Id) && (x.Status.Key == StatusKeys.PendingDocumentsApproval || x.Status.Key == StatusKeys.PendingApplicationFeePaymentValidation)).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.Status).ToList();

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


        }

        public ActionResult EHCComplexAreaDashboard()
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
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
                List<PreferredComplexArea> prr = new List<PreferredComplexArea>();
                if (User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                    prr = db.PreferredComplexAreas.Include(x => x.HousingSuper).Include(x => x.LettingOfficer).ToList();
                }
                else
                {
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;

                    prr = db.PreferredComplexAreas.Include(x => x.HousingSuper).Include(x => x.LettingOfficer).ToList();
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.SystemUser.CCCId == Customer.SystemUser.CCCId && x.StatusId == SubmittedId).ToList();
                }

                return View(prr);

            }

            return View();

        }

        [DecryptParameter]
        public ActionResult EditArea(int? id, string ViewName, string TitleName, string BodyName)
        {
            Initialise();
            var userID = Customer.Id;
            var area = db.PreferredComplexAreas.Include(r=>r.LettingOfficer).Include(r=>r.HousingSuper).FirstOrDefault(x => x.Id==id);

            var userrole = area.HousingSuper != null ? db.ApplicationUserRoles.Include(r => r.IdentityRole).OrderByDescending(x => x.Id).FirstOrDefault(x => x.SystemUserId == area.HousingSuper.SystemUserId) : null;
            var RoleName = userrole != null ? db.Roles.Where(x => x.Name == userrole.IdentityRole.Name).FirstOrDefault().Id : db.Roles.Where(x => x.Name == "Housing Supervisor").FirstOrDefault().Id;

            var userrole2 = area.LettingOfficer != null ? db.ApplicationUserRoles.Include(r => r.IdentityRole).OrderByDescending(x => x.Id).FirstOrDefault(x => x.SystemUserId == area.LettingOfficer.SystemUserId) : null;
            var RoleName2 = userrole2 != null ? db.Roles.Where(x => x.Name == userrole2.IdentityRole.Name).FirstOrDefault().Id : db.Roles.Where(x => x.Name == "Client Services Officer").FirstOrDefault().Id;

            var LOUsersList = GetUsersInRole(RoleName2).Where(x => x.RoundRobinIsActive == true).ToList();
            var HSUsersList = GetUsersInRole(RoleName).Where(x => x.RoundRobinIsActive == true).ToList();

            var vm = new ManualReAllocationViewModel();
            vm.PreferredComplexArea = area;
            vm.CurrentFullName = area.Name;
            vm.PropertyLeaseApplicationId = (int)id;
            vm.CurrentAssignedUserName = area.Name;
            vm.ViewName = ViewName;
            vm.TitleName = TitleName;
            vm.BodyName = BodyName;
            ViewBag.BOUsers = new SelectList(HSUsersList, "SystemUser.Id", "SystemUser.FullName");
            ViewBag.LOUsers = new SelectList(LOUsersList, "SystemUser.Id", "SystemUser.FullName");
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditArea(int? id, ManualReAllocationViewModel vm)
        {
            Initialise();

            var area = db.PreferredComplexAreas.Include(r => r.LettingOfficer).Include(r => r.HousingSuper).FirstOrDefault(x => x.Id == vm.PropertyLeaseApplicationId);

            area.HousingSuperId = db.Customers.FirstOrDefault(x => x.SystemUserId == vm.NewBackOfficeUser).Id;
            area.LettingOfficerId = db.Customers.FirstOrDefault(x => x.SystemUserId == vm.LettingOfficerId).Id;
            area.ModifiedDateTime = DateTime.Now;
            db.Entry(area).State = EntityState.Modified;
            db.SaveChanges();

            var Title = vm.TitleName;
            var Body = vm.BodyName;
            return RedirectToAction(vm.ViewName);
        }

        [DecryptParameter]
        public ActionResult EditIsActive(int? id, string ViewName, string TitleName, string BodyName)
        {
            var findItem = db.PreferredComplexAreas.FirstOrDefault(x => x.Id == id);
            var result = findItem.IsActive == true ? MatchingHelper.ModifyComplexAvailability(db, (int)id, false) : MatchingHelper.ModifyComplexAvailability(db, (int)id, true);
            return RedirectToAction("EHCComplexAreaDashboard");
        }

        public ActionResult RoundRobbinActiveWorkQueues()
        {
            Initialise();
            var userID = Customer.Id;

            var Keys = db.Status;
            int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (User.IsInRole("Area Manager") || User.IsInRole("Rates Clearance Sectional Head") || User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
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
                List<Customer> prr = new List<Customer>();





                if (User.IsInRole("Support Admin") || User.IsInRole("Back Office System Administrator"))
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.StatusId == SubmittedId).ToList();
                    var list = rrq.Select(x => x.ClerkId).ToList();

                    prr = db.Customers.Include(x=>x.SystemUser).Where(x => list.Contains(x.Id)).ToList();

                    foreach(var Item in prr)
                    {
                        Item.Data = (db.RoundRobinQueues.Where(x => x.ClerkId == Item.Id && x.StatusId == SubmittedId).Count()).ToString();


                        var user = UserManager.FindByName(Item.SystemUser.UserName);
                        var userId = user.Id;
                        // get user roles
                        List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                        string RolesList = "";
                        int counter = 0;
                        foreach (var Roles in rolesArray)
                        {
                            if (counter > 0)
                            {
                                RolesList += ", ";
                            }
                            counter++;
                            RolesList += Roles;
                        }
                        Item.ColorCode = RolesList;
                    }

                }
                else
                {
                    rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.StatusId == SubmittedId).ToList();
                    var list = rrq.Select(x => x.ClerkId).ToList();

                    prr = db.Customers.Where(x => list.Contains(x.Id)).ToList();

                    foreach (var Item in prr)
                    {
                        Item.Data = (db.RoundRobinQueues.Where(x => x.ClerkId == Item.Id && x.StatusId == SubmittedId).Count()).ToString();


                        var user = UserManager.FindByName(Item.SystemUser.UserName);
                        var userId = user.Id;
                        // get user roles
                        List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                        string RolesList = "";
                        int counter = 0;
                        foreach (var Roles in rolesArray)
                        {
                            if (counter > 0)
                            {
                                RolesList += ", ";
                            }
                            counter++;
                            RolesList += Roles;
                        }
                        Item.ColorCode = RolesList;
                    }
                }

                return View(prr);

            }

            return View();

        }














































        #region DbConns
        //private eServicesDbContext db = new eServicesDbContext();


        private IntegrationEntities db2 = new IntegrationEntities();
        #endregion

        #region Json Methods


        #region Add Water Info
        public JsonResult AddWater(string WaterMeterInformation_WaterMeterNo, string WaterMeterInformation_WaterMeterReading, string WaterMeterInformation_WaterMeterReadingDateTaken)
        {
            var result = "";

            try
            {
                object Wilist = TempData["wp"];
                List<WaterMeterInformation> wl = new List<WaterMeterInformation>();

                if (Wilist == null)
                {
                    Wilist = wl;
                }

                else
                {
                    wl = (List<WaterMeterInformation>)Wilist;
                }

                WaterMeterInformation waterinfo = new WaterMeterInformation();
                waterinfo.WaterMeterNo = WaterMeterInformation_WaterMeterNo;
                waterinfo.WaterMeterReading = WaterMeterInformation_WaterMeterReading;

                waterinfo.WaterMeterReadingDateTaken = WaterMeterInformation_WaterMeterReadingDateTaken;



                if (waterinfo != null)
                {
                    wl.Add(waterinfo);
                    Wilist = wl;
                    TempData["wp"] = Wilist;
                    TempData.Keep("wp");
                    result = "Success";
                }
                else
                {
                    result = "Error";
                }

            }
            catch (Exception ex)
            {
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Add Water Info
        public JsonResult AddWaterApi(string WaterMeterInformation_WaterMeterNo, string WaterMeterInformation_WaterMeterReading, string WaterMeterInformation_WaterMeterReadingDateTaken)
        {
            var result = "";

            try
            {
                object Wilist = TempData["wp"];




                List<WaterMeterInformation> wl = new List<WaterMeterInformation>();

                if (Wilist == null)
                {
                    Wilist = wl;
                }

                else
                {
                    wl = (List<WaterMeterInformation>)Wilist;
                }

                WaterMeterInformation waterinfo = new WaterMeterInformation();
                waterinfo.WaterMeterNo = WaterMeterInformation_WaterMeterNo;
                waterinfo.WaterMeterReading = WaterMeterInformation_WaterMeterReading;
                if (WaterMeterInformation_WaterMeterReadingDateTaken.Length == 8)
                {
                    var Year = WaterMeterInformation_WaterMeterReadingDateTaken.Substring(0, 4);
                    var Month = WaterMeterInformation_WaterMeterReadingDateTaken.Substring(4, 2);
                    var Day = WaterMeterInformation_WaterMeterReadingDateTaken.Substring(6, 2);
                    var DateFinal = Year + '-' + Month + '-' + Day;
                    waterinfo.WaterMeterReadingDateTaken = DateFinal;
                }




                if (waterinfo != null)
                {
                    wl.Add(waterinfo);
                    Wilist = wl;
                    TempData["wp"] = Wilist;
                    TempData.Keep("wp");
                    result = "Success";
                }
                else
                {
                    result = "Error";
                }

            }
            catch (Exception ex)
            {
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Add Electricity Info
        public JsonResult AddElectricity(string ElectricityMeterInformation_ElectricityMeterNo, string ElectricityMeterInformation_ElectricityMeterReading, string ElectricityMeterInformation_ElectricityMeterReadingDateTaken)
        {
            var result = "";

            try
            {
                object Eilist = TempData["ep"];
                List<ElectricityMeterInformation> el = new List<ElectricityMeterInformation>();

                if (Eilist == null)
                {
                    Eilist = el;
                }

                else
                {
                    el = (List<ElectricityMeterInformation>)Eilist;
                }

                ElectricityMeterInformation electmeter = new ElectricityMeterInformation();
                electmeter.ElectricityMeterNo = ElectricityMeterInformation_ElectricityMeterNo;
                electmeter.ElectricityMeterReading = ElectricityMeterInformation_ElectricityMeterReading;
                electmeter.ElectricityMeterReadingDateTaken = Convert.ToDateTime(ElectricityMeterInformation_ElectricityMeterReadingDateTaken);



                if (electmeter != null)
                {
                    el.Add(electmeter);
                    Eilist = el;
                    TempData["ep"] = Eilist;
                    TempData.Keep("ep");
                    result = "Success";
                }
                else
                {
                    result = "Error";
                }

            }
            catch (Exception ex)
            {
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Add Electricity Info
        public JsonResult AddElectricity4(string ElectricityMeterInformation_ElectricityMeterNo, string ElectricityMeterInformation_ElectricityMeterReading, string ElectricityMeterInformation_ElectricityMeterReadingDateTaken)
        {
            var result = "";

            try
            {
                RegistrationViewModel vm = new RegistrationViewModel();
                List<RoundRobinLog> errorList = new List<RoundRobinLog>();


                var usernameAssigned = false;
                var emailAssigned = false;
                var mobileAssigned = false;


                usernameAssigned = db.SystemUsers.Any(u => u.UserName.ToLower() == ElectricityMeterInformation_ElectricityMeterNo.ToLower() && u.IsActive && u.IsDeleted == false);

                emailAssigned = db.SystemUsers.Any(u => u.EmailAddress.ToLower() == ElectricityMeterInformation_ElectricityMeterReadingDateTaken.ToLower() && u.IsActive && u.IsDeleted == false);

                mobileAssigned = db.SystemUsers.Any(u => u.MobileNumber == ElectricityMeterInformation_ElectricityMeterReading && u.IsActive && !u.IsDeleted);

                var response = "";
                string Username = ElectricityMeterInformation_ElectricityMeterNo;
                var count = ElectricityMeterInformation_ElectricityMeterNo.Length;

                if (usernameAssigned)
                {
                    RoundRobinLog error = new RoundRobinLog();
                    error.LogEntry = "The username entered is already taken.";
                    errorList.Add(error);
                    //response = "The username entered is already taken.";
                }
                if (count < 6 || count > 20)
                {
                    RoundRobinLog error0 = new RoundRobinLog();
                    error0.LogEntry = "The Username field must be 6 to 20 characters long.";
                    errorList.Add(error0);
                    //response = "The username entered is already taken.";
                }

                if (emailAssigned)
                {

                    RoundRobinLog error2 = new RoundRobinLog();
                    error2.LogEntry = "The email address entered is already in use.";
                    errorList.Add(error2);

                }
                if (mobileAssigned)
                {
                    RoundRobinLog error3 = new RoundRobinLog();
                    error3.LogEntry = "The mobile number entered is already in use.";
                    errorList.Add(error3);

                }


                if (usernameAssigned == false && emailAssigned == false && mobileAssigned == false)
                {
                    RoundRobinLog error4 = new RoundRobinLog();
                    error4.LogEntry = "Success";
                    errorList.Add(error4);
                }

                return Json(errorList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region CLear Electricity Water
        public JsonResult ClearMeterTable()
        {
            var result = "";

            try
            {
                object Eilist = TempData["ep"];
                List<ElectricityMeterInformation> el = new List<ElectricityMeterInformation>();
                TempData["ep"] = el;
                TempData.Keep("ep");
                //TempData.Remove("ep");





                object Wilist = TempData["wp"];
                List<WaterMeterInformation> wl2 = new List<WaterMeterInformation>();
                TempData["wp"] = wl2;
                TempData.Keep("wp");
                //TempData.Remove("wp");
                result = "Success";


            }
            catch (Exception ex)
            {
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Add Electricity Info Api
        public JsonResult AddElectricityApi(string ElectricityMeterInformation_ElectricityMeterNo, string ElectricityMeterInformation_ElectricityMeterReading, string ElectricityMeterInformation_ElectricityMeterReadingDateTaken)
        {
            var result = "";

            try
            {
                object Eilist = TempData["ep"];
                List<ElectricityMeterInformation> el = new List<ElectricityMeterInformation>();

                if (Eilist == null)
                {
                    Eilist = el;
                }

                else
                {
                    el = (List<ElectricityMeterInformation>)Eilist;
                }

                ElectricityMeterInformation electmeter = new ElectricityMeterInformation();
                electmeter.ElectricityMeterNo = ElectricityMeterInformation_ElectricityMeterNo;
                electmeter.ElectricityMeterReading = ElectricityMeterInformation_ElectricityMeterReading;

                if (ElectricityMeterInformation_ElectricityMeterReadingDateTaken.Length == 8)
                {
                    var Year = ElectricityMeterInformation_ElectricityMeterReadingDateTaken.Substring(0, 4);
                    var Month = ElectricityMeterInformation_ElectricityMeterReadingDateTaken.Substring(4, 2);
                    var Day = ElectricityMeterInformation_ElectricityMeterReadingDateTaken.Substring(6, 2);
                    var DateFinal = Year + '-' + Month + '-' + Day;
                    electmeter.ElectricityMeterReadingDateTaken = Convert.ToDateTime(DateFinal);
                }

                //electmeter.ElectricityMeterReadingDateTaken = Convert.ToDateTime(ElectricityMeterInformation_ElectricityMeterReadingDateTaken);



                if (electmeter != null)
                {
                    el.Add(electmeter);
                    Eilist = el;
                    TempData["ep"] = Eilist;
                    TempData.Keep("ep");
                    result = "Success";
                }
                else
                {
                    result = "Error";
                }

            }
            catch (Exception ex)
            {
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Add Natural Person
        public JsonResult AddNaturalPerson(string Gender, string PurchaserInformation_Capacity, string TitleType, string MaritialStatus, string PurchaserInformation_Initial, string PurchaserInformation_FirstName, string PurchaserInformation_LastName, string PurchaserInformation_IDNo, string PurchaserInformation_DOB, int PurchaserInformation_Age, string PurchaserInformation_Passport, string PurchaserInformation_PurDateExpiry, string PurchaserInformation_Nationality, string PurchaserInformation_CellNo, string PurchaserInformation_HomeNo, string PurchaserInformation_WorkNo, string PurchaserInformation_PurEmail, string PurchaserInformation_PostAddress, string PurchaserInformation_PostSuburb, string PurchaserInformation_PostalCity, string PurchaserInformation_PostPostalCode, string PurchaserInformation_PurchaseType, string PurchaserInformation_NominatedAddress)
        {
            var result = "";

            try
            {
                object Pilist = TempData["np"];
                List<PurchaserInformation> pl = new List<PurchaserInformation>();

                if (Pilist == null)
                {
                    Pilist = pl;
                }

                else
                {
                    pl = (List<PurchaserInformation>)Pilist;
                }

                PurchaserInformation naturalPerson = new PurchaserInformation();
                naturalPerson.Gender = Gender;
                naturalPerson.Title = TitleType;
                naturalPerson.MaritalStatus = MaritialStatus;
                naturalPerson.Initial = PurchaserInformation_Initial;
                naturalPerson.FirstName = PurchaserInformation_FirstName;
                naturalPerson.LastName = PurchaserInformation_LastName;
                naturalPerson.IDNo = PurchaserInformation_IDNo;
                if (PurchaserInformation_DOB != "")
                {
                    naturalPerson.DOB = Convert.ToDateTime(PurchaserInformation_DOB);
                }

                //if (PurchaserInformation_PurDateExpiry != "")
                //{
                //    naturalPerson.PurDateExpiry = Convert.ToDateTime(PurchaserInformation_PurDateExpiry);
                //}

                naturalPerson.CompanyName = PurchaserInformation_Capacity;
                naturalPerson.con_Com = PurchaserInformation_Passport;
                naturalPerson.Nationality = PurchaserInformation_Nationality;
                naturalPerson.CellNo = PurchaserInformation_CellNo;
                naturalPerson.HomeNo = PurchaserInformation_HomeNo;
                naturalPerson.WorkNo = PurchaserInformation_WorkNo;
                naturalPerson.PurEmail = PurchaserInformation_PurEmail;
                naturalPerson.CellNo = PurchaserInformation_PostAddress;
                naturalPerson.CompanyName = PurchaserInformation_PostSuburb;
                naturalPerson.CCRegNo = PurchaserInformation_PostalCity;
                naturalPerson.BusinessRegNo = PurchaserInformation_PostPostalCode;

                var getpurchasertypeid = db.PurchaserType.Where(x => x.Key == PurchaserInformation_PurchaseType).FirstOrDefault().Id;
                //naturalPerson. = getpurchasertypeid;
                naturalPerson.PurchaseType = getpurchasertypeid;
                naturalPerson.AppStatus = PurchaserInformation_NominatedAddress;
                naturalPerson.PurchaserTypeKey = PurchaserInformation_PurchaseType;


                if (naturalPerson != null)
                {
                    pl.Add(naturalPerson);
                    Pilist = pl;
                    TempData["np"] = Pilist;
                    TempData.Keep("np");
                    result = "Success";
                }
                else
                {
                    result = "Error";
                }

            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region View VA Details
        public JsonResult VAInformation(string MunicipalAccNo)
        {
            object result = null;

            try
            {
                if (MunicipalAccNo.Trim() == "")
                {
                    result = null;
                }
                else
                {
                    var MuniAccNo = Convert.ToInt64(MunicipalAccNo.Trim());
                    PropertyOwnerDetailsApi p = new PropertyOwnerDetailsApi();
                    var details = p.GetOwnerInfo(MuniAccNo.ToString());


                    //SolarAccountBalanceApi sab = new SolarAccountBalanceApi();
                    //var sabs = sab.GetAccountBalance(MuniAccNo.ToString());

                    //SolarCalculateAssessmentFigureApi sca = new SolarCalculateAssessmentFigureApi();
                    //var scas = sca.calculateAssessmentFigures(MuniAccNo.ToString());

                    dynamic data = JObject.Parse(details);
                    data = data.SolarERP.payload;
                    LimsPayload payload = new LimsPayload();
                    payload.streetaddress = data.streetaddress;




                    CustomerAccount custacc = new CustomerAccount();
                    string getidno = Convert.ToString(data.IDNumber);
                    //custacc.AccountNo= Convert.ToString(data.AccountNo);
                    custacc.Initials = Convert.ToString(data.FirstName);
                    custacc.Surname = Convert.ToString(data.LastName);
                    if (getidno != null && getidno.Length == 13)
                    {
                        custacc.IDNo = Convert.ToInt64(getidno);
                    }
                    custacc.WorkTel = Convert.ToString(data.WorkTelNo);
                    custacc.HomeTel = Convert.ToString(data.HomeTelNo);
                    custacc.EmailAddr = Convert.ToString(data.EmailAddr);
                    custacc.CellNo = Convert.ToString(data.CellPhoneNo);
                    custacc.PostalCode = Convert.ToString(data.PostalCode);
                    custacc.PostalAddress2 = Convert.ToString(data.PhysicalAddress2);
                    custacc.StreetAddr = Convert.ToString(data.PhysicalAddress1);
                    custacc.PostalAddress3 = Convert.ToString(data.PhysicalAddress3);



                    result = custacc;

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

        #endregion


        #region get Lims Property Details
        public JsonResult LimsPropertyDetails(string MunicipalAccNo)
        {
            object result = null;

            try
            {
                if (MunicipalAccNo.Trim() == "")
                {
                    result = null;
                }
                else
                {


                    var MuniAccNo = Convert.ToInt64(MunicipalAccNo.Trim());
                    LimsApi p = new LimsApi();
                    var details = p.GetPropertyDetails(MuniAccNo.ToString());

                    LimsPayload payload = new LimsPayload();

                    string CCCPrefix = MunicipalAccNo.Substring(0, 2);
                    var CCC = db.CCCs.Where(x => x.Prefix == CCCPrefix).FirstOrDefault();

                    if (CCC != null)
                    {
                        if (CCC.AreaManagerId == null)
                        {
                            var CCCNotSetup = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.CCCNotSetup).FirstOrDefault();

                            CCCNotSetup.Body = CCCNotSetup.Body.Replace("{1}", CCC.CCCName);
                            payload.Status = CCCNotSetup.Body;
                            payload.Title = CCCNotSetup.Title;
                            payload.StatusKey = CCCNotSetup.Key;
                        }
                        else
                        {
                            dynamic data = JObject.Parse(details);


                            var ttt = data.SolarERP.Header.Version;

                            payload.Status = data.SolarERP.Header.Result.Status;

                            if (payload.Status == "Success")
                            {
                                var rcsType = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.TypeofProperty).OrderBy(x => x.Name);

                                foreach (var item in data.SolarERP.Payload)
                                {
                                    payload.streetaddress = item.streetaddress;
                                    payload.registeredownerids = item.registeredownerids;
                                    payload.registeredownernames = item.registeredownernames;
                                    payload.categoryname = item.categoryname;

                                    if (payload.categoryname != null)
                                    {
                                        if (payload.categoryname.ToLower().Contains("residential"))
                                        {

                                            payload.categoryname = Convert.ToString(db.RCSTypes.Where(x => x.Description == RCSTypeKeys.residential_property).FirstOrDefault().Id);
                                        }
                                        else if (payload.categoryname.ToLower().Contains("commercial"))
                                        {
                                            payload.categoryname = Convert.ToString(db.RCSTypes.Where(x => x.Description == RCSTypeKeys.commercial_property).FirstOrDefault().Id);

                                        }
                                        else if (payload.categoryname.ToLower().Contains("vacant"))
                                        {
                                            payload.categoryname = Convert.ToString(db.RCSTypes.Where(x => x.Description == RCSTypeKeys.vacant_property).FirstOrDefault().Id);

                                        }
                                    }




                                    payload.erfnumber = item.erfnumber;
                                    if (item.deedextents != null)
                                    {
                                        payload.deedextents = item.deedextents;
                                    }
                                    else
                                    {
                                        payload.deedextents = 0;
                                    }

                                    payload.ssourceaccountnumber = item.ssourceaccountnumber;
                                    payload.regionname = item.regionname;

                                    //payload.registeredownernames = item.registeredownernames;

                                    //payload.registeredownernames = item.registeredownernames;




                                }

                            }

                        }
                    }
                    else
                    {
                        var NoCCCExists = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.NoCCCExistsForMunicipalAccNum).FirstOrDefault();
                        payload.Status = NoCCCExists.Body;
                        payload.Title = NoCCCExists.Title;
                        payload.StatusKey = NoCCCExists.Key;
                    }

                    //SolarAccountBalanceApi sab = new SolarAccountBalanceApi();
                    //var sabs = sab.GetAccountBalance(MuniAccNo.ToString());

                    //SolarCalculateAssessmentFigureApi sca = new SolarCalculateAssessmentFigureApi();
                    //var scas = sca.calculateAssessmentFigures(MuniAccNo.ToString());


                    //var streetad = data.SolarERP.Payload.streetaddress;

                    //var testwho = data.SolarERP.Payload;
                    //dynamic data2 = JObject.Parse(data.SolarERP.Payload);



                    return Json(payload, JsonRequestBehavior.AllowGet);

                    //if (details != string.Empty)
                    //{
                    //    dynamic data = JObject.Parse(details);

                    //    payload.CaudAccountNo = data.CaudAccountNo;
                    //    foreach (var item in data.StatusMessages)
                    //    {



                    //        payload.Status = item;
                    //        if (payload.Status.Contains("No transactions found for account-no"))
                    //        {
                    //            payload.Status = "No transactions found for account-no";
                    //        }

                    //    }
                    //    if (data.CaudAccountNo == null || data.CaudAccountNo == "")
                    //    {

                    //    }
                    //    else
                    //    {
                    //        foreach (var item in data.PaymentDetailsList)
                    //        {
                    //            PaymentDetailsListPayload = new PaymentDetailsList();
                    //            PaymentDetailsListPayload.Amt = item.Amt;
                    //            PaymentDetailsListPayload.Date = item.Date;
                    //            var date2 = Convert.ToString(item.Date);
                    //            var date = Convert.ToString(item.Date);
                    //            var len = date2.Length;
                    //            if (date.Length == 8)
                    //            {
                    //                var Year = date.Substring(0, 4);
                    //                var Month = date.Substring(4, 2);
                    //                var Day = date.Substring(6, 2);
                    //                var DateFinal = Year + '-' + Month + '-' + Day;
                    //                PaymentDetailsListPayload.ConvertedDate = Convert.ToDateTime(DateFinal);
                    //            }
                    //            PaymentDetailsListPayload.Ref = item.Ref;

                    //            PaymentDetailsListPayload.CustomerFirstName = item.CustomerFirstName;
                    //            PaymentDetailsListPayload.CustomerLastName = item.CustomerLastName;
                    //            PaymentDetailsListArray.Add(PaymentDetailsListPayload);

                    //        }

                    //        payload.PaymentDetailsList = PaymentDetailsListArray;

                    //    }



                    //}




                    //CustomerAccount custacc = new CustomerAccount();
                    //string getidno = Convert.ToString(data.IDNumber);
                    ////custacc.AccountNo= Convert.ToString(data.AccountNo);
                    //custacc.Initials = Convert.ToString(data.FirstName);
                    //custacc.Surname = Convert.ToString(data.LastName);
                    //if (getidno != null && getidno.Length == 13)
                    //{
                    //    custacc.IDNo = Convert.ToInt64(getidno);
                    //}
                    //custacc.WorkTel = Convert.ToString(data.WorkTelNo);
                    //custacc.HomeTel = Convert.ToString(data.HomeTelNo);
                    //custacc.EmailAddr = Convert.ToString(data.EmailAddr);
                    //custacc.CellNo = Convert.ToString(data.CellPhoneNo);
                    //custacc.PostalCode = Convert.ToString(data.PostalCode);
                    //custacc.PostalAddress2 = Convert.ToString(data.PhysicalAddress2);
                    //custacc.StreetAddr = Convert.ToString(data.PhysicalAddress1);
                    //custacc.PostalAddress3 = Convert.ToString(data.PhysicalAddress3);



                    //result = custacc;

                    //var propertyinfo = db.CustomerAccounts.Where(x => x.AccountNo == MuniAccNo).FirstOrDefault();
                    //result = propertyinfo;

                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                var e = ex.Message;
                result = "Error";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Municipal Meter Details
        public JsonResult MeterDetails(string MunicipalAccNo)
        {
            object result = null;
            ClearMeterTable();
            try
            {
                if (MunicipalAccNo.Trim() == "")
                {
                    result = null;
                }
                else
                {
                    MunicipalServiceDetailsApi API = new MunicipalServiceDetailsApi();
                    result = API.GetMunicipalServiceDetails(MunicipalAccNo);

                    //if(result.AttorneyCode == null || result.AttorneyCode == "")
                    //{
                    //    result.Status = "Attorney Code Not Found";
                    //}
                    var data = Json(result, JsonRequestBehavior.AllowGet);



                    return data;

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

        #endregion

        #region Search by Attorney Code 

        public JsonResult AttorneyCodeInformation(string AttorneyCode)
        {
            AttorneyDetails result = new AttorneyDetails();
            string Err;
            try
            {

                AttorneyDetailsApi API = new AttorneyDetailsApi();
                result = API.AttorneyDetails(AttorneyCode);

                dynamic data = JObject.Parse(result.Json);

                var tester = Json(result, JsonRequestBehavior.AllowGet);

                return tester;

            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                Err = "Error";
            }
            return Json(Err, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult AttorneyCodeInformation(string AttorneyCode)
        //{
        //    object result = null;

        //    try
        //    {
        //        if (AttorneyCode.Trim() == "")
        //        {
        //            result = null;
        //        }
        //        else
        //        {
        //            var AttCode = AttorneyCode.Trim();
        //            var AttorneyInfo = db2.TBL_RCS_ATTORNEYS.Where(x => x.Attorney_Code == AttCode).FirstOrDefault();

        //            AttorneyDetailsApi API = new AttorneyDetailsApi();
        //            result = API.AttorneyDetails(AttorneyCode);

        //            dynamic data = JObject.Parse(result.Json);

        //            if (AttorneyInfo == null)
        //            {
        //                result = "Error";

        //            }
        //            else
        //            {
        //                result = AttorneyInfo;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var e = ex.Message;
        //    }
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        #endregion
        #region Get Attorney Details
        public JsonResult GetAttorneyDetails(string id)
        {
            object result = null;

            try
            {

                result = "Unique";
                //var AttorneyDetails = RCSAttorneyApi.GetAttorneyDetails(id.Trim()).ToList();
                //result = AttorneyDetails;

            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region Capture OnLoad


        [DecryptParameter]
        public ActionResult ReUploadDocuments(int rcsAppId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rpropertyLeaseApplication = db.PropertyLeaseApplications
                    .Include(r => r.PurchaserType)
                    .Include(r => r.HumanEHCOptions).Include(r => r.IdentificationType)
                    .Include(r => r.Status)
                    .Where(x => x.Id == rcsAppId).FirstOrDefault();

                    var Customer = db.Customers.Where(x => x.Id == rpropertyLeaseApplication.CustomerId).FirstOrDefault();

                    string baseFormat = "";
                    var referenceType = cxt.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                    var application = cxt.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(
                        new
                        {
                            referenceId = Customer.Id,
                            customerId = Customer.Id,
                            referenceTypeId = referenceType.Id,
                            applicationId = application.Id,
                            agentId = application.Id,
                            returnUrl = baseFormat,
                            rcsappId = rpropertyLeaseApplication.Id
                        }))); 
                
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        
        public JsonResult ValidateAgainstWaitingList(string ID)
        {
            //var Application = db.PropertyLeaseApplications.FirstOrDefault(x => x.IDNo == ID);
            //var waiting = db.waitingListQues.FirstOrDefault(x => x.PropertyLeaseApplicationId == Application.Id);
            return Json(true, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Capture()
        {
            Initialise();
            var core = new eServicesDbContext();
            var empty = "";
            string id = String.Empty;
            var rcsType = core.RCSTypes.OrderBy(x => x.Name);
            var USER_CHECK_RELATIONSHIP = String.Empty;
            bool ValidateActiveApplication = false;
            var application = (PropertyLeaseApplication)null;
            var rrq = (LeaseDetails)null;
            if (User.IsInRole("Customers"))
            {
                if (Customer == null) return RedirectToAction("Dashboard", "Profile");

                var customer = core.Customers.Include(s => s.SystemUser).FirstOrDefault(c => c.Id == Customer.Id);
                if (customer == null) throw new Exception("Invalid Customer");

                var status = core.Status.FirstOrDefault(s => s.Key.Equals(StatusKeys.CustomerPendingApproval));
                if (status == null) throw new Exception(string.Format("Invalid/ missing status key {0}", StatusKeys.CustomerActive));

                object obj = new { customerId = Customer.Id, agentId = Agent.Id };
                if (customer.StatusId == status.Id) return RedirectToAction("Index3", "Profile", SecureActionLinkExtension.Encrypt(obj));

                ViewBag.Name = customer.FirstName;
                ViewBag.LastName = customer.LastName;
                ViewBag.Genderr = customer.Gender != null ? new SelectList(rcsType.Where(x => x.Name == customer.Gender), "Name", "Name") : new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_Gender), "Name", "Name");
                ViewBag.TitleTyperr = new SelectList(core.TitleTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Id == customer.TitleTypeId), "Id", "Name");
                ViewBag.IdentificationNumber = customer.IdentificationNumber;
                ViewBag.ContactNumbers = customer.CellPhoneNumber;
                ViewBag.EmailAddress = customer.EmailAddress;
                ViewBag.HomeNumber = customer.HomePhoneNumber;
                ViewBag.WorkNumber = customer.WorkPhoneNumber;
                ViewBag.Suburb = customer.PhysicalAddress5;
                ViewBag.PostalCode = customer.PhysicalAddressCode;
                ViewBag.Address = customer.PostalAddress1+" "+ customer.PostalAddress2 + " " + customer.PostalAddress3 + " " + customer.PostalAddress4 + " ";
                id = customer.IdentificationNumber;
            }
            else
            {
                ViewBag.Name = String.Empty;
                ViewBag.LastName = String.Empty;
                ViewBag.Genderr = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_Gender), "Name", "Name");
                ViewBag.TitleTyperr = new SelectList(core.TitleTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Id", "Name");
                ViewBag.IdentificationNumber = String.Empty;
                ViewBag.ContactNumbers = String.Empty;
                ViewBag.EmailAddress = String.Empty;
                ViewBag.HomeNumber = String.Empty;
                ViewBag.WorkNumber = String.Empty;
                ViewBag.Suburb = String.Empty;
                ViewBag.PostalCode = String.Empty;
                ViewBag.Address = String.Empty;
                id = String.Empty;
            }


            application = core.PropertyLeaseApplications.Include(o => o.Status).FirstOrDefault(r => r.CustomerId == Customer.Id) ?? null;
            if (application != null) rrq = core.LeaseDetails.Include(o => o.Status).FirstOrDefault(c => c.PropertyLeaseApplicationId == application.Id) ?? null;
            USER_CHECK_RELATIONSHIP = core.AppSettings.FirstOrDefault(r => r.Key == AppSettingKeys.USER_CHECK_RELATIONSHIP).Value.ToLower();
            if (application != null && rrq != null && rrq.Status.Key != StatusKeys.TerminatedLease) ValidateActiveApplication = true;
            if (application != null && application.Status.Key != StatusKeys.CreditScoreRejected) ValidateActiveApplication = true;
            if (application == null && rrq == null) ValidateActiveApplication = false;
            if (ValidateActiveApplication && USER_CHECK_RELATIONSHIP == "on")
            {
                Session["MessageBody"] = "You have an application in progress, you are unable to make another application at the moment.";
                Session["Display"] = "Display";
                Session["ApplicationRefNo"] = ".";
                Session["MessageTitle"] = "Unable to capture!";
                return RedirectToAction("Inbox");
            }


            TempData.Remove("np");
            ViewBag.Purchaser = new SelectList(core.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Key == PurchaserTypeKeys.NaturalPerson), "Id", "Name");
            ViewBag.Option = new SelectList(core.IncomeSources.OrderBy(x => x.SourceOfIncome).ToList(), "Id", "SourceOfIncome");
            ViewBag.UnitType = new SelectList(core.OccupationTypes.OrderBy(x => x.Propertytype), "Id", "Propertytype");
            ViewBag.PreferredArea = new SelectList(core.PreferredComplexAreas.OrderBy(x => x.Name).Where(x => x.IsActive && x.Key != "ekurhuleni_complex").ToList(), "Key", "Name");
            ViewBag.CompanyType = new SelectList(core.companyTypes.OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.HumanOptions = new SelectList(core.humanEHCOptions.OrderBy(x => x.Name).Where(x => x.Key != "hs_single_flats" || x.Key != "hs_single_flats").ToList(), "Id", "Name");
            ViewBag.EnvisageUsage = new SelectList(core.envisagedUsages.OrderBy(x => x.UssageName).ToList(), "Id", "UssageName");
            ViewBag.PropertyType = new SelectList(core.OccupationTypes.OrderBy(x => x.Propertytype).ToList(), "Id", "Propertytype");

            ViewBag.TitleType = new SelectList(core.TitleTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Id", "Name");
            ViewBag.TypeOfTransfer = new SelectList(core.TransferTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
            ViewBag.JuristicType = new SelectList(core.EntityTypes.Where(x => (bool)x.IsActive != false), "id", "Name");
            ViewBag.IdentificationType = new SelectList(core.IdentificationTypes.Where(x => x.Key == IdentificationTypeKey.SouthAfricanID), "id", "Name");
            ViewBag.MaritialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_MaritialStatus), "Name", "Name");
            ViewBag.TypeofProperty = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.TypeofProperty), "Id", "Name");
            ViewBag.ResidentialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_ResidentialStatus), "Name", "Name");
            ViewBag.Gender = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_Gender), "Name", "Name");
            ViewBag.SecondApplicantBool = false;
            var instructions = core.InstructionContents.Where(x => x.Key == InstructionContentKey.CaptureInstructions).FirstOrDefault();
            ViewBag.CaptureInstructions = "Capture Applicant Information </br> Upload Documents";
            ViewBag.ParkingRequiredStatus = new SelectList(core.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.PlmYes || x.Key == RCSActionTypeKeys.PlmNo).OrderByDescending(x => x.Name), "Key", "Name");

            ApplicationDepartment applicationDepartment = new ApplicationDepartment();
            return View();
        }

        #endregion

        #region Capture
        [HttpPost]
        public ActionResult Capture(CaptureViewModel capture, string TransferInformationSellingPrice, bool? ApplicantJointId, bool SecondApplicantBool, string ParkingRequired)
        {

            var Appref = "";
            var RCSAppID = 0;

            if (TransferInformationSellingPrice != null & TransferInformationSellingPrice != "")
            {
                //var test = (TransferInformationSellingPrice).Replace(',', '.');
                //capture.TransferInformation.SellingPrice = Convert.ToDecimal(test, CultureInfo.InvariantCulture);


                var test = (TransferInformationSellingPrice).Replace(',', '.');
                var ttt = Convert.ToDecimal(TransferInformationSellingPrice, CultureInfo.InvariantCulture);

                capture.TransferInformation.SellingPrice = ttt;

            }

            using (var cxt = new eServicesDbContext())
            {
                //start of capture PLM
                try
                {
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);
                    var AppSettings = cxt.AppSettings;
                    var Customers = cxt.Customers;
                    var Statuses = cxt.Status;
                    PropertyLeaseApplication PLA = new PropertyLeaseApplication();
                    PLA = capture.PropertyLeaseApplication;
                    var result = SecondApplicantBool == true ? PLA.SecondApplicant = true : PLA.SecondApplicant = false;
                    ////PLA.BType = (int)capture.PropertyLeaseApplication.BType;
                    ////PLA.BUsage = (int)capture.PropertyLeaseApplication.BUsage;
                    int limiter = 0;

                    AppSetting query = null;
                    var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSSequenceLimit);
                    var PreFix = "";
                    if (PLA.PurchaserTypeId == (cxt.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.NaturalPerson).FirstOrDefault().Id))
                    {
                        PreFix = "EHC";
                        query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.EHCReferenceDailyCounter);
                        SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.EHCReferenceDailyLimit);
                    }
                    else if ((PLA.PurchaserTypeId == (cxt.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.Company).FirstOrDefault().Id)))
                    {
                        PreFix = "RED";
                        query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.REDReferenceDailyCounter);
                        SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.REDReferenceDailyLimit);
                    }
                    if (ParkingRequired == RCSActionTypeKeys.PlmYes)
                        PLA.ParkingRequired = "Yes";
                    else
                        PLA.ParkingRequired = "No";


                    var BatchCounter = query.Value;
                    limiter = Convert.ToInt16(SeqLimit.Value);
                    if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                    {
                        var lastRef = query.Value;
                        BatchCounter = lastRef.ToString();
                        int nextSeq = Convert.ToInt16(query.Value) + 1;
                        string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                        query.Value = nextVal;
                        cxt.Entry(query).State = EntityState.Modified;
                        cxt.SaveChanges();
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
                        cxt.Entry(query).State = EntityState.Modified;
                        cxt.SaveChanges();

                    }


                    var RefNum = PreFix + DateTime.Now.ToString("yyyyMMdd") + BatchCounter;
                    PLA.ApplicationReferenceNumber = RefNum;
                    PLA.CompanyType = capture.PropertyLeaseApplication.CompanyType;
                    PLA.StatusId = Statuses.FirstOrDefault(o => o.Key == StatusKeys.ReuploadApplicationDocs).Id;
                    PLA.SystemUserId = SystemUser.Id;
                    PLA.CustomerId = Customer.Id;
                    PLA.IdentificationTypeId = capture.PropertyLeaseApplication.IdentificationTypeId;

                    if (PLA.PurchaserTypeId == (cxt.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.NaturalPerson).FirstOrDefault().Id))
                    {
                        PLA.ApplicantFullName = cxt.TitleTypes.FirstOrDefault(x=>x.Id== PLA.TitleTypeId).Name + " " + PLA.FirstName.ToString().Substring(0, 1) + " " + PLA.LastName;
                    }

                    if (PLA.PrefArea != null && PLA.PrefAreaOption2 != null)
                    {
                        PLA.PreferredComplexAreaId = cxt.PreferredComplexAreas.FirstOrDefault(x => x.Key == PLA.PrefArea).Id;
                        PLA.PreferredComplexArea2Id = cxt.PreferredComplexAreas.FirstOrDefault(x => x.Key == PLA.PrefAreaOption2).Id;

                    }
                    else
                    {
                        PLA.PreferredComplexAreaId = cxt.PreferredComplexAreas.FirstOrDefault().Id;
                        PLA.PreferredComplexArea2Id = cxt.PreferredComplexAreas.FirstOrDefault().Id;
                    }
                    if (capture.PropertyLeaseApplication.HouseRequired != null)
                    {
                        PLA.HumanEHCOptionsId = Convert.ToInt32(capture.PropertyLeaseApplication.HouseRequired);
                        
                        PLA.HouseRequired = capture.PropertyLeaseApplication.HouseRequired;
                    }
                    else
                    {
                        PLA.HumanEHCOptionsId = cxt.humanEHCOptions.FirstOrDefault().Id;
                        //PLA.HouseRequired = capture.PropertyLeaseApplication.HouseRequired;
                    }

                    

                    PLA.PurchaserTypeId = capture.PropertyLeaseApplication.PurchaserTypeId;
                   
                    var holdId = Convert.ToInt32(PLA.HouseRequired);
                    var HousingType = cxt.PurchaserType.Where(x => x.Id == PLA.PurchaserTypeId).FirstOrDefault();

                    if(holdId != 0 )
                    {
                        var HousingOption1 = cxt.humanEHCOptions.Where(x => x.Id == holdId).FirstOrDefault();

                        if (HousingType.Key == PurchaserTypeKeys.NaturalPerson)
                        {
                            if (HousingOption1.Key == HousingTypeKeys.HumanSettllement)
                            {
                                PLA.HousingType = HousingTypeKeys.HumanSettllement;
                            }
                            else if (HousingOption1.Key == HousingTypeKeys.EkurhuleniHousingC)
                            {
                                PLA.HousingType = HousingTypeKeys.EkurhuleniHousingC;
                            }
                            else
                            {
                                PLA.HousingType = HousingTypeKeys.EkurhuleniHousingC;
                            }
                        }
                    }
                 

                    if (HousingType.Key== PurchaserTypeKeys.Company)
                    {
                        PLA.HousingType = HousingTypeKeys.Company;
                    }

                    cxt.PropertyLeaseApplications.Add(PLA);
                    cxt.SaveChanges();

                    if (capture.ApplicantJoint != null)
                    {
                        ApplicantJoint joint = new ApplicantJoint();
                        joint = capture.ApplicantJoint;
                        joint.PropertyLeaseApplicationId = PLA.Id;

                        //var year = Convert.ToInt32(capture.ApplicantJoint.IDNo.Substring(0, 2));
                        //string dob = null;
                        //if (year>10)
                        //    dob = Convert.ToString("19" + year + "/" + capture.ApplicantJoint.IDNo.Substring(3, 2) + "/" + capture.ApplicantJoint.IDNo.Substring(5, 2));
                        //else
                        //    dob = Convert.ToString("200" + year + "/" + capture.ApplicantJoint.IDNo.Substring(3, 2) + "/" + capture.ApplicantJoint.IDNo.Substring(5, 2));

                        //DateTime newdob = Convert.ToDateTime(dob);
                        //joint.DOB = newdob;
                        cxt.ApplicantJoints.Add(joint);
                        cxt.SaveChanges();
                    }

                    //Monthly income
                    if (capture.MonthlyIncomeExpenses != null)
                    {
                        MonthlyIncome monthlyIncome = new MonthlyIncome();
                        monthlyIncome.PropertyLeaseApplicationId = PLA.Id;
                        monthlyIncome.GrossIncome = capture.MonthlyIncomeExpenses.GrossIncome;
                        monthlyIncome.Allowances = capture.MonthlyIncomeExpenses.Allowances;
                        monthlyIncome.FringeBenefits = capture.MonthlyIncomeExpenses.FringeBenefits;
                        monthlyIncome.OtherRegularIncome = capture.MonthlyIncomeExpenses.OtherRegularIncome;
                        monthlyIncome.TotalGrossIncome = capture.MonthlyIncomeExpenses.TotalGrossIncome;
                        monthlyIncome.PayeTaxLessDeductions = capture.MonthlyIncomeExpenses.PayeTaxLessDeductions;
                        monthlyIncome.PensionProvidentLessDeductions = capture.MonthlyIncomeExpenses.PensionProvidentLessDeductions;
                        monthlyIncome.UIFLessDeductions = capture.MonthlyIncomeExpenses.UIFLessDeductions;
                        monthlyIncome.MedicalAidLessDeductions = capture.MonthlyIncomeExpenses.MedicalAidLessDeductions;
                        monthlyIncome.OtherLessDeductions = capture.MonthlyIncomeExpenses.OtherLessDeductions;
                        monthlyIncome.TotalDeductions = capture.MonthlyIncomeExpenses.TotalDeductions;
                        monthlyIncome.NetIncome = capture.MonthlyIncomeExpenses.NetIncome;
                        monthlyIncome.OtherDividendsIncome = capture.MonthlyIncomeExpenses.OtherDividendsIncome;
                        monthlyIncome.TotalNetIncome = capture.MonthlyIncomeExpenses.TotalNetIncome;
                        monthlyIncome.ApplicantTypeId = cxt.ApplicantTypes.Where(p => p.Key == ActivityTrackerMessageKeys.FirstApplicant).FirstOrDefault().Id;
                        cxt.MonthlyIncomes.Add(monthlyIncome);
                        cxt.SaveChanges();

                        //For Expenses
                        MonthlyExpense monthlyExpense = new MonthlyExpense();
                        monthlyExpense.PropertyLeaseApplicationId = PLA.Id;
                        monthlyExpense.HEAccommodation = capture.MonthlyIncomeExpenses.HEAccommodation;
                        monthlyExpense.HEInsurances = capture.MonthlyIncomeExpenses.HEInsurances;
                        monthlyExpense.HERatesTaxes = capture.MonthlyIncomeExpenses.HERatesTaxes;
                        monthlyExpense.HESecurity = capture.MonthlyIncomeExpenses.HESecurity;
                        monthlyExpense.HEUpkeep = capture.MonthlyIncomeExpenses.HEUpkeep;
                        monthlyExpense.HEUtilitiesElectricity = capture.MonthlyIncomeExpenses.HEUtilitiesElectricity;
                        monthlyExpense.HEUtilitiesWater = capture.MonthlyIncomeExpenses.HEUtilitiesWater;
                        monthlyExpense.HEOthers = capture.MonthlyIncomeExpenses.HEOthers;
                        monthlyExpense.VEFuel = capture.MonthlyIncomeExpenses.VEFuel;
                        monthlyExpense.VEInsurance = capture.MonthlyIncomeExpenses.VEInsurance;
                        monthlyExpense.VEMaintenance = capture.MonthlyIncomeExpenses.VEMaintenance;
                        monthlyExpense.VEVehicleFinance = capture.MonthlyIncomeExpenses.VEVehicleFinance;
                        monthlyExpense.ELifeAssurances = capture.MonthlyIncomeExpenses.ELifeAssurances;
                        monthlyExpense.EShortTermInsurances = capture.MonthlyIncomeExpenses.EShortTermInsurances;
                        monthlyExpense.EOtherInsurancesFuneral = capture.MonthlyIncomeExpenses.EOtherInsurancesFuneral;

                        //Living expenses
                        monthlyExpense.LESupportMaintenance = capture.MonthlyIncomeExpenses.LESupportMaintenance;
                        monthlyExpense.LEBankCharges = capture.MonthlyIncomeExpenses.LEBankCharges;
                        monthlyExpense.LECellularAirtimeData = capture.MonthlyIncomeExpenses.LECellularAirtimeData;
                        monthlyExpense.LEClothing = capture.MonthlyIncomeExpenses.LEClothing;
                        monthlyExpense.LECreditCards = capture.MonthlyIncomeExpenses.LECreditCards;
                        monthlyExpense.LEDomesticEmployees = capture.MonthlyIncomeExpenses.LEDomesticEmployees;
                        monthlyExpense.LEDonations = capture.MonthlyIncomeExpenses.LEDonations;
                        monthlyExpense.LEEducationSchool = capture.MonthlyIncomeExpenses.LEEducationSchool;
                        monthlyExpense.LEEntertainment = capture.MonthlyIncomeExpenses.LEEntertainment;
                        monthlyExpense.LEGroceries = capture.MonthlyIncomeExpenses.LEGroceries;
                        monthlyExpense.LEInstalmentAccounts = capture.MonthlyIncomeExpenses.LEInstalmentAccounts;
                        monthlyExpense.LEMedicalAid = capture.MonthlyIncomeExpenses.LEMedicalAid;
                        monthlyExpense.LEMemberships = capture.MonthlyIncomeExpenses.LEMemberships;
                        monthlyExpense.LEPersonalLoans = capture.MonthlyIncomeExpenses.LEPersonalLoans;
                        monthlyExpense.LEPetCare = capture.MonthlyIncomeExpenses.LEPetCare;
                        monthlyExpense.LERetailAccounts = capture.MonthlyIncomeExpenses.LERetailAccounts;
                        monthlyExpense.LESecurity = capture.MonthlyIncomeExpenses.LESecurity;
                        monthlyExpense.LESubscriptions = capture.MonthlyIncomeExpenses.LESubscriptions;
                        monthlyExpense.LETelephones = capture.MonthlyIncomeExpenses.LETelephones;
                        monthlyExpense.LETransport = capture.MonthlyIncomeExpenses.LETransport;
                        monthlyExpense.LETV = capture.MonthlyIncomeExpenses.LETV;
                        monthlyExpense.LEOtherExpenses = capture.MonthlyIncomeExpenses.LEOtherExpenses;
                        monthlyExpense.LETotalExpenses = capture.MonthlyIncomeExpenses.LETotalExpenses;
                        
                        monthlyExpense.ApplicantTypeId = cxt.ApplicantTypes.Where(p => p.Key == ActivityTrackerMessageKeys.FirstApplicant).FirstOrDefault().Id;
                        cxt.MonthlyExpenses.Add(monthlyExpense);
                        cxt.SaveChanges();

                        if (SecondApplicantBool) 
                        {
                            MonthlyIncome secApplicantMonthlyIncome = new MonthlyIncome();
                            secApplicantMonthlyIncome.PropertyLeaseApplicationId = PLA.Id;
                            secApplicantMonthlyIncome.GrossIncome = capture.MonthlyIncomeExpenses.SecGrossIncome;
                            secApplicantMonthlyIncome.Allowances = capture.MonthlyIncomeExpenses.SecAllowances;
                            secApplicantMonthlyIncome.FringeBenefits = capture.MonthlyIncomeExpenses.SecFringeBenefits;
                            secApplicantMonthlyIncome.OtherRegularIncome = capture.MonthlyIncomeExpenses.SecOtherRegularIncome;
                            secApplicantMonthlyIncome.TotalGrossIncome = capture.MonthlyIncomeExpenses.SecTotalGrossIncome;
                            secApplicantMonthlyIncome.PayeTaxLessDeductions = capture.MonthlyIncomeExpenses.SecPayeTaxLessDeductions;
                            secApplicantMonthlyIncome.PensionProvidentLessDeductions = capture.MonthlyIncomeExpenses.SecPensionProvidentLessDeductions;
                            secApplicantMonthlyIncome.UIFLessDeductions = capture.MonthlyIncomeExpenses.SecUIFLessDeductions;
                            secApplicantMonthlyIncome.MedicalAidLessDeductions = capture.MonthlyIncomeExpenses.SecMedicalAidLessDeductions;
                            secApplicantMonthlyIncome.OtherLessDeductions = capture.MonthlyIncomeExpenses.SecOtherLessDeductions;
                            secApplicantMonthlyIncome.TotalDeductions = capture.MonthlyIncomeExpenses.SecTotalDeductions;
                            secApplicantMonthlyIncome.NetIncome = capture.MonthlyIncomeExpenses.SecNetIncome;
                            secApplicantMonthlyIncome.OtherDividendsIncome = capture.MonthlyIncomeExpenses.SecOtherDividendsIncome;
                            secApplicantMonthlyIncome.TotalNetIncome = capture.MonthlyIncomeExpenses.SecTotalNetIncome;
                            secApplicantMonthlyIncome.ApplicantTypeId = cxt.ApplicantTypes.Where(p => p.Key == ActivityTrackerMessageKeys.SpouseApplicant).FirstOrDefault().Id;
                            cxt.MonthlyIncomes.Add(secApplicantMonthlyIncome);
                            cxt.SaveChanges();

                            //For Spouse Expenses
                            MonthlyExpense secMonthlyExpense = new MonthlyExpense();
                            secMonthlyExpense.PropertyLeaseApplicationId = PLA.Id;
                            secMonthlyExpense.HEAccommodation = capture.MonthlyIncomeExpenses.SecHEAccommodation;
                            secMonthlyExpense.HEInsurances = capture.MonthlyIncomeExpenses.SecHEInsurances;
                            secMonthlyExpense.HERatesTaxes = capture.MonthlyIncomeExpenses.SecHERatesTaxes;
                            secMonthlyExpense.HESecurity = capture.MonthlyIncomeExpenses.SecHESecurity;
                            secMonthlyExpense.HEUpkeep = capture.MonthlyIncomeExpenses.SecHEUpkeep;
                            secMonthlyExpense.HEUtilitiesElectricity = capture.MonthlyIncomeExpenses.SecHEUtilitiesElectricity;
                            secMonthlyExpense.HEUtilitiesWater = capture.MonthlyIncomeExpenses.SecHEUtilitiesWater;
                            secMonthlyExpense.HEOthers = capture.MonthlyIncomeExpenses.SecHEOthers;
                            secMonthlyExpense.VEFuel = capture.MonthlyIncomeExpenses.SecVEFuel;
                            secMonthlyExpense.VEInsurance = capture.MonthlyIncomeExpenses.SecVEInsurance;
                            secMonthlyExpense.VEMaintenance = capture.MonthlyIncomeExpenses.SecVEMaintenance;
                            secMonthlyExpense.VEVehicleFinance = capture.MonthlyIncomeExpenses.SecVEVehicleFinance;
                            secMonthlyExpense.ELifeAssurances = capture.MonthlyIncomeExpenses.SecELifeAssurances;
                            secMonthlyExpense.EShortTermInsurances = capture.MonthlyIncomeExpenses.SecEShortTermInsurances;
                            secMonthlyExpense.EOtherInsurancesFuneral = capture.MonthlyIncomeExpenses.SecEOtherInsurancesFuneral;
                            //Living expenses
                            secMonthlyExpense.LESupportMaintenance = capture.MonthlyIncomeExpenses.SecLESupportMaintenance;
                            secMonthlyExpense.LEBankCharges = capture.MonthlyIncomeExpenses.SecLEBankCharges;
                            secMonthlyExpense.LECellularAirtimeData = capture.MonthlyIncomeExpenses.SecLECellularAirtimeData;
                            secMonthlyExpense.LEClothing = capture.MonthlyIncomeExpenses.SecLEClothing;
                            secMonthlyExpense.LECreditCards = capture.MonthlyIncomeExpenses.SecLECreditCards;
                            secMonthlyExpense.LEDomesticEmployees = capture.MonthlyIncomeExpenses.SecLEDomesticEmployees;
                            secMonthlyExpense.LEDonations = capture.MonthlyIncomeExpenses.SecLEDonations;
                            secMonthlyExpense.LEEducationSchool = capture.MonthlyIncomeExpenses.SecLEEducationSchool;
                            secMonthlyExpense.LEEntertainment = capture.MonthlyIncomeExpenses.SecLEEntertainment;
                            secMonthlyExpense.LEGroceries = capture.MonthlyIncomeExpenses.SecLEGroceries;
                            secMonthlyExpense.LEInstalmentAccounts = capture.MonthlyIncomeExpenses.SecLEInstalmentAccounts;
                            secMonthlyExpense.LEMedicalAid = capture.MonthlyIncomeExpenses.SecLEMedicalAid;
                            secMonthlyExpense.LEMemberships = capture.MonthlyIncomeExpenses.SecLEMemberships;
                            secMonthlyExpense.LEPersonalLoans = capture.MonthlyIncomeExpenses.SecLEPersonalLoans;
                            secMonthlyExpense.LEPetCare = capture.MonthlyIncomeExpenses.SecLEPetCare;
                            secMonthlyExpense.LERetailAccounts = capture.MonthlyIncomeExpenses.SecLERetailAccounts;
                            secMonthlyExpense.LESecurity = capture.MonthlyIncomeExpenses.SecLESecurity;
                            secMonthlyExpense.LESubscriptions = capture.MonthlyIncomeExpenses.SecLESubscriptions;
                            secMonthlyExpense.LETelephones = capture.MonthlyIncomeExpenses.SecLETelephones;
                            secMonthlyExpense.LETransport = capture.MonthlyIncomeExpenses.SecLETransport;
                            secMonthlyExpense.LETV = capture.MonthlyIncomeExpenses.SecLETV;
                            secMonthlyExpense.LEOtherExpenses = capture.MonthlyIncomeExpenses.SecLEOtherExpenses;
                            secMonthlyExpense.LETotalExpenses = capture.MonthlyIncomeExpenses.SecLETotalExpenses;
                            secMonthlyExpense.ApplicantTypeId = cxt.ApplicantTypes.Where(p => p.Key == ActivityTrackerMessageKeys.SpouseApplicant).FirstOrDefault().Id;
                            cxt.MonthlyExpenses.Add(secMonthlyExpense);
                            cxt.SaveChanges();
                        }
                    }


                    if (capture.WaterSessionList != null && capture.WaterSessionList != "")
                    {
                        List<CompanyDirectors> WaterMeterInfo = JsonConvert.DeserializeObject<List<CompanyDirectors>>(capture.WaterSessionList);

                        if (WaterMeterInfo.Count > 0)
                        {
                            foreach (var item in WaterMeterInfo)
                            {
                                CompanyDirectors company = new CompanyDirectors();
                                company.IDNo = item.IDNo;
                                company.FirstNames = item.FirstNames;
                                company.LastName = item.LastName;
                                company.PropertyLeaseApplicationId = PLA.Id;
                                company.StatusId = cxt.Status.Where(x=>x.Key==StatusKeys.CustomerActive).FirstOrDefault().Id;
                                cxt.CompanyDirectors.Add(company);
                                cxt.SaveChanges();
                            }
                        }
                    }
                    if (PreFix == "EHC")
                    {
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationCapture).Description.ToString() + " By " + PLA.FirstName + " " + PLA.LastName+" for Ekurhuleni Housing"; // * siyanda if statement for company vs individual
                        MatchingHelper.ActivityTrackerAudit(cxt, PLA.Id, ActivityTrackerMessage, Customer.Id);
                    }
                    else if (PreFix == "RED")
                    {
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationCapture).Description.ToString() + " By " + PLA.FirstName + " " + PLA.LastName + " for Real Estate"; // * siyanda if statement for company vs individual
                        MatchingHelper.ActivityTrackerAudit(cxt, PLA.Id, ActivityTrackerMessage, Customer.Id);
                    }

                    //Send e-mail and SMS notification
                    int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationCaptureEmail).Id;
                    EmailHelper.CustomerEmailNotification(cxt, PLA.Id, emailboodyId);

                    string baseFormat = "";
                    //return Redirect(baseFormat);
                    var referenceType = cxt.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                    var application = cxt.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    var returnUrl2 = "Test"/*success.ToString(CultureInfo.InvariantCulture)*/;
                    return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(
                        new { 
                            referenceId = Customer.Id,
                            customerId = Customer.Id,
                            referenceTypeId = referenceType.Id, 
                            applicationId = application.Id, 
                            agentId = application.Id, 
                            returnUrl = baseFormat, 
                            rcsappId = PLA.Id
                        })));



                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    return View("Inbox");
                }
            }

            //using (var cxt = new eServicesDbContext())
            //{
            //    try
            //    {
            //        Initialise();
            //        BaseHelper _base = new BaseHelper();
            //        _base.Initialise(cxt);
            //        var CodeForRCSSave = "";
            //        if (CodeForRCSSave != "Ready")
            //        {
            //            var AppSettings = cxt.AppSettings;
            //            var Customers = cxt.Customers;
            //            var Statuses = cxt.Status;

            //            TransferInformation ti = new TransferInformation();
            //            ti = capture.TransferInformation;
            //            var getTransfertypeid = 2;
            //            ti.TransferTypeName = "Normal";
            //            ti.TransferType = getTransfertypeid;
            //            ti.RatesNumber = "1802391077";
            //            cxt.TransferInformations.Add(ti);
            //            cxt.SaveChanges();
            //            var TransferInfoID = ti.Id;

            //            MunicipalAccountInformation MI = new MunicipalAccountInformation();
            //            MI = capture.MunicipalAccountInformation;
            //            cxt.MunicipalAccountInformations.Add(MI);
            //            cxt.SaveChanges();

            //            string CCCPrefix = ti.RatesNumber.Substring(0, 2);
            //            var CCC = cxt.CCCs.Where(x => x.Prefix == CCCPrefix).FirstOrDefault();

            //            RCSApplicationStatus appli = new RCSApplicationStatus();
            //            appli.Customer = Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
            //            appli.StatusId = Statuses.FirstOrDefault(o => o.Key == StatusKeys.ReuploadApplicationDocs).Id;
            //            appli.CCCId = CCC.Id;
            //            appli.TransferInformationId = TransferInfoID;
            //            appli.TransferInformation = ti;

            //            int limiter = 0;

            //            AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSSequence);
            //            var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSSequenceLimit);
            //            var BatchCounter = query.Value;
            //            limiter = Convert.ToInt16(SeqLimit.Value);
            //            if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
            //            {
            //                var lastRef = query.Value;
            //                BatchCounter = lastRef.ToString();
            //                int nextSeq = Convert.ToInt16(query.Value) + 1;
            //                string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
            //                query.Value = nextVal;
            //                cxt.Entry(query).State = EntityState.Modified;
            //                cxt.SaveChanges();
            //            }
            //            else
            //            {
            //                int nextSeq = 1;
            //                string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
            //                BatchCounter = nextVal;
            //                nextSeq = 2;
            //                nextVal = nextSeq.ToString().PadLeft(limiter, '0');
            //                query.Value = nextVal;
            //                query.ModifiedDateTime = DateTime.Now.Date;
            //                cxt.Entry(query).State = EntityState.Modified;
            //                cxt.SaveChanges();

            //            }

            //            if (ti.RatesNumber != null)
            //            {
            //                var refs = ti.RatesNumber;
            //                var RefNum = refs + "PLM" + DateTime.Now.ToString("ddMMyyyy") + BatchCounter;
            //                appli.ApplicationReferenceNumber = RefNum;
            //                Appref = appli.ApplicationReferenceNumber;
            //            }

            //            appli.MunicipalAccountInformationId = MI.Id;
            //            appli.MunicipalAccountInformation = MI;

            //            appli.TransferInformationId = TransferInfoID;
            //            appli.TransferInformation = ti;

            //            //var SystUserId = RoundRobin(false, false, false, false, false, true);
            //            //var cust = Customers.Where(x => x.SystemUserId == SystUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
            //            //appli.ClerkId = cust;

            //            cxt.RCSApplicationStatus.Add(appli);
            //            cxt.SaveChanges();

            //            RCSAppID = appli.Id;

            //            SellerInformation sellerinfo = new SellerInformation();

            //            sellerinfo = capture.SellerInformation;
            //            sellerinfo.RCSApplicationStatusId = appli.Id;
            //            sellerinfo.RCSApplicationStatus = appli;

            //            cxt.SellerInformations.Add(sellerinfo);
            //            cxt.SaveChanges();


            //            var PI = (List<PurchaserInformation>)TempData["np"];
            //            if (PI != null)
            //            {
            //                foreach (var item in PI)
            //                {
            //                    PurchaserInformation purchInfo = new PurchaserInformation();

            //                    purchInfo = item;
            //                    purchInfo.RCSApplicationStatusId = appli.Id;
            //                    purchInfo.RCSApplicationStatus = appli;
            //                    cxt.PurchaserInformations.Add(purchInfo);
            //                    cxt.SaveChanges();
            //                }
            //            }

            //            if (PI == null)
            //            {
            //                PurchaserInformation pi = new PurchaserInformation();
            //                pi = capture.PurchaserInformation;

            //                pi.RCSApplicationStatusId = appli.Id;
            //                var getpurchasertypeid = db.PurchaserType.Where(x => x.Key == pi.PurchaserTypeKey).FirstOrDefault().Id;
            //                pi.PurchaserTypeId = getpurchasertypeid;
            //                pi.PurchaseType = getpurchasertypeid;
            //                cxt.PurchaserInformations.Add(pi);
            //                cxt.SaveChanges();
            //            }
            //            //var EI = (List<ElectricityMeterInformation>)TempData["ep"];
            //            //if (EI != null)
            //            //{
            //            //    foreach (var item in EI)
            //            //    {
            //            //        ElectricityMeterInformation elecinfo = new ElectricityMeterInformation();

            //            //        elecinfo = item;
            //            //        elecinfo.RCSApplicationStatusId = appli.Id;
            //            //        elecinfo.RCSApplicationStatus = appli;
            //            //        cxt.ElectricityMeterInformations.Add(elecinfo);
            //            //        cxt.SaveChanges();
            //            //    }
            //            //}
            //            //string testll = "ssss";
            //            //var test = JsonConvert.DeserializeObject<List<ElectricityMeterInformation>>(testll);
            //            if (capture.ElectricitySessionList != null && capture.ElectricitySessionList != "")
            //            {
            //                List<ElectricityMeterInformation> ElectricityMeterInfo = JsonConvert.DeserializeObject<List<ElectricityMeterInformation>>(capture.ElectricitySessionList);
            //                if (ElectricityMeterInfo.Count > 0)
            //                {
            //                    foreach (var item in ElectricityMeterInfo)
            //                    {
            //                        ElectricityMeterInformation elecinfo = new ElectricityMeterInformation();

            //                        //if (item.DateString.Length == 8)
            //                        //{
            //                        //    CultureInfo provider = CultureInfo.InvariantCulture;
            //                        //    string format = "yyyyMMdd";
            //                        //    DateTime result = DateTime.ParseExact(item.DateString, format, provider);
            //                        //    item.ElectricityMeterReadingDateTaken = result;
            //                        //}
            //                        //else
            //                        //{
            //                        //    item.ElectricityMeterReadingDateTaken = Convert.ToDateTime(item.DateString);
            //                        //}
            //                        elecinfo = item;
            //                        elecinfo.RCSApplicationStatusId = appli.Id;
            //                        elecinfo.RCSApplicationStatus = appli;
            //                        cxt.ElectricityMeterInformations.Add(elecinfo);
            //                        cxt.SaveChanges();
            //                    }
            //                }
            //            }

            //            if (capture.WaterSessionList != null && capture.WaterSessionList != "")
            //            {
            //                List<WaterMeterInformation> WaterMeterInfo = JsonConvert.DeserializeObject<List<WaterMeterInformation>>(capture.WaterSessionList);

            //                if (WaterMeterInfo.Count > 0)
            //                {
            //                    foreach (var item in WaterMeterInfo)
            //                    {
            //                        WaterMeterInformation waterinfo = new WaterMeterInformation();
            //                        CultureInfo provider = CultureInfo.InvariantCulture;

            //                        //if (item.DateString.Length == 8)
            //                        //{
            //                        //    string format = "yyyyMMdd";
            //                        //    DateTime result = DateTime.ParseExact(item.DateString, format, provider);
            //                        //    item.WaterMeterReadingDateTaken = result;
            //                        //}
            //                        //else
            //                        //{
            //                        //    item.WaterMeterReadingDateTaken = Convert.ToDateTime(item.DateString);
            //                        //}

            //                        waterinfo = item;
            //                        waterinfo.RCSApplicationStatusId = appli.Id;
            //                        waterinfo.RCSApplicationStatus = appli;
            //                        cxt.WaterMeterInformations.Add(waterinfo);
            //                        cxt.SaveChanges();
            //                    }
            //                }
            //            }



            //            //var WI = (List<WaterMeterInformation>)TempData["wp"];
            //            //if (WI != null)
            //            //{
            //            //    foreach (var item in WI)
            //            //    {
            //            //        WaterMeterInformation waterinfo = new WaterMeterInformation();

            //            //        waterinfo = item;
            //            //        waterinfo.RCSApplicationStatusId = appli.Id;
            //            //        waterinfo.RCSApplicationStatus = appli;
            //            //        cxt.WaterMeterInformations.Add(waterinfo);
            //            //        cxt.SaveChanges();
            //            //    }
            //            //}

            //            var ActivityTrackerMessage = cxt.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ConevayncerCapturesNewApplication).Description.ToString()/* + DecisionType*/;
            //            var RCSHistoryLog = new RCSApplicationHistoryLog
            //            {
            //                RCSApplicationStatusId = appli.Id,
            //                AuditAction = ActivityTrackerMessage,
            //                UserId = Customer.Id,
            //                CreatedDateTime = DateTime.Now,
            //                IsActive = true
            //            };
            //            cxt.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
            //            cxt.SaveChanges();
            //            Email SendMail = new Email();
            //            var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.ApplicationCapturedSuccessfully).FirstOrDefault();
            //            string attorneyemail = appli.Customer.EmailAddress;
            //            string attorneyname = appli.Customer.FirstName + " " + appli.Customer.LastName;
            //            //string emailbody = "Your RCS Application has been captured successfully your Reference Number is : "+ appli.ApplicationReferenceNumber;
            //            string emailbody = getemailbody.Description + appli.ApplicationReferenceNumber;
            //            SendMail.GenerateEmail(attorneyemail, "PLM - New Online Application Submission",
            //                          emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

            //            var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EmailNewApplication).Description.ToString() + emailbody;
            //            var RCSHistoryLog2 = new RCSApplicationHistoryLog
            //            {
            //                RCSApplicationStatusId = appli.Id,
            //                AuditAction = ActivityTrackerMessage2,
            //                UserId = Customer.Id,
            //                CreatedDateTime = DateTime.Now,
            //                IsActive = true
            //            };
            //            cxt.RCSApplicationHistoryLogs.Add(RCSHistoryLog2);
            //            cxt.SaveChanges();
            //            //List<RCSDepartmentType> DepartmentList = new List<RCSDepartmentType>();
            //            //DepartmentList = db.RCSDepartmentTypes.ToList();
            //            //CaptureController c = new CaptureController();

            //            ////Insert all departments
            //            //foreach (var department in DepartmentList)
            //            //{
            //            //    DepartmentsApproval depApprovals = new DepartmentsApproval();
            //            //    List<DepartmentsApproval> depList = new List<DepartmentsApproval>();
            //            //    var ApprovrcsType = Statuses.Where(x => x.Key == StatusKeys.PendingDocumentsApproval).FirstOrDefault();
            //            //    depApprovals.StatusId = ApprovrcsType.Id;
            //            //    depApprovals.DepartmentId = department.Id;
            //            //    depApprovals.CapturedDate = DateTime.Now;
            //            //    depApprovals.FailureReason = "Failure at " + department.Name;
            //            //    depApprovals.RCSApplicationStatusId = Convert.ToInt32(appli.Id);

            //            //    if (department.Key == RCSDepartmentTypeKeys.SundryAccountSection)
            //            //    {
            //            //        var SystemUserId = c.RoundRobin(true, false, false, false, false, false);
            //            //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
            //            //        depApprovals.AssignedToCustomerId = custId;
            //            //    }
            //            //    else if (department.Key == RCSDepartmentTypeKeys.LegalSection)
            //            //    {
            //            //        var SystemUserId = c.RoundRobin(false, true, false, false, false, false);
            //            //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
            //            //        depApprovals.AssignedToCustomerId = custId;
            //            //    }
            //            //    else if (department.Key == RCSDepartmentTypeKeys.CreditControlSection)
            //            //    {
            //            //        var SystemUserId = c.RoundRobin(false, false, true, false, false, false);
            //            //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
            //            //        depApprovals.AssignedToCustomerId = custId;
            //            //    }
            //            //    else if (department.Key == RCSDepartmentTypeKeys.BillingSection)
            //            //    {
            //            //        var SystemUserId = c.RoundRobin(false, false, false, true, false, false);
            //            //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
            //            //        depApprovals.AssignedToCustomerId = custId;
            //            //    }
            //            //    else if (department.Key == RCSDepartmentTypeKeys.EndowmentSection)
            //            //    {
            //            //        var SystemUserId = c.RoundRobin(false, false, false, false, true, false);
            //            //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
            //            //        depApprovals.AssignedToCustomerId = custId;
            //            //    }

            //            //    depList.Add(depApprovals);
            //            //    c.departmentsApprovals(depList);
            //            //    //break;
            //            //}
            //            string pgMerchantId = "pg_crm_app_rcs";
            //            string voteNumber = appli.TransferInformation.RatesNumber;
            //            voteNumber = "RCS " + voteNumber;
            //            string pgMerchantReference = appli.ApplicationReferenceNumber;
            //            string pgMerchantDescription = "RCS Application Fee";
            //            var ApplicationFeeAmount = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSApplicationFeeAmt).Value;

            //            string Amount = ApplicationFeeAmount;
            //            //string pgEmail = "Sashen.moodley@xetgroup.com";
            //            //string pgMobile = "0846666435";

            //            string pgEmail = null;
            //            string pgMobile = null;
            //            if (!string.IsNullOrEmpty(appli.Customer.SystemUser.EmailAddress))
            //            {
            //                pgEmail = appli.Customer.SystemUser.EmailAddress;
            //            }

            //            if (!string.IsNullOrEmpty(appli.Customer.SystemUser.MobileNumber))
            //            {
            //                pgMobile = appli.Customer.SystemUser.MobileNumber;
            //            }
            //            //pgMobile = "0846666435";



            //            //string customerFirstName = "Sashen";
            //            string customerFirstName = appli.Customer.FirstName;
            //            //string customerLastName = "Moodley";
            //            string customerLastName = appli.Customer.LastName;
            //            string returnUrl = capture.returnurl;
            //            //returnUrl = "http://localhost:3450/Capture/Capture";
            //            string adhocRef1 = Convert.ToString(RCSAppID);

            //            string adhocRef2 = "";
            //            string adhocRef3 = "";
            //            string adhocRef4 = "";
            //            string adhocRef5 = "";
            //            returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
            //            //returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
            //            string amt = Amount.Replace('.', ',');
            //            decimal conAmt = Convert.ToDecimal(amt);
            //            returnUrl = returnUrl + "/ReturnBackUrl";
            //            //Format  parameters into Single Delimited String
            //            string enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}",
            //                pgMerchantId, voteNumber, pgMerchantReference, pgMerchantDescription, Amount,
            //                pgEmail, pgMobile, customerFirstName, customerLastName, returnUrl, adhocRef1, adhocRef2, adhocRef3, adhocRef4, adhocRef5);
            //            //Encrypt the Single String to and Encrypted string e
            //            var e = new AesCrypto(encp).Encrypt(enc);
            //            AppSetting PGDomain = db.AppSettings.FirstOrDefault(o => o.Key == PaymentGatewayKeys.PGDomain);
            //            AppSetting PGEnvironment = db.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSPaymentGateway);

            //            var baseFormat = PGDomain.Value + "PaymentGateway/" + PGEnvironment.Value + "?q=" + e;

            //            //return Redirect(baseFormat);
            //            var referenceType = cxt.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
            //            var application = cxt.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

            //            var returnUrl2 = "Test"/*success.ToString(CultureInfo.InvariantCulture)*/;
            //            return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = baseFormat, rcsappId = appli.Id })));



            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            //        throw;
            //    }
            //}
            //return RedirectToAction("Index", "RCSApplication");
        }
        #endregion

        #region AffectedDepartGET
        [DecryptParameter]
        public ActionResult AffectedDepart(int id)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var Customerid = Customer;
                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                    var userID = User.Identity.GetUserId();
                    var applicationProp = db.PropertyLeaseApplications.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Include(r => r.Customer).Include(r=>r.SystemUser).Where(x => x.Id == id).FirstOrDefault();
                    ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                    ViewBag.FirstName = applicationProp.FirstName;
                    ViewBag.RfNo = applicationProp.ApplicationReferenceNumber;
                    ViewBag.DateCreated = applicationProp.CreatedDateTime.Value;
                    ViewBag.Sat = applicationProp.FirstName;
                    ViewBag.Id = id;

                    ViewBag.UserFName = custmusers.FirstName;
                    ViewBag.UserLName = custmusers.LastName;
                    ViewBag.UserName = custmusers.AttorneyCode;
                    ViewBag.UserEmail = custmusers.EmailAddress;

                    var applicationuserrole = context.ApplicationUserRoles.Find(id);
                    var systemUser = context.SystemUsers.Find(applicationuserrole.SystemUserId);
                    var user = UserManager.FindByName(systemUser.UserName);
                    /*ar userId = user.Id;*/

                    var depart = db.DepartmentsCoEs.Where(r => r.RepresentedBy != "" || r.RepresentedBy != null).ToList();
                    var vm = new DepartmentsApprovalViewModel();

                    vm.Features = new SelectList(depart, "Name", "Name");
                    vm.PropertyLeaseApplication = applicationProp;
                    var RolesList2 = depart.Select(x => new SelectListItem()
                    {

                        Text = x.DepartmentName,
                        Value = x.DepartmentName,

                    });
                    vm.Features = RolesList2;
                    


                    ViewBag.ApplicationId = id;
                    ViewBag.SystemUser = systemUser;
                    ViewBag.RoleId = new SelectList(depart, "Name", "Name");



                    CommitteeOutcome hd = new CommitteeOutcome();
                    ApplicationDepartment AP = new ApplicationDepartment();
                    hd.Surname = systemusers.LastName;
                    hd.FirstName = systemusers.FirstName;
                    ApplicationDepartment application = new ApplicationDepartment();
                    var dvm = new DepartmentsApprovalViewModel
                    {
                        ApplicationDepartment = AP,
                        PropertyLeaseApplication = applicationProp,
                        CommitteeOutcome = hd
                    };
                    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                    ViewBag.CommitteeName = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                    ViewBag.RecommendDFC = new SelectList(db.recommendations.OrderBy(x => x.RecommendatinName).Where(x => x.Key == "dfc"), "Key", "Name");
                    ViewBag.RecommendREAC = new SelectList(db.recommendations.OrderBy(x => x.RecommendatinName).Where(x => x.Key == "reac"), "Key", "Name");
                    ViewBag.date = DateTime.Now.Date;
                    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                    return View(vm);
                }
                catch
                {

                }
            }
            return View();
        }
        #endregion

        #region Schedule Unit Inspection
        [DecryptParameter]
        public ActionResult ScheduleInspectionSlots(int id)
        {
            if (id == 0) throw new Exception("Invalid application.");

            if (Session["Display"] != null)
            {
                if (Session["ApplicationRefNo"] == null)
                {
                    Session["Display"] = "True";

                    if (Session["Display"].ToString() == "True")
                    {

                        ViewBag.Display = "True";
                        ViewBag.MessageTitle3 = Session["MessageTitle"].ToString() /*message.Title + Session["ApplicationRefNo"]*/;
                        ViewBag.MessageBody3 = Session["MessageBody"].ToString();
                        Session["MessageBody"] = null;
                    }
                    Session["Display"] = null;
                    Session["ApplicationRefNo"] = null;
                    Session["MessageTitle"] = null;
                }
            }
            Session["Display"] = null;
            Session["ApplicationRefNo"] = null;
            Session["MessageTitle"] = null;


            Initialise();
            var Customerid = Customer;
            var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
            var applicationProp = db.PropertyLeaseApplications.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Include(r => r.Customer).Include(r => r.SystemUser).Where(x => x.Id == id).FirstOrDefault();
            var TimeSlots = db.TimeSlots.Where(r => r.IsActive).ToList();
            var vm = new DepartmentsApprovalViewModel();
            vm.Features = new SelectList(TimeSlots, "Id", "Name");
            vm.PropertyLeaseApplication = applicationProp;

            var rr = db.InspectionSchedules.Where(x => x.PropertyLeaseApplicationId == id && x.IsDeleted).ToList();
            var rrq = rr.Select(r => r.DateToScheduleId).ToList();

            vm.InspectionScheduleList = db.InspectionSchedules.Include(r=>r.DateToSchedule).Include(r=>r.TimeSlot).Where(x => x.PropertyLeaseApplicationId == id && !x.IsDeleted && !x.IsInspected).ToList() ?? null;
            ViewBag.DateToSchedule = "";
            ViewBag.Id = id;
            ViewBag.PrpId = id;
            ViewBag.UserFName = custmusers.FirstName;
            ViewBag.UserLName = custmusers.LastName;
            ViewBag.UserName = custmusers.AttorneyCode;
            ViewBag.UserEmail = custmusers.EmailAddress;

            if (Session["TimeslotScheduledSession"] != null)
            {
                var value = Session["TimeslotScheduledSession"].ToString();
                Session["TimeslotScheduledSession"] = null;
                ViewBag.TimeslotScheduledSession = value;
            }
            Session["TimeslotScheduledSession"] = null;
            return View(vm);
        }

        [HttpPost]
        public ActionResult ScheduleInspectionSlots(DepartmentsApprovalViewModel departments, DateTime DateToSchedule, params string[] SelectedRoles)
        {
            if (SelectedRoles == null)
            {
                Session["Display"] = "display";
                Session["MessageBody"] = "Please select hours available for the day!";
                Session["MessageTitle"] = "Invalid hours available";
                return RedirectToAction("ScheduleInspectionSlots", "PropertyLeaseApplication", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + departments.PropertyLeaseApplication.Id.ToString()) });

            }

            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    if (SelectedRoles != null)
                    {
                        Initialise();
                        var Customerid = Customer;
                        var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);

                        var rr = context.InspectionSchedules.Where(x => x.PropertyLeaseApplicationId == departments.PropertyLeaseApplication.Id && !x.IsDeleted).ToList();
                        var rrq = rr.Select(r => r.DateToScheduleId).ToList();
                        var Dates = context.DateToSchedules.Where(x => x.PropertyLeaseApplicationId == departments.PropertyLeaseApplication.Id && x.ShecduleDate == DateToSchedule).GroupBy(r => r.ShecduleDate).Select(x => x.FirstOrDefault()).ToList();
                        if (Dates.Count == 0)
                        {
                            DateToSchedule dateTo = new DateToSchedule();
                            dateTo.ShecduleDate = DateToSchedule.Date;
                            dateTo.PropertyLeaseApplicationId = departments.PropertyLeaseApplication.Id;
                            context.DateToSchedules.Add(dateTo);
                            context.SaveChanges();
                            Dates = db.DateToSchedules.Where(x => x.PropertyLeaseApplicationId == departments.PropertyLeaseApplication.Id && x.ShecduleDate == DateToSchedule).GroupBy(r => r.ShecduleDate).Select(x => x.FirstOrDefault()).ToList();
                        }
                        foreach (var item in Dates)
                        {
                            foreach (var item2 in SelectedRoles)
                            {
                                var thisItem = Convert.ToInt32(item2);
                                var ispsch = db.InspectionSchedules.OrderByDescending(x=>x.Id).FirstOrDefault(x => x.DateToScheduleId == item.Id && x.TimeSlotId== thisItem && !x.IsDeleted);
                                var timeslot = db.TimeSlots.FirstOrDefault(x => x.Id == thisItem).Id;
                                var scheduleddates = db.ScheduledInspections.ToList();
                                var User = GetBackOfficeId(db,  departments.PropertyLeaseApplication.Id, false);
                                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                                var UserId = User.Id != 0 ? User.Id : activeDirectoryOn;

                                if (ispsch != null)
                                {
                                    var result = true;
                                }
                                else
                                {
                                    InspectionSchedule apd = new InspectionSchedule();
                                    DateToSchedule dateTo = new DateToSchedule();
                                    dateTo.ShecduleDate = DateToSchedule.Date;
                                    dateTo.PropertyLeaseApplicationId = departments.PropertyLeaseApplication.Id;
                                    var checkdate = context.DateToSchedules.Where(x => x.ShecduleDate == DateToSchedule && x.PropertyLeaseApplicationId == departments.PropertyLeaseApplication.Id).FirstOrDefault();
                                    if (checkdate != null)
                                    {
                                        dateTo = checkdate;
                                    }
                                    else
                                    {
                                        context.DateToSchedules.Add(dateTo);
                                        context.SaveChanges();
                                    }
                                    apd.PropertyLeaseApplicationId = departments.PropertyLeaseApplication.Id;
                                    apd.DateToScheduleId = dateTo.Id;
                                    apd.TimeSlotId = timeslot;
                                    context.InspectionSchedules.Add(apd);
                                    context.SaveChanges();
                                }

                            }
                        }

                        Session["TimeslotScheduledSession"] = string.Format($"Timeslot(s) has been scheduled for application reference ,{departments.PropertyLeaseApplication.ApplicationReferenceNumber}");

                    }
                    return RedirectToAction("ScheduleInspectionSlots", "PropertyLeaseApplication", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + departments.PropertyLeaseApplication.Id.ToString()) });
                }
                catch (Exception IO) { }
            }
            return View("Inbox");
        }

        [DecryptParameter]
        public ActionResult InspectionNotificationApplicant(int Id)
        {
            using (var context = new eServicesDbContext())
            {
                var dates = context.DateToSchedules.Where(x => x.PropertyLeaseApplicationId == Id).ToList();
                if (dates.Count>0)
                {
                    MatchingHelper.ChangeApplicationStatus(context, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwitingInspectionSchedule)?.Id, Id);

                    var User =  GetBackOfficeId(db,  Id, false);    
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.ScheduleInspectionSlots).FirstOrDefault();
                    var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                    var UserId = User.Id != 0 ? User.Id : activeDirectoryOn;
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)Id, null, ResponsibilityTypeId.Id, UserId);

                    int emailboodyId = context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.UnitInspectionScheduleMail).Id;
                    EmailHelper.CustomerEmailNotification(context, Id, emailboodyId);
                    var plmApps = db.PropertyLeaseApplications.Where(x => x.Id == Id).FirstOrDefault();
                    Session["UnitInspectionScheduledSession"] = string.Format($"Unit inspection schedule has been completed successfully for application reference ,{plmApps.ApplicationReferenceNumber}");
                }
            }
            return RedirectToAction("PropertyLeaseInspections", "PropertyLeaseApplication");
        }

        public ActionResult DeleteSchedule(int Id, int PrpId)
        {
            if (Id == 0) throw new Exception("Invalid schedule");
            using (var context = new eServicesDbContext())
            {
                MatchingHelper.DeleteSchedule(context, Id);
            }
            ViewBag.PrpId = PrpId;
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ApplicationRequestTimeSlot(int Id)
        {
            if (Id == 0) throw new Exception("Invalid schedule");
            using (var core = new eServicesDbContext())
            {
                MatchingHelper.ChangeApplicationStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id, Id);
                EHCRoundRobin(Id, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RequestTimeSlots).Id;
                EmailHelper.CustomerEmailNotification(core, Id, emailboodyId);
                //return RedirectToAction("Inbox");
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [DecryptParameter]
        public ActionResult ScheduleUnitInspectionSlot(int id)
        {
            if (id == 0) throw new Exception("Invalid application.");

            Initialise();
            var Customerid = Customer;
            var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
            var applicationProp = db.PropertyLeaseApplications.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Include(r => r.Customer).Include(r => r.SystemUser).Where(x => x.Id == id).FirstOrDefault();
            if (applicationProp.Status.Key == StatusKeys.RatesRebateAdditionalPropertyOwnersPending) return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = applicationProp.Id, appId = "" });

            var TimeSlots = db.TimeSlots.Where(r => r.IsActive).ToList();
            var vm = new DepartmentsApprovalViewModel();
            vm.Features = new SelectList(TimeSlots, "Id", "Name");

            vm.PropertyLeaseApplication = applicationProp;
            vm.InspectionScheduleList = db.InspectionSchedules.Include(r => r.DateToSchedule).Include(r => r.TimeSlot).Where(x => x.PropertyLeaseApplicationId == id && !x.IsDeleted && !x.IsInspected).ToList() ?? null;
            ViewBag.DateToSchedule = "";
            ViewBag.Id = id;
            ViewBag.PrpId = id;
            ViewBag.UserFName = custmusers.FirstName;
            ViewBag.UserLName = custmusers.LastName;
            ViewBag.UserName = custmusers.AttorneyCode;
            ViewBag.UserEmail = custmusers.EmailAddress;
            return View(vm);
        }

        public ActionResult ApproveTimeSlot(int Id)
        {
            if (Id == 0) throw new Exception("Invalid schedule");
            using (var context = new eServicesDbContext())
            {
                MatchingHelper.ScheduleInspectionUnit(context, Id);
                MatchingHelper.DeleteSchedule(context, Id);
                MatchingHelper.RemoveUnselectedSchedules(context, Id);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ValidateSlotAvailability(int Id)
        {
            if (Id == 0) throw new Exception("Invalid schedule");
            
            using (var context = new eServicesDbContext())
            {
                var schedule = context.InspectionSchedules.FirstOrDefault(x => x.Id == Id);
                var User =  GetBackOfficeId(db, (int)schedule.PropertyLeaseApplicationId, false);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var UserId = User.Id != 0 ? User.Id : activeDirectoryOn;

                bool result = MatchingHelper.ValidateSelectedWithApproved(context, Id, UserId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion




        #region AffectedDepartPOST
        [HttpPost]
        public ActionResult AffectedDepart(DepartmentsApprovalViewModel departments,params string[] SelectedRoles)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    var names = "";
                    if (SelectedRoles != null)
                   {
                        ApplicationDepartment apd = new ApplicationDepartment();
                        Initialise();
                        var Customerid = Customer;
                        var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        foreach (var item in SelectedRoles)
                        {
                            if (names!=""){
                                names = names + ", "; }
                            apd.PropertyApplicationRef = departments.PropertyLeaseApplication.ApplicationReferenceNumber;
                            apd.PropertyLeaseApplicationId = departments.PropertyLeaseApplication.Id;
                            apd.DepartmentType = item;
                            apd.StatusId = (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepartmentResponse)?.Id;
                            apd.ActionDueDate = departments.ApplicationDepartment.ActionDueDate;
                            context.ApplicationDepart.Add(apd);
                            context.SaveChanges();
                            names = names + item ;

                            MatchingHelper.ChangeDepartmentStatus(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepartmentResponse)?.Id, apd.Id);

                        }
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationDepartmentInputs).Description.ToString() + ": " + names+".";
                        //var Result = ActivityTrackerAudit(apd.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                        var PropertyId = db.PropertyLeaseApplications.Where(x=>x.ApplicationReferenceNumber== departments.PropertyLeaseApplication.ApplicationReferenceNumber).FirstOrDefault().Id;
                        //MatchingHelper.ChangeApplicationStatus(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepartmentComments)?.Id, PropertyId);
                        MatchingHelper.ChangeApplicationStatusAudi(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepartmentComments)?.Id, PropertyId, ActivityTrackerMessage, custmusers.Id);
                    }
                    return RedirectToAction("Inbox");
                }
                catch(Exception IO) { }
            }
            return View("Inbox");
        }
        #endregion






        #region Lease AffectedDepartGET
        [DecryptParameter]
        public ActionResult LeeaseAffectedDepart(int id)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var Customerid = Customer;
                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                    var userID = User.Identity.GetUserId();

                    LeaseDetails leaseDetails = new LeaseDetails();

                    leaseDetails = db.LeaseDetails.Where(x => x.Id == id).Include(r=>r.Status).Include(r => r.PurchaserType).Include(r => r.CreatedBySystemUser).FirstOrDefault();
                    var applicationProp = db.PropertyLeaseApplications.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Include(r => r.Customer).Include(r => r.SystemUser).Where(x => x.Id == leaseDetails.PropertyLeaseApplicationId).FirstOrDefault();
                    var newLease = db.LeaseDetails.OrderByDescending(x => x.Id).Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Include(r => r.SystemUser).Where(x => x.Id == id).FirstOrDefault();
                    
                    ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                    ViewBag.FirstName = newLease.FirstNames;
                    ViewBag.RfNo = newLease.LeaseReferenceNo;
                    ViewBag.DateCreated = Convert.ToString(newLease.CreatedDateTime.Value);
                    ViewBag.Sstatus = newLease.Status.Name;
                    ViewBag.CurrentLeaseId = leaseDetails.Id;
                    ViewBag.Purchaser = leaseDetails.PurchaserType.Name;

                    Session["LeaseId"] = leaseDetails.Id;

                    ViewBag.UserFName = custmusers.FirstName;
                    ViewBag.UserLName = custmusers.LastName;
                    ViewBag.UserName = custmusers.AttorneyCode;
                    ViewBag.UserEmail = custmusers.EmailAddress;

                    var applicationuserrole = context.ApplicationUserRoles.Find(id);
                    var systemUser = context.SystemUsers.Find(applicationuserrole.SystemUserId);
                    var user = UserManager.FindByName(systemUser.UserName);
                    /*ar userId = user.Id;*/

                    var depart = db.DepartmentsCoEs.Where(r => r.RepresentedBy != "" || r.RepresentedBy != null).ToList();
                    var vm = new DepartmentsApprovalViewModel();

                    vm.Features = new SelectList(depart, "Name", "Name");
                    vm.PropertyLeaseApplication = applicationProp;
                    var RolesList2 = depart.Select(x => new SelectListItem()
                    {

                        Text = x.DepartmentName,
                        Value = x.DepartmentName,

                    });
                    vm.Features = RolesList2;



                    ViewBag.ApplicationId = id;
                    ViewBag.SystemUser = systemUser;
                    ViewBag.RoleId = new SelectList(depart, "Name", "Name");



                    CommitteeOutcome hd = new CommitteeOutcome();
                    ApplicationDepartment AP = new ApplicationDepartment();
                    hd.Surname = systemusers.LastName;
                    hd.FirstName = systemusers.FirstName;
                    ApplicationDepartment application = new ApplicationDepartment();
                    var dvm = new DepartmentsApprovalViewModel();

                    dvm.LeaseDetails = newLease;
                    dvm.ApplicationDepartment = AP;
                    dvm.PropertyLeaseApplication = applicationProp;
                    dvm.CommitteeOutcome = hd;

                   



                    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                    ViewBag.CommitteeName = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                    ViewBag.RecommendDFC = new SelectList(db.recommendations.OrderBy(x => x.RecommendatinName).Where(x => x.Key == "dfc"), "Key", "Name");
                    ViewBag.RecommendREAC = new SelectList(db.recommendations.OrderBy(x => x.RecommendatinName).Where(x => x.Key == "reac"), "Key", "Name");
                    ViewBag.date = DateTime.Now.Date;
                    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");

                    return View(vm);
                }
                catch
                {

                }
            }
            return View();
        }
        #endregion

        #region Lease AffectedDepartPOST
        [HttpPost]
        public ActionResult LeeaseAffectedDepart(DepartmentsApprovalViewModel departments, params string[] SelectedRoles)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    var names = "";
                    if (SelectedRoles != null)
                    {
                        ApplicationDepartment apd = new ApplicationDepartment();
                        Initialise();
                        var Customerid = Customer;
                        var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        var LeaseId = Convert.ToInt32(Session["LeaseId"].ToString());
                        Session["LeaseId"] = "";
                        var leae = context.LeaseDetails.Where(x => x.Id == LeaseId).FirstOrDefault();

                        foreach (var item in SelectedRoles)
                        {
                            if (names != "")
                            {
                                names = names + ", ";
                            }
                            apd.LeaseDetailsReference = leae.LeaseReferenceNo;
                            apd.LeaseDetailsId = leae.Id;
                            apd.PurchaserTypeId = leae.PurchaserTypeId;
                            apd.DepartmentType = item;
                            apd.StatusId = (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepartmentResponse)?.Id;
                            apd.ActionDueDate = departments.ApplicationDepartment.ActionDueDate;
                            context.ApplicationDepart.Add(apd);
                            context.SaveChanges();
                            names = names + item;

                            MatchingHelper.ChangeDepartmentStatus(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepartmentResponse)?.Id, apd.Id);

                        }
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationDepartmentInputs).Description.ToString() + ": " + names + ".";
                        //var Result = ActivityTrackerAudit(apd.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                        var PropertyId = db.PropertyLeaseApplications.Where(x => x.ApplicationReferenceNumber == departments.PropertyLeaseApplication.ApplicationReferenceNumber).FirstOrDefault().Id;
                        //MatchingHelper.ChangeApplicationStatus(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepartmentComments)?.Id, PropertyId);
                        MatchingHelper.ChangeApplicationStatusAudi(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepartmentComments)?.Id, PropertyId, ActivityTrackerMessage, custmusers.Id);
                    }
                    return RedirectToAction("Inbox");
                }
                catch (Exception IO) { }
            }
            return View("Inbox");
        }
        #endregion

        [EncryptedActionParameter]
        public ActionResult UploadDocs(int rcsAppId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.RCSApplicationStatus.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).Where(x => x.CustomerId == Customer.Id).Where(x => x.Id == rcsAppId).FirstOrDefault();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    int num = 5;

                    return RedirectToAction("ReUpload", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = rcsAppId })));



                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        #region CaptureWalkIn OnLoad
        public ActionResult CaptureWalkIn()
        {
            TempData.Remove("np");
            var rcsType = db.RCSTypes.OrderBy(x => x.Name);
            ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
            ViewBag.TitleType = new SelectList(db.TitleTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Id", "Name");
            ViewBag.TypeOfTransfer = new SelectList(db.TransferTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
            ViewBag.JuristicType = new SelectList(db.EntityTypes.Where(x => (bool)x.IsActive != false), "id", "Name");
            ViewBag.IdentificationType = new SelectList(db.IdentificationTypes.Where(x => (bool)x.IsDeleted != true), "id", "Name");
            ViewBag.ActOnBehalfOf = new SelectList(db.ActOnBehalfTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Id", "Name");
            ViewBag.MaritialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_MaritialStatus), "Name", "Name");
            ViewBag.TypeofProperty = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.TypeofProperty), "Id", "Name");
            ViewBag.ResidentialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_ResidentialStatus), "Name", "Name");
            ViewBag.Gender = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_Gender), "Name", "Name");
            var instructions = db.InstructionContents.Where(x => x.Key == InstructionContentKey.CaptureWalkinInstructions).FirstOrDefault();
            ViewBag.CaptureInstructions = instructions.Description;
            return View();
        }

        #endregion

        #region CaptureWalkIn
        [HttpPost]
        public ActionResult CaptureWalkIn(CaptureViewModel capture, string TransferInformationSellingPrice)
        {
            var Appref = "";
            var RCSAppID = 0;

            if (TransferInformationSellingPrice != null & TransferInformationSellingPrice != "")
            {
                var test = (TransferInformationSellingPrice).Replace(',', '.');
                var ttt = Convert.ToDecimal(TransferInformationSellingPrice, CultureInfo.InvariantCulture);

                capture.TransferInformation.SellingPrice = ttt;
            }

            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);
                    var CodeForRCSSave = "";
                    if (CodeForRCSSave != "Ready")
                    {
                        var AppSettings = cxt.AppSettings;
                        var Customers = cxt.Customers;
                        var Statuses = cxt.Status;

                        TransferInformation ti = new TransferInformation();
                        ti = capture.TransferInformation;
                        var getTransfertypeid = db.TransferTypes.Where(x => x.Key == ti.TransferTypeName).FirstOrDefault().Id;

                        ti.TransferTypeName = ti.TransferTypeName;
                        ti.TransferType = getTransfertypeid;
                        cxt.TransferInformations.Add(ti);
                        cxt.SaveChanges();
                        var TransferInfoID = ti.Id;

                        MunicipalAccountInformation MI = new MunicipalAccountInformation();
                        MI = capture.MunicipalAccountInformation;
                        cxt.MunicipalAccountInformations.Add(MI);
                        cxt.SaveChanges();

                        string CCCPrefix = ti.RatesNumber.Substring(0, 2);
                        var CCC = cxt.CCCs.Where(x => x.Prefix == CCCPrefix).FirstOrDefault();

                        ConveyancingAttorneyDetail CADgetattorneycode = new ConveyancingAttorneyDetail();
                        CADgetattorneycode = capture.ConveyancingAttorneyDetail;

                        RCSApplicationStatus appli = new RCSApplicationStatus();
                        appli.Customer = Customers.Include(o => o.SystemUser).Where(o => o.AttorneyCode == CADgetattorneycode.AttorneyCode).FirstOrDefault();
                        appli.StatusId = Statuses.FirstOrDefault(o => o.Key == StatusKeys.ReuploadApplicationDocs).Id;
                        appli.TransferInformationId = TransferInfoID;
                        appli.TransferInformation = ti;
                        appli.CCCId = CCC.Id;





                        int limiter = 0;

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
                            cxt.Entry(query).State = EntityState.Modified;
                            cxt.SaveChanges();
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
                            cxt.Entry(query).State = EntityState.Modified;
                            cxt.SaveChanges();

                        }

                        if (ti.RatesNumber != null)
                        {
                            var refs = ti.RatesNumber;
                            var RefNum = refs + "RCC" + DateTime.Now.ToString("ddMMyyyy") + BatchCounter;
                            appli.ApplicationReferenceNumber = RefNum;
                            Appref = appli.ApplicationReferenceNumber;
                        }

                        appli.MunicipalAccountInformationId = MI.Id;
                        appli.MunicipalAccountInformation = MI;

                        appli.TransferInformationId = TransferInfoID;
                        appli.TransferInformation = ti;

                        var SystUserId = RoundRobin(false, false, false, false, false, true);
                        var cust = Customers.Where(x => x.SystemUserId == SystUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        appli.ClerkId = cust;

                        cxt.RCSApplicationStatus.Add(appli);
                        cxt.SaveChanges();

                        RCSAppID = appli.Id;

                        ConveyancingAttorneyDetail CAD = new ConveyancingAttorneyDetail();
                        CAD = capture.ConveyancingAttorneyDetail;
                        CAD.RCSApplicationStatusId = appli.Id;
                        cxt.ConveyancingAttorneyDetails.Add(CAD);
                        cxt.SaveChanges();

                        SellerInformation sellerinfo = new SellerInformation();

                        sellerinfo = capture.SellerInformation;
                        sellerinfo.RCSApplicationStatusId = appli.Id;
                        sellerinfo.RCSApplicationStatus = appli;
                        cxt.SellerInformations.Add(sellerinfo);
                        cxt.SaveChanges();

                        var PI = (List<PurchaserInformation>)TempData["np"];
                        if (PI != null)
                        {
                            foreach (var item in PI)
                            {
                                PurchaserInformation purchInfo = new PurchaserInformation();

                                purchInfo = item;
                                purchInfo.RCSApplicationStatusId = appli.Id;
                                purchInfo.RCSApplicationStatus = appli;
                                cxt.PurchaserInformations.Add(purchInfo);
                                cxt.SaveChanges();
                            }
                        }
                        if (PI == null)
                        {
                            PurchaserInformation pi = new PurchaserInformation();
                            pi = capture.PurchaserInformation;

                            pi.RCSApplicationStatusId = appli.Id;
                            var getpurchasertypeid = db.PurchaserType.Where(x => x.Key == pi.PurchaserTypeKey).FirstOrDefault().Id;
                            pi.PurchaserTypeId = getpurchasertypeid;
                            pi.PurchaseType = getpurchasertypeid;
                            cxt.PurchaserInformations.Add(pi);
                            cxt.SaveChanges();
                        }
                        if (capture.ElectricitySessionList != null && capture.ElectricitySessionList != "")
                        {
                            List<ElectricityMeterInformation> ElectricityMeterInfo = JsonConvert.DeserializeObject<List<ElectricityMeterInformation>>(capture.ElectricitySessionList);
                            if (ElectricityMeterInfo.Count > 0)
                            {
                                foreach (var item in ElectricityMeterInfo)
                                {
                                    ElectricityMeterInformation elecinfo = new ElectricityMeterInformation();

                                    if (item.DateString.Length == 8)
                                    {
                                        CultureInfo provider = CultureInfo.InvariantCulture;
                                        string format = "yyyyMMdd";
                                        DateTime result = DateTime.ParseExact(item.DateString, format, provider);
                                        item.ElectricityMeterReadingDateTaken = result;
                                    }
                                    else
                                    {
                                        item.ElectricityMeterReadingDateTaken = Convert.ToDateTime(item.DateString);
                                    }
                                    elecinfo = item;
                                    elecinfo.RCSApplicationStatusId = appli.Id;
                                    elecinfo.RCSApplicationStatus = appli;
                                    cxt.ElectricityMeterInformations.Add(elecinfo);
                                    cxt.SaveChanges();
                                }
                            }
                        }

                        if (capture.WaterSessionList != null && capture.WaterSessionList != "")
                        {
                            List<WaterMeterInformation> WaterMeterInfo = JsonConvert.DeserializeObject<List<WaterMeterInformation>>(capture.WaterSessionList);

                            if (WaterMeterInfo.Count > 0)
                            {
                                foreach (var item in WaterMeterInfo)
                                {
                                    WaterMeterInformation waterinfo = new WaterMeterInformation();
                                    CultureInfo provider = CultureInfo.InvariantCulture;

                                    //if (item.DateString.Length == 8)
                                    //{
                                    //    string format = "yyyyMMdd";
                                    //    DateTime result = DateTime.ParseExact(item.DateString, format, provider);
                                    //    item.WaterMeterReadingDateTaken = result;
                                    //}
                                    //else
                                    //{
                                    //    item.WaterMeterReadingDateTaken = Convert.ToDateTime(item.DateString);
                                    //}

                                    waterinfo = item;
                                    waterinfo.RCSApplicationStatusId = appli.Id;
                                    waterinfo.RCSApplicationStatus = appli;
                                    cxt.WaterMeterInformations.Add(waterinfo);
                                    cxt.SaveChanges();
                                }
                            }
                        }


                        //var EI = (List<ElectricityMeterInformation>)TempData["ep"];
                        //if (EI != null)
                        //{
                        //    foreach (var item in EI)
                        //    {
                        //        ElectricityMeterInformation elecinfo = new ElectricityMeterInformation();

                        //        elecinfo = item;
                        //        elecinfo.RCSApplicationStatusId = appli.Id;
                        //        elecinfo.RCSApplicationStatus = appli;
                        //        cxt.ElectricityMeterInformations.Add(elecinfo);
                        //        cxt.SaveChanges();
                        //    }
                        //}
                        //var WI = (List<WaterMeterInformation>)TempData["wp"];
                        //if (WI != null)
                        //{
                        //    foreach (var item in WI)
                        //    {
                        //        WaterMeterInformation waterinfo = new WaterMeterInformation();

                        //        waterinfo = item;
                        //        waterinfo.RCSApplicationStatusId = appli.Id;
                        //        waterinfo.RCSApplicationStatus = appli;
                        //        cxt.WaterMeterInformations.Add(waterinfo);
                        //        cxt.SaveChanges();
                        //    }
                        //}
                        var ActivityTrackerMessage = cxt.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ConevayncerCapturesNewApplication).Description.ToString()/* + DecisionType*/;
                        var RCSHistoryLog = new RCSApplicationHistoryLog
                        {
                            RCSApplicationStatusId = appli.Id,
                            AuditAction = ActivityTrackerMessage,
                            UserId = Customer.Id,
                            CreatedDateTime = DateTime.Now,
                            IsActive = true
                        };
                        cxt.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                        cxt.SaveChanges();

                        WalkInApplicantDetails WIA = new WalkInApplicantDetails();
                        WIA = capture.WalkInApplicant;
                        WIA.RCSApplicationStatusId = appli.Id;
                        cxt.WalkInApplicantDetails.Add(WIA);
                        cxt.SaveChanges();



                        Email SendMail = new Email();
                        string attorneyemail = "";
                        if (CAD.Email == null)
                        {
                            attorneyemail = "suhail.dada@xetgroup.com";

                        }
                        else
                        {
                            attorneyemail = CAD.Email;
                        }

                        string attorneyname = CAD.ContactPerson1;
                        var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.ApplicationCapturedSuccessfully).FirstOrDefault();

                        string emailbody = getemailbody.Description + appli.ApplicationReferenceNumber;
                        SendMail.GenerateEmail(attorneyemail, "RCS- New Online Application Submission",
                                      emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);



                        var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EmailNewApplication).Description.ToString() + emailbody;
                        var RCSHistoryLog2 = new RCSApplicationHistoryLog
                        {
                            RCSApplicationStatusId = appli.Id,
                            AuditAction = ActivityTrackerMessage2,
                            UserId = Customer.Id,
                            CreatedDateTime = DateTime.Now,
                            IsActive = true
                        };
                        cxt.RCSApplicationHistoryLogs.Add(RCSHistoryLog2);
                        cxt.SaveChanges();
                        //List<RCSDepartmentType> DepartmentList = new List<RCSDepartmentType>();
                        //DepartmentList = db.RCSDepartmentTypes.ToList();
                        //CaptureController c = new CaptureController();

                        ////Insert all departments
                        //foreach (var department in DepartmentList)
                        //{
                        //    DepartmentsApproval depApprovals = new DepartmentsApproval();
                        //    List<DepartmentsApproval> depList = new List<DepartmentsApproval>();
                        //    var ApprovrcsType = Statuses.Where(x => x.Key == StatusKeys.PendingDocumentsApproval).FirstOrDefault();
                        //    depApprovals.StatusId = ApprovrcsType.Id;
                        //    depApprovals.DepartmentId = department.Id;
                        //    depApprovals.CapturedDate = DateTime.Now;
                        //    depApprovals.FailureReason = "Failure at " + department.Name;
                        //    depApprovals.RCSApplicationStatusId = Convert.ToInt32(appli.Id);

                        //    if (department.Key == RCSDepartmentTypeKeys.SundryAccountSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(true, false, false, false, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.LegalSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, true, false, false, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.CreditControlSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, false, true, false, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.BillingSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, false, false, true, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.EndowmentSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, false, false, false, true, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }

                        //    depList.Add(depApprovals);
                        //    c.departmentsApprovals(depList);
                        //    //break;
                        //}
                        string pgMerchantId = "pg_crm_app_rcs";
                        string voteNumber = appli.TransferInformation.RatesNumber;
                        voteNumber = "RCS " + voteNumber;
                        string pgMerchantReference = appli.ApplicationReferenceNumber;
                        string pgMerchantDescription = "RCS Application Fee";
                        var ApplicationFeeAmount = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSApplicationFeeAmt).Value;

                        string Amount = ApplicationFeeAmount;
                        //string pgEmail = "Sashen.moodley@xetgroup.com";
                        //string pgMobile = "0846666435";

                        string pgEmail = null;
                        string pgMobile = null;
                        if (!string.IsNullOrEmpty(appli.Customer.SystemUser.EmailAddress))
                        {
                            pgEmail = appli.Customer.SystemUser.EmailAddress;
                        }

                        if (!string.IsNullOrEmpty(appli.Customer.SystemUser.MobileNumber))
                        {
                            pgMobile = appli.Customer.SystemUser.MobileNumber;
                        }
                        //pgMobile = "0846666435";



                        //string customerFirstName = "Sashen";
                        string customerFirstName = appli.Customer.FirstName;
                        //string customerLastName = "Moodley";
                        string customerLastName = appli.Customer.LastName;
                        string returnUrl = capture.returnurl;
                        //returnUrl = "http://localhost:3450/Capture/Capture";
                        string adhocRef1 = Convert.ToString(RCSAppID);

                        string adhocRef2 = "";
                        string adhocRef3 = "";
                        string adhocRef4 = "";
                        string adhocRef5 = "";
                        returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
                        //returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
                        string amt = Amount.Replace('.', ',');
                        decimal conAmt = Convert.ToDecimal(amt);
                        returnUrl = returnUrl + "/ReturnBackUrl";
                        //Format  parameters into Single Delimited String
                        string enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}",
                            pgMerchantId, voteNumber, pgMerchantReference, pgMerchantDescription, Amount,
                            pgEmail, pgMobile, customerFirstName, customerLastName, returnUrl, adhocRef1, adhocRef2, adhocRef3, adhocRef4, adhocRef5);
                        //Encrypt the Single String to and Encrypted string e
                        var e = new AesCrypto(encp).Encrypt(enc);
                        AppSetting PGDomain = db.AppSettings.FirstOrDefault(o => o.Key == PaymentGatewayKeys.PGDomain);
                        AppSetting PGEnvironment = db.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSPaymentGateway);

                        var baseFormat = PGDomain.Value + "PaymentGateway/" + PGEnvironment.Value + "?q=" + e;

                        //return Redirect(baseFormat);
                        var referenceType = cxt.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                        var application = cxt.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                        var returnUrl2 = "Test"/*success.ToString(CultureInfo.InvariantCulture)*/;
                        return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = baseFormat, rcsappId = appli.Id })));



                    }

                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
            return RedirectToAction("Index", "RCSApplication");
        }
        #endregion
        #region ViewAllDetails
        [DecryptParameter]
        public ActionResult Details(int? refNo)
        {
            if (refNo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var _context = new eServicesDbContext())
            {
                var vm = new ApplicationDetailsViewModel();

                PropertyLeaseApplication propertyLeaseApplication = null;
                MatchedUnits matchedUnits = null;
                ApplicantUnit applicantUnit = null;
                PreferredComplexArea preferredComplexArea1 = null;
                PreferredComplexArea preferredComplexArea2 = null;
                List<PLMApplicationHistortyLog> pLMApplicationHistortyLogs = null;
                EnvisagedUsage envisagedUsage = null;
                HumanEHCOptions humanEHCOptions = null;
                PurchaserType purchaserType = null;
                IncomeSource incomeSource = null;
                List<DepartmentalComments> DepartmentalComments = null;
                CommitteeOutcome committeeOutcome = null;
                List<HoD> HOD = null;
                OccupationType occupationType = null;
                Units units = null;
                IdentificationType identification = null;
                CompanyType companyType = null;
                ApplicantJoint secondApplicant = null;
                TitleType titleType = null;
                Customer HousingSupervisor = null;
                Customer LettingOfficer = null;
                ApplicationAllocatedProperty ekurhuleniHousingCompany = null;
               
                var NotMatched = "Unit Not Matched";
                ApplicationAllocatedProperty EmptyUnit = new ApplicationAllocatedProperty();

                propertyLeaseApplication = _context.PropertyLeaseApplications.Include(r => r.PurchaserType).Include(x=>x.SecAppTitleType).Include(r => r.HumanEHCOptions).Include(r => r.IncomeSource).Include(r => r.TitleType).Include(r => r.IdentificationType).Where(x => x.Id == refNo).FirstOrDefault();
                applicantUnit = _context.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id && x.IsActive && !x.IsDeleted) ?? null;
                purchaserType = _context.PurchaserType.Where(x => x.Id == propertyLeaseApplication.PurchaserTypeId).FirstOrDefault();
                if (applicantUnit != null)
                {
                    matchedUnits = _context.MatchedUnits.FirstOrDefault(x => x.Id == applicantUnit.MatchedID);
                    units = _context.Units.FirstOrDefault(x => x.Id == matchedUnits.UnitsId);
                    ekurhuleniHousingCompany = _context.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == matchedUnits.ApplicationAllocatedPropertyId);
                    var LF = GetBackOfficeId(_context, (int)matchedUnits.PropertyLeaseApplicationId, true);
                    var HS = GetBackOfficeId(_context, (int)applicantUnit.PropertyLeaseApplicationId, false);

                    var HousingSuperviso = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                    var LettingOffice = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                    LettingOfficer = LF.Id != 0 ? LF : _context.Customers.Include(r=>r.SystemUser).FirstOrDefault(x => x.Id == LettingOffice);
                    HousingSupervisor = HS.Id != 0 ? HS : _context.Customers.Include(r => r.SystemUser).FirstOrDefault(x => x.Id == HousingSuperviso);
                }
                if (propertyLeaseApplication.PurchaserTypeId == (_context.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.Company).FirstOrDefault().Id))
                {
                    ViewBag.TenantType = "Company";
                    ViewBag.ApplicationType = "Company";

                    int cop = Convert.ToInt32(propertyLeaseApplication.CompanyType);
                    companyType = _context.companyTypes.Where(x => x.Id == cop).FirstOrDefault();
                    occupationType = _context.OccupationTypes.Where(x => x.Id == propertyLeaseApplication.BType).FirstOrDefault();
                    envisagedUsage = _context.envisagedUsages.Where(x => x.Id == propertyLeaseApplication.BUsage).FirstOrDefault();
                    DepartmentalComments = _context.DepartmentalComments.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).ToList();
                    committeeOutcome = _context.committeeOutcomes.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).FirstOrDefault();
                    HOD = _context.HoDs.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).ToList();

                }
                else if (propertyLeaseApplication.PurchaserTypeId == (_context.PurchaserType.Where(x => x.Key == PurchaserTypeKeys.NaturalPerson).FirstOrDefault().Id))
                {
                    ViewBag.TenantType = "Individual";
                    ViewBag.ApplicationType = "Individual";

                    identification = _context.IdentificationTypes.Where(x => x.Id == propertyLeaseApplication.IdentificationTypeId).FirstOrDefault();
                    preferredComplexArea1 = _context.PreferredComplexAreas.Where(x => x.Id == propertyLeaseApplication.PreferredComplexAreaId).FirstOrDefault();
                    preferredComplexArea2 = _context.PreferredComplexAreas.Where(x => x.Id == propertyLeaseApplication.PreferredComplexArea2Id).FirstOrDefault();
                    pLMApplicationHistortyLogs = _context.PLMApplicationHistortyLogs.Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id).OrderBy(x=>x.CreatedDateTime).Include(r=>r.CreatedBySystemUser).Include(r=>r.User).ToList();
                    humanEHCOptions = _context.humanEHCOptions.Where(x => x.Id == propertyLeaseApplication.HumanEHCOptionsId).FirstOrDefault();
                    incomeSource = _context.IncomeSources.Where(x => x.Id == propertyLeaseApplication.IncomeSourceId).FirstOrDefault();
                    secondApplicant = _context.ApplicantJoints.Include(r=>r.TitleType).FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id);

                    if (incomeSource != null)
                    {
                        ViewBag.Incomesource = incomeSource.SourceOfIncome;
                    }
                }

                if (ekurhuleniHousingCompany == null)
                {
                    EmptyUnit.BuildingName = NotMatched.ToString();
                    EmptyUnit.SpaceUnitNumber = NotMatched.ToString();
                    EmptyUnit.StreetName = NotMatched.ToString();
                    EmptyUnit.Township = NotMatched.ToString();
                    EmptyUnit.Postal = NotMatched.ToString();
                    EmptyUnit.LettingRequirements = NotMatched.ToString();
                    ViewBag.UnitAvailable = "1";
                }

                vm.ApplicationAllocatedProperty = ekurhuleniHousingCompany ?? EmptyUnit;
                vm.HousingSupervisor = HousingSupervisor;
                vm.LettingOfficer = LettingOfficer;
                vm.ApplicantUnit = applicantUnit;
                vm.PropertyLeaseApplication = propertyLeaseApplication;
                vm.MatchedUnits = matchedUnits;
                vm.PreferredComplexArea = preferredComplexArea1;
                vm.PreferredComplexAreaSecondOpt = preferredComplexArea2;
                vm.PLMApplicationHistortyLogList = pLMApplicationHistortyLogs;
                vm.EnvisagedUsage = envisagedUsage;
                vm.HumanEHCOptions = humanEHCOptions;
                vm.PurchaserType = purchaserType;
                vm.IncomeSource = incomeSource;
                vm.DepartmentalCommentsList = DepartmentalComments;
                vm.hodList = HOD;
                vm.OccupationType = occupationType;
                vm.Units = units ?? null;
                vm.IdentificationType = identification;
                vm.CompanyType = companyType;
                vm.ApplicantJoint = secondApplicant;
                vm.TitleType = titleType;

                // Documents required variables
                var documentCheckLists = new List<DocumentCheckList>();
                var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();
                var customer = db.Customers.Include(s => s.SystemUser).Include(s => s.Status).Include(s => s.CustomerType).FirstOrDefault(c => c.Id == propertyLeaseApplication.CustomerId);

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
                var dvmDebitOrder = new DocumentsViewModel();
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
                MatchingHelper.DocumentDebitOrder(dvmDebitOrder, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
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
                vm.DocumentDebitOrder = dvmDebitOrder;
                vm.MonthlyIncomeList = _context.MonthlyIncomes.Where(s=>s.PropertyLeaseApplicationId==propertyLeaseApplication.Id).ToList();
                vm.MonthlyExpenseList = _context.MonthlyExpenses.Where(s=>s.PropertyLeaseApplicationId==propertyLeaseApplication.Id).ToList();

                var propertyLeaseActionComments = _context.propertyLeaseActionComments.Include(r => r.CreatedBySystemUser).Include(r => r.Clerk).Where(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id && x.LeaseWarningLetter).ToList();

                var documentLeaseWarningLetterDetails = new DepartmentsApprovalViewModel
                {
                    DocumentsViewModel = dvmLeaseWarningLetter,
                    WarningLetterReasons = propertyLeaseActionComments
                };
                vm.DocumentLeaseWarningLetterDetails = documentLeaseWarningLetterDetails;

                return View(vm);
            }
            
        }

        [HttpGet]
        public ActionResult PropertyLeaseApplicationNotice()
        {

            Initialise();
            if (Session["Display"] != null)
            {
                Session["Display"] = null;
                ViewBag.DisplaySwal = "True";
                if (Session["Message"] != null)
                {
                    ViewBag.MessageBodySwal = Session["Message"].ToString();
                    //ViewBag.MessageTitle3 = Session["Message"].ToString();

                    Session["Message"] = null;
                }
            }
            else
            {
                ViewBag.DisplaySwal = null;
            }

            var vm = new DepartmentsApprovalViewModel();
            LeaseDetails leaseDetails = new LeaseDetails();
            vm.LeaseDetails = leaseDetails;
            return View(vm);

        }

        [HttpPost]
        public ActionResult PropertyLeaseApplicationNotice(string ApprovalStatusddl)
        {
            using (var _context = new eServicesDbContext())
            {
                Initialise();
                var CustomerId = Customer.Id;
                var upperreref = ApprovalStatusddl.ToUpper();
                var application = _context.PropertyLeaseApplications.FirstOrDefault(x => x.CustomerId == CustomerId && x.ApplicationReferenceNumber == upperreref) ?? null;
                var leaseInfo = application != null ? _context.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.PropertyLeaseApplicationId == application.Id).FirstOrDefault() : null;

                if (application != null)
                {
                    if (leaseInfo != null)
                    {
                        return RedirectToAction("ApplicationLeaseServeNotice", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + leaseInfo.Id.ToString()) });
                    }
                    else
                    {
                        Session["Message"] = "The Reference Number " + ApprovalStatusddl + " Is Inactive, Try Another Reference!";
                        Session["Display"] = "Display";
                        return RedirectToAction("PropertyLeaseApplicationNotice", "propertyLeaseApplication");
                    }
                }
                else
                {
                    Session["Message"] = "The Reference Number You Have Entered Is Invalid, Try Again!";
                    Session["Display"] = "Display";
                }

                return RedirectToAction("PropertyLeaseApplicationNotice", "propertyLeaseApplication");
            }

        }

        #region Lease Termination OnLoad
        [DecryptParameter]
        public ActionResult ApplicationLeaseServeNotice(int? id, string refno)
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

            if ((lease.Status.Key == StatusKeys.AwaitingRiskAssessment) ||(lease.Status.Key == StatusKeys.AwaitingRenewalDocuments) || (lease.Status.Key == StatusKeys.ApplicationUpForRenewalAtThreeMonths) || (lease.Status.Key == StatusKeys.InAwaitingPropertyManagersReview) || (lease.Status.Key == StatusKeys.InAwaitingRevenueManagersReview) || (lease.Status.Key == StatusKeys.AwaitingTenantAcceptance))
                return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = rcsApps.Id, appId = lease.Id });

            ViewBag.Id = lease.Id;
            ViewBag.PropId = rcsApps.Id;

            if (User.IsInRole("Area Manager"))
            {
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved/* || x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReAllocate*/).OrderBy(x => x.Name), "Key", "Name");

            }
            else
            {

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.LeaseNotRenuewed || x.Key == RCSActionTypeKeys.EndOfLeasePeriod60M || x.Key == RCSActionTypeKeys.TenantNotice).OrderBy(x => x.Name), "Key", "Name");

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

                //MatchingHelper.DocumentTerminationValidation(dvm, context, customer.Id, customer.Id, documentReferenceType.Id, application.Id, "", rcsApps.Id, true);







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
                bool isCustomerNotice = false;
                // For getting termination letter and banking details
                if (User.IsInRole("Customers"))
                {
                    isCustomerNotice = true;
                }
                MatchingHelper.DocumentTerminationLetterAndProofBanking(dvm, context, customer.Id, customer.Id, (int)documentReferenceType.Id, (int)application.Id, "", rcsApps.Id, true, isCustomerNotice);
                vm.DocumentsViewModel = dvm;

                vm.Customer = customer;
                vm.PropertyLeaseApplications = rcsApps;

                LeaseTermination leaseTermination = new LeaseTermination();
                vm.LeaseTermination = leaseTermination;

                ViewBag.LeaseId = db.LeaseDetails.Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).OrderByDescending(x => x.Id).FirstOrDefault().Id;
                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;
                ViewBag.Date = null;

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

        #region Lease Termination
        [DecryptParameter]
        [HttpPost]
        public ActionResult ApplicationLeaseServeNotice(DepartmentsApprovalViewModel vm, int? id, string ApprovalStatusddl, string ServeNoticeDate, string TerminationReason)
        {
            using (var _context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var userID = Customer;
                    var Keys = _context.Status;
                    var LeaseApplication = _context.LeaseDetails.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();

                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);

                    //Saving Termination reason
                    PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                    {
                        PropertyLeaseApplicationId = LeaseApplication.PropertyLeaseApplicationId,
                        RejectReason = TerminationReason,
                        LeaseTerminationLetter = true,
                        CreatedDateTime = DateTime.Now,
                        ClerkId = Customer.Id
                    };
                    _context.propertyLeaseActionComments.Add(comments);
                    _context.SaveChanges();

                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantNotice).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(_context, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                    MatchingHelper.ServeNoticeAppllicationDate(_context, LeaseApplication.PropertyLeaseApplicationId, Convert.ToDateTime(ServeNoticeDate));

                    LeaseApplication.NoticeDate = Convert.ToDateTime(ServeNoticeDate.ToString());
                    _context.Entry(LeaseApplication).State = EntityState.Modified;
                    _context.SaveChanges();

                    //Send e-mail and SMS notification
                    int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ServeNotice).Id;
                    EmailHelper.CustomerEmailNotification(db, LeaseApplication.PropertyLeaseApplicationId, emailboodyId);

                    
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Terminations).FirstOrDefault();
                    //                                                               1     2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17      18   19
                    EHCRoundRobin((int)LeaseApplication.PropertyLeaseApplicationId, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                    var BackOffice =GetBackOfficeId(db, LeaseApplication.PropertyLeaseApplicationId, true);

                    var result = BackOffice.UserFullName != null ? BackOfficeNotification(LeaseApplication.PropertyLeaseApplicationId, BackOffice.Id, ResponsibilityTypeId.Name) : true;
                    Session["TenantServeNoticeSession"] = string.Format($"Serve notice submitted successfully for application ,{LeaseApplication.LeaseReferenceNo}");
                    return RedirectToAction("ServeNotice", "propertyLeaseApplication");
                }
                catch (Exception Io)
                {
                    return RedirectToAction("Login", "propertyLeaseApplication");
                }
            }
        }
        #endregion

        [HttpGet]
        public ActionResult Termination()
        {
            Initialise();
            if (Session["Display"] != null)
            {
                Session["Display"] = null;
                ViewBag.DisplaySwal = "True";
                if (Session["Message"] != null)
                {
                    ViewBag.MessageBodySwal = Session["Message"].ToString();
                    //ViewBag.MessageTitle3 = Session["Message"].ToString();

                    Session["Message"] = null;
                }
            }
            else
            {
                ViewBag.DisplaySwal = null;
            }
            if (Session["LeaseTerminationLOSession"] != null)
            {
                var value = Session["LeaseTerminationLOSession"].ToString();
                Session["LeaseTerminationLOSession"] = null;
                ViewBag.LeaseTerminationLOSession = value;
            }
            Session["LeaseTerminationLOSession"] = null;

            var vm = new DepartmentsApprovalViewModel();
            LeaseDetails leaseDetails = new LeaseDetails();
            vm.LeaseDetails = leaseDetails;
            return View(vm);
            
        }
        [HttpPost]
        public ActionResult Termination(string ApprovalStatusddl)
        {
            using ( var _context = new eServicesDbContext())
            {
                var getApp = ApprovalStatusddl.Substring(0, 3);
                var getLease = ApprovalStatusddl.Substring(0, 4);

                if (getLease == "PLM-")
                {
                    var leaseInfo = _context.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.LeaseReferenceNo == ApprovalStatusddl && !x.IsDeleted && x.IsNew).FirstOrDefault();
                    if (leaseInfo != null)
                    {
                        if (leaseInfo.StatusId == (_context.Status.FirstOrDefault(r => r.Key == StatusKeys.DeactivateLeaseNewCaptured).Id))
                        {
                            Session["Message"] = "The Reference Number " + ApprovalStatusddl + " Is Inactive, Try Another Reference!";

                            Session["Display"] = "Display";
                            return RedirectToAction("Termination", "propertyLeaseApplication");
                        }
                        else if (leaseInfo.StatusId == (_context.Status.FirstOrDefault(r => r.Key == StatusKeys.TerminationDateIssued).Id))
                        {
                            Session["Message"] = "Renewal For " + ApprovalStatusddl + " Has Been Issued!";

                            Session["Display"] = "Display";
                            return RedirectToAction("Termination", "propertyLeaseApplication");
                        }
                        else
                            return RedirectToAction("LeaseTerminationValidation", "leaseDetails", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + leaseInfo.Id.ToString()) });
                    }
                    else
                    {
                        Session["Message"] = "The Reference Number " + ApprovalStatusddl + " Is Inactive, Try Another Reference!";
                        
                        Session["Display"] = "Display";
                        return RedirectToAction("Termination", "propertyLeaseApplication");
                    }
                }
                else if ((getApp == "PLM" || getApp == "EHC"))
                {
                    var PropertyInfo = _context.PropertyLeaseApplications.Where(x => x.ApplicationReferenceNumber == ApprovalStatusddl && !x.IsDeleted).FirstOrDefault();

                    LeaseDetails leaseInfo = null;
                    if (PropertyInfo != null)
                    {
                        leaseInfo = _context.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.PropertyLeaseApplicationId == PropertyInfo.Id && !x.IsDeleted && x.IsNew).FirstOrDefault();
                    }
                    if (leaseInfo!=null)
                    {
                        return RedirectToAction("LeaseTerminationValidation", "leaseDetails", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + leaseInfo.Id.ToString()) });
                    }
                    else
                    {
                        Session["Message"] = "The Reference Number " + ApprovalStatusddl + " Did Not Fid A Match, Try Again!";
                        Session["Display"] = "Display";
                        return RedirectToAction("Termination", "propertyLeaseApplication");
                    }
                }
                else
                {
                    Session["Message"] = "The Reference Number You Have Entered Is Invalid, Try Again!";
                    Session["Display"] = "Display";
                }

                return RedirectToAction("Termination", "propertyLeaseApplication");
            }
               
        }
        
        [HttpGet]
        public ActionResult ApplicationEviction()
        {
            Initialise();
            if (Session["Display"] != null)
            {
                Session["Display"] = null;
                ViewBag.DisplaySwal = "True";
                if (Session["Message"] != null)
                {
                    ViewBag.MessageBodySwal = Session["Message"].ToString();
                    //ViewBag.MessageTitle3 = Session["Message"].ToString();

                    Session["Message"] = null;
                }
            }
            else
            {
                ViewBag.DisplaySwal = null;
            }


            var vm = new DepartmentsApprovalViewModel();
            LeaseDetails leaseDetails = new LeaseDetails();
            vm.LeaseDetails = leaseDetails;
            if (Session["EvictionDetailsSession"] != null)
            {
                var value = Session["EvictionDetailsSession"].ToString();
                Session["EvictionDetailsSession"] = null;
                ViewBag.EvictionDetailsSession = value;
            }
            Session["EvictionDetailsSession"] = null;
            
            return View(vm);
            
        }
        [HttpPost]
        public ActionResult ApplicationEviction(string ApprovalStatusddl)
        {
            using (var _context = new eServicesDbContext())
            {
                var application = _context.PropertyLeaseApplications.Where(x => x.ApplicationReferenceNumber == ApprovalStatusddl && !x.IsDeleted).FirstOrDefault();
                if (application != null)
                {
                    return RedirectToAction("CaptureEvictionDetails", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + application.Id.ToString()) });
                }
                else
                {
                    Session["Message"] = "The Reference Number " + ApprovalStatusddl + " Did Not Fid A Match, Try Again!";
                    Session["Display"] = "Display";
                    return RedirectToAction("ApplicationEviction", "propertyLeaseApplication");
                }
            }

        }






        #region Tenant Account Balance Validation OnLoad
        [DecryptParameter]
        public ActionResult TenantAccountValidation(int? id)
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

                var PropertyCoomments = db.propertyLeaseActionComments.Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).ToList();
                foreach (var item in PropertyCoomments)
                {
                    item.Data = item.ExitInspection == true ? "Exit Inspection" : "";
                }
                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description,
                    PropertyLeaseActionCommentList = PropertyCoomments != null ? PropertyCoomments : null
                };

                Entity entity = null;
                Agent agent = null;

                vm.Customer = customer;
                vm.PropertyLeaseApplications = rcsApps;

                LeaseTermination leaseTermination = new LeaseTermination();
                vm.LeaseTermination = leaseTermination;
                ViewBag.RejectComment = "";
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

                //start
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                bool IsUpload = true;
                var returnUrl = "";
                MatchingHelper.DocumentTenantAccountValidation(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, returnUrl, rcsApps.Id, IsUpload);

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
            return View(rcsApps);
        }
        #endregion

        #region Tenant Account Balance Validation
        [DecryptParameter]
        [HttpPost]
        public ActionResult TenantAccountValidation(int? id, string ApprovalStatusddl, string RejectComment)
        {
            using (var _context = new eServicesDbContext())
            {
                Initialise();
                var userID = Customer;
                var Keys = _context.Status;
                var LeaseApplication = _context.LeaseDetails.Include(r=>r.Status).Where(x => x.Id == id && !x.IsDeleted && x.IsNew).FirstOrDefault();
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);


                var LeaseId = id;
                if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                {
                    if (LeaseApplication.Status.Key==StatusKeys.TenantNotice)
                    {
                        int emailboodyId = _context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.TerminationApprovedForTenant).Id;
                        EmailHelper.CustomerEmailNotification(_context, (int)LeaseApplication.PropertyLeaseApplicationId, emailboodyId);
                    }
                    else
                    {
                        int emailboodyId = _context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.TerminationAccepted).Id;
                        EmailHelper.CustomerEmailNotification(_context, (int)LeaseApplication.PropertyLeaseApplicationId, emailboodyId);
                    }

                    var item = _context.LeaseTerminations.OrderByDescending(x=>x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == LeaseApplication.PropertyLeaseApplicationId);

                    LeaseApplication.EndDate = item.TerminationDate;
                    LeaseApplication.TerminationNotice = item.TerminationDate.AddMonths(-1).AddDays(1);
                    LeaseApplication.RenewalNotice = item.TerminationDate.AddMonths(-3);
                    LeaseApplication.PeriodInMonths = 0;
                    

                    MatchingHelper.SaveLeaseDetaisInfo(_context, LeaseApplication);
                    MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.Where(x => x.Key == StatusKeys.TerminationApproved).FirstOrDefault().Id, (int)LeaseId);

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.TerminationValidation).FirstOrDefault();
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)LeaseApplication.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, activeDirectoryOn);
                    int AwaitingExitInspection = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingExitInspection).Id;
                    //EHCRoundRobin((int)LeaseApplication.PropertyLeaseApplicationId, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                    EHCRoundRobin((int)LeaseApplication.PropertyLeaseApplicationId, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                    MatchingHelper.ChangeApplicationStatus(_context, AwaitingExitInspection, LeaseApplication.PropertyLeaseApplicationId);

                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TerminationApproved).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    Session["BOAccountValidationSession"] = string.Format($"Account validation approved for application ref ,{LeaseApplication.LeaseReferenceNo}");
                }
                else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
                {
                    if (LeaseApplication.Status.Key == StatusKeys.TenantNotice)
                    {
                        int emailboodyId = _context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.TerminationRejectedForTenant).Id;
                        EmailHelper.CustomerEmailNotification(_context, (int)LeaseApplication.PropertyLeaseApplicationId, emailboodyId);
                    }
                    else
                    {
                        int emailboodyId = _context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.TerminationRejectedFromLO).Id;
                        EmailHelper.CustomerEmailNotification(_context, (int)LeaseApplication.PropertyLeaseApplicationId, emailboodyId);
                    }

                    var item = _context.LeaseTerminations.FirstOrDefault(x => x.PropertyLeaseApplicationId == LeaseApplication.PropertyLeaseApplicationId);
                    //_context.LeaseTerminations.Remove(item);
                    _context.SaveChanges();

                    MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.Where(x => x.Key == StatusKeys.TerminationReject).FirstOrDefault().Id, (int)LeaseId);

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.TerminationValidation).FirstOrDefault();
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)LeaseApplication.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, activeDirectoryOn);


                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TerminationRejected).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    Session["BOAccountValidationSession"] = string.Format($"Account validation approved for application ref ,{LeaseApplication.LeaseReferenceNo}");
                }
                return RedirectToAction("PropertyLeaseApplicationTerminations");

            }


        }
        #endregion
        




        #region WaitingListRe-Wail OnLoad
        [DecryptParameter]
        public ActionResult WaitingListValidation(int? PropertyId)
        {

            eServicesDbContext context = new eServicesDbContext();
            Initialise();
            PropertyLeaseApplication rcsApps = null;
            LeaseDetails lease = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status).Include(r => r.PurchaserType)
              .Where(x => x.Id == PropertyId).FirstOrDefault();

            var vm = new DepartmentsApprovalViewModel
            {
                LeaseDetails = lease,
                PropertyLeaseApplications = rcsApps
            };

            ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.PlmYes || x.Key == RCSActionTypeKeys.PlmNo).OrderByDescending(x => x.Name), "Key", "Name");

            return View(vm);
        }
        #endregion

        #region WaitingListRe-Wail Validation
        [DecryptParameter]
        [HttpPost]
        public ActionResult WaitingListValidation(int? PropertyId, string ApprovalStatusddl)
        {
            if (PropertyId == 0) throw new Exception("Invalid application.");
            using (var _context = new eServicesDbContext())
            {
                Initialise();
                if (ApprovalStatusddl == RCSActionTypeKeys.PlmYes)
                {
                    var findItem = _context.waitingListQues.FirstOrDefault(x => x.PropertyLeaseApplicationId == (int)PropertyId);
                    findItem.IsReListed = false;
                    findItem.QueueDate = DateTime.Now;
                    _context.Entry(findItem).State = EntityState.Modified;
                    _context.SaveChanges();
                    MatchingHelper.ChangeApplicationStatus(_context, _context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingUnitOffers).Id, (int)PropertyId);
                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.WaitingListReEntry).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(_context, PropertyId, ActivityTrackerMessage, custmusers.Id);
                    MatchingHelper.MarkQueueAsREListed(_context, (int)PropertyId);
                    int emailboodyId = _context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.plm_re_list_to_queue).Id;
                    EmailHelper.CustomerEmailNotification(_context, (int)PropertyId, emailboodyId);
                    MatchingHelper.MatchUnitParallelProcessor(_context);
                }
                else if (ApprovalStatusddl == RCSActionTypeKeys.PlmNo)
                {
                    int QueueId = _context.waitingListQues.FirstOrDefault(x => x.PropertyLeaseApplicationId == (int)PropertyId).Id;
                    MatchingHelper.RemoveApplicationFromWaitingList(_context, QueueId);
                    MatchingHelper.ChangeApplicationStatus(_context, _context.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicationDiscardedNoUnitAvailable).Id, (int)PropertyId);
                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.WaitingListExit).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(_context, PropertyId, ActivityTrackerMessage, custmusers.Id);
                    int emailboodyId = _context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.plm_remove_from_queue).Id;
                    EmailHelper.CustomerEmailNotification(_context, (int)PropertyId, emailboodyId);
                }
                return RedirectToAction("Inbox", "PropertyLeaseApplication");
            }
        }
        #endregion





        #region Property Eviction Validation OnLoad
        [DecryptParameter]
        public ActionResult PropertyEvictionValidation(int? id)
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
            LeaseTermination termination = null;

            lease = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.ModifiedBySystemUser)
              .Include(r => r.Status)
              .Include(r => r.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();

            termination = db.LeaseTerminations.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
             .Include(r => r.ModifiedBySystemUser).Include(r => r.Status)
             .Where(x => x.LeaseDetailsId == lease.Id).FirstOrDefault();

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();

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
                List<Attachments> attachments = new List<Attachments>();
                var atth = db.Attachments.FirstOrDefault();
                attachments.Add(atth);

                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    LeaseTermination = termination,
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description


                };

                Entity entity = null;
                Agent agent = null;

                vm.Customer = customer;
                vm.PropertyLeaseApplications = rcsApps;

                ViewBag.RejectComment = "";
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

                //start
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                bool IsUpload = true;
                var returnUrl = "";
                MatchingHelper.DocumentPropertyEvictionValidation(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, returnUrl, rcsApps.Id, IsUpload);

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
            return View(rcsApps);
        }
        #endregion

        #region Property Eviction Validation
        [DecryptParameter]
        [HttpPost]
        public ActionResult PropertyEvictionValidation(int? id, string ApprovalStatusddl, string RejectComment)
        {

            using (var _context = new eServicesDbContext())
            {
                Initialise();
                var userID = Customer;
                var Keys = _context.Status;
                var LeaseApplication = _context.LeaseDetails.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();

                PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                {
                    PropertyLeaseApplicationId = LeaseApplication.PropertyLeaseApplicationId,
                    RejectReason = RejectComment,
                    PropertyEvictionValidation = true
                };
                _context.propertyLeaseActionComments.Add(comments);
                _context.SaveChanges();

                var LeaseId = id;
                if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                {
                    MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.Where(x => x.Key == StatusKeys.AwaitingCommitteEviction).FirstOrDefault().Id, (int)LeaseId);

                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ProopertyEvictionAprove).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                }
                else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
                {
                    MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.Where(x => x.Key == StatusKeys.AwaitingCommitteEviction).FirstOrDefault().Id, (int)LeaseId);


                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ProopertyEvictionAprove).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                }
                //MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.Where(x => x.Key == StatusKeys.AwaitingTenantAccountBalanceReview).FirstOrDefault().Id, (int)LeaseId);
                return RedirectToAction("PropertyLeaseApplicationTerminations");

            }


        }
        #endregion




        #region Eviction Committee OnLoad
        [DecryptParameter]
        public ActionResult EvictionCommitteeOutcome(int? id)
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
            LeaseTermination termination = null;

            lease = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.ModifiedBySystemUser)
              .Include(r => r.Status)
              .Include(r => r.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();

            termination = db.LeaseTerminations.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
             .Include(r => r.ModifiedBySystemUser).Include(r => r.Status)
             .Where(x => x.LeaseDetailsId == lease.Id).FirstOrDefault();

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();

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
                //var attachments = db.Attachments.Where(x => x.PropertyLeaseApplicationId == id && x.DocumentTypeId == docdets.Id).ToList();
                List<Attachments> attachments = new List<Attachments>();
                var atth = db.Attachments.FirstOrDefault();
                attachments.Add(atth);

                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    LeaseTermination = termination,
                    Attachments = attachments,
                    PropertyLeaseApplications = rcsApps,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description


                };

                Entity entity = null;
                Agent agent = null;


                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                CommitteeOutcome committee = new CommitteeOutcome();
                committee.Surname = systemusers.LastName;
                committee.FirstName = systemusers.FirstName;

                vm.CommitteeOutcome = committee;
                vm.Customer = customer;
                vm.PropertyLeaseApplications = rcsApps;

                ViewBag.RejectComment = "";
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

                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                bool IsUpload = true;
                var returnUrl = "";
                MatchingHelper.DocumentEvictionCommitteeOutcome(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, returnUrl, rcsApps.Id, IsUpload);

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
            return View(rcsApps);
        }
        #endregion

        #region Property Eviction Validation
        [DecryptParameter]
        [HttpPost]
        public ActionResult EvictionCommitteeOutcome(DepartmentsApprovalViewModel vm, int? id, string ApprovalStatusddl, string CommitteeOutcome_OfficialNumber, string CommitteeOutcome_Reason, string RejectComment)
        {
            using (var _context = new eServicesDbContext())
            {
                Initialise();
                var userID = Customer;
                var Keys = _context.Status;
                var LeaseApplication = _context.LeaseDetails.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();

                PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                {
                    PropertyLeaseApplicationId = LeaseApplication.PropertyLeaseApplicationId,
                    RejectReason = RejectComment,
                    PropertyEvictionValidation = true
                };
                _context.propertyLeaseActionComments.Add(comments);
                _context.SaveChanges();

                var LeaseId = id;
                CommitteeOutcome committee = new CommitteeOutcome();
                committee = vm.CommitteeOutcome;
                committee.FirstName = vm.CommitteeOutcome.FirstName;
                committee.Surname = vm.CommitteeOutcome.Surname;
                committee.Reason = CommitteeOutcome_Reason;
                committee.OfficialNumber = CommitteeOutcome_OfficialNumber;
                committee.PropertyLeaseApplicationId = LeaseApplication.PropertyLeaseApplicationId;
                committee.LeaseDetailsId = LeaseApplication.Id;
                committee.DateStamp = DateTime.Now;
                _context.committeeOutcomes.Add(committee);
                _context.SaveChanges();

                var terminate = _context.LeaseTerminations.Where(x => x.LeaseDetailsId == id && !x.IsDeleted).FirstOrDefault();
                var LL = ": " + LeaseApplication.LeaseReferenceNo;
                if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                {
                    LeaseApplication.EndDate = terminate.TerminationDate;
                    LeaseApplication.RenewalNotice = (terminate.TerminationDate.AddMonths(-3));
                    _context.Entry(LeaseApplication).State = EntityState.Modified;
                    _context.SaveChanges();

                    terminate.StatusId = (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminationDateIssued).Id;
                    _context.Entry(terminate).State = EntityState.Modified;
                    _context.SaveChanges();

                    MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingVacatingConfirm).Id, (int)LeaseId);

                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EvictionCommitteeActionsTermination).Description.ToString()+ LL;
                    MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                }
                else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
                {
                    terminate.StatusId = (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminationRejected).Id;
                    _context.Entry(terminate).State = EntityState.Modified;
                    _context.SaveChanges();

                    MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminationRejected).Id, (int)LeaseId);

                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EvictionCommitteeActionsTermination).Description.ToString()+ LL;
                    MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                }
                return RedirectToAction("PropertyLeaseApplicationTerminations");

            }


        }
        #endregion











        [DecryptParameter]
        public ActionResult ConfirmVacatingAppicant(int? id)
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
            LeaseTermination termination = null;

            lease = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.ModifiedBySystemUser)
              .Include(r => r.Status)
              .Include(r => r.PurchaserType)
              .Where(x => x.Id == id).FirstOrDefault();

            termination = db.LeaseTerminations.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
             .Include(r => r.ModifiedBySystemUser).Include(r => r.Status)
             .Where(x => x.LeaseDetailsId == lease.Id).FirstOrDefault();

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();

            ViewBag.Id = lease.Id;
            ViewBag.PropId = rcsApps.Id;

            ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Vacated || x.Key == RCSActionTypeKeys.NotVacated).OrderBy(x => x.Name), "Key", "Name");

            try
            {
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                if (customer == null) throw new Exception("Invalid Customer");

                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    LeaseTermination = termination,
                    PropertyLeaseApplications = rcsApps,
                };

                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                CommitteeOutcome committee = new CommitteeOutcome();
                committee.Surname = systemusers.LastName;
                committee.FirstName = systemusers.FirstName;

                vm.CommitteeOutcome = committee;
                vm.Customer = customer;
                vm.PropertyLeaseApplications = rcsApps;

                ViewBag.RejectComment = "";
                ViewBag.LeaseId = db.LeaseDetails.Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).OrderByDescending(x => x.Id).FirstOrDefault().Id;
               
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
        public ActionResult ConfirmVacatingAppicant(DepartmentsApprovalViewModel vm, int? id, string ApprovalStatusddl)
        {
            using (var _context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var userID = Customer;
                    var Keys = _context.Status;
                    var LeaseApplication = _context.LeaseDetails.OrderByDescending(x=>x.Id).Where(x => x.Id == vm.LeaseDetails.Id && x.IsDeleted == false).FirstOrDefault();
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.VacatingConfirmation).FirstOrDefault();
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)LeaseApplication.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, Customer.Id);

                    if (ApprovalStatusddl == RCSActionTypeKeys.Vacated)
                    {
                        MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicantVacated).Id, (int)LeaseApplication.Id);
                        //MatchingHelper.ChangeApplicationStatus(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicantVacated).Id, (int)LeaseApplication.PropertyLeaseApplicationId);
                        MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)LeaseApplication.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, Customer.Id);

                        var app = _context.ApplicantUnits.Include(r=>r.Matched).FirstOrDefault(x => x.PropertyLeaseApplicationId == LeaseApplication.PropertyLeaseApplicationId).Matched;
                        //var ehcekurhuleniunit = _context.UnitsEkurhuleniHousingCompany.FirstOrDefault(r => r.Id == app.UnitsEkurhuleniHousingCompanyId);
                        var ehcekurhuleniunit = _context.ApplicationAllocatedProperty.FirstOrDefault(r => r.Id == app.ApplicationAllocatedPropertyId);
                        if (ehcekurhuleniunit != null)
                        {
                            ehcekurhuleniunit.IsTaken = false;
                            _context.Entry(ehcekurhuleniunit).State = EntityState.Modified;
                            _context.SaveChanges();
                        }
                        var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EvictionCommitteeActionsTermination).Description.ToString();
                        MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                        //                      1      2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17     18   19
                        EHCRoundRobin((int)LeaseApplication.PropertyLeaseApplicationId, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, true, 1);
                        Session["ConfirmVacatingAppicantSession"] = string.Format($"Tenant vacated successfully for application reference ,{LeaseApplication.LeaseReferenceNo}");

                    }
                    else if (ApprovalStatusddl == RCSActionTypeKeys.NotVacated)
                    {
                        MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicantNotVacated).Id, (int)LeaseApplication.Id);
                        //MatchingHelper.ChangeApplicationStatus(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicantNotVacated).Id, (int)LeaseApplication.PropertyLeaseApplicationId);
                        //    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)LeaseApplication.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, Customer.Id);
                        //    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        //    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EvictionCommitteeActionsTermination).Description.ToString();
                        //    MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                        Session["ConfirmVacatingAppicantSession"] = string.Format($"Tenant not vacated for application reference ,{LeaseApplication.LeaseReferenceNo}");
                    }
                    return RedirectToAction("LeaseTerminated", "leaseDetails");
                }
                catch (Exception Io)
                {
                    throw;
                }
                
            }
        }










































        [DecryptParameter]
        public ActionResult viewpaymenthistory(int? refNo)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();

            RCSApplicationStatus rcsApps = null;
            rcsApps = context.RCSApplicationStatus.Where(x => x.Id == refNo && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.TransferInformation).Include(d => d.Status).FirstOrDefault();

            var vm = new DepartmentsApprovalViewModel();
            vm.OnlinePaymentHistory = db.AssessmentPaymentTransactions.Where(x => x.RCSApplicationStatusId == refNo && x.IsActive == true).ToList();

            var FiguresPaymentDocumentType = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);

            var FiguresPayment = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == FiguresPaymentDocumentType.Id);

            var ApplicationFeeDocumentType = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfPayment);

            var ApplicationFee = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ApplicationFeeDocumentType.Id);


            var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status).Include(d => d.DocumentCheckList).Include(d => d.DocumentCheckList.DocumentType)
                                         .Where(d => d.CustomerId == Customer.Id && (d.DocumentCheckListId == FiguresPayment.Id || d.DocumentCheckListId == ApplicationFee.Id)
                     && d.IsActive
                     && d.IsDeleted == false && d.RCSApplicationStatusId == rcsApps.Id).ToList();

            foreach (var doc in customerDocuments)
            {
                doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

            }
            var dvm = new DocumentsViewModel
            {


                Documents = customerDocuments


            };


            vm.Document = dvm;



            vm.RCSApplicationStatus = rcsApps;


            return View(vm);

        }

        #endregion
        #region Department Comments Enum
        public enum DepartmentsComments
        {
            Legal,
            CreditControl,
            Endowment,
            Accounts
        }
        #endregion

        #region Insert For a Specific Department
        public void departmentsApprovals(IEnumerable<DepartmentsApproval> depList)
        {
            var cxt = new eServicesDbContext();
            foreach (var item in depList)
            {
                DepartmentsApproval depApprovals = new DepartmentsApproval();
                depApprovals = item;
                cxt.DepartmentsApprovals.Add(depApprovals);
                cxt.SaveChanges();
            }
        }
        #endregion

        #region Approve Or Reject 
        public void ApproveOrReject(int id, string ApprovalStatusddl, string ReAssignddl, int? ReAllocateddl)
        {
            var ApprovrcsType = db.Status;
            var approval = db.DepartmentsApprovals.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                ReAllocateddl = 0;
                approval.StatusId = ApprovrcsType.Where(x => x.Key == StatusKeys.Approved).FirstOrDefault().Id;
                updateApproval(approval, ReAssignddl, (int)ReAllocateddl);
            }

            else if (ApprovalStatusddl == RCSActionTypeKeys.ReAllocate)
            {
                approval.StatusId = ApprovrcsType.Where(x => x.Key == StatusKeys.ReAllocate).FirstOrDefault().Id;
                updateApproval(approval, ReAssignddl, (int)ReAllocateddl);
            }

            else if (ApprovalStatusddl == RCSActionTypeKeys.ReAssignDep)
            {
                approval.StatusId = ApprovrcsType.Where(x => x.Key == StatusKeys.ReAssignDep).FirstOrDefault().Id;
                updateApproval(approval, ReAssignddl, (int)ReAllocateddl);
            }

            else if (ApprovalStatusddl == RCSActionTypeKeys.Disprove)
            {
                approval.StatusId = ApprovrcsType.Where(x => x.Key == StatusKeys.Disprove).FirstOrDefault().Id;
                updateApproval(approval, ReAssignddl, (int)ReAllocateddl);
            }

            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                approval.StatusId = ApprovrcsType.Where(x => x.Key == StatusKeys.Rejected).FirstOrDefault().Id;
                updateApproval(approval, ReAssignddl, (int)ReAllocateddl);
            }

            var AllApprovals = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == approval.RCSApplicationStatusId).ToList();

            int countApproved = 0;
            int countNothing = 0;
            int countDisapprove = 0;
            int countRejected = 0;

            var item = AllApprovals.ToList();

            for (int i = 0; i < item.Count(); i++)
            {

                if (item[i].StatusId == ApprovrcsType.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id)
                {
                    countNothing++;
                }

                else if (item[i].StatusId == ApprovrcsType.Where(x => x.Key == StatusKeys.Approved).FirstOrDefault().Id)
                {
                    countApproved++;
                }

                else if (item[i].StatusId == ApprovrcsType.Where(x => x.Key == StatusKeys.Disprove).FirstOrDefault().Id)
                {
                    countDisapprove++;
                }

                else if (item[i].StatusId == ApprovrcsType.Where(x => x.Key == StatusKeys.Rejected).FirstOrDefault().Id)
                {
                    countRejected++;
                }

                if (countNothing > 0)
                {
                    //break;
                }

                if (item.Count() == countApproved)
                {
                    //Write codeMethod to update rcs application status   
                    updateApprovals(item, item[i].RCSApplicationStatusId, ApprovrcsType.Where(x => x.Key == StatusKeys.BackOffice).FirstOrDefault().Id);
                }

                //Goes to the BO so that they can choose to reject the application.
                if (countDisapprove > 0)
                {
                    //Write codeMethod to update rcs application status   
                    updateApprovals(item, item[i].RCSApplicationStatusId, ApprovrcsType.Where(x => x.Key == StatusKeys.Disprove).FirstOrDefault().Id);
                }

                //BO rejected the application.
                if (countRejected > 0)
                {
                    //Write codeMethod to update rcs application status   
                    updateApprovals(item, item[i].RCSApplicationStatusId, ApprovrcsType.Where(x => x.Key == StatusKeys.Rejected).FirstOrDefault().Id);
                }
            }
        }


        #endregion

        #region Update Appproval Status Single Record
        public void updateApproval(DepartmentsApproval approval, string ReAssignddl, int ReAllocateddl)
        {
            var status = db.Status.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();

            if (approval.StatusId == status.Where(x => x.Key == StatusKeys.Approved).FirstOrDefault().Id)
            {
                //Approval
                var depApproval = db.DepartmentsApprovals.Where(x => x.Id == approval.Id).FirstOrDefault();
                depApproval.Id = approval.Id;
                depApproval.StatusId = approval.StatusId;
                depApproval.CapturedDate = DateTime.Now.Date;
                depApproval.CreatedDateTime = DateTime.Now;
                db.SaveChanges();
            }

            else if (approval.StatusId == status.Where(x => x.Key == StatusKeys.ReAllocate).FirstOrDefault().Id)
            {
                //Re-allocate to another user
                var depApproval = db.DepartmentsApprovals.Where(x => x.Id == approval.Id).FirstOrDefault();
                depApproval.Id = approval.Id;
                depApproval.StatusId = approval.StatusId;
                depApproval.CapturedDate = DateTime.Now.Date;
                depApproval.CreatedDateTime = DateTime.Now;
                depApproval.AssignedToCustomerId = ReAllocateddl;

                db.SaveChanges();
            }

            else if (approval.StatusId == status.Where(x => x.Key == StatusKeys.ReAssignDep).FirstOrDefault().Id)
            {
                // Re-assign to another dep & rr to another user.
                var depApproval = db.DepartmentsApprovals.Where(x => x.Id == approval.Id).FirstOrDefault();

                depApproval.Id = approval.Id;
                depApproval.StatusId = approval.StatusId;
                depApproval.CapturedDate = DateTime.Now.Date;
                depApproval.CreatedDateTime = DateTime.Now;
                depApproval.AssignedToCustomerId = ReAllocateddl;
                var depId = db.RCSDepartmentTypes.Where(x => x.Key == ReAssignddl).FirstOrDefault().Id;
                depApproval.DepartmentId = Convert.ToInt32(depId);
                db.SaveChanges();
            }

            else if (approval.StatusId == status.Where(x => x.Key == StatusKeys.Disprove).FirstOrDefault().Id)
            {
                //Disapproval
                var depApproval = db.DepartmentsApprovals.Where(x => x.Id == approval.Id).FirstOrDefault();
                depApproval.Id = approval.Id;
                depApproval.StatusId = approval.StatusId;
                depApproval.CapturedDate = DateTime.Now.Date;
                depApproval.CreatedDateTime = DateTime.Now;
                db.SaveChanges();
            }

            else if (approval.StatusId == status.Where(x => x.Key == StatusKeys.Rejected).FirstOrDefault().Id)
            {
                //Rejected
                var depApproval = db.DepartmentsApprovals.Where(x => x.Id == approval.Id).FirstOrDefault();
                depApproval.Id = approval.Id;
                depApproval.StatusId = approval.StatusId;
                depApproval.CapturedDate = DateTime.Now.Date;
                depApproval.CreatedDateTime = DateTime.Now;
                db.SaveChanges();
            }
        }
        #endregion

        #region Update Appproval Status Multiple Records
        public void updateApprovals(IEnumerable<DepartmentsApproval> depList, int RCSId, int status)
        {
            var statuses = db.Status.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
            var application = db.RCSApplicationStatus.Where(x => x.Id == RCSId && x.IsDeleted == false).FirstOrDefault();
            var BackOfficeStatus = statuses.Where(x => x.Key == StatusKeys.BackOffice).FirstOrDefault().Id;

            foreach (var item in depList)
            {
                DepartmentsApproval depApprovals = new DepartmentsApproval();
                depApprovals = item;
                //Update what needs to be updated so that it can go to the bo for example a status
                depApprovals.StatusId = status;
                db.SaveChanges();
            }

            if (BackOfficeStatus == status)
            {
                application.StatusId = BackOfficeStatus;
                db.SaveChanges();
            }
        }
        #endregion

        #region NotificationsForStaff
        public bool BackOfficeNotification(int RCSAppID, int CustomerID, string QueueName)
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

        public bool BackOfficeReAllocatedNotification(int RCSAppID, int CustomerID, string QueueName)
        {
            try
            {
                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.BONewCaseReAllocated).FirstOrDefault();
                Email SendMail = new Email();
                var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == RCSAppID).FirstOrDefault();
                var BackOfficeClerk = db.Customers.Where(x => x.Id == CustomerID).FirstOrDefault();
                string attorneyemail = "siyanda.ngxonga@xetgroup.com" /*BackOfficeClerk.EmailAddress*/;
                string attorneyname = BackOfficeClerk.FirstName + " " + BackOfficeClerk.LastName;
                string emailbody = getemailbody.Description;
                emailbody = emailbody.Replace("{0}", QueueName);
                emailbody = emailbody.Replace("{1}", RCSApplication.ApplicationReferenceNumber);
                var systemusermobilenum = BackOfficeClerk.CellPhoneNumber;
                //string emailbody = "Your proof of payment documents for RCS assessment figures has been rejected , please re-upload required documents for RCS Application Reference Number: " + rcsApps.ApplicationReferenceNumber;

                var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + emailbody;

                SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum, RCSApplication.Id, CustomerID, emailbody, attorneyemail, "PLM-Online Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

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

        #region Round Robin
        //Sashen Round robin
        public int RoundRobinCCC(bool AcknowlegeApplication, bool SubmitFigures, bool IssueRCC, bool BillingSection, bool Rates, bool BackOffice, bool CreditControl, bool Sundries, int RCSAppID, int DepartmentID, bool AcknowlegeRefund, bool IssueRefundsCollection, int RefundAppID)
        {
            //get all users for each department
            var applicationUserRoles = db.Roles.ToList();
            var dates = new string[2];
            dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            var startDate = DateTime.Parse(dates[0]).Date;
            var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            var oneDayTime = endDate - startDate;
            var RoundRobingQueue = db.RoundRobinQueues.Where(x => x.IsActive == true && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();

            //var RoundRobingQueue = db.RoundRobinQueues.Where(x=>x.IsActive == true && DateTime.Compare(x.CreatedDateTime.Value.Date, DateTime.Now.Date) <= 0).ToList();
            //var RoundRobingQueue = db.RoundRobinQueues.Where(x=>x.CreatedDateTime.Value.Day == DateTime.Now.Date).ToList();
            var AccountsManagementUsers = (List<SystemIdentityUser>)null;
            var SubmitFiguresUsers = (List<SystemIdentityUser>)null;
            var IssueRCCUsers = (List<SystemIdentityUser>)null;
            var BillingUsers = (List<SystemIdentityUser>)null;
            var RateUsers = (List<SystemIdentityUser>)null;
            var BOUsers = (List<SystemIdentityUser>)null;
            var CreditControlUsers = (List<SystemIdentityUser>)null;
            var SundriesUsers = (List<SystemIdentityUser>)null;
            var RefundUsers = (List<SystemIdentityUser>)null;

            //DateTime dt = DateTime.Now;

            //var test = dt.Date;
            //get a count of applications for each user in each department
            var AssignedToUser = 0;

            var responsibilityTypes = db.ResponsibilityTypes.ToList();

            if (AcknowlegeApplication)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var AccountsManagement = applicationUserRoles.Where(x => x.Name == "Acknowledge RCS Application").FirstOrDefault().Id;
                AccountsManagementUsers = GetUsersInRole(AccountsManagement).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault();
                if (AccountsManagementUsers.Count() > 0)
                {

                    AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;




                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityTypeId.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CCCId = Convert.ToInt16(RcsApplication.CCCId),
                        CCCName = RcsApplication.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                        RoleID = AccountsManagement,
                        RoleName = "Acknowledge RCS Application",
                        IsActive = true,
                        IsDeleted = false
                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();
                }
            }

            else if (SubmitFigures)
            {


                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Submit Figures").FirstOrDefault().Id;
                SubmitFiguresUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault();

                if (SubmitFiguresUsers.Count() > 0)
                {

                    AssignedToUser = AssigedToCCRR(SubmitFiguresUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
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
                }
            }

            else if (IssueRCC)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Issue Certificate").FirstOrDefault().Id;
                IssueRCCUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault();


                if (IssueRCCUsers.Count() > 0)
                {

                    AssignedToUser = AssigedToCCRR(IssueRCCUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
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

                }
            }

            else if (BillingSection)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Billing").FirstOrDefault().Id;
                BillingUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(BillingUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId = DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
            }

            else if (Rates)
            {

                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Rates").FirstOrDefault().Id;
                RateUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(RateUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId = DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
            }


            else if (CreditControl)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Credit Control").FirstOrDefault().Id;
                CreditControlUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(CreditControlUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId = DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
            }
            else if (Sundries)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Sundry Account").FirstOrDefault().Id;
                SundriesUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(SundriesUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId = DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);

            }
            else if (AcknowlegeRefund)
            {
                var RcsApplication = db.RefundApplications.Include(x => x.Status).Include(x => x.RCSApplicationStatus).Include(x => x.RCSApplicationStatus.CCC).OrderByDescending(x => x.Id).FirstOrDefault(x => x.Id == RefundAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Acknowledge Refund Application").FirstOrDefault().Id;
                RefundUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.RCSApplicationStatus.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault();

                if (RefundUsers.Count() > 0)
                {
                    AssignedToUser = AssigedToCCRR(RefundUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.RCSApplicationStatus.Id,
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CCCId = Convert.ToInt16(RcsApplication.RCSApplicationStatus.CCCId),
                        CCCName = RcsApplication.RCSApplicationStatus.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.RCSApplicationStatus.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();

                }


            }
            else if (IssueRefundsCollection)
            {

                var RcsApplication = db.RefundApplications.Include(x => x.Status).Include(x => x.RCSApplicationStatus).Include(x => x.RCSApplicationStatus.CCC).FirstOrDefault(x => x.Id == RefundAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Issue Refunds Collection").FirstOrDefault().Id;
                RefundUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.RCSApplicationStatus.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault();

                if (RefundUsers.Count() > 0)
                {
                    AssignedToUser = AssigedToCCRR(RefundUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.RCSApplicationStatus.Id,
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CCCId = Convert.ToInt16(RcsApplication.RCSApplicationStatus.CCCId),
                        CCCName = RcsApplication.RCSApplicationStatus.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.RCSApplicationStatus.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();

                }



            }

            return AssignedToUser;
        }

        public int RoundRobinRedistribution(bool AcknowlegeApplication, bool SubmitFigures, bool IssueRCC, bool BillingSection, bool Rates, bool BackOffice, bool CreditControl, bool Sundries, int RCSAppID, int DepartmentID, int RRQueueID, bool AcknowlegeRefund, bool IssueRefundsCollection)
        {
            //get all users for each department
            var applicationUserRoles = db.Roles.ToList();
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.Id == RRQueueID).FirstOrDefault();


            var dates = new string[2];
            dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            var startDate = DateTime.Parse(dates[0]).Date;
            var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            var oneDayTime = endDate - startDate;
            var RoundRobingQueue = db.RoundRobinQueues.Where(x => x.IsActive == true && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();

            //var RoundRobingQueue = db.RoundRobinQueues.Where(x=>x.IsActive == true && DateTime.Compare(x.CreatedDateTime.Value.Date, DateTime.Now.Date) <= 0).ToList();
            //var RoundRobingQueue = db.RoundRobinQueues.Where(x=>x.CreatedDateTime.Value.Day == DateTime.Now.Date).ToList();
            var AccountsManagementUsers = (List<SystemIdentityUser>)null;
            var SubmitFiguresUsers = (List<SystemIdentityUser>)null;
            var IssueRCCUsers = (List<SystemIdentityUser>)null;
            var BillingUsers = (List<SystemIdentityUser>)null;
            var RateUsers = (List<SystemIdentityUser>)null;
            var BOUsers = (List<SystemIdentityUser>)null;
            var CreditControlUsers = (List<SystemIdentityUser>)null;
            var SundriesUsers = (List<SystemIdentityUser>)null;
            var Users = (List<SystemIdentityUser>)null;
            //DateTime dt = DateTime.Now;

            //var test = dt.Date;
            //get a count of applications for each user in each department
            var AssignedToUser = 0;

            var responsibilityTypes = db.ResponsibilityTypes.ToList();

            if (AcknowlegeApplication)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);
                var OldClerk = rrq.Clerk.SystemUserId;
                var AccountsManagement = applicationUserRoles.Where(x => x.Name == "Acknowledge RCS Application").FirstOrDefault().Id;
                AccountsManagementUsers = GetUsersInRole(AccountsManagement).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();
                if (AccountsManagementUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == RCSAppID && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();

                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }



            }

            else if (SubmitFigures)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);
                var OldClerk = rrq.Clerk.SystemUserId;
                var RoleId = applicationUserRoles.Where(x => x.Name == "Submit Figures").FirstOrDefault().Id;
                SubmitFiguresUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();


                if (SubmitFiguresUsers.Count() > 0)
                {

                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(SubmitFiguresUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == RCSAppID && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();

                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }

            }

            else if (IssueRCC)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);
                var OldClerk = rrq.Clerk.SystemUserId;
                var RoleId = applicationUserRoles.Where(x => x.Name == "Issue Certificate").FirstOrDefault().Id;
                IssueRCCUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();


                if (IssueRCCUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(IssueRCCUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == RCSAppID && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }

            }
            else if (AcknowlegeRefund)
            {
                var RcsApplication = db.RefundApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var CCCForApp = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RcsApplication.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var RoleId = applicationUserRoles.Where(x => x.Name == "Acknowledge Refund Application").FirstOrDefault().Id;
                Users = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == CCCForApp.CCCId && x.SystemUserId != OldClerk).ToList();


                if (Users.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(Users, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RefundApplicationId == RcsApplication.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                //var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                //var RoleId = applicationUserRoles.Where(x => x.Name == "Acknowledge Refund Application").FirstOrDefault().Id;
                //RefundUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                //var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;
                //AssignedToUser = AssigedToCCRR(RefundUsers, RoundRobingQueue, ResponsibilityTypeId);
                //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                //var statusList = db.Status.ToList();
                //var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                //var roundRobinQueue = new RoundRobinQueue
                //{
                //    DepartmentApprovalId = DepartmentID,
                //    ResponsibilityTypeId = ResponsibilityTypeId,
                //    ClerkId = ClerkId,
                //    StatusId = StatusId


                //};
                //db.RoundRobinQueues.Add(roundRobinQueue);
                //db.SaveChanges();

            }
            else if (IssueRefundsCollection)
            {
                var RcsApplication = db.RefundApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var CCCForApp = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RcsApplication.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var RoleId = applicationUserRoles.Where(x => x.Name == "Issue Refunds Collection").FirstOrDefault().Id;
                Users = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == CCCForApp.CCCId && x.SystemUserId != OldClerk).ToList();




                if (Users.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(Users, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RefundApplicationId == RcsApplication.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
            }

            else if (BillingSection)
            {
                var Dep = db.DepartmentsApprovals.Include(x => x.Status).FirstOrDefault(x => x.Id == DepartmentID);
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == Dep.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var Billings = applicationUserRoles.Where(x => x.Name == "Billing").FirstOrDefault().Id;
                BillingUsers = GetUsersInRole(Billings).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();
                if (BillingUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(BillingUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == Dep.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        DepartmentApprovalId = Dep.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }


            }

            else if (Rates)
            {
                var Dep = db.DepartmentsApprovals.Include(x => x.Status).FirstOrDefault(x => x.Id == DepartmentID);
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == Dep.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var RateDepts = applicationUserRoles.Where(x => x.Name == "Rates").FirstOrDefault().Id;
                RateUsers = GetUsersInRole(RateDepts).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();
                if (RateUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(RateUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == Dep.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        DepartmentApprovalId = Dep.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }


            }


            else if (CreditControl)
            {
                var Dep = db.DepartmentsApprovals.Include(x => x.Status).FirstOrDefault(x => x.Id == DepartmentID);
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == Dep.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var creditcontrols = applicationUserRoles.Where(x => x.Name == "Credit Control").FirstOrDefault().Id;
                CreditControlUsers = GetUsersInRole(creditcontrols).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();
                if (CreditControlUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(CreditControlUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == Dep.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        DepartmentApprovalId = Dep.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
            }
            else if (Sundries)
            {
                var Dep = db.DepartmentsApprovals.Include(x => x.Status).FirstOrDefault(x => x.Id == DepartmentID);
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == Dep.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var sundries = applicationUserRoles.Where(x => x.Name == "Sundry Account").FirstOrDefault().Id;
                SundriesUsers = GetUsersInRole(sundries).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();
                if (SundriesUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault();

                    AssignedToUser = AssigedToCCRedistributionRR(SundriesUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == Dep.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        DepartmentApprovalId = Dep.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }

            }

            return AssignedToUser;
        }

        public int RoundRobin(bool AccountsManagementDep, bool LegalSection, bool CreditControlSection, bool BillingSection, bool EndowmentSection, bool BackOffice)
        {
            //get all users for each department
            var applicationUserRoles = db.Roles.ToList();
            var DepartmentApprovals = db.DepartmentsApprovals.ToList();

            var AccountsManagementUsers = (List<SystemIdentityUser>)null;
            var LegalUsers = (List<SystemIdentityUser>)null;
            var CreditControlUsers = (List<SystemIdentityUser>)null;
            var BillingUsers = (List<SystemIdentityUser>)null;
            var EndowmentUsers = (List<SystemIdentityUser>)null;
            var BOUsers = (List<SystemIdentityUser>)null;

            //get a count of applications for each user in each department
            var AssignedToUser = 0;

            if (AccountsManagementDep)
            {
                var AccountsManagement = applicationUserRoles.Where(x => x.Name == "Accounts Management").FirstOrDefault().Id;
                AccountsManagementUsers = GetUsersInRole(AccountsManagement).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(AccountsManagementUsers, DepartmentApprovals);
            }

            else if (LegalSection)
            {
                var Legal = applicationUserRoles.Where(x => x.Name == "Legal").FirstOrDefault().Id;
                LegalUsers = GetUsersInRole(Legal).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(LegalUsers, DepartmentApprovals);
            }

            else if (CreditControlSection)
            {
                var CreditControl = applicationUserRoles.Where(x => x.Name == "Credit Control").FirstOrDefault().Id;
                CreditControlUsers = GetUsersInRole(CreditControl).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(CreditControlUsers, DepartmentApprovals);
            }

            else if (BillingSection)
            {
                var Billing = applicationUserRoles.Where(x => x.Name == "Billing").FirstOrDefault().Id;
                BillingUsers = GetUsersInRole(Billing).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(BillingUsers, DepartmentApprovals);
            }

            else if (EndowmentSection)
            {
                var Endowment = applicationUserRoles.Where(x => x.Name == "Endowment").FirstOrDefault().Id;
                EndowmentUsers = GetUsersInRole(Endowment).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(EndowmentUsers, DepartmentApprovals);
            }

            else if (BackOffice)
            {
                var BO = applicationUserRoles.Where(x => x.Name == "Clerks").FirstOrDefault().Id;
                BOUsers = GetUsersInRole(BO).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(BOUsers, DepartmentApprovals);
            }

            return AssignedToUser;
        }

        #region Assign RR Active Users
        public ActionResult RRActiveUsers()
        {
            Initialise();
            var Users = (List<SystemIdentityUser>)null;
            if (User.IsInRole("Back Office System Administrator") || User.IsInRole("Support Admin"))
            {
                Users = UserManager.Users.Where(x => x.isInternalUser == true && x.isDeleted == false).ToList();
                return View(Users);
            }
            else
            {

                var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id).FirstOrDefault();

                Users = UserManager.Users.Where(x => x.isInternalUser == true && x.CCCId == CCCClerk.Id && x.isDeleted == false).ToList();
                return View(Users);
            }

        }

        #region Assign RR Active Users Edit 
        //
        [DecryptParameter]
        public ActionResult EditRRUser(string id)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    if (id == null) throw new Exception("Invalid User");

                    var Users = UserManager.Users.Where(x => x.Id == id);
                    ViewBag.Name = Users.FirstOrDefault().UserName;
                    ViewBag.Isactive = Users.FirstOrDefault().RoundRobinIsActive;

                    var Message = TempData["RoundRobinRedistributionTitle"];
                    var Title = TempData["RoundRobinRedistribution"];
                    if (Message != null && Title != null)
                    {
                        ViewBag.MessageTitle = TempData["RoundRobinRedistributionTitle"].ToString();
                        ViewBag.Message = TempData["RoundRobinRedistribution"].ToString();
                    }
                    return View(Users);
                }
                catch
                {
                    return View("_Error");
                }
            }
        }
        #endregion

        #region Assign RR Active Users Edit POST
        [HttpPost]
        [DecryptParameter]
        public ActionResult EditRRUser(string Id, bool rrIsActive, bool ReAllocateCases)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var user = (SystemIdentityUser)UserManager.FindById(Id);

                    if (ReAllocateCases == true)
                    {
                        var Keys = db.Status;
                        int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                        var SysUserID = user.SystemUserId;
                        var Clerk = db.Customers.Where(x => x.SystemUserId == SysUserID).FirstOrDefault();
                        var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.ResponsibilityType).Include(x => x.Clerk.SystemUser).Include(x => x.RefundApplication).Include(x => x.RCSApplicationStatus).Where(x => x.ClerkId == Clerk.Id && x.StatusId == SubmittedId).ToList();
                        AesCrypto AES = new AesCrypto();
                        var q = AES.Encrypt("id=" + Id);
                        foreach (var item in rrqList)
                        {

                            DepartmentsApprovalsController DA = new DepartmentsApprovalsController();
                            CaptureController c = new CaptureController();
                            switch (item.ResponsibilityType.Key)
                            {
                                case (ResponsibilityTypeKeys.AcknowledgeRCSApplication):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(true, false, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);
                                        //var SystUserId = c.RoundRobinCCC(true, false, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, false, false, 0);
                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);


                                        }
                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.SubmitFigures):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.IssueCertificates):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, true, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.Billing):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, true, false, false, false, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.CreditControl):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, true, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.SundryAccount):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, true, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.Rates):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, true, false, false, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.AcknowledgeRefundApplication):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, Convert.ToInt16(item.RefundApplicationId), 0, item.Id, true, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.IssueRefundCollection):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, Convert.ToInt16(item.RefundApplicationId), 0, item.Id, false, true);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                            }






                        }

                    }

                    // Update it with the values from the view model
                    user.RoundRobinIsActive = rrIsActive;
                    UserManager.Update(user);

                    return RedirectToAction("RRActiveUsers");
                }
                catch
                {
                    return View("_Error");
                }
            }
        }

        #endregion

        #endregion
        #endregion


        #region Department Approvals
        public static int AssigedToRR(List<SystemIdentityUser> Users, List<DepartmentsApproval> DepartmentsApprovals)
        {
            var AssignedToPerson = 0;

            if (Users != null)
            {
                List<int> Approvals = new List<int>();
                List<int> User = new List<int>();
                eServicesDbContext db = new eServicesDbContext();


                foreach (var item in Users)
                {
                    var custid = db.Customers.Where(x => x.SystemUserId == item.SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var depApprovals = DepartmentsApprovals.Where(x => x.AssignedToCustomerId == custid.Id).Count();
                    Approvals.Add(depApprovals);
                    User.Add(item.SystemUserId);
                }

                int min = Approvals[0];
                int minIndex = 0;

                for (int i = 1; i < Approvals.Count; ++i)
                {
                    if (Approvals[i] < min)
                    {
                        min = Approvals[i];
                        minIndex = i;
                    }
                }

                var lowestIndex = minIndex;
                AssignedToPerson = User[lowestIndex];
            }


            return AssignedToPerson;
        }

        //Sashen Round robin
        public static int AssigedToCCRR(List<SystemIdentityUser> Users, List<RoundRobinQueue> roundRobinQueues, int ResponsibilityType)
        {
            eServicesDbContext db = new eServicesDbContext();
            var AssignedToPerson = 0;
            var statusList = db.Status.ToList();
            var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (Users != null)
            {
                List<int> Approvals = new List<int>();
                List<int> User = new List<int>();



                foreach (var item in Users)
                {
                    var custid = db.Customers.Where(x => x.SystemUserId == item.SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();


                    var depApprovals = roundRobinQueues.Where(x => x.ClerkId == custid.Id && x.ResponsibilityTypeId == ResponsibilityType && x.StatusId == StatusId).Count();
                    Approvals.Add(depApprovals);
                    User.Add(item.SystemUserId);
                }

                int min = Approvals[0];
                int minIndex = 0;

                for (int i = 1; i < Approvals.Count; ++i)
                {
                    if (Approvals[i] < min)
                    {
                        min = Approvals[i];
                        minIndex = i;
                    }
                }

                var lowestIndex = minIndex;
                AssignedToPerson = User[lowestIndex];
            }


            return AssignedToPerson;
        }
        public static int AssigedToCCRedistributionRR(List<SystemIdentityUser> Users, List<RoundRobinQueue> roundRobinQueues, int ResponsibilityType)
        {
            eServicesDbContext db = new eServicesDbContext();
            var AssignedToPerson = 0;
            var statusList = db.Status.ToList();
            var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (Users != null)
            {
                List<int> Approvals = new List<int>();
                List<int> User = new List<int>();



                foreach (var item in Users)
                {
                    var custid = db.Customers.Where(x => x.SystemUserId == item.SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();


                    var depApprovals = roundRobinQueues.Where(x => x.ClerkId == custid.Id && x.ResponsibilityTypeId == ResponsibilityType && x.StatusId == StatusId).Count();
                    Approvals.Add(depApprovals);
                    User.Add(item.SystemUserId);
                }

                int min = Approvals[0];
                int minIndex = 0;

                for (int i = 1; i < Approvals.Count; ++i)
                {
                    if (Approvals[i] < min)
                    {
                        min = Approvals[i];
                        minIndex = i;
                    }
                }

                var lowestIndex = minIndex;
                AssignedToPerson = User[lowestIndex];
            }


            return AssignedToPerson;
        }

        #endregion

        #region GetUsersInRole
        public IEnumerable<SystemIdentityUser> GetUsersInRole(string roleId)
        {
            return UserManager.Users.Where(o => o.Roles.Any(s => s.RoleId == roleId)).ToList();
        }
        #endregion

        #region PDF Generation
        public ActionResult PDFExample(int id)

        {
            var root = Server.MapPath("~/PDFFiles/");
            string pdfname = "Pdfexm.pdf";
            var path = System.IO.Path.Combine(root, pdfname);
            path = System.IO.Path.GetFullPath(path);
            var parameters = 80;

            var actionPDF = new Rotativa.ActionAsPdf("PDFExample1", new { parameters = parameters })
            {
                FileName = "PDFExample.pdf",
                SaveOnServerPath = path,
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                PageMargins = { Left = 10, Right = 6, Top = 10 }

            };

            byte[] applicationPDFData = actionPDF.BuildFile(ControllerContext);
            var fullpathtofile = path;
            var mimetype = "application/pdf";
            var filecontents = System.IO.File.ReadAllBytes(fullpathtofile);

            return new FileContentResult(filecontents, mimetype);
        }
        #region MyRegion

        /* @parameters
         * formName -Name of your Application Form (eThekwini Name)
         * fileName -Name of the File
         * urlDomain- The IP address of the Domain for Dev it is your Local Host with Port Number
         * controllerName- Name of the Controller your View Method is In
         * viewName - Name of the View your Form is on
         * pdfParameter -value of the ID you send to your View
         */
        public ActionResult GenerateViewToPDF(string formName, string fileName, string urlDomain, string controllerName, string viewName)
        {
            try
            {

                var root = Server.MapPath("~/PDFFiles/");
                string pdfname = "Cloud_Based_Form_" + formName + ".pdf";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);

                //var urlDomain = PGDomain.Value;
                //urlDomain = "localhost:3456";
                //controllerName = "CloudBasedForms";
                //viewName = "OnlineApplications";
                //url="localhost:3456/CloudBasedForms/OnlineApplications"

                var url = string.Format("{0}/{1}/{2}", urlDomain, controllerName, viewName);
                var actionPDF = new Rotativa.UrlAsPdf(url)
                {
                    FileName = fileName,
                    //SaveOnServerPath = path, // JK.20200404a - Deprecated, save bytes as below.
                    PageSize = Rotativa.Options.Size.A4,
                    PageOrientation = Rotativa.Options.Orientation.Portrait,
                    PageMargins = { Left = 10, Right = 6, Top = 10 }
                };

                byte[] applicationPDFData = actionPDF.BuildFile(ControllerContext);
                // JK.20200404a - SaveOnServerPath is deprecated, have to save bytes manually.
                System.IO.File.WriteAllBytes(path, applicationPDFData);

                var fullpathtofile = path;
                var mimetype = "application/pdf";
                var filecontents = System.IO.File.ReadAllBytes(fullpathtofile);

                return new FileContentResult(filecontents, mimetype);
            }
            catch (Exception x)
            {
                EventLogHelper.LogSystemError(x.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                db.Logs.Add(new Log()
                {
                    LogTypeId = 1,
                    LogEntry = x.ToString(),
                    ReferenceId = 0,
                    ReferenceTypeId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                });
                db.SaveChanges();
                //throw x;
            }

            return null;
        }

        #endregion
        //Get
        public ActionResult PDFExample1()
        {
            return View();
        }

        //Post Method
        //public ActionResult PDFExample1(string parameters1, int parametr2, double parameter3, bool prameter4, File doc1, File Doc2)
        //{
        //    //db context

        //    eServicesDbContext db = new eServicesDbContext();
        //    //Save parameters to DB Table
        //    Customer newCust = new Customer();
        //    newCust.FirstName = parameters1;
        //    db.Customers.Add(newCust);
        //    db.SaveChanges();
        //    //Create PDF of Form
        //    GenerateViewToPDF(parameters1, parametr2.ToString(), parameter3.ToString(), parameter3.ToString(), parameter3.ToString());

        //    //pdfLink urlDomain/PDFFiles/FileName.pdf

        //    //Upload Documents to Sharepoint       

        //    //Return Sharepoint Links
        //    //link for Form.pdf
        //    //Link for Doxc1
        //    //Link for Doc 2

        //    //Cesar Email

        //    Email SendMail = new Email();
        //    string emailbody = "Hi Department Man /n" +
        //        "Please See Attached Sharepoint Links for:/n" +
        //        newCust.FullName + " CloudBasedForms Online Application /n" +
        //        "And supporting Documents";
        //    SendMail.GenerateEmail(newCust.EmailAddress, "Deparment Name- New Online Application Submission",
        //                            emailbody,
        //                            newCust.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, newCust.FullName);

        //    //Congrats work s done Redirect Custoemr To Complete Page
        //    return View();
        //}
        #endregion

        #region API Test
        public ActionResult APITest()
        {
            string key = "";
            string secret = "";
            //var suprimaConsumerCheck = TokenApi.GenerateToken(key, secret);

            return View();
        }
        public ActionResult UnitInspectionChecklist()
        {
            return View();
        }


        #endregion
        public ActionResult MaintenanceJobSheetTemp()
        {
            return View();
        }
        public ActionResult PropertyInsppectionform()
        {
            return View();
        }
        //Use Case 11.36
        public ActionResult DepositRefunds()
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

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.DepositRefund).FirstOrDefault().Id;

                    rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.StatusId == SubmittedId).ToList();

                    var list = rrq.Select(x => x.LeaseDetailsId).ToList();
                    
                    int ApplicantVacated = db.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicantVacated).Id;

                    var rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false && list.Contains(x.Id) && (x.StatusId == ApplicantVacated))
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

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    if (Session["StartRefundProcessSession"] != null)
                    {
                        var value = Session["StartRefundProcessSession"].ToString();
                        Session["StartRefundProcessSession"] = null;
                        ViewBag.StartRefundProcessSession = value;
                    }
                    Session["StartRefundProcessSession"] = null;

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
        public ActionResult StartRefundProcess(int? id)
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

            var LeaseApplication = db.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();

            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == LeaseApplication.PropertyLeaseApplicationId).FirstOrDefault();

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
                DocumentsViewModel refundDocs = new DocumentsViewModel();

                MatchingHelper.DocumentUploadBakingDetailsProof(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, true);
                MatchingHelper.DocumentDepositRefunds(refundDocs, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, false);

                var vm = new DepartmentsApprovalViewModel
                {
                    PropertyLeaseApplications = rcsApps,
                    DocumentsViewModel = dvm,
                    DocumentsViewModelRefundDeposits = refundDocs
                };
                vm.Customer = customer;
                vm.PropertyLeaseApplications = rcsApps;
                ViewBag.ApplicationId = application.Id;
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
        public ActionResult StartRefundProcess(int? id, string comment)
        {

            using (var _context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var userID = Customer;
                    var LeaseApplication = _context.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
                    PropertyLeaseApplication rcsApps = null;
                    rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                      .Where(x => x.Id == LeaseApplication.PropertyLeaseApplicationId).FirstOrDefault();

                    var password = PasswordGenerator.GeneratePassword(true, true, true, false, false, 6);
                    //rcsApps.RefundProcessPassword = password;
                    //_context.Entry(rcsApps).State = EntityState.Modified;
                    //_context.SaveChanges();

                    MatchingHelper.UpdatePasswordForRefundProcess(_context,password,rcsApps.Id);

                    var UserId = Customer.Id;
                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.DepositRefund).FirstOrDefault();
                    MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRefundResponse).Id, (int)LeaseApplication.Id);
                    //MatchingHelper.ChangeApplicationStatus(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRefundResponse).Id, (int)LeaseApplication.PropertyLeaseApplicationId);
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)LeaseApplication.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, Customer.Id);

                    //                      1      2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17     18   19
                    EHCRoundRobin((int)LeaseApplication.PropertyLeaseApplicationId, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1, true);

                    //ActivityTrackerAudit
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AwaitingRefundResponse).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                    //Send Email
                    //Confirmation email

                    //Getting Letting officer
                    var LettingOfficer = GetBackOfficeId(db, rcsApps.Id, true);
                    var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                    var activeDirectoryOn = LettingOfficer.Id != 0 ? LettingOfficer.Id : StoredUser;
                    var customerLettingOfficer = _context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == activeDirectoryOn);
                    if (customerLettingOfficer == null) throw new Exception("Invalid Letting Officer");

                    var confirmationEmail = new Email();
                    var emailAddress = db.AppSettings.FirstOrDefault(r => r.Key == AppSettingKeys.RefundProcessEmail).Value;
                    string subject = "Property Lease Application Refund Process – " + rcsApps.ApplicationReferenceNumber + " – Letting Officer "+ customerLettingOfficer.FirstName + customerLettingOfficer.LastName;
                    var body = string.Format("Application is in refund process, please view link below which contains all documents for refund process,: <br/><a href=\"{0}\" title=\"Refund Process\">{0}</a>", Url.Action("RefundProcessDocuments", "OpenLinks", SecureActionLinkExtension.Encrypt(new { id = rcsApps.Id }), HttpContext.Request.Url.Scheme));

                    body += "<br/> Password: " + password;
                    body += "<br/> Letting officer : " + customerLettingOfficer.FirstName + " " + customerLettingOfficer.LastName + ",<br/> contact number "+customerLettingOfficer.CellPhoneNumber;
                    
                    confirmationEmail.GenerateEmail(emailAddress, subject, body, userID.SystemUser.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, "");

                    Session["StartRefundProcessSession"] = string.Format($"Email for refund has been sent successfully for application reference ,{rcsApps.ApplicationReferenceNumber}");
                    return RedirectToAction("DepositRefunds", "PropertyLeaseApplication");
                }
                catch (Exception e)
                {

                    throw;
                }
            }

        }

        //Use case 11.37
        public ActionResult UpdateRefundResponse()
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

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.DepositRefundResponse).FirstOrDefault().Id;

                    rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.StatusId == SubmittedId).ToList();

                    var list = rrq.Select(x => x.LeaseDetailsId).ToList();

                    int AwaitingRefundResponse = db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRefundResponse).Id;


                    var rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false && list.Contains(x.Id) && (x.StatusId == AwaitingRefundResponse))
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

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    if (Session["UpdateResponseForRefundSession"] != null)
                    {
                        var value = Session["UpdateResponseForRefundSession"].ToString();
                        Session["UpdateResponseForRefundSession"] = null;
                        ViewBag.UpdateResponseForRefundSession = value;
                    }
                    Session["UpdateResponseForRefundSession"] = null;
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
        public ActionResult UpdateResponseForRefund(int? id)
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
            var LeaseApplication = db.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            PropertyLeaseApplication rcsApps = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == LeaseApplication.PropertyLeaseApplicationId).FirstOrDefault();

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
                DocumentsViewModel refundDocs = new DocumentsViewModel();

                MatchingHelper.DocumentUploadBakingDetailsProof(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, true);
                MatchingHelper.DocumentDepositRefunds(refundDocs, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, false);
                
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.RefundDue || x.Key == RCSActionTypeKeys.NoRefundDue).OrderBy(x => x.Name), "Key", "Name");

                var vm = new DepartmentsApprovalViewModel
                {
                    PropertyLeaseApplications = rcsApps,
                    DocumentsViewModel = dvm,
                    DocumentsViewModelRefundDeposits = refundDocs
                };
                vm.Customer = customer;
                vm.PropertyLeaseApplications = rcsApps;
                ViewBag.ApplicationId = application.Id;
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
        public ActionResult UpdateResponseForRefund(int? id, string ApprovalStatusddl,string Comment)
        {

            using (var _context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var userID = Customer;
                    var LeaseApplication = db.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
                    PropertyLeaseApplication rcsApps = null;
                    rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                      .Where(x => x.Id == LeaseApplication.PropertyLeaseApplicationId).FirstOrDefault();

                    var UserId = Customer.Id;
                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.DepositRefundResponse).FirstOrDefault();
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)LeaseApplication.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, Customer.Id);
                    if ((ApprovalStatusddl == RCSActionTypeKeys.RefundDue)) 
                    {
                        MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.RefundReadyForCollection).Id, (int)LeaseApplication.Id);
                        
                        //sending email
                        int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.UpdateRefundResponseRefundDue).Id;
                        EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                    }
                    else if (ApprovalStatusddl == RCSActionTypeKeys.NoRefundDue) 
                    {
                        MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.RefundRejected).Id, (int)LeaseApplication.Id);

                        //sending email
                        int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.UpdateRefundResponseNoRefundDue).Id;
                        EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId,null, Comment);
                    }

                    //ActivityTrackerAudit
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UpdateRefundResponse).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                    Session["UpdateResponseForRefundSession"] = string.Format($"Refund response updated successfully for application reference ,{rcsApps.ApplicationReferenceNumber}");
                    return RedirectToAction("UpdateRefundResponse", "PropertyLeaseApplication");
                }
                catch (Exception e)
                {

                    throw;
                }
            }

        }

        //Manual Process
        public ActionResult PlmManualApplicationProcess()
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

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RiskAssessment).FirstOrDefault().Id;

                    rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => (x.ResponsibilityTypeId == ResponsibilityTypeId) && x.ClerkId == UserId && x.EndTaskDateTime == null).ToList();

                    var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                    var AwaitingRiskAssessment = cxt.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingRiskAssessment).Id;

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id) && x.StatusId == AwaitingRiskAssessment)
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.Customer)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.HumanEHCOptions)
                        .Include(r => r.Status).ToList();

                    foreach (var item in rCSApplicationStatus)
                    {
                        PlmManualDataApplication plmManualApplication = new PlmManualDataApplication();
                        //ApplicationAllocatedProperty collection1 = cxt.ApplicationAllocatedProperty.FirstOrDefault(a => a.SolarReference == collection.SolarReference && a.SpaceUnitNumber == collection.SpaceUnitNumber);
                        
                        plmManualApplication = cxt.PlmManualDataApplications.Where(p => p.ApplicationReferenceNumber == item.ApplicationReferenceNumber).FirstOrDefault();

                        if (plmManualApplication != null)
                        {
                            ApplicationAllocatedProperty collection = new ApplicationAllocatedProperty();

                            collection.SpaceUnitNumber = (plmManualApplication != null ? plmManualApplication.UnitNumber : "");
                            collection.SolarReference = (plmManualApplication != null ? plmManualApplication.SolarReference : "");
                            collection.NumOfBeds = 1;
                            collection.RequiedDepositAmount = (plmManualApplication != null && plmManualApplication.DepositAmount != null ? plmManualApplication.DepositAmount.Value : 0);
                            collection.MonthlyRentalAmount = (plmManualApplication != null && plmManualApplication.RentalCode != null ? plmManualApplication.RentalCode.Value : 0);
                            collection.HumanEHCOptionId = (plmManualApplication != null ? plmManualApplication.HumanEHCOptionsIdFinalDecision : null);
                            collection.PropertyLeaseApplicationId = item.Id;

                            collection.OfferedComplexId = (plmManualApplication != null ? (plmManualApplication.PropertyCodeId != null ? plmManualApplication.PropertyCodeId.Value : 50) : 50);

                            //Not in sheet
                            //collection.SpaceUnitSize = 1;
                            //collection.StreetName = "Test";
                            //collection.Shower = 5;
                            //collection.Township = "Test";
                            //collection.Postal = "Test";

                            collection.AllocatedByUserId = SystemUser.Id;
                            collection.PropertyLeaseApplicationId = item.Id;
                            collection.IsTaken = true;
                            unitAllocationService.Save(collection);

                            MatchingHelper.ChangeApplicationStatus(cxt, (Int32)cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.DocumentUploadNotRiskAssessment)?.Id, item.Id);

                            //Save Matched Units 
                            MatchedUnits matchedUnit = new MatchedUnits
                            {
                                ApplicationAllocatedPropertyId = collection.Id,
                                PropertyLeaseApplicationId = item.Id,
                                IsAccepted = true,
                                RejectedProperty = false,
                            };
                            unitAllocationService.Save(matchedUnit);


                            ApplicantUnit applicantUnit = new ApplicantUnit
                            {
                                IsActive = true,
                                IsDeleted = false,
                                CreatedDateTime = DateTime.Now,
                                DepositPaid = 0.00,
                                OutstandingDepopsitAmount = collection.RequiedDepositAmount,
                                PropertyLeaseApplicationId = item.Id,
                                MatchedID = matchedUnit.Id,
                                CreatedBySystemUserId = item.CreatedBySystemUserId
                            };
                            cxt.ApplicantUnits.Add(applicantUnit);
                            cxt.SaveChanges();

                            MatchingHelper.RoundRobinMarkJobAsFinished(cxt, item.Id, null, ResponsibilityTypeId, Customer.Id);

                            //For Lease
                            LeaseDetails lease = new LeaseDetails();

                            lease.leaseApplicationRef = item.ApplicationReferenceNumber;
                            lease.LeaseReferenceNo = item.ApplicationReferenceNumber;

                            var ThisUnit = cxt.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == collection.Id);
                            lease.buildingName = ThisUnit.BuildingName;
                            lease.SpaceUnitNo = ThisUnit.SpaceUnitNumber;
                            lease.LeaAddress = ThisUnit.Address;
                            lease.LeaPostal = ThisUnit.Postal;
                            lease.LeaSuburb = ThisUnit.Township;

                            var leaseStartDate = plmManualApplication != null && plmManualApplication.LeaseStartDate != null ? plmManualApplication.LeaseStartDate : DateTime.Now;
                            lease.StartDate = leaseStartDate;
                            lease.PeriodInMonths = 36;


                            var month = leaseStartDate.Value.Month;
                            var year = leaseStartDate.Value.Year;
                            var tempday = 15;

                            var NextDate = leaseStartDate;
                            var aa = NextDate.Value.AddMonths(23);
                            var nextdate = new DateTime(aa.Year, aa.Month, tempday);


                            var NXmonth = nextdate.Month;
                            var NXyear = nextdate.Year;

                            var bb = nextdate.AddMonths(1);
                            var terminateday1 = new DateTime(bb.Year, bb.Month, 1);
                            var terminateday = terminateday1.Day;
                            var exitdate = plmManualApplication != null && plmManualApplication.LeaseEndDate != null ? plmManualApplication.LeaseEndDate : new DateTime(NXyear, NXmonth, terminateday); ; //new DateTime(NXyear, NXmonth, terminateday);

                            var cc = nextdate.AddMonths(-2);
                            var renewalNotice = new DateTime(cc.Year, cc.Month, 1);

                            var terminationNotice = new DateTime(NXyear, NXmonth, 1);

                            lease.EndDate = exitdate;
                            lease.RenewalNotice = renewalNotice;
                            lease.TerminationNotice = terminationNotice;
                            lease.DepositeAmount = (int)ThisUnit.RequiedDepositAmount;
                            lease.RentalAmount = (int)ThisUnit.MonthlyRentalAmount;
                            lease.VATAmount = (int)((double)ThisUnit.MonthlyRentalAmount * 0.15);
                            lease.TotalIncludingVAT = (lease.RentalAmount + lease.VATAmount);
                            lease.Email = true;
                            lease.SMS = true;
                            lease.Postal = true;
                            lease.PropertyLeaseApplicationId = item.Id;
                            lease.StatusId = cxt.Status.Where(x => x.Key == StatusKeys.ActiveLease).ToList().FirstOrDefault().Id;
                            lease.IsNew = true;
                            lease.DetailsUpdated = true;
                            lease.TerminationReminder = Convert.ToDateTime("1900/01/01 00:00:00");
                            lease.PurchaserTypeId = 1;
                            lease.FirstNames = item.FirstName;
                            lease.LastName = item.LastName;
                            lease.IDNo = item.IDNo;
                            lease.TypeOfActivities = "N/A";
                            lease.ComplianceDetails = "N/A";
                            cxt.LeaseDetails.Add(lease);
                            cxt.SaveChanges();

                            EHCRoundRobin(item.Id, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1, false, true);
                            MatchingHelper.UpdateIsMigratedPLMApplication(cxt, item.Id);
                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.IsMigrated).Description.ToString();
                            var Result = ActivityTrackerAudit(item.Id, ActivityTrackerMessage, Customer.Id);
                        }
                    }
                    ViewBag.Response = "Complete";
                    return View();
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                ViewBag.Response = "InComplete";
                return View();
            }
        }

        //For getting all IM-Migrated applications
        public ActionResult PlmManualApplications()
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
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AwaitingDocUploadingForMigratedApps).FirstOrDefault().Id;

                    rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.StatusId == SubmittedId).ToList();

                    var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                    var DocumentUploadNotRiskAssessment = cxt.Status.FirstOrDefault(i => i.Key == StatusKeys.DocumentUploadNotRiskAssessment).Id;

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id) && x.StatusId == DocumentUploadNotRiskAssessment && x.IsMigrated)
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.Customer)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.HumanEHCOptions)
                        .Include(r => r.Status).ToList();

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

        [DecryptParameter]
        public ActionResult PlmManualApplicationsDocuments(int rcsAppId)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();

            PropertyLeaseApplication rcsApps = null;
            LeaseDetails lease = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.HumanEHCOptions).Include(r => r.Status)
              .Where(x => x.Id == rcsAppId).FirstOrDefault();

            lease = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
              .Include(r => r.ModifiedBySystemUser)
              .Include(r => r.Status)
              .Include(r => r.PurchaserType)
              .Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).FirstOrDefault();

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
                MatchingHelper.DocumentsPlmManualApplications(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, true);

                var vm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = lease,
                    PropertyLeaseApplications = rcsApps,
                    DocumentsViewModel = dvm
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
        public ActionResult PlmManualApplicationsDocuments(int? rcsAppId)
        {

            using (var _conx = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var userID = Customer;
                    PropertyLeaseApplication rcsApps = null;
                    LeaseDetails lease = null;

                    rcsApps = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                      .Where(x => x.Id == rcsAppId).FirstOrDefault();

                    lease = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.Status)
                      .Include(r => r.PurchaserType)
                      .Where(x => x.PropertyLeaseApplicationId == rcsApps.Id).FirstOrDefault();


                    MatchingHelper.ChangeLeaseStatusII(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicationUpForRenewalAtThreeMonths).Id, (int)lease.Id);
                    MatchingHelper.ChangeApplicationStatus(_conx, _conx.Status.FirstOrDefault(x => x.Key == StatusKeys.ActiveLease).Id, rcsApps.Id);
                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AwaitingDocUploadingForMigratedApps).FirstOrDefault().Id;
                    MatchingHelper.RoundRobinMarkJobAsFinished(_conx, rcsApps.Id, null, ResponsibilityTypeId, Customer.Id);
                    EHCRoundRobin(rcsApps.Id, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 1, false, false, 1);
                    MatchingHelper.UpdateFullyMigratedPLMApplication(_conx, rcsAppId.Value);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.FullyMigrated).Description.ToString();
                    var Result = ActivityTrackerAudit(rcsApps.Id, ActivityTrackerMessage, Customer.Id);
                    return RedirectToAction("PlmManualApplications", "PropertyLeaseApplication");
                }
                catch (Exception e)
                {

                    throw;
                }
            }

        }

        public ActionResult PlmFullyMigartedApplications()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var Keys = cxt.Status;
                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    int UserId = Customer.Id;

                    var DocumentUploadNotRiskAssessment = cxt.Status.FirstOrDefault(i => i.Key == StatusKeys.DocumentUploadNotRiskAssessment).Id;

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && x.IsFullyMigrated)
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.Customer)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.HumanEHCOptions)
                        .Include(r => r.Status).ToList();

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

        public JsonResult FinalLeaseAgreementDocChecker(int id)
        {
            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

            var findItem = db.propertyLeaseAgreementMasters.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == id);
            if (findItem != null)
            {
                if (!findItem.RevenueManagerSigned || !findItem.PropertyManagerSigned)
                {
                    return Json("true", JsonRequestBehavior.AllowGet); // At least one manager hasn't signed yet, no need to check documents
                }
            }

            var ApplicationLeaseAgreementEHC = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ApplicationLeaseAgreementEHC);
            var ApplicationLeaseAgreementEHCChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ApplicationLeaseAgreementEHC.Id && dcl.ReferenceTypeId == referenceType.Id);

            var ApplicationLeaseAgreementEHC_Docs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.PropertyLeaseApplicationId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList ApplicationLeaseAgreementEHC_Docs_CheckList = new DocumentCheckList();
            if (ApplicationLeaseAgreementEHC_Docs != null)
            {
                ApplicationLeaseAgreementEHC_Docs_CheckList =
                    db.DocumentCheckLists.Include(d => d.DocumentType)
                        .FirstOrDefault(c => c.Id == ApplicationLeaseAgreementEHCChecklist.Id && c.IsActive && !c.IsDeleted);
            }

            var ApplicationLeaseAgreementEHC_Docs_Final = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.PropertyLeaseApplicationId == id && o.DocumentCheckListId == ApplicationLeaseAgreementEHC_Docs_CheckList.Id && o.IsActive && !o.IsDeleted).ToList();

            var isChecked = ApplicationLeaseAgreementEHC_Docs_Final.Count > 0;

            return Json(isChecked ? "true" : "false", JsonRequestBehavior.AllowGet);
        }

        public JsonResult CaptureDocChecker2(int id)
        {

            var documentCheckLists = new List<DocumentCheckList>();
            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();

            //var RiskAssessmrnt = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RiskAssessmentOutcomes);
            //var ITC_Check = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ITCCheck);


            //documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ITC_Check.Id && dcl.ReferenceTypeId == referenceType.Id));
            //documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RiskAssessmrnt.Id && dcl.ReferenceTypeId == referenceType.Id));

            //db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.DocumentCheckListId == MunicipalStatementdocCheckList.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();

            
            
            var RiskAssessmentOutcomes = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RiskAssessmentOutcomes);
            var RiskAssessmentChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RiskAssessmentOutcomes.Id && dcl.ReferenceTypeId == referenceType.Id);

            var RiskAssessmentDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.PropertyLeaseApplicationId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList RiskAssessmentOutcomesCheckList = new DocumentCheckList();
            if (RiskAssessmentDocs != null)
            {

                RiskAssessmentOutcomesCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == RiskAssessmentChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var RiskAssessmentDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.PropertyLeaseApplicationId == id && o.DocumentCheckListId == RiskAssessmentOutcomesCheckList.Id && o.IsActive && !o.IsDeleted).ToList();

            //-------------

            var ITCCheck = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ITCCheck);
            var ITCCheckChecklist = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ITCCheck.Id && dcl.ReferenceTypeId == referenceType.Id);

            var ITCCheckDocs = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.PropertyLeaseApplicationId == id && o.IsActive && !o.IsDeleted).FirstOrDefault();
            DocumentCheckList ITCDocsCheckList = new DocumentCheckList();
            if (ITCCheckDocs != null)
            {

                ITCDocsCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == RiskAssessmentChecklist.Id && c.IsActive && !c.IsDeleted);

            }

            var ITCCheckDocsFinal = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.PropertyLeaseApplicationId == id && o.DocumentCheckListId == ITCDocsCheckList.Id && o.IsActive && !o.IsDeleted).ToList();


            var isChecked = false;



            if (RiskAssessmentDocsFinal.Count > 0 && ITCCheckDocsFinal.Count > 0)
            {
                isChecked = true;
            }

            string result = "";
            if (isChecked == false)
            {
                result = "false";
            }
            else
            {
                result = "true";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
