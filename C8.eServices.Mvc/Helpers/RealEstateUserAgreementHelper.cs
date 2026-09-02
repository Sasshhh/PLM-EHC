using System;
using System.IO;
using System.Diagnostics;
using System.Web;
using System.Web.Hosting;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Helpers
{
    /* =================================================================================================================
     * ARCHITECTURAL & BUSINESS RULE LIFECYCLE COMMENT (EXACT REPLICA ENGINE WITH PERFECT 180px BALANCED LOGO):
     * -----------------------------------------------------------------------------------------------------------------
     * 1. UC21 (Permission to Occupy - PTO): 12 MONTHS validity.
     *    - Department: DEVELOPMENT PLANNING & REAL ESTATE DEPARTMENT.
     *    - Banking / Payment Clause 3.2: Direct CoE Lease income account (Standard Bank Acc: 281181578, Vote No: 17901402870MHZZZZZ16).
     * 
     * 2. UC23 (Full Lease Agreement): 36 MONTHS validity (3 Years).
     *    - Department: ECONOMIC DEVELOPMENT DEPARTMENT / DPRE.
     *    - Payment Clause 3.2: Ulwazi Resource Consulting (URC) Strategic Partnership rental offset.
     * 
     * 🔄 BACK-OFFICE LIFECYCLE TRANSITION:
     *    - When Back-Office completes processing & approves the 36-month Lease (UC23), the system flags the 12-month 
     *      PTO Certificate as Superseded/Inactive (IsActive = false).
     * ================================================================================================================= */

    public static class RealEstateUserAgreementHelper
    {
        private static readonly string EdgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";

        private static string GetCoELogoBase64()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] possiblePaths = new string[]
                {
                    HostingEnvironment.IsHosted ? HostingEnvironment.MapPath("~/Content/Images/CoE Logo .png") : null,
                    HostingEnvironment.IsHosted ? HostingEnvironment.MapPath("~/Content/siyakhokhaStyles/COELogo.png") : null,
                    System.IO.Path.Combine(baseDir, "Content", "Images", "CoE Logo .png"),
                    System.IO.Path.Combine(baseDir, "Content", "siyakhokhaStyles", "COELogo.png"),
                    System.IO.Path.Combine(baseDir, "..", "Content", "Images", "CoE Logo .png"),
                    System.IO.Path.Combine(baseDir, "..", "Content", "siyakhokhaStyles", "COELogo.png")
                };

                foreach (var p in possiblePaths)
                {
                    if (!string.IsNullOrEmpty(p) && System.IO.File.Exists(p))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(p);
                        return Convert.ToBase64String(bytes);
                    }
                }
            }
            catch { }
            return "";
        }

        /// <summary>
        /// Generates the 12-Month Permission to Occupy (PTO) Certificate PDF for UC21 matching template exactly.
        /// </summary>
        public static byte[] GeneratePtoCertificatePdf(RE_Application app, string siteUrl = "")
        {
            string html = BuildUserAgreementHtml(app, isPto12Months: true);
            return ConvertHtmlToPdfBytes(html);
        }

        /// <summary>
        /// Generates the 36-Month Full Lease Agreement PDF for UC23 matching template exactly.
        /// </summary>
        public static byte[] GenerateFullLeaseAgreementPdf(RE_Application app, string siteUrl = "")
        {
            string html = BuildUserAgreementHtml(app, isPto12Months: false);
            return ConvertHtmlToPdfBytes(html);
        }

        private static byte[] ConvertHtmlToPdfBytes(string htmlContent)
        {
            // Attempt 1: High-Fidelity Edge Headless Renderer (100% pixel match with 180px Logo)
            if (System.IO.File.Exists(EdgePath))
            {
                try
                {
                    string tempDir = System.IO.Path.GetTempPath();
                    string htmlFile = System.IO.Path.Combine(tempDir, "RE_Agreement_" + Guid.NewGuid().ToString("N") + ".html");
                    string pdfFile = System.IO.Path.Combine(tempDir, "RE_Agreement_" + Guid.NewGuid().ToString("N") + ".pdf");

                    System.IO.File.WriteAllText(htmlFile, htmlContent, System.Text.Encoding.UTF8);

                    var psi = new ProcessStartInfo
                    {
                        FileName = EdgePath,
                        Arguments = string.Format("--headless --disable-gpu --print-to-pdf=\"{0}\" --no-margins \"{1}\"", pdfFile, htmlFile),
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var proc = Process.Start(psi))
                    {
                        proc.WaitForExit(10000);
                    }

                    if (System.IO.File.Exists(pdfFile))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(pdfFile);
                        try { System.IO.File.Delete(htmlFile); System.IO.File.Delete(pdfFile); } catch { }
                        return bytes;
                    }
                }
                catch { }
            }

            // Attempt 2: iTextSharp XMLWorker Fallback Renderer
            using (var ms = new MemoryStream())
            {
                var doc = new iTextSharp.text.Document(PageSize.A4, 40, 40, 40, 40);
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                using (var sr = new StringReader(htmlContent))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, sr);
                }

                doc.Close();
                return ms.ToArray();
            }
        }

        private static string BuildUserAgreementHtml(RE_Application app, bool isPto12Months)
        {
            string logoB64 = GetCoELogoBase64();
            string logoImgHtml = !string.IsNullOrEmpty(logoB64)
                ? string.Format("<img src=\"data:image/png;base64,{0}\" style=\"width: 180px; height: auto; display: block; margin: 0 auto;\" alt=\"City of Ekurhuleni Logo\">", logoB64)
                : "<div style=\"font-size: 16pt; font-weight: bold;\">City of<br>Ekurhuleni</div>";

            string tenantName = !string.IsNullOrEmpty(app.EntityName) ? app.EntityName : (app.SystemUser?.FullName ?? "N/A");
            string identityNum = !string.IsNullOrEmpty(app.CompanyRegistrationNumber) ? app.CompanyRegistrationNumber : "N/A";
            string repName = !string.IsNullOrEmpty(app.AuthorizedRepresentativeName) ? app.AuthorizedRepresentativeName : tenantName;
            string repCap = !string.IsNullOrEmpty(app.AuthorizedRepresentativeCapacity) ? app.AuthorizedRepresentativeCapacity : "Director";
            string resDate = (app.CreatedDateTime.HasValue ? app.CreatedDateTime.Value : DateTime.Today).ToString("dd MMMM yyyy");

            string deptName = isPto12Months ? "DEVELOPMENT PLANNING & REAL ESTATE DEPARTMENT" : "ECONOMIC DEVELOPMENT DEPARTMENT / DPRE";
            string facilityName = app.SelectedFacility?.Name ?? (isPto12Months ? "Tsakane Business Park" : "Kwa Thema Business Hub");
            string unitName = app.SelectedFacilityUnit != null ? ("Unit " + app.SelectedFacilityUnit.Id) : "kiosk";
            string facilityAddr = app.SelectedFacility?.Address ?? (isPto12Months ? "7522 Hlakwana Street, Tsakane" : "Corner Thabahadi Road & Rhokana Road, Ext 3, Kwa-Thema");
            decimal extentSize = app.SelectedFacilityUnit?.UnitSize ?? 45.0m;

            int durationMonths = isPto12Months ? 12 : 36;
            string durationText = isPto12Months ? "12 months" : "three (3) years (36 months)";
            DateTime startDate = app.LeaseStartDate ?? (app.PtoStartDate ?? DateTime.Today);
            DateTime endDate = app.LeaseEndDate ?? (app.PtoEndDate ?? startDate.AddMonths(durationMonths));

            decimal rentalAmt = app.CalculatedMonthlyRental ?? (isPto12Months ? 195.00m : 22000.00m);
            decimal depositAmt = app.LeaseDepositAmount ?? (isPto12Months ? 390.00m : 22000.00m);
            string purposeStr = !string.IsNullOrEmpty(app.PtoPurposeOfOccupation) 
                ? app.PtoPurposeOfOccupation 
                : (isPto12Months ? "Administration & Light Workshop Operations" : "Business Training, Pitching Boosters and related Business Services");

            string clause32 = isPto12Months
                ? "3.2 Payment shall be made at Ekurhuleni Customer Care centre deposited to the following vote number 17901402870MHZZZZZ16 of via EFT to the following CoE bank account number STANDARD BANK Account Name City of Ekurhuleni Lease income Account no: 281181578 Code:013042 Code(EFT):051001 Or pay at CCC into vote no: 17901402870MHZZZZZ16 Please send POP to CEDFacilities@ekurhuleni.gov.za"
                : "3.2 However, as Ulwazi Resource Consulting (URC) is rendering business acceleration and pitching booster services to local business enterprises and entrepreneurs in terms of the Strategic Partnership Agreement between CoE and URC, which are aligned to a local economic development mandate of The City, the rental will be offset against the services rendered by URC to the City subject to submission of a monthly performance report.";

            return string.Format(@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<style>
    @page {{
        size: A4;
        margin: 12mm 18mm 15mm 18mm;
    }}
    body {{
        font-family: Arial, Helvetica, sans-serif;
        font-size: 10.5pt;
        line-height: 1.35;
        color: #000;
        margin: 0;
        padding: 0;
    }}
    .header-container {{
        text-align: center;
        margin-top: 5px;
        margin-bottom: 15px;
    }}
    .header-dept {{
        text-align: center;
        font-weight: bold;
        font-size: 11pt;
        margin-bottom: 10px;
    }}
    .header-title {{
        text-align: center;
        font-weight: bold;
        font-size: 11pt;
        margin-bottom: 25px;
    }}
    .clause-num {{
        font-weight: bold;
        font-size: 10.5pt;
        margin-top: 16px;
        margin-bottom: 8px;
    }}
    p {{
        margin: 6px 0;
        text-align: justify;
    }}
    .text-center {{
        text-align: center;
        font-weight: bold;
    }}
    .dotted-fill {{
        border-bottom: 1px dotted #555;
        display: inline-block;
        padding: 0 4px;
        font-weight: bold;
    }}
    table.sig-table {{
        width: 100%;
        margin-top: 30px;
        border-collapse: collapse;
    }}
    table.sig-table td {{
        vertical-align: top;
        width: 50%;
        padding: 5px;
    }}
</style>
</head>
<body>

<div class=""header-container"">
    {0}
</div>

<div class=""header-dept"">{1}</div>

<div class=""header-title"">USER AGREEMENT FOR THE USE OF MUNICIPALITY OWNED PROPERTY ENTERED INTO BETWEEN COE</div>

<p>Herein represented by <strong>Mr Caiphus Chauke</strong> in his capacity as <strong>Head of Department: {2}</strong>, duly authorized thereto by a resolution/delegation of the <strong>Council</strong> of the said Municipality............. (Hereinafter referred to as “the City”)</p>

<p class=""text-center"" style=""margin-top: 15px; margin-bottom: 15px;"">and</p>

<p><strong>Name and Surname</strong> <span class=""dotted-fill"" style=""min-width: 480px;"">{3}</span></p>

<p><strong>Identity Number.....</strong> <span class=""dotted-fill"" style=""min-width: 480px;"">{4}</span></p>

<p style=""text-align: right; font-style: italic; margin-right: 60px; margin-top: 4px;"">(Hereinafter referred to as “the User”).</p>

<p class=""text-center"" style=""margin-top: 15px; margin-bottom: 15px;"">And/Or</p>

<p style=""font-weight: bold; text-decoration: underline; margin-top: 15px;"">If the User is a Juristic person:</p>

<p><strong>Company Name:</strong> {3} (Hereinafter referred to as the User), represented by <span class=""dotted-fill"" style=""min-width: 200px;"">{5}</span> in his/her/their capacity as <span class=""dotted-fill"" style=""min-width: 80px;"">{6}</span> duly authorized hereto by a Resolution passed by the aforementioned on <span class=""dotted-fill"" style=""min-width: 100px;"">{7}</span>.</p>

<div class=""clause-num"">1. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; PREMISES</div>

<p>The {8} hereby grants the User access to use the City’s property: <strong>{9}</strong> situated at {10}, <strong>{11}</strong> approximately <strong>{12:F1} m²</strong> in extent, as indicated on the plan annexed as Annexure ""B""; (hereinafter referred to as the Premises)</p>

<div class=""clause-num"">2. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; DURATION</div>

<p>The User Agreement permits the use of the aforesaid premises commencing on <strong>{13:dd/MM/yyyy}</strong> and continues for a period of {14} and terminates on <strong>{15:dd/MM/yyyy}</strong>.</p>

<div class=""clause-num"">3. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; RENTAL</div>

<p>3.1 The rental payable in respect of the premises shall be R <strong>{16:N2}</strong> (ex.VAT) per month subject to a 5 % (percent) escalation per annum compounded on the previous year’s rental. The rental payable excludes water and electricity. A flat fee of R100 for water will be charged monthly.</p>

<p>{17}</p>

<div class=""clause-num"">4. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; SUB-LEASE</div>

<p>4.1 The User shall not be entitled to sub-let the premises or any part thereof nor assign, cede or transfer its interest under this lease nor part with possession of the PREMISES to any other party or body, without the prior written consent of the City being had and obtained, which consent shall not be unreasonably withheld, provided that:</p>
<p>4.2 In the event of the City consenting to the sub-letting of the PREMISES, the sub-tenant shall be bound to pay the rentals direct to the User who for the purpose of this clause be deemed to be the duly authorised agent of the City, and</p>
<p>4.3 The User shall in no way be relieved of her obligations to the City under this lease by reason of any sub-lease.</p>

<div class=""clause-num"">5. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CANCELLATION CLAUSE</div>

<p>5.1 Both the City and the User shall have the right during the currency of this User Agreement to terminate by giving the other party at least three (3) months prior written notice of termination.</p>
<p>5.2 The User is required to occupy the premises within one month of being issued with a permission to use premises agreement. Failure to start operating from the premises will lead to cancellation of this User Agreement.</p>
<p>5.3 The User is required to comply with the Business Park operating hours, from 07h00 to 18h00 during business days and 08h00 to 14h00 on weekends and/or holidays. Failure to comply with this requirement shall lead to cancelation of the agreement.</p>

<div class=""clause-num"">6. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; USE OF PREMISES</div>

<p>6.1 The premises shall be used for the purpose of <strong>{18}</strong> and purposes aligned thereto and shall not be used for any nor for any other purposes without the prior written consent of the City, which consent shall not be unreasonably withheld;</p>
<p>6.2 The User shall conduct its activities in the premises in conformity with the existing municipal by-law, regulations, government ordinances and statutes and with any other laws which may be promulgated at any time in the future</p>

<div class=""clause-num"">7. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; COSTS</div>

<p>The User shall pay a once off amount of R550.00 to The City as an administrative fee;</p>

<div class=""clause-num"">8. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; JURISDICTION</div>

<p>Both {19} and CoE hereby consent in terms of section 45 of the Magistrates Court Act No. 32 of 1994 of the jurisdiction of any Magistrate's Court having jurisdiction over their respective persons under section 28 of that Act, notwithstanding that any action or proceeding arising out of this agreement would otherwise be beyond the jurisdiction of such court, provided that the City shall have the right to institute action in any other competent court.</p>

<div class=""clause-num"">9. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; DEPOSIT AS SECURITY</div>

<p>11.1 The User shall upon the signing of this User Agreement, deposit with the City, an amount of R <strong>{20:N2}</strong> excluding VAT as security for damages that may be caused to the premises due to negligence</p>
<p>11.2 The City may appropriate the deposit monies towards any rental due, damages, loss of keys, and/or to offset other monies for which the User may be liable in terms of this Agreement hereof;</p>
<p>11.3 The City shall refund the deposit monies referred to in 11.1 to the User on expiry or termination of the User Agreement, save for any portion appropriated by the City in terms of 11.2 above.</p>

<div class=""clause-num"">10. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; MAINTENANCE</div>

<p>12.1 Within fourteen (14) working days from the date of signing of this User Agreement, the User shall be obliged to notify the City in writing of any pre-existing deficiencies which may exist in the premises, failing which the User is deemed to have received the premises in good order and condition.</p>
<p>12.2 The User undertakes to regularly care for and maintain the premises, repair damages/breakages at own cost, and keep sewerage, water pipes, gutters and drains free from obstructions.</p>

<table class=""sig-table"">
<tr>
    <td>
        <strong>FOR CITY OF EKURHULENI:</strong><br><br>
        THUS DONE AND SIGNED by Mr. Caiphus Chauke in his capacity as Head of Department: {2}, signed at Kempton Park on _______________ 2024.<br><br><br>
        __________________________<br>
        <strong>MR. CAIPHUS CHAUKE</strong>
    </td>
    <td>
        <strong>AS WITNESSES (CoE):</strong><br><br>
        1. __________________________<br><br>
        2. __________________________
    </td>
</tr>
<tr>
    <td style=""padding-top: 25px;"">
        <strong>FOR THE USER:</strong><br><br>
        THUS DONE AND SIGNED by <strong>{5}</strong> in his/her capacity as <strong>{6}</strong>, signed at Kempton Park on {7}.<br><br><br>
        __________________________<br>
        <strong>FOR USER</strong>
    </td>
    <td style=""padding-top: 25px;"">
        <strong>AS WITNESSES (USER):</strong><br><br>
        1. __________________________<br><br>
        2. __________________________
    </td>
</tr>
</table>

</body>
</html>",
            logoImgHtml,                                                 // 0
            deptName,                                                   // 1
            isPto12Months ? "Development Planning & Real Estate" : "Economic Development", // 2
            tenantName,                                                 // 3
            identityNum,                                                // 4
            repName,                                                    // 5
            repCap,                                                     // 6
            resDate,                                                    // 7
            isPto12Months ? "Development Planning & Real Estate Department" : "Economic Development Department", // 8
            unitName,                                                   // 9
            facilityName,                                               // 10
            facilityAddr,                                               // 11
            extentSize,                                                 // 12
            startDate,                                                  // 13
            durationText,                                               // 14
            endDate,                                                    // 15
            rentalAmt,                                                  // 16
            clause32,                                                   // 17
            purposeStr,                                                 // 18
            isPto12Months ? "the user" : "URC",                         // 19
            depositAmt                                                  // 20
            );
        }
    }
}
