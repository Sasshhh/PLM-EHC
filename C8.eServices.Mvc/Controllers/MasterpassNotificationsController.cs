using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Controllers
{
    public class MasterpassNotificationsController : ApiController
    {
        private eServicesDbContext db = new eServicesDbContext();

        public async Task<HttpResponseMessage> PostAsync()
        {

            var content = Request.Content.ReadAsByteArrayAsync();

            using (var contentStream = await Request.Content.ReadAsStreamAsync())
            {
                contentStream.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(contentStream))
                {
                    string rawContent = sr.ReadToEnd();
                    JObject json = JObject.Parse(rawContent);

                    if (bool.Parse(db.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.MasterpassDailyTest).Value) == false)
                    {
                        int transactionId = (int)json["transactionId"];
                        string reference = (string)json["reference"];
                        decimal amount = (decimal)json["amount"];
                        string currencyCode = (string)json["currencyCode"];
                        string status = (string)json["status"];
                        int retrievalReferenceNumber = (int)json["bankResponse"]["retrievalReferenceNumber"];
                        string authCode = (string)json["bankResponse"]["authCode"];
                        string bankResponse = (string)json["bankResponse"]["bankResponse"];
                        string code = (string)json["code"];
                        string msisdn = (string)json["msisdn"];
                        string cardInfo = (string)json["cardInfo"]["cardType"];
                        string binLast4 = (string)json["cardInfo"]["binLast4"];
                        string accountType = (string)json["cardInfo"]["accountType"];

                        MasterpassTransaction masterpassTr = new MasterpassTransaction();
                        masterpassTr.TransactionId = transactionId;
                        masterpassTr.Reference = reference;
                        masterpassTr.Amount = amount;
                        masterpassTr.CurrencyCode = currencyCode;
                        masterpassTr.Status = status;
                        masterpassTr.RetrievalReferenceNumber = retrievalReferenceNumber;
                        masterpassTr.AuthCode = authCode;
                        masterpassTr.BankResponse = bankResponse;
                        masterpassTr.Code = code;
                        masterpassTr.CardType = cardInfo;
                        masterpassTr.BinLast4 = binLast4;
                        masterpassTr.AccountType = accountType;
                        masterpassTr.IsActive = true;
                        masterpassTr.IsDeleted = false;
                        masterpassTr.CreatedDateTime = DateTime.Now;
                        masterpassTr.StatusId = 9;

                        if (db.MasterpassRequests.FirstOrDefault(o => o.Code == code) != null)
                        {
                            masterpassTr.MasterpassRequestId = db.MasterpassRequests.FirstOrDefault(o => o.Code == code).Id;
                        } else 
                            masterpassTr.MasterpassRequestId = 1;

                        try
                        {
                            db.MasterpassTransactions.Add(masterpassTr);
                            db.SaveChanges();
                        }
                        catch (Exception x)
                        {
                            System.IO.File.WriteAllText(@"C:\tmp\error_siyakhokha.log", x.ToString());
                        }
                    }
                    
                    System.IO.File.WriteAllText(@"C:\tmp\rawContent.txt", rawContent);
                }
            }
            System.IO.File.WriteAllText(@"C:\tmp\WriteLines.txt", content.ToString());
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private async Task<String> getRawPostData()
        {
            var rertn = string.Empty;
            using (var contentStream = await this.Request.Content.ReadAsStreamAsync())
            {
                try
                {
                    contentStream.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(contentStream))
                    {
                        rertn = sr.ReadToEnd();
                    }
                }
                catch (Exception e)
                {
                    System.IO.File.WriteAllText(@"C:\tmp\WriteLines.txt", e.ToString());
                }

                return rertn;
            }
        }
    }
}