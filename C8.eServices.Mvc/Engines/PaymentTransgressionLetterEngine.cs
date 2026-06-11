using System;
using System.IO;
using System.Linq;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Engines
{
    public class PaymentTransgressionLetterEngine
    {
        private readonly eServicesDbContext _db;

        public PaymentTransgressionLetterEngine(eServicesDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Generates payment transgression letter HTML content
        /// </summary>
        public string GenerateLetterContent(int paymentTransgressionId, string letterType)
        {
            var transgression = _db.PaymentTransgressions.Find(paymentTransgressionId);
            if (transgression == null) return string.Empty;

            string letterContent = string.Empty;

            switch (letterType)
            {
                case PaymentTransgressionLetterTypes.PaymentTransgressionNotice:
                    letterContent = GeneratePaymentTransgressionNotice(transgression);
                    break;
                case PaymentTransgressionLetterTypes.WrittenWarningLetter:
                    letterContent = GenerateWrittenWarningLetter(transgression);
                    break;
                case PaymentTransgressionLetterTypes.FinalWrittenWarningLetter:
                    letterContent = GenerateFinalWrittenWarningLetter(transgression);
                    break;
            }

            return letterContent;
        }

        private string GeneratePaymentTransgressionNotice(PaymentTransgression transgression)
        {
            var lastPaymentStr = transgression.LastPaymentAmount.GetValueOrDefault().ToString("N2");
            var totalAmountStr = transgression.TotalAmountDue.ToString("N2");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .content {{ line-height: 1.6; }}
        .details {{ margin: 20px 0; padding: 15px; background-color: #f5f5f5; border-left: 4px solid #007bff; }}
        .footer {{ margin-top: 40px; font-size: 0.9em; color: #666; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>PAYMENT TRANSGRESSION NOTICE</h2>
        <p>Case Reference: {transgression.CaseReferenceNumber}</p>
        <p>Date: {DateTime.Now:dd MMMM yyyy}</p>
    </div>
    
    <div class='content'>
        <p>Dear {transgression.TenantName} {transgression.TenantSurname},</p>
        
        <p>This notice is issued to inform you of a payment transgression recorded against your tenancy account.</p>
        
        <div class='details'>
            <strong>Tenant Details:</strong><br/>
            Official Number: {transgression.OfficialNumber}<br/>
            Tenancy Reference: {transgression.TenancyReferenceNumber}<br/>
            Property: {transgression.Complex?.Name} - Block {transgression.BlockNumber}, Unit {transgression.UnitNumber}<br/>
            Account Number: {transgression.AccountNumber}
        </div>
        
        <div class='details'>
            <strong>Financial Information:</strong><br/>
            Last Payment: R{lastPaymentStr} on {transgression.LastPaymentDate:dd MMM yyyy}<br/>
            <strong>Total Amount Due: R{totalAmountStr}</strong>
        </div>
        
        <div class='details'>
            <strong>Transgression Details:</strong><br/>
            Category: {transgression.Category?.Name}<br/>
            Type: {transgression.Type?.Name}<br/>
            Severity: {transgression.Severity?.Name}<br/><br/>
            <strong>Description:</strong><br/>
            {transgression.DetailedDescription}
        </div>
        
        <p><strong>Required Action:</strong></p>
        <p>You are required to address this matter within <strong>7 working days</strong> from the date of this notice by:</p>
        <ul>
            <li>Settling your outstanding balance in full, or</li>
            <li>Contacting our office to arrange a suitable payment plan</li>
        </ul>
        
        <p><strong>Consequences of Non-Compliance:</strong></p>
        <p>Failure to address this transgression within the stipulated timeframe may result in further action, including escalation to a written warning and potential lease termination proceedings.</p>
        
        <p>Should you require clarification or wish to discuss this matter, please contact our Client Services Office.</p>
        
        <p>Yours sincerely,</p>
        <p><strong>Client Services Office</strong><br/>
        Property Lease Management<br/>
        Contact: [Contact Details]</p>
    </div>
    
    <div class='footer'>
        <p>This is an official communication. Please retain this notice for your records.</p>
    </div>
</body>
</html>";
        }

        private string GenerateWrittenWarningLetter(PaymentTransgression transgression)
        {
            var transgressionCount = _db.PaymentTransgressions
                .Count(pt => pt.OfficialNumber == transgression.OfficialNumber 
                          && pt.IsActive 
                          && !pt.IsDeleted 
                          && pt.Id <= transgression.Id);

            var lastPaymentStr = transgression.LastPaymentAmount.GetValueOrDefault().ToString("N2");
            var totalAmountStr = transgression.TotalAmountDue.ToString("N2");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .header {{ text-align: center; margin-bottom: 30px; border-bottom: 2px solid #dc3545; padding-bottom: 10px; }}
        .warning {{ background-color: #fff3cd; border: 2px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .content {{ line-height: 1.6; }}
        .details {{ margin: 20px 0; padding: 15px; background-color: #f5f5f5; border-left: 4px solid #dc3545; }}
        .footer {{ margin-top: 40px; font-size: 0.9em; color: #666; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>WRITTEN WARNING LETTER</h2>
        <h3 style='color: #dc3545;'>PAYMENT TRANSGRESSION - WARNING #{transgressionCount}</h3>
        <p>Case Reference: {transgression.CaseReferenceNumber}</p>
        <p>Date: {DateTime.Now:dd MMMM yyyy}</p>
    </div>
    
    <div class='content'>
        <div class='warning'>
            <strong>âš ï¸ OFFICIAL WARNING</strong><br/>
            This is warning number {transgressionCount} issued against your tenancy. Continued non-compliance may result in lease termination.
        </div>
        
        <p>Dear {transgression.TenantName} {transgression.TenantSurname},</p>
        
        <p>This written warning is issued due to ongoing payment transgressions against your tenancy account.</p>
        
        <div class='details'>
            <strong>Tenant Details:</strong><br/>
            Official Number: {transgression.OfficialNumber}<br/>
            Tenancy Reference: {transgression.TenancyReferenceNumber}<br/>
            Property: {transgression.Complex?.Name} - Block {transgression.BlockNumber}, Unit {transgression.UnitNumber}<br/>
            Account Number: {transgression.AccountNumber}
        </div>
        
        <div class='details'>
            <strong>Financial Information:</strong><br/>
            Last Payment: R{lastPaymentStr} on {transgression.LastPaymentDate:dd MMM yyyy}<br/>
            <strong style='color: #dc3545;'>Total Amount Due: R{totalAmountStr}</strong>
        </div>
        
        <div class='details'>
            <strong>Transgression Details:</strong><br/>
            Category: {transgression.Category?.Name}<br/>
            Type: {transgression.Type?.Name}<br/>
            Severity: {transgression.Severity?.Name} (Level {transgression.Severity?.Level})<br/><br/>
            <strong>Description:</strong><br/>
            {transgression.DetailedDescription}
        </div>
        
        <p><strong style='color: #dc3545;'>IMMEDIATE ACTION REQUIRED:</strong></p>
        <p>You are <strong>formally warned</strong> and required to:</p>
        <ul>
            <li>Settle your outstanding balance within <strong>7 working days</strong> from the date of this letter</li>
            <li>Contact our office immediately to discuss payment arrangements</li>
            <li>Provide a written explanation for the continued non-payment</li>
        </ul>
        
        <p><strong style='color: #dc3545;'>SERIOUS CONSEQUENCES:</strong></p>
        <p>Please be advised that:</p>
        <ul>
            <li>This warning will be placed on your tenancy record</li>
            <li>Further non-compliance will result in escalation to a Final Written Warning</li>
            <li>After 3 warnings, lease termination proceedings will be initiated</li>
            <li>Legal action may be taken to recover outstanding amounts</li>
        </ul>
        
        <p>This is a serious matter. We strongly urge you to address this transgression immediately to avoid further action.</p>
        
        <p>Yours sincerely,</p>
        <p><strong>Client Services Office</strong><br/>
        Property Lease Management<br/>
        Contact: [Contact Details]</p>
    </div>
    
    <div class='footer'>
        <p><strong>This is an official warning. Please retain this letter for your records.</strong></p>
    </div>
</body>
</html>";
        }

        private string GenerateFinalWrittenWarningLetter(PaymentTransgression transgression)
        {
            var transgressionCount = _db.PaymentTransgressions
                .Count(pt => pt.OfficialNumber == transgression.OfficialNumber 
                          && pt.IsActive 
                          && !pt.IsDeleted 
                          && pt.Id <= transgression.Id);

            var lastPaymentStr = transgression.LastPaymentAmount.GetValueOrDefault().ToString("N2");
            var totalAmountStr = transgression.TotalAmountDue.ToString("N2");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .header {{ text-align: center; margin-bottom: 30px; border-bottom: 3px solid #dc3545; padding-bottom: 10px; background-color: #f8d7da; padding: 20px; }}
        .final-warning {{ background-color: #dc3545; color: white; padding: 20px; margin: 20px 0; text-align: center; font-size: 1.2em; font-weight: bold; }}
        .content {{ line-height: 1.6; }}
        .details {{ margin: 20px 0; padding: 15px; background-color: #f5f5f5; border-left: 4px solid #dc3545; }}
        .critical {{ background-color: #f8d7da; border: 2px solid #dc3545; padding: 15px; margin: 20px 0; }}
        .footer {{ margin-top: 40px; font-size: 0.9em; color: #666; border-top: 2px solid #dc3545; padding-top: 20px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1 style='color: #dc3545; margin: 0;'>âš ï¸ FINAL WRITTEN WARNING âš ï¸</h1>
        <h2 style='margin: 10px 0;'>PAYMENT TRANSGRESSION - LAST WARNING</h2>
        <p>Case Reference: {transgression.CaseReferenceNumber}</p>
        <p>Date: {DateTime.Now:dd MMMM yyyy}</p>
    </div>
    
    <div class='content'>
        <div class='final-warning'>
            ðŸš¨ THIS IS YOUR FINAL WARNING - WARNING #{transgressionCount} ðŸš¨<br/>
            LEASE TERMINATION PROCEEDINGS WILL COMMENCE IF THIS MATTER IS NOT RESOLVED IMMEDIATELY
        </div>
        
        <p>Dear {transgression.TenantName} {transgression.TenantSurname},</p>
        
        <p><strong style='color: #dc3545;'>This is your FINAL WRITTEN WARNING regarding payment transgressions on your tenancy account.</strong></p>
        
        <p>Despite previous notices and warnings, you have failed to address the outstanding payment obligations. This letter serves as your last opportunity to rectify this matter before lease termination proceedings are initiated.</p>
        
        <div class='details'>
            <strong>Tenant Details:</strong><br/>
            Official Number: {transgression.OfficialNumber}<br/>
            Tenancy Reference: {transgression.TenancyReferenceNumber}<br/>
            Property: {transgression.Complex?.Name} - Block {transgression.BlockNumber}, Unit {transgression.UnitNumber}<br/>
            Account Number: {transgression.AccountNumber}
        </div>
        
        <div class='details'>
            <strong>Financial Information:</strong><br/>
            Last Payment: R{lastPaymentStr} on {transgression.LastPaymentDate:dd MMM yyyy}<br/>
            <strong style='color: #dc3545; font-size: 1.2em;'>Total Amount Due: R{totalAmountStr}</strong>
        </div>
        
        <div class='details'>
            <strong>Transgression Details:</strong><br/>
            Category: {transgression.Category?.Name}<br/>
            Type: {transgression.Type?.Name}<br/>
            Severity: {transgression.Severity?.Name} (Level {transgression.Severity?.Level})<br/>
            Warning Count: {transgressionCount}<br/><br/>
            <strong>Description:</strong><br/>
            {transgression.DetailedDescription}
        </div>
        
        <div class='critical'>
            <strong style='color: #dc3545;'>âš ï¸ CRITICAL - IMMEDIATE ACTION REQUIRED âš ï¸</strong><br/><br/>
            You are required to take the following actions <strong>WITHIN 7 WORKING DAYS</strong>:
            <ol style='margin: 15px 0;'>
                <li>Settle your outstanding balance of R{totalAmountStr} in full</li>
                <li>If unable to pay in full, contact our office IMMEDIATELY to arrange a payment plan</li>
                <li>Provide a written explanation and commitment to prevent future transgressions</li>
                <li>Attend a compulsory meeting with our Client Services Office</li>
            </ol>
        </div>
        
        <p><strong style='color: #dc3545; font-size: 1.1em;'>LEASE TERMINATION CONSEQUENCES:</strong></p>
        <p>Please be advised that failure to comply with this final warning will result in:</p>
        <ul>
            <li><strong>Immediate initiation of lease termination proceedings</strong></li>
            <li>Legal action to recover all outstanding amounts</li>
            <li>Eviction proceedings in accordance with lease terms</li>
            <li>Additional legal costs and interest charges</li>
            <li>Negative impact on your tenancy record</li>
            <li>Potential difficulty in securing future rental accommodation</li>
        </ul>
        
        <div class='critical'>
            <p style='margin: 0;'><strong>NO FURTHER WARNINGS WILL BE ISSUED.</strong></p>
            <p style='margin: 10px 0 0 0;'>This is your final opportunity to resolve this matter. We urge you to treat this with the utmost seriousness and take immediate action.</p>
        </div>
        
        <p>For any clarification or to arrange payment, contact our Client Services Office urgently.</p>
        
        <p>Yours sincerely,</p>
        <p><strong>Client Services Office</strong><br/>
        Property Lease Management<br/>
        Contact: [Contact Details]</p>
    </div>
    
    <div class='footer'>
        <p><strong style='color: #dc3545;'>âš ï¸ THIS IS YOUR FINAL WARNING - ACTION REQUIRED WITHIN 7 WORKING DAYS âš ï¸</strong></p>
        <p>This is an official legal document. Please retain this letter for your records.</p>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Saves letter as HTML file and returns file path
        /// </summary>
        public string SaveLetterAsFile(int paymentTransgressionId, string letterType, string letterContent)
        {
            try
            {
                var transgression = _db.PaymentTransgressions.Find(paymentTransgressionId);
                if (transgression == null) return null;

                var directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads/PaymentTransgressions/Letters/");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var fileName = $"{transgression.CaseReferenceNumber}_{letterType.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.html";
                var filePath = Path.Combine(directory, fileName);

                System.IO.File.WriteAllText(filePath, letterContent, System.Text.Encoding.UTF8);

                return filePath;
            }
            catch (Exception ex)
            {
                // Log error
                return null;
            }
        }
    }
}

