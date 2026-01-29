using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
//using JamaaTech.Smpp.Net.Client;
//using JamaaTech.Smpp.Net.Lib.Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace C8.eServices.Mvc.Helpers
{
    public class SmsHelper
    {
        public static bool Send(string mobileNumber, string message)
        {
            eServicesDbContext cxt = new eServicesDbContext();

            if (message.Length > 256)
                message = message.Substring(0, 256);

            // Demo, remove.
            //return true;

            mobileNumber = FormatMobileNo(mobileNumber);
            //if (mobileNumber == null) throw new Exception("Invalid mobile number.");

            //string messages = "";
            //SmppClient client = new SmppClient();

            //try
            //{
            //    SmppConnectionProperties properties = new SmppConnectionProperties();
            //    TextMessage msg = new TextMessage();

            //    client = new SmppClient();

            //    var host = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.SmppHost).Value;
            //    var username = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.SmppUsername).Value;
            //    var password = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.SmppPassword).Value;
            //    var port = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.SmppPort).Value;

            //    properties = client.Properties;
            //    properties.SystemID = username;
            //    properties.Password = password;
            //    properties.Port = Convert.ToInt32(port);
            //    properties.Host = host;
            //    properties.SystemType = "smpp";
            //    properties.DefaultServiceType = ServiceType.DEFAULT;

            //    client.AutoReconnectDelay = 3000;
            //    client.KeepAliveInterval = 30000;

            //    client.Start();

            //    if (client.ConnectionState != SmppConnectionState.Connected) client.ForceConnect(5000);

            //    msg = new TextMessage();

            //    msg.DestinationAddress = mobileNumber;
            //    msg.SourceAddress = ""; //sourceA;
            //    msg.Text = message;
            //    msg.RegisterDeliveryNotification = true;

            //    client.SendMessage(msg, 3000);
            //    messages = "Success";
            //}
            //catch (Exception e)
            //{
            //    return false;
            //    //messages = "failure: " + e;
            //}
            //finally
            //{
            //    client.Shutdown();
            //}
            string messages = "";
            String CustomerID = "jai1970@gmail.com";
            String API_KEY = "9dad8a95-42d2-4633-8057-ed86ddbbd0cc";
            string endpoint = "https://www.zoomconnect.com/app/api/rest/v1/sms/send.json";
            HttpWebResponse response = null;
            Encoding enc = System.Text.Encoding.GetEncoding(1252);
            try
            {
                string authInfo = CustomerID + ":" + API_KEY;
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                HttpWebRequest http = (HttpWebRequest)WebRequest.Create(endpoint);
                http.Method = "POST";
                http.PreAuthenticate = true;
                http.Headers["Authorization"] = "Basic " + authInfo;
                http.ContentType = "application/json";

                using (var streamWriter = new StreamWriter(http.GetRequestStream()))
                {
                    string json = "{\"message\":\"" + message + "\"," +
                                    "\"recipientNumber\":\"" + mobileNumber + "\"}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                response = (HttpWebResponse)http.GetResponse();
            }
            catch (WebException ex)
            {
                messages = ("Exception: " + ex.Message);
                return false;
            }
            finally
            {
                if (response != null)
                {
                    StreamReader loResponseStream = new StreamReader(response.GetResponseStream(), enc);
                    messages = loResponseStream.ReadToEnd();
                }


            }

            return true;

            //string url = @"https://localhost/SendSms/api/SendSms";
            //string parameters = string.Format("?mobileNo={0}&message={1}", mobileNumber, HttpUtility.UrlEncode(message));

            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url + parameters);
            //request.Method = "GET";
            //string status = string.Empty;

            //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //{
            //    Stream dataStream = response.GetResponseStream();
            //    StreamReader reader = new StreamReader(dataStream);
            //    status = reader.ReadToEnd();
            //    reader.Close();
            //    dataStream.Close();

            //    if (status.Equals("Success"))
            //        return true;
            //}

            //return false;
        }

        public static string FormatMobileNo(string mobileNo)
        {
            if (!mobileNo.Trim().Equals(string.Empty))
            {
                if (mobileNo[0] == '0') return "27" + mobileNo.Substring(1).Replace(" ", "");
                else return mobileNo.Replace(" ", "");
            }

            return null;
        }
    }
}
