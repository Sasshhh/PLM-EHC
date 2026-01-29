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
    public class Lims10
    {

        public static string LIMSApi(string IDNumber)
        {
            try
            {
                //return "2";
                IDNumber = "4901020644089";
                string tokenendpoint = "https://solarprodintam.ekurhuleni.gov.za/services/coe/property_master_list/v1.0.0" + IDNumber;
                //string tokenendpoint = "http://solartestintam.ekurhuleni.gov.za/services/coe/property_master_list/v1.0.0?RegisteredOwnerID=4901020644089";

                string keys = /*"sM85WQwCTjpRfYIvgm5iJ2jdxJoa"*/"lyOGsmLBM6P0mQEEOWqIMofPNNoa";
                string secret = /*"IDnXdNA_F3UTQs5k8Ls0KNjBXIoa"*/"Dwmh7C9EbktAQqsZF7aDUS3QATAa";
                Lims10 law = new Lims10();
                var token = ws02gentoken(keys, secret);

                RestClient client = new RestClient(tokenendpoint);

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;

                RestRequest request = new RestRequest() { Method = Method.GET };    
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);

                var response = client.Execute(request);
                var content = response.Content;

                dynamic data = JObject.Parse(content);

                if (content != string.Empty)
                {
                    try
                    {
                        if (data.SolarERP.Header.Result.Status == "Success")
                        {
                            if (data.SolarERP.Payload.LIMS.Properties == null)
                            {
                                return ("1");
                            }
                            else
                            {
                                var proptest2 = data.SolarERP.Payload.LIMS;
                                int length = ((JArray)proptest2["Properties"]).Count();
                                if (length > 0)
                                {
                                    return "0";
                                }
                                else if (length == 0)
                                {
                                    return "1";
                                }
                            }
                        }
                        else
                        {
                            return "2";
                        }
                    }
                    catch (Exception io)
                    {
                        return "1";
                    }
                }
                else
                {
                    return ("2");
                }
                return data;
            }
            catch (Exception e)
            {
                string message = e.Message.ToString();
                return "2";
                //  throw;
            }
        }
        public static string ws02gentoken(string keyst, string secretst)
        {
            try
            {
                string tokenendpoint = "https://solarprodintam.ekurhuleni.gov.za/token";
                var token = string.Empty;
                RestClient client = new RestClient(tokenendpoint);
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.Authenticator = new HttpBasicAuthenticator(keyst, secretst);
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
                return null;
            }
        }
    }
}