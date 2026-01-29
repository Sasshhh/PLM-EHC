using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;


namespace C8.eServices.Mvc.Models
{
    public class CesarSMS
    {
        private eServicesDbContext db = new eServicesDbContext();
        private CesarDbContext core = new CesarDbContext();

        public CesarSMS()
        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }

        public void GenerateSMS(string recipientMobileNumber, string textMessage,
        string referenceId, int statusId, string recipientName = "")
        {
            try
            {
                if (recipientMobileNumber == null)
                    return;

                var smsApplication = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesApplication);
                var smsAccount = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.EservicesEmailAccount);

                if (smsApplication == null) throw new Exception("Invalid Application setting");
                if (smsAccount == null) throw new Exception("Invalid Application setting");

                //if (!string.IsNullOrEmpty(referenceNumber))
                //    emailBody = emailBody.Replace("#REFERENCENUMBER#", referenceNumber);

                var sms = new SmsQueueItem
                {
                    ApplicationId = 1,
                    QueueDateTime = DateTime.Now,
                    SmsAccountId = 1,
                    MobileNumber = recipientMobileNumber,
                    TextMessage = textMessage,
                    FailureCount = 0,
                    ReferenceId = referenceId,
                    StatusId = statusId
                };

                core.SmsQueue.Add(sms);
                core.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Adds Email Queue Item
        /// </summary>
        ///// <param name="applicationId"></param>
        ///// <param name="attachment"></param>
        ///// <param name="emailQueueId"></param>
        ///// <param name="fileName"></param>
        //private void AddEmailAttachement(int applicationId, byte[] attachment, int emailQueueId, string fileName)
        //{
        //    try
        //    {
        //        var emailAttachment = new EmailAttachmentQueue
        //        {
        //            ApplicationId = applicationId,
        //            Attachment = attachment,
        //            ContentType = "application/pdf",
        //            EmailQueueId = emailQueueId,
        //            Filename = fileName
        //        };

        //        core.EmailAttachmentQueue.Add(emailAttachment);
        //        core.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        //public List<string> ExtractEmails(string text)
        //{
        //    if (text.Trim().Length == 0) return new List<string>();

        //    JavaScriptSerializer js = new JavaScriptSerializer();
        //    string[] emails = js.Deserialize<string[]>(text);
        //    List<string> emailList = new List<string>();
        //    const string MatchEmailPattern =
        //        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z0-9]+[\w-]+\.)+[a-zA-Z]{1}[a-zA-Z0-9-]{1,23})$";
        //    Regex rx = new Regex(MatchEmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //    foreach (var e in emails)
        //    {
        //        // Find matches.
        //        MatchCollection matches = rx.Matches(e);
        //        // Report the number of matches found.
        //        int noOfMatches = matches.Count;
        //        // Report on each match.

        //        foreach (Match match in matches)
        //        {
        //            emailList.Add(match.Value.ToString());
        //        }
        //    }

        //    return emailList;
        //}
    }
}