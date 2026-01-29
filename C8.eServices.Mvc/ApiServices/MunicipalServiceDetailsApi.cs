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
    public class MunicipalServiceDetailsApi
    {

        private static eServicesDbContext _cxt = new eServicesDbContext();

        public string ws02gentoken()
        {
            try
            {
                var getkey = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.MunservWso2key).FirstOrDefault();
                var getsecret = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.MunservWso2Secret).FirstOrDefault();
                var gettokenendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.MunservWs02gentokenendpoint).FirstOrDefault();
                //string keys = "QiOYa4UIB2oxvm1WCQMQDyrmjaoa";/* getkey.Value;*/
                //string secrets = "UZxhVyUyogJ3GJut474JliIqTG8a"; /*getsecret.Value;*/
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
        public MunicipalServiceDetails GetMunicipalServiceDetails(string AccountNo)
        {
            try
            {

                var gettokenendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.MunservWs02endpoint).FirstOrDefault();
                string endpoint = gettokenendpoint.Value;


                MunicipalServiceDetails payload = new MunicipalServiceDetails();
                MetersList MeterListPayload = new MetersList();
                List<MetersList> MetersListArray = new List<MetersList>();

               

               


              


                MunicipalServiceDetailsApi law = new MunicipalServiceDetailsApi();
                var token = law.ws02gentoken();


                //RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/RCSStandNumProcesssAPI/v1.1/api/MunicipalServicesDetailsV2/AccountLookup");

                RestClient client = new RestClient(endpoint);

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


                if (content != string.Empty)
                {

                    dynamic data = JObject.Parse(content);

                    foreach (var item in data.SolarERP.payload.StatusMessages)
                    {



                        payload.Status = item;
                        //if (payload.Status.Contains("No transactions found for account-no"))
                        //{
                        //    payload.Status = "No transactions found for account-no";
                        //}

                    }
                    data = data.SolarERP.payload;
                    if (payload.Status == "Success")
                    {
                        payload.RatesServiceExist = data.RatesServiceExist;
                        payload.SewerServiceExist = data.SewerServiceExist;

                        payload.RefuseServiceExist = data.RefuseServiceExist;
                        payload.AccountNumber = data.AccountNumber;


                        foreach (var item in data.MetersList)
                        {
                            MeterListPayload = new MetersList();
                            MeterListPayload.Service = item.Service;
                            MeterListPayload.ServiceType = item.ServiceType;
                            MeterListPayload.MeterNumber = item.MeterNumber;


                            if (item.AmountLevied != null)
                            {
                                MeterListPayload.AmountLevied = item.AmountLevied;
                            }

                            if (item.VatLevied != null)
                            {
                                MeterListPayload.VatLevied = item.VatLevied;
                            }

                            if (item.MeterReading != null)
                            {
                                MeterListPayload.MeterReading = item.MeterReading;
                            }

                            if (item.MeterReadingDate != null)
                            {
                                MeterListPayload.MeterReadingDate = item.MeterReadingDate;
                            }






                            if (item.PreviousMeterReading != null && item.PreviousMeterReadingDate != null)
                            {
                                MeterListPayload.PreviousMeterReading = item.PreviousMeterReading;
                                MeterListPayload.PreviousMeterReadingDate = item.PreviousMeterReadingDate;
                                MeterListPayload.hasPreviousMeter = true;
                            }




                            if (item.MeterConsumption != null)
                            {
                                MeterListPayload.MeterConsumption = item.MeterConsumption;
                            }


                            if (item.UnprocessedMeterReading != null && item.UnprocessedMeterReadingDate != null)
                            {
                                MeterListPayload.UnprocessedMeterReading = item.UnprocessedMeterReading;
                                MeterListPayload.UnprocessedMeterReadingDate = item.UnprocessedMeterReadingDate;
                                MeterListPayload.hasUnprocessedMeter = true;
                                MeterListPayload.Status = "Unprocessed";
                            }

                            MetersListArray.Add(MeterListPayload);
                        }
                            payload.MetersList = MetersListArray;

                    }
                }
                    






                return payload;
            }
            catch (Exception x)
            {

                throw x;
            }
        }


        public MunicipalServiceDetails RESTGetMunicipalServiceDetails(string AccountNo)
        {
            try
            {



                MunicipalServiceDetails payload = new MunicipalServiceDetails();
                MetersList MeterListPayload = new MetersList();
                List<MetersList> MetersListArray = new List<MetersList>();









                MunicipalServiceDetailsApi law = new MunicipalServiceDetailsApi();
                var token = law.ws02gentoken();



                RestClient client = new RestClient("http://10.1.2.222:8086/solarbpi/rest/WfLookup/");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                //request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
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
                    foreach (var item in data.StatusMessages)
                    {



                        payload.Status = item;
                        //if (payload.Status.Contains("No transactions found for account-no"))
                        //{
                        //    payload.Status = "No transactions found for account-no";
                        //}

                    }
                    if (payload.Status == "Success")
                    {
                        payload.RatesServiceExist = data.RatesServiceExist;
                        payload.SewerServiceExist = data.SewerServiceExist;

                        payload.RefuseServiceExist = data.RefuseServiceExist;
                        payload.AccountNumber = data.AccountNumber;


                        foreach (var item in data.MetersList)
                        {
                            MeterListPayload = new MetersList();
                            MeterListPayload.Service = item.Service;
                            MeterListPayload.ServiceType = item.ServiceType;
                            MeterListPayload.MeterNumber = item.MeterNumber;


                            if(item.AmountLevied != null)
                            {
                                MeterListPayload.AmountLevied = item.AmountLevied;
                            }

                            if (item.VatLevied != null)
                            {
                                MeterListPayload.VatLevied = item.VatLevied;
                            }

                            if (item.MeterReading != null)
                            {
                                MeterListPayload.MeterReading = item.MeterReading;
                            }

                            if (item.MeterReadingDate != null)
                            {
                                MeterListPayload.MeterReadingDate = item.MeterReadingDate;
                            }

                        
                           

                      

                            if (item.PreviousMeterReading != null && item.PreviousMeterReadingDate != null)
                            {
                                MeterListPayload.PreviousMeterReading = item.PreviousMeterReading;
                                MeterListPayload.PreviousMeterReadingDate = item.PreviousMeterReadingDate;
                                MeterListPayload.hasPreviousMeter = true;
                            }

                         


                            if (item.MeterConsumption != null)
                            {
                                MeterListPayload.MeterConsumption = item.MeterConsumption;
                            }
                          

                            if(item.UnprocessedMeterReading != null && item.UnprocessedMeterReadingDate != null)
                            {
                                MeterListPayload.UnprocessedMeterReading = item.UnprocessedMeterReading;
                                MeterListPayload.UnprocessedMeterReadingDate = item.UnprocessedMeterReadingDate;
                                MeterListPayload.hasUnprocessedMeter = true;
                                MeterListPayload.Status = "Unprocessed";
                            }

                            MetersListArray.Add(MeterListPayload);
                        }
                        payload.MetersList = MetersListArray;

                    }
                }







                return payload;
            }
            catch (Exception x)
            {

                throw x;
            }
        }

    }
}
