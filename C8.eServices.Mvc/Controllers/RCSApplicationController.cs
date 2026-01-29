namespace C8.eServices.Mvc.Controllers
{
    using C8.eServices.Mvc.DataAccessLayer;
    using C8.eServices.Mvc.Helpers;
    using C8.eServices.Mvc.Keys;
    using C8.eServices.Mvc.Models;
    using C8.eServices.Mvc.ViewModels;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using C8.eServices.Mvc.ApiServices;
    using Helpers;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Defines the <see cref="RCSApplicationController" />.
    /// </summary>
    public class RCSApplicationController : Controller
    {
        /// <summary>
        /// Defines the db.
        /// </summary>
        private eServicesDbContext db = new eServicesDbContext();

        /// <summary>
        /// Defines the encp.
        /// </summary>
        public string encp = "spgencpassp";

        /// <summary>
        /// Gets the UserManager.
        /// </summary>
        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RCSApplicationController"/> class.
        /// </summary>
        public RCSApplicationController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RCSApplicationController"/> class.
        /// </summary>
        /// <param name="userManager">The userManager<see cref="UserManager{SystemIdentityUser}"/>.</param>
        public RCSApplicationController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RCSApplicationController"/> class.
        /// </summary>
        /// <param name="db">The db<see cref="eServicesDbContext"/>.</param>
        public RCSApplicationController(eServicesDbContext db)
        {
            UserManager =
            new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(db));
        }

        /// <summary>
        /// Gets or sets the IdentityManager.
        /// </summary>
        public IdentityManager IdentityManager { get; set; }

        /// <summary>
        /// Gets or sets the SystemUser.
        /// </summary>
        public SystemUser SystemUser { get; set; }

        /// <summary>
        /// Gets or sets the Customer.
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the Entity.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Gets or sets the Agent.
        /// </summary>
        public Agent Agent { get; set; }

        /// <summary>
        /// Gets or sets the CustomerId.
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// The Initialise.
        /// </summary>
        private void Initialise()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    IdentityManager = new IdentityManager(context);

                    if (User != null && User.Identity.IsAuthenticated)
                    {
                        IdentityManager.CurrentUser(User);
                        SystemUser = IdentityManager.CurrentUser(User);
                    }

                    if (SystemUser != null)
                    {
                        Customer =
                            context.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                                .Include(o => o.CustomerType)
                                .Include(o => o.Country)
                                .Include(o => o.IdentificationType)
                                .Include(o => o.TitleType)
                                .FirstOrDefault();

                        if (Customer != null)
                        {
                            Entity =
                                context.Entities.Where(o => o.CustomerId == Customer.Id)
                                    .Include(o => o.EntityType)
                                    .FirstOrDefault();
                            CustomerId = Customer.Id;
                        }

                    }

                    if (Customer != null)
                    {
                        Agent = context.Agents.FirstOrDefault(o => o.CustomerId == Customer.Id);
                    }
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }
        #region Return back url
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
                if (values[13] != null && values[15] != null)
                {
                    int RCSAppID = Convert.ToInt16(values[15]);
                    RCSApplicationStatus RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == RCSAppID).FirstOrDefault();
                    Appref = RCSApplication.ApplicationReferenceNumber;
                    if (RCSApplication != null)
                    {
                        //Save to request table
                        AssessmentPaymentTransaction assessmentPaymentTransaction = new AssessmentPaymentTransaction();
                        assessmentPaymentTransaction.ApplicationReference = RCSApplication.ApplicationReferenceNumber;
                        assessmentPaymentTransaction.RCSApplicationStatusId = RCSApplication.Id;
                        assessmentPaymentTransaction.Descritpion = "Rates Clearance Figures Payment";
                        assessmentPaymentTransaction.PaymentMethod = values[8];
                        assessmentPaymentTransaction.ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
                        assessmentPaymentTransaction.Reference = values[2];
                        //save receipt num
                        assessmentPaymentTransaction.Status = values[13];
                        assessmentPaymentTransaction.StatusId = db.Status.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                        if (values[17] != null && values[17] != "")
                        {
                            assessmentPaymentTransaction.AssessmentPaymentRequestId = Convert.ToInt16(values[17]);

                        }
                        assessmentPaymentTransaction.Amount = Convert.ToDecimal(values[7]);
                        assessmentPaymentTransaction.MerchantReference = values[4];
                        //assessmentPaymentTransaction.SmsNotify = values[13];
                        //assessmentPaymentTransaction.EmailNotify = values[13];
                        assessmentPaymentTransaction.VoteNumber = values[2];
                        db.AssessmentPaymentTransactions.Add(assessmentPaymentTransaction);
                        db.SaveChanges();


                        decimal AmountPaid = Convert.ToDecimal(values[7]);
                        RCSApplication.AssessmentFigureAmountPaid = RCSApplication.AssessmentFigureAmountPaid + AmountPaid;
                        RCSApplication.AssessmentFigureAmountOutstanding = RCSApplication.AssessmentFigureAmountOutstanding - AmountPaid;
                        RCSApplication.FiguresPaymentOnline = true;

                        if (RCSApplication.AssessmentFigureAmountPaid == RCSApplication.AssessmentFigureTotalAmount || RCSApplication.AssessmentFigureAmountPaid > RCSApplication.AssessmentFigureTotalAmount)
                        {
                            if (values[13] == "Success" || values[13] == "Test")
                            {
                                if (RCSApplication.StatusId == statuses.FirstOrDefault(o => o.Key == StatusKeys.ViewAssessmentFigure).Id)
                                {
                                    RCSApplication.StatusId = statuses.FirstOrDefault(o => o.Key == StatusKeys.PendingAssessmentFeePaymentValidation).Id;

                                    db.Entry(RCSApplication).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                Session["ApplicationRefNo"] = Appref;

                                Session["ReceiptValues"] = values;


                                return RedirectToAction("Index", "RCSApplication");
                            }
                        }
                        else if (RCSApplication.AssessmentFigureAmountPaid < RCSApplication.AssessmentFigureTotalAmount)
                        {

                            if (values[13] == "Success" || values[13] == "Test")
                            {
                                if (RCSApplication.StatusId == statuses.FirstOrDefault(o => o.Key == StatusKeys.ViewAssessmentFigure).Id)
                                {

                                    db.Entry(RCSApplication).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                Session["ApplicationRefNo"] = Appref;

                                Session["ReceiptValues"] = values;

                                return RedirectToAction("Index", "RCSApplication");
                            }

                        }

                        //Start of partial payments
                        //Update outstanding amount

                        //End of partial payments

                        //   if(RCSApplication.StatusId == statuses.FirstOrDefault(o => o.Key == StatusKeys.PendingDocumentsApproval).Id)
                    }
                }
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
            return View();
        }
        #endregion





        #region View Assessment Figure
        [DecryptParameter]
        public ActionResult ViewAssessmentFigure(int id, DocumentsViewModel documentsViewModel)
        {
            Initialise();
            var rcsapp = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).Include(x => x.TransferInformation).FirstOrDefault();
            var userID = rcsapp.ClerkId;
            decimal OutstandingAmount = rcsapp.AssessmentFigureAmountOutstanding;
            var documentCheckLists = new List<DocumentCheckList>();
            var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

            var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCUploadAssessment).FirstOrDefault();
            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();



            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            string pgMerchantId = "pg_crm_app_rcs";
            string voteNumber = rcsapp.TransferInformation.RatesNumber;
            voteNumber = "RCS " + voteNumber;
            string pgMerchantReference = rcsapp.ApplicationReferenceNumber;
            string pgMerchantDescription = "Rates Clearance Figures Payment";
            var ApplicationFeeAmount = documentsViewModel.Amount;

            string Amount = rcsapp.amount.ToString();


            string pgEmail = null;
            string pgMobile = null;
            if (!string.IsNullOrEmpty(rcsapp.Customer.EmailAddress))
            {
                pgEmail = rcsapp.Customer.EmailAddress;
            }

            if (!string.IsNullOrEmpty(rcsapp.Customer.CellPhoneNumber))
            {
                pgMobile = rcsapp.Customer.CellPhoneNumber;
            }
            //pgMobile = "0846666435";



            //string customerFirstName = "Sashen";
            string customerFirstName = rcsapp.Customer.FirstName;
            //string customerLastName = "Moodley";
            string customerLastName = rcsapp.Customer.LastName;
            string returnUrl = Request.Url.AbsoluteUri;
            //returnUrl = "http://localhost:3450/Capture/Capture";
            string adhocRef1 = Convert.ToString(rcsapp.Id);

            string adhocRef2 = rcsapp.Id.ToString();
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
            var dvm = new DocumentsViewModel
            {
                CustomerId = userID,
                Amount = OutstandingAmount,
                ApplicationId = (int)application.Id,
                Application = application,
                RcsApplicationId = id,
                ReferenceTypeId = (int)referenceType.Id,
                ReferenceType = referenceType,
                ReferenceId = Customer.Id,
                IsUploadView = true,
                Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList(),
                ReturnUrl = baseFormat

            };
            dvm.DocumentCheckLists = documentCheckLists;



            foreach (var customerDocument in dvm.Documents)
            {
                //if (SystemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(SystemUserId, customerDocument, ViewCodeKeys.UpdateCustomerDocuments))
                //{
                //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                //    return RedirectToAction("Index", "Error");
                //}
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
                //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, Customer.Id, Customer.Id, application.Id, id))));
            }
            dvm.OnlinePaymentHistory = db.AssessmentPaymentTransactions.Where(x => x.RCSApplicationStatusId == id && x.IsActive == true).ToList();
            dvm.RCSApplicationStatus = rcsapp;
            return View(dvm);
        }
        #endregion
        #region View Assessment Figure Post
        [DecryptParameter]
        [HttpPost]
        public ActionResult ViewAssessmentFigure(DocumentsViewModel documentsViewModel)
        {
            Initialise();

            decimal PaymentAmount = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    PaymentAmount = documentsViewModel.Amount;


                    int id = documentsViewModel.RcsApplicationId;
                    var rcsapp = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).Include(x => x.TransferInformation).FirstOrDefault();
                    var userID = rcsapp.ClerkId;



                    var documentCheckLists = new List<DocumentCheckList>();
                    var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

                    documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCUploadAssessment).FirstOrDefault();
                    var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();



                    var ratesRebateProperty = new RatesRebateProperty();
                    var incentivePolicyProperty = new IncentivePolicyProperty();

                    string pgMerchantId = "pg_crm_app_rcs";
                    string voteNumber = rcsapp.TransferInformation.RatesNumber;
                    voteNumber = "RCS " + voteNumber;
                    string pgMerchantReference = rcsapp.ApplicationReferenceNumber;
                    string pgMerchantDescription = "RCS Assessment Payment";
                    var ApplicationFeeAmount = documentsViewModel.Amount;

                    string Amount = Convert.ToString(documentsViewModel.Amount);


                    string pgEmail = null;
                    string pgMobile = null;
                    if (!string.IsNullOrEmpty(rcsapp.Customer.EmailAddress))
                    {
                        pgEmail = rcsapp.Customer.EmailAddress;
                    }

                    if (!string.IsNullOrEmpty(rcsapp.Customer.CellPhoneNumber))
                    {
                        pgMobile = rcsapp.Customer.CellPhoneNumber;
                    }
                    //pgMobile = "0846666435";



                    //string customerFirstName = "Sashen";
                    string customerFirstName = rcsapp.Customer.FirstName;
                    //string customerLastName = "Moodley";
                    string customerLastName = rcsapp.Customer.LastName;
                    string returnUrl = Request.Url.AbsoluteUri;
                    //returnUrl = "http://localhost:3450/Capture/Capture";

                    //Save to request table
                    AssessmentPaymentRequest assessmentPaymentRequest = new AssessmentPaymentRequest();
                    assessmentPaymentRequest.ApplicationReference = rcsapp.ApplicationReferenceNumber;
                    assessmentPaymentRequest.RCSApplicationStatusId = id;
                    assessmentPaymentRequest.Descritpion = "RCS Assessment Payment";
                    assessmentPaymentRequest.Amount = documentsViewModel.Amount;
                    assessmentPaymentRequest.MerchantReference = pgMerchantReference;
                    assessmentPaymentRequest.SmsNotify = pgMobile;
                    assessmentPaymentRequest.EmailNotify = pgEmail;
                    assessmentPaymentRequest.VoteNumber = voteNumber;
                    db.AssessmentPaymentRequests.Add(assessmentPaymentRequest);
                    db.SaveChanges();


                    string adhocRef1 = Convert.ToString(rcsapp.Id);

                    string adhocRef2 = rcsapp.Id.ToString();
                    string adhocRef3 = Convert.ToString(assessmentPaymentRequest.Id);
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
                    return Redirect(baseFormat);
                }
                catch (Exception IO)
                {
                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

            }

            if (!ModelState.IsValid)
            {

                int id = documentsViewModel.RcsApplicationId;
                var rcsapp = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(x => x.Customer).Include(x => x.TransferInformation).FirstOrDefault();
                var userID = rcsapp.ClerkId;



                var documentCheckLists = new List<DocumentCheckList>();
                var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCAssessmentFigure);

                documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

                var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCUploadAssessment).FirstOrDefault();
                var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();



                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();

                string pgMerchantId = "pg_crm_app_rcs";
                string voteNumber = rcsapp.TransferInformation.RatesNumber;
                voteNumber = "RCS " + voteNumber;
                string pgMerchantReference = rcsapp.ApplicationReferenceNumber;
                string pgMerchantDescription = "RCS Assessment Payment";
                var ApplicationFeeAmount = documentsViewModel.Amount;

                string Amount = Convert.ToString(documentsViewModel.Amount);


                string pgEmail = null;
                string pgMobile = null;
                if (!string.IsNullOrEmpty(rcsapp.Customer.EmailAddress))
                {
                    pgEmail = rcsapp.Customer.EmailAddress;
                }

                if (!string.IsNullOrEmpty(rcsapp.Customer.CellPhoneNumber))
                {
                    pgMobile = rcsapp.Customer.CellPhoneNumber;
                }
                //pgMobile = "0846666435";



                //string customerFirstName = "Sashen";
                string customerFirstName = rcsapp.Customer.FirstName;
                //string customerLastName = "Moodley";
                string customerLastName = rcsapp.Customer.LastName;
                string returnUrl = Request.Url.AbsoluteUri;
                //returnUrl = "http://localhost:3450/Capture/Capture";
                string adhocRef1 = Convert.ToString(rcsapp.Id);

                string adhocRef2 = rcsapp.Id.ToString();
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
                var dvm = new DocumentsViewModel
                {
                    CustomerId = userID,
                    ApplicationId = (int)application.Id,
                    Application = application,
                    RcsApplicationId = id,
                    ReferenceTypeId = (int)referenceType.Id,
                    ReferenceType = referenceType,
                    ReferenceId = Customer.Id,
                    IsUploadView = true,
                    Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList(),
                    ReturnUrl = baseFormat

                };
                dvm.DocumentCheckLists = documentCheckLists;



                foreach (var customerDocument in dvm.Documents)
                {
                    //if (SystemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(SystemUserId, customerDocument, ViewCodeKeys.UpdateCustomerDocuments))
                    //{
                    //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                    //    return RedirectToAction("Index", "Error");
                    //}
                    if (customerDocument.File != null)
                        customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                    var docCheckList =
                         db.DocumentCheckLists.Include(d => d.DocumentType)
                             .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                    customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                        customerDocument.DocumentName);
                    //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
                    //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
                    customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
                }

                foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
                {

                    documentCheckList.DataList = new List<string>();
                    documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, Customer.Id, Customer.Id, application.Id, id))));
                }

                return View(dvm);
            }

            //ViewBag.Error = error;
            //return View(model);








            return View();
        }
        #endregion
        #region View RCC Certificate
        [DecryptParameter]
        public ActionResult ViewRCCCertificate(int id)
        {
            Initialise();

            var rcsapp = db.RCSApplicationStatus.Include(x => x.TransferInformation).Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
            var userID = rcsapp.ClerkId;

            //try
            //{


            //RefundApplication refundApplication = new RefundApplication();
            //refundApplication.StatusId = db.Status.Where(x => x.Key == StatusKeys.RefundRequestAvailable).FirstOrDefault().Id;
            //refundApplication.CustomerId = rcsapp.CustomerId;
            //refundApplication.CustomerId = rcsapp.CustomerId;
            //    refundApplication.IsActive = true;
            //    refundApplication.IsDeleted = false;
            //    refundApplication.IsLocked = false;
            //int limiter = 0;
            //var AppSettings = db.AppSettings.ToList();
            //AppSetting query = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSRefundSequence);
            //var SeqLimit = AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSRefundSequenceLimit);
            //var BatchCounter = query.Value;
            //limiter = Convert.ToInt16(SeqLimit.Value);
            ////if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
            ////{
            ////    var lastRef = query.Value;
            ////    BatchCounter = lastRef.ToString();
            ////    int nextSeq = Convert.ToInt16(query.Value) + 1;
            ////    string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
            ////    query.Value = nextVal;
            ////    db.Entry(query).State = EntityState.Modified;
            ////    db.SaveChanges();
            ////}
            ////else
            ////{
            ////    int nextSeq = 1;
            ////    string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
            ////    BatchCounter = nextVal;
            ////    nextSeq = 2;
            ////    nextVal = nextSeq.ToString().PadLeft(limiter, '0');
            ////    query.Value = nextVal;
            ////    query.ModifiedDateTime = DateTime.Now.Date;
            ////    db.Entry(query).State = EntityState.Modified;
            ////    db.SaveChanges();

            ////}
            //refundApplication.ApplicationReferenceNumber = "125663";
            //var refs = rcsapp.TransferInformation.RatesNumber;
            //var RefNum = refs + "RCC" + DateTime.Now.ToString("ddMMyyyy") + BatchCounter;
            //refundApplication.RCSApplicationStatusId = rcsapp.Id;
            //db.RefundApplications.Add(refundApplication);
            //db.SaveChanges();
            //}


            //catch(Exception IO)
            //{

            //}
            //var test = db.RefundApplications.FirstOrDefault();

            ////RefundApplication rr = new RefundApplication
            //                 RefundApplication rf = new RefundApplication
            //                 {

            //                     ApplicationReferenceNumber = "125663",
            //                     IsActive = true,
            //                     IsDeleted = false,
            //                     IsLocked = false,
            //                     CreatedDateTime = DateTime.Now,
            //                     ModifiedDateTime = DateTime.Now

            //                 };
            //db.RefundApplications.Add(rf);
            //db.Entry(rf).State = EntityState.Added;
            //db.SaveChanges();
            var documentCheckLists = new List<DocumentCheckList>();
            var RCAssessmentFigure = db.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RCCCertificate);

            documentCheckLists.Add(db.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RCAssessmentFigure.Id));

            var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCCCertificateUpload).FirstOrDefault();
            var application = db.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();



            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                CustomerId = userID,
                ApplicationId = (int)application.Id,
                Application = application,
                RcsApplicationId = id,
                ReferenceTypeId = (int)referenceType.Id,
                ReferenceType = referenceType,
                ReferenceId = Customer.Id,
                IsUploadView = true,
                Documents = db.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceTypeId == referenceType.Id && o.RCSApplicationStatusId == id && o.IsActive && !o.IsDeleted).ToList()
            };
            dvm.DocumentCheckLists = documentCheckLists;



            foreach (var customerDocument in dvm.Documents)
            {
                //if (SystemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(SystemUserId, customerDocument, ViewCodeKeys.UpdateCustomerDocuments))
                //{
                //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                //    return RedirectToAction("Index", "Error");
                //}
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     db.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                //DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation);
                //SecureActionLinkExtension.Encrypt(string.Format("generatedFileLocation={0}", DocumentHelper.GetDocumentAbsoluteUrl(customerDocument.DocumentLocation)));
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, Customer.Id, Customer.Id, application.Id, id))));
            }

            return View(dvm);
        }
        #endregion

        // GET: RCSApplicationStatus
        /// <summary>
        /// The Index.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Index()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    //SolarAccountBalanceApi ap = new SolarAccountBalanceApi();
                    //var tehh = ap.GetAccountBalance("1705370332");
                    //var dd = Wso2Api.GetAllUsers();
                    //var dd2 = Wso2Api.GetUserInfoWithRoles();
                    //var dd3 = Wso2Api.GetUserProfile();
                    var rCSApplicationStatus = db.RCSApplicationStatus.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).Where(x => x.CustomerId == Customer.Id).ToList();



                    //int numdd = Convert.ToInt32("text");

                        if (Session["RefOption"] != null)
                    {
                        var AppFee = db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.RCSApplicationFeeAmt);
                        var MessageContent = db.RCSUserMessages.FirstOrDefault(x => x.Key == RCSUserMessageKeys.ChoosePaymentOption);
                        var body = MessageContent.Body;
                        var refnum = Session["RefOption"].ToString();
                        body = body.Replace("{0}","R "+ AppFee.Value);
                        body = body.Replace("{1}", refnum);
                        ViewBag.MessageBody5 = body;
                        ViewBag.MessageTitle5 = MessageContent.Title+ refnum;


                        Session["RefOption"] = null;
                    }


                    if (Session["Display"] != null)
                    {

                        if (Session["ApplicationRefNo"] != null)
                        {

                            if (Session["Display"].ToString() == "True")
                            {
                                ViewBag.Display = "True";
                                ViewBag.MessageTitle3 = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.Rcs_assessment).FirstOrDefault().Description + Session["ApplicationRefNo"].ToString();
                            }
                            else
                            {
                                ViewBag.MessageTitle3 = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RCSSuccessMessage).FirstOrDefault().Body + Session["ApplicationRefNo"].ToString();
                            }


                            //ViewBag.MessageBody3 = TempData["ApplicationRefNo"].ToString();
                            Session["Display"] = null;
                            Session["ApplicationRefNo"] = null;
                        }
                        if (Session["MessageBody"] != null)
                        {



                            ViewBag.MessageBody3 = Session["MessageBody"].ToString();
                            Session["MessageBody"] = null;
                        }



                    }
                    else
                    {
                        if (Session["ApplicationRefNo"] != null)
                        {
                            ViewBag.MessageBody = Session["ApplicationRefNo"].ToString();
                            ViewBag.MessageTitle = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RCSSuccessMessage).FirstOrDefault().Body;
                            Session["ApplicationRefNo"] = null;
                        }
                        if (Session["ReceiptValues"] != null)
                        {
                            ViewBag.Value = Session["ReceiptValues"];
                            Session["ReceiptValues"] = null;
                        }



                    }

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    //int num = 5;
                    foreach (var item in rCSApplicationStatus)
                    {
                        //item.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = num })));
                        //item.Data= Convert.ToString(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id }));
                        ////item.Data= new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id })));
                        //item.DataList = new List<string>();
                        //item.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("referenceId={0}??customerId={1}??referenceTypeId={2}??applicationId={3}??agentId={4}??returnUrl={5}??rcsappId={6}??ratesRebateId={7}??incentivePolicyId={8}??errorList={8}", Customer.Id, Customer.Id, referenceType.Id, application.Id, application.Id, "sds", item.Id,1,1,""))));
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));

                        //return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id })));

                    }
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        [DecryptParameter]
        public ActionResult Index7(string p1)
        {
            Session["RefOption"] = p1;

            return RedirectToAction("Index", "RCSApplication");
            RedirectToAction("Index");

        }

        [DecryptParameter]
        public ActionResult Index2(int RcsApplicationId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.RCSApplicationStatus.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).Where(x => x.Id == RcsApplicationId).FirstOrDefault();



                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.CustomerProfile).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                    int num = 5;

                    var statuses = cxt.Status.ToList();
                    if (rCSApplicationStatus.StatusId == statuses.FirstOrDefault(o => o.Key == StatusKeys.AwaitingAssessmentPayment).Id)
                    {
                        rCSApplicationStatus.StatusId = statuses.FirstOrDefault(o => o.Key == StatusKeys.UploadPOPApplicationFee).Id;

                        db.Entry(rCSApplicationStatus).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    //foreach (var item in rCSApplicationStatus)
                    //{
                    //    //item.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = num })));
                    //    //item.Data= Convert.ToString(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id }));
                    //    ////item.Data= new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id })));
                    //    //item.DataList = new List<string>();
                    //    //item.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("referenceId={0}??customerId={1}??referenceTypeId={2}??applicationId={3}??agentId={4}??returnUrl={5}??rcsappId={6}??ratesRebateId={7}??incentivePolicyId={8}??errorList={8}", Customer.Id, Customer.Id, referenceType.Id, application.Id, application.Id, "sds", item.Id,1,1,""))));
                    //    item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));

                    //    //return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id })));

                    //}
                    Session["Display"] = "True";
                    Session["ApplicationRefNo"] = rCSApplicationStatus.ApplicationReferenceNumber;
                    Session["MessageBody"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.ApplicationExternalPaymentRecipt).FirstOrDefault().Body + rCSApplicationStatus.ApplicationReferenceNumber;

                    //if (Session["ApplicationRefNo"] != null)
                    //{
                    //    ViewBag.MessageBody = Session["ApplicationRefNo"].ToString();
                    //    ViewBag.MessageTitle = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.SuccessMessage).FirstOrDefault().Name;
                    //}
                    //if (Session["ReceiptValues"] != null)
                    //{
                    //    ViewBag.Value = Session["ReceiptValues"];
                    //}
                    //string ss = "true";
                    //AesCrypto aes = new AesCrypto();
                    //var test = aes.Encrypt(ss);
                    return RedirectToAction("Index"/*, new { param = test}*/);
                    return RedirectToAction("Login", "Account");
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        [DecryptParameter]
        public ActionResult Index4(int RcsApplicationId, string PageUrl)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var curl = Request.Url.AbsoluteUri;
                    string returnUrl = curl;
                    returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
                    returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
                    returnUrl = returnUrl + "/Capture/ReturnBackUrl";
                    var AppSettings = cxt.AppSettings.Where(x => x.IsDeleted != true && x.IsActive == true).ToList();
                    var appli = db.RCSApplicationStatus.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).Where(x => x.Id == RcsApplicationId).FirstOrDefault();


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

                    //returnUrl = "http://localhost:3450/Capture/Capture";
                    string adhocRef1 = Convert.ToString(RcsApplicationId);

                    string adhocRef2 = "";
                    string adhocRef3 = "";
                    string adhocRef4 = "";
                    string adhocRef5 = "";

                    //returnUrl = returnUrl.Substring(0, returnUrl.LastIndexOf('/'));
                    string amt = Amount.Replace('.', ',');
                    decimal conAmt = Convert.ToDecimal(amt);

                    //Format  parameters into Single Delimited String
                    string enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}",
                        pgMerchantId, voteNumber, pgMerchantReference, pgMerchantDescription, Amount,
                        pgEmail, pgMobile, customerFirstName, customerLastName, returnUrl, adhocRef1, adhocRef2, adhocRef3, adhocRef4, adhocRef5);
                    //Encrypt the Single String to and Encrypted string e
                    var e = new AesCrypto(encp).Encrypt(enc);
                    AppSetting PGDomain = db.AppSettings.FirstOrDefault(o => o.Key == PaymentGatewayKeys.PGDomain);
                    AppSetting PGEnvironment = db.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSPaymentGateway);

                    var baseFormat = PGDomain.Value + "PaymentGateway/" + PGEnvironment.Value + "?q=" + e;


                    //var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.CustomerProfile).FirstOrDefault();
                    //var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                    //int num = 5;

                    //var statuses = cxt.Status.ToList();
                    //if (rCSApplicationStatus.StatusId == statuses.FirstOrDefault(o => o.Key == StatusKeys.AwaitingAssessmentPayment).Id)
                    //{
                    //    rCSApplicationStatus.StatusId = statuses.FirstOrDefault(o => o.Key == StatusKeys.UploadPOPApplicationFee).Id;

                    //    db.Entry(rCSApplicationStatus).State = EntityState.Modified;
                    //    db.SaveChanges();
                    //}

                    //Session["Display"] = "True";
                    //Session["ApplicationRefNo"] = rCSApplicationStatus.ApplicationReferenceNumber;
                    //Session["MessageBody"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.ApplicationExternalPaymentRecipt).FirstOrDefault().Body + rCSApplicationStatus.ApplicationReferenceNumber;

                    return Redirect(baseFormat);
                    return RedirectToAction("Index"/*, new { param = test}*/);

                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        [DecryptParameter]
        public ActionResult Index3(int RcsApplicationId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.RCSApplicationStatus.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).Where(x => x.Id == RcsApplicationId).FirstOrDefault();



                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));


                    var statuses = cxt.Status.ToList();
                    if (rCSApplicationStatus.StatusId == statuses.FirstOrDefault(o => o.Key == StatusKeys.ViewAssessmentFigure).Id)
                    {
                        rCSApplicationStatus.StatusId = statuses.FirstOrDefault(o => o.Key == StatusKeys.UploadAssessmentFeePayment).Id;
                        //rCSApplicationStatus.CheckAssessmentFigureExpiry = false;
                        db.Entry(rCSApplicationStatus).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    DepartmentsApprovalsController DA = new DepartmentsApprovalsController();
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ExternalPaymentAssessmentFiguresPaymentOption).Description.ToString();

                    var Result = DA.ActivityTrackerAudit(rCSApplicationStatus.Id, ActivityTrackerMessage, Customer.Id);

                    //foreach (var item in rCSApplicationStatus)
                    //{
                    //    //item.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = num })));
                    //    //item.Data= Convert.ToString(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id }));
                    //    ////item.Data= new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id })));
                    //    //item.DataList = new List<string>();
                    //    //item.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("referenceId={0}??customerId={1}??referenceTypeId={2}??applicationId={3}??agentId={4}??returnUrl={5}??rcsappId={6}??ratesRebateId={7}??incentivePolicyId={8}??errorList={8}", Customer.Id, Customer.Id, referenceType.Id, application.Id, application.Id, "sds", item.Id,1,1,""))));
                    //    item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));

                    //    //return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id })));

                    //}
                    Session["Display"] = "True";
                    Session["ApplicationRefNo"] = rCSApplicationStatus.ApplicationReferenceNumber;
                    Session["MessageBody"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.AssessmentFigureExternalPaymentRecipt).FirstOrDefault().Body + rCSApplicationStatus.ApplicationReferenceNumber;


                    //if (Session["ApplicationRefNo"] != null)
                    //{
                    //    ViewBag.MessageBody = Session["ApplicationRefNo"].ToString();
                    //    ViewBag.MessageTitle = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.SuccessMessage).FirstOrDefault().Name;
                    //}
                    //if (Session["ReceiptValues"] != null)
                    //{
                    //    ViewBag.Value = Session["ReceiptValues"];
                    //}
                    //string ss = "true";
                    //AesCrypto aes = new AesCrypto();
                    //var test = aes.Encrypt(ss);
                    return RedirectToAction("Index"/*, new { param = test}*/);
                    return RedirectToAction("Login", "Account");
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        [EncryptedActionParameter]
        public ActionResult UploadDocs(int rcsAppId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.RCSApplicationStatus.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).Where(x => x.CustomerId == Customer.Id).Where(x => x.Id == rcsAppId).FirstOrDefault();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    int num = 5;

                    return RedirectToAction("ReUpload", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = rcsAppId })));



                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        [EncryptedActionParameter]
        public ActionResult UploadRefundDocs(int refundAppId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.RefundApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.Status).Where(x => x.CustomerId == Customer.Id).Where(x => x.Id == refundAppId).FirstOrDefault();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSRefund).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    int num = 5;

                    return RedirectToAction("Refund", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = refundAppId })));



                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult CustomAssessmentFigureMethod()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {

                    var AssessmentFigureApplicableList = cxt.RCSApplicationStatus.Where(x => x.AssessmentFigureUploadDate != null  && x.AssessmentFiguresEndDate == null).ToList().OrderByDescending(x => x.Id);
                    DepartmentsApprovalsController dac = new DepartmentsApprovalsController();
                    foreach (var item in AssessmentFigureApplicableList)
                    {

                           var result = dac.GenerareAssessmentFigures2(item.Id.ToString()) ?? null;
                       
                            if (result?.AssessmentFigureExpiryDate != null && result?.AssessmentFiguresEndDate != null)
                            {
                                var rcsApps = cxt.RCSApplicationStatus.FirstOrDefault(x => x.Id == item.Id) ?? null;

                                rcsApps.AssessmentFigureExpiryDate = result?.AssessmentFigureExpiryDate;
                                rcsApps.AssessmentFiguresEndDate = result?.AssessmentFiguresEndDate;
                          
                                 db.Entry(rcsApps).State = EntityState.Modified;
                            db.SaveChanges();
                                
                            }
                    }
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }


        public ActionResult CustomSendApplicationBackToAssessmentFigures()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {


                    //start
                    var applications = cxt.RCSApplicationStatus.Include(x =>x.Customer).Include(x => x.Status).Where(x => (x.CheckAssessmentFigureExpiry == true && x.AssessmentFigureExpiryDate < DateTime.Now)||(x.StatusId == 123 && x.AssessmentFigureExpiryDate < DateTime.Now)).ToList();
                    DepartmentsApprovalsController dac = new DepartmentsApprovalsController();
                    var StatusId = cxt.Status.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                    foreach (var item in applications)
                    {

                        var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.SubmitFigures).FirstOrDefault().Id;
                   
                        var roundrobinqueues = cxt.RoundRobinQueues.Include(x=>x.Clerk.SystemUser).Include(x => x.Clerk).FirstOrDefault(x =>x.RCSApplicationStatusId == item.Id && x.ResponsibilityTypeId == ResponsibilityTypeId && x.StatusId == StatusId);
                        
                        //clerk id with this one
                        var EmailContent = "Please note that your assessment figures have expired, the application will be routed to the back office to calculate and upload new assessment figures to be paid. RCS Application Reference Number: " + item.ApplicationReferenceNumber;

                        var Result2 = dac.ActivityTrackerAudit(item.Id, EmailContent, roundrobinqueues.ClerkId);

                        var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.AssessmentFigureUploaded).FirstOrDefault();
                        Email SendMail = new Email();
                        string attorneyemail = item.Customer.EmailAddress;
                        string attorneyname = item.Customer.FirstName + " " + item.Customer.LastName;
                        string emailbody = getemailbody.Description;
                        var systemusermobilenum = db.SystemUsers.Where(x => x.Id == item.Customer.SystemUserId).FirstOrDefault().MobileNumber;
                        //string emailbody = "Rates Clearance figures sent, please login to your RCS application and view your assessment figures. ";
                        var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.SubmitFiguresEmail).Description.ToString() + " " + emailbody;
                        var cont2 = "Email Sent From System To Conevayncer. Email Content: "+ EmailContent;

                        //email to conveyancer
                        SendMail.GenerateEmailSMS(cont2, systemusermobilenum, item.Id, item.Customer.Id, EmailContent, attorneyemail, "RCS- New Online Application Submission",
                                      EmailContent, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);


                        var EmailContent2 = "Please note that an application in your assessment figures queue has assessment figures that are unpaid and have expired, please login and calculate new assessment figures to be sent out to the conveyancer. RCS Application Reference Number: "+item.ApplicationReferenceNumber;
                        //var EmailToBackOffice = "Please note that an application in your assessment figures queue has assessment figures that are unpaid and have expired, please login to the RCS application, navigate to your assessment figures work queue, locate the target application by searching using the application reference number,click view calculate and submit new assessment figures that will be sent out to the conveyancer. RCS Application Reference Number: " + item.ApplicationReferenceNumber;

                        var cont = "Email Sent From System To Back Office. Email Content: "+ EmailContent2;

                        SendMail.GenerateEmailSMS(cont, roundrobinqueues.Clerk.SystemUser.MobileNumber, item.Id, roundrobinqueues.Clerk.Id, EmailContent2, roundrobinqueues.Clerk.SystemUser.EmailAddress, "RCS- New Online Application Submission",
                                   EmailContent2, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, roundrobinqueues.Clerk.UserFullName);

                        var rcsappUpdate = cxt.RCSApplicationStatus.FirstOrDefault(x => x.Id == item.Id);

                        rcsappUpdate.StatusId = 100;
                        rcsappUpdate.AssessmentFiguresExpired = true;
                        rcsappUpdate.CheckAssessmentFigureExpiry = false;
                        cxt.SaveChanges();



                    }

                 
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        [EncryptedActionParameter]
        public ActionResult UploadApplicationFee(int rcsAppId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.RCSApplicationStatus.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).Where(x => x.CustomerId == Customer.Id).Where(x => x.Id == rcsAppId).FirstOrDefault();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    int num = 5;

                    return RedirectToAction("ProofOfPayment", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = rcsAppId })));



                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        [EncryptedActionParameter]
        public ActionResult UploadAssessmentFee(int rcsAppId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.RCSApplicationStatus.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).Where(x => x.CustomerId == Customer.Id).Where(x => x.Id == rcsAppId).FirstOrDefault();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    int num = 5;

                    return RedirectToAction("ProofOfPaymentAssessment", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = rcsAppId })));



                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        /// <summary>
        /// The MessageInbox.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult MessageInbox(int id)
        {

            List<Communication> Communications = new List<Communication>();
            Communication CommunicationObject = new Communication();
            List<Comment> Comments = new List<Comment>();
            Communications = db.Communications.Where(x => x.RCSApplicationStatusId == id && x.IsDeleted == false).ToList();

            var MessageInboxViewModel = new MessageInboxViewModel
            {
                Communications = Communications,
                Comments = Comments
            };
            //CommunicationViewModel CVM = new CommunicationViewModel();
            ////////var CommunicationTypeId = db.CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationType).Id; please uncomment here


            ////////MessageInboxViewModel.Communications.RCSApplicationStatusId = id;
            ////////MessageInboxViewModel.Communications.CommunicationTypeId = CommunicationTypeId; to here
            return View(MessageInboxViewModel);
        }

        /// <summary>
        /// The CreateCommunication.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult CreateCommunication(int id)
        {
            Communication Communications = new Communication();
            Comment Comments = new Comment();
            var CommunicationViewModel = new CommunicationViewModel
            {
                Communications = Communications,
                Comments = Comments
            };
            //CommunicationViewModel CVM = new CommunicationViewModel();
            //var CommunicationTypeId = db.CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationType).Id;


            CommunicationViewModel.Communications.RCSApplicationStatusId = id;
            return PartialView("_CommunicationList", CommunicationViewModel);
        }

        /// <summary>
        /// The CreateMessage.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="CommunicationType">The CommunicationType<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [DecryptParameter]
        public ActionResult CreateMessage(int id, string CommunicationType)
        {
            TempData["id"] = id;
            TempData.Keep("id");

            TempData["CommunicationType"] = CommunicationType;
            TempData.Keep("CommunicationType");
            Communication Communications = new Communication();
            Comment Comments = new Comment();
            var CommunicationViewModel = new CommunicationViewModel
            {
                Communications = Communications,
                Comments = Comments,
                RCSApplicationID = id
            };
            //CommunicationViewModel CVM = new CommunicationViewModel();
            var CommunicationTypeId = db.CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationType).Id;


            CommunicationViewModel.Communications.RCSApplicationStatusId = id;
            CommunicationViewModel.Communications.CommunicationTypeId = CommunicationTypeId;
            return View(CommunicationViewModel);
        }

        /// <summary>
        /// The CreateMessage.
        /// </summary>
        /// <param name="request">The request<see cref="CommunicationViewModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        /// 
        [DecryptParameter]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMessage(CommunicationViewModel request)
        {
            Initialise();
            BaseHelper _base = new BaseHelper();
            _base.Initialise(db);

            var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == request.Communications.RCSApplicationStatusId && x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).FirstOrDefault();
            var Communications = db.Communications;
            Communication Conversation = new Communication();
            Communication InitialConversation = new Communication();

            var commType = db.CommunicationTypes;
            var depApp = db.DepartmentsApprovals;
            var keys = db.RCSDepartmentTypes;

            if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToBo).Id)
            {
                //Conversation = null;
                Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {
                    //Conversation = null;
                    //InitialConversation = null;

                    InitialConversation.RecipientCustomerId = RCSApplication.ClerkId;
                    InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RCSApplication.CustomerId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }
            else if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToWater).Id)
            {

                Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId && x.IsDeleted == false).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {

                    var waterDepId = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.LegalSection).FirstOrDefault().Id).FirstOrDefault();
                    InitialConversation.RecipientCustomerId = waterDepId.AssignedToCustomerId;
                    InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RCSApplication.CustomerId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }
            else if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToElectricity).Id)
            {

                Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId && x.IsDeleted == false).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {


                    var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.CreditControlSection).FirstOrDefault().Id).FirstOrDefault();
                    InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                    InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RCSApplication.CustomerId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }
            else if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToRealEstate).Id)
            {

                Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {

                    var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.EndowmentSection).FirstOrDefault().Id).FirstOrDefault();
                    InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                    InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RCSApplication.CustomerId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }
            else if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToSIE).Id)
            {

                Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {

                    var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.BillingSection).FirstOrDefault().Id).FirstOrDefault();
                    InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                    InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RCSApplication.CustomerId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }
            else if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToAccountsManagement).Id)
            {

                Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {

                    var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.SundryAccountSection).FirstOrDefault().Id).FirstOrDefault();
                    InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                    InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RCSApplication.CustomerId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }
            else if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToWater).Id)
            {

                Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {

                    var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.LegalSection).FirstOrDefault().Id).FirstOrDefault();
                    InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                    InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RCSApplication.ClerkId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }
            else if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToElectricity).Id)
            {

                Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {

                    var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.CreditControlSection).FirstOrDefault().Id).FirstOrDefault();
                    InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                    InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RCSApplication.ClerkId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }
            else if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToRealEstate).Id)
            {

                Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {

                    var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.EndowmentSection).FirstOrDefault().Id).FirstOrDefault();
                    InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                    InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RCSApplication.ClerkId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }
            else if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToSIE).Id)
            {

                Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {

                    var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.BillingSection).FirstOrDefault().Id).FirstOrDefault();
                    InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                    InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RCSApplication.ClerkId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }
            else if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToAccountsManagement).Id)
            {

                Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {

                    var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.SundryAccountSection).FirstOrDefault().Id).FirstOrDefault();
                    InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                    InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RCSApplication.ClerkId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }

            if (RCSApplication != null)
            {//if customer send email to BO Clerks working on application Else if BO send notification to customer 
                DepartmentsApprovalsController DA = new DepartmentsApprovalsController();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NewMessageSentOnChat).Description.ToString() + " " + request.Comments.Comments;
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", Customer.UserFullName);


                var Result = DA.ActivityTrackerAudit(RCSApplication.Id, ActivityTrackerMessage, Customer.Id);

                if (RCSApplication.CustomerId == Customer.Id)
                {
                    var ActiveStatus = db.Status.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault();
                    var RoundRobin = db.RoundRobinQueues.Include(x => x.Clerk).Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.StatusId == ActiveStatus.Id).ToList();
                    var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.ConveyancerSendsNewMessage).FirstOrDefault();
                    ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", RCSApplication.ApplicationReferenceNumber);
                    foreach (var item in RoundRobin)
                    {
                        Email SendMail = new Email();
                        string attorneyemail = item.Clerk.EmailAddress;
                        string attorneyname = item.Clerk.FirstName + " " + item.Clerk.LastName;
                        string emailbody = getemailbody.Description;
                        emailbody = emailbody.Replace("{1}", RCSApplication.ApplicationReferenceNumber);
                        var systemusermobilenum = db.SystemUsers.Where(x => x.Id == item.Clerk.SystemUserId).FirstOrDefault().MobileNumber;
                        //string emailbody = "Rates Clearance figures sent, please login to your RCS application and view your assessment figures. ";
                        var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + emailbody;

                        SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum, RCSApplication.Id, item.Clerk.Id, emailbody, attorneyemail, "RCS - New Message Notification",
                                      emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                    }




                }
                else
                {
                    //Email Content - 
                    var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.BOSendsNewMessage).FirstOrDefault();
                    Email SendMail = new Email();
                    string attorneyemail = RCSApplication.Customer.EmailAddress;
                    string attorneyname = RCSApplication.Customer.FirstName + " " + RCSApplication.Customer.LastName;
                    string emailbody = getemailbody.Description;
                    emailbody = emailbody.Replace("{1}", RCSApplication.ApplicationReferenceNumber);
                    var systemusermobilenum = db.SystemUsers.Where(x => x.Id == RCSApplication.Customer.SystemUserId).FirstOrDefault().MobileNumber;
                    //string emailbody = "Rates Clearance figures sent, please login to your RCS application and view your assessment figures. ";
                    var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyConevayncerOfNewMessage).Description.ToString() + " " + emailbody;

                    SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum, RCSApplication.Id, Customer.Id, emailbody, attorneyemail, "RCS - New Message Notification",
                                  emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                    var ActiveStatus = db.Status.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault();
                    var RoundRobin = db.RoundRobinQueues.Include(x => x.Clerk).Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.StatusId == ActiveStatus.Id).ToList();
                    var BOgetEmailBody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.NotifyBOOfNewBOMessage).FirstOrDefault();
                    ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", RCSApplication.ApplicationReferenceNumber);
                    foreach (var item in RoundRobin)
                    {

                        if (item.ClerkId != Customer.Id)
                        {
                            Email SendMail2 = new Email();
                            string BOEmail = item.Clerk.EmailAddress;
                            string BOName = item.Clerk.FirstName + " " + item.Clerk.LastName;
                            string BOemailbody = BOgetEmailBody.Description;
                            BOemailbody = BOemailbody.Replace("{1}", RCSApplication.ApplicationReferenceNumber);
                            var BOsystemusermobilenum = db.SystemUsers.Where(x => x.Id == item.Clerk.SystemUserId).FirstOrDefault().MobileNumber;
                            //string emailbody = "Rates Clearance figures sent, please login to your RCS application and view your assessment figures. ";
                            var BOActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + BOemailbody;

                            SendMail.GenerateEmailSMS(BOActivityTrackerMessageEmail, BOsystemusermobilenum, RCSApplication.Id, item.Clerk.Id, BOemailbody, attorneyemail, "RCS - New Message Notification",
                                          BOemailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                        }

                    }


                }
            }

            Comment Comments = new Comment();
            Comments.Comments = request.Comments.Comments;
            Comments.CommunicationId = Conversation.Id;
            Comments.SenderCustomerId = Customer.Id;
            Comments.ReferenceNumeric = 123;

            db.Comments.Add(Comments);
            db.SaveChanges();
            AesCrypto Aes = new AesCrypto();
            var q = Aes.Encrypt("id=" + RCSApplication.Id.ToString() + "&" + "CommunicationType=" + Conversation.CommunicationType.Key);
            return RedirectToAction("CreateMessage", "RCSApplication", new { q = q });
        }

        /// <summary>
        /// The MessageTimeline.
        /// </summary>
        /// <param name="RCSApplicationID">The RCSApplicationID<see cref="int"/>.</param>
        /// <param name="CommunicationType">The CommunicationType<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult MessageTimeline(int RCSApplicationID, int CommunicationType)
        {

            try
            {

                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(db);

                Communication InitialConversation = new Communication();
                Communication Conversation = new Communication();
                List<Comment> Comments = new List<Comment>();
                var Comms = db.Comments;
                var Communications = db.Communications;
                var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == RCSApplicationID && x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).FirstOrDefault();
                var CommunicationTypes = db.CommunicationTypes;
                var depApp = db.DepartmentsApprovals.Where(x => x.IsDeleted == false).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                var keys = db.RCSDepartmentTypes;

                if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToBo).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        InitialConversation.RecipientCustomerId = RCSApplication.ClerkId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToWater).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.LegalSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToElectricity).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.CreditControlSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToSIE).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.BillingSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToAccountsManagement).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.SundryAccountSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToWater).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.LegalSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.ClerkId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToElectricity).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.CreditControlSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.ClerkId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToRealEstate).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.EndowmentSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.ClerkId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToSIE).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.BillingSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.ClerkId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToAccountsManagement).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.BillingSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.ClerkId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }

                var loggedInUser = Customer.Id;
                var requests = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.IsDeleted == false && (x.CustomerId == loggedInUser && x.RecipientCustomerId == RCSApplication.ClerkId || x.CustomerId == RCSApplication.ClerkId && x.RecipientCustomerId == loggedInUser)).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).ToList();
                ViewBag.requests = requests;
                ViewBag.comments = Comments;
                return View();
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
        }

        /// <summary>
        /// The generateMessage.
        /// </summary>
        /// <param name="request">The request<see cref="Communication"/>.</param>
        public void generateMessage(Communication request)
        {
            Initialise();
            BaseHelper _base = new BaseHelper();
            _base.Initialise(db);
            db.Communications.Add(request);
            db.SaveChanges();
        }

        /// <summary>
        /// The RCSLateApplicationFee.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult RCSLateApplicationFee(int id)
        {

            try
            {
                var urlBuilder =
    new System.UriBuilder(Request.Url.AbsoluteUri)
    {
        Path = Url.Action("ReturnBackUrl", "Capture"),
        Query = null,
    };

                Uri uri = urlBuilder.Uri;
                string returnUrl = urlBuilder.ToString();
                var AppSettings = db.AppSettings;
                var appli = db.RCSApplicationStatus.Where(x => x.Id == id && x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).FirstOrDefault();

                string pgMerchantId = "pg_crm_app_rcs";
                string voteNumber = "453414z0610mkzzzzz16";
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

                //returnUrl = "http://localhost:3450/Capture/Capture";
                string adhocRef1 = Convert.ToString(appli.Id);

                string adhocRef2 = "";
                string adhocRef3 = "";
                string adhocRef4 = "";
                string adhocRef5 = "";

                string amt = Amount.Replace('.', ',');
                decimal conAmt = Convert.ToDecimal(amt);

                //Format  parameters into Single Delimited String
                string enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}",
                    pgMerchantId, voteNumber, pgMerchantReference, pgMerchantDescription, Amount,
                    pgEmail, pgMobile, customerFirstName, customerLastName, returnUrl, adhocRef1, adhocRef2, adhocRef3, adhocRef4, adhocRef5);
                //Encrypt the Single String to and Encrypted string e
                var e = new AesCrypto(encp).Encrypt(enc);
                AppSetting PGDomain = db.AppSettings.FirstOrDefault(o => o.Key == PaymentGatewayKeys.PGDomain);

                var baseFormat = PGDomain.Value + "PaymentGateway/PaygateTest?q=" + e;

                return Redirect(baseFormat);
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }



        }
        #region Test Messenger service
        public ActionResult MessengerService(int RCSApplicationID, int CommunicationType, CommunicationViewModel ViewModel)
        {
            try
            {
                #region base initialization
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(db);
                #endregion

                #region Reference object models
                Communication InitialConversation = new Communication();
                Communication Conversation = new Communication();
                List<Comment> Comments = new List<Comment>();
                Comment lastcomment = new Comment();
                var department = new CommunicationType();
                #endregion

                #region Instantiation
                var Comms = db.Comments;
                var Communications = db.Communications;
                var RCSApplication = db.RCSApplicationStatus.Where(x => x.Id == RCSApplicationID).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).FirstOrDefault();
                var CommunicationTypes = db.CommunicationTypes;
                var depApp = db.DepartmentsApprovals.Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                var keys = db.RCSDepartmentTypes;
                #endregion

                #region Communication Type Functionality
                if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToBo).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                        ViewBag.LastComment = lastcomment.FindDepartmentWithComments(RCSApplicationID);
                        //var comment = lastcomment.FindLastComment(RCSApplicationID);

                        //List of comments = dbContext.Comments.Where(x ==> x.CommunicationId == ConversationId)
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        InitialConversation.RecipientCustomerId = RCSApplication.ClerkId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();


                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                        ViewBag.LastComment = lastcomment.FindDepartmentWithComments(RCSApplicationID);
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToWater).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.LegalSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToElectricity).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.CreditControlSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToSIE).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.BillingSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToAccountsManagement).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.SundryAccountSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToWater).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.LegalSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.ClerkId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToElectricity).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.CreditControlSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.ClerkId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToRealEstate).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.EndowmentSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.ClerkId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToSIE).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.BillingSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.ClerkId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                else if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.BOToAccountsManagement).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        var AssignedTo = depApp.Where(x => x.RCSApplicationStatus.Id == RCSApplication.Id && x.DepartmentId == keys.Where(y => y.Key == RCSDepartmentTypeKeys.BillingSection).FirstOrDefault().Id).FirstOrDefault();
                        InitialConversation.RecipientCustomerId = AssignedTo.AssignedToCustomerId;
                        InitialConversation.RCSApplicationStatusId = RCSApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RCSApplication.ClerkId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }
                #endregion

                #region ViewBag
                var loggedInUser = Customer.Id;
                var requests = Communications.Where(x => x.RCSApplicationStatusId == RCSApplicationID && (x.CustomerId == loggedInUser && x.RecipientCustomerId == RCSApplication.ClerkId || x.CustomerId == RCSApplication.ClerkId && x.RecipientCustomerId == loggedInUser)).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).ToList();
                ViewBag.requests = requests;
                ViewBag.comments = Comments;
                ViewBag.LoggedInUser = loggedInUser;
                ViewBag.Departments = department.GetAllCommunicationActive();
                #endregion

                return View(ViewModel);
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
        }
        #endregion
        // GET: RCSApplicationStatus/Details/5
        /// <summary>
        /// The Details.
        /// </summary>
        /// <param name="refNo">The refNo<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [DecryptParameter]
        public ActionResult Details(int? refNo)
        {

            var departmentsApproval = (IEnumerable<DepartmentsApproval>)null;

            departmentsApproval = db.DepartmentsApprovals.Where(x => x.RCSApplicationStatusId == refNo && x.IsDeleted == false).Include(d => d.RCSApplicationStatus).Include(d => d.AssignedToCustomer).Include(d => d.RCSDepartment).Include(d => d.Status).ToList();

            return View(departmentsApproval);
        }

        // GET: RCSApplicationStatus/Create
        /// <summary>
        /// The Create.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.MunicipalAccountInformationId = new SelectList(db.MunicipalAccountInformations, "Id", "WaterAccountNo");
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name");
            ViewBag.TransferInformationId = new SelectList(db.TransferInformations, "Id", "PropertyType");
            return View();
        }

        // POST: RCSApplicationStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The Create.
        /// </summary>
        /// <param name="rCSApplicationStatus">The rCSApplicationStatus<see cref="RCSApplicationStatus"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ApplicationReferenceNumber,StatusId,CustomerId,TransferInformationId,MunicipalAccountInformationId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] RCSApplicationStatus rCSApplicationStatus)
        {
            if (ModelState.IsValid)
            {
                db.RCSApplicationStatus.Add(rCSApplicationStatus);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSApplicationStatus.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", rCSApplicationStatus.CustomerId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSApplicationStatus.ModifiedBySystemUserId);
            ViewBag.MunicipalAccountInformationId = new SelectList(db.MunicipalAccountInformations, "Id", "WaterAccountNo", rCSApplicationStatus.MunicipalAccountInformationId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", rCSApplicationStatus.StatusId);
            ViewBag.TransferInformationId = new SelectList(db.TransferInformations, "Id", "PropertyType", rCSApplicationStatus.TransferInformationId);
            return View(rCSApplicationStatus);
        }

        // GET: RCSApplicationStatus/Edit/5
        /// <summary>
        /// The Edit.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RCSApplicationStatus rCSApplicationStatus = db.RCSApplicationStatus.Find(id);
            if (rCSApplicationStatus == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSApplicationStatus.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", rCSApplicationStatus.CustomerId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSApplicationStatus.ModifiedBySystemUserId);
            ViewBag.MunicipalAccountInformationId = new SelectList(db.MunicipalAccountInformations, "Id", "WaterAccountNo", rCSApplicationStatus.MunicipalAccountInformationId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", rCSApplicationStatus.StatusId);
            ViewBag.TransferInformationId = new SelectList(db.TransferInformations, "Id", "PropertyType", rCSApplicationStatus.TransferInformationId);
            return View(rCSApplicationStatus);
        }

        // POST: RCSApplicationStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The Edit.
        /// </summary>
        /// <param name="rCSApplicationStatus">The rCSApplicationStatus<see cref="RCSApplicationStatus"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ApplicationReferenceNumber,StatusId,CustomerId,TransferInformationId,MunicipalAccountInformationId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] RCSApplicationStatus rCSApplicationStatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rCSApplicationStatus).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSApplicationStatus.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", rCSApplicationStatus.CustomerId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSApplicationStatus.ModifiedBySystemUserId);
            ViewBag.MunicipalAccountInformationId = new SelectList(db.MunicipalAccountInformations, "Id", "WaterAccountNo", rCSApplicationStatus.MunicipalAccountInformationId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", rCSApplicationStatus.StatusId);
            ViewBag.TransferInformationId = new SelectList(db.TransferInformations, "Id", "PropertyType", rCSApplicationStatus.TransferInformationId);
            return View(rCSApplicationStatus);
        }

        // GET: RCSApplicationStatus/Delete/5
        /// <summary>
        /// The Delete.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RCSApplicationStatus rCSApplicationStatus = db.RCSApplicationStatus.Find(id);
            if (rCSApplicationStatus == null)
            {
                return HttpNotFound();
            }
            return View(rCSApplicationStatus);
        }

        // POST: RCSApplicationStatus/Delete/5
        /// <summary>
        /// The DeleteConfirmed.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RCSApplicationStatus rCSApplicationStatus = db.RCSApplicationStatus.Find(id);
            db.RCSApplicationStatus.Remove(rCSApplicationStatus);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// The Dispose.
        /// </summary>
        /// <param name="disposing">The disposing<see cref="bool"/>.</param>
        /// 
        #region Refund Chat


        public ActionResult RefundMessengerService(int RefundApplicationID, int CommunicationType, CommunicationViewModel ViewModel)
        {
            try
            {
                #region base initialization
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(db);
                #endregion

                #region Reference object models
                Communication InitialConversation = new Communication();
                Communication Conversation = new Communication();
                List<Comment> Comments = new List<Comment>();
                Comment lastcomment = new Comment();
                var department = new CommunicationType();
                #endregion

                #region Instantiation
                var Comms = db.Comments;
                var Communications = db.Communications;
                var RefundApplication = db.RefundApplications.Where(x => x.Id == RefundApplicationID).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.Status).FirstOrDefault();
                var CommunicationTypes = db.CommunicationTypes;
                var depApp = db.DepartmentsApprovals.Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                var keys = db.RCSDepartmentTypes;
                #endregion

                #region Communication Type Functionality
                if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToBo).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RefundApplicationId == RefundApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                        ViewBag.LastComment = lastcomment.RefundFindDepartmentWithComments(RefundApplicationID);
                        //var comment = lastcomment.FindLastComment(RCSApplicationID);

                        //List of comments = dbContext.Comments.Where(x ==> x.CommunicationId == ConversationId)
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        InitialConversation.RecipientCustomerId = RefundApplication.ClerkId;
                        InitialConversation.RefundApplicationId = RefundApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RefundApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();


                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                        ViewBag.LastComment = lastcomment.RefundFindDepartmentWithComments(RefundApplicationID);
                    }
                }

                #endregion

                #region ViewBag
                var loggedInUser = Customer.Id;
                var requests = Communications.Where(x => x.RefundApplicationId == RefundApplicationID && (x.CustomerId == loggedInUser && x.RecipientCustomerId == RefundApplication.ClerkId || x.CustomerId == RefundApplication.ClerkId && x.RecipientCustomerId == loggedInUser)).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).ToList();
                ViewBag.requests = requests;
                ViewBag.comments = Comments;
                ViewBag.LoggedInUser = loggedInUser;
                ViewBag.Departments = department.GetAllCommunicationActive();
                #endregion

                return View(ViewModel);
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
        }
        public ActionResult RefundMessageInbox(int id)
        {

            List<Communication> Communications = new List<Communication>();
            Communication CommunicationObject = new Communication();
            List<Comment> Comments = new List<Comment>();
            Communications = db.Communications.Where(x => x.RefundApplicationId == id && x.IsDeleted == false).ToList();

            var MessageInboxViewModel = new MessageInboxViewModel
            {
                Communications = Communications,
                Comments = Comments
            };
            //CommunicationViewModel CVM = new CommunicationViewModel();
            ////////var CommunicationTypeId = db.CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationType).Id; please uncomment here


            ////////MessageInboxViewModel.Communications.RCSApplicationStatusId = id;
            ////////MessageInboxViewModel.Communications.CommunicationTypeId = CommunicationTypeId; to here
            return View(MessageInboxViewModel);
        }

        /// <summary>
        /// The CreateCommunication.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult RefundCreateCommunication(int id)
        {
            Communication Communications = new Communication();
            Comment Comments = new Comment();
            var CommunicationViewModel = new CommunicationViewModel
            {
                Communications = Communications,
                Comments = Comments
            };
            //CommunicationViewModel CVM = new CommunicationViewModel();
            //var CommunicationTypeId = db.CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationType).Id;


            CommunicationViewModel.Communications.RefundApplicationId = id;
            return PartialView("_CommunicationList", CommunicationViewModel);
        }

        /// <summary>
        /// The CreateMessage.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="CommunicationType">The CommunicationType<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [DecryptParameter]
        public ActionResult RefundCreateMessage(int id, string CommunicationType)
        {
            TempData["id"] = id;
            TempData.Keep("id");

            TempData["CommunicationType"] = CommunicationType;
            TempData.Keep("CommunicationType");
            Communication Communications = new Communication();
            Comment Comments = new Comment();
            var CommunicationViewModel = new CommunicationViewModel
            {
                Communications = Communications,
                Comments = Comments,
                RefundApplicationID = id
            };
            //CommunicationViewModel CVM = new CommunicationViewModel();
            var CommunicationTypeId = db.CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationType).Id;


            CommunicationViewModel.Communications.RefundApplicationId = id;
            CommunicationViewModel.Communications.CommunicationTypeId = CommunicationTypeId;
            return View(CommunicationViewModel);
        }

        /// <summary>
        /// The CreateMessage.
        /// </summary>
        /// <param name="request">The request<see cref="CommunicationViewModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        /// 
        [DecryptParameter]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RefundCreateMessage(CommunicationViewModel request)
        {
            Initialise();
            BaseHelper _base = new BaseHelper();
            _base.Initialise(db);

            var RefundApplication = db.RefundApplications.Where(x => x.Id == request.Communications.RefundApplicationId && x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.Status).FirstOrDefault();
            var Communications = db.Communications;
            Communication Conversation = new Communication();
            Communication InitialConversation = new Communication();

            var commType = db.CommunicationTypes;
            var depApp = db.DepartmentsApprovals;
            var keys = db.RCSDepartmentTypes;

            if (request.Communications.CommunicationTypeId == commType.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToBo).Id)
            {
                //Conversation = null;
                Conversation = db.Communications.Where(x => x.RefundApplicationId == RefundApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();

                if (Conversation != null)
                {

                }
                else if (Conversation == null)
                {
                    //Conversation = null;
                    //InitialConversation = null;

                    InitialConversation.RecipientCustomerId = RefundApplication.ClerkId;
                    InitialConversation.RefundApplicationId = RefundApplication.Id;
                    InitialConversation.IsActive = true;
                    InitialConversation.IsDeleted = false;
                    InitialConversation.IsLocked = false;
                    InitialConversation.CustomerId = RefundApplication.CustomerId;
                    InitialConversation.ReferenceNumeric = 123;
                    InitialConversation.CommunicationTypeId = request.Communications.CommunicationTypeId;

                    db.Communications.Add(InitialConversation);
                    db.SaveChanges();

                    Conversation = db.Communications.Where(x => x.RefundApplicationId == RefundApplication.Id && x.CommunicationTypeId == request.Communications.CommunicationTypeId).FirstOrDefault();


                }


            }

            if (RefundApplication != null)
            {//if customer send email to BO Clerks working on application Else if BO send notification to customer 
                DepartmentsApprovalsController DA = new DepartmentsApprovalsController();
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NewMessageSentOnChat).Description.ToString() + " " + request.Comments.Comments;
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", Customer.UserFullName);


                //var Result = DA.ActivityTrackerAudit(RCSApplication.Id, ActivityTrackerMessage, Customer.Id);

                if (RefundApplication.CustomerId == Customer.Id)
                {
                    var ActiveStatus = db.Status.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault();
                    var RoundRobin = db.RoundRobinQueues.Include(x => x.Clerk).Where(x => x.RefundApplicationId == RefundApplication.Id && x.StatusId == ActiveStatus.Id).ToList();
                    var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.RefundConveyancerSendsNewMessage).FirstOrDefault();
                    ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", RefundApplication.ApplicationReferenceNumber);
                    foreach (var item in RoundRobin)
                    {
                        Email SendMail = new Email();
                        string attorneyemail = item.Clerk.EmailAddress;
                        string attorneyname = item.Clerk.FirstName + " " + item.Clerk.LastName;
                        string emailbody = getemailbody.Description;
                        emailbody = emailbody.Replace("{1}", RefundApplication.ApplicationReferenceNumber);
                        var systemusermobilenum = db.SystemUsers.Where(x => x.Id == item.Clerk.SystemUserId).FirstOrDefault().MobileNumber;
                        //string emailbody = "Rates Clearance figures sent, please login to your RCS application and view your assessment figures. ";
                        var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + emailbody;

                        //SendMail.GenerateRefundEmail(ActivityTrackerMessageEmail, systemusermobilenum, RefundApplication.Id, item.Clerk.Id, emailbody, attorneyemail, "RCS - New Message Notification",
                        //              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                    }




                }
                else
                {
                    //Email Content - 
                    var getemailbody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.RefundBOSendsNewMessage).FirstOrDefault();
                    Email SendMail = new Email();
                    string attorneyemail = RefundApplication.Customer.EmailAddress;
                    string attorneyname = RefundApplication.Customer.FirstName + " " + RefundApplication.Customer.LastName;
                    string emailbody = getemailbody.Description;
                    emailbody = emailbody.Replace("{1}", RefundApplication.ApplicationReferenceNumber);
                    var systemusermobilenum = db.SystemUsers.Where(x => x.Id == RefundApplication.Customer.SystemUserId).FirstOrDefault().MobileNumber;
                    //string emailbody = "Rates Clearance figures sent, please login to your RCS application and view your assessment figures. ";
                    var ActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyConevayncerOfNewMessage).Description.ToString() + " " + emailbody;

                    //SendMail.GenerateEmailSMS(ActivityTrackerMessageEmail, systemusermobilenum, RefundApplication.Id, Customer.Id, emailbody, attorneyemail, "RCS - New Message Notification",
                    //              emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                    var ActiveStatus = db.Status.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault();
                    var RoundRobin = db.RoundRobinQueues.Include(x => x.Clerk).Where(x => x.RefundApplicationId == RefundApplication.Id && x.StatusId == ActiveStatus.Id).ToList();
                    var BOgetEmailBody = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.RefundNotifyBOOfNewBOMessage).FirstOrDefault();
                    ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", RefundApplication.ApplicationReferenceNumber);
                    foreach (var item in RoundRobin)
                    {

                        if (item.ClerkId != Customer.Id)
                        {
                            Email SendMail2 = new Email();
                            string BOEmail = item.Clerk.EmailAddress;
                            string BOName = item.Clerk.FirstName + " " + item.Clerk.LastName;
                            string BOemailbody = BOgetEmailBody.Description;
                            BOemailbody = BOemailbody.Replace("{1}", RefundApplication.ApplicationReferenceNumber);
                            var BOsystemusermobilenum = db.SystemUsers.Where(x => x.Id == item.Clerk.SystemUserId).FirstOrDefault().MobileNumber;
                            //string emailbody = "Rates Clearance figures sent, please login to your RCS application and view your assessment figures. ";
                            var BOActivityTrackerMessageEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotifyBOOfNewMessage).Description.ToString() + " " + BOemailbody;

                            //SendMail.GenerateEmailSMS(BOActivityTrackerMessageEmail, BOsystemusermobilenum, RCSApplication.Id, item.Clerk.Id, BOemailbody, attorneyemail, "RCS - New Message Notification",
                            //              BOemailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);

                        }

                    }


                }
            }

            Comment Comments = new Comment();
            Comments.Comments = request.Comments.Comments;
            Comments.CommunicationId = Conversation.Id;
            Comments.SenderCustomerId = Customer.Id;
            Comments.ReferenceNumeric = 123;

            db.Comments.Add(Comments);
            db.SaveChanges();
            AesCrypto Aes = new AesCrypto();
            var q = Aes.Encrypt("id=" + RefundApplication.Id.ToString() + "&" + "CommunicationType=" + Conversation.CommunicationType.Key);
            return RedirectToAction("RefundCreateMessage", "RCSApplication", new { q = q });
        }

        /// <summary>
        /// The MessageTimeline.
        /// </summary>
        /// <param name="RCSApplicationID">The RCSApplicationID<see cref="int"/>.</param>
        /// <param name="CommunicationType">The CommunicationType<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult RefundMessageTimeline(int RefundApplicationID, int CommunicationType)
        {

            try
            {

                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(db);

                Communication InitialConversation = new Communication();
                Communication Conversation = new Communication();
                List<Comment> Comments = new List<Comment>();
                var Comms = db.Comments;
                var Communications = db.Communications;
                var RefundApplication = db.RefundApplications.Where(x => x.Id == RefundApplicationID && x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.Status).FirstOrDefault();
                var CommunicationTypes = db.CommunicationTypes;
                var depApp = db.DepartmentsApprovals.Where(x => x.IsDeleted == false).Include(d => d.RCSApplicationStatus).Include(d => d.RCSDepartment).Include(d => d.Status);
                var keys = db.RCSDepartmentTypes;

                if (CommunicationType == CommunicationTypes.FirstOrDefault(o => o.Key == CommunicationTypeKeys.AttorneyToBo).Id)
                {
                    //Conversation = null;
                    Conversation = Communications.Where(x => x.RefundApplicationId == RefundApplicationID && x.CommunicationTypeId == CommunicationType).FirstOrDefault();

                    if (Conversation != null)
                    {
                        Comments = Comms.Where(x => x.CommunicationId == Conversation.Id).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).Include(c => c.Communication).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType).Include(c => c.SenderCustomer).ToList();
                    }
                    else if (Conversation == null)
                    {
                        //InitialConversation = null;

                        InitialConversation.RecipientCustomerId = RefundApplication.ClerkId;
                        InitialConversation.RefundApplicationId = RefundApplication.Id;
                        InitialConversation.IsActive = true;
                        InitialConversation.IsDeleted = false;
                        InitialConversation.IsLocked = false;
                        InitialConversation.CustomerId = RefundApplication.CustomerId;
                        InitialConversation.ReferenceNumeric = 123;
                        InitialConversation.CommunicationTypeId = CommunicationType;

                        db.Communications.Add(InitialConversation);
                        db.SaveChanges();
                        Comments = Comms.Where(x => x.CommunicationId == InitialConversation.Id).ToList();
                    }
                }

                var loggedInUser = Customer.Id;
                var requests = Communications.Where(x => x.RefundApplicationId == RefundApplicationID && x.IsDeleted == false && (x.CustomerId == loggedInUser && x.RecipientCustomerId == RefundApplication.ClerkId || x.CustomerId == RefundApplication.ClerkId && x.RecipientCustomerId == loggedInUser)).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).ToList();
                ViewBag.requests = requests;
                ViewBag.comments = Comments;
                return View();
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
        }

        /// <summary>
        /// The generateMessage.
        /// </summary>
        /// <param name="request">The request<see cref="Communication"/>.</param>
        public void RefundgenerateMessage(Communication request)
        {
            Initialise();
            BaseHelper _base = new BaseHelper();
            _base.Initialise(db);
            db.Communications.Add(request);
            db.SaveChanges();
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
