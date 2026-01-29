using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Services.Protocols;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.DataAccessLayer.eBilling6Months;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Microsoft.AspNet.Identity;
using Microsoft.SqlServer.ReportExecution;
using OfficeOpenXml;
using Microsoft.AspNet.Identity.EntityFramework;
using PdfSharp.Pdf.Security;
using Document = iTextSharp.text.Document;
using PdfDocument = PdfSharp.Pdf.PdfDocument;
using PdfReader = PdfSharp.Pdf.IO.PdfReader;
using Microsoft.Reporting.WebForms;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class RCSReportController : Controller
    {
        //eServicesDbContext _eServices = new eServicesDbContext();
        //eBilling6MonthsDbContext _eBilling6Months = new eBilling6MonthsDbContext();
        //private ValueAssistDbContext _valueAssist = new ValueAssistDbContext();
        //CesarDbContext _Cesar = new CesarDbContext();

        public RCSReportController()
       : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }
        public RCSReportController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }
        public RCSReportController(eServicesDbContext db)
        {
            UserManager =
            new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(db));
        }
        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }
        public byte[] Buffer { get; set; }

        public class IncentivePolicyStats
        {
            public int NoOfApplication { get; set; }
            public int NoOfSubmittedApplications { get; set; }
            public int NoOfInProgressApplications { get; set; }
            public int NoOfApplicationsInQuery { get; set; }
            public int NoOfSubmittedForAssessment { get; set; }
            public int NoOfApplicationApprovedAssessment { get; set; }
            public int NoOfApplicationDeclinedAssessment { get; set; }
        }

        #region Report Init
        private void Initialise()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    IdentityManager = new IdentityManager(context);

                    if (User != null && User.Identity.IsAuthenticated)
                    {
                        IdentityManager.CurrentUser(User);
                        SystemUser = IdentityManager.CurrentUser(User);
                    }

                    if (SystemUser != null)
                    {
                        Customer =
                            context.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                                .Include(o => o.CustomerType)
                                .Include(o => o.Country)
                                .Include(o => o.IdentificationType)
                                .Include(o => o.TitleType)
                                .FirstOrDefault();

                        if (Customer != null)
                        {
                            Entity =
                                context.Entities.Where(o => o.CustomerId == Customer.Id)
                                    .Include(o => o.EntityType)
                                    .FirstOrDefault();
                            CustomerId = Customer.Id;
                        }

                    }

                    if (Customer != null)
                    {
                        Agent = context.Agents.FirstOrDefault(o => o.CustomerId == Customer.Id);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }



        public UserManager<SystemIdentityUser> UserManager { get; private set; }
        #endregion

        #region Report Online Bills GET
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult OnlineBills()
        {
            try
            {
                Initialise();
                if (Customer == null)
                    return RedirectToAction("Index", "Profile");

                var isAgent = Customer != null && Customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent;

                ViewBag.Message = LinkedAccountHelper.LinkedAccountNotification(false);
                ViewBag.IsAgent = isAgent;
                return View();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #region Report Back Office Doc View
        // [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DocView()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        [EncryptedActionParameter]
        public ActionResult GenerateDocView()
        {
            //NetworkCredential nwc = new NetworkCredential("Ekurhuleni\nataliec", "Louella@2016");
            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            AppSetting genBillLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenBillLink);
            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value, AdDom.Value);

            WebClient client = new WebClient();
            client.Credentials = nwc;
            //string yearD = _billDate.Substring(0, 4);
            //string monthD = _billDate.Substring(5, 2);
            //string accNo = accountNumber.Replace("z", string.Empty); // to take the z away from the account number after forcing it into a string -- see LoadOnlineBils


            //string yearN = yearD;
            //string month = monthD;
            string reportURL = genBillLink.Value;
            return File(client.DownloadData(reportURL), "application/pdf");
        }


        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum Type
        {
            WORD,
            EXCEL,
            PDF
        }
        #region ReturnValues
        public string MimeType { get; set; }
        public byte[] Content { get; set; }
        public string FileExtension
        {
            get
            {
                string extension;
                switch (ReportType)
                {
                    case Type.EXCEL:
                        extension = "xls";
                        break;

                    case Type.WORD:
                        extension = "doc";
                        break;

                    default:
                        extension = "pdf";
                        break;
                }

                return extension;
            }
        }
        #endregion
        #region InternalValues
        private string Path { get; set; }
        private string DataSetName { get; set; }
        private IEnumerable DataSetSource { get; set; }
        private Type ReportType { get; set; }
        #endregion

        public ActionResult Reports()
        {
            using (var cxt = new eServicesDbContext())
            {
                string l = "License";
                ViewBag.Reports = new SelectList(cxt.ReportSettings.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Key", "Name");
                ViewBag.RegionType = new SelectList(cxt.CCCs.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "CCCName");
                ViewBag.StatusType = new SelectList(cxt.Status.Where(o => o.IsActive && !o.IsDeleted && o.StatusTypeId==20 ).ToList(), "Id", "Name");
                ViewBag.Rejected= new SelectList(cxt.Status.Where(o => o.IsActive && !o.IsDeleted && o.Key== "s_rcs_Disprove" || o.Key== "s_rcs_Cancelled").ToList(), "Id", "Name");

                List<SelectListItem> items = new List<SelectListItem>();
                items.Add(new SelectListItem() { Text = "Excel", Value = "EXCELOPENXML" });
                items.Add(new SelectListItem() { Text = "Word", Value = "WORDOPENXML" });
                items.Add(new SelectListItem() { Text = "PDF", Value = "PDF" });
                ViewBag.DocTypes = new SelectList(items, "Value", "Text");
            }
            return View();
        }

        public ActionResult getpdfReportORG(string cusAcc, string StateYear, string stateMonth)
        {

            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            AppSetting genBillLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenBillLink);
            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string accNo = cusAcc;
            string yearN = StateYear;
            string month = stateMonth;
            string reportURL = "http://10.1.2.212/ReportServer_SOLARERP/Pages/ReportViewer.aspx?%2fCustomer_Bill%2fCustomer_Bill_View&rs:Command=Render&rs:Format=PDF&customer=" + accNo + "&year=" + yearN + "&month=" + month;
            return File(client.DownloadData(reportURL), "application/pdf");


            //LocalReport lr = new LocalReport
            //{
            //    ReportPath = "http://10.1.2.212/ReportServer_SOLARERP?%2fCustomer_Bill%2fCustomer_Bill_View&rs:Command=Render"
            //};

            //ReportDataSource reportDataSource = new ReportDataSource(DataSetName, DataSetSource);
            //lr.DataSources.Add(reportDataSource);

            //string deviceInfo = $@"<DeviceInfo>
            //                          <OutputFormat>PDF</OutputFormat>
            //                          <PageWidth>8.5in</PageWidth>
            //                          <PageHeight>11in</PageHeight>
            //                          <MarginTop>0.5in</MarginTop>
            //                          <MarginLeft>1in</MarginLeft>
            //                          <MarginRight>1in</MarginRight>
            //                          <MarginBottom>0.5in</MarginBottom>
            //                       </DeviceInfo>";

            //string mimeType;
            //string encoding;
            //string fileNameExtension;
            //Microsoft.Reporting.WebForms.Warning[] warnings;
            //string[] streams;

            //var content = lr.Render(
            //                    ReportType.ToString(),
            //                    deviceInfo,
            //                    out mimeType,
            //                    out encoding,
            //                    out fileNameExtension,
            //                    out streams,
            //                    out warnings);

            //MimeType = mimeType;

            //return File(content, report.MimeType, $"ReportDataSet_{DateTime.Now.ToString("yyyy-MM-dd-HH'h'mm")}.{report.FileExtension}");
        }

        #region Request Access to Account Number POST
        [HttpPost]
        //[Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult getpdfReport(string cusAcc, string StateYear, string stateMonth)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {

                    AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
                    AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
                    AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
                    AppSetting genBillLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenBillLink);
                    NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value);
                    WebClient client = new WebClient();
                    client.Credentials = nwc;
                    string accNo = cusAcc;
                    string yearN = StateYear;
                    string month = stateMonth;
                    string reportURL = genBillLink.Value + accNo + "&year=" + yearN + "&month=" + month;
                    File(client.DownloadData(reportURL), "application/pdf");

                    return Json(new { isValid = true }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                }

            }

        }
        #endregion
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult GeneratePDF(string cusAcc, string StateYear, string stateMonth)
        {
            //NetworkCredential nwc = new NetworkCredential("Ekurhuleni\nataliec", "Louella@2016");
            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            AppSetting genBillLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenBillLink);
            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value, AdDom.Value);

            WebClient client = new WebClient();
            client.Credentials = nwc;
            string yearD = StateYear;
            string monthD = stateMonth;
            string accNo = cusAcc;
            string fileName = "" + yearD + "-" + monthD + "_" + accNo + "_Statement.pdf";
            string yearN = yearD;
            string month = monthD;
            string reportURL = genBillLink.Value + accNo + "&year=" + yearN + "&month=" + month;
            return File(client.DownloadData(reportURL), "application/pdf", fileName);
        }
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        [EncryptedActionParameter]
        public ActionResult GenerateBill(string accountNumber, string _billDate, bool isCopy = false, bool isResend = false)
        {
            //NetworkCredential nwc = new NetworkCredential("Ekurhuleni\nataliec", "Louella@2016");
            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            AppSetting genBillLink = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adGenBillLink);

            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value, AdDom.Value);

            WebClient client = new WebClient();
            client.Credentials = nwc;
            string yearD = _billDate.Substring(0, 4);
            string monthD = _billDate.Substring(5, 2);
            string accNo = accountNumber.Replace("z", string.Empty); // to take the z away from the account number after forcing it into a string -- see LoadOnlineBils


            string yearN = yearD;
            string month = monthD;
            string reportURL = genBillLink.Value + accNo + "&year=" + yearN + "&month=" + month;
            return File(client.DownloadData(reportURL), "application/pdf");
        }

        //___________________________________________Report Dashboard_______________________________________________________//

        // [Authorize(Roles = "Administrators" + "," + "Super Administrators")]
        public ActionResult ReportViewer()
        {
            using (var cxt = new eServicesDbContext())
            {
                ViewBag.Reports = new SelectList(cxt.ReportSettings.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Key", "Name");
                //ViewBag.RegionType = new SelectList(cxt.Regions, "RegionName", "RegionName");
                //ViewBag.StatusType = new SelectList(cxt.Status, "Key", "Name");

                List<SelectListItem> items = new List<SelectListItem>();
                items.Add(new SelectListItem() { Text = "Excel", Value = "EXCELOPENXML" });
                items.Add(new SelectListItem() { Text = "Word", Value = "WORDOPENXML" });
                items.Add(new SelectListItem() { Text = "PDF", Value = "PDF" });
                ViewBag.DocTypes = new SelectList(items, "Value", "Text");
            }
            return View();
        }
        // [Authorize(Roles = "Administrators" + "," + "Super Administrators")]
        public ActionResult ReportDashboard()
        {

            return View();
        }
        //  [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult GetConsumption(string meterType, string accountNumber, string startDate, string endDate)
        {
            if (meterType == null && accountNumber == null && startDate == null && endDate == null)
                return Json(string.Empty);

            // TODO: Move to AppSettings.
            string json =
                "{ " +
                    "\"serviceProperties\" : { " +
                        "\"systemCode\" : \"XET\", \"key\" : \"1234\" }, " +
                    "\"serviceRequest\" : { " +
                        "\"assetType\" : \"meter\", " +
                        "\"searchType\" : \"meterConsumption\", " +
                        "\"searchCriteria\" : [ " +
                            "{ \"name\" : \"meterType\", \"value\" : \"" + meterType + "\" }, " +
                            "{ \"name\" : \"accountNumber\", \"value\" : \"" + accountNumber + "\" },  " +
                            "{ \"name\" : \"startDate\", \"value\" : \"" + Convert.ToDateTime(startDate).ToString("yyyyMMdd") + "\" },  " +
                            "{ \"name\" : \"endDate\", \"value\" : \"" + Convert.ToDateTime(endDate).ToString("yyyyMMdd") + "\" } ] } } ";

            WebHelper webHelper = new C8.eServices.Mvc.Helpers.WebHelper();
            var assetUrl = new AppSetting();

            using (var cxt = new eServicesDbContext())
            {
                assetUrl = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.MeterConsumptionUrl);
            }

            Uri url = new Uri(string.Format(assetUrl.Value + HttpUtility.UrlEncode(json)));

            string response = webHelper.Get(url, json);
            dynamic jo = JObject.Parse(response);
            string consumption = "[";
            string reading, d;
            string meterNo = jo.assetProperties.meterProperties[1].meterIdentifiers.meterNumber;
            DateTime date;

            foreach (dynamic mc in jo.assetProperties.meterProperties[1].meterConsumption)
            {
                d = mc["date"].ToString();
                date = Convert.ToDateTime(d.Substring(0, 4) + "-" + d.Substring(4, 2) + "-" + d.Substring(6, 2));
                reading = mc["consumption"].ToString();
                consumption += "{ \"label\": \"" + meterNo + ": " + date.ToString("yyyy-MM-dd") + "\", \"y\": " + reading + "},";
            }
            consumption = consumption.Substring(0, consumption.Length - 1);
            consumption += "]";

            if (response != null)
            {
                //Handle your reponse here
                return Json(consumption);
            }
            else
            {
                //No Response from the server
                System.Diagnostics.Debug.WriteLine(response);
                return Json("No data available.");
            }
        }
        // [Authorize(Roles = "Administrators" + "," + "Super Administrators")]
        //[EncryptedActionParameter]
        public ActionResult GenerateDateRangeReport(string startDate, string endDate, string reportId, string docTypeId, string RegionType, string StatusType, string Rejected)
        {
            //NetworkCredential nwc = new NetworkCredential("Ekurhuleni\nataliec", "Louella@2016");
            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            ReportSetting genDateRangeReport = context.ReportSettings.FirstOrDefault(o => o.Key == reportId);

            var apporvedstatus = context.Status.Where(x => x.Key == "s_rcs_approved").FirstOrDefault();
            var rccissuedstatus= context.Status.Where(x => x.Key == "s_rcs_RCCIssued").FirstOrDefault();
            string rejectedid = Rejected;


            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value, AdDom.Value);

            WebClient client = new WebClient();
            client.Credentials = nwc;
            DateTime StartDate = Convert.ToDateTime(startDate);
            DateTime EndDate = Convert.ToDateTime(endDate);
            var ExeTime = DateTime.Now;
            string docType = "";
            string docApplicationType = "";
            switch (docTypeId)
            {
                case "PDF":
                    docType = ".pdf";
                    docApplicationType = "application/pdf";
                    break;

                case "WORDOPENXML":
                    docType = ".docx";
                    docApplicationType = "application/msword";
                    break;

                default:
                    docType = ".xlsx";
                    docApplicationType = "application/ms-excel";
                    break;
            }


            string fileName = "PLM" + genDateRangeReport.Name + " Report_" + ExeTime + docType;
            string reportURL = "";
            
            

             if (genDateRangeReport.Key == ReportTypeKeys.drr_acknowledgments_report)
            {
                reportURL = genDateRangeReport.Value + ":Format=" + docTypeId + "&SD=" + StartDate + "&ED=" + EndDate + "&Status=" + apporvedstatus.Id; //R1
            }
            else if (genDateRangeReport.Key == ReportTypeKeys.rdrr_rejected_applications_report)
            {
                reportURL = genDateRangeReport.Value + ":Format=" + docTypeId + "&SD=" + StartDate + "&ED=" + EndDate + "&Status=" + rejectedid; //R2
            }
            else if (genDateRangeReport.Key == ReportTypeKeys.drr_application_fees_report)
            {
                reportURL = genDateRangeReport.Value + ":Format=" + docTypeId + "&SD=" + StartDate + "&ED=" + EndDate; //R3
            }
            else if (genDateRangeReport.Key == ReportTypeKeys.drr_rcc_issued_report)
            {
                reportURL = genDateRangeReport.Value + ":Format=" + docTypeId + "&SD=" + StartDate + "&ED=" + EndDate + "&Status=" + rccissuedstatus.Id ; //R4
            }
            else if (genDateRangeReport.Key == ReportTypeKeys.drr_rcc_turnaround_report)
            {
                reportURL = genDateRangeReport.Value + ":Format=" + docTypeId + "&SD=" + StartDate + "&ED=" + EndDate; //R5
            }
            else if (genDateRangeReport.Key == ReportTypeKeys.sdrr_rcs_status_report)
            {
                reportURL = genDateRangeReport.Value + ":Format=" + docTypeId + "&SD=" + StartDate + "&ED=" + EndDate + "&Status="+StatusType; //R5
            }
            else if (genDateRangeReport.Key == ReportTypeKeys.drr_plm_vacancy_report)
            {
                reportURL = genDateRangeReport.Value + ":Format=" + docTypeId + "&SD=" + StartDate + "&ED=" + EndDate/* + "&Status="+StatusType*/; //R5
            }
            return File(client.DownloadData(reportURL), docApplicationType, fileName);
        }
        // [Authorize(Roles = "Administrators" + "," + "Super Administrators")]
        //[EncryptedActionParameter]
        public ActionResult GenerateDetailedReport(string reportId, string docTypeId)
        {
            //NetworkCredential nwc = new NetworkCredential("Ekurhuleni\nataliec", "Louella@2016");
            eServicesDbContext context = new eServicesDbContext();
            AppSetting AdUser = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdUserName);
            AppSetting AdPass = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AdPassword);
            AppSetting AdDom = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.adDomain);
            ReportSetting genDetailedLink = context.ReportSettings.FirstOrDefault(o => o.Key == reportId);

            NetworkCredential nwc = new NetworkCredential(AdUser.Value, AdPass.Value, AdDom.Value);
            var ExeTime = DateTime.Now;
            string docType = "";
            string docApplicationType = "";
            switch (docTypeId)
            {
                case "PDF":
                    docType = ".pdf";
                    docApplicationType = "pdf";
                    break;

                case "WORDOPENXML":
                    docType = ".docx";
                    docApplicationType = "msword";
                    break;

                default:
                    docType = ".xlsx";
                    docApplicationType = "ms-excel";
                    break;
            }

            string fileName = "Siyakhokha " + genDetailedLink.Name + " Report_" + ExeTime + docType;
            WebClient client = new WebClient();
            client.Credentials = nwc;
            //string accNo = accountNumber.Replace("z", string.Empty); // to take the z away from the account number after forcing it into a string -- see LoadOnlineBils
            string reportURL = genDetailedLink.Value + ":Format=" + docTypeId;
            return File(client.DownloadData(reportURL), docApplicationType, fileName);
        }

    }
}