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

namespace C8.eServices.Mvc.ApiServices
{
    public class LawTrustApi
    {
        private static eServicesDbContext _cxt = new eServicesDbContext();
        const string _LawTrustKey = "Client_XET";

        //const string _LawTrustSecret = "6F11AFFA4BA21974721B99BEF7D1A42148E548D0456B922B970FA46100FFB68B";

        const string _LawTrustSecret = "F5F2D44C3A269A427BA8A82FD467B20BF78C48FC23A0E145FF5A589872562486";

        //When using ws02
        public string ws02gentoken(string keys, string secrets)
        {
            try
            {
                var getkey = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.ws02key).FirstOrDefault();
                var getsecret = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.ws02Secret).FirstOrDefault();
                var gettokenendpoint= _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.Ws02gentokenendpoint).FirstOrDefault();
                keys = getkey.Value;
                secrets = getsecret.Value;
                string tokenendpoint = gettokenendpoint.Value;

                var token = string.Empty;
                //keys = "khVWK7eB9_RLYkrvjvofTcXvCmUa";
                //secrets = "9Sm2K1d0tNYVISihCroghZOhaqMa";



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


        //Lawtrust when uploading
        public string  wso2genlink()
        {
            try
            {
                
                
                var baseContext = HttpContext.Current;

                for (int i = 0; i < baseContext.Request.Files.Count; i++)
                {

                    var baseFile = baseContext.Request.Files[i];
                    Models.File oFile = new Models.File();
                    Stream fileContent = baseFile.InputStream;
                    oFile.FileName = baseFile.FileName;
                    oFile.ContentType = baseFile.ContentType;

                    Byte[] bytes = new BinaryReader(baseFile.InputStream).ReadBytes(baseFile.ContentLength);
                 
                    string filename = oFile.FileName;
                   

                    string s = Convert.ToBase64String(bytes);

                    

                    LawTrustApi law = new LawTrustApi();
                    var token = law.ws02gentoken(_LawTrustKey, _LawTrustSecret);

                    var getlinknendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.Ws02genlinkendpoint).FirstOrDefault();
                    string genlinkendpoint = getlinknendpoint.Value;
                    //RestClient client = new RestClient("http://b85c8594f925.ngrok.io/api/Fileupload/Upload?filename="+filename);

                    RestClient client = new RestClient(genlinkendpoint + filename);
                
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                    RestRequest request = new RestRequest() { Method = Method.GET };
                    request.RequestFormat = DataFormat.Json;
                    //request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Content-Type", "multipart/form-data");

                    request.Parameters.Clear();
                    request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                    request.AddParameter("docbytes",s, ParameterType.HttpHeader);


                    var response = client.Execute(request);
                    var content = response.Content;

                    if (content != string.Empty)
                    {


                        return content;



                    }

                }
                return null;
            }
            catch (Exception x)
            {

                throw x;
            }

        }

        //Lawtrust Getting Doc from solar
        public static GenerateSolarRCCPayload GenerateRCC(string AccountNo)
        {
            try
            {


                //AccountNo = "1801965166";This is a test account with a rcc

                GenerateSolarRCCPayload payload = new GenerateSolarRCCPayload();

                GenerateRCCApi law = new GenerateRCCApi();
                var token = law.ws02gentoken();

                var gettokenendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.GenerateRccWs02endpoint).FirstOrDefault();
                string endpoint = gettokenendpoint.Value;

                //RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/RCSMunicipalcertApi/v1.0/api/ServiceDetailsPDF/AccountLookup");
                RestClient client = new RestClient(endpoint);

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "gen-rcc.json";
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
                data = data.SolarERP.payload;



                if (content != string.Empty)
                {

                    //payload.AccountNumber = Convert.ToString(data.SolarERP.payload.AccountNumber);
                    //payload.ClearanceCertificate = Convert.ToString(data.SolarERP.payload.ClearanceCertificate);
                    payload.AccountNumber = Convert.ToString(data.AccountNumber);
                    payload.ClearanceCertificate = Convert.ToString(data.ClearanceCertificate);

                    return payload;

                }






                return payload;
            }
            catch (Exception x)
            {

                throw x;
            }
        }
        public string wso2gendocandlink(string accountno,string email)
        {
            try
            {



                

                

                string filename = "rcccertificate.pdf";
                var details = LawTrustApi.GenerateRCC(accountno);
                string bytestring = details.ClearanceCertificate;
                byte[] bytes = Encoding.ASCII.GetBytes(bytestring);

                byte[] newBytes = Convert.FromBase64String(bytestring);

                string s = bytestring;
                //fh is the first half of the string
                string fh = bytestring.Substring(0,5092) ;
                //sh is second half of the string
                string sh = bytestring.Substring(5092);

                string useremail = email;

                LawTrustApi laws = new LawTrustApi();
                string contents = laws.lawtrustintegrration(filename, newBytes, email);
               

                //LawTrustApi law = new LawTrustApi();
                //    var token = law.ws02gentoken(_LawTrustKey, _LawTrustSecret);

                //    var getlinknendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.Ws02genlinkendpoint).FirstOrDefault();
                //    string genlinkendpoint = getlinknendpoint.Value;
                //    //RestClient client = new RestClient("http://b85c8594f925.ngrok.io/api/Fileupload/Upload?filename="+filename);

                //    RestClient client = new RestClient(genlinkendpoint + filename);

                //    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                //    RestRequest request = new RestRequest() { Method = Method.GET };
                //    request.RequestFormat = DataFormat.Json;
                //    //request.AddHeader("Content-Type", "application/json");
                //    request.AddHeader("Content-Type", "multipart/form-data");

                //    request.Parameters.Clear();
                //    request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                //    request.AddParameter("docbytes", fh, ParameterType.HttpHeader);
                //    request.AddParameter("docbytes2", sh, ParameterType.HttpHeader);
                //request.AddParameter("emails", useremail, ParameterType.HttpHeader);



                //var response = client.Execute(request);
                var content = contents; /*response.Content;*/

                    if (content != string.Empty)
                    {


                        return content;



                    }

                
                return null;
            }
            catch (Exception x)
            {

                throw x;
            }

        }

        public  string lawtrustintegrration(string filename, byte[] newBytes, string email)
        {
            //var baseContext = HttpContext.Current;

            //byte[] bytes = Encoding.ASCII.GetBytes(bytestring);
            var details = LawTrustApi.UploadDocument(filename, newBytes);
            string packageid = details.package_id.ToString();

            string documentid = details.documentid;
            string token= details.token;

            var adduser = LawTrustApi.AddUserToWorkFLow(packageid, documentid, token,email);

            var signatureblock = LawTrustApi.InsertSignatureBlock(packageid, documentid, token);

            var signatureblock2 = LawTrustApi.InsertSignatureBlock2(packageid, documentid, token);
            
            var signatureblock3 = LawTrustApi.InsertSignatureBlock3(packageid, documentid, token);


            var sharedoc= LawTrustApi.ShareDocument(packageid, documentid, token);

            var genlink= LawTrustApi.GenerateIntergrationLink(packageid, documentid, token,email);

            return genlink.ToString();
          
            LawTrustApi.DownloadDoc(packageid, documentid, token);

        }

        public static LawTrustPackage AddNewPackage()
        {
            try
            {
                TokenApi newToke = new TokenApi();
                var token = newToke.GenerateToken(_LawTrustKey, _LawTrustSecret);
                LawTrustPackage payload = new LawTrustPackage();
                //RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages");
                RestClient client = new RestClient("https://api.signinghub.co.za/v3/packages");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("Content-Type", "application/json");
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "lawtrust-addnewpackage.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
                //json = json.Replace("{0}", meterNumber);
                //json = json.Replace("{1}", amount.ToString().Replace(",", "."));
                //json = json.Replace("{2}", tender.ToString().Replace(",", "."));

                request.AddHeader("Accept", "*/*");
                request.AddHeader("Access-Control-Allow-Origin", "*");
                request.AddHeader("Access-Control-Allow-Headers", "authorization,Access-Control-Allow-Origin,Content-Type,SOAPAction");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json; charset=UTF-8");
                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;

                if (content != string.Empty)
                {


                    dynamic data = JObject.Parse(content);

                    payload.package_id = data.package_id;
                    payload.workflow_mode = Convert.ToString(data.workflow_mode);
                    payload.workflow_type = Convert.ToString(data.workflow_type);
                    payload.token = token;


                    if (payload.package_id != 0)
                    {
                        payload.Error = "No Errors";
                        return payload;

                    }
                    else
                    {
                        payload.Error = Convert.ToString(data.SolarERP.Payload.suprima.thinClient.consumerChk.error.Value);
                        return payload;
                    }
                }

                return null;
            }
            catch (Exception x)
            {

                throw x;
            }

        }
        public static LawTrustPackage UploadDocument(string filename,byte[] newBytes)
        {
            var baseContext = HttpContext.Current;
            LawTrustPackage payload = new LawTrustPackage();

            
               
                

                   



                    var addpackage = LawTrustApi.AddNewPackage();



                   // RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/" + addpackage.package_id + "/documents");
            RestClient client = new RestClient("https://api.signinghub.co.za/v3/packages/" + addpackage.package_id + "/documents");

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                    RestRequest request = new RestRequest() { Method = Method.POST };



                    request.Parameters.Clear();

                    request.RequestFormat = DataFormat.Json;
                    request.AddParameter("Authorization", "Bearer " + addpackage.token, ParameterType.HttpHeader);
                    request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);
                    request.AddParameter("Content-Type", "application/octet-stream", ParameterType.HttpHeader);
                    request.AddParameter("x-file-name",filename, ParameterType.HttpHeader);
                    request.AddParameter("x-convert-document", "true", ParameterType.HttpHeader);
                    request.AddParameter("x-source", "API", ParameterType.HttpHeader);
                    request.AddParameter("application/octet-stream", newBytes, ParameterType.RequestBody);

                    //request.AddFile("content", file);









                    var response = client.Execute(request);
                    var content = response.Content;

                    if (content != null)
                    {
                        string con = content.Substring(14);
                        string docid = con.Remove(con.Length - 1, 1);
                        payload.package_id = addpackage.package_id;
                        payload.token = addpackage.token;
                        payload.documentid = docid;
                        return payload;
                    }








                
            
            return payload;
        }
        public static LawTrustPackage AddUserToWorkFLow(string packageid, string documentid, string token,string emails)
        {
            try
            {



                string email = emails; /*"tishen.naidoo@xetgroup.com";*/
                string username = "service manager";
                string role = "SIGNER";

                

                LawTrustPackage payload = new LawTrustPackage();
                // RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/"+packageid+"/workflow/users");
                RestClient client = new RestClient("https://api.signinghub.co.za/v3/packages/" + packageid + "/workflow/users");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "add-user.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
                json = json.Replace("{0}", email);
                json = json.Replace("{1}", username);
                json = json.Replace("{2}", role);

               
                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;

                if (content != string.Empty)
                {
                   
                  

                    }

             




                return null;
            }
            catch (Exception x)
            {

                throw x;
            }
        }

        public static LawTrustPackage InsertSignatureBlock(string packageid, string docuemntid, string token)
        {
            try
            {
                
                LawTrustPackage payload = new LawTrustPackage();
                //RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/"+packageid+"/documents/"+docuemntid+"/fields/autoplace");
                RestClient client = new RestClient("https://api.signinghub.co.za/v3/packages/" + packageid + "/documents/" + docuemntid + "/fields/electronic_signature");
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "insert-signatureblock.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
              
                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;

                if (content != string.Empty)
                {



                }






                return null;
            }
            catch (Exception x)
            {

                throw x;
            }
        }
        public static LawTrustPackage InsertSignatureBlock2(string packageid, string docuemntid, string token)
        {
            try
            {

                LawTrustPackage payload = new LawTrustPackage();
                RestClient client = new RestClient("https://api.signinghub.co.za/v3/packages/" + packageid + "/documents/" + docuemntid + "/fields/electronic_signature");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "insert-signatureblock2.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);

                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;

                if (content != string.Empty)
                {



                }






                return null;
            }
            catch (Exception x)
            {

                throw x;
            }
        }

        public static LawTrustPackage InsertSignatureBlock3(string packageid, string docuemntid, string token)
        {
            try
            {

                LawTrustPackage payload = new LawTrustPackage();
                RestClient client = new RestClient("https://api.signinghub.co.za/v3/packages/" + packageid + "/documents/" + docuemntid + "/fields/electronic_signature");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "insert-signatureblock3.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);

                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;

                if (content != string.Empty)
                {



                }






                return null;
            }
            catch (Exception x)
            {

                throw x;
            }
        }

        public static LawTrustPackage ShareDocument(string packageid, string docuemntid, string token)
        {
            try
            {

                LawTrustPackage payload = new LawTrustPackage();
               // RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/"+packageid+"/workflow");

                RestClient client = new RestClient("https://api.signinghub.co.za/v3/packages/" + packageid + "/workflow");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
              

                var response = client.Execute(request);
                var content = response.Content;

                if (content != string.Empty)
                {



                }






                return null;
            }
            catch (Exception x)
            {

                throw x;
            }
        }

        public static string  GenerateIntergrationLink(string packageid, string docuemntid, string token,string emails)
        {
            try
            {
                string package_id = packageid.ToString();

                string useremail = emails; /*"tishen.naidoo@xetgroup.com";*/
              


                LawTrustPackage payload = new LawTrustPackage();
                // RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/links/integration");

                RestClient client = new RestClient("https://api.signinghub.co.za/v3/links/integration");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "intergration-link.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
                json = json.Replace("{0}", package_id);
                json = json.Replace("{1}", useremail);
              


                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;

                if (content != string.Empty)
                {

                    return content;

                    

                }






                return null;
            }
            catch (Exception x)
            {

                throw x;
            }
        }


        public static LawTrustPackage GetDocStatus(int packageid, string docuemntid, string token)
        {
            try
            {

                LawTrustPackage payload = new LawTrustPackage();
                RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/"+packageid+"/log");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);


                var response = client.Execute(request);
                var content = response.Content;

                if (content != string.Empty)
                {



                }






                return null;
            }
            catch (Exception x)
            {

                throw x;
            }
        }


        public static LawTrustPackage DownloadDoc(string packageid, string docuemntid, string token)
        {
            try
            {

                LawTrustPackage payload = new LawTrustPackage();
                RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/"+packageid+"/base64?");

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.GET };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);

                byte[] response = client.DownloadData(request);
                //var response = client.Execute(request);
                //var content = response.Content;

                //if (content != string.Empty)
                //{



                //}






                return null;
            }
            catch (Exception x)
            {

                throw x;
            }
        }


    }
}