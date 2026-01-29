using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Microsoft.AspNet.Identity;

namespace C8.eServices.Mvc.Controllers
{
    public class CommentsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        // GET: Comments
        public ActionResult Index()
        {
            var comments = db.Comments.Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.ReferenceType);
            return View(comments.ToList());
        }

        // GET: Comments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: Comments/Create
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "Id");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "Id");
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Id");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ReferenceTypeId,ReferenceNumeric,ReferenceAlpha,Comments,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", comment.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", comment.ModifiedBySystemUserId);
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name", comment.ReferenceTypeId);
            return View(comment);
        }

        // GET: Comments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", comment.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", comment.ModifiedBySystemUserId);
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name", comment.ReferenceTypeId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ReferenceTypeId,ReferenceNumeric,ReferenceAlpha,Comments,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(comment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", comment.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", comment.ModifiedBySystemUserId);
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name", comment.ReferenceTypeId);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comments.Find(id);
            db.Comments.Remove(comment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Request Access to Account Number POST
        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult RequestAccountAccess(string accountNo,string Comment)
        {
            using (var context = new eServicesDbContext())
            {
                try {
                var CurrUser = User.Identity.GetUserName();
                var customerSys = context.SystemUsers.SingleOrDefault(c => c.UserName == CurrUser);
                var customer= context.Customers.Include(c => c.CustomerType)
                         .SingleOrDefault(c => c.SystemUserId == customerSys.Id);
                if (customer == null) throw new Exception("Invalid Customer");
                Comment Comms = new Comment();
                Request Req = new Request();
                string accNo = accountNo.Substring(0, 10);
                var AccountHolder = context.LinkedAccounts.Include(la => la.Account)
                    .Include(la => la.Customer)
                    .Include(la => la.Customer.CustomerType)
                    .Include(la => la.LinkedAccountType)
                    .FirstOrDefault(la => la.Account.AccountNumber == accNo);
                    var AccountHolderSys= context.SystemUsers.SingleOrDefault(c => c.Id == AccountHolder.Customer.SystemUserId);
                    var contactMeth = "";
                    if(AccountHolderSys.EmailAddress!=null && AccountHolderSys.EmailAddress !="")
                    {
                        contactMeth = AccountHolderSys.EmailAddress;
                    }
                    else
                    {
                        contactMeth = AccountHolderSys.MobileNumber;
                    }
                    var referenceType = (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent || customer.CustomerType.Key == CustomerTypeKeys.Entity)
                                           ? context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.LinkedAccountsManagingAgent)) :
                                           context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccounts);
                    var statusType= context.Status.FirstOrDefault(d => d.Key == StatusKeys.LinkedAccountPending);
                    //check if request already made
                    var checkRequestAlreadyExists = db.Requests.Include(r => r.Comment).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.RecipientCustomer).Include(r => r.ReferenceType).Include(r => r.Status)
                        .Where(a=>a.Status.Key == StatusKeys.LinkedAccountPending && a.IsActive && !a.IsDeleted && a.ReferenceAlpha==accNo && a.CustomerId== customer.Id && a.RecipientCustomerId== AccountHolder.CustomerId).FirstOrDefault();
                    if(checkRequestAlreadyExists!=null)
                    {
                        return Json(new { isRequested = true }, JsonRequestBehavior.AllowGet);
                    }

                    //Add Comment
                    Comms.ReferenceAlpha = accNo;
                    Comms.ReferenceTypeId = referenceType.Id;
                    
                    Comms.IsActive = true;
                Comms.Comments = Comment;
                Comms.CreatedBySystemUserId = customerSys.Id;
                Comms.ModifiedBySystemUserId= customerSys.Id;

                    

                    //Create Request
                    Req.CustomerId = customer.Id;
                    Req.CommentId = Comms.Id;
                    Req.ReferenceTypeId = referenceType.Id;
                    Req.ReferenceAlpha = accNo;
                    Req.ReferenceNumeric= customerSys.Id;
                    Req.RecipientContact = contactMeth.ToString();
                    Req.RecipientCustomerId = AccountHolder.CustomerId;
                    Req.StatusId = statusType.Id;
                    Req.CreatedBySystemUserId = customerSys.Id;
                    Req.ModifiedBySystemUserId = customerSys.Id;

                    Comms.ReferenceNumeric = Req.Id;
                    context.Comments.Add(Comms);
                    context.Requests.Add(Req);
                    

                    context.SaveChanges();
                    //Add Email/sms Notification
                    if (AccountHolderSys != null)
                    {
                        var RequestAccountAccessEmail = new Email();
                        try
                        {
                            if (AccountHolderSys.EmailAddress != null && !AccountHolderSys.EmailAddress.Trim().Equals(string.Empty))
                            {
                                const string subject = "Ekurhuleni Housing Company: Request to access Municipal Account";
                                var body = "Dear "+ AccountHolder.Customer.FullName+ " there has been a request to access your municipal account on Siyakhokha<br/>" +
                                    "Requested by: " + customer.FullName + "<br/>" +
                                    "Municipal Account: " + accNo +"<br/> Please log in to your Siyakhokha profile to view the request.";
                                RequestAccountAccessEmail.GenerateEmail(AccountHolderSys.EmailAddress, subject, body, AccountHolder.Customer.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, AccountHolder.Customer.FullName);

                            }

                            else if (AccountHolderSys.MobileNumber != null && !AccountHolderSys.MobileNumber.Trim().Equals(string.Empty))
                            {
                                var bodysms = "Dear " + AccountHolder.Customer.FullName + " there has been a request to access your municipal account on Siyakhokha\\n" +
                                   "Requested by: " + customer.FullName + "\\n" +
                                   "Municipal Account: " + accNo + "\\n Please log in to your Siyakhokha profile to view the request.";
                                var sms = new CesarSMS();
                                var cxt = new eServicesDbContext();
                                var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                                sms.GenerateSMS(AccountHolderSys.MobileNumber, bodysms.ToString(),
        AccountHolderSys.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, AccountHolderSys.FullName);
                                //SmsHelper.Send(AccountHolderSys.MobileNumber, bodysms.ToString());

                            }
                           
                        }
                        catch (Exception smserr)
                        {
                            return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json(new { isValid = true }, JsonRequestBehavior.AllowGet);

                }
                catch(Exception e)
                {
                    return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                }
           
            }

        }
        #endregion
        #region Reject Request to Access Account Number POST
        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult RejectRequest(string ReqNo, string Comment)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var CurrUser = User.Identity.GetUserName();
                    var customerSys = context.SystemUsers.SingleOrDefault(c => c.UserName == CurrUser);
                    var customer = context.Customers.Include(c => c.CustomerType)
                             .SingleOrDefault(c => c.SystemUserId == customerSys.Id);
                    if (customer == null) throw new Exception("Invalid Customer");
                    Comment Comms = new Comment();
                    string accNo = ReqNo;
                    int ReqID = Convert.ToInt32(ReqNo);
                    Request Req = context.Requests.Find(ReqID);

                    var Requester = context.Customers.Include(c => c.CustomerType)
                     .SingleOrDefault(c => c.Id == Req.CustomerId);
                    var RequesterSys = context.SystemUsers.SingleOrDefault(c => c.Id == Requester.SystemUserId);
                    var contactMeth = "";
                    if (RequesterSys.EmailAddress != null && RequesterSys.EmailAddress != "")
                    {
                        contactMeth = RequesterSys.EmailAddress;
                    }
                    else
                    {
                        contactMeth = RequesterSys.MobileNumber;
                    }
                    var referenceType = (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent || customer.CustomerType.Key == CustomerTypeKeys.Entity)
                                           ? context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.LinkedAccountsManagingAgent)) :
                                           context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccounts);
                    var statusType = context.Status.FirstOrDefault(d => d.Key == StatusKeys.IncentivePolicyApplicationDeclined);
                    //Add Comment
                    Comms.ReferenceAlpha = Req.ReferenceAlpha;
                    Comms.ReferenceTypeId = Req.ReferenceTypeId;
                    Comms.ReferenceNumeric = Req.Id;
                    Comms.IsActive = true;
                    Comms.Comments = Comment;
                    Comms.CreatedBySystemUserId = customerSys.Id;
                    Comms.ModifiedBySystemUserId = customerSys.Id;

                    context.Comments.Add(Comms);

                    //Create Request
                    Req.StatusId = statusType.Id;
                    Req.ModifiedBySystemUserId = RequesterSys.Id;
                    Req.CommentId = Comms.Id;
                    db.Entry(Req).State = EntityState.Modified;

                    context.SaveChanges();
                    //Add Email/sms Notification
                    if (RequesterSys != null)
                    {
                        var rejectRequestEmail = new Email();
                        try
                        {
                            if (RequesterSys.EmailAddress != null && !RequesterSys.EmailAddress.Trim().Equals(string.Empty))
                            {
                                const string subject = "Ekurhuleni Housing Company: Request to access Municipal Account";
                                var body = "Dear " + Requester.FullName + " your request to access the Municipal Account: " + Req.ReferenceAlpha +
                                    " has been rejected by the current Account Holder.<br/>" +
                                    "Please log in to your Siyakhokha profile to view the Account Holders Comment.";
                                rejectRequestEmail.GenerateEmail(RequesterSys.EmailAddress, subject, body, Requester.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, Requester.FullName);

                            }

                            else if (RequesterSys.MobileNumber != null && !RequesterSys.MobileNumber.Trim().Equals(string.Empty))
                            {
                                var bodysms = "Dear " + Requester.FullName + " your request to access the Municipal Account: " + Req.ReferenceAlpha +
                                    " has been rejected by the current Account Holder.\\n" +
                                    "Please log in to your Siyakhokha profile to view the Account Holders Comment.";
                                var sms = new CesarSMS();
                                var cxt = new eServicesDbContext();
                                var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                                sms.GenerateSMS(RequesterSys.MobileNumber, bodysms.ToString(),
        RequesterSys.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, RequesterSys.FullName);
                                //SmsHelper.Send(RequesterSys.MobileNumber, bodysms.ToString());

                            }

                        }
                        catch (Exception smserr)
                        {
                            return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json(new { isValid = true }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                }

            }

        }
        #endregion

        #region Accept Request to Access Account Number POST
        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult AcceptRequest(string ReqNo, string Comment)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var CurrUser = User.Identity.GetUserName();
                    var customerSys = context.SystemUsers.SingleOrDefault(c => c.UserName == CurrUser);
                    var customer = context.Customers.Include(c => c.CustomerType)
                             .SingleOrDefault(c => c.SystemUserId == customerSys.Id);
                    if (customer == null) throw new Exception("Invalid Customer");
                    Comment Comms = new Comment();
                    string accNo = ReqNo;
                    int ReqID = Convert.ToInt32(ReqNo);
                    Request Req = context.Requests.Find(ReqID);

                    var Requester = context.Customers.Include(c => c.CustomerType)
                     .SingleOrDefault(c => c.Id == Req.CustomerId);
                    var RequesterSys = context.SystemUsers.SingleOrDefault(c => c.Id == Requester.SystemUserId);
                    var contactMeth = "";
                    if (RequesterSys.EmailAddress != null && RequesterSys.EmailAddress != "")
                    {
                        contactMeth = RequesterSys.EmailAddress;
                    }
                    else
                    {
                        contactMeth = RequesterSys.MobileNumber;
                    }
                    var referenceType = (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent || customer.CustomerType.Key == CustomerTypeKeys.Entity)
                                           ? context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.LinkedAccountsManagingAgent)) :
                                           context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccounts);
                    var statusType = context.Status.FirstOrDefault(d => d.Key == StatusKeys.IncentivePolicyApplicationApproved);
                    //Add Comment
                    Comms.ReferenceAlpha = Req.ReferenceAlpha;
                    Comms.ReferenceTypeId = Req.ReferenceTypeId;
                    Comms.ReferenceNumeric = Req.Id;
                    Comms.IsActive = true;
                    Comms.Comments = Comment;
                    Comms.CreatedBySystemUserId = customerSys.Id;
                    Comms.ModifiedBySystemUserId = customerSys.Id;

                    context.Comments.Add(Comms);

                    //Create Request
                    Req.StatusId = statusType.Id;
                    Req.ModifiedBySystemUserId = RequesterSys.Id;
                    Req.CommentId = Comms.Id;
                    db.Entry(Req).State = EntityState.Modified;


                    //Create Dual Link to new Profiles
                    var linkedAccount = new LinkedAccount();
                    var emailLink = new Email();
                    Account account= context.Accounts.FirstOrDefault(d => d.AccountNumber== Req.ReferenceAlpha);
                    var recipientType = context.RecipientTypes.FirstOrDefault(d => d.Key == RecepientTypeKeys.ViewOnly);
                    var linkedAccountActive = context.Status.FirstOrDefault(x => x.Key.Equals(StatusKeys.LinkedAccountActive));
                    var linkedAccountType = context.LinkedAccountTypes.FirstOrDefault(la => la.Key == LinkedAccountTypeKeys.Unassociated);
                    linkedAccount.Account = account;
                        linkedAccount.CustomerId = Requester.Id;
                        linkedAccount.RecipientTypeId = recipientType.Id;
                        linkedAccount.StatusId = linkedAccountActive.Id;
                        
                        linkedAccount.LinkedAccountTypeId = linkedAccountType.Id;
                        linkedAccount.IsActive = true;
                        linkedAccount.IsDeleted = false;
                        linkedAccount.IsLocked = false;
                        context.LinkedAccounts.Add(linkedAccount);
                        context.SaveChanges();

                 //send email notification 
                const string subjectLink = "Siyakhokha: Linked Account";
                var bodyLink = string.Empty;
                var notificationLink = new StringBuilder(string.Empty);
                string messageLink = "";
                string emailTemplateLink;
                if (RequesterSys.EmailAddress != null && RequesterSys.EmailAddress != "")
                {
                        emailTemplateLink = AppSettingKeys.LinkedAccountsAssociatedEmailTemplate;
                        var activeMessage =
                            context.AppSettings.FirstOrDefault(
                                a => a.Key == AppSettingKeys.LinkedAccountsActiveMessage);
                        if (activeMessage == null) throw new Exception("Invalid appsetting");

                        messageLink = activeMessage.Value;

                    emailLink.GenerateEmail(RequesterSys.EmailAddress, subjectLink, bodyLink, Requester.Id.ToString(CultureInfo.InvariantCulture), false, emailTemplateLink, recipientName: Requester.FullName, referenceNumber: linkedAccount.Account.AccountNumber);

                }
                else if (RequesterSys.MobileNumber != null || RequesterSys.MobileNumber != "")
                {
                        var bodysms = " Dear " + Requester.FullName + "\\n" +
                           "Account: " + linkedAccount.Account.AccountNumber + " is now successfully linked to your profile.\\n" +
                           "Regards, \\nSiyakhokha Team";
                        var sms = new CesarSMS();
                        var cxt = new eServicesDbContext();
                        var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                        sms.GenerateSMS(RequesterSys.MobileNumber, bodysms.ToString(),
RequesterSys.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, RequesterSys.FullName);
                        //SmsHelper.Send(RequesterSys.MobileNumber, bodysms.ToString());
                    }

                //ModelState.Clear();

                    //Add Email/sms Notification
                    if (RequesterSys != null)
                    {
                        var acceptRequestEmail = new Email();
                        try
                        {
                            if (RequesterSys.EmailAddress != null && !RequesterSys.EmailAddress.Trim().Equals(string.Empty))
                            {
                                const string subject = "Ekurhuleni Housing Company: Request to access Municipal Account";
                                var body = "Dear " + Requester.FullName + " your request to access the Municipal Account: " + Req.ReferenceAlpha +
                                    " has been approved by the current Account Holder.<br/> The requested account has been linked to your Siyakhokha Profile.<br/>" +
                                    "Please log in to your Siyakhokha profile to view the new Municipal Account.";
                                acceptRequestEmail.GenerateEmail(RequesterSys.EmailAddress, subject, body, Requester.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, Requester.FullName);

                            }

                            else if (RequesterSys.MobileNumber != null && !RequesterSys.MobileNumber.Trim().Equals(string.Empty))
                            {
                                var bodysms = "Dear " + Requester.FullName + " your request to access the Municipal Account: " + Req.ReferenceAlpha +
                                    " has been approved by the current Account Holder.\\n The requested account has been linked to your Siyakhokha Profile.\\n" +
                                    "Please log in to your Siyakhokha profile to view the new Municipal Account.";
                                var sms = new CesarSMS();
                                var cxt = new eServicesDbContext();
                                var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                                sms.GenerateSMS(RequesterSys.MobileNumber, bodysms.ToString(),
        RequesterSys.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, RequesterSys.FullName);

                                //SmsHelper.Send(RequesterSys.MobileNumber, bodysms.ToString());

                            }

                        }
                        catch (Exception smserr)
                        {
                            return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json(new { isValid = true }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                }

            }

        }
        #endregion
    }
}
