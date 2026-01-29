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
    public class ReturnStandUnitNumber
    {
        private static eServicesDbContext _cxt = new eServicesDbContext();
        public string ws02gentoken()
        {
            try
            {
                var getkey = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.StandnumberWso2key).FirstOrDefault();
                var getsecret = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.StandnumberWso2Secret).FirstOrDefault();
                var gettokenendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.StandnumberWs02gentokenendpoint).FirstOrDefault();
                
                //string keys = "khVWK7eB9_RLYkrvjvofTcXvCmUa";/* getkey.Value;*/
                //string secrets = "9Sm2K1d0tNYVISihCroghZOhaqMa"; /*getsecret.Value;*/


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



        public string GetStandNumber(string AccountNo)
        {
            try
            {



                var gettokenendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.StandnumberWs02endpoint).FirstOrDefault();
                string endpoint = gettokenendpoint.Value;



                ReturnStandUnitNumber law = new ReturnStandUnitNumber();
                var token = law.ws02gentoken();



                //RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/RCSStandNumProcesssAPI/v1.1/api/StandNumber/AccountLookup");//old old

                /*RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/RCSStandNoV2/v1.0/api/StandtNoAPI");*///last working


                RestClient client = new RestClient(endpoint);

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "stand-number.json";
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

                dynamic data = JObject.Parse(content);

                string standnum ="";
              
                string Accountstatus= "";
                if (content != string.Empty)
                {
                    try
                    {
                        standnum = Convert.ToString(data.SolarERP.payload.UnitNumber);
                        Accountstatus = Convert.ToString(data.SolarERP.payload.AccountStatus);
                    }
                    catch(Exception IO)
                    {
                        return "I";
                    }
                    

                }
                if (Accountstatus == "A")
                {
                    return standnum;
                }

                else
                    if(standnum == "")
                {
                    return "I";
                }
                    return Accountstatus;


           
            }
            catch (Exception x)
            {

                throw x;
            }
        }






    }
}