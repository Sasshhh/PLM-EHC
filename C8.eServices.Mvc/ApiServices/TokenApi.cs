using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ApiServices
{
    public class TokenApi
    {
        private static eServicesDbContext _cxt = new eServicesDbContext();

        public string GenerateToken(string key, string secret)
        {
            try
            {
                var token = string.Empty;
                key = "Client_XET";
                //secret = "6F11AFFA4BA21974721B99BEF7D1A42148E548D0456B922B970FA46100FFB68B";
                secret = "F5F2D44C3A269A427BA8A82FD467B20BF78C48FC23A0E145FF5A589872562486";

                //RestClient client = new RestClient("https://uatapi.signinghub.co.za/authenticate");
                RestClient client = new RestClient("https://api.signinghub.co.za/authenticate");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.Authenticator = new HttpBasicAuthenticator(key, secret);
                RestRequest request = new RestRequest() { Method = Method.POST };
              
                request.AddParameter("Content-Type", "application/x-www-form-urlencoded", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);

                //request.AddParameter("grant_type", "password");
                //request.AddParameter("client_id", "Client_XET");
                //request.AddParameter("client_secret", "6F11AFFA4BA21974721B99BEF7D1A42148E548D0456B922B970FA46100FFB68B");

                //request.AddParameter("username", "tishen.naidoo@xetgroup.com");
                //request.AddParameter("password", "P@ssword1xet");
                request.AddParameter("grant_type", "password");
                request.AddParameter("client_id", "Client_XET");
                request.AddParameter("client_secret", "F5F2D44C3A269A427BA8A82FD467B20BF78C48FC23A0E145FF5A589872562486");

                request.AddParameter("username", "integrations@xetgroup.com");
                request.AddParameter("password", "P@ssword1xet");




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