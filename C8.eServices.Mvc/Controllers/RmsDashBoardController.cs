using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Controllers
{
    #region RmsBillDirectoryStats Class
    public class RmsBillDirectoryStats
    {
        public int Id { get; set; }
        public string BillDate { get; set; }
        public int BillsProcessed { get; set; }
    }
    #endregion

    public class RmsDashBoardController : Controller
    {

        #region GET BillDirectoryStatistics View
        // GET: RmsDashBoard Bill Directory Statistics
        [HttpGet]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult BillDirectoryStatistics()
        {
            return View();
        }
        #endregion

        #region JSON Load Bill Directory Statistics
        // Load Bill Directory Statistics
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult LoadBillDirectoryStatistics(int pageNumber, int pageSize, string searchText)
        {
            try
            {
                using (var eServicesContext = new eServicesDbContext())
                {
                    var rows = new List<object>();
                    var total = 0;
                    var skip = (pageNumber - 1) * pageSize;
                    var stats = new List<RmsBillDirectoryStats>();

                    stats.AddRange(eServicesContext.Database.SqlQuery<RmsBillDirectoryStats>(
                        "pr_REPORT_BillDirectoryStatistics").ToList());

                    total = stats.Count();

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        stats = stats.Where(s => (s.BillDate ?? "").Contains(searchText)).ToList();
                        total = stats.Count();
                        stats = stats.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                    }
                    else
                    {                     
                        stats = stats.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();                       
                    }

                    foreach (var stat in stats)
                    {
                        rows.Add(new
                        {
                            BillDate = stat.BillDate,
                            BillsProcessed = stat.BillsProcessed
                        });
                    }

                    return Json(new { total, rows }, JsonRequestBehavior.AllowGet);
                }           
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion


        #region GET BillDirectoryArchive View
        // GET: RmsDashBoard Bill Directory Archive
        [HttpGet]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult BillDirectoryArchive()
        {
            return View();
        }
        #endregion

        #region JSON Load Bill Directory Archive
        // Load Bill Directory Archive
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult LoadBillDirectoryArchive(int pageNumber, int pageSize, string searchText)
        {
            try
            {
                using (var eServicesContext = new eServicesDbContext())
                {
                    var rows = new List<object>();
                    var total = 0;
                    var skip = (pageNumber - 1) * pageSize;
                    var billDirectories = new List<BillDirectory>();

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        billDirectories =
                            eServicesContext.BillDirectories.Where(s => (s.BillDate ?? "").Contains(searchText)
                                                            || (s.AccountNumber ?? "").Contains(searchText)).ToList();

                        total = billDirectories.Count();
                        billDirectories = billDirectories.OrderByDescending(l => l.Id).Skip(skip).Take(pageSize).ToList();

                        foreach (var billDirectory in billDirectories)
                        {
                            rows.Add(new
                            {
                                Id = billDirectory.Id,
                                BillDate = billDirectory.BillDate,
                                AccountNumber = billDirectory.AccountNumber
                            });
                        }
                    }

                    return Json(new { total, rows }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion


        #region FileResult Download RMS Bill
        // Load Bill Directory Archive
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public FileResult DownloadBill(int ? id)
        {
            try
            {
                using (var eServicesContext = new eServicesDbContext())
                {
                    var fileLocation = eServicesContext.BillDirectories.FirstOrDefault(b => b.Id == id);
                    if (fileLocation == null) throw new Exception("Invalid Bill Directory Id");

                    var fileBytes = System.IO.File.ReadAllBytes(fileLocation.BillLocation);
                    var fileName = $"{fileLocation.AccountNumber}_{fileLocation.BillDate}.pdf";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
#endregion

    }
}