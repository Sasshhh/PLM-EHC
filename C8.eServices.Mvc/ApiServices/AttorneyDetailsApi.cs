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

namespace C8.eServices.Mvc.ApiServices
{
    public class AttorneyDetailsApi
    {
        private static eServicesDbContext _cxt = new eServicesDbContext();
        const string _LawTrustKey = "QiOYa4UIB2oxvm1WCQMQDyrmjaoa";

        const string _LawTrustSecret = "UZxhVyUyogJ3GJut474JliIqTG8a";

        //public class AttorneyDetails
        //{
        //    public string AttorneyCode { get; set; }
        //    public string AttorneyName { get; set; }
        //    public string AddressLine1 { get; set; }
        //    public string AddressLine2 { get; set; }
        //    public string Postcode { get; set; }
        //    public string WorkNumber { get; set; }
        //    public List<string> StatusMessages { get; set; }
        //}

        //When using ws02
        public string ws02gentoken(string keys, string secrets)
        {
            try
            {
                var getkey = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.AttorneydetailsWso2key).FirstOrDefault();
                var getsecret = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.AttorneydetailsWso2Secret).FirstOrDefault();
                var gettokenendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.AttorneydetailsWs02gentokenendpoint).FirstOrDefault();
                keys = getkey.Value;
                secrets = getsecret.Value;
                string tokenendpoint = gettokenendpoint.Value;
               
                var token = string.Empty;
                //keys = "QiOYa4UIB2oxvm1WCQMQDyrmjaoa";
                //secrets = "UZxhVyUyogJ3GJut474JliIqTG8a";
                //tokenendpoint = "https://solartestintam.ekurhuleni.gov.za/token";


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
        public string wso2genlink()
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



                    AttorneyDetailsApi law = new AttorneyDetailsApi();
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
                    request.AddParameter("docbytes", s, ParameterType.HttpHeader);


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


        //public string lawtrustintegrration(string filename, byte[] newBytes)
        //{
        //    var baseContext = HttpContext.Current;
        //    var details = LawTrustApi.UploadDocument(filename, newBytes);
        //    string packageid = details.package_id.ToString();

        //    string documentid = details.documentid;
        //    string token = details.token;

        //    //var adduser = LawTrustApi.AddUserToWorkFLow(packageid, documentid, token);

        //    var signatureblock = LawTrustApi.InsertSignatureBlock(packageid, documentid, token);

        //    var sharedoc = LawTrustApi.ShareDocument(packageid, documentid, token);

        //    //var genlink = LawTrustApi.GenerateIntergrationLink(packageid, documentid, token);

        //    return genlink.ToString();

        //    LawTrustApi.DownloadDoc(packageid, documentid, token);

        //}

