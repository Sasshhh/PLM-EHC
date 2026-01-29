using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace C8.eServices.Mvc.Helpers
{
    public class TokenHelper
    {
        private static eServicesDbContext core = new eServicesDbContext();

        public static string GenerateToken(string JsonFile, string TokenEndpoind)
        {
            try
            {
                string keys = String.Format("crm-integration");
                string secrets = String.Format("5a2f2e80-a281-4f34-88a6-3ad9c04b7cf6");


                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "sams-creds.json";
                var path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);
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
    }
}