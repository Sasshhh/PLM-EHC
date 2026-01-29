using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Linq;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.ViewModels;
using MoreLinq;
using OfficeOpenXml;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class LinkedAccountController : Controller
    {
        eServicesDbContext context = new eServicesDbContext();
        private ValueAssistDbContext vassist = new ValueAssistDbContext();
        BaseHelper baseHelper = new BaseHelper();

        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }


        #region LinkedAccount Index GET
        // GET: /LinkedAccount/
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index(int id)
        {
            try
            {
                string linkedAccountsType;

                switch (id)
                {
                    case 0:
                        linkedAccountsType = "Pending Approval";
                        break;
                    case 1:
                        linkedAccountsType = "Pending Documentation";
                        break;
                    case 2:
                        linkedAccountsType = "Pending Email Activation";
                        break;
                    default:
                        linkedAccountsType = "Active";
                        break;
                }

                ViewBag.ShowNotification = (id == 1 || id == 2);
                ViewBag.CalendarDate = (id == 1 || id == 2) ? DateTime.Now.AddMonths(-6).ToString("yyyy/MM/dd"): "2019/07/01";
                ViewBag.LinkedAccountsType = linkedAccountsType;
                ViewBag.LinkedAccountTypeId = id;

                return View();
            }
            catch (Exception)
            {
                return View("_Error");
            }
        }
        #endregion


        #region LinkedAccount Add Note
        public JsonResult AddNote(int linkeAccountId, string note)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                try
                {
                    baseHelper.Initialise(eServicesDbContext);

                    var email = new Email();
                    var referenceType = eServicesDbContext.ReferenceTypes.FirstOrDefault(r => r.Key == ReferenceTypeKeys.LinkedAccounts);

                    if (referenceType == null) throw new Exception(string.Format("Invalid/ Missing reference type key: {0}", ReferenceTypeKeys.LinkedAccounts));

                    var noteType = eServicesDbContext.NoteTypes.FirstOrDefault(n => n.Key == NoteTypeKeys.Email);

                    if (noteType == null) throw new Exception(string.Format("Invalid/ Missing note type key: {0}", NoteTypeKeys.Email));

                    if (linkeAccountId > 0 && !string.IsNullOrEmpty(note))
                    {
                        var linkedAccount = eServicesDbContext.LinkedAccounts.Include(a => a.Account).FirstOrDefault(l => l.Id == linkeAccountId);

                        if (linkedAccount == null) throw new Exception("Invalid linked account");

                        var linkedAccountNote = new Note
                        {
                            ReferenceId = linkeAccountId,
                            ReferenceTypeId = referenceType.Id,
                            NoteTypeId = noteType.Id,
                            NoteEntry = note,
                            IsActive = true,
                            IsDeleted = false
                        };

                        eServicesDbContext.Notes.Add(linkedAccountNote);
                        var count = eServicesDbContext.SaveChanges();

                        if (count > 0)
                        {
                            var systemUser = new SystemUser();
                            //Check if linked account is created by Managing Agent
                            if (eServicesDbContext.AgentCustomers.Any(c => c.CustomerId == linkedAccount.CustomerId))
                            {
                                var agentCustomer = eServicesDbContext.AgentCustomers.FirstOrDefault(a => a.CustomerId == linkedAccount.CustomerId);
                                var agent =
                                    eServicesDbContext.Agents.Include(a => a.Customer)
                                        .Include(a => a.Customer.SystemUser)
                                        .FirstOrDefault(a => a.Id == agentCustomer.AgentId);
                                if (agent != null)
                                {
                                    systemUser = agent.Customer.SystemUser;
                                }
                            }
                            else
                            {
                                var customer =
                                    eServicesDbContext.Customers.Include(c => c.SystemUser)
                                        .FirstOrDefault(c => c.Id == linkedAccount.CustomerId);
                                if (customer == null) throw new Exception("Invalid Customer");

                                systemUser = customer.SystemUser;
                            }

                            var subject = string.Format("Siyakhokha Linked Account [{0}]",
                                linkedAccount.Account.AccountNumber);
                            var body = note;
                            var toList = systemUser.EmailAddress;

                            email.GenerateEmail(toList, subject, body, linkeAccountId.ToString(CultureInfo.InvariantCulture),
                                false, AppSettingKeys.EservicesDefaultEmailTemplate);
                        }
                    }
                    else
                    {
                        return Json(new { data = "Error" }, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception ex)
                {
                    throw;
                }

                return Json(new { data = "Success" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region LinkedAccount Details GET
        [EncryptedActionParameter]
        // GET: /LinkedAccount/Details/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                var emailList = new List<string>();
                var mobileList = new List<string>();
                CustomerLinkedAccountViewModel customerLinkedAccountVm;

                var linkedaccount = eServicesDbContext.LinkedAccounts.Include(l => l.Customer)
                .Include(l => l.Customer.CustomerType)
                .Include(l => l.Status)
                .Include(l => l.Account)
                .Include(l => l.RecipientType)
                .Include(l => l.LinkedAccountType)
                .SingleOrDefault(l => l.Id == id);

                if (linkedaccount == null) throw new Exception("Invalid Linked account");
                customerLinkedAccountVm = LinkedAccountHelper.PopulateCustomerLinkedModel(linkedaccount, emailList, mobileList);
                this.PopulateDocuments(linkedaccount);

                object obj = new { id };
                ViewBag.ReturnUrl = Url.Action("Details", "LinkedAccount", SecureActionLinkExtension.Encrypt(obj));
                ViewBag.RecipientType = linkedaccount.RecipientType.Key;
                ViewBag.EmailAddresses = emailList.ToArray();
                ViewBag.MobileNumbers = mobileList.ToArray();
                ViewBag.EncodedId = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", linkedaccount.Id)));
                return View(customerLinkedAccountVm);
            }
        }
        #endregion
        #region LinkedAccount Details POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(CustomerLinkedAccountViewModel customerLinkedAccountVm)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                try
                {
                    baseHelper.Initialise(eServicesDbContext);

                    var linkedaccount = eServicesDbContext.LinkedAccounts.Include(l => l.Customer)
                    .Include(l => l.Customer.CustomerType)
                    .Include(l => l.Status)
                    .Include(l => l.Account)
                    .Include(l => l.RecipientType)
                    .Include(l => l.LinkedAccountType)
                    .SingleOrDefault(l => l.Id == customerLinkedAccountVm.LinkedAccount.Id);

                    var emailList = new List<string>();
                    var mobileList = new List<string>();

                    if (linkedaccount == null)
                    {
                        TempData["Success"] = false;
                        throw new Exception("Invalid Linked account");
                    }

                    var email = new Email();
                    var currentLinkedEmails = eServicesDbContext.LinkedEmails.Where(le => le.LinkedAccountId == customerLinkedAccountVm.LinkedAccount.Id && le.IsActive
                                                                          && le.IsDeleted == false);
                    var secondaryEmails = email.ExtractEmails(Request.Form["SecondaryEmails"]);

                    if (!secondaryEmails.Any())
                    {
                        TempData["Success"] = false;
                        ViewBag.CompletedAction = "Recipient type emails requires at least one email.";
                    }
                    else
                    {
                        //If emails removed by customer
                        foreach (var mail in currentLinkedEmails)
                        {
                            var currentEmailExists = secondaryEmails.Any(le => string.Compare(le, mail.EmailAddress, StringComparison.OrdinalIgnoreCase) == 0);

                            if (!currentEmailExists)
                            {
                                mail.IsActive = false;
                                mail.IsDeleted = true;
                                mail.IsLocked = false;
                            }
                        }

                        //Add new secondary emails for customer
                        foreach (var mail in secondaryEmails)
                        {
                            var emailExists = currentLinkedEmails.Any(le => le.EmailAddress == mail);

                            if (!emailExists)
                            {
                                var customerLinkedAccountController = new CustomerLinkedAccountController();
                                customerLinkedAccountController.AddLinkedEmail(linkedaccount, linkedaccount.Customer, mail, this);

                                TempData["Success"] = true;
                                ViewBag.CompletedAction = "Emails linked to account";
                            }
                        }
                    }

                    eServicesDbContext.SaveChanges();
                    customerLinkedAccountVm = LinkedAccountHelper.PopulateCustomerLinkedModel(linkedaccount, emailList, mobileList);
                    this.PopulateDocuments(linkedaccount);

                    object obj = new { id = linkedaccount.Id };
                    ViewBag.ReturnUrl = Url.Action("Details", "LinkedAccount", SecureActionLinkExtension.Encrypt(obj));
                    ViewBag.RecipientType = linkedaccount.RecipientType.Key;
                    ViewBag.EmailAddresses = emailList.ToArray();
                    ViewBag.MobileNumbers = mobileList.ToArray();
                    ViewBag.EncodedId = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", linkedaccount.Id)));

                    return View(customerLinkedAccountVm);
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion


        #region LinkedAccount Create GET
        // GET: /LinkedAccount/Create
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create()
        {
            ViewBag.RecipientTypeId = new SelectList(context.RecipientTypes.Where(rt => rt.IsActive && rt.IsDeleted == false),
                "Id", "Name");
            ViewBag.CreatedBySystemUserId = new SelectList(context.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(context.SystemUsers, "Id", "FirstName");
            return View();
        }
        #endregion
        #region LinkedAccount Create POST
        // POST: /LinkedAccount/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create([Bind(Include = "Id,AccountId,CustomerId,EmailAddress,RecipientTypeId,StatusId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] LinkedAccount linkedaccount,
            string customerIdNo, string accountNo, string createType, HttpPostedFileBase file, List<HttpPostedFileBase> documents)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                try
                {
                    baseHelper.Initialise(eServicesDbContext);

                    var email = new Email();
                    var customerLinkedAccountController = new CustomerLinkedAccountController();
                    var success = false;
                    var documentsRequired = false;
                    var completedAction = "";
                    var entity = eServicesDbContext.Entities.FirstOrDefault(e => e.EntityRegistrationNumber == customerIdNo && (e.IsActive && !e.IsDeleted));
                    var customer = (entity != null) ? eServicesDbContext.Customers.Include(c => c.CustomerType).FirstOrDefault(c => c.Id == entity.CustomerId && (c.IsActive && !c.IsDeleted)): 
                                   eServicesDbContext.Customers.Include(c => c.CustomerType).FirstOrDefault(c => c.IdentificationNumber == customerIdNo && (c.IsActive && !c.IsDeleted)); 
                    var idNoExists = eServicesDbContext.Customers.Any(c => c.IdentificationNumber == customerIdNo)
                                      || (eServicesDbContext.Entities.Any(c => c.EntityRegistrationNumber == customerIdNo));
                    var secondaryEmails = email.ExtractEmails(Request.Form["SecondaryEmails"]);
                    var bulkUploadLog = new List<BulkUploadLog>();

                    if (!idNoExists)
                    {
                        completedAction = "ID/Registration number not found";
                    }
                    else if (createType.Equals("BulkUpload"))
                    {
                        if (file == null || file.FileName == null)
                        {
                            completedAction = "Please upload excel file";
                        }
                        else if (!Path.GetExtension(file.FileName).Equals(".xlsx"))
                        {
                            completedAction = "Please upload excel file with .xlsx format";
                        }
                        else if (documents == null)
                        {
                            completedAction = "Please upload document(s)";
                        }
                        else
                        {
                            using (var package = new ExcelPackage(file.InputStream))
                            {
                                if (package.Workbook.Worksheets.Any())
                                {
                                    var worksheet = package.Workbook.Worksheets.First();
                                    const int startRow = 2;
                                    var accounts = new List<LinkedAccount>();

                                    for (var rowNum = startRow; rowNum <= worksheet.Dimension.End.Row; rowNum++)
                                    {
                                        var wsRow = worksheet.Cells[rowNum, 1, rowNum, worksheet.Dimension.End.Column];

                                        foreach (var cell in wsRow)
                                        {
                                            var accountNumber = cell.Text;
                                            var action = CreateLinkAccount(customerIdNo, accountNumber, true);
                                            var log = new BulkUploadLog
                                            {
                                                CellId = cell.Address,
                                                IsSuccess = action.Item2,
                                                AccountNumber = accountNumber,
                                                LinkedAccountId = action.Item3,
                                                Message = action.Item1
                                            };

                                            bulkUploadLog.Add(log);

                                            if (log.IsSuccess)
                                                accounts.Add(action.Item5);

                                            //cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                            //cell.Style.Fill.BackgroundColor.SetColor(log.IsSuccess
                                            //    ? Color.Green
                                            //    : Color.Red);
                                        }
                                    }

                                    if (secondaryEmails != null && secondaryEmails.Any() && accounts.Any())
                                    {
                                        foreach (var mail in secondaryEmails)
                                        {
                                            customerLinkedAccountController.AddLinkedEmail(accounts, customer, mail, this);
                                        }
                                    }

                                    if (bulkUploadLog.Any(b => b.IsSuccess))
                                    {
                                        var document = new FileController();
                                        var documentReferenceType = (customer.CustomerType.Key == CustomerTypeKeys.Individual) ? eServicesDbContext.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccounts) :
                                        eServicesDbContext.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccountsManagingAgent);
                                        var application = eServicesDbContext.Applications.FirstOrDefault(a => a.Key == ApplicationKeys.LinkedAccounts);

                                        if (documentReferenceType == null) throw new Exception("Invalid document reference type");
                                        if (application == null) throw new Exception("Invalid application");

                                        document.ManualAddDocument(customer.Id, documentReferenceType.Id, application.Id,
                                            bulkUploadLog.Where(b => b.IsSuccess).Select(b => b.LinkedAccountId).ToList(), documents);
                                    }

                                    ViewBag.UploadLog = bulkUploadLog;
                                    success = true;
                                    completedAction = "Bulk upload complete";
                                }
                                else
                                {
                                    completedAction = "No Worksheet found on excel file";
                                }

                                //var fileBytesArray = package.GetAsByteArray();
                                //return File(fileBytesArray, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.FileName + ".xlsx");
                            }
                        }
                    }
                    else if (createType.Equals("SingleAccount"))
                    {
                        var result = CreateLinkAccount(customerIdNo, accountNo, false);
                        completedAction = result.Item1;
                        success = result.Item2;
                        documentsRequired = result.Item4;
                        linkedaccount = result.Item5;

                        if (secondaryEmails != null && secondaryEmails.Any() && success)
                        {
                            foreach (var mail in secondaryEmails)
                            {
                                customerLinkedAccountController.AddLinkedEmail(result.Item5, customer, mail, this);
                            }
                        }
                    }

                    ViewBag.DocumentsRequired = documentsRequired;
                    ViewBag.IsActivationEmailSent = eServicesDbContext.LinkedEmails.Include(le => le.Status)
                                          .Any(le => le.LinkedAccountId == linkedaccount.Id
                                          && le.Status.Key == StatusKeys.PendingEmailVerification);
                    TempData["Success"] = success;
                    ViewBag.CompletedAction = completedAction;
                    ViewBag.RecipientTypeId = new SelectList(eServicesDbContext.RecipientTypes.Where(rt => rt.IsActive && rt.IsDeleted == false).ToList(),
                    "Id", "Name");
                    ViewBag.CreatedBySystemUserId = new SelectList(eServicesDbContext.SystemUsers.ToList(), "Id", "FirstName", linkedaccount.CreatedBySystemUserId);
                    ViewBag.ModifiedBySystemUserId = new SelectList(eServicesDbContext.SystemUsers.ToList(), "Id", "FirstName", linkedaccount.ModifiedBySystemUserId);

                    return View(linkedaccount);
                }
                catch (Exception ex)
                {
                    return View("_Error");
                }
            }
        }
        #endregion

        #region LinkedAccount Link Account References

        public Tuple<string, bool, int, bool, LinkedAccount> CreateLinkAccount(string customerIdNo, string accountNo, bool isBulkUpload)
        {
            using (var eServicesContext = new eServicesDbContext())
            {
                try
                {
                    baseHelper.Initialise(eServicesContext);

                    var success = false;
                    var account = new Account();
                    var linkedAccount = new LinkedAccount();
                    var customer = new Customer();
                    string completedAction;
                    var isVwAccount = vassist.Accounts.FirstOrDefault(c => c.AccountNumber == accountNo);
                    var accountExistsEservices =
                        eServicesContext.Accounts.FirstOrDefault(a => a.AccountNumber == accountNo && a.IsActive && !a.IsDeleted);
                    var entity =
                          eServicesContext.Entities.FirstOrDefault(e => e.EntityRegistrationNumber == customerIdNo && e.IsActive && !e.IsDeleted);
                    customer = (entity != null)
                        ? eServicesContext.Customers.FirstOrDefault(c => c.Id == entity.CustomerId && c.IsActive && !c.IsDeleted)
                        : eServicesContext.Customers.FirstOrDefault(c => c.IdentificationNumber == customerIdNo && c.IsActive && !c.IsDeleted);
                    var accLinked = eServicesContext.LinkedAccounts.Include(l => l.Account).Include(l => l.Customer)
                        .Any(l => l.Account.AccountNumber == accountNo
                                  && l.Customer.IdentificationNumber == customer.IdentificationNumber
                                  && l.IsActive && !l.IsDeleted);
                    var documentsRequired = false;
                    
                    if (isVwAccount == null)
                    {
                        completedAction = "Account number not found";
                    }
                    else if (accLinked)
                    {
                        completedAction = "Account already linked to customer profile";
                    }
                    else
                    {
                        var customerRegisteredAccounts = (entity != null)
                            ? vassist.Accounts.Where(a => a.ReferenceNumber == entity.EntityRegistrationNumber).ToList()
                            : vassist.Accounts.Where(a => a.ReferenceNumber == customer.IdentificationNumber).ToList();

                        var accountStatus =
                            eServicesContext.Status.FirstOrDefault(x => x.Key.Equals(StatusKeys.AccountActive));
                        var linkedAccountPendingStatus =
                            eServicesContext.Status.FirstOrDefault(s => s.IsActive && s.IsDeleted == false
                                                                        && s.Key == StatusKeys.DocumentPending);
                        var linkedAccountActiveStatus =
                            eServicesContext.Status.FirstOrDefault(s => s.IsActive && s.IsDeleted == false
                                                                        && s.Key == StatusKeys.LinkedAccountActive);
                        var recipientTypeId = Convert.ToInt32(Request.Form["RecipientTypeDdl"]);
                        var recipientType = eServicesContext.RecipientTypes.Find(recipientTypeId);

                        if (customer == null) throw new Exception("Invalid customer");
                        if (accountStatus == null) throw new Exception("Invalid status");
                        if (linkedAccountPendingStatus == null) throw new Exception("Invalid status");
                        if (linkedAccountActiveStatus == null) throw new Exception("Invalid status");
                        if (recipientType == null) throw new Exception("Invalid recipient type");

                        if (accountExistsEservices == null)
                        {
                            account.AccountHolder = isVwAccount.AccountHolder;
                            account.IsRMSAccount = isVwAccount.IsRMSAccount;
                            account.AccountNumber = isVwAccount.AccountNumber;
                            account.StatusId = accountStatus.Id;
                            eServicesContext.Accounts.Add(account);
                            eServicesContext.SaveChanges();
                        }
                        else
                        {
                            account = accountExistsEservices;
                        }

                        var linkedAccountTypeAssociated =
                            eServicesContext.LinkedAccountTypes.FirstOrDefault(
                                la => la.Key == LinkedAccountTypeKeys.Associated);
                        var linkedAccountTypeUnassociated =
                            eServicesContext.LinkedAccountTypes.FirstOrDefault(
                                la => la.Key == LinkedAccountTypeKeys.Unassociated);

                        if (linkedAccountTypeAssociated == null) throw new Exception("Invalid linked account type");
                        if (linkedAccountTypeUnassociated == null) throw new Exception("Invalid linked account type");

                        var isRegistered = customerRegisteredAccounts.Any(va => va.AccountNumber == account.AccountNumber);

                        linkedAccount.CustomerId = customer.Id;
                        linkedAccount.AccountId = account.Id;
                        linkedAccount.StatusId = (isBulkUpload) ? linkedAccountActiveStatus.Id : (!isRegistered)
                            ? linkedAccountPendingStatus.Id
                            : linkedAccountActiveStatus.Id;
                        linkedAccount.LinkedAccountTypeId = (!isRegistered)
                            ? linkedAccountTypeUnassociated.Id
                            : linkedAccountTypeAssociated.Id;
                        linkedAccount.RecipientTypeId = recipientTypeId;
                        //linkedAccount.EmailAddress = customer.EmailAddress;
                        linkedAccount.IsActive = true;
                        linkedAccount.IsDeleted = false;
                        linkedAccount.IsLocked = false;

                        eServicesContext.LinkedAccounts.Add(linkedAccount);
                        eServicesContext.SaveChanges();

                        if (!isRegistered)
                            documentsRequired = true;
                        completedAction = "Linked account created";
                        success = true;
                    }

                    return Tuple.Create(completedAction, success, linkedAccount.Id, documentsRequired, linkedAccount);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion


        #region DB Update Exception Handler
        //private Exception HandleDbUpdateException(DbUpdateException dbu)
        //{
        //    var builder = new StringBuilder("A DbUpdateException was caught while saving changes. ");

        //    try
        //    {
        //        foreach (var result in dbu.Entries)
        //        {
        //            builder.AppendFormat("Type: {0} was part of the problem. ", result.Entity.GetType().Name);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        builder.Append("Error parsing DbUpdateException: " + e.ToString());
        //    }

        //    string message = builder.ToString();
        //    return new Exception(message, dbu);
        //}
        #endregion


        #region LinkedAccount Edit GET
        // GET: /LinkedAccount/Edit/5
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(int? id)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                try
                {
                    var linkedaccount = eServicesDbContext.LinkedAccounts.Include(l => l.Customer)
                        .Include(l => l.Account)
                        .SingleOrDefault(l => l.Id == id);

                    if (linkedaccount == null) throw new Exception("Invalid Linked account");

                    ViewBag.RecipientTypeId = new SelectList(eServicesDbContext.RecipientTypes.Where(rt => rt.IsActive && rt.IsDeleted == false).ToList(),
                    "Id", "Name");
                    ViewBag.CreatedBySystemUserId = new SelectList(eServicesDbContext.SystemUsers.ToList(), "Id", "FirstName", linkedaccount.CreatedBySystemUserId);
                    ViewBag.ModifiedBySystemUserId = new SelectList(eServicesDbContext.SystemUsers.ToList(), "Id", "FirstName", linkedaccount.ModifiedBySystemUserId);
                    return View(linkedaccount);
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion
        #region LinkedAccount Edit POST
        // POST: /LinkedAccount/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit([Bind(Include = "Id,AccountId,CustomerId,EmailAddress,RecipientTypeId,StatusId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] LinkedAccount linkedaccount)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                try
                {
                    baseHelper.Initialise(eServicesDbContext);

                    if (ModelState.IsValid)
                    {
                        eServicesDbContext.Entry(linkedaccount).State = EntityState.Modified;
                        eServicesDbContext.SaveChanges();

                        return RedirectToAction("Index", new { id = 0 });
                    }

                    ViewBag.RecipientTypeId = new SelectList(eServicesDbContext.RecipientTypes.Where(rt => rt.IsActive && rt.IsDeleted == false).ToList(),
                    "Id", "Name");
                    ViewBag.CreatedBySystemUserId = new SelectList(eServicesDbContext.SystemUsers.ToList(), "Id", "FirstName", linkedaccount.CreatedBySystemUserId);
                    ViewBag.ModifiedBySystemUserId = new SelectList(eServicesDbContext.SystemUsers.ToList(), "Id", "FirstName", linkedaccount.ModifiedBySystemUserId);
                    return View(linkedaccount);
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion


        #region LinkedAccount Delete GET
        // GET: /LinkedAccount/Delete/5
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Delete(int? id)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                try
                {
                    baseHelper.Initialise(eServicesDbContext);

                    var linkedaccount = eServicesDbContext.LinkedAccounts.Include(l => l.Customer)
                    .Include(l => l.Status)
                    .Include(l => l.Account)
                    .Include(l => l.RecipientType)
                    .SingleOrDefault(l => l.Id == id);

                    if (linkedaccount == null) throw new Exception("Invalid Linked account");

                    return View(linkedaccount);
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion
        #region LinkedAccount Delete POST
        // POST: /LinkedAccount/Delete/5
        [HttpPost, ActionName("Delete")]
        [EncryptedActionParameter]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                try
                {
                    baseHelper.Initialise(eServicesDbContext);
                    var email = new Email();
                    var unlinkStatus = eServicesDbContext.Status.FirstOrDefault(s => s.Key == StatusKeys.LinkedAccountUnlinked);
                    var linkedaccount = eServicesDbContext.LinkedAccounts.Include(l => l.Customer)
                    .Include(l => l.Status)
                    .Include(l => l.Account)
                    .Include(l => l.RecipientType)
                    .FirstOrDefault(l => l.Id == id);
                    
                    if (unlinkStatus == null) throw new Exception("Invalid status");
                    if (linkedaccount == null) throw new Exception("Invalid linked account");

                    var currentStatus = linkedaccount.Status;
                    linkedaccount.IsActive = false;
                    linkedaccount.IsDeleted = true;
                    linkedaccount.IsLocked = false;
                    linkedaccount.StatusId = unlinkStatus.Id;

                    eServicesDbContext.SaveChanges();

                    //send email notification 
                    const string subject = "Siyakhokha: Linked Account";
                    var body = (currentStatus.Key == StatusKeys.LinkedAccountActive) ? string.Format("Account: <strong>" + linkedaccount.Account.AccountNumber + "</strong> has been unlinked.") :
                                string.Format("Your request to link account: <strong>" + linkedaccount.Account.AccountNumber + "</strong> has been rejected.");

                    email.GenerateEmail(linkedaccount.Customer.EmailAddress, subject, body, linkedaccount.CustomerId.ToString(CultureInfo.InvariantCulture),
                        false, AppSettingKeys.EservicesDefaultEmailTemplate, linkedaccount.Customer.FullName);

                    TempData["Success"] = "Success! Account Unlinked.";
                    
                    object obj = new { id = 0 };
                    return RedirectToAction("Index", "LinkedAccount", SecureActionLinkExtension.Encrypt(obj));
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion


        #region LinkedAccount Approve Linked Account POST
        //Approve Link Account
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LinkAccount(int? id, bool overrideDoc)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                try
                {
                    baseHelper.Initialise(eServicesDbContext);

                    var linkedaccount = eServicesDbContext.LinkedAccounts.Include(l => l.Customer)
                    .Include(l => l.Customer.CustomerType)
                    .Include(l => l.Status)
                    .Include(l => l.Account)
                    .Include(l => l.RecipientType)
                    .Include(l => l.LinkedAccountType)
                    .SingleOrDefault(l => l.Id == id);

                    var linkedAccountActive = eServicesDbContext.Status.FirstOrDefault(s => s.IsActive && !s.IsDeleted
                        && s.Key == StatusKeys.LinkedAccountActive);
                    var linkedAccountPendingApproval = eServicesDbContext.Status.FirstOrDefault(s => s.IsActive && !s.IsDeleted
                        && s.Key == StatusKeys.LinkedAccountPending);

                    if (linkedaccount == null) throw new Exception("Invalid Linked account");
                    if (linkedAccountActive == null) throw new Exception("Invalid status");
                    if (linkedAccountPendingApproval == null) throw new Exception("Invalid status");

                    var returnId = 0;
                    var email = new Email();

                    if (overrideDoc || linkedaccount.Status.Key == StatusKeys.LinkedAccountPending)
                    {
                        linkedaccount.StatusId = linkedAccountActive.Id;
                        linkedaccount.IsActive = true;
                        linkedaccount.IsDeleted = false;
                        linkedaccount.IsLocked = false;

                        //lock documents
                        DocumentHelper.UpdateDocumentReferences(linkedaccount.CustomerId, linkedaccount.Id);

                        //send email notification 
                        const string subject = "Siyakhokha: Linked Account";
                        var body = string.Format("Your request to link account: <strong>" + linkedaccount.Account.AccountNumber + "</strong> has been approved.");
                        email.GenerateEmail(linkedaccount.Customer.EmailAddress, subject, body, linkedaccount.CustomerId.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, linkedaccount.Customer.FullName);

                        eServicesDbContext.SaveChanges();
                        TempData["Success"] = "Success! Account linked.";
                        returnId = 3;
                    }
                    else if (linkedaccount.Status.Key == StatusKeys.DocumentPending)
                    {
                        TempData["Error"] = "Linked account pending documentation. Please make sure customer has uploaded required documentation.";
                        returnId = 1;
                    }
                    else
                    {
                        TempData["Error"] = "Linked Account has been processed already. Pending Linked Accounts Refreshed";                       
                    }

                    object obj = new { id = returnId };
                    return RedirectToAction("Index", "LinkedAccount", SecureActionLinkExtension.Encrypt(obj));
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion


        #region LinkedAccount Load Linked Accounts GET
        [HttpGet]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult LoadLinkedAccounts(int id, string startDate, string endDate, int pageNumber, int pageSize, string searchText)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                try
                {
                    baseHelper.Initialise(eServicesDbContext);
                    var rows = new List<object>();
                    List<LinkedAccount> linkedAccounts;
                    int total;
                    var skip = (pageNumber - 1) * pageSize;
                    const string goLiveDate = "2019/07/01";
                    var sixMonthsHistoryDate = DateTime.Now.AddMonths(-6).ToString("yyyyMMdd");
                    var filterDate = (id == 1 || id == 2) ? sixMonthsHistoryDate : goLiveDate;

                    var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(filterDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
                    var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);

                    var linkedAccountPending = eServicesDbContext.Status.FirstOrDefault(s => s.Key == StatusKeys.LinkedAccountPending);
                    var documentsPending = eServicesDbContext.Status.FirstOrDefault(s => s.Key == StatusKeys.DocumentPending);
                    var linkedAccountActive = eServicesDbContext.Status.FirstOrDefault(s => s.Key == StatusKeys.LinkedAccountActive);
                    var linkedEmailPendingActivation = eServicesDbContext.Status.FirstOrDefault(s => s.Key == StatusKeys.PendingEmailVerification);

                    if (linkedAccountPending == null) throw new Exception("Invalid status");
                    if (documentsPending == null) throw new Exception("Invalid status");
                    if (linkedAccountActive == null) throw new Exception("Invalid status");
                    if (linkedEmailPendingActivation == null) throw new Exception("Invalid status");

                    var linkedAccountModels = eServicesDbContext.LinkedAccounts.Include(l => l.Account)
                        .Include(l => l.Customer)
                        .Include(l => l.Customer.CustomerType)
                        .Where(
                            l =>
                                l.IsActive && !l.IsDeleted &&
                                (DbFunctions.TruncateTime(l.CreatedDateTime) >= startDateTime &&
                                 DbFunctions.TruncateTime(l.CreatedDateTime) <= endDateTime));

                    switch (id)
                    {
                        case 0:
                            linkedAccountModels = linkedAccountModels.Where(l => l.StatusId == linkedAccountPending.Id);
                            break;
                        case 1:
                            linkedAccountModels = linkedAccountModels.Where(l => l.StatusId == documentsPending.Id);                            
                            break;
                        case 2:
                            linkedAccountModels = (from linkedaccount in linkedAccountModels
                                                   join linkedEmail in eServicesDbContext.LinkedEmails
                                                  on linkedaccount.Id equals linkedEmail.LinkedAccountId
                                              where linkedEmail.Status.Key == linkedEmailPendingActivation.Key
                                              select linkedaccount).Include(l => l.Customer).Include(l => l.Account)
                                              .Include(l => l.Customer.CustomerType);
                            break;
                        default:
                            linkedAccountModels = linkedAccountModels.Where(l => l.StatusId == linkedAccountActive.Id);
                            break;
                    }

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        linkedAccounts = (from linkedaccount in linkedAccountModels
                                              join customer in eServicesDbContext.Customers
                                                  on linkedaccount.CustomerId equals customer.Id
                                                  into custacc from y in custacc.DefaultIfEmpty() 
                                              join entity in eServicesDbContext.Entities
                                                  on linkedaccount.CustomerId equals entity.CustomerId
                                                  into custEntity from x in custEntity.DefaultIfEmpty()
                                              join entityAgent in eServicesDbContext.EntityAgents
                                                  on x.Id equals entityAgent.EntityId
                                                  into entityAgents from z in entityAgents.DefaultIfEmpty()
                                          where (y.IdentificationNumber ?? "").Contains(searchText) ||
                                                (x.EntityRegistrationNumber ?? "").Contains(searchText) ||
                                                (linkedaccount.Account.AccountNumber ?? "").Contains(searchText)
                                          select linkedaccount).Include(l => l.Customer.CustomerType)
                                          .Include(l => l.Account).Distinct().ToList();
                                              
                        total = linkedAccounts.Count();
                        linkedAccounts = linkedAccounts.OrderByDescending(l => l.ModifiedDateTime)
                            .Skip(skip).Take(pageSize).ToList();
                    }
                    else
                    {
                        linkedAccounts = linkedAccountModels.OrderByDescending(l => l.ModifiedDateTime)
                         .Skip(skip).Take(pageSize).ToList();
                        total = linkedAccountModels.Count();
                    }

                    foreach (var linkedaccount in linkedAccounts)
                    {
                        var identificationNumber = "";

                        switch (linkedaccount.Customer.CustomerType.Key)
                        {
                            case CustomerTypeKeys.Entity:
                                var entity = context.Entities.FirstOrDefault(e => e.CustomerId == linkedaccount.CustomerId);
                                if (entity != null)
                                    identificationNumber = entity.EntityRegistrationNumber;
                                break;
                            case CustomerTypeKeys.ManagingAgent:
                                var agent = context.Agents.FirstOrDefault(a => a.CustomerId == linkedaccount.CustomerId);

                                if (agent != null)
                                {
                                    var entityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);

                                    identificationNumber = (entityAgent != null)
                                        ? context.Entities.Find(entityAgent.EntityId).EntityRegistrationNumber
                                        : context.Entities.First(e => e.CustomerId == agent.CustomerId).EntityRegistrationNumber;
                                }
                                break;
                            default:
                                identificationNumber = linkedaccount.Customer.IdentificationNumber;
                                break;
                        }

                        rows.Add(new
                        {
                            LinkedAccountId = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", linkedaccount.Id))),
                            IDNumber = identificationNumber,
                            AccountNumber = linkedaccount.Account.AccountNumber,
                            DateCreated = String.Format("{0:yyyy/MM/dd}", linkedaccount.CreatedDateTime),
                            DateModified = String.Format("{0:yyyy/MM/dd}", linkedaccount.ModifiedDateTime)
                        });

                        ViewBag.CalendarDate = filterDate;
                    }

                    return Json(new { total, rows }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region LinkedAccount Resend Customer Activation Email
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult SendActivationEmail(string id)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                try
                {
                    baseHelper.Initialise(eServicesDbContext);

                    var linkedEmailId = Convert.ToInt32(id);
                    var linkedEmail = eServicesDbContext.LinkedEmails.Include(le => le.LinkedAccount.Account)
                                      .Include(le => le.LinkedAccount.Customer)
                                      .FirstOrDefault(le => le.Id == linkedEmailId);

                    if (linkedEmail == null) throw new Exception("Invalid Linked email");

                    //send activation email
                    var activationEmail = new Email();
                    const string subject = "Siyakhokha: Linked Account Activation Email";
                    var body = string.Format("You have requested to link a new email address: " + linkedEmail.EmailAddress + " to your linked account: <strong>" + linkedEmail.LinkedAccount.Account.AccountNumber + ".</strong><br/><br/>Please click on the link below to activate this email address: <br/><a href=\"{1}\" title=\"Activation Email\">{1}</a>", linkedEmail.LinkedAccount.Customer.FullName, Url.Action("ConfirmActivationEmail", "Account", SecureActionLinkExtension.Encrypt(new { token = linkedEmail.Id }), protocol: HttpContext.Request.Url.Scheme));

                    activationEmail.GenerateEmail(linkedEmail.EmailAddress, subject, body, linkedEmail.LinkedAccount.CustomerId.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, linkedEmail.LinkedAccount.Customer.FullName);
                    var response = "<strong>Success!</strong> Activation email sent to " + "<strong>" + linkedEmail.EmailAddress + "</strong>";

                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region LinkedAccount Linked Emails GET View
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LinkedEmailsIndex()
        {
            try
            {
                return View("LinkedEmails");
            }
            catch (Exception)
            {
                return View("_Error");
            }
        }
        #endregion

        #region LinkedAccount Load Linked Emails GET
        [HttpGet]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult LoadLinkedEmails(int pageNumber, int pageSize, string searchText)
        {
            using (var eServicesDbContext = new eServicesDbContext())
            {
                try
                {
                    baseHelper.Initialise(eServicesDbContext);
                    var rows = new List<object>();
                    List<LinkedEmail> linkedEmails;
                    int total;
                    var skip = (pageNumber - 1) * pageSize;

                    var linkedEmailsModels = eServicesDbContext.LinkedEmails.Include(l => l.LinkedAccount.Account)
                        .Include(l => l.Status)
                        .Where(
                            l =>
                                l.IsActive && !l.IsDeleted);

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        linkedEmails = (from linkedemail in linkedEmailsModels
                                        join linkedaccount in eServicesDbContext.LinkedAccounts
                                              on linkedemail.LinkedAccountId equals linkedaccount.Id
                                              into linkedEmailAcc
                                          from y in linkedEmailAcc.DefaultIfEmpty()
                                          join account in eServicesDbContext.Accounts
                                              on linkedemail.LinkedAccount.AccountId equals account.Id
                                              into linkedAcc
                                          from x in linkedAcc.DefaultIfEmpty()
                                          where (linkedemail.EmailAddress ?? "").Contains(searchText) ||
                                                (x.AccountNumber ?? "").Contains(searchText) ||
                                                (linkedemail.Status.Name ?? "").Contains(searchText)
                                        select linkedemail).Include(l => l.LinkedAccount.Account)
                                        .Include(l => l.Status).ToList();

                        total = linkedEmails.Count();
                        linkedEmails = linkedEmails.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                    }
                    else
                    {
                        linkedEmails = linkedEmailsModels.OrderBy(l => l.Id)
                         .Skip(skip).Take(pageSize).ToList();
                        total = linkedEmailsModels.Count();
                    }

                    foreach (var linkedemail in linkedEmails)
                    {
                        rows.Add(new
                        {
                            LinkedAccountId = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", linkedemail.LinkedAccount.Id))),
                            AccountNumber = linkedemail.LinkedAccount.Account.AccountNumber,
                            EmailAddress = linkedemail.EmailAddress,
                            Status = linkedemail.Status.Name,
                            AccountId = linkedemail.IsActive,
                        });
                    }

                    return Json(new { total, rows }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion


        #region LinkedAccount Bind Email RecipientType
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        private void PopulateEmailRecipientType(RecipientType currentType = null)
        {
            var recipientQuery = context.RecipientTypes.ToList();
            ViewBag.RecipientTypes = (currentType != null) ? new SelectList(recipientQuery, "Id", "Name", currentType.Id)
                : new SelectList(recipientQuery, "Id", "Name");
        }
        #endregion


        #region LinkedAccount Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
