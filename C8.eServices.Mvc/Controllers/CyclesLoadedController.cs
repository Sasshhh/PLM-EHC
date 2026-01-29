using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Controllers
{
    public class CyclesLoadedController : Controller
    {
        private BillingDbContext db = new BillingDbContext();

        // GET: CyclesLoaded
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            string currentYear = DateTime.Now.Year.ToString();
            int currentMonth = (DateTime.Now.Month);
            string currMonthSub = "";
            if(currentMonth<10)
            {
                currMonthSub = currentYear.ToString()+"/0" + currentMonth.ToString();
            }
            else
            {
                currMonthSub = currentYear.ToString() +"/"+currentMonth.ToString();
            }
           // var currentMontCycles = db.BillInfos.Where(o => o.RunDate.Contains(currMonthSub)).Distinct().ToList();
            var currentMontCyclesDist = db.BillInfos.GroupBy(x => new { x.RunDate, x.CycleNo }).Select(x => x.FirstOrDefault()).Where(o => o.RunDate.Contains(currMonthSub)).OrderByDescending(x=>x.RunDate).ToList();

            return View(currentMontCyclesDist);
        }

        #region All Loaded Statement Cycles GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ViewCyclesLoaded()
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

        #region Load All statement Cycles GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult LoadCycles(int pageSize, string startDate, string endDate, int pageNumber, string searchText)
        {
            var total = 0;
            var cyclesLoaded = new List<BillInfo>();
            var rows = new List<object>();
            var skip = (pageNumber - 1) * pageSize;
           
            string goLiveDate = DateTime.Now.AddMonths(-1).ToString("yyyy/MM/01");
            var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            var startDateT = startDateTime.ToString("MM");
            var endDateT = endDateTime.ToString("MM");
            try
            {
                using (var context = new BillingDbContext())
                {
                    //Gets system users with no customer profile o=>o.CreatedDateTime>golive
                    // var currentDebitOrders = cxt.DebitOrders.Include(o => o.Account).Include(o => o.BankAccount).Include(o => o.BankAccount.Bank).Include(o => o.Status).Include(o => o.Customer).Where(o=>o.CreatedDateTime>golive).ToList();
                    var cyclesLoadedModel = db.BillInfos.ToList().Where(x => (DateTime.ParseExact(x.RunDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) >= startDateTime && DateTime.ParseExact(x.RunDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) <= endDateTime)).ToList().GroupBy(x => new { x.RunDate, x.CycleNo }).Select(x => x.FirstOrDefault()).OrderByDescending(x => x.RunDate).ToList();



                    //Filter
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        cyclesLoaded = cyclesLoadedModel.Where(s => (s.CycleNo.ToString() ?? "").Contains(searchText) ||
                                                                 (s.RunDate ?? "").Contains(searchText) ||
                                                                 (s.InvDate ?? "").Contains(searchText) ||
                                                                 (s.InvMonth ?? "").Contains(searchText)).ToList();

                        cyclesLoaded = cyclesLoaded.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                    }
                    else
                    {
                        cyclesLoaded = cyclesLoadedModel.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                        total = cyclesLoadedModel.Count();
                    }

                    foreach (var cycleLoaded in cyclesLoaded)
                    {
                        //var doPay = UserManager.FindByName(debitOrder.Customer.SystemUser.UserName);//Id = debitOrder.Id,
                        rows.Add(new
                        {

                            cycleNo = cycleLoaded.CycleNo,
                            runDate = cycleLoaded.RunDate,
                            invDate = cycleLoaded.InvDate,
                            stateMonth = cycleLoaded.InvMonth

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

        // GET: CyclesLoaded/Details/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillInfo billInfo = db.BillInfos.Find(id);
            if (billInfo == null)
            {
                return HttpNotFound();
            }
            return View(billInfo);
        }

        // GET: CyclesLoaded/Create
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: CyclesLoaded/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create([Bind(Include = "Id,AccountNumber,DueDate,RunDate,Total,CycleNo,InvDate")] BillInfo billInfo)
        {
            if (ModelState.IsValid)
            {
                db.BillInfos.Add(billInfo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(billInfo);
        }

        // GET: CyclesLoaded/Edit/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillInfo billInfo = db.BillInfos.Find(id);
            if (billInfo == null)
            {
                return HttpNotFound();
            }
            return View(billInfo);
        }

        // POST: CyclesLoaded/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit([Bind(Include = "Id,AccountNumber,DueDate,RunDate,Total,CycleNo,InvDate")] BillInfo billInfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(billInfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(billInfo);
        }

        // GET: CyclesLoaded/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillInfo billInfo = db.BillInfos.Find(id);
            if (billInfo == null)
            {
                return HttpNotFound();
            }
            return View(billInfo);
        }

        // POST: CyclesLoaded/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteConfirmed(string id)
        {
            BillInfo billInfo = db.BillInfos.Find(id);
            db.BillInfos.Remove(billInfo);
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
