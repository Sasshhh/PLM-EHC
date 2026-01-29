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
    public class EDRMHelper
    {
        private static eServicesDbContext db = new eServicesDbContext();


        static bool CheckIfValueExist(string x, string y, List<Datum> t) => (!String.IsNullOrEmpty(x) && !String.IsNullOrEmpty(y) && t != null) ? true : false;
        static string FindKeyForEDRMS(eServicesDbContext db, string KeyVale) => (!String.IsNullOrEmpty(KeyVale) ? db.AppSettings.FirstOrDefault(x => x.Key == KeyVale).Value : null);
        public static ResultDocs EDRMS(byte[] content, string DocumentName, Int64 FileSize, string mimeType, string Extent, string SystemName)
        {
            string Key = FindKeyForEDRMS(db, EDRMsKeys.Ws02KeyQA);
            string secret = FindKeyForEDRMS(db, EDRMsKeys.Ws02SecretQA);
            var GetWS02Token =  ws02gentokenApiCall(Key, secret);
            var GetEDRMSAuth = eDRMAuth(GetWS02Token);
            var GetEDRMSDocTypes = ReturnDocId(GetEDRMSAuth, GetWS02Token);
            var Result = CheckIfValueExist(GetWS02Token, GetEDRMSAuth, GetEDRMSDocTypes);

            List<DocumentVM> Docs = new List<DocumentVM>();
            UploadDoc uploadDoc = new UploadDoc();
            if (Result == true)
            {
                Docs.Add(new DocumentVM
                {
                    Author = "Propery Lease Management",
                    DateCreation = DateTime.Now.ToString(),
                    DateReceived = DateTime.Now.ToString(),
                    DateSubmission = DateTime.Now.ToString(),
                    DateTimeStamp = DateTime.Now.ToString(),
                    Department = "Ekurhuleni Housing Company",
                    DocumentName = DocumentName,
                    DocumentTypeExtensionForm = "",
                    DocumentTypeID = GetEDRMSDocTypes.FirstOrDefault(r => r.documentType == "PROPERTY LEASE").id.ToString() ?? "d821a1ad-d594-43de-8578-ea311ad74c34",
                    Extension = Extent,
                    File = content,
                    Filename = DocumentName,
                    MimeDocType = mimeType,
                    Organisation = "Ekurhuleni",
                    SubmittedBy = SystemName,
                    SystemName = SystemName

                });
                uploadDoc.AccessToken = GetEDRMSAuth;
                uploadDoc.DocumentsUpload = Docs;

                return UploadDocs(uploadDoc, GetWS02Token);

            }
            else
            {
                return null;
            }
        }
        public static ResultDocs EDRMS2(byte[] content, string DocumentName, Int64 FileSize, string mimeType, string Extent, string SystemName)
        {
            string Key = FindKeyForEDRMS(db, EDRMsKeys.Ws02KeyQA);
            string secret = FindKeyForEDRMS(db, EDRMsKeys.Ws02SecretQA);
            var GetWS02Token = ws02gentokenApiCall(Key, secret);
            var GetEDRMSAuth = eDRMAuth(GetWS02Token);
            var GetEDRMSDocTypes = ReturnDocId(GetEDRMSAuth, GetWS02Token);
            var Result = CheckIfValueExist(GetWS02Token, GetEDRMSAuth, GetEDRMSDocTypes);

            List<DocumentVM> Docs = new List<DocumentVM>();
            UploadDoc uploadDoc = new UploadDoc();
            if (Result == true)
            {
                Docs.Add(new DocumentVM
                {
                    Author = "Propery Lease Management",
                    DateCreation = DateTime.Now.ToString(),
                    DateReceived = DateTime.Now.ToString(),
                    DateSubmission = DateTime.Now.ToString(),
                    DateTimeStamp = DateTime.Now.ToString(),
                    Department = "Human Settlement Development",
                    DocumentName = DocumentName,
                    DocumentTypeExtensionForm = "",
                    DocumentTypeID = GetEDRMSDocTypes.FirstOrDefault(r => r.documentType == "PROPERTY LEASE").id.ToString() ?? "d821a1ad-d594-43de-8578-ea311ad74c34",
                    Extension = Extent,
                    File = content,
                    Filename = DocumentName,
                    MimeDocType = mimeType,
                    Organisation = "Ekurhuleni",
                    SubmittedBy = SystemName,
                    SystemName = SystemName

                });
                uploadDoc.AccessToken = GetEDRMSAuth;
                uploadDoc.DocumentsUpload = Docs;

                return UploadDocs(uploadDoc, GetWS02Token);

            }
            else
            {
                return null;
            }
        }
        public static string ws02gentokenApiCall(string keyst, string secretst)
        {
            try
            {
                string tokenendpoint = FindKeyForEDRMS(db , EDRMsKeys.Ws02EndpointCon);

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
        public static string eDRMAuth(string tokenWs02)

        {
            var token = string.Empty;

            try
            {



                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "EDRMSAuth.json";//Create a json file with request in it
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);

                string tokenendpoint = FindKeyForEDRMS(db, EDRMsKeys.GetAccessCodeEDRMS);
                //tokenendpoint = "https://coewso2qa02.ekurhuleni.gov.za:8243/documents/1.0.0/api/account/AuthenticatePost";
                RestClient client = new RestClient(tokenendpoint);



                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;




                RestRequest request = new RestRequest() { Method = Method.POST };
                request.AddJsonBody(json);
                request.AddParameter("Authorization", "Bearer " + tokenWs02, ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);




                var response = client.Execute(request);
                token = response.Content;


                dynamic data = JObject.Parse(token);


                if (token != string.Empty)
                {

                    token = data.data.accessToken;
                }
                else
                {
                    token = string.Empty;
                }

                return token;
            }
            catch (Exception e)
            {
                string message = e.Message.ToString();
                return message;
                //    throw;
            }

        }
        public static List<Datum> ReturnDocId(string AccessId, string tokenWs02)
        {
            try
            {
                if (!String.IsNullOrEmpty(AccessId))
                {
                    List<Datum> result = new List<Datum>();
                    //string tokenendpoint = /*"https://10.2.2.238:8243/Api/1.0.0/api/docustore/documents/get-document-types?AccessToken="+*/db.AppSettings.FirstOrDefault(r => r.Key == EDRMsKeys.ReturnDocEDRMSId)?.Value + AccessId ?? null;
                    //string tokenendpoint = "https://10.2.2.238:8243/Documents/1.0.0/api/docustore/documents/get-document-types?AccessToken=" + AccessId;
                    //string tokenendpoint = "https://wso2apimanager.ekurhuleni.gov.za:8243/Documents/1.0.0/api/docustore/documents/get-document-types?AccessToken=" + AccessId;
                    string tokenendpoint = FindKeyForEDRMS(db, EDRMsKeys.DocTypesEDRMS) + AccessId;
                    //tokenendpoint = "https://coewso2qa02.ekurhuleni.gov.za:8243/documents/1.0.0/api/docustore/documents/get-document-types?AccessToken=" + AccessId;

                    RestClient client = new RestClient(tokenendpoint);


                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    ServicePointManager.Expect100Continue = true;




                    RestRequest request = new RestRequest() { Method = Method.GET };
                    request.AddParameter("Authorization", "Bearer " + tokenWs02, ParameterType.HttpHeader);
                    request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);

                    var response = client.Execute(request);
                    var content = response.Content;

                    dynamic data = JObject.Parse(content);
                    var CorrectJson = data.data.ToString().Trim().TrimStart('{').TrimEnd('}');


                    result = JsonConvert.DeserializeObject<List<Datum>>(CorrectJson);

                    return result;

                }
            }
            catch (Exception e)
            {
                string message = e.Message.ToString();
                //  throw;
            }
            return null;
        }
        public static ResultDocs UploadDocs(UploadDoc uploadDoc, string tokenWs02)
        {
            try
            {
                var SerJson = JsonConvert.SerializeObject(uploadDoc);

                var ReplaceName = SerJson.Replace("DocumentsUpload", "Documents");
                //string tokenendpoint = "https://wso2apimanager.ekurhuleni.gov.za:8243/Documents/1.0.0/api/docustore/documents/upload-documents";
                // string tokenendpoint = "https://10.2.2.238:8243/Documents/1.0.0/api/docustore/documents/upload-documents";
                string tokenendpoint = FindKeyForEDRMS(db, EDRMsKeys.UploadEndPointEDRMS);
                //string tokenendpoint = db.AppSettings.FirstOrDefault(r => r.Key == EDRMsKeys.UploadEndPointEDRMS).Value;
                //tokenendpoint = "https://coewso2qa02.ekurhuleni.gov.za:8243/documents/1.0.0/api/docustore/documents/upload-documents";

                RestClient client = new RestClient(tokenendpoint);

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.AddJsonBody(ReplaceName);
                request.AddParameter("Authorization", "Bearer " + tokenWs02, ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);

                var response = client.Execute(request);
                var content = response.Content;

                dynamic data = JObject.Parse(content);

                var GetRoot = data.ToString();
                GetRoot = GetRoot.Replace("{{", "{");
                GetRoot = GetRoot.Replace("}}", "}");

                //var  mystr = data.Substring(1, data.Length - 2);

                ResultDocs result = JsonConvert.DeserializeObject<ResultDocs>(GetRoot);
                if (String.IsNullOrEmpty(result.ToString()))
                {
                    UploadDocs(uploadDoc, tokenWs02);
                }

                return result;

            }
            catch (Exception)
            {
                return null;
                // throw;
            }

        }
        public static List<GetDocumentResult> ReturnDocs(string GroupId)
        {
            try
            {
                string Key = FindKeyForEDRMS(db, EDRMsKeys.Ws02KeyQA);
                string secret = FindKeyForEDRMS(db, EDRMsKeys.Ws02SecretQA);
                var GetWS02Token = ws02gentokenApiCall(Key, secret);
                var GetEDRMSAuth = eDRMAuth(GetWS02Token);

                //string tokenendpoint = db.AppSettings.FirstOrDefault(r => r.Key == EDRMsKeys.GetBackDocs).Value + GetEDRMSAuth + "&GroupId=" + GroupId;
                //tokenendpoint = "https://coewso2qa02.ekurhuleni.gov.za:8243/documents/1.0.0/api/docustore/documents/get-uploaded-documents" + GetEDRMSAuth + "&GroupId=" + GroupId;

                //string tokenendpoint = "https://wso2apimanager.ekurhuleni.gov.za:8243/Documents/1.0.0/api/docustore/documents/get-uploaded-documents?Accesstoken=" + GetEDRMSAuth + "&GroupId=" + GroupId;
                //string tokenendpoint = "https://10.2.2.238:8243/Documents/1.0.0/api/docustore/documents/get-uploaded-documents?Accesstoken=" + GetEDRMSAuth + "&GroupId=" + GroupId;
                string tokenendpoint = FindKeyForEDRMS(db, EDRMsKeys.GetBackDocs) + GetEDRMSAuth + "&GroupId=" + GroupId;

                RestClient client = new RestClient(tokenendpoint);


                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;




                RestRequest request = new RestRequest() { Method = Method.GET };
                request.AddParameter("Authorization", "Bearer " + GetWS02Token, ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);

                var response = client.Execute(request);
                var content = response.Content;

                dynamic data = JObject.Parse(content);


                var GetRoot = data.data.ToString();
                GetRoot = GetRoot.Replace("{{", "{");
                GetRoot = GetRoot.Replace("}}", "}");



                var result = JsonConvert.DeserializeObject<List<GetDocumentResult>>(GetRoot);
                return result;




                //  return result.data;
            }
            catch (Exception)
            {
                return null;
                //  throw;
            }
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public static void AddToAppsetting()
        {
            using (var db = new eServicesDbContext())
            {
                var CheckIfEDRMSActive = db.AppSettings.FirstOrDefault(x => x.Key == "EDRMSACTIVE") ?? null;
                if (CheckIfEDRMSActive == null)
                {
                    AppSetting appSettings = new AppSetting()
                    {
                        Key = "EDRMSACTIVE",
                        Name = "EDRMSACTIVE",
                        Value = "off",
                        CreatedBySystemUserId = 1,
                        CreatedDateTime = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false,
                    };
                    db.AppSettings.Add(appSettings);
                    db.SaveChanges();
                }

            }
        }

    }
}