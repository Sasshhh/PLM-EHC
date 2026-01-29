using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;

namespace C8.eServices.Mvc.ApiServices
{
    public class WSO2Token
    {
        public static string TokenValue = null;
        public static string Token(string endpoint, string Secret, string ClientId)
        {
            try
            {
                RestClient client = new RestClient(endpoint);
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.Authenticator = new HttpBasicAuthenticator(ClientId, Secret);
                RestRequest request = new RestRequest() { Method = Method.POST };

                request.AddParameter("Content-Type", "application/x-www-form-urlencoded", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);
                request.AddParameter("grant_type", "client_credentials");

                var response = client.Execute(request);
                TokenValue = response.Content;

                if (!string.IsNullOrEmpty(TokenValue))
                {
                    dynamic data = JObject.Parse(TokenValue);
                    TokenValue = data.access_token;
                }
                return TokenValue;
            }
            catch (Exception error)
            {
                return null;
            }
        }
    }
}