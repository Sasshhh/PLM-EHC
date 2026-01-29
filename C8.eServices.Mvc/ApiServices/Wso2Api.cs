using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace C8.eServices.Mvc.ApiServices
{
   
        public class Wso2Api
        {
            //NC - TO DO: Complete Wso2 API

            public const string _url = "https://10.2.2.214:9443";
            public const string _username = "admin";
            public const string _password = "admin";
            public static void InitaliseSSL()
            {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };
            }
            public static string GetAllUsers()
            {
                // used to validate if the server cannot detect an SSL Certficate
                InitaliseSSL();

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            var api = "/scim2/Users";

                var client = new RestClient(_url);
                client.Authenticator = new HttpBasicAuthenticator(_username, _password);

                var request = new RestRequest(api, Method.GET);
                request.RequestFormat = DataFormat.Json;
                var response = client.Execute(request);

                var message = JObject.Parse(response.Content);
                return message.ToString();
            }
            public static string GetAdditionalUserStores()
            {
                // used to validate if the server cannot detect an SSL Certficate
                InitaliseSSL();

                var api = "/api/server/v1/userstores";

                var client = new RestClient(_url);
                client.Authenticator = new HttpBasicAuthenticator(_username, _password);

                var request = new RestRequest(api, Method.GET);
                request.RequestFormat = DataFormat.Json;
                var response = client.Execute(request);

                var message = JObject.Parse(response.Content);
                return message.ToString();
            }
            public static string GetRoles()
            {
                // used to validate if the server cannot detect an SSL Certficate
                InitaliseSSL();

                var api = "/scim2/Roles";

                var client = new RestClient(_url);
                client.Authenticator = new HttpBasicAuthenticator(_username, _password);

                var request = new RestRequest(api, Method.GET);
                request.RequestFormat = DataFormat.Json;
                var response = client.Execute(request);

                var message = JObject.Parse(response.Content);
                return message.ToString();
            }
            public static string GetUsersPerDomain()
            {
                // used to validate if the server cannot detect an SSL Certficate
                InitaliseSSL();

                var api = "/scim2/Users?startIndex=1&count=10&domain=PRIMARY";

                var client = new RestClient(_url);
                client.Authenticator = new HttpBasicAuthenticator(_username, _password);

                var request = new RestRequest(api, Method.GET);
                request.RequestFormat = DataFormat.Json;
                var response = client.Execute(request);

                var message = JObject.Parse(response.Content);
                return message.ToString();
            }
            public static string GetUserProfile()
            {
                // used to validate if the server cannot detect an SSL Certficate
                InitaliseSSL();

                var api = "/api/identity/user/v1.0/me";

                var client = new RestClient(_url);
                client.Authenticator = new HttpBasicAuthenticator(_username, _password);

                var request = new RestRequest(api, Method.GET);
                request.RequestFormat = DataFormat.Json;
                var response = client.Execute(request);

                var message = JObject.Parse(response.Content);
                return message.ToString();
            }
            public static string GetUserInfoWithRoles()
            {
                // used to validate if the server cannot detect an SSL Certficate
                InitaliseSSL();

                var api = "/scim2/Users?startIndex=1&count=10&filter=userName+sw+PRIMARY/admin";

                var client = new RestClient(_url);
                client.Authenticator = new HttpBasicAuthenticator(_username, _password);

                var request = new RestRequest(api, Method.GET);
                request.RequestFormat = DataFormat.Json;
                var response = client.Execute(request);

                var message = JObject.Parse(response.Content);
                return message.ToString();
            }
            public static string AssignUserToRole()
            {
                // used to validate if the server cannot detect an SSL Certficate
                InitaliseSSL();

                // TO DO: format the string with parameters
                var api = "/scim2/Users/6905509f-e5eb-41e9-8a8f-b2fdf4539336";

                //TO DO: Add payload to update the request
                var client = new RestClient(_url);
                client.Authenticator = new HttpBasicAuthenticator(_username, _password);

                var request = new RestRequest(api, Method.PUT);
                request.RequestFormat = DataFormat.Json;
                var response = client.Execute(request);

                var message = JObject.Parse(response.Content);
                return message.ToString();
            }
        }
    
}