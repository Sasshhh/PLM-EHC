using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
namespace C8.eServices.Mvc.Helpers
{
    public static class DocumentHelper
    {
        #region Get Document References 
        public static void ManageDocumentReferences(int linkedAccountId, string documentKey)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var linkedAccount = context.LinkedAccounts.FirstOrDefault(l => l.Id == linkedAccountId);
                    var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                    var profileReferenceType = context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.CustomerProfile);

                    if (linkedAccount == null) throw new Exception("Invalid linked account");
                    if (application == null) throw new Exception("Invalid application");
                    if (profileReferenceType == null) throw new Exception("Invalid document reference type");

                    var customerProfileDocuments = context.Documents.Include(d => d.File).Include(d => d.DocumentCheckList)
                                    .Include(d => d.DocumentCheckList.DocumentType)
                                    .FirstOrDefault(d => d.CustomerId == linkedAccount.CustomerId
                                    && d.ReferenceId == linkedAccount.CustomerId
                                    && d.IsActive && d.IsDeleted == false
                                    && d.ReferenceTypeId == profileReferenceType.Id
                                    && d.DocumentCheckList.DocumentType.Key == documentKey);

                    if (customerProfileDocuments != null)
                    {
                        var linkedAccountReferenceType = context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccounts);
                        if (linkedAccountReferenceType == null) throw new Exception("Invalid reference type");

                        var linkedAccountDocumentCheckList = context.DocumentCheckLists.Include(d => d.DocumentType)
                                             .Include(d => d.ReferenceType)
                                             .FirstOrDefault(dc => dc.ApplicationId == application.Id &&
                                             dc.ReferenceTypeId == linkedAccountReferenceType.Id &&
                                             dc.DocumentType.Key == documentKey);

                        if (linkedAccountDocumentCheckList == null) throw new Exception("Invalid document checklist");

                        var dbLocationType = context.LocationTypes.FirstOrDefault(l => l.Key == LocationTypeKeys.Database);
                        if (dbLocationType == null) throw new Exception("Invalid location type");

                        var status = context.Status.SingleOrDefault(o => o.Key == StatusKeys.DocumentPending);
                        if (status == null) throw new Exception("Invalid status");

                        var file = context.Files.FirstOrDefault(f => f.Id == customerProfileDocuments.FileId);
                        if (file == null) throw new Exception("Invalid file");

                        var document = new Document
                        {
                            CustomerId = linkedAccount.CustomerId,
                            ReferenceTypeId = linkedAccountReferenceType.Id,
                            ReferenceId = linkedAccount.Id,
                            DocumentCheckListId = linkedAccountDocumentCheckList.Id,
                            LocationTypeId = dbLocationType.Id,
                            StatusId = status.Id,
                            DocumentLocation = string.Format("{0}", "eServicesDb"),
                            DocumentName = file.FileName,
                            FileId = file.Id,
                            IsActive = true,
                            IsDeleted = false
                        };

                        context.Documents.Add(document);
                        context.SaveChanges();

                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region Update Document Status  
        public static void UpdateDocumentReferences(int customerId, int referenceId)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var customerDocuments = context.Documents.Include(d => d.ReferenceType)
                        .Where(d => d.ReferenceId == customerId || d.ReferenceId == referenceId
                                    && d.IsLocked == false && d.IsActive
                                    && (d.ReferenceType.Key == ReferenceTypeKeys.CustomerProfile ||
                                        d.ReferenceType.Key == ReferenceTypeKeys.LinkedAccounts)).ToList();

                    foreach (var document in customerDocuments)
                    {
                        document.IsLocked = true;
                        document.IsActive = true;
                        document.IsDeleted = false;

                        context.Entry(document).State = EntityState.Modified;                       
                    }

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region Verify Customer Documents
        public static void VerifyDocuments(int customerId, ReferenceType documentReferenceType)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var documentVerifiedStatus = context.Status.FirstOrDefault(s => s.Key == StatusKeys.DocumentVerified);                    
                    if (documentVerifiedStatus == null) throw new Exception("Invalid status");

                    var customerDocuments = context.Documents.Where(d => d.CustomerId == customerId
                                                                          && d.ReferenceId == d.CustomerId
                                                                          &&
                                                                          d.ReferenceTypeId ==
                                                                          documentReferenceType.Id);
                    foreach (var document in customerDocuments)
                    {
                        document.StatusId = documentVerifiedStatus.Id;
                        context.Entry(document).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        public static string GetDocumentAbsoluteUrl(string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    url = url.Replace(@"\", @"/");
                }
            }
            catch (Exception ex)
            {
                SecurityHelper.LogError(ex,"Error");
            }

            return url;
        }


       
        public static void WriteToTextFile(string textFileName, string textValue)
        {
            var root = HttpContext.Current.Server.MapPath("~/Logs/");
            string fileName = FormatTextFileName(textFileName);
            var path = System.IO.Path.Combine(root, fileName);
            path = System.IO.Path.GetFullPath(path);
            System.IO.File.WriteAllText(path, textValue);
        }
        public static string FormatTextFileName(string textFileName)
        {
            if (textFileName.ToLower().Contains(".txt"))
            {
                return textFileName;
            }
            else
            {
                return textFileName + ".txt";
            }
        }
    }
}