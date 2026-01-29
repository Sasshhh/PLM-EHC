using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Keys;
using System.Globalization;

namespace C8.eServices.Mvc.Controllers
{
    public class EmailQueueProcessedsController : Controller
    {
        private CesarDbContext db = new CesarDbContext();
        private CesarBulkDBContext dbBulk = new CesarBulkDBContext();

        // GET: EmailQueueProcesseds
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            //Sets Failure Count based on Cesar Appsettings
            var settingsgetaccount = db.EmailAccounts.Where(x => x.EmailAccountId == 5).FirstOrDefault();
            ViewBag.failurecounter = settingsgetaccount.MaxFailureCount;
              
            return View(db.EmailQueueProcessed.ToList());
        }
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Resend(int? id)
        {
            var failedemail = db.EmailQueueProcessed.Where(e => e.EmailQueueId == id).ToList().LastOrDefault();
            EmailQueueItem emailqitem = new EmailQueueItem();

            emailqitem.ApplicationId = failedemail.ApplicationId;
            emailqitem.EmailAccountId = failedemail.EmailAccountId;
            emailqitem.ToList = failedemail.ToList;
            emailqitem.CcList = failedemail.CcList;
            emailqitem.BccList = failedemail.BccList;
            emailqitem.Subject = failedemail.Subject;
            emailqitem.Body = failedemail.Body;
            emailqitem.IsHtml = failedemail.IsHtml;
            emailqitem.ReferenceId = failedemail.ReferenceId;
            emailqitem.FailureCount = 0;
            emailqitem.HasAttachments = failedemail.HasAttachments;

            DateTime Today = DateTime.Now;
            emailqitem.QueueDateTime = Today;



            db.EmailQueue.Add(emailqitem);
            db.SaveChanges();
            return RedirectToAction("ViewEmailNotifications");
        }


        #region All Email Notifications GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ViewEmailNotifications()
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

        #region Load All Emails GET

        public JsonResult LoadEmailNotifications(int pageSize, string startDate, string endDate, int pageNumber, string searchText)
        {
            var total = 0;
            var emails = new List<EmailQueueProcessed>();
            var rows = new List<object>();
            var skip = (pageNumber - 1) * pageSize;
            const string goLiveDate = "2019/07/01";
            var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            try
            {
                using (var context = new CesarDbContext())
                {
                    //Gets system users with no customer profile o=>o.CreatedDateTime>golive
                    // var currentDebitOrders = cxt.DebitOrders.Include(o => o.Account).Include(o => o.BankAccount).Include(o => o.BankAccount.Bank).Include(o => o.Status).Include(o => o.Customer).Where(o=>o.CreatedDateTime>golive).ToList();
                    var emailsModel = context.EmailQueueProcessed.Where(s => (DbFunctions.TruncateTime(s.QueueDateTime) >= startDateTime && DbFunctions.TruncateTime(s.QueueDateTime) <= endDateTime));
                    var settingsgetaccount = db.EmailAccounts.Where(x => x.EmailAccountId == 5).FirstOrDefault();

                    //Filter
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        emails = emailsModel.Where(s => (s.QueueDateTime.ToString() ?? "").Contains(searchText) ||
                                                                 (s.Subject?? "").Contains(searchText) ||
                                                                 (s.ToList ?? "").Contains(searchText) ||
                                                                 (s.ProcessedDateTime.ToString() ?? "").Contains(searchText)).ToList();

                        emails = emails.OrderBy(l => l.EmailQueueId).Skip(skip).Take(pageSize).ToList();
                    }
                    else
                    {
                        emails = emailsModel.OrderBy(l => l.EmailQueueId).Skip(skip).Take(pageSize).ToList();
                        total = emailsModel.Count();
                    }

                    foreach (var emailitem in emails)
                    {
                        //var doPay = UserManager.FindByName(debitOrder.Customer.SystemUser.UserName);//Id = debitOrder.Id,
                        var statusCount = "Success";
                        if(emailitem.FailureCount == settingsgetaccount.MaxFailureCount || emailitem.FailureCount >settingsgetaccount.MaxFailureCount)
                        {
                            statusCount = "Failed";
                        }
                        rows.Add(new
                        {
                            queueDate = String.Format("{0:yyyy/MM/dd}", emailitem.QueueDateTime),
                            toList = emailitem.ToList,
                            subjectEmail = emailitem.Subject,
                            processedDate = String.Format("{0:yyyy/MM/dd}", emailitem.ProcessedDateTime),
                            status = statusCount,
                            emQID=emailitem.EmailQueueId

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
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ResendStatement(int? id)
        {
            var failedemail = dbBulk.EmailQueueProcessedBulk.Where(e => e.EmailQueueId == id).ToList().LastOrDefault();
            EmailQueueItemBulk emailqitem = new EmailQueueItemBulk();

            emailqitem.ApplicationId = failedemail.ApplicationId;
            emailqitem.EmailAccountId = failedemail.EmailAccountId;
            emailqitem.ToList = failedemail.ToList;
            emailqitem.CcList = failedemail.CcList;
            emailqitem.BccList = failedemail.BccList;
            emailqitem.Subject = failedemail.Subject;
            emailqitem.Body = failedemail.Body;
            emailqitem.IsHtml = failedemail.IsHtml;
            emailqitem.ReferenceId = failedemail.ReferenceId;
            emailqitem.FailureCount = 0;
            emailqitem.HasAttachments = failedemail.HasAttachments;

            DateTime Today = DateTime.Now;
            emailqitem.QueueDateTime = Today;



            dbBulk.EmailQueueBulk.Add(emailqitem);
            dbBulk.SaveChanges();
            return RedirectToAction("ViewEmailStatementNotifications");
        }
        #region All Email Statement Notifications GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ViewEmailStatementNotifications()
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

        #region Load All Emails GET

        public JsonResult LoadEmailStatementNotifications(int pageSize, string startDate, string endDate, int pageNumber, string searchText)
        {
            var total = 0;
            var emails = new List<EmailQueueProcessedBulk>();
            var rows = new List<object>();
            var skip = (pageNumber - 1) * pageSize;
            const string goLiveDate = "2019/07/01";
            var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            try
            {
                using (var context = new CesarBulkDBContext())
                {
                    //Gets system users with no customer profile o=>o.CreatedDateTime>golive
                    // var currentDebitOrders = cxt.DebitOrders.Include(o => o.Account).Include(o => o.BankAccount).Include(o => o.BankAccount.Bank).Include(o => o.Status).Include(o => o.Customer).Where(o=>o.CreatedDateTime>golive).ToList();
                    var emailsModel = context.EmailQueueProcessedBulk.Where(s => (DbFunctions.TruncateTime(s.QueueDateTime) >= startDateTime && DbFunctions.TruncateTime(s.QueueDateTime) <= endDateTime));
                    var settingsgetaccount = db.EmailAccounts.Where(x => x.EmailAccountId == 5).FirstOrDefault();

                    //Filter
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        emails = emailsModel.Where(s => (s.QueueDateTime.ToString() ?? "").Contains(searchText) ||
                                                                 (s.Subject ?? "").Contains(searchText) ||
                                                                 (s.ToList ?? "").Contains(searchText) ||
                                                                 (s.ProcessedDateTime.ToString() ?? "").Contains(searchText)).ToList();

                        emails = emails.OrderBy(l => l.EmailQueueId).Skip(skip).Take(pageSize).ToList();
                    }
                    else
                    {
                        emails = emailsModel.OrderBy(l => l.EmailQueueId).Skip(skip).Take(pageSize).ToList();
                        total = emailsModel.Count();
                    }

                    foreach (var emailitem in emails)
                    {
                        //var doPay = UserManager.FindByName(debitOrder.Customer.SystemUser.UserName);//Id = debitOrder.Id,
                        var statusCount = "Success";
                        if (emailitem.FailureCount == settingsgetaccount.MaxFailureCount || emailitem.FailureCount > settingsgetaccount.MaxFailureCount)
                        {
                            statusCount = "Failed";
                        }
                        rows.Add(new
                        {
                            queueDate = String.Format("{0:yyyy/MM/dd}", emailitem.QueueDateTime),
                            toList = emailitem.ToList,
                            subjectEmail = emailitem.Subject,
                            processedDate = String.Format("{0:yyyy/MM/dd}", emailitem.ProcessedDateTime),
                            status = statusCount,
                            emQID = emailitem.EmailQueueId

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

        // GET: EmailQueueProcesseds/Details/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailQueueProcessed emailQueueProcessed = db.EmailQueueProcessed.Find(id);
            if (emailQueueProcessed == null)
            {
                return HttpNotFound();
            }
            return View(emailQueueProcessed);
        }

        // GET: EmailQueueProcesseds/Create
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmailQueueProcesseds/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EmailQueueId,ApplicationId,EmailAccountId,QueueDateTime,ToList,CcList,BccList,Subject,Body,IsHtml,FailureCount,ReferenceId,ReferenceTypeId,HasAttachments,ProcessedDateTime")] EmailQueueProcessed emailQueueProcessed)
        {
            if (ModelState.IsValid)
            {
                db.EmailQueueProcessed.Add(emailQueueProcessed);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(emailQueueProcessed);
        }

        // GET: EmailQueueProcesseds/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailQueueProcessed emailQueueProcessed = db.EmailQueueProcessed.Find(id);
            if (emailQueueProcessed == null)
            {
                return HttpNotFound();
            }
            return View(emailQueueProcessed);
        }

        // POST: EmailQueueProcesseds/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EmailQueueId,ApplicationId,EmailAccountId,QueueDateTime,ToList,CcList,BccList,Subject,Body,IsHtml,FailureCount,ReferenceId,ReferenceTypeId,HasAttachments,ProcessedDateTime")] EmailQueueProcessed emailQueueProcessed)
        {
            if (ModelState.IsValid)
            {
                db.Entry(emailQueueProcessed).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(emailQueueProcessed);
        }

        // GET: EmailQueueProcesseds/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailQueueProcessed emailQueueProcessed = db.EmailQueueProcessed.Find(id);
            if (emailQueueProcessed == null)
            {
                return HttpNotFound();
            }
            return View(emailQueueProcessed);
        }

        // POST: EmailQueueProcesseds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EmailQueueProcessed emailQueueProcessed = db.EmailQueueProcessed.Find(id);
            db.EmailQueueProcessed.Remove(emailQueueProcessed);
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
    }
}