        public static LawTrustPackage AddNewPackage()
        {
            try
            {
                TokenApi newToke = new TokenApi();
                var token = newToke.GenerateToken(_LawTrustKey, _LawTrustSecret);
                LawTrustPackage payload = new LawTrustPackage();
                RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages");

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
        public static LawTrustPackage UploadDocument(string filename, byte[] newBytes)
        {
            var baseContext = HttpContext.Current;
            LawTrustPackage payload = new LawTrustPackage();









            var addpackage = LawTrustApi.AddNewPackage();



            RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/" + addpackage.package_id + "/documents");

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            RestRequest request = new RestRequest() { Method = Method.POST };



            request.Parameters.Clear();

            request.RequestFormat = DataFormat.Json;
            request.AddParameter("Authorization", "Bearer " + addpackage.token, ParameterType.HttpHeader);
            request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);
            request.AddParameter("Content-Type", "application/octet-stream", ParameterType.HttpHeader);
            request.AddParameter("x-file-name", filename, ParameterType.HttpHeader);
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
        public static LawTrustPackage AddUserToWorkFLow(string packageid, string documentid, string token)
        {
            try
            {

               
                string email = "tishen.naidoo@xetgroup.com";
                string username = "service manager";
                string role = "SIGNER";



                LawTrustPackage payload = new LawTrustPackage();
                RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/" + packageid + "/workflow/users");

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
        public AttorneyDetails AttorneyDetails(string attorneycode)
        {
            try
            {

                var gettokenendpoint = _cxt.AppSettings.Where(x => x.Key == AppSettingKeys.AttorneydetailsWs02endpoint).FirstOrDefault();
                string endpoint = gettokenendpoint.Value;

                AttorneyDetailsApi law = new AttorneyDetailsApi();
                var token = law.ws02gentoken(_LawTrustKey, _LawTrustSecret);

                string email = "tishen.naidoo@xetgroup.com";
                string username = "service manager";
                string role = "SIGNER";

                AttorneyDetails payload = new AttorneyDetails();

                //LawTrustPackage payload = new LawTrustPackage();
                //RestClient client = new RestClient("https://solartestintam.ekurhuleni.gov.za/RCSAttorneyApi/v1/api/AttorneyDetails/AttorneyCodeLookup");

                RestClient client = new RestClient(endpoint);

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                RestRequest request = new RestRequest() { Method = Method.POST };
                request.RequestFormat = DataFormat.Json;
                request.Parameters.Clear();
                request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
                request.AddParameter("LookupRoutine", "WBS8009A", ParameterType.HttpHeader);
                request.AddParameter("UserId", "SOLARBPI", ParameterType.HttpHeader);
                var root = HttpContext.Current.Server.MapPath("~/JSON/");
                string pdfname = "add-attorney-code.json";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
               

                var timeStamp = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ssK");
                string corrId = Guid.NewGuid().ToString(); ;
                string transId = Guid.NewGuid().ToString(); ;
                string messId = Guid.NewGuid().ToString(); ;
                json = json.Replace("{0}", attorneycode);
                json = json.Replace("{1}", timeStamp);
                json = json.Replace("{2}", corrId);
                json = json.Replace("{3}", transId);
                json = json.Replace("{4}", messId);

                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = client.Execute(request);
                var content = response.Content;

                dynamic data = JObject.Parse(content);
                data = data.SolarERP.Payload;
                //data = data[0];
                if (content != string.Empty)
                {
                    
                   
                    payload.AttorneyName = Convert.ToString(data.AttorneyName);
                    payload.AttorneyCode = Convert.ToString(data.AttorneyCode);
                    payload.AddressLine1 = Convert.ToString(data.AddressLine1);
                    if(payload.AddressLine1.Contains("P.O"))
                        {
                        payload.POBox = payload.AddressLine1;
                    }
                    else
                    {
                        payload.StreetName = payload.AddressLine1;

                    }
                    payload.AddressLine2 = Convert.ToString(data.AddressLine2);
                    payload.Suburb = Convert.ToString(data.AddressLine2);
                    payload.Postcode = Convert.ToString(data.Postcode);
                    payload.WorkNumber = Convert.ToString(data.WorkNumber);
                    var status3 = Convert.ToString(data.StatusMessages);

                    foreach (var file in data.StatusMessages)
                    {
                        System.Console.WriteLine(file);
                        payload.Status = file;
                    }
                    //int indexstart = status3.IndexOf('"');
                    //string ss = status3.Substring(indexstart);

                    //int indexstart2 = ss.IndexOf('"');
                    //string ss2 = status3.Substring(indexstart2);

                    //var mod = JsonConvert.DeserializeObject(content);
                    //var test2= JsonConvert.DeserializeObject<List<string>>(data.StatusMessages);
                    //var sss = status3.TRIM(MID(SUBSTITUTE(A2, """", REPT(" ", 999)), 2999, 999))
                    //String[] arr = Convert.ToString(data.StatusMessages).split("(?x)   " +
                    // ",          " +   // Split on comma
                    // "(?=        " +   // Followed by
                    // "  (?:      " +   // Start a non-capture group
                    // "    [^\"]* " +   // 0 or more non-quote characters
                    // "    \"     " +   // 1 quote
                    // "    [^\"]* " +   // 0 or more non-quote characters
                    // "    \"     " +   // 1 quote
                    // "  )*       " +   // 0 or more repetition of non-capture group (multiple of 2 quotes will be even)
                    // "  [^\"]*   " +   // Finally 0 or more non-quotes
                    // "  $        " +   // Till the end  (This is necessary, else every comma will satisfy the condition)
                    // ")          "     // End look-ahead
                    //     );
                    //var jArray = JArray.Parse(data.StatusMessages);
                    //var result = jArray.FirstOrDefault();
                    ////var test = Convert.ToString(data.StatusMessages);
                    //// dynamic data2 = JObject.Parse(data.StatusMessages);
                    //var statt = data.StatusMessages.slice(1);
                    //payload.StatusMessages.Add(data.statStatusMessages);
                    //var JsonArr = data.StatusMessages;
                    //for(int i=0; i< JsonArr.length(); i++)
                    //{
                    //    payload.StatusMessages.Add(JsonArr.getString(i));
                    //}

                    //payload.StatusMessages = data.StatusMessages;
                    payload.Json = Convert.ToString(content);

                }






               return payload;
            }
            catch (Exception x)
            {

                throw x;
                
            }
        }
        //public AttorneyDetails AttorneyDetails2(string attorneycode)
        //{
        //    try
        //    {

        //        AttorneyDetailsApi law = new AttorneyDetailsApi();
        //        var token = law.ws02gentoken(_LawTrustKey, _LawTrustSecret);

        //        string email = "tishen.naidoo@xetgroup.com";
        //        string username = "service manager";
        //        string role = "SIGNER";

        //        AttorneyDetails payload = new AttorneyDetails();

        //        //LawTrustPackage payload = new LawTrustPackage();
        //        RestClient client = new RestClient("http://10.1.2.222:8086/solarbpi/rest/WfLookup/");

        //        ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

        //        RestRequest request = new RestRequest() { Method = Method.POST };
        //        request.RequestFormat = DataFormat.Json;
        //        request.Parameters.Clear();
        //        request.AddParameter("Authorization", "Bearer " + token, ParameterType.HttpHeader);
        //        request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
        //        request.AddParameter("Accept", "application/json ", ParameterType.HttpHeader);
        //        request.AddParameter("LookupRoutine", "WBS8009A", ParameterType.HttpHeader);
        //        request.AddParameter("UserId", "SOLARBPI", ParameterType.HttpHeader);
        //        var root = HttpContext.Current.Server.MapPath("~/JSON/");
        //        string pdfname = "add-attorney-code.json";
        //        var path = System.IO.Path.Combine(root, pdfname);
        //        path = System.IO.Path.GetFullPath(path);
        //        var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
        //        json = json.Replace("{0}", attorneycode);



        //        request.AddParameter("application/json", json, ParameterType.RequestBody);

        //        var response = client.Execute(request);
        //        var content = response.Content;

        //        if (content != string.Empty)
        //        {
        //            dynamic data = JObject.Parse(content);

        //            payload.AttorneyName = Convert.ToString(data.AttorneyName);
        //            payload.AttorneyCode = Convert.ToString(data.AttorneyCode);
        //            payload.AddressLine1 = Convert.ToString(data.AddressLine1);
        //            payload.AddressLine2 = Convert.ToString(data.AddressLine2);
        //            payload.Postcode = Convert.ToString(data.Postcode);
        //            payload.WorkNumber = Convert.ToString(data.WorkNumber);
        //            payload.StatusMessages = Convert.ToString(data.StatusMessages);

        //            return data;
        //        }






               
        //    }
        //    catch (Exception x)
        //    {

        //        throw x;
        //    }
        //}

        public static LawTrustPackage InsertSignatureBlock(string packageid, string docuemntid, string token)
        {
            try
            {

                LawTrustPackage payload = new LawTrustPackage();
                RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/" + packageid + "/documents/" + docuemntid + "/fields/autoplace");

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


        public static LawTrustPackage ShareDocument(string packageid, string docuemntid, string token)
        {
            try
            {

                LawTrustPackage payload = new LawTrustPackage();
                RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/" + packageid + "/workflow");

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

        public static string GenerateIntergrationLink(string packageid, string docuemntid, string token)
        {
            try
            {
                string package_id = packageid.ToString();

                string useremail = "tishen.naidoo@xetgroup.com";



                LawTrustPackage payload = new LawTrustPackage();
                RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/links/integration");

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
                RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/" + packageid + "/log");

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
                RestClient client = new RestClient("https://uatapi.signinghub.co.za/v3/packages/" + packageid + "/base64?");

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