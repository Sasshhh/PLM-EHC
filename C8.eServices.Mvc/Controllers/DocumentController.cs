using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace C8.eServices.Mvc.Controllers
{
    public class DocumentController : Controller
    {
        //private eServicesDbContext _context = new eServicesDbContext();
        private BaseHelper _base = new BaseHelper();
        public DocumentController()
        {
            eServicesDbContext _context = new eServicesDbContext();
            IdentityManager = new IdentityManager(_context);
            UserManager =
                new UserManager<SystemIdentityUser>(
                new UserStore<SystemIdentityUser>(_context));
        }

        public IdentityManager IdentityManager { get; set; }
        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        private int SystemUserId = -1;
        //[Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]
        #region Documents For All Steps



       
        #endregion

        //






        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]

        public ActionResult HumanUploadOccupantDoc(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var appstatus = _context.PropertyLeaseApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == rcsappId);
            //if (appstatus.Status.Key == StatusKeys.AwaitingRiskAssessment) return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = rcsappId, appId = "" });

            DocumentsViewModel dvm = new DocumentsViewModel();
            MatchingHelper.DocumentUploadHumanOccupants(dvm,_context, customerId, customerId, referenceTypeId, applicationId, returnUrl, rcsappId, true);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
           
            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };
                ViewBag.NavigationParameters = nav;
            }

            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();

            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.PropertyLeaseApplication = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }

        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]

        public ActionResult AddOccupants(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var appstatus = _context.PropertyLeaseApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == rcsappId);
            //if (appstatus.Status.Key == StatusKeys.AwaitingRiskAssessment) return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = rcsappId, appId = "" });

            DocumentsViewModel dvm = new DocumentsViewModel();
            bool IsUpload = true;
            MatchingHelper.DocumentCaptureAddOccupantsDocuments(dvm,_context, customerId, customerId, referenceTypeId, applicationId, returnUrl, rcsappId, IsUpload);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
           
            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };
                ViewBag.NavigationParameters = nav;
            }

            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();

            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.PropertyLeaseApplication = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }

        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]
        public ActionResult Index(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var appstatus = _context.PropertyLeaseApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == rcsappId);
            if (appstatus.Status.Key == StatusKeys.AwaitingRiskAssessment) return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = rcsappId, appId = "" });

            DocumentsViewModel dvm = new DocumentsViewModel();
            bool IsUpload = true;
            MatchingHelper.DocumentCaptureApplication(dvm,_context, referenceId, customerId, referenceTypeId, applicationId, returnUrl, rcsappId, IsUpload);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
           
            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }

            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();

            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.PropertyLeaseApplication = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }

        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]
        public ActionResult Index3(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var appstatus = _context.HumanSettlementApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == rcsappId);
            if (appstatus.Status.Key == StatusKeys.AwaitingRiskAssessment) return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = rcsappId, appId = "" });

            DocumentsViewModel dvm = new DocumentsViewModel();
            MatchingHelper.HumanRenewalDocuments(dvm,_context, referenceId, customerId, referenceTypeId, applicationId, returnUrl, rcsappId, true);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
           
            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }

            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();

            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.HumanSettlementApplicationId = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }

        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]
        public ActionResult IndexTenants(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var record = _context.LeaseDetails.Include(r => r.Status).Where(x => x.PropertyLeaseApplicationId == rcsappId).OrderByDescending(x => x.Id).FirstOrDefault();
            if ((record.Status.Key == StatusKeys.AwaitingRiskAssessment))
            {
                Session["View"] = "TenantLeaseRenewalOffer";
                Session["Controller"] = "leaseDetails";
                return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = rcsappId, appId = record.Id });
            }


            DocumentsViewModel dvm = new DocumentsViewModel();
            MatchingHelper.DocumentCaptureTenantLease(dvm, _context, referenceId, customerId, referenceTypeId, applicationId, returnUrl, rcsappId, true);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
           
            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }

            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();
            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.PropertyLeaseApplication = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            
            if (Session["LeaseOfferValidationSession"] != null)
            {
                var value = Session["LeaseOfferValidationSession"].ToString();
                Session["LeaseOfferValidationSession"] = null;
                ViewBag.LeaseOfferValidationSession = value;
            }
            Session["LeaseOfferValidationSession"] = null;
            return View(dvm);
        }
        public ActionResult CommentsIndex(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");


            //var PurchaseInfo = _context.PurchaserInformations.Where(x => x.RCSApplicationStatusId == RcsApplication.Id).Include(r => r.RCSApplicationStatus)/*.Include(r =>r.PurchaseType)*/.FirstOrDefault();
            //if (PurchaseInfo == null) throw new Exception("Invalid purchaser info.");


            //var systemUser = new IdentityManager().CurrentUser( User );
            //var customer = _context.Customers.SingleOrDefault( o => o.SystemUserId == systemUser.Id );

            // Always defaults ID document to application. 
            var documentCheckLists = new List<DocumentCheckList>();

            //var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
            //required
            var AuthorityToActAttorney = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var ProofOfProperty = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceTypeId));

            //required
            var MunicipalStatement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

            var MunicipalCheckList = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));


            //required
            var BankConfirmationLetter = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankConfirmationLetter);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankConfirmationLetter.Id && dcl.ReferenceTypeId == referenceTypeId));

            //
            var DeedSearch = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DeedSearch);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceTypeId));
            //required
            //this code will change
            var SellerID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceTypeId));
            //required
            var PurchaserID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceTypeId));


            //OccupantsTypes

            var department1 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DepartmentalComents);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == department1.Id && dcl.ReferenceTypeId == referenceTypeId));



            //var Occupant2 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant2);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant2.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var Occupant3 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant3);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant3.Id && dcl.ReferenceTypeId == referenceTypeId));
            //



            //Departmental Comments



            LeaseDetails leaseDetails = _context.LeaseDetails.Where(x => x.PropertyLeaseApplicationId == rcsappId && x.IsActive && !x.IsDeleted).OrderByDescending(x => x.Id).FirstOrDefault();
            List<PropertyResident> PropertyResident = _context.PropertyResidents.Where(x => x.LeaseDetailsId == leaseDetails.Id).ToList();
            int order = 1;
            foreach (var item in PropertyResident)
            {
                switch (order)
                {
                    case (1):
                        {
                            var Occupant1 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant1);

                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant1.Id && dcl.ReferenceTypeId == referenceTypeId));

                            break;
                        }
                    case (2):
                        {
                            var Occupant2 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant2);

                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant2.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;
                        }
                    case (3):
                        {
                            var Occupant3 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant3);

                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant3.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;
                        }
                    case (4):
                        {
                            var Occupant4 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant4);

                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant4.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;
                        }
                    case (5):
                        {
                            var Occupant5 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant5);

                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant5.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;
                        }
                    case (6):
                        {
                            var Occupant6 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant6);

                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant6.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;
                        }
                    case (7):
                        {
                            var Occupant7 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant7);

                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant7.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;
                        }
                    case (8):
                        {
                            var Occupant8 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant8);

                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant8.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;
                        }
                    case (9):
                        {
                            var Occupant9 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant9);

                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant9.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;
                        }

                    case (10):
                        {
                            var Occupant10 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant10);

                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant10.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;
                        }
                    case (11):
                        {
                            var Occupant11 = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant11);

                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant11.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;
                        }


                }

                order++;


            }






            //var ElectricityMeterReading = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ElectricityMeterReading);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ElectricityMeterReading.Id && dcl.ReferenceTypeId == referenceTypeId));


            //try
            //{
            //    var purchasers = _context.PurchaserInformations.Include(x=>x.PurchaserTypes).Where(x => x.RCSApplicationStatusId == RcsApplication.Id).FirstOrDefault();


            //    if(purchasers != null)
            //    {
            //        if (purchasers.PurchaserTypes.Key == PurchaserTypeKeys.Company || purchasers.PurchaserTypes.Key == PurchaserTypeKeys.CloseCorporation)
            //        {
            //            var PurchaserSalesAgreement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

            //            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));


            //        }
            //    }


            //    var walkin = _context.WalkInApplicantDetails.Include(x => x.ActOnBehalfType).Where(x => x.RCSApplicationStatusId == RcsApplication.Id).FirstOrDefault();
            //    if(walkin != null)
            //    {
            //        if (walkin.ActOnBehalfType.Key == ActOnBehalfTypeKeys.ExecutorofEstate )
            //        {
            //            var ExecutorOfestate = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ExecutorofEstate);

            //            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ExecutorOfestate.Id && dcl.ReferenceTypeId == referenceTypeId));


            //        }
            //        if(walkin.ActOnBehalfType.Key == ActOnBehalfTypeKeys.ExecutorofEstate)
            //        {

            //        }
            //    }
            //    else
            //    {

            //    }
            //}
            //catch (Exception)
            //{

            //}




            //var PurchaserSalesAgreement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));
            ////var testtt = PurchaseInfo.PurchaseType;
            //test

            //if(PurchaseInfo.PurchaseType == 2)
            //{

            //}

            // Gets the document types needed for user selection.
            //var ckDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CkDocument);
            //var cipcDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CipcDocument);
            //var baaDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthorithyBlackAdministrationAct);
            //var partnersDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PartnershipAgreement);
            //var ptoDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PtoCertificate);
            //var executorDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Executorship);
            var authorityDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Authority);


            // If there are additional documents need for an application.
            var addDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                ReturnUrl = returnUrl,
                // CustomerId = (agentId == null) ? customerId : referenceId,
                CustomerId = customerId ?? referenceId,
                ApplicationId = (int)applicationId,
                Application = application,
                PropertyLeaseApplicationId = rcsappId,
                RcsApplicationId = rcsappId,
                ReferenceTypeId = (int)referenceTypeId,
                ReferenceType = referenceType,
                ReferenceId = (int)referenceId,
                IsUploadView = true,
                Documents = _context.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList()
            };

            // Checks the reference type of the documents needed.
            switch (referenceType.Key)
            {
                //case ReferenceTypeKeys.LinkedAccounts:
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
                //case ReferenceTypeKeys.LinkedAccountsManagingAgent:
                //    documentCheckLists.Clear();
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
            }


            var addDoc = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            {
                documentCheckLists.Add(addDoc);
            }
            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId && dcl.ApplicationId == applicationId));
            dvm.DocumentCheckLists = documentCheckLists;

            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }

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
                     _context.DocumentCheckLists.Include(d => d.DocumentType)
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
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceId, customerId, applicationId, rcsappId))));
            }
            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();
            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.PropertyLeaseApplication = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }

        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]

        public ActionResult Refund(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");
            
            var RefundApplication = _context.RefundApplications.Where(x => x.Id == rcsappId).Include(r => r.RCSApplicationStatus).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).FirstOrDefault();
            if (RefundApplication == null) throw new Exception("Invalid application.");

            var RcsApplication = _context.RCSApplicationStatus.Where(x => x.Id == RefundApplication.RCSApplicationStatusId).Include(r => r.Clerk).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).FirstOrDefault();
            if (RcsApplication == null) throw new Exception("Invalid application.");

            //var PurchaseInfo = _context.PurchaserInformations.Where(x => x.RCSApplicationStatusId == RcsApplication.Id).Include(r => r.RCSApplicationStatus)/*.Include(r =>r.PurchaseType)*/.FirstOrDefault();
            //if (PurchaseInfo == null) throw new Exception("Invalid purchaser info.");


            //var systemUser = new IdentityManager().CurrentUser( User );
            //var customer = _context.Customers.SingleOrDefault( o => o.SystemUserId == systemUser.Id );

            // Always defaults ID document to application. 
            var documentCheckLists = new List<DocumentCheckList>();

            //var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

            var meterreading = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundMeterReading);

            var watermeterreading = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundWaterMeterReading);


            var bankletter = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundConveyancerBankingDetails);

            var DeedSearch=_context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundDeedSearch);

            var MunicipalAccStatement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundMunicipalStatement);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalAccStatement.Id && dcl.ReferenceTypeId == referenceTypeId));

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceTypeId));

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == meterreading.Id && dcl.ReferenceTypeId == referenceTypeId));

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == watermeterreading.Id && dcl.ReferenceTypeId == referenceTypeId));

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == bankletter.Id && dcl.ReferenceTypeId == referenceTypeId));


            var addDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var dvm = new DocumentsViewModel
            {
                ReturnUrl = returnUrl,
                // CustomerId = (agentId == null) ? customerId : referenceId,
                CustomerId = customerId ?? referenceId,
                ApplicationId = (int)applicationId,
                Application = application,
                RefundApplicationId = RefundApplication.Id,
                RcsApplicationId = RefundApplication.Id,
                ReferenceTypeId = (int)referenceTypeId,
                ReferenceType = referenceType,
                ReferenceId = (int)referenceId,
                IsUploadView = true,
                Documents = _context.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.RefundApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList()
            };

            // Checks the reference type of the documents needed.
          


            var addDoc = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            {
                documentCheckLists.Add(addDoc);
            }
            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId && dcl.ApplicationId == applicationId));
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
                     _context.DocumentCheckLists.Include(d => d.DocumentType)
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
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??refundappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceId, customerId, applicationId, rcsappId))));
            }
            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();
            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }



        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]

        public ActionResult ReUpload(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");
            var RcsApplication = _context.RCSApplicationStatus.Where(x => x.Id == rcsappId).Include(r => r.Clerk).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).FirstOrDefault();
            if (RcsApplication == null) throw new Exception("Invalid application.");

            var PurchaseInfo = _context.PurchaserInformations.Where(x => x.Id == RcsApplication.Id).Include(r => r.RCSApplicationStatus)/*.Include(r =>r.PurchaseType)*/.FirstOrDefault();
            //if (PurchaseInfo == null) throw new Exception("Invalid purchaser info.");


            //var systemUser = new IdentityManager().CurrentUser( User );
            //var customer = _context.Customers.SingleOrDefault( o => o.SystemUserId == systemUser.Id );

            // Always defaults ID document to application. 
            var documentCheckLists = new List<DocumentCheckList>();

            //var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

            var AuthorityToActAttorney = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var ProofOfProperty = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceTypeId));

            //required
            var MunicipalStatement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

            var MunicipalCheckList = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));


            var BankConfirmationLetter = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankConfirmationLetter);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankConfirmationLetter.Id && dcl.ReferenceTypeId == referenceTypeId));



            //this code will change
            var SellerID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceTypeId));

            var PurchaserID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceTypeId));



            var DeedSearch = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DeedSearch);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceTypeId));

            try
            {
                var MunicipalAccInfo = _context.MunicipalAccountInformations.Where(x => x.Id == RcsApplication.MunicipalAccountInformationId).FirstOrDefault();
                var ElectricityInfo = _context.ElectricityMeterInformations.Where(x => x.RCSApplicationStatusId == RcsApplication.Id).ToList();
                var WaterInfo = _context.WaterMeterInformations.Where(x => x.RCSApplicationStatusId == RcsApplication.Id).ToList();
                if (WaterInfo != null && ElectricityInfo != null)
                {
                    if (ElectricityInfo.Count > 0)
                    {
                        var ElectricityMeterReading = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ElectricityMeterReading);

                        documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ElectricityMeterReading.Id && dcl.ReferenceTypeId == referenceTypeId));


                    }

                    if (WaterInfo.Count > 0)
                    {
                        var WaterMeterReading = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.WaterMeterReading);

                        documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == WaterMeterReading.Id && dcl.ReferenceTypeId == referenceTypeId));


                    }

                }

            }
            catch (Exception)
            {

            }

            //var PurchaserSalesAgreement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));
            ////var testtt = PurchaseInfo.PurchaseType;
            //test

            //if(PurchaseInfo.PurchaseType == 2)
            //{

            //}

            // Gets the document types needed for user selection.
            //var ckDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CkDocument);
            //var cipcDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CipcDocument);
            //var baaDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthorithyBlackAdministrationAct);
            //var partnersDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PartnershipAgreement);
            //var ptoDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PtoCertificate);
            //var executorDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Executorship);
            var authorityDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Authority);


            // If there are additional documents need for an application.
            var addDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                ReturnUrl = returnUrl,
                // CustomerId = (agentId == null) ? customerId : referenceId,
                CustomerId = customerId ?? referenceId,
                ApplicationId = (int)applicationId,
                Application = application,
                ApplicationReferenceNumber = RcsApplication.ApplicationReferenceNumber,
                RcsApplicationId = rcsappId,
                ReferenceTypeId = (int)referenceTypeId,
                ReferenceType = referenceType,
                ReferenceId = (int)referenceId,
                IsUploadView = true,
                Documents = _context.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.RCSApplicationStatusId == rcsappId && o.IsActive && !o.IsDeleted).ToList()
            };

            // Checks the reference type of the documents needed.
            switch (referenceType.Key)
            {
                //case ReferenceTypeKeys.LinkedAccounts:
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
                //case ReferenceTypeKeys.LinkedAccountsManagingAgent:
                //    documentCheckLists.Clear();
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
            }


            var addDoc = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            {
                documentCheckLists.Add(addDoc);
            }
            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId && dcl.ApplicationId == applicationId));
            dvm.DocumentCheckLists = documentCheckLists;

            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }

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
                     _context.DocumentCheckLists.Include(d => d.DocumentType)
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
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceId, customerId, applicationId, rcsappId))));
            }
            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();
            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }

        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]

        public ActionResult ProofOfPayment(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");
            var RcsApplication = _context.RCSApplicationStatus.Where(x => x.Id == rcsappId).Include(r => r.Clerk).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).FirstOrDefault();
            if (RcsApplication == null) throw new Exception("Invalid application.");



            //// Always defaults ID document to application. 
            var documentCheckLists = new List<DocumentCheckList>();




            var ProofOfPayment = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfPayment);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == referenceTypeId));

            var authorityDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Authority);


            // If there are additional documents need for an application.
            var addDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                ReturnUrl = returnUrl,
                // CustomerId = (agentId == null) ? customerId : referenceId,
                CustomerId = customerId ?? referenceId,
                ApplicationId = (int)applicationId,
                Application = application,
                RcsApplicationId = rcsappId,
                ReferenceTypeId = (int)referenceTypeId,
                ReferenceType = referenceType,
                ReferenceId = (int)referenceId,
                IsUploadView = true,
                Documents = _context.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.RCSApplicationStatusId == rcsappId && o.IsActive && !o.IsDeleted).ToList()
            };

            switch (referenceType.Key)
            {
              
            }

          dvm.DocumentCheckLists = documentCheckLists;

            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }

            foreach (var customerDocument in dvm.Documents)
            {
               
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     _context.DocumentCheckLists.Include(d => d.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceId, customerId, applicationId, rcsappId))));
            }
            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();
            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }

        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]

        public ActionResult UploadRefundDocs(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int refundappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");
            var RcsApplication = _context.RefundApplications.Where(x => x.Id == refundappId).Include(r => r.Clerk).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).FirstOrDefault();
            if (RcsApplication == null) throw new Exception("Invalid application.");

            //var PurchaseInfo = _context.PurchaserInformations.Where(x => x.Id == RcsApplication.Id).Include(r => r.RCSApplicationStatus)/*.Include(r =>r.PurchaseType)*/.FirstOrDefault();
            ////if (PurchaseInfo == null) throw new Exception("Invalid purchaser info.");


            ////var systemUser = new IdentityManager().CurrentUser( User );
            ////var customer = _context.Customers.SingleOrDefault( o => o.SystemUserId == systemUser.Id );

            //// Always defaults ID document to application. 
            var documentCheckLists = new List<DocumentCheckList>();

            ////var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

            ////documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var AuthorityToActAttorney = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var ProofOfProperty = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var MunicipalStatement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

            //var MunicipalCheckList = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));

            ////this code will change
            //var SellerID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var PurchaserID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceTypeId));


            var RefundMunicipalStatement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RefundMunicipalStatement);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RefundMunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var PurchaserSalesAgreement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));
            ////var testtt = PurchaseInfo.PurchaseType;
            //test

            //if(PurchaseInfo.PurchaseType == 2)
            //{

            //}

            // Gets the document types needed for user selection.
            //var ckDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CkDocument);
            //var cipcDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CipcDocument);
            //var baaDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthorithyBlackAdministrationAct);
            //var partnersDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PartnershipAgreement);
            //var ptoDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PtoCertificate);
            //var executorDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Executorship);
            var authorityDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Authority);


            // If there are additional documents need for an application.
            var addDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                ReturnUrl = returnUrl,
                // CustomerId = (agentId == null) ? customerId : referenceId,
                CustomerId = customerId ?? referenceId,
                ApplicationId = (int)applicationId,
                Application = application,
                RcsApplicationId = refundappId,
                ReferenceTypeId = (int)referenceTypeId,
                ReferenceType = referenceType,
                ReferenceId = (int)referenceId,
                IsUploadView = true,
                Documents = _context.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.RCSApplicationStatusId == refundappId && o.IsActive && !o.IsDeleted).ToList()
            };

            // Checks the reference type of the documents needed.
            switch (referenceType.Key)
            {
                //case ReferenceTypeKeys.LinkedAccounts:
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
                //case ReferenceTypeKeys.LinkedAccountsManagingAgent:
                //    documentCheckLists.Clear();
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
            }


            //var addDoc = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            //if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            //{
            //    documentCheckLists.Add(addDoc);
            //}
            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId && dcl.ApplicationId == applicationId));
            dvm.DocumentCheckLists = documentCheckLists;

            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }

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
                     _context.DocumentCheckLists.Include(d => d.DocumentType)
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
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??refundappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceId, customerId, applicationId, refundappId))));
            }
            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();
            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = refundappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }


        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]

        public ActionResult ProofOfPaymentAssessment(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");
            var RcsApplication = _context.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                      .Where(x => x.Id == rcsappId).FirstOrDefault();
            if (RcsApplication.Status.Key == StatusKeys.PendingAssessmentFeePaymentValidation) return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = rcsappId, appId = "" });


            if (RcsApplication == null) throw new Exception("Invalid application.");

            //var PurchaseInfo = _context.PurchaserInformations.Where(x => x.Id == RcsApplication.Id).Include(r => r.RCSApplicationStatus)/*.Include(r =>r.PurchaseType)*/.FirstOrDefault();
            ////if (PurchaseInfo == null) throw new Exception("Invalid purchaser info.");


            ////var systemUser = new IdentityManager().CurrentUser( User );
            ////var customer = _context.Customers.SingleOrDefault( o => o.SystemUserId == systemUser.Id );

            //// Always defaults ID document to application. 
            var documentCheckLists = new List<DocumentCheckList>();

            ////var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

            ////documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var AuthorityToActAttorney = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var ProofOfProperty = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var MunicipalStatement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

            //var MunicipalCheckList = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));

            ////this code will change
            //var SellerID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var PurchaserID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceTypeId));


            var ProofOfPayment = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var PurchaserSalesAgreement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));
            ////var testtt = PurchaseInfo.PurchaseType;
            //test

            //if(PurchaseInfo.PurchaseType == 2)
            //{

            //}

            // Gets the document types needed for user selection.
            //var ckDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CkDocument);
            //var cipcDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CipcDocument);
            //var baaDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthorithyBlackAdministrationAct);
            //var partnersDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PartnershipAgreement);
            //var ptoDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PtoCertificate);
            //var executorDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Executorship);
            var authorityDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Authority);


            // If there are additional documents need for an application.
            var addDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                ReturnUrl = returnUrl,
                // CustomerId = (agentId == null) ? customerId : referenceId,
                CustomerId = customerId ?? referenceId,
                ApplicationId = (int)applicationId,
                Application = application,
                RcsApplicationId = rcsappId,
                PropertyLeaseApplicationId = rcsappId,
                ReferenceTypeId = (int)referenceTypeId,
                ReferenceType = referenceType,
                ReferenceId = (int)referenceId,
                IsUploadView = true,
                Documents = _context.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList()
            };

            // Checks the reference type of the documents needed.
            switch (referenceType.Key)
            {
                //case ReferenceTypeKeys.LinkedAccounts:
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
                //case ReferenceTypeKeys.LinkedAccountsManagingAgent:
                //    documentCheckLists.Clear();
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
            }


            //var addDoc = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            //if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            //{
            //    documentCheckLists.Add(addDoc);
            //}
            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId && dcl.ApplicationId == applicationId));
            dvm.DocumentCheckLists = documentCheckLists;

            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }

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
                     _context.DocumentCheckLists.Include(d => d.DocumentType)
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
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceId, customerId, applicationId, rcsappId))));
            }
            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();
            System.Globalization.CultureInfo za = new System.Globalization.CultureInfo("en-ZA");
            ViewBag.DepositAmount = _context.ApplicantUnits.FirstOrDefault(r => r.PropertyLeaseApplicationId == rcsappId).OutstandingDepopsitAmount.ToString("C", za);
            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }

        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]

        public ActionResult ProofOfApplicationFeePayment(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");
            var RcsApplication = _context.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                      .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                      .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                      .Where(x => x.Id == rcsappId).FirstOrDefault();
            if (RcsApplication.Status.Key == StatusKeys.AwaitingApplicationFeeValidation) return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = rcsappId, appId = "" });


            if (RcsApplication == null) throw new Exception("Invalid application.");

            //var PurchaseInfo = _context.PurchaserInformations.Where(x => x.Id == RcsApplication.Id).Include(r => r.RCSApplicationStatus)/*.Include(r =>r.PurchaseType)*/.FirstOrDefault();
            ////if (PurchaseInfo == null) throw new Exception("Invalid purchaser info.");


            ////var systemUser = new IdentityManager().CurrentUser( User );
            ////var customer = _context.Customers.SingleOrDefault( o => o.SystemUserId == systemUser.Id );

            //// Always defaults ID document to application. 
            var documentCheckLists = new List<DocumentCheckList>();

            ////var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

            ////documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var AuthorityToActAttorney = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var ProofOfProperty = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var MunicipalStatement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

            //var MunicipalCheckList = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));

            ////this code will change
            //var SellerID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var PurchaserID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceTypeId));


            var ProofOfPayment = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ApplicationFeePOP);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var PurchaserSalesAgreement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));
            ////var testtt = PurchaseInfo.PurchaseType;
            //test

            //if(PurchaseInfo.PurchaseType == 2)
            //{

            //}

            // Gets the document types needed for user selection.
            //var ckDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CkDocument);
            //var cipcDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CipcDocument);
            //var baaDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthorithyBlackAdministrationAct);
            //var partnersDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PartnershipAgreement);
            //var ptoDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PtoCertificate);
            //var executorDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Executorship);
            var authorityDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Authority);


            // If there are additional documents need for an application.
            var addDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                ReturnUrl = returnUrl,
                // CustomerId = (agentId == null) ? customerId : referenceId,
                CustomerId = customerId ?? referenceId,
                ApplicationId = (int)applicationId,
                Application = application,
                RcsApplicationId = rcsappId,
                PropertyLeaseApplicationId = rcsappId,
                ReferenceTypeId = (int)referenceTypeId,
                ReferenceType = referenceType,
                ReferenceId = (int)referenceId,
                IsUploadView = true,
                Documents = _context.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList()
            };

            // Checks the reference type of the documents needed.
            switch (referenceType.Key)
            {
                //case ReferenceTypeKeys.LinkedAccounts:
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
                //case ReferenceTypeKeys.LinkedAccountsManagingAgent:
                //    documentCheckLists.Clear();
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
            }


            //var addDoc = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            //if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            //{
            //    documentCheckLists.Add(addDoc);
            //}
            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId && dcl.ApplicationId == applicationId));
            dvm.DocumentCheckLists = documentCheckLists;

            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }

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
                     _context.DocumentCheckLists.Include(d => d.DocumentType)
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
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceId, customerId, applicationId, rcsappId))));
            }
            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();
            System.Globalization.CultureInfo za = new System.Globalization.CultureInfo("en-ZA");
            //ViewBag.DepositAmount = _context.ApplicantUnits.FirstOrDefault(r => r.PropertyLeaseApplicationId == rcsappId).OutstandingDepopsitAmount.ToString("C", za);
            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }


        public ActionResult AcceptanceLetter(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int rcsappId, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");
            var RcsApplication = _context.RCSApplicationStatus.Where(x => x.Id == rcsappId).Include(r => r.Clerk).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).FirstOrDefault();
            if (RcsApplication == null) throw new Exception("Invalid application.");

            //var PurchaseInfo = _context.PurchaserInformations.Where(x => x.Id == RcsApplication.Id).Include(r => r.RCSApplicationStatus)/*.Include(r =>r.PurchaseType)*/.FirstOrDefault();
            ////if (PurchaseInfo == null) throw new Exception("Invalid purchaser info.");


            ////var systemUser = new IdentityManager().CurrentUser( User );
            ////var customer = _context.Customers.SingleOrDefault( o => o.SystemUserId == systemUser.Id );

            //// Always defaults ID document to application. 
            var documentCheckLists = new List<DocumentCheckList>();

            ////var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

            ////documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var AuthorityToActAttorney = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var ProofOfProperty = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceTypeId));


            //var MunicipalStatement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

            //var MunicipalCheckList = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId));

            ////this code will change
            //var SellerID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var PurchaserID = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceTypeId));


            var ProofOfPayment = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssessmentProofOfPayment);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfPayment.Id && dcl.ReferenceTypeId == referenceTypeId));

            //var PurchaserSalesAgreement = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserPurchaseAgreement);

            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserSalesAgreement.Id && dcl.ReferenceTypeId == referenceTypeId));
            ////var testtt = PurchaseInfo.PurchaseType;
            //test

            //if(PurchaseInfo.PurchaseType == 2)
            //{

            //}

            // Gets the document types needed for user selection.
            //var ckDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CkDocument);
            //var cipcDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CipcDocument);
            //var baaDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthorithyBlackAdministrationAct);
            //var partnersDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PartnershipAgreement);
            //var ptoDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PtoCertificate);
            //var executorDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Executorship);
            var authorityDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Authority);


            // If there are additional documents need for an application.
            var addDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                ReturnUrl = returnUrl,
                // CustomerId = (agentId == null) ? customerId : referenceId,
                CustomerId = customerId ?? referenceId,
                ApplicationId = (int)applicationId,
                Application = application,
                RcsApplicationId = rcsappId,
                ReferenceTypeId = (int)referenceTypeId,
                ReferenceType = referenceType,
                ReferenceId = (int)referenceId,
                IsUploadView = true,
                Documents = _context.Documents.Include(o => o.File).Include(o => o.Status).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.RCSApplicationStatusId == rcsappId && o.IsActive && !o.IsDeleted).ToList()
            };

            // Checks the reference type of the documents needed.
            switch (referenceType.Key)
            {
                //case ReferenceTypeKeys.LinkedAccounts:
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
                //case ReferenceTypeKeys.LinkedAccountsManagingAgent:
                //    documentCheckLists.Clear();
                //    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //    break;
            }


            //var addDoc = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            //if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            //{
            //    documentCheckLists.Add(addDoc);
            //}
            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId && dcl.ApplicationId == applicationId));
            dvm.DocumentCheckLists = documentCheckLists;

            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }

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
                     _context.DocumentCheckLists.Include(d => d.DocumentType)
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
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceId, customerId, applicationId, rcsappId))));
            }
            var CustType = _context.Customers.Where(o => o.SystemUserId == SystemUserId).FirstOrDefault();
            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.RcsAppId = rcsappId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }

        // GET: /Document/
        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]

        public ActionResult Register(int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, int? agentId, string returnUrl, int? ratesRebateId, int? incentivePolicyId, string errorList = null)
        {
            eServicesDbContext _context = new eServicesDbContext();
            if (referenceId == null || referenceTypeId == null || applicationId == null)
                return HttpNotFound();
            _base.Initialise(_context);
            var agent = _base.Agent;

            if (User != null && User.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(User);
                SystemUserId = IdentityManager.CurrentUser(User).Id;
            }

            var test = _base.Customer;

            var referenceType = _context.ReferenceTypes.Find(referenceTypeId);
            var application = _context.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            //var systemUser = new IdentityManager().CurrentUser( User );
            //var customer = _context.Customers.SingleOrDefault( o => o.SystemUserId == systemUser.Id );

            // Always defaults ID document to application. 
            var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);
            var documentCheckLists = new List<DocumentCheckList>();
            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));

            // Gets the document types needed for user selection.
            //var ckDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CkDocument);
            //var cipcDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CipcDocument);
            //var baaDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthorithyBlackAdministrationAct);
            //var partnersDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PartnershipAgreement);
            //var ptoDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PtoCertificate);
            //var executorDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Executorship);
            var authorityDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Authority);


            // If there are additional documents need for an application.
            var addDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();
            var dvm = new DocumentsViewModel
            {
                ReturnUrl = returnUrl,
                // CustomerId = (agentId == null) ? customerId : referenceId,
                CustomerId = customerId ?? referenceId,
                ApplicationId = (int)applicationId,
                Application = application,
                ReferenceTypeId = (int)referenceTypeId,
                ReferenceType = referenceType,
                ReferenceId = (int)referenceId,
                IsUploadView = true,
                Documents = _context.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.IsActive && !o.IsDeleted).ToList()
            };
          
            // Checks the reference type of the documents needed.
            switch (referenceType.Key)
            {
                case ReferenceTypeKeys.LinkedAccounts:
                    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                    break;
                case ReferenceTypeKeys.LinkedAccountsManagingAgent:
                    documentCheckLists.Clear();
                    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId ));
                    break;
            }

            var addDoc = _context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            {
                documentCheckLists.Add(addDoc);
            }
            //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId && dcl.ApplicationId == applicationId));
            dvm.DocumentCheckLists = documentCheckLists;

            if (application.Key == ApplicationKeys.RatesRebate)
            {
                ViewBag.RatesRebateStatus = ratesRebateProperty.RatesRebate.Status.Key;
                ViewBag.PropertyId = ratesRebateProperty.PropertyId;

                var nav = new NavigationProperty
                {
                    CustomerId = ratesRebateProperty.RatesRebate.OwnerCustomerId,
                    PropertyId = ratesRebateProperty == null ? 0 : ratesRebateProperty.PropertyId,
                    RatesRebateId = ratesRebateProperty.RatesRebate.Id,
                    AgentId = agent == null ? 0 : agent.Id,
                    ApplicationId = application.Id,
                    ReferenceTypeId = referenceType.Id,
                    ReferenceId = referenceId,
                    Step = ViewCodeKeys.StepFive
                };

                ViewBag.NavigationParameters = nav;
            }
 
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
                     _context.DocumentCheckLists.Include(d => d.DocumentType)
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
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceId, customerId, applicationId))));
            }
            var CustType = _context.Customers.Where(o=>o.CreatedBySystemUserId==SystemUserId).FirstOrDefault();
            ViewBag.Application = application.Key;
            ViewBag.ReferenceId = referenceId;
            ViewBag.ApplicationId = applicationId;
            ViewBag.ReferenceTypeId = referenceTypeId;
            ViewBag.CustomerId = customerId;
            ViewBag.IncentivePolicyId = incentivePolicyId;
            ViewBag.Data = SecureActionLinkExtension.Encrypt(string.Format("incentivePolicyId={0}", incentivePolicyId));
            ViewBag.CustomerType = CustType.CustomerTypeId;
            return View(dvm);
        }


        public ActionResult Index2()
        {
            eServicesDbContext context = new eServicesDbContext();
            _base.Initialise(context);

            try
            {
                var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                               .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == 13);
                if (customer == null) throw new Exception("Invalid Customer");

                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.LinkedAccounts));

                var documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.CustomerProfile);
                if (documentReferenceType == null)
                    throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                        ReferenceTypeKeys.CustomerProfile));

                var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                                        .Include(d => d.ReferenceType)
                                        .Where(dc => dc.ApplicationId == application.Id &&
                                        dc.ReferenceTypeId == documentReferenceType.Id);

                var vm = new CustomerProfileViewModel();
                var documents = context.Documents.Where(d => d.CustomerId == customer.Id).ToList();
                var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                var customerDocuments = context.Documents.Include(d => d.File)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false).ToList();

                foreach (var doc in customerDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                }

                var dvm = new DocumentsViewModel
                {
                    ApplicationId = application.Id,
                    CustomerId = customer.Id,
                    Documents = customerDocuments,
                    IsUploadView = false,
                    DocumentCheckLists = documentCheckLists.ToList(),
                };

                Entity entity = null;
                Agent agent = null;

                if (customer.CustomerType.Key == CustomerTypeKeys.Entity)
                {
                    entity = context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id);
                }
                else if (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent)
                {
                    agent = context.Agents.FirstOrDefault(a => a.CustomerId == customer.Id);

                    if (agent != null)
                    {
                        var entityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);

                        entity = (entityAgent != null)
                            ? context.Entities.Find(entityAgent.EntityId)
                            : context.Entities.First(e => e.CustomerId == agent.CustomerId);
                    }
                }

                vm.Customer = customer;
                vm.Entity = entity;
                vm.Agent = agent;
                vm.CustomerDocuments = documents;
                vm.Notes = notes;
                vm.Document = dvm;

                ViewBag.ReferenceTypeId = documentReferenceType.Id;
                ViewBag.ApplicationId = application.Id;

                var obj = new
                {
                    IdentificationNumber = vm.Customer.IdentificationNumber,
                    FullName = vm.Customer.FullName,
                    EmailAddress = vm.Customer.SystemUser == null ? vm.Customer.EmailAddress : vm.Customer.SystemUser.EmailAddress,
                    SystemUserId = vm.Customer.SystemUser == null ? 0 : vm.Customer.SystemUserId,
                    CustomerId = vm.Customer.Id
                };

                ViewBag.CustomerModel = obj;

                return View(vm);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
        }
        /// <summary>
        /// Gets Document checklists for an Application
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="referenceTypeKey"></param>
        /// <returns></returns>
        public List<DocumentCheckList> GetDocumentCheckLists(object obj, string referenceTypeKey)
        {
            var _context = new eServicesDbContext();
            var documentCheckLists = new List<DocumentCheckList>();

            var referenceType =
             _context.ReferenceTypes.FirstOrDefault(r => r.Key == referenceTypeKey);
            //var referenceType =
            //  _context.ReferenceTypes.FirstOrDefault(r => r.Key == ReferenceTypeKeys.RatesRebateProperty);
            if (referenceType == null) throw new Exception(string.Format("Invalid/ missing reference type key - {0}", ReferenceTypeKeys.RatesRebateProperty));

            // Always defaults ID document to application. 
            var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);          
            // If there are additional documents need for an application.
            var addDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            // Gets the document types needed for user selection.
            var ckDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CkDocument);
            var cipcDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CipcDocument);
            var baaDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthorithyBlackAdministrationAct);
            var partnersDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PartnershipAgreement);
            var ptoDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PtoCertificate);
            var authorityDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Authority);
            var executorDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Executorship);

            var brDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BuildingSpecialistReport);
            var raDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CopyOfRatesAccount);
            var mbDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CopyMunicipalBill);
            var lgsDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.LetterOfGoodStanding);
            var taxClearanceDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.TaxClearanceCertificate);
            var varRegDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.VatRegistrationCertificate);
            var bpDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BuildingPlan);
            var costDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MilestoneAndCostSchedule);

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
            switch (referenceTypeKey)
            {
                case ReferenceTypeKeys.RatesRebateProperty:
                    var ratesRebateProperty = (RatesRebateProperty)obj;
                    var applicantCustomer = _context.Customers.Include(c => c.CustomerType).FirstOrDefault(c => c.Id == ratesRebateProperty.RatesRebate.ApplicantCustomerId);
                    if (applicantCustomer == null) throw new Exception("Missing Applicant Customer");

                    // Checks if application was done as a thrid party.
                    if (!ratesRebateProperty.RatesRebate.InOwnCapacity || applicantCustomer.CustomerType.Key == CustomerTypeKeys.ManagingAgent)
                        documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == authorityDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));

                    // Checks if application was done as an executor.
                    if (ratesRebateProperty.RatesRebate.ExecutorId > 0 || ratesRebateProperty.RatesRebate.ExecutorCustomerId > 0)
                        documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == executorDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));

                    var customerType = _context.CustomerTypes.FirstOrDefault(ct => ct.Id == ratesRebateProperty.Customer.CustomerTypeId && ct.IsActive && !ct.IsDeleted);
                    if (null == customerType) throw new Exception("Invalid or no customer type.");

                    // Builds up the document check list based on the user selections in the application.
                    switch (customerType.Key)
                    {
                        case CustomerTypeKeys.DeemedOwner:
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == baaDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            break;
                        case CustomerTypeKeys.Individual:
                            break;
                        case CustomerTypeKeys.PTO:
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ptoDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            break;
                        case CustomerTypeKeys.Entity:
                            var entity = _context.Entities.FirstOrDefault(e => e.CustomerId == ratesRebateProperty.Customer.Id);
                            var entityType = _context.EntityTypes.FirstOrDefault(et => et.Id == entity.EntityTypeId);
                            if (entityType == null) throw new Exception("Invalid or no entity type.");

                            // Checks what entity the application pertains to and selects the relevant document.
                            switch (entityType.Key)
                            {
                                case EntityTypeKey.CloseCorporation:
                                    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ckDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                                    break;
                                case EntityTypeKey.Company:
                                    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == cipcDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                                    break;
                                case EntityTypeKey.Organisation:
                                    break;
                                case EntityTypeKey.Partnership:
                                    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == partnersDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                                    break;
                                case EntityTypeKey.SoleProprietory:
                                    break;
                            }
                            break;
                    }
                    break;
                case ReferenceTypeKeys.IncentivePolicyProperty:
                    referenceType =
                        _context.ReferenceTypes.FirstOrDefault(r => r.Key == ReferenceTypeKeys.IncentivePolicyProperty);
                    if (referenceType == null) throw new Exception(string.Format("Invalid/ missing reference type key - {0}", ReferenceTypeKeys.RatesRebateProperty));

                    var incentivePolicyProperty = (IncentivePolicyProperty)obj;

                    customerType = _context.CustomerTypes.FirstOrDefault(ct => ct.Id == incentivePolicyProperty.IncentivePolicy.Customer.CustomerTypeId && ct.IsActive && !ct.IsDeleted);
                    if (null == customerType) throw new Exception("Invalid or no customer type.");

                    // Builds up the document check list based on the user selections in the application.
                    switch (customerType.Key)
                    {
                        case CustomerTypeKeys.DeemedOwner:
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == baaDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            break;
                        case CustomerTypeKeys.Individual:
                            break;
                        case CustomerTypeKeys.PTO:
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ptoDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            break;
                        case CustomerTypeKeys.Entity:
                            var entity = _context.Entities.FirstOrDefault(e => e.CustomerId == incentivePolicyProperty.IncentivePolicy.Customer.Id);
                            var entityType = _context.EntityTypes.FirstOrDefault(et => et.Id == entity.EntityTypeId);
                            if (entityType == null) throw new Exception("Invalid or no entity type.");

                            //Add Incentive Policy Documents
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == raDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == lgsDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == taxClearanceDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == varRegDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == bpDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == costDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == mbDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == brDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                            // Checks what entity the application pertains to and selects the relevant document.
                            switch (entityType.Key)
                            {
                                case EntityTypeKey.CloseCorporation:
                                    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ckDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                                    break;
                                case EntityTypeKey.Company:
                                    documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == cipcDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                                    break;
                                case EntityTypeKey.Organisation:
                                    break;
                                case EntityTypeKey.Partnership:
                                   // documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == partnersDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));
                                    break;
                                case EntityTypeKey.SoleProprietory:
                                    break;
                            }
                            break;
                    }
                    break;                 
            }

            documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id));

            return documentCheckLists;
        }

        /// <summary>
        /// Renders the document details.
        /// </summary>
        /// <param name="documentCheckListId">The document check list identifier.</param>
        /// <param name="referenceTypeId">The reference type identifier.</param>
        /// <param name="referenceId">The reference identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="applicationId">The application identifier.</param>
        /// <returns></returns>
        [EncryptedActionParameter]
        [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Customer, Housing Liaison Officer, Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer, Revenue Officer")]

        public ActionResult RenderDocumentDetails(int documentCheckListId, int referenceTypeId, int referenceId, int customerId, int applicationId, int rcsappId)
        {
            eServicesDbContext _context = new eServicesDbContext();
            List<Document> docs =
            _context.Documents.Include(d => d.File)
            .Include(d => d.LocationType)
            .Include(d => d.DocumentCheckList)
            .Include(d=> d.DocumentCheckList.DocumentType)
            .Include(d => d.DocumentCheckList.Application).Where(
               d =>
                d.DocumentCheckListId == documentCheckListId && d.ReferenceTypeId == referenceTypeId &&
                d.ReferenceId == referenceId && d.CustomerId == customerId &&
                d.DocumentCheckList.ApplicationId == applicationId && d.PropertyLeaseApplicationId == rcsappId && d.IsActive && !d.IsDeleted).ToList();
                
            foreach (var doc in docs)
            {
                doc.DocumentLocation = string.Format("uploads/{0}/{1}", doc.DocumentCheckList.DocumentType.Name,doc.DocumentName);
                if (doc.LocationType.Key == LocationTypeKeys.Database) 
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));

                doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));
            }

            var obj = new
            {
                status = docs != null && docs.Count > 0 ? "Success" : "Failure",
                view = RenderHelper.PartialView(this, "_DocumentDetailsPartial", docs)
            };

            return Json(obj);
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Caretaker, Senior Housing Specialist, Regional Manager,Housing Liaison Officer, Customer,  Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer")]

        public ActionResult HumanRenderDocumentDetails(int documentCheckListId, int referenceTypeId, int referenceId, int customerId, int applicationId, int rcsappId)
        {
            eServicesDbContext _context = new eServicesDbContext();
            List<Document> docs =
            _context.Documents.Include(d => d.File)
            .Include(d => d.LocationType)
            .Include(d => d.DocumentCheckList)
            .Include(d => d.DocumentCheckList.DocumentType)
            .Include(d => d.DocumentCheckList.Application).Where(
               d =>
                d.DocumentCheckListId == documentCheckListId && d.ReferenceTypeId == referenceTypeId &&
                d.ReferenceId == referenceId && d.CustomerId == customerId &&
                d.DocumentCheckList.ApplicationId == applicationId && d.HumanSettlementApplicationId == rcsappId && d.IsActive && !d.IsDeleted).ToList();

            foreach (var doc in docs)
            {
                doc.DocumentLocation = string.Format("uploads/{0}/{1}", doc.DocumentCheckList.DocumentType.Name, doc.DocumentName);
                if (doc.LocationType.Key == LocationTypeKeys.Database)
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));

                doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));
            }

            var obj = new
            {
                status = docs != null && docs.Count > 0 ? "Success" : "Failure",
                view = RenderHelper.PartialView(this, "_DocumentDetailsPartial", docs)
            };

            return Json(obj);
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Caretaker, Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]

        public ActionResult RefundDocumentDetails(int documentCheckListId, int referenceTypeId, int referenceId, int customerId, int applicationId, int refundappId)
        {
            eServicesDbContext _context = new eServicesDbContext();
            List<Document> docs =
            _context.Documents.Include(d => d.File)
            .Include(d => d.LocationType)
            .Include(d => d.DocumentCheckList)
            .Include(d => d.DocumentCheckList.DocumentType)
            .Include(d => d.DocumentCheckList.Application).Where(
               d =>
                d.DocumentCheckListId == documentCheckListId && d.ReferenceTypeId == referenceTypeId &&
                d.ReferenceId == referenceId && d.CustomerId == customerId &&
                d.DocumentCheckList.ApplicationId == applicationId && d.RefundApplicationId == refundappId && d.IsActive && !d.IsDeleted).ToList();

            foreach (var doc in docs)
            {
                doc.DocumentLocation = string.Format("uploads/{0}/{1}", doc.DocumentCheckList.DocumentType.Name, doc.DocumentName);
                if (doc.LocationType.Key == LocationTypeKeys.Database)
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));

                doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));
            }

            var obj = new
            {
                status = docs != null && docs.Count > 0 ? "Success" : "Failure",
                view = RenderHelper.PartialView(this, "_DocumentDetailsPartial", docs)
            };

            return Json(obj);
        }
        [EncryptedActionParameter]
      [Authorize(Roles = "Senior Housing Specialist, Regional Manager, Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer, Client Services Officer,Revenue Officer")]

        public ActionResult RenderRegisterDocumentDetails(int documentCheckListId, int referenceTypeId, int referenceId, int customerId, int applicationId)
        {
            eServicesDbContext _context = new eServicesDbContext();
            List<Document> docs =
            _context.Documents.Include(d => d.File)
            .Include(d => d.LocationType)
            .Include(d => d.DocumentCheckList)
            .Include(d => d.DocumentCheckList.DocumentType)
            .Include(d => d.DocumentCheckList.Application).Where(
               d =>
                d.DocumentCheckListId == documentCheckListId && d.ReferenceTypeId == referenceTypeId &&
                d.ReferenceId == referenceId && d.CustomerId == customerId &&
                d.DocumentCheckList.ApplicationId == applicationId && d.IsActive && !d.IsDeleted).ToList();

            foreach (var doc in docs)
            {
                doc.DocumentLocation = string.Format("uploads/{0}/{1}", doc.DocumentCheckList.DocumentType.Name, doc.DocumentName);
                if (doc.LocationType.Key == LocationTypeKeys.Database)
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));

                doc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", doc.Id)));
            }

            var obj = new
            {
                status = docs != null && docs.Count > 0 ? "Success" : "Failure",
                view = RenderHelper.PartialView(this, "_DocumentDetailsPartial", docs)
            };

            return Json(obj);
        }
    }
}