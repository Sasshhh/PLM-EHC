using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace C8.eServices.Mvc.Engines
{
    public class NotificationEngine
    {
        private readonly eServicesDbContext _db;
        private readonly CesarDbContext _core;

        public NotificationEngine(eServicesDbContext db)
        {
            _db = db;
            _core = new CesarDbContext();
        }

        private void QueueEmail(string recipientEmail, string recipientName, string subject, string body, string referenceNumber)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail)) return;

            var emailApplication = _db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
            var emailAccount = _db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
            var emailTemplate = _db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.PropertyLeaseManagementDefaultEmailTempate);

            if (emailApplication == null || emailAccount == null || emailTemplate == null) return;

            var emailBody = emailTemplate.Value;
            emailBody = emailBody.Replace("#NAME#", !string.IsNullOrEmpty(recipientName) ? recipientName : "Sir/ Madam");
            emailBody = emailBody.Replace("#BODYTEXT#", body);
            if (!string.IsNullOrEmpty(referenceNumber))
                emailBody = emailBody.Replace("#REFERENCENUMBER#", referenceNumber);

            _core.EmailQueue.Add(new EmailQueueItem
            {
                ApplicationId = Convert.ToInt32(emailApplication.Value),
                QueueDateTime = DateTime.Now,
                EmailAccountId = Convert.ToInt32(emailAccount.Value),
                ToList = recipientEmail,
                Subject = subject,
                Body = emailBody,
                IsHtml = true,
                FailureCount = 0,
                ReferenceId = "1",
                HasAttachments = false,
                StatusId = 1
            });
            _core.SaveChanges();
        }

        private void QueueSms(string mobileNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber)) return;

            var statusId = _db.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending)?.Id ?? 1;

            _core.SmsQueue.Add(new SmsQueueItem
            {
                ApplicationId = 1,
                QueueDateTime = DateTime.Now,
                SmsAccountId = 1,
                MobileNumber = mobileNumber,
                TextMessage = StripHtml(message),
                FailureCount = 0,
                ReferenceId = "1",
                StatusId = statusId
            });
            _core.SaveChanges();
        }

        private static string StripHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var s = Regex.Replace(input, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, "<.*?>", string.Empty);
            return s.Trim();
        }

        public bool SendComplaintAcknowledgement(TenantComplaint complaint)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(complaint.ComplainantEmail) &&
                    string.IsNullOrWhiteSpace(complaint.ComplainantCellphone))
                    return false;

                var subject = $"Complaint Acknowledgement - {complaint.CaseReferenceNumber}";
                var body = $"We acknowledge receipt of your complaint <strong>{complaint.CaseReferenceNumber}</strong> submitted on {complaint.DateSubmitted?.ToString("dd MMMM yyyy")}.<br/>" +
                           "Your complaint has been assigned to a Client Services Officer and will be investigated. You will be notified of any updates.<br/>" +
                           "Thank you for bringing this matter to our attention.<br/><br/>Regards,<br/>Property Lease Management Team";

                QueueEmail(complaint.ComplainantEmail, $"{complaint.ComplainantFirstName} {complaint.ComplainantSurname}", subject, body, complaint.CaseReferenceNumber);
                QueueSms(complaint.ComplainantCellphone, $"Complaint {complaint.CaseReferenceNumber} received. A Client Services Officer will contact you. PLM Team");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SendAppointmentNotification(TenantComplaint complaint, ComplaintInvestigation investigation)
        {
            try
            {
                var respondent = GetRespondentContactInfo(complaint);
                if (respondent == null) return false;

                var subject = $"Investigation Appointment - {complaint.CaseReferenceNumber}";
                var body = $"An investigation appointment has been scheduled for complaint case <strong>{complaint.CaseReferenceNumber}</strong>.<br/>" +
                           $"Date: {investigation.AppointmentDate?.ToString("dd MMMM yyyy")}<br/>" +
                           $"Time: {investigation.AppointmentTime?.ToString(@"hh\:mm")}<br/><br/>" +
                           "Please log in to the Property Lease Management system to confirm your attendance or propose an alternative date.<br/><br/>" +
                           "Regards,<br/>Property Lease Management Team";

                QueueEmail(respondent.Email, $"{complaint.RespondentFirstName} {complaint.RespondentSurname}", subject, body, complaint.CaseReferenceNumber);
                QueueSms(respondent.Cellphone, $"Investigation appointment for {complaint.CaseReferenceNumber} on {investigation.AppointmentDate?.ToString("dd MMM")} at {investigation.AppointmentTime?.ToString(@"hh\:mm")}. Please confirm via PLM system.");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SendAppointmentConfirmationToCSO(ComplaintInvestigation investigation)
        {
            try
            {
                var complaint = _db.TenantComplaints.FirstOrDefault(t => t.Id == investigation.TenantComplaintId);
                if (complaint?.AssignedToId == null) return false;

                var cso = _db.Customers.Find(complaint.AssignedToId);
                if (cso == null) return false;

                var systemUser = _db.SystemUsers.FirstOrDefault(su => su.Id == cso.SystemUserId);
                if (systemUser == null || string.IsNullOrWhiteSpace(systemUser.EmailAddress)) return false;

                var subject = $"Appointment Confirmed - {complaint.CaseReferenceNumber}";
                var body = $"The respondent has confirmed the investigation appointment for case <strong>{complaint.CaseReferenceNumber}</strong>.<br/>" +
                           $"Date: {investigation.AppointmentDate?.ToString("dd MMMM yyyy")}<br/>" +
                           $"Time: {investigation.AppointmentTime?.ToString(@"hh\:mm")}<br/><br/>" +
                           "Please proceed with the investigation as scheduled.<br/><br/>" +
                           "Regards,<br/>Property Lease Management System";

                QueueEmail(systemUser.EmailAddress, $"{cso.FirstName} {cso.LastName}", subject, body, complaint.CaseReferenceNumber);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SendAssignmentNotification(TenantComplaint complaint, Customer cso)
        {
            try
            {
                var systemUser = _db.SystemUsers.FirstOrDefault(su => su.Id == cso.SystemUserId);
                if (systemUser == null || string.IsNullOrWhiteSpace(systemUser.EmailAddress)) return false;

                var subject = $"New Complaint Assigned - {complaint.CaseReferenceNumber}";
                var body = $"A new complaint has been assigned to you:<br/>" +
                           $"Case Reference: <strong>{complaint.CaseReferenceNumber}</strong><br/>" +
                           $"Category: {complaint.ComplaintCategory?.Name}<br/>" +
                           $"Submitted By: {complaint.ComplainantFirstName} {complaint.ComplainantSurname}<br/>" +
                           $"Date Submitted: {complaint.DateSubmitted?.ToString("dd MMMM yyyy")}<br/><br/>" +
                           "Please log in to the Property Lease Management system to review and take action.<br/><br/>" +
                           "Regards,<br/>Property Lease Management System";

                QueueEmail(systemUser.EmailAddress, $"{cso.FirstName} {cso.LastName}", subject, body, complaint.CaseReferenceNumber);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SendOutcomeNotification(TenantComplaint complaint, ComplaintInvestigation investigation)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(complaint.ComplainantEmail) &&
                    string.IsNullOrWhiteSpace(complaint.ComplainantCellphone))
                    return false;

                var outcomeText = investigation.Outcome == "Resolved" ? "resolved" :
                                  investigation.Outcome == "Referral" ? "referred to an external agency" : "unresolved";

                var subject = $"Complaint Outcome - {complaint.CaseReferenceNumber}";
                var body = $"The investigation for your complaint <strong>{complaint.CaseReferenceNumber}</strong> has been completed.<br/>" +
                           $"Outcome: {investigation.Outcome}<br/>" +
                           $"Details: {investigation.OutcomeDetails}<br/>" +
                           $"Date: {investigation.OutcomeDate?.ToString("dd MMMM yyyy")}<br/><br/>" +
                           "If you have any questions, please contact our Client Services team.<br/><br/>" +
                           "Regards,<br/>Property Lease Management Team";

                QueueEmail(complaint.ComplainantEmail, $"{complaint.ComplainantFirstName} {complaint.ComplainantSurname}", subject, body, complaint.CaseReferenceNumber);
                QueueSms(complaint.ComplainantCellphone, $"Complaint {complaint.CaseReferenceNumber} {outcomeText}. Check your email for details. PLM Team");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SendServiceRequestOutcomeNotification(ServiceRequest serviceRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serviceRequest.EmailAddress) &&
                    string.IsNullOrWhiteSpace(serviceRequest.ContactNumber))
                    return false;

                var subject = $"Service Request Resolved - {serviceRequest.RequestReferenceNumber}";
                var body = $"Your service request <strong>{serviceRequest.RequestReferenceNumber}</strong> has been resolved.<br/>" +
                           $"Resolved Date: {serviceRequest.DateResolved?.ToString("dd MMMM yyyy HH:mm")}<br/><br/>" +
                           "If you have any questions or require further assistance, please contact our Client Services team.<br/><br/>" +
                           "Regards,<br/>Property Lease Management Team";

                QueueEmail(serviceRequest.EmailAddress, $"{serviceRequest.ReportedByName} {serviceRequest.ReportedBySurname}", subject, body, serviceRequest.RequestReferenceNumber);
                QueueSms(serviceRequest.ContactNumber, $"Service Request {serviceRequest.RequestReferenceNumber} has been resolved. Check your email for details. PLM Team");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SendServiceRequestAssignmentNotification(ServiceRequest serviceRequest, Customer cso)
        {
            try
            {
                var systemUser = _db.SystemUsers.FirstOrDefault(su => su.Id == cso.SystemUserId);
                if (systemUser == null || string.IsNullOrWhiteSpace(systemUser.EmailAddress)) return false;

                var subject = $"New Service Request Assigned - {serviceRequest.RequestReferenceNumber}";
                var body = $"A new service request has been assigned to you:<br/>" +
                           $"Reference: <strong>{serviceRequest.RequestReferenceNumber}</strong><br/>" +
                           $"Complex: {serviceRequest.Complex?.Name ?? serviceRequest.ComplexId.ToString()}<br/>" +
                           $"Category: {serviceRequest.Category?.Name}<br/>" +
                           $"Priority: {serviceRequest.Priority?.Name ?? "Not set"}<br/>" +
                           $"Reported By: {serviceRequest.ReportedByName} {serviceRequest.ReportedBySurname}<br/>" +
                           $"Date Submitted: {serviceRequest.DateSubmitted:dd MMMM yyyy HH:mm}<br/><br/>" +
                           "Please log in to the Property Lease Management system to action this request.<br/><br/>" +
                           "Regards,<br/>Property Lease Management System";

                QueueEmail(systemUser.EmailAddress, $"{cso.FirstName} {cso.LastName}", subject, body, serviceRequest.RequestReferenceNumber);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SendPaymentTransgressionNotification(PaymentTransgression transgression, string letterType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transgression.TenantEmail) &&
                    string.IsNullOrWhiteSpace(transgression.TenantCellphone))
                    return false;

                var subject = $"Payment Transgression Notice - {transgression.CaseReferenceNumber}";
                var body = $"A payment transgression has been recorded against your tenancy account.<br/>" +
                           $"Case Reference: <strong>{transgression.CaseReferenceNumber}</strong><br/>" +
                           $"Letter Type: {letterType}<br/>" +
                           $"Total Amount Due: R{transgression.TotalAmountDue:N2}<br/><br/>" +
                           "Please take immediate action to resolve this matter within <strong>7 working days</strong>.<br/>" +
                           "Failure to comply may result in further action including lease termination proceedings.<br/><br/>" +
                           "Regards,<br/>Property Lease Management Team";

                QueueEmail(transgression.TenantEmail, $"{transgression.TenantName} {transgression.TenantSurname}", subject, body, transgression.CaseReferenceNumber);
                QueueSms(transgression.TenantCellphone, $"Payment Transgression {transgression.CaseReferenceNumber}: R{transgression.TotalAmountDue:N2} outstanding. Action required within 7 days. Check email. PLM");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SendWarningLetter(TenantComplaint complaint, int warningNumber)
        {
            try
            {
                var respondent = GetRespondentContactInfo(complaint);
                if (respondent == null) return false;

                var subject = $"Warning Letter #{warningNumber} - {complaint.CaseReferenceNumber}";
                var finalWarning = warningNumber >= 3
                    ? "<strong>FINAL WARNING:</strong> This is your third and final warning. Failure to comply may result in lease termination proceedings."
                    : "Please take immediate action to address this matter. Further violations may result in additional warnings and potential lease termination.";

                var body = $"This is official warning <strong>#{warningNumber} of 3</strong> regarding complaint case <strong>{complaint.CaseReferenceNumber}</strong>.<br/>" +
                           $"Violation Category: {complaint.ComplaintCategory?.Name}<br/><br/>" +
                           $"{finalWarning}<br/><br/>" +
                           "If you have any questions, please contact our Client Services team immediately.<br/><br/>" +
                           "Regards,<br/>Property Lease Management Team";

                QueueEmail(respondent.Email, $"{complaint.RespondentFirstName} {complaint.RespondentSurname}", subject, body, complaint.CaseReferenceNumber);
                QueueSms(respondent.Cellphone, $"Warning #{warningNumber}: Complaint {complaint.CaseReferenceNumber}. {(warningNumber >= 3 ? "FINAL WARNING. " : "")}Check email for details. PLM");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private RespondentContactInfo GetRespondentContactInfo(TenantComplaint complaint)
        {
            var allocatedProperty = _db.ApplicationAllocatedProperty
                .Where(aap => aap.OfferedComplexId == complaint.RespondentComplexId &&
                             aap.SpaceUnitNumber == complaint.RespondentUnitNumber &&
                             (aap.BuildingName == null || aap.BuildingName == "" ||
                              aap.BuildingName == complaint.RespondentBlockNumber) &&
                             aap.IsActive && !aap.IsDeleted)
                .OrderByDescending(aap => aap.Id)
                .FirstOrDefault();

            if (allocatedProperty == null) return null;

            var matchedUnit = _db.MatchedUnits
                .FirstOrDefault(mu => mu.ApplicationAllocatedPropertyId == allocatedProperty.Id
                                   && mu.PropertyLeaseApplicationId != null);

            if (matchedUnit?.PropertyLeaseApplicationId == null) return null;

            var application = _db.PropertyLeaseApplications.Find(matchedUnit.PropertyLeaseApplicationId);
            if (application == null) return null;

            var customer = _db.Customers.Find(application.CustomerId);
            if (customer == null) return null;

            var systemUser = _db.SystemUsers.Find(customer.SystemUserId);
            if (systemUser == null) return null;

            return new RespondentContactInfo
            {
                Email = systemUser.EmailAddress,
                Cellphone = application.CellNo
            };
        }

        /// <summary>
        /// Queues an SLA escalation email (used by ComplaintSLAEngine).
        /// </summary>
        public void QueueEscalationEmail(string recipientEmail, string recipientName, string subject, string body, string referenceNumber)
        {
            QueueEmail(recipientEmail, recipientName, subject, body, referenceNumber);
        }

        private class RespondentContactInfo
        {
            public string Email { get; set; }
            public string Cellphone { get; set; }
        }
    }
}
