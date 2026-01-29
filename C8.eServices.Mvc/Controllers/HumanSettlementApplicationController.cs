using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.ViewModels;
using C8.eServices.Mvc.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Globalization;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ApiServices;
using System.Web.Routing;
using System.Net;
using System.Data.Entity.Core.Objects;
using Newtonsoft.Json.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using iTextSharp.text.pdf;
using System.Net.Sockets;
using System.Web.Http.Results;
using System.Data;

namespace C8.eServices.Mvc.Controllers
{
    public class HumanSettlementApplicationController : Controller
    {

        public string encp = "spgencpassp";
        private eServicesDbContext db = new eServicesDbContext();
        private static Random random = new Random();
        BaseHelper _base = new BaseHelper();

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

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        public ActionResult Dashboard()
        {
           // var limsdata = LIMSv2.GetLimsIdenityLinkedProperties(string.Format("8307240099086").Trim());
            Initialise();
            eServicesDbContext core = new eServicesDbContext();
            DashboardViewModel dashboardViewModel = new DashboardViewModel();
            ViewBag.User = SystemUser.UserFullName;
            var rrrr = core.HumanSettlementApplications.ToList();
            var agr = core.HSLeaseAgreementMasters.ToList();
            dashboardViewModel.AllApplications = rrrr.Count();
            dashboardViewModel.MasterApplications = rrrr.Where(a => a.IsMaster).ToList().Count();
            dashboardViewModel.NewApplications = rrrr.Where(a => !a.IsMaster).ToList().Count();
            dashboardViewModel.CurrentAgreeentsSigned = agr.Where(a => !string.IsNullOrEmpty(a.ApplicantSignature) && !string.IsNullOrEmpty(a.Applicant_sign_date)).ToList().Count();
            var RoundRobinQueues = core.RoundRobinQueues.Include(d => d.Status).Include(d => d.ResponsibilityType).Include(d => d.HumanSettlementApplication.Status)
                .Where(a => a.HumanSettlementApplicationId != null).ToList();

            if (((User.IsInRole("Housing Liaison Officer")) || (User.IsInRole("Senior Housing Specialist")) || (User.IsInRole("Regional Manager")))) 
            {
                RoundRobinQueues = RoundRobinQueues.Where(a => a.ClerkId == Customer.Id).ToList();
            }

            if (User.IsInRole("Housing Liaison Officer") || (User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
            {
                dashboardViewModel.RenewalFirstRecommendation = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.RenewalFirstRecommendation);
                dashboardViewModel.AgreementRenewalAcceptance = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.AgreementRenewalAcceptance);
                dashboardViewModel.RenewalUploadDocs = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.RenewalUploadDocs);
                dashboardViewModel.GenerateLeaseAgreement = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.GenerateLeaseAgreement);
                dashboardViewModel.AgreementSignature = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.AgreementSignature);
                dashboardViewModel.FinalizeAgreement = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.FinalizeAgreement);
                dashboardViewModel.UpdateAgreement = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.UpdateAgreement);
                dashboardViewModel.NoticedAgreements = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.NoticedAgreements);
                dashboardViewModel.PendingTakeOff = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.PendingTakeOff);
                dashboardViewModel.PostInspection = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.PostInspection);
            }
            if (User.IsInRole("Senior Housing Specialist") || (User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
            {
                dashboardViewModel.Renewal2ndRecommendation = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.Renewal2ndRecommendation);
                dashboardViewModel.AgreementReview = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.AgreementReview);
                dashboardViewModel.PendingTerminationreview = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.PendingTerminationreview);
            }
            if (User.IsInRole("Regional Manager") || (User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
            {
                var test = RoundRobinQueues.Where(a => a.ResponsibilityType.Key == ResponsibilityTypeKeys.RenewalReview).ToList();
                dashboardViewModel.RenewalReview = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.RenewalReview);
                dashboardViewModel.AgreementApproval = GetRoundRobinQueues(RoundRobinQueues, ResponsibilityTypeKeys.AgreementApproval);
   
            }

            return View(dashboardViewModel);

        }

        public static List<RoundRobinQueue> GetRoundRobinQueues(List<RoundRobinQueue> roundRobinQueues, string ResponsibilityTypeKey) 
            => roundRobinQueues.Where(a => a.ResponsibilityType.Key == ResponsibilityTypeKey).ToList();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [DecryptParameter]
        public bool pdfDenerateAgreementOfLease(int? ApplicationId)
        {
            var core = new eServicesDbContext();
            var application = core.HumanSettlementApplications
                .Include(r => r.CoTitleType)
                .Include(r => r.Relationship)
                .Where(x => x.Id == ApplicationId).FirstOrDefault();
            var Agreement = core.HSLeaseAgreementMasters.FirstOrDefault(r => r.HumanSettlementApplicationId == application.Id) ?? null;
            var template = core.AppSettings.FirstOrDefault(r => r.Key == AppSettingKeys.AOL).Value;

            //var file = "/PDFTemplates/HS_AgreementOfLease.pdf";
            //var url = Request.Url.AbsoluteUri.ToString();
            //if (url.Contains("/PLM")) file = string.Format("/{0}{1}", url.Split('/').ToList().SingleOrDefault(d => d.Contains("PLM")), file);
            //var pdfTemplate = Server.MapPath(file);



            var file = "/PDFTemplates/HS_AgreementOfLease.pdf";
            var url = Request.Url.LocalPath; // Use LocalPath to get the path without the domain
            if (url.Contains("/PLM"))
            {
                file = file.Replace("/PLM", "/PLM_HSD");
            }
       var pdfTemplate = Server.MapPath("~" + file);

   
      

            var timestamp2 = DateTime.Now.ToString("ddMMyyyyHHmmss");
            string folderName = Server.MapPath("~/Templates");
            string pathString = System.IO.Path.Combine(folderName, timestamp2);
            System.IO.Directory.CreateDirectory(pathString);
            string nFolderName = null;
            string newFile = nFolderName = folderName;
            Random rnd = new Random();
            int randomNum = rnd.Next(1, 51);
            newFile = nFolderName + "\\" + timestamp2 + "_" + application.IDNo + "_PLMLeaseAgreement.pdf";
            var filename = application.ApplicationReferenceNumber + ".AgreementOfLease.pdf";
            var sign = application.FirstName.Substring(0, 1) + application.LastName.Substring(0, 1) + " " + application.IDNo;

            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
            AcroFields pdfFormFields = pdfStamper.AcroFields;

            if (Agreement != null)
            {
                //DO NOT REMOVE THE SPACES
                pdfFormFields.SetField("ApplicantName", Agreement.ApplicantFullName.ToString());
                pdfFormFields.SetField("IdentityNumber", Agreement.IdentityNumber.ToString());
                //DO NOT REMOVE THE SPACES
                pdfFormFields.SetField("TheUnit", "                                                                  " +//DO NOT REMOVE THE SPACES
                    Agreement.TheUnit.ToString() +
                    " (hereinafter called THE UNIT)");
                //DO NOT REMOVE THE SPACES
                pdfFormFields.SetField("duration_terminatioln", "                                                        " +//DO NOT REMOVE THE SPACES
                    Agreement.CommenceDate.ToString() +
                    " and expire on  " + Agreement.ExpireryDate.ToString() +
                    " (period not to exceed two years) unless terminated prematurely in accordance with clause 5.2 hereof");
                //DO NOT REMOVE THE SPACES
                pdfFormFields.SetField("PayableAmount", "                                                                                                           " +//DO NOT REMOVE THE SPACES
                    Agreement.PayableAmount.ToString("c") + " per month until the annual increase as stipulated in clause 6.3.");
                if (Agreement.Nominated)
                {
                    pdfFormFields.SetField("NomineeName", Agreement.NoName == null ? "N/A" : Agreement.NoName.ToString());
                    pdfFormFields.SetField("NomineeTitle", Agreement.NoTitle == null ? "N/A" : Agreement.NoTitle.ToString());
                    pdfFormFields.SetField("NomineeSurname", Agreement.NoSurname == null ? "N/A" : Agreement.NoSurname.ToString());
                    pdfFormFields.SetField("NomineeIdentityNo", Agreement.NoIdentityNumber == null ? "N/A" : Agreement.NoIdentityNumber.ToString());
                    pdfFormFields.SetField("NomineeContacts", Agreement.NoContact == null ? "N/A" : Agreement.NoContact.ToString());
                    pdfFormFields.SetField("NomineesRelation", Agreement.NoRelationship == null ? "N/A" : Agreement.NoRelationship.ToString());

                }
                else
                {
                    pdfFormFields.SetField("NomineeName", string.Format("{0}", application.CoFirstName ?? ""));
                    pdfFormFields.SetField("NomineeTitle", application.CoTitleType?.Name);
                    pdfFormFields.SetField("NomineeSurname", string.Format("{0}", application.CoLastName ?? ""));
                    pdfFormFields.SetField("NomineeIdentityNo", string.Format("N/A"));
                    pdfFormFields.SetField("NomineeContacts", string.Format("(C) {0} | (E) {1} ", application.CoCell ?? "", application.CoEmail ?? ""));
                    pdfFormFields.SetField("NomineesRelation", application.Relationship?.Name);
                }
                if (!string.IsNullOrEmpty(Agreement.ApplicantSignature))
                {
                    pdfFormFields.SetField("lessee_date", Agreement.Applicant_sign_date.ToString().ToUpper());
                    pdfFormFields.SetField("Applicant_signature_es_:signer:signature", Agreement.ApplicantSignature.ToString());
                    pdfFormFields.SetField("Applicants_second_witness", Agreement.aa_Witness_two == null ? "N/A" : Agreement.aa_Witness_two.ToString());
                    pdfFormFields.SetField("Applicants_first_witness", Agreement.aa_Witness_one == null ? "N/A" : Agreement.aa_Witness_one.ToString());

                    pdfFormFields.SetField("NomineesSignature_es_:signer:signature", Agreement.NomineeSignature == null ? "N/A" : Agreement.NomineeSignature.ToString());

                }
                if (!string.IsNullOrEmpty(Agreement.LessorSignature))
                {
                    pdfFormFields.SetField("lessor_date", Agreement.Lessor_sign_date.ToString().ToUpper());
                    pdfFormFields.SetField("lessors_signature_es_:signer:signature", Agreement.LessorSignature.ToString());
                    pdfFormFields.SetField("lessors_first_witness", Agreement.rr_Witness_one == null ? "N/A" : Agreement.rr_Witness_one.ToString());
                    pdfFormFields.SetField("lessors_second_witness", Agreement.rr_Witness_two == null ? "N/A" : Agreement.rr_Witness_two.ToString());
                }

            }
            //pdfFormFields.SetField("Reference_no", application.ApplicationReferenceNumber.ToString()); //????? ?????? ?????? ?????? ?????? ?????? ????? ?????? ?????? ?????? ?????? ?????? ??????

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

            return true;
        }

        [DecryptParameter]
        public void pdfDeneratePropertyLeaseAgreement(int? ApplicationId)
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
            var filename = timestamp2 + "_" + application.IDNo + "LEASEAGREEMENT.pdf";

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
                var sign_p = master.PropertyManagerSigned == true ? pdfFormFields.SetField("PropertyManagersSignature", master.PropertyManagersSignature.ToString()) : true;
                var m_sndt = master.PropertyManagerSigned == true && master.RevenueManagerSigned == true ? pdfFormFields.SetField("ManagersSignDate", master.ManagersSignDate.ToString()) && pdfFormFields.SetField("ManagersSignDay", master.ManagersSignDay.ToString()) : true;
                var sign_r = master.RevenueManagerSigned == true ? pdfFormFields.SetField("RevenueManagersSignature", master.RevenueManagersSignature.ToString()) : true;
                var t_wtn1 = master.TenantWitnessONE == null ? true : pdfFormFields.SetField("TenantsWitness1", master.TenantWitnessONE.ToString());
                var t_wtn2 = master.TenantWitnessTWO == null ? true : pdfFormFields.SetField("TenantsWitness2", master.TenantWitnessTWO.ToString());
                var m_wtn1 = master.ManagersWitnessONE == null ? true : pdfFormFields.SetField("ManagersWitness1", master.ManagersWitnessONE.ToString());
                var m_wtn2 = master.ManagersWitnessTWO == null ? true : pdfFormFields.SetField("ManagersWitness2", master.ManagersWitnessTWO.ToString());
                var m_lesee = master.MainLesseeSigned == true ? pdfFormFields.SetField("SignatureMainLessee", master.SignatureMainLessee.ToString()) : true;
                var spousee = master.SpouseSigned == true ? pdfFormFields.SetField("SignatureOFSpouse", master.SignatureOfSpouse.ToString()) : true;

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

            var leaseInfo = db.PropertyLeaseApplications.Include(x => x.Customer).FirstOrDefault(x => x.Id == id);
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

                return View("_Error");
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
                var User = GetBackOfficeId(db, rcsApps.Id, false);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor).FirstOrDefault().Value);
                var UserId = User.Id != 0 ? User.Id : activeDirectoryOn;

                maintenance.Inspection = false;
                db.Entry(maintenance).State = EntityState.Modified;
                db.SaveChanges();

                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.UnitMaintenanance).FirstOrDefault();
                MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);

            }
            return RedirectToAction("PropertyLeaseInspections");
        }



        [Authorize]
        [DecryptParameter]
        public ActionResult AllocateAvailableUnitMatch(int ApplicationId, string Data)
        {
            if (ApplicationId == 0) return View("_Error");
            try
            {
                var context = new eServicesDbContext();
                Initialise();
                var PLA = context.HumanSettlementApplications
                    .Include(x => x.Status)
                    .Include(x => x.PurchaserType)
                    .Include(x => x.Customer)
                    .Include(x => x.SystemUser).FirstOrDefault(x => x.Id == ApplicationId);
                PLA.Data = Data;
                var cmv = new CaptureViewModel
                {
                    HumanSettlementApplication = PLA,
                    vaIDno = PLA.Id.ToString()
                };

                ViewBag.PreferredArea = new SelectList(db.PreferredComplexAreas.OrderBy(x => x.Name).Where(x => x.IsActive && x.Key.Contains("h_")).ToList(), "Id", "Name");
                var cc = db.PreferredComplexAreas.Select(d => d.Id).ToList();
                ViewBag.PreferredUnit = new SelectList(new List<UnitsEkurhuleniHousingCompany>(), "Id", "SpaceUnitNo");
                ViewBag.HumanSettlementApplicationId = PLA.Id;
                return View(cmv);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
                ///throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AllocateAvailableUnitMatch(int? HumanSettlementApplicationId, CaptureViewModel capture, int PreferredArea, int PreferredUnitID, string data)
        {
            using (var core = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var ActivityTrackerMessage = "";
                    int emailboodyId = 0;
                    var Key = core.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRiskAssessment);
                    var PLA = core.HumanSettlementApplications.FirstOrDefault(x => x.Id == capture.HumanSettlementApplication.Id);

                    var match = new MatchedUnits
                    {
                        UnitsHumanSettlement01Id = PreferredUnitID,
                        HumanSettlementApplicationId = PLA.Id,
                        RejectedProperty = false,
                        IsAccepted = false,
                        IsActive = true,
                        CreatedBySystemUserId=SystemUser.Id
                    };
                    core.MatchedUnits.Add(match);
                    core.SaveChanges();

                    var unit = core.UnitsHumanSettlement01s.FirstOrDefault(a => a.Id == PreferredUnitID);
                    unit.IsTaken = true;
                    core.Entry(unit).State = EntityState.Modified;
                    core.SaveChanges();

                    ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SubmitForRiskAssessmrnt).Description.ToString();
                    emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationAssignedToRiskAssessment).Id;

                    MatchingHelper.ChangeHumanStatus(core, Key.Id, PLA.Id);
                    EmailHelper.CustomerEmailOrSMSNotification(core, PLA.Id, emailboodyId);
                    MatchingHelper.ActivityTrackerHuman(core, PLA.Id, ActivityTrackerMessage, Customer.Id);

                    return RedirectToAction("WaitingListQueue", "HumanSettlementApplication", new { q = capture.HumanSettlementApplication.Data });
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    return View("_Error");
                    ///throw;
                }
            }

        }



        [Authorize]
        [DecryptParameter]
        public ActionResult RequirementChange(int ApplicationId, string Data)
        {
            var core = new eServicesDbContext();
            try
            {
                var plmApps = core.HumanSettlementApplications
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                    .Include(r => r.Status)
                    .Include(r => r.UnitTypology)
                    .Include(r => r.UnitCategory)
                    .Include(r => r.HSIncomeBracket)
                    .Include(r => r.PurchaserType)
                    .Where(x => x.Id == ApplicationId).FirstOrDefault();

                var Request = core.WaitingQueueChangeRequests.OrderByDescending(a => a.Id)
                    .Include(c=>c.PreferredComplexAreaOne) .Include(c=>c.PreferredComplexAreaTwo)
                    .Include(c=>c.UnitCategory).Include(c=>c.UnitTypology)
                    .Where(d => d.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

                ViewBag.ApplicationId = plmApps.Id;
                ViewBag.Data = Data;

                var vm = new DepartmentsApprovalViewModel
                {
                    HumanSettlementApplication = plmApps,
                    _Data = Data,
                    WaitingQueueChangeRequest = Request
                };
                ViewBag.ApprovalStatus = new SelectList(core.RCSActionTypes.Where(d => d.Key == RCSActionTypeKeys.Approved || d.Key == RCSActionTypeKeys.Disprove).ToList(), "Key", "Name");
                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //throw;
                return View("_Error");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult RequirementChange(int ApplicationId, string Data, string ApprovalStatusddl)
        {
            var core = new eServicesDbContext();
            try
            {
                _base.Initialise(core);
                var plmApps = core.HumanSettlementApplications
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                    .Include(r => r.Status)
                    .Include(r => r.UnitTypology)
                    .Include(r => r.UnitCategory)
                    .Include(r => r.HSIncomeBracket)
                    .Include(r => r.PurchaserType)
                    .Where(x => x.Id == ApplicationId).FirstOrDefault();

                var Request = core.WaitingQueueChangeRequests.OrderByDescending(a => a.Id)
                    .Include(c=>c.PreferredComplexAreaOne) .Include(c=>c.PreferredComplexAreaTwo)
                    .Include(c=>c.UnitCategory).Include(c=>c.UnitTypology)
                    .Where(d => d.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

                var wait = core.WaitingListQueueHumans.OrderByDescending(d => d.Id).FirstOrDefault(a => a.HumanSettlementApplicationId == plmApps.Id);
                var ActivityMessage = string.Empty;
                switch (ApprovalStatusddl)
                {
                    case RCSActionTypeKeys.Approved:
                        wait.IsApproved = true;
                        plmApps.PreferredComplexAreaId = Request.PreferredComplexAreaOneId;
                        plmApps.PreferredComplexArea2Id = Request.PreferredComplexAreaTwoId;
                        plmApps.UnitCategoryId = Request.UnitCategoryId;
                        plmApps.UnitTypologyId = Request.UnitTypologyId;
                        core.Entry(plmApps).State = EntityState.Modified;
                        core.SaveChanges();
                        ActivityMessage = core.ActivityTrackerMessages.FirstOrDefault(a => a.Key == ActivityTrackerMessageKeys.AcceptCreditScore).Description;
                        break;
                    case RCSActionTypeKeys.Disprove:
                        wait.IsDisapproved = true;
                        ActivityMessage = core.ActivityTrackerMessages.FirstOrDefault(a => a.Key == ActivityTrackerMessageKeys.AcceptCreditScore).Description;
                        break;
                }
                core.Entry(wait).State = EntityState.Modified;
                core.SaveChanges();

                var Key = core.Status.FirstOrDefault(a => a.Key == StatusKeys.AwaitingUnitOffers);
                MatchingHelper.ChangeHumanStatus(core, Key.Id, plmApps.Id);
                MatchingHelper.ActivityTrackerHuman(core, plmApps.Id, ActivityMessage, _base.Customer.Id);

                return RedirectToAction("Inbox");
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //throw;
                return View("_Error");
            }
        }


        [Authorize]
        [DecryptParameter]
        public ActionResult WaitingListQueue(string ViewName)
        {
            try
            {
                using (var core = new eServicesDbContext())
                {
                    Initialise();
                    var rrq = core.HumanSettlementApplications
                    .Include(c => c.PurchaserType)
                    .Include(c => c.Status)
                    .Include(c => c.PreferredComplexArea).Include(c => c.PreferredComplexArea2)
                    .Include(c => c.UnitTypology).Include(c => c.UnitCategory).Include(c => c.HSIncomeBracket)
                    .Where(d => d.IsActive && d.Status.Key == StatusKeys.AwaitingUnitOffers).ToList();
                    var waits = core.WaitingListQueueHumans.OrderBy(a => a.Position).Where(d => d.IsActive).ToList();
                    var wait = waits;
                    var WaitingListQueueData = new List<TemporaryDisplayModel>();
                    var availble = core.UnitsHumanSettlement01s.ToList();
                    foreach(var queue in waits)
                    {
                        var item = rrq.FirstOrDefault(a => a.Id == queue.HumanSettlementApplicationId);
                        if (item != null)
                        {
                            var QueItemUnit = availble.FirstOrDefault(d => d.UnitCategoryId == item.UnitCategoryId && d.UnitTypologyId == item.UnitTypologyId && (d.PreferredComplexAreaId == item.PreferredComplexAreaId || d.PreferredComplexAreaId == item.PreferredComplexArea2Id));
                            int index = rrq.FindIndex(a => a.Id == queue.HumanSettlementApplicationId);
                            if (QueItemUnit != null)
                            {
                                var count = availble.Count();
                                rrq[index].Initial = FlagKeys.AvailableMatchingUnit;
                                availble.Remove(QueItemUnit);
                            }
                            else
                                rrq[index].Initial = String.Format("a_matching_unavailable");
                        }
                    }
                    switch (ViewName)
                    {
                        case ViewCodeKeys.ApprovedCahnge:
                            waits = waits.Where(d => d.IsApproved && d.IsNew).ToList();
                            ViewBag.ViewFrom = "Approved Unit Change";
                            break;
                        case ViewCodeKeys.DisapprovedCahnge:
                            waits = waits.Where(d => d.IsDisapproved && d.IsNew).ToList();
                            ViewBag.ViewFrom = "Disapproved Unit Change";
                            break;
                        case ViewCodeKeys.PendingTransfer:
                            waits = waits.Where(d => d.IsTransfer).ToList();
                            ViewBag.ViewFrom = "In Awaiting Transfer Approval";
                            break;
                        case ViewCodeKeys.PendingUnitMatch:
                            waits = waits.Where(d => !d.IsDisapproved && !d.IsApproved && !d.IsTransfer).ToList();
                            ViewBag.ViewFrom = "In Awaiting Available Unit match";
                            break;
                    }
                    var rr = waits.Select(d => d.HumanSettlementApplicationId).ToList();
                    rrq = rrq.Where(d => rr.Contains(d.Id)).ToList();
                    foreach (var item in rrq)
                    {
                        var data = new TemporaryDisplayModel();
                        var Q = wait.FirstOrDefault(d => d.HumanSettlementApplicationId == item.Id);
                        item.Data = string.Format("{0}", new AesCrypto().Encrypt("ViewName=" + ViewName));
                        item.ColorCode = string.Format("{0}", (int)Q.Position);
                        var q = waits.Where(x => x.HumanSettlementApplicationId == item.Id).FirstOrDefault();
                        data.WaitingListQueueId = q.Id;
                        data.Order = (int)q.Position;
                        data.WaitingListQueueDate = q.QueueDate.ToString().Substring(0, 10);
                        data.IsRelisted = q.IsReListed;
                        data.Position = string.Format("Waiting List No. {0}", q.Position);
                        data.ApplicantFullName = string.Format("{0} {1}", item.FirstName, item.LastName);
                        data.ApplicationReferenceNo = item.ApplicationReferenceNumber;
                        data.BedRooms = item.UnitTypology.Name;
                        data.HumanSettlementApplicationId = (int)q.HumanSettlementApplicationId;
                        data.ComplexAreas = (item.PreferredComplexArea.Name == item.PreferredComplexArea2.Name) ? item.PreferredComplexArea.Name : string.Format("{0}, {1}", item.PreferredComplexArea.Name, item.PreferredComplexArea2.Name);
                        data.Data = string.Format("{0}", new AesCrypto().Encrypt("ViewName=" + ViewName));
                        data.ColorCode = string.Format("{0}", (int)Q.Position);
                        data.PendingAllocationTransfer = Q.IsTransfer;
                        data.WaitingQueueChangedApproved = Q.IsApproved;
                        data.WaitingQueueChangedDisapproved = Q.IsDisapproved;
                        data.AvailabilityKey = item.Initial;
                        WaitingListQueueData.Add(data);
                    }
                    ViewBag.QueueSum = WaitingListQueueData.Count();
                    WaitingListQueueData.OrderBy(d => d.Position);
                    return View(WaitingListQueueData);
                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
                ///throw;
            }
        }

        [Authorize]
        [DecryptParameter]
        public ActionResult WaitingQueueChangeRequest(int WaitingListQueueId, int HumanSettlementApplicationId, string ApplicantFullName, string Position, string data)
        {
            try
            {
                using(var core = new eServicesDbContext())
                {
                    int p = (int)core.WaitingListQueueHumans.FirstOrDefault(x => x.Id == WaitingListQueueId).Position;
                    var pp = core.HumanSettlementApplications.
                        Include(r => r.PreferredComplexArea).Include(r => r.PreferredComplexArea2)
                        .Include(r => r.UnitTypology).Include(r => r.UnitCategory).Include(r => r.HSIncomeBracket).Where(x => x.Id == HumanSettlementApplicationId).FirstOrDefault();
                    TemporaryDisplayModel vm = new TemporaryDisplayModel()
                    {
                        WaitingListQueueId = WaitingListQueueId,
                        Queue = 99,
                        Position = Position,
                        ApplicationReferenceNo = pp.ApplicationReferenceNumber,
                        HumanSettlementApplicationId = HumanSettlementApplicationId,
                        ApplicantFullName = ApplicantFullName,
                        ItemPosition = p,
                        ComplexAreas = (pp.PreferredComplexAreaId != pp.PreferredComplexArea2Id) ? String.Format("{0}, {1}", pp.PreferredComplexArea.Name, pp.PreferredComplexArea2.Name) : pp.PreferredComplexArea.Name,
                        BedRooms = pp.UnitTypology.Name,
                        HumanSettlementApplication = pp,
                        Data = data
                    };
                    var rrj = core.HSUnitTypologies.ToList();
                    ViewBag.ComplexOptionOne = new SelectList(core.PreferredComplexAreas.Where(x => x.IsActive && x.Key != "ekurhuleni_complex" && x.Key.Contains("h_")).ToList(), "Id", "Name");
                    ViewBag.ComplexOptionTwo = new SelectList(core.PreferredComplexAreas.Where(x => x.IsActive && x.Key != "ekurhuleni_complex" && x.Key.Contains("h_")).ToList(), "Id", "Name");
                    ViewBag.Typology = new SelectList(core.HSUnitTypologies.Where(x => x.IsActive).ToList(), "Id", "Name");
                    ViewBag.Category = new SelectList(core.HSUnitCategories.Where(x => x.IsActive).ToList(), "Id", "Name");
                    return View(vm);
                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
                ///throw;
            }
        }

        //[Authorize(Roles = "Area Manager,Back Office System Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WaitingQueueChangeRequest(TemporaryDisplayModel vm, int HumanSettlementApplicationId, string data, int ComplexOptionOneId, int ComplexOptionTwoId, int TypologyId, int CategoryId)
        {
            try
            {
                using (var core = new eServicesDbContext())
                {
                    _base.Initialise(core);
                    var PLA = core.HumanSettlementApplications.Where(x => x.Id == HumanSettlementApplicationId).FirstOrDefault();
                    var waitRequest = new WaitingQueueChangeRequest
                    {
                        HumanSettlementApplicationId = PLA.Id,
                        UnitCategoryId = CategoryId,
                        UnitTypologyId = TypologyId,
                        RequestByUserId = _base.SystemUser.Id,
                        PreferredComplexAreaOneId = ComplexOptionOneId,
                        PreferredComplexAreaTwoId = ComplexOptionTwoId,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false
                    };
                    core.WaitingQueueChangeRequests.Add(waitRequest);
                    core.SaveChanges();

                    var Key = core.Status.FirstOrDefault(d => d.Key == StatusKeys.RequestRequirementChange);
                    var Activity = core.ActivityTrackerMessages.FirstOrDefault(a => a.Key == ActivityTrackerMessageKeys.RequirementCnhange);
                    MatchingHelper.ChangeHumanStatus(core, Key.Id, PLA.Id);
                    MatchingHelper.ActivityTrackerHuman(core, PLA.Id, Activity.Description, _base.Customer.Id);
                    return RedirectToAction("WaitingListQueue", new { q = data });
                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
                ///throw;
            }
        }



        [DecryptParameter]
        public ActionResult WaitingListReEntryConfirmation(int id)
        {
            var context = new eServicesDbContext();
            Initialise();
            PropertyLeaseApplication rcsApps = null;
            LeaseDetails leaseDetails = null;

            rcsApps = db.PropertyLeaseApplications.Where(x => x.Id== id).Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.Status).Include(x => x.PurchaserType).FirstOrDefault();


            ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.PlmYes || x.Key == RCSActionTypeKeys.PlmNo).OrderBy(x => x.Name), "Key", "Name");

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
                int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
            }
            if (ApprovalStatusddl == RCSActionTypeKeys.PlmNo)
            {

                //customer email here
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.WaitingListExit).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, rcsApps.Id, ActivityTrackerMessage, Customer.Id);
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

            return View("_Error");
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

                return View("_Error");
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
                MeetingRequest meeting = dvm.MeetingRequest;
                meeting.PropertyLeaseApplicationId = (int)id;
                meeting.CustomerId = userID;
                cxt.MeetingRequests.Add(meeting);
                cxt.SaveChanges();

                int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InviteTenantForTraining).Id;
                EmailHelper.CustomerEmailNotification(cxt, rcsApps.Id, emailboodyId);
                MatchingHelper.ChangeApplicationStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id, rcsApps.Id);

                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.CommunityDevelopmentOfficer).FirstOrDefault().Value);
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.InviteToClientTraining).FirstOrDefault();
                MatchingHelper.RoundRobinMarkJobAsFinished(cxt, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, activeDirectoryOn);

                EHCRoundRobin(rcsApps.Id, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                return RedirectToAction("PropertyLeaseTenantTraining");
            }

        }

        [DecryptParameter]
        public ActionResult GenerateLeaseAgreement(int ApplicationId, string IsMaster, string Data)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var result = Convert.ToBoolean(IsMaster);
                    var Application = db.HumanSettlementApplications.Where(x => x.IsDeleted == false && x.Id == ApplicationId).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.PreferredComplexArea).Include(r => r.PreferredComplexArea2)
                      .Include(r => r.TitleType).Include(r => r.SecAppTitleType)
                      .Include(r => r.PurchaserType).Include(r => r.Status).FirstOrDefault();
                    result = Application.IsMaster;
                    var leaseApplication = db.HumanSettlementLeaseMasters.Where(x => x.IsDeleted == false && x.HumanSettlementApplicationId == ApplicationId).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.ModifiedBySystemUser).Include(r => r.Status).FirstOrDefault() ?? null;

                    var findItem = db.HumanSettlementLeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.ModifiedBySystemUser).Include(r => r.Status).FirstOrDefault() ?? null;

                    if (result) MatchingHelper.HumanSettlementMasterAgreement(cxt, leaseApplication, Application, null);
                    if (!result) MatchingHelper.HumanSettlementMasterAgreement(cxt, leaseApplication, Application, findItem);

                    var docdets = cxt.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.AgreementOfLease);
                    var attachments = cxt.Attachments.Where(x => x.PropertyLeaseApplicationId == Application.Id && x.DocumentTypeId == docdets.Id).ToList();
                    MatchingHelper.ChangeHumanStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingLeaseAgreementReview).Id, Application.Id);

                    var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);

                    var ResponsibilityTypeId = cxt.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement).Id;
                    
                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(cxt, Application.Id, ResponsibilityTypeId, Customer.Id);

                    WorkAllocationHumanHelper.AgreementOfLease((int)Application.Id, false, true, false, false, false, false);

                    var custmusers = cxt.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = cxt.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.PropertyLeaseAgreementGenerated).Description.ToString();
                    MatchingHelper.ActivityTrackerHuman(cxt, Application.Id, ActivityTrackerMessage, custmusers.Id);

                    //Send e-mail and SMS notification
                    int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                    EmailHelper.CustomerEmailOrSMSNotification(cxt, Application.Id, emailboodyId);

                    Application.Data = Data;

                    Session["GenerateLeaseAgreement"] = "true";

                    var vm = new DepartmentsApprovalViewModel()
                    {
                        Attachments = attachments,
                        HumanSettlementApplication = Application,
                        DocName = docdets.Name,
                        DocDesc = docdets.Description
                    };
                    ViewBag.PropertyLeaseAppliactionId = Application.Id;
                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
            }
        }


        [DecryptParameter]
        public ActionResult DebitOrderRegistration(int rcsAppId, string Data)
        {
            try
            {
                var rcsApp = db.PropertyLeaseApplications.Include(r => r.PurchaserType).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.IdentificationType).FirstOrDefault(x => x.Id == rcsAppId && !x.IsDeleted);
                var matchedUnits = db.MatchedUnits.Where(x => x.PropertyLeaseApplicationId == rcsApp.Id).ToList().FirstOrDefault();
                var units = db.Units.Where(x => x.Id == matchedUnits.UnitsId).FirstOrDefault() ?? null;
                var unit = db.UnitsEkurhuleniHousingCompany.Include(r => r.PreferredComplexArea).Where(x => x.Id == matchedUnits.UnitsEkurhuleniHousingCompanyId).FirstOrDefault() ?? null;
                var debitorder = db.DebitOrderRegistrations.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApp.Id) ?? null;
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
                try
                {

                }
                catch (Exception)
                {

                    throw;
                }
                ViewBag.UnitsID = unit.Id;
                ViewBag.RentalAmount = unit.PropertyPrice.ToString("C");
                ViewBag.Applicant = rcsApp.FirstName + " " + rcsApp.LastName;
                ViewBag.UNITNo = unit.SpaceUnitNo.ToUpper().ToString() + ", " + unit.PreferredComplexArea.Name.ToUpper();
                ViewBag.Data = Data;
                return View(vm);
            }
            catch (Exception IO)
            {
                throw;
            }
            return View();
        }

        [HttpPost]
        public ActionResult DebitOrderRegistration(DepartmentsApprovalViewModel model, string Data, int UnitsID)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var findItem = context.DebitOrderRegistrations.OrderByDescending(r => r.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == model.PropertyLeaseApplications.Id) ?? null;
                    var units = context.UnitsEkurhuleniHousingCompany.Include(r => r.PreferredComplexArea).FirstOrDefault(x => x.Id == UnitsID);
                    if (findItem != null)
                    {
                        var propertyapplication = context.PropertyLeaseApplications.FirstOrDefault(x => x.Id == model.PropertyLeaseApplications.Id);
                        var debit = findItem;
                        debit = model.DebitOrderRegistration;
                        debit.Email = debit.Email != null ? debit.Email : propertyapplication.PurEmail;
                        debit.BankName = model.DebitOrderRegistration.BankName;
                        debit.BankNumber = model.DebitOrderRegistration.BankNumber;
                        debit.UnitNumber = units.SpaceUnitNumber.ToUpper() + ", " + units.PreferredComplexArea.Name.ToUpper();
                        debit.Email = model.DebitOrderRegistration.Email;
                        debit.CellNo = model.DebitOrderRegistration.CellNo;
                        debit.PropertyLeaseApplicationId = propertyapplication.Id;
                        debit.TenantSignature = true;
                        debit.DebitCheckActionDate = DateTime.Now;
                        debit.RentalAmount = units.PropertyPrice.ToString();
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
                        debit.UnitNumber = units.SpaceUnitNumber.ToUpper() + ", " + units.PreferredComplexArea.Name.ToUpper();
                        debit.PropertyLeaseApplicationId = propertyapplication.Id;
                        debit.Email = debit.Email != null ? debit.Email : propertyapplication.PurEmail;
                        debit.TenantSignature = true;
                        debit.DebitCheckActionDate = DateTime.Now;
                        debit.RentalAmount = units.PropertyPrice.ToString();
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

                    var leased = db.LeaseDetails.OrderByDescending(x => x.Id).Where(x => !x.IsDeleted && x.IsNew).Include(r => r.CreatedBySystemUser)
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
                        LeaseDetails = leased,
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
                    Test2();
                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
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

                return View("_Error");
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
                    if (PropertyLeaseApp.Status.Key == StatusKeys.AwaitingRiskAssessment)
                    {
                        Session["MessageBody"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.PLMSuccessMessage).FirstOrDefault().Body + PropertyLeaseApp.ApplicationReferenceNumber;
                        Session["MessageTitle"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.PLMSuccessMessage).FirstOrDefault().Title + PropertyLeaseApp.ApplicationReferenceNumber;
                    }
                    else
                    {

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
                    return View("_Error");
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
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
                    var rCSApplicationStatus = db.UnitsEkurhuleniHousingCompany.Include(r => r.PreferredComplexArea).Include(r => r.HumanEHCOption).Where(x => x.IsDeleted == false && !x.IsTaken && x.PreferredComplexArea.HousingSuperId == Customer.Id).ToList();
                    if (User.IsInRole("Housing Supervisor"))
                    {
                        rCSApplicationStatus = db.UnitsEkurhuleniHousingCompany.Include(r => r.PreferredComplexArea).Include(r => r.HumanEHCOption).Where(x => x.IsDeleted == false && !x.IsTaken && x.PreferredComplexArea.HousingSuperId == Customer.Id).ToList();
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

                return View("_Error");
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

                    if (User.IsInRole("Housing Supervisor"))
                    {
                        var Areas = db.PreferredComplexAreas.Where(x => x.HousingSuperId == Customer.Id).ToList();
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

                return View("_Error");
            }
        }

        public JsonResult ValidateWaitingListSorting()
        {
            try {
                var val = db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting).Value;
                bool result = Convert.ToBoolean(Convert.ToInt16(db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting).Value));
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception error) {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ReturnError()
        {
            return View("_Error");
        }

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
                    return address + "/" + Ip2 + "/" + System.Web.HttpContext.Current.Request.UserHostAddress;
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

                    var limsToken = Lims10.LIMSApi("IDnXdNA_F3UTQs5k8Ls0KNjBXIoa");


                    var months = 36;
                    double currentprice = 100000;
                    string payout = string.Empty;
                    for (var i=1; i <= months; i++)
                    {
                        var amount = (currentprice.ToString().Length > 6 && currentprice.ToString().Length < 10) ? "M" : (currentprice.ToString().Length < 7) ? "K" : (currentprice.ToString().Length < 13) ? "B" : "T";
                        payout = string.Format("{0}<br/> {1} - {2} {3}", payout, i, currentprice.ToString("c"), amount);
                        currentprice = currentprice * 2;
                    }

                    var result = SolarAssetManagementApi.GetAssetDetails("", "", string.Format("193"), "", string.Format("RR973"));
                    if (Request.IsAuthenticated)
                    {
                        Initialise();
                        var  rrq = new List<string>();
                        rrq.Add(StatusKeys.AwaitingAssessmentPayment);
                        rrq.Add(StatusKeys.awaited);
                        rrq.Add(StatusKeys.ReuploadApplicationDocs);
                        rrq.Add(StatusKeys.Approved);
                        rrq.Add(StatusKeys.AwaitingAssessmentPayment);
                        rrq.Add(StatusKeys.AwaitingDepositPaid);
                        rrq.Add(StatusKeys.ViewRCCCertificate);
                        rrq.Add(StatusKeys.IncentivePolicyApplicationPending);
                        rrq.Add(StatusKeys.ReuploadApplicationDocs);
                        rrq.Add(StatusKeys.AwaitingTenantDocuments);
                        rrq.Add(StatusKeys.InAwaitingWaitingListReEntry);
                        rrq.Add(StatusKeys.LeaseAgreementGenerated);
                        rrq.Add(StatusKeys.AwitingInspectionSchedule);
                        rrq.Add(StatusKeys.RequestRequirementChange);
                        MatchingHelper.WaitingListNotificationAtOneYear2(cxt);
                        var rCSApplicationStatus = db.HumanSettlementApplications.Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Customer)
                            .Include(r => r.ModifiedBySystemUser)
                            .Include(r => r.Status)
                            .Where(x => x.IsDeleted == false && rrq.Contains(x.Status.Key) && x.CustomerId == Customer.Id).ToList();

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

                        var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                        var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                        foreach (var item in rCSApplicationStatus)
                        {
                            item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                        }
                        return View(rCSApplicationStatus);
                    }
                    else
                    {
                        return View("_Error");

                    }

                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Logout", "Account");
            }
        }
        [DecryptParameter]
        public ActionResult HumanSettlementApplications(string Master, string All, string New)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    if (Request.IsAuthenticated)
                    {
                        Initialise();

                        bool master = Convert.ToBoolean(Master);
                        bool allApps = Convert.ToBoolean(All);
                        bool newApps = Convert.ToBoolean(New);

                        var rCSApplicationStatus = db.HumanSettlementApplications.Where(x => x.IsDeleted == false)
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Customer)
                            .Include(r => r.ModifiedBySystemUser)
                            //.Include(r => r.HumanEHCOptions)
                            .Include(r => r.Status).ToList();

                        if (master)
                        {
                            ViewBag.Tittle = "Master Applications";
                            rCSApplicationStatus = rCSApplicationStatus.Where(x => x.IsMaster).ToList();
                        }

                        if (allApps)
                        {
                            ViewBag.Tittle = "All Applications";
                            rCSApplicationStatus = rCSApplicationStatus.ToList();
                        }

                        if (newApps)
                        {
                            ViewBag.Tittle = "New Applications";
                            rCSApplicationStatus = rCSApplicationStatus.Where(x => x.IsMaster).ToList();
                        }

                        List<int> rrq = new List<int>();
                        rrq.Add(db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingAssessmentPayment).Id);
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


                        if (Session["Display"] != null)
                        {
                            if (Session["ApplicationRefNo"] != null)
                            {
                                Session["Display"] = "True";
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

                        var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                        var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                        foreach (var item in rCSApplicationStatus)
                        {
                            item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                        }
                        return View(rCSApplicationStatus);
                    }
                    else
                    {
                        return View("_Error");
                    }

                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Logout", "Account");
            }
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult HumanAgreementRenewwals(string Recommend, string Review, string Approve, string Accept, string Renew)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    if (Request.IsAuthenticated)
                    {
                        Initialise();

                        List<TenantViewModel> vm = new List<TenantViewModel>();
                        MatchingHelper.HumanRenewalNotification(cxt);
                        var Master = db.HumanSettlementLeaseMasters.Where(x => x.IsActive)
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.HumanSettlementApplication)
                            .Include(r => r.ModifiedBySystemUser)
                            .Include(r => r.Status).ToList();

                        var findItem = db.HumanSettlementLeaseDetails.Where(x => x.IsActive)
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.ModifiedBySystemUser)
                            .Include(r => r.HumanSettlementApplication)
                            .Include(r => r.Status).ToList();



                        var Keys = cxt.Status;
                        int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                        int UserId = Customer.Id;
                        List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                        var ResponsibilityTypeId = 0;
                        var list = new List<int?>();

                        bool _recommend = Convert.ToBoolean(Recommend);
                        bool _review = Convert.ToBoolean(Review);
                        bool _approve = Convert.ToBoolean(Approve);
                        bool _accept = Convert.ToBoolean(Accept);
                        bool _renew = Convert.ToBoolean(Renew);
                        var _data = string.Empty;

                        #region if's
                        if (_recommend)
                        {
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalFirstRecommendation).FirstOrDefault().Id;

                            if((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else 
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();
                            Master = Master.Where(x => x.Status.Key == StatusKeys.AgreementOfLeaseReview && list.Contains(x.HumanSettlementApplicationId)).ToList();
                            findItem = findItem.Where(x => x.Status.Key == StatusKeys.AgreementOfLeaseReview && list.Contains(x.HumanSettlementApplicationId)).ToList();
                            ViewBag.Tittle = "Awaiting Agreement Recommendation";
                            _data = string.Format("Recommend={0}&Review={1}&Approve={2}&Accept={3}&Renew={4}", true, false, false, false, false);
                        }

                        if (_review)
                        {
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Renewal2ndRecommendation).FirstOrDefault().Id;
                            //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                            if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }

                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();

                            Master = Master.Where(x => x.Status.Key == StatusKeys.AwaitingRecommendationReview && list.Contains(x.HumanSettlementApplicationId)).ToList();
                            findItem = findItem.Where(x => x.Status.Key == StatusKeys.AwaitingRecommendationReview && list.Contains(x.HumanSettlementApplicationId)).ToList();
                            ViewBag.Tittle = "Awaiting Recommendation Review";
                            _data = string.Format("Recommend={0}&Review={1}&Approve={2}&Accept={3}&Renew={4}", false, true, false, false, false);
                        }

                        if (_approve)
                        {
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalReview).FirstOrDefault().Id;
                            //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                            if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }

                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();

                            Master = Master.Where(x => x.Status.Key == StatusKeys.AwaitingRecommendationApproval && list.Contains(x.HumanSettlementApplicationId)).ToList();
                            findItem = findItem.Where(x => x.Status.Key == StatusKeys.AwaitingRecommendationApproval && list.Contains(x.HumanSettlementApplicationId)).ToList();
                            ViewBag.Tittle = "Awaiting Recommendation Approval";
                            _data = string.Format("Recommend={0}&Review={1}&Approve={2}&Accept={3}&Renew={4}", false, false, true, false, false);
                        }

                        if (_accept)
                        {
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementRenewalAcceptance).FirstOrDefault().Id;
                            //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                            if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }

                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();

                            Master = Master.Where(x => x.Status.Key == StatusKeys.AwaitingTenantAcceptance && list.Contains(x.HumanSettlementApplicationId)).ToList();
                            findItem = findItem.Where(x => x.Status.Key == StatusKeys.AwaitingTenantAcceptance && list.Contains(x.HumanSettlementApplicationId)).ToList();
                            ViewBag.Tittle = "Awaiting Renewal Acceptance";
                            _data = string.Format("Recommend={0}&Review={1}&Approve={2}&Accept={3}&Renew={4}", false, false, false, true, false);
                        }

                        if (_renew)
                        {
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalUploadDocs).FirstOrDefault().Id;
                            //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                            if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }

                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();

                            Master = Master.Where(x => x.Status.Key == StatusKeys.AgreementRenewal && list.Contains(x.HumanSettlementApplicationId)).ToList();
                            findItem = findItem.Where(x => x.Status.Key == StatusKeys.AgreementRenewal && list.Contains(x.HumanSettlementApplicationId)).ToList();
                            ViewBag.Tittle = "Awaiting Agreement Renewal";
                            _data = string.Format("Recommend={0}&Review={1}&Approve={2}&Accept={3}&Renew={4}", false, false, false, false, true);
                        }

                        if (Session["RoundRobinSession"] != null)
                        {
                            var value = Session["RoundRobinSession"].ToString();
                            Session["RoundRobinSession"] = null;
                            var Id = int.Parse(value.Split(',').ToList()[1]);
                            var roundrobin = new RoundRobinQueue();
                            using (var core = new eServicesDbContext())
                            {
                                roundrobin = RoundRobinQueueLog(core, Id);
                            }
                            if (roundrobin != null)
                            {
                                ViewBag.RoundRobinQueue = value.Split(',').ToList()[0];
                                ViewBag.RoundRobinQueueMessage = string.Format($"The application with ref no. {roundrobin.HumanSettlementApplication.ApplicationReferenceNumber} has moved to {roundrobin.ResponsibilityType.Name} queue assigned to {roundrobin.Clerk.SystemUser.FullName}.");
                            }
                        }
                        Session["RoundRobinSession"] = null;
                        #endregion

                        var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                        var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                        foreach (var item in Master)
                        {
                            item.Data = new AesCrypto().Encrypt(_data);
                            var rrq2 = new TenantViewModel { HumanSettlementApplication = item.HumanSettlementApplication, HumanSettlementLeaseMaster = item, ViewId = 0, HumanSettlementLeaseDetailsId = 0, HumanSettlementLeaseMasterId = 0 };
                            vm.Add(rrq2);
                        }
                        foreach (var item in findItem)
                        {
                            item.Data = new AesCrypto().Encrypt(_data);
                            var rrq2 = new TenantViewModel { HumanSettlementApplication = item.HumanSettlementApplication, HumanSettlementLeaseDetails = item };
                            vm.Add(rrq2);
                        }
                        return View(vm ?? new List<TenantViewModel>());
                    }
                    else
                    {
                        return View("_Error");
                    }

                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Logout", "Account");
            }
        }
        [AllowAnonymous]
        [DecryptParameter]
        [HttpGet]
        public ActionResult HumanAgreementAgreementSignature(int ApplicationId, string Data)
        {
            var core = new eServicesDbContext();
            try
            {
                HumanSettlementApplication plmApps = null;
                HumanSettlementLeaseDetails lease = null;
                HumanSettlementLeaseMaster master = null;

                plmApps = core.HumanSettlementApplications
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                   /* .Include(r => r.HumanEHCOptions)*/.Include(r => r.Status)
                    .Where(x => x.Id == ApplicationId).FirstOrDefault();

                master = core.HumanSettlementLeaseMasters
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.ModifiedBySystemUser)
                    .Include(r => r.Status)
                    .Include(r => r.PurchaserType)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

                lease = core.HumanSettlementLeaseDetails
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.ModifiedBySystemUser)
                    .Include(r => r.Status)
                    .Include(r => r.PurchaserType)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

                var Key = plmApps.Status.Key;
                var ActionTypes = core.RCSActionTypes;
                var rr = new List<HSRenewalAction>();
                var dvm = new DocumentsViewModel();
                switch (Key)
                {
                    case StatusKeys.AwaitingApplicantssignature:
                        ViewBag.ApprovalAction = new SelectList(ActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                        ViewBag.AgreementSignature = new SelectList(ActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
                        ViewBag.Results = string.Empty;
                        break;
                    default:
                        return RedirectToAction("CustomerAgreementRenewalAcceptance", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("IsUploadView=false") });
                }
                ViewBag.ApplicationId = plmApps.Id;
                ViewBag.Data = Data;

                var vm = new DepartmentsApprovalViewModel
                {
                    HumanSettlementApplication = plmApps,
                    HumanSettlementLeaseMaster = master,
                    HumanSettlementLeaseDetails = lease,
                    _Data = Data,
                    HSRenewalActions = rr,
                    DocumentsViewModel = dvm
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //throw;
                return RedirectToAction("Logout", "Account");
            }
        }
        [AllowAnonymous]
        [DecryptParameter]
        [HttpPost]
        public ActionResult HumanAgreementAgreementSignature(int ApplicationId, string ApprovalAction, string ReasonForReject, string data2)
        {
            var core = new eServicesDbContext();
            try
            {

                var HumanApp = core.HumanSettlementApplications
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                    /*.Include(r => r.HumanEHCOptions)*/.Include(r => r.Status)
                    .Where(x => x.Id == ApplicationId).FirstOrDefault();

                var Keys = core.Status.ToList();
                var ActivityTrackerMessage = string.Empty;
                int ResponsibilityTypeId = 0;
                var work = core.RoundRobinQueues
                    .Include(r => r.ResponsibilityType)
                    .Where(d => d.ResponsibilityType.Key == ResponsibilityTypeKeys.AgreementSignature && d.HumanSettlementApplicationId == HumanApp.Id).FirstOrDefault();
                switch (ApprovalAction)
                {
                    case RCSActionTypeKeys.Approved:
                        MatchingHelper.ChangeHumanStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingFinalAgreementOfLease).Id, HumanApp.Id);
                        //emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                        //EmailHelper.CustomerEmailOrSMSNotification(core, HumanApp.Id, emailboodyId);
                        ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SignAgreementByApplicant).Description.ToString();
                        ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", HumanApp.ApplicantFullName);
                        MatchingHelper.ActivityTrackerHuman(core, HumanApp.Id, ActivityTrackerMessage, HumanApp.Customer.Id);

                        ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.AgreementSignature).Id;
                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, work.ClerkId);
                        WorkAllocationHumanHelper.AgreementOfLease((int)HumanApp.Id, false, false, false, false, true, false);

                        var onetimepin = core.OneTimePins.FirstOrDefault(o => o.OTP == HumanApp.OTP);
                        onetimepin.IsDeleted = true;
                        onetimepin.IsVerified = true;
                        core.Entry(onetimepin).State = EntityState.Modified;
                        core.SaveChanges();
                        break;
                    case RCSActionTypeKeys.Rejected:
                        MatchingHelper.ChangeHumanStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingAgreementUpdate).Id, HumanApp.Id);
                        //emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                        //EmailHelper.CustomerEmailOrSMSNotification(core, HumanApp.Id, emailboodyId);
                        ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementOfLeaseRejected).Description.ToString();
                        ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", HumanApp.ApplicantFullName);
                        ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", ReasonForReject);
                        MatchingHelper.ActivityTrackerHuman(core, HumanApp.Id, ActivityTrackerMessage, HumanApp.Customer.Id);

                        ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.AgreementSignature).Id;
                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, work.ClerkId);
                        WorkAllocationHumanHelper.AgreementOfLease((int)HumanApp.Id, false, false, false, false, false, true);

                        var onetimepin_ = core.OneTimePins.FirstOrDefault(o => o.OTP == HumanApp.OTP);
                        onetimepin_.IsDeleted = true;
                        onetimepin_.IsVerified = true;
                        core.Entry(onetimepin_).State = EntityState.Modified;
                        core.SaveChanges();
                        break;
                }


                return View("_AgreementAccepted");
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //throw;
                return RedirectToAction("Logout", "Account");
            }
        }



        [AllowAnonymous]
        [DecryptParameter]
        public ActionResult HumanAgreementRenewalAcceptance(int ApplicationId, string Data)
        {
            var core = new eServicesDbContext();
            try
            {
                HumanSettlementApplication plmApps = null;
                HumanSettlementLeaseDetails lease = null;
                HumanSettlementLeaseMaster master = null;

                plmApps = core.HumanSettlementApplications
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                   /* .Include(r => r.HumanEHCOptions)*/.Include(r => r.Status)
                    .Where(x => x.Id == ApplicationId).FirstOrDefault();

                master = core.HumanSettlementLeaseMasters
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.ModifiedBySystemUser)
                    .Include(r => r.Status)
                    .Include(r => r.PurchaserType)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

                lease = core.HumanSettlementLeaseDetails
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.ModifiedBySystemUser)
                    .Include(r => r.Status)
                    .Include(r => r.PurchaserType)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

                var Key = master.Status.Key ?? lease.Status.Key;
                var ActionTypes = core.RCSActionTypes;
                var rr = new List<HSRenewalAction>();
                var dvm = new DocumentsViewModel();
                switch (Key)
                {
                    case StatusKeys.AwaitingTenantAcceptance:
                        ViewBag.ApprovalStatus = new SelectList(ActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                        ViewBag.Results = string.Empty;
                        break;
                    default:
                        return RedirectToAction("CustomerAgreementRenewalAcceptance", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("IsUploadView=false") });
                }
                ViewBag.ApplicationId = plmApps.Id;
                ViewBag.Data = Data;

                var vm = new DepartmentsApprovalViewModel
                {
                    HumanSettlementApplication = plmApps,
                    HumanSettlementLeaseMaster = master,
                    HumanSettlementLeaseDetails = lease,
                    _Data = Data,
                    HSRenewalActions = rr,
                    DocumentsViewModel = dvm
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //throw;
                return RedirectToAction("Logout", "Account");
            }
        }

        [AllowAnonymous]
        [DecryptParameter]
        public ActionResult CustomerAgreementRenewalAcceptance(string IsUploadView)
        {
            var value = Convert.ToBoolean(IsUploadView);
            var vm = new DocumentsViewModel
            {
                IsUploadView = value
            };

            return View("_ErrorAcceptanceRenewal", vm);
        }

        [AllowAnonymous]
        public ActionResult VerificatioCodeValidation2(int ApplicationId, string OTP, string ApprovalStatusddl)
        {
            var core = new eServicesDbContext();
            var plmApps = core.HumanSettlementApplications.FirstOrDefault(r => r.Id == ApplicationId);
            var master = core.HumanSettlementLeaseMasters
                          .Include(r => r.CreatedBySystemUser)
                          .Include(r => r.ModifiedBySystemUser)
                          .Include(r => r.Status)
                          .Include(r => r.PurchaserType)
                          .Include(r => r.HumanSettlementApplication)
                          .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

            var lease = core.HumanSettlementLeaseDetails
                  .Include(r => r.CreatedBySystemUser)
                  .Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.Status)
                  .Include(r => r.PurchaserType)
                  .Include(r => r.HumanSettlementApplication)
                  .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();
            var Keys = core.Status;
            var ActivityTrackerMessage = string.Empty;
            var result = string.Empty;
            int emailboodyId = 0;
            var message = string.Empty;
            int ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.AgreementRenewalAcceptance).Id;
            var Emails = core.EmailContentTypes;
            var Activities = core.ActivityTrackerMessages;
            if (plmApps.OTP == OTP)
            {
                var work = core.RoundRobinQueues
                    .Include(f => f.ResponsibilityType)
                    .Include(f => f.Status)
                    .Where(r => r.HumanSettlementApplicationId == plmApps.Id && r.Status.Key == StatusKeys.Submitted && r.ResponsibilityType.Key == ResponsibilityTypeKeys.AgreementRenewalAcceptance)
                    .FirstOrDefault();

                var findItem = core.OneTimePins.OrderByDescending(a => a.Id).FirstOrDefault(r => r.OTP == OTP && !r.IsVerified && !r.IsAbandoned);
                if (!findItem.IsVerified && !findItem.IsAbandoned)
                {
                    switch (ApprovalStatusddl)
                    {
                        case RCSActionTypeKeys.Rejected:
                            switch (plmApps.IsMaster)
                            {
                                case true:
                                    MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.TerminateAtEndOfPeriod).Id, master.Id);
                                    emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.CustomerAgreementRenewalRejected).Id;
                                    ActivityTrackerMessage = string.Format(core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.CustomerAgreementRenewalRejected).Description);
                                    break;
                                case false:
                                    MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.TerminateAtEndOfPeriod).Id, lease.Id);
                                    emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.CustomerAgreementRenewalRejected).Id;
                                    ActivityTrackerMessage = string.Format(core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.CustomerAgreementRenewalRejected).Description);
                                    break;
                            }
                            WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, work.ClerkId);
                            break;
                        case RCSActionTypeKeys.Approved:
                            switch (plmApps.IsMaster)
                            {
                                case true:
                                    MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AgreementRenewal).Id, master.Id);
                                    emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalAccepted).Id;
                                    ActivityTrackerMessage = string.Format(core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.CustomerAgreementRenewalAccepted).Description);
                                    break;
                                case false:
                                    MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AgreementRenewal).Id, lease.Id);
                                    emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalAccepted).Id;
                                    ActivityTrackerMessage = string.Format(core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.CustomerAgreementRenewalAccepted).Description);
                                    break;
                            }
                            WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, work.ClerkId);
                            WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)plmApps.Id, false, false, false, false, true);
                            break;
                    }

                    MatchingHelper.ActivityTrackerHuman(core, plmApps.Id, ActivityTrackerMessage, plmApps.CustomerId);
                    EmailHelper.CustomerEmailOrSMSNotification(core, plmApps.Id, emailboodyId);
                    result = "Success";
                    message = "Agreement Of Lease Successfully Accepted.";
                    Session["Results"] = string.Format("Successfully accepted renewal for - {0}.", plmApps.ApplicationReferenceNumber);

                    findItem.IsVerified = true;
                    findItem.IsDeleted = true;
                    findItem.IsActive = false;
                    findItem.IsAbandoned = true;
                    core.Entry(findItem).State = EntityState.Modified;
                    core.SaveChanges();
                }
                else
                {
                    message = string.Format("The One Time Pin {0} has been used", OTP);
                }
            }
            else
            {
                message = string.Format("Invalid One Time Pin {0}", OTP);
            }
            var obj = new
            {
                status = result == "Success" ? "Success" : "Failure",
                message = string.Format("{0}", message)
            };

            return Json(obj);
        }


        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        [HttpGet]
        public ActionResult HumanAgreementRenewal(int ApplicationId, string Data)
        {
            Initialise();
            var core = new eServicesDbContext();
            try
            {

                //EscalationHelper.Esacalations(core);
                HumanSettlementApplication plmApps = null;
                HumanSettlementLeaseDetails lease = null;
                HumanSettlementLeaseMaster master = null;

                plmApps = core.HumanSettlementApplications
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                    /*.Include(r => r.HumanEHCOptions)*/.Include(r => r.Status)
                    .Where(x => x.Id == ApplicationId).FirstOrDefault();

                master = core.HumanSettlementLeaseMasters
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.ModifiedBySystemUser)
                    .Include(r => r.Status)
                    .Include(r => r.PurchaserType)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

                lease = core.HumanSettlementLeaseDetails
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.ModifiedBySystemUser)
                    .Include(r => r.Status)
                    .Include(r => r.PurchaserType)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

                var Key = master.Status.Key ?? lease.Status.Key;
                var ActionTypes = core.RCSActionTypes;
                var rr = new List<HSRenewalAction>();
                var dvm = new DocumentsViewModel();
                switch (Key)
                {
                    case StatusKeys.AgreementOfLeaseReview:
                        if (!User.IsInRole("Housing Liaison Officer") && !(User.IsInRole("Super Administrators")) && !(User.IsInRole("Back Office System Administrator")))
                            return RedirectToAction("HumanAgreementRenewwals", "HumanSettlementApplication", new { q = Data });
                        ViewBag.ApprovalStatus = new SelectList(ActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approve24Months || x.Key == RCSActionTypeKeys.NotRenew).OrderBy(x => x.Name), "Key", "Name");

                        break;
                    case StatusKeys.AwaitingRecommendationReview:
                        if (!User.IsInRole("Senior Housing Specialist") && !(User.IsInRole("Super Administrators")) && !(User.IsInRole("Back Office System Administrator")))
                            return RedirectToAction("HumanAgreementRenewwals", "HumanSettlementApplication", new { q = Data });
                        ViewBag.ApprovalStatus = new SelectList(ActionTypes.Where(x => x.Key == RCSActionTypeKeys.Seconded || x.Key == RCSActionTypeKeys.NotSeconded).OrderBy(x => x.Name), "Key", "Name");
                        rr = core.HSRenewalActions.Include(x => x.ActionByUser).Where(r => r.HumanSettlementApplicationId == ApplicationId).ToList();
                        break;
                    case StatusKeys.AwaitingRecommendationApproval:
                        if (!User.IsInRole("Regional Manager") && !(User.IsInRole("Super Administrators")) && !(User.IsInRole("Back Office System Administrator")))
                            return RedirectToAction("HumanAgreementRenewwals", "HumanSettlementApplication", new { q = Data });
                        ViewBag.ApprovalStatus = new SelectList(ActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                        rr = core.HSRenewalActions.Include(x => x.ActionByUser).Where(r => r.HumanSettlementApplicationId == ApplicationId).ToList();
                        break;
                    case StatusKeys.AwaitingTenantAcceptance:
                        if (!User.IsInRole("Housing Liaison Officer") && !User.IsInRole("Customers") && !(User.IsInRole("Super Administrators")) && !(User.IsInRole("Back Office System Administrator")))
                            return RedirectToAction("HumanAgreementRenewwals", "HumanSettlementApplication", new { q = Data });
                        ViewBag.ApprovalStatus = new SelectList(ActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                        ViewBag.Results = string.Empty;
                        break;
                    case StatusKeys.AgreementRenewal:
                        if (!User.IsInRole("Housing Liaison Officer") && !User.IsInRole("Housing Liaison officer") && !(User.IsInRole("Super Administrators")) && !(User.IsInRole("Back Office System Administrator")))
                            return RedirectToAction("HumanAgreementRenewwals", "HumanSettlementApplication", new { q = Data });
                        var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                        var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                        var customer = core.Customers.FirstOrDefault(r => r.Id == plmApps.CustomerId);
                        MatchingHelper.HumanRenewalDocuments(dvm, core, customer.Id, customer.Id, referenceType.Id, application.Id, Data, plmApps.Id, true);
                        break;
                }
                ViewBag.ApplicationId = plmApps.Id;
                ViewBag.Data = Data;

                var vm = new DepartmentsApprovalViewModel
                {
                    HumanSettlementApplication = plmApps,
                    HumanSettlementLeaseMaster = master,
                    HumanSettlementLeaseDetails = lease,
                    _Data = Data,
                    HSRenewalActions = rr,
                    DocumentsViewModel = dvm
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //throw;
                return RedirectToAction("Logout", "Account");
            }
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [HttpPost]
        public ActionResult HumanAgreementRenewal(string Data, int? id, string ApprovalStatusddl, string RejectComment, int ApplicationId)
        {
            using (var core = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var plmApps = core.HumanSettlementApplications
                     .Include(r => r.CreatedBySystemUser)
                     .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                     /*.Include(r => r.HumanEHCOptions)*/.Include(r => r.Status)
                     .Where(x => x.Id == ApplicationId).FirstOrDefault();

                    var master = core.HumanSettlementLeaseMasters
                          .Include(r => r.CreatedBySystemUser)
                          .Include(r => r.ModifiedBySystemUser)
                          .Include(r => r.Status)
                          .Include(r => r.PurchaserType)
                          .Include(r => r.HumanSettlementApplication)
                          .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

                    var lease = core.HumanSettlementLeaseDetails
                          .Include(r => r.CreatedBySystemUser)
                          .Include(r => r.ModifiedBySystemUser)
                          .Include(r => r.Status)
                          .Include(r => r.PurchaserType)
                          .Include(r => r.HumanSettlementApplication)
                          .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

                    var Keys = core.Status;
                    var ActivityTrackerMessage = string.Empty;
                    int emailboodyId = 0;
                    var Emails = core.EmailContentTypes;
                    var Activities = core.ActivityTrackerMessages;
                    var status = master.Status?.Key ?? lease.Status?.Key;
                    var ResponsibilityTypeId = 0;
                    var email = new Email();
                    switch (ApprovalStatusddl)
                    {
                        case RCSActionTypeKeys.Approve24Months:
                            #region Approve24
                            core.HSRenewalActions.Add(new HSRenewalAction
                            {
                                Comment = string.Format("Recommended for 24 months: {0}", RejectComment),
                                ActionByUserId = SystemUser.Id,
                                HLO = true,
                                SHS = false,
                                HumanSettlementApplicationId = plmApps.Id,
                                IsApproved = true
                            });
                            core.SaveChanges();
                            switch (plmApps.IsMaster)
                            {
                                case true:
                                    MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationReview).Id, master.Id);
                                    break;
                                case false:
                                    MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationReview).Id, lease.Id);
                                    break;
                            }
                            ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RenewalFirstRecommendation).Id;
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id,true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id);
                            }
                            WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)plmApps.Id, false, true, false, false, false);
                            emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                            ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeasePassRenewalValidation).Description);

                            #endregion
                            break;
                        case RCSActionTypeKeys.NotRenew:
                            #region NotRenew
                            core.HSRenewalActions.Add(new HSRenewalAction
                            {
                                Comment = string.Format("Recommended not to renew: {0}", RejectComment),
                                ActionByUserId = SystemUser.Id,
                                HLO = true,
                                SHS = false,
                                HumanSettlementApplicationId = plmApps.Id,
                                IsApproved = false
                            });
                            switch (plmApps.IsMaster)
                            {
                                case true:
                                    MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationReview).Id, master.Id);
                                    break;
                                case false:
                                    MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationReview).Id, lease.Id);
                                    break;
                            }
                            ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RenewalFirstRecommendation).Id;
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id, true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id);
                            }
                            WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)plmApps.Id, false, true, false, false, false);
                            #endregion
                            break;
                        case RCSActionTypeKeys.NotSeconded:
                            #region NotSeconded
                            core.HSRenewalActions.Add(new HSRenewalAction
                            {
                                Comment = string.Format("Recommendation not seconded: {0}", RejectComment),
                                ActionByUserId = SystemUser.Id,
                                HLO = false,
                                SHS = true,
                                HumanSettlementApplicationId = plmApps.Id,
                                IsApproved = false
                            });
                            core.SaveChanges();
                            switch (plmApps.IsMaster)
                            {
                                case true:
                                    MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationApproval).Id, master.Id);
                                    break;
                                case false:
                                    MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationApproval).Id, lease.Id);
                                    break;
                            }
                            ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.Renewal2ndRecommendation).Id;
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id, true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id);
                            }
                            WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)plmApps.Id, false, false, true, false, false);
                            emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                            ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeasePassRenewalValidation).Description);
                            #endregion
                            break;
                        case RCSActionTypeKeys.Seconded:
                            #region Seconded
                            core.HSRenewalActions.Add(new HSRenewalAction
                            {
                                Comment = string.Format("Recommendation seconded: {0}", RejectComment),
                                ActionByUserId = SystemUser.Id,
                                HLO = false,
                                SHS = true,
                                HumanSettlementApplicationId = plmApps.Id,
                                IsApproved = true
                            });
                            core.SaveChanges();
                            switch (plmApps.IsMaster)
                            {
                                case true:
                                    MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationApproval).Id, master.Id);
                                    break;
                                case false:
                                    MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationApproval).Id, lease.Id);
                                    break;
                            }
                            ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.Renewal2ndRecommendation).Id;
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id, true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id);
                            }
                            WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)plmApps.Id, false, false, true, false, false);
                            emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                            ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationLeasePassRenewalValidation).Description);
                            #endregion
                            break;
                        case RCSActionTypeKeys.Rejected:
                            #region Rejected
                            bool result01 = core.HSRenewalActions.FirstOrDefault(r => r.HumanSettlementApplicationId == ApplicationId).IsApproved;
                            switch (plmApps.IsMaster)
                            {
                                case true:
                                    if (result01)
                                    {
                                        MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationReview).Id, master.Id);
                                        emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalRejected).Id;
                                        ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementRenewalRecjected).Description, SystemUser.UserFullName);

                                        core.HSRenewalActions.Add(new HSRenewalAction
                                        {
                                            Comment = string.Format("Renewal Rejected: {0}", RejectComment),
                                            ActionByUserId = SystemUser.Id,
                                            HLO = false,
                                            SHS = false,
                                            HumanSettlementApplicationId = plmApps.Id,
                                            IsApproved = false
                                        });
                                        core.SaveChanges();
                                    }
                                    else
                                    {
                                        MatchingHelper.OneTimePinGenerate(core, ApplicationId);
                                        MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingTenantAcceptance).Id, master.Id);
                                        emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalApproved).Id;
                                        ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementRenewalApproved).Description, SystemUser.UserFullName);

                                        string Domain = Request.Url.Scheme + "://" + Request.Url.Authority;
                                        var url = Request.Url.AbsoluteUri.ToString();
                                        if (url.Contains("/PLM")) Domain = string.Format("{0}/{1}", Domain, url.Split('/').ToList().SingleOrDefault(d => d.Contains("PLM")));

                                        string encr = new AesCrypto().Encrypt("ApplicationId=" + plmApps.Id + "&Data=" + Data);
                                        string link = string.Format("{0}/{1}/{2}?q={3}", Domain, "HumanSettlementApplication", "HumanAgreementRenewalAcceptance", encr);
                                        string body = "Please Note your agreement of lease is due for renewal, to accept or decline offer please click <a href='{0}'>here</a>.";
                                        body = string.Format("{0}<br/><br/>Use OTP: {1}, NOT NOT SHARE IT unless required on walk-in application.", body, plmApps.OTP);
                                        string Body = string.Format(body, link);
                                        string mail = plmApps.PurEmail == null ? plmApps.Customer.EmailAddress : plmApps.PurEmail;
                                        email.GenerateEmail(mail,
                                            "PLM: Agreement Renewal Acceptance",
                                            Body, plmApps.CustomerId.ToString(),
                                            false, AppSettingKeys.EservicesDefaultEmailTemplate,
                                           CultureInfo.CurrentCulture.TextInfo.ToTitleCase(string.Format("{0} {1}", plmApps.FirstName, plmApps.LastName).ToLower()), plmApps.CellNo);


                                        core.HSRenewalActions.Add(new HSRenewalAction
                                        {
                                            Comment = string.Format("Renewal Approved: {0}", RejectComment),
                                            ActionByUserId = SystemUser.Id,
                                            HLO = false,
                                            SHS = false,
                                            HumanSettlementApplicationId = plmApps.Id,
                                            IsApproved = true
                                        });
                                        core.SaveChanges();
                                    }

                                    //Notify customer

                                    ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RenewalReview).Id;
                                    if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                    {
                                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id, true);
                                    }
                                    else
                                    {
                                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id);
                                    }
                                    WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)plmApps.Id, false, false, false, true, false);
                                    break;
                                case false:
                                    if (result01)
                                    {
                                        MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationReview).Id, lease.Id);
                                        emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalRejected).Id;
                                        ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementRenewalRecjected).Description, SystemUser.UserFullName);


                                        core.HSRenewalActions.Add(new HSRenewalAction
                                        {
                                            Comment = string.Format("Renewal Rejected: {0}", RejectComment),
                                            ActionByUserId = SystemUser.Id,
                                            HLO = false,
                                            SHS = false,
                                            HumanSettlementApplicationId = plmApps.Id,
                                            IsApproved = false
                                        });
                                        core.SaveChanges();
                                    }
                                    else
                                    {
                                        MatchingHelper.OneTimePinGenerate(core, ApplicationId);
                                        MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingTenantAcceptance).Id, lease.Id);
                                        emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalApproved).Id;
                                        ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementRenewalApproved).Description, SystemUser.UserFullName);

                                        string Domain = Request.Url.Scheme + "://" + Request.Url.Authority;
                                        var url = Request.Url.AbsoluteUri.ToString();
                                        if (url.Contains("/PLM")) Domain = string.Format("{0}/{1}", Domain, url.Split('/').ToList().SingleOrDefault(d => d.Contains("PLM")));

                                        //Notify customer
                                        string encr = new AesCrypto().Encrypt("ApplicationId=" + plmApps.Id + "&Data=" + Data);
                                        string link = string.Format("{0}/{1}/{2}?q={3}", Domain, "HumanSettlementApplication", "HumanAgreementRenewalAcceptance", encr);
                                        string body = "Please Note your agreement of lease is due for renewal, to accept or decline offer please click <a href='{0}'>here</a>.";
                                        body = string.Format("{0}<br/><br/>Use OTP: {1}, NOT NOT SHARE IT unless required on walk-in application.", body, plmApps.OTP);
                                        string Body = string.Format(body, link);
                                        string mail = plmApps.PurEmail == null ? plmApps.Customer.EmailAddress : plmApps.PurEmail;
                                        email.GenerateEmail(mail,
                                            "PLM: Agreement Renewal Acceptance",
                                            Body, plmApps.CustomerId.ToString(),
                                            false, AppSettingKeys.EservicesDefaultEmailTemplate,
                                           CultureInfo.CurrentCulture.TextInfo.ToTitleCase(string.Format("{0} {1}", plmApps.FirstName, plmApps.LastName).ToLower()), plmApps.CellNo);


                                        core.HSRenewalActions.Add(new HSRenewalAction
                                        {
                                            Comment = string.Format("Renewal Approved: {0}", RejectComment),
                                            ActionByUserId = SystemUser.Id,
                                            HLO = false,
                                            SHS = false,
                                            HumanSettlementApplicationId = plmApps.Id,
                                            IsApproved = true
                                        });
                                        core.SaveChanges();
                                    }


                                    ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RenewalReview).Id;
                                    if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                    {
                                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id, true);
                                    }
                                    else
                                    {
                                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id);
                                    }
                                    WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)plmApps.Id, false, false, false, true, false);
                                    break;
                            }
                            #endregion
                            break;
                        case RCSActionTypeKeys.Approved:
                            #region Approved
                            bool result02 = core.HSRenewalActions.FirstOrDefault(r => r.HumanSettlementApplicationId == ApplicationId).IsApproved;
                            switch (plmApps.IsMaster)
                            {
                                case true:
                                    if (result02)
                                    {
                                        MatchingHelper.OneTimePinGenerate(core, ApplicationId);
                                        MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingTenantAcceptance).Id, master.Id);
                                        emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalApproved).Id;
                                        ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementRenewalApproved).Description, SystemUser.UserFullName);

                                        string Domain = Request.Url.Scheme + "://" + Request.Url.Authority;
                                        var url = Request.Url.AbsoluteUri.ToString();
                                        if (url.Contains("/PLM")) Domain = string.Format("{0}/{1}", Domain, url.Split('/').ToList().SingleOrDefault(d => d.Contains("PLM")));

                                        string encr = new AesCrypto().Encrypt("ApplicationId=" + plmApps.Id + "&Data=" + Data);
                                        string link = string.Format("{0}/{1}/{2}?q={3}", Domain, "HumanSettlementApplication", "HumanAgreementRenewalAcceptance", encr);
                                        string body = "Please Note your agreement of lease is due for renewal, to accept or decline offer please click <a href='{0}'>here</a>.";
                                        string Body = string.Format(body, link);
                                        string mail = plmApps.PurEmail == null ? plmApps.Customer.EmailAddress : plmApps.PurEmail;
                                        email.GenerateEmail(mail,
                                            "PLM: Agreement Renewal Acceptance",
                                            Body, plmApps.CustomerId.ToString(),
                                            false, AppSettingKeys.EservicesDefaultEmailTemplate,
                                           CultureInfo.CurrentCulture.TextInfo.ToTitleCase(string.Format("{0} {1}", plmApps.FirstName, plmApps.LastName).ToLower()), plmApps.CellNo);


                                        core.HSRenewalActions.Add(new HSRenewalAction
                                        {
                                            Comment = string.Format("Renewal Approved: {0}", RejectComment),
                                            ActionByUserId = SystemUser.Id,
                                            HLO = false,
                                            SHS = false,
                                            HumanSettlementApplicationId = plmApps.Id,
                                            IsApproved = true
                                        });
                                        core.SaveChanges();
                                    }
                                    else
                                    {
                                        MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationReview).Id, master.Id);
                                        emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalRejected).Id;
                                        ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementRenewalRecjected).Description, SystemUser.UserFullName);

                                        core.HSRenewalActions.Add(new HSRenewalAction
                                        {
                                            Comment = string.Format("Renewal Rejected: {0}", RejectComment),
                                            ActionByUserId = SystemUser.Id,
                                            HLO = false,
                                            SHS = false,
                                            HumanSettlementApplicationId = plmApps.Id,
                                            IsApproved = false
                                        });
                                        core.SaveChanges();
                                    }
                                    ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RenewalReview).Id;
                                    if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                    {
                                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id, true);
                                    }
                                    else
                                    {
                                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id);
                                    }
                                    WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)plmApps.Id, false, false, false, true, false);
                                    break;
                                case false:
                                    if (result02)
                                    {
                                        MatchingHelper.OneTimePinGenerate(core, ApplicationId);
                                        MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingTenantAcceptance).Id, lease.Id);
                                        emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalApproved).Id;
                                        ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementRenewalApproved).Description, SystemUser.UserFullName);

                                        string Domain = Request.Url.Scheme + "://" + Request.Url.Authority;
                                        var url = Request.Url.AbsoluteUri.ToString();
                                        if (url.Contains("/PLM")) Domain = string.Format("{0}/{1}", Domain, url.Split('/').ToList().SingleOrDefault(d => d.Contains("PLM")));

                                        string encr = new AesCrypto().Encrypt("ApplicationId=" + plmApps.Id + "&Data=" + Data);
                                        string link = string.Format("{0}/{1}/{2}?q={3}", Domain, "HumanSettlementApplication", "HumanAgreementRenewalAcceptance", encr);
                                        string body = "Please Note your agreement of lease is due for renewal, to accept or decline offer please click <a href='{0}'>here</a>.";
                                        string Body = string.Format(body, link);
                                        string mail = plmApps.PurEmail == null ? plmApps.Customer.EmailAddress : plmApps.PurEmail;
                                        email.GenerateEmail(mail,
                                            "PLM: Agreement Renewal Acceptance",
                                            Body, plmApps.CustomerId.ToString(),
                                            false, AppSettingKeys.EservicesDefaultEmailTemplate,
                                           CultureInfo.CurrentCulture.TextInfo.ToTitleCase(string.Format("{0} {1}", plmApps.FirstName, plmApps.LastName).ToLower()), plmApps.CellNo);


                                        core.HSRenewalActions.Add(new HSRenewalAction
                                        {
                                            Comment = string.Format("Renewal Approved: {0}", RejectComment),
                                            ActionByUserId = SystemUser.Id,
                                            HLO = false,
                                            SHS = false,
                                            HumanSettlementApplicationId = plmApps.Id,
                                            IsApproved = true
                                        });
                                        core.SaveChanges();
                                    }
                                    else
                                    {
                                        MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingRecommendationReview).Id, lease.Id);
                                        emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalRejected).Id;
                                        ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementRenewalRecjected).Description, SystemUser.UserFullName);

                                        core.HSRenewalActions.Add(new HSRenewalAction
                                        {
                                            Comment = string.Format("Renewal Rejected: {0}", RejectComment),
                                            ActionByUserId = SystemUser.Id,
                                            HLO = false,
                                            SHS = false,
                                            HumanSettlementApplicationId = plmApps.Id,
                                            IsApproved = false
                                        });
                                        core.SaveChanges();
                                    }
                                    ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RenewalReview).Id;
                                    if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                    {
                                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id, true);
                                    }
                                    else
                                    {
                                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id);
                                    }
                                    WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)plmApps.Id, false, false, false, true, false);
                                    break;
                            }
                            #endregion
                            break;
                        default:
                            #region Accept Renewal And Update docs
                            switch (status)
                            {
                                case StatusKeys.AwaitingTenantAcceptance:
                                    switch (plmApps.IsMaster)
                                    {
                                        case true:
                                            break;
                                        case false:
                                            break;
                                    }
                                    break;
                                case StatusKeys.AgreementRenewal:
                                    switch (plmApps.IsMaster)
                                    {
                                        case true:
                                            MatchingHelper.RenewalExtentionOfMonths(core, plmApps.IsMaster, master.Id);
                                            MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.SuccessfullyRenewed).Id, master.Id);
                                            emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.DocumentUploadRenewal).Id;
                                            ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UploadDocumentRenewal).Description, SystemUser.UserFullName);
                                            break;
                                        case false:
                                            MatchingHelper.RenewalExtentionOfMonths(core, plmApps.IsMaster, lease.Id);
                                            //MatchingHelper.OneTimePinGenerate(core, ApplicationId);
                                            MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.SuccessfullyRenewed).Id, lease.Id);
                                            emailboodyId = Emails.FirstOrDefault(x => x.Key == EmailContentKeys.DocumentUploadRenewal).Id;
                                            ActivityTrackerMessage = string.Format(Activities.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UploadDocumentRenewal).Description, SystemUser.UserFullName);
                                            break;
                                    }
                                    MatchingHelper.ChangeHumanStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingLeaseAgreement).Id, (int)plmApps.Id);
                                    ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RenewalUploadDocs).Id;
                                    if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                    {
                                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id, true);
                                    }
                                    else
                                    {
                                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, Customer.Id);
                                    }
                                    WorkAllocationHumanHelper.AgreementOfLease((int)plmApps.Id, true, false, false, false, false, false); //Generate New Agreement Of Lease
                                    break;
                            }
                            #endregion
                            break;
                    }
                    Session["RoundRobinSession"] = string.Format($"ShowRobin,{plmApps.Id}");
                    if (!string.IsNullOrEmpty(ActivityTrackerMessage) && emailboodyId != 0)
                    {
                        MatchingHelper.ActivityTrackerHuman(core, plmApps.Id, ActivityTrackerMessage, Customer.Id);
                        EmailHelper.CustomerEmailOrSMSNotification(core, plmApps.Id, emailboodyId);
                    }
                    return RedirectToAction("HumanAgreementRenewwals", "HumanSettlementApplication", new { q = Data });
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    //throw;
                    return RedirectToAction("Logout", "Account");
                }
            }

        }

        public static RoundRobinQueue RoundRobinQueueLog(eServicesDbContext core, int HumanSettlementApplicationId)
            => core.RoundRobinQueues.Include(d => d.Status).Include(d => d.Clerk.SystemUser).Include(d => d.ResponsibilityType)
            .Include(d=>d.HumanSettlementApplication).OrderByDescending(c => c.Id)
                        .FirstOrDefault(a => a.HumanSettlementApplicationId == HumanSettlementApplicationId && a.Status.Key == StatusKeys.Submitted && a.EndTaskDateTime == null);


        
        public ActionResult DocFinalAgreementValid(int HumanSettlementApplicationId)
        {
            var core = new eServicesDbContext();
            var documentCheckLists = new List<int>();
            var Docs = 0;
            var HumanApplication = core.HumanSettlementApplications.Find(HumanSettlementApplicationId);
            var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
            var HSAgreementOfLease = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.HSAgreementOfLease);
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == HSAgreementOfLease.Id && dcl.ReferenceTypeId == referenceType.Id).Id);

            List<Document> docs =
            core.Documents.Include(d => d.File)
            .Include(d => d.LocationType)
            .Include(d => d.DocumentCheckList)
            .Include(d => d.DocumentCheckList.DocumentType)
            .Include(d => d.DocumentCheckList.Application).Where(
               d => documentCheckLists.Contains(d.DocumentCheckListId) && d.ReferenceTypeId == referenceType.Id &&
                d.ReferenceId == HumanApplication.CustomerId && d.CustomerId == HumanApplication.CustomerId &&
                d.DocumentCheckList.ApplicationId == application.Id && d.HumanSettlementApplicationId == HumanSettlementApplicationId && d.IsActive && !d.IsDeleted).ToList();

            var rrq = docs.GroupBy(c => c.DocumentCheckListId).Select(group => group.First()).ToList();



            var obj = new
            {
                status = docs.Count > 0 ? "Success" : "Failure",
                message = string.Format("Please upload the finalized ageement document!")
            };

            return Json(obj);
        }


        public string GetAppUrl()
        {
            return "";
        }


        [Authorize(Roles = "Housing liaison officer, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        public ActionResult ValidateDocumentsUploaded(/*int  documentCheckListId, */int referenceTypeId, int referenceId, int customerId, int applicationId, int rcsappId)
        {
            var core = new eServicesDbContext();
            var documentCheckLists = new List<int>();
            var Docs = 0;

            var PayslipPensionGrant = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PayslipPensionGrant);
            var BankStatement = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankStatement);
            var IdentityDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdentityDocument);
            var ProofOfEmployment = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfEmployment);
            var Affidavit = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Affidavit);
            var ProofOfAddress = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfAddress);

            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == IdentityDocument.Id && dcl.ReferenceTypeId == referenceTypeId).Id);
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PayslipPensionGrant.Id && dcl.ReferenceTypeId == referenceTypeId).Id);
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankStatement.Id && dcl.ReferenceTypeId == referenceTypeId).Id);
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfEmployment.Id && dcl.ReferenceTypeId == referenceTypeId).Id);
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Affidavit.Id && dcl.ReferenceTypeId == referenceTypeId).Id);
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfAddress.Id && dcl.ReferenceTypeId == referenceTypeId).Id);

            List<Document> docs =
            core.Documents.Include(d => d.File)
            .Include(d => d.LocationType)
            .Include(d => d.DocumentCheckList)
            .Include(d => d.DocumentCheckList.DocumentType)
            .Include(d => d.DocumentCheckList.Application).Where(
               d => documentCheckLists.Contains(d.DocumentCheckListId) && d.ReferenceTypeId == referenceTypeId &&
                d.ReferenceId == referenceId && d.CustomerId == customerId &&
                d.DocumentCheckList.ApplicationId == applicationId && d.HumanSettlementApplicationId == rcsappId && d.IsActive && !d.IsDeleted).ToList();

            var rrq = docs.GroupBy(c => c.DocumentCheckListId).Select(group => group.First()).ToList();

            foreach (var Item in rrq)
            {
                switch (Item.DocumentCheckList.DocumentType.Key)
                {
                    case DocumentTypeKeys.PayslipPensionGrant:
                        Docs += 1;
                        break;
                    case DocumentTypeKeys.BankStatement:
                        Docs += 1;
                        break;
                    case DocumentTypeKeys.IdentityDocument:
                        Docs += 1;
                        break;
                    case DocumentTypeKeys.ProofOfEmployment:
                        Docs += 1;
                        break;
                    case DocumentTypeKeys.Affidavit:
                        Docs += 1;
                        break;
                    case DocumentTypeKeys.ProofOfAddress:
                        Docs += 1;
                        break;
                }
            }

            var obj = new
            {
                status = Docs >= documentCheckLists.Count ? "Success" : "Failure",
                message = string.Format("Please upload complete documents, {0} of {1} missing!", (Docs < documentCheckLists.Count) ? documentCheckLists.Count - Docs : 0, documentCheckLists.Count)
            };

            return Json(obj);
        }
        [Authorize(Roles = "Housing liaison officer, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        public ActionResult VerificatioCodeValidation(int ApplicationId, string OTP, string ApprovalStatusddl)
        {
            var core = new eServicesDbContext();
            _base.Initialise(core);
            var plmApps = core.HumanSettlementApplications.FirstOrDefault(r => r.Id == ApplicationId);
            var master = core.HumanSettlementLeaseMasters
                          .Include(r => r.CreatedBySystemUser)
                          .Include(r => r.ModifiedBySystemUser)
                          .Include(r => r.Status)
                          .Include(r => r.PurchaserType)
                          .Include(r => r.HumanSettlementApplication)
                          .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();

            var lease = core.HumanSettlementLeaseDetails
                  .Include(r => r.CreatedBySystemUser)
                  .Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.Status)
                  .Include(r => r.PurchaserType)
                  .Include(r => r.HumanSettlementApplication)
                  .Where(x => x.HumanSettlementApplicationId == plmApps.Id).FirstOrDefault();
            var Keys = core.Status;
            var ActivityTrackerMessage = string.Empty;
            var result = string.Empty;
            int emailboodyId = 0;
            var message = string.Empty;
            int ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.AgreementRenewalAcceptance).Id;
            var Emails = core.EmailContentTypes;
            var Activities = core.ActivityTrackerMessages;
            if (plmApps.OTP == OTP)
            {
                var work = core.RoundRobinQueues
                    .Include(f => f.ResponsibilityType)
                    .Include(f => f.Status)
                    .Where(r => r.HumanSettlementApplicationId == plmApps.Id && r.Status.Key == StatusKeys.Submitted && r.ResponsibilityType.Key == ResponsibilityTypeKeys.AgreementRenewalAcceptance)
                    .FirstOrDefault();

                var findItem = core.OneTimePins.OrderByDescending(a => a.Id).FirstOrDefault(r => r.OTP == OTP && !r.IsVerified && !r.IsAbandoned);
                if (!findItem.IsVerified && !findItem.IsAbandoned)
                {
                    switch (ApprovalStatusddl)
                    {
                        case RCSActionTypeKeys.Rejected:
                            switch (plmApps.IsMaster)
                            {
                                case true:
                                    MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.TerminateAtEndOfPeriod).Id, master.Id);
                                    emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.CustomerAgreementRenewalRejected).Id;
                                    ActivityTrackerMessage = string.Format(core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.CustomerAgreementRenewalRejected).Description);
                                    break;
                                case false:
                                    MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.TerminateAtEndOfPeriod).Id, lease.Id);
                                    emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.CustomerAgreementRenewalRejected).Id;
                                    ActivityTrackerMessage = string.Format(core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.CustomerAgreementRenewalRejected).Description);
                                    break;
                            }
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, work.ClerkId, true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, work.ClerkId);
                            }
                            break;
                        case RCSActionTypeKeys.Approved:
                            switch (plmApps.IsMaster)
                            {
                                case true:
                                    MatchingHelper.ChangeMasterStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AgreementRenewal).Id, master.Id);
                                    emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalAccepted).Id;
                                    ActivityTrackerMessage = string.Format(core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.CustomerAgreementRenewalAccepted).Description);
                                    break;
                                case false:
                                    MatchingHelper.ChangeLeaseDetailsStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AgreementRenewal).Id, lease.Id);
                                    emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.AgreementRenewalAccepted).Id;
                                    ActivityTrackerMessage = string.Format(core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.CustomerAgreementRenewalAccepted).Description);
                                    break;
                            }
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, work.ClerkId, true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, plmApps.Id, ResponsibilityTypeId, work.ClerkId);
                            }
                            WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)plmApps.Id, false, false, false, false, true);
                            break;
                    }

                    MatchingHelper.ActivityTrackerHuman(core, plmApps.Id, ActivityTrackerMessage, _base.Customer.Id);
                    EmailHelper.CustomerEmailOrSMSNotification(core, plmApps.Id, emailboodyId);
                    result = "Success";
                    message = "Successfully actioned, communication sent.";
                    Session["Results"] = string.Format("Successfully accepted renewal for {0}.", plmApps.ApplicationReferenceNumber);

                    findItem.IsVerified = true;
                    findItem.IsDeleted = true;
                    findItem.IsActive = false;
                    findItem.IsAbandoned = true;
                    core.Entry(findItem).State = EntityState.Modified;
                    core.SaveChanges();
                }
                else
                {
                    message = string.Format("The One Time Pin {0} has been used", OTP);
                }
            }
            else
            {
                message = string.Format("Invalid One Time Pin {0}", OTP);
            }
            var obj = new
            {
                status = result == "Success" ? "Success" : "Failure",
                message = string.Format("{0}", message)
            };

            return Json(obj);
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult NoticedAgreementTerminations(string Search, string Render, int Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    if (Request.IsAuthenticated)
                    {
                        Initialise();
                        var Keys = cxt.Status;
                        int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                        int UserId = Customer.Id;
                        List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                        var ResponsibilityTypeId = 0;
                        var list = new List<int?>();

                        var rCSApplicationStatus = db.HumanSettlementApplications.Where(x => x.ServeNoticeDate != null)
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Customer)
                            .Include(r => r.ModifiedBySystemUser)
                            //.Include(r => r.HumanEHCOptions)
                            .Include(r => r.Status).ToList();

                        var _data = string.Empty;

                        #region if's
                        ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.NoticedAgreements).FirstOrDefault().Id;
                        //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                        if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                        {
                            rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                        }
                        else
                        {
                            rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                        }

                        list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();

                        rCSApplicationStatus = rCSApplicationStatus
                            .Where(x => list.Contains(x.Id))
                            .ToList();
                        if (Session["NoticeSuccess"] != null)
                            ViewBag.NoticeSuccess = "Success";
                        Session["NoticeSuccess"] = null;
                        ViewBag.Tittle = "Noticed Agreement Terminations";
                        _data = "Generate=" + true + "&Review=" + false + "&Approve=" + false + "&Signature=" + false + "&Finalized=" + false + "&Update=" + false;
                        #endregion

                        var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                        var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                        foreach (var item in rCSApplicationStatus)
                        {
                            item.Data = new AesCrypto().Encrypt(_data);
                        }
                        return View(rCSApplicationStatus);
                    }
                    else
                    {
                        return View("_Error");
                    }

                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Logout", "Account");
            }
        }
        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult HumanSettlementAgreements(string Generate, string Review, string Approve, string Signature, string Finalized, string Update)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    if (Request.IsAuthenticated)
                    {
                        Initialise();
                        var Keys = cxt.Status;
                        int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                        int UserId = Customer.Id;
                        List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                        var ResponsibilityTypeId = 0;
                        var list = new List<int?>();

                        var rCSApplicationStatus = db.HumanSettlementApplications.Where(x => x.IsDeleted == false)
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Customer)
                            .Include(r => r.ModifiedBySystemUser)
                            //.Include(r => r.HumanEHCOptions)
                            .Include(r => r.Status).ToList();

                        var QQ = rCSApplicationStatus.Select(x => x.Id).ToList();


                        bool _generate = Convert.ToBoolean(Generate);
                        bool _review = Convert.ToBoolean(Review);
                        bool _approve = Convert.ToBoolean(Approve);
                        bool _signature = Convert.ToBoolean(Signature);
                        bool _finalized = Convert.ToBoolean(Finalized);
                        bool _update = Convert.ToBoolean(Update);
                        var _data = string.Empty;

                        #region if's
                        if (_generate)
                        {
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement).FirstOrDefault().Id;
                            //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }

                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();
                            //Master = Master
                            //    .Where(x => x.Status.Key == StatusKeys.AwaitingLeaseAgreement && list.Contains(x.HumanSettlementApplicationId))
                            //    .ToList();
                            //Details = Details
                            //   .Where(x => x.Status.Key == StatusKeys.AwaitingLeaseAgreement && list.Contains(x.Id))
                            //   .ToList();
                            rCSApplicationStatus = rCSApplicationStatus
                                .Where(x => x.Status.Key == StatusKeys.AwaitingLeaseAgreement && list.Contains(x.Id))
                                .ToList();
                            ViewBag.Tittle = "Generate Lease Agreement";
                            _data = "Generate=" + true + "&Review=" + false + "&Approve=" + false + "&Signature=" + false + "&Finalized=" + false + "&Update=" + false;
                        }

                        if (_review)
                        {
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementReview).FirstOrDefault().Id;
                            //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                            if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }

                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();

                            //Master = Master
                            //    .Where(x => x.Status.Key == StatusKeys.AwaitingLeaseAgreementReview && list.Contains(x.HumanSettlementApplicationId))
                            //    .ToList();
                            //Details = Details
                            //   .Where(x => x.Status.Key == StatusKeys.AwaitingLeaseAgreementReview && list.Contains(x.HumanSettlementApplicationId))
                            //   .ToList();
                            rCSApplicationStatus = rCSApplicationStatus
                                .Where(x => x.Status.Key == StatusKeys.AwaitingLeaseAgreementReview && list.Contains(x.Id))
                                .ToList();

                            ViewBag.Tittle = "Awaiting Agreement Review";
                            _data = "Generate=" + false + "&Review=" + true + "&Approve=" + false + "&Signature=" + false + "&Finalized=" + false + "&Update=" + false;
                        }

                        if (_approve)
                        {
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementApproval).FirstOrDefault().Id;
                            //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                            if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }

                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();
                            //Master = Master
                            //    .Where(x => x.Status.Key == StatusKeys.AwaitingLeaseAgreementApproval && list.Contains(x.HumanSettlementApplicationId))
                            //    .ToList();
                            //Details = Details
                            //   .Where(x => x.Status.Key == StatusKeys.AwaitingLeaseAgreementApproval && list.Contains(x.HumanSettlementApplicationId))
                            //   .ToList();
                            rCSApplicationStatus = rCSApplicationStatus
                                .Where(x => x.Status.Key == StatusKeys.AwaitingLeaseAgreementApproval && list.Contains(x.Id))
                                .ToList();

                            ViewBag.Tittle = "Awaiting Agreement Approval";
                            _data = "Generate=" + false + "&Review=" + false + "&Approve=" + true + "&Signature=" + false + "&Finalized=" + false + "&Update=" + false;
                        }

                        if (_signature)
                        {
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementSignature).FirstOrDefault().Id;
                            //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                            if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }

                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();
                            //Master = Master
                            //.Where(x => x.Status.Key == StatusKeys.AwaitingApplicantssignature && list.Contains(x.HumanSettlementApplicationId))
                            //.ToList();
                            //Details = Details
                            //   .Where(x => x.Status.Key == StatusKeys.AwaitingApplicantssignature && list.Contains(x.HumanSettlementApplicationId))
                            //   .ToList();
                            rCSApplicationStatus = rCSApplicationStatus
                                .Where(x => x.Status.Key == StatusKeys.AwaitingApplicantssignature && list.Contains(x.Id))
                                .ToList();

                            ViewBag.Tittle = "Awaiting Aplicants Signature";
                            _data = "Generate=" + false + "&Review=" + false + "&Approve=" + false + "&Signature=" + true + "&Finalized=" + false + "&Update=" + false;
                        }

                        if (_finalized)
                        {
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.FinalizeAgreement).FirstOrDefault().Id;
                            //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                            if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }

                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();
                            // Master = Master
                            //.Where(x => x.Status.Key == StatusKeys.AwaitingFinalAgreementOfLease && list.Contains(x.HumanSettlementApplicationId))
                            //.ToList();
                            // Details = Details
                            //    .Where(x => x.Status.Key == StatusKeys.AwaitingFinalAgreementOfLease && list.Contains(x.HumanSettlementApplicationId))
                            //    .ToList();
                            rCSApplicationStatus = rCSApplicationStatus
                                .Where(x => x.Status.Key == StatusKeys.AwaitingFinalAgreementOfLease && list.Contains(x.Id))
                                .ToList();

                            ViewBag.Tittle = "Finalized Agreement Of Lease";
                            _data = "Generate=" + false + "&Review=" + false + "&Approve=" + false + "&Signature=" + false + "&Finalized=" + true + "&Update=" + false;
                        }

                        if (_update)
                        {
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.UpdateAgreement).FirstOrDefault().Id;
                            //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                            if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }

                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();
                            //  Master = Master
                            //.Where(x => x.Status.Key == StatusKeys.AwaitingAgreementUpdate && list.Contains(x.HumanSettlementApplicationId))
                            //.ToList();
                            //  Details = Details
                            //     .Where(x => x.Status.Key == StatusKeys.AwaitingAgreementUpdate && list.Contains(x.HumanSettlementApplicationId))
                            //     .ToList();
                            rCSApplicationStatus = rCSApplicationStatus
                                .Where(x => x.Status.Key == StatusKeys.AwaitingAgreementUpdate && list.Contains(x.Id))
                                .ToList();

                            ViewBag.Tittle = "Awaiting Agreement Update";
                            _data = "Generate=" + false + "&Review=" + false + "&Approve=" + false + "&Signature=" + false + "&Finalized=" + false + "&Update=" + true;
                        }

                        if (Session["RoundRobinSession"] != null)
                        {
                            var value = Session["RoundRobinSession"].ToString();
                            var Id = int.Parse(value.Split(',').ToList()[1]);
                            var roundrobin = new RoundRobinQueue();
                            using (var core = new eServicesDbContext())
                            {
                                roundrobin = RoundRobinQueueLog(core, Id);
                            }
                            if (roundrobin != null)
                            {
                                Session["RoundRobinSession"] = null;
                                ViewBag.RoundRobinQueue = value.Split(',').ToList()[0];
                                ViewBag.RoundRobinQueueMessage = string.Format($"The application with ref no. {roundrobin.HumanSettlementApplication.ApplicationReferenceNumber} has moved to {roundrobin.ResponsibilityType.Name} queue assigned to {roundrobin.Clerk.SystemUser.FullName}.");
                            }
                        }
                        Session["RoundRobinSession"] = null;
                        #endregion

                        var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                        var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                        foreach (var item in rCSApplicationStatus)
                        {
                            item.Data = new AesCrypto().Encrypt(_data);
                        }
                        //foreach (var item in Master)
                        //{
                        //    rCSApplicationStatus.Single(r => r.Id == item.HumanSettlementApplicationId).Status = item.Status;
                        //    rCSApplicationStatus.Single(r => r.Id == item.HumanSettlementApplicationId).CreatedDateTime = item.CreatedDateTime;

                        //}
                        if (Session["GenerateLeaseAgreement"] != null)
                            ViewBag.GenerateAgreement = "Success";
                        else
                            ViewBag.GenerateAgreement = null;
                        Session["GenerateLeaseAgreement"] = null;

                        return View(rCSApplicationStatus);
                    }
                    else
                    {
                        return View("_Error");
                    }

                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Logout", "Account");
            }
        }

        public ActionResult OneTimePinValidation(int ApplicationId, string OneTimePin)
        {
            using (var core = new eServicesDbContext())
            {
                bool result = false;
                var finditem = core.HumanSettlementApplications.FirstOrDefault(d => d.Id == ApplicationId);
                if (finditem.OTP == OneTimePin)
                {
                    result = true;
                }

                var obj = new
                {
                    status = result ? "Success" : "Failure"
                };

                return Json(obj);
            }
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult HumanAgreementOfLease(int ApplicationId, string Data)
        {

            try
            {
                Initialise();
                var cxt = new eServicesDbContext();
                var Application = cxt.HumanSettlementApplications.Where(x => x.IsDeleted == false && x.Id == ApplicationId).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.PreferredComplexArea).Include(r => r.PreferredComplexArea2)
                  .Include(r => r.TitleType).Include(r => r.SecAppTitleType)
                  /*.Include(r => r.HumanEHCOptions)*/.Include(r => r.PurchaserType).Include(r => r.Status).FirstOrDefault();

                var leaseApplication = cxt.HumanSettlementLeaseMasters.Where(x => x.IsDeleted == false && x.HumanSettlementApplicationId == ApplicationId).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.ModifiedBySystemUser).Include(r => r.Status).FirstOrDefault() ?? null;

                var findItem = cxt.HumanSettlementLeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.ModifiedBySystemUser).Include(r => r.Status).FirstOrDefault() ?? null;

                ViewBag.ApprovalAction = new SelectList(cxt.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.AgreementSignature = new SelectList(cxt.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");

                var application = cxt.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));
                if (referenceType == null) throw new Exception("Invalid reference type.");
                var dvm = new DocumentsViewModel();
                MatchingHelper.DocumentAgreementOfLease(dvm, cxt, Application.CustomerId, Application.CustomerId, (int)referenceType.Id, (int)application.Id, "", Application.Id, true);

                var docdets = cxt.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.AgreementOfLease);
                var attachments = cxt.Attachments.Where(x => x.PropertyLeaseApplicationId == Application.Id && x.DocumentTypeId == docdets.Id).ToList();
                //MatchingHelper.ChangeHumanStatus(cxt, cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingLeaseAgreementReview).Id, Application.Id);

                var activeDirectoryOn = Convert.ToInt16(cxt.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);

                //var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement).FirstOrDefault();
                //MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)Application.Id, null, ResponsibilityTypeId.Id, Customer.Id);

                var custmusers = cxt.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = cxt.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.PropertyLeaseAgreementGenerated).Description.ToString();
                MatchingHelper.ActivityTrackerHuman(cxt, Application.Id, ActivityTrackerMessage, custmusers.Id);

                //Send e-mail and SMS notification
                int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.InActionGenerateLeaseAgreement).Id;
                EmailHelper.CustomerEmailOrSMSNotification(cxt, Application.Id, emailboodyId);

                Application.Data = Data;

                var vm = new DepartmentsApprovalViewModel()
                {
                    Attachments = attachments,
                    HumanSettlementApplication = Application,
                    DocName = docdets.Name,
                    DocDesc = docdets.Description,
                    DocumentsViewModel = dvm
                };
                ViewBag.PropertyLeaseAppliactionId = Application.Id;
                return View(vm);
            }
            catch (Exception io)
            {
                EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }

            return View("_Error");
        }

        //here
        [Authorize(Roles = "Housing Liaison Officer, Senior Housing Specialist, Regional Manager, Customer, Super Administrators, Back Office System Administrator")]
        [HttpPost]
        public ActionResult HumanAgreementOfLease(DepartmentsApprovalViewModel model, string ApprovalAction, int HumanSettlementApplicationId, string Data, string ReasonForReject)
        {
            Initialise();
            using (var core = new eServicesDbContext())
            {
                var HumanApp = core.HumanSettlementApplications.Include(r => r.Customer).Include(r => r.Status).Where(x => x.Id == HumanSettlementApplicationId).FirstOrDefault();
                var StatusKey = HumanApp.Status.Key;
                var Keys = core.Status.ToList();
                int emailboodyId = 0;
                var ResponsibilityTypeId = 0;
                var ActivityTrackerMessage = string.Empty;
                var ActivityUser = SystemUser.FullName;
                var email = new Email();
                switch (StatusKey)
                {
                    case StatusKeys.AwaitingLeaseAgreementReview:
                        if (!User.IsInRole("Senior Housing Specialist") && !User.IsInRole("Super Administrators") && !User.IsInRole("Back Office System Administrator")) break;
                        switch (ApprovalAction)
                        {
                            case RCSActionTypeKeys.Approved:
                                MatchingHelper.ChangeHumanStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingLeaseAgreementApproval).Id, HumanApp.Id);
                                //emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                                //EmailHelper.CustomerEmailOrSMSNotification(core, HumanApp.Id, emailboodyId);
                                ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApproveAgreementOfLease).Description.ToString();
                                MatchingHelper.ActivityTrackerHuman(core, HumanApp.Id, ActivityTrackerMessage, Customer.Id);

                                ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.AgreementReview).Id;
                                //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id, true);
                                }
                                else
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                }

                                WorkAllocationHumanHelper.AgreementOfLease((int)HumanApp.Id, false, false, true, false, false, false);
                                break;

                            case RCSActionTypeKeys.Rejected:
                                MatchingHelper.ChangeHumanStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingAgreementUpdate).Id, HumanApp.Id);
                                //emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                                //EmailHelper.CustomerEmailOrSMSNotification(core, HumanApp.Id, emailboodyId);
                                ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementOfLeaseRejected).Description.ToString();
                                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", ActivityUser);
                                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", ReasonForReject);
                                MatchingHelper.ActivityTrackerHuman(core, HumanApp.Id, ActivityTrackerMessage, Customer.Id);

                                ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.AgreementReview).Id;
                                //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id, true);
                                }
                                else
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                }

                                WorkAllocationHumanHelper.AgreementOfLease((int)HumanApp.Id, false, false, false, false, false, true);
                                break;
                        }
                        break;

                    case StatusKeys.AwaitingLeaseAgreementApproval:
                        if (!User.IsInRole("Regional Manager") && !User.IsInRole("Super Administrators") && !User.IsInRole("Back Office System Administrator")) break;
                        switch (ApprovalAction)
                        {
                            case RCSActionTypeKeys.Approved:
                                MatchingHelper.ChangeHumanStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingApplicantssignature).Id, HumanApp.Id);
                                emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.AwaitingApplicantsSignature).Id;
                                EmailHelper.CustomerEmailOrSMSNotification(core, HumanApp.Id, emailboodyId);
                                ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApproveAgreementOfLease).Description.ToString();
                                MatchingHelper.ActivityTrackerHuman(core, HumanApp.Id, ActivityTrackerMessage, Customer.Id);

                                ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.AgreementApproval).Id;
                                //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id, true);
                                }
                                else
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                }


                                WorkAllocationHumanHelper.AgreementOfLease((int)HumanApp.Id, false, false, false, true, false, false);
                                MatchingHelper.OneTimePinGenerate(core, HumanApp.Id);
                                string Domain = Request.Url.Scheme + "://" + Request.Url.Authority;
                                var url = Request.Url.AbsoluteUri.ToString();
                                if (url.Contains("/PLM")) Domain = string.Format("{0}/{1}", Domain, url.Split('/').ToList().SingleOrDefault(d => d.Contains("PLM")));
                                string encr = new AesCrypto().Encrypt("ApplicationId=" + HumanApp.Id + "&Data=" + Data);
                                string link = string.Format("{0}/{1}/{2}?q={3}", Domain, "HumanSettlementApplication", "HumanAgreementAgreementSignature", encr);
                                string body = "Please Note your agreement of lease is awaiting signature, to accept or decline offer please click <a href='{0}'>here</a>. <br/> Here is the OTP: <b>{1}</b>, to be used for verification.";
                                string Body = string.Format(body, link, HumanApp.OTP);
                                string mail = HumanApp.PurEmail == null ? HumanApp.Customer.EmailAddress : HumanApp.PurEmail;
                                email.GenerateEmail(mail,
                                    "PLM: Agreement Signature",
                                    Body, HumanApp.CustomerId.ToString(),
                                    false, AppSettingKeys.EservicesDefaultEmailTemplate,
                                   CultureInfo.CurrentCulture.TextInfo.ToTitleCase(string.Format("{0} {1}", HumanApp.FirstName, HumanApp.LastName).ToLower()), HumanApp.CellNo);

                                break;

                            case RCSActionTypeKeys.Rejected:
                                MatchingHelper.ChangeHumanStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingAgreementUpdate).Id, HumanApp.Id);
                                //emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.AwaitingApplicantsSignature).Id;
                                //EmailHelper.CustomerEmailOrSMSNotification(core, HumanApp.Id, emailboodyId);
                                var Agreement = core.HSLeaseAgreementMasters.FirstOrDefault(x => x.HumanSettlementApplicationId == HumanApp.Id);
                                Agreement.Lessor_sign_date = null;
                                Agreement.LessorSignature = null;
                                core.Entry(Agreement).State = EntityState.Modified;
                                core.SaveChanges();
                                ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementOfLeaseRejected).Description.ToString();
                                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", ActivityUser);
                                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", ReasonForReject);
                                MatchingHelper.ActivityTrackerHuman(core, HumanApp.Id, ActivityTrackerMessage, Customer.Id);

                                ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.AgreementApproval).Id;
                                //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id, true);
                                }
                                else
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                }
                                WorkAllocationHumanHelper.AgreementOfLease((int)HumanApp.Id, false, false, false, false, false, true);
                                break;
                        }
                        break;

                    case StatusKeys.AwaitingApplicantssignature:
                        if (!User.IsInRole("Housing Liaison Officer") && !User.IsInRole("Super Administrators") && !User.IsInRole("Back Office System Administrator")) break;
                        switch (ApprovalAction)
                        {
                            case RCSActionTypeKeys.Approved:
                                MatchingHelper.ChangeHumanStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingFinalAgreementOfLease).Id, HumanApp.Id);
                                //emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                                //EmailHelper.CustomerEmailOrSMSNotification(core, HumanApp.Id, emailboodyId);
                                ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SignAgreementByApplicant).Description.ToString();
                                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", ActivityUser);
                                MatchingHelper.ActivityTrackerHuman(core, HumanApp.Id, ActivityTrackerMessage, Customer.Id);

                                ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.AgreementSignature).Id;
                                //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id, true);
                                }
                                else
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                }
                                WorkAllocationHumanHelper.AgreementOfLease((int)HumanApp.Id, false, false, false, false, true, false);

                                var onetimepin = core.OneTimePins.FirstOrDefault(o => o.OTP == HumanApp.OTP);
                                onetimepin.IsDeleted = true;
                                onetimepin.IsVerified = true;
                                core.Entry(onetimepin).State = EntityState.Modified;
                                core.SaveChanges();
                                break;
                            case RCSActionTypeKeys.Rejected:
                                MatchingHelper.ChangeHumanStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingAgreementUpdate).Id, HumanApp.Id);
                                //emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                                //EmailHelper.CustomerEmailOrSMSNotification(core, HumanApp.Id, emailboodyId);
                                ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementOfLeaseRejected).Description.ToString();
                                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", ActivityUser);
                                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", ReasonForReject);
                                MatchingHelper.ActivityTrackerHuman(core, HumanApp.Id, ActivityTrackerMessage, Customer.Id);

                                ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.AgreementSignature).Id;
                                //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id, true);
                                }
                                else
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                }
                                WorkAllocationHumanHelper.AgreementOfLease((int)HumanApp.Id, false, false, false, false, false, true);

                                var onetimepin_ = core.OneTimePins.FirstOrDefault(o => o.OTP == HumanApp.OTP);
                                onetimepin_.IsDeleted = true;
                                onetimepin_.IsVerified = true;
                                core.Entry(onetimepin_).State = EntityState.Modified;
                                core.SaveChanges();
                                break;
                        }
                        break;

                    case StatusKeys.AwaitingFinalAgreementOfLease:
                        if (!User.IsInRole("Housing Liaison Officer") && !User.IsInRole("Super Administrators") && !User.IsInRole("Back Office System Administrator")) break;
                        switch (ApprovalAction)
                        {
                            case RCSActionTypeKeys.Approved:
                                MatchingHelper.ChangeHumanStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.SignedAgreementOfLeaseUploaded).Id, HumanApp.Id);
                                //emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                                //EmailHelper.CustomerEmailOrSMSNotification(core, HumanApp.Id, emailboodyId);
                                ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.PropertyLeaseAgreementGenerated).Description.ToString();
                                MatchingHelper.ActivityTrackerHuman(core, HumanApp.Id, ActivityTrackerMessage, Customer.Id);

                                ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.FinalizeAgreement).Id;
                                //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id, true);
                                }
                                else
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                }
                                //WorkAllocationHumanHelper.AgreementOfLease((int)HumanApp.Id, false, false, false, false, true, false);
                                break;
                            case RCSActionTypeKeys.Rejected:
                                MatchingHelper.ChangeHumanStatus(core, Keys.FirstOrDefault(r => r.Key == StatusKeys.AwaitingAgreementUpdate).Id, HumanApp.Id);
                                //emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                                //EmailHelper.CustomerEmailOrSMSNotification(core, HumanApp.Id, emailboodyId);
                                ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AgreementOfLeaseRejected).Description.ToString();
                                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", ActivityUser);
                                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", ReasonForReject);
                                MatchingHelper.ActivityTrackerHuman(core, HumanApp.Id, ActivityTrackerMessage, Customer.Id);

                                ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.FinalizeAgreement).Id;
                                //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id, true);
                                }
                                else
                                {
                                    WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, HumanApp.Id, ResponsibilityTypeId, Customer.Id);
                                }
                                WorkAllocationHumanHelper.AgreementOfLease((int)HumanApp.Id, false, false, false, false, false, true);
                                break;
                        }
                        break;

                }
                Session["RoundRobinSession"] = string.Format($"ShowRobin,{HumanApp.Id}");
                return RedirectToAction("HumanSettlementAgreements", "HumanSettlementApplication", new { q = Data });
            }
            return View();
        }


        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult HuamnAgreementOccupants(string ViewFrom)
        {
            using (var core = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    List<HumanViewModel> vm = new List<HumanViewModel>();
                    var masters = core.HumanSettlementLeaseMasters.Include(r => r.HumanSettlementApplication).Include(r => r.PurchaserType).Include(r => r.Status).Where(x => x.IsActive).ToList();
                    var details = core.HumanSettlementLeaseDetails.Include(r => r.HumanSettlementApplication).Include(r => r.Status).Where(x => x.IsActive).ToList();

                    switch (ViewFrom)
                    {
                        case "HumanMyLeases":
                            foreach (var I in masters)
                            {
                                var rrq = new HumanViewModel()
                                {
                                    HumanSettlementApplicationId = I.HumanSettlementApplication.Id,
                                    HumanSettlementLeaseMasterId = I.Id,
                                    ApplicantFullName = I.HumanSettlementApplication.ApplicantFullName,
                                    ApplicationReferenceNumber = I.HumanSettlementApplication.ApplicationReferenceNumber,
                                    ApplicationType = I.PurchaserType.Name,
                                    DateTimeCreated = (DateTime)I.CreatedDateTime,
                                    StatusKey = I.Status.Key,
                                    StastusName = I.Status.Name,
                                    Data = new AesCrypto().Encrypt(ViewFrom),
                                    ViewName = "HuamnAgreementOccupants"
                                };
                                vm.Add(rrq);
                            }
                            foreach (var I in details)
                            {
                                var rrq = new HumanViewModel()
                                {
                                    HumanSettlementApplicationId = I.HumanSettlementApplication.Id,
                                    HumanSettlementLeaseId = I.Id,
                                    ApplicantFullName = I.HumanSettlementApplication.ApplicantFullName,
                                    ApplicationReferenceNumber = I.HumanSettlementApplication.ApplicationReferenceNumber,
                                    ApplicationType = I.PurchaserType.Name,
                                    DateTimeCreated = (DateTime)I.CreatedDateTime,
                                    StatusKey = I.Status.Key,
                                    StastusName = I.Status.Name,
                                    Data = new AesCrypto().Encrypt(ViewFrom),
                                    ViewName = "HuamnAgreementOccupants"
                                };
                                vm.Add(rrq);
                            }
                            break;
                        case "HumanManageOccupants":
                            var Keys = core.Status.FirstOrDefault(r => r.Key == StatusKeys.DeactivateLeaseNewCaptured).Id;
                            masters = masters.Where(r => (r.Status.Key != StatusKeys.DeactivateLeaseNewCaptured && r.Status.Key != StatusKeys.ApplicantVacated)).ToList();
                            details = details.Where(r => (r.Status.Key != StatusKeys.DeactivateLeaseNewCaptured && r.Status.Key != StatusKeys.ApplicantVacated)).ToList();
                            foreach (var I in masters)
                            {
                                var qq = core.HumanSettlementApplications.Include(d => d.Status).Where(r => r.Id == I.HumanSettlementApplicationId).FirstOrDefault();
                                var rrq = new HumanViewModel();
                                rrq.HumanSettlementApplicationId = I.HumanSettlementApplication.Id;
                                rrq.HumanSettlementLeaseMasterId = I.Id;
                                rrq.HumanSettlementLeaseId = 0;
                                rrq.ApplicantFullName = I.HumanSettlementApplication.ApplicantFullName;
                                rrq.ApplicationReferenceNumber = I.HumanSettlementApplication.ApplicationReferenceNumber;
                                rrq.ApplicationType = I.PurchaserType.Name;
                                rrq.DateTimeCreated = (DateTime)I.CreatedDateTime;
                                rrq.StatusKey = I.Status.Key;
                                rrq.StastusName = I.Status.Name;
                                rrq.Data = new AesCrypto().Encrypt(ViewFrom);
                                rrq.ViewName = "HumanManageOccupants";
                                vm.Add(rrq);
                            }
                            foreach (var I in details)
                            {
                                var qq = core.HumanSettlementApplications.Include(d => d.Status).Where(r => r.Id == I.HumanSettlementApplicationId).FirstOrDefault();
                                var rrq = new HumanViewModel()
                                {
                                    HumanSettlementApplicationId = I.HumanSettlementApplication.Id,
                                    HumanSettlementLeaseId = I.Id,
                                    ApplicantFullName = I.HumanSettlementApplication.ApplicantFullName,
                                    ApplicationReferenceNumber = I.HumanSettlementApplication.ApplicationReferenceNumber,
                                    ApplicationType = I.PurchaserType?.Name,
                                    DateTimeCreated = (DateTime)I.CreatedDateTime,
                                    StatusKey = qq.Status.Key,
                                    StastusName = qq.Status.Name,
                                    Data = new AesCrypto().Encrypt(ViewFrom),
                                    ViewName = "HumanManageOccupants"
                                };
                                vm.Add(rrq);
                            }
                            break;
                    }



                    //foreach (var item in HumanApplications)
                    //{
                    //    item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    //}
                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
            }
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer")]
        [DecryptParameter]
        public ActionResult HumanActivateResident(string Url, int Id)
        {
            var core = new eServicesDbContext();
            var qqq = core.HSUnitOccupants.FirstOrDefault(r => r.Id == Id);
            qqq.StatusId = core.Status.FirstOrDefault(r => r.Key == StatusKeys.ActiveOccupant).Id;
            core.Entry(qqq).State = EntityState.Modified;
            core.SaveChanges();
            return RedirectToAction("HumanManageAgreementOccupants", "HumanSettlementApplication", new { q = Url });
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer")]
        [DecryptParameter]
        public ActionResult HumanDeactivateResident(string Url, int Id)
        {
            var core = new eServicesDbContext();
            var qqq = core.HSUnitOccupants.FirstOrDefault(r => r.Id == Id);
            qqq.StatusId = core.Status.FirstOrDefault(r => r.Key == StatusKeys.DeactiveOccupant).Id;
            core.Entry(qqq).State = EntityState.Modified;
            core.SaveChanges();
            return RedirectToAction("HumanManageAgreementOccupants", "HumanSettlementApplication", new { q = Url });
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult HumanManageAgreementOccupants(int Aplication, int? MasterId, int DetaisId, string ViewName, string Data)
        {
            if (Session["ShowSuccess"] != null)
            {
                ViewBag.Display = "success";
                Session["ShowSuccess"] = null;
            }
            var core = new eServicesDbContext();
            var UrlReturn = new AesCrypto().Encrypt("Aplication=" + Aplication + "&MasterId=" + MasterId + "&DetaisId=" + DetaisId + "&ViewName=" + ViewName + "&Data=" + Data);
            TenantViewModel vm = new TenantViewModel();
            vm.UrlReturn = UrlReturn;
            vm.Data = Data;
            vm.HumanSettlementLeaseDetailsId = (int)MasterId;
            vm.HumanSettlementLeaseMasterId = DetaisId;
            vm.ViewName = ViewName;
            vm.ListHSUnitOccupants = core.HSUnitOccupants.Include(c => c.Status).Include(c => c.TitleType).Where(r => r.HumanSettlementApplicationId == Aplication && r.Status.Key == StatusKeys.ActiveOccupant && r.IsActive && !r.IsDeleted).ToList();
            vm.DeacListHSUnitOccupants = core.HSUnitOccupants.Include(c => c.Status).Include(c => c.TitleType).Where(r => r.HumanSettlementApplicationId == Aplication && r.StatusId == core.Status.FirstOrDefault(x => x.Key == StatusKeys.DeactiveOccupant).Id).ToList();
            vm.HumanSettlementApplication = core.HumanSettlementApplications.Include(c => c.PurchaserType).Include(c => c.Status).Where(r => r.Id == Aplication).FirstOrDefault();
            ViewBag.TitleTypeId = new SelectList(core.TitleTypes.ToList(), "Id", "Name");
            return View(vm);
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [HttpPost]
        public ActionResult HumanManageAgreementOccupants(TenantViewModel vm, string Url, int HumanSettlementApplicationId)
        {
            var core = new eServicesDbContext();
            var plmApps = core.HumanSettlementApplications.FirstOrDefault(r => r.Id == HumanSettlementApplicationId);
            var qq = core.HumanSettlementLeaseMasters.FirstOrDefault(r => r.HumanSettlementApplicationId == HumanSettlementApplicationId);
            var rrq = core.HumanSettlementLeaseDetails.FirstOrDefault(r => r.HumanSettlementApplicationId == HumanSettlementApplicationId);
            if (plmApps.IsMaster) vm.HSUnitOccupant.HumanSettlementLeaseMasterId = qq.Id;
            if (!plmApps.IsMaster) vm.HSUnitOccupant.HumanSettlementLeaseDetailsId = rrq.Id;
            vm.HSUnitOccupant.HumanSettlementApplicationId = HumanSettlementApplicationId;
            vm.HSUnitOccupant.StatusId = core.Status.FirstOrDefault(r => r.Key == StatusKeys.ActiveOccupant).Id;

            var findItem = db.HSUnitOccupants.FirstOrDefault(x => x.HumanSettlementApplicationId == HumanSettlementApplicationId && x.IDNo == vm.HSUnitOccupant.IDNo);
            var Resident = new HSUnitOccupant();
            if (findItem != null)
            {

                foreach (var prop in vm.HSUnitOccupant.GetType().GetProperties())
                {
                    if (prop.GetValue(vm.HSUnitOccupant, null) != null && prop.Name != "Id")
                    {
                        findItem.GetType().GetProperty(prop.Name).SetValue(findItem, prop.GetValue(vm.HSUnitOccupant));
                    }
                }
                core.Entry(findItem).State = EntityState.Modified;
                core.SaveChanges();
                Resident = findItem;
            }
            else
            {
                Resident = vm.HSUnitOccupant;
                core.HSUnitOccupants.Add(Resident);
                core.SaveChanges();
            }

            if (Resident.IsNominated)
            {
                var ccc = core.HSUnitOccupants.Where(r => r.HumanSettlementApplicationId == HumanSettlementApplicationId && r.Id != Resident.Id).ToList();
                plmApps.NominateSpouse = false;
                core.Entry(plmApps).State = EntityState.Modified;
                core.SaveChanges();
                foreach (var Item in ccc)
                {
                    Item.IsNominated = false;
                    core.Entry(Item).State = EntityState.Modified;
                    core.SaveChanges();
                }
            }

            Session["ShowSuccess"] = "success";
            return RedirectToAction("HumanManageAgreementOccupants", "HumanSettlementApplication", new { q = Url });
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer")]
        public JsonResult ValidateDuplicates(int? Id, string IDNo)
        {
            bool resul = false;
            var findItem = db.HSUnitOccupants.FirstOrDefault(x => x.HumanSettlementApplicationId == Id && x.IDNo == IDNo && !x.IsDeleted && x.IsActive);
            if (findItem != null) resul = true;
            return Json(resul, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        public JsonResult AgreementOfLeaseSignature(int? Id)
        {
            if (Id == null) throw new Exception("Invalid Application");
            Initialise();
            var application = db.HumanSettlementApplications.Include(r => r.Status).Where(x => x.Id == Id).FirstOrDefault();
            var findItem = db.HumanSettlementLeaseDetails.FirstOrDefault(x => x.HumanSettlementApplicationId == application.Id) ?? null;
            var _findItem = db.HumanSettlementLeaseMasters.FirstOrDefault(x => x.HumanSettlementApplicationId == application.Id) ?? null;

            if (application.Status.Key == StatusKeys.AwaitingApplicantssignature)
                MatchingHelper.HumanApplicantsSignature(new eServicesDbContext(), application.Id);
            if (application.Status.Key == StatusKeys.AwaitingLeaseAgreementApproval)
                MatchingHelper.HumanRegionalmanagersSignature(new eServicesDbContext(), application.Id, SystemUser.Id);

            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult AgreementOfLeaseSignatureAnonymous(int? Id)
        {
            if (Id == null) throw new Exception("Invalid Application");
            var application = db.HumanSettlementApplications.Include(r => r.Status).Where(x => x.Id == Id).FirstOrDefault();
            var findItem = db.HumanSettlementLeaseDetails.FirstOrDefault(x => x.HumanSettlementApplicationId == application.Id) ?? null;
            var _findItem = db.HumanSettlementLeaseMasters.FirstOrDefault(x => x.HumanSettlementApplicationId == application.Id) ?? null;
            MatchingHelper.HumanApplicantsSignature(new eServicesDbContext(), application.Id);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult UploadOccupantsDoc(int Aplication, int MasterId, int DetaisId, string ViewName, string Data)
        {
            if (Aplication == 0) throw new Exception("Invalid Application");

            Initialise();
            var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
            var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
            return RedirectToAction("HumanUploadOccupantDoc", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(
                        new
                        {
                            referenceId = Customer.Id,
                            customerId = Customer.Id,
                            referenceTypeId = referenceType.Id,
                            applicationId = application.Id,
                            agentId = application.Id,
                            returnUrl = new AesCrypto().Encrypt("Aplication=" + Aplication + "&MasterId=" + MasterId + "&DetaisId=" + DetaisId + "&ViewName=" + ViewName + "&Data=" + Data),
                            rcsappId = Aplication,
                        })));
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult AgreementOfLeaseTransfer(string Viewfrom)
        {
            Initialise();
            if (Session["Display"] != null)
            {
                ViewBag.Success = null;
                Session["Display"] = null;
                ViewBag.DisplaySwal = "True";
                if (Session["Message"] != null)
                {
                    ViewBag.MessageBodySwal = Session["Message"].ToString();
                    Session["Message"] = null;
                }
            }
            else
            {
                if (Session["TransferTenant"] != null)
                {
                    ViewBag.TransferTenant = Session["TransferTenant"].ToString();
                    Session["TransferTenant"] = null;
                    ViewBag.Success = "success";
                }
            }


            var vm = new DepartmentsApprovalViewModel();
            LeaseDetails leaseDetails = new LeaseDetails();
            vm.LeaseDetails = leaseDetails;
            return View(vm);

        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [HttpPost]
        public ActionResult AgreementOfLeaseTransfer(string ApplicationReferenceNumber, string Id)
        {
            using (var core = new eServicesDbContext())
            {
                ApplicationReferenceNumber = ApplicationReferenceNumber.ToUpper();
                var _application = core.HumanSettlementApplications.Include(d => d.Status).Where(r => r.ApplicationReferenceNumber == ApplicationReferenceNumber).FirstOrDefault() ?? null;
                switch (_application)
                {
                    case null:
                        Session["Message"] = "The Reference Number " + ApplicationReferenceNumber + " Did Not Fid A Match, Try Again!";
                        Session["Display"] = "Display";
                        return RedirectToAction("AgreementOfLeaseTransfer", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewFrom=" + ViewCodeKeys.AgreementOfLeaseTransfer) });
                    default:
                        if (_application.Status.Key == StatusKeys.ApplicantVacated)
                        {
                            Session["Message"] = "The tenant has vacated the unit for the ref. no entered!";
                            Session["Display"] = "Display";
                            return RedirectToAction("AgreementOfLeaseTransfer", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewFrom=" + ViewCodeKeys.AgreementOfLeaseTransfer) });
                        }
                        break;
                }
                return RedirectToAction("HumanTransferAgreementOfLease", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Id=" + _application.Id.ToString()) });
            }
            return View();
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult HumanTransferAgreementOfLease(int Id)
        {
            var core = new eServicesDbContext();
            var vm = new DepartmentsApprovalViewModel();
            vm.HumanSettlementApplication = core.HumanSettlementApplications.Include(r => r.PurchaserType).Include(r => r.Status).Where(x => x.Id == Id).FirstOrDefault();
            vm.HumanSettlementLeaseMaster = core.HumanSettlementLeaseMasters.Include(r => r.HumanSettlementApplication).Include(r => r.PurchaserType).Include(r => r.Status).Where(x => x.HumanSettlementApplicationId == Id).FirstOrDefault();
            vm.HumanSettlementLeaseDetails = core.HumanSettlementLeaseDetails.Include(r => r.HumanSettlementApplication).Include(r => r.PurchaserType).Include(r => r.Status).Where(x => x.HumanSettlementApplicationId == Id).FirstOrDefault();

            var Replacement = core.HSUnitOccupants.Include(f => f.Status)
                .Where(r => r.Status.Key == StatusKeys.ActiveOccupant && r.HumanSettlementApplicationId == vm.HumanSettlementApplication.Id && r.IsActive && !r.IsDeleted).ToList();
            var Spouse = new HSUnitOccupant() { Id = -1, FirstName = "Spouse" };
            if (vm.HumanSettlementApplication.SecondApplicant) Replacement.Add(Spouse);
            ViewBag.ReplacementId = new SelectList(Replacement, "Id", "FullName");
            ViewBag.TransferReason = new SelectList(core.RCSActionTypes.Where(r => r.Key == RCSActionTypeKeys.TenantDeceased || r.Key == RCSActionTypeKeys.Other).ToList(), "Key", "Name");
            ViewBag.Id = vm.HumanSettlementApplication.Id;
            return View(vm);
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [HttpPost]
        public ActionResult HumanTransferAgreementOfLease(int Id, int ReplacementId, string ReasonKey, string Email, string CellNo, DepartmentsApprovalViewModel vm)
        {
            Initialise();
            using (var core = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(core);
                    var findIApps = core.HumanSettlementApplications.Include(d => d.TitleType).Where(r => r.Id == Id).FirstOrDefault();
                    var Replacement = core.HSUnitOccupants.FirstOrDefault(r => r.Id == ReplacementId) ?? null;
                    var tenant = new HSUnitOccupant();
                    var ActivityTrackerMessage = string.Empty;
                    int emailboodyId = 0;
                    switch (ReasonKey)
                    {
                        case RCSActionTypeKeys.TenantDeceased:
                            switch (ReplacementId)
                            {
                                case -1:
                                    findIApps.FirstName = findIApps.SecAppFirstName;
                                    findIApps.LastName = findIApps.SecAppLastName;
                                    findIApps.IDNo = findIApps.SecAppIDNo;
                                    findIApps.PurEmail = findIApps.SecAppEmail;
                                    findIApps.CellNo = findIApps.SecAppCellNo;
                                    findIApps.ApplicantFullName = string.Format("{0} {1}", findIApps.FirstName, findIApps.SecAppLastName);
                                    findIApps.DOB = findIApps.SecAppDOB;
                                    findIApps.NominateSpouse = false;
                                    findIApps.IsTransferred = true;
                                    findIApps.SecondApplicant = false;
                                    MatchingHelper.UpdateApplication(core, findIApps, findIApps.Id);

                                    break;
                                default:
                                    findIApps.FirstName = Replacement.FirstName;
                                    findIApps.LastName = Replacement.LastName;
                                    findIApps.IDNo = Replacement.IDNo;
                                    findIApps.PurEmail = string.IsNullOrEmpty(Email) ? Replacement.Email : Email;
                                    findIApps.CellNo = string.IsNullOrEmpty(CellNo) ? Replacement.CellNo : CellNo;
                                    findIApps.ApplicantFullName = string.Format("{0} {1}", Replacement.FirstName, Replacement.LastName);
                                    findIApps.DOB = null;
                                    findIApps.NominateSpouse = false;
                                    findIApps.IsTransferred = true;
                                    findIApps.SecondApplicant = false;
                                    MatchingHelper.UpdateApplication(core, findIApps, findIApps.Id);

                                    Replacement.IsActive = false;
                                    Replacement.IsDeleted = true;
                                    Replacement.IsLocked = true;
                                    Replacement.IsNominated = false;
                                    core.Entry(Replacement).State = EntityState.Modified;
                                    core.SaveChanges();
                                    break;
                            }
                            break;
                        case RCSActionTypeKeys.Other:
                            switch (vm._Make_tenant)
                            {
                                case true:
                                    switch (ReplacementId)
                                    {
                                        case -1:
                                            tenant.FirstName = findIApps.FirstName;
                                            tenant.LastName = findIApps.LastName;
                                            tenant.IDNo = findIApps.IDNo;
                                            tenant.CellNo = findIApps.CellNo;
                                            tenant.Email = findIApps.PurEmail ?? null;
                                            tenant.TitleTypeId = findIApps.TitleType?.Id ?? 1;
                                            tenant.StatusId = core.Status.FirstOrDefault(r => r.Key == StatusKeys.ActiveOccupant).Id;
                                            tenant.HumanSettlementApplicationId = Id;
                                            tenant.HumanSettlementLeaseDetailsId = Replacement?.HumanSettlementLeaseDetailsId;
                                            tenant.HumanSettlementLeaseMasterId = Replacement?.HumanSettlementLeaseMasterId;
                                            tenant.Relationship = "N/A";
                                            MatchingHelper.HumanOccupantSave(core, tenant);

                                            findIApps.FirstName = findIApps.SecAppFirstName;
                                            findIApps.LastName = findIApps.SecAppLastName;
                                            findIApps.IDNo = findIApps.SecAppIDNo;
                                            findIApps.PurEmail = findIApps.SecAppEmail;
                                            findIApps.CellNo = findIApps.SecAppCellNo;
                                            findIApps.ApplicantFullName = string.Format("{0} {1}", findIApps.SecAppFirstName, findIApps.SecAppLastName);
                                            findIApps.DOB = findIApps.SecAppDOB;
                                            findIApps.NominateSpouse = false;
                                            findIApps.IsTransferred = true;
                                            findIApps.SecondApplicant = false;
                                            MatchingHelper.UpdateApplication(core, findIApps, findIApps.Id);
                                            break;

                                        default:
                                            tenant.FirstName = findIApps.FirstName;
                                            tenant.LastName = findIApps.LastName;
                                            tenant.IDNo = findIApps.IDNo;
                                            tenant.CellNo = findIApps.CellNo ?? null;
                                            tenant.Email = findIApps.PurEmail ?? null;
                                            tenant.TitleTypeId = findIApps.TitleType?.Id ?? 1;
                                            tenant.StatusId = core.Status.FirstOrDefault(r => r.Key == StatusKeys.ActiveOccupant).Id;
                                            tenant.HumanSettlementApplicationId = Id;
                                            tenant.HumanSettlementLeaseDetailsId = Replacement?.HumanSettlementLeaseDetailsId;
                                            tenant.HumanSettlementLeaseMasterId = Replacement?.HumanSettlementLeaseMasterId;
                                            tenant.Relationship = "N/A";
                                            MatchingHelper.HumanOccupantSave(core, tenant);

                                            findIApps.FirstName = Replacement.FirstName;
                                            findIApps.LastName = Replacement.LastName;
                                            findIApps.IDNo = Replacement.IDNo;
                                            findIApps.PurEmail = string.IsNullOrEmpty(Email) ? Replacement.Email : Email;
                                            findIApps.CellNo =string.IsNullOrEmpty(CellNo) ? Replacement.CellNo : CellNo;
                                            findIApps.ApplicantFullName = string.Format("{0} {1}", Replacement.FirstName, Replacement.LastName);
                                            findIApps.DOB = null;
                                            findIApps.NominateSpouse = false;
                                            findIApps.IsTransferred = true;
                                            findIApps.SecondApplicant = false;
                                            MatchingHelper.UpdateApplication(core, findIApps, findIApps.Id);

                                            Replacement.IsActive = false;
                                            Replacement.IsDeleted = true;
                                            Replacement.IsLocked = true;
                                            Replacement.IsNominated = false;
                                            core.Entry(Replacement).State = EntityState.Modified;
                                            core.SaveChanges();
                                            break;

                                    }
                                    break;

                                case false:
                                    switch (ReplacementId)
                                    {
                                        case -1:
                                            findIApps.FirstName = findIApps.SecAppFirstName;
                                            findIApps.LastName = findIApps.SecAppLastName;
                                            findIApps.IDNo = findIApps.SecAppIDNo;
                                            findIApps.PurEmail = findIApps.SecAppEmail;
                                            findIApps.CellNo = findIApps.SecAppCellNo;
                                            findIApps.ApplicantFullName = string.Format("{0} {1}", findIApps.SecAppFirstName, findIApps.SecAppLastName);
                                            findIApps.DOB = findIApps.SecAppDOB;
                                            findIApps.NominateSpouse = false;
                                            findIApps.IsTransferred = true;
                                            findIApps.SecondApplicant = false;
                                            MatchingHelper.UpdateApplication(core, findIApps, findIApps.Id);
                                            break;

                                        default:
                                            findIApps.FirstName = Replacement.FirstName;
                                            findIApps.LastName = Replacement.LastName;
                                            findIApps.IDNo = Replacement.IDNo;
                                            findIApps.PurEmail = string.IsNullOrEmpty(Email) ? Replacement.Email : Email;
                                            findIApps.CellNo = string.IsNullOrEmpty(CellNo) ? Replacement.CellNo : CellNo;
                                            findIApps.ApplicantFullName = string.Format("{0} {1}", Replacement.FirstName, Replacement.LastName);
                                            findIApps.DOB = null;
                                            findIApps.NominateSpouse = false;
                                            findIApps.IsTransferred = true;
                                            findIApps.SecondApplicant = false;
                                            MatchingHelper.UpdateApplication(core, findIApps, findIApps.Id);

                                            Replacement.IsActive = false;
                                            Replacement.IsDeleted = true;
                                            Replacement.IsLocked = true;
                                            Replacement.IsNominated = false;
                                            core.Entry(Replacement).State = EntityState.Modified;
                                            core.SaveChanges();
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }

                    emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.TransferSuccessful).Id;
                    EmailHelper.CustomerEmailOrSMSNotification(core, findIApps.Id, emailboodyId);

                    ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TransferredSuccessfully).Description.ToString();
                    ActivityTrackerMessage = string.Format(ActivityTrackerMessage, findIApps.FirstName, findIApps.LastName, Customer.FullName);
                    MatchingHelper.ActivityTrackerHuman(core, findIApps.Id, ActivityTrackerMessage, Customer.Id);

                    Session["TransferTenant"] = string.Format("{0} {1}", findIApps.FirstName, findIApps.LastName);
                }
                catch (Exception Ex)
                {

                    //throw;
                }
            }
            return RedirectToAction("AgreementOfLeaseTransfer", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewFrom=" + ViewCodeKeys.AgreementOfLeaseTransfer) });
        }

        [Authorize(Roles = "Housing liaison officer, Senior Housing Specialist, Regional Manager, Customers, Housing Liaison Officer")]
        public ActionResult CheckContactInfoForReplacement(int Id)
        {
            var core = new eServicesDbContext();
            var find = core.HSUnitOccupants.FirstOrDefault(r => r.Id == Id);
            var result = string.Empty;
            if (string.IsNullOrEmpty(find.Email) && !string.IsNullOrEmpty(find.CellNo)) result = "email";
            if (!string.IsNullOrEmpty(find.Email) && string.IsNullOrEmpty(find.CellNo)) result = "cell";
            if (string.IsNullOrEmpty(find.Email) && string.IsNullOrEmpty(find.CellNo)) result = "both";
            if (!string.IsNullOrEmpty(find.Email) && !string.IsNullOrEmpty(find.CellNo)) result = "_good_record";
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AllPropertyLeases()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var rCSApplicationStatus = cxt.LeaseDetails.OrderByDescending(r => r.Id).Include(r => r.Status).Include(r => r.PropertyLeaseApplication).Include(r => r.SystemUser).Include(r => r.PurchaserType).ToList();
                    List<LeaseDetailsViewModel> vm = new List<LeaseDetailsViewModel>();
                    foreach (var item in rCSApplicationStatus)
                    {
                        var rr = new LeaseDetailsViewModel();
                        var mm = cxt.MatchedUnits.OrderByDescending(r => r.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == item.PropertyLeaseApplicationId && x.IsAccepted)?.UnitsEkurhuleniHousingCompanyId ?? null;
                        rr.EkurhulreniEHCUnitId = mm != null ? mm.ToString() : 1000.ToString();
                        rr.StatusName = item.Status.Name;
                        rr.TenantType = item.PurchaserType.Name;
                        rr.TenantFullName = item.FirstNames + " " + item.LastName;
                        rr.ReferenceNumber = item.leaseApplicationRef;
                        rr.CreatedDateTime = item.CreatedDateTime;
                        rr.Id = item.Id;
                        rr.PropertyLeaseApplicationId = item.PropertyLeaseApplicationId;
                        rr.CompletionStatus = item.Completed == true ? "Completed" : "In Progress";
                        vm.Add(rr);
                    }
                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
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
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
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

                return View("_Error");
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
                        .Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.PurchaserType).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();

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

                return View("_Error");
            }
        }

        [Authorize(Roles = "Regional Manager, Senior Housing Specialist, Housing Liaison Officer, Caretaker, Super Administrators, Back Office System Administrator")]
        [HttpGet]
        public ActionResult PropertyTenantCommunication()
        {
            try
            {


                Initialise();
                var core = new eServicesDbContext();
                var user = UserManager.FindByName(SystemUser.UserName);
                var userId = user.Id;
                List<string> Roles = UserManager.GetRoles(userId).ToList();

                var vm = new DepartmentsApprovalViewModel();
                if (Roles.Contains("Regional Manager") || (User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                {
                    var UserId = Customer.Id;
                    
                    var Alloc = new List<UserWorkAllocation>();
                    if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                        Alloc = core.UserWorkAllocations.Where(r => r.IsActive && !r.IsDeleted && r.RRActive).ToList();
                    else
                        Alloc = core.UserWorkAllocations.Where(r => r.SystemUserId == SystemUser.Id && r.IsActive && !r.IsDeleted && r.RRActive).ToList();

                    var Allocation = Alloc.Select(d => d.PreferredComplexAreaId).Distinct().ToList();
                    var ComplexNames = core.PreferredComplexAreas.Where(x => Allocation.Contains(x.Id) && x.Key.Contains("h_") && x.IsActive).ToList();
                    var rrq = ComplexNames.Select(x => x.Id).ToList();
                    var list = core.HumanSettlementApplications.Include(r => r.Customer).Where(x => (rrq.Contains((int)x.PreferredComplexArea2Id) || rrq.Contains((int)x.PreferredComplexAreaId))).ToList()/*.Select(x => x.Customer.SystemUserId)*/;
                    //var users = core.SystemUsers.Where(x => list.Contains(x.Id)).ToList();
                    //var ttt = (list.FirstOrDefault().ApplicantFullName)
                    vm.Features = new SelectList(ComplexNames.OrderBy(r => r.Name), "Id", "Name");
                    vm.Data = ComplexNames;
                    ViewBag.Users = new SelectList(list.GroupBy(d => d.IDNo).Select(grp => grp.First()).OrderBy(ord => ord.ApplicantFullName).ToList(), "Id", "ApplicantFullName").ToList();
                    ViewBag.Features = new SelectList(ComplexNames, "Id", "Name");
                    ViewBag.Complexes = ComplexNames;

                }
                else if (Roles.Contains("Housing Liaison Officer") || Roles.Contains("Senior Housing Specialist") || Roles.Contains("Caretaker")|| (User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                {
                    var UserId = Customer.Id;

                    var Alloc = new List<UserWorkAllocation>();
                    if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                        Alloc = core.UserWorkAllocations.Where(r => r.IsActive && !r.IsDeleted && r.RRActive).ToList();
                    else
                        Alloc = core.UserWorkAllocations.Where(r => r.SystemUserId == SystemUser.Id && r.IsActive && !r.IsDeleted && r.RRActive).ToList();

                    var Allocation = Alloc.Select(d => d.PreferredComplexAreaId).Distinct().ToList();

                    var ComplexNames = core.PreferredComplexAreas.Where(x => Allocation.Contains(x.Id) && x.Key.Contains("h_") && x.IsActive).ToList();
                    var rrq = ComplexNames.Select(x => x.Id).ToList();
                    var list = core.HumanSettlementApplications.Include(r => r.Customer).Where(x => (rrq.Contains((int)x.PreferredComplexArea2Id) || rrq.Contains((int)x.PreferredComplexAreaId))).ToList()/*.Select(x => x.Customer.SystemUserId)*/;
                    //var users = core.SystemUsers.Where(x => list.Contains(x.Id)).ToList();

                    vm.Features = new SelectList(ComplexNames.OrderBy(r => r.Name), "Id", "Name");
                    vm.Data = ComplexNames;
                    ViewBag.Users = new SelectList(list.GroupBy(d => d.IDNo).Select(grp => grp.First()).OrderBy(ord => ord.ApplicantFullName).ToList(), "Id", "ApplicantFullName").ToList();
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

        [Authorize(Roles = "Regional Manager, Senior Housing Specialist, Housing Liaison Officer, Caretaker, Super Administrators, Back Office System Administrator")]
        [HttpPost]
        public ActionResult PropertyTenantCommunication(DepartmentsApprovalViewModel vm, string CommunicationType, string Title, string MessageBody, string Title2, string MessageBody2, bool EMAIL, bool SMS, bool POSTAL, int? User, List<PreferredComplexArea> Features, params string[] SelectedRoles)
        {

            try
            {
                Initialise();
                var success = "";
                var core = new eServicesDbContext();
                if (CommunicationType == "Individual")
                {
                    if (User != null)
                    {
                        var rr = core.SystemUsers.FirstOrDefault(x => x.Id == Customer.SystemUserId);
                        var user = UserManager.FindByName(rr.UserName);
                        var userId = user.Id;
                        string role = UserManager.GetRoles(userId).FirstOrDefault();
                        var users = core.SystemUsers.Where(x => x.Id == User).ToList();
                        var p = core.HumanSettlementApplications.Include(r => r.Customer.SystemUser).Where(x => x.Id == User).ToList();
                        var messagebody = MessageBody;
                        messagebody += "<br/><br/>";
                        messagebody += "<b>Management</b><br/>";
                        messagebody += "<b>" + role + "</b><br/>";
                        messagebody += rr.FullName + "</b></b>";
                        messagebody += "Cell: " + rr.MobileNumber;
                        messagebody += "<br/><br/>";
                        messagebody += "Email: " + rr.EmailAddress;

                        MessageBody = String.Format("Property Lease(HSD), {0}", MessageBody);

                        Email email = new Email();
                        var result = email.GenerateBulkEmailsOrSMS2(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Title.ToLower()), messagebody, SMS, EMAIL, POSTAL, AppSettingKeys.EservicesDefaultEmailTemplate, p, Customer.Id, false, "Tenats", MessageBody);

                        success = string.Format("Communication sent to {0} successfully. {1}", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.First().ApplicantFullName.ToLower()), result);
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
                        var rr = core.SystemUsers.FirstOrDefault(x => x.Id == Customer.SystemUserId);
                        var user = UserManager.FindByName(rr.UserName);
                        var userId = user.Id;
                        string role = UserManager.GetRoles(userId).FirstOrDefault();
                        List<int> dd = new List<int>();
                        foreach (var I in SelectedRoles)
                        {
                            dd.Add(Convert.ToInt16(I));
                        }
                        var selected = core.PreferredComplexAreas.Where(x => dd.Contains(x.Id)).ToList();
                        var UserId = Customer.Id;
                        var Allocation = core.UserWorkAllocations.Where(r => r.SystemUserId == user.SystemUserId && r.IsActive && !r.IsDeleted && r.RRActive).ToList().Select(d => d.PreferredComplexAreaId);
                        var ComplexNames = core.PreferredComplexAreas.Where(x => Allocation.Contains(x.Id) && x.Key.Contains("h_") && x.IsActive).ToList();
                        var Users = core.HumanSettlementApplications.Include(d => d.Customer.SystemUser).Where(x => (dd.Contains((int)x.PreferredComplexArea2Id) || dd.Contains((int)x.PreferredComplexAreaId))).ToList();

                        int counter = 0;
                        var names = "";
                        foreach (var Roles in selected)
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
                        messagebody += "<b>Management</b><br/>";
                        messagebody += "<b>" + role + "</b><br/>";
                        messagebody += rr.FullName + "</b></b>";
                        messagebody += "Cell: " + rr.MobileNumber;
                        messagebody += "<br/>";
                        messagebody += "Email: " + rr.EmailAddress;
              MessageBody = String.Format("Property Lease(HSD), {0}", MessageBody);
                      
                        Email email = new Email();
                        var result = email.GenerateBulkEmailsOrSMS2(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Title.ToLower()), messagebody, SMS, EMAIL, POSTAL, AppSettingKeys.EservicesDefaultEmailTemplate, Users.GroupBy(d => d.IDNo).Select(grp => grp.First()).OrderBy(ord => ord.ApplicantFullName).ToList(), Customer.Id, true, "Tenants", MessageBody);
                        vm.Features = new SelectList(ComplexNames, "Id", "Name");
                        ViewBag.Users = new SelectList(Users.GroupBy(d => d.IDNo).Select(grp => grp.First()).OrderBy(ord => ord.ApplicantFullName).ToList(), "Id", "ApplicantFullName").ToList();
                        ViewBag.SMS = false;
                        ViewBag.EMAIL = false;
                        ViewBag.POSTAL = false;
                        success = string.Format("Communication sent to selected group(s) successfully. {0}", result);
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

        [DecryptParameter]
        public ActionResult ConductAssessment(int rcsAppId)
        {
            if (rcsAppId == null) throw new Exception("Invalid application.");
            var context = new eServicesDbContext();
            try
            {
                Initialise();
                var PLA = context.HumanSettlementApplications.Include(x => x.Status).Include(x => x.PurchaserType).Include(x => x.Customer).Include(x => x.SystemUser).FirstOrDefault(x => x.Id == rcsAppId);
                var Customerid = Customer;
                var customer = context.Customers.FirstOrDefault(x => x.Id == PLA.CustomerId);
                var systemusers = context.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                var applicationProp = context.HumanSettlementApplications.Include(x => x.SystemUser).Include(x => x.Status).Include(r => r.Customer).Include(x => x.PurchaserType).FirstOrDefault();
                ViewBag.r = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.FirstName = applicationProp.FirstName;
                ViewBag.RfNo = applicationProp.ApplicationReferenceNumber;
                ViewBag.DateCreated = applicationProp.CreatedDateTime.Value;
                ViewBag.Sat = applicationProp.FirstName;
                RiskAssessmentOutcome rao = new RiskAssessmentOutcome();
                rao.FirstName = systemusers.LastName;
                rao.LastName = systemusers.FirstName;

                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                var referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                MatchingHelper.DocumentRiskAssessmentOutcome(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsAppId, true);

                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();

                var cmv = new CaptureViewModel
                {
                    DocumentsViewModel = dvm,
                    HumanSettlementApplication = PLA,
                    //DocumentCheckList = documentCheckLists,
                    //PropertyLeaseApplication = PLA,
                    RiskAssessmentOutcome = rao,
                    vaIDno = PLA.Id.ToString()
                };

                ViewBag.DateStampConduct = DateTime.Now;
                ViewBag.ConductPrpertyId = PLA.Id;
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.Outcome = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Id", "Name");
                ViewBag.date = DateTime.Now.Date;
                ViewBag.CreatedBySystemUserId = new SelectList(context.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(context.SystemUsers, "Id", "FirstName");
                ViewBag.PreferredArea = new SelectList(db.PreferredComplexAreas.OrderBy(x => x.Name).Where(x => x.IsActive && x.Key.Contains("h_")).ToList(), "Id", "Name");
                var cc = db.PreferredComplexAreas.Select(d => d.Id).ToList();
                ViewBag.PreferredUnit = null;

                return View(cmv);
            }
            catch (Exception Io)
            {

                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConductAssessment(CaptureViewModel capture, string ApprovalStatusddl, string OutcomeReason1, string OutcomeReason2)
        {
            using (var core = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var ActivityTrackerMessage = "";
                    int emailboodyId = 0;
                    var Key = core.Status.FirstOrDefault(x => x.Key == StatusKeys.Approved);
                    var approval = core.RCSActionTypes.FirstOrDefault(d => d.Key == ApprovalStatusddl);
                    var rrq = new RiskAssessmentOutcome
                    {
                        Outcome = approval.Description,
                        HumanSettlementApplicationId = capture.HumanSettlementApplication.Id,
                        FirstName = SystemUser.FirstName,
                        LastName = SystemUser.LastName,
                        Reason = String.IsNullOrEmpty(OutcomeReason1) ? OutcomeReason2 : OutcomeReason1,
                        OfficialNumber = capture.RiskAssessmentOutcome.OfficialNumber,
                        DateStamp = DateTime.Now,
                        StatusId = Key.Id,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false,
                        CapturedById = SystemUser.Id
                    };
                    core.RiskAssessmentOutcomes.Add(rrq);
                    core.SaveChanges();

                    var PLA = core.HumanSettlementApplications.FirstOrDefault(x => x.Id == capture.HumanSettlementApplication.Id);
                    var ResponsibilityTypeId = core.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RiskAssessment).FirstOrDefault();
                    switch (ApprovalStatusddl)
                    {
                        case RCSActionTypeKeys.Approved:
                            MatchingHelper.ChangeHumanStatus(core, (int)core.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots)?.Id, PLA.Id);
                            ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantUploadsDocuments).Description.ToString();
                            emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentRejected).Id;
                            break;
                        case RCSActionTypeKeys.Rejected:
                            MatchingHelper.ChangeHumanStatus(core, (int)core.Status.FirstOrDefault(x => x.Key == StatusKeys.CreditScoreRejected)?.Id, PLA.Id);
                            ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantUploadsDocuments).Description.ToString();
                            emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentRejected).Id;
                            break;
                    }
                    EmailHelper.CustomerEmailOrSMSNotification(core, PLA.Id, emailboodyId);
                    MatchingHelper.ActivityTrackerHuman(core, PLA.Id, ActivityTrackerMessage, Customer.Id);

                    var encryptstring = new AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.AgreementOfLeaseTransfer);
                    return RedirectToAction("ApplicantRiskAssessment", "HumanSettlementApplication", new { q = encryptstring });
                }
                catch (Exception Io)
                {
                    return View("_Error");
                }
            }

        }
        [Authorize]
        [DecryptParameter]
        public ActionResult ApplicationRiskAssessment(string ViewName)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    //MatchingHelper.MatchUnitParallelProcessor(cxt);
                    //var Keys = cxt.Status;
                    //int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    //int UserId = Customer.Id;
                    //List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();

                    //var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RiskAssessment).FirstOrDefault().Id;
                    //var ResponsibilityTypeId2 = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalRiskAssessment).FirstOrDefault().Id;

                    //rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => (x.ResponsibilityTypeId == ResponsibilityTypeId || x.ResponsibilityTypeId == ResponsibilityTypeId2) &&x.ClerkId== UserId && x.StatusId == SubmittedId).ToList();

                    //var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();
                    //var list2 = rrq.Select(x => x.LeaseDetailsId).ToList();

                    //var AwaitingRiskAssessment = cxt.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingRiskAssessment).Id;

                    var rCSApplicationStatus = db.HumanSettlementApplications.Include(d => d.Status).Where(x => /*x.IsDeleted == false && list.Contains(x.Id) &&*/ x.Status.Key == StatusKeys.AwaitingRiskAssessment)
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.Customer)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.ModifiedBySystemUser)
                        .Include(r => r.Status).ToList();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    var vm = new DepartmentsApprovalViewModel
                    {
                        HumanSettlementApplicationtList = rCSApplicationStatus
                        //LeaseDetailsList = LeaseApplications
                    };


                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
            }
        }
        public ActionResult ApplicantRiskAssessment()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var rCSApplicationStatus = db.HumanSettlementApplications.Include(d => d.Status).Where(x => /*x.IsDeleted == false && list.Contains(x.Id) &&*/ x.Status.Key == StatusKeys.AwaitingRiskAssessment)
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.Customer)
                        .Include(r => r.PurchaserType)
                        .Include(r => r.ModifiedBySystemUser)
                        //.Include(r => r.HumanEHCOptions)
                        .Include(r => r.Status).ToList();


                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }


                    var vm = new DepartmentsApprovalViewModel
                    {
                        HumanSettlementApplicationtList = rCSApplicationStatus
                        //LeaseDetailsList = LeaseApplications
                    };


                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
            }
        }

        [Authorize(Roles = "Revenue Officer,Housing Supervisor")]
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

                        rCSApplicationStatus = db.LeaseDetails.Where(x => x.IsDeleted == false && list.Contains(x.Id) && (x.StatusId == AwaitingTenantAccountBalanceReview || x.StatusId == EndOfLeaseTerm || x.StatusId == LeaseNotRenewed || x.StatusId == TenantNotice))
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Status)
                            .Include(r => r.ModifiedBySystemUser).ToList();
                    }
                    else if (User.IsInRole("Housing Supervisor"))
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
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
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

                return View("_Error");
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

                    var rCSApplicationStatus = db.HumanSettlementApplications.Where(x => x.IsDeleted == false && x.CustomerId == Customer.Id).Include(r => r.PurchaserType)
                        .Include(r => r.CreatedBySystemUser).Include(r => r.PurchaserType).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.Status).ToList();

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

                return View("_Error");
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

                    rrq = cxt.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.StatusId == SubmittedId).ToList();

                    var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                    var AssessmentFeePaymentApproved = db.Status.FirstOrDefault(i => i.Key == StatusKeys.AssessmentFeePaymentApproved).Id;

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && list.Contains(x.Id) && x.StatusId == AssessmentFeePaymentApproved)
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
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
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

                    if ((User.IsInRole("Lease Official")) || (User.IsInRole("Letting Officer")))
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
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
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
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
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
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
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

                return View("_Error");
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
                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AssessmentFeePaymentApproved).Id, (int)id);
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, activeDirectoryOn);
                    EHCRoundRobin(rcsApps.Id, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);


                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApproveDeposit).Description.ToString();
                    MatchingHelper.ActivityTrackerAudit(db, rcsApps.Id, ActivityTrackerMessage, custmusers.Id);
                    int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.DepositPaymentApprove).Id;
                    EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                }
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

                    MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingManagersSignature).Id, (int)id);

                    EHCRoundRobin(rcsApps.Id, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                    //Send e-mail and SMS notification
                    int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                    EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                }
                //not sure what message should be recorded here
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.LeaseAgreementApproved).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantUpdateDetails).Id, (int)id);
                EHCRoundRobin(rcsApps.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);


                MatchingHelper.AddCommentOnRejectAgreement(db, Comment, (int)id);

                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresPOPRejected).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
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

        public ActionResult GetLIMSProperties(string IDNo)
        {

            var res = Lims10.LIMSApi(IDNo);
            //var uploaded = Convert.ToString(Session["DocumentUploadedCapture"]);
            //Session["DocumentUploadedCapture"] = null;
            //ViewBag.DocumentUploadedCapture = uploaded;
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        
        [HttpPost]
        [Authorize]
        public ActionResult CaptureCommitteeOutcome(int ApplicationId, string ApprovalStatusddl, string CommitteeDate)
        {
            try
            {
                using (var core = new eServicesDbContext())
                {
                    Initialise();
                    var Keys = core.Status;
                    var rcsApps = core.HumanSettlementApplications.FirstOrDefault(d => d.Id == ApplicationId);
                    var master = core.HumanSettlementLeaseMasters.FirstOrDefault(x => x.HumanSettlementApplicationId == rcsApps.Id);
                    var details = core.HumanSettlementLeaseDetails.FirstOrDefault(x => x.HumanSettlementApplicationId == rcsApps.Id);
                    var ResponsibilityTypeId = core.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CommitteeOutcomes).FirstOrDefault();

                    if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                    {
                        CaptureController c = new CaptureController();
                        MatchingHelper.ChangeHumanStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionGranted).Id, ApplicationId);
                        if (details != null) 
                            MatchingHelper.ChangeLeaseDetailsStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionGranted).Id, details.Id);
                        if (details != null) 
                            MatchingHelper.ChangeMasterStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionGranted).Id, master.Id);
                        MatchingHelper.Commiteedate2(core, rcsApps.Id, CommitteeDate);

                        int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.EvictionGranted).Id;
                        EmailHelper.CustomerEmailOrSMSNotification(db, rcsApps.Id, emailboodyId);

                        var custmusers = core.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        var ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.LeaseAgreementApproved).Description.ToString();
                        MatchingHelper.ActivityTrackerHuman(core, ApplicationId, ActivityTrackerMessage, custmusers.Id);

                        Session["MessageBody"] = "Eviction Granted, Outcome Captured Successfully";
                        Session["OutcomeCapturedSuccessfully"] = "true";
                        Session["Display"] = "true";
                    }
                    else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
                    {
                        MatchingHelper.ChangeHumanStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionNotGranted).Id, ApplicationId);
                        if (details != null)
                            MatchingHelper.ChangeLeaseDetailsStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionNotGranted).Id, details.Id);
                        if (master != null)
                            MatchingHelper.ChangeMasterStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.EvictionNotGranted).Id, master.Id);

                        var custmusers = core.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                        var ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresPOPRejected).Description.ToString();
                        MatchingHelper.ActivityTrackerHuman(core, ApplicationId, ActivityTrackerMessage, custmusers.Id);

                        Session["MessageBody"] = "Eviction Not Granted, Outcome Captured Successfully";
                        Session["OutcomeCapturedSuccessfully"] = "true";
                        Session["Display"] = "true";
                    }
                    return RedirectToAction("HumanApplicationEvictions", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.EvictionCommitteOutcomeList) });
                }
            }
            catch (Exception Error)
            {
                EventLogHelper.LogSystemError(Error.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
            }          
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
                    int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                    EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                }
                //not sure what message should be recorded here
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.LeaseAgreementApproved).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantUpdateDetails).Id, (int)id);
                EHCRoundRobin(rcsApps.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);


                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresPOPRejected).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
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

                var findItem = context.propertyLeaseAgreementMasters.FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsApps.Id);
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
                    int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                    EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);
                }
                //not sure what message should be recorded here
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.LeaseAgreementApproved).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
            }
            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                MatchingHelper.ChangeApplicationStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingTenantUpdateDetails).Id, (int)id);
                EHCRoundRobin(rcsApps.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);



                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AssessmentFiguresPOPRejected).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, id, ActivityTrackerMessage, custmusers.Id);
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



        public JsonResult RevenueManagergnLeaseAgreement(int? Id)
        {
            if (Id == null) throw new Exception("Invalid Application");
            Initialise();
            var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == Id);
            var lease = db.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IsNew && x.IsActive && !x.IsDeleted);
            var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueManager).FirstOrDefault().Value);
            var User = db.Customers.Include(r => r.SystemUser).FirstOrDefault(x => x.Id == activeDirectoryOn);
            if (lease == null) throw new Exception("Invalid Application Lease");
            MatchingHelper.RevenueManagerSignLeaseAgreement(db, application, lease, User);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RevenueSignDebitOrder(int? id)
        {
            if (id == null) throw new Exception("Invalid Application");
            Initialise();
            var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == id);
            var dbto = db.DebitOrderRegistrations.OrderByDescending(r => r.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id);
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
        #region Init
        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        public HumanSettlementApplicationController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {
            Initialise();

        }



        public HumanSettlementApplicationController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
            Initialise();
        }

        public HumanSettlementApplicationController(eServicesDbContext context)
        {

            UserManager =
            new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(context));
            Initialise();
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
            var AppUnit = core.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == Id);
            var Match = AppUnit != null ? core.MatchedUnits.FirstOrDefault(x => x.Id == AppUnit.MatchedID) : null;
            var Unit = Match != null ? core.Units.FirstOrDefault(x => x.Id == Match.UnitsId) : null;
            var Units = Match != null ? core.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == Match.UnitsEkurhuleniHousingCompanyId) : null;
            var pca = Units != null ? core.PreferredComplexAreas.FirstOrDefault(x => x.Id == Units.PreferredComplexAreaId) : null;
            if (pca != null)
            {
                var result = LF == true && pca != null ? UserId = core.Customers.FirstOrDefault(x => x.Id == pca.LettingOfficerId) : UserId = core.Customers.FirstOrDefault(x => x.Id == pca.HousingSuperId);
            }
            return UserId;
        }
        //                                              1                       2                           3                       4                       5                           6                           7                               8                           9                              10                   11                   12                       13                        14                         15                          16                              17                        18                       19                         20                   21         22          23          24                    25                     26                         27                                       
        public int EHCRoundRobin(int RCSAppID, bool RiskAssessment, bool ValidateDepositPayment, bool InviteToClientTraining, bool UnitInspections, bool UpdateTenantDetails, bool GenerateLeaseAgreement, bool LeaseAgreementValidation, bool DebitOrderValidation, bool ShechuleInspectionSlots, bool MaintananceJobSheet, bool Terminations, bool TerminationValidation, bool CommitteeOutcomes, bool VacatingConfirmation, bool RecomendForRenewal, bool SecondRenewalRecommendation, bool LeaseRenewalRevenue, bool RenewalRiskAssessment, bool UnitMaintenance, bool AgreemrntValidateRevenue, bool six, bool seven, bool eight, int DepartmentID, bool AcknowlegeRefund, bool IssueRefundsCollection, int RefundAppID)
        {
            Initialise();
            //get all users for each department
            var applicationUserRoles = db.Roles.ToList();
            //DateTime.Compare(x.DateTimeValueColumn.Date, DateTime.Now.Date) <= 0
            //x => EntityFunctions.TruncateTime(x.DateTimeStart) == currentDate.Date
            //try
            //{
            //    var dates = new string[2];
            //    dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            //    dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            //    var startDate = DateTime.Parse(dates[0]).Date;
            //    var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            //    var oneDayTime = endDate - startDate;
            //    var NumOfDDAppPending = db.RoundRobinQueues.Where(x => x.IsActive == true && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();

            //    //DateTime DateFrom = DateTime.Now;
            //    //DateTime DateTo = DateTime.Now;
            //    //var startDate = DateTime.TryParseExact(DateFrom, "yyyyMMdd");
            //    //endDate = DateTime.ParseExact(dates[1], "yyyyMMdd").AddTicks(-1).AddDays(1);
            //    //var RoundRobingQueue1 = db.RoundRobinQueues.Where(x => DbFunctions.TruncateTime(x.CreatedDateTime) == DateTime.Now).ToList();

            //    //var NumOfDDAppPending = db.RoundRobinQueues.Where(x => x.IsActive == true && (DateFrom <= x.CreatedDateTime && DateTo >= x.CreatedDateTime)).ToList();

            //}
            //catch (Exception IO)
            //{

            //}
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

            var AssignedToUser = 0;
            var responsibilityTypes = db.ResponsibilityTypes.ToList();

            if (RiskAssessment)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);

                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);

                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RiskAssessment).FirstOrDefault();
                if (activeDirectoryOn != null)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var roundRobinQueue = new RoundRobinQueue();
                    roundRobinQueue.PropertyLeaseApplicationId = RcsApplication.Id;
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

                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.CommunityDevelopmentOfficer).FirstOrDefault().Value);

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
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor).FirstOrDefault().Value);
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

                var UserId = GetBackOfficeId(db, RCSAppID, false);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor).FirstOrDefault().Value);
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

                var UserId = GetBackOfficeId(db, RCSAppID, false);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor).FirstOrDefault().Value);
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
                var UserId = GetBackOfficeId(db, RCSAppID, true);
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
                var UserId = GetBackOfficeId(db, RCSAppID, true);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                var activeDirectoryOn = UserId.Id;

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
                var UserId = GetBackOfficeId(db, RCSAppID, true);
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
                var UserId = GetBackOfficeId(db, RCSAppID, false);
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
                var UserId = GetBackOfficeId(db, RCSAppID, true);
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
                var UserId = GetBackOfficeId(db, RCSAppID, false);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor).FirstOrDefault().Value);
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
                var UserId = GetBackOfficeId(db, RCSAppID, true);
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
                var UserId = GetBackOfficeId(db, RCSAppID, true);
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
                var UserId = GetBackOfficeId(db, RCSAppID, false);
                var StoredUser = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor).FirstOrDefault().Value);
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
            var findItem = db.RoundRobinQueues.Include(r => r.Clerk).Include(r => r.ResponsibilityType).FirstOrDefault(x => x.StatusId == SubmittedId && x.PropertyLeaseApplicationId == plmApps.Id);
            var bouserid = db.Customers.FirstOrDefault(x => x.Id == findItem.ClerkId).SystemUserId;
            var userrole = db.ApplicationUserRoles.Include(r => r.IdentityRole).OrderByDescending(x => x.Id).FirstOrDefault(x => x.SystemUserId == bouserid);
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
                    .Include(r => r.Status)
                    .Include(r => r.SystemUser)
                    .Include(r => r.CreatedBySystemUser)
                    .Include(r => r.HumanEHCOptions)
                    .Include(r => r.Customer).ToList();

                foreach (var Item in plmApps)
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
            var area = db.PreferredComplexAreas.Include(r => r.LettingOfficer).Include(r => r.HousingSuper).FirstOrDefault(x => x.Id == id);

            var userrole = area.HousingSuper != null ? db.ApplicationUserRoles.Include(r => r.IdentityRole).OrderByDescending(x => x.Id).FirstOrDefault(x => x.SystemUserId == area.HousingSuper.SystemUserId) : null;
            var RoleName = userrole != null ? db.Roles.Where(x => x.Name == userrole.IdentityRole.Name).FirstOrDefault().Id : db.Roles.Where(x => x.Name == "Housing Supervisor").FirstOrDefault().Id;

            var userrole2 = area.LettingOfficer != null ? db.ApplicationUserRoles.Include(r => r.IdentityRole).OrderByDescending(x => x.Id).FirstOrDefault(x => x.SystemUserId == area.LettingOfficer.SystemUserId) : null;
            var RoleName2 = userrole2 != null ? db.Roles.Where(x => x.Name == userrole2.IdentityRole.Name).FirstOrDefault().Id : db.Roles.Where(x => x.Name == "Letting Officer").FirstOrDefault().Id;

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

                    prr = db.Customers.Include(x => x.SystemUser).Where(x => list.Contains(x.Id)).ToList();

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

                    var PLA = db.HumanSettlementApplications
                    .Include(r => r.PurchaserType)
                    .Include(r => r.IdentificationType)
                    .Include(r => r.Status)
                    .Where(x => x.Id == rcsAppId).FirstOrDefault();

                    var Customer = db.Customers.Where(x => x.Id == PLA.CustomerId).FirstOrDefault();

                    string baseFormat = "";
                    var referenceType = cxt.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                    var application = cxt.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    return RedirectToAction("Index3", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(
                        new
                        {
                            referenceId = Customer.Id,
                            customerId = Customer.Id,
                            referenceTypeId = referenceType.Id,
                            applicationId = application.Id,
                            agentId = application.Id,
                            returnUrl = baseFormat,
                            rcsappId = PLA.Id
                        })));

                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
            }
        }




        [DecryptParameter]
        public ActionResult CaptureMasterUpdateAgreement(int? ApplicationId, string Data)
        {
            Initialise();
            if (Request.IsAuthenticated)
            {
                var core = new eServicesDbContext();
                var Application = db.HumanSettlementApplications.Where(x => x.IsDeleted == false && x.Id == ApplicationId).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.PreferredComplexArea).Include(r => r.PreferredComplexArea2)
                      .Include(r => r.TitleType).Include(r => r.SecAppTitleType)
                     /* .Include(r => r.HumanEHCOptions)*/.Include(r => r.PurchaserType).Include(r => r.Status).FirstOrDefault();

                var leaseApplication = db.HumanSettlementLeaseMasters.Where(x => x.IsDeleted == false && x.HumanSettlementApplicationId == ApplicationId).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.ModifiedBySystemUser).Include(r => r.Status).FirstOrDefault() ?? null;

                var findItem = db.HumanSettlementLeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                  .Include(r => r.ModifiedBySystemUser).Include(r => r.Status).FirstOrDefault() ?? null;

                Application.Data = Data;

                TenantViewModel model = new TenantViewModel
                {
                    HumanSettlementApplication = Application,
                    HumanSettlementLeaseDetails = findItem,
                    HumanSettlementLeaseMaster = leaseApplication
                };
                ViewBag.ApplicationId = Application.Id;

                var Users = db.SystemUsers.ToList();
                foreach (var rr in Users)
                    rr.Data = string.Format("{0} {1}", rr.FirstName, rr.LastName);

                var userWorkAllocation = db.UserWorkAllocations.Where(a => a.SystemUserId == SystemUser.Id && a.RRActive).Select(d => d.PreferredComplexAreaId).ToList();
                var rcsType = db.RCSTypes.OrderBy(x => x.Name);
                ViewBag.DOB = model.HumanSettlementApplication.DOB.ToString().Substring(0, 10);
                ViewBag.IncomeBrackets = new SelectList(db.HSIncomeBrackets.OrderBy(x => x.Id).ToList(), "Id", "Name");
                ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Key == PurchaserTypeKeys.NaturalPerson), "Id", "Name");
                ViewBag.Option = new SelectList(db.IncomeSources.OrderBy(x => x.SourceOfIncome).ToList(), "Id", "SourceOfIncome");
                ViewBag.UnitType = new SelectList(db.OccupationTypes.OrderBy(x => x.Propertytype), "Id", "Propertytype");
                ViewBag.PreferredArea = new SelectList(db.PreferredComplexAreas.OrderBy(x => x.Name).Where(x => x.IsActive && x.Key.Contains("h_") && userWorkAllocation.Contains(x.Id)).ToList(), "Id", "Name");
                ViewBag.CompanyType = new SelectList(db.companyTypes.OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.HumanOptions = new SelectList(db.humanEHCOptions.OrderBy(x => x.Name).Where(x => x.Key != "hs_single_flats" || x.Key != "hs_single_flats").ToList(), "Id", "Name");
                ViewBag.EnvisageUsage = new SelectList(db.envisagedUsages.OrderBy(x => x.UssageName).ToList(), "Id", "UssageName");
                ViewBag.PropertyType = new SelectList(db.OccupationTypes.OrderBy(x => x.Propertytype).ToList(), "Id", "Propertytype");

                ViewBag.TitleType = new SelectList(db.TitleTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Id", "Name");
                ViewBag.TypeOfTransfer = new SelectList(db.TransferTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                ViewBag.JuristicType = new SelectList(db.EntityTypes.Where(x => (bool)x.IsActive != false), "id", "Name");
                ViewBag.IdentificationType = new SelectList(db.IdentificationTypes.Where(x => x.Key == IdentificationTypeKey.SouthAfricanID), "id", "Name");
                ViewBag.MaritialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_MaritialStatus), "Name", "Name");
                ViewBag.TypeofProperty = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.TypeofProperty), "Id", "Name");
                ViewBag.Relationship = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Relationship), "Id", "Name");
                ViewBag.ResidentialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_ResidentialStatus), "Name", "Name");
                ViewBag.Gender = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_Gender), "Id", "Name");
                ViewBag.Users = new SelectList(Users.OrderBy(x => x.FirstName).ToList(), "Id", "Data");
                ViewBag.UnitCategory = new SelectList(db.HSUnitCategories.OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.UnitTypologies = new SelectList(db.HSUnitTypologies.OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.EHCMonths = Convert.ToInt16(db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EHCFirstStayInMonths).Value);
                //ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Key == PurchaserTypeKeys.NaturalPerson), "Id", "Name");

                return View(model);
            }
            return View("_Error");
        }
        [HttpPost]
        public ActionResult CaptureMasterUpdateAgreement(TenantViewModel model, bool NominateSpouse, string ResAddress, string ResPostal, string ResSuburb, bool SecondApplicantBool, int HumanSettlementApplicationId, string Data, string RentalAmount, string DepositeAmount)
        {
            Initialise();
            using (var core = new eServicesDbContext())
            {
                try
                {
                    var PLA = core.HumanSettlementApplications.Where(x => x.IsDeleted == false && x.Id == HumanSettlementApplicationId).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.PreferredComplexArea).Include(r => r.PreferredComplexArea2)
                      .Include(r => r.TitleType).Include(r => r.SecAppTitleType)
                      .Include(r => r.PurchaserType).Include(r => r.Status).FirstOrDefault();
                    bool ApplicationType = PLA.IsMaster;
                    foreach (var prop in model.HumanSettlementApplication.GetType().GetProperties())
                    {
                        if (prop.GetValue(model.HumanSettlementApplication, null) != null)
                        {
                            if (prop.Name != "Id" && prop.Name != "Data" && prop.Name != "FullName")
                                PLA.GetType().GetProperty(prop.Name).SetValue(PLA, prop.GetValue(model.HumanSettlementApplication));
                        }
                    }

                    PLA.Id = HumanSettlementApplicationId;
                    PLA.MaritalStatus = null;
                    PLA.IsMaster = true;
                    PLA.ResSuburb = ResSuburb;
                    PLA.ResPostal = ResPostal;
                    PLA.ResAddress = ResAddress;
                    PLA.DepartmentId = core.ApplicationEntities.FirstOrDefault(r => r.Key == ApplicationEntityKeys.HumanSettlmentDevelopment).Id;
                    PLA.PurchaserTypeId = core.PurchaserType.FirstOrDefault(r => r.Key == PurchaserTypeKeys.NaturalPerson).Id;
                    PLA.CustomerId = Customer.Id;
                    PLA.SystemUserId = SystemUser.Id;
                    PLA.IdentificationTypeId = core.IdentificationTypes.FirstOrDefault(r => r.Key == IdentificationTypeKey.SouthAfricanID).Id;
                    PLA.HomeNo = model.HumanSettlementApplication.CellNo;
                    PLA.WorkNo = model.HumanSettlementApplication.CellNo;
                    PLA.PreferredComplexArea2Id = model.HumanSettlementApplication.PreferredComplexAreaId;
                    PLA.StatusId = core.Status.FirstOrDefault(o => o.Key == StatusKeys.AwaitingLeaseAgreement).Id;
                    PLA.NominateSpouse = NominateSpouse;
                    PLA.SecondApplicant = SecondApplicantBool;
                    PLA.ApplicantFullName = model.HumanSettlementApplication.FirstName + " " + model.HumanSettlementApplication.LastName;
                    
                    var Master = core.HumanSettlementLeaseMasters.Where(x => x.IsDeleted == false && x.HumanSettlementApplicationId == PLA.Id).Include(r => r.CreatedBySystemUser)
                            .Include(r => r.ModifiedBySystemUser).Include(r => r.Status).FirstOrDefault() ?? null;
                    if (ApplicationType)
                    {
                        
                        foreach (var prop in model.HumanSettlementLeaseMaster.GetType().GetProperties())
                        {
                            if (prop.GetValue(model.HumanSettlementLeaseMaster, null) != null)
                            {
                                if (prop.Name != "Id" && prop.Name != "Data" && prop.Name != "FullName")
                                    Master.GetType().GetProperty(prop.Name).SetValue(Master, prop.GetValue(model.HumanSettlementLeaseMaster));
                            }
                        }

                        int masterId = Master.Id;
                        Master.Id = masterId;
                        var str = model.HumanSettlementLeaseMaster.StoreRooms == 0 || model.HumanSettlementLeaseMaster.StoreRooms == null ? Master.STR = false : Master.STR = true;
                        var spp = model.HumanSettlementLeaseMaster.ShadePortParking == 0 || model.HumanSettlementLeaseMaster.ShadePortParking == null ? Master.SPP = false : Master.SPP = true;
                        var opp = model.HumanSettlementLeaseMaster.OpenParking == 0 || model.HumanSettlementLeaseMaster.OpenParking == null ? Master.OPP = false : Master.OPP = true;
                        var elc = model.HumanSettlementLeaseMaster.Electricity == 0 || model.HumanSettlementLeaseMaster.Electricity == null ? Master.ELEC = false : Master.ELEC = true;
                        var wtr = model.HumanSettlementLeaseMaster.Water == 0 || model.HumanSettlementLeaseMaster.SecurityFee == null ? Master.WTR = false : Master.WTR = true;
                        var sec = model.HumanSettlementLeaseMaster.SecurityFee == 0 || model.HumanSettlementLeaseMaster.SecurityFee == null ? Master.SEC = false : Master.SEC = true;
                        Master.SystemUserId = SystemUser.Id;
                        Master.MasterByUserId = SystemUser.Id;
                        Master.StartDate = model.HumanSettlementLeaseMaster.StartDate;
                        Master.EndDate = model.HumanSettlementLeaseMaster.EndDate;
                        Master.RenewalNotice = model.HumanSettlementLeaseMaster.RenewalNotice;
                        Master.TerminationNotice = model.HumanSettlementLeaseMaster.TerminationNotice;
                        Master.Email = model.HumanSettlementLeaseMaster.Email;
                        Master.SMS = model.HumanSettlementLeaseMaster.SMS;
                        Master.Postal = model.HumanSettlementLeaseMaster.Postal;
                        Master.StatusId = core.Status.FirstOrDefault(r => r.Key == StatusKeys.ActiveLease).Id;
                        Master.IsNew = false;
                        Master.DetailsUpdated = false;
                        Master.HumanSettlementApplicationId = HumanSettlementApplicationId;

                        RentalAmount = RentalAmount.Replace(".", ",");
                        decimal d = Math.Round(decimal.Parse(RentalAmount), 2);
                        double rr = double.Parse(RentalAmount);

                        Master.TotalMonthlyCharges = Convert.ToDouble(RentalAmount);
                        Master.VATAmount = decimal.Parse((rr * 0.15).ToString());
                        Master.RentalAmount = Math.Round(decimal.Parse(RentalAmount), 2);
                        Master.DepositHeld = double.Parse(DepositeAmount);
                        Master.DepositeAmount = Math.Round(decimal.Parse(DepositeAmount), 2);

                        Master.LeasedAddress = PLA.ResAddress;
                        Master.IsActive = true;
                        Master.LeasedPostal = PLA.ResPostal;
                        Master.LeasedSuburb = PLA.ResSuburb;
                        Master.IDNo = PLA.IDNo;
                        Master.FirstNames = PLA.FirstName;
                        Master.LastName = PLA.LastName;
                        Master.DepartmentId = core.ApplicationEntities.FirstOrDefault(r => r.Key == ApplicationEntityKeys.HumanSettlmentDevelopment).Id;
                        core.Entry(Master).State = EntityState.Modified;
                        core.SaveChanges();
                    }
                    else if (!ApplicationType)
                    {

                        PLA.IsMaster = ApplicationType;
                        core.Entry(PLA).State = EntityState.Modified;
                        core.SaveChanges();

                        var agreement = core.HumanSettlementLeaseDetails.FirstOrDefault(a => a.HumanSettlementApplicationId == PLA.Id) ?? null;
                        var DepartmentKey = core.ApplicationEntities.FirstOrDefault(r => r.Key == ApplicationEntityKeys.HumanSettlmentDevelopment);
                        if (agreement != null)
                        {
                            foreach (var prop in model.HumanSettlementLeaseMaster.GetType().GetProperties())
                            {
                                if (prop.GetValue(model.HumanSettlementLeaseMaster, null) != null)
                                {
                                    if (prop.Name != "Id" && prop.Name != "Data" && prop.Name != "FullName" && prop.Name != "MasterByUserId")
                                        agreement.GetType().GetProperty(prop.Name).SetValue(Master, prop.GetValue(model.HumanSettlementLeaseMaster));
                                }
                            }
                            var opp = model.HumanSettlementLeaseMaster.OpenParking == 0 || model.HumanSettlementLeaseMaster.OpenParking == null ? agreement.OPP = false : agreement.OPP = true;
                            var elc = model.HumanSettlementLeaseMaster.Electricity == 0 || model.HumanSettlementLeaseMaster.Electricity == null ? agreement.ELEC = false : agreement.ELEC = true;
                            var wtr = model.HumanSettlementLeaseMaster.Water == 0 || model.HumanSettlementLeaseMaster.SecurityFee == null ? agreement.WTR = false : agreement.WTR = true;
                            var sec = model.HumanSettlementLeaseMaster.SecurityFee == 0 || model.HumanSettlementLeaseMaster.SecurityFee == null ? agreement.SEC = false : agreement.SEC = true;
                            var str = model.HumanSettlementLeaseMaster.StoreRooms == 0 || model.HumanSettlementLeaseMaster.StoreRooms == null ? agreement.STR = false : agreement.STR = true;
                            var spp = model.HumanSettlementLeaseMaster.ShadePortParking == 0 || model.HumanSettlementLeaseMaster.ShadePortParking == null ? agreement.SPP = false : agreement.SPP = true;
                            agreement.SystemUserId = PLA.SystemUserId;
                            agreement.StartDate = model.HumanSettlementLeaseMaster.StartDate;
                            agreement.EndDate = model.HumanSettlementLeaseMaster.EndDate;
                            agreement.RenewalNotice = model.HumanSettlementLeaseMaster.RenewalNotice;
                            agreement.TerminationNotice = model.HumanSettlementLeaseMaster.TerminationNotice;
                            agreement.Email = model.HumanSettlementLeaseMaster.Email;
                            agreement.SMS = model.HumanSettlementLeaseMaster.SMS;
                            agreement.Postal = model.HumanSettlementLeaseMaster.Postal;
                            agreement.StatusId = core.Status.FirstOrDefault(r => r.Key == StatusKeys.ActiveLease).Id;
                            agreement.IsNew = false;
                            agreement.DetailsUpdated = false;
                            agreement.HumanSettlementApplicationId = HumanSettlementApplicationId;

                            RentalAmount = RentalAmount.Replace(".", ",");
                            decimal d = Math.Round(decimal.Parse(RentalAmount), 2);
                            double rr = double.Parse(RentalAmount);

                            agreement.TotalMonthlyCharges = Convert.ToDouble(RentalAmount);
                            agreement.VATAmount = decimal.Parse((rr * 0.15).ToString());
                            agreement.RentalAmount = Math.Round(decimal.Parse(RentalAmount), 2);
                            agreement.DepositHeld = double.Parse(DepositeAmount);
                            agreement.DepositeAmount = Math.Round(decimal.Parse(DepositeAmount), 2);

                            agreement.LeasedAddress = PLA.ResAddress;
                            agreement.LeasedPostal = PLA.ResPostal;
                            agreement.LeasedSuburb = PLA.ResSuburb;
                            agreement.IDNo = PLA.IDNo;
                            agreement.FirstNames = PLA.FirstName;
                            agreement.LastName = PLA.LastName;
                            agreement.DepartmentId = DepartmentKey.Id;
                            core.Entry(agreement).State = EntityState.Modified;
                            core.SaveChanges();
                        }
                        else
                        {
                            agreement = new HumanSettlementLeaseDetails();
                            agreement.SystemUserId = PLA.SystemUserId;
                            agreement.StartDate = Convert.ToDateTime(model.HumanSettlementLeaseMaster.StartDate);
                            agreement.EndDate = Convert.ToDateTime(model.HumanSettlementLeaseMaster.EndDate);
                            agreement.RenewalNotice = Convert.ToDateTime(model.HumanSettlementLeaseMaster.RenewalNotice);
                            agreement.TerminationNotice = Convert.ToDateTime(model.HumanSettlementLeaseMaster.TerminationNotice);
                            agreement.Email = model.HumanSettlementLeaseMaster.Email;
                            agreement.SMS = model.HumanSettlementLeaseMaster.SMS;
                            agreement.Postal = model.HumanSettlementLeaseMaster.Postal;
                            agreement.StatusId = core.Status.FirstOrDefault(r => r.Key == StatusKeys.ActiveLease).Id;
                            agreement.IsNew = false;
                            agreement.DetailsUpdated = false;
                            agreement.HumanSettlementApplicationId = HumanSettlementApplicationId;

                            RentalAmount = RentalAmount.Replace(".", ",");
                            decimal d = Math.Round(decimal.Parse(RentalAmount), 2);
                            double rr = double.Parse(RentalAmount);

                            agreement.TotalMonthlyCharges = Convert.ToDouble(RentalAmount);
                            agreement.VATAmount = decimal.Parse((rr * 0.15).ToString());
                            agreement.RentalAmount = Math.Round(decimal.Parse(RentalAmount), 2);
                            agreement.DepositHeld = double.Parse(DepositeAmount);
                            agreement.DepositeAmount = Math.Round(decimal.Parse(DepositeAmount), 2);

                            agreement.LeasedAddress = PLA.ResAddress;
                            agreement.LeasedPostal = PLA.ResPostal;
                            agreement.LeasedSuburb = PLA.ResSuburb;
                            agreement.IDNo = PLA.IDNo;
                            agreement.FirstNames = PLA.FirstName;
                            agreement.LastName = PLA.LastName;
                            agreement.DepartmentId = DepartmentKey.Id;
                            var opp = model.HumanSettlementLeaseMaster.OpenParking == 0 || model.HumanSettlementLeaseMaster.OpenParking == null ? agreement.OPP = false : agreement.OPP = true;
                            var elc = model.HumanSettlementLeaseMaster.Electricity == 0 || model.HumanSettlementLeaseMaster.Electricity == null ? agreement.ELEC = false : agreement.ELEC = true;
                            var wtr = model.HumanSettlementLeaseMaster.Water == 0 || model.HumanSettlementLeaseMaster.SecurityFee == null ? agreement.WTR = false : agreement.WTR = true;
                            var sec = model.HumanSettlementLeaseMaster.SecurityFee == 0 || model.HumanSettlementLeaseMaster.SecurityFee == null ? agreement.SEC = false : agreement.SEC = true;
                            var str = model.HumanSettlementLeaseMaster.StoreRooms == 0 || model.HumanSettlementLeaseMaster.StoreRooms == null ? agreement.STR = false : agreement.STR = true;
                            var spp = model.HumanSettlementLeaseMaster.ShadePortParking == 0 || model.HumanSettlementLeaseMaster.ShadePortParking == null ? agreement.SPP = false : agreement.SPP = true;
                         

                            core.HumanSettlementLeaseDetails.Add(agreement);
                            core.SaveChanges();
                        }
                    }

                    core.Entry(PLA).State = EntityState.Modified;
                    core.SaveChanges();

                    var ResponsibilityTypeId = core.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.UpdateAgreement).Id;
                    //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, PLA.Id, ResponsibilityTypeId, Customer.Id);
                    if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                    {
                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, PLA.Id, ResponsibilityTypeId, Customer.Id, true);
                    }
                    else
                    {
                        WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(core, PLA.Id, ResponsibilityTypeId, Customer.Id);
                    }
                    WorkAllocationHumanHelper.AgreementOfLease((int)PLA.Id, true, false, false, false, false, false);

                    return RedirectToAction("HumanSettlementAgreements", "HumanSettlementApplication", new { q = Data });
                }
                catch (Exception Io)
                {
                    return View("_Error");
                    //throw;
                }
            }
        }




        public ActionResult CaptureMaster(int? id)
        {
            Initialise();
            if (Request.IsAuthenticated)
            {
                TenantViewModel model = new TenantViewModel
                {
                    HumanSettlementApplication = new HumanSettlementApplication(),
                    HumanSettlementLeaseDetails = new HumanSettlementLeaseDetails(),
                    HumanSettlementLeaseMaster = new HumanSettlementLeaseMaster()
                };

                var Users = db.SystemUsers.ToList();
                foreach (var rr in Users)
                    rr.Data = rr.FirstName + " " + rr.LastName;

                var userWorkAllocation = new List<int?>();
                if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                {
                    userWorkAllocation = db.UserWorkAllocations.Where(a => a.RRActive).Select(d => d.PreferredComplexAreaId).Distinct().ToList();
                }
                else 
                {
                    userWorkAllocation = db.UserWorkAllocations.Where(a => a.SystemUserId == SystemUser.Id && a.RRActive).Select(d => d.PreferredComplexAreaId).ToList();
                }

                var rcsType = db.RCSTypes.OrderBy(x => x.Name);
                ViewBag.IncomeBrackets = new SelectList(db.HSIncomeBrackets.OrderBy(x => x.Id).Where(r => r.Key != IncomeBracketsKeys.Other).ToList(), "Id", "Name");
                ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Key == PurchaserTypeKeys.NaturalPerson), "Id", "Name");
                ViewBag.Option = new SelectList(db.IncomeSources.OrderBy(x => x.SourceOfIncome).ToList(), "Id", "SourceOfIncome");
                ViewBag.UnitType = new SelectList(db.OccupationTypes.OrderBy(x => x.Propertytype), "Id", "Propertytype");
                ViewBag.PreferredArea = new SelectList(db.PreferredComplexAreas.OrderBy(x => x.Name).Where(x => x.IsActive && x.Key.Contains("h_") && userWorkAllocation.Contains(x.Id)).ToList(), "Id", "Name");
                ViewBag.CompanyType = new SelectList(db.companyTypes.OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.HumanOptions = new SelectList(db.humanEHCOptions.OrderBy(x => x.Name).Where(x => x.Key != "hs_single_flats" || x.Key != "hs_single_flats").ToList(), "Id", "Name");
                ViewBag.EnvisageUsage = new SelectList(db.envisagedUsages.OrderBy(x => x.UssageName).ToList(), "Id", "UssageName");
                ViewBag.PropertyType = new SelectList(db.OccupationTypes.OrderBy(x => x.Propertytype).ToList(), "Id", "Propertytype");

                ViewBag.TitleType = new SelectList(db.TitleTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Id", "Name");
                ViewBag.TypeOfTransfer = new SelectList(db.TransferTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                ViewBag.JuristicType = new SelectList(db.EntityTypes.Where(x => (bool)x.IsActive != false), "id", "Name");
                ViewBag.IdentificationType = new SelectList(db.IdentificationTypes.Where(x => x.Key == IdentificationTypeKey.SouthAfricanID), "id", "Name");
                ViewBag.MaritialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_MaritialStatus), "Name", "Name");
                ViewBag.TypeofProperty = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.TypeofProperty), "Id", "Name");
                ViewBag.Relationship = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Relationship), "Id", "Name");
                ViewBag.ResidentialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_ResidentialStatus), "Name", "Name");
                ViewBag.Gender = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_Gender), "Id", "Name");
                ViewBag.Users = new SelectList(Users.OrderBy(x => x.FirstName).ToList(), "Id", "Data");
                ViewBag.UnitCategory = new SelectList(db.HSUnitCategories.OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.UnitTypologies = new SelectList(db.HSUnitTypologies.OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.EHCMonths = Convert.ToInt16(db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EHCFirstStayInMonths).Value);
                ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Key == PurchaserTypeKeys.NaturalPerson), "Id", "Name");

                return View(model);
            }
            return View("_Error");
        }

        static bool CheckIfValueExist(string x, string y, string t) => (!String.IsNullOrEmpty(x) && !String.IsNullOrEmpty(y) && !String.IsNullOrEmpty(t)) ? true : false;

        public ActionResult UnitPayableRentalAmount(int CategotyId, int TypologyId, int IncomeBracksId)
        {
            decimal PayableRental = 0;
            decimal Deposit = 0;
            var Recs = (EscalationMaster)null;
            using (var core = new eServicesDbContext())
            {
                var Category = core.HSUnitCategories.FirstOrDefault(r => r.Id == CategotyId);
                var Typology = core.HSUnitTypologies.FirstOrDefault(r => r.Id == TypologyId);
                var IncomeBraks = core.HSIncomeBrackets.FirstOrDefault(r => r.Id == IncomeBracksId);

                var result = CheckIfValueExist(Category?.Key, Typology?.Key, IncomeBraks?.Key);
                if (result)
                {
                    if (Category.Key.Contains("retirement_units") || Category.Key.Contains("hostel") || Category.Key.Contains("dormitory"))
                    {
                        Recs = core.EscalationMaster.Include(d => d.HSUnitTypology).Include(d => d.HSUnitCategory).Include(d => d.HSIncomeBracket).Include(d => d.Slot1EscalationYears)
                            .Include(d => d.Slot2EscalationYears).Include(d => d.Slot3EscalationYears).Include(d => d.Slot4EscalationYears).Include(d => d.Slot5EscalationYears)
                            .Include(d => d.Slot6EscalationYears).Where(r => r.HSUnitTypologyId == Typology.Id && r.HSUnitCategoryId == Category.Id).FirstOrDefault();
                    }
                    else
                    {
                        Recs = core.EscalationMaster.Include(d => d.HSUnitTypology).Include(d => d.HSUnitCategory).Include(d => d.HSIncomeBracket).Include(d => d.Slot1EscalationYears)
                            .Include(d => d.Slot2EscalationYears).Include(d => d.Slot3EscalationYears).Include(d => d.Slot4EscalationYears).Include(d => d.Slot5EscalationYears)
                            .Include(d => d.Slot6EscalationYears).Where(r => r.HSUnitCategoryId == Category.Id && r.HSUnitTypologyId == Typology.Id && r.HSIncomeBracketId == IncomeBraks.Id).FirstOrDefault();
                    }
                    var year = core.HSEscalationYears.FirstOrDefault(r => r.Id == Recs.HSEscalationYearsId);
                    if (year?.Key == Recs.Slot1EscalationYears.Key)
                    {
                        PayableRental = Recs.Slot1Price;
                        Deposit = Recs.Slot1Price;
                    }
                    else if (year?.Key == Recs.Slot2EscalationYears.Key)
                    {
                        PayableRental = Recs.Slot2Price;
                        Deposit = Recs.Slot2Price;
                    }
                    else if (year?.Key == Recs.Slot3EscalationYears.Key)
                    {
                        PayableRental = Recs.Slot3Price;
                        Deposit = Recs.Slot3Price;
                    }
                    else if (year?.Key == Recs.Slot4EscalationYears.Key)
                    {
                        PayableRental = Recs.Slot4Price;
                        Deposit = Recs.Slot4Price;
                    }
                    else if (year?.Key == Recs.Slot5EscalationYears.Key)
                    {
                        PayableRental = Recs.Slot5Price;
                        Deposit = Recs.Slot5Price;
                    }
                    else if (year?.Key == Recs.Slot6EscalationYears.Key)
                    {
                        PayableRental = Recs.Slot6Price;
                        Deposit = Recs.Slot6Price;
                    }
                }

                var obj = new
                {
                    status = result && Recs != null ? "Success" : "Failure",
                    applicationrenewable = (Category.Key.Contains("retirement_units")) ? true : false,
                    payable = string.Format("{0}", PayableRental),
                    deposit = string.Format("{0}", Deposit)
                };

                return Json(obj);
            }
        }
        public ActionResult CaptureUpdateDropOptions(int CategotyId)
        {
            var core = new eServicesDbContext();
            var escalatetions = core.EscalationMaster.ToList();
            var rrq = escalatetions.Where(a => a.HSUnitCategoryId == CategotyId).Select(d => d.HSUnitTypologyId).ToList();
            var Typologies = core.HSUnitTypologies.Where(a => rrq.Contains(a.Id)).ToList();
            var retrn = Typologies.Select(a => new { a.Id, a.Name }).Distinct();
            return Json(retrn);
        }


        [HttpPost]
        public ActionResult CaptureMaster(TenantViewModel model, bool NominateSpouse, string ResAddress, string ResPostal, string ResSuburb, bool SecondApplicantBool, string RentalAmount, string DepositeAmount)
        {
            Initialise();
            using (var core = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(core);
                    var AppSettings = core.AppSettings;
                    var query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.hsd_daily_sequence_counter);
                    var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.hsd_daily_sequence_limiter);

                    var BatchCounter = query.Value;
                    int limiter = Convert.ToInt16(SeqLimit.Value);
                    if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                    {
                        var lastRef = query.Value;
                        BatchCounter = lastRef.ToString();
                        int nextSeq = Convert.ToInt16(query.Value) + 1;
                        string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                        query.Value = nextVal;
                        core.Entry(query).State = EntityState.Modified;
                        core.SaveChanges();
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
                        core.Entry(query).State = EntityState.Modified;
                        core.SaveChanges();
                    }
                    var Master = model.HumanSettlementLeaseMaster;
                    var PLA = model.HumanSettlementApplication;
                    var RefNum = "HSD" + DateTime.Now.ToString("yyyyMMdd") + BatchCounter;
                    PLA.ApplicationReferenceNumber = RefNum;
                    PLA.IsMaster = true;
                    PLA.ResSuburb = ResSuburb;
                    PLA.ResPostal = ResPostal;
                    PLA.ResAddress = ResAddress;
                    PLA.DepartmentId = core.ApplicationEntities.FirstOrDefault(r => r.Key == ApplicationEntityKeys.HumanSettlmentDevelopment).Id;
                    PLA.PurchaserTypeId = core.PurchaserType.FirstOrDefault(r => r.Key == PurchaserTypeKeys.NaturalPerson).Id;
                    PLA.CustomerId = Customer.Id;
                    PLA.SystemUserId = SystemUser.Id;
                    PLA.IdentificationTypeId = core.IdentificationTypes.FirstOrDefault(r => r.Key == IdentificationTypeKey.SouthAfricanID).Id;
                    PLA.HomeNo = PLA.CellNo;
                    PLA.WorkNo = PLA.CellNo;
                    PLA.PreferredComplexArea2Id = PLA.PreferredComplexAreaId;
                    PLA.StatusId = core.Status.FirstOrDefault(o => o.Key == StatusKeys.ApplicationCompleted).Id;
                    PLA.NominateSpouse = NominateSpouse;
                    PLA.SecondApplicant = SecondApplicantBool;
                    PLA.ApplicantFullName = PLA.FirstName + " " + PLA.LastName;
                    core.HumanSettlementApplications.Add(PLA);
                    core.SaveChanges();


                    var str = Master.StoreRooms == 0 || Master.StoreRooms == null ? Master.STR = false : Master.STR = true;
                    var spp = Master.ShadePortParking == 0 || Master.ShadePortParking == null ? Master.SPP = false : Master.SPP = true;
                    var opp = Master.OpenParking == 0 || Master.OpenParking == null ? Master.OPP = false : Master.OPP = true;
                    var elc = Master.Electricity == 0 || Master.Electricity == null ? Master.ELEC = false : Master.ELEC = true;
                    var wtr = Master.Water == 0 || Master.SecurityFee == null ? Master.WTR = false : Master.WTR = true;
                    var sec = Master.SecurityFee == 0 || Master.SecurityFee == null ? Master.SEC = false : Master.SEC = true;
                    Master.SystemUserId = PLA.SystemUserId;
                    Master.MasterByUserId = SystemUser.Id;
                    Master.StartDate = Master.StartDate;
                    Master.EndDate = Master.EndDate;
                    Master.RenewalNotice = Master.RenewalNotice;
                    Master.TerminationNotice = Master.TerminationNotice;
                    Master.Email = Master.Email;
                    Master.SMS = Master.SMS;
                    Master.Postal = Master.Postal;
                    Master.StatusId = core.Status.FirstOrDefault(r => r.Key == StatusKeys.ActiveLease).Id;
                    Master.IsNew = false;
                    Master.DetailsUpdated = false;
                    Master.HumanSettlementApplicationId = PLA.Id;

                    RentalAmount = /*RentalAmount.Replace(",", "")*/RentalAmount.Replace(".", ",");
                    decimal d = Math.Round(decimal.Parse(RentalAmount), 2);
                    double rr = double.Parse(RentalAmount);

                    DepositeAmount = /*RentalAmount.Replace(",", "")*/DepositeAmount.Replace(".", ",");

                    Master.TotalMonthlyCharges = Convert.ToDouble(RentalAmount);
                    Master.VATAmount = decimal.Parse((rr * 0.15).ToString());
                    Master.RentalAmount = Math.Round(decimal.Parse(RentalAmount), 2);
                    Master.DepositHeld = double.Parse(DepositeAmount);
                    Master.DepositeAmount = Math.Round(decimal.Parse(DepositeAmount), 2);
                    Master.LeasedAddress = PLA.ResAddress;
                    Master.LeasedPostal = PLA.ResPostal;
                    Master.LeasedSuburb = PLA.ResSuburb;
                    Master.IDNo = PLA.IDNo;
                    Master.FirstNames = PLA.FirstName;
                    Master.LastName = PLA.LastName;
                    Master.DepartmentId = core.ApplicationEntities.FirstOrDefault(r => r.Key == ApplicationEntityKeys.HumanSettlmentDevelopment).Id;
                    core.HumanSettlementLeaseMasters.Add(Master);
                    core.SaveChanges();


                    Session["Display"] = "Display";
                    Session["ApplicationRefNo"] = PLA.ApplicationReferenceNumber;
                    Session["MessageTitle"] = "Successfully Captured";
                    Session["MessageBody"] = string.Format("Master Lease has been captured successfully, the reference no {0}, it is currently {1}", PLA.ApplicationReferenceNumber, core.Status.FirstOrDefault(o => o.Key == StatusKeys.ApplicationCompleted).Name);

                    if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                    {
                        var Activity = core.ActivityTrackerMessages.FirstOrDefault(a => a.Key == ActivityTrackerMessageKeys.AdminLeaseCapture);
                        Activity.Description += " of application " + PLA.ApplicationReferenceNumber;
                        MatchingHelper.ActivityTrackerHuman(core, PLA.Id, Activity.Description, _base.Customer.Id);
                    }
                    var data = (PLA.IsMaster) ? "Master=" + true + "&All=" + false + "&New=" + false : "Master=" + false + "&All=" + false + "&New=" + true;
                    var Data = new AesCrypto().Encrypt(data);

                    return RedirectToAction("HumanSettlementApplications", new { q = Data });
                }
                catch (Exception Io)
                {

                    throw;
                }

                return View();
            }
        }

        [Authorize]
        [DecryptParameter]
        public ActionResult CaptureUniTransfer(string Data, int ApplicationId)
        {
            try
            {
                Initialise();
                var context = new eServicesDbContext();
                var rcsApps = context.HumanSettlementApplications.Include(r => r.CreatedBySystemUser)
                  .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                  .Include(r => r.Status).Include(r => r.PurchaserType)
                  .Where(x => x.Id == ApplicationId).FirstOrDefault();

                var vm = new DepartmentsApprovalViewModel
                {
                    HumanSettlementApplication = rcsApps
                };

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.PlmYes || x.Key == RCSActionTypeKeys.PlmNo).OrderByDescending(x => x.Name), "Key", "Name");

                return View(vm);
            }
            catch (Exception)
            {

                throw;
            }
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult CaptureUniTransfer(int ApplicationId, string ReasonForTransfer, string LocationKey)
        {
            return View();
        }



        [Authorize(Roles = "Customers")]
        public ActionResult Capture()
        {
            Initialise(); 
            var rcsType = db.RCSTypes.OrderBy(x => x.Name);
            var actionType = db.RCSActionTypes.OrderBy(x => x.Name);
            string id = "";
            if (Customer == null) return RedirectToAction("Dashboard", "Profile");

            var customer = db.Customers.Include(s => s.SystemUser).FirstOrDefault(c => c.Id == Customer.Id);
            if (customer == null) throw new Exception("Invalid Customer");

            var status = db.Status.FirstOrDefault(s => s.Key.Equals(StatusKeys.CustomerPendingApproval));
            if (status == null) throw new Exception(string.Format("Invalid/ missing status key {0}", StatusKeys.CustomerActive));

            object obj = new { customerId = Customer.Id, agentId = Agent.Id };
            if (customer.StatusId == status.Id) return RedirectToAction("Index3", "Profile", SecureActionLinkExtension.Encrypt(obj));

            ViewBag.Name = customer.FirstName;
            ViewBag.LastName = customer.LastName;
            ViewBag.Genderr = customer.Gender != null ? new SelectList(rcsType.Where(x => x.Name == customer.Gender), "Name", "Name") : new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_Gender), "Name", "Name");
            ViewBag.TitleTyperr = new SelectList(db.TitleTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Id == customer.TitleTypeId), "Id", "Name");
            ViewBag.IdentificationNumber = customer.IdentificationNumber;
            ViewBag.ContactNumbers = customer.CellPhoneNumber;
            ViewBag.EmailAddress = customer.EmailAddress;
            ViewBag.HomeNumber = customer.HomePhoneNumber;
            ViewBag.WorkNumber = customer.WorkPhoneNumber;
            ViewBag.Suburb = customer.PhysicalAddress5;
            ViewBag.PostalCode = customer.PhysicalAddressCode;
            ViewBag.Address = customer.PostalAddress1 + " " + customer.PostalAddress2 + ", " + customer.PostalAddress3 + ", " + customer.PostalAddress4 + " ";
            id = customer.IdentificationNumber;

            ViewBag.IncomeBrackets = new SelectList(db.HSIncomeBrackets.OrderBy(x => x.Id).Where(r => r.Key != IncomeBracketsKeys.Other).ToList(), "Id", "Name");
            ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Key == PurchaserTypeKeys.NaturalPerson), "Id", "Name");
            ViewBag.Option = new SelectList(db.IncomeSources.OrderBy(x => x.SourceOfIncome).ToList(), "Id", "SourceOfIncome");
            ViewBag.UnitType = new SelectList(db.OccupationTypes.OrderBy(x => x.Propertytype), "Id", "Propertytype");
            ViewBag.PreferredArea = new SelectList(db.PreferredComplexAreas.OrderBy(x => x.Name).Where(x => x.IsActive && x.Key.Contains("h_")).ToList(), "Id", "Name");
            ViewBag.CompanyType = new SelectList(db.companyTypes.OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.HumanOptions = new SelectList(db.humanEHCOptions.OrderBy(x => x.Name).Where(x => x.Key != "hs_single_flats" || x.Key != "hs_single_flats").ToList(), "Id", "Name");
            ViewBag.EnvisageUsage = new SelectList(db.envisagedUsages.OrderBy(x => x.UssageName).ToList(), "Id", "UssageName");
            ViewBag.PropertyType = new SelectList(db.OccupationTypes.OrderBy(x => x.Propertytype).ToList(), "Id", "Propertytype");

            ViewBag.TitleType = new SelectList(db.TitleTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Id", "Name");
            ViewBag.TypeOfTransfer = new SelectList(db.TransferTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
            ViewBag.JuristicType = new SelectList(db.EntityTypes.Where(x => (bool)x.IsActive != false), "id", "Name");
            ViewBag.IdentificationType = new SelectList(db.IdentificationTypes.Where(x => x.Key == IdentificationTypeKey.SouthAfricanID), "id", "Name");
            ViewBag.MaritialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_MaritialStatus), "Name", "Name");
            ViewBag.TypeofProperty = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.TypeofProperty), "Id", "Name");
            ViewBag.MotivateNeed = new SelectList(actionType.Where(x => x.Key == RCSActionTypeKeys.PlmYes || x.Key == RCSActionTypeKeys.PlmNo), "Id", "Name");
            ViewBag.Relationship = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Relationship), "Id", "Name");
            ViewBag.ResidentialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_ResidentialStatus), "Name", "Name");
            ViewBag.Gender = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_Gender), "Id", "Name");
            ViewBag.UnitCategory = new SelectList(db.HSUnitCategories.OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.UnitTypologies = new SelectList(db.HSUnitTypologies.OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.EHCMonths = Convert.ToInt16(db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EHCFirstStayInMonths).Value);
            ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Key == PurchaserTypeKeys.NaturalPerson), "Id", "Name");
            ViewBag.CaptureInstructions = "Capture Applicant Information </br> Upload Documents";


            ApplicationDepartment applicationDepartment = new ApplicationDepartment();
            return View();
        }

        #endregion

        #region Capture
        [Authorize(Roles = "Customers")]
        [HttpPost]
        public ActionResult Capture(CaptureViewModel capture, string TransferInformationSellingPrice, bool? ApplicantJointId, bool SecondApplicantBool)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);
                    var AppSettings = cxt.AppSettings;
                    var Customers = cxt.Customers;
                    var Statuses = cxt.Status;
                    HumanSettlementApplication PLA = new HumanSettlementApplication();
                    PLA = capture.HumanSettlementApplication;
                    var result = SecondApplicantBool == true ? PLA.SecondApplicant = true : PLA.SecondApplicant = false;

                    var query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.hsd_daily_sequence_counter);
                    var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.hsd_daily_sequence_limiter);

                    var BatchCounter = query.Value;
                    int limiter = Convert.ToInt16(SeqLimit.Value);
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
                    var Key = Statuses.FirstOrDefault(o => o.Key == StatusKeys.ReuploadApplicationDocs);
                    var RefNum = string.Format("HSD{0}{1}", DateTime.Now.ToString("yyyyMMdd"), BatchCounter);

                    PLA.ApplicationReferenceNumber = RefNum;
                    PLA.StatusId = Key.Id;
                    PLA.IsMaster = false;
                    PLA.SystemUserId = SystemUser.Id;
                    PLA.CustomerId = Customer.Id;
                    PLA.ApplicantFullName = string.Format("{0} {1}", PLA.FirstName, PLA.LastName);
                    cxt.HumanSettlementApplications.Add(PLA);
                    cxt.SaveChanges();

                    //Activity Tracker Messages
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationCapture).Description.ToString();
                    ActivityTrackerMessage = string.Format("{0} {1}.", ActivityTrackerMessage, PLA.ApplicantFullName);
                    MatchingHelper.ActivityTrackerHuman(cxt, PLA.Id, ActivityTrackerMessage, Customer.Id);

                    //Send e-mail and SMS notification
                    int emailboodyId = cxt.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationCaptureEmail).Id;
                    EmailHelper.CustomerEmailOrSMSNotification(cxt, PLA.Id, emailboodyId);

                    var referenceType = cxt.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                    var application = cxt.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                    Session["Display"] = "Display";
                    Session["ApplicationRefNo"] = PLA.ApplicationReferenceNumber;
                    Session["MessageTitle"] = "Successfully Captured";
                    Session["MessageBody"] = string.Format("Property lease application has been captured successfully, the reference no {0}, it is currently {1}", PLA.ApplicationReferenceNumber, Key.Name);


                    return RedirectToAction("Index3", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(
                        new
                        {
                            referenceId = Customer.Id,
                            customerId = Customer.Id,
                            referenceTypeId = referenceType.Id,
                            applicationId = application.Id,
                            agentId = application.Id,
                            returnUrl = "",
                            rcsappId = PLA.Id
                        })));



                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    return View("Inbox");
                }
            }
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
                    var applicationProp = db.PropertyLeaseApplications.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Include(r => r.Customer).Include(r => r.SystemUser).Where(x => x.Id == id).FirstOrDefault();
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
                        //PropertyLeaseApplication = applicationProp,
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



        #region All of Inspection Methods should be here

        [Authorize]
        [DecryptParameter]
        public ActionResult HumanApplicationInspection(int ApplicationId, string Data, string ViewName)
        {
            try
            {
                using (var core = new eServicesDbContext())
                {
                    Initialise();
                    var vm = new DepartmentsApprovalViewModel();
                    var dvm = new DocumentsViewModel();
                    var dvmTemplate = new DocumentsViewModel();
                    var applicationProp = core.HumanSettlementApplications
                                .Include(x => x.SystemUser).Include(x => x.Status)
                                .Include(x => x.PurchaserType).Include(r => r.Customer)
                                .Include(r => r.SystemUser).Where(x => x.Id == ApplicationId).FirstOrDefault();

                    var customer = core.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == applicationProp.CustomerId);
                    if (customer == null)
                        throw new Exception("Invalid Customer");

                    var application = core.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    if (application == null)
                        throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));
                    var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                    if (referenceType == null)
                        throw new Exception("Invalid reference type.");
                    var documentReferenceType = core.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSUpload);
                    if (documentReferenceType == null)
                        throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                            ReferenceTypeKeys.RCSUpload));
                    var DVM = new List<DocumentCheckList>();

                    var docdets = core.DocumentTypes.FirstOrDefault(x => x.Key == DocumentTypeKeys.RefundMeterReading);
                    List<Attachments> attachments = new List<Attachments>();
                    var atth = core.Attachments.FirstOrDefault();

                    attachments.Add(atth);
                    vm.Attachments = attachments;
                    vm.DocName = docdets.Name;
                    vm.DocDesc = docdets.Description;

                    vm.HumanSettlementApplication = applicationProp;

                    Session["Inspections"] = "true";
                    switch (ViewName)
                    {
                        case ViewCodeKeys.ScheduleInspectionSlots:
                            #region Pop-Up display
                            if (Session["Display"] != null)
                            {
                                if (Session["ApplicationRefNo"] == null)
                                {
                                    Session["Display"] = "True";

                                    if (Session["Display"].ToString() == "True")
                                    {

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
                            Session["Display"] = null;
                            Session["ApplicationRefNo"] = null;
                            Session["MessageTitle"] = null;
                            #endregion
                            var TimeSlots = core.TimeSlots.Where(r => r.IsActive).ToList();
                            vm.Features = new SelectList(TimeSlots, "Id", "Name");
                            applicationProp.Data = Data;
                            var rr = core.InspectionSchedules.Where(x => x.HumanSettlementApplicationId == ApplicationId && x.IsDeleted).ToList();
                            var rrq = rr.Select(r => r.DateToScheduleId).ToList();
                            vm.InspectionScheduleList = core.InspectionSchedules
                                .Include(r => r.DateToSchedule).Include(r => r.TimeSlot)
                                .Where(x => x.HumanSettlementApplicationId == ApplicationId && !x.IsDeleted && !x.IsInspected).ToList() ?? null;
                            ViewBag.DateToSchedule = "";
                            ViewBag.Id = ApplicationId;
                            ViewBag.PrpId = ApplicationId;
                            ViewBag.UserFName = Customer.FirstName;
                            ViewBag.UserLName = Customer.LastName;
                            ViewBag.UserName = Customer.AttorneyCode;
                            ViewBag.UserEmail = Customer.EmailAddress;
                            return View("ScheduleInspectionSlots", vm);
                        case ViewCodeKeys.PreInspection:
                            MatchingHelper.DocumentConductUnitInspection2(dvm, core, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", applicationProp.Id, true);
                            MatchingHelper.DocumentGetConductUnitInspectionTemplate(dvmTemplate, core, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, false);
                            vm.DocumentsViewModel = dvm;
                            vm.DocumentsViewModelTemplate = dvmTemplate;
                            ViewBag.ApprovalStatus = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Habitable || x.Key == RCSActionTypeKeys.HabitableMinorDefects || x.Key == RCSActionTypeKeys.NotHabitable).OrderBy(x => x.Name), "Key", "Name");
                            return View("ConductUnitInspection", vm);
                        case ViewCodeKeys.MaintananceJobSheet:
                            var conduct = core.conductUnitInspections.OrderByDescending(x => x.Id).Include(r => r.CreatedBySystemUser).FirstOrDefault(x => x.HumanSettlementApplicationId == applicationProp.Id);
                            if (conduct.CreatedBySystemUser == null)
                                conduct.CreatedBySystemUser = core.ConductUnitInspectionAudits.Include(a => a.CreatedBySystemUser).FirstOrDefault(d => d.Id == conduct.Id && d.Action == "Added").CreatedBySystemUser;
                            vm.ConductUnitInspection = conduct;
                            MatchingHelper.DocumentConductMaintanaceJobSheet2(dvm, core, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", applicationProp.Id, true);
                            MatchingHelper.DocumentGetConductMaintananceJobSheetTemplate(dvmTemplate, db, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, false);
                            vm.DocumentsViewModel = dvm;
                            vm.DocumentsViewModelTemplate = dvmTemplate;
                            ViewBag.ApprovalStatus = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");
                            return View("MaintenanceJobSheet", vm);
                        case ViewCodeKeys.PostInspection:
                            var master = core.HumanSettlementLeaseMasters
                                .Include(r => r.HumanSettlementApplication.PurchaserType)
                                .Include(r => r.Status)
                                .Include(r => r.HumanSettlementApplication)
                                .Where(c => c.HumanSettlementApplicationId == applicationProp.Id && c.IsActive).FirstOrDefault();

                            var details = core.HumanSettlementLeaseDetails
                                .Include(r => r.HumanSettlementApplication.PurchaserType)
                                .Include(r => r.Status)
                                .Include(r => r.HumanSettlementApplication)
                                .Where(c => c.HumanSettlementApplicationId == applicationProp.Id && c.IsActive).FirstOrDefault();

                            MatchingHelper.DocumentTenantAccountValidation(dvm, core, applicationProp.CustomerId, applicationProp.CustomerId, (int)referenceType.Id, (int)application.Id, "", applicationProp.Id, true);
                            MatchingHelper.DocumentPostInspection(dvm, core, applicationProp.CustomerId, applicationProp.CustomerId, (int)referenceType.Id, (int)application.Id, "", applicationProp.Id, true);
                            MatchingHelper.DocumentGetPostInspectionTemplate(dvmTemplate, db, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, false);

                            vm.HumanSettlementApplication = master?.HumanSettlementApplication ?? details?.HumanSettlementApplication;
                            vm.HumanSettlementLeaseMaster = master;
                            vm.HumanSettlementLeaseDetails = details;
                            vm.DocumentsViewModel = dvm;
                            vm.DocumentsViewModelTemplate = dvmTemplate;                            
                            ViewBag.ApprovalStatus = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Habitable || x.Key == RCSActionTypeKeys.HabitableMinorDefects || x.Key == RCSActionTypeKeys.NotHabitable).OrderBy(x => x.Name), "Key", "Name");

                            return View("ConductExitInspection", vm);
                    }

                }
                return View("_Error");
            }
            catch (Exception error)
            {
                EventLogHelper.LogSystemError(error.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
            }
           
        }

        [Authorize]
        [HttpPost]
        public ActionResult ScheduleInspectionSlots(DepartmentsApprovalViewModel departments, DateTime DateToSchedule, string Data, params string[] SelectedRoles)
        {
            if (SelectedRoles == null)
            {
                Session["Display"] = "display";
                Session["MessageBody"] = "Please select hours available for the day!";
                Session["MessageTitle"] = "Invalid hours available";
                return RedirectToAction("HumanApplicationInspection",
                    new { q = new AesCrypto().Encrypt("ApplicationId=" + departments.HumanSettlementApplication.Id.ToString() + "&Data=" + Data + "&ViewName=" + ViewCodeKeys.ScheduleInspectionSlots) });
            }
            using (var context = new eServicesDbContext())
            {
                try
                {
                    if (SelectedRoles != null)
                    {
                        _base.Initialise(context);
                        Initialise();
                        var rr = context.InspectionSchedules.Where(x => x.HumanSettlementApplicationId == departments.HumanSettlementApplication.Id && !x.IsDeleted).ToList();
                        var rrq = rr.Select(r => r.DateToScheduleId).ToList();
                        var Dates = context.DateToSchedules.Where(x => x.HumanSettlementApplicationId == departments.HumanSettlementApplication.Id && x.ShecduleDate == DateToSchedule).GroupBy(r => r.ShecduleDate).Select(x => x.FirstOrDefault()).ToList();
                        if (Dates.Count == 0)
                        {
                            var dateTo = new DateToSchedule();
                            dateTo.ShecduleDate = DateToSchedule.Date;
                            dateTo.HumanSettlementApplicationId = departments.HumanSettlementApplication.Id;
                            context.DateToSchedules.Add(dateTo);
                            context.SaveChanges();
                            Dates = context.DateToSchedules.Where(x => x.HumanSettlementApplicationId == departments.HumanSettlementApplication.Id && x.ShecduleDate == DateToSchedule).GroupBy(r => r.ShecduleDate).Select(x => x.FirstOrDefault()).ToList();
                        }
                        foreach (var item in Dates)
                        {
                            foreach (var item2 in SelectedRoles)
                            {
                                var thisItem = Convert.ToInt32(item2);
                                var ispsch = context.InspectionSchedules.OrderByDescending(x => x.Id).FirstOrDefault(x => x.DateToScheduleId == item.Id && x.TimeSlotId == thisItem && !x.IsDeleted);
                                var timeslot = context.TimeSlots.FirstOrDefault(x => x.Id == thisItem).Id;
                                var scheduleddates = context.ScheduledInspections.ToList();
                                var User = GetBackOfficeId(context, departments.HumanSettlementApplication.Id, false);
                                var activeDirectoryOn = Convert.ToInt16(context.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor).FirstOrDefault().Value);
                                var UserId = User.Id != 0 ? User.Id : activeDirectoryOn;

                                if (ispsch == null)
                                {
                                    var apd = new InspectionSchedule();
                                    var dateTo = new DateToSchedule();
                                    dateTo.ShecduleDate = DateToSchedule.Date;
                                    dateTo.HumanSettlementApplicationId = departments.HumanSettlementApplication.Id;
                                    var checkdate = context.DateToSchedules.Where(x => x.ShecduleDate == DateToSchedule && x.HumanSettlementApplicationId == departments.HumanSettlementApplication.Id).FirstOrDefault();
                                    if (checkdate != null)
                                    {
                                        dateTo = checkdate;
                                    }
                                    else
                                    {
                                        context.DateToSchedules.Add(dateTo);
                                        context.SaveChanges();
                                    }
                                    apd.HumanSettlementApplicationId = departments.HumanSettlementApplication.Id;
                                    apd.DateToScheduleId = dateTo.Id;
                                    apd.TimeSlotId = timeslot;
                                    context.InspectionSchedules.Add(apd);
                                    context.SaveChanges();
                                }

                            }
                        }
                    }
                    return RedirectToAction("HumanApplicationInspection",
                        new { q = new AesCrypto().Encrypt("ApplicationId=" + departments.HumanSettlementApplication.Id.ToString() + 
                                                            "&Data=" + Data + 
                                                            "&ViewName=" + ViewCodeKeys.ScheduleInspectionSlots) });
                }
                catch (Exception error)
                {
                    return View("_Error");
                }
            }
        }
        [DecryptParameter]
        public ActionResult InspectionNotificationApplicant(int Id)
        {
            using (var context = new eServicesDbContext())
            {
                var dates = context.DateToSchedules.Where(x => x.HumanSettlementApplicationId == Id).ToList();
                if (dates.Count > 0)
                {
                    var Key = context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwitingInspectionSchedule);
                    MatchingHelper.ChangeHumanStatus(context, (int)Key?.Id, Id);

                    //var User = GetBackOfficeId(db, Id, false);
                    //var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.ScheduleInspectionSlots).FirstOrDefault();
                    //var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor).FirstOrDefault().Value);
                    //var UserId = User.Id != 0 ? User.Id : activeDirectoryOn;
                    //MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)Id, null, ResponsibilityTypeId.Id, UserId);

                    int emailboodyId = context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.UnitInspectionScheduleMail).Id;
                    EmailHelper.CustomerEmailOrSMSNotification(context, Id, emailboodyId);
                }
            }
            return RedirectToAction("PropertyLeaseInspections", new { q = new AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.ScheduleInspectionSlots) });
        }

        public ActionResult ApplicationRequestTimeSlot(int Id)
        {
            if (Id == 0) throw new Exception("Invalid schedule");
            using (var core = new eServicesDbContext())
            {
                MatchingHelper.ChangeHumanStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots).Id, Id);
                //EHCRoundRobin(Id, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RequestTimeSlots).Id;
                EmailHelper.CustomerEmailOrSMSNotification(core, Id, emailboodyId);
                // return RedirectToAction("Inbox");
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
            var applicationProp = db.HumanSettlementApplications.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Include(r => r.Customer).Include(r => r.SystemUser).Where(x => x.Id == id).FirstOrDefault();
            if (applicationProp.Status.Key == StatusKeys.RatesRebateAdditionalPropertyOwnersPending) return View("_Error");

            var TimeSlots = db.TimeSlots.Where(r => r.IsActive).ToList();
            var vm = new DepartmentsApprovalViewModel();
            vm.Features = new SelectList(TimeSlots, "Id", "Name");

            vm.HumanSettlementApplication = applicationProp;
            vm.InspectionScheduleList = db.InspectionSchedules.Include(r => r.DateToSchedule).Include(r => r.TimeSlot).Where(x => x.HumanSettlementApplicationId == id && !x.IsDeleted && !x.IsInspected).ToList() ?? null;
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
                //MatchingHelper.ScheduleInspectionUnit(context, Id);
                MatchingHelper.HumanScheduleInspectionUnit(context, Id);

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
                var User = GetBackOfficeId(db, (int)schedule.HumanSettlementApplicationId, false);
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.HousingSupervisor).FirstOrDefault().Value);
                var UserId = User.Id != 0 ? User.Id : activeDirectoryOn;

                bool result = MatchingHelper.HumanValidateSelectedWithApproved(context, Id, UserId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
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

        [Authorize]
        [HttpPost]
        public ActionResult ConductUnitInspection(int ApplicationId, string ApprovalStatusddl, string UnitInspectionComment)
        {
            try
            {
                using(var core = new eServicesDbContext())
                {
                    _base.Initialise(core);
                    var userID = _base.Customer.Id;
                    var Keys = core.Status;
                    var custmusers = core.Customers.FirstOrDefault(x => x.Id == _base.Customer.Id);
                    var rcsApps = core.HumanSettlementApplications.Where(x => x.Id == ApplicationId && x.IsDeleted == false).Include(x => x.Customer).FirstOrDefault();
                    var ResponsibilityTypeId = core.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Inspections).FirstOrDefault();
                    var ActivityTrackerMessage = string.Empty;
                    var c = new CaptureController();
                    switch (ApprovalStatusddl)
                    {
                        case RCSActionTypeKeys.Habitable:
                            MatchingHelper.ChangeHumanStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.awaited).Id, ApplicationId);
                            // MatchingHelper.RoundRobinMarkJobAsFinished(core, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);
                            //   EHCRoundRobin(rcsApps.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                            //customer email here
                            ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UnitInspectionApproved).Description.ToString();
                            break;
                        case RCSActionTypeKeys.HabitableMinorDefects:
                            MatchingHelper.ChangeHumanStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.awaited).Id, ApplicationId);
                            //     MatchingHelper.RoundRobinMarkJobAsFinished(core, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);
                            //  EHCRoundRobin(rcsApps.Id, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

                            var UnitId = core.ApplicantUnits.Include(r => r.Matched).FirstOrDefault(x => x.HumanSettlementApplicationId == rcsApps.Id).Matched.UnitsHumanSettlement01Id;
                            var findItem = core.UnitsHumanSettlement01s.FirstOrDefault(x => x.Id == UnitId);
                            findItem.Inspection = true;
                            core.Entry(findItem).State = EntityState.Modified;
                            core.SaveChanges();

                            //customer email here
                            ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UnitInspectionApproved).Description.ToString();
                            break;
                        case RCSActionTypeKeys.NotHabitable:
                            MatchingHelper.ChangeHumanStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.CustomerQueryPending).Id, ApplicationId);
                            // MatchingHelper.RoundRobinMarkJobAsFinished(core, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);
                            //                      1      2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17     18   19
                            //EHCRoundRobin((int)id, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                            //customer email here
                            ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UnitInspectionFailedUninhabitable).Description.ToString();
                            break;
                    }
                    MatchingHelper.ActivityTrackerHuman(core, ApplicationId, ActivityTrackerMessage, custmusers.Id);
                    MatchingHelper.MarkHumanInspectionDatesAsInspected(core, ApplicationId);

                    core.conductUnitInspections.Add(new ConductUnitInspection()
                    {
                        HumanSettlementApplicationId = rcsApps.Id,
                        InspectionComment = UnitInspectionComment,
                        CreatedBySystemUserId = _base.SystemUser.Id,
                        CreatedDateTime = DateTime.Now
                    });
                    core.SaveChanges();
                    ViewBag.PostSuccess = "Success";
                    return RedirectToAction("PropertyLeaseInspections", "HumanSettlementApplication", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.PreInspection) });
                }
            }
            catch (Exception error)
            {
                return View("_Error");
            }
        }


        //[Authorize(Roles = "Senior Housing Specialist, Housing Liaison Officer, Caretaker")]
        //[DecryptParameter]
        //public ActionResult ConductExitInspection(int Id, string ViewName)
        //{
        //    try
        //    {
        //        Initialise();
        //        var vm = new DepartmentsApprovalViewModel();
        //        var core = new eServicesDbContext();
        //        var plmApps = core.HumanSettlementApplications.FirstOrDefault(r => r.Id == Id);

        //        var master = core.HumanSettlementLeaseMasters
        //            .Include(r => r.HumanSettlementApplication.PurchaserType)
        //            .Include(r => r.Status)
        //            .Include(r => r.HumanSettlementApplication)
        //            .Where(c => c.HumanSettlementApplicationId == plmApps.Id && c.IsActive).FirstOrDefault();

        //        var details = core.HumanSettlementLeaseDetails
        //            .Include(r => r.HumanSettlementApplication.PurchaserType)
        //            .Include(r => r.Status)
        //            .Include(r => r.HumanSettlementApplication)
        //            .Where(c => c.HumanSettlementApplicationId == plmApps.Id && c.IsActive).FirstOrDefault();

        //        ViewBag.ApprovalStatus = new SelectList(core.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Habitable || x.Key == RCSActionTypeKeys.HabitableMinorDefects || x.Key == RCSActionTypeKeys.NotHabitable).OrderBy(x => x.Name), "Key", "Name");

        //        var application = core.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
        //        if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));
        //        var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
        //        if (referenceType == null) throw new Exception("Invalid reference type.");

        //        DocumentsViewModel dvm = new DocumentsViewModel();
        //        DocumentsViewModel dvmTemplate = new DocumentsViewModel();

        //        MatchingHelper.DocumentTenantAccountValidation(dvm, core, plmApps.CustomerId, plmApps.CustomerId, (int)referenceType.Id, (int)application.Id, "", plmApps.Id, true);
        //        MatchingHelper.DocumentPostInspection(dvm, core, plmApps.CustomerId, plmApps.CustomerId, (int)referenceType.Id, (int)application.Id, "", plmApps.Id, true);
        //        MatchingHelper.DocumentGetPostInspectionTemplate(dvmTemplate, db, 2, 2, (int)referenceType.Id, (int)application.Id, "", 100000, false);

        //        vm.HumanSettlementApplication = master?.HumanSettlementApplication ?? details?.HumanSettlementApplication;
        //        vm.HumanSettlementLeaseMaster = master;
        //        vm.HumanSettlementLeaseDetails = details;
        //        vm.DocumentsViewModel = dvm;
        //        vm.DocumentsViewModelTemplate = dvmTemplate;

        //        return View(vm);
        //    }
        //    catch (Exception error)
        //    {
        //        return View("_Error");
        //        // throw;
        //    }
        //}

        [HttpPost]
        [Authorize]
        public ActionResult MaintenanceJobSheet(int ApplicationId, string ApprovalStatusddl)
        {
            using (var core = new eServicesDbContext())
            {
                Initialise();
                var userID = Customer.Id;
                var Keys = core.Status;
                var application = core.HumanSettlementApplications.FirstOrDefault(x => x.Id == ApplicationId);
                var c = new CaptureController();
                var ActivityTrackerMessage = string.Empty;

                if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                {
                    var Key = core.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingInspectionScheduleSlots);
                    MatchingHelper.ChangeHumanStatus(core, Key.Id, application.Id);
                    var ResponsibilityTypeId = core.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.MaintananceJobSheet).FirstOrDefault();
                    ActivityTrackerMessage = string.Format("Application Unit maintanance has been approved by {0}", SystemUser.FullName);
                    MatchingHelper.ActivityTrackerHuman(core, ApplicationId, ActivityTrackerMessage, Customer.Id);

                    //MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)rcsApps.Id, null, ResponsibilityTypeId.Id, UserId);

                    ////                        1      2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17      18   19
                    //EHCRoundRobin(rcsApps.Id, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                }
                else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
                {
                    var Key = core.Status.FirstOrDefault(x => x.Key == StatusKeys.CustomerQueryPending);
                    MatchingHelper.ChangeHumanStatus(core, Key.Id, application.Id);
                    ActivityTrackerMessage = string.Format("Application Unit maintanance has been rejected by {0}", SystemUser.FullName);
                    MatchingHelper.ActivityTrackerHuman(core, ApplicationId, ActivityTrackerMessage, Customer.Id);
                }
                return RedirectToAction("PropertyLeaseInspections", "HumanSettlementApplication", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.MaintananceJobSheet) });
            }
        }




        #endregion


        [DecryptParameter]
        public ActionResult MatchedUnitDetails(int ApplicationId)
        {
            if (ApplicationId == 0) return View("_Error");
            try
            {
                using(var core = new eServicesDbContext())
                {
                    var MatchRec = core.MatchedUnits.OrderByDescending(d => d.Id).FirstOrDefault(a => a.HumanSettlementApplicationId == ApplicationId && !a.IsAccepted && !a.RejectedProperty);
                    var unit = core.UnitsHumanSettlement01s.FirstOrDefault(x => x.Id == MatchRec.UnitsHumanSettlement01Id) ?? null;
                    var model = new UnitViewModel();
                    model.UnitsHumanSettlement01 = unit ?? null;
                    model.MatchedUnitId = (int)MatchRec?.Id;
                    model.UnitDescription = unit.LettingRequirements ?? "No Description Available";
                    return View(model);
                }
            }
            catch (Exception error)
            {
                return View("_Error");
            }
        }
        public JsonResult AcceptMatchedUnit(int id)
        {
            var core = new eServicesDbContext();
            var Keys = core.Status.ToList();
            var matchedUnit = core.MatchedUnits.FirstOrDefault(x => x.Id == id) ?? null;
            var unitInformation = core.UnitsHumanSettlement01s.FirstOrDefault(x => x.Id == matchedUnit.UnitsHumanSettlement01Id);
            var result = MatchingHelper.HumanAcceptMatchedUnit(core, unitInformation, (int)matchedUnit.HumanSettlementApplicationId, 1, matchedUnit.Id);
            var output = result == true ? "Success" : "Failure";
            var rcsApps = core.HumanSettlementApplications.FirstOrDefault(x => x.Id == matchedUnit.HumanSettlementApplicationId);
            WorkAllocationHumanHelper.AgreementOfLease((int)rcsApps.Id, false, false, false, false, false, true);
            var custmusers = core.Customers.FirstOrDefault(x => x.Id == rcsApps.CustomerId);
            var ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AcceptMatchedUnit).Description.ToString();
            MatchingHelper.ActivityTrackerHuman(core, id, ActivityTrackerMessage, custmusers.Id);
            int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.Accetproperty).Id;
            EmailHelper.CustomerEmailOrSMSNotification(core, rcsApps.Id, emailboodyId);
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RejectMatchedUnit(int id)
        {
            var core = new eServicesDbContext();
            var matchedUnit = core.MatchedUnits.FirstOrDefault(x => x.HumanSettlementApplicationId == id) ?? null;
            var unitInformation = core.UnitsHumanSettlement01s.FirstOrDefault(x => x.Id == matchedUnit.UnitsHumanSettlement01Id);
            MatchingHelper.MarkMatchedUnitAsRejected(core, matchedUnit.Id);
            var result = MatchingHelper.HumanRejectMatchedUnit(core, unitInformation, (int)matchedUnit.HumanSettlementApplicationId, (int)matchedUnit.Id);
            var output = result == true ? "Success" : "Failure";
            var rcsApps = core.HumanSettlementApplications.FirstOrDefault(x => x.Id == matchedUnit.HumanSettlementApplicationId);
            var custmusers = core.Customers.FirstOrDefault(x => x.Id == rcsApps.CustomerId);
            var ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RejectMatchedUnit).Description.ToString();
            MatchingHelper.ActivityTrackerHuman(core, id, ActivityTrackerMessage, custmusers.Id);
            int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.Rejectproperty).Id;
            EmailHelper.CustomerEmailOrSMSNotification(core, rcsApps.Id, emailboodyId);
            return Json(output, JsonRequestBehavior.AllowGet);
        }






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

                return View("_Error");
            }
        }

        public JsonResult GetUnitbyPreferredAreaId(int id, int ApplicationId)
        {
            try
            {
                using (var core = new eServicesDbContext())
                {
                    var PLA = core.HumanSettlementApplications.FirstOrDefault(d => d.Id == ApplicationId);
                    //need to select units with the following criteria
                    var category = PLA.UnitCategoryId;
                    var typology = PLA.UnitTypologyId;

                    //var units = core.UnitsEkurhuleniHousingCompany2.Where(a=>a.UnitCategoryId== category && a.UnitTypologyId== typology)
                    var units = core.UnitsHumanSettlement01s.Where(a => a.UnitTypologyId == PLA.UnitTypologyId && a.UnitCategoryId == PLA.UnitCategoryId && !a.IsTaken).ToList();
                    foreach(var unit in units)
                    {
                        unit.ColorCode = String.Format("{0}, unit no. {1}", unit.BuildingName, unit.SpaceUnitNumber);
                    }
                    units.Where(unit => (unit.PreferredComplexAreaId == id)).ToList();
                    var rrq = units.Select(a => new { a.Id, a.ColorCode }).ToList();
                    return Json(rrq);
                }
            }
            catch (Exception io)
            {
                EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return Json("", JsonRequestBehavior.AllowGet);
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
        public ActionResult Details(int ApplicationId)
        {
            if (ApplicationId == 0) throw new Exception(string.Format("Invalid Application - {0}", ApplicationId));

            using (var _context = new eServicesDbContext())
            {
                var vm = new ApplicationDetailsViewModel();

                HumanSettlementApplication humanSettlementApplications = null;
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
                UnitsEkurhuleniHousingCompany ekurhuleniHousingCompany = null;
                HumanSettlementLeaseDetails Details = null;
                HumanSettlementLeaseMaster Master = null;



                var NotMatched = "Information Not Available";
                UnitsEkurhuleniHousingCompany EmptyUnit = new UnitsEkurhuleniHousingCompany();

                humanSettlementApplications = _context.HumanSettlementApplications
                    .Include(r => r.UnitTypology)
                    .Include(r => r.UnitCategory)
                    .Include(r => r.HSIncomeBracket)
                    .Include(r => r.TitleType)
                    .Include(r => r.CoTitleType)
                    .Include(r => r.SecAppTitleType)
                    .Include(r => r.SecAppGender)
                    .Include(r => r.IdentificationType)
                    .Include(r => r.PreferredComplexArea)
                    .Include(r => r.PreferredComplexArea2)
                    .Include(r => r.PurchaserType)
                    .Include(r => r.IncomeSource)
                    .Include(r => r.SecAppIncomeSource)
                    .Where(x => x.Id == ApplicationId).FirstOrDefault();

                Master = _context.HumanSettlementLeaseMasters
                    .Where(x => x.HumanSettlementApplicationId == ApplicationId).FirstOrDefault();
                Details = _context.HumanSettlementLeaseDetails
                    .Where(x => x.HumanSettlementApplicationId == ApplicationId).FirstOrDefault();

                var renewal = _context.HSRenewalActions.Include(r => r.ActionByUser).Where(e => e.HumanSettlementApplicationId == ApplicationId).ToList();
                vm.RenewalActions = renewal;

                ViewBag.TenantType = "Individual";
                ViewBag.ApplicationType = "Individual";

                pLMApplicationHistortyLogs = _context.PLMApplicationHistortyLogs.Where(x => x.HumanSettlementApplicationId == humanSettlementApplications.Id).OrderBy(x => x.CreatedDateTime).Include(r => r.CreatedBySystemUser).Include(r => r.User).ToList();

                if (ekurhuleniHousingCompany == null)
                {
                    EmptyUnit.BuildingName = NotMatched.ToString();
                    EmptyUnit.SpaceUnitNo = NotMatched.ToString();
                    EmptyUnit.Address = NotMatched.ToString();
                    EmptyUnit.Surburb = NotMatched.ToString();
                    EmptyUnit.GeoLocation = NotMatched.ToString();
                    EmptyUnit.Postal = NotMatched.ToString();
                    EmptyUnit.LettingRequirements = NotMatched.ToString();
                    ViewBag.UnitAvailable = "1";
                }

                vm.UnitsEkurhuleniHousingCompany = ekurhuleniHousingCompany ?? EmptyUnit;
                vm.HousingSupervisor = HousingSupervisor;
                vm.LettingOfficer = LettingOfficer;
                vm.ApplicantUnit = applicantUnit;
                vm.HumanSettlementApplication = humanSettlementApplications;
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
                vm.HumanSettlementLeaseMaster = Master;
                vm.HumanSettlementLeaseDetails = Details;

                // Documents required variables
                var documentCheckLists = new List<DocumentCheckList>();
                var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();
                var customer = db.Customers.Include(s => s.SystemUser).Include(s => s.Status).Include(s => s.CustomerType).FirstOrDefault(c => c.Id == humanSettlementApplications.CustomerId);

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


                if (application == null) throw new Exception("Invalid application.");
                if (referenceType == null) throw new Exception("Invalid reference type.");

                // MatchingHelper.DocumentCaptureApplication(dvm, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", humanSettlementApplications.Id, false);
                MatchingHelper.HumanRenewalDocuments(dvmTenantLease, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", humanSettlementApplications.Id, false);
                MatchingHelper.DocumentPostInspection(dvmExitInspection, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", humanSettlementApplications.Id, false);
                //MatchingHelper.DocumentConductMaintanaceJobSheet(dvmMaintananceJobSheet, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                //MatchingHelper.DocumentConductUnitInspection(dvmUnitInspection, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                //MatchingHelper.DocumentEvictionCommitteeOutcome(dvmEvictionCommittee, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                //MatchingHelper.DocumentPropertyEvictionValidation(dvmPropertyEviction, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                //MatchingHelper.DocumentRiskAssessmentOutcome(dvmRisk, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentAccountValidation(dvmTenantAccoutValidation, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", humanSettlementApplications.Id, false);
                //MatchingHelper.DocumentTenantRiskAssessment(dmvTenantRiskAssessment, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);
                MatchingHelper.DocumentAgreementOfLease(dvmLeaseAgreement, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", humanSettlementApplications.Id, false);
                //MatchingHelper.DocumentDebitOrder(dvmDebitOrder, _context, customer.Id, customer.Id, referenceType.Id, application.Id, "", propertyLeaseApplication.Id, false);

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

                return View(vm);
            }

        }

        [Authorize(Roles = "Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult PropertyLeaseApplicationNotice(string Search, string Render, int? Id, string ReferenceNumber)
        {
            Initialise();
            bool _search = Convert.ToBoolean(Search);
            bool _render = Convert.ToBoolean(Render);
            var vm = new DepartmentsApprovalViewModel();


            if (_search)
            {
                if (Session["Display"] != null)
                {
                    Session["Display"] = null;
                    ViewBag.DisplaySwal = "True";
                    if (Session["Message"] != null)
                    {
                        ViewBag.MessageBodySwal = Session["Message"].ToString();
                        Session["Message"] = null;
                    }
                }
                if (Session["success"] != null) ViewBag.success = "display";
                Session["success"] = null;
                vm.LeaseDetails = new LeaseDetails();
                vm.Search = true;
                vm.Render = false;
            }
            else if (_render)
            {
                var core = new eServicesDbContext();
                ReferenceNumber = ReferenceNumber != null ? ReferenceNumber.ToUpper() : null;
                var plmApps = core.HumanSettlementApplications.Include(d => d.Status).Where(r => r.ApplicationReferenceNumber == ReferenceNumber).FirstOrDefault() ?? null;
                if (plmApps == null)
                {
                    Session["Message"] = "The Reference Number You Have Entered Is Invalid, Try Again!";
                    Session["Display"] = "Display";
                    return RedirectToAction("PropertyLeaseApplicationNotice", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Search=" + true + "&Render=" + false + "&Id=0") });
                }
                if (plmApps.Status.Key == StatusKeys.ApplicantVacated)
                {
                    Session["Message"] = "The tenant has vacated the unit for the ref. no entered!";
                    Session["Display"] = "Display";
                    return RedirectToAction("PropertyLeaseApplicationNotice", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Search=" + true + "&Render=" + false + "&Id=0") });
                }

                var master = core.HumanSettlementLeaseMasters
                    .Include(r => r.HumanSettlementApplication.PurchaserType)
                    .Include(r => r.Status)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(c => c.HumanSettlementApplicationId == plmApps.Id && c.IsActive).FirstOrDefault();
                var details = core.HumanSettlementLeaseDetails
                    .Include(r => r.HumanSettlementApplication.PurchaserType)
                    .Include(r => r.Status)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(c => c.HumanSettlementApplicationId == plmApps.Id && c.IsActive).FirstOrDefault();

                vm.HumanSettlementApplication = master?.HumanSettlementApplication ?? details?.HumanSettlementApplication;
                vm.HumanSettlementLeaseMaster = master;
                vm.HumanSettlementLeaseDetails = details;
                vm.Render = true;
                vm.Search = false;
            }

            return View(vm);
        }

        [Authorize(Roles = "Customers, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [HttpPost]
        public ActionResult HumanAgreementServeNotice(int ApplicatoinId, string ServeNoticeDate)
        {
            using (var _context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var userID = Customer;
                    var Keys = _context.Status;
                    var LeaseApplication = _context.HumanSettlementApplications.FirstOrDefault(x => x.Id == ApplicatoinId && x.IsDeleted == false);
                    var master = _context.HumanSettlementLeaseMasters
                        .Include(r => r.HumanSettlementApplication.PurchaserType)
                        .Include(r => r.Status)
                        .Include(r => r.HumanSettlementApplication)
                        .Where(c => c.HumanSettlementApplicationId == LeaseApplication.Id && c.IsActive).FirstOrDefault();
                    var details = _context.HumanSettlementLeaseDetails
                        .Include(r => r.HumanSettlementApplication.PurchaserType)
                        .Include(r => r.Status)
                        .Include(r => r.HumanSettlementApplication)
                        .Where(c => c.HumanSettlementApplicationId == LeaseApplication.Id && c.IsActive).FirstOrDefault();

                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantNotice).Description.ToString();
                    MatchingHelper.ActivityTrackerHuman(_context, LeaseApplication.Id, ActivityTrackerMessage, custmusers.Id);

                    MatchingHelper.HumanServeNoticeDate(_context, LeaseApplication.Id, Convert.ToDateTime(ServeNoticeDate));

                    //var ResponsibilityTypeId = _context.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RenewalFirstRecommendation).Id;
                    //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                    WorkAllocationHumanHelper.AgreementTerminations((int)LeaseApplication.Id, true, false, false);
                    //Send e-mail and SMS notification
                    int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ServeNotice).Id;
                    EmailHelper.CustomerEmailOrSMSNotification(db, LeaseApplication.Id, emailboodyId);

                    if (LeaseApplication.IsMaster) master.NoticeDate = Convert.ToDateTime(ServeNoticeDate);
                    if (!LeaseApplication.IsMaster) details.NoticeDate = Convert.ToDateTime(ServeNoticeDate);
                    if (LeaseApplication.IsMaster) _context.Entry(master).State = EntityState.Modified;
                    if (!LeaseApplication.IsMaster) _context.Entry(details).State = EntityState.Modified;
                    _context.SaveChanges();

                    //var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Terminations).FirstOrDefault();
                    ////                                                               1     2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17      18   19
                    //EHCRoundRobin((int)LeaseApplication.Id, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                    //var BackOffice = GetBackOfficeId(db, LeaseApplication.Id, true);

                    //var result = BackOffice.UserFullName != null ? BackOfficeNotification(LeaseApplication.Id, BackOffice.Id, ResponsibilityTypeId.Name) : true;
                    Session["success"] = "display";
                    return RedirectToAction("PropertyLeaseApplicationNotice", "HumanSettlementApplication", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("Search=" + true + "&Render=" + false + "&Id=0") });
                }
                catch (Exception Io)
                {
                    return RedirectToAction("Logoff", "Account");
                }
            }
        }

        [Authorize(Roles = "Senior Housing Specialist, Housing Liaison Officer, Caretaker, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult Termination(string Search, string Render, int Id, string Review, string ReferenceNumber)
        {
            Initialise();
            bool _search = Convert.ToBoolean(Search);
            bool _render = Convert.ToBoolean(Render);
            bool _review = Convert.ToBoolean(Review);
            var vm = new DepartmentsApprovalViewModel();

            if (_search)
            {
                if (Session["Display"] != null)
                {
                    Session["Display"] = null;
                    ViewBag.DisplaySwal = "True";
                    if (Session["Message"] != null)
                    {
                        ViewBag.MessageBodySwal = Session["Message"].ToString();
                        Session["Message"] = null;
                    }
                }
                if (Session["success"] != null) ViewBag.success = "display";
                Session["success"] = null;
                vm.LeaseDetails = new LeaseDetails();
                vm.Search = true;
                vm.Render = false;
            }
            else if (_render)
            {
                var core = new eServicesDbContext();
                ReferenceNumber = ReferenceNumber != null ? ReferenceNumber.ToUpper() : null;
                var plmApps = core.HumanSettlementApplications.Include(d => d.Status).Where(r => r.ApplicationReferenceNumber == ReferenceNumber).FirstOrDefault() ?? null;


                if (plmApps == null) Session["Message"] = "The Reference Number You Have Entered Is Invalid, Try Again!";
                if (plmApps == null) Session["Display"] = "Display";
                if (plmApps == null) return RedirectToAction("Termination", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Search=" + true + "&Render=" + false + "&Id=0") });
                var master = core.HumanSettlementLeaseMasters
                    .Include(r => r.HumanSettlementApplication.PurchaserType)
                    .Include(r => r.Status)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(c => c.HumanSettlementApplicationId == plmApps.Id && c.IsActive).FirstOrDefault();
                var details = core.HumanSettlementLeaseDetails
                    .Include(r => r.HumanSettlementApplication.PurchaserType)
                    .Include(r => r.Status)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(c => c.HumanSettlementApplicationId == plmApps.Id && c.IsActive).FirstOrDefault();
                var Key = master?.Status.Key ?? details.Status.Key;
                if (plmApps.Status.Key == StatusKeys.ApplicantVacated || Key == StatusKeys.ApplicantVacated)
                {
                    Session["Message"] = "The tenant has vacated the unit for the ref. no entered!";
                    Session["Display"] = "Display";
                    return RedirectToAction("Termination", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Search=" + true + "&Render=" + false + "&Id=0") });
                }


                plmApps.ColorCode = plmApps.ServeNoticeDate?.ToString().Substring(0, 10);
                var NoticeDate = master?.NoticeDate.ToString() ?? details?.NoticeDate.ToString();

                if (!string.IsNullOrEmpty(NoticeDate))
                {
                    ViewBag.ApprovalStatus = new SelectList(core.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.LeaseNotRenuewed || x.Key == RCSActionTypeKeys.TenantNotice || x.Key == RCSActionTypeKeys.TenantDeceased).OrderBy(x => x.Name), "Key", "Name");
                }
                else
                {
                    ViewBag.ApprovalStatus = new SelectList(core.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.LeaseNotRenuewed || x.Key == RCSActionTypeKeys.TenantDeceased).OrderBy(x => x.Name), "Key", "Name");
                }
                var enddate = master?.NoticeDate ?? details?.NoticeDate;


                vm.HumanSettlementApplication = master?.HumanSettlementApplication ?? details?.HumanSettlementApplication;
                vm.HumanSettlementLeaseMaster = master;
                vm.HumanSettlementLeaseDetails = details;
                vm.HumanSettlementApplication.ColorCode = Convert.ToDateTime(enddate).ToString("yyyy-MM-dd");
                vm.Render = true;
                vm.Search = false;
            }
            else if (_review)
            {
                var core = new eServicesDbContext();
                var plmApps = core.HumanSettlementApplications.FirstOrDefault(r => r.Id == Id) ?? null;

                var master = core.HumanSettlementLeaseMasters
                    .Include(r => r.HumanSettlementApplication.PurchaserType)
                    .Include(r => r.Status)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(c => c.HumanSettlementApplicationId == plmApps.Id && c.IsActive).FirstOrDefault();

                var details = core.HumanSettlementLeaseDetails
                    .Include(r => r.HumanSettlementApplication.PurchaserType)
                    .Include(r => r.Status)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(c => c.HumanSettlementApplicationId == plmApps.Id && c.IsActive).FirstOrDefault();

                var application = core.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                MatchingHelper.DocumentAccountValidation(dvm, core, plmApps.CustomerId, plmApps.CustomerId, (int)referenceType.Id, (int)application.Id, "", plmApps.Id, true);

                vm.HumanSettlementApplication = master?.HumanSettlementApplication ?? details?.HumanSettlementApplication;
                vm.HumanSettlementLeaseMaster = master;
                vm.HumanSettlementLeaseDetails = details;
                vm.Render = false;
                vm.Search = false;
                vm.Review = true;
                vm.DocumentsViewModel = dvm;

                ViewBag.ApprovalStatus = new SelectList(core.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).OrderBy(x => x.Name), "Key", "Name");

            }
            return View(vm);

        }

        [Authorize(Roles = "Senior Housing Specialist, Housing Liaison Officer, Caretaker, Super Administrators, Back Office System Administrator")]
        [HttpPost]
        public ActionResult HumanAgreementTermination(int ApplicatoinId, string ServeNoticeDate, string ApprovalStatusddl)
        {
            using (var _context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var userID = Customer;
                    var Keys = _context.Status;
                    var LeaseApplication = _context.HumanSettlementApplications.FirstOrDefault(x => x.Id == ApplicatoinId && x.IsDeleted == false);
                    var master = _context.HumanSettlementLeaseMasters
                        .Include(r => r.HumanSettlementApplication.PurchaserType)
                        .Include(r => r.Status)
                        .Include(r => r.HumanSettlementApplication)
                        .Where(c => c.HumanSettlementApplicationId == LeaseApplication.Id && c.IsActive).FirstOrDefault();
                    var details = _context.HumanSettlementLeaseDetails
                        .Include(r => r.HumanSettlementApplication.PurchaserType)
                        .Include(r => r.Status)
                        .Include(r => r.HumanSettlementApplication)
                        .Where(c => c.HumanSettlementApplicationId == LeaseApplication.Id && c.IsActive).FirstOrDefault();

                    var ActivityTrackerMessage = string.Empty;
                    var custmusers = (Customer)null;
                    int StatusId = 0;
                    int emailboodyId = 0;
                    var ResponsibilityTypeId = 0;
                    var termination = new LeaseTermination
                    {
                        LeaseReferenceNumber = LeaseApplication.ApplicationReferenceNumber,
                        HumanSettlementApplicationId = LeaseApplication.Id,
                        TerminationDate = Convert.ToDateTime(ServeNoticeDate),
                        StatusId = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingterminantionApproval).Id
                    };

                    if (ApprovalStatusddl != RCSActionTypeKeys.Approved && ApprovalStatusddl != RCSActionTypeKeys.Habitable && ApprovalStatusddl != RCSActionTypeKeys.HabitableMinorDefects && ApprovalStatusddl != RCSActionTypeKeys.NotHabitable && ApprovalStatusddl != RCSActionTypeKeys.NotVacated && ApprovalStatusddl != RCSActionTypeKeys.Vacated)
                    {
                        LeaseApplication.ServeNoticeDate = Convert.ToDateTime(ServeNoticeDate);
                        if (LeaseApplication.IsMaster) master.NoticeDate = Convert.ToDateTime(ServeNoticeDate);
                        if (!LeaseApplication.IsMaster) details.NoticeDate = Convert.ToDateTime(ServeNoticeDate);
                        if (LeaseApplication.IsMaster) _context.Entry(master).State = EntityState.Modified;
                        if (!LeaseApplication.IsMaster) _context.Entry(details).State = EntityState.Modified;
                        _context.Entry(LeaseApplication).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                    Session["NoticeSuccess"] = "true";

                    switch (ApprovalStatusddl)
                    {
                        case RCSActionTypeKeys.TenantNotice:
                            termination.ReasonForTermination = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.TenantNotice).Description;
                            _context.LeaseTerminations.Add(termination);
                            _context.SaveChanges();
                            StatusId = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.TenantNotice).Id;
                            if (LeaseApplication.IsMaster) MatchingHelper.ChangeMasterStatus(_context, StatusId, master.Id);
                            if (!LeaseApplication.IsMaster) MatchingHelper.ChangeLeaseDetailsStatus(_context, StatusId, details.Id);
                            //MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)LeaseApplication.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, UserId);
                            custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                            ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantNotice).Description.ToString();
                            MatchingHelper.ActivityTrackerHuman(_context, LeaseApplication.Id, ActivityTrackerMessage, custmusers.Id);

                            ResponsibilityTypeId = _context.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.NoticedAgreements).Id;
                            //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id, true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            }
                            WorkAllocationHumanHelper.AgreementTerminations((int)LeaseApplication.Id, false, true, false);
                            
                            return RedirectToAction("NoticedAgreementTerminations", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Search=" + true + "&Render=" + false + "&Id=0") });
                        case RCSActionTypeKeys.LeaseNotRenuewed:
                            StatusId = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.LeaseNotRenewed).Id;
                            if (LeaseApplication.IsMaster) MatchingHelper.ChangeMasterStatus(_context, StatusId, master.Id);
                            if (!LeaseApplication.IsMaster) MatchingHelper.ChangeLeaseDetailsStatus(_context, StatusId, details.Id);
                            termination.ReasonForTermination = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.LeaseNotRenewed).Description;
                            _context.LeaseTerminations.Add(termination);
                            _context.SaveChanges();
                            custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                            ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.LeaseNotRenewed).Description.ToString();
                            MatchingHelper.ActivityTrackerHuman(_context, LeaseApplication.Id, ActivityTrackerMessage, custmusers.Id);
                            WorkAllocationHumanHelper.AgreementTerminations((int)LeaseApplication.Id, false, true, false);
                            break;
                        case RCSActionTypeKeys.TenantDeceased:
                            StatusId = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.TenantDeceased).Id;
                            if (LeaseApplication.IsMaster) MatchingHelper.ChangeMasterStatus(_context, StatusId, master.Id);
                            if (!LeaseApplication.IsMaster) MatchingHelper.ChangeLeaseDetailsStatus(_context, StatusId, details.Id);
                            termination.ReasonForTermination = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.TenantDeceased).Description;
                            _context.LeaseTerminations.Add(termination);
                            _context.SaveChanges();
                            custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                            ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantDeceased).Description.ToString();
                            MatchingHelper.ActivityTrackerHuman(_context, LeaseApplication.Id, ActivityTrackerMessage, custmusers.Id);
                            WorkAllocationHumanHelper.AgreementTerminations((int)LeaseApplication.Id, false, true, false);
                            break;
                        case RCSActionTypeKeys.EndOfLeasePeriod60M:
                            StatusId = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.EndOfLeaseTerm).Id;
                            if (LeaseApplication.IsMaster) MatchingHelper.ChangeMasterStatus(_context, StatusId, master.Id);
                            if (!LeaseApplication.IsMaster) MatchingHelper.ChangeLeaseDetailsStatus(_context, StatusId, details.Id);
                            termination.ReasonForTermination = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.EndOfLeaseTerm).Description;
                            _context.LeaseTerminations.Add(termination);
                            _context.SaveChanges();
                            custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                            ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EndOfLeaseTerm).Description.ToString();
                            MatchingHelper.ActivityTrackerHuman(_context, LeaseApplication.Id, ActivityTrackerMessage, custmusers.Id);
                            WorkAllocationHumanHelper.AgreementTerminations((int)LeaseApplication.Id, false, true, false);
                            break;
                        case RCSActionTypeKeys.Approved:
                            StatusId = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingExitInspection).Id;
                            if (LeaseApplication.IsMaster) MatchingHelper.ChangeMasterStatus(_context, StatusId, master.Id);
                            if (!LeaseApplication.IsMaster) MatchingHelper.ChangeLeaseDetailsStatus(_context, StatusId, details.Id);
                            MatchingHelper.UpdateteAgreementTerminationDates(_context, LeaseApplication.Id);
                            _context.SaveChanges();
                            custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                            ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EndOfLeaseTerm).Description.ToString(); //Wrong message ***new message required***
                            MatchingHelper.ActivityTrackerHuman(_context, LeaseApplication.Id, ActivityTrackerMessage, custmusers.Id);

                            ResponsibilityTypeId = _context.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.PendingTerminationreview).Id;
                            //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id, true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            }
                            WorkAllocationHumanHelper.InspectionsPrePost((int)LeaseApplication.Id, true, false, false);
                            return RedirectToAction("AgreementTermination", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.AgreementTermination) });
                        case RCSActionTypeKeys.HabitableMinorDefects:
                        case RCSActionTypeKeys.NotHabitable:
                            StatusId = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingVacatingConfirm).Id;
                            if (LeaseApplication.IsMaster) MatchingHelper.ChangeMasterStatus(_context, StatusId, master.Id);
                            if (!LeaseApplication.IsMaster) MatchingHelper.ChangeLeaseDetailsStatus(_context, StatusId, details.Id);
                            MatchingHelper.ChangeHumanStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingVacatingConfirm).Id, LeaseApplication.Id);
                            ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UnitInspectionApproved).Description.ToString();
                            //Send e-mail and SMS notification
                            emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ServeNotice).Id;
                            EmailHelper.CustomerEmailOrSMSNotification(db, LeaseApplication.Id, emailboodyId);
                            MatchingHelper.ActivityTrackerHuman(_context, LeaseApplication.Id, ActivityTrackerMessage, Customer.Id);

                            ResponsibilityTypeId = _context.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.PostInspection).Id;
                            //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id, true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            }
                            WorkAllocationHumanHelper.AgreementTerminations((int)LeaseApplication.Id, false, false, true);
                            return RedirectToAction("PropertyLeaseInspections", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.AgreementTermination) });
                        case RCSActionTypeKeys.Habitable:
                            StatusId = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingVacatingConfirm).Id;
                            if (LeaseApplication.IsMaster) MatchingHelper.ChangeMasterStatus(_context, StatusId, master.Id);
                            if (!LeaseApplication.IsMaster) MatchingHelper.ChangeLeaseDetailsStatus(_context, StatusId, details.Id);
                            MatchingHelper.ChangeHumanStatus(db, db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingVacatingConfirm).Id, LeaseApplication.Id);
                            //Send e-mail and SMS notification
                            emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ServeNotice).Id;
                            EmailHelper.CustomerEmailOrSMSNotification(db, LeaseApplication.Id, emailboodyId);
                            ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UnitInspectionApproved).Description.ToString();
                            MatchingHelper.ActivityTrackerHuman(_context, LeaseApplication.Id, ActivityTrackerMessage, Customer.Id);

                            ResponsibilityTypeId = _context.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.PostInspection).Id;
                            //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id, true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            }
                            WorkAllocationHumanHelper.AgreementTerminations((int)LeaseApplication.Id, false, false, true);
                            return RedirectToAction("PropertyLeaseInspections", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.AgreementTermination) });
                        case RCSActionTypeKeys.Vacated:
                            StatusId = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicantVacated).Id;
                            if (LeaseApplication.IsMaster) MatchingHelper.ChangeMasterStatus(_context, StatusId, master.Id);
                            if (!LeaseApplication.IsMaster) MatchingHelper.ChangeLeaseDetailsStatus(_context, StatusId, details.Id);
                            MatchingHelper.ChangeHumanStatus(_context, StatusId, LeaseApplication.Id);

                            ResponsibilityTypeId = _context.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.PendingTakeOff).Id;
                            //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id, true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            }
                            return RedirectToAction("AgreementTermination", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.PenndingTakeOffConfirmation) });
                        case RCSActionTypeKeys.NotVacated:
                            StatusId = _context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingVacatingConfirm).Id;
                            if (LeaseApplication.IsMaster) MatchingHelper.ChangeMasterStatus(_context, StatusId, master.Id);
                            if (!LeaseApplication.IsMaster) MatchingHelper.ChangeLeaseDetailsStatus(_context, StatusId, details.Id);
                            MatchingHelper.ChangeHumanStatus(_context, StatusId, LeaseApplication.Id);

                            ResponsibilityTypeId = _context.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.PendingTakeOff).Id;
                            //WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            if (((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator"))))
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id, true);
                            }
                            else
                            {
                                WorkAllocationHumanHelper.RoundRobinMarkJobAsFinished(_context, LeaseApplication.Id, ResponsibilityTypeId, Customer.Id);
                            }
                            return RedirectToAction("AgreementTermination", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.PenndingTakeOffConfirmation) });
                    }

                    Session["success"] = "display";
                    return RedirectToAction("Termination", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Search=" + true + "&Render=" + false + "&Id=0") });
                }
                catch (Exception Io)
                {
                    return RedirectToAction("LogOff", "Account");
                }
            }
        }

        [Authorize(Roles = "Housing Liaison Officer, Senior Housing Specialist, Caretaker, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult AgreementTermination(string ViewName)
        {
            using (var core = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var Keys = core.Status;
                    int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    int UserId = Customer.Id;
                    List<RoundRobinQueue> rrq = new List<RoundRobinQueue>();
                    var ResponsibilityTypeId = 0;
                    var list = new List<int?>();
                    var vm = new List<DepartmentsApprovalViewModel>();

                    if (ViewName == ViewCodeKeys.AgreementTermination)
                    {
                        ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.PendingTerminationreview).FirstOrDefault().Id;
                        //rrq = core.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                        if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                        {
                            rrq = core.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                        }
                        else
                        {
                            rrq = core.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                        }

                        list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();

                        var master = core.HumanSettlementLeaseMasters
                            .Include(r => r.HumanSettlementApplication.PurchaserType)
                            .Include(r => r.Status)
                            .Include(r => r.HumanSettlementApplication)
                            .Where(c => (c.Status.Key == StatusKeys.TenantDeceased || c.Status.Key == StatusKeys.EndOfLeaseTerm || c.Status.Key == StatusKeys.LeaseNotRenewed || c.Status.Key == StatusKeys.TenantNotice) && list.Contains(c.HumanSettlementApplicationId)).ToList();
                        var details = core.HumanSettlementLeaseDetails
                            .Include(r => r.HumanSettlementApplication.PurchaserType)
                            .Include(r => r.Status)
                            .Include(r => r.HumanSettlementApplication)
                            .Where(c => (c.Status.Key == StatusKeys.TenantDeceased || c.Status.Key == StatusKeys.EndOfLeaseTerm || c.Status.Key == StatusKeys.LeaseNotRenewed || c.Status.Key == StatusKeys.TenantNotice) && list.Contains(c.HumanSettlementApplicationId)).ToList();

                        ViewBag.ViewName = "Pending Account Review";

                        var referenceType = core.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                        var application = core.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                        foreach (var item in master)
                        {
                            var rr = new DepartmentsApprovalViewModel
                            {
                                HumanSettlementLeaseMaster = item,
                                HumanSettlementApplication = item.HumanSettlementApplication
                            };
                            vm.Add(rr);
                            item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                        }
                        foreach (var item in details)
                        {
                            var rr = new DepartmentsApprovalViewModel
                            {
                                HumanSettlementLeaseDetails = item,
                                HumanSettlementApplication = item.HumanSettlementApplication
                            };
                            vm.Add(rr);
                            item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                        }
                    }
                    else if (ViewName == ViewCodeKeys.PenndingTakeOffConfirmation)
                    {
                        ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.PendingTakeOff).FirstOrDefault().Id;
                        //rrq = core.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();

                        if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                        {
                            rrq = core.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                        }
                        else
                        {
                            rrq = core.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                        }

                        list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();

                        var master = core.HumanSettlementLeaseMasters
                            .Include(r => r.HumanSettlementApplication.PurchaserType)
                            .Include(r => r.Status)
                            .Include(r => r.HumanSettlementApplication)
                            .Where(c => c.Status.Key == StatusKeys.AwaitingVacatingConfirm && list.Contains(c.HumanSettlementApplicationId)).ToList();
                        var details = core.HumanSettlementLeaseDetails
                            .Include(r => r.HumanSettlementApplication.PurchaserType)
                            .Include(r => r.Status)
                            .Include(r => r.HumanSettlementApplication)
                            .Where(c => c.Status.Key == StatusKeys.AwaitingVacatingConfirm && list.Contains(c.HumanSettlementApplicationId)).ToList();
                        ViewBag.ViewName = "Awaiting Take-Off Confirmation";

                        var referenceType = core.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                        var application = core.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                        foreach (var item in master)
                        {
                            var rr = new DepartmentsApprovalViewModel
                            {
                                HumanSettlementLeaseMaster = item,
                                HumanSettlementApplication = item.HumanSettlementApplication
                            };
                            vm.Add(rr);
                            item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                        }
                        foreach (var item in details)
                        {
                            var rr = new DepartmentsApprovalViewModel
                            {
                                HumanSettlementLeaseDetails = item,
                                HumanSettlementApplication = item.HumanSettlementApplication
                            };
                            vm.Add(rr);
                            item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                        }
                    }



                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Logoff", "Account");
            }
        }

        //[Authorize(Roles = "Housing Liaison Officer, Senior Housing Specialist, Caretaker")]
        [DecryptParameter]
        public ActionResult PropertyLeaseInspections(string ViewName)
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
                    var ResponsibilityTypeId = 0;
                    var list = new List<int?>();
                    var vm = new List<DepartmentsApprovalViewModel>();

                    switch (ViewName)
                    {
                        case ViewCodeKeys.PostInspection:
                            ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.PostInspection).FirstOrDefault().Id;
                            //rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            if ((User.IsInRole("Super Administrators")) || (User.IsInRole("Back Office System Administrator")))
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }
                            else
                            {
                                rrq = cxt.RoundRobinQueues.Include(x => x.Status).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.ClerkId == UserId && x.Status.Key == StatusKeys.Submitted).ToList();
                            }

                            list = rrq.Select(x => x.HumanSettlementApplicationId).ToList();

                            var master = cxt.HumanSettlementLeaseMasters
                                .Include(r => r.HumanSettlementApplication.PurchaserType)
                                .Include(r => r.Status)
                                .Include(r => r.HumanSettlementApplication)
                                .Where(c => c.Status.Key == StatusKeys.AwaitingExitInspection && list.Contains(c.HumanSettlementApplicationId)).ToList();
                            var details = cxt.HumanSettlementLeaseDetails
                                .Include(r => r.HumanSettlementApplication.PurchaserType)
                                .Include(r => r.Status)
                                .Include(r => r.HumanSettlementApplication)
                                .Where(c => c.Status.Key == StatusKeys.AwaitingExitInspection && list.Contains(c.HumanSettlementApplicationId)).ToList();

                            var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                            var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                            foreach (var item in master)
                            {
                                var rr = new DepartmentsApprovalViewModel
                                {
                                    HumanSettlementLeaseMaster = item,
                                    HumanSettlementApplication = item.HumanSettlementApplication
                                };
                                vm.Add(rr);
                                item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                            }
                            foreach (var item in details)
                            {
                                var rr = new DepartmentsApprovalViewModel
                                {
                                    HumanSettlementLeaseDetails = item,
                                    HumanSettlementApplication = item.HumanSettlementApplication
                                };
                                vm.Add(rr);
                                item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                            }
                            
                            ViewBag.ViewFrom = " In Awaiting Exit Inspection";
                            ViewBag.ViewName = ViewCodeKeys.PostInspection;
                            break;
                        case ViewCodeKeys.PreInspection:
                            var apps2 = cxt.HumanSettlementApplications
                                .Include(db => db.PurchaserType)
                                .Include(db => db.Status)
                                .Where(a => a.Status.Key == StatusKeys.AwaitingUnitInspection).ToList();
                            foreach (var app in apps2)
                            {
                                app.Data = new AesCrypto().Encrypt("ViewName=" + ViewName);
                                var rr = new DepartmentsApprovalViewModel
                                {
                                    HumanSettlementApplication = app
                                };
                                vm.Add(rr);
                            }
                            ViewBag.ViewFrom = " In Awaiting Pre-Inspection";
                            ViewBag.ViewName = ViewCodeKeys.PreInspection;
                            break;
                        case ViewCodeKeys.MaintananceJobSheet:
                            var apps3 = cxt.HumanSettlementApplications
                              .Include(db => db.PurchaserType)
                              .Include(db => db.Status)
                              .Where(a => a.Status.Key == StatusKeys.AwaitingMaintananceJobSheet && !a.IsMaster).ToList();
                            foreach (var app in apps3)
                            {
                                app.Data = new AesCrypto().Encrypt("ViewName=" + ViewName);
                                var rr = new DepartmentsApprovalViewModel
                                {
                                    HumanSettlementApplication = app
                                };
                                vm.Add(rr);
                            }
                            ViewBag.ViewFrom = " In Awaiting Maintanance Job Sheet";
                            ViewBag.ViewName = ViewCodeKeys.MaintananceJobSheet;
                            break;
                        case ViewCodeKeys.ScheduleInspectionSlots:
                            var apps = cxt.HumanSettlementApplications
                               .Include(db => db.PurchaserType)
                               .Include(db => db.Status)
                               .Where(a => a.Status.Key == StatusKeys.AwaitingInspectionScheduleSlots && !a.IsMaster).ToList();
                            foreach (var app in apps)
                            {
                                app.Data = new AesCrypto().Encrypt("ViewName=" + ViewName);
                                var rr = new DepartmentsApprovalViewModel
                                {
                                    HumanSettlementApplication = app
                                };
                                vm.Add(rr);
                            }
                            ViewBag.ViewFrom = " In Awaiting Inspection Shcedule";
                            ViewBag.ViewName = ViewCodeKeys.ScheduleInspectionSlots;
                            break;
                    }
                    if (Session["NoticeSuccess"] != null)
                        ViewBag.PostSuccess = "true";
                    Session["NoticeSuccess"] = null;


                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return View("_Error");
            }
        }


        #region Human Application Or Lease Evictions

        //[Authorize]
        [DecryptParameter]
        public ActionResult HumanApplicationEvictions(string ViewName, int? ApplicationId)
        {
            try
            {
                Initialise();
                var vm = new DepartmentsApprovalViewModel();
                var core = new eServicesDbContext();
                var HumanApplication = new HumanSettlementApplication();
                var customer = new Customer();
                var application = new Application();
                var documentReferenceType = new ReferenceType();
                var referenceType = new ReferenceType();
                //Required ViewModels For displaying
                var dvm = new DocumentsViewModel();
                if (ApplicationId != null)
                {
                    HumanApplication = core.HumanSettlementApplications.Include(r => r.CreatedBySystemUser)
                                         .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                                         .Include(r => r.PurchaserType).Include(r => r.Status)
                                         .Where(x => x.Id == ApplicationId).FirstOrDefault();

                    customer = core.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                       .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == HumanApplication.CustomerId);
                    if (customer == null) throw new Exception("Invalid Customer");

                    application = core.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                    documentReferenceType = core.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSUpload);
                    if (documentReferenceType == null)
                        throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                            ReferenceTypeKeys.RCSUpload));

                    // Documents required variables
                    referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                    if (application == null) throw new Exception("Invalid application.");
                    if (referenceType == null) throw new Exception("Invalid reference type.");
                }
                switch (ViewName)
                {
                    case ViewCodeKeys.CaptureApplicationEviction:
                        if (Session["Display"] != null)
                        {
                            Session["Display"] = null;
                            ViewBag.DisplaySwal = "True";
                            if (Session["Message"] != null)
                            {
                                ViewBag.MessageBodySwal = Session["Message"].ToString();
                                Session["Message"] = null;
                            }
                        }
                        else
                        {
                            ViewBag.DisplaySwal = null;
                        }
                        vm.LeaseDetails = new LeaseDetails();
                        return View("ApplicationEviction", vm);
                    case ViewCodeKeys.CaptureEvictionDetails:
                        ViewBag.ApprovalStatus = new SelectList(core.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.NonPayment
                        || x.Key == RCSActionTypeKeys.NonCompliance || x.Key == RCSActionTypeKeys.Subletting
                        || x.Key == RCSActionTypeKeys.IllegalActivities).OrderBy(x => x.Name), "Key", "Name");

                        MatchingHelper.DocumentPropertyEvictionValidationHuman(dvm, core, customer.Id, customer.Id, referenceType.Id, application.Id, "", HumanApplication.Id, true);
                        vm.DocumentsViewModel = dvm;
                        vm.HumanSettlementApplication = HumanApplication;

                        return View("CaptureEvictionDetails", vm);
                    case ViewCodeKeys.EvictionCommitteOutcomeList:
                        var plmApplicationList = core.HumanSettlementApplications.Where(x => (x.Status.Key == StatusKeys.NonCompliance || x.Status.Key == StatusKeys.NonPayment || x.Status.Key == StatusKeys.Subletting || x.Status.Key == StatusKeys.IllegalActivities))
                            .Include(r => r.CreatedBySystemUser)
                            .Include(r => r.PurchaserType)
                            .Include(r => r.Customer)
                            .Include(r => r.ModifiedBySystemUser)
                            .Include(r => r.Status).ToList();
                        return View("CommitteeEvictionsOutcomes", plmApplicationList);
                    case ViewCodeKeys.EvictionCommitteOutcomes:
                        MatchingHelper.DocumentEvictionOutcomeSupportingDoc(dvm, core, customer.Id, customer.Id, referenceType.Id, application.Id, "", HumanApplication.Id, true);
                        vm.DocumentsViewModel = dvm;
                        vm.HumanSettlementApplication = HumanApplication;
                        vm.HumanSettlementLeaseDetails = (!HumanApplication.IsMaster) ? core.HumanSettlementLeaseDetails.FirstOrDefault(d => d.HumanSettlementApplicationId == HumanApplication.Id) : null;
                        vm.HumanSettlementLeaseMaster = (HumanApplication.IsMaster) ? core.HumanSettlementLeaseMasters.FirstOrDefault(d => d.HumanSettlementApplicationId == HumanApplication.Id) : null;
                        ViewBag.ApprovalStatus = new SelectList(core.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                        return View("CaptureCommitteeOutcome", vm);
                       
                }

                return View("_Error");
            }
            catch (Exception Error)
            {
                EventLogHelper.LogSystemError(Error.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult ApplicationEviction(string ApprovalStatusddl)
        {
            try
            {
                using (var _context = new eServicesDbContext())
                {
                    var application = _context.HumanSettlementApplications.FirstOrDefault(x => x.ApplicationReferenceNumber == ApprovalStatusddl);
                    if (application != null)
                    {
                        //return RedirectToAction("CaptureEvictionDetails", new { q = new AesCrypto().Encrypt("ApplicationId=" + application.Id.ToString()) });
                        return RedirectToAction("HumanApplicationEvictions", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.CaptureEvictionDetails + "&ApplicationId=" + application.Id) });
                    }
                    else
                    {
                        Session["Message"] = "The Reference Number " + ApprovalStatusddl + " Did Not Fid A Match, Try Again!";
                        Session["Display"] = "Display";
                        return RedirectToAction("HumanApplicationEvictions", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.CaptureApplicationEviction) });
                    }
                }
            }
            catch (Exception Error)
            {
                EventLogHelper.LogSystemError(Error.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult CaptureEvictionDetails(int ApplicationId, string ApprovalStatusddl)
        {
            try
            {
                using (var core = new eServicesDbContext())
                {
                    _base.Initialise(core);
                    var Keys = core.Status.ToList();
                    var ActivityTrackerMessage = string.Empty;
                    var HumanApplication = core.HumanSettlementApplications.Where(x => x.Id == ApplicationId).Include(x => x.Customer).Include(x => x.PurchaserType).FirstOrDefault();
                    var customer = core.Customers.FirstOrDefault(a => a.Id == HumanApplication.CustomerId);
                    if (customer == null)
                        throw new Exception(String.Format("Incvalid customer, ref - {0}", HumanApplication.ApplicationReferenceNumber));
                    switch (ApprovalStatusddl)
                    {
                        case RCSActionTypeKeys.NonPayment:
                            var NonPayment = Keys.FirstOrDefault(x => x.Key == StatusKeys.NonPayment);
                            MatchingHelper.ChangeHumanStatus(core, NonPayment.Id, HumanApplication.Id);
                            ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EvictionCapture).Description.Replace("{#}", NonPayment.Name);
                            break;
                        case RCSActionTypeKeys.NonCompliance:
                            var NonCompliance = Keys.FirstOrDefault(x => x.Key == StatusKeys.NonCompliance);
                            MatchingHelper.ChangeHumanStatus(core, NonCompliance.Id, HumanApplication.Id);
                            ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EvictionCapture).Description.Replace("{#}", NonCompliance.Name);
                            break;
                        case RCSActionTypeKeys.Subletting:
                            var Subletting = Keys.FirstOrDefault(x => x.Key == StatusKeys.Subletting);
                            MatchingHelper.ChangeHumanStatus(core, Subletting.Id, HumanApplication.Id);
                            ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EvictionCapture).Description.Replace("{#}", Subletting.Name);
                            break;
                        case RCSActionTypeKeys.IllegalActivities:
                            var IllegalActivities = Keys.FirstOrDefault(x => x.Key == StatusKeys.IllegalActivities);
                            MatchingHelper.ChangeHumanStatus(core, IllegalActivities.Id, HumanApplication.Id);
                            ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EvictionCapture).Description.Replace("{#}", IllegalActivities.Name);
                            break;
                    }
                    MatchingHelper.ActivityTrackerHuman(core, HumanApplication.Id, ActivityTrackerMessage, customer.Id);
                    int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.EvictionCapture).Id;
                    EmailHelper.CustomerEmailOrSMSNotification(core, HumanApplication.Id, emailboodyId);
                    return RedirectToAction("HumanApplicationEvictions", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("ViewName=" + ViewCodeKeys.CaptureApplicationEviction) });
                }
            }
            catch (Exception Error)
            {
                EventLogHelper.LogSystemError(Error.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
            }
        }


        [DecryptParameter]
        [Authorize]
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
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EvictionCommitteeActionsTermination).Description.ToString() + LL;
                    MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                }
                else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
                {
                    terminate.StatusId = (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminationRejected).Id;
                    _context.Entry(terminate).State = EntityState.Modified;
                    _context.SaveChanges();

                    MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminationRejected).Id, (int)LeaseId);

                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EvictionCommitteeActionsTermination).Description.ToString() + LL;
                    MatchingHelper.ActivityTrackerAudit(db, LeaseApplication.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                }
                return RedirectToAction("PropertyLeaseApplicationTerminations");

            }


        }




        #endregion





        #region WaitingListRe-Wail OnLoad
        [DecryptParameter]
        public ActionResult WaitingListValidation(int ApplicationId)
        {
            Initialise();
            var context = new eServicesDbContext();
            var rcsApps = context.HumanSettlementApplications.Include(r => r.CreatedBySystemUser)
              .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
              .Include(r => r.Status).Include(r => r.PurchaserType)
              .Where(x => x.Id == ApplicationId).FirstOrDefault();

            var vm = new DepartmentsApprovalViewModel
            {
                HumanSettlementApplication = rcsApps
            };

            ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.PlmYes || x.Key == RCSActionTypeKeys.PlmNo).OrderByDescending(x => x.Name), "Key", "Name");

            return View(vm);
        }
        #endregion

        #region WaitingListRe-Wail Validation
        [DecryptParameter]
        [HttpPost]
        public ActionResult WaitingListValidation(int ApplicationId, string ApprovalStatusddl)
        {
            using (var _context = new eServicesDbContext())
            {
                Initialise();
                if (ApprovalStatusddl == RCSActionTypeKeys.PlmYes)
                {
                    var findItem = _context.WaitingListQueueHumans.FirstOrDefault(x => x.HumanSettlementApplicationId == ApplicationId);
                    findItem.IsReListed = false;
                    findItem.QueueDate = DateTime.Now;
                    _context.Entry(findItem).State = EntityState.Modified;
                    _context.SaveChanges();
                    MatchingHelper.ChangeHumanStatus(_context, _context.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingUnitOffers).Id, ApplicationId);
                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.WaitingListReEntry).Description.ToString();
                    MatchingHelper.ActivityTrackerHuman(_context, ApplicationId, ActivityTrackerMessage, custmusers.Id);
                    MatchingHelper.MarkQueueAsRelisted2(_context, ApplicationId);
                    int emailboodyId = _context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.plm_re_list_to_queue).Id;
                    EmailHelper.CustomerEmailOrSMSNotification(_context, ApplicationId, emailboodyId);
                    //MatchingHelper.MatchUnitParallelProcessor(_context);
                }
                else if (ApprovalStatusddl == RCSActionTypeKeys.PlmNo)
                {
                    int QueueId = _context.WaitingListQueueHumans.FirstOrDefault(x => x.HumanSettlementApplicationId == ApplicationId).Id;
                    MatchingHelper.HumanRemoveApplicationFromWaitingList(_context, QueueId);
                    MatchingHelper.ChangeHumanStatus(_context, _context.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicationDiscardedNoUnitAvailable).Id, ApplicationId);
                    var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.WaitingListExit).Description.ToString();
                    MatchingHelper.ActivityTrackerHuman(_context, ApplicationId, ActivityTrackerMessage, custmusers.Id);
                    int emailboodyId = _context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.plm_remove_from_queue).Id;
                    EmailHelper.CustomerEmailOrSMSNotification(_context, ApplicationId, emailboodyId);
                }
                return RedirectToAction("Inbox");
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


        [Authorize(Roles = "Caretaker, Housing Liaison Officer, Super Administrators, Back Office System Administrator")]
        [DecryptParameter]
        public ActionResult ConfirmVacatingAppicant(int ApplicatoinId)
        {
            try
            {
                Initialise();
                var core = new eServicesDbContext();
                var vm = new DepartmentsApprovalViewModel();
                var LeaseApplication = core.HumanSettlementApplications.FirstOrDefault(x => x.Id == ApplicatoinId && x.IsDeleted == false);
                var master = core.HumanSettlementLeaseMasters
                    .Include(r => r.HumanSettlementApplication.PurchaserType)
                    .Include(r => r.Status)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(c => c.HumanSettlementApplicationId == LeaseApplication.Id && c.IsActive).FirstOrDefault();
                var details = core.HumanSettlementLeaseDetails
                    .Include(r => r.HumanSettlementApplication.PurchaserType)
                    .Include(r => r.Status)
                    .Include(r => r.HumanSettlementApplication)
                    .Where(c => c.HumanSettlementApplicationId == LeaseApplication.Id && c.IsActive).FirstOrDefault();

                vm.HumanSettlementApplication = master?.HumanSettlementApplication ?? details?.HumanSettlementApplication;
                vm.HumanSettlementLeaseMaster = master;
                vm.HumanSettlementLeaseDetails = details;
                ViewBag.ApprovalStatus = new SelectList(core.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Vacated || x.Key == RCSActionTypeKeys.NotVacated).OrderBy(x => x.Name), "Key", "Name");


                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //throw;
                return RedirectToAction("Logoff", "Account");
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
                string attorneyemail = "siyanda.ngxonga@xetgroup.com" /*BackOfficeClerk.EmailAddress*/;
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
            //DateTime.Compare(x.DateTimeValueColumn.Date, DateTime.Now.Date) <= 0
            //x => EntityFunctions.TruncateTime(x.DateTimeStart) == currentDate.Date
            //try
            //{
            //    var dates = new string[2];
            //    dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            //    dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            //    var startDate = DateTime.Parse(dates[0]).Date;
            //    var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            //    var oneDayTime = endDate - startDate;
            //    var NumOfDDAppPending = db.RoundRobinQueues.Where(x => x.IsActive == true && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();

            //    //DateTime DateFrom = DateTime.Now;
            //    //DateTime DateTo = DateTime.Now;
            //    //var startDate = DateTime.TryParseExact(DateFrom, "yyyyMMdd");
            //    //endDate = DateTime.ParseExact(dates[1], "yyyyMMdd").AddTicks(-1).AddDays(1);
            //    //var RoundRobingQueue1 = db.RoundRobinQueues.Where(x => DbFunctions.TruncateTime(x.CreatedDateTime) == DateTime.Now).ToList();

            //    //var NumOfDDAppPending = db.RoundRobinQueues.Where(x => x.IsActive == true && (DateFrom <= x.CreatedDateTime && DateTo >= x.CreatedDateTime)).ToList();

            //}
            //catch (Exception IO)
            //{

            //}
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


                // JK.20140724a - Custom profile information.

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
    }
}
