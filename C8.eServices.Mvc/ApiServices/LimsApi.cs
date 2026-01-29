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
    public class LimsApi
    {
        private static eServicesDbContext _cxt = new eServicesDbContext();

        public string ws02gentoken()
        {
            try
            {
                var getkey = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.limsWso2key).FirstOrDefault();
                var getsecret = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.limsWso2Secret).FirstOrDefault();
                var gettokenendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.limsWs02gentokenendpoint).FirstOrDefault();
                string keys = getkey.Value;
                string secrets =  getsecret.Value;

                string tokenendpoint = gettokenendpoint.Value;
                //tokenendpoint = "https://solarprodintam.ekurhuleni.gov.za/token ";
                var token = string.Empty;

                RestClient client = new RestClient(tokenendpoint);

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
        public string GetPropertyDetails(string AccountNo)
            {
            PaymentDetails payload = new PaymentDetails();
            PaymentDetailsList PaymentDetailsListPayload = new PaymentDetailsList();
            List<PaymentDetailsList> PaymentDetailsListArray = new List<PaymentDetailsList>();
            string content2 = "";
            try
            {
                //AccountNo = "1800046082";
                var gettokenendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.limsWs02endpoint).FirstOrDefault();
                string endpoint = gettokenendpoint.Value;

                LimsApi law = new LimsApi();
                var token = law.ws02gentoken();

                //ReturnStandUnitNumber rts = new ReturnStandUnitNumber();
                //string standnum = rts.GetStandNumber(AccountNo);

                //RestClient client = new RestClient("https://solarprodintam.ekurhuleni.gov.za/services/coe/property_master_list/v1.0.0/?billingidentifier=C03000000016200000000000000"); //prod
                //RestClient client = new RestClient("https://solarprodintam.ekurhuleni.gov.za/services/coe/property_master_list/v1.0.0/?billingidentifier=" + AccountNo); //prod


                //RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/services/coe/property_master_list/v1.0.0/?billingidentifier=" + AccountNo); //test
                RestClient client = new RestClient("http://solartestintam.ekurhuleni.gov.za/services/coe/property_master_list/v1.0.0?registeredownerids=" + AccountNo); //test
                //RestClient client = new RestClient("http://solartestintam.ekurhuleni.gov.za/services/coe/property_master_list/v1.0.0?billingidentifier=K15004000011500000000000000"); //test

                //RestClient client = new RestClient(endpoint+AccountNo);

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                RestRequest request = new RestRequest() { Method = Method.GET };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);

                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "payment-details.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
                json = json.Replace("{0}", AccountNo);

                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;
                content2 = content;

                //add code to check if there is a response
                return content;
                if (content != string.Empty)
                {
                    dynamic data = JObject.Parse(content);

                    if (data.SolarERP.Header.Result.Status == "Success")
                    {
                        var prop = data.SolarERP.Payload.LIMS.Properties;
                        if(prop.count() > 0 )
                        {
                         
                        }
                    }
                    else
                    {

                    }
                    
                    //foreach (var item in data.StatusMessages)
                    //{



                    //    payload.Status = item;
                    //    if (payload.Status.Contains("No transactions found for account-no"))
                    //    {
                    //        payload.Status = "No transactions found for account-no";
                    //    }
                     
                    //}
                    //if (data.CaudAccountNo == null || data.CaudAccountNo == "")
                    //{
                      
                    //}
                    //else
                    //{
                    //    foreach (var item in data.PaymentDetailsList)
                    //    {
                    //        PaymentDetailsListPayload = new PaymentDetailsList();
                    //        PaymentDetailsListPayload.Amt = item.Amt;
                    //        PaymentDetailsListPayload.Date = item.Date;
                    //        var date2 = Convert.ToString(item.Date);
                    //        var date = Convert.ToString(item.Date);
                    //        var len = date2.Length;
                    //        if (date.Length == 8)
                    //        {
                    //            var Year = date.Substring(0, 4);
                    //            var Month = date.Substring(4, 2);
                    //            var Day = date.Substring(6, 2);
                    //            var DateFinal = Year + '-' + Month + '-' + Day;
                    //            PaymentDetailsListPayload.ConvertedDate = Convert.ToDateTime(DateFinal);
                    //        }
                    //        PaymentDetailsListPayload.Ref = item.Ref;

                    //        PaymentDetailsListPayload.CustomerFirstName = item.CustomerFirstName;
                    //        PaymentDetailsListPayload.CustomerLastName = item.CustomerLastName;
                    //        PaymentDetailsListArray.Add(PaymentDetailsListPayload);

                    //    }

                    //    payload.PaymentDetailsList = PaymentDetailsListArray;

                    //}



                }

              
            }
            catch (Exception x)
            {
                return content2;
            }
        }


    
    }
}