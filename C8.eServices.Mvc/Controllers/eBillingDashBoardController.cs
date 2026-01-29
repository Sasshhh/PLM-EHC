using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Linq.Dynamic;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.DataAccessLayer.eBilling6Months;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Statistic = C8.eServices.Mvc.DataAccessLayer.eBilling6Months.Statistic;

namespace C8.eServices.Mvc.Controllers
{
    public class Stats
    {
        public string Pending { get; set; }
        public string Completed { get; set; }
        public string Errors { get; set; }
        public string Date { get; set; }
    }

    public class LinkedAccountStats
    {
        public DateTime CreatedDateTime { get; set; }
        public int RecievedLinkedAccounts { get; set; }
        public int ApprovedLinkedAccounts { get; set; }
        public int PendingDocumentLinkedAccounts { get; set; }
        public int PendingLinkedAccounts { get; set; }

    }
    public class StatsVm : eServices.Mvc.DataAccessLayer.eBilling6Months.Statistic
    {
        public string DateString { get; set; }

        public int? Accounts { get; set; }

        public int? EmailAccounts { get; set; }

        public int? Errors { get; set; }

        public int? CompletedMails { get; set; }

        public int? ExpectedAccounts { get; set; }

        public StatsVm(Statistic s)
        {
            DateString = s.BillDate.ToString();
            Accounts = s.Accounts ?? 0;
            EmailAccounts = s.EmailAccounts ?? 0;
            Errors = s.Errors ?? 0;
            CompletedMails = s.CompletedEmails ?? 0;
            ExpectedAccounts = s.ExpectedAccounts ?? 0;
        }

    }


    public class CompleteStats
    {
        public string Date { get; set; }

        public int? Accounts { get; set; }

        public int? EmailAccounts { get; set; }

        public int? Errors { get; set; }

        public int? CompletedMails { get; set; }
    }

    public class eBillingDashBoardController : Controller
    {
        private eBilling6Months dc = new eBilling6Months();


        // GET: eBillingDashBoard
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Statistics()
        {
            return View();
        }

        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LinkedAccountStatistics()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult Pie()
        {
            StatsVm s = new StatsVm(dc.Statistics.FirstOrDefault(d => d.BillDate == dc.Statistics.Max(x => x.BillDate)));
            return Json(s,
                JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult Stats()
        {
            var datelList = dc.Statistics.ToList();
            var aList = datelList.OrderByDescending(x => x.Id).Select(d => new CompleteStats()
            {
                Date = d.BillDate.ToString(),
                Accounts = d.Accounts,
                EmailAccounts = d.EmailAccounts,
                CompletedMails = d.CompletedEmails,
                Errors = d.Errors ?? 0
            }).Take(30).ToList();

            aList.Reverse();
            return Json(aList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult BarStats()
        {
            var barDateList = dc.Statistics.OrderByDescending(x => x.Id).Select(a => a.BillDate.ToString()).Take(7);
            var accoutnsList = dc.Statistics.OrderByDescending(x => x.Id).Select(a => a.Accounts).Take(7);
            var emailAccountsList = dc.Statistics.OrderByDescending(x => x.Id).Select(e => e.EmailAccounts).Take(7);
            var completedMailsList = dc.Statistics.OrderByDescending(x => x.Id).Select(c => c.CompletedEmails).Take(7);
            var errorList = dc.Statistics.OrderByDescending(x => x.Id).Select(e => e.Errors ?? 0).Take(7).Take(7);


            var barChart = new
            {
                dates = barDateList,
                accounts = accoutnsList.ToList(),
                emailAccounts = emailAccountsList.ToList(),
                completedMails = completedMailsList.ToList(),
                errors = errorList.ToList(),
            };
            return Json(barChart, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Errors(int id = 0)
        {
            ViewBag.Id = id;
            return View();
        }

        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Pending()
        {
            return View();
        }

        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Archive()
        {
            return View();
        }

        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public Stats DailyStats()
        {
            var s = new Stats();

            s.Pending = dc.QueuePendings.Count().ToString();

            var latestStats = dc.Statistics.OrderByDescending(x => x.Id).FirstOrDefault();

            s.Completed = latestStats != null && latestStats.CompletedEmails == null
                ? "0"
                : latestStats.CompletedEmails.ToString();

            s.Errors = latestStats.Errors == null ? "0" : latestStats.Errors.ToString();

            s.Date = latestStats.BillDate.Date.ToShortDateString();
            return s;
        }

        public JsonResult LinkedAccountStats()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var time1 = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
            var time2 = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var stats = new List<LinkedAccountStats>();
            try
            {
                using (var _eservices = new eServicesDbContext())
                {
                    stats.AddRange(_eservices.Database.SqlQuery<LinkedAccountStats>(
                                    "pr_REPORT_LinkedAccountStatisticsForDay").ToList());
                    stats = FilterByDateRange(stats, time1, time2);
                }

            }
            catch (Exception ex)
            {
                SecurityHelper.LogError(ex, "Error");
            }
            totalRecords = stats.Count();

            var data = stats.Skip(skip).Take(pageSize).ToList();
            return Json(
                new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data },
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Filter Linked Account Statistics
        /// </summary>
        /// <param name="stats">Statistics</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<LinkedAccountStats> FilterByDateRange(List<LinkedAccountStats> stats, string start, string end)
        {

            if ((!string.IsNullOrEmpty(start)) && (start != "Select Date"))
            {
                var d = Convert.ToDateTime(start);
                stats = stats.Where(a => a.CreatedDateTime >= d).ToList();
            }

            if ((!string.IsNullOrEmpty(end)) && (end != "Select Date"))
            {
                var d = Convert.ToDateTime(end);
                stats = stats.Where(a => a.CreatedDateTime <= d).ToList();
            }

            return stats;
        }

        [HttpGet]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult RefreshStats()
        {
            return Json(DailyStats(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult loaddata()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns value

            var sortColumn =
                Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() +
                                       "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var accountNumber = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();

            var customerName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();

            var emailAddress = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();


            var time1 = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();

            var time2 = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            using (eBilling6Months dc = new eBilling6Months())
            {
                var v = (from a in dc.QueuePendings select a);

                if (!string.IsNullOrEmpty(accountNumber))
                {
                    var d = Convert.ToDecimal(accountNumber);
                    v = v.Where(a => a.AccountNumber == d);
                }

                if (!string.IsNullOrEmpty(emailAddress))
                {
                    v = v.Where(a => a.EmailAddressTo.Contains(emailAddress));
                }

                if (!string.IsNullOrEmpty(customerName))
                {
                    v = v.Where(a => a.CustomerAccountName.Contains(customerName));
                }

                if ((!string.IsNullOrEmpty(time1)) && (time1 != "Select Date"))
                {
                    var d = Convert.ToDateTime(time1);
                    v = v.Where(a => a.BillDate.Value >= d);
                }

                if ((!string.IsNullOrEmpty(time2)) && (time2 != "Select Date"))
                {
                    var d = Convert.ToDateTime(time2);
                    v = v.Where(a => a.BillDate.Value <= d);
                }

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.OrderBy(sortColumn + " " + sortColumnDir);
                }

                sortColumn = "BillDate";

                sortColumnDir = "desc";

                v = v.OrderBy(sortColumn + " " + sortColumnDir);

                totalRecords = v.Count();

                var data = v.Skip(skip).Take(pageSize).ToList();
                return Json(
                    new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data },
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult loadErrors(int id = 0)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;


            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();

            if (id != -1)
            {
                var singleOrDefault = dc.Statistics.SingleOrDefault(x => x.Id == id);

                if (singleOrDefault != null)
                {
                    startDate = singleOrDefault.BillDate.Date;
                }

                endDate = startDate.AddDays(1);
            }
            using (eBilling6Months d = new eBilling6Months())
            {

                {
                    var v = from a in d.SystemErrors
                            join e in d.QueueArchives on a.QueueId equals e.Id
                            where e.BillDate == startDate
                            select new
                            {
                                a.Id,
                                e.BillDate,
                                a.Details,
                                e.AccountNumber,
                                e.CustomerAccountName,
                                e.EmailSubject,
                                e.EmailAddressTo
                            };


                    //v = v.Union(c);

                    v = v.OrderBy("AccountNumber");
                    totalRecords = v.Count();

                    var data = v.Skip(skip).Take(pageSize).ToList();
                    return
                        Json(
                            new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data },
                            JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult loadErrorsAll(int id = 0)
        {
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();

            if (id != 0)
            {
                var singleOrDefault = dc.Statistics.SingleOrDefault(x => x.Id == id);

                if (singleOrDefault != null)
                {
                    startDate = singleOrDefault.BillDate.Date;
                }
            }
            endDate = startDate;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn =
                Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() +
                                       "][name]").FirstOrDefault();

            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var accountNumber = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();

            var customerName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();

            var emailAddress = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();

            var time1 = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();

            var time2 = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();


            if (customerName != "" || accountNumber != "" || emailAddress != "" || time1 != "" || time2 != "")
            {
                id = 0;
            }


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            using (eBilling6Months dc = new eBilling6Months())
            {
                var v = (from a in dc.QueueArchives
                         join e in dc.SystemErrors on a.Id equals e.QueueId
                         select new
                         {
                             a.Id,
                             a.BillDate,
                             e.Details,
                             e.QueueId,
                             a.AccountNumber,
                             a.CustomerAccountName,
                             a.EmailSubject,
                             a.EmailAddressTo
                         }).Union(
                               (from a in dc.QueuePendings
                                join e in dc.SystemErrors on a.Id equals e.QueueId
                                select new
                                {
                                    a.Id,
                                    a.BillDate,
                                    e.Details,
                                    e.QueueId,
                                    a.AccountNumber,
                                    a.CustomerAccountName,
                                    a.EmailSubject,
                                    a.EmailAddressTo
                                }));


                if (!string.IsNullOrEmpty(accountNumber))
                {
                    var d = Convert.ToDecimal(accountNumber);
                    v = v.Where(a => a.AccountNumber == d);
                }

                if (!string.IsNullOrEmpty(emailAddress))
                {
                    v = v.Where(a => a.EmailAddressTo.Contains(emailAddress));
                }

                if (!string.IsNullOrEmpty(customerName))
                {
                    v = v.Where(a => a.CustomerAccountName.Contains(customerName));
                }

                if ((!string.IsNullOrEmpty(time1)) && (time1 != "Select Date"))
                {
                    var d = Convert.ToDateTime(time1);
                    v = v.Where(a => a.BillDate.Value >= d);
                }

                if ((!string.IsNullOrEmpty(time2)) && (time2 != "Select Date"))
                {
                    var d = Convert.ToDateTime(time2);
                    v = v.Where(a => a.BillDate.Value <= d);
                }

                if (id != 0)
                {
                    v = v.Where(a => a.BillDate.Value == startDate);
                }

                sortColumn = "BillDate";

                sortColumnDir = "desc";

                v = v.OrderBy(sortColumn + " " + sortColumnDir);

                totalRecords = v.Count();

                var data = v.Skip(skip).Take(pageSize).ToList();
                return Json(
                    new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data },
                    JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult loaddataArchive()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn =
                Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() +
                                       "][name]").FirstOrDefault();

            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var accountNumber = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();

            var customerName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();

            var emailAddress = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();


            var time1 = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();

            var time2 = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            using (eBilling6Months dc = new eBilling6Months())
            {
                var v = (from a in dc.QueueArchives select a);

                if (!string.IsNullOrEmpty(accountNumber))
                {
                    var d = Convert.ToDecimal(accountNumber);
                    v = v.Where(a => a.AccountNumber == d);
                }

                if (!string.IsNullOrEmpty(emailAddress))
                {
                    v = v.Where(a => a.EmailAddressTo.Contains(emailAddress));
                }

                if (!string.IsNullOrEmpty(customerName))
                {
                    v = v.Where(a => a.CustomerAccountName.Contains(customerName));
                }

                if ((!string.IsNullOrEmpty(time1)) && (time1 != "Select Date"))
                {
                    var d = Convert.ToDateTime(time1);
                    v = v.Where(a => a.BillDate.Value >= d);
                }

                if ((!string.IsNullOrEmpty(time2)) && (time2 != "Select Date"))
                {
                    var d = Convert.ToDateTime(time2);
                    v = v.Where(a => a.BillDate.Value <= d);
                }


                sortColumn = "BillDate";

                sortColumnDir = "desc";

                v = v.OrderBy(sortColumn + " " + sortColumnDir);

                totalRecords = v.Count();

                var data = v.Skip(skip).Take(pageSize).ToList();
                return Json(
                    new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data },
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult loaddataStats(string nId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns value


            //var sortColumn = Request.Form.GetValues("order[0][column]").FirstOrDefault();
            var sortColumn =
                Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() +
                                       "][name]").FirstOrDefault();

            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var accountNumber = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();

            var customerName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();

            var emailAddress = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();


            var time1 = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();

            var time2 = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            using (eBilling6Months dc = new eBilling6Months())
            {
                var v = (from a in dc.Statistics select a);


                if ((!string.IsNullOrEmpty(time1)) && (time1 != "Select Date"))
                {
                    var d = Convert.ToDateTime(time1);
                    v = v.Where(a => a.BillDate >= d);
                }

                if ((!string.IsNullOrEmpty(time2)) && (time2 != "Select Date"))
                {
                    var d = Convert.ToDateTime(time2);
                    v = v.Where(a => a.BillDate <= d);
                }

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    sortColumnDir = "desc";
                    v = v.OrderBy(sortColumn + " " + sortColumnDir);
                }


                totalRecords = v.Count();

                var data = v.Skip(skip).Take(pageSize).ToList();
                return Json(
                    new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data },
                    JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult BillingHistory()
        {
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult loadBillingHistory(string id)
        {
            eBilling6MonthsDbContext _eBilling6Months = new eBilling6MonthsDbContext();

            Int64 accountNo = Convert.ToInt64(id);

            List<pr_REPORT_BillingHistorySummary_Result> billingHistories =
                dc.pr_REPORT_BillingHistorySummary(accountNo)
                .OrderByDescending(bh => bh.BillDate.Year)
                .ThenByDescending(bh => bh.BillDate.Month)
                .ToList();


            return Json(new { data = billingHistories }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult loadBillingHistoryDetail(string id, string accountno)
        {
            DateTime date = DateTime.Parse(id);

            Int64 accountNo = Convert.ToInt64(accountno);

            List<pr_REPORT_BillingHistoryDetails_Result> billingHistories =
                dc.pr_REPORT_BillingHistoryDetails(accountNo, date)
                .OrderByDescending(bh => bh.BillDate.Year)
                .ThenByDescending(bh => bh.BillDate.Month)
                .ToList();


            return Json(new { data = billingHistories }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ResendBill(string id)
        {
            int qId = Convert.ToInt32(id);

            var q = dc.QueueArchives.Find(qId);

            dc.pr_SCRIPT_RequeueBill(q.AccountNumber, q.BillDate);

            return Json(" ", JsonRequestBehavior.AllowGet);
        }
    }
}