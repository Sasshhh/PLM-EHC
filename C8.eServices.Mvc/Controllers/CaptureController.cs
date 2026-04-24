using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.ViewModels;
using C8.eServices.Mvc.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Globalization;
using C8.eServices.Mvc.ApiServices;
using System.Web.Routing;
using System.Net;
using System.Data.Entity.Core.Objects;
using Newtonsoft.Json.Linq;
using System.Web;
using Newtonsoft.Json;

namespace C8.eServices.Mvc.Controllers
{
    public class CaptureController : Controller
    {
      
        public string encp = "spgencpassp";
        private eServicesDbContext db = new eServicesDbContext();
        #region PaymentGateway
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string pgMerchantId, string voteNumber, string pgMerchantReference,
            string pgMerchantDescription, string Amount, string pgEmail, string pgMobile,
            string customerFirstName, string customerLastName, string returnUrl, string adhocRef1,
            string adhocRef2, string adhocRef3, string adhocRef4, string adhocRef5)
        {
            returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
            returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
            string amt = Amount.Replace('.', ',');
            decimal conAmt = Convert.ToDecimal(amt);
            returnUrl = returnUrl + "/ReturnBackUrl";
            //Format  parameters into Single Delimited String
            string enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}",
                pgMerchantId, voteNumber, pgMerchantReference, pgMerchantDescription, Amount,
                pgEmail, pgMobile, customerFirstName, customerLastName, returnUrl, adhocRef1, adhocRef2, adhocRef3, adhocRef4, adhocRef5);
            //Encrypt the Single String to and Encrypted string e
            var e = new AesCrypto(encp).Encrypt(enc);

            AppSetting PGDomain = db.AppSettings.FirstOrDefault(o => o.Key == PaymentGatewayKeys.PGDomain);


            var segment = string.Join("q?", e);
            var escapedSegment = Uri.EscapeDataString(segment);
            var baseFormat = PGDomain.Value+ "PaymentGateway/PaygateTest?q="+e;
            //+"https://www.google.co.za/maps/search/{0}/"
            var url = string.Format(baseFormat, escapedSegment);
            // {e} is your encrypted String
            //return RedirectToAction("PayGate", new { q = e });
            return Redirect(baseFormat);
        }

        [HttpGet]
        public ActionResult ReturnBackUrl(string q)
        {
            //Create an object of the Encryption Class
            AesCrypto aes = new AesCrypto(encp);
            //use enryption class obj to call the Decryption Method and pass the Encryption String Value (q)
            var decrypted = aes.Decrypt(q);
            // once Decrypted you will need to Split the Single string back into the individual Variables/parameters
            var values = decrypted.Split('|');
            var statuses = db.Status;
            var Appref = "";
            try
            {
                if(values[13] != null && values[15] != null)
                {
                    int RCSAppID = Convert.ToInt16(values[15]);
                    RCSApplicationStatus RCSApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).Where(x => x.Id == RCSAppID).FirstOrDefault();
                    Appref = RCSApplication.ApplicationReferenceNumber;
                    if (RCSApplication != null)
                    {

                        //AssessmentPaymentTransaction assessmentPaymentTransaction = new AssessmentPaymentTransaction();
                        //assessmentPaymentTransaction.ApplicationReference = RCSApplication.ApplicationReferenceNumber;
                        //assessmentPaymentTransaction.RCSApplicationStatusId = RCSApplication.Id;
                        //assessmentPaymentTransaction.Descritpion = "Rates Clearance Application Fee";
                        //assessmentPaymentTransaction.PaymentMethod = values[8];
                        //assessmentPaymentTransaction.ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;
                        ////save receipt num
                        //assessmentPaymentTransaction.Reference = values[4];
                        //assessmentPaymentTransaction.Status = values[14];
                        //assessmentPaymentTransaction.StatusId = db.Status.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                        //if (values[17] != null && values[17] != "")
                        //{
                        //    assessmentPaymentTransaction.AssessmentPaymentRequestId = Convert.ToInt16(values[17]);

                        //}
                        //assessmentPaymentTransaction.Amount = Convert.ToDecimal(values[7]);
                        //assessmentPaymentTransaction.MerchantReference = values[4];
                        ////assessmentPaymentTransaction.SmsNotify = values[13];
                        ////assessmentPaymentTransaction.EmailNotify = values[13];
                        //assessmentPaymentTransaction.VoteNumber = values[2];
                        //db.AssessmentPaymentTransactions.Add(assessmentPaymentTransaction);
                        //db.SaveChanges();
                        if (values[13] == "Success" || values[13] == "Test")
                        {
                            if (RCSApplication.StatusId == statuses.FirstOrDefault(o => o.Key == StatusKeys.AwaitingAssessmentPayment).Id)
                            {

                                CaptureController c = new CaptureController();
                                var SystUserId = c.RoundRobinCCC(true, false, false, false, false, false, false, false, RCSApplication.Id, 0, false, false, 0);

                                if (SystUserId != 0)
                                {
                                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                    RCSApplication.StatusId = db.Status.FirstOrDefault(o => o.Key == StatusKeys.PendingApplicationFeePaymentValidation).Id;

                                    RCSApplication.ApplicationFeeAmount = values[7];
                                    RCSApplication.ApplicationFeePaymentOnline = true;
                                    RCSApplication.ApplicationFeePayMethod = values[8];

                                    RCSApplication.ApplicationFeeReceiptNumber = values[4];
                                    RCSApplication.ApplicationFeePaymentDate = values[5];
                                    RCSApplication.ApplicationFeeStatus = values[14];


                                    db.Entry(RCSApplication).State = EntityState.Modified;
                                    db.SaveChanges();


                                    //var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ConevayncerUploadsApplicationFeeProofOfPayment).Description.ToString()/* + DecisionType*/;
                                    //var RCSHistoryLog = new RCSApplicationHistoryLog
                                    //{
                                    //    RCSApplicationStatusId = RCSApplication.Id,
                                    //    AuditAction = ActivityTrackerMessage,
                                    //    UserId = RCSApplication.CustomerId,
                                    //    CreatedDateTime = DateTime.Now,
                                    //    IsActive = true
                                    //};
                                    //db.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                                    //db.SaveChanges();

                                    var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationSentForAcknowledgementAppFee).Description.ToString()/* + DecisionType*/;
                                    var RCSHistoryLog2 = new RCSApplicationHistoryLog
                                    {
                                        RCSApplicationStatusId = RCSApplication.Id,
                                        AuditAction = ActivityTrackerMessage2,
                                        UserId = ClerkId.Id,
                                        CreatedDateTime = DateTime.Now,
                                        IsActive = true
                                    };
                                    db.RCSApplicationHistoryLogs.Add(RCSHistoryLog2);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var ResponsibilityType = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault();

                                    var UserMessage = db.RCSUserMessages.FirstOrDefault(x => x.Key == RCSUserMessageKeys.DepartmentNotSetup);
                                    UserMessage.Body = UserMessage.Body.Replace("{0}", ResponsibilityType.Name);
                                    UserMessage.Body = UserMessage.Body.Replace("{1}", RCSApplication.CCC.CCCName);
                                    var obj2 = new
                                    {
                                        status = "Department Failure",
                                        title = UserMessage.Title,
                                        body = UserMessage.Body
                                    };

                                    
                                    RCSApplication.StatusId = statuses.FirstOrDefault(o => o.Key == StatusKeys.ReuploadApplicationDocs).Id;

                                    db.Entry(RCSApplication).State = EntityState.Modified;
                                    db.SaveChanges();
                                    return RedirectToAction("Index", "RCSApplication");
                                }


                                RCSApplication.StatusId = statuses.FirstOrDefault(o => o.Key == StatusKeys.PendingApplicationFeePaymentValidation).Id;
                                
                                db.Entry(RCSApplication).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            Session["ApplicationRefNo"] = Appref;
                           
                            Session["ReceiptValues"] = values;
                           
                            return RedirectToAction("Index", "RCSApplication");
                        }
                        //   if(RCSApplication.StatusId == statuses.FirstOrDefault(o => o.Key == StatusKeys.PendingDocumentsApproval).Id)
                    }
                }
            }
            catch( Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View();
        }


        #endregion
        #region Init
        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        public CaptureController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }

        public CaptureController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }

        public CaptureController(eServicesDbContext context)
        {
        
            UserManager =
            new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(context));
        }

        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }


        private void Initialise()
        {
       
                try
                {
                    IdentityManager = new IdentityManager(db);

                    if (User != null && User.Identity.IsAuthenticated)
                    {
                        IdentityManager.CurrentUser(User);
                        SystemUser = IdentityManager.CurrentUser(User);
                    }

                    if (SystemUser != null)
                    {
                        Customer =
                            db.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                                .Include(o => o.CustomerType)
                                .Include(o => o.Country)
                                .Include(o => o.IdentificationType)
                                .Include(o => o.TitleType)
                                  .Include(o => o.Status)
                                .FirstOrDefault();

                        if (Customer != null)
                        {
                            Entity =
                                db.Entities.Where(o => o.CustomerId == Customer.Id)
                                    .Include(o => o.EntityType)
                                    .FirstOrDefault();
                            CustomerId = Customer.Id;
                        }

                    }

                    if (Customer != null)
                    {
                        Agent = db.Agents.FirstOrDefault(o => o.CustomerId == Customer.Id);
                    }
                }
                catch (Exception ex)
                {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
                }
            
        }

        #endregion

        #region DbConns
        //private eServicesDbContext db = new eServicesDbContext();
       

        private IntegrationEntities db2 = new IntegrationEntities();
        #endregion

        #region Json Methods


        #region Add Water Info
        public JsonResult AddWater(string WaterMeterInformation_WaterMeterNo, string WaterMeterInformation_WaterMeterReading, string WaterMeterInformation_WaterMeterReadingDateTaken)
        {
            var result = "";

            try
            {
                object Wilist = TempData["wp"];
                List<WaterMeterInformation> wl = new List<WaterMeterInformation>();

                if (Wilist == null)
                {
                    Wilist = wl;
                }

                else
                {
                    wl = (List<WaterMeterInformation>)Wilist;
                }

                WaterMeterInformation waterinfo = new WaterMeterInformation();
                waterinfo.WaterMeterNo = WaterMeterInformation_WaterMeterNo;
                waterinfo.WaterMeterReading = WaterMeterInformation_WaterMeterReading;
               
                waterinfo.WaterMeterReadingDateTaken = WaterMeterInformation_WaterMeterReadingDateTaken;



                if (waterinfo != null)
                {
                    wl.Add(waterinfo);
                    Wilist = wl;
                    TempData["wp"] = Wilist;
                    TempData.Keep("wp");
                    result = "Success";
                }
                else
                {
                    result = "Error";
                }

            }
            catch (Exception ex)
            {
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Add Water Info
        public JsonResult AddWaterApi(string WaterMeterInformation_WaterMeterNo, string WaterMeterInformation_WaterMeterReading, string WaterMeterInformation_WaterMeterReadingDateTaken)
        {
            var result = "";

            try
            {
                object Wilist = TempData["wp"];

             
          

                List<WaterMeterInformation> wl = new List<WaterMeterInformation>();

                if (Wilist == null)
                {
                    Wilist = wl;
                }

                else
                {
                    wl = (List<WaterMeterInformation>)Wilist;
                }

                WaterMeterInformation waterinfo = new WaterMeterInformation();
                waterinfo.WaterMeterNo = WaterMeterInformation_WaterMeterNo;
                waterinfo.WaterMeterReading = WaterMeterInformation_WaterMeterReading;
                if (WaterMeterInformation_WaterMeterReadingDateTaken.Length == 8)
                {
                    var Year = WaterMeterInformation_WaterMeterReadingDateTaken.Substring(0, 4);
                    var Month = WaterMeterInformation_WaterMeterReadingDateTaken.Substring(4, 2);
                    var Day = WaterMeterInformation_WaterMeterReadingDateTaken.Substring(6, 2);
                    var DateFinal = Year + '-' + Month + '-' + Day;
                    waterinfo.WaterMeterReadingDateTaken = DateFinal;
                }




                if (waterinfo != null)
                {
                    wl.Add(waterinfo);
                    Wilist = wl;
                    TempData["wp"] = Wilist;
                    TempData.Keep("wp");
                    result = "Success";
                }
                else
                {
                    result = "Error";
                }

            }
            catch (Exception ex)
            {
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Add Electricity Info
        public JsonResult AddElectricity(string ElectricityMeterInformation_ElectricityMeterNo, string ElectricityMeterInformation_ElectricityMeterReading, string ElectricityMeterInformation_ElectricityMeterReadingDateTaken)
        {
            var result = "";

            try
            {
                object Eilist = TempData["ep"];
                List<ElectricityMeterInformation> el = new List<ElectricityMeterInformation>();

                if (Eilist == null)
                {
                    Eilist = el;
                }

                else
                {
                    el = (List<ElectricityMeterInformation>)Eilist;
                }

                ElectricityMeterInformation electmeter = new ElectricityMeterInformation();
                electmeter.ElectricityMeterNo = ElectricityMeterInformation_ElectricityMeterNo;
                electmeter.ElectricityMeterReading = ElectricityMeterInformation_ElectricityMeterReading;
                electmeter.ElectricityMeterReadingDateTaken = Convert.ToDateTime(ElectricityMeterInformation_ElectricityMeterReadingDateTaken);
             


                if (electmeter != null)
                {
                    el.Add(electmeter);
                    Eilist = el;
                    TempData["ep"] = Eilist;
                    TempData.Keep("ep");
                    result = "Success";
                }
                else
                {
                    result = "Error";
                }

            }
            catch (Exception ex)
            {
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Add Electricity Info
        [AllowAnonymous]
        public JsonResult AddElectricity4(string ElectricityMeterInformation_ElectricityMeterNo, string ElectricityMeterInformation_ElectricityMeterReading, string ElectricityMeterInformation_ElectricityMeterReadingDateTaken)
        {
            var result = "";
            var currentStep = "init";

            try
            {
                ElectricityMeterInformation_ElectricityMeterNo = ElectricityMeterInformation_ElectricityMeterNo ?? string.Empty;
                ElectricityMeterInformation_ElectricityMeterReading = ElectricityMeterInformation_ElectricityMeterReading ?? string.Empty;
                ElectricityMeterInformation_ElectricityMeterReadingDateTaken = ElectricityMeterInformation_ElectricityMeterReadingDateTaken ?? string.Empty;

                WriteLog("AddElectricity4 START - username=" + ElectricityMeterInformation_ElectricityMeterNo
                    + " | mobile=" + ElectricityMeterInformation_ElectricityMeterReading
                    + " | email=" + ElectricityMeterInformation_ElectricityMeterReadingDateTaken);

                RegistrationViewModel vm = new RegistrationViewModel();
                List<RoundRobinLog> errorList = new List<RoundRobinLog>();

                var usernameAssigned = false;
                var emailAssigned = false;
                var mobileAssigned = false;

                currentStep = "username DB check";
                usernameAssigned = db.SystemUsers.Any(u => u.UserName.ToLower() == ElectricityMeterInformation_ElectricityMeterNo.ToLower() && u.IsActive && u.IsDeleted == false);
                WriteLog("AddElectricity4 [username DB check] done: usernameAssigned=" + usernameAssigned);

                currentStep = "email DB check";
                if (!string.IsNullOrEmpty(ElectricityMeterInformation_ElectricityMeterReadingDateTaken))
                    emailAssigned = db.SystemUsers.Any(u => u.EmailAddress != null && u.EmailAddress.ToLower() == ElectricityMeterInformation_ElectricityMeterReadingDateTaken.ToLower() && u.IsActive && u.IsDeleted == false);
                WriteLog("AddElectricity4 [email DB check] done: emailAssigned=" + emailAssigned);

                currentStep = "mobile DB check";
                mobileAssigned = db.SystemUsers.Any(u => u.MobileNumber == ElectricityMeterInformation_ElectricityMeterReading && u.IsActive && !u.IsDeleted);
                WriteLog("AddElectricity4 [mobile DB check] done: mobileAssigned=" + mobileAssigned);

                currentStep = "building errorList";
                var response = "";
                string Username = ElectricityMeterInformation_ElectricityMeterNo;
                var count = ElectricityMeterInformation_ElectricityMeterNo.Length;

                if (usernameAssigned)
                {
                    RoundRobinLog error = new RoundRobinLog();
                    error.LogEntry = "The username entered is already taken.";
                    errorList.Add(error);
                }
                if (count < 6 || count > 20)
                {
                    RoundRobinLog error0 = new RoundRobinLog();
                    error0.LogEntry = "The Username field must be 6 to 20 characters long.";
                    errorList.Add(error0);
                }

                if (emailAssigned)
                {
                    RoundRobinLog error2 = new RoundRobinLog();
                    error2.LogEntry = "The email address entered is already in use.";
                    errorList.Add(error2);
                }
                if (mobileAssigned)
                {
                    RoundRobinLog error3 = new RoundRobinLog();
                    error3.LogEntry = "The mobile number entered is already in use.";
                    errorList.Add(error3);
                }

                if (usernameAssigned == false && emailAssigned == false && mobileAssigned == false)
                {
                    RoundRobinLog error4 = new RoundRobinLog();
                    error4.LogEntry = "Success";
                    errorList.Add(error4);
                }

                WriteLog("AddElectricity4 COMPLETE - errorList.Count=" + errorList.Count
                    + " | first=" + (errorList.Count > 0 ? errorList[0].LogEntry : "none"));
                return Json(errorList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var logMsg = "AddElectricity4 EXCEPTION at step [" + currentStep + "]: " + ex.ToString();
                EventLogHelper.LogSystemError(ex.ToString(), LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                try
                {
                    using (var logDb = new eServicesDbContext())
                    {
                        logDb.Logs.Add(new Log
                        {
                            LogTypeId = 1,
                            LogEntry = logMsg,
                            ReferenceId = 0,
                            ReferenceTypeId = 1,
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false
                        });
                        logDb.SaveChanges();
                    }
                }
                catch { }
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region CLear Electricity Water
        public JsonResult ClearMeterTable()
        {
            var result = "";

            try
            {
                object Eilist = TempData["ep"];
                List<ElectricityMeterInformation> el = new List<ElectricityMeterInformation>();
                TempData["ep"] = el;
                TempData.Keep("ep");
                //TempData.Remove("ep");

              

              

                object Wilist = TempData["wp"];
                List<WaterMeterInformation> wl2 = new List<WaterMeterInformation>();
                TempData["wp"] = wl2;
                TempData.Keep("wp");
                //TempData.Remove("wp");
                result = "Success";
          

            }
            catch (Exception ex)
            {
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Add Electricity Info Api
        public JsonResult AddElectricityApi(string ElectricityMeterInformation_ElectricityMeterNo, string ElectricityMeterInformation_ElectricityMeterReading, string ElectricityMeterInformation_ElectricityMeterReadingDateTaken)
        {
            var result = "";

            try
            {
                object Eilist = TempData["ep"];
                List<ElectricityMeterInformation> el = new List<ElectricityMeterInformation>();

                if (Eilist == null)
                {
                    Eilist = el;
                }

                else
                {
                    el = (List<ElectricityMeterInformation>)Eilist;
                }

                ElectricityMeterInformation electmeter = new ElectricityMeterInformation();
                electmeter.ElectricityMeterNo = ElectricityMeterInformation_ElectricityMeterNo;
                electmeter.ElectricityMeterReading = ElectricityMeterInformation_ElectricityMeterReading;

                if (ElectricityMeterInformation_ElectricityMeterReadingDateTaken.Length == 8)
                {
                    var Year = ElectricityMeterInformation_ElectricityMeterReadingDateTaken.Substring(0, 4);
                    var Month = ElectricityMeterInformation_ElectricityMeterReadingDateTaken.Substring(4, 2);
                    var Day = ElectricityMeterInformation_ElectricityMeterReadingDateTaken.Substring(6, 2);
                    var DateFinal = Year + '-' + Month + '-' + Day;
                    electmeter.ElectricityMeterReadingDateTaken = Convert.ToDateTime(DateFinal);
                }

                //electmeter.ElectricityMeterReadingDateTaken = Convert.ToDateTime(ElectricityMeterInformation_ElectricityMeterReadingDateTaken);



                if (electmeter != null)
                {
                    el.Add(electmeter);
                    Eilist = el;
                    TempData["ep"] = Eilist;
                    TempData.Keep("ep");
                    result = "Success";
                }
                else
                {
                    result = "Error";
                }

            }
            catch (Exception ex)
            {
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Add Natural Person
        public JsonResult AddNaturalPerson(string Gender, string TitleType, string MaritialStatus, string PurchaserInformation_Initial, string PurchaserInformation_FirstName, string PurchaserInformation_LastName, string PurchaserInformation_IDNo, string PurchaserInformation_DOB, int PurchaserInformation_Age, string PurchaserInformation_Passport, string PurchaserInformation_PurDateExpiry, string PurchaserInformation_Nationality, string PurchaserInformation_CellNo, string PurchaserInformation_HomeNo, string PurchaserInformation_WorkNo, string PurchaserInformation_PurEmail,string PurchaserInformation_PostAddress,string PurchaserInformation_PostSuburb, string PurchaserInformation_PostalCity, string PurchaserInformation_PostPostalCode,string PurchaserInformation_PurchaseType, string PurchaserInformation_NominatedAddress)
         {
            var result = "";

            try
            {
                object Pilist = TempData["np"];
                List<PurchaserInformation> pl  = new List<PurchaserInformation>();

                if (Pilist == null)
                {
                    Pilist = pl;
                }

                else
                {
                    pl = (List<PurchaserInformation>)Pilist;
                }

                PurchaserInformation naturalPerson = new PurchaserInformation();
                naturalPerson.Gender = Gender;
                naturalPerson.Title = TitleType;
                naturalPerson.MaritalStatus = MaritialStatus;
                naturalPerson.Initial = PurchaserInformation_Initial;
                naturalPerson.FirstName = PurchaserInformation_FirstName;
                naturalPerson.LastName = PurchaserInformation_LastName;
                naturalPerson.IDNo = PurchaserInformation_IDNo;
                if (PurchaserInformation_DOB != "")
                {
                    naturalPerson.DOB = Convert.ToDateTime(PurchaserInformation_DOB);
                }

                if (PurchaserInformation_PurDateExpiry != "")
                {
                    naturalPerson.PurDateExpiry = Convert.ToDateTime(PurchaserInformation_PurDateExpiry);
                }

                naturalPerson.Age = PurchaserInformation_Age;
                naturalPerson.Passport = PurchaserInformation_Passport;
                naturalPerson.Nationality = PurchaserInformation_Nationality;
                naturalPerson.CellNo = PurchaserInformation_CellNo;
                naturalPerson.HomeNo = PurchaserInformation_HomeNo;
                naturalPerson.WorkNo = PurchaserInformation_WorkNo;
                naturalPerson.PurEmail = PurchaserInformation_PurEmail;
                naturalPerson.PostAddress = PurchaserInformation_PostAddress;
                naturalPerson.PostSuburb = PurchaserInformation_PostSuburb;
                naturalPerson.PostalCity = PurchaserInformation_PostalCity;
                naturalPerson.PostPostalCode = PurchaserInformation_PostPostalCode;

                var getpurchasertypeid= db.PurchaserType.Where(x => x.Key == PurchaserInformation_PurchaseType).FirstOrDefault().Id;
                naturalPerson.PurchaserTypeId = getpurchasertypeid;
                naturalPerson.PurchaseType = getpurchasertypeid;
                naturalPerson.NominatedAddress = PurchaserInformation_NominatedAddress;
                naturalPerson.PurchaserTypeKey = PurchaserInformation_PurchaseType;


                if (naturalPerson != null)
                {
                    pl.Add(naturalPerson);
                    Pilist = pl;
                    TempData["np"] = Pilist;
                    TempData.Keep("np");
                    result = "Success";
                }
                else
                {
                    result = "Error";
                }

            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

            #region View VA Details
            public JsonResult VAInformation(string MunicipalAccNo)
            {
                object result = null;

                try
                {
                    if (MunicipalAccNo.Trim() == "")
                    {
                        result = null;
                    }
                    else
                    {
                        var MuniAccNo = Convert.ToInt64(MunicipalAccNo.Trim());
                    PropertyOwnerDetailsApi p = new PropertyOwnerDetailsApi();
                    var details = p.GetOwnerInfo(MuniAccNo.ToString());


                    //SolarAccountBalanceApi sab = new SolarAccountBalanceApi();
                    //var sabs = sab.GetAccountBalance(MuniAccNo.ToString());

                    //SolarCalculateAssessmentFigureApi sca = new SolarCalculateAssessmentFigureApi();
                    //var scas = sca.calculateAssessmentFigures(MuniAccNo.ToString());

                    dynamic data = JObject.Parse(details);
                    data = data.SolarERP.payload;
                    LimsPayload payload = new LimsPayload();
                    payload.streetaddress = data.streetaddress;




                    CustomerAccount custacc = new CustomerAccount();
                    string getidno =Convert.ToString(data.IDNumber);
                    //custacc.AccountNo= Convert.ToString(data.AccountNo);
                    custacc.Initials = Convert.ToString(data.FirstName);
                    custacc.Surname = Convert.ToString(data.LastName);
                    if (getidno!=null && getidno.Length==13)
                    {
                        custacc.IDNo = Convert.ToInt64(getidno);
                    }
                    custacc.WorkTel = Convert.ToString(data.WorkTelNo);
                    custacc.HomeTel = Convert.ToString(data.HomeTelNo);
                    custacc.EmailAddr = Convert.ToString(data.EmailAddr);
                    custacc.CellNo = Convert.ToString(data.CellPhoneNo);
                    custacc.PostalCode = Convert.ToString(data.PostalCode);
                    custacc.PostalAddress2 = Convert.ToString(data.PhysicalAddress2);
                    custacc.StreetAddr = Convert.ToString(data.PhysicalAddress1);
                    custacc.PostalAddress3 = Convert.ToString(data.PhysicalAddress3);



                    result = custacc;

                    //var propertyinfo = db.CustomerAccounts.Where(x => x.AccountNo == MuniAccNo).FirstOrDefault();
                    //result = propertyinfo;

                }
                }
                catch (Exception ex)
                {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                var e = ex.Message;
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        #endregion


        #region get Lims Property Details
        public JsonResult LimsPropertyDetails (string MunicipalAccNo)
        {
            object result = null;

            try
            {
                if (MunicipalAccNo.Trim() == "")
                {
                    result = null;
                }
                else
                {
                   

                    var MuniAccNo = Convert.ToInt64(MunicipalAccNo.Trim());
                    LimsApi p = new LimsApi();
                    var details = p.GetPropertyDetails(MuniAccNo.ToString());

                    LimsPayload payload = new LimsPayload();

                    string CCCPrefix = MunicipalAccNo.Substring(0, 2);
                    var CCC = db.CCCs.Where(x => x.Prefix == CCCPrefix).FirstOrDefault();

                    if(CCC != null)
                    {
                        if(CCC.AreaManagerId == null)
                        {
                            var CCCNotSetup = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.CCCNotSetup).FirstOrDefault();
                           
                            CCCNotSetup.Body = CCCNotSetup.Body.Replace("{1}", CCC.CCCName);
                            payload.Status = CCCNotSetup.Body;
                            payload.Title = CCCNotSetup.Title;
                            payload.StatusKey = CCCNotSetup.Key;
                        }
                        else
                        {
                             dynamic data = JObject.Parse(details);

                  
                                    var ttt = data.SolarERP.Header.Version;

                                    payload.Status = data.SolarERP.Header.Result.Status;

                                    if(payload.Status == "Success")
                                    {
                                        var rcsType = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.TypeofProperty).OrderBy(x => x.Name);
                     
                                       foreach (var item in data.SolarERP.Payload)
                                        {
                                            payload.streetaddress = item.streetaddress;
                                            payload.registeredownerids = item.registeredownerids;
                                            payload.registeredownernames = item.registeredownernames;
                                            payload.categoryname = item.categoryname;

                                    if(payload.categoryname != null)
                                    {
                                        if (payload.categoryname.ToLower().Contains("residential"))
                                        {

                                            payload.categoryname = Convert.ToString(db.RCSTypes.Where(x => x.Description == RCSTypeKeys.residential_property).FirstOrDefault().Id);
                                        }
                                        else if (payload.categoryname.ToLower().Contains("commercial"))
                                        {
                                            payload.categoryname = Convert.ToString(db.RCSTypes.Where(x => x.Description == RCSTypeKeys.commercial_property).FirstOrDefault().Id);

                                        }
                                        else if (payload.categoryname.ToLower().Contains("vacant"))
                                        {
                                            payload.categoryname = Convert.ToString(db.RCSTypes.Where(x => x.Description == RCSTypeKeys.vacant_property).FirstOrDefault().Id);

                                        }
                                    }
                                           



                                            payload.erfnumber = item.erfnumber;
                                    if(item.deedextents!= null )
                                    {
                                        payload.deedextents = item.deedextents;
                                    }
                                    else
                                    {
                                        payload.deedextents = 0;
                                    }
                                          
                                            payload.ssourceaccountnumber = item.ssourceaccountnumber;
                                    payload.regionname = item.regionname;

                                            //payload.registeredownernames = item.registeredownernames;

                                            //payload.registeredownernames = item.registeredownernames;




                                        }

                                    }

                        }
                    }
                    else
                    {
                        var NoCCCExists = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.NoCCCExistsForMunicipalAccNum).FirstOrDefault();
                        payload.Status = NoCCCExists.Body;
                        payload.Title = NoCCCExists.Title;
                        payload.StatusKey = NoCCCExists.Key;
                    }

                    //SolarAccountBalanceApi sab = new SolarAccountBalanceApi();
                    //var sabs = sab.GetAccountBalance(MuniAccNo.ToString());

                    //SolarCalculateAssessmentFigureApi sca = new SolarCalculateAssessmentFigureApi();
                    //var scas = sca.calculateAssessmentFigures(MuniAccNo.ToString());

                   
                    //var streetad = data.SolarERP.Payload.streetaddress;

                    //var testwho = data.SolarERP.Payload;
                    //dynamic data2 = JObject.Parse(data.SolarERP.Payload);



                    return Json(payload, JsonRequestBehavior.AllowGet);

                    //if (details != string.Empty)
                    //{
                    //    dynamic data = JObject.Parse(details);

                    //    payload.CaudAccountNo = data.CaudAccountNo;
                    //    foreach (var item in data.StatusMessages)
                    //    {



                    //        payload.Status = item;
                    //        if (payload.Status.Contains("No transactions found for account-no"))
                    //        {
                    //            payload.Status = "No transactions found for account-no";
                    //        }

                    //    }
                    //    if (data.CaudAccountNo == null || data.CaudAccountNo == "")
                    //    {

                    //    }
                    //    else
                    //    {
                    //        foreach (var item in data.PaymentDetailsList)
                    //        {
                    //            PaymentDetailsListPayload = new PaymentDetailsList();
                    //            PaymentDetailsListPayload.Amt = item.Amt;
                    //            PaymentDetailsListPayload.Date = item.Date;
                    //            var date2 = Convert.ToString(item.Date);
                    //            var date = Convert.ToString(item.Date);
                    //            var len = date2.Length;
                    //            if (date.Length == 8)
                    //            {
                    //                var Year = date.Substring(0, 4);
                    //                var Month = date.Substring(4, 2);
                    //                var Day = date.Substring(6, 2);
                    //                var DateFinal = Year + '-' + Month + '-' + Day;
                    //                PaymentDetailsListPayload.ConvertedDate = Convert.ToDateTime(DateFinal);
                    //            }
                    //            PaymentDetailsListPayload.Ref = item.Ref;

                    //            PaymentDetailsListPayload.CustomerFirstName = item.CustomerFirstName;
                    //            PaymentDetailsListPayload.CustomerLastName = item.CustomerLastName;
                    //            PaymentDetailsListArray.Add(PaymentDetailsListPayload);

                    //        }

                    //        payload.PaymentDetailsList = PaymentDetailsListArray;

                    //    }



                    //}




                    //CustomerAccount custacc = new CustomerAccount();
                    //string getidno = Convert.ToString(data.IDNumber);
                    ////custacc.AccountNo= Convert.ToString(data.AccountNo);
                    //custacc.Initials = Convert.ToString(data.FirstName);
                    //custacc.Surname = Convert.ToString(data.LastName);
                    //if (getidno != null && getidno.Length == 13)
                    //{
                    //    custacc.IDNo = Convert.ToInt64(getidno);
                    //}
                    //custacc.WorkTel = Convert.ToString(data.WorkTelNo);
                    //custacc.HomeTel = Convert.ToString(data.HomeTelNo);
                    //custacc.EmailAddr = Convert.ToString(data.EmailAddr);
                    //custacc.CellNo = Convert.ToString(data.CellPhoneNo);
                    //custacc.PostalCode = Convert.ToString(data.PostalCode);
                    //custacc.PostalAddress2 = Convert.ToString(data.PhysicalAddress2);
                    //custacc.StreetAddr = Convert.ToString(data.PhysicalAddress1);
                    //custacc.PostalAddress3 = Convert.ToString(data.PhysicalAddress3);



                    //result = custacc;

                    //var propertyinfo = db.CustomerAccounts.Where(x => x.AccountNo == MuniAccNo).FirstOrDefault();
                    //result = propertyinfo;

                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                var e = ex.Message;
                result = "Error";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Municipal Meter Details
        public JsonResult MeterDetails(string MunicipalAccNo)
        {
            object result = null;
            ClearMeterTable();
            try
            {
                if (MunicipalAccNo.Trim() == "")
                {
                    result = null;
                }
                else
                {
                    MunicipalServiceDetailsApi API = new MunicipalServiceDetailsApi();
                    result = API.GetMunicipalServiceDetails(MunicipalAccNo);

                    //if(result.AttorneyCode == null || result.AttorneyCode == "")
                    //{
                    //    result.Status = "Attorney Code Not Found";
                    //}
                    var data = Json(result, JsonRequestBehavior.AllowGet);
                    
                    
                
                    return data;

                    //var propertyinfo = db.CustomerAccounts.Where(x => x.AccountNo == MuniAccNo).FirstOrDefault();
                    //result = propertyinfo;

                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                var e = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Search by Attorney Code 

        public JsonResult AttorneyCodeInformation(string AttorneyCode)
        {
            AttorneyDetails result = new AttorneyDetails();
            string Err;
            try
            {

                AttorneyDetailsApi API = new AttorneyDetailsApi();
                result = API.AttorneyDetails(AttorneyCode);

                dynamic data = JObject.Parse(result.Json);
               
                var tester = Json(result, JsonRequestBehavior.AllowGet);
               
                return tester;
               
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                Err = "Error";
            }
            return Json(Err, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult AttorneyCodeInformation(string AttorneyCode)
        //{
        //    object result = null;

        //    try
        //    {
        //        if (AttorneyCode.Trim() == "")
        //        {
        //            result = null;
        //        }
        //        else
        //        {
        //            var AttCode = AttorneyCode.Trim();
        //            var AttorneyInfo = db2.TBL_RCS_ATTORNEYS.Where(x => x.Attorney_Code == AttCode).FirstOrDefault();

        //            AttorneyDetailsApi API = new AttorneyDetailsApi();
        //            result = API.AttorneyDetails(AttorneyCode);

        //            dynamic data = JObject.Parse(result.Json);

        //            if (AttorneyInfo == null)
        //            {
        //                result = "Error";

        //            }
        //            else
        //            {
        //                result = AttorneyInfo;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var e = ex.Message;
        //    }
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        #endregion
        #region Get Attorney Details
        public JsonResult GetAttorneyDetails(string id)
        {
            object result = null;

            try
            {
            
                    result = "Unique";
                    //var AttorneyDetails = RCSAttorneyApi.GetAttorneyDetails(id.Trim()).ToList();
                    //result = AttorneyDetails;
          
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region Capture OnLoad
        public ActionResult Capture()
            {
            Initialise();
            if (Customer != null)
            {
                if(Customer.Status.Key != StatusKeys.CustomerActive)
                {
                   
                    object obj = new { customerId = Customer.Id, agentId = Agent.Id };
                    return RedirectToAction("Index3", "Profile", SecureActionLinkExtension.Encrypt(obj));
                }
            }
            else
            {
                int agentId = 0;
                object obj = new { customerId = 0, agentId = agentId };
                return RedirectToAction("Index3", "Profile", SecureActionLinkExtension.Encrypt(obj));
            }
               


            var rcsType = db.RCSTypes.OrderBy(x=>x.Name);
            TempData.Remove("np");
            ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true ), "Key", "Name");
                ViewBag.TitleType = new SelectList(db.TitleTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Id", "Name");
                ViewBag.TypeOfTransfer = new SelectList(db.TransferTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                ViewBag.JuristicType = new SelectList(db.EntityTypes.Where(x => (bool)x.IsActive != false), "id", "Name");
                ViewBag.IdentificationType = new SelectList(db.IdentificationTypes.Where(x => (bool)x.IsDeleted != true), "id", "Name");
                ViewBag.MaritialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_MaritialStatus), "Name", "Name");
                ViewBag.TypeofProperty = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.TypeofProperty), "Id", "Name");
                ViewBag.ResidentialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_ResidentialStatus), "Name", "Name");
                ViewBag.Gender = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_Gender), "Name", "Name");
            var instructions= db.InstructionContents.Where(x => x.Key == InstructionContentKey.CaptureInstructions).FirstOrDefault();
            ViewBag.CaptureInstructions = instructions.Description;
                return View();
            }

        #endregion

        #region Capture
        [HttpPost]
            public ActionResult   Capture(CaptureViewModel capture , string TransferInformationSellingPrice)
            {

            var Appref = "";
            var RCSAppID = 0;

            if (TransferInformationSellingPrice != null & TransferInformationSellingPrice != "")
            {
                //var test = (TransferInformationSellingPrice).Replace(',', '.');
                //capture.TransferInformation.SellingPrice = Convert.ToDecimal(test, CultureInfo.InvariantCulture);


                var test = (TransferInformationSellingPrice).Replace(',', '.');
                var ttt = Convert.ToDecimal(TransferInformationSellingPrice, CultureInfo.InvariantCulture);

                capture.TransferInformation.SellingPrice = ttt;

            }

            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);
                    var CodeForRCSSave = "";
                    if (CodeForRCSSave != "Ready")
                    {
                        var AppSettings = cxt.AppSettings;
                        var Customers = cxt.Customers;
                        var Statuses = cxt.Status;

                        TransferInformation ti = new TransferInformation();
                        ti = capture.TransferInformation;
                        var getTransfertypeid = db.TransferTypes.Where(x => x.Key == ti.TransferTypeName).FirstOrDefault().Id;
                        ti.TransferTypeName = ti.TransferTypeName;
                        ti.TransferType = getTransfertypeid;
                        cxt.TransferInformations.Add(ti);
                        cxt.SaveChanges();
                        var TransferInfoID = ti.Id;

                        MunicipalAccountInformation MI = new MunicipalAccountInformation();
                        MI = capture.MunicipalAccountInformation;
                        cxt.MunicipalAccountInformations.Add(MI);
                        cxt.SaveChanges();

                        string CCCPrefix = ti.RatesNumber.Substring(0, 2);
                        var CCC = cxt.CCCs.Where(x => x.Prefix == CCCPrefix).FirstOrDefault();

                        RCSApplicationStatus appli = new RCSApplicationStatus();
                        appli.Customer = Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
                        appli.StatusId = Statuses.FirstOrDefault(o => o.Key == StatusKeys.ReuploadApplicationDocs).Id;
                        appli.CCCId = CCC.Id;
                        appli.TransferInformationId = TransferInfoID;
                        appli.TransferInformation = ti;

                        int limiter = 0;

                        AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSSequence);
                        var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSSequenceLimit);
                        var BatchCounter = query.Value;
                        limiter = Convert.ToInt16(SeqLimit.Value);
                        if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                        {
                            var lastRef = query.Value;
                            BatchCounter = lastRef.ToString();
                            int nextSeq = Convert.ToInt16(query.Value) + 1;
                            string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                            query.Value = nextVal;
                            cxt.Entry(query).State = EntityState.Modified;
                            cxt.SaveChanges();
                        }
                        else
                        {
                            int nextSeq = 1;
                            string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                            BatchCounter = nextVal;
                            nextSeq = 2;
                            nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                            query.Value = nextVal;
                            query.ModifiedDateTime = DateTime.Now.Date;
                            cxt.Entry(query).State = EntityState.Modified;
                            cxt.SaveChanges();

                        }
                        
                        if (ti.RatesNumber != null)
                        {
                            var refs = ti.RatesNumber;
                            var RefNum = refs + "RCC" + DateTime.Now.ToString("ddMMyyyy") + BatchCounter;
                            appli.ApplicationReferenceNumber = RefNum;
                            Appref = appli.ApplicationReferenceNumber;
                        }

                        appli.MunicipalAccountInformationId = MI.Id;
                        appli.MunicipalAccountInformation = MI;

                        appli.TransferInformationId = TransferInfoID;
                        appli.TransferInformation = ti;

                        //var SystUserId = RoundRobin(false, false, false, false, false, true);
                        //var cust = Customers.Where(x => x.SystemUserId == SystUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //appli.ClerkId = cust;

                        cxt.RCSApplicationStatus.Add(appli);
                        cxt.SaveChanges();

                        RCSAppID = appli.Id;

                        SellerInformation sellerinfo = new SellerInformation();

                        sellerinfo = capture.SellerInformation; 
                        sellerinfo.RCSApplicationStatusId = appli.Id;
                        sellerinfo.RCSApplicationStatus = appli;
                      
                        cxt.SellerInformations.Add(sellerinfo);
                        cxt.SaveChanges();


                        var PI = (List<PurchaserInformation>)TempData["np"];
                        if (PI != null)
                        {
                            foreach (var item in PI)
                            {
                                PurchaserInformation purchInfo = new PurchaserInformation();

                                purchInfo = item;
                                purchInfo.RCSApplicationStatusId = appli.Id;
                                purchInfo.RCSApplicationStatus = appli;
                                cxt.PurchaserInformations.Add(purchInfo);
                                cxt.SaveChanges();
                            }
                        }

                        if (PI == null)
                        {
                            PurchaserInformation pi = new PurchaserInformation();
                            pi = capture.PurchaserInformation;
                            
                            pi.RCSApplicationStatusId = appli.Id;
                            var getpurchasertypeid = db.PurchaserType.Where(x => x.Key == pi.PurchaserTypeKey).FirstOrDefault().Id;
                            pi.PurchaserTypeId = getpurchasertypeid;
                            pi.PurchaseType = getpurchasertypeid;
                            cxt.PurchaserInformations.Add(pi);
                            cxt.SaveChanges();
                        }
                        //var EI = (List<ElectricityMeterInformation>)TempData["ep"];
                        //if (EI != null)
                        //{
                        //    foreach (var item in EI)
                        //    {
                        //        ElectricityMeterInformation elecinfo = new ElectricityMeterInformation();

                        //        elecinfo = item;
                        //        elecinfo.RCSApplicationStatusId = appli.Id;
                        //        elecinfo.RCSApplicationStatus = appli;
                        //        cxt.ElectricityMeterInformations.Add(elecinfo);
                        //        cxt.SaveChanges();
                        //    }
                        //}
                        //string testll = "ssss";
                        //var test = JsonConvert.DeserializeObject<List<ElectricityMeterInformation>>(testll);
                      if(capture.ElectricitySessionList != null && capture.ElectricitySessionList != "")
                        {
                            List<ElectricityMeterInformation> ElectricityMeterInfo = JsonConvert.DeserializeObject<List<ElectricityMeterInformation>>(capture.ElectricitySessionList);
                            if (ElectricityMeterInfo.Count > 0)
                            {
                                foreach (var item in ElectricityMeterInfo)
                                {
                                    ElectricityMeterInformation elecinfo = new ElectricityMeterInformation();

                                    if (item.DateString.Length == 8)
                                    {
                                        CultureInfo provider = CultureInfo.InvariantCulture;
                                        string format = "yyyyMMdd";
                                        DateTime result = DateTime.ParseExact(item.DateString, format, provider);
                                        item.ElectricityMeterReadingDateTaken = result;
                                    }
                                    else
                                    {
                                        item.ElectricityMeterReadingDateTaken = Convert.ToDateTime(item.DateString);
                                    }
                                    elecinfo = item;
                                    elecinfo.RCSApplicationStatusId = appli.Id;
                                    elecinfo.RCSApplicationStatus = appli;
                                    cxt.ElectricityMeterInformations.Add(elecinfo);
                                    cxt.SaveChanges();
                                }
                            }
                        }

                      if(capture.WaterSessionList != null && capture.WaterSessionList != "")
                        {
                            List<WaterMeterInformation> WaterMeterInfo = JsonConvert.DeserializeObject<List<WaterMeterInformation>>(capture.WaterSessionList);

                            if (WaterMeterInfo.Count > 0)
                            {
                                foreach (var item in WaterMeterInfo)
                                {
                                    WaterMeterInformation waterinfo = new WaterMeterInformation();
                                    CultureInfo provider = CultureInfo.InvariantCulture;

                                    //if (item.DateString.Length == 8)
                                    //{
                                    //    string format = "yyyyMMdd";
                                    //    DateTime result = DateTime.ParseExact(item.DateString, format, provider);
                                    //    item.WaterMeterReadingDateTaken = result;
                                    //}
                                    //else
                                    //{
                                    //    item.WaterMeterReadingDateTaken = Convert.ToDateTime(item.DateString);
                                    //}

                                    waterinfo = item;
                                    waterinfo.RCSApplicationStatusId = appli.Id;
                                    waterinfo.RCSApplicationStatus = appli;
                                    cxt.WaterMeterInformations.Add(waterinfo);
                                    cxt.SaveChanges();
                                }
                            }
                        }
                     
                     

                        //var WI = (List<WaterMeterInformation>)TempData["wp"];
                        //if (WI != null)
                        //{
                        //    foreach (var item in WI)
                        //    {
                        //        WaterMeterInformation waterinfo = new WaterMeterInformation();

                        //        waterinfo = item;
                        //        waterinfo.RCSApplicationStatusId = appli.Id;
                        //        waterinfo.RCSApplicationStatus = appli;
                        //        cxt.WaterMeterInformations.Add(waterinfo);
                        //        cxt.SaveChanges();
                        //    }
                        //}

                        var ActivityTrackerMessage = cxt.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ConevayncerCapturesNewApplication).Description.ToString()/* + DecisionType*/;
                        var RCSHistoryLog = new RCSApplicationHistoryLog
                        {
                            RCSApplicationStatusId = appli.Id,
                            AuditAction = ActivityTrackerMessage,
                            UserId = Customer.Id,
                            CreatedDateTime = DateTime.Now,
                            IsActive = true
                        };
                        cxt.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                        cxt.SaveChanges();
                        Email SendMail = new Email();
                        var getemailbody= db.EmailContentTypes.Where(x => x.Key ==EmailContentKeys.ApplicationCapturedSuccessfully).FirstOrDefault();
                        string attorneyemail= appli.Customer.EmailAddress;
                        string attorneyname = appli.Customer.FirstName + " " + appli.Customer.LastName;
                        //string emailbody = "Your RCS Application has been captured successfully your Reference Number is : "+ appli.ApplicationReferenceNumber;
                        string emailbody = getemailbody.Description + appli.ApplicationReferenceNumber;
                        SendMail.GenerateEmail(attorneyemail, "RCS - New Online Application Submission",
                                      emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                        var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EmailNewApplication).Description.ToString() + emailbody;
                        var RCSHistoryLog2 = new RCSApplicationHistoryLog
                        {
                            RCSApplicationStatusId = appli.Id,
                            AuditAction = ActivityTrackerMessage2,
                            UserId = Customer.Id,
                            CreatedDateTime = DateTime.Now,
                            IsActive = true
                        };
                        cxt.RCSApplicationHistoryLogs.Add(RCSHistoryLog2);
                        cxt.SaveChanges();
                        //List<RCSDepartmentType> DepartmentList = new List<RCSDepartmentType>();
                        //DepartmentList = db.RCSDepartmentTypes.ToList();
                        //CaptureController c = new CaptureController();

                        ////Insert all departments
                        //foreach (var department in DepartmentList)
                        //{
                        //    DepartmentsApproval depApprovals = new DepartmentsApproval();
                        //    List<DepartmentsApproval> depList = new List<DepartmentsApproval>();
                        //    var ApprovrcsType = Statuses.Where(x => x.Key == StatusKeys.PendingDocumentsApproval).FirstOrDefault();
                        //    depApprovals.StatusId = ApprovrcsType.Id;
                        //    depApprovals.DepartmentId = department.Id;
                        //    depApprovals.CapturedDate = DateTime.Now;
                        //    depApprovals.FailureReason = "Failure at " + department.Name;
                        //    depApprovals.RCSApplicationStatusId = Convert.ToInt32(appli.Id);

                        //    if (department.Key == RCSDepartmentTypeKeys.SundryAccountSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(true, false, false, false, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.LegalSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, true, false, false, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.CreditControlSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, false, true, false, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.BillingSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, false, false, true, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.EndowmentSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, false, false, false, true, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }

                        //    depList.Add(depApprovals);
                        //    c.departmentsApprovals(depList);
                        //    //break;
                        //}
                        string pgMerchantId = "pg_crm_app_rcs";
                        string voteNumber = appli.TransferInformation.RatesNumber;
                        voteNumber = "RCS " + voteNumber;
                        string pgMerchantReference = appli.ApplicationReferenceNumber;
                        string pgMerchantDescription = "RCS Application Fee";
                        var ApplicationFeeAmount = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSApplicationFeeAmt).Value;

                        string Amount = ApplicationFeeAmount;
                        //string pgEmail = "Sashen.moodley@xetgroup.com";
                        //string pgMobile = "0846666435";

                        string pgEmail = null;
                        string pgMobile = null;
                        if (!string.IsNullOrEmpty(appli.Customer.SystemUser.EmailAddress))
                        {
                            pgEmail = appli.Customer.SystemUser.EmailAddress;
                        }

                        if (!string.IsNullOrEmpty(appli.Customer.SystemUser.MobileNumber))
                        {
                            pgMobile = appli.Customer.SystemUser.MobileNumber;
                        }
                        //pgMobile = "0846666435";



                        //string customerFirstName = "Sashen";
                        string customerFirstName = appli.Customer.FirstName;
                        //string customerLastName = "Moodley";
                        string customerLastName = appli.Customer.LastName;
                        string returnUrl = capture.returnurl;
                        //returnUrl = "http://localhost:3450/Capture/Capture";
                        string adhocRef1 = Convert.ToString(RCSAppID);

                        string adhocRef2 = "";
                        string adhocRef3 = "";
                        string adhocRef4 = "";
                        string adhocRef5 = "";
                        returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
                        //returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
                        string amt = Amount.Replace('.', ',');
                        decimal conAmt = Convert.ToDecimal(amt);
                        returnUrl = returnUrl + "/ReturnBackUrl";
                        //Format  parameters into Single Delimited String
                        string enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}",
                            pgMerchantId, voteNumber, pgMerchantReference, pgMerchantDescription, Amount,
                            pgEmail, pgMobile, customerFirstName, customerLastName, returnUrl, adhocRef1, adhocRef2, adhocRef3, adhocRef4, adhocRef5);
                        //Encrypt the Single String to and Encrypted string e
                        var e = new AesCrypto(encp).Encrypt(enc);
                        AppSetting PGDomain = db.AppSettings.FirstOrDefault(o => o.Key == PaymentGatewayKeys.PGDomain);
                        AppSetting PGEnvironment = db.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSPaymentGateway);

                        var baseFormat = PGDomain.Value + "PaymentGateway/" + PGEnvironment.Value + "?q=" + e;

                        //return Redirect(baseFormat);
                        var referenceType = cxt.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                        var application = cxt.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                        var returnUrl2 = "Test"/*success.ToString(CultureInfo.InvariantCulture)*/;
                        return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = baseFormat, rcsappId = appli.Id })));


             
                    }

                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
            return RedirectToAction("Index", "RCSApplication");
        }
        #endregion



        #region CaptureWalkIn OnLoad
        public ActionResult CaptureWalkIn()
        {
            TempData.Remove("np");
            var rcsType = db.RCSTypes.OrderBy(x => x.Name);
            ViewBag.Purchaser = new SelectList(db.PurchaserType.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
            ViewBag.TitleType = new SelectList(db.TitleTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Id", "Name");
            ViewBag.TypeOfTransfer = new SelectList(db.TransferTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
            ViewBag.JuristicType = new SelectList(db.EntityTypes.Where(x => (bool)x.IsActive != false), "id", "Name");
            ViewBag.IdentificationType = new SelectList(db.IdentificationTypes.Where(x => (bool)x.IsDeleted != true), "id", "Name");
            ViewBag.ActOnBehalfOf = new SelectList(db.ActOnBehalfTypes.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Id", "Name");
            ViewBag.MaritialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_MaritialStatus), "Name", "Name");
            ViewBag.TypeofProperty = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.TypeofProperty), "Id", "Name");
            ViewBag.ResidentialStatus = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_ResidentialStatus), "Name", "Name");
            ViewBag.Gender = new SelectList(rcsType.Where(x => x.Key == RCSTypeKeys.Rcs_Gender), "Name", "Name");
            var instructions = db.InstructionContents.Where(x => x.Key == InstructionContentKey.CaptureWalkinInstructions).FirstOrDefault();
            ViewBag.CaptureInstructions = instructions.Description;
            return View();
        }

        #endregion

        #region CaptureWalkIn
        [HttpPost]
        public ActionResult CaptureWalkIn(CaptureViewModel capture, string TransferInformationSellingPrice)
        {
            var Appref = "";
            var RCSAppID = 0;

            if (TransferInformationSellingPrice != null & TransferInformationSellingPrice != "")
            {
                var test = (TransferInformationSellingPrice).Replace(',', '.');
                var ttt = Convert.ToDecimal(TransferInformationSellingPrice, CultureInfo.InvariantCulture);
        
                capture.TransferInformation.SellingPrice = ttt;
            }

            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);
                    var CodeForRCSSave = "";
                    if (CodeForRCSSave != "Ready")
                    {
                        var AppSettings = cxt.AppSettings;
                        var Customers = cxt.Customers;
                        var Statuses = cxt.Status;

                        TransferInformation ti = new TransferInformation();
                        ti = capture.TransferInformation;
                        var getTransfertypeid = db.TransferTypes.Where(x => x.Key == ti.TransferTypeName).FirstOrDefault().Id;
              
                        ti.TransferTypeName = ti.TransferTypeName;
                        ti.TransferType = getTransfertypeid;
                        cxt.TransferInformations.Add(ti);
                        cxt.SaveChanges();
                        var TransferInfoID = ti.Id;

                        MunicipalAccountInformation MI = new MunicipalAccountInformation();
                        MI = capture.MunicipalAccountInformation;
                        cxt.MunicipalAccountInformations.Add(MI);
                        cxt.SaveChanges();

                        string CCCPrefix = ti.RatesNumber.Substring(0, 2);
                        var CCC = cxt.CCCs.Where(x => x.Prefix == CCCPrefix).FirstOrDefault();

                        ConveyancingAttorneyDetail CADgetattorneycode = new ConveyancingAttorneyDetail();
                        CADgetattorneycode = capture.ConveyancingAttorneyDetail;

                        RCSApplicationStatus appli = new RCSApplicationStatus();
                        appli.Customer = Customers.Include(o=>o.SystemUser).Where(o => o.AttorneyCode == CADgetattorneycode.AttorneyCode).FirstOrDefault();
                        appli.StatusId = Statuses.FirstOrDefault(o => o.Key == StatusKeys.ReuploadApplicationDocs).Id;
                        appli.TransferInformationId = TransferInfoID;
                        appli.TransferInformation = ti;
                        appli.CCCId = CCC.Id;

                      
                      
                  

                        int limiter = 0;

                        AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSSequence);
                        var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSSequenceLimit);
                        var BatchCounter = query.Value;
                        limiter = Convert.ToInt16(SeqLimit.Value);
                        if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                        {
                            var lastRef = query.Value;
                            BatchCounter = lastRef.ToString();
                            int nextSeq = Convert.ToInt16(query.Value) + 1;
                            string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                            query.Value = nextVal;
                            cxt.Entry(query).State = EntityState.Modified;
                            cxt.SaveChanges();
                        }
                        else
                        {
                            int nextSeq = 1;
                            string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                            BatchCounter = nextVal;
                            nextSeq = 2;
                            nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                            query.Value = nextVal;
                            query.ModifiedDateTime = DateTime.Now.Date;
                            cxt.Entry(query).State = EntityState.Modified;
                            cxt.SaveChanges();

                        }

                        if (ti.RatesNumber != null)
                        {
                            var refs = ti.RatesNumber;
                            var RefNum = refs + "RCC" + DateTime.Now.ToString("ddMMyyyy") + BatchCounter;
                            appli.ApplicationReferenceNumber = RefNum;
                            Appref = appli.ApplicationReferenceNumber;
                        }

                        appli.MunicipalAccountInformationId = MI.Id;
                        appli.MunicipalAccountInformation = MI;

                        appli.TransferInformationId = TransferInfoID;
                        appli.TransferInformation = ti;

                        var SystUserId = RoundRobin(false, false, false, false, false, true);
                        var cust = Customers.Where(x => x.SystemUserId == SystUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        appli.ClerkId = cust;

                        cxt.RCSApplicationStatus.Add(appli);
                        cxt.SaveChanges();

                        RCSAppID = appli.Id;

                        ConveyancingAttorneyDetail CAD = new ConveyancingAttorneyDetail();
                        CAD = capture.ConveyancingAttorneyDetail;
                        CAD.RCSApplicationStatusId = appli.Id;
                        cxt.ConveyancingAttorneyDetails.Add(CAD);
                        cxt.SaveChanges();

                        SellerInformation sellerinfo = new SellerInformation();

                        sellerinfo = capture.SellerInformation;
                        sellerinfo.RCSApplicationStatusId = appli.Id;
                        sellerinfo.RCSApplicationStatus = appli;
                        cxt.SellerInformations.Add(sellerinfo);
                        cxt.SaveChanges();

                        var PI = (List<PurchaserInformation>)TempData["np"];
                        if (PI != null)
                        {
                            foreach (var item in PI)
                            {
                                PurchaserInformation purchInfo = new PurchaserInformation();

                                purchInfo = item;
                                purchInfo.RCSApplicationStatusId = appli.Id;
                                purchInfo.RCSApplicationStatus = appli;
                                cxt.PurchaserInformations.Add(purchInfo);
                                cxt.SaveChanges();
                            }
                        }
                        if (PI == null)
                        {
                            PurchaserInformation pi = new PurchaserInformation();
                            pi = capture.PurchaserInformation;

                            pi.RCSApplicationStatusId = appli.Id;
                            var getpurchasertypeid = db.PurchaserType.Where(x => x.Key == pi.PurchaserTypeKey).FirstOrDefault().Id;
                            pi.PurchaserTypeId = getpurchasertypeid;
                            pi.PurchaseType = getpurchasertypeid;
                            cxt.PurchaserInformations.Add(pi);
                            cxt.SaveChanges();
                        }
                        if (capture.ElectricitySessionList != null && capture.ElectricitySessionList != "")
                        {
                            List<ElectricityMeterInformation> ElectricityMeterInfo = JsonConvert.DeserializeObject<List<ElectricityMeterInformation>>(capture.ElectricitySessionList);
                            if (ElectricityMeterInfo.Count > 0)
                            {
                                foreach (var item in ElectricityMeterInfo)
                                {
                                    ElectricityMeterInformation elecinfo = new ElectricityMeterInformation();

                                    if (item.DateString.Length == 8)
                                    {
                                        CultureInfo provider = CultureInfo.InvariantCulture;
                                        string format = "yyyyMMdd";
                                        DateTime result = DateTime.ParseExact(item.DateString, format, provider);
                                        item.ElectricityMeterReadingDateTaken = result;
                                    }
                                    else
                                    {
                                        item.ElectricityMeterReadingDateTaken = Convert.ToDateTime(item.DateString);
                                    }
                                    elecinfo = item;
                                    elecinfo.RCSApplicationStatusId = appli.Id;
                                    elecinfo.RCSApplicationStatus = appli;
                                    cxt.ElectricityMeterInformations.Add(elecinfo);
                                    cxt.SaveChanges();
                                }
                            }
                        }

                        if (capture.WaterSessionList != null && capture.WaterSessionList != "")
                        {
                            List<WaterMeterInformation> WaterMeterInfo = JsonConvert.DeserializeObject<List<WaterMeterInformation>>(capture.WaterSessionList);

                            if (WaterMeterInfo.Count > 0)
                            {
                                foreach (var item in WaterMeterInfo)
                                {
                                    WaterMeterInformation waterinfo = new WaterMeterInformation();
                                    CultureInfo provider = CultureInfo.InvariantCulture;

                                    //if (item.DateString.Length == 8)
                                    //{
                                    //    string format = "yyyyMMdd";
                                    //    DateTime result = DateTime.ParseExact(item.DateString, format, provider);
                                    //    item.WaterMeterReadingDateTaken = result;
                                    //}
                                    //else
                                    //{
                                    //    item.WaterMeterReadingDateTaken = Convert.ToDateTime(item.DateString);
                                    //}

                                    waterinfo = item;
                                    waterinfo.RCSApplicationStatusId = appli.Id;
                                    waterinfo.RCSApplicationStatus = appli;
                                    cxt.WaterMeterInformations.Add(waterinfo);
                                    cxt.SaveChanges();
                                }
                            }
                        }


                        //var EI = (List<ElectricityMeterInformation>)TempData["ep"];
                        //if (EI != null)
                        //{
                        //    foreach (var item in EI)
                        //    {
                        //        ElectricityMeterInformation elecinfo = new ElectricityMeterInformation();

                        //        elecinfo = item;
                        //        elecinfo.RCSApplicationStatusId = appli.Id;
                        //        elecinfo.RCSApplicationStatus = appli;
                        //        cxt.ElectricityMeterInformations.Add(elecinfo);
                        //        cxt.SaveChanges();
                        //    }
                        //}
                        //var WI = (List<WaterMeterInformation>)TempData["wp"];
                        //if (WI != null)
                        //{
                        //    foreach (var item in WI)
                        //    {
                        //        WaterMeterInformation waterinfo = new WaterMeterInformation();

                        //        waterinfo = item;
                        //        waterinfo.RCSApplicationStatusId = appli.Id;
                        //        waterinfo.RCSApplicationStatus = appli;
                        //        cxt.WaterMeterInformations.Add(waterinfo);
                        //        cxt.SaveChanges();
                        //    }
                        //}
                        var ActivityTrackerMessage = cxt.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ConevayncerCapturesNewApplication).Description.ToString()/* + DecisionType*/;
                        var RCSHistoryLog = new RCSApplicationHistoryLog
                        {
                            RCSApplicationStatusId = appli.Id,
                            AuditAction = ActivityTrackerMessage,
                            UserId = Customer.Id,
                            CreatedDateTime = DateTime.Now,
                            IsActive = true
                        };
                        cxt.RCSApplicationHistoryLogs.Add(RCSHistoryLog);
                        cxt.SaveChanges();

                        WalkInApplicantDetails WIA = new WalkInApplicantDetails();
                        WIA = capture.WalkInApplicant;
                        WIA.RCSApplicationStatusId = appli.Id;
                        cxt.WalkInApplicantDetails.Add(WIA);
                        cxt.SaveChanges();

                     

                        Email SendMail = new Email();
                        string attorneyemail ="";
                        if (CAD.Email == null)
                        {
                            attorneyemail = "suhail.dada@xetgroup.com";

                        }
                        else { 
                            attorneyemail = CAD.Email;
                             }
                        
                        string attorneyname = CAD.ContactPerson1;
                        var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.ApplicationCapturedSuccessfully).FirstOrDefault();

                        string emailbody = getemailbody.Description + appli.ApplicationReferenceNumber;
                        SendMail.GenerateEmail(attorneyemail, "RCS- New Online Application Submission",
                                      emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);



                        var ActivityTrackerMessage2 = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.EmailNewApplication).Description.ToString() + emailbody;
                        var RCSHistoryLog2 = new RCSApplicationHistoryLog
                        {
                            RCSApplicationStatusId = appli.Id,
                            AuditAction = ActivityTrackerMessage2,
                            UserId = Customer.Id,
                            CreatedDateTime = DateTime.Now,
                            IsActive = true
                        };
                        cxt.RCSApplicationHistoryLogs.Add(RCSHistoryLog2);
                        cxt.SaveChanges();
                        //List<RCSDepartmentType> DepartmentList = new List<RCSDepartmentType>();
                        //DepartmentList = db.RCSDepartmentTypes.ToList();
                        //CaptureController c = new CaptureController();

                        ////Insert all departments
                        //foreach (var department in DepartmentList)
                        //{
                        //    DepartmentsApproval depApprovals = new DepartmentsApproval();
                        //    List<DepartmentsApproval> depList = new List<DepartmentsApproval>();
                        //    var ApprovrcsType = Statuses.Where(x => x.Key == StatusKeys.PendingDocumentsApproval).FirstOrDefault();
                        //    depApprovals.StatusId = ApprovrcsType.Id;
                        //    depApprovals.DepartmentId = department.Id;
                        //    depApprovals.CapturedDate = DateTime.Now;
                        //    depApprovals.FailureReason = "Failure at " + department.Name;
                        //    depApprovals.RCSApplicationStatusId = Convert.ToInt32(appli.Id);

                        //    if (department.Key == RCSDepartmentTypeKeys.SundryAccountSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(true, false, false, false, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.LegalSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, true, false, false, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.CreditControlSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, false, true, false, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.BillingSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, false, false, true, false, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }
                        //    else if (department.Key == RCSDepartmentTypeKeys.EndowmentSection)
                        //    {
                        //        var SystemUserId = c.RoundRobin(false, false, false, false, true, false);
                        //        var custId = db.Customers.Where(x => x.SystemUserId == SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault().Id;
                        //        depApprovals.AssignedToCustomerId = custId;
                        //    }

                        //    depList.Add(depApprovals);
                        //    c.departmentsApprovals(depList);
                        //    //break;
                        //}
                        string pgMerchantId = "pg_crm_app_rcs";
                        string voteNumber = appli.TransferInformation.RatesNumber;
                        voteNumber = "RCS " + voteNumber;
                        string pgMerchantReference = appli.ApplicationReferenceNumber;
                        string pgMerchantDescription = "RCS Application Fee";
                        var ApplicationFeeAmount = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSApplicationFeeAmt).Value;

                        string Amount = ApplicationFeeAmount;
                        //string pgEmail = "Sashen.moodley@xetgroup.com";
                        //string pgMobile = "0846666435";

                        string pgEmail = null;
                        string pgMobile = null;
                        if (!string.IsNullOrEmpty(appli.Customer.SystemUser.EmailAddress))
                        {
                            pgEmail = appli.Customer.SystemUser.EmailAddress;
                        }

                        if (!string.IsNullOrEmpty(appli.Customer.SystemUser.MobileNumber))
                        {
                            pgMobile = appli.Customer.SystemUser.MobileNumber;
                        }
                        //pgMobile = "0846666435";



                        //string customerFirstName = "Sashen";
                        string customerFirstName = appli.Customer.FirstName;
                        //string customerLastName = "Moodley";
                        string customerLastName = appli.Customer.LastName;
                        string returnUrl = capture.returnurl;
                        //returnUrl = "http://localhost:3450/Capture/Capture";
                        string adhocRef1 = Convert.ToString(RCSAppID);

                        string adhocRef2 = "";
                        string adhocRef3 = "";
                        string adhocRef4 = "";
                        string adhocRef5 = "";
                        returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
                        //returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
                        string amt = Amount.Replace('.', ',');
                        decimal conAmt = Convert.ToDecimal(amt);
                        returnUrl = returnUrl + "/ReturnBackUrl";
                        //Format  parameters into Single Delimited String
                        string enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}",
                            pgMerchantId, voteNumber, pgMerchantReference, pgMerchantDescription, Amount,
                            pgEmail, pgMobile, customerFirstName, customerLastName, returnUrl, adhocRef1, adhocRef2, adhocRef3, adhocRef4, adhocRef5);
                        //Encrypt the Single String to and Encrypted string e
                        var e = new AesCrypto(encp).Encrypt(enc);
                        AppSetting PGDomain = db.AppSettings.FirstOrDefault(o => o.Key == PaymentGatewayKeys.PGDomain);
                        AppSetting PGEnvironment = db.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSPaymentGateway);

                        var baseFormat = PGDomain.Value + "PaymentGateway/" + PGEnvironment.Value + "?q=" + e;

                        //return Redirect(baseFormat);
                        var referenceType = cxt.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                        var application = cxt.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

                        var returnUrl2 = "Test"/*success.ToString(CultureInfo.InvariantCulture)*/;
                        return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = baseFormat, rcsappId = appli.Id })));



                    }

                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
            return RedirectToAction("Index", "RCSApplication");
        }
        #endregion
        #region ViewAllDetails
        [DecryptParameter]
        public ActionResult Details(int? refNo)
        {
            if (refNo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var vm = new ApplicationDetailsViewModel();
            RCSApplicationStatus rcsapp=db.RCSApplicationStatus.Where(x => x.Id == refNo && x.IsDeleted == false).ToList().FirstOrDefault();
            var TransferInformationID = rcsapp.TransferInformationId;
            var MunicipalAccountInformationID = rcsapp.MunicipalAccountInformationId;
            TransferInformation TransferInformation = db.TransferInformations.Include(x => x.TransferTypes).Where(x => x.Id == TransferInformationID).ToList().FirstOrDefault();
            SellerInformation sellerInformation= db.SellerInformations.Where(x => x.RCSApplicationStatusId == refNo).ToList().FirstOrDefault();
            MunicipalAccountInformation MunicipalAccountInformation= db.MunicipalAccountInformations.Where(x => x.Id == MunicipalAccountInformationID).ToList().FirstOrDefault();

            WalkInApplicantDetails walkInApplicantDetails= db.WalkInApplicantDetails.Include(x=>x.ActOnBehalfType).Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList().FirstOrDefault();
            if(walkInApplicantDetails != null)
            {
                ViewBag.walkIn = "walkIn";
            }
            if(TransferInformation.TransferTypeName=="tt_SectionalTitle")
            {
                ViewBag.sectionaltitle = "ST";
            }
            ConveyancingAttorneyDetail conveyancingAttorneyDetail= db.ConveyancingAttorneyDetails.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList().FirstOrDefault();


            var CaseHistoryLog =
db.RCSApplicationHistoryLogs.Where(d => d.RCSApplicationStatusId == rcsapp.Id && d.IsActive == true && d.IsDeleted == false).Include(d => d.RCSApplicationStatus).Include(d => d.User).ToList();

            vm.ElectricityMeterList = db.ElectricityMeterInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();
            vm.WaterMeterList=db.WaterMeterInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();
            vm.PurchaserList=db.PurchaserInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();

            vm.ConveyancingAttorneyDetail = conveyancingAttorneyDetail;
            vm.TransferInformation = TransferInformation;
            vm.SellerInformation = sellerInformation;
            vm.MunicipalAccountInformation = MunicipalAccountInformation;
            vm.WalkInApplicant = walkInApplicantDetails;
            vm.RCSApplicationHistoryLogs = CaseHistoryLog;



            var documentReferenceType = db.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.RCSRefund);
            if (documentReferenceType == null)
                throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                    ReferenceTypeKeys.RCSUpload));

            var documentCheckLists = new List<DocumentCheckList>();

            var referenceTypeId = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload)).Id;

            var AuthorityToActAttorney = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var ProofOfProperty = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

            //documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceTypeId));


            var MunicipalStatement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

            var MunicipalCheckList = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));



            var BankConfirmationLetter = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankConfirmationLetter);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankConfirmationLetter.Id && dcl.ReferenceTypeId == referenceTypeId));

            var DeedSearch = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DeedSearch);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceTypeId));

            //this code will change
            var SellerID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceTypeId));

            var PurchaserID = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceTypeId));


            var AdditonalDocCap = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditonalDocCap.Id && dcl.ReferenceTypeId == referenceTypeId));






            //var addDocumentType = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            //var addDoc = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == documentReferenceType.Id);
            //if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            //{
            //    documentCheckLists.Add(addDoc);
            //}
            try
            {
                //var MunicipalAccInfo = db.MunicipalAccountInformations.Where(x => x.Id == rcsapp.MunicipalAccountInformationId).FirstOrDefault();
                var ElectricityInfo = db.ElectricityMeterInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();
                var WaterInfo = db.WaterMeterInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();
                if (WaterInfo != null && ElectricityInfo != null)
                {
                    if (ElectricityInfo.Count > 0)
                    {
                        var ElectricityMeterReading = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ElectricityMeterReading);

                        documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ElectricityMeterReading.Id && dcl.ReferenceTypeId == referenceTypeId));


                    }

                    if (WaterInfo.Count > 0)
                    {
                        var WaterMeterReading = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.WaterMeterReading);

                        documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == WaterMeterReading.Id && dcl.ReferenceTypeId == referenceTypeId));


                    }

                }

            }
            catch (Exception)
            {

            }



            //var ElectricityMeterReading = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ElectricityMeterReading);

            //documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ElectricityMeterReading.Id && dcl.ReferenceTypeId == referenceTypeId));


            try
            {
                var purchasers = db.PurchaserInformations.Include(x => x.PurchaserTypes).Where(x => x.RCSApplicationStatusId == rcsapp.Id).FirstOrDefault();


                if (purchasers != null)
                {
                    if (purchasers.PurchaserTypes.Key == PurchaserTypeKeys.Company || purchasers.PurchaserTypes.Key == PurchaserTypeKeys.CloseCorporation)
                    {
                        var PurchaserSalesAgreement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

                        documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));


                    }
                }


                var walkin = db.WalkInApplicantDetails.Include(x => x.ActOnBehalfType).Where(x => x.RCSApplicationStatusId == rcsapp.Id).FirstOrDefault();
                if (walkin != null)
                {
                    if (walkin.ActOnBehalfType.Key == ActOnBehalfTypeKeys.ExecutorofEstate)
                    {
                        var ExecutorOfestate = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ExecutorofEstate);

                        documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ExecutorOfestate.Id && dcl.ReferenceTypeId == referenceTypeId));


                    }
                    if (walkin.ActOnBehalfType.Key == ActOnBehalfTypeKeys.ExecutorofEstate)
                    {

                    }
                }
                else
                {

                }

                var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

                documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

            }


            catch (Exception)
            {

            }



            //var MunicipalAccStatement = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundMunicipalStatement);

            //documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalAccStatement.Id && dcl.ReferenceTypeId == documentReferenceType.Id));

            //var addDocumentType = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            //var addDocumentType = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            //var addDoc = db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == documentReferenceType.Id);
            //if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            //{
            //    documentCheckLists.Add(addDoc);
            //}
          
            var documents = db.Documents.Where(d =>d.RCSApplicationStatusId == rcsapp.Id).ToList();
            var notes = db.Notes.Where(n => n.ReferenceId == rcsapp.CustomerId).ToList();
            var customerDocuments = db.Documents.Include(d => d.File).Include(d => d.Status)
                                            .Where(d => d.RCSApplicationStatusId == rcsapp.Id
                        && d.IsActive
                        && d.IsDeleted == false && d.RCSApplicationStatusId == rcsapp.Id).ToList();

            foreach (var doc in customerDocuments)
            {
                doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

            }
            var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
            var dvm = new DocumentsViewModel
            {
                ApplicationId = application.Id,
                CustomerId = rcsapp.CustomerId,
                Documents = customerDocuments,
                IsUploadView = false,
                DocumentCheckLists = documentCheckLists.ToList(),
            };

            vm.Document = dvm;

            return View(vm);
        }



        [DecryptParameter]
        public ActionResult viewpaymenthistory(int? refNo)
        {
            eServicesDbContext context = new eServicesDbContext();
            Initialise();
           



            RCSApplicationStatus rcsApps = null;

            rcsApps = context.RCSApplicationStatus.Where(x => x.Id == refNo && x.IsDeleted == false).Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.TransferInformation).Include(d => d.Status).FirstOrDefault();
          

           
               
                var vm = new DepartmentsApprovalViewModel();
                vm.OnlinePaymentHistory = db.AssessmentPaymentTransactions.Where(x => x.RCSApplicationStatusId == refNo && x.IsActive == true).ToList();

            var FiguresPaymentDocumentType = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);

           var FiguresPayment= context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == FiguresPaymentDocumentType.Id);

            var ApplicationFeeDocumentType = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfPayment);

            var ApplicationFee = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ApplicationFeeDocumentType.Id);


            var customerDocuments = context.Documents.Include(d => d.File).Include(d => d.Status).Include(d=>d.DocumentCheckList).Include(d=>d.DocumentCheckList.DocumentType)
                                         .Where(d => d.CustomerId == Customer.Id && (d.DocumentCheckListId== FiguresPayment.Id||d.DocumentCheckListId == ApplicationFee.Id)
                     && d.IsActive
                     && d.IsDeleted == false && d.RCSApplicationStatusId == rcsApps.Id).ToList();

            foreach (var doc in customerDocuments)
            {
                doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));

            }
            var dvm = new DocumentsViewModel
                    {
                       
                       
                        Documents = customerDocuments
                      
                     
                    };


            vm.Document = dvm;



            vm.RCSApplicationStatus = rcsApps;
                

                return View(vm);
          
        }

        #endregion
        #region Department Comments Enum
        public enum DepartmentsComments
        {
            Legal,
            CreditControl,
            Endowment,
            Accounts
        }
        #endregion

        #region Insert For a Specific Department
        public void departmentsApprovals(IEnumerable<DepartmentsApproval> depList)
        {
            var cxt = new eServicesDbContext();
            foreach (var item in depList)
            {
                DepartmentsApproval depApprovals = new DepartmentsApproval();
                depApprovals = item;
                cxt.DepartmentsApprovals.Add(depApprovals);
                cxt.SaveChanges();
            }
        }
        #endregion

        #region Approve Or Reject 
        public void ApproveOrReject(int id, string ApprovalStatusddl, string ReAssignddl , int? ReAllocateddl)
        {
            var ApprovrcsType = db.Status;
            var approval = db.DepartmentsApprovals.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();

            if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
            {
                ReAllocateddl = 0;
                approval.StatusId = ApprovrcsType.Where(x=>x.Key == StatusKeys.Approved).FirstOrDefault().Id;
                updateApproval(approval, ReAssignddl , (int)ReAllocateddl);
            }

            else if (ApprovalStatusddl == RCSActionTypeKeys.ReAllocate)
            {
                approval.StatusId = ApprovrcsType.Where(x => x.Key == StatusKeys.ReAllocate).FirstOrDefault().Id;
                updateApproval(approval, ReAssignddl, (int)ReAllocateddl);
            }

            else if (ApprovalStatusddl == RCSActionTypeKeys.ReAssignDep)
            {
                approval.StatusId = ApprovrcsType.Where(x => x.Key == StatusKeys.ReAssignDep).FirstOrDefault().Id;
                updateApproval(approval, ReAssignddl, (int)ReAllocateddl);
            }

            else if (ApprovalStatusddl == RCSActionTypeKeys.Disprove)
            {
                approval.StatusId = ApprovrcsType.Where(x => x.Key == StatusKeys.Disprove).FirstOrDefault().Id;
                updateApproval(approval, ReAssignddl, (int)ReAllocateddl);
            }

            else if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
            {
                approval.StatusId = ApprovrcsType.Where(x => x.Key == StatusKeys.Rejected).FirstOrDefault().Id;
                updateApproval(approval, ReAssignddl, (int)ReAllocateddl);
            }

            var AllApprovals = db.DepartmentsApprovals.Where(x=>x.RCSApplicationStatusId == approval.RCSApplicationStatusId).ToList();
          
            int countApproved = 0;
            int countNothing = 0;
            int countDisapprove = 0;
            int countRejected = 0;

            var item = AllApprovals.ToList();

            for (int i = 0; i < item.Count(); i++)
                {

                    if (item[i].StatusId == ApprovrcsType.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id)
                    {
                        countNothing++;
                    }

                    else if (item[i].StatusId == ApprovrcsType.Where(x => x.Key == StatusKeys.Approved).FirstOrDefault().Id)
                    {
                        countApproved++;
                    }

                    else if (item[i].StatusId == ApprovrcsType.Where(x => x.Key == StatusKeys.Disprove).FirstOrDefault().Id)
                    {
                        countDisapprove++;
                    }

                    else if (item[i].StatusId == ApprovrcsType.Where(x => x.Key == StatusKeys.Rejected).FirstOrDefault().Id)
                    {
                        countRejected++;
                    }

                    if (countNothing > 0)
                    {
                        //break;
                    }

                    if (item.Count() == countApproved)
                    {
                        //Write codeMethod to update rcs application status   
                        updateApprovals(item, item[i].RCSApplicationStatusId, ApprovrcsType.Where(x => x.Key == StatusKeys.BackOffice).FirstOrDefault().Id);
                    }

                    //Goes to the BO so that they can choose to reject the application.
                    if (countDisapprove > 0)
                    {   
                        //Write codeMethod to update rcs application status   
                        updateApprovals(item, item[i].RCSApplicationStatusId, ApprovrcsType.Where(x => x.Key == StatusKeys.Disprove).FirstOrDefault().Id);
                    }

                    //BO rejected the application.
                    if (countRejected > 0)
                    {
                        //Write codeMethod to update rcs application status   
                        updateApprovals(item, item[i].RCSApplicationStatusId, ApprovrcsType.Where(x => x.Key == StatusKeys.Rejected).FirstOrDefault().Id);
                    }
                }
        }


        #endregion

        #region Update Appproval Status Single Record
            public void updateApproval(DepartmentsApproval approval , string ReAssignddl , int ReAllocateddl)
            {
                var status = db.Status.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();

                if (approval.StatusId == status.Where(x=>x.Key == StatusKeys.Approved).FirstOrDefault().Id)
                {
                    //Approval
                    var depApproval = db.DepartmentsApprovals.Where(x => x.Id == approval.Id).FirstOrDefault();
                    depApproval.Id = approval.Id;
                    depApproval.StatusId = approval.StatusId;
                    depApproval.CapturedDate = DateTime.Now.Date;
                    depApproval.CreatedDateTime = DateTime.Now;
                    db.SaveChanges();
                }

                else if (approval.StatusId == status.Where(x => x.Key == StatusKeys.ReAllocate).FirstOrDefault().Id)
                {
                    //Re-allocate to another user
                    var depApproval = db.DepartmentsApprovals.Where(x => x.Id == approval.Id).FirstOrDefault();
                    depApproval.Id = approval.Id;
                    depApproval.StatusId = approval.StatusId;
                    depApproval.CapturedDate = DateTime.Now.Date;
                    depApproval.CreatedDateTime = DateTime.Now;
                    depApproval.AssignedToCustomerId = ReAllocateddl;

                    db.SaveChanges();
                }

                else if (approval.StatusId == status.Where(x => x.Key == StatusKeys.ReAssignDep).FirstOrDefault().Id)
                {
                    // Re-assign to another dep & rr to another user.
                    var depApproval = db.DepartmentsApprovals.Where(x => x.Id == approval.Id).FirstOrDefault();

                    depApproval.Id = approval.Id;
                    depApproval.StatusId = approval.StatusId;
                    depApproval.CapturedDate = DateTime.Now.Date;
                    depApproval.CreatedDateTime = DateTime.Now;
                    depApproval.AssignedToCustomerId = ReAllocateddl;
                    var depId = db.RCSDepartmentTypes.Where(x => x.Key == ReAssignddl).FirstOrDefault().Id;
                    depApproval.DepartmentId = Convert.ToInt32(depId);
                    db.SaveChanges();
                }

                else if (approval.StatusId == status.Where(x=>x.Key == StatusKeys.Disprove).FirstOrDefault().Id)
                {
                    //Disapproval
                    var depApproval = db.DepartmentsApprovals.Where(x => x.Id == approval.Id).FirstOrDefault();
                    depApproval.Id = approval.Id;
                    depApproval.StatusId = approval.StatusId;
                    depApproval.CapturedDate = DateTime.Now.Date;
                    depApproval.CreatedDateTime = DateTime.Now;
                    db.SaveChanges();
                }

                else if (approval.StatusId == status.Where(x=>x.Key == StatusKeys.Rejected).FirstOrDefault().Id)
                {
                    //Rejected
                    var depApproval = db.DepartmentsApprovals.Where(x => x.Id == approval.Id).FirstOrDefault();
                    depApproval.Id = approval.Id;
                    depApproval.StatusId = approval.StatusId;
                    depApproval.CapturedDate = DateTime.Now.Date;
                    depApproval.CreatedDateTime = DateTime.Now;
                    db.SaveChanges();
                }
            }
        #endregion

        #region Update Appproval Status Multiple Records
        public void updateApprovals(IEnumerable<DepartmentsApproval> depList , int RCSId, int status)
        {
            var statuses = db.Status.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
            var application = db.RCSApplicationStatus.Where(x => x.Id == RCSId && x.IsDeleted == false).FirstOrDefault();
            var BackOfficeStatus = statuses.Where(x => x.Key == StatusKeys.BackOffice).FirstOrDefault().Id;

            foreach (var item in depList)
            {
                DepartmentsApproval depApprovals = new DepartmentsApproval();
                depApprovals = item;
                //Update what needs to be updated so that it can go to the bo for example a status
                depApprovals.StatusId = status;
                db.SaveChanges();
            }

            if (BackOfficeStatus == status)
            {
                application.StatusId = BackOfficeStatus;
                db.SaveChanges();
            }
        }
        #endregion

        #region NotificationsForStaff
        public bool BackOfficeNotification(int RCSAppID, int CustomerID, string QueueName)
        {
          

            try
            {
                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.BONewCaseLoaded).FirstOrDefault();
                Email SendMail = new Email();
                var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == RCSAppID).FirstOrDefault();
                var BackOfficeClerk = db.Customers.Where(x => x.Id == CustomerID).FirstOrDefault();
                string attorneyemail = BackOfficeClerk.EmailAddress;
                string attorneyname = BackOfficeClerk.FirstName + " " + BackOfficeClerk.LastName;
                string emailbody = getemailbody.Description;
                emailbody = emailbody.Replace("{0}", QueueName);
                emailbody = emailbody.Replace("{1}", RCSApplication.ApplicationReferenceNumber);
                var systemusermobilenum = BackOfficeClerk.CellPhoneNumber;
                //string emailbody = "Your proof of payment documents for RCS assessment figures has been rejected , please re-upload required documents for RCS Application Reference Number: " + rcsApps.ApplicationReferenceNumber;

                var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + emailbody;

                SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum, RCSApplication.Id, CustomerID, emailbody, attorneyemail, "RCS-Online Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                return true;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }

            //return "test";
        }

        public bool BackOfficeReAllocatedNotification(int RCSAppID, int CustomerID, string QueueName)
        {


            try
            {
                var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.BONewCaseReAllocated).FirstOrDefault();
                Email SendMail = new Email();
                var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == RCSAppID).FirstOrDefault();
                var BackOfficeClerk = db.Customers.Where(x => x.Id == CustomerID).FirstOrDefault();
                string attorneyemail = BackOfficeClerk.EmailAddress;
                string attorneyname = BackOfficeClerk.FirstName + " " + BackOfficeClerk.LastName;
                string emailbody = getemailbody.Description;
                emailbody = emailbody.Replace("{0}", QueueName);
                emailbody = emailbody.Replace("{1}", RCSApplication.ApplicationReferenceNumber);
                var systemusermobilenum = BackOfficeClerk.CellPhoneNumber;
                //string emailbody = "Your proof of payment documents for RCS assessment figures has been rejected , please re-upload required documents for RCS Application Reference Number: " + rcsApps.ApplicationReferenceNumber;

                var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + emailbody;

                SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum, RCSApplication.Id, CustomerID, emailbody, attorneyemail, "RCS-Online Application",
                              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                return true;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }

            //return "test";
        }
        #endregion

        #region Round Robin
        //Sashen Round robin
        public int RoundRobinCCC(bool AcknowlegeApplication, bool SubmitFigures, bool IssueRCC, bool BillingSection, bool Rates, bool BackOffice, bool CreditControl , bool Sundries , int RCSAppID, int DepartmentID, bool AcknowlegeRefund, bool IssueRefundsCollection, int RefundAppID)
        {
            //get all users for each department
            var applicationUserRoles = db.Roles.ToList();
            //DateTime.Compare(x.DateTimeValueColumn.Date, DateTime.Now.Date) <= 0
            //x => EntityFunctions.TruncateTime(x.DateTimeStart) == currentDate.Date
            //try
            //{
            //    var dates = new string[2];
            //    dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            //    dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            //    var startDate = DateTime.Parse(dates[0]).Date;
            //    var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            //    var oneDayTime = endDate - startDate;
            //    var NumOfDDAppPending = db.RoundRobinQueues.Where(x => x.IsActive == true && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();

            //    //DateTime DateFrom = DateTime.Now;
            //    //DateTime DateTo = DateTime.Now;
            //    //var startDate = DateTime.TryParseExact(DateFrom, "yyyyMMdd");
            //    //endDate = DateTime.ParseExact(dates[1], "yyyyMMdd").AddTicks(-1).AddDays(1);
            //    //var RoundRobingQueue1 = db.RoundRobinQueues.Where(x => DbFunctions.TruncateTime(x.CreatedDateTime) == DateTime.Now).ToList();

            //    //var NumOfDDAppPending = db.RoundRobinQueues.Where(x => x.IsActive == true && (DateFrom <= x.CreatedDateTime && DateTo >= x.CreatedDateTime)).ToList();

            //}
            //catch (Exception IO)
            //{

            //}
            var dates = new string[2];
            dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            var startDate = DateTime.Parse(dates[0]).Date;
            var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            var oneDayTime = endDate - startDate;
            var RoundRobingQueue = db.RoundRobinQueues.Where(x => x.IsActive == true && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();
 
            //var RoundRobingQueue = db.RoundRobinQueues.Where(x=>x.IsActive == true && DateTime.Compare(x.CreatedDateTime.Value.Date, DateTime.Now.Date) <= 0).ToList();
            //var RoundRobingQueue = db.RoundRobinQueues.Where(x=>x.CreatedDateTime.Value.Day == DateTime.Now.Date).ToList();
            var AccountsManagementUsers = (List<SystemIdentityUser>)null;
            var SubmitFiguresUsers = (List<SystemIdentityUser>)null;
            var IssueRCCUsers = (List<SystemIdentityUser>)null;
            var BillingUsers = (List<SystemIdentityUser>)null;
            var RateUsers = (List<SystemIdentityUser>)null;
            var BOUsers = (List<SystemIdentityUser>)null;
            var CreditControlUsers = (List<SystemIdentityUser>)null;
            var SundriesUsers = (List<SystemIdentityUser>)null;
            var RefundUsers = (List<SystemIdentityUser>)null;

            //DateTime dt = DateTime.Now;

            //var test = dt.Date;
            //get a count of applications for each user in each department
            var AssignedToUser = 0;

            var responsibilityTypes = db.ResponsibilityTypes.ToList();

            if (AcknowlegeApplication)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x=>x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var AccountsManagement = applicationUserRoles.Where(x => x.Name == "Acknowledge RCS Application").FirstOrDefault().Id;
                AccountsManagementUsers = GetUsersInRole(AccountsManagement).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault();
                if (AccountsManagementUsers.Count() > 0)
                {
            
                    AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;




                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityTypeId.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CCCId = Convert.ToInt16(RcsApplication.CCCId),
                        CCCName = RcsApplication.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName +" for role: "+ ResponsibilityTypeId.Name,
                        RoleID= AccountsManagement,
                        RoleName= "Acknowledge RCS Application",
                        IsActive = true,
                        IsDeleted= false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();

                }

          
                // JK.20140724a - Custom profile information.
              
            }

            else if (SubmitFigures)
            {


                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Submit Figures").FirstOrDefault().Id;
                SubmitFiguresUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault();

                if (SubmitFiguresUsers.Count() > 0)
                {

                    AssignedToUser = AssigedToCCRR(SubmitFiguresUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityType.Id,
                        CCCId = Convert.ToInt16(RcsApplication.CCCId),
                        CCCName = RcsApplication.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();
                }
            }

            else if (IssueRCC)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Issue Certificate").FirstOrDefault().Id;
                IssueRCCUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault();


                if (IssueRCCUsers.Count() > 0)
                {

                    AssignedToUser = AssigedToCCRR(IssueRCCUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CCCId = Convert.ToInt16(RcsApplication.CCCId),
                        CCCName = RcsApplication.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();

                }
            }

            else if (BillingSection)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Billing").FirstOrDefault().Id;
                BillingUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(BillingUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            


                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId = DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
            }

            else if (Rates)
            {

                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Rates").FirstOrDefault().Id;
                RateUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(RateUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
       


                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId = DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
            }

        
            else if(CreditControl)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Credit Control").FirstOrDefault().Id;
                CreditControlUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(CreditControlUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
               


                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId = DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
            }
            else if(Sundries)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Sundry Account").FirstOrDefault().Id;
                SundriesUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(SundriesUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
          


                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId =DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);

            }
            else if (AcknowlegeRefund)
            {
                var RcsApplication = db.RefundApplications.Include(x => x.Status).Include(x => x.RCSApplicationStatus).Include(x => x.RCSApplicationStatus.CCC).OrderByDescending(x=>x.Id).FirstOrDefault(x => x.Id == RefundAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Acknowledge Refund Application").FirstOrDefault().Id;
                RefundUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.RCSApplicationStatus.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault();

                if (RefundUsers.Count() >0)
                {
                    AssignedToUser = AssigedToCCRR(RefundUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.RCSApplicationStatus.Id,
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CCCId = Convert.ToInt16(RcsApplication.RCSApplicationStatus.CCCId),
                        CCCName = RcsApplication.RCSApplicationStatus.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.RCSApplicationStatus.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();

                }


            }
            else if (IssueRefundsCollection)
            {
              
                var RcsApplication = db.RefundApplications.Include(x => x.Status).Include(x => x.RCSApplicationStatus).Include(x => x.RCSApplicationStatus.CCC).FirstOrDefault(x => x.Id == RefundAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Issue Refunds Collection").FirstOrDefault().Id;
                RefundUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.RCSApplicationStatus.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault();

                if (RefundUsers.Count() >0)
                {
                    AssignedToUser = AssigedToCCRR(RefundUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.RCSApplicationStatus.Id,
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CCCId = Convert.ToInt16(RcsApplication.RCSApplicationStatus.CCCId),
                        CCCName = RcsApplication.RCSApplicationStatus.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.RCSApplicationStatus.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();

                }



            }

            return AssignedToUser;
        }



        public int EHCRoundRobinCCC(bool AcknowlegeApplication, bool SubmitFigures, bool IssueRCC, bool BillingSection, bool Rates, bool BackOffice, bool CreditControl, bool Sundries, int RCSAppID, int DepartmentID, bool AcknowlegeRefund, bool IssueRefundsCollection, int RefundAppID)
        {
            //get all users for each department
            var applicationUserRoles = db.Roles.ToList();
            //DateTime.Compare(x.DateTimeValueColumn.Date, DateTime.Now.Date) <= 0
            //x => EntityFunctions.TruncateTime(x.DateTimeStart) == currentDate.Date
            //try
            //{
            //    var dates = new string[2];
            //    dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            //    dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            //    var startDate = DateTime.Parse(dates[0]).Date;
            //    var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            //    var oneDayTime = endDate - startDate;
            //    var NumOfDDAppPending = db.RoundRobinQueues.Where(x => x.IsActive == true && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();

            //    //DateTime DateFrom = DateTime.Now;
            //    //DateTime DateTo = DateTime.Now;
            //    //var startDate = DateTime.TryParseExact(DateFrom, "yyyyMMdd");
            //    //endDate = DateTime.ParseExact(dates[1], "yyyyMMdd").AddTicks(-1).AddDays(1);
            //    //var RoundRobingQueue1 = db.RoundRobinQueues.Where(x => DbFunctions.TruncateTime(x.CreatedDateTime) == DateTime.Now).ToList();

            //    //var NumOfDDAppPending = db.RoundRobinQueues.Where(x => x.IsActive == true && (DateFrom <= x.CreatedDateTime && DateTo >= x.CreatedDateTime)).ToList();

            //}
            //catch (Exception IO)
            //{

            //}
            var dates = new string[2];
            dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            var startDate = DateTime.Parse(dates[0]).Date;
            var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            var oneDayTime = endDate - startDate;
            var RoundRobingQueue = db.RoundRobinQueues.Where(x => x.IsActive == true && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();

            //var RoundRobingQueue = db.RoundRobinQueues.Where(x=>x.IsActive == true && DateTime.Compare(x.CreatedDateTime.Value.Date, DateTime.Now.Date) <= 0).ToList();
            //var RoundRobingQueue = db.RoundRobinQueues.Where(x=>x.CreatedDateTime.Value.Day == DateTime.Now.Date).ToList();
            var AccountsManagementUsers = (List<SystemIdentityUser>)null;
            var SubmitFiguresUsers = (List<SystemIdentityUser>)null;
            var IssueRCCUsers = (List<SystemIdentityUser>)null;
            var BillingUsers = (List<SystemIdentityUser>)null;
            var RateUsers = (List<SystemIdentityUser>)null;
            var BOUsers = (List<SystemIdentityUser>)null;
            var CreditControlUsers = (List<SystemIdentityUser>)null;
            var SundriesUsers = (List<SystemIdentityUser>)null;
            var RefundUsers = (List<SystemIdentityUser>)null;

            //DateTime dt = DateTime.Now;

            //var test = dt.Date;
            //get a count of applications for each user in each department
            var AssignedToUser = 0;

            var responsibilityTypes = db.ResponsibilityTypes.ToList();

            if (AcknowlegeApplication)
            {
                var RcsApplication = db.PropertyLeaseApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);


                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.ActiveDirectoryActive).FirstOrDefault().Value);


                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault();
                if (activeDirectoryOn != null)
                {

                    //AssignedToUser = AssigedToCCRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId.Id);
                    //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser && x.IsDeleted == false).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;




                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId.Id,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = activeDirectoryOn,
                        StatusId = StatusId
                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, activeDirectoryOn, ResponsibilityTypeId.Name);
                }
                else
                {
                    //var roundRobinLog = new RoundRobinLog
                    //{
                    //    RCSApplicationStatusId = RcsApplication.Id,
                    //    ResponsibilityTypeId = ResponsibilityTypeId.Id,
                    //    CCCId = Convert.ToInt16(RcsApplication.CCCId),
                    //    CCCName = RcsApplication.CCC.CCCName,
                    //    LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityTypeId.Name,
                    //    RoleID = AccountsManagement,
                    //    RoleName = "Acknowledge RCS Application",
                    //    IsActive = true,
                    //    IsDeleted = false

                    //};
                    //db.RoundRobinLogs.Add(roundRobinLog);
                    //db.SaveChanges();

                }


                // JK.20140724a - Custom profile information.

            }

            else if (SubmitFigures)
            {


                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Submit Figures").FirstOrDefault().Id;
                SubmitFiguresUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault();

                if (SubmitFiguresUsers.Count() > 0)
                {

                    AssignedToUser = AssigedToCCRR(SubmitFiguresUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityType.Id,
                        CCCId = Convert.ToInt16(RcsApplication.CCCId),
                        CCCName = RcsApplication.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();
                }
            }

            else if (IssueRCC)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Issue Certificate").FirstOrDefault().Id;
                IssueRCCUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault();


                if (IssueRCCUsers.Count() > 0)
                {

                    AssignedToUser = AssigedToCCRR(IssueRCCUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CCCId = Convert.ToInt16(RcsApplication.CCCId),
                        CCCName = RcsApplication.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();

                }
            }

            else if (BillingSection)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Billing").FirstOrDefault().Id;
                BillingUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(BillingUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId = DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
            }

            else if (Rates)
            {

                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Rates").FirstOrDefault().Id;
                RateUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(RateUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId = DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
            }


            else if (CreditControl)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Credit Control").FirstOrDefault().Id;
                CreditControlUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(CreditControlUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId = DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
            }
            else if (Sundries)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Sundry Account").FirstOrDefault().Id;
                SundriesUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault();
                AssignedToUser = AssigedToCCRR(SundriesUsers, RoundRobingQueue, ResponsibilityTypeId);
                var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                var statusList = db.Status.ToList();
                var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                var roundRobinQueue = new RoundRobinQueue
                {
                    DepartmentApprovalId = DepartmentID,
                    ResponsibilityTypeId = ResponsibilityTypeId,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = ClerkId,
                    StatusId = StatusId


                };
                db.RoundRobinQueues.Add(roundRobinQueue);
                db.SaveChanges();
                BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);

            }
            else if (AcknowlegeRefund)
            {
                var RcsApplication = db.RefundApplications.Include(x => x.Status).Include(x => x.RCSApplicationStatus).Include(x => x.RCSApplicationStatus.CCC).OrderByDescending(x => x.Id).FirstOrDefault(x => x.Id == RefundAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Acknowledge Refund Application").FirstOrDefault().Id;
                RefundUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.RCSApplicationStatus.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault();

                if (RefundUsers.Count() > 0)
                {
                    AssignedToUser = AssigedToCCRR(RefundUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.RCSApplicationStatus.Id,
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CCCId = Convert.ToInt16(RcsApplication.RCSApplicationStatus.CCCId),
                        CCCName = RcsApplication.RCSApplicationStatus.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.RCSApplicationStatus.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();

                }


            }
            else if (IssueRefundsCollection)
            {

                var RcsApplication = db.RefundApplications.Include(x => x.Status).Include(x => x.RCSApplicationStatus).Include(x => x.RCSApplicationStatus.CCC).FirstOrDefault(x => x.Id == RefundAppID);

                var RoleId = applicationUserRoles.Where(x => x.Name == "Issue Refunds Collection").FirstOrDefault().Id;
                RefundUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.RCSApplicationStatus.CCCId).ToList();
                var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;
                var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault();

                if (RefundUsers.Count() > 0)
                {
                    AssignedToUser = AssigedToCCRR(RefundUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                else
                {
                    var roundRobinLog = new RoundRobinLog
                    {
                        RCSApplicationStatusId = RcsApplication.RCSApplicationStatus.Id,
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CCCId = Convert.ToInt16(RcsApplication.RCSApplicationStatus.CCCId),
                        CCCName = RcsApplication.RCSApplicationStatus.CCC.CCCName,
                        LogEntry = "No user set up for CCC " + RcsApplication.RCSApplicationStatus.CCC.CCCName + " for role: " + ResponsibilityType.Name,
                        RoleID = RoleId,
                        RoleName = ResponsibilityType.Name,
                        IsActive = true,
                        IsDeleted = false



                    };
                    db.RoundRobinLogs.Add(roundRobinLog);
                    db.SaveChanges();

                }



            }

            return AssignedToUser;
        }

        public int RoundRobinRedistribution(bool AcknowlegeApplication, bool SubmitFigures, bool IssueRCC, bool BillingSection, bool Rates, bool BackOffice, bool CreditControl, bool Sundries, int RCSAppID, int DepartmentID, int RRQueueID, bool AcknowlegeRefund, bool IssueRefundsCollection)
        {
            //get all users for each department
            var applicationUserRoles = db.Roles.ToList();
            var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x=>x.Id== RRQueueID).FirstOrDefault();

            
            var dates = new string[2];
            dates[0] = DateTime.Today.AddTicks(1).ToString(CultureInfo.InvariantCulture);
            dates[1] = DateTime.Today.AddDays(1).AddTicks(-2).ToString(CultureInfo.InvariantCulture);
            var startDate = DateTime.Parse(dates[0]).Date;
            var endDate = DateTime.Parse(dates[1]).Date.AddDays(1).AddTicks(-1);
            var oneDayTime = endDate - startDate;
            var RoundRobingQueue = db.RoundRobinQueues.Where(x => x.IsActive == true && (startDate <= x.CreatedDateTime && endDate >= x.CreatedDateTime)).ToList();

            //var RoundRobingQueue = db.RoundRobinQueues.Where(x=>x.IsActive == true && DateTime.Compare(x.CreatedDateTime.Value.Date, DateTime.Now.Date) <= 0).ToList();
            //var RoundRobingQueue = db.RoundRobinQueues.Where(x=>x.CreatedDateTime.Value.Day == DateTime.Now.Date).ToList();
            var AccountsManagementUsers = (List<SystemIdentityUser>)null;
            var SubmitFiguresUsers = (List<SystemIdentityUser>)null;
            var IssueRCCUsers = (List<SystemIdentityUser>)null;
            var BillingUsers = (List<SystemIdentityUser>)null;
            var RateUsers = (List<SystemIdentityUser>)null;
            var BOUsers = (List<SystemIdentityUser>)null;
            var CreditControlUsers = (List<SystemIdentityUser>)null;
            var SundriesUsers = (List<SystemIdentityUser>)null;
            var Users = (List<SystemIdentityUser>)null;
            //DateTime dt = DateTime.Now;

            //var test = dt.Date;
            //get a count of applications for each user in each department
            var AssignedToUser = 0;

            var responsibilityTypes = db.ResponsibilityTypes.ToList();

            if (AcknowlegeApplication)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);
                var OldClerk = rrq.Clerk.SystemUserId;
                var AccountsManagement = applicationUserRoles.Where(x => x.Name == "Acknowledge RCS Application").FirstOrDefault().Id;
                AccountsManagementUsers = GetUsersInRole(AccountsManagement).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId !=OldClerk).ToList();
                if(AccountsManagementUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRCSApplication).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(AccountsManagementUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == RCSAppID && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime=DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();

                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                
                

            }

            else if (SubmitFigures)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);
                var OldClerk = rrq.Clerk.SystemUserId;
                var RoleId = applicationUserRoles.Where(x => x.Name == "Submit Figures").FirstOrDefault().Id;
                SubmitFiguresUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();
               

                if (SubmitFiguresUsers.Count() > 0)
                {

                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(SubmitFiguresUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == RCSAppID && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();

                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }

            }

            else if (IssueRCC)
            {
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);
                var OldClerk = rrq.Clerk.SystemUserId;
                var RoleId = applicationUserRoles.Where(x => x.Name == "Issue Certificate").FirstOrDefault().Id;
                IssueRCCUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();


                if (IssueRCCUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueCertificates).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(IssueRCCUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RCSApplicationStatusId == RCSAppID && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RCSApplicationStatusId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
             
            }
            else if (AcknowlegeRefund)
            {
                var RcsApplication = db.RefundApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var CCCForApp = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RcsApplication.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var RoleId = applicationUserRoles.Where(x => x.Name == "Acknowledge Refund Application").FirstOrDefault().Id;
                Users = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == CCCForApp.CCCId && x.SystemUserId != OldClerk).ToList();


                if (Users.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(Users, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RefundApplicationId == RcsApplication.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
                //var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RCSAppID);

                //var RoleId = applicationUserRoles.Where(x => x.Name == "Acknowledge Refund Application").FirstOrDefault().Id;
                //RefundUsers = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId).ToList();
                //var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AcknowledgeRefundApplication).FirstOrDefault().Id;
                //AssignedToUser = AssigedToCCRR(RefundUsers, RoundRobingQueue, ResponsibilityTypeId);
                //var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                //var statusList = db.Status.ToList();
                //var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;



                //var roundRobinQueue = new RoundRobinQueue
                //{
                //    DepartmentApprovalId = DepartmentID,
                //    ResponsibilityTypeId = ResponsibilityTypeId,
                //    ClerkId = ClerkId,
                //    StatusId = StatusId


                //};
                //db.RoundRobinQueues.Add(roundRobinQueue);
                //db.SaveChanges();

            }
            else if (IssueRefundsCollection)
            {
                var RcsApplication = db.RefundApplications.Include(x => x.Status).FirstOrDefault(x => x.Id == RCSAppID);
                var CCCForApp = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == RcsApplication.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var RoleId = applicationUserRoles.Where(x => x.Name == "Issue Refunds Collection").FirstOrDefault().Id;
                Users = GetUsersInRole(RoleId).Where(x => x.RoundRobinIsActive == true && x.CCCId == CCCForApp.CCCId && x.SystemUserId != OldClerk).ToList();

             
               

                if (Users.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.IssueRefundCollection).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(Users, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.RefundApplicationId == RcsApplication.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        RefundApplicationId = RcsApplication.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
            }

            else if (BillingSection)
            {
                var Dep = db.DepartmentsApprovals.Include(x => x.Status).FirstOrDefault(x => x.Id == DepartmentID);
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == Dep.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var Billings = applicationUserRoles.Where(x => x.Name == "Billing").FirstOrDefault().Id;
                BillingUsers = GetUsersInRole(Billings).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();
                if (BillingUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Billing).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(BillingUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == Dep.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        DepartmentApprovalId = Dep.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }


            }

            else if (Rates)
            {
                var Dep = db.DepartmentsApprovals.Include(x => x.Status).FirstOrDefault(x => x.Id == DepartmentID);
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == Dep.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var RateDepts = applicationUserRoles.Where(x => x.Name == "Rates").FirstOrDefault().Id;
                RateUsers = GetUsersInRole(RateDepts).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();
                if (RateUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.Rates).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(RateUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == Dep.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        DepartmentApprovalId = Dep.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }


            }


            else if (CreditControl)
            {
                var Dep = db.DepartmentsApprovals.Include(x => x.Status).FirstOrDefault(x => x.Id == DepartmentID);
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == Dep.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var creditcontrols = applicationUserRoles.Where(x => x.Name == "Credit Control").FirstOrDefault().Id;
                CreditControlUsers = GetUsersInRole(creditcontrols).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();
                if (CreditControlUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.CreditControl).FirstOrDefault();
                    AssignedToUser = AssigedToCCRedistributionRR(CreditControlUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == Dep.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        DepartmentApprovalId = Dep.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }
            }
            else if (Sundries)
            {
                var Dep = db.DepartmentsApprovals.Include(x => x.Status).FirstOrDefault(x => x.Id == DepartmentID);
                var RcsApplication = db.RCSApplicationStatus.Include(x => x.Status).Include(x => x.CCC).FirstOrDefault(x => x.Id == Dep.RCSApplicationStatusId);
                var OldClerk = rrq.Clerk.SystemUserId;
                var sundries = applicationUserRoles.Where(x => x.Name == "Sundry Account").FirstOrDefault().Id;
                SundriesUsers = GetUsersInRole(sundries).Where(x => x.RoundRobinIsActive == true && x.CCCId == RcsApplication.CCCId && x.SystemUserId != OldClerk).ToList();
                if (SundriesUsers.Count() > 0)
                {
                    var ResponsibilityTypeId = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault().Id;
                    var ResponsibilityType = responsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SundryAccount).FirstOrDefault();

                    AssignedToUser = AssigedToCCRedistributionRR(SundriesUsers, RoundRobingQueue, ResponsibilityTypeId);
                    var ClerkId = db.Customers.Where(x => x.SystemUserId == AssignedToUser).FirstOrDefault().Id;
                    var statusList = db.Status.ToList();
                    var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                    var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser).Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.DepartmentApprovalId == Dep.Id && x.StatusId == StatusId).ToList();
                    foreach (var item in rrqList)
                    {
                        item.StatusId = statusList.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var roundRobinQueue = new RoundRobinQueue
                    {
                        DepartmentApprovalId = Dep.Id,
                        ResponsibilityTypeId = ResponsibilityTypeId,
                        CurrentTaskDateTime = DateTime.Now,
                        ClerkId = ClerkId,
                        StatusId = StatusId


                    };
                    db.RoundRobinQueues.Add(roundRobinQueue);
                    db.SaveChanges();
                    BackOfficeReAllocatedNotification(RCSAppID, ClerkId, ResponsibilityType.Name);
                }

            }

            return AssignedToUser;
        }

        public int RoundRobin(bool AccountsManagementDep , bool LegalSection, bool CreditControlSection , bool BillingSection, bool EndowmentSection, bool BackOffice)
        {
            //get all users for each department
            var applicationUserRoles = db.Roles.ToList();
            var DepartmentApprovals = db.DepartmentsApprovals.ToList();

            var AccountsManagementUsers = (List<SystemIdentityUser>)null;
            var LegalUsers = (List<SystemIdentityUser>)null;
            var CreditControlUsers = (List<SystemIdentityUser>)null;
            var BillingUsers = (List<SystemIdentityUser>)null;
            var EndowmentUsers = (List<SystemIdentityUser>)null;
            var BOUsers = (List<SystemIdentityUser>)null;

            //get a count of applications for each user in each department
            var AssignedToUser = 0;

            if (AccountsManagementDep)
            {
                var AccountsManagement = applicationUserRoles.Where(x => x.Name == "Accounts Management").FirstOrDefault().Id;
                AccountsManagementUsers = GetUsersInRole(AccountsManagement).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(AccountsManagementUsers, DepartmentApprovals);
            }

            else if (LegalSection)
            {
                var Legal = applicationUserRoles.Where(x => x.Name == "Legal").FirstOrDefault().Id;
                LegalUsers = GetUsersInRole(Legal).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(LegalUsers, DepartmentApprovals);
            }

            else if (CreditControlSection)
            {
                var CreditControl = applicationUserRoles.Where(x => x.Name == "Credit Control").FirstOrDefault().Id;
                CreditControlUsers = GetUsersInRole(CreditControl).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(CreditControlUsers, DepartmentApprovals);
            }

            else if (BillingSection)
            {
                var Billing = applicationUserRoles.Where(x => x.Name == "Billing").FirstOrDefault().Id;
                BillingUsers = GetUsersInRole(Billing).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(BillingUsers, DepartmentApprovals);
            }

            else if (EndowmentSection)
            {
                var Endowment = applicationUserRoles.Where(x => x.Name == "Endowment").FirstOrDefault().Id;
                EndowmentUsers = GetUsersInRole(Endowment).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(EndowmentUsers, DepartmentApprovals);
            }

            else if (BackOffice)
            {
                var BO = applicationUserRoles.Where(x => x.Name == "Clerks").FirstOrDefault().Id;
                BOUsers = GetUsersInRole(BO).Where(x => x.RoundRobinIsActive != false).ToList();
                AssignedToUser = AssigedToRR(BOUsers, DepartmentApprovals);
            }

            return AssignedToUser;
        }

        #region Assign RR Active Users
        public ActionResult RRActiveUsers()
        {
            Initialise();
            List<SystemIdentityUser> Users = (List<SystemIdentityUser>)null;
            String curremtUserDepartment = db.ApplicationEntities.Find(SystemUser.DepartmentId)?.Key;
            Users = UserManager.Users.Include(c => c.SystemUser.Department).Where(x => x.isInternalUser && !x.isDeleted).ToList();
            if (!String.IsNullOrEmpty(curremtUserDepartment))
            {
                Users = Users.Where(x => x.SystemUser.DepartmentId != null).ToList();
                Users = Users.Where(c => c.SystemUser.Department.Key.Equals(curremtUserDepartment)).ToList();
            }
            return View(Users);
        }


        #region Assign RR Active Users Edit 
        //
        [DecryptParameter]
        public ActionResult EditRRUser(string id)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    if (id == null) throw new Exception("Invalid User");

                    var Users = UserManager.Users.Where(x => x.Id == id);
                    ViewBag.Name = Users.FirstOrDefault().UserName;
                    ViewBag.Isactive = Users.FirstOrDefault().RoundRobinIsActive;

                    var Message = TempData["RoundRobinRedistributionTitle"];
                    var Title = TempData["RoundRobinRedistribution"];
                    if (Message != null && Title != null)
                    {
                        ViewBag.MessageTitle = TempData["RoundRobinRedistributionTitle"].ToString();
                        ViewBag.Message = TempData["RoundRobinRedistribution"].ToString();
                    }
                    return View(Users);
                }
                catch
                {
                    return View("_Error");
                }
            }
        }
        #endregion

        #region Assign RR Active Users Edit POST
        [HttpPost]
        [DecryptParameter]
        public ActionResult EditRRUser(string Id, bool rrIsActive,bool ReAllocateCases)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var user = (SystemIdentityUser)UserManager.FindById(Id);

                    if(ReAllocateCases == true)
                    {
                        var Keys = db.Status;
                        int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                        var SysUserID = user.SystemUserId;
                        var Clerk = db.Customers.Where(x => x.SystemUserId == SysUserID).FirstOrDefault();
                        var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.ResponsibilityType).Include(x => x.Clerk.SystemUser).Include(x=>x.RefundApplication).Include(x => x.RCSApplicationStatus).Where(x => x.ClerkId == Clerk.Id && x.StatusId == SubmittedId).ToList();
                        AesCrypto AES = new AesCrypto();
                        var q = AES.Encrypt("id=" + Id);
                        foreach (var item in rrqList)
                        {
                         
                            DepartmentsApprovalsController DA = new DepartmentsApprovalsController();
                            CaptureController c = new CaptureController();
                            switch (item.ResponsibilityType.Key)
                            {
                                case (ResponsibilityTypeKeys.AcknowledgeRCSApplication):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(true, false, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);
                                        //var SystUserId = c.RoundRobinCCC(true, false, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, false, false, 0);
                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;
                                           
                                        
                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                           
                                        }
                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.SubmitFigures):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);
                                  
                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.IssueCertificates):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, true, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();
                                            
                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.Billing):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, true, false, false, false, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.CreditControl):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, true, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.SundryAccount):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, true, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                                case (ResponsibilityTypeKeys.Rates):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, true, false, false, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.AcknowledgeRefundApplication):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, Convert.ToInt16(item.RefundApplicationId), 0, item.Id, true, false);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId),Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }

                                case (ResponsibilityTypeKeys.IssueRefundCollection):
                                    {
                                        var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, Convert.ToInt16(item.RefundApplicationId), 0, item.Id, false, true);

                                        if (SystUserId == 0)
                                        {
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                            var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                            TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                            TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                            return RedirectToAction("EditRRUser", new { q = q });
                                        }
                                        else
                                        {
                                            var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                            var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                        }

                                        item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                        db.Entry(item).State = EntityState.Modified;
                                        db.SaveChanges();
                                        break;
                                    }
                            }
                        }
                    }
                    else

                    // Update it with the values from the view model
                    user.RoundRobinIsActive = rrIsActive;
                    UserManager.Update(user);
                    var workallocation = db.UserWorkAllocations.Where(r => r.SystemUserId == user.SystemUserId).ToList();
                    foreach (var area in workallocation)
                    {
                        area.RRActive = rrIsActive;
                        db.Entry(area).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    return RedirectToAction("RRActiveUsers");
                }
                catch
                {
                    return View("_Error");
                }
            }
        }

        #endregion

        #endregion
        #endregion


        #region Department Approvals
        public static int AssigedToRR(List<SystemIdentityUser> Users , List<DepartmentsApproval> DepartmentsApprovals)
        {
            var AssignedToPerson = 0;

            if (Users != null)
            {
                List<int> Approvals = new List<int>();
                List<int> User = new List<int>();
                eServicesDbContext db = new eServicesDbContext();


                foreach (var item in Users)
                {
                    var custid = db.Customers.Where(x => x.SystemUserId == item.SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var depApprovals = DepartmentsApprovals.Where(x => x.AssignedToCustomerId == custid.Id).Count();
                    Approvals.Add(depApprovals);
                    User.Add(item.SystemUserId);
                }

                int min = Approvals[0];
                int minIndex = 0;

                for (int i = 1; i < Approvals.Count; ++i)
                {
                    if (Approvals[i] < min)
                    {
                        min = Approvals[i];
                        minIndex = i;
                    }
                }

                var lowestIndex = minIndex;
                AssignedToPerson = User[lowestIndex];
            }


            return AssignedToPerson;
        }

        //Sashen Round robin
        public static int AssigedToCCRR(List<SystemIdentityUser> Users, List<RoundRobinQueue> roundRobinQueues, int ResponsibilityType)
        {
            eServicesDbContext db = new eServicesDbContext();
            var AssignedToPerson = 0;
            var statusList = db.Status.ToList();
            var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (Users != null)
            {
                List<int> Approvals = new List<int>();
                List<int> User = new List<int>();
                


                foreach (var item in Users)
                {
                    var custid = db.Customers.Where(x => x.SystemUserId == item.SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                

                    var depApprovals = roundRobinQueues.Where(x => x.ClerkId == custid.Id && x.ResponsibilityTypeId == ResponsibilityType && x.StatusId ==StatusId).Count();
                    Approvals.Add(depApprovals);
                    User.Add(item.SystemUserId);
                }

                int min = Approvals[0];
                int minIndex = 0;

                for (int i = 1; i < Approvals.Count; ++i)
                {
                    if (Approvals[i] < min)
                    {
                        min = Approvals[i];
                        minIndex = i;
                    }
                }

                var lowestIndex = minIndex;
                AssignedToPerson = User[lowestIndex];
            }


            return AssignedToPerson;
        }
        public static int AssigedToCCRedistributionRR(List<SystemIdentityUser> Users, List<RoundRobinQueue> roundRobinQueues, int ResponsibilityType)
        {
            eServicesDbContext db = new eServicesDbContext();
            var AssignedToPerson = 0;
            var statusList = db.Status.ToList();
            var StatusId = statusList.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
            if (Users != null)
            {
                List<int> Approvals = new List<int>();
                List<int> User = new List<int>();



                foreach (var item in Users)
                {
                    var custid = db.Customers.Where(x => x.SystemUserId == item.SystemUserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();


                    var depApprovals = roundRobinQueues.Where(x => x.ClerkId == custid.Id && x.ResponsibilityTypeId == ResponsibilityType && x.StatusId == StatusId).Count();
                    Approvals.Add(depApprovals);
                    User.Add(item.SystemUserId);
                }

                int min = Approvals[0];
                int minIndex = 0;

                for (int i = 1; i < Approvals.Count; ++i)
                {
                    if (Approvals[i] < min)
                    {
                        min = Approvals[i];
                        minIndex = i;
                    }
                }

                var lowestIndex = minIndex;
                AssignedToPerson = User[lowestIndex];
            }


            return AssignedToPerson;
        }

        #endregion

        #region GetUsersInRole
        public IEnumerable<SystemIdentityUser> GetUsersInRole(string roleId)
            {
                return UserManager.Users.Where(o => o.Roles.Any(s => s.RoleId == roleId)).ToList();
            }
          #endregion

        private void WriteLog(string message)
        {
            try
            {
                using (var logDb = new eServicesDbContext())
                {
                    logDb.Logs.Add(new Log
                    {
                        LogTypeId = 1,
                        LogEntry = message,
                        ReferenceId = 0,
                        ReferenceTypeId = 1,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false
                    });
                    logDb.SaveChanges();
                }
            }
            catch { }
        }

        #region PDF Generation
        public ActionResult PDFExample(int id)

            {
                var root = Server.MapPath("~/PDFFiles/");
                string pdfname = "Pdfexm.pdf";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                var parameters = 80;

                var actionPDF = new Rotativa.ActionAsPdf("PDFExample1", new { parameters = parameters })
                {
                    FileName = "PDFExample.pdf",
                    SaveOnServerPath = path,
                    PageSize = Rotativa.Options.Size.A4,
                    PageOrientation = Rotativa.Options.Orientation.Portrait,
                    PageMargins = { Left = 10, Right = 6, Top = 10 }

                };

                byte[] applicationPDFData = actionPDF.BuildFile(ControllerContext);
                var fullpathtofile = path;
                var mimetype = "application/pdf";
                var filecontents = System.IO.File.ReadAllBytes(fullpathtofile);

                return new FileContentResult(filecontents, mimetype);
            }
        #region MyRegion

        /* @parameters
         * formName -Name of your Application Form (eThekwini Name)
         * fileName -Name of the File
         * urlDomain- The IP address of the Domain for Dev it is your Local Host with Port Number
         * controllerName- Name of the Controller your View Method is In
         * viewName - Name of the View your Form is on
         * pdfParameter -value of the ID you send to your View
         */
        public ActionResult GenerateViewToPDF(string formName, string fileName, string urlDomain, string controllerName, string viewName)
        {
            try
            {

                var root = Server.MapPath("~/PDFFiles/");
                string pdfname = "Cloud_Based_Form_" + formName + ".pdf";
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);

                //var urlDomain = PGDomain.Value;
                //urlDomain = "localhost:3456";
                //controllerName = "CloudBasedForms";
                //viewName = "OnlineApplications";
                //url="localhost:3456/CloudBasedForms/OnlineApplications"

                var url = string.Format("{0}/{1}/{2}", urlDomain, controllerName, viewName);
                var actionPDF = new Rotativa.UrlAsPdf(url)
                {
                    FileName = fileName,
                    //SaveOnServerPath = path, // JK.20200404a - Deprecated, save bytes as below.
                    PageSize = Rotativa.Options.Size.A4,
                    PageOrientation = Rotativa.Options.Orientation.Portrait,
                    PageMargins = { Left = 10, Right = 6, Top = 10 }
                };

                byte[] applicationPDFData = actionPDF.BuildFile(ControllerContext);
                // JK.20200404a - SaveOnServerPath is deprecated, have to save bytes manually.
                System.IO.File.WriteAllBytes(path, applicationPDFData);

                var fullpathtofile = path;
                var mimetype = "application/pdf";
                var filecontents = System.IO.File.ReadAllBytes(fullpathtofile);

                return new FileContentResult(filecontents, mimetype);
            }
            catch (Exception x)
            {
                EventLogHelper.LogSystemError(x.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                db.Logs.Add(new Log()
                {
                    LogTypeId = 1,
                    LogEntry = x.ToString(),
                    ReferenceId = 0,
                    ReferenceTypeId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                });
                db.SaveChanges();
                //throw x;
            }

            return null;
        }

        #endregion
        //Get
        public ActionResult PDFExample1()
            {
                return View();
            }

        //Post Method
        public ActionResult PDFExample1(string parameters1, int parametr2, double parameter3, bool prameter4, File doc1, File Doc2)
        {
            //db context

            eServicesDbContext db = new eServicesDbContext();
            //Save parameters to DB Table
            Customer newCust = new Customer();
            newCust.FirstName = parameters1;
            db.Customers.Add(newCust);
            db.SaveChanges();
            //Create PDF of Form
            GenerateViewToPDF(parameters1, parametr2.ToString(), parameter3.ToString(), parameter3.ToString(), parameter3.ToString());

            //pdfLink urlDomain/PDFFiles/FileName.pdf

            //Upload Documents to Sharepoint       

            //Return Sharepoint Links
            //link for Form.pdf
            //Link for Doxc1
            //Link for Doc 2

            //Cesar Email

            Email SendMail = new Email();
            string emailbody = "Hi Department Man /n" +
                "Please See Attached Sharepoint Links for:/n" +
                newCust.FullName + " CloudBasedForms Online Application /n" +
                "And supporting Documents";
            SendMail.GenerateEmail(newCust.EmailAddress, "Deparment Name- New Online Application Submission",
                                    emailbody,
                                    newCust.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, newCust.FullName);

            //Congrats work s done Redirect Custoemr To Complete Page
            return View();
        }
        #endregion
        
        #region API Test
        public ActionResult APITest()
        {
            string key = "";
            string secret = "";
            //var suprimaConsumerCheck = TokenApi.GenerateToken(key, secret);
     
            return View();
        }

        #endregion

    }
}
