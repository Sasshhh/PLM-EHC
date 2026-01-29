using System;
using System.Data.Entity;
using System.Linq;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Helpers
{
    public static class CustomerHelper
    {
        #region Populate Documents For Customer Profile
        public static void PopulateDocuments(int customerId, int? rcsappID)
        {
            using (var context = new eServicesDbContext())
            {
                var customer = context.Customers.Include(c => c.CustomerType).Include(c => c.Status)
                                    .FirstOrDefault(c => c.Id == customerId);

                if (customer == null) throw new Exception("Customer not found");

                if (customer.Status.Key != StatusKeys.CustomerActive && customer.CustomerType.Key != CustomerTypeKeys.Individual)
                {
                    var documentRefType = context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.CustomerProfile);
                    var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));

                    if (documentRefType == null) throw new Exception("Document reference type not found");
                    if (application == null) throw new Exception("Application not found");

                    var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                                            .Include(d => d.ReferenceType)
                                            .Where(dc => dc.ApplicationId == application.Id &&
                                            dc.ReferenceTypeId == documentRefType.Id && dc.DocumentType.Key != DocumentTypeKeys.AdditionalDocuments)
                                            .ToList().Select(dc => dc.Id);

                    var uploadedDocuments = context.Documents.Include(d => d.File)
                                            .Where(d => d.CustomerId == customer.Id && d.IsActive
                                            && d.ReferenceId == customer.Id
                                            && d.IsDeleted == false && d.ReferenceTypeId == documentRefType.Id && d.RCSApplicationStatusId == rcsappID)
                                            .ToList().Select(d => d.DocumentCheckListId);

                    var outstandingDocuments = documentCheckLists.Except(uploadedDocuments);

                    var pendingApproval = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                    var pendingDocuments = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingDocuments);

                    if (pendingApproval == null) throw new Exception("Status not found");
                    if (pendingDocuments == null) throw new Exception("Status not found");

                    customer.StatusId = (outstandingDocuments.Any()) ? pendingDocuments.Id : pendingApproval.Id;
                    context.Entry(customer).State = EntityState.Modified;

                    if (customer.StatusId == pendingApproval.Id)
                        SendApprovalRequest(customer);

                    context.SaveChanges();
                }

                //if (customer.Status.Key == StatusKeys.CustomerActive)
                //{
                //    var documentRefType = context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.CustomerProfile);
                //    var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));

                //    if (documentRefType == null) throw new Exception("Document reference type not found");
                //    if (application == null) throw new Exception("Application not found");

                //    var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                //                            .Include(d => d.ReferenceType)
                //                            .Where(dc => dc.ApplicationId == application.Id &&
                //                            dc.ReferenceTypeId == documentRefType.Id && dc.DocumentType.Key != DocumentTypeKeys.AdditionalDocuments)
                //                            .ToList().Select(dc => dc.Id);

                //    var uploadedDocuments = context.Documents.Include(d => d.File)
                //                            .Where(d => d.CustomerId == customer.Id && d.IsActive
                //                            && d.ReferenceId == customer.Id
                //                            && d.IsDeleted == false && d.ReferenceTypeId == documentRefType.Id && d.RCSApplicationStatusId == rcsappID)
                //                            .ToList().Select(d => d.DocumentCheckListId);

                //    var outstandingDocuments = documentCheckLists.Except(uploadedDocuments);

                //    var pendingApproval = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                //    var pendingDocuments = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingDocuments);

                //    if (pendingApproval == null) throw new Exception("Status not found");
                //    if (pendingDocuments == null) throw new Exception("Status not found");

                //    customer.StatusId = (outstandingDocuments.Any()) ? pendingDocuments.Id : pendingApproval.Id;
                //    context.Entry(customer).State = EntityState.Modified;

                //    if (customer.StatusId == pendingApproval.Id)
                //        SendApprovalRequest(customer);

                //    context.SaveChanges();
                //}

            }
        }
        public static void PopulateRefundDocuments(int customerId, int? RefundAppID)
        {
            using (var context = new eServicesDbContext())
            {
                var customer = context.Customers.Include(c => c.CustomerType).Include(c => c.Status)
                                    .FirstOrDefault(c => c.Id == customerId);

                if (customer == null) throw new Exception("Customer not found");

                if (customer.Status.Key != StatusKeys.CustomerActive && customer.CustomerType.Key != CustomerTypeKeys.Individual)
                {
                    var documentRefType = context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.CustomerProfile);
                    var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));

                    if (documentRefType == null) throw new Exception("Document reference type not found");
                    if (application == null) throw new Exception("Application not found");

                    var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                                            .Include(d => d.ReferenceType)
                                            .Where(dc => dc.ApplicationId == application.Id &&
                                            dc.ReferenceTypeId == documentRefType.Id && dc.DocumentType.Key != DocumentTypeKeys.AdditionalDocuments)
                                            .ToList().Select(dc => dc.Id);

                    var uploadedDocuments = context.Documents.Include(d => d.File)
                                            .Where(d => d.CustomerId == customer.Id && d.IsActive
                                            && d.ReferenceId == customer.Id
                                            && d.IsDeleted == false && d.ReferenceTypeId == documentRefType.Id && d.RefundApplicationId == RefundAppID)
                                            .ToList().Select(d => d.DocumentCheckListId);

                    var outstandingDocuments = documentCheckLists.Except(uploadedDocuments);

                    var pendingApproval = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                    var pendingDocuments = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingDocuments);

                    if (pendingApproval == null) throw new Exception("Status not found");
                    if (pendingDocuments == null) throw new Exception("Status not found");

                    customer.StatusId = (outstandingDocuments.Any()) ? pendingDocuments.Id : pendingApproval.Id;
                    context.Entry(customer).State = EntityState.Modified;

                    if (customer.StatusId == pendingApproval.Id)
                        SendApprovalRequest(customer);

                    context.SaveChanges();
                }

                //if (customer.Status.Key == StatusKeys.CustomerActive)
                //{
                //    var documentRefType = context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.CustomerProfile);
                //    var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));

                //    if (documentRefType == null) throw new Exception("Document reference type not found");
                //    if (application == null) throw new Exception("Application not found");

                //    var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                //                            .Include(d => d.ReferenceType)
                //                            .Where(dc => dc.ApplicationId == application.Id &&
                //                            dc.ReferenceTypeId == documentRefType.Id && dc.DocumentType.Key != DocumentTypeKeys.AdditionalDocuments)
                //                            .ToList().Select(dc => dc.Id);

                //    var uploadedDocuments = context.Documents.Include(d => d.File)
                //                            .Where(d => d.CustomerId == customer.Id && d.IsActive
                //                            && d.ReferenceId == customer.Id
                //                            && d.IsDeleted == false && d.ReferenceTypeId == documentRefType.Id && d.RCSApplicationStatusId == rcsappID)
                //                            .ToList().Select(d => d.DocumentCheckListId);

                //    var outstandingDocuments = documentCheckLists.Except(uploadedDocuments);

                //    var pendingApproval = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                //    var pendingDocuments = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingDocuments);

                //    if (pendingApproval == null) throw new Exception("Status not found");
                //    if (pendingDocuments == null) throw new Exception("Status not found");

                //    customer.StatusId = (outstandingDocuments.Any()) ? pendingDocuments.Id : pendingApproval.Id;
                //    context.Entry(customer).State = EntityState.Modified;

                //    if (customer.StatusId == pendingApproval.Id)
                //        SendApprovalRequest(customer);

                //    context.SaveChanges();
                //}

            }
        }
        public static void PopulateDocuments(int customerId)
        {
            using (var context = new eServicesDbContext())
            {
                var customer = context.Customers.Include(c => c.CustomerType).Include(c => c.Status)
                                    .FirstOrDefault(c => c.Id == customerId);

                if (customer == null) throw new Exception("Customer not found");

                if (customer.Status.Key != StatusKeys.CustomerActive && customer.CustomerType.Key != CustomerTypeKeys.Individual)
                {
                    var documentRefType = context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.CustomerProfile);
                    var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));

                    if (documentRefType == null) throw new Exception("Document reference type not found");
                    if (application == null) throw new Exception("Application not found");

                    var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                                            .Include(d => d.ReferenceType)
                                            .Where(dc => dc.ApplicationId == application.Id &&
                                            dc.ReferenceTypeId == documentRefType.Id && dc.DocumentType.Key != DocumentTypeKeys.AdditionalDocuments)
                                            .ToList().Select(dc => dc.Id);

                    var uploadedDocuments = context.Documents.Include(d => d.File)
                                            .Where(d => d.CustomerId == customer.Id && d.IsActive
                                            && d.ReferenceId == customer.Id
                                            && d.IsDeleted == false && d.ReferenceTypeId == documentRefType.Id)
                                            .ToList().Select(d => d.DocumentCheckListId);

                    var outstandingDocuments = documentCheckLists.Except(uploadedDocuments);

                    var pendingApproval = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                    var pendingDocuments = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingDocuments);

                    if (pendingApproval == null) throw new Exception("Status not found");
                    if (pendingDocuments == null) throw new Exception("Status not found");

                    customer.StatusId = (outstandingDocuments.Any()) ? pendingDocuments.Id : pendingApproval.Id;
                    context.Entry(customer).State = EntityState.Modified;

                    if (customer.StatusId == pendingApproval.Id)
                        SendApprovalRequest(customer);

                    context.SaveChanges();
                }
            }
        }
        #endregion

        #region Profile Send Customer Profile Approval Request
        public static void SendApprovalRequest(Customer customer)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    //var ticket = new Query();
                    //var customerApprovalQueryType = context.QueryTypes.FirstOrDefault(qt => qt.Key == QueryTypeKeys.ProfileApproval);
                    //var ticketPendingStatus = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerQueryPending);

                    //if (customerApprovalQueryType == null) throw new Exception("Invalid query type");
                    //if (ticketPendingStatus == null) throw new Exception("Invalid status");

                    //var ticketComment =
                    //    $"Customer Identification : {customer.IdentificationNumber}" +
                    //    " requires profile approval. Please review uploaded documents.";

                    //ticket.Comment = ticketComment;
                    //ticket.StatusId = ticketPendingStatus.Id;
                    //ticket.QueryTypeId = customerApprovalQueryType.Id;
                    //ticket.Email = customer.EmailAddress;
                    //ticket.FullName = customer.FullName;
                    //ticket.ContactNumber = customer.HomePhoneNumber ?? customer.CellPhoneNumber;
                    //ticket.IsActive = true;
                    //ticket.IsDeleted = false;
                    //ticket.IsLocked = false;

                    //context.Queries.Add(ticket);
                    //context.SaveChanges();
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