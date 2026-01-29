using C8.eServices.Mvc.ApiServices;
using C8.eServices.Mvc.Controllers;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using iTextSharp.tool.xml.html;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq;
using System.Net;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web;

using System.Web.Mvc;
using System.Web.Routing;


namespace C8.eServices.Mvc.Helpers
{
    public class EmailHelper
    {
        private eServicesDbContext context = new eServicesDbContext();
        private CesarDbContext core = new CesarDbContext();

        private string _username = @"Ekurhuleni/Emmbpmdev";//"smtpdev @ithinkweb.co.za";//"noreply@calc8.co.za";
        private string _password = "#Eku2019!";//"dev @itw@Pass123";//"kFqR72JBUK?x";
        private string _smtpServer = "10.31.3.24";//"mail.ithinkweb.co.za";//"host27.axxesslocal.co.za";
        private int _smtpPort = 25;

        public static string PrepareEmailBody(string emailBody)
        {
            if (string.IsNullOrWhiteSpace(emailBody)) return string.Empty;

            var looksLikeHtml = Regex.IsMatch(
                emailBody, @"^\s*<(?:!DOCTYPE|html|head|body|div|table|p|span)\b",
                RegexOptions.IgnoreCase);

            if (looksLikeHtml) return emailBody;

            emailBody = emailBody.Replace(@"\r\n", "\n")
                                 .Replace(@"\r", "\n")
                                 .Replace(@"\n", "\n");

            var encoded = WebUtility.HtmlEncode(emailBody);
            return encoded.Replace("\r\n", "<br/>")
                          .Replace("\n", "<br/>")
                          .Replace("\r", "<br/>");
        }

        // Normalize HTML/email body into clean SMS text (keeps newlines, removes tags)
        private static string FormatSmsText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Replace common HTML line breaks with \n first
            var s = Regex.Replace(input, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);

            // Remove remaining tags
            s = Regex.Replace(s, "<.*?>", string.Empty);

            // Decode HTML entities
            s = HttpUtility.HtmlDecode(s ?? string.Empty);

            // Normalize CRLF to \n
            s = s.Replace("\r\n", "\n").Replace("\r", "\n");

            // Collapse multiple newlines to a single newline
            s = Regex.Replace(s, @"\n{2,}", "\n\n");

            // Replace multiple spaces/tabs with single space
            s = Regex.Replace(s, @"[ \t]{2,}", " ");

            // Trim
            s = s.Trim();

            return s;
        }
        public string Recipient { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public void SendEmail()
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(_smtpServer);

            mail.IsBodyHtml = true;
            mail.From = new MailAddress(!_username.Contains("@") ? "noreply@ekurhuleni.gov.za" : _username);
            mail.To.Add(Recipient);
            mail.Subject = Subject;
            mail.Body = Body;

            SmtpServer.Port = _smtpPort;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential(_username, _password);
            SmtpServer.EnableSsl = true;
            SmtpServer.Timeout = 100000;

            SmtpServer.Send(mail);
        }

        public static bool CustomerEmailNotification(eServicesDbContext context,int PropertyId, int EmailBodyId, string emailSubject = null, string appendedBody = null)
        {
            CesarDbContext core = new CesarDbContext();
            
            try
            {
                Email SendMail = new Email();
                var getemailbody = context.EmailContentTypes.Where(x => x.Id == EmailBodyId).FirstOrDefault();
                var emailTemp = AppSettingKeys.PropertyLeaseManagementDefaultEmailTempate;
                var appJoin = context.ApplicantJoints.FirstOrDefault(x => x.PropertyLeaseApplicationId == PropertyId && x.IDNo != null);
                string ccList = null;
                string bccList = null;
                string subject = emailSubject !=null ? emailSubject : "PLM-Online Application";
                bool hasAttachment = false;
                string referenceId = "1";
                var RiskAssessmentOutcome = context.RiskAssessmentOutcomes.OrderByDescending(x=>x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == PropertyId);
                var propertyLeaseApplication = context.PropertyLeaseApplications.Include(r=>r.TitleType).Include(r=>r.Status).Include(r=>r.Customer).Where(x => x.Id == PropertyId).FirstOrDefault();
                var appunit = context.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id);
                var findItem = context.LeaseDetails.OrderByDescending(x=>x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id);
                if (appJoin != null || propertyLeaseApplication.SecondApplicant) ccList = propertyLeaseApplication.SecondApplicant == true ? propertyLeaseApplication.SecAppEmail : appJoin.JointEmail;
                string recipientMobileNumber = propertyLeaseApplication.CellNo;
                var schedule = context.InspectionSchedules.OrderByDescending(x=>x.Id).Include(r=>r.DateToSchedule).Include(r=>r.TimeSlot).FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id && x.IsApproved && !x.IsDeleted && x.IsActive && !x.IsInspected);
                var meeting = context.MeetingRequests.OrderByDescending(r=>r.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id);
                var CustomerId = context.Customers.FirstOrDefault(x => x.Id == propertyLeaseApplication.CustomerId).Id;
                var BackOfficeClerk = context.Customers.Where(x => x.Id == CustomerId).FirstOrDefault();
                
                var User = context.SystemUsers.Where(x => x.Id == propertyLeaseApplication.Customer.SystemUserId).FirstOrDefault();
                string attorneyemail = propertyLeaseApplication.PurEmail == User.EmailAddress ? propertyLeaseApplication.PurEmail : User.EmailAddress;

                string attorneyname = propertyLeaseApplication.FirstName + " " + propertyLeaseApplication.LastName;
                string emailbody = getemailbody.Description;

                if (getemailbody.Key == EmailContentKeys.ApplicationCaptureEmail)
                {
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.Status.Name);
                    emailbody = emailbody.Replace("{1}", propertyLeaseApplication.ApplicationReferenceNumber);
                }

                if (getemailbody.Key == EmailContentKeys.ApplicationFeeUploaded)
                {
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);

                }

                if (getemailbody.Key == EmailContentKeys.InviteTenantForTraining)
                {
                    var add = ", additional comments: " + meeting.Comment;
                    var mpt = "";
                    emailbody = emailbody.Replace("{date}", meeting.MeetingDate.ToString().Substring(0,10));
                    emailbody = emailbody.Replace("{venue}", meeting.MeetingVenue);
                    emailbody = meeting.Comment != null? emailbody.Replace("{add}", add): emailbody.Replace("{add}", mpt);
                 
                    var customerBo = context.Customers.FirstOrDefault(x => x.Id == meeting.CustomerId);
                    var SystemuserBO = context.SystemUsers.FirstOrDefault(x => x.Id == customerBo.SystemUserId);
                    var test = meeting.CustomerId;
                    emailbody = emailbody + " Email address: "+ SystemuserBO.EmailAddress;
                }

                if (getemailbody.Key == EmailContentKeys.ApplicationAssignedToRiskAssessment)
                {
                    emailbody = emailbody.Replace("{1}", propertyLeaseApplication.Status.Name);
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                }

                if (getemailbody.Key == EmailContentKeys.ApplicationRiskAssessmentApproved)
                {
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", propertyLeaseApplication.Status.Name);
                }

                if (getemailbody.Key == EmailContentKeys.InActionGenerateLeaseAgreement)
                {
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.Status.Name);
                    emailbody = emailbody.Replace("{1}", propertyLeaseApplication.ApplicationReferenceNumber);
                }

                if (getemailbody.Key == EmailContentKeys.GenerateDebitOrderAuthorityForm)
                {
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", propertyLeaseApplication.CreatedDateTime.ToString().Substring(0,10));
                }

                if (getemailbody.Key == EmailContentKeys.BackOfficeApproveDebitOrder)
                {
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", propertyLeaseApplication.CreatedDateTime.ToString().Substring(0,10));
                }

                if (getemailbody.Key == EmailContentKeys.UnitInspectionScheduleByApplicant)
                {
                    emailbody = emailbody.Replace("{date}", schedule.DateToSchedule.ShecduleDate.ToString().Substring(0, 10));
                    emailbody = emailbody.Replace("{time}", schedule.TimeSlot.Name);
                }

                if (getemailbody.Key == EmailContentKeys.DebitOrderRejectByBO)
                {
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                }else
                {
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                }

                if (getemailbody.Key == EmailContentKeys.ServeNotice)
                {
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", propertyLeaseApplication.ServeNoticeDate.ToString().Substring(0, 10));
                }
                if (getemailbody.Key == EmailContentKeys.RenewalApprovedRecommendation)
                {
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", findItem.MonthsOffered.ToString());
                }
                if (getemailbody.Key == EmailContentKeys.Accetproperty)
                {
                    CultureInfo zar = new CultureInfo("en-ZA");
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{R}", appunit.OutstandingDepopsitAmount.ToString("C", zar));
                }
                if (getemailbody.Key == EmailContentKeys.RiskAssessmentRejectUnaffordability)
                {
                    CultureInfo zar = new CultureInfo("en-ZA");
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", RiskAssessmentOutcome?.Reason ?? "");
                }
                if (getemailbody.Key == EmailContentKeys.RiskAssessmentRejectDocuments)
                {
                    CultureInfo zar = new CultureInfo("en-ZA");
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", RiskAssessmentOutcome?.Reason ?? "");
                }

                if (getemailbody.Key == EmailContentKeys.RiskAssessmentRejectITC)
                {
                    CultureInfo zar = new CultureInfo("en-ZA");
                    emailbody = emailbody.Replace("{0}", propertyLeaseApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", RiskAssessmentOutcome?.Reason ?? "");
                }


                if (!string.IsNullOrEmpty(appendedBody))
                {
                    emailbody = emailbody + appendedBody;
                }
                var textMessage = emailbody;
                var systemusermobilenum = BackOfficeClerk.CellPhoneNumber;
                var ActivityTrackerMessageEmail = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + emailbody;

                if (attorneyemail == null)
                    return false;

                var emailApplication = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = context.AppSettings.FirstOrDefault(a => a.Key == emailTemp);

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var emailBody = emailTemplate.Value;
                emailBody = emailBody.Replace("#NAME#", (attorneyname != "") ? attorneyname : "Sir/ Madam");
                emailBody = emailBody.Replace("#BODYTEXT#", emailbody);

                if (!string.IsNullOrEmpty(propertyLeaseApplication.ApplicationReferenceNumber))
                    emailBody = emailBody.Replace("#REFERENCENUMBER#", propertyLeaseApplication.ApplicationReferenceNumber);


                string mailBody = EmailHelper.PrepareEmailBody(emailBody);
                var email = new EmailQueueItem
                {
                    ApplicationId = Convert.ToInt32(emailApplication.Value),
                    QueueDateTime = DateTime.Now,
                    EmailAccountId = Convert.ToInt32(emailAccount.Value),
                    ToList = attorneyemail,
                    CcList = (ccList != "") ? ccList : null,
                    BccList = (bccList != "") ? bccList : null,
                    Subject = subject,
                    Body = mailBody,
                    IsHtml = true,
                    FailureCount = 0,
                    ReferenceId = referenceId,
                    HasAttachments = hasAttachment,
                    StatusId = 1
                };

                core.EmailQueue.Add(email);
                core.SaveChanges();


                if (recipientMobileNumber == null)
                    return true;

                var smsApplication = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var smsAccount = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);

                if (smsApplication == null) throw new Exception("Invalid Application setting");
                if (smsAccount == null) throw new Exception("Invalid Application setting");

                var statusIdSms = context.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                var smsText = FormatSmsText(textMessage);

                var sms = new SmsQueueItem
                {
                    ApplicationId = 1,
                    QueueDateTime = DateTime.Now,
                    SmsAccountId = 1,
                    MobileNumber = recipientMobileNumber,
                    TextMessage = smsText,
                    FailureCount = 0,
                    ReferenceId = referenceId,
                    StatusId = statusIdSms
                };
                core.SmsQueue.Add(sms);
                core.SaveChanges();

                var PlmHistoryLog = new PLMApplicationHistortyLog
                {
                    PropertyLeaseApplicationId = propertyLeaseApplication.Id,
                    AuditAction = ActivityTrackerMessageEmail,
                    UserId = CustomerId,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                context.PLMApplicationHistortyLogs.Add(PlmHistoryLog);
                context.SaveChanges();

                return true;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }
        }




        public static bool CustomerEmailOrSMSNotification(eServicesDbContext context, int ApplicationId, int EmailBodyId)
        {
            CesarDbContext core = new CesarDbContext();
            try
            {
                Email SendMail = new Email();
                var getemailbody = context.EmailContentTypes.Where(x => x.Id == EmailBodyId).FirstOrDefault();
                var emailTemp = AppSettingKeys.PropertyLeaseManagementDefaultEmailTempate;
                string ccList = null;
                string bccList = null;
                string subject = "PLM-Online Application";
                bool hasAttachment = false;
                string referenceId = "1";
                var HumanApplication = context.HumanSettlementApplications.Include(r => r.TitleType).Include(r => r.Status).Include(r => r.Customer).FirstOrDefault(x => x.Id == ApplicationId);
                var Master = (HumanApplication.IsMaster) ? context.HumanSettlementLeaseMasters.FirstOrDefault(x => x.HumanSettlementApplicationId == HumanApplication.Id) : null;
                var findItem = new HumanSettlementLeaseDetails();
                if (Master == null ) findItem = context.HumanSettlementLeaseDetails.FirstOrDefault(x => x.IsActive);
                string recipientMobileNumber = HumanApplication.CellNo;
                //var schedule = context.InspectionSchedules.OrderByDescending(x => x.Id).Include(r => r.DateToSchedule).Include(r => r.TimeSlot).FirstOrDefault(x => x.PropertyLeaseApplicationId == HumanApplication.Id && x.IsApproved && !x.IsDeleted && x.IsActive && !x.IsInspected);
                //var meeting = context.MeetingRequests.OrderByDescending(r => r.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == HumanApplication.Id);
                var CustomerId = context.Customers.FirstOrDefault(x => x.Id == HumanApplication.CustomerId).Id;
                var BackOfficeClerk = context.Customers.Where(x => x.Id == CustomerId).FirstOrDefault();

                var User = context.SystemUsers.Where(x => x.Id == HumanApplication.Customer.SystemUserId).FirstOrDefault();
                string attorneyemail = HumanApplication.PurEmail /*== User.EmailAddress ? HumanApplication.PurEmail : User.EmailAddress*/;

                string attorneyname = HumanApplication.FirstName + " " + HumanApplication.LastName;
                string emailbody = getemailbody.Description;

                if (getemailbody.Key == EmailContentKeys.ApplicationCaptureEmail)
                {
                    emailbody = emailbody.Replace("{0}", HumanApplication.Status.Name);
                    emailbody = emailbody.Replace("{1}", HumanApplication.ApplicationReferenceNumber);
                }

                if (getemailbody.Key == EmailContentKeys.ApplicationAssignedToRiskAssessment)
                {
                    emailbody = emailbody.Replace("{1}", HumanApplication.Status.Name);
                    emailbody = emailbody.Replace("{0}", HumanApplication.ApplicationReferenceNumber);
                }

                if (getemailbody.Key == EmailContentKeys.ApplicationRiskAssessmentApproved)
                {
                    emailbody = emailbody.Replace("{0}", HumanApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", HumanApplication.Status.Name);
                }

                if (getemailbody.Key == EmailContentKeys.AgreementRenewalApproved)
                {
                    emailbody = string.Format(emailbody, HumanApplication.ApplicationReferenceNumber, HumanApplication.OTP);
                }

                if (getemailbody.Key == EmailContentKeys.InActionGenerateLeaseAgreement)
                {
                    emailbody = emailbody.Replace("{0}", HumanApplication.Status.Name);
                    emailbody = emailbody.Replace("{1}", HumanApplication.ApplicationReferenceNumber);
                }

                if (getemailbody.Key == EmailContentKeys.GenerateDebitOrderAuthorityForm)
                {
                    emailbody = emailbody.Replace("{0}", HumanApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", HumanApplication.CreatedDateTime.ToString().Substring(0, 10));
                }

                if (getemailbody.Key == EmailContentKeys.BackOfficeApproveDebitOrder)
                {
                    emailbody = emailbody.Replace("{0}", HumanApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", HumanApplication.CreatedDateTime.ToString().Substring(0, 10));
                }

                if (getemailbody.Key == EmailContentKeys.TransferSuccessful)
                {
                    emailbody = string.Format(emailbody, HumanApplication.ApplicationReferenceNumber, HumanApplication.FirstName, HumanApplication.LastName);
                }

                if (getemailbody.Key == EmailContentKeys.DocumentUploadRenewal)
                {
                    emailbody = string.Format(emailbody, HumanApplication.ApplicationReferenceNumber, 
                        (HumanApplication.UnitCategory?.Key == UnitCategoryKeys.Dormitory) ? 12 : 24,
                        HumanApplication.IsMaster ? Master.EndDate.ToString().Substring(0, 10) : findItem.EndDate.ToString().Substring(0, 10));
                }
              

                if (getemailbody.Key == EmailContentKeys.AnnualRentalEscalation)
                {
                    emailbody = string.Format(emailbody, HumanApplication.IsMaster ? Master.RentalAmount.ToString() : findItem.RentalAmount.ToString());
                }

                if (getemailbody.Key == EmailContentKeys.ServeNotice)
                {
                    emailbody = emailbody.Replace("{0}", HumanApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", HumanApplication.ServeNoticeDate.ToString().Substring(0, 10));
                    emailbody = emailbody.Replace("Letting Officer", "Housing Liason Officer");
                }
                if (getemailbody.Key == EmailContentKeys.RenewalApprovedRecommendation)
                {
                    emailbody = emailbody.Replace("{0}", HumanApplication.ApplicationReferenceNumber);
                    emailbody = emailbody.Replace("{1}", findItem.MonthsOffered.ToString());
                }
                if (getemailbody.Key == EmailContentKeys.Accetproperty)
                {
                    CultureInfo zar = new CultureInfo("en-ZA");
                    emailbody = emailbody.Replace("{0}", HumanApplication.ApplicationReferenceNumber);
                }
                if (getemailbody.Key == EmailContentKeys.DebitOrderRejectByBO)
                {
                    emailbody = emailbody.Replace("{0}", HumanApplication.ApplicationReferenceNumber);
                }
                else
                {
                    emailbody = emailbody.Replace("{0}", HumanApplication.ApplicationReferenceNumber);
                }



                var textMessage = emailbody;
                var systemusermobilenum = BackOfficeClerk.CellPhoneNumber;
                var ActivityTrackerMessageEmail = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + emailbody;

                if (attorneyemail == null)
                    return false;

                var emailApplication = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = context.AppSettings.FirstOrDefault(a => a.Key == emailTemp);

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var emailBody = emailTemplate.Value;
                emailBody = emailBody.Replace("#NAME#", (attorneyname != "") ? attorneyname : "Sir/ Madam");
                emailBody = emailBody.Replace("#BODYTEXT#", emailbody);

                if (!string.IsNullOrEmpty(HumanApplication.ApplicationReferenceNumber))
                    emailBody = emailBody.Replace("#REFERENCENUMBER#", HumanApplication.ApplicationReferenceNumber);

                var email = new EmailQueueItem
                {
                    ApplicationId = Convert.ToInt32(emailApplication.Value),
                    QueueDateTime = DateTime.Now,
                    EmailAccountId = Convert.ToInt32(emailAccount.Value),
                    ToList = attorneyemail,
                    CcList = (ccList != "") ? ccList : null,
                    BccList = (bccList != "") ? bccList : null,
                    Subject = subject,
                    Body = emailBody,
                    IsHtml = true,
                    FailureCount = 0,
                    ReferenceId = referenceId,
                    HasAttachments = hasAttachment,
                    StatusId = 1
                };

                core.EmailQueue.Add(email);
                core.SaveChanges();


                if (recipientMobileNumber == null)
                    return true;

                var smsApplication = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var smsAccount = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);

                if (smsApplication == null) throw new Exception("Invalid Application setting");
                if (smsAccount == null) throw new Exception("Invalid Application setting");

                var statusIdSms = context.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;

                var smsText = FormatSmsText(textMessage);

                var sms = new SmsQueueItem
                {
                    ApplicationId = 1,
                    QueueDateTime = DateTime.Now,
                    SmsAccountId = 1,
                    MobileNumber = recipientMobileNumber,
                    TextMessage = smsText,
                    FailureCount = 0,
                    ReferenceId = referenceId,
                    StatusId = statusIdSms
                };
                core.SmsQueue.Add(sms);
                core.SaveChanges();

                var PlmHistoryLog = new PLMApplicationHistortyLog
                {
                    HumanSettlementApplicationId = HumanApplication.Id,
                    AuditAction = ActivityTrackerMessageEmail,
                    UserId = CustomerId,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                context.PLMApplicationHistortyLogs.Add(PlmHistoryLog);
                context.SaveChanges();

                return true;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }
        }

        public static bool BackOfficeNotification(eServicesDbContext db, int RCSAppID, int CustomerID, int EmailBodyId, string emailBodyExtended = null)
        {
            if (CustomerID == 0) return false;
            try
            {
                var getemailbody = db.EmailContentTypes.Where(x => x.Id == EmailBodyId).FirstOrDefault();
                Email SendMail = new Email();
                var RCSApplication = db.PropertyLeaseApplications.Where(x => x.Id == RCSAppID).FirstOrDefault();
                var BackOfficeClerk = db.Customers.Where(x => x.Id == CustomerID).FirstOrDefault();
                string attorneyemail = BackOfficeClerk.EmailAddress;
                string attorneyname = BackOfficeClerk.FirstName + " " + BackOfficeClerk.LastName;
                string emailbody = getemailbody.Description;
                if (emailBodyExtended != null)
                    emailbody = emailbody + " " + emailBodyExtended;

                if(getemailbody.Key == EmailContentKeys.RenewalRejectByCustomer)
                {
                    emailbody = emailbody.Replace("{0}", RCSApplication.ApplicationReferenceNumber);
                }
                string subject = getemailbody.Name;
                var systemusermobilenum = BackOfficeClerk.CellPhoneNumber;
                var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + emailbody;
                SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum, RCSApplication.Id, CustomerID, emailbody, attorneyemail, subject, emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                return true;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }
        }

        public static bool BackOfficeEmailNotification(eServicesDbContext context,int BackOfficeId,  int PropertyId, int EmailBodyId, bool ServeNotice)
        {
            CesarDbContext core = new CesarDbContext();
            try
            {
                Email SendMail = new Email();
                var getemailbody = context.EmailContentTypes.Where(x => x.Id == EmailBodyId).FirstOrDefault();
                var emailTemp = AppSettingKeys.PropertyLeaseManagementDefaultEmailTempate;
                string QueueName = "Capture";
                string ccList = null;
                string bccList = null;
                string subject = "PLM-Online Application";
                bool hasAttachment = false;
                string referenceId = "1";
                string recipientMobileNumber = "0799815312";
                var propertyLeaseApplication = context.PropertyLeaseApplications.Include(r => r.TitleType).Include(r => r.Status).Where(x => x.Id == PropertyId).FirstOrDefault();
                var schedule = context.InspectionSchedules.Include(r => r.DateToSchedule).Include(r => r.TimeSlot).FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id && x.IsApproved && !x.IsDeleted && x.IsActive && !x.IsInspected);
                var meeting = context.MeetingRequests.OrderByDescending(r => r.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplication.Id);
                var CustomerId = context.Customers.FirstOrDefault(x => x.Id == propertyLeaseApplication.CustomerId).Id;
                var BackOfficeClerk = context.Customers.Where(x => x.Id == BackOfficeId).FirstOrDefault();
                string attorneyemail = BackOfficeClerk.EmailAddress;

                string attorneyname = BackOfficeClerk.FirstName + " " + BackOfficeClerk.LastName;
                string emailbody = getemailbody.Description;

                var textMessage = emailbody;
                var systemusermobilenum = BackOfficeClerk.CellPhoneNumber;
                var ActivityTrackerMessageEmail = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + emailbody;

                if (attorneyemail == null)
                    return false;

                var emailApplication = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = context.AppSettings.FirstOrDefault(a => a.Key == emailTemp);

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var emailBody = emailTemplate.Value;
                emailBody = emailBody.Replace("#NAME#", (attorneyname != "") ? attorneyname : "Sir/ Madam");
                emailBody = emailBody.Replace("#BODYTEXT#", emailbody);

                if (!string.IsNullOrEmpty(propertyLeaseApplication.ApplicationReferenceNumber))
                    emailBody = emailBody.Replace("#REFERENCENUMBER#", propertyLeaseApplication.ApplicationReferenceNumber);

                var email = new EmailQueueItem
                {
                    ApplicationId = Convert.ToInt32(emailApplication.Value),
                    QueueDateTime = DateTime.Now,
                    EmailAccountId = Convert.ToInt32(emailAccount.Value),
                    ToList = attorneyemail,
                    CcList = (ccList != "") ? ccList : null,
                    BccList = (bccList != "") ? bccList : null,
                    Subject = subject,
                    Body = emailBody,
                    IsHtml = true,
                    FailureCount = 0,
                    ReferenceId = referenceId,
                    HasAttachments = hasAttachment
                };

                core.EmailQueue.Add(email);
                core.SaveChanges();


                if (recipientMobileNumber == null)
                    return true;

                var smsApplication = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var smsAccount = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);

                if (smsApplication == null) throw new Exception("Invalid Application setting");
                if (smsAccount == null) throw new Exception("Invalid Application setting");

                var statusIdSms = context.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                var sms = new SmsQueueItem
                {
                    ApplicationId = 1,
                    QueueDateTime = DateTime.Now,
                    SmsAccountId = 1,
                    MobileNumber = recipientMobileNumber,
                    TextMessage = textMessage,
                    FailureCount = 0,
                    ReferenceId = referenceId,
                    StatusId = statusIdSms
                };
                core.SmsQueue.Add(sms);
                core.SaveChanges();

                var PlmHistoryLog = new PLMApplicationHistortyLog
                {
                    PropertyLeaseApplicationId = propertyLeaseApplication.Id,
                    AuditAction = ActivityTrackerMessageEmail,
                    UserId = CustomerId,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                context.PLMApplicationHistortyLogs.Add(PlmHistoryLog);
                context.SaveChanges();

                return true;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }
        }
































    }
}