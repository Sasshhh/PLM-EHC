using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using C8.eServices.Mvc.ViewModels;
using C8.eServices.Mvc.ApiServices;
using C8.eServices.Mvc.Keys;
using System.Globalization;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private DataWarehouse db = new DataWarehouse();
        private eServicesDbContext _context = new eServicesDbContext();

        public IdentityManager IdentityManager { get; set; }
        public UserManager<SystemIdentityUser> UserManager { get; private set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        BaseHelper _base = new BaseHelper();

        public void Initialise()
        {
            _base.Initialise(_context);
            SystemUser = _base.SystemUser;
            Customer = _base.Customer;

            var context = new eServicesDbContext();

            IdentityManager = new IdentityManager(context);
            UserManager =
                new UserManager<SystemIdentityUser>(
                new UserStore<SystemIdentityUser>(context));
        }

        public PaymentController()
        {
            Initialise();
        }

        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        private int CheckScanned(string code)
        {
            // JK.20190629 - Moved the settings to db.            
            var asMpUrl = _context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.MasterpassUrl);
            var asMpUsername = _context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.MasterpassUsername);
            var asMpPassword = _context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.MasterpassPassword);

            if (asMpUrl == null) throw new Exception("AppSetting masterpass url does not exists.");
            if (asMpUsername == null) throw new Exception("AppSetting masterpass username does not exists.");
            if (asMpPassword == null) throw new Exception("AppSetting masterpass password does not exists.");

            string url = string.Format("{0}/public/{1}/scanned", asMpUrl.Value, code);
            WebRequest request = WebRequest.Create(url);
            WebResponse response;
            string result = string.Empty;
            request.ContentType = "application/json";
            request.Method = "GET";

            NetworkCredential credentials = new NetworkCredential(asMpUsername.Value, asMpPassword.Value);
            CredentialCache credentialCache = new CredentialCache();
            credentialCache.Add(new Uri(url), "Basic", credentials);
            request.PreAuthenticate = true;
            request.Credentials = credentialCache;

            try
            {
                response = (WebResponse)request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                    JObject jo = JObject.Parse(result);
                    result = jo["scanned"].Value<string>();
                }
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }

            if (bool.Parse(result))
                return 2;
            else
                return 0;
        }

        // GET: Payment/Details/5
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        [EncryptedActionParameter]
        public ActionResult Details(string accountNumber, string _billDate)
        {
            accountNumber = accountNumber.Replace("z", string.Empty);
            List<Payment> payments = new List<Payment>();
            decimal total = 0;
            
            using (var bcxt = new BillingDbContext())
            {
                var biList = bcxt.BillInfos.Where(o => o.AccountNumber.Trim() == accountNumber).ToList();
                var billInfo = biList.FirstOrDefault(o => o.AccountNumber.Trim() == accountNumber && o.DueDateConvert.Year == DateTime.Now.Year && o.DueDateConvert.Month == DateTime.Now.Month);
                AccountInformation billInt = null;
                decimal bill = 0;

                try
                {
                    billInt = CustomerAccountApi.GetBalance(accountNumber.ToString());
                }
                catch (Exception)
                {
                    // JK.20190629a - TODO: Move this value to appsettings.
                    bill = 100;
                }

                if (billInt != null)
                    bill = billInt.TotalAmount;
                else if (billInfo != null)
                    bill = billInfo.TotalConvert;

                total = decimal.Parse(bill.ToString());
            }

            Payment payment = new Payment()
            {
                Amount = total,
                AccountNumber = accountNumber,
                BillDate = DateTime.Parse(_billDate)
            };
            if (payment == null)
            {
                return HttpNotFound();
            }
            
            // JK.20190401 - Pay via masterpass.
            PayViaMasterPass(payment);

            return View(payment);
        }

        // GET: Payment/Details/5
        public ActionResult Result()
        {
            //Insert the returned data as well as the merchant specific data PAYGATE_ID and REFERENCE
            var data = new Dictionary<string, string>()
            {
                {"PAYGATE_ID", Regex.IsMatch(SessionModel.pgid, @"^\d{9,12}$") ? SessionModel.pgid : "INVALID ID" },
                {"PAY_REQUEST_ID", Regex.IsMatch(Request["PAY_REQUEST_ID"], @"^[A-Z0-9]{8}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{12}$") ? Request["PAY_REQUEST_ID"] : "INVALID PAY_REQUEST_ID" },
                {"TRANSACTION_STATUS", Regex.IsMatch(Request["TRANSACTION_STATUS"], @"^[A-Za-z0-9]*$") ? Request["TRANSACTION_STATUS"] : "INVALID TRANSACTION STATUS" },
                {"REFERENCE", SessionModel.reference },
                {"CHECKSUM", Regex.IsMatch(Request["CHECKSUM"], @"^[A-Za-z0-9]*$") ? Request["CHECKSUM"] : "INVALID CHECKSUM" },
            };

            var checksum = data["CHECKSUM"];

            var pg3 = new PaygatePayweb3();
            pg3.setEncryptionKey(SessionModel.key);

            //Validate returned checksum
            var isValid = pg3.validateChecksum(data);

            StringBuilder validHtml = new StringBuilder();
            validHtml.Append(!isValid ? "The checksums do not match <i class=\"glyphicon glyphicon-remove text-danger\"></i>" : "Checksums match OK <i class=\"glyphicon glyphicon - ok text - success\"></i>");
            var validText = validHtml;

            int transactionStatus;
            Int32.TryParse(data["TRANSACTION_STATUS"], out transactionStatus);

            var key = SessionModel.key;

            ViewBag.Checksum = checksum;
            ViewBag.ValidText = validText;
            ViewBag.Data = data;
            ViewBag.TransactionStatus = transactionStatus;

            return View();
        }

        private void PayViaSID(Payment payment)
        {
            var processUrl = PaygatePayweb3.process_url;
            var optionalHtml = string.Empty;

            var payGateId = "10011072130";
            var reference = "pgtest_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            var amount = "1.00";
            var currency = "ZAR";
            var returnUrl = "http://localhost:3456/Payment/Result";
            var locale = "en-za";
            var country = "ZAF";
            var transactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var customerEmail = "jayan.kistasami@durban.gov.za";
            var encryptionKey = "secret";

            //Get the mandatory request data from the request and do some validation checks on it
            //Test for "Reference" is only for these examples
            var mandatoryFields = new Dictionary<string, string>()
            {
                {"PAYGATE_ID", Regex.IsMatch(payGateId, @"^\d{9,12}$") ? payGateId : "INVALID ID" },
                {"REFERENCE", Regex.IsMatch(reference, @"^pgtest_\d{14}$") ? reference : "INVALID REFERENCE" },
                {"AMOUNT", Regex.IsMatch(amount, @"^\d*\.?\d{0,2}$") ? amount : "INVALID AMOUNT" },
                {"CURRENCY", Regex.IsMatch(currency, @"^[A-Z]{3}$") ? currency : "INVALID CURRENCY" },
                {"RETURN_URL", Regex.IsMatch(returnUrl, @"^(https?:\/\/)?([a-z0-9\.-]+):?\.?([/\w\.-]*)*\/?$") ? returnUrl : "INVALID RETURN_URL" },
                {"TRANSACTION_DATE", Regex.IsMatch(transactionDate, @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$") ? transactionDate : "INVALID TRANSACTION DATE" },
                {"LOCALE", Regex.IsMatch(locale, @"^[a-z]{2}-[a-zA-Z]{2}$") ? locale : "INVALID LOCALE" },
                {"COUNTRY", Regex.IsMatch(country, @"^[A-Z]{3}$") ? country : "INVALID COUNTRY" },
                {"EMAIL", Regex.IsMatch(customerEmail, @"^([a-z0-9_\.-]+)@([a-z0-9_\.-]+)\.([a-z\.]{2,6})$") ? customerEmail : "INVALID EMAIL" },
            };

            //Set the session variables needed on the result page
            SessionModel.pgid = mandatoryFields["PAYGATE_ID"];
            SessionModel.reference = mandatoryFields["REFERENCE"];
            SessionModel.key = encryptionKey;

            //Get the optional fields and validate
            var optionalFields = new Dictionary<string, string>()
            {
                {"PAY_METHOD", Request["PAY_METHOD"] != null && Request["PAY_METHOD"] != "" ? Regex.IsMatch(Request["PAY_METHOD"], @"^[a-zA-Z ]{0,10}$") ? Request["PAY_METHOD"] : "INVALID PAY METHOD" : "" },
                {"PAY_METHOD_DETAIL", Request["PAY_METHOD_DETAIL"] != null && Request["PAY_METHOD_DETAIL"] != "" ? Regex.IsMatch(Request["PAY_METHOD_DETAIL"], @"^[a-zA-Z ]{0,10}$") ? Request["PAY_METHOD_DETAIL"] : "INVALID PAY METHOD" : "" },
                {"NOTIFY_URL", Request["NOTIFY_URL"] != null && Request["NOTIFY_URL"] != "" ? Regex.IsMatch(Request["NOTIFY_URL"], @"^(https?:\/\/)?([a-z0-9\.-]+):?\.?([/\w\.-]*)*\/?$") ? Request["NOTIFY_URL"] : "INVALID URL" : "" },
                {"USER1", Request["USER1"] != null && Request["USER1"] != "" ? Regex.IsMatch(Request["USER1"], @"^[a-zA-Z \d]{0,10}$") ? Request["USER1"] : "INVALID FORMAT" : "" },
                {"USER2", Request["USER2"] != null && Request["USER2"] != "" ? Regex.IsMatch(Request["USER2"], @"^[a-zA-Z \d]{0,10}$") ? Request["USER2"] : "INVALID FORMAT" : "" },
                {"USER3", Request["USER3"] != null && Request["USER3"] != "" ? Regex.IsMatch(Request["USER3"], @"^[a-zA-Z \d]{0,10}$") ? Request["USER3"] : "INVALID FORMAT" : "" },
                {"VAULT", Request.Form.AllKeys.Contains("VAULT") ? Regex.IsMatch(Request["VAULT"], @"^[a-zA-Z \d]{0,10}$") ? Request["VAULT"] : "INVALID FORMAT" : "" },
                {"VAULT_ID", Request.Form.AllKeys.Contains("VAULT_ID") ? Regex.IsMatch(Request["VAULT_ID"], @"^[a-zA-Z \d]{0,10}$") ? Request["VAULT_ID"] : "INVALID FORMAT" : "" },
            };

            //Merge the optional fields
            foreach (var item in optionalFields)
            {
                mandatoryFields.Add(item.Key, item.Value);
            }
            var data = mandatoryFields;

            var displayOptionalFields = false;

            foreach (var item in optionalFields)
            {
                if (data[item.Key] != "")
                {
                    displayOptionalFields = true;
                }
            }

            //Display optional fields if any
            if (displayOptionalFields)
            {
                StringBuilder optional = new StringBuilder();
                optional.Append("<div class=\"well\">");

                foreach (var item in optionalFields)
                {
                    if (data[item.Key] != "")
                    {
                        optional.Append("<div class=\"form-group\"><label for=\"");
                        optional.Append(item.Key);
                        optional.Append("\" class=\"col-sm-3 control-label\">");
                        optional.Append(item.Key);
                        optional.Append("</label><p id=\"");
                        optional.Append(item.Key);
                        optional.Append("\" class=\"form-control-static\">");
                        optional.Append(data[item.Key]);
                        optional.Append("</p></div>");
                    }
                }
                optional.Append("</div>");
                optionalHtml = optional.ToString();
            }

            //var encryptionKey = Request["encryption_key"];

            //Set session variables
            Session["pgid"] = data["PAYGATE_ID"];
            Session["reference"] = data["REFERENCE"];
            Session["key"] = encryptionKey;

            var pw3 = new PaygatePayweb3();
            pw3.setEncryptionKey(encryptionKey);
            pw3.setInitiateRequest(data);

            var checksum = pw3.generateChecksum(data);

            //Initiate request and check for valid response
            var returnData = pw3.doInitiate();

            StringBuilder results = new StringBuilder();
            var resultsHtml = "";
            StringBuilder resultsForm = new StringBuilder();
            var resultsFormHtml = string.Empty;

            if (returnData) //Have a valid response
            {
                if (pw3.lastError == null)
                {
                    //There is no error - continue
                    foreach (var item in pw3.processRequest)
                    {
                        results.Append(item.Key + " = " + item.Value + "\n");
                    }
                    resultsHtml = results.ToString();
                }

                if (pw3.lastError != "")
                {
                    //There is no error - continue
                    //Check checksums match
                    var isValid = pw3.validateChecksum(pw3.initiateResponse);
                    if (isValid)
                    {
                        foreach (var item in pw3.processRequest)
                        {
                            resultsForm.Append("<input type=\"hidden\" name=\"" + item.Key + "\" value=\"" + item.Value + "\"/>");
                        }
                        resultsFormHtml = resultsForm.ToString();
                    }
                    else
                    {
                        resultsForm.Append("The checksums do not match");
                        resultsFormHtml = resultsForm.ToString();
                    }
                }
            }
            else
            {
                resultsHtml = "There was an error in the request. Please check your data.";
            }

            ViewBag.Data = data;
            ViewBag.OptionalHtml = optionalHtml;
            ViewBag.EncryptionKey = encryptionKey;
            ViewBag.ResultsHtml = resultsHtml;
            ViewBag.ResultsFormHtml = resultsFormHtml;
        }

        private void PayViaMasterPass(Payment payment)
        {
            // Code to avoid below error: The request was aborted: Could not create SSL/TLS secure channel.
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (sender, x509Certificate, chain, sslPolicyErrors) => true;
            //var account = payment.AccountNumber.Replace("z", string.Empty); // to take the z away from the account number after forcing it into a string -- see LoadOnlineBils
            string descript = "Account Number:" + payment.AccountNumber.Replace("z", string.Empty) + "\\nDate:" + payment.BillDate.ToString("yyyy-MM-dd") + "";
            string json = "{" +
                "\"amount\": \"" + payment.Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace("z", string.Empty) + "\"," +
                "\"merchantReference\": \"" + payment.AccountNumber.Replace("z", string.Empty) + "\"," +
                "\"shortDescription\": \"" + descript + "\"," +
                "\"subMerchantName\": \"Online Payments\",";
                //"\"terminalId\": \"0806cfb2-f475-42d2-8715-0035ca3427a\",";
            if (string.IsNullOrEmpty(Customer.CellPhoneNumber))
                json += "\"smsNotify\": [\"" + Customer.InternationalCellPhoneNumber + "\"],";
            if (!string.IsNullOrEmpty(Customer.EmailAddress))
                json += "\"emailNotify\": [\"" + Customer.EmailAddress + "\"],";
            else if (!string.IsNullOrEmpty(SystemUser.EmailAddress))
                json += "\"emailNotify\": [\"" + SystemUser.EmailAddress + "\"],";
            json += "\"requestPartialPayment\": \"true\"," +
                "\"useOnce\": false" +
                "}";

            // JK.20190629 - Moved the settings to db.            
            var asMpUrl = _context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.MasterpassUrl);
            var asMpUsername = _context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.MasterpassUsername);
            var asMpPassword = _context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.MasterpassPassword);

            if (asMpUrl == null) throw new Exception("AppSetting masterpass url does not exists.");
            if (asMpUsername == null) throw new Exception("AppSetting masterpass username does not exists.");
            if (asMpPassword == null) throw new Exception("AppSetting masterpass password does not exists.");

            string url = string.Format(@"{0}/code/create", asMpUrl.Value);

            WebRequest request = WebRequest.Create(url);
            WebResponse response;
            string result = string.Empty;
            request.ContentType = "application/json";
            request.Method = "POST";

            NetworkCredential credentials = new NetworkCredential(asMpUsername.Value, asMpPassword.Value);
            CredentialCache credentialCache = new CredentialCache();
            credentialCache.Add(new Uri(url), "Basic", credentials);
            request.PreAuthenticate = true;

            request.Credentials = credentialCache;

            byte[] bytes = Encoding.UTF8.GetBytes(json);
            request.ContentLength = bytes.Length;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            // JK.20190208a - Code to simulate the payment list for a user. Must remove.
            try
            {
                response = (WebResponse)request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                    JObject jo = JObject.Parse(result);
                    //payments.Where(p => p.IdentificationNumber == id && p.AccountNumber == accountNo && p.BillDate == billDate).SingleOrDefault().Code = jo["code"].Value<string>(); ;
                    payment.Code = jo["code"].Value<string>();
                    ViewBag.USSDMasPass = "*120*395*" + payment.Code + "#";
                    ViewBag.MasPassCode =payment.Code;
                }

                var mpRequest = new MasterpassRequest()
                {
                    Amount = 0,
                    EmailNotify = !string.IsNullOrEmpty(Customer.EmailAddress) ? Customer.EmailAddress : !string.IsNullOrEmpty(SystemUser.EmailAddress) ? SystemUser.EmailAddress : string.Empty,
                    MerchantReference = payment.AccountNumber.Replace("z", string.Empty),
                    ShortDescription = descript,
                    SmsNotify = Customer.CellPhoneNumber,
                    SubMerchantName = string.Empty,
                    TerminalId = string.Empty,
                    UseOnce = false,
                    Code = payment.Code
                };
                _context.MasterpassRequests.Add(mpRequest);
                _context.SaveChanges();
                
                //Session["payments"] = payments;
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    System.Diagnostics.Debug.WriteLine(reader.ReadToEnd());
                }
            }
        }

        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult PaymentHistory()
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Customer == null)
                    return RedirectToAction("Index", "Profile");

                var phvm = new PaymentHistoryViewModel()
                {
                    Customer = cxt.Customers.FirstOrDefault(o => o.Id == Customer.Id)
                };

                phvm.Customer.Data = SecureActionLinkExtension.Encrypt(String.Format("Id={0}", Customer.Id));
                return View(phvm);
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadPaymentHistory(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    // JK.20190618 - Change code to point to the proper data.
                    var phvm = new PaymentHistoryViewModel()
                    {
                        Customer = cxt.Customers.FirstOrDefault(o => o.Id == Customer.Id)
                    };
                    var custid = Customer.Id;

                    List<PaymentHistoryDTO> list = new List<PaymentHistoryDTO>();


                    // List<PaymentHistory> list = cxt.PaymentHistories.Where(o => o.IsActive && !o.IsDeleted  && o.PaymentType != "MASTERPASS").ToList();
                    var query = (from ot in cxt.Customers
                                 join v in cxt.LinkedAccounts on ot.Id equals v.CustomerId
                                 join a in cxt.Accounts on v.AccountId equals a.Id
                                 join c in cxt.PaymentHistories on a.AccountNumber equals (c.MetroAccountNumber).ToString()
                                 where ot.Id == Id && c.PaymentType != "MASTERPASS"
                                 select new PaymentHistoryDTO()
                                 {
                                    
                                     Id = c.Id,
                                     IsActive = c.IsActive,
                                     IsDeleted = c.IsDeleted,
                                     IsLocked = c.IsLocked,
                                     CreatedBySystemUserId = c.CreatedBySystemUserId,
                                     CreatedDateTime = c.CreatedDateTime,
                                     ModifiedBySystemUserId = c.ModifiedBySystemUserId,
                                     ModifiedDateTime = c.ModifiedDateTime,
                                     MetroAccountNumber = c.MetroAccountNumber,
                                     PaymentDateTime = c.PaymentDateTime,
                                     ReconDateTime = c.ReconDateTime,
                                     BankAccountNumber = c.BankAccountNumber,
                                     BankBranchCode = c.BankBranchCode,
                                     AmountPaid = c.AmountPaid,
                                     PaymentType = c.PaymentType

                                 }).ToList();

                   
                    //List<PaymentHistory> list = new List<PaymentHistory>();
                    //var query = (from ot in cxt.Customers
                    //              join v in cxt.LinkedAccounts on ot.Id equals v.CustomerId
                    //              join a in cxt.Accounts on v.AccountId equals a.Id
                    //              join c in cxt.PaymentHistories on a.AccountNumber equals ( c.MetroAccountNumber).ToString()
                    //              where ot.Id == 3
                    //              select new {c}).ToList();
                    //toDo
                    list = query;

                    foreach (var ph in list)
                    {
                        ph.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", ph.Id));
                    }

                    var json = Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }

                return null;
            }
        }
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadMPPaymentHistory(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    // JK.20190618 - Change code to point to the proper data.


                    List<MasterpassHistoryDTO> list = new List<MasterpassHistoryDTO>();


                    // List<PaymentHistory> list = cxt.PaymentHistories.Where(o => o.IsActive && !o.IsDeleted  && o.PaymentType != "MASTERPASS").ToList();
                    var query = (from ot in cxt.Customers
                                 join v in cxt.LinkedAccounts on ot.Id equals v.CustomerId
                                 join a in cxt.Accounts on v.AccountId equals a.Id
                                 join c in cxt.MasterpassTransactions on a.AccountNumber equals (c.Reference).ToString()
                                 where ot.Id == Id
                                 select new MasterpassHistoryDTO()
                                 {

                                     Id = c.Id,
                                     IsActive = c.IsActive,
                                     IsDeleted = c.IsDeleted,
                                     IsLocked = c.IsLocked,
                                     CreatedBySystemUserId = c.CreatedBySystemUserId,
                                     CreatedDateTime = c.CreatedDateTime,
                                     ModifiedBySystemUserId = c.ModifiedBySystemUserId,
                                     ModifiedDateTime = c.ModifiedDateTime,
                                     Reference = c.Reference,
                                     TransactionId = c.TransactionId,
                                     Amount = c.Amount,
                                     CurrencyCode = c.CurrencyCode,
                                     Status = c.Status,
                                     RetrievalReferenceNumber = c.RetrievalReferenceNumber,
                                     AuthCode = c.AuthCode,
                                     BankResponse = c.BankResponse,
                                     Code = c.Code,
                                     CardType = c.CardType,
                                     BinLast4 = c.BinLast4,
                                     AccountType = c.AccountType,
                                     StatusId = c.StatusId,
                                     MasterpassRequestId = c.MasterpassRequestId

                                 }).ToList();


          
                    list = query;


                 
                    foreach (var ph in list)
                    {
                        ph.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", ph.Id));
                    }

                    var json = Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }

                return null;
            }
        }

        // GET: Payment/EftPayment/5
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult EftPayment()
        {
            SidEftTransaction eft = new SidEftTransaction()
            {
                Merchant = "YOURMERCHANTCODE",
                Currency = "ZAR",
                Country = "ZA",
                Reference = "SELLER-REF",
                DefaultReference = "BEN-REF",
                Amount = 10.53M,
                Custom01 = "438393",
                Custom02 = "782142",
                Custom03 = "519263",
                Custom04 = "233335",
                Custom05 = "282234"
            };

            return View(eft);
        }

        // GET: Payment/EftPayment/5
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult CardPayment()
        {
            return View();
        }

        //Get All MasterPass Payments
        //[Authorize(Roles = "Financial Clerk" + "," + "Super Administrators")]
        public ActionResult GAMP()
        {
            using (var cxt = new eServicesDbContext())
            {
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);
                //return View(cxt.MasterpassTransactions.Include(o =>o.MasterpassRequest).ToList());
                return View();

            }

        }
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult GAMPLoad()
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);
                var query = (from a in cxt.MasterpassTransactions
                             join b in cxt.MasterpassRequests on a.MasterpassRequestId equals b.Id
                             join c in cxt.SystemUsers on b.CreatedBySystemUserId equals c.Id

                             select new
                             {

                                 Account = a.Reference,
                                 Name = c.FirstName + " " + c.LastName,
                                 CustomerID = c.IdentificationNumber,
                                 Email = c.EmailAddress,
                                 Number = c.MobileNumber,
                                 CreatedDateTime = a.CreatedDateTime,
                                 Code = a.Code,
                                 Amount = a.Amount,
                                 Status = a.Status,
                                
                             }).ToList();

                ViewBag.MasterpassStatusList = query;

                    var json = Json(new { data = query }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
            }
        }

        #region All Masterpass PAyments GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ViewMPPayments()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return View("_Error");
            }
        }
        #endregion

        #region Load All Masterpass Payments GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult LoadMPPayments(int pageSize, string startDate, string endDate, int pageNumber, string searchText)
        {
            var total = 0;
           
            var rows = new List<object>();
            var skip = (pageNumber - 1) * pageSize;
            const string goLiveDate = "2019/07/01";
            var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            var emailText = "";
            var mobileNoText = "";
            try
            {
                using (var context = new eServicesDbContext())
                {
                    //Gets system users with no customer profile o=>o.CreatedDateTime>golive
                    // var currentDebitOrders = cxt.DebitOrders.Include(o => o.Account).Include(o => o.BankAccount).Include(o => o.BankAccount.Bank).Include(o => o.Status).Include(o => o.Customer).Where(o=>o.CreatedDateTime>golive).ToList();
                    //var masterpassModel = context.MasterpassTransactions.Include(o => o.Account).Include(o => o.BankAccount).Include(o => o.BankAccount.Bank).Include(o => o.Status).Include(o => o.Customer)
                    //    .Where(s => (DbFunctions.TruncateTime(s.CreatedDateTime) >= startDateTime && DbFunctions.TruncateTime(s.CreatedDateTime) <= endDateTime));
                    var masterpassModel =  (from a in context.MasterpassTransactions
                     join b in context.MasterpassRequests on a.MasterpassRequestId equals b.Id
                     join c in context.SystemUsers on b.CreatedBySystemUserId equals c.Id

                     select new
                     {

                         Account = a.Reference,
                         Name = c.FirstName + " " + c.LastName,
                         System = c.Id,
                         CustomerID = c.IdentificationNumber,
                         Email = c.EmailAddress,
                         Number = c.MobileNumber,
                         CreatedDateTime = a.CreatedDateTime,
                         Code = a.Code,
                         Amount = a.Amount,
                         Status = a.Status,

                     }).Where(s => (DbFunctions.TruncateTime(s.CreatedDateTime) >= startDateTime && DbFunctions.TruncateTime(s.CreatedDateTime) <= endDateTime)).ToList();

                    var masterpass = (from a in context.MasterpassTransactions
                                      join b in context.MasterpassRequests on a.MasterpassRequestId equals b.Id
                                      join c in context.SystemUsers on b.CreatedBySystemUserId equals c.Id

                                      select new
                                      {

                                          Account = a.Reference,
                                          Name = c.FirstName + " " + c.LastName,
                                          System = c.Id,
                                          CustomerID = c.IdentificationNumber,
                                          Email = c.EmailAddress,
                                          Number = c.MobileNumber,
                                          CreatedDateTime = a.CreatedDateTime,
                                          Code = a.Code,
                                          Amount = a.Amount,
                                          Status = a.Status,

                                      }).Where(s => (DbFunctions.TruncateTime(s.CreatedDateTime) >= startDateTime && DbFunctions.TruncateTime(s.CreatedDateTime) <= endDateTime)).ToList();
                    //Filter
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        var isNumericAccount = double.TryParse(searchText, out double n);
                        var isString = double.TryParse(searchText, out double str);

                        //|| (searchText.ToUpper() == "SINGLE") || (searchText.ToUpper() == "RECURRING") || searchText.ToUpper() == "ONCE-OFF" || searchText.ToUpper() == "ONCEOFF" || searchText.ToUpper() == "ONCE OFF"
                        if (searchText.ToUpper() == "BREJ" )
                        {
                            searchText = "BREJ";
                            masterpass = masterpassModel.Where(s => (s.Status ?? "").Contains(searchText)).ToList();
                        }
                        else if(searchText.ToUpper() == "BANK REJECTED" || searchText.ToUpper() == "REJECTED" || searchText.ToUpper() == "BANK" || searchText.ToUpper() == "BANK_REJECTED")
                        {
                            searchText = "BANK_REJECTED";
                            masterpass = masterpassModel.Where(s => (s.Status ?? "").Contains(searchText)).ToList();
                        }
                        else if (searchText.ToUpper() == "SUCCESS")
                        {
                            searchText = "SUCCESS";
                            masterpass = masterpassModel.Where(s => (s.Status ?? "").Contains(searchText)).ToList();
                        }
                        else
                        {
                            masterpass = masterpassModel.Where(s => (s.Account ?? "").Contains(searchText) ||
                                                                (s.Name ?? "").Contains(searchText) ||
                                                                (s.CustomerID ?? "").Contains(searchText) ||
                                                                (s.Amount.ToString() ?? "").Contains(searchText) ||
                                                                (s.Code ?? "").Contains(searchText) ||
                                                                (s.Status ?? "").Contains(searchText) ||
                                                                (s.CreatedDateTime.ToString() ?? "").Contains(searchText) ||
                                                                (s.Email ?? "").Contains(searchText) ||
                                                                (s.Number ?? "").Contains(searchText)).ToList();
                        }
                       

                        masterpass = masterpass.OrderBy(l => l.Account).Skip(skip).Take(pageSize).ToList();
                        total = masterpassModel.Count();
                    }
                    else
                    {
                        masterpass = masterpass.OrderBy(l => l.Account).Skip(skip).Take(pageSize).ToList();
                        total = masterpassModel.Count();
                    }

                    foreach (var mpass in masterpass)
                    {
                        //var doPay = UserManager.FindByName(debitOrder.Customer.SystemUser.UserName);//Id = debitOrder.Id,

                        var custSystem = context.SystemUsers.FirstOrDefault(s => s.Id == mpass.System);
                        if (!string.IsNullOrEmpty(custSystem.EmailAddress))
                        {
                            emailText = custSystem.EmailAddress;
                        }
                        else
                        {
                            emailText = "N/A";
                        }

                        if (!string.IsNullOrEmpty(custSystem.MobileNumber))
                        {
                            mobileNoText = custSystem.MobileNumber;
                        }
                        else
                        {
                            mobileNoText = "N/A";
                        }



                        rows.Add(new
                        {

                            municipalAccount = mpass.Account,
                            customerName = mpass.Name,
                            customerId = mpass.CustomerID,
                            amount = mpass.Amount.ToString("0.00"),
                            reference = mpass.Code,
                            status = mpass.Status,
                            createdDate = String.Format("{0:yyyy/MM/dd}", mpass.CreatedDateTime),
                            emailAddress = emailText,
                            mobileNo = mobileNoText

                        });
                    }

                    return Json(new { total, rows }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
