using C8.eServices.Mvc.ApiServices;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace C8.eServices.Mvc.Helpers
{
    public class LIMSHelper
    {
        private static eServicesDbContext core = new eServicesDbContext();

        public string ws02gentoken()
        {
            try
            {
                var getkey = core.AppSettings.Where(x => x.Key == AppSettingKeys.ws02key).FirstOrDefault();
                var getsecret = core.AppSettings.Where(x => x.Key == AppSettingKeys.ws02Secret).FirstOrDefault();
                var gettokenendpoint = core.AppSettings.Where(x => x.Key == AppSettingKeys.Ws02gentokenendpoint).FirstOrDefault();
                //string keys = "QiOYa4UIB2oxvm1WCQMQDyrmjaoa";/* getkey.Value;*/
                //string secrets = "UZxhVyUyogJ3GJut474JliIqTG8a"; /*getsecret.Value;*/
                string keys = "khVWK7eB9_RLYkrvjvofTcXvCmUa";/* getkey.Value;*/
                string secrets = "9Sm2K1d0tNYVISihCroghZOhaqMa"; /*getsecret.Value;*/
                string tokenendpoint = gettokenendpoint.Value;

                var token = string.Empty;

                RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/token");

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

                core.Logs.Add(new Log()
                {
                    ReferenceId = 1,
                    ReferenceTypeId = 1,
                    LogEntry = x.ToString(),
                    LogTypeId = 1,
                    IsActive = true,
                    IsDeleted = false
                });
                core.SaveChanges();
                throw x;
            }
        }



        public string GetStandNumber(string IdentificationNumber)
        {
            try
            {
                ReturnStandUnitNumber law = new ReturnStandUnitNumber();
                var token = law.ws02gentoken();

                //RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/RCSStandNumProcesssAPI/v1.1/api/StandNumber/AccountLookup");
                RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/RCSStandNoV2/v1.0/api/StandtNoAPI");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "stand-number.json";//Create a json file with request in it
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
                json = json.Replace("{0}", IdentificationNumber);

                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;

                dynamic data = JObject.Parse(content);
                string standnum = Convert.ToString(data.UnitNumber);
                string Accountstatus = Convert.ToString(data.AccountStatus);
                if (content != string.Empty)
                {

                }
                if (Accountstatus == "A")
                {
                    return standnum;
                }

                else

                    return Accountstatus;

            }
            catch (Exception x)
            {

                throw x;
            }
        }
    }
}