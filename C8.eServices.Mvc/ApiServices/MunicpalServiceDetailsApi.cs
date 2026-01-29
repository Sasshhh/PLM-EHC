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
    public class MunicpalServiceDetailsApi
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
        public string GetMunicipalServiceDetails(string AccountNo)
        {
            try
            {



                MunicipalServiceDetails payload = new MunicipalServiceDetails();
                MetersList MeterListPayload = new MetersList();


                MunicpalServiceDetailsApi law = new MunicpalServiceDetailsApi();
                var token = law.ws02gentoken();



                RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/EkuRCSMunicipalAPI/v1.0/api/AccountDetails/AccountLookup");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "municipal-service-details.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
                json = json.Replace("{0}", AccountNo);



                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;


                if (content != string.Empty)
                {

                    dynamic data = JObject.Parse(content);
                    payload.RatesServiceExist = data.RatesServiceExist;
                    payload.SewerServiceExist = data.SewerServiceExist;

                    payload.RefuseServiceExist = data.RefuseServiceExist;
                    payload.AccountNumber = data.AccountNumber;


                    foreach(var item in data.MetersList)
                    {

                        MeterListPayload.Service = item.Service;
                        MeterListPayload.ServiceType = item.ServiceType;
                        MeterListPayload.MeterNumber = item.MeterNumber;



                        MeterListPayload.AmountLevied = item.AmountLevied;
                        MeterListPayload.VatLevied = item.VatLevied;
                        MeterListPayload.MeterNumber = item.MeterNumber;


                        MeterListPayload.MeterReading = item.MeterReading;
                        MeterListPayload.MeterReadingDate = item.MeterReadingDate;
                        MeterListPayload.PreviousMeterReading = item.PreviousMeterReading;

                        MeterListPayload.PreviousMeterReadingDate = item.PreviousMeterReadingDate;
                        MeterListPayload.MeterConsumption = item.MeterConsumption;

                        payload.MetersList.Add(MeterListPayload);
    }
      

    }






                return content;
            }
            catch (Exception x)
            {

                throw x;
            }
        }



    }
}
