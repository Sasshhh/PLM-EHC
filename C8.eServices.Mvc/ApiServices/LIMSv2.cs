using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;


namespace C8.eServices.Mvc.ApiServices
{
    public static class LIMSv2
    {
        #region
        public const string ZERO_PROPERTIES = "a_owns_zero_or_none_registered";
        public const string HAS_PROPERTIES = "a_owns_property_within_coe";
        public const string API_ERROR_or_NULL = "a_api_error_occured_or_null";
        public const string API_Success_Result = "Success";
        public static string TokenEndpoint = null;
        public static string APIEndpoint = null;
        public static string APIKey = null;
        public static string APISecret = null;
        public static string Token = null;
        public static int Payload = 0;
        #endregion

        public static string GetAppsettingsValue(eServicesDbContext core, string AppsettingsKey)
            => core.AppSettings.FirstOrDefault(a => a.Key == AppsettingsKey).Value;
        public static void DataInitializer()
        {
            using (eServicesDbContext core = new eServicesDbContext())
            {
                TokenEndpoint = GetAppsettingsValue(core, AppSettingKeys.limsWs02gentokenendpoint);
                APIEndpoint = GetAppsettingsValue(core, AppSettingKeys.limsWs02endpoint);
                APIKey = GetAppsettingsValue(core, AppSettingKeys.limsWso2key);
                APISecret = GetAppsettingsValue(core, AppSettingKeys.limsWso2Secret);
            }
        }

        public static Lims10v2ViewModelRoot wso2limsapi()
        {
            Lims10v2ViewModelRoot lims10V2ViewModelRoot = new Lims10v2ViewModelRoot();
            try
            {
                RestClient client = new RestClient(APIEndpoint);

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;

                RestRequest request = new RestRequest() { Method = Method.GET };
                request.AddParameter("Authorization", "Bearer " + Token, ParameterType.HttpHeader);
                request.AddParameter("Accept", "application/json", ParameterType.HttpHeader);

                IRestResponse response = client.Execute(request);
                var content = response.Content;

                dynamic data = JObject.Parse(content);

                if (content != string.Empty)
                {
                    lims10V2ViewModelRoot = JsonConvert.DeserializeObject<Lims10v2ViewModelRoot>(content);
                }

            }
            catch (Exception error)
            {
                //throw;
            }
            return lims10V2ViewModelRoot;
        }

        public static string GetLimsIdenityLinkedProperties(string IdentificationNumber)
        {
            DataInitializer();
            Token = WSO2Token.Token(TokenEndpoint, APISecret, APIKey);
            APIEndpoint = string.Format($"{APIEndpoint}{IdentificationNumber}");
            Lims10v2ViewModelRoot result = wso2limsapi();
            if (result != null)
            {
                if (result.SolarERP.Header.Result.Status == API_Success_Result)
                {
                    if (result.SolarERP.Payload != null)
                        Payload = (result.SolarERP.Payload.Count);
                    else
                        return ZERO_PROPERTIES;
                    if (Payload > 0)
                        return HAS_PROPERTIES;
                }
                else
                    return API_ERROR_or_NULL;
            }
            else
                return API_ERROR_or_NULL;
            return null;
        }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    #region Models
    public class Context
    {
        public Property Property { get; set; }
    }

    public class Header
    {
        public string Version { get; set; }
        public string TimeStamp { get; set; }
        public string CorrelationID { get; set; }
        public string TransactionID { get; set; }
        public string MessageID { get; set; }
        public string SenderID { get; set; }
        public string RecipientID { get; set; }
        public string Action { get; set; }
        public string SolarUser { get; set; }
        public Message Message { get; set; }
        public Result Result { get; set; }
        public List<Context> Context { get; set; }
    }

    public class Message
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class Payload
    {
        public string binjurisdiction { get; set; }
        public object excludedextents { get; set; }
        public string regioncode { get; set; }
        public string categorycode { get; set; }
        public string spropertysource { get; set; }
        public string effectiveextents { get; set; }
        public string id_townshipext { get; set; }
        public string idpropertytypeclass { get; set; }
        public string registeredownernames { get; set; }
        public string regionname { get; set; }
        public object proposedmarketvalue { get; set; }
        public string landusename { get; set; }
        public string billingaccountnumber { get; set; }
        public string townshipextensionnumber { get; set; }
        public object ivirtualpropertynumber { get; set; }
        public object ssname { get; set; }
        public string sgextents { get; set; }
        public string townshipextname { get; set; }
        public object unit { get; set; }
        public string re { get; set; }
        public string propertytypecode { get; set; }
        public string townshipname { get; set; }
        public object basepropertytype { get; set; }
        public string erfnumber { get; set; }
        public object id_sectionalscheme { get; set; }
        public object improvementextents { get; set; }
        public string portion { get; set; }
        public string registeredownerids { get; set; }
        public string id_property { get; set; }
        public string landusecode { get; set; }
        public string ward { get; set; }
        public object sectionalschemenumber { get; set; }
        public string tariffcode { get; set; }
        public string id_townshipname { get; set; }
        public string deedextents { get; set; }
        public string idpropertystate { get; set; }
        public string streetaddress { get; set; }
        public object constraintpercent { get; set; }
        public string bssexclusiveusearea { get; set; }
        public string currentmarketvalue { get; set; }
        public string szonecode { get; set; }
        public string spropertytype { get; set; }
        public string lpi { get; set; }
        public object improvementscount { get; set; }
        public object sestatename { get; set; }
        public string tariffname { get; set; }
        public string titledeednumbers { get; set; }
        public string gisextents { get; set; }
        public string locationlongitude { get; set; }
        public string townshiptypecode { get; set; }
        public string ssourceaccountnumber { get; set; }
        public object ssuburbname { get; set; }
        public string financialextents { get; set; }
        public string locationlatitude { get; set; }
        public string szonename { get; set; }
        public object updatedsourceaccountnumber { get; set; }
        public string billingidentifier { get; set; }
        public string categoryname { get; set; }
    }

    public class Property
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Result
    {
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }

    public class Lims10v2ViewModelRoot
    {
        public SolarERP SolarERP { get; set; }
    }

    public class SolarERP
    {
        public Header Header { get; set; }
        public List<Payload> Payload { get; set; }
    }


    #endregion
}