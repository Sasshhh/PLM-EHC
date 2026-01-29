using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace C8.eServices.Mvc.ApiServices
{
    public class SolarApiServices
    {
        private static eServicesDbContext _cxt = new eServicesDbContext();

        public string ws02gentoken()
        {
            try
            {
                var getkey = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.ws02key).FirstOrDefault();
                var getsecret = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.ws02Secret).FirstOrDefault();
                var gettokenendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.Ws02gentokenendpoint).FirstOrDefault();
                string keys = "HFlybnp9fNRxYioOMwu3uAXDmDsa";/* getkey.Value;*/
                string secrets = "ofA3uguYcJNq4s4lHreaNmK_EFQa"; /*getsecret.Value;*/
                string tokenendpoint = gettokenendpoint.Value;

                var token = string.Empty;




                RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/token");
                //RestClient client = new RestClient(tokenendpoint);


                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.Authenticator = new HttpBasicAuthenticator(keys, secrets);
                RestRequest request = new RestRequest() { Method = Method.POST };

                request.AddParameter("Content-Type", "application/x-www-form-urlencoded", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);

                request.AddParameter("grant_type", "client_credentials");





                var response = client.Execute(request);
                token = response.Content;

                if (token != string.Empty)
                {
                    dynamic data = JObject.Parse(token);
                    token = data.access_token;
                }

                return token;
            }
            catch (Exception x)
            {

                _cxt.Logs.Add(new Log()
                {
                    ReferenceId = 1,
                    ReferenceTypeId = 1,
                    LogEntry = x.ToString(),
                    LogTypeId = 1,
                    IsActive = true,
                    IsDeleted = false
                });
                _cxt.SaveChanges();
                throw x;
            }
        }
        public string getIDNumber(string AccountNo)
        {
            try
            {

                //AccountNo = "1800009844";




                SolarApiServices law = new SolarApiServices();
                var token = law.ws02gentoken();



                RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/RCSCustomerServiceApi/v1.0/api/ReturnIDNumber/AccountLookup");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "id-number.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
                json = json.Replace("{0}", AccountNo);



                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;


                if (content != string.Empty)
                {



                }






                return content;
            }
            catch (Exception x)
            {

                throw x;
            }
        }

        public string getRCNumber(string AccountNo)
        {
            try
            {

                //AccountNo = "1800009844";




                SolarApiServices law = new SolarApiServices();
                var token = law.ws02gentoken();



                RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/RCSCustomerServiceApi/v1.0/api/ReturnClearanceNumber/AccountLookup");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "rc-number.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
               

                var timeStamp = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ssK");
                string corrId = Guid.NewGuid().ToString(); ;
                string transId = Guid.NewGuid().ToString(); ;
                string messId = Guid.NewGuid().ToString(); ;
                json = json.Replace("{0}", AccountNo);
                json = json.Replace("{1}", timeStamp);
                json = json.Replace("{2}", corrId);
                json = json.Replace("{3}", transId);
                json = json.Replace("{4}", messId);

                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;


                if (content != string.Empty)
                {



                }






                return content;
            }
            catch (Exception x)
            {

                throw x;
            }
        }

        public DebtorsNote getDebtorNotes(string AccountNo)
        {
            DebtorsNote payload = new DebtorsNote();
            DebtorsNoteList DebtorsNoteListPayload = new DebtorsNoteList();
            List<DebtorsNoteList> DebtorsNoteListArray = new List<DebtorsNoteList>();

            try
            {

                //AccountNo = "1dddd4";




                SolarApiServices law = new SolarApiServices();
                var token = law.ws02gentoken();
             


                RestClient client = new RestClient("http://solartestintam.ekurhuleni.gov.za/RCSAPIDebtorsNotes/v1.0/api/DebtorNotesRetrieval/AccountLookup");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "debtor-note.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
         
                var timeStamp = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ssK");
                string corrId = Guid.NewGuid().ToString(); ;
                string transId = Guid.NewGuid().ToString(); ;
                string messId = Guid.NewGuid().ToString(); ;
                json = json.Replace("{0}", AccountNo);
                json = json.Replace("{1}", timeStamp);
                json = json.Replace("{2}", corrId);
                json = json.Replace("{3}", transId);
                json = json.Replace("{4}", messId);


                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;


                if (content != string.Empty)
                {
                    dynamic data = JObject.Parse(content);

            
                    foreach (var item in data.StatusMessages)
                    {
                        payload.Status = item;
                        if (payload.Status.Contains("No transactions found for account-no"))
                        {
                            payload.Status = "No transactions found for account-no";
                        }

                    }
                    if (data.AccountNo == null || data.AccountNo == ""|| data.Note != "Y")
                    {
                        payload.AccountNo = data.AccountNo;
                        payload.Note = data.Note;
                        payload.Note = "N";
                        payload.Status = "No Notes Found For Account Number ";
                    }
                    else
                    {
                        payload.AccountNo = data.AccountNo;
                        payload.Note = data.Note;
                        foreach (var item in data.NoteList)
                        {
                            DebtorsNoteListPayload = new DebtorsNoteList();
                            DebtorsNoteListPayload.NoteTime = item.NoteTime;
                            DebtorsNoteListPayload.NoteDate = item.NoteDate;
                            DebtorsNoteListPayload.Note1 = item.Note1;
                            DebtorsNoteListPayload.Note2 = item.Note2;
                            DebtorsNoteListPayload.NoteType = item.NoteType;
                            DebtorsNoteListPayload.NoteUser = item.NoteUser;
                            var date2 = Convert.ToString(item.NoteDate);
                            var date = Convert.ToString(item.NoteDate);
                            var len = date2.Length;
                            if (date.Length == 8)
                            {
                                var Year = date.Substring(0, 4);
                                var Month = date.Substring(4, 2);
                                var Day = date.Substring(6, 2);
                                var DateFinal = Year + '-' + Month + '-' + Day;
                                DebtorsNoteListPayload.ConvertedDate = Convert.ToDateTime(DateFinal);
                            }
                            //PaymentDetailsListPayload.Ref = item.Ref;

                            //PaymentDetailsListPayload.CustomerFirstName = item.CustomerFirstName;
                            //PaymentDetailsListPayload.CustomerLastName = item.CustomerLastName;
                            DebtorsNoteListArray.Add(DebtorsNoteListPayload);

                        }

                        payload.NoteList = DebtorsNoteListArray;

                    }



                }







                return payload;
            }
            catch (Exception x)
            {
                payload.Note = "N";
                payload.Status = "Failure";
                return payload;
                throw x;
            }
        }
    }
}