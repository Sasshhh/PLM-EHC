using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Web;


namespace C8.eServices.Mvc.ApiServices
{
    public class RCSAttorneyApi
    {
        #region GetAttorneyDetails
         public static List<AttorneyDetails> GetAttorneyDetails(string id)
        {
            List<AttorneyDetails> attorneyDetails = new List<AttorneyDetails>();

            try
            {
                using (var cxt = new eServicesDbContext())
                {
                    string url = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AttorneyDetails).Value;
                    url = string.Format("{0}{1}", url, id.Trim());

                    Uri myUri = new Uri(url);
                    var ip = Dns.GetHostAddresses(myUri.Host)[0];

                    Ping requestServer = new Ping();
                    PingReply serverResponse = requestServer.Send(ip);

                    if (serverResponse.Status == IPStatus.Success)
                    {
                        var request = (HttpWebRequest)WebRequest.Create(url);

                        request.Method = "GET";
                        request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                        var content = string.Empty;

                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            using (var stream = response.GetResponseStream())
                            {
                                using (var sr = new StreamReader(stream))
                                {
                                    content = sr.ReadToEnd();
                                }
                            }
                        }

                        JArray jsonArray = JArray.Parse(content);
                        dynamic data = JObject.Parse(jsonArray[0].ToString());

                        var AttorneyCode = id.Trim();
                        string FirmName, FirmAddress, FirmPostalCode, AttorneyName, CCCLocation = "";
                        DateTime GeneratedDate;

                        JToken details = data;

                        id = details.Root["id"].Value<string>();
                        AttorneyCode = details.Root["attorneyCode"].Value<string>();
                        FirmName = details.Root["firmName"].Value<string>();
                        FirmAddress = details.Root["firmAddress"].Value<string>();
                        FirmPostalCode = details.Root["firmPostalCode"].Value<string>();
                        AttorneyName = details.Root["attorneyName"].Value<string>();
                        CCCLocation = details.Root["cccLocation"].Value<string>();
                        GeneratedDate = details.Root["generatedDate"].Value<DateTime>();

                        attorneyDetails.Add(new AttorneyDetails()
                        {
                            Id = Convert.ToInt32(id),
                            AttorneyCode = AttorneyCode,
                            FirmName = FirmName,
                            FirmAddress = FirmAddress,
                            FirmPostalCode = FirmPostalCode,
                            AttorneyName = AttorneyName,
                            CCCLocation = CCCLocation,
                            GeneratedDate = GeneratedDate
                        });
                    }
                }
            }
            catch (Exception x)
            {

                string ErrMess = x.ToString();
                return attorneyDetails;
            }

            return attorneyDetails;
        }
        #endregion
    }
}