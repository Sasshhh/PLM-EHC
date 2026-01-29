using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using C8.eServices.Mvc.Controllers;
using Org.BouncyCastle.Ocsp;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Collections.ObjectModel;
using System.Web.UI.WebControls;

namespace C8.eServices.Mvc.Helpers
{
    public static class MatchingHelper
    {
        public static string RegistrationSave(eServicesDbContext core, ClerkRegistration cr)
        {
            core.ClerkRegistrations.Add(cr);
            core.SaveChanges();
            return "Successful";
        }
        public static void MatchUnitParallelProcessor(eServicesDbContext core)
        {

            var finfItem = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
            finfItem.Value = "1";
            core.SaveChanges();

            var waitingQueue = core.waitingListQues.OrderBy(x => x.Position).Where(x => !x.IsMatched && x.IsActive && !x.IsDeleted && x.Position != null).ToList() ?? null;

            foreach (var item in waitingQueue)
            {
                DoWork(core, item);
            }

            finfItem.Value = "0";
            core.SaveChanges();
        }
        public static DocumentsViewModel DocumentCaptureApplication(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            var addDocumentType = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);


            var PayslipPensionGrant = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PayslipPensionGrant);
            var BankStatement = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankStatement);
            var IdentityDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdentityDocument);
            var ProofOfEmployment = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfEmployment);
            var Affidavit = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Affidavit);
            var ProofOfAddress = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfAddress);
            var BirthCertificate = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BirthCertificate);

            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == IdentityDocument.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PayslipPensionGrant.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankStatement.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfEmployment.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Affidavit.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfAddress.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BirthCertificate.Id && dcl.ReferenceTypeId == referenceTypeId));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            var addDoc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            {
                documentCheckLists.Add(addDoc);
            }

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentRiskAssessmentOutcome(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var RiskAssessmrnt = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RiskAssessmentOutcomes);
            var ITC_Check = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ITCCheck);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentRiskAssessment);

            //Add to checklists
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ITC_Check.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RiskAssessmrnt.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceTypeId));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentRiskAssessmentHuman(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var RiskAssessmrnt = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RiskAssessmentOutcomes);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentRiskAssessment);

            //Add to checklists
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RiskAssessmrnt.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceTypeId));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.HumanSettlementApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.HumanSettlementApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentCaptureTenantLease(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            //var AuthorityToActAttorney = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);
            //var MunicipalStatement = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);
            //var BankConfirmationLetter = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankConfirmationLetter);
            ////var DeedSearch = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DeedSearch);
            //var SellerID = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);
            //var PurchaserID = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);
            var addDocumentType = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);
            //var MunicipalCheckList = core.DocumentCheckLists.SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceTypeId);


            var PayslipPensionGrant = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PayslipPensionGrant);
            var BankStatement = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankStatement);
            var IdentityDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdentityDocument);
            var ProofOfEmployment = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfEmployment);
            var Affidavit = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Affidavit);
            var ProofOfAddress = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfAddress);



            //Add to checklists
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == IdentityDocument.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PayslipPensionGrant.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankStatement.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfEmployment.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Affidavit.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfAddress.Id && dcl.ReferenceTypeId == referenceTypeId));


            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            var addDoc = core.DocumentCheckLists.Include(o => o.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            {
                documentCheckLists.Add(addDoc);
            }

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentCaptureAddOccupantsDocuments(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            var addDocumentType = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var PayslipPensionGrant = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PayslipPensionGrant);
            var BankStatement = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankStatement);
            var IdentityDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdentityDocument);
            var ProofOfEmployment = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfEmployment);
            var Affidavit = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Affidavit);
            var ProofOfAddress = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfAddress);

            LeaseDetails leaseDetails = core.LeaseDetails.Where(x => x.PropertyLeaseApplicationId == rcsappId && x.IsActive && !x.IsDeleted).OrderByDescending(x => x.Id).FirstOrDefault();
            List<PropertyResident> PropertyResident = null;
            var Keys = core.Status.FirstOrDefault(r => r.Key == StatusKeys.ActiveOccupant).Id;
            if (leaseDetails != null)
                PropertyResident = core.PropertyResidents.OrderByDescending(r => r.Id).Where(x => x.LeaseDetailsId == leaseDetails.Id && x.StatusId == Keys).ToList();
            int order = 1;
            if (PropertyResident != null)
                foreach (var item in PropertyResident)
                {
                    switch (order)
                    {
                        case (1):
                            var Occupant1 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant1);
                            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant1.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;

                        case (2):
                            var Occupant2 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant2);
                            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant2.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;

                        case (3):
                            var Occupant3 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant3);
                            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant3.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;

                        case (4):
                            var Occupant4 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant4);
                            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant4.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;

                        case (5):
                            var Occupant5 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant5);
                            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant5.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;

                        case (6):
                            var Occupant6 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant6);
                            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant6.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;

                        case (7):
                            var Occupant7 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant7);
                            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant7.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;

                        case (8):
                            var Occupant8 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant8);
                            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant8.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;

                        case (9):
                            var Occupant9 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant9);
                            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant9.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;

                        case (10):
                            var Occupant10 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant10);
                            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant10.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;

                        case (11):
                            var Occupant11 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant11);
                            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant11.Id && dcl.ReferenceTypeId == referenceTypeId));
                            break;
                    }
                    order++;
                }

            var findItem = core.LeaseDetails.OrderByDescending(p => p.Id).Include(p => p.PropertyLeaseApplication).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsappId);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.LeaseDetailsId = findItem.Id;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            var addDoc = core.DocumentCheckLists.Include(o => o.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            {
                documentCheckLists.Add(addDoc);
            }

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentUploadHumanOccupants(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            var addDocumentType = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var PayslipPensionGrant = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PayslipPensionGrant);
            var BankStatement = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankStatement);
            var IdentityDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdentityDocument);
            var ProofOfEmployment = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfEmployment);
            var Affidavit = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Affidavit);
            var ProofOfAddress = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfAddress);

            var PlmApps = core.HumanSettlementApplications.Where(r => r.Id == rcsappId).FirstOrDefault();
            var PropertyResident = core.HSUnitOccupants.Include(d => d.Status).Where(r => r.HumanSettlementApplicationId == PlmApps.Id && r.Status.Key == StatusKeys.ActiveOccupant && !r.IsDeleted).ToList();

            var Keys = core.Status.FirstOrDefault(r => r.Key == StatusKeys.ActiveOccupant).Id;
            int order = 1;
            if (PropertyResident != null)
                foreach (var item in PropertyResident)
                {
                    var Doc = new DocumentCheckList();
                    switch (order)
                    {
                        case (1):
                            var Occupant1 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant1);
                            Doc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant1.Id && dcl.ReferenceTypeId == referenceTypeId);
                            Doc.DocumentType.Name = string.Format("ID Document", item.FirstName, item.LastName);
                            Doc.DocumentType.Description = string.Format("Identity Document for {0} {1}", item.FirstName, item.LastName);
                            documentCheckLists.Add(Doc);
                            break;

                        case (2):
                            var Occupant2 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant2);
                            Doc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant2.Id && dcl.ReferenceTypeId == referenceTypeId);
                            Doc.DocumentType.Name = string.Format("ID Document", item.FirstName, item.LastName);
                            Doc.DocumentType.Description = string.Format("Identity Document for {0} {1}", item.FirstName, item.LastName);
                            documentCheckLists.Add(Doc); 
                            break;

                        case (3):
                            var Occupant3 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant3);
                            Doc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant3.Id && dcl.ReferenceTypeId == referenceTypeId);
                            Doc.DocumentType.Name = string.Format("ID Document", item.FirstName, item.LastName);
                            Doc.DocumentType.Description = string.Format("Identity Document for {0} {1}", item.FirstName, item.LastName);
                            documentCheckLists.Add(Doc); 
                            break;

                        case (4):
                            var Occupant4 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant4);
                            Doc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant4.Id && dcl.ReferenceTypeId == referenceTypeId);
                            Doc.DocumentType.Name = string.Format("ID Document", item.FirstName, item.LastName);
                            Doc.DocumentType.Description = string.Format("Identity Document for {0} {1}", item.FirstName, item.LastName);
                            documentCheckLists.Add(Doc); 
                            break;

                        case (5):
                            var Occupant5 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant5);
                            Doc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant5.Id && dcl.ReferenceTypeId == referenceTypeId);
                            Doc.DocumentType.Name = string.Format("ID Document", item.FirstName, item.LastName);
                            Doc.DocumentType.Description = string.Format("Identity Document for {0} {1}", item.FirstName, item.LastName);
                            documentCheckLists.Add(Doc); 
                            break;

                        case (6):
                            var Occupant6 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant6);
                            Doc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant6.Id && dcl.ReferenceTypeId == referenceTypeId);
                            Doc.DocumentType.Name = string.Format("ID Document", item.FirstName, item.LastName);
                            Doc.DocumentType.Description = string.Format("Identity Document for {0} {1}", item.FirstName, item.LastName);
                            documentCheckLists.Add(Doc); 
                            break;

                        case (7):
                            var Occupant7 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant7);
                            Doc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant7.Id && dcl.ReferenceTypeId == referenceTypeId);
                            Doc.DocumentType.Name = string.Format("ID Document", item.FirstName, item.LastName);
                            Doc.DocumentType.Description = string.Format("Identity Document for {0} {1}", item.FirstName, item.LastName);
                            documentCheckLists.Add(Doc); 
                            break;

                        case (8):
                            var Occupant8 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant8);
                            Doc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant8.Id && dcl.ReferenceTypeId == referenceTypeId);
                            Doc.DocumentType.Name = string.Format("ID Document", item.FirstName, item.LastName);
                            Doc.DocumentType.Description = string.Format("Identity Document for {0} {1}", item.FirstName, item.LastName);
                            documentCheckLists.Add(Doc); 
                            break;

                        case (9):
                            var Occupant9 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant9);
                            Doc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant9.Id && dcl.ReferenceTypeId == referenceTypeId);
                            Doc.DocumentType.Name = string.Format("ID Document", item.FirstName, item.LastName);
                            Doc.DocumentType.Description = string.Format("Identity Document for {0} {1}", item.FirstName, item.LastName);
                            documentCheckLists.Add(Doc); 
                            break;

                        case (10):
                            var Occupant10 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant10);
                            Doc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant10.Id && dcl.ReferenceTypeId == referenceTypeId);
                            Doc.DocumentType.Name = string.Format("ID Document", item.FirstName, item.LastName);
                            Doc.DocumentType.Description = string.Format("Identity Document for {0} {1}", item.FirstName, item.LastName);
                            documentCheckLists.Add(Doc); 
                            break;

                        case (11):
                            var Occupant11 = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Occupant11);
                            Doc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Occupant11.Id && dcl.ReferenceTypeId == referenceTypeId);
                            Doc.DocumentType.Name = string.Format("ID Document", item.FirstName, item.LastName);
                            Doc.DocumentType.Description = string.Format("Identity Document for {0} {1}", item.FirstName, item.LastName);
                            documentCheckLists.Add(Doc); 
                            break;
                    }
                    order++;
                }

            var findItem = core.LeaseDetails.OrderByDescending(p => p.Id).Include(p => p.PropertyLeaseApplication).FirstOrDefault(x => x.PropertyLeaseApplicationId == rcsappId);

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.HumanSettlementApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            //dvm.LeaseDetailsId = findItem.Id;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.HumanSettlementApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            var addDoc = core.DocumentCheckLists.Include(o => o.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            {
                documentCheckLists.Add(addDoc);
            }

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentConductUnitInspection(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var CompleteUnitInspection = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CompleteUnitInspection);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentConductUnitInspection);
            var addDocumentType = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == CompleteUnitInspection.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentConductUnitInspection2(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var CompleteUnitInspection = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CompleteUnitInspection);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentConductUnitInspection);
            var addDocumentType = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == CompleteUnitInspection.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.HumanSettlementApplicationId = rcsappId;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.HumanSettlementApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentConductMaintanaceJobSheet(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var MaintananceJobSheet = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MaintananceJobSheet);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentMaintanaceJobSheet);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MaintananceJobSheet.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            //dvm.AllocatedUnitMaintenanceId = 
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }

        public static DocumentsViewModel DocumentConductExitMaintanaceJobSheet(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var MaintananceJobSheet = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ExitMaintananceJobSheet);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentMaintanaceJobSheet);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MaintananceJobSheet.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            //dvm.AllocatedUnitMaintenanceId = 
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }

        public static DocumentsViewModel DocumentConductMaintanaceJobSheet2(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var MaintananceJobSheet = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MaintananceJobSheet);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentMaintanaceJobSheet);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MaintananceJobSheet.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.HumanSettlementApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.HumanSettlementApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentConductConductExitInspection(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var ConductExitInspection = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ConductExitInspection);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentExitInspection);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ConductExitInspection.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentTenantAccountValidation(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var FinancialSupportingDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.FinancialSupportingDocument);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentAccountValidation);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == FinancialSupportingDocument.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentPropertyEvictionValidation(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var PropertyEvictionDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PropertyEvictionDocument);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentPropertyEviction);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PropertyEvictionDocument.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentPropertyEvictionValidationHuman(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var PropertyEvictionDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PropertyEvictionDocument);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentPropertyEviction);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PropertyEvictionDocument.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentEvictionCommitteeOutcome(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var EvictionCommitteeDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.EvictionCommitteeDocument);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentEvictionCommittee);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == EvictionCommitteeDocument.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocsEvictionCommitteeOutcome(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var EvictionCommitteeDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.EvictionCommitteeDocument);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentEvictionCommittee);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == EvictionCommitteeDocument.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.HumanSettlementApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.HumanSettlementApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentTenantRiskAssessment(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var RiskAssessmentLeaseRenewal = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RiskAssessmentLeaseRenewal);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentTenantRiskAssessment);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RiskAssessmentLeaseRenewal.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentEvictionCapture(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var EvictionOutcomeCapture = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.EvictionOutcomeCapture);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == EvictionOutcomeCapture.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentEvictionOutcomeSupportingDoc(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var EvictionOutcomeSupportingDoc = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.EvictionOutcomeSupportingDoc);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == EvictionOutcomeSupportingDoc.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentUploadFinalLeaseAgreement(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var ApplicationLeaseAgreementEHC = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ApplicationLeaseAgreementEHC);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ApplicationLeaseAgreementEHC.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentUploadTemplates(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var MaintananceJobSheetTemplate = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MaintananceJobSheetTemplate);
            var UnitInspectionTemplate = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.UnitInspectionTemplate);
            var ExitInspectionTemplate = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ExitInspectionTemplate);
            var PropertyEvictionTemplate = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PropertyEvictionTemplate);
            var PostInspectionTemplate = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PostInspectionTemplate);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MaintananceJobSheetTemplate.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == UnitInspectionTemplate.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ExitInspectionTemplate.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PropertyEvictionTemplate.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PostInspectionTemplate.Id && dcl.ReferenceTypeId == referenceType.Id));


            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = "";
            dvm.CustomerId = 1256;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = 1;
            dvm.RcsApplicationId = 1;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)1256;
            dvm.IsUploadView = true;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == 1256 && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == 1 && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, 1256, 1256, applicationId, 1))));
            }
            return dvm;
        }
        public static DocumentsViewModel DocumentGetUploadedTemplates(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var ApplicationLeaseAgreementEHC = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ApplicationLeaseAgreementEHC);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ApplicationLeaseAgreementEHC.Id && dcl.ReferenceTypeId == referenceType.Id));

            var latest_doc = core.Documents.OrderByDescending(o => o.Id).Where(o => o.ReferenceTypeId == referenceTypeId && o.IsActive && !o.IsDeleted && o.PropertyLeaseApplicationId == 100000).FirstOrDefault();
            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentGetConductUnitInspectionTemplate(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var UnitInspectionTemplate = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.UnitInspectionTemplate);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == UnitInspectionTemplate.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = "";
            dvm.CustomerId = 1256;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = 1;
            dvm.RcsApplicationId = 1;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)1256;
            dvm.IsUploadView = false;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == 1256 && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == 1 && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, 1256, 1256, applicationId, 1))));
            }

            return dvm;
        }
        public static DocumentsViewModel DocumentGetConductMaintananceJobSheetTemplate(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var MaintananceJobSheetTemplate = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MaintananceJobSheetTemplate);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MaintananceJobSheetTemplate.Id && dcl.ReferenceTypeId == referenceType.Id));

            var latest_doc = core.Documents.OrderByDescending(o => o.Id).Where(o => o.ReferenceTypeId == referenceTypeId && o.IsActive && !o.IsDeleted).FirstOrDefault();
            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = "";
            dvm.CustomerId = 1256;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = 1;
            dvm.RcsApplicationId = 1;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)1256;
            dvm.IsUploadView = false;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == 1256 && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == 1 && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, 1256, 1256, applicationId, 1))));
            }

            return dvm;
        }
        public static DocumentsViewModel DocumentGetConductExitInspectionTemplate(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var ExitInspectionTemplate = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ExitInspectionTemplate);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ExitInspectionTemplate.Id && dcl.ReferenceTypeId == referenceType.Id));

            var latest_doc = core.Documents.OrderByDescending(o => o.Id).Where(o => o.ReferenceTypeId == referenceTypeId && o.IsActive && !o.IsDeleted).FirstOrDefault();
            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = "";
            dvm.CustomerId = 1256;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = 1;
            dvm.RcsApplicationId = 1;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)1256;
            dvm.IsUploadView = false;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == 1256 && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == 1 && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, 1256, 1256, applicationId, 1))));
            }

            return dvm;
        }
        public static DocumentsViewModel DocumentTerminationValidation(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var TerminationValidationSupportingDoc = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.TerminationValidationSupportingDoc);

            //Add to checklists
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == TerminationValidationSupportingDoc.Id && dcl.ReferenceTypeId == referenceTypeId));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentDebitOrder(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            var addDocumentType = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);


            var DebitOrder = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DebitOrder);

            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DebitOrder.Id && dcl.ReferenceTypeId == referenceTypeId));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            var addDoc = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            //if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            //{
            //    documentCheckLists.Add(addDoc);
            //}

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentAgreementOfLease(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int plmappsId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var HSAgreementOfLease = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.HSAgreementOfLease);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == HSAgreementOfLease.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.HumanSettlementApplicationId = plmappsId;
            dvm.RcsApplicationId = plmappsId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.HumanSettlementApplicationId == plmappsId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceId, customerId, applicationId, plmappsId))));
            }

            return dvm;
        }
        public static DocumentsViewModel HumanRenewalDocuments(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            var addDocumentType = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocuments);

            var PayslipPensionGrant = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PayslipPensionGrant);
            var BankStatement = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankStatement);
            var IdentityDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdentityDocument);
            var ProofOfEmployment = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfEmployment);
            var Affidavit = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.Affidavit);
            var ProofOfAddress = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfAddress);

            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == IdentityDocument.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PayslipPensionGrant.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankStatement.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfEmployment.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == Affidavit.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfAddress.Id && dcl.ReferenceTypeId == referenceTypeId));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.HumanSettlementApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            //dvm.LeaseDetailsId = findItem.Id;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.HumanSettlementApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            var addDoc = core.DocumentCheckLists.Include(o => o.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == addDocumentType.Id && dcl.ReferenceTypeId == referenceType.Id);
            if (documentCheckLists.All(chk => chk.Id != addDoc.Id))
            {
                documentCheckLists.Add(addDoc);
            }

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentAccountValidation(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var FinancialSupportingDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.FinancialSupportingDocument);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentAccountValidation);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == FinancialSupportingDocument.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();


            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.HumanSettlementApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            //dvm.LeaseDetailsId = findItem.Id;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.HumanSettlementApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();


            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, referenceId, customerId, applicationId, rcsappId))));
            }

            return dvm;
        }
        public static DocumentsViewModel DocumentPostInspection(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var ConductExitInspection = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PostInspection);
            var AdditionalDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AdditionalDocumentExitInspection);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ConductExitInspection.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AdditionalDocument.Id && dcl.ReferenceTypeId == referenceType.Id));

            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.HumanSettlementApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentGetPostInspectionTemplate(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var ExitInspectionTemplate = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PostInspectionTemplate);

            //Add to checklist
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ExitInspectionTemplate.Id && dcl.ReferenceTypeId == referenceType.Id));

            var latest_doc = core.Documents.OrderByDescending(o => o.Id).Where(o => o.ReferenceTypeId == referenceTypeId && o.IsActive && !o.IsDeleted).FirstOrDefault();
            var ratesRebateProperty = new RatesRebateProperty();
            var incentivePolicyProperty = new IncentivePolicyProperty();

            dvm.ReturnUrl = "";
            dvm.CustomerId = 1256;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = 1;
            dvm.RcsApplicationId = 1;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)1256;
            dvm.IsUploadView = false;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == 1256 && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == 1 && o.IsActive && !o.IsDeleted).ToList();
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
                         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                    customerDocument.DocumentName);
                customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
            }

            foreach (var documentCheckList in dvm.DocumentCheckLists.Where(dcl => dcl != null))
            {

                documentCheckList.DataList = new List<string>();
                documentCheckList.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentCheckListId={0}??referenceTypeId={1}??referenceId={2}??customerId={3}??applicationId={4}??rcsappId={5}", documentCheckList.Id, documentCheckList.ReferenceTypeId, 1256, 1256, applicationId, 1))));
            }

            return dvm;
        }
        public static void ChangeLeaseToPropertyRef(eServicesDbContext core)
        {
            var leaselist = core.LeaseDetails.ToList();
            foreach (var item in leaselist)
            {
                var pp = core.PropertyLeaseApplications.FirstOrDefault(x => x.Id == item.PropertyLeaseApplicationId);
                item.LeaseReferenceNo = pp.ApplicationReferenceNumber;
                core.SaveChanges();
            }
        }
        public static bool IsWaitingListSorting(eServicesDbContext core)
        {
            bool result = Convert.ToBoolean(Convert.ToInt16(core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting).Value));
            return result;
        }
        public static void DoWork(eServicesDbContext core, WaitingListQue queueItem)
        {
            var findPropertyLease = core.PropertyLeaseApplications.FirstOrDefault(x => x.Id == queueItem.PropertyLeaseApplicationId) ?? null;
            var matched = core.MatchedUnits.Where(x => x.PropertyLeaseApplicationId == findPropertyLease.Id && x.RejectedProperty).ToList() ?? null;
            var rrq = matched.Select(x => x.UnitsId).ToList();
            var findUnits = core.Units.Where(x => !x.IsTaken && !rrq.Contains(x.Id) && (x.PreferredComplexAreaId == findPropertyLease.PreferredComplexAreaId || x.PreferredComplexAreaId == findPropertyLease.PreferredComplexArea2Id)
            && x.OccupationTypeId == findPropertyLease.HumanEHCOptionsId).FirstOrDefault() ?? null;
            var findItem = core.UnitsEkurhuleniHousingCompany.Where(x => !x.IsTaken && !rrq.Contains(x.Id) && (x.PreferredComplexAreaId == findPropertyLease.PreferredComplexAreaId || x.PreferredComplexAreaId == findPropertyLease.PreferredComplexArea2Id)
            && x.HumanEHCOptionId == findPropertyLease.HumanEHCOptionsId).FirstOrDefault() ?? null;
            var CurrentPLMAppMatched = false;
            if (findItem != null)
            {
                findItem.IsTaken = true;
                core.SaveChanges();

                // Construct the match unit
                MatchedUnits match = new MatchedUnits
                {
                    PropertyLeaseApplicationId = (int)findPropertyLease?.Id,
                    LeaseReferenceNo = findPropertyLease?.ApplicationReferenceNumber,
                    UnitsEkurhuleniHousingCompanyId = findItem.Id,
                    IsAccepted = false,
                };

                SaveMatchedUnit(core, match);
                MarkQueueAsMatched(core, (int)findPropertyLease?.Id);
                MarkApplicationAsMatched(core, findPropertyLease);
                CurrentPLMAppMatched = true;
            }
        }
        public static LeaseDetails SaveLeaseDetaisInfo(eServicesDbContext core, LeaseDetails lease)
        {
            try
            {
                //if (lease.Id != 0)
                //{
                //    var findItem = core.LeaseDetails.FirstOrDefault(x => x.Id == lease.Id);
                //    if (findItem != null)
                //    {
                //        foreach (var prop in typeof(LeaseDetails).GetProperties())
                //        {
                //            if (prop.CanWrite)
                //            {
                //                prop.SetValue(findItem, prop.GetValue(lease));
                //            }
                //        }
                //        findItem.TerminationReminder = Convert.ToDateTime("1900/01/01 00:00:00");
                //        core.Entry(findItem).State = EntityState.Modified;
                //        core.SaveChanges();
                //    }
                //}
                if (lease.Id != 0)
                {
                    //var findItem = core.LeaseDetails.FirstOrDefault(x => x.Id == lease.Id);

                    var findItem = core.LeaseDetails
    .Include(x => x.PropertyLeaseApplication)
    .Include(x => x.Status)
    .Include(x => x.PurchaserType)
    .Include(x => x.SystemUser)
    .OrderByDescending(x => x.Id)
    .FirstOrDefault(x => x.Id == lease.Id);

                    if (findItem != null)
                    {

                        if (lease.EndDate != null)
                        {
                            findItem.EndDate = lease.EndDate;
                        }

                        if (lease.TerminationNotice != null)
                        {
                            findItem.TerminationNotice = lease.TerminationNotice;
                        }
                        if (lease.RenewalNotice != null)
                        {
                            findItem.RenewalNotice = lease.RenewalNotice;
                        }
                        if (lease.PeriodInMonths != null)
                        {
                            findItem.PeriodInMonths = lease.PeriodInMonths;
                        }


                        // New fields set 3
                        findItem.DepositeAmount = lease.DepositeAmount != null ? lease.DepositeAmount : findItem.DepositeAmount;
                        findItem.RentalAmount = lease.RentalAmount != null ? lease.RentalAmount : findItem.RentalAmount;
                        findItem.VATAmount = lease.VATAmount != null ? lease.VATAmount : findItem.VATAmount;
                        findItem.TotalIncludingVAT = lease.TotalIncludingVAT != null ? lease.TotalIncludingVAT : findItem.TotalIncludingVAT;
                        //findItem.StatementDate = lease.StatementDate != null ? lease.StatementDate : findItem.StatementDate;
                        //findItem.EscalationDate = lease.EscalationDate != null ? lease.EscalationDate : findItem.EscalationDate;
                        findItem.Email = lease.Email != null ? lease.Email : findItem.Email;
                        findItem.SMS = lease.SMS != null ? lease.SMS : findItem.SMS;
                        findItem.Postal = lease.Postal != null ? lease.Postal : findItem.Postal;

                        findItem.FirstNames = lease.FirstNames != null ? lease.FirstNames : findItem.FirstNames;
                        //findItem.SupportingDoccuments = lease.SupportingDoccuments != null ? lease.SupportingDoccuments : findItem.SupportingDoccuments;

                        //findItem.TypeOfActivities = lease.TypeOfActivities != null ? lease.TypeOfActivities : findItem.TypeOfActivities;
                        //findItem.ComplianceDetails = lease.ComplianceDetails != null ? lease.ComplianceDetails : findItem.ComplianceDetails;

                        findItem.LeaAddress = lease.LeaAddress != null ? lease.LeaAddress : findItem.LeaAddress;
                        findItem.LeaPostal = lease.LeaPostal != null ? lease.LeaPostal : findItem.LeaPostal;
                        findItem.LeaSuburb = lease.LeaSuburb != null ? lease.LeaSuburb : findItem.LeaSuburb;

                        //findItem.buildingName = lease.buildingName != null ? lease.buildingName : findItem.buildingName;
                        //findItem.PropertyId = lease.PropertyId != null ? lease.PropertyId : findItem.PropertyId;
                        findItem.SpaceUnitNo = lease.SpaceUnitNo != null ? lease.SpaceUnitNo : findItem.SpaceUnitNo;

                        //findItem.OfficeParkName = lease.OfficeParkName != null ? lease.OfficeParkName : findItem.OfficeParkName;



                        findItem.PreparationFee = lease.PreparationFee != null ? lease.PreparationFee : findItem.PreparationFee;
                        findItem.CreditCheckFee = lease.CreditCheckFee != null ? lease.CreditCheckFee : findItem.CreditCheckFee;

                        findItem.CalculatedAsFolllows = lease.CalculatedAsFolllows != null ? lease.CalculatedAsFolllows : findItem.CalculatedAsFolllows;
                        findItem.InitialDepositPremises = lease.InitialDepositPremises != null ? lease.InitialDepositPremises : findItem.InitialDepositPremises;
                        findItem.DepositTenantContribution = lease.DepositTenantContribution != null ? lease.DepositTenantContribution : findItem.DepositTenantContribution;
                        findItem.ShadePortParking = lease.ShadePortParking != null ? lease.ShadePortParking : findItem.ShadePortParking;
                        findItem.SPP = lease.SPP != null ? lease.SPP : findItem.SPP;
                        findItem.OpenParking = lease.OpenParking != null ? lease.OpenParking : findItem.OpenParking;
                        findItem.OPP = lease.OPP != null ? lease.OPP : findItem.OPP;
                        findItem.StoreRooms = lease.StoreRooms != null ? lease.StoreRooms : findItem.StoreRooms;
                        findItem.STR = lease.STR != null ? lease.STR : findItem.STR;
                        findItem.Electricity = lease.Electricity != null ? lease.Electricity : findItem.Electricity;
                        findItem.ELEC = lease.ELEC != null ? lease.ELEC : findItem.ELEC;
                        findItem.Refuse = lease.Refuse != null ? lease.Refuse : findItem.Refuse;
                        findItem.SecurityFee = lease.SecurityFee != null ? lease.SecurityFee : findItem.SecurityFee;
                        findItem.SEC = lease.SEC != null ? lease.SEC : findItem.SEC;
                        findItem.Sewerage = lease.Sewerage != null ? lease.Sewerage : findItem.Sewerage;
                        findItem.Water = lease.Water != null ? lease.Water : findItem.Water;
                        findItem.WTR = lease.WTR != null ? lease.WTR : findItem.WTR;
                        findItem.TerminationDate = lease.TerminationDate != null ? lease.TerminationDate : findItem.TerminationDate;
                        findItem.CarportParkingBayNumber = lease.CarportParkingBayNumber != null ? lease.CarportParkingBayNumber : findItem.CarportParkingBayNumber;
                        findItem.OPenParkingBayNumber = lease.OPenParkingBayNumber != null ? lease.OPenParkingBayNumber : findItem.OPenParkingBayNumber;
                        findItem.ShadePortBayNumber = lease.ShadePortBayNumber != null ? lease.ShadePortBayNumber : findItem.ShadePortBayNumber;
                        findItem.CarportParkingBay = lease.CarportParkingBay != null ? lease.CarportParkingBay : findItem.CarportParkingBay;
                        findItem.LeaseAdministrationFee = lease.LeaseAdministrationFee != null ? lease.LeaseAdministrationFee : findItem.LeaseAdministrationFee;
                        findItem.FloorNumber = lease.FloorNumber != null ? lease.FloorNumber : findItem.FloorNumber;
                        findItem.TotalMonthlyCharges = lease.TotalMonthlyCharges != null ? lease.TotalMonthlyCharges : findItem.TotalMonthlyCharges;

                        //findItem.MonthsOffered = lease.MonthsOffered != null ? lease.MonthsOffered : findItem.MonthsOffered;
                        findItem.Completed = lease.Completed != null ? lease.Completed : findItem.Completed;
                        findItem.DepositHeld = lease.DepositHeld != null ? lease.DepositHeld : findItem.DepositHeld;

                        findItem.TerminationReminder = Convert.ToDateTime("1900/01/01 00:00:00");
                        core.Entry(findItem).State = EntityState.Modified;
                        core.SaveChanges();
                    }
                }

                else
                {
                    lease.TerminationReminder = Convert.ToDateTime("1900/01/01 00:00:00");
                    core.LeaseDetails.Add(lease);
                    core.SaveChanges();
                    var list = core.LeaseDetails.Where(x => x.PropertyLeaseApplicationId == lease.PropertyLeaseApplicationId && x.Id != lease.Id).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            item.IsNew = false;
                            core.SaveChanges();
                        }
                    }
                }
                return lease;
            }
            catch (Exception Io)
            {
                return null;
            }

        }
        public static PropertyLeaseApplication ModifyPropertyLeaseApplication(eServicesDbContext core, PropertyLeaseApplication propertyLeaseApplication)
        {
            core.Entry(propertyLeaseApplication).State = EntityState.Modified;
            core.SaveChanges();
            return propertyLeaseApplication;
        }
        public static void DeleteSchedule(eServicesDbContext core, int ScheduleId)
        {
            var schedule = core.InspectionSchedules.FirstOrDefault(x => x.Id == ScheduleId);
            schedule.IsDeleted = true;
            core.SaveChanges();
        }
        public static void RemoveUnselectedSchedules(eServicesDbContext core, int ScheduleId)
        {
            var schedule = core.InspectionSchedules.FirstOrDefault(x => x.Id == ScheduleId);
            schedule.IsDeleted = true;
            core.SaveChanges();

            var findItem = core.InspectionSchedules.Where(x => x.PropertyLeaseApplicationId == schedule.PropertyLeaseApplicationId).ToList();
            foreach (var Item in findItem)
            {
                Item.IsDeleted = true;
                core.SaveChanges();
            }
        }
        public static void ScheduleInspectionUnit(eServicesDbContext core, int ScheduleId)
        {
            var schedule = core.InspectionSchedules.FirstOrDefault(x => x.Id == ScheduleId);
            schedule.IsApproved = true;
            core.SaveChanges();

            ChangeApplicationStatus(core, (int)core.Status.FirstOrDefault(x => x.Key == StatusKeys.RatesRebateAdditionalPropertyOwnersPending)?.Id, (int)schedule.PropertyLeaseApplicationId);
            PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
            //                                                           1      2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17      18   19
            cc.EHCRoundRobin((int)schedule.PropertyLeaseApplicationId, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

            int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.UnitInspectionScheduleByApplicant).Id;
            EmailHelper.CustomerEmailNotification(core, (int)schedule.PropertyLeaseApplicationId, emailboodyId);
        }
        public static bool ValidateSelectedWithApproved(eServicesDbContext core, int ScheduleId, int User)
        {
            var schedule = core.InspectionSchedules.Include(r => r.DateToSchedule).FirstOrDefault(x => x.Id == ScheduleId);
            var findItem = core.ScheduledInspections.FirstOrDefault(x => x.DateToSchedule.ShecduleDate == schedule.DateToSchedule.ShecduleDate && x.TimeSlotId == schedule.TimeSlotId && x.HousingSupervisorId == User);
            if (findItem != null)
            {
                DeleteSchedule(core, ScheduleId);
                return true;
            }
            else
            {
                var scheduledInspection = new ScheduledInspection
                {
                    PropertyLeaseApplicationId = schedule.PropertyLeaseApplicationId,
                    TimeSlotId = schedule.TimeSlotId,
                    HousingSupervisorId = User,
                    DateToScheduleId = schedule.DateToScheduleId
                };
                core.ScheduledInspections.Add(scheduledInspection);
                core.SaveChanges();
                return false;
            }
        }
        public static void HumanScheduleInspectionUnit(eServicesDbContext core, int ScheduleId)
        {
            var schedule = core.InspectionSchedules.FirstOrDefault(x => x.Id == ScheduleId);
            schedule.IsApproved = true;
            core.SaveChanges();

            ChangeHumanStatus(core, (int)core.Status.FirstOrDefault(x => x.Key == StatusKeys.RatesRebateAdditionalPropertyOwnersPending)?.Id, (int)schedule.HumanSettlementApplicationId);
            PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();


            //                                                           1      2       3     4     5       6      7      8      9      10     11     12     13     14     15   16  17      18   19
            //cc.EHCRoundRobin((int)schedule.PropertyLeaseApplicationId, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);

            int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.UnitInspectionScheduleByApplicant).Id;
            EmailHelper.CustomerEmailOrSMSNotification(core, (int)schedule.HumanSettlementApplicationId, emailboodyId);
        }
        public static bool HumanValidateSelectedWithApproved(eServicesDbContext core, int ScheduleId, int User)
        {
            var schedule = core.InspectionSchedules.Include(r => r.DateToSchedule).FirstOrDefault(x => x.Id == ScheduleId);
            var findItem = core.ScheduledInspections.FirstOrDefault(x => x.DateToSchedule.ShecduleDate == schedule.DateToSchedule.ShecduleDate && x.TimeSlotId == schedule.TimeSlotId && x.HousingSupervisorId == User);
            if (findItem != null)
            {
                DeleteSchedule(core, ScheduleId);
                return true;
            }
            else
            {
                var scheduledInspection = new ScheduledInspection
                {
                    //PropertyLeaseApplicationId = schedule.PropertyLeaseApplicationId,

                    HumanSettlementApplicationId = schedule.HumanSettlementApplicationId,
                    TimeSlotId = schedule.TimeSlotId,
                    HousingSupervisorId = User,
                    DateToScheduleId = schedule.DateToScheduleId
                };
                core.ScheduledInspections.Add(scheduledInspection);
                core.SaveChanges();
                return false;
            }

        }
        public static void SaveMatchedUnit(eServicesDbContext core, MatchedUnits unit)
        {
            core.MatchedUnits.Add(unit);
            core.SaveChanges();
        }
        public static void MarkQueueAsMatched(eServicesDbContext core, int propertyApplicationId)
        {
            var findQueueItem = core.waitingListQues.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyApplicationId) ?? null;
            findQueueItem.IsMatched = true;
            core.SaveChanges();
        }
        public static void MarkQueueAsDeleted(eServicesDbContext core, int propertyApplicationId)
        {
            var findQueueItem = core.waitingListQues.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyApplicationId) ?? null;
            findQueueItem.IsMatched = false;
            findQueueItem.IsDeleted = false;
            core.SaveChanges();
        }
        public static void MarkQueueAsREListed(eServicesDbContext core, int propertyApplicationId)
        {
            var findQueueItem = core.waitingListQues.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyApplicationId) ?? null;
            findQueueItem.IsMatched = false;
            findQueueItem.QueueDate = DateTime.Now;
            findQueueItem.IsReListed = false;
            core.SaveChanges();
        }
        public static void MarkQueueAsRelisted2(eServicesDbContext core, int ApplicationId)
        {
            var findQueueItem = core.WaitingListQueueHumans.FirstOrDefault(x => x.HumanSettlementApplicationId == ApplicationId) ?? null;
            findQueueItem.QueueDate = DateTime.Now;
            findQueueItem.IsReListed = false;
            core.SaveChanges();
        }
        public static void ReListWaittingQueue(eServicesDbContext core, int propertyApplicationId)
        {
            var findQueueItem = core.waitingListQues.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyApplicationId && x.IsReListed) ?? null;
            findQueueItem.IsReListed = false;
            findQueueItem.QueueDate = DateTime.Now;
            core.SaveChanges();
        }
        public static void RemoveWaittingQueueItem(eServicesDbContext core, int propertyApplicationId)
        {
            var findQueueItem = core.waitingListQues.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyApplicationId) ?? null;
            findQueueItem.IsReListed = false;
            findQueueItem.IsDeleted = true;
            findQueueItem.IsActive = false;
            core.SaveChanges();
        }
        public static void RemoveWaittingQueueItem2(eServicesDbContext core, int HumanSettlementApplicationId)
        {
            var findQueueItem = core.waitingListQues.FirstOrDefault(x => x.HumanSettlementApplicationId == HumanSettlementApplicationId) ?? null;
            findQueueItem.IsReListed = false;
            findQueueItem.IsDeleted = true;
            findQueueItem.IsActive = false;
            core.SaveChanges();
        }
        public static void RenewalNotificationAtEndOfTime(eServicesDbContext core)
        {
            try
            {
                var ApplicatioUnit = core.ApplicantUnits.Where(x => x.IsActive && !x.IsDeleted).ToList();
                var list = ApplicatioUnit.OrderBy(x => x.PropertyLeaseApplicationId).Select(x => x.PropertyLeaseApplicationId);
                var Details = core.LeaseDetails.Include(r => r.PropertyLeaseApplication).OrderByDescending(f => f.Id).ToList();
                var minutes = Convert.ToInt32(core.AppSettings.FirstOrDefault(r => r.Key == AppSettingKeys.renewal_notification_in_minutes).Value);
                DateTime notification = DateTime.Now;
                foreach (var Item in Details)
                {
                    Item.RenewalNotice = Convert.ToDateTime(Item.RenewalNotice);
                }
                Details = Details.Where(x => x.IsActive && list.Contains(x.PropertyLeaseApplicationId) && x.PeriodInMonths >= 12 && x.Completed && x.RenewalNotice <= notification && x.IsNew && !x.IsRenewed).ToList();
                foreach (var Item in Details)
                {
                    PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
              
                    ChangeLeaseStatusII(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicationUpForRenewalAtThreeMonths).Id, (int)Item.Id);
                    int result = cc.EHCRoundRobin(Item.PropertyLeaseApplicationId, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 1, false, false, 1);
                    int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationUpForRenewal).Id;
                    //CustomerEmailNotification(core, Item.PropertyLeaseApplicationId, emailboodyId);
                    Item.IsRenewed = true;
                    core.SaveChanges();
                }
            }
            catch (Exception IO)
            {
                //LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }

            //try
            //{
            //    var ApplicatioUnit = core.ApplicantUnits.Where(x => x.IsActive && !x.IsDeleted).ToList();
            //    var list = ApplicatioUnit.Select(x => x.PropertyLeaseApplicationId);
            //    var minutes = Convert.ToInt32(core.AppSettings.FirstOrDefault(r => r.Key == AppSettingKeys.renewal_notification_in_minutes).Value);
            //    DateTime notification = DateTime.Now.AddMinutes(-minutes);
            //    var findItem = core.LeaseDetails.Include(r => r.PropertyLeaseApplication).OrderByDescending(f => f.Id).Where(x => x.IsActive && list.Contains(x.PropertyLeaseApplicationId) && x.PeriodInMonths >= 12 && x.Completed && /*x.RenewalNotice temporary for UAT*/ x.CreatedDateTime <= notification && x.IsNew && !x.IsRenewed).ToList();
            //    //foreach (var Item in findItem)
            //    //{
            //    //    ChangeLeaseStatusII(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicationUpForRenewalAtThreeMonths).Id, (int)Item.Id);
            //    //    PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
            //    //    cc.EHCRoundRobin(Item.PropertyLeaseApplicationId, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, 1, false, false, 1);
            //    //    int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationUpForRenewal).Id;
            //    //    EmailHelper.CustomerEmailNotification(core, Item.PropertyLeaseApplicationId, emailboodyId);
            //    //    Item.IsRenewed = true;
            //    //    core.SaveChanges();
            //    //}
            //}
            //catch (Exception)
            //{

            //}
        }

        public static void HumanRenewalNotification(eServicesDbContext core)
        {
            //Only Family units, retirement flats and hostels leases can be renewed. 

            var list = core.HumanSettlementLeaseMasters.Include(r => r.HumanSettlementApplication).ToList();
            var Details = core.HumanSettlementLeaseDetails.Include(r => r.HumanSettlementApplication).ToList();
            DateTime notification = DateTime.Now;
            foreach (var Item in Details)
            {
                Item.RenewalNotice = Convert.ToDateTime(Item.RenewalNotice);
                Item.EndDate = Convert.ToDateTime(Item.EndDate);
            }
            foreach (var Item in list)
            {
                if (Item.HumanSettlementApplicationId == 139)
                {

                }
                Item.RenewalNotice = Convert.ToDateTime(Item.RenewalNotice);
                Item.EndDate = Convert.ToDateTime(Item.EndDate);
            }
            Details = Details.OrderByDescending(r => r.Id).Where(x => notification >= x.RenewalNotice &&  !x.IsRenewed).ToList();
            list = list.OrderByDescending(r => r.Id).Where(x => notification >= x.RenewalNotice  && !x.IsRenewed).ToList();

            var StatusId = core.Status.FirstOrDefault(r => r.Key == StatusKeys.AgreementOfLeaseReview).Id;
            int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationUpForRenewal).Id;
            foreach (var Item in list)
            {
                Item.IsRenewed = true;
                core.SaveChanges();
                ChangeMasterStatus(core, StatusId, Item.Id);

                WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)Item.HumanSettlementApplicationId, true, false, false, false, false);
                EmailHelper.CustomerEmailOrSMSNotification(core, (int)Item.HumanSettlementApplicationId, emailboodyId);

            }
            foreach (var Item in Details)
            {
                Item.IsRenewed = true;
                core.SaveChanges();
                ChangeLeaseDetailsStatus(core, StatusId, Item.Id);
                WorkAllocationHumanHelper.AgreementOfLeaseRenewal((int)Item.HumanSettlementApplicationId, true, false, false, false, false);
                EmailHelper.CustomerEmailOrSMSNotification(core, (int)Item.HumanSettlementApplicationId, emailboodyId);
            }
        }
        public static void WaitingListNotificationAtOneYear(eServicesDbContext core)
        {
            var minutes = Convert.ToInt32(core.AppSettings.FirstOrDefault(r => r.Key == AppSettingKeys.waiting_list_notification_in_munites).Value);
            DateTime notification = DateTime.Now.AddMinutes(-minutes);
            var findQueueItem = core.waitingListQues.OrderBy(x => x.Position).Include(r => r.PropertyLeaseApplication).Where(x => x.IsDeleted != true && x.IsActive && !x.IsMatched && notification >= x.QueueDate && x.IsReListed == false && x.Position != null).ToList();
            //foreach (var item in findQueueItem)
            //{
            //    ChangeApplicationStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.InAwaitingWaitingListReEntry).Id, (int)item.PropertyLeaseApplicationId);
            //    EmailHelper.CustomerEmailNotification(core, (int)item.PropertyLeaseApplicationId, core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.WaitingListReEntery).Id);
            //    item.IsReListed = true;
            //    core.Entry(item).State = EntityState.Modified;
            //}
            //core.SaveChanges();
        }
        public static void WaitingListNotificationAtOneYear2(eServicesDbContext core)
        {
            var minutes = Convert.ToInt32(core.AppSettings.FirstOrDefault(r => r.Key == AppSettingKeys.waiting_list_notification_in_munites).Value);
            DateTime notification = DateTime.Now;
            var findQueueItem = core.WaitingListQueueHumans.OrderBy(x => x.Position).Include(r => r.HumanSettlementApplication).Where(x => !x.IsMatched && x.Position != null).ToList();
            foreach (var item in findQueueItem)
            {
                var days = (DateTime.Now - Convert.ToDateTime(item.QueueDate)).Days;
                if (days >= 334 && !item.IsReListed)
                {
                    var StatusKey = core.Status.FirstOrDefault(x => x.Key == StatusKeys.InAwaitingWaitingListReEntry);
                    var ApplicationId = (int)item.HumanSettlementApplicationId;
                    var EmailContent = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.WaitingListReEntery);
                    ChangeHumanStatus(core, StatusKey.Id, ApplicationId);
                    EmailHelper.CustomerEmailOrSMSNotification(core, ApplicationId, EmailContent.Id);
                    item.IsReListed = true;
                    item.ReListDate = DateTime.Now;
                    core.SaveChanges();
                }
            }
            core.SaveChanges();
        }
        public static void VerifrySecondPropertyReject(eServicesDbContext core, int propertyApplicationId)
        {
            try
            {
                var findQueueItem = core.MatchedUnits.Where(x => x.PropertyLeaseApplicationId == propertyApplicationId && x.RejectedProperty).ToList();
                if (findQueueItem.Count > 1)
                {
                    foreach (var item in findQueueItem)
                        item.RejectedProperty = true;
                    core.SaveChanges();

                    ChangeApplicationStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicationDiscardedNoUnitAvailable).Id, (int)propertyApplicationId);
                    int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RemoveWaitingListOnSecondReject).Id;
                    EmailHelper.CustomerEmailNotification(core, propertyApplicationId, emailboodyId);
                }
                else
                {
                    PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
                    cc.EHCRoundRobin(propertyApplicationId, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, 1, false, false, 1);
                }
            }
            catch
            {

            }
        }
        public static void SavePropertyManagersResponse(eServicesDbContext core, int PropertyId, string Comment, bool t)
        {
            PropertyManager property = new PropertyManager
            {
                PropertyLeaseApplicationId = PropertyId,
                PropertyComments = Comment,
                Approved = t
            };
            core.PropertyManagers.Add(property);
            core.SaveChanges();
        }
        public static void MarkUnitAsTaken(eServicesDbContext core, int unitId)
        {
            var findUnitItem = core.Units.FirstOrDefault(x => x.Id == unitId) ?? null;
            findUnitItem.IsTaken = true;
            core.SaveChanges();
        }
        public static void MarkUnitAsAvailable(eServicesDbContext core, int unitId)
        {
            var findUnitItem = core.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == unitId) ?? null;
            findUnitItem.IsTaken = false;
            core.SaveChanges();

            MatchedUnits _match = core.MatchedUnits.OrderByDescending(o => o.Id).FirstOrDefault(a => a.ApplicationAllocatedPropertyId == findUnitItem.Id);
            BaseHelper baseHelper = new BaseHelper();
            baseHelper.Initialise(core);
            SaveUnitHistory(core, (Int32)_match.PropertyLeaseApplicationId, "Allocated Unit Rejected By Customer - Unit Available for next match", findUnitItem.Id, baseHelper.SystemUser.Id);
        }
        public static void MarkUnitAsAvailableHuman(eServicesDbContext core, int unitId)
        {
            var findUnitItem = core.UnitsHumanSettlement01s.FirstOrDefault(x => x.Id == unitId) ?? null;
            findUnitItem.IsTaken = false;
            core.SaveChanges();
        }
        public static void MarkApplicationAsMatched(eServicesDbContext core, PropertyLeaseApplication property)
        {
            property.StatusId = (int)core.Status.FirstOrDefault(x => x.Key == StatusKeys.awaited)?.Id;
            core.SaveChanges();

            //Send e-mail and SMS notification
            int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.AtUnitMatchApplication).Id;
            EmailHelper.CustomerEmailNotification(core, property.Id, emailboodyId);
        }
        public static void ChangeApplicationStatus(eServicesDbContext core, int statusId, int PropertyLeaseId)
        {

            var findApplication = core.PropertyLeaseApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == PropertyLeaseId) ?? null;
            if (!(String.IsNullOrEmpty(findApplication.Status.Name)))
            {
                var Key = core.Status.FirstOrDefault(x => x.Id == statusId);
                var ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.at_change_application_status).Description.ToString();
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", findApplication.Status.Name);
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", Key.Name);
                var Result = ActivityTrackerAudit(core, PropertyLeaseId, ActivityTrackerMessage, findApplication.CustomerId);
            }

            findApplication.StatusId = statusId;
            core.SaveChanges();
        }
        public static void ChangeHumanStatus(eServicesDbContext core, int statusId, int ApplicationId)
        {

            var findApplication = core.HumanSettlementApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == ApplicationId) ?? null;
            if (!(String.IsNullOrEmpty(findApplication.Status.Name)))
            {
                var Key = core.Status.FirstOrDefault(x => x.Id == statusId);
                var ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.at_change_application_status).Description.ToString();
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", findApplication.Status.Name);
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", Key.Name);
                var Result = ActivityTrackerHuman(core, ApplicationId, ActivityTrackerMessage, findApplication.CustomerId);
            }

            findApplication.StatusId = statusId;
            core.SaveChanges();
        }
        public static void AddCommentOnRejectAgreement(eServicesDbContext core, string Comment, int PropertyLeaseId)
        {
            var findItem = core.LeaseDetails.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == PropertyLeaseId) ?? null;
            findItem.ApplicantComment = Comment;
            core.SaveChanges();
        }
        public static void UpdateDebitOrder(eServicesDbContext core, int debitOrderId, DebitOrderRegistration debit)
        {
            try
            {
                var findApplication = core.DebitOrderRegistrations.FirstOrDefault(x => x.Id == debitOrderId) ?? null;
                findApplication = debit;
                core.SaveChanges();
            }
            catch (Exception IO)
            {
                throw;
            }

        }
        public static void ChangeLeaseStatus(eServicesDbContext core, int statusId, int PropertyLeaseId)
        {
            var findApplication = core.LeaseDetails.Include(r => r.Status).Include(r => r.PropertyLeaseApplication).FirstOrDefault(x => x.PropertyLeaseApplicationId == PropertyLeaseId && x.IsNew) ?? null;
            if (!(String.IsNullOrEmpty(findApplication.Status.Name)))
            {
                var Key = core.Status.FirstOrDefault(x => x.Id == statusId);
                var ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.at_change_lease_application_status).Description.ToString();
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", findApplication.Status.Name);
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", Key.Name);
                var Result = ActivityTrackerAudit(core, findApplication.PropertyLeaseApplication.Id, ActivityTrackerMessage, findApplication.PropertyLeaseApplication.CustomerId);
            }
            findApplication.StatusId = statusId;
            core.SaveChanges();
        }
        public static void ChangeMasterStatus(eServicesDbContext core, int statusId, int AgreementId)
        {
            var findApplication = core.HumanSettlementLeaseMasters.Include(r => r.Status).Include(r => r.HumanSettlementApplication).FirstOrDefault(x => x.Id == AgreementId) ?? null;
            if (!(String.IsNullOrEmpty(findApplication.Status.Name)))
            {
                var Key = core.Status.FirstOrDefault(x => x.Id == statusId);
                var ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.at_change_lease_application_status).Description;
                ActivityTrackerMessage = string.Format(ActivityTrackerMessage, findApplication.Status.Name, Key.Name);
                //ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", findApplication.Status.Name);
                //ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", Key.Name);
                var Result = ActivityTrackerHuman(core, findApplication.HumanSettlementApplication.Id, ActivityTrackerMessage, findApplication.HumanSettlementApplication.CustomerId);
            }
            findApplication.StatusId = statusId;
            core.SaveChanges();
        }
        public static void ChangeLeaseDetailsStatus(eServicesDbContext core, int statusId, int AgreementId)
        {
            var findApplication = core.HumanSettlementLeaseDetails.Include(r => r.Status).Include(r => r.HumanSettlementApplication).FirstOrDefault(x => x.Id == AgreementId) ?? null;
            if (!(String.IsNullOrEmpty(findApplication.Status.Name)))
            {
                var Key = core.Status.FirstOrDefault(x => x.Id == statusId);
                var ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.at_change_lease_application_status).Description.ToString();
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", findApplication.Status.Name);
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", Key.Name);
                ActivityTrackerMessage = string.Format(ActivityTrackerMessage, findApplication.Status.Name, Key.Name);
                var Result = ActivityTrackerHuman(core, findApplication.HumanSettlementApplication.Id, ActivityTrackerMessage, findApplication.HumanSettlementApplication.CustomerId);
            }
            findApplication.StatusId = statusId;
            core.SaveChanges();
        }
        public static void ChangeLeaseTerminationStatus(eServicesDbContext core, int statusId, int LeaseId)
        {
            var terminate = core.LeaseTerminations.FirstOrDefault(x => x.LeaseDetailsId == LeaseId);

            terminate.StatusId = statusId;
            core.SaveChanges();
        }
        public static void ChangeLeaseStatusII(eServicesDbContext core, int statusId, int LeaseId)
        {
            var findApplication = core.LeaseDetails.Include(r => r.Status).Include(r => r.PropertyLeaseApplication).Where(x => x.Id == LeaseId).FirstOrDefault();
            if (!(String.IsNullOrEmpty(findApplication.Status.Name)))
            {
                var Key = core.Status.FirstOrDefault(x => x.Id == statusId);
                var ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.at_change_lease_application_status).Description.ToString();
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{0}", findApplication.Status.Name);
                ActivityTrackerMessage = ActivityTrackerMessage.Replace("{1}", Key.Name);
                var Result = ActivityTrackerAudit(core, findApplication.PropertyLeaseApplication.Id, ActivityTrackerMessage, findApplication.PropertyLeaseApplication.CustomerId);
            }
            findApplication.StatusId = statusId;
            core.SaveChanges();
        }
        public static void MarkInspectionDatesAsInspected(eServicesDbContext core, int Id)
        {
            var findApplication = core.InspectionSchedules.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == Id && !x.IsInspected);
            findApplication.IsInspected = true;
            core.SaveChanges();

            var findItem = core.ScheduledInspections.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == Id && !x.IsInspected);
            findItem.IsInspected = true;
            core.SaveChanges();

        }
        public static void MarkHumanInspectionDatesAsInspected(eServicesDbContext core, int Id)
        {
            var findApplication = core.InspectionSchedules.OrderByDescending(x => x.Id).FirstOrDefault(x => x.HumanSettlementApplicationId == Id && !x.IsInspected);

            if (findApplication != null)
            {
                findApplication.IsInspected = true;
                core.SaveChanges();

                var findItem = core.ScheduledInspections.OrderByDescending(x => x.Id).FirstOrDefault(x => x.HumanSettlementApplicationId == Id && !x.IsInspected);
                findItem.IsInspected = true;
                core.SaveChanges();

            }

        }
        public static void ChangeLeaseEndDate(eServicesDbContext core, int Months, int LeaseId)
        {
            var findApplication = core.LeaseDetails.Where(x => x.Id == LeaseId).FirstOrDefault();
            findApplication.MonthsOffered = Months;
            core.SaveChanges();
        }
        public static void ReferenceOldOccupants(eServicesDbContext core, int StatusId, int OldLeaseId, int NewLeaseId)
        {
            var resident = core.PropertyResidents.Where(x => x.LeaseDetailsId == OldLeaseId).ToList();
            if (resident.Count > 0)
                foreach (var Item in resident)
                {
                    Item.RenewalLeaseId = NewLeaseId;
                }
            core.SaveChanges();
            ChangeLeaseStatusII(core, StatusId, OldLeaseId);
        }
        public static void ChangeLeaseStatusNewLeaseCapture(eServicesDbContext core, int statusId, int LeaseId, int PropertyId)
        {
            var findApplication = core.LeaseDetails.FirstOrDefault(x => x.Id == LeaseId && x.PropertyLeaseApplicationId == PropertyId);
            findApplication.StatusId = statusId;
            core.SaveChanges();
        }
        public static void ChangeApplicationStatusAudi(eServicesDbContext core, int statusId, int PropertyLeaseId, string ActivityTrackerMessage, int custid)
        {
            var findApplication = core.PropertyLeaseApplications.FirstOrDefault(x => x.Id == PropertyLeaseId) ?? null;
            findApplication.StatusId = statusId;
            core.SaveChanges();

            var Result = ActivityTrackerAudit(core, PropertyLeaseId, ActivityTrackerMessage, custid);
        }
        public static bool ActivityTrackerAudit(eServicesDbContext core, int? PropertyId, string ActivityTrackerMessage, int CustomerID)
        {
            try
            {
                PLMApplicationHistortyLog newlog = new PLMApplicationHistortyLog();

                newlog.PropertyLeaseApplicationId = PropertyId;
                newlog.AuditAction = ActivityTrackerMessage;
                newlog.UserId = CustomerID;
                newlog.CreatedDateTime = DateTime.Now;
                newlog.IsActive = true;
                newlog.IsDeleted = false;
                newlog.IsLocked = false;
                newlog.CreatedBySystemUserId = CustomerID;

                core.PLMApplicationHistortyLogs.Add(newlog);
                core.SaveChanges();
                return true;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }
        }
        public static bool ActivityTrackerHuman(eServicesDbContext core, int ApplicationId, string ActivityTrackerMessage, int CustomerID)
        {
            try
            {
                PLMApplicationHistortyLog newlog = new PLMApplicationHistortyLog();

                newlog.HumanSettlementApplicationId = ApplicationId;
                newlog.AuditAction = ActivityTrackerMessage;
                newlog.UserId = CustomerID;
                newlog.CreatedDateTime = DateTime.Now;
                newlog.IsActive = true;
                newlog.IsDeleted = false;
                newlog.IsLocked = false;
                newlog.CreatedBySystemUserId = CustomerID;

                core.PLMApplicationHistortyLogs.Add(newlog);
                core.SaveChanges();
                return true;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }
        }
        public static void ChangeDepartmentStatus(eServicesDbContext core, int statusId, int Id)
        {
            var findApplication = core.ApplicationDepart.FirstOrDefault(x => x.Id == Id) ?? null;
            findApplication.StatusId = statusId;
            core.SaveChanges();


        }
        public static void ChangeDepartmentStatusAudit(eServicesDbContext core, int statusId, int Id, string ActivityTrackerMessage, int? PropertyLeaseId, int CustomerId)
        {
            var findApplication = core.ApplicationDepart.FirstOrDefault(x => x.Id == Id) ?? null;
            findApplication.StatusId = statusId;
            core.SaveChanges();

            var Result = ActivityTrackerAudit(core, PropertyLeaseId, ActivityTrackerMessage, CustomerId);
        }
        public static void TLAChangeDepartmentStatusAudit(eServicesDbContext core, int statusId, int Id, string ActivityTrackerMessage, int? LeaseId, int CustomerId)
        {
            var findApplication = core.ApplicationDepart.FirstOrDefault(x => x.Id == Id) ?? null;
            findApplication.StatusId = statusId;
            core.SaveChanges();
        }
        public static void MarkMatchedUnitAsDeleted(eServicesDbContext core, int matchedUnitId)
        {
            var findMatchedItem = core.MatchedUnits.FirstOrDefault(x => x.Id == matchedUnitId) ?? null;
            findMatchedItem.IsDeleted = true;
            findMatchedItem.RejectedProperty = true;
            core.SaveChanges();
        }
        public static void MarkMatchedUnitAsRejected(eServicesDbContext core, int matchedUnitId)
        {
            var findMatchedItem = core.MatchedUnits.FirstOrDefault(x => x.Id == matchedUnitId) ?? null;
            findMatchedItem.RejectedProperty = true;
            core.SaveChanges();
        }
        public static void MarkMatchedUnitAsAccepted(eServicesDbContext core, int matchedUnitId)
        {
            var findMatchedItem = core.MatchedUnits.FirstOrDefault(x => x.Id == matchedUnitId) ?? null;
            findMatchedItem.RejectedProperty = false;
            findMatchedItem.IsAccepted = true;
            core.Entry(findMatchedItem).State = EntityState.Modified;
            core.SaveChanges();
            //var Id = core.waitingListQues.FirstOrDefault(x => x.PropertyLeaseApplicationId == findMatchedItem.PropertyLeaseApplicationId).Id;
            //RemoveApplicationFromWaitingList(core, Id);
        }
        public static void DeactivateDuplicateLeaseRecords(eServicesDbContext core)
        {
            var list = core.LeaseDetails.ToList();
            foreach (var Item in list)
            {
                var records = core.LeaseDetails.Where(x => x.PropertyLeaseApplicationId == Item.PropertyLeaseApplicationId &&
                x.StatusId != (core.Status.FirstOrDefault(r => r.Key == StatusKeys.DeactivateLeaseNewCaptured).Id)).OrderBy(x => x.Id).ToList();
                if (records.Count > 1)
                {
                    var count = records.Count;
                    foreach (var rec in records)
                    {
                        while (count > 1 && rec.StatusId != core.Status.FirstOrDefault(x => x.Key == StatusKeys.DeactivateLeaseNewCaptured).Id)
                        {
                            ChangeLeaseStatusII(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.DeactivateLeaseNewCaptured).Id, (int)rec.Id);
                            rec.IsNew = false;
                        }
                        count--;
                    }
                }
            }

            core.SaveChanges();
        }
        public static void SaveToWaitingListQueue(eServicesDbContext core, int propertyApplicationId, int userId)
        {
            var findItem = core.waitingListQues.OrderByDescending(x => x.Position).FirstOrDefault(x => x.Position != null && x.PropertyLeaseApplicationId != null);

            WaitingListQue queue = new WaitingListQue
            {
                PropertyLeaseApplicationId = propertyApplicationId,
                CreatedDateTime = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                IsMatched = false,
                CreatedBySystemUserId = userId,
                QueueDate = DateTime.Now,
                ModifiedDateTime = DateTime.Now
            };

            core.waitingListQues.Add(queue);
            core.SaveChanges();

            AddWaitingListPosition(core, queue.Id);
        }
        public static void SaveToWaitingListQueueHuman(eServicesDbContext core,  WaitingListQueueHuman wait)
        {
            var open = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
            open.Value = "1";
            core.WaitingListQueueHumans.Add(wait);
            core.SaveChanges();

            var findItem = core.WaitingListQueueHumans.OrderByDescending(x => x.Position).Where(x => x.Position != null).ToList();
            var rrq = findItem.Select(x => x.Position).ToList();
            var value = 0;
            var last = rrq.Count == 0 ? 0 : (int)rrq[0];
            if (findItem.Count > 0) value = last + 1;
            if (findItem.Count == 0) value = 1;

            wait.Position = value;
            open.Value = "0";
            core.SaveChanges();
        }
        public static void ModifyWaitinglistPositions(eServicesDbContext core)
        {
            var findItem = core.waitingListQues.OrderBy(x => x.QueueDate).Where(x => x.Position != null && !x.IsMatched).ToList();
            int tmp = 1;
            foreach (var Item in findItem)
            {
                Item.Position = tmp;
                core.SaveChanges();
                tmp += 1;
            }
        }
        public static void RemoveApplicationFromWaitingList(eServicesDbContext core, int Id)
        {
            var finfItem = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
            finfItem.Value = "1";
            core.SaveChanges();

            var remove = core.waitingListQues.FirstOrDefault(x => x.Id == Id);
            int crp = (int)remove.Position;
            var findItem = core.waitingListQues.OrderByDescending(x => x.QueueDate).Where(x => x.Position > crp && x.Position != null).ToList();
            int tmp = 1;
            foreach (var Item in findItem)
            {
                Item.Position = Item.Position - tmp;
                core.SaveChanges();
            }

            remove.Position = null;
            core.SaveChanges();

            finfItem.Value = "0";
            core.SaveChanges();
        }
        public static void HumanRemoveApplicationFromWaitingList(eServicesDbContext core, int Id)
        {
            var finfItem = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
            finfItem.Value = "1";
            core.SaveChanges();

            var remove = core.WaitingListQueueHumans.FirstOrDefault(x => x.Id == Id);
            int crp = (int)remove.Position;
            var findItem = core.WaitingListQueueHumans.OrderByDescending(x => x.QueueDate).Where(x => x.Position > crp && x.Position != null).ToList();
            int tmp = 1;
            foreach (var Item in findItem)
            {
                Item.Position = Item.Position - tmp;
                core.SaveChanges();
            }

            remove.Position = null;
            core.SaveChanges();

            finfItem.Value = "0";
            core.SaveChanges();
        }
        public static void MoveToPositionWaitingListQueue(eServicesDbContext core, int Id, int Position)
        {
            var finfItem = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
            finfItem.Value = "1";
            core.SaveChanges();

            var move = core.waitingListQues.FirstOrDefault(x => x.Id == Id);
            int crp = (int)move.Position;
            int tmp = 1;
            if (crp > Position)
            {
                var findItem = core.waitingListQues.OrderByDescending(x => x.QueueDate).Where(x => x.Position < crp && x.Position >= Position && x.Position != null).ToList();
                foreach (var Item in findItem)
                {
                    Item.Position = Item.Position + tmp;
                    core.SaveChanges();
                }
            }

            if (crp < Position)
            {
                var findItem2 = core.waitingListQues.OrderByDescending(x => x.QueueDate).Where(x => x.Position > crp && x.Position <= Position && x.Position != null).ToList();
                foreach (var Item in findItem2)
                {
                    Item.Position = Item.Position - tmp;
                    core.SaveChanges();
                }
            }

            move.Position = Position;
            core.SaveChanges();
            finfItem.Value = "0";
            core.SaveChanges();
        }
        public static void AddWaitingListPosition(eServicesDbContext core, int Id)
        {
            var open = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
            open.Value = "1";
            core.SaveChanges();

            var findItem = core.waitingListQues.OrderByDescending(x => x.Position).Where(x => x.Position != null && x.PropertyLeaseApplicationId != null).ToList();
            var rrq = findItem.Select(x => x.Position).ToList();
            var value = 0;
            var last = rrq.Count == 0 ? 0 : (int)rrq[0];
            if (findItem.Count > 0) value = last + 1;
            if (findItem.Count == 0) value = 1;

            var QueueItem = core.waitingListQues.FirstOrDefault(x => x.Id == Id);
            QueueItem.Position = value;
            core.SaveChanges();

            open.Value = "0";
            core.SaveChanges();
        }
        public static void AddWaitingListPositionHumnan(eServicesDbContext core, int Id)
        {
            var open = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
            open.Value = "1";
            core.SaveChanges();

            var findItem = core.waitingListQues.OrderByDescending(x => x.Position).Where(x => x.Position != null && x.HumanSettlementApplicationId != null).ToList();
            var rrq = findItem.Select(x => x.Position).ToList();
            var value = 0;
            var last = rrq.Count == 0 ? 0 : (int)rrq[0];
            if (findItem.Count > 0) value = last + 1;
            if (findItem.Count == 0) value = 1;

            var QueueItem = core.waitingListQues.FirstOrDefault(x => x.Id == Id);
            QueueItem.Position = value;
            core.SaveChanges();

            open.Value = "0";
            core.SaveChanges();
        }
        public static void MarkQueueAsUnMatched(eServicesDbContext core, int propertyApplicationId)
        {
            var open = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
            open.Value = "1";
            core.SaveChanges();

            try
            {
                var move = core.waitingListQues.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyApplicationId) ?? null;
                move.IsMatched = false;

                core.SaveChanges();

                int crp = (int)move.Position;
                var findItem = core.waitingListQues.OrderByDescending(x => x.Position).Where(x => x.Position > crp && x.Position != null).ToList();
                var rrq = findItem.Select(x => x.Position).ToList();
                var value = rrq[0];
                foreach (var Item in findItem)
                {
                    Item.Position -= 1;
                    core.SaveChanges();
                }

                move.Position = value;
                core.SaveChanges();
            }
            catch (Exception)
            {

            }

            open.Value = "0";
            core.SaveChanges();
        }
        public static void MarkQueueAsUnMatched2(eServicesDbContext core, int HumanSettlementApplicationId)
        {
            var open = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
            open.Value = "1";
            core.SaveChanges();

            try
            {
                var move = core.WaitingListQueueHumans.FirstOrDefault(x => x.HumanSettlementApplicationId == HumanSettlementApplicationId) ?? null;
                //move.IsMatched = false;
                core.SaveChanges();
                int crp = (int)move.Position;
                var findItem = core.WaitingListQueueHumans.OrderByDescending(x => x.Position).Where(x => x.Position > crp && x.Position != null).ToList();
                var rrq = findItem.Select(x => x.Position).ToList();
                var value = rrq[0];
                foreach (var Item in findItem)
                {
                    Item.Position -= 1;
                    core.SaveChanges();
                }

                move.Position = value;
                core.SaveChanges();
            }
            catch 
            {

            }

            open.Value = "0";
            core.SaveChanges();
        }
        public static void ValidateInactionedMatchedUnits(eServicesDbContext core)
        {
            var finfItem = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
            finfItem.Value = "1";
            core.SaveChanges();

            var DateToGet = DateTime.Now.AddMinutes(-5);
            var date2 = Convert.ToDateTime("2022-06-14 12:18:31.637");
            var match = core.MatchedUnits.Include(r => r.Units).Include(r => r.PropertyLeaseApplication).Where(x => !x.IsAccepted && !x.RejectedProperty && x.CreatedDateTime < DateToGet && x.UnitsId != null).ToList();
            foreach (var Item in match)
            {
                Item.RejectedProperty = true;
                core.SaveChanges();

                Item.Units.IsTaken = false;
                core.SaveChanges();

                VerifrySecondPropertyReject(core, (int)Item.PropertyLeaseApplicationId);
            }

            finfItem.Value = "0";
            core.SaveChanges();
        }
        public static bool RemoveWaitingListQueueItem(eServicesDbContext core, int Id)
        {
            try
            {
                var finfItem = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
                finfItem.Value = "1";
                core.SaveChanges();

                var remove = core.waitingListQues.FirstOrDefault(x => x.Id == Id);
                int crp = (int)remove.Position;
                var findItem = core.waitingListQues.OrderByDescending(x => x.QueueDate).Where(x => x.Position > crp && x.Position != null).ToList();
                int tmp = 1;
                foreach (var Item in findItem)
                {
                    Item.Position = Item.Position - tmp;
                    core.SaveChanges();
                }

                remove.Position = null;
                core.SaveChanges();

                finfItem.Value = "0";
                core.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public static void ValidateWaitingListTime(eServicesDbContext core)
        {
            DateTime Date = DateTime.Now.AddDays(-5).Date;
            var wait = core.waitingListQues.Where(x => x.IsMatched && x.Position != null).ToList();
            var r = wait.Select(x => x.PropertyLeaseApplicationId).ToList();
            var rrq = core.MatchedUnits.Where(x => x.CreatedDateTime.Value < Date && !x.IsAccepted & !x.RejectedProperty && r.Contains(x.PropertyLeaseApplicationId)).ToList();
            //if (rrq.Count > 0)
            //{
            //    var finfItem = core.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting);
            //    finfItem.Value = "1";
            //    core.SaveChanges();
            //    try
            //    {
            //        foreach (var Item in rrq)
            //        {
            //            MarkQueueAsUnMatched(core, (int)Item.PropertyLeaseApplicationId);

            //            //int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.UnitInspectionScheduleByApplicant).Id;
            //            //EmailHelper.CustomerEmailNotification(core, (int)Item.PropertyLeaseApplicationId, emailboodyId);

            //            core.Entry(Item).State = EntityState.Deleted;
            //            core.SaveChanges();
            //        }
            //    }
            //    catch (Exception)
            //    {

            //    }
            //    finfItem.Value = "0";
            //    core.SaveChanges();
            //}
        }
        public static void WaitingListReEntry(eServicesDbContext core, int propertyApplicationId, int userId)
        {
            WaitingListQue queue = new WaitingListQue
            {
                PropertyLeaseApplicationId = propertyApplicationId,
                CreatedDateTime = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                IsMatched = false,
                CreatedBySystemUserId = userId,
                QueueDate = DateTime.Now,
                ModifiedDateTime = DateTime.Now
            };

            core.waitingListQues.Add(queue);
            core.SaveChanges();
        }
        public static void SaveApplicantsUnit(eServicesDbContext core, ApplicationAllocatedProperty unit, int propertyApplicationId, int userId, int matchedUnitId)
        {
            ApplicantUnit applicantUnit = new ApplicantUnit
            {
                IsActive = true,
                IsDeleted = false,
                CreatedDateTime = DateTime.Now,
                DepositPaid = 0.00,
                OutstandingDepopsitAmount = unit.RequiedDepositAmount,
                PropertyLeaseApplicationId = propertyApplicationId,
                MatchedID = matchedUnitId,
                CreatedBySystemUserId = userId
            };
            core.ApplicantUnits.Add(applicantUnit);
            core.SaveChanges();
            BaseHelper baseHelper = new BaseHelper();
            baseHelper.Initialise(core);
            SaveUnitHistory(core, propertyApplicationId, "Allocated Unit Accepted By Customer", unit.Id, baseHelper.SystemUser.Id);
        }
        public static void RemoveApplicantAndMatched(eServicesDbContext core, int propertyApplicationId)
        {
            var findItem = core.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyApplicationId);
            findItem.IsActive = false;
            findItem.IsDeleted = true;
            core.SaveChanges();

            var findMatch = core.MatchedUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyApplicationId && x.IsAccepted && !x.IsDeleted && x.IsActive && x.Id == findItem.MatchedID);
            findMatch.IsActive = false;
            findMatch.IsDeleted = true;
            findMatch.IsAccepted = false;
            core.SaveChanges();

            var findUnit = core.Units.FirstOrDefault(x => x.Id == findMatch.UnitsId && x.IsTaken && x.IsActive && !x.IsDeleted);
            findUnit.IsTaken = false;
            core.SaveChanges();
        }
        public static bool AcceptMatchedUnit(eServicesDbContext core, ApplicationAllocatedProperty unit, int propertyApplicationId, int userId, int matchedUnitId)
        {
            try
            {
                SaveApplicantsUnit(core, unit, propertyApplicationId, userId, matchedUnitId);
                MarkMatchedUnitAsAccepted(core, matchedUnitId);
                ChangeApplicationStatus(core, (int)core.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingDepositPaid)?.Id, propertyApplicationId);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool HumanAcceptMatchedUnit(eServicesDbContext core, UnitsHumanSettlement01 unit, int HumanSesttlementApplicationId, int userId, int matchedUnitId)
        {
            try
            {
                HumanSaveApplicantsUnit(core, unit, HumanSesttlementApplicationId, userId, matchedUnitId);
                HumanMarkMatchedUnitAsAccepted(core, matchedUnitId);
                ChangeHumanStatus(core, (int)core.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingAgreementUpdate)?.Id, HumanSesttlementApplicationId);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool RejectMatchedUnit(eServicesDbContext core, ApplicationAllocatedProperty unit, int propertyApplicationId, int matchedUnitId)
        {
            try
            {
                MarkUnitAsAvailable(core, (int)unit?.Id);
                MarkMatchedUnitAsDeleted(core, matchedUnitId);
                MarkMatchedUnitAsRejected(core, matchedUnitId);
                ChangeApplicationStatus(core, (int)core.Status.FirstOrDefault(x => x.Key == StatusKeys.VertedApplication)?.Id, propertyApplicationId);
                VerifrySecondPropertyReject(core, propertyApplicationId);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool HumanRejectMatchedUnit(eServicesDbContext core, UnitsHumanSettlement01 unit, int HumanSettlementApplicationId, int matchedUnitId)
        {
            try
            {
                MarkUnitAsAvailableHuman(core, (int)unit?.Id);
                MarkMatchedUnitAsDeleted(core, matchedUnitId);
                MarkMatchedUnitAsRejected(core, matchedUnitId);
                ChangeHumanStatus(core, (int)core.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicationDiscardedNoUnitAvailable)?.Id, HumanSettlementApplicationId);
                HumanVerifrySecondPropertyReject(core, HumanSettlementApplicationId);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static void HumanSaveApplicantsUnit(eServicesDbContext core, UnitsHumanSettlement01 unit, int HumanSesttlementApplicationId, int userId, int matchedUnitId)
        {
            ApplicantUnit applicantUnit = new ApplicantUnit
            {
                IsActive = true,
                IsDeleted = false,
                CreatedDateTime = DateTime.Now,
                DepositPaid = 0.00,
                OutstandingDepopsitAmount = (double)unit?.PropertyDeposit,
                HumanSettlementApplicationId = HumanSesttlementApplicationId,
                MatchedID = matchedUnitId,
                CreatedBySystemUserId = userId
            };
            core.ApplicantUnits.Add(applicantUnit);
            core.SaveChanges();
        }
        public static void HumanVerifrySecondPropertyReject(eServicesDbContext core, int HumanSettlementApplicationId)
        {
            try
            {
                var findQueueItem = core.MatchedUnits.Where(x => x.HumanSettlementApplicationId == HumanSettlementApplicationId && x.RejectedProperty).ToList();
                if (findQueueItem.Count > 1)
                {
                    foreach (var item in findQueueItem)
                    {
                        item.RejectedProperty = true;
                        core.SaveChanges();
                    }
                    ChangeHumanStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.ApplicationDiscardedNoUnitAvailable).Id, HumanSettlementApplicationId);
                    //Send e-mail and SMS notification
                    int emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RemoveWaitingListOnSecondReject).Id;
                    EmailHelper.CustomerEmailOrSMSNotification(core, HumanSettlementApplicationId, emailboodyId);
                }

            }
            catch
            {

            }
        }
        public static void HumanMarkMatchedUnitAsAccepted(eServicesDbContext core, int matchedUnitId)
        {
            var findMatchedItem = core.MatchedUnits.FirstOrDefault(x => x.Id == matchedUnitId) ?? null;
            findMatchedItem.RejectedProperty = false;
            findMatchedItem.IsAccepted = true;
            core.Entry(findMatchedItem).State = EntityState.Modified;
            core.SaveChanges();

            var Id = core.WaitingListQueueHumans.FirstOrDefault(x => x.HumanSettlementApplicationId == findMatchedItem.HumanSettlementApplicationId).Id;
            HumanRemoveApplicationFromWaitingList(core, Id);
        }
        public static void UpdatePropertyLeaseDates(eServicesDbContext core, int Months, int LeaseId)
        {
            var findRecord = core.LeaseDetails.FirstOrDefault(x => x.Id == LeaseId);
            findRecord.EndDate = Convert.ToDateTime(findRecord.EndDate).AddMonths(Months);
            findRecord.RenewalNotice = Convert.ToDateTime(findRecord.RenewalNotice).AddMonths(Months);
            findRecord.TerminationNotice = Convert.ToDateTime(findRecord.TerminationNotice).AddMonths(Months);
            findRecord.PeriodInMonths -= Months;
            findRecord.IsRenewed = false;
            findRecord.IsNew = true;
            core.SaveChanges();
        }
        public static void AcceptRenewalOfferPeriod(eServicesDbContext core, int Id)
        {
            var findRecord = core.PropertyLeaseRenewalOffers.FirstOrDefault(x => x.Id == Id);
            findRecord.IsAccepted = true;
            core.SaveChanges();
        }
        public static void AddMonthsToDates(eServicesDbContext core, int Id)
        {
            try
            {
                var findRecord = core.LeaseDetails.FirstOrDefault(x => x.Id == Id);
                int months = findRecord.MonthsOffered;
                findRecord.EndDate = Convert.ToDateTime(findRecord.EndDate).AddMonths(months);
                findRecord.RenewalNotice = Convert.ToDateTime(findRecord.RenewalNotice).AddMonths(months);
                findRecord.TerminationNotice = Convert.ToDateTime(findRecord.TerminationDate).AddMonths(months);
                findRecord.MonthsOffered = 0;
                core.SaveChanges();
            }
            catch (Exception IO)
            {
            }

        }
        public static void RemoveOfferedMonths(eServicesDbContext core, int Id)
        {
            var findRecord = core.LeaseDetails.FirstOrDefault(x => x.Id == Id);
            findRecord.MonthsOffered = 0;
            core.SaveChanges();
        }
        public static void MarkLeaseAsOld(eServicesDbContext core, int Id)
        {
            var findRecord = core.LeaseDetails.FirstOrDefault(x => x.Id == Id);
            findRecord.IsNew = false;
            core.SaveChanges();
        }
        public static void RevenueSignDebitOrderAuthorityForm(eServicesDbContext core, SystemUser User, DebitOrderRegistration Debit)
        {
            Debit.RevenueOfficerId = User.Id;
            Debit.RevenueOfficer = User.FirstName + " " + User.LastName;
            Debit.RevenueSignature = true;
            core.SaveChanges();
        }
        public static void LeaseOfficialSignDebitOrderAuthorityForm(eServicesDbContext core, Customer User, DebitOrderRegistration Debit)
        {
            Debit.LeasingOfficerId = User.Id;
            Debit.LeasingOfficer = User.FirstName + " " + User.LastName;
            Debit.LeasingSignature = true;
            core.SaveChanges();

            var ResponsibilityTypeId = core.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.DebitOrderVAlidation).FirstOrDefault();
            RoundRobinMarkJobAsFinished(core, (int)Debit.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, User.Id);
        }
        public static void Commiteedate(eServicesDbContext core, int PropertyId, string Date)
        {
            var findItem = core.PropertyLeaseApplications.FirstOrDefault(x => x.Id == PropertyId);
            findItem.CommitteeDate = Convert.ToDateTime(Date);
            core.SaveChanges();
        }
        public static void Commiteedate2(eServicesDbContext core, int ApplicationId, string Date)
        {
            var findItem = core.HumanSettlementApplications.FirstOrDefault(x => x.Id == ApplicationId);
            findItem.CommitteeDate = Convert.ToDateTime(Date);
            core.SaveChanges();
        }
        public static void PropertyLeaseMasterData(eServicesDbContext core, ApplicationAllocatedProperty Unit, LeaseDetails lease, PropertyLeaseApplication property)
        {
            var Occupants = core.PropertyResidents.OrderBy(x => x.Id).Where(x => (x.LeaseDetailsId == lease.Id || x.RenewalLeaseId == lease.Id) && x.IsActive && !x.IsDeleted && x.StatusId != (core.Status.FirstOrDefault(r => r.Key == StatusKeys.DeactiveOccupant).Id)).ToList() ?? null;
            var Q = core.Customers.FirstOrDefault(x => x.Id == Unit.OfferedComplex.HousingSuperId);
            var day = DateTime.DaysInMonth(DateTime.Now.Year, 07);
            PropertyLeaseAgreementMaster MasterLease = new PropertyLeaseAgreementMaster();

            MasterLease = core.propertyLeaseAgreementMasters.Include(x => x.CreatedBySystemUser).Include(x => x.LeaseDetails).Include(x => x.PropertyLeaseApplication).FirstOrDefault(x => x.PropertyLeaseApplicationId == property.Id && x.LeaseDetailsId == lease.Id);

            if (MasterLease == null)
            {
                MasterLease = new PropertyLeaseAgreementMaster();
                // Initialize MasterLease properties here if needed
            }

            MasterLease.PropertyLeaseApplicationId = property.Id;
            MasterLease.LeaseDetailsId = lease.Id;
            MasterLease.RepresentedBy = Q.FirstName + " " + Q.LastName;
            MasterLease.ApplicantFullName = property.FirstName + " " + property.LastName;
            MasterLease.ApplicantIdentityNumber = property.IDNo;
            MasterLease.UnitNumber = Unit.SpaceUnitNumber + " " + Unit.BuildingName;
            MasterLease.PreparationFee = (double)lease.PreparationFee;
            MasterLease.CreditCheckFee = (double)lease.CreditCheckFee;
            MasterLease.CalculatedAsFolllows = (double)lease.CalculatedAsFolllows;
            MasterLease.InitialDepositPremises = (double)lease.InitialDepositPremises;
            MasterLease.DepositTenantContribution = Unit.RequiedDepositAmount;
            MasterLease.MonthlyUnitRental = Unit.MonthlyRentalAmount;
            MasterLease.ShadePortParking = (double)lease.ShadePortParking;
            MasterLease.OpenParking = (double)lease.OpenParking;
            MasterLease.StoreRooms = (double)lease.StoreRooms;
            MasterLease.Electricity = lease.Electricity != null ? (double)lease.Electricity : 0;
            MasterLease.Refuse = (double)lease.Refuse;
            MasterLease.SecurityFee = (double)lease.SecurityFee;
            MasterLease.Sewerage = (double)lease.Sewerage;
            MasterLease.Water = (double)lease.Water;
            MasterLease.SPP = lease.SPP;
            MasterLease.OPP = lease.OPP;
            MasterLease.STR = lease.STR;
            MasterLease.ELEC = lease.ELEC;
            MasterLease.SEC = lease.SEC;
            MasterLease.WTR = lease.WTR;
            MasterLease.BedRooms = Unit.NumOfBeds;
            MasterLease.FloorNumber = lease.FloorNumber;
            MasterLease.BlockNumber =/* Unit.PreferredComplexArea.Name*/"";
            MasterLease.Day = "01";
            MasterLease.CommencementDate = Convert.ToDateTime(lease.StartDate).ToString("MMMM", CultureInfo.InvariantCulture) + " " + Convert.ToDateTime(lease.StartDate).Year.ToString();
            MasterLease.EndDate = Convert.ToDateTime(lease.EndDate).ToString("dd/MM/yyyy").Substring(0, 2) + "/" + Convert.ToDateTime(lease.EndDate).ToString("dd/MM/yyyy").Substring(3, 2) + "/" + Convert.ToDateTime(lease.EndDate).ToString("dd/MM/yyyy").Substring(8, 2);
            MasterLease.NoPenaltyMonth = lease.TerminationNotice.ToString().Substring(0, 10);
            MasterLease.RentalDueUntill = lease.TerminationNotice.ToString().Substring(0, 10);
            MasterLease.PenaltyMonth = lease.TerminationNotice.ToString().Substring(0, 10);
            MasterLease.InitialDepositAmonunt = Unit.RequiedDepositAmount;
            MasterLease.LeaseAdministrationFee = lease.LeaseAdministrationFee != null ? (double)lease.LeaseAdministrationFee : 0;
            MasterLease.UnitRentalAmountPM = Unit.MonthlyRentalAmount;
            MasterLease.UnitRentalDay = lease.EndDate.ToString().Substring(8, 2);
            MasterLease.UnitRentalDate = Convert.ToDateTime(lease.EndDate).ToString("MMMM", CultureInfo.InvariantCulture) + " " + Convert.ToDateTime(lease.EndDate).Year.ToString();
            MasterLease.RentalIncreaseDate = "01/07/" + DateTime.Now.Year.ToString().Substring(2, 2);
            MasterLease.CarportParkingBayNumber = lease.CarportParkingBayNumber;
            MasterLease.OPenParkingBayNumber = lease.OPenParkingBayNumber;
            MasterLease.OPenParkingBayRental = (double)lease.OpenParking;
            MasterLease.ShadePortBayNumber = lease.ShadePortBayNumber;
            MasterLease.ShadePortBayRental = (double)lease.ShadePortParking;
            MasterLease._Of1July = day.ToString();
            MasterLease.ParkingIncreaseDay = lease.EndDate.ToString().Substring(8, 2);
            MasterLease.ParkingIncreaseMonth = lease.EndDate.ToString().Substring(0, 7);
            MasterLease._water = (double)lease.Water;
            MasterLease._sewerage = (double)lease.Refuse;
            MasterLease._refuse = (double)lease.Refuse;
            MasterLease.PeopleAllowedOnPremises = Unit.NumOfBeds;
            MasterLease.LandlordAddress = Unit.Address;

            if (Occupants != null)
            {
                int order = 1;
                foreach (var item in Occupants)
                {
                    switch (order)
                    {
                        case (1):
                            MasterLease.OccupantONE = item.FirstNames + " " + item.LastName;
                            MasterLease.OccupantONEIdentityNo = item.IDNo;
                            break;

                        case (2):
                            MasterLease.OccupantTWO = item.FirstNames + " " + item.LastName;
                            MasterLease.OccupantTWOIdentityNo = item.IDNo;
                            break;

                        case (3):
                            MasterLease.OccupantTHREE = item.FirstNames + " " + item.LastName;
                            MasterLease.OccupantTHREEIdentityNo = item.IDNo;
                            break;

                        case (4):
                            MasterLease.OccupantFOUR = item.FirstNames + " " + item.LastName;
                            MasterLease.OccupantFOURIdentityNo = item.IDNo;
                            break;

                        case (5):
                            MasterLease.OccupantFIVE = item.FirstNames + " " + item.LastName;
                            MasterLease.OccupantFIVEIdentityNo = item.IDNo;
                            break;

                        case (6):
                            MasterLease.OccupantSIX = item.FirstNames + " " + item.LastName;
                            MasterLease.OccupantSIXIdentityNo = item.IDNo;
                            break;
                    }
                    order++;
                }
            }

            var findItem = core.propertyLeaseAgreementMasters.Include(x=>x.CreatedBySystemUser).Include(x=>x.LeaseDetails).Include(x => x.PropertyLeaseApplication).FirstOrDefault(x => x.PropertyLeaseApplicationId == property.Id && x.LeaseDetailsId == lease.Id);

            if (findItem == null)
            {
                core.propertyLeaseAgreementMasters.Add(MasterLease);
            }
            else
            {
                //MasterLease.Id = findItem.Id;
                // Update only the modified properties
                //var entry = core.Entry(findItem);
                //foreach (var prop in typeof(PropertyLeaseAgreementMaster).GetProperties())
                //{
                //    if (prop.CanWrite)
                //    {
                //        var newValue = prop.GetValue(MasterLease);
                //        if (newValue != null && newValue !=  "0" && !newValue.Equals(prop.GetValue(findItem)))
                //        {
                //            prop.SetValue(findItem, newValue);
                //            entry.Property(prop.Name).IsModified = true;
                //        }
                //    }
                //}
                core.Entry(MasterLease).State = EntityState.Modified;
            }
           
            core.SaveChanges();
            //else
            //{
            //    MasterLease.Id = findItem.Id;
            //    MasterLease.LeaseDetails = findItem.LeaseDetails;
            //    MasterLease.PropertyLeaseApplication = findItem.PropertyLeaseApplication;
            //    MasterLease.LeaseDetailsId = findItem.LeaseDetailsId;
            //    MasterLease.PropertyLeaseApplicationId = findItem.PropertyLeaseApplicationId;
            //    MasterLease.CreatedBySystemUser = findItem.CreatedBySystemUser;
            //    MasterLease.CreatedDateTime = findItem.CreatedDateTime;
            //    MasterLease.CreatedBySystemUserId = findItem.CreatedBySystemUserId;
            //    // Use reflection to copy properties from MasterLease to findItem
            //    foreach (var prop in typeof(PropertyLeaseAgreementMaster).GetProperties())
            //    {
            //        if (prop.CanWrite)
            //        {
            //            prop.SetValue(findItem, prop.GetValue(MasterLease));
            //        }
            //    }
            //    core.Entry(findItem).State = EntityState.Modified;
            //}

            //core.SaveChanges();
            //var findItem = core.propertyLeaseAgreementMasters.FirstOrDefault(x => x.PropertyLeaseApplicationId == property.Id && x.LeaseDetailsId == lease.Id);
            //var result = findItem == null ? core.propertyLeaseAgreementMasters.Add(MasterLease) : findItem = MasterLease;
            //core.SaveChanges();


        }
        public static void HumanSettlementMasterAgreement(eServicesDbContext core, HumanSettlementLeaseMaster master, HumanSettlementApplication application, HumanSettlementLeaseDetails Info)
        {
            try
            {
               
                var findItem = core.HSLeaseAgreementMasters.FirstOrDefault(x => x.HumanSettlementApplicationId == application.Id) ?? null;
                if (findItem == null)
                {
                    var region = core.RegionTypes.FirstOrDefault(r => r.Id == application.PreferredComplexArea.RegionTypeId);
                    var Escalation = core.EscalationMaster.Where(r => r.HSUnitTypologyId == application.UnitTypologyId && r.HSUnitCategoryId == application.UnitCategoryId && r.HSIncomeBracketId == application.HSIncomeBracketId).ToList();
                    if (application.IsMaster)
                    {
                        var start_date = Convert.ToDateTime(master.StartDate);
                        var end_date = Convert.ToDateTime(master.EndDate);
                        HSLeaseAgreementMaster Item = new HSLeaseAgreementMaster();
                        Item.HumanSettlementApplicationId = application.Id;
                        Item.ApplicantFullName = application.ApplicantFullName;
                        Item.IdentityNumber = application.IDNo;
                        Item.TheUnit = "Unit no. " + master.SpaceUnitNo + " " + application.PreferredComplexArea.Name + ", " + region.Name;
                        Item.CommenceDate = start_date.Day.ToString("00") + " " + GetMonthName(start_date.Month) + " " + start_date.Year;
                        Item.ExpireryDate = end_date.Day.ToString("00") + " " + GetMonthName(end_date.Month) + " " + end_date.Year;
                        Item.PayableAmount = Convert.ToDouble(master.RentalAmount);
                        if (application.NominateSpouse)
                        {
                            Item.Nominated = true;
                            Item.NoTitle = application.SecAppTitleType.Name;
                            Item.NoName = application.SecAppFirstName;
                            Item.NoSurname = application.SecAppLastName;
                            Item.NoIdentityNumber = application.SecAppIDNo;
                            Item.NoRelationship = "Spouse";
                            Item.NoContact = "(C) " + application.SecAppCellNo + " | (E) " + application.SecAppEmail;
                        }
                        else
                        {

                        }

                        core.HSLeaseAgreementMasters.Add(Item);
                        core.SaveChanges();
                    }
                    else
                    {
                        var start_date = Convert.ToDateTime(Info.StartDate);
                        var end_date = Convert.ToDateTime(Info.EndDate);
                        HSLeaseAgreementMaster Item = new HSLeaseAgreementMaster();
                        Item.HumanSettlementApplicationId = application.Id;
                        Item.ApplicantFullName = application.ApplicantFullName;
                        Item.IdentityNumber = application.IDNo;
                        Item.TheUnit = "Unit no. " + Info.SpaceUnitNo + " " + application.PreferredComplexArea.Name + ", " + region.Name;
                        Item.CommenceDate = start_date.Day.ToString("00") + " " + GetMonthName(start_date.Month) + " " + start_date.Year;
                        Item.ExpireryDate = end_date.Day.ToString("00") + " " + GetMonthName(end_date.Month) + " " + end_date.Year;
                        Item.PayableAmount = Convert.ToDouble(Info.RentalAmount);
                        if (application.NominateSpouse)
                        {
                            Item.Nominated = true;
                            Item.NoTitle = application.SecAppTitleType.Name;
                            Item.NoName = application.SecAppFirstName;
                            Item.NoSurname = application.SecAppLastName;
                            Item.NoIdentityNumber = application.SecAppIDNo;
                            Item.NoRelationship = "Spouse";
                            Item.NoContact = "(C) " + application.SecAppCellNo + " | (E) " + application.SecAppEmail;
                        }
                        else
                        {

                        }
                        core.HSLeaseAgreementMasters.Add(Item);
                        core.SaveChanges();
                    }
                }
                else
                {
                    var region = core.RegionTypes.FirstOrDefault(r => r.Id == application.PreferredComplexArea.RegionTypeId);
                    if (application.IsMaster)
                    {
                        var start_date = Convert.ToDateTime(master.StartDate);
                        var end_date = Convert.ToDateTime(master.EndDate);
                        findItem.HumanSettlementApplicationId = application.Id;
                        findItem.ApplicantFullName = application.ApplicantFullName;
                        findItem.IdentityNumber = application.IDNo;
                        findItem.TheUnit = "Unit no. " + master.SpaceUnitNo + " " + application.PreferredComplexArea.Name + ", " + region.Name;
                        findItem.CommenceDate = start_date.Day.ToString("00") + " " + GetMonthName(start_date.Month)+ " " + start_date.Year;
                        findItem.ExpireryDate = end_date.Day.ToString("00") + " " + GetMonthName(end_date.Month) + " " + end_date.Year;
                        findItem.PayableAmount = Convert.ToDouble(master.RentalAmount);
                        if (application.NominateSpouse)
                        {
                            findItem.Nominated = true;
                            findItem.NoTitle = application.SecAppTitleType.Name;
                            findItem.NoName = application.SecAppFirstName;
                            findItem.NoSurname = application.SecAppLastName;
                            findItem.NoIdentityNumber = application.SecAppIDNo;
                            findItem.NoRelationship = "Spouse";
                            findItem.NoContact = "(C) " + application.SecAppCellNo + " | (E) " + application.SecAppEmail;
                        }
                        else
                        {

                        }
                        core.SaveChanges();
                    }
                    else
                    {
                        var start_date = Convert.ToDateTime(Info.StartDate);
                        var end_date = Convert.ToDateTime(Info.EndDate);
                        findItem.HumanSettlementApplicationId = application.Id;
                        findItem.ApplicantFullName = application.ApplicantFullName;
                        findItem.IdentityNumber = application.IDNo;
                        findItem.TheUnit = "Unit no. " + Info.SpaceUnitNo + " " + application.PreferredComplexArea.Name + ", " + region.Name;
                        findItem.CommenceDate = start_date.Day.ToString("00") + " " + GetMonthName(start_date.Month) + " " + start_date.Year;
                        findItem.ExpireryDate = end_date.Day.ToString("00") + " " + GetMonthName(end_date.Month) + " " + end_date.Year;
                        findItem.PayableAmount = Convert.ToDouble(Info.RentalAmount);
                        if (application.NominateSpouse)
                        {
                            findItem.Nominated = true;
                            findItem.NoTitle = application.SecAppTitleType.Name;
                            findItem.NoName = application.SecAppFirstName;
                            findItem.NoSurname = application.SecAppLastName;
                            findItem.NoIdentityNumber = application.SecAppIDNo;
                            findItem.NoRelationship = "Spouse";
                            findItem.NoContact = "(C) " + application.SecAppCellNo + " | (E) " + application.SecAppEmail;
                        }
                        else
                        {

                        }

                        core.SaveChanges();
                    }
                    core.SaveChanges();
                }
            }
            catch (Exception Io)
            {
                EventLogHelper.LogSystemError(Io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                //throw;
            }

           

        }
        public static string GetMonthName(int month)
        {
            #region 
            var name = string.Empty;
            switch (month)
            {
                case 1:
                    name = "January";
                    break;
                case 2:
                    name = "February";
                    break;
                case 3:
                    name = "March";
                    break;
                case 4:
                    name = "April";
                    break;
                case 5:
                    name = "May";
                    break;
                case 6:
                    name = "June";
                    break;
                case 7:
                    name = "July";
                    break;
                case 8:
                    name = "August";
                    break;
                case 9:
                    name = "September";
                    break;
                case 10:
                    name = "October";
                    break;
                case 11:
                    name = "November";
                    break;
                case 12:
                    name = "December";
                    break;
            }
            return name;
            #endregion
        }
        public static void HumanApplicantsSignature(eServicesDbContext core, int ApplicayionId)
        {
            var Application = core.HumanSettlementApplications.FirstOrDefault(r => r.Id == ApplicayionId);
            var Agreement = core.HSLeaseAgreementMasters.FirstOrDefault(x => x.HumanSettlementApplicationId == Application.Id);
            Agreement.ApplicantSignature = string.Format("{0}{1} {2}", Application.LastName.Substring(0, 1), Application.FirstName.Substring(0, 1), Application.IDNo);
            Agreement.Applicant_sign_date = string.Format("{0} {1} {2}", DateTime.Now.Day.ToString("00"), GetMonthName(DateTime.Now.Month), DateTime.Now.Year);
            if (Application.NominateSpouse) Agreement.NomineeSignature = string.Format("{0}{1} {2}", Application.SecAppLastName.Substring(0, 1), Application.SecAppFirstName.Substring(0, 1), Application.SecAppIDNo);
            core.SaveChanges();
        }
        public static void HumanRegionalmanagersSignature(eServicesDbContext core, int ApplicationId, int UserId)
        {
            var RoundRobbinWorks = core.RoundRobinQueues.Include(r=>r.Clerk.SystemUser).Include(r=>r.ResponsibilityType).Include(r=>r.Status).Where(c => c.HumanSettlementApplicationId == ApplicationId).ToList();
            var Application = core.HumanSettlementApplications.FirstOrDefault(r => r.Id == ApplicationId);
            var Agreement = core.HSLeaseAgreementMasters.FirstOrDefault(x => x.HumanSettlementApplicationId == Application.Id);
            var User = core.SystemUsers.FirstOrDefault(r => r.Id == UserId);
            Agreement.Lessor_sign_date = string.Format("{0} {1} {2}", DateTime.Now.Day.ToString("00"), GetMonthName(DateTime.Now.Month), DateTime.Now.Year);
            Agreement.LessorSignature = string.Format("{0}{1} {2}", User.LastName.Substring(0, 1), User.FirstName.Substring(0, 1), User.ServiceNo);

            var Witness_1 = RoundRobbinWorks.OrderByDescending(d => d.Id).FirstOrDefault(x => x.ResponsibilityType.Key == ResponsibilityTypeKeys.GenerateLeaseAgreement).Clerk.SystemUser;
            var Witness_2 = RoundRobbinWorks.OrderByDescending(d => d.Id).FirstOrDefault(x => x.ResponsibilityType.Key == ResponsibilityTypeKeys.AgreementReview).Clerk.SystemUser;
            Agreement.rr_Witness_one = string.Format("{0} {1}, {2}", Witness_1.FirstName, Witness_1.LastName, Witness_1.ServiceNo);
            Agreement.rr_Witness_two = string.Format("{0} {1}, {2}", Witness_2.FirstName, Witness_2.LastName, Witness_2.ServiceNo);
            core.SaveChanges();
        }
        public static void ApplicantSignLeaseAgreement(eServicesDbContext core, PropertyLeaseApplication property, LeaseDetails lease, ApplicantJoint joint)
        {
            var findItem = core.propertyLeaseAgreementMasters.FirstOrDefault(x => x.PropertyLeaseApplicationId == property.Id && x.LeaseDetailsId == lease.Id);
            DateTime Today = DateTime.Now.Date;
            findItem.TenantSignDate = Today.ToString().Substring(0, 7);
            findItem.TenantSignDay = Today.ToString().Substring(8, 2);
            findItem.TenantSigned = true;
            findItem.TenantSignature = property.FirstName.Substring(0, 1) + property.LastName.Substring(0, 1) + " " + property.IDNo;
            findItem.MainLesseeSigned = true;
            findItem.SignatureMainLessee = property.FirstName.Substring(0, 1) + property.LastName.Substring(0, 1) + " " + property.IDNo;
            if (property.SecondApplicant == true)
            {
                findItem.SpouseSigned = true;
                findItem.SignatureOfSpouse = property.SecAppFirstName.Substring(0, 1) + property.SecAppLastName.Substring(0, 1) + " " + property.SecAppIDNo;

            }
            else if (joint != null)
            {
                findItem.SpouseSigned = true;
                findItem.SignatureOfSpouse = joint.JointFirstName.Substring(0, 1) + joint.JointLastName.Substring(0, 1) + " " + joint.IDNo;
            }
            core.SaveChanges();
        }
        public static void RevenueManagerSignLeaseAgreement(eServicesDbContext core, PropertyLeaseApplication property, LeaseDetails lease, Customer User)
        {
            var findItem = core.propertyLeaseAgreementMasters.FirstOrDefault(x => x.PropertyLeaseApplicationId == property.Id && x.LeaseDetailsId == lease.Id);
            DateTime Today = DateTime.Now.Date;//2020/02/02 04/55/5251
            if (findItem.PropertyManagerSigned == true)
            {
                findItem.ManagersSignDate = Today.ToString().Substring(0, 7);
                findItem.ManagersSignDay = Today.ToString().Substring(8, 2);
                ChangeApplicationStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.IncentivePolicyApplicationPending).Id, (int)property.Id);

                lease.Completed = true;
                core.SaveChanges();
                //customer email awaiting debit order
            }

            var ResponsibilityTypeId = core.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseAgreementValidation).FirstOrDefault();
            RoundRobinMarkJobAsFinished(core, (int)property.Id, null, ResponsibilityTypeId.Id, User.Id);

            findItem.RevenueManagerSigned = true;
            findItem.RevenueManagerId = User.Id;
            findItem.RevenueManagersSignature = User.FirstName.Substring(0, 1) + User.LastName.Substring(0, 1) + " " + User.SystemUser.ServiceNo;
            core.SaveChanges();
        }
        public static void PropertyManagerSignLeaseAgreement(eServicesDbContext core, PropertyLeaseApplication property, LeaseDetails lease, Customer User)
        {
            var findItem = core.propertyLeaseAgreementMasters.FirstOrDefault(x => x.PropertyLeaseApplicationId == property.Id && x.LeaseDetailsId == lease.Id);
            DateTime Today = DateTime.Now.Date;
            if (findItem.RevenueManagerSigned == true)
            {
                findItem.ManagersSignDate = Today.ToString().Substring(0, 7);
                findItem.ManagersSignDay = Today.ToString().Substring(8, 2);
                ChangeApplicationStatus(core, core.Status.FirstOrDefault(x => x.Key == StatusKeys.IncentivePolicyApplicationPending).Id, (int)property.Id);

                lease.Completed = true;
                core.SaveChanges();
                //customer email awaiting debit order
            }

            var ResponsibilityTypeId = core.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.LeaseAgreementValidation).FirstOrDefault();
            RoundRobinMarkJobAsFinished(core, (int)property.Id, null, ResponsibilityTypeId.Id, User.Id);

            findItem.PropertyManagerSigned = true;
            findItem.PropertyManagerId = User.Id;
            findItem.PropertyManagersSignature = User.FirstName.Substring(0, 1) + User.LastName.Substring(0, 1) + " " + User.SystemUser.ServiceNo;
            core.SaveChanges();
        }
        public static void MarkAplicationAsDeleted(eServicesDbContext core, int Id)
        {
            var findItem = core.ApplicantUnits.FirstOrDefault(x => x.Id == Id);
            findItem.IsActive = false;
            findItem.IsDeleted = true;
            core.SaveChanges();
        }
        public static void MarkUnitAsInspection(eServicesDbContext core, int Id)
        {
            var findItem = core.Units.FirstOrDefault(x => x.Id == Id);
            findItem.Inspection = true;
            core.SaveChanges();
        }
        public static Customer GetBackOfficeId(eServicesDbContext core, int Id, bool LF)
        {
            Customer UserId = new Customer();
            ApplicantUnit AppUnit = core.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == Id);
            if (AppUnit != null)
            {
                MatchedUnits Match = AppUnit != null ? core.MatchedUnits.FirstOrDefault(x => x.Id == AppUnit.MatchedID) : null;
                if (Match != null)
                {
                    ApplicationAllocatedProperty allocatedUnit = core.ApplicationAllocatedProperty.FirstOrDefault(a => a.Id == Match.ApplicationAllocatedPropertyId);
                    PreferredComplexArea preferredComplexArea = allocatedUnit != null ? core.PreferredComplexAreas.FirstOrDefault(x => x.Id == allocatedUnit.OfferedComplexId) : null;

                    if (preferredComplexArea != null)
                        _ = (LF && preferredComplexArea != null) ?
                            UserId = core.Customers.FirstOrDefault(x => x.Id == preferredComplexArea.LettingOfficerId)
                            : UserId = core.Customers.FirstOrDefault(x => x.Id == preferredComplexArea.HousingSuperId);
                }
            }
            return UserId;
        }
        public static void ServeNoticeAppllicationDate(eServicesDbContext core, int Id, DateTime Date)
        {
            var findItem = core.PropertyLeaseApplications.FirstOrDefault(x => x.Id == Id);
            findItem.ServeNoticeDate = Date;
            core.SaveChanges();
        }
        public static void HumanServeNoticeDate(eServicesDbContext core, int Id, DateTime Date)
        {
            var findItem = core.HumanSettlementApplications.FirstOrDefault(x => x.Id == Id);
            findItem.ServeNoticeDate = Date;
            core.SaveChanges();
        }
        public static void RoundRobinMarkJobAsFinished(eServicesDbContext core, int? PropertyId, int? LeaseId, int ResposibilityId, int ClerkId)
        {
            try
            {
                if (PropertyId != null)
                {
                    var findItem = core.RoundRobinQueues.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == PropertyId && x.ResponsibilityTypeId == ResposibilityId && x.ClerkId == ClerkId/* && x.EndTaskDateTime == null*/);
                    findItem.StatusId = core.Status.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                    findItem.EndTaskDateTime = DateTime.Now;
                    core.SaveChanges();
                }

                if (LeaseId != null)
                {
                    var findItem = core.RoundRobinQueues.OrderByDescending(x => x.Id).FirstOrDefault(x => x.LeaseDetailsId == LeaseId && x.ResponsibilityTypeId == ResposibilityId && x.ClerkId == ClerkId && x.EndTaskDateTime == null);
                    findItem.StatusId = core.Status.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                    findItem.EndTaskDateTime = DateTime.Now;
                    core.SaveChanges();
                }
            }
            catch (Exception)
            {

            }

        }
        public static void MarkUnitAsInpected(eServicesDbContext core, int Id)
        {
            var findItem = core.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == Id);
            findItem.Inspection = false;
            core.SaveChanges();
        }
        public static bool ModifyComplexAvailability(eServicesDbContext core, int Id, bool val)
        {
            var findItem = core.PreferredComplexAreas.FirstOrDefault(x => x.Id == Id);
            findItem.IsActive = val;
            core.SaveChanges();
            return true;
        }
        public static void MarkLeaseInfoAsCompleted(eServicesDbContext core)
        {
            var comp = core.propertyLeaseAgreementMasters.Select(x => new { x.PropertyLeaseApplicationId, x.LeaseDetailsId }).ToList();
            var list = core.LeaseDetails.ToList();
            var count = 0;
            foreach (var Item in comp)
            {
                var findItem = list.FirstOrDefault(x => x.Id == Item.LeaseDetailsId && x.PropertyLeaseApplicationId == Item.PropertyLeaseApplicationId);
                findItem.Completed = true;
                count++;
            }
            //core.SaveChanges();

        }
        public static bool ReallocateCaseToNewUser(eServicesDbContext core, RoundRobinQueue queue, int ClerkId)
        {
            try
            {
                var submited = core.Status.FirstOrDefault(x => x.Key == StatusKeys.Submitted).Id;
                var Queue = new RoundRobinQueue()
                {
                    ClerkId = ClerkId,
                    StatusId = submited,
                    CurrentTaskDateTime = DateTime.Now,
                    PropertyLeaseApplicationId = queue.PropertyLeaseApplicationId,
                    LeaseDetailsId = queue.LeaseDetailsId == null ? null : queue.LeaseDetailsId,
                    AssignedFrom = queue.Id,
                    ResponsibilityTypeId = queue.ResponsibilityTypeId
                };
                core.RoundRobinQueues.Add(Queue);
                core.SaveChanges();

                PropertyLeaseApplicationController cc = new PropertyLeaseApplicationController();
                cc.BackOfficeNotification((int)queue.PropertyLeaseApplicationId, ClerkId, queue.ResponsibilityType.Name);
                return true;
            }
            catch (Exception IO)
            {
                return false;
            }
        }
        public static bool ReallocateCaseToNewUserHsd(eServicesDbContext core, RoundRobinQueue queue, int ClerkId)
        {
            try
            {
                var submited = core.Status.FirstOrDefault(x => x.Key == StatusKeys.Submitted).Id;
                var Queue = new RoundRobinQueue()
                {
                    ClerkId = ClerkId,
                    StatusId = submited,
                    CurrentTaskDateTime = DateTime.Now,
                    HumanSettlementApplicationId = queue.HumanSettlementApplicationId,
                    LeaseDetailsId = queue.LeaseDetailsId == null ? null : queue.LeaseDetailsId,
                    AssignedFrom = queue.Id,
                    ResponsibilityTypeId = queue.ResponsibilityTypeId
                };
                core.RoundRobinQueues.Add(Queue);
                core.SaveChanges();
                return true;
            }
            catch (Exception IO)
            {
                return false;
            }
        }
        public static bool ValidateActiveApplication(eServicesDbContext core, int CustomerId)
        {
            var customer = core.Customers.FirstOrDefault(r => r.Id == CustomerId);
            var application = core.PropertyLeaseApplications.Include(o => o.Status).FirstOrDefault(r => r.CustomerId == customer.Id) ?? null;
            var rrq = core.LeaseDetails.Include(o => o.Status).FirstOrDefault(c => c.PropertyLeaseApplicationId == application.Id) ?? null;

            if (application != null && rrq != null && rrq.Status.Key == StatusKeys.TerminatedLease) return false;
            if (application != null && application.Status.Key == StatusKeys.CreditScoreRejected) return false;
            return true;
        }
        public static void UpdateApplicationMaster(eServicesDbContext core, int ApplicationId, int MasterId, HumanSettlementApplication application, HumanSettlementLeaseMaster human)
        {
            var Application = core.HumanSettlementApplications.FirstOrDefault(r => r.Id == ApplicationId);
            var Human = core.HumanSettlementLeaseMasters.FirstOrDefault(r => r.Id == MasterId);
            Application = application;
            human = Human;
            core.SaveChanges();
        }
        public static void UpdateApplication(eServicesDbContext core, HumanSettlementApplication rr, int Id)
        {
            var findItem = core.HumanSettlementApplications.FirstOrDefault(r => r.Id == Id);
            findItem = rr;
            core.SaveChanges();
        }
        public static void HumanOccupantSave(eServicesDbContext core, HSUnitOccupant rr)
        {
            core.HSUnitOccupants.Add(rr);
            core.SaveChanges();
        }
        public static void OneTimePinGenerate(eServicesDbContext core, int Id)
        {
            var findItem = core.HumanSettlementApplications.FirstOrDefault(r => r.Id == Id);

            Random rnd = new Random();
            int rnd00 = rnd.Next(0, 10);
            int rnd01 = rnd.Next(0, 10);
            int rnd02 = rnd.Next(0, 10);
            int rnd03 = rnd.Next(0, 10);
            //int rnd00 = rnd.Next(52);     // creates a number between 0 and 51
            var otp = string.Format("{0}{1}{2}{3}", rnd00, rnd01, rnd02, rnd03);
             //otp = "4245";
            var rrr = core.OneTimePins.FirstOrDefault(r => !r.IsVerified && !r.IsAbandoned && r.OTP == otp)?.OTP;

            while (!string.IsNullOrEmpty(rrr) && otp == rrr)
            {
                rnd00 = rnd.Next(0, 10);
                rnd01 = rnd.Next(0, 10);
                rnd02 = rnd.Next(0, 10);
                rnd03 = rnd.Next(0, 10);
                otp = string.Format("{0}{1}{2}{3}", rnd00, rnd01, rnd02, rnd03);
            }

            core.OneTimePins.Add(new OneTimePin { OTP = otp, IsAbandoned = false, IsVerified = false });
            core.SaveChanges();

            findItem.OTP = otp;
            core.SaveChanges();
        }
        public static void RenewalExtentionOfMonths(eServicesDbContext core, bool IsMaster, int Id)
        {
            var months = 24;
            switch (IsMaster)
            {
                case true:
                    var _master = core.HumanSettlementLeaseMasters.Include(d=>d.HumanSettlementApplication.UnitCategory).Where(r => r.Id == Id).FirstOrDefault();
                    if (_master.HumanSettlementApplication.UnitCategory?.Key == UnitCategoryKeys.Dormitory) months = 12;
                    _master.EndDate = Convert.ToDateTime(_master.EndDate).AddMonths(months);
                    _master.RenewalNotice = Convert.ToDateTime(_master.RenewalNotice).AddMonths(months);
                    _master.TerminationNotice = Convert.ToDateTime(_master.TerminationNotice).AddMonths(months);
                    core.Entry(_master).State = EntityState.Modified;
                    core.SaveChanges();
                    break;
                case false:
                    var _details = core.HumanSettlementLeaseDetails.Include(d => d.HumanSettlementApplication.UnitCategory).Where(r => r.Id == Id).FirstOrDefault();
                    if (_details.HumanSettlementApplication.UnitCategory?.Key == UnitCategoryKeys.Dormitory) months = 12;
                    _details.EndDate = Convert.ToDateTime(_details.EndDate).AddMonths(months);
                    _details.RenewalNotice = Convert.ToDateTime(_details.RenewalNotice).AddMonths(months);
                    _details.TerminationNotice = Convert.ToDateTime(_details.TerminationNotice).AddMonths(months);
                    core.Entry(_details).State = EntityState.Modified;
                    core.SaveChanges();
                    break;
            }
        }
        public static void UpdateteAgreementTerminationDates(eServicesDbContext core, int Id)
        {
            var findItem = core.HumanSettlementApplications.FirstOrDefault(r => r.Id == Id);
            switch (findItem.IsMaster)
            {
                case true:
                    var master = core.HumanSettlementLeaseMasters.FirstOrDefault(r => r.HumanSettlementApplicationId == findItem.Id);
                    master.EndDate = findItem.ServeNoticeDate ?? master.NoticeDate;
                    break;
                case false:
                    var details = core.HumanSettlementLeaseMasters.FirstOrDefault(r => r.HumanSettlementApplicationId == findItem.Id);
                    details.EndDate = findItem.ServeNoticeDate ?? details.NoticeDate;
                    break;
            }
            core.SaveChanges();

        }
        public static void addUsersToWorkAllocation(eServicesDbContext core)
        {
            var findItem = core.UserWorkAllocations.OrderBy(d=>d.Id).Where(g => g.PreferredComplexAreaId == 108).ToList();
            var rrq = core.PreferredComplexAreas.Where(d => d.Id > 108).ToList();
            //foreach(var user in findItem)
            //{
            //    var recrd = user;
            //    foreach(var cc in rrq)
            //    {
            //        var rec = new UserWorkAllocation
            //        {
            //            Roles = user.Roles,
            //            SystemUserId = user.SystemUserId,
            //            PreferredComplexAreaId = cc.Id,
            //            RRActive = user.RRActive,
            //            IsActive =true,
            //            IsDeleted =false,
            //            IsLocked =false,
            //            CreatedDateTime = DateTime.Now,
            //            ModifiedDateTime = DateTime.Now,
            //        };
            //        core.UserWorkAllocations.Add(rec);
            //        core.SaveChanges();
            //    }
            //}
        }
        public static DocumentsViewModel DocumentRenewalLetter(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var RenewalLetter = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.RenewalLetter);

            //Add to checklists
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == RenewalLetter.Id && dcl.ReferenceTypeId == referenceTypeId));

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentWarningLetter(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var WarningLetter = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.WarningLetter);

            //Add to checklists
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == WarningLetter.Id && dcl.ReferenceTypeId == referenceTypeId));

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;

            var checkListIds = documentCheckLists.Select(x => x.Id).ToList();

            dvm.Documents = core.Documents.Include(o => o.File).Where(o => checkListIds.Contains(o.DocumentCheckListId) && o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).OrderByDescending(p=>p.CreatedDateTime).ToList();

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static int WarningLetterCount(eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, int documentCheckListId)
        {
            var docCount = core.Documents.Where(o => o.DocumentCheckListId == documentCheckListId && o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).Count();
            return docCount;
        }
        public static DocumentsViewModel DocumentTerminationLetterAndProofBanking(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload, bool isCustomerNotice)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();
             
            //required documents

            var TerminationLetter = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.TerminationLetter);
            var ProofOfBankingDetails = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfBankingDetails);

            //Add to checklists
            if (!isCustomerNotice) 
            {
                documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == TerminationLetter.Id && dcl.ReferenceTypeId == referenceType.Id));
            }
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfBankingDetails.Id && dcl.ReferenceTypeId == referenceType.Id));
            var checkListIds = documentCheckLists.Select(x => x.Id).ToList();

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => checkListIds.Contains(o.DocumentCheckListId) && o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentExitInterviewForm(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents

            var ExitInterviewForm = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ExitInterviewForm);

            //Add to checklists
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ExitInterviewForm.Id && dcl.ReferenceTypeId == referenceType.Id));


            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;
            dvm.Documents = core.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentUploadBakingDetailsProof(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            var ProofOfBankingDetails = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ProofOfBankingDetails);
            
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfBankingDetails.Id && dcl.ReferenceTypeId == referenceType.Id));
            
            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;

            var checkListIds = documentCheckLists.Select(x => x.Id).ToList();

            dvm.Documents = core.Documents.Include(o => o.File).Where(o => checkListIds.Contains(o.DocumentCheckListId) && o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }
        public static DocumentsViewModel DocumentDepositRefunds(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            var IdentityDocument = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdentityDocument);
            var CompleteUnitInspection = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CompleteUnitInspection);
            var ExitInterviewForm = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ExitInterviewForm);
            var ApplicationLeaseAgreementEHC = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ApplicationLeaseAgreementEHC);
            var WarningLetter = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.WarningLetter);
            var TerminationLetter = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.TerminationLetter);

            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == IdentityDocument.Id && dcl.ReferenceTypeId == referenceTypeId));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ExitInterviewForm.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == CompleteUnitInspection.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ApplicationLeaseAgreementEHC.Id && dcl.ReferenceTypeId == referenceType.Id));
            
            //Termination Letter - Cancellatrion letter
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == TerminationLetter.Id && dcl.ReferenceTypeId == referenceType.Id));
            
            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;

            var checkListIds = documentCheckLists.Select(x => x.Id).ToList();

            dvm.Documents = core.Documents.Include(o => o.File).Where(o => checkListIds.Contains(o.DocumentCheckListId) && o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).ToList();

            //Warning Letter - Contravention letter
            var warningLetterCheckList = core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == WarningLetter.Id && dcl.ReferenceTypeId == referenceType.Id);
            documentCheckLists.Add(warningLetterCheckList);
            var warningLetterDoc = core.Documents.Include(o => o.File).Where(o => o.DocumentCheckListId == warningLetterCheckList.Id && o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).OrderByDescending(p=>p.CreatedDateTime).FirstOrDefault();
            if (warningLetterDoc != null) 
            {
                 dvm.Documents.Add(warningLetterDoc);
            }
            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }

        public static DocumentsViewModel DocumentsPlmManualApplications(DocumentsViewModel dvm, eServicesDbContext core, int? referenceId, int? customerId, int? referenceTypeId, int? applicationId, string returnUrl, int rcsappId, bool IsUpload)
        {
            var referenceType = core.ReferenceTypes.Find(referenceTypeId);
            var application = core.Applications.Find(applicationId);

            if (application == null) throw new Exception("Invalid application.");
            if (referenceType == null) throw new Exception("Invalid reference type.");

            var documentCheckLists = new List<DocumentCheckList>();

            //required documents
            var CompleteUnitInspection = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.CompleteUnitInspection);
            var ApplicationLeaseAgreementEHC = core.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.ApplicationLeaseAgreementEHC);

            //Add to checklists
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == CompleteUnitInspection.Id && dcl.ReferenceTypeId == referenceType.Id));
            documentCheckLists.Add(core.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ApplicationLeaseAgreementEHC.Id && dcl.ReferenceTypeId == referenceType.Id));

            dvm.ReturnUrl = returnUrl;
            dvm.CustomerId = customerId ?? referenceId;
            dvm.ApplicationId = (int)applicationId;
            dvm.Application = application;
            dvm.PropertyLeaseApplicationId = rcsappId;
            dvm.RcsApplicationId = rcsappId;
            dvm.ReferenceTypeId = (int)referenceTypeId;
            dvm.ReferenceType = referenceType;
            dvm.ReferenceId = (int)referenceId;
            dvm.IsUploadView = IsUpload;

            var checkListIds = documentCheckLists.Select(x => x.Id).ToList();

            dvm.Documents = core.Documents.Include(o => o.File).Where(o => checkListIds.Contains(o.DocumentCheckListId) && o.ReferenceId == referenceId && o.ReferenceTypeId == referenceTypeId && o.PropertyLeaseApplicationId == rcsappId && o.IsActive && !o.IsDeleted).OrderByDescending(p => p.CreatedDateTime).ToList();

            dvm.DocumentCheckLists = documentCheckLists;

            foreach (var customerDocument in dvm.Documents)
            {
                if (customerDocument.File != null)
                    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                var docCheckList =
                     core.DocumentCheckLists.Include(c => c.DocumentType)
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

            return dvm;
        }

        public static void UpdatePasswordForRefundProcess(eServicesDbContext core, string password, int appId)
        {

            var findApplication = core.PropertyLeaseApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == appId) ?? null;
            findApplication.RefundProcessPassword = password;
            core.SaveChanges();
        }
        public static void SaveUnitHistory(eServicesDbContext core, Int32 AppId, String State, Int32 UnitId, Int32 CustomerId)
        {
            //core.AllocatedUnitHistory.Add(new AllocatedUnitHistory
            //{
            //    //ApplicationAllocatedPropertyId = UnitId,
            //    State = State,
            //    PropertyLeaseApplicationId = AppId,
            //    ByLettingOfficerId = CustomerId
            //});
            core.SaveChanges();
        }
        public static void UpdateIsMigratedPLMApplication(eServicesDbContext core, int PropertyLeaseId)
        {
            var findApplication = core.PropertyLeaseApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == PropertyLeaseId) ?? null;
            findApplication.IsMigrated = true;
            core.SaveChanges();
        }
        public static void UpdateFullyMigratedPLMApplication(eServicesDbContext core,int PropertyLeaseId)
        {
            var findApplication = core.PropertyLeaseApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == PropertyLeaseId) ?? null;
            findApplication.IsFullyMigrated = true;
            core.SaveChanges();
        }
    }
}