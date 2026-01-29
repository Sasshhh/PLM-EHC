using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.Ajax.Utilities;

namespace C8.eServices.Mvc.Helpers
{
    public static class LinkedAccountHelper
    {
        //static eServicesDbContext _context = new eServicesDbContext();
        #region PopulateCustomerLinkedModel
        public static CustomerLinkedAccountViewModel PopulateCustomerLinkedModel(LinkedAccount model,
          List<string> emailList, List<string> mobileList)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var vm = new CustomerLinkedAccountViewModel();
                    var entity = new Entity();
                    var agent = new Agent();
                    if (model != null)
                    {
                        var customer = model.Customer;

                        if (customer == null) throw new Exception("Invalid customer");
                        var custType = context.CustomerTypes.SingleOrDefault(c => c.Id == customer.CustomerTypeId);
                        if (custType == null) throw new Exception("Invalid customer type");

                        if (custType.Key == CustomerTypeKeys.Entity || custType.Key == CustomerTypeKeys.ManagingAgent)
                        {
                            entity = context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id);

                            if (null == entity)
                            {
                                //Check if customer is managing Agent
                                var _agent = context.Agents.FirstOrDefault(a => a.CustomerId == customer.Id);
                                if (_agent != null)
                                {
                                    var entityAgent = context.EntityAgents.Include(e => e.Entity)
                                        .FirstOrDefault(a => a.AgentId == _agent.Id);
                                    if (entityAgent != null)
                                        entity =
                                            entityAgent
                                                .Entity;
                                }
                            }
                        }

                        //Checks if linked account created by managing Agent
                        var agentCustomer = context.AgentCustomers.FirstOrDefault(a => a.CustomerId == customer.Id);
                        if (agentCustomer != null)
                        {
                            agent = context.Agents.FirstOrDefault(a => a.Id == agentCustomer.AgentId);
                        }
                        vm.Customer = customer;
                        vm.LinkedAccount = model;
                        vm.FirstName = customer.FirstName;
                        vm.LastName = customer.LastName;
                        vm.AccountNumber = context.Accounts.Find(model.AccountId).AccountNumber;
                        vm.Status = context.Status.Find(model.StatusId);
                        vm.Entity = entity;
                        vm.Agent = agent;
                        //vm.EmailBill = model.EmailBill;
                        vm.CustomerId = customer.Id;
                        vm.CustomerEmail = model.EmailAddress;
                        vm.Id = model.Id;
                        vm.CreatedDateTime = Convert.ToDateTime(model.CreatedDateTime);
                        vm.RecipientType = model.RecipientType.Key;
                        vm.LinkedAccountType = model.LinkedAccountType.Name;

                        if (model.RecipientType.Key == RecepientTypeKeys.ToPrimaryEmail)
                        {
                            var linkedEmails = context.LinkedEmails.Where(le => le.LinkedAccountId == model.Id && le.IsActive
                                               && le.IsDeleted == false);

                            foreach (var mail in linkedEmails)
                            {
                                emailList.Add(mail.EmailAddress);
                            }
                        }

                        if (model.RecipientType.Key == RecepientTypeKeys.Mobile)
                        {
                            var linkedMobiles = context.LinkedMobiles.Where(le => le.LinkedAccountId == model.Id && le.IsActive
                                               && le.IsDeleted == false);

                            foreach (var mail in linkedMobiles)
                            {
                                mobileList.Add(mail.MobileNumber);
                            }
                        }

                        var notes = context.Notes.Where(n => n.ReferenceId == model.Id).OrderByDescending(n => n.CreatedDateTime).ToList();
                        vm.Notes = notes;
                    }

                    return vm;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region Populate Documents - Controller, Linked Account
        public static void PopulateDocuments(this ControllerBase controller, LinkedAccount la)
        {
            using (var context = new eServicesDbContext())
            {
                var documentRefType = (la.Customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent || la.Customer.CustomerType.Key == CustomerTypeKeys.Entity)
                ? context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccountsManagingAgent) :
                context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccounts);
                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));

                if (documentRefType == null) throw new Exception("Invalid reference type");
                if (application == null) throw new Exception("Invalid application");

                var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                                        .Include(d => d.ReferenceType)
                                        .Where(dc => dc.ApplicationId == application.Id &&
                                        dc.ReferenceTypeId == documentRefType.Id);
                var uploadedDocuments = context.Documents.Include(d => d.File)
                .Where(d => d.CustomerId == la.CustomerId && d.IsActive
                && d.ReferenceId == la.Id
                && d.IsDeleted == false && d.ReferenceTypeId == documentRefType.Id);

                foreach (var doc in uploadedDocuments)
                {
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                }

                var dvm = new DocumentsViewModel
                {
                    ApplicationId = application.Id,
                    CustomerId = la.CustomerId,
                    Documents = uploadedDocuments.ToList(),
                    IsUploadView = false,
                    DocumentCheckLists = documentCheckLists.ToList(),

                };

                controller.ViewData["UploadList"] = uploadedDocuments;
                controller.ViewData["DocumentsVm"] = dvm;

                controller.ViewBag.DocumentsVerified = uploadedDocuments.Any(c => (bool)c.IsLocked);
                controller.ViewBag.LinkedAccountId = la.Id;
                controller.ViewBag.ReferenceTypeId = documentRefType.Id;
                controller.ViewBag.ApplicationId = application.Id;
            }
        }
        #endregion

        #region Populate Documents - Linked Account ID
        public static void PopulateDocuments(int linkedAccountId)
        {
            using (var context = new eServicesDbContext())
            {
                var linkedAccount = context.LinkedAccounts.Include(l => l.Customer.CustomerType).Include(l => l.Status)
                                    .FirstOrDefault(l => l.Id == linkedAccountId);

                if (linkedAccount == null) throw new Exception("Linked Account not found for id " + linkedAccountId);

                if (linkedAccount.Status.Key != StatusKeys.LinkedAccountActive)
                {
                    var documentRefType = (linkedAccount.Customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent || linkedAccount.Customer.CustomerType.Key == CustomerTypeKeys.Entity)
                           ? context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccountsManagingAgent) :
                           context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccounts);
                    var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));

                    if (documentRefType != null && application != null)
                    {
                        var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                            .Include(d => d.ReferenceType)
                            .Where(dc => dc.ApplicationId == application.Id &&
                            dc.ReferenceTypeId == documentRefType.Id && dc.DocumentType.Key != DocumentTypeKeys.AdditionalDocuments)
                            .ToList().Select(dc => dc.Id);

                        var uploadedDocuments = context.Documents.Include(d => d.File)
                                                .Where(d => d.CustomerId == linkedAccount.CustomerId && d.IsActive
                                                && d.ReferenceId == linkedAccount.Id
                                                && d.IsDeleted == false && d.ReferenceTypeId == documentRefType.Id)
                                                .ToList().Select(d => d.DocumentCheckListId);

                        var outstandingDocuments = documentCheckLists.Except(uploadedDocuments);

                        var pendingApproval = context.Status.FirstOrDefault(s => s.Key == StatusKeys.LinkedAccountPending);
                        var pendingDocuments = context.Status.FirstOrDefault(s => s.Key == StatusKeys.DocumentPending);

                        if (pendingApproval != null && pendingDocuments != null && linkedAccount.Status.Key != StatusKeys.LinkedAccountActive)
                        {
                            linkedAccount.StatusId = (outstandingDocuments.Any()) ? pendingDocuments.Id : pendingApproval.Id;
                            context.Entry(linkedAccount).State = EntityState.Modified;
                            context.SaveChanges();
                        }
                    }
                }
            }
        }
        #endregion

        #region LinkedAccount Notification for email and alerts
        public static string LinkedAccountNotification(bool isEmailNotification)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var appSettingMessage = (isEmailNotification)
                        ? context.AppSettings
                            .FirstOrDefault(
                                a => a.Key == AppSettingKeys.LinkedAccountsPendingAccountApprovalAndEmailActivationMessage)
                        : context.AppSettings.FirstOrDefault(
                            a => a.Key == AppSettingKeys.LinkedAccountsPendingAccountApprovalMessage);

                    if (appSettingMessage == null) throw new Exception("Invalid appsetting");

                    return appSettingMessage.Value;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region LinkedEmail Notification for email and alerts
        public static string LinkedEmailNotification(Status linkedAccountStatus)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var appSettingMessage = (linkedAccountStatus.Key != StatusKeys.LinkedAccountActive)
                        ? context.AppSettings.FirstOrDefault(
                            a => a.Key == AppSettingKeys.LinkedEmailsPendingEmailActivationAndLinkedAccountApprovalMessage)
                        : context.AppSettings.FirstOrDefault(
                            a => a.Key == AppSettingKeys.LinkedEmailsPendingEmailActivationMessage);

                    if (appSettingMessage == null) throw new Exception("Invalid appsetting");

                    return appSettingMessage.Value;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

        }
        #endregion
    }
}