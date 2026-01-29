using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using RestSharp;
//using RestSharp.Authenticators;
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
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.ViewModels;
using System.Net.Http;
using System.Net.Http.Headers;
using RestSharp;
using RestSharp.Authenticators;

namespace C8.eServices.Mvc.ApiServices
{
    public class SolarAssetManagementApi
    {
        private static eServicesDbContext core = new eServicesDbContext();

        public static string GenerateToken()
        {
            try
            {
                //BaseHelper _base = new BaseHelper();
                //_base.Initialise(core);
                //var getkey = core.AppSettings.Where(x => x.Key == AppSettingKeys.limsWso2key).FirstOrDefault();
                //var getsecret = core.AppSettings.Where(x => x.Key == AppSettingKeys.limsWso2Secret).FirstOrDefault();
                //var gettokenendpoint = core.AppSettings.Where(x => x.Key == AppSettingKeys.limsWs02gentokenendpoint).FirstOrDefault();
                string keys = String.Format("crm-integration");
                string secrets = String.Format("5a2f2e80-a281-4f34-88a6-3ad9c04b7cf6");


                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "sams-creds.json";//Create a json file with request in it
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);


                string tokenendpoint = "ttp://10.1.2.229:8180/auth/realms/sams-api/protocol/openid-connect/token";
                tokenendpoint = "http://10.1.2.229:8180/auth/realms/sams-api/protocol/openid-connect/token";
                var token = string.Empty;

                RestClient client = new RestClient(tokenendpoint);

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.Authenticator = new HttpBasicAuthenticator(keys, secrets);
                RestRequest request = new RestRequest() { Method = Method.POST };

                request.AddParameter("Content-Type", "application/x-www-form-urlencoded", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);
                //request.AddParameter("grant_type", "password");
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


        public static AssetResult GetAssetDetails(string sg_number, string address_street, string building_number, string floor_number, string room_number)
        {
            try
            {
                var client = new RestClient("http://10.1.2.229:50080/api/v1/property-lease/asset/detail");
                client.Timeout = -1;
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer " + GenerateToken());
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Cookie", "JSESSIONID=srerDSgMCdTWZEXpO5QOIB6zxvTjYWKU_7WOwRIV");

                #region JSON Body
                var body = @"{" + "\n" +
                           @"   ""SolarERP"":{" + "\n" +
                           @"      ""Header"":{" + "\n" +
                           @"         ""Version"":""3.1""," + "\n" +
                           @"         ""TimeStamp"":""2022-11-24T04:33:33.866478100Z""," + "\n" +
                           @"         ""CorrelationID"":""2b6b36cc-6b9d-11ed-a1eb-0242ac120002""," + "\n" +
                           @"         ""TransactionID"":""314aaff0-6b9d-11ed-a1eb-0242ac120002""," + "\n" +
                           @"         ""MessageID"":""3690a60e-6b9d-11ed-a1eb-0242ac120002""," + "\n" +
                           @"         ""SenderID"":""Property Lease""," + "\n" +
                           @"         ""RecipientID"":""SAMS""," + "\n" +
                           @"         ""Action"":""Create""," + "\n" +
                           @"         ""User"":""Username""," + "\n" +
                           @"         ""Message"":{" + "\n" +
                           @"            ""Name"":""Get Asset Details""," + "\n" +
                           @"            ""Type"":""Request""" + "\n" +
                           @"         }," + "\n" +
                           @"         ""Result"":{" + "\n" +
                           @"            ""Status"":""SUCCESS""," + "\n" +
                           @"            ""ErrorCode"":""""," + "\n" +
                           @"            ""ErrorDescription"":""""" + "\n" +
                           @"         }," + "\n" +
                           @"         ""Context"":[" + "\n" +
                           @"            {" + "\n" +
                           @"               ""Property"":{" + "\n" +
                           @"                  ""Name"":""Entity""," + "\n" +
                           @"                  ""Value"":""CoE""" + "\n" +
                           @"               }" + "\n" +
                           @"            }" + "\n" +
                           @"         ]" + "\n" +
                           @"      }," + "\n" +
                           @"      ""Payload"":{" + "\n" +
                           @"         ""sg_number"":""{0}""," + "\n" +
                           @"         ""address_street"":""{1}""," + "\n" +
                           @"         ""building_number"":""{2}""," + "\n" +
                           @"         ""floor_number"":""{3}""," + "\n" +
                           @"         ""room_number"":""{4}""" + "\n" +
                           @"      }" + "\n" +
                           @"   }" + "\n" +
                           @"}";

                body = body.Replace("{0}", string.IsNullOrEmpty(sg_number) ? "" : sg_number);
                body = body.Replace("{1}", string.IsNullOrEmpty(address_street) ? "" : address_street);
                body = body.Replace("{2}", string.IsNullOrEmpty(building_number) ? "" : building_number);
                body = body.Replace("{3}", (string.IsNullOrEmpty(floor_number) && !string.IsNullOrEmpty(sg_number)) ? "UNKNOWN" : string.IsNullOrEmpty(floor_number) ? "" : floor_number);
                body = body.Replace("{4}", string.IsNullOrEmpty(room_number) ? "" : room_number);
                #endregion

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                var content = response.Content;
                var result = new AssetResult();
                if (content != string.Empty)
                {
                    dynamic data = JObject.Parse(content);
                    var StatusCode = data.SolarERP.Header.Result.Status;

                    data = data.SolarERP.Payload;
                    if (StatusCode == "SUCCESS")
                    {
                        var GetRoot = data.ToString();
                        GetRoot = GetRoot.Replace("{{", "{");
                        GetRoot = GetRoot.Replace("}}", "}");
                        if (GetRoot != null) result = JsonConvert.DeserializeObject<AssetResult>(GetRoot);
                    }
                    else if (StatusCode == "Failure")
                    {
                        object error = data.SolarERP.Header.Result;
                        result.Error = error;
                    }
                }
                return result;
            }
            catch (Exception)
            {

                //
            }
            return null;
        }

        public static void UpdateAssesGeolocation(string departmentKey)
        {
            if (departmentKey == ApplicationEntityKeys.EkurhuleniHousingCompany)
            {
                var list = core.UnitsEkurhuleniHousingCompany
                    .Where(e => string.IsNullOrEmpty(e.GeoLocation) || string.IsNullOrEmpty(e.Address)).ToList();
                foreach (var asset in list)
                {
                    var token = GenerateToken();
                    var assetdetails = GetAssetDetails("", "", "", "", "");
                    if (assetdetails != null){
                        asset.GeoLocation = String.Format("longitude: {0} | latitude: {1}", assetdetails.longitude, assetdetails.latitude);
                        core.Entry(asset).State = System.Data.Entity.EntityState.Modified;
                        core.SaveChanges();

                        if (asset.IsTaken){
                            var applicantunit = core.ApplicantUnits
                                .FirstOrDefault(a => a.Matched.UnitsEkurhuleniHousingCompanyId == asset.Id);
                            if (applicantunit == null)
                                throw new Exception(String.Format("Invalid asset match - {0}", asset.Id));

                            var application = core.PropertyLeaseApplications.Find(applicantunit.PropertyLeaseApplicationId);
                            if (application == null)
                                throw new Exception(String.Format("Invalid application - {0}", applicantunit.PropertyLeaseApplicationId));
                        }
                    }
                }
            }
        }
    }
}