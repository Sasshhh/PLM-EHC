using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Models
{
    public class Email
    {
        private eServicesDbContext db = new eServicesDbContext();
        private CesarDbContext core = new CesarDbContext();

        public Email()
        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }

        public void GenerateEmail(string recipientEmail, string subject, string body,
        string referenceId, bool hasAttachment, string emailTemp, string recipientName = "", string MobileNumber = "", string ccList = "",
        string bccList = "", string referenceNumber = "", byte[] attachment = null, string attachmentName = "", List<byte[]> attachments = null, List<string> fileNames = null)
        {
            try
            {
                if (recipientEmail == null)
                    return;

                var emailApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = db.AppSettings.FirstOrDefault(a => a.Key == emailTemp);
                var statusIdSms = db.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var emailBody = emailTemplate.Value;
                emailBody = emailBody.Replace("#NAME#", (recipientName != "") ? recipientName : "Sir/ Madam");
                emailBody = emailBody.Replace("#BODYTEXT#", body);

                if (!string.IsNullOrEmpty(referenceNumber))
                    emailBody = emailBody.Replace("#REFERENCENUMBER#", referenceNumber);

                var email = new EmailQueueItem
                {
                    ApplicationId = Convert.ToInt32(emailApplication.Value),
                    QueueDateTime = DateTime.Now,
                    EmailAccountId = Convert.ToInt32(emailAccount.Value),
                    ToList = recipientEmail,
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

                if (!string.IsNullOrEmpty(MobileNumber))
                {
                    body = string.Format("Dear <b>{0}</b> <br/><br/>{1}", recipientName, body);
                    //Remove Html tags before sms sending
                    body = RemoveHtmlTagsFromSms(body);

                    var SMS = new SmsQueueItem
                    {
                        ApplicationId = 1,
                        QueueDateTime = DateTime.Now,
                        SmsAccountId = 1,
                        MobileNumber = MobileNumber,
                        TextMessage = body,
                        FailureCount = 0,
                        ReferenceId = null,
                        StatusId = statusIdSms
                    };
                    core.SmsQueue.Add(SMS);
                    core.SaveChanges();
                }
                
                //Single attachments
                if (!hasAttachment) return;
                if (attachment != null)
                {
                    AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachment, email.EmailQueueId, attachmentName);
                }

                //Attach more than 1 attachments
                if (attachments == null || fileNames == null) return;
                for (var i = 0; i < attachments.Count; i++)
                {
                    if (attachments[i] != null && fileNames[i] != null)
                    {
                        AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachments[i], email.EmailQueueId, fileNames[i]);
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }


        }

        public string GenerateBulkEmails(List<SystemUser> Users, string subject, string mesagebody, bool sms, bool mail, bool postal, string emailTemp, List<PropertyLeaseApplication> applications,int UserId , bool BulkMessages, string recipientName, string referenceNumber=null)
        {
            try
            {
                if (Users == null)
                    return "No messages sent."; ;

                var total = Users.Count.ToString();

                var emailApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = db.AppSettings.FirstOrDefault(a => a.Key == emailTemp);

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var smsApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var smsAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);

                if (smsApplication == null) throw new Exception("Invalid Application setting");
                if (smsAccount == null) throw new Exception("Invalid Application setting");

                var statusIdSms = db.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;

                var emailBody = emailTemplate.Value;
                
                emailBody = emailBody.Replace("#BODYTEXT#", mesagebody);
                emailBody = emailBody.Replace("Regards,", "");
                emailBody = emailBody.Replace("PLM Team", "");

                if (!string.IsNullOrEmpty(referenceNumber))
                    emailBody = emailBody.Replace("#REFERENCENUMBER#", referenceNumber);
                int sent = 0;
                if (BulkMessages)
                {
                    emailBody = emailBody.Replace("#NAME#", recipientName);

                    List<EmailQueueItem> mails = new List<EmailQueueItem>();
                    foreach (var e in Users)
                    {
                        var appId = applications.FirstOrDefault(x => x.SystemUserId == e.Id);

                        if (mail)
                        {
                            var email = new EmailQueueItem
                            {
                                ApplicationId = Convert.ToInt32(emailApplication.Value),
                                QueueDateTime = DateTime.Now,
                                EmailAccountId = Convert.ToInt32(emailAccount.Value),
                                ToList = e.EmailAddress,
                                CcList = null,
                                BccList = null,
                                Subject = subject,
                                Body = emailBody,
                                IsHtml = true,
                                FailureCount = 0,
                                ReferenceId = null,
                                HasAttachments = false,
                                StatusId = 1
                            };
                            core.EmailQueue.Add(email);
                        }
                        if (sms)
                        {
                            var SMS = new SmsQueueItem
                            {
                                ApplicationId = 1,
                                QueueDateTime = DateTime.Now,
                                SmsAccountId = 1,
                                MobileNumber = e.MobileNumber,
                                TextMessage = emailBody,
                                FailureCount = 0,
                                ReferenceId = null,
                                StatusId = statusIdSms
                            };
                            core.SmsQueue.Add(SMS);
                        }
                        if (postal)
                        {

                        }
                        core.SaveChanges();

                        var PlmHistoryLog = new PLMApplicationHistortyLog
                        {
                            PropertyLeaseApplicationId = appId.Id,
                            AuditAction = emailBody,
                            UserId = UserId,
                            CreatedDateTime = DateTime.Now,
                            IsActive = true
                        };
                        db.PLMApplicationHistortyLogs.Add(PlmHistoryLog);
                        db.SaveChanges();
                        sent++;
                    }
                }
                else
                {
                    var e = Users.FirstOrDefault();
                    var appId = applications.FirstOrDefault(x => x.SystemUserId == e.Id);
                    emailBody = emailBody.Replace("#NAME#", e.FullName);

                    if (mail)
                    {
                        var email = new EmailQueueItem
                        {
                            ApplicationId = Convert.ToInt32(emailApplication.Value),
                            QueueDateTime = DateTime.Now,
                            EmailAccountId = Convert.ToInt32(emailAccount.Value),
                            ToList = e.EmailAddress,
                            CcList = null,
                            BccList = null,
                            Subject = subject,
                            Body = emailBody,
                            IsHtml = true,
                            FailureCount = 0,
                            ReferenceId = null,
                            HasAttachments = false,
                            StatusId = 1
                        };
                        core.EmailQueue.Add(email);
                    }
                    if (sms)
                    {
                        var SMS = new SmsQueueItem
                        {
                            ApplicationId = 1,
                            QueueDateTime = DateTime.Now,
                            SmsAccountId = 1,
                            MobileNumber = e.MobileNumber,
                            TextMessage = emailBody,
                            FailureCount = 0,
                            ReferenceId = null,
                            StatusId = statusIdSms
                        };
                        core.SmsQueue.Add(SMS);
                    }
                    if (postal)
                    {

                    }
                    core.SaveChanges();
                    sent++;

                    var PlmHistoryLog = new PLMApplicationHistortyLog
                    {
                        PropertyLeaseApplicationId = appId.Id,
                        AuditAction = emailBody,
                        UserId = UserId,
                        CreatedDateTime = DateTime.Now,
                        IsActive = true
                    };
                    db.PLMApplicationHistortyLogs.Add(PlmHistoryLog);
                    db.SaveChanges();
                }
                return sent.ToString()+"/"+total+" message/s";

            }
            catch (Exception ex)
            {
                throw;
            }


        }
        public string GenerateBulkEmailsOrSMS(List<SystemUser> Users, string subject, string mesagebody, bool sms, bool mail, bool postal, string emailTemp, List<HumanSettlementApplication> applications, int UserId, bool BulkMessages, string recipientName, string referenceNumber = null)
        {
            try
            {
                if (Users == null)
                    return "No messages sent."; ;

                var total = Users.Count.ToString();

                var emailApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = db.AppSettings.FirstOrDefault(a => a.Key == emailTemp);

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var smsApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var smsAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);

                if (smsApplication == null) throw new Exception("Invalid Application setting");
                if (smsAccount == null) throw new Exception("Invalid Application setting");

                var statusIdSms = db.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;

                var emailBody = emailTemplate.Value;

                emailBody = emailBody.Replace("#BODYTEXT#", mesagebody);
                emailBody = emailBody.Replace("Regards,", "");
                emailBody = emailBody.Replace("PLM Team", "");

                if (!string.IsNullOrEmpty(referenceNumber))
                    emailBody = emailBody.Replace("#REFERENCENUMBER#", referenceNumber);
                int sent = 0;
                if (BulkMessages)
                {
                    emailBody = emailBody.Replace("#NAME#", recipientName);

                    List<EmailQueueItem> mails = new List<EmailQueueItem>();
                    foreach (var e in Users)
                    {
                        var appId = applications.FirstOrDefault(x => x.SystemUserId == e.Id);

                        if (mail)
                        {
                            var email = new EmailQueueItem
                            {
                                ApplicationId = Convert.ToInt32(emailApplication.Value),
                                QueueDateTime = DateTime.Now,
                                EmailAccountId = Convert.ToInt32(emailAccount.Value),
                                ToList = appId?.PurEmail??e.EmailAddress,
                                CcList = null,
                                BccList = null,
                                Subject = subject,
                                Body = emailBody,
                                IsHtml = true,
                                FailureCount = 0,
                                ReferenceId = null,
                                HasAttachments = false
                            };
                            core.EmailQueue.Add(email);
                        }
                        if (sms)
                        {
                            var SMS = new SmsQueueItem
                            {
                                ApplicationId = 1,
                                QueueDateTime = DateTime.Now,
                                SmsAccountId = 1,
                                MobileNumber = appId.CellNo,
                                TextMessage = emailBody,
                                FailureCount = 0,
                                ReferenceId = null,
                                StatusId = statusIdSms
                            };
                            if (!String.IsNullOrEmpty(appId.CellNo)) core.SmsQueue.Add(SMS);
                        }
                        if (postal)
                        {

                        }
                        core.SaveChanges();

                        var PlmHistoryLog = new PLMApplicationHistortyLog
                        {
                            HumanSettlementApplicationId = appId.Id,
                            AuditAction = emailBody,
                            UserId = UserId,
                            CreatedDateTime = DateTime.Now,
                            IsActive = true
                        };
                        db.PLMApplicationHistortyLogs.Add(PlmHistoryLog);
                        db.SaveChanges();
                        sent++;
                    }
                }
                else
                {
                    var e = Users.FirstOrDefault();
                    var appId = applications.FirstOrDefault(x => x.SystemUserId == e.Id);
                    emailBody = emailBody.Replace("#NAME#", e.FullName);

                    if (mail)
                    {
                        var email = new EmailQueueItem
                        {
                            ApplicationId = Convert.ToInt32(emailApplication.Value),
                            QueueDateTime = DateTime.Now,
                            EmailAccountId = Convert.ToInt32(emailAccount.Value),
                            ToList = appId?.PurEmail??e.EmailAddress,
                            CcList = null,
                            BccList = null,
                            Subject = subject,
                            Body = emailBody,
                            IsHtml = true,
                            FailureCount = 0,
                            ReferenceId = null,
                            HasAttachments = false
                        };
                        core.EmailQueue.Add(email);
                    }
                    if (sms)
                    {
                        var SMS = new SmsQueueItem
                        {
                            ApplicationId = 1,
                            QueueDateTime = DateTime.Now,
                            SmsAccountId = 1,
                            MobileNumber = appId.CellNo,
                            TextMessage = emailBody,
                            FailureCount = 0,
                            ReferenceId = null,
                            StatusId = statusIdSms
                        };
                        if (!String.IsNullOrEmpty(appId.CellNo)) core.SmsQueue.Add(SMS);

                    }
                    if (postal)
                    {

                    }
                    core.SaveChanges();
                    sent++;

                    var PlmHistoryLog = new PLMApplicationHistortyLog
                    {
                        HumanSettlementApplicationId = appId.Id,
                        AuditAction = emailBody,
                        UserId = UserId,
                        CreatedDateTime = DateTime.Now,
                        IsActive = true
                    };
                    db.PLMApplicationHistortyLogs.Add(PlmHistoryLog);
                    db.SaveChanges();
                }
                return sent.ToString() + "/" + total + " message/s";

            }
            catch (Exception ex)
            {
                throw;
            }


        }


        public string GenerateBulkEmailsOrSMS2(string subject, string mesagebody, bool sms, bool mail, bool postal, string emailTemp, List<HumanSettlementApplication> Users, int UserId, bool BulkMessages, string recipientName, string smsbody = null)
        {
            try
            {
                if (Users == null)
                    return "No messages sent."; ;

                var total = Users.Count.ToString();

                var emailApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = db.AppSettings.FirstOrDefault(a => a.Key == emailTemp);

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var smsApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var smsAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);

                if (smsApplication == null) throw new Exception("Invalid Application setting");
                if (smsAccount == null) throw new Exception("Invalid Application setting");

                var statusIdSms = db.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;

                var emailBody = emailTemplate.Value;

                emailBody = emailBody.Replace("#BODYTEXT#", mesagebody);
                emailBody = emailBody.Replace("Regards,", "");
                emailBody = emailBody.Replace("PLM Team", "");


                int sent = 0;
                if (BulkMessages)
                {
                    emailBody = emailBody.Replace("#NAME#", recipientName);

                    List<EmailQueueItem> mails = new List<EmailQueueItem>();
                    foreach (var e in Users)
                    {
                        try
                        {
                            if (mail)
                            {
                                var email = new EmailQueueItem
                                {
                                    ApplicationId = Convert.ToInt32(emailApplication.Value),
                                    QueueDateTime = DateTime.Now,
                                    EmailAccountId = Convert.ToInt32(emailAccount.Value),
                                    ToList = e?.PurEmail ?? e.Customer.SystemUser.EmailAddress,
                                    CcList = null,
                                    BccList = null,
                                    Subject = subject,
                                    Body = emailBody,
                                    IsHtml = true,
                                    FailureCount = 0,
                                    ReferenceId = null,
                                    HasAttachments = false,
                                    StatusId = 1
                                };
                                if (!String.IsNullOrEmpty(email.ToList)) core.EmailQueue.Add(email);
                            }
                            if (sms)
                            {
                                smsbody = RemoveHtmlTagsFromSms(smsbody);
                                var SMS = new SmsQueueItem
                                {
                                    ApplicationId = 1,
                                    QueueDateTime = DateTime.Now,
                                    SmsAccountId = 1,
                                    MobileNumber = e?.CellNo ?? e.Customer.SystemUser.MobileNumber,
                                    TextMessage = smsbody,
                                    FailureCount = 0,
                                    ReferenceId = null,
                                    StatusId = statusIdSms
                                };
                                if (!String.IsNullOrEmpty(SMS.MobileNumber)) core.SmsQueue.Add(SMS);
                            }
                            core.SaveChanges();

                            var PlmHistoryLog = new PLMApplicationHistortyLog
                            {
                                HumanSettlementApplicationId = e.Id,
                                AuditAction = emailBody,
                                UserId = UserId,
                                CreatedDateTime = DateTime.Now,
                                IsActive = true
                            };
                            db.PLMApplicationHistortyLogs.Add(PlmHistoryLog);
                            db.SaveChanges();
                            sent++;
                        }
                        catch
                        {
                            //Do nothing
                        }
                       
                    }
                }
                else
                {
                    var e = Users.FirstOrDefault();
                    emailBody = emailBody.Replace("#NAME#", e.ApplicantFullName);

                    if (mail)
                    {
                        var email = new EmailQueueItem
                        {
                            ApplicationId = Convert.ToInt32(emailApplication.Value),
                            QueueDateTime = DateTime.Now,
                            EmailAccountId = Convert.ToInt32(emailAccount.Value),
                            ToList = e.PurEmail,
                            CcList = null,
                            BccList = null,
                            Subject = subject,
                            Body = emailBody,
                            IsHtml = true,
                            FailureCount = 0,
                            ReferenceId = null,
                            HasAttachments = false,
                            StatusId = 1
                        };
                        core.EmailQueue.Add(email);
                    }
                    if (sms)
                    {
                        smsbody = RemoveHtmlTagsFromSms(smsbody);

                        var SMS = new SmsQueueItem
                        {
                            ApplicationId = 1,
                            QueueDateTime = DateTime.Now,
                            SmsAccountId = 1,
                            MobileNumber = e.CellNo,
                            TextMessage = smsbody,
                            FailureCount = 0,
                            ReferenceId = null,
                            StatusId = statusIdSms
                        };
                        if (!String.IsNullOrEmpty(e.CellNo)) core.SmsQueue.Add(SMS);

                    }
                    if (postal)
                    {

                    }
                    core.SaveChanges();
                    sent++;

                    var PlmHistoryLog = new PLMApplicationHistortyLog
                    {
                        HumanSettlementApplicationId = e.Id,
                        AuditAction = emailBody,
                        UserId = UserId,
                        CreatedDateTime = DateTime.Now,
                        IsActive = true
                    };
                    db.PLMApplicationHistortyLogs.Add(PlmHistoryLog);
                    db.SaveChanges();
                }
                return sent.ToString() + "/" + total + " message/s";

            }
            catch (Exception ex)
            {
                throw;
            }


        }



        public void GenerateEmail2(string SmsNumber, int RCSAppID, int CustomerID,string EmailContent, string recipientEmail, string subject, string body,
string referenceId, bool hasAttachment, string emailTemp, string recipientName = "", string ccList = "",
string bccList = "", string referenceNumber = "", byte[] attachment = null, string attachmentName = "", List<byte[]> attachments = null, List<string> fileNames = null)
        {
            try
            {
                if (recipientEmail == null)
                    return;

                var emailApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = db.AppSettings.FirstOrDefault(a => a.Key == emailTemp);

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var emailBody = emailTemplate.Value;
                emailBody = emailBody.Replace("#NAME#", (recipientName != "") ? recipientName : "Sir/ Madam");
                emailBody = emailBody.Replace("#BODYTEXT#", body);

                if (!string.IsNullOrEmpty(referenceNumber))
                    emailBody = emailBody.Replace("#REFERENCENUMBER#", referenceNumber);

                var email = new EmailQueueItem
                {
                    ApplicationId = Convert.ToInt32(emailApplication.Value),
                    QueueDateTime = DateTime.Now,
                    EmailAccountId = Convert.ToInt32(emailAccount.Value),
                    ToList = recipientEmail,
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


              var RCSHistoryLog = new PLMApplicationHistortyLog
              {
                    PropertyLeaseApplicationId = RCSAppID,
                    AuditAction = EmailContent,
                    UserId = CustomerID,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.PLMApplicationHistortyLogs.Add(RCSHistoryLog);
                db.SaveChanges();
                //Single attachments
                if (!hasAttachment) return;
                if (attachment != null)
                {
                    AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachment, email.EmailQueueId, attachmentName);
                }

                //Attach more than 1 attachments
                if (attachments == null || fileNames == null) return;
                for (var i = 0; i < attachments.Count; i++)
                {
                    if (attachments[i] != null && fileNames[i] != null)
                    {
                        AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachments[i], email.EmailQueueId, fileNames[i]);
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void PLMGenerateEmailSMS(string activitycontent, string SmsNumber, int RCSAppID, int CustomerID, string EmailContent, string recipientEmail, string subject, string body,
string referenceId, bool hasAttachment, string emailTemp, string recipientName = "", string ccList = "",
string bccList = "", string referenceNumber = "", byte[] attachment = null, string attachmentName = "", List<byte[]> attachments = null, List<string> fileNames = null)
        {
            try
            {
                if (recipientEmail == null)
                    return;

                var emailApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = db.AppSettings.FirstOrDefault(a => a.Key == emailTemp);

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var emailBody = emailTemplate.Value;
                emailBody = emailBody.Replace("#NAME#", (recipientName != "") ? recipientName : "Sir/ Madam");
                emailBody = emailBody.Replace("#BODYTEXT#", body);

                if (!string.IsNullOrEmpty(referenceNumber))
                    emailBody = emailBody.Replace("#REFERENCENUMBER#", referenceNumber);

                var email = new EmailQueueItem
                {
                    ApplicationId = Convert.ToInt32(emailApplication.Value),
                    QueueDateTime = DateTime.Now,
                    EmailAccountId = Convert.ToInt32(emailAccount.Value),
                    ToList = recipientEmail,
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


                var RCSHistoryLog = new PLMApplicationHistortyLog
                {
                    PropertyLeaseApplicationId = RCSAppID,
                    AuditAction = activitycontent,
                    UserId = CustomerID,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.PLMApplicationHistortyLogs.Add(RCSHistoryLog);
                db.SaveChanges();

                if (SmsNumber != null && !SmsNumber.Trim().Equals(string.Empty))
                {
                    try
                    {
                        var smsConfirmation = new CesarSMS();
                        var cxt = new eServicesDbContext();
                        var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                        smsConfirmation.GenerateSMS(SmsNumber, EmailContent, CustomerID.ToString(), statusIdSms, recipientName);
                        //SmsHelper.Send(model.MobileNumber, string.Format("Welcome to Rates Clearance System, please confirm your account with the following code {0}.", code));
                    }
                    catch (Exception x)
                    {

                    }

                }
                //Single attachments
                if (!hasAttachment) return;
                if (attachment != null)
                {
                    AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachment, email.EmailQueueId, attachmentName);
                }

                //Attach more than 1 attachments
                if (attachments == null || fileNames == null) return;
                for (var i = 0; i < attachments.Count; i++)
                {
                    if (attachments[i] != null && fileNames[i] != null)
                    {
                        AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachments[i], email.EmailQueueId, fileNames[i]);
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }


        }
        public void GenerateEmailSMS2(string activitycontent, string SmsNumber, int RCSAppID, int CustomerID, string EmailContent, string recipientEmail, string subject, string body, string referenceId, bool hasAttachment, string emailTemp, string recipientName = "", string ccList = "", string bccList = "", string referenceNumber = "", byte[] attachment = null, string attachmentName = "", List<byte[]> attachments = null, List<string> fileNames = null)
        {
            try
            {
                if (recipientEmail == null)
                    return;

                var emailApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = db.AppSettings.FirstOrDefault(a => a.Key == emailTemp);

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var emailBody = emailTemplate.Value;
                emailBody = emailBody.Replace("#NAME#", (recipientName != "") ? recipientName : "Sir/ Madam");
                emailBody = emailBody.Replace("#BODYTEXT#", body);

                if (!string.IsNullOrEmpty(referenceNumber))
                    emailBody = emailBody.Replace("#REFERENCENUMBER#", referenceNumber);

                var email = new EmailQueueItem
                {
                    ApplicationId = Convert.ToInt32(emailApplication.Value),
                    QueueDateTime = DateTime.Now,
                    EmailAccountId = Convert.ToInt32(emailAccount.Value),
                    ToList = recipientEmail,
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


                var RCSHistoryLog = new PLMApplicationHistortyLog
                {
                    HumanSettlementApplicationId = RCSAppID,
                    AuditAction = activitycontent,
                    UserId = CustomerID,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.PLMApplicationHistortyLogs.Add(RCSHistoryLog);
                db.SaveChanges();

                if (SmsNumber != null && !SmsNumber.Trim().Equals(string.Empty))
                {
                    try
                    {
                        var smsConfirmation = new CesarSMS();
                        var cxt = new eServicesDbContext();
                        var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;

                        //Remove Html tags before sms sending
                        EmailContent = RemoveHtmlTagsFromSms(EmailContent);

                        smsConfirmation.GenerateSMS(SmsNumber, EmailContent, CustomerID.ToString(), statusIdSms, recipientName);
                        //SmsHelper.Send(model.MobileNumber, string.Format("Welcome to Rates Clearance System, please confirm your account with the following code {0}.", code));
                    }
                    catch (Exception x)
                    {

                    }
                }
                //Single attachments
                if (!hasAttachment) return;
                if (attachment != null)
                {
                    AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachment, email.EmailQueueId, attachmentName);
                }

                //Attach more than 1 attachments
                if (attachments == null || fileNames == null) return;
                for (var i = 0; i < attachments.Count; i++)
                {
                    if (attachments[i] != null && fileNames[i] != null)
                    {
                        AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachments[i], email.EmailQueueId, fileNames[i]);
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }


        }

        public void GenerateEmailSMS(string activitycontent, string SmsNumber,int RCSAppID, int CustomerID, string EmailContent, string recipientEmail, string subject, string body,string referenceId, bool hasAttachment, string emailTemp, string recipientName = "", string ccList = "",string bccList = "", string referenceNumber = "", byte[] attachment = null, string attachmentName = "", List<byte[]> attachments = null, List<string> fileNames = null)
        {
            try
            {
                if (recipientEmail == null)
                    return;

                var emailApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = db.AppSettings.FirstOrDefault(a => a.Key == emailTemp);

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var emailBody = emailTemplate.Value;
                emailBody = emailBody.Replace("#NAME#", (recipientName != "") ? recipientName : "Sir/ Madam");
                emailBody = emailBody.Replace("#BODYTEXT#", body);

                if (!string.IsNullOrEmpty(referenceNumber))
                    emailBody = emailBody.Replace("#REFERENCENUMBER#", referenceNumber);

                var email = new EmailQueueItem
                {
                    ApplicationId = Convert.ToInt32(emailApplication.Value),
                    QueueDateTime = DateTime.Now,
                    EmailAccountId = Convert.ToInt32(emailAccount.Value),
                    ToList = recipientEmail,
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


                var RCSHistoryLog = new PLMApplicationHistortyLog
                {
                    PropertyLeaseApplicationId = RCSAppID,
                    AuditAction = activitycontent,
                    UserId = CustomerID,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.PLMApplicationHistortyLogs.Add(RCSHistoryLog);
                db.SaveChanges();

                if (SmsNumber != null && !SmsNumber.Trim().Equals(string.Empty))
                {
                    try
                    {
                        var smsConfirmation = new CesarSMS();
                        var cxt = new eServicesDbContext();
                        var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;

                        //Remove Html tags before sms sending
                        EmailContent = RemoveHtmlTagsFromSms(EmailContent);

                        smsConfirmation.GenerateSMS(SmsNumber, EmailContent, CustomerID.ToString(), statusIdSms, recipientName);
                        //SmsHelper.Send(model.MobileNumber, string.Format("Welcome to Rates Clearance System, please confirm your account with the following code {0}.", code));
                    }
                    catch (Exception x)
                    {

                    }
                }
                //Single attachments
                if (!hasAttachment) return;
                if (attachment != null)
                {
                    AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachment, email.EmailQueueId, attachmentName);
                }

                //Attach more than 1 attachments
                if (attachments == null || fileNames == null) return;
                for (var i = 0; i < attachments.Count; i++)
                {
                    if (attachments[i] != null && fileNames[i] != null)
                    {
                        AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachments[i], email.EmailQueueId, fileNames[i]);
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }

            
        }
        public void GenerateRefundEmail(int RefundAppID, int RCSAppID, int CustomerID, string EmailContent, string recipientEmail, string subject, string body,
string referenceId, bool hasAttachment, string emailTemp, string recipientName = "", string ccList = "",
string bccList = "", string referenceNumber = "", byte[] attachment = null, string attachmentName = "", List<byte[]> attachments = null, List<string> fileNames = null)
        {
            try
            {
                if (recipientEmail == null)
                    return;

                var emailApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var emailAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);
                var emailTemplate = db.AppSettings.FirstOrDefault(a => a.Key == emailTemp);

                if (emailApplication == null) throw new Exception("Invalid Application setting");
                if (emailAccount == null) throw new Exception("Invalid Application setting");
                if (emailTemplate == null) throw new Exception("Invalid Application setting");

                var emailBody = emailTemplate.Value;
                emailBody = emailBody.Replace("#NAME#", (recipientName != "") ? recipientName : "Sir/ Madam");
                emailBody = emailBody.Replace("#BODYTEXT#", body);

                if (!string.IsNullOrEmpty(referenceNumber))
                    emailBody = emailBody.Replace("#REFERENCENUMBER#", referenceNumber);

                var email = new EmailQueueItem
                {
                    ApplicationId = Convert.ToInt32(emailApplication.Value),
                    QueueDateTime = DateTime.Now,
                    EmailAccountId = Convert.ToInt32(emailAccount.Value),
                    ToList = recipientEmail,
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


                var RCSHistoryLog = new PLMApplicationHistortyLog
                {
                    PropertyLeaseApplicationId = RCSAppID,
                    AuditAction = EmailContent,
                    UserId = CustomerID,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true
                };
                db.PLMApplicationHistortyLogs.Add(RCSHistoryLog);
                db.SaveChanges();
                //Single attachments
                if (!hasAttachment) return;
                if (attachment != null)
                {
                    AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachment, email.EmailQueueId, attachmentName);
                }

                //Attach more than 1 attachments
                if (attachments == null || fileNames == null) return;
                for (var i = 0; i < attachments.Count; i++)
                {
                    if (attachments[i] != null && fileNames[i] != null)
                    {
                        AddEmailAttachement(Convert.ToInt32(emailApplication.Value), attachments[i], email.EmailQueueId, fileNames[i]);
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Adds Email Queue Item
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="attachment"></param>
        /// <param name="emailQueueId"></param>
        /// <param name="fileName"></param>
        private void AddEmailAttachement(int applicationId, byte[] attachment, int emailQueueId, string fileName)
        {
            try
            {
                var emailAttachment = new EmailAttachmentQueue
                {
                    ApplicationId = applicationId,
                    Attachment = attachment,
                    ContentType = "application/pdf",
                    EmailQueueId = emailQueueId,
                    Filename = fileName
                };

                core.EmailAttachmentQueue.Add(emailAttachment);
                core.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<string> ExtractEmails(string text)
        {
            if (text.Trim().Length == 0) return new List<string>();

            JavaScriptSerializer js = new JavaScriptSerializer();
            string[] emails = js.Deserialize<string[]>(text);
            List<string> emailList = new List<string>();
            const string MatchEmailPattern =
                @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z0-9]+[\w-]+\.)+[a-zA-Z]{1}[a-zA-Z0-9-]{1,23})$";
            Regex rx = new Regex(MatchEmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            foreach (var e in emails)
            {
                // Find matches.
                MatchCollection matches = rx.Matches(e);
                // Report the number of matches found.
                int noOfMatches = matches.Count;
                // Report on each match.

                foreach (Match match in matches)
                {
                    emailList.Add(match.Value.ToString());
                }
            }
            
            return emailList;
        }

        private string RemoveATag(string input)
        {
            input = input.Replace("<a"," ");
            input = input.Replace("</a>", " ");
            input = input.Replace("href='", " ");
            input = input.Replace("'>", " ");
            return input;

        }
        private string RemoveHtmlTagsFromSms(string input)
        {
            input = RemoveATag(input);
            string pattern = "<.*?>";
            string plainText = Regex.Replace(input, pattern, string.Empty);
            
            return plainText;
        }
    }
}