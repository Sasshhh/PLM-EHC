using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Newtonsoft.Json.Linq;
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
    public class CustomerAccountApi
    {
        public static List<Account> GetAccounts(string id)
        {
            List<Account> accounts = new List<Account>();

            try
            {
                using (var cxt = new eServicesDbContext())
                {
                    string url = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.CustomerAccountsApi).Value;
                    url = string.Format("{0}?rsaIDNumber={1}", url, id);

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

                        JObject jo = JObject.Parse(content);
                        string accountNo = "";
                        string StreetDes = string.Empty;//not used
                        string description = "";
                        string accHolder = "";
                        string IDnum = "";
                        //var errorFlag = bool.Parse(jo["errorFlag"].Value<string>());

                        //if (errorFlag)
                        //{
                        //    throw new Exception("Venus/Solar API is unavailable.");
                        //}

                        foreach (JToken account in jo.SelectToken("customerResult"))
                        {
                            accountNo = account["customerAccountStatus"]["venusAccountNo"].Value<string>();
                            IDnum= account["customerContactDetails"]["IDNumber"].Value<string>();
                            accHolder = account["customerContactDetails"]["fullName"].Value<string>();
                            //  accHolder = account[""].ToString();
                            if (account["propertyDetails"]["street2"].Value<string>() != null && account["propertyDetails"]["street2"].Value<string>() != "") //Matches the Lookup data Format
                            {
                                // StreetDes = account["propertyDetails.street1"].ToString() +" "+ account["propertyDetails.street2"].ToString() + " "+ account["propertyDetails.street3"].ToString();
                                //To Do change the Street  address to only show Street Name so it matches the CustomerAccounts Table data.
                                description = string.Format("{0} - {1} {2}", accountNo, account["propertyDetails"]["street2"].Value<string>(), account["propertyDetails"]["street3"].Value<string>());
                               
                            }
                            else if(account["customerContactDetails"]["postalAddress1"].Value<string>() != null&& account["customerContactDetails"]["postalAddress1"].Value<string>() !="") //Contains the Full Address details including the house No
                            {
                               description = string.Format("{0} - {1}", accountNo, account["customerContactDetails"]["postalAddress1"].Value<string>());
                            }
                            else
                            {
                                description = string.Format("{0} - No address data", accountNo);
                            }

                            accounts.Add(new Account()
                            {
                                AccountNumber = accountNo,
                                ReferenceNumber = IDnum,
                                AccountHolder = accHolder,
                                Description = description

                            });
                        }
                    }
                }
            }
            catch (Exception x)
            {

                string ErrMess= x.ToString();
                return accounts;
            }

            return accounts;
        }

        public static AccountInformation GetBalance(string accountNo)
        {
            AccountInformation accInfo = new AccountInformation();

            try
            {
                using (var cxt = new eServicesDbContext())
                {
                    string url = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AccountBalanceApi).Value;
                    string username = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AccountBalanceApiUsername).Value;
                    string password = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AccountBalanceApiPassword).Value;

                    url = string.Format("{0}?emmAccountNo={1}&username={2}&password={3}", url, accountNo, username, password);

                    Ping requestServer = new Ping();
                    PingReply serverResponse = requestServer.Send(url);

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

                        // JK.20190626a - TODO: Include trapping for server offline response.

                        JObject jo = JObject.Parse(content);
                        decimal disconnectionRequireAmount;
                        string ownership;
                        string disconnectionStatus;
                        DateTime disconnectionDate;
                        string indigentStatus;
                        decimal currentBalance;
                        var errorFlag = bool.Parse(jo["errorFlag"].Value<string>());

                        if (errorFlag)
                            throw new Exception("Venus/Solar API is unavailable.");

                        foreach (JToken details in jo.SelectToken("customerDetails"))
                        {
                            disconnectionRequireAmount = decimal.Parse(details["customerAccountStatus.disconnectionRequireAmount"].ToString());
                            ownership = details["customerAccountStatus.ownership"].ToString();
                            disconnectionStatus = details["customerAccountStatus.disconnectionStatus"].ToString();
                            disconnectionDate = DateTime.Parse(details["customerAccountStatus.disconnectionDate"].ToString());
                            indigentStatus = details["customerAccountStatus.indigentStatus"].ToString();
                            currentBalance = decimal.Parse(details["customerBalanceDetails.currentBalance"].ToString());

                            accInfo = new AccountInformation()
                            {
                                DisconnectionRequireAmount = disconnectionRequireAmount,
                                Ownership = ownership,
                                DisconnectionStatus = disconnectionStatus,
                                DisconnectionDate = disconnectionDate,
                                IndigentStatus = indigentStatus,
                                CurrentBalance = currentBalance
                            };
                        }
                    }
                }
            }
            catch (Exception x)
            {

                throw x;
            }

            return accInfo;
        }
    }
}