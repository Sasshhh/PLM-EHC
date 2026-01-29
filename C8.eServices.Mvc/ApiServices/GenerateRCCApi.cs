using System;
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
    public class GenerateRCCApi
    {
        private static eServicesDbContext _cxt = new eServicesDbContext();
        public string ws02gentoken()
        {
            try
            {
                var getkey = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.GenerateRccWso2key).FirstOrDefault();
                var getsecret = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.GenerateRccWso2Secret).FirstOrDefault();
                var gettokenendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.GenerateRccWs02gentokenendpoint).FirstOrDefault();
                //string keys = "HFlybnp9fNRxYioOMwu3uAXDmDsa";/* getkey.Value;*/
                //string secrets = "ofA3uguYcJNq4s4lHreaNmK_EFQa"; /*getsecret.Value;*/
                string keys = getkey.Value;
                string secrets = getsecret.Value;
                string tokenendpoint = gettokenendpoint.Value;

                var token = string.Empty;




                //RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/token");
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
       
    }
}