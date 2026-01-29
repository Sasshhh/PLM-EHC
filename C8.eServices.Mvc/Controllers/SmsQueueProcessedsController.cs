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
    public class SmsQueueProcessedsController:Controller
    {
        private CesarDbContext db = new CesarDbContext();
        private CesarBulkDBContext dbBulk = new CesarBulkDBContext();
        private eServicesDbContext context = new eServicesDbContext();


        // GET: EmailQueueProcesseds
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            //Sets Failure Count based on Cesar Appsettings
            var settingsgetaccount = db.EmailAccounts.Where(x => x.EmailAccountId == 5).FirstOrDefault();
            ViewBag.failurecounter = settingsgetaccount.MaxFailureCount;

            return View(db.SmsQueueProcessed.ToList());
        }
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Resend(int? id)
        {
            var failedsms = db.SmsQueueProcessed.Where(e => e.SmsQueueId == id).ToList().LastOrDefault();
            SmsQueueItem Smsqitem = new SmsQueueItem();
            var statusIdsms = context.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
            Smsqitem.ApplicationId = failedsms.ApplicationId;
            Smsqitem.SmsAccountId = failedsms.SmsAccountId;
            Smsqitem.MobileNumber = failedsms.MobileNumber;
            Smsqitem.TextMessage = failedsms.TextMessage;
            Smsqitem.FailureCount = 0;
            Smsqitem.ReferenceId = failedsms.ReferenceId;
            Smsqitem.StatusId = statusIdsms;

            DateTime Today = DateTime.Now;
            Smsqitem.QueueDateTime = Today;



            db.SmsQueue.Add(Smsqitem);
            db.SaveChanges();
            return RedirectToAction("ViewSmsNotifications");
        }
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ResendStatementSms(int? id)
        {
            var failedsms = dbBulk.SmsQueueProcessedBulk.Where(e => e.SmsQueueId == id).ToList().LastOrDefault();
            SmsQueueItemBulk Smsqitem = new SmsQueueItemBulk();
            var statusIdsms = context.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
            Smsqitem.ApplicationId = failedsms.ApplicationId;
            Smsqitem.SmsAccountId = failedsms.SmsAccountId;
            Smsqitem.MobileNumber = failedsms.MobileNumber;
            Smsqitem.TextMessage = failedsms.TextMessage;
            Smsqitem.FailureCount = 0;
            Smsqitem.ReferenceId = failedsms.ReferenceId;
            Smsqitem.StatusId = statusIdsms;

            DateTime Today = DateTime.Now;
            Smsqitem.QueueDateTime = Today;



            dbBulk.SmsQueueBulk.Add(Smsqitem);
            dbBulk.SaveChanges();
            return RedirectToAction("ViewSmsNotifications");
        }
        #region All SMS Notifications GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ViewSmsNotifications()
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

        #region Load All Sms GET

        public JsonResult LoadSmsNotifications(int pageSize, string startDate, string endDate, int pageNumber, string searchText)
        {
            var total = 0;
            var smss = new List<SmsQueueProcessed>();
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
                    var smssModel = context.SmsQueueProcessed.Where(s => (DbFunctions.TruncateTime(s.QueueDateTime) >= startDateTime && DbFunctions.TruncateTime(s.QueueDateTime) <= endDateTime));
                    var settingsgetaccount = db.EmailAccounts.Where(x => x.EmailAccountId == 5).FirstOrDefault();

                    //Filter
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        smss = smssModel.Where(s => (s.QueueDateTime.ToString() ?? "").Contains(searchText) ||
                                                                 (s.TextMessage ?? "").Contains(searchText) ||
                                                                 (s.MobileNumber ?? "").Contains(searchText) ||
                                                                 (s.ProcessedDateTime.ToString() ?? "").Contains(searchText)).ToList();

                        smss = smss.OrderBy(l => l.SmsQueueId).Skip(skip).Take(pageSize).ToList();
                    }
                    else
                    {
                        smss = smssModel.OrderBy(l => l.SmsQueueId).Skip(skip).Take(pageSize).ToList();
                        total = smssModel.Count();
                    }

                    foreach (var smsitem in smss)
                    {
                        //var doPay = UserManager.FindByName(debitOrder.Customer.SystemUser.UserName);//Id = debitOrder.Id,
                        var statusCount = "Success";
                        if (smsitem.FailureCount == settingsgetaccount.MaxFailureCount || smsitem.FailureCount > settingsgetaccount.MaxFailureCount)
                        {
                            statusCount = "Failed";
                        }
                        rows.Add(new
                        {
                            queueDate = String.Format("{0:yyyy/MM/dd}", smsitem.QueueDateTime),
                            toList = smsitem.MobileNumber,
                            subjectEmail = smsitem.TextMessage,
                            processedDate = String.Format("{0:yyyy/MM/dd}", smsitem.ProcessedDateTime),
                            status = statusCount,
                            smsQID=smsitem.SmsQueueId

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
        #region All SMS Statement Notifications GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ViewSmsStatementNotifications()
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

        #region Load All Sms GET

        public JsonResult LoadSmsStatementNotifications(int pageSize, string startDate, string endDate, int pageNumber, string searchText)
        {
            var total = 0;
            var smss = new List<SmsQueueProcessedBulk>();
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
                    var smssModel = context.SmsQueueProcessedBulk.Where(s => (DbFunctions.TruncateTime(s.QueueDateTime) >= startDateTime && DbFunctions.TruncateTime(s.QueueDateTime) <= endDateTime));
                    var settingsgetaccount = db.EmailAccounts.Where(x => x.EmailAccountId == 5).FirstOrDefault();

                    //Filter
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        smss = smssModel.Where(s => (s.QueueDateTime.ToString() ?? "").Contains(searchText) ||
                                                                 (s.TextMessage ?? "").Contains(searchText) ||
                                                                 (s.MobileNumber ?? "").Contains(searchText) ||
                                                                 (s.ProcessedDateTime.ToString() ?? "").Contains(searchText)).ToList();

                        smss = smss.OrderBy(l => l.SmsQueueId).Skip(skip).Take(pageSize).ToList();
                    }
                    else
                    {
                        smss = smssModel.OrderBy(l => l.SmsQueueId).Skip(skip).Take(pageSize).ToList();
                        total = smssModel.Count();
                    }

                    foreach (var smsitem in smss)
                    {
                        //var doPay = UserManager.FindByName(debitOrder.Customer.SystemUser.UserName);//Id = debitOrder.Id,
                        var statusCount = "Success";
                        if (smsitem.FailureCount == settingsgetaccount.MaxFailureCount || smsitem.FailureCount > settingsgetaccount.MaxFailureCount)
                        {
                            statusCount = "Failed";
                        }
                        rows.Add(new
                        {
                            queueDate = String.Format("{0:yyyy/MM/dd}", smsitem.QueueDateTime),
                            toList = smsitem.MobileNumber,
                            subjectEmail = smsitem.TextMessage,
                            processedDate = String.Format("{0:yyyy/MM/dd}", smsitem.ProcessedDateTime),
                            status = statusCount,
                            smsQID = smsitem.SmsQueueId

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
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
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
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
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
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
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
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
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
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
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
