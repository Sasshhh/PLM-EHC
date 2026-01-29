using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class QueryController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();

        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            using (var eServicesContext = new eServicesDbContext())
            {
                var queryStatuses = eServicesContext.Status.Where(s => s.IsActive && !s.IsDeleted
                                    && s.Key == StatusKeys.CustomerQueryPending ||
                                    s.Key == StatusKeys.CustomerQueryInProgress ||
                                    s.Key == StatusKeys.CustomerQueryResolved).ToList();

                ViewBag.QueryStatuses = new SelectList(queryStatuses, "Id", "Name");
                return View(); 
            }           
        }

        [HttpGet]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult LoadQueries(string startDate, string endDate, string queryStatus,
            int pageNumber, string sortOrder, string sortName, int pageSize, string searchText)
        {
            var rows = new List<object>();
            var queries = new List<Support>();
            var total = 0;
            var skip = (pageNumber - 1) * pageSize;
            const string goLiveDate = "2019/07/01";
            var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);

            try
            {
                var statusResolved = db.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerQueryResolved && s.IsActive && !s.IsDeleted);
                if (null == statusResolved) throw new Exception(string.Format("Missing/ invalid status key -{0}", StatusKeys.CustomerQueryResolved));

                var queryModel =
                    db.SupportQueries.Include(s => s.Status).Include(s=>s.QueryType)
                        .OrderByDescending(q => q.Id).ToList();

                if (!string.IsNullOrEmpty(queryStatus))
                {
                    var statusFilter = Convert.ToInt32(queryStatus);
                    //queries = queries.Where(q => q.StatusId == statusFilter).ToList();
                    queryModel = queryModel.Where(q => q.Status.Id == statusFilter).ToList();
                }

                int parsedId;
                int.TryParse(searchText, out parsedId);

                if (!string.IsNullOrEmpty(searchText))
                {
                //queries = db.SupportQueries.Include(s => s.Status).Include(s => s.QueryType)
                //    .OrderByDescending(q => q.Id).Where(c => (c.Description ?? "").Contains(searchText)
                //                                || (c.CaseNumber ?? "").Contains(searchText)
                //                                || (c.QueryType.Name ?? "").Contains(searchText)
                //                                || (c.Status.Name ?? "").Contains(searchText)
                //                                || (c.Id == parsedId)).ToList();
                queries = queryModel.Where(x => (x.Description??"").Contains(searchText)||(x.CaseNumber??"").Contains(searchText)||(x.Status.Name??"").Contains(searchText)||(x.QueryType.Name??"").Contains(searchText)).ToList();

                     total = queries.Count();
                }
                else
                {
                    queries = queryModel.Skip(skip).Take(pageSize).ToList(); 
                    total = queryModel.Count();                  
                }

            #region Sort
            switch (sortName)
            {
                case "Description":
                    queries = sortOrder == "desc"
                        ? queries.OrderByDescending(q => q.Description).ToList()
                        : queries.OrderBy(q => q.Description).ToList();
                    break;
                case "CaseNumber":
                    queries = sortOrder == "desc"
                        ? queries.OrderByDescending(q => q.CaseNumber).ToList()
                        : queries.OrderBy(q => q.CaseNumber).ToList();
                    break;
                case "Name":
                    queries = sortOrder == "desc"
                        ? queries.OrderByDescending(q => q.QueryType.Name).ToList()
                        : queries.OrderBy(q => q.QueryType.Name).ToList();
                    break;
                case "Status":
                    queries = sortOrder == "desc"
                        ? queries.OrderByDescending(q => q.Status.Name).ToList()
                        : queries.OrderBy(q => q.Status.Name).ToList();
                    break;
                default:
                    queries = queries.OrderByDescending(q => q.Id).ToList();
                    break;
            }

            #endregion

            foreach (var query in queries)
                {
                    var isResolved = query.StatusId == statusResolved.Id;
                    rows.Add(new
                    {
                        queryId = query.Id,
                        query.CaseNumber,
                        Name = query.QueryType == null ? "" : query.QueryType.Name,
                        query.Description,
                        Status = query.Status == null ? "" : query.Status.Name,
                        CreatedDate = String.Format("{0:yyyy/MM/dd}", query.CreatedDateTime),
                        Resolved = isResolved,
                        Id= HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", query.CustomerId))),
                    });
                }

        }
        catch (Exception ex)
        {
            SecurityHelper.LogError(ex, "Error:");
        }
           return Json(new { total, rows}, JsonRequestBehavior.AllowGet);
}

//
// GET: /Query/
public ActionResult GetQueryTypes()
        {
            var queryTypes = db.QueryTypes.OrderBy(q => q.Index).Where(q => q.IsActive && !q.IsDeleted);
            var ddl = from queryType in queryTypes
                      select new
                      {
                          value = queryType.Id,
                          text = queryType.Description
                      };

            var json = Json(ddl.ToArray());

            return json;
        }

        public ActionResult ProcessQuery(int queryTypeId, string fullName, string email,
                        string comment, string referenceNumber,
                        string contactNumber, string captchaResponse)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            var result = "failed";
            var captchaHelper = new CaptchaHelper();
            var queryType = _context.QueryTypes.FirstOrDefault(qt => qt.Id == queryTypeId);

            if (queryType == null) throw new Exception("Invalid query type");

            Query query = new Query
            {
                QueryTypeId = queryTypeId,
                FullName = fullName,
                Email = email,
                Comment = comment,
                ReferenceNumber = referenceNumber,
                ContactNumber = contactNumber
            };

            if (query != null && captchaHelper.ValidateCaptcha(captchaResponse))
            {
                var statusPending = db.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerQueryPending && s.IsActive && !s.IsDeleted);
                if (null == statusPending) throw new Exception(string.Format("Missing/ invalid status key -{0}", StatusKeys.CustomerQueryPending));

                query.StatusId = statusPending.Id;
                _context.Queries.Add(query);
                _context.SaveChanges();

                // Send email to customer.
                Email queryEmail = new Email();
                string subject = "Siyakhokha: Customer Help";

                if (query.Id > 0)
                    subject += string.Format(" : Ticket No {0}", query.Id);

                string body = string.Format("Thank you for using Siyakhokha Customer Help, we have received your query and will contact you as soon as possible.");
                queryEmail.GenerateEmail(query.Email, subject, body, query.ReferenceNumber, false, AppSettingKeys.EservicesDefaultEmailTemplate, recipientName: query.FullName);

                // Send email to back office.
                queryEmail = new Email();
                subject = "Siyakhokha: Customer Help";

                if (query.Id > 0)
                    subject += string.Format(" : Ticket No {0}", query.Id);

                var adminEmails = _context.AppSettings.SingleOrDefault(a => a.Key == AppSettingKeys.CustomerHelperEmails);
                var revlineEmails = _context.AppSettings.SingleOrDefault(a => a.Key == AppSettingKeys.RevlineHelperEmail);
                var ToEmails = adminEmails.Value;

                if (adminEmails == null) throw new Exception("Invalid application settings: customer query emails.");
                if (revlineEmails == null) throw new Exception("Invalid application settings: revline query emails.");

                if (queryType.Key == QueryTypeKeys.Complaint || queryType.Key == QueryTypeKeys.Other)
                {
                    ToEmails = revlineEmails.Value;
                }

                body = string.Format("Reference No: {0}<br/>Name: {1}<br/>Contact No: {2}<br/>Email: {3}<br/>Query Type: {4}<br/>Comment: {5}", query.ReferenceNumber, query.FullName, query.ContactNumber, query.Email, queryType.Description, query.Comment);
                queryEmail.GenerateEmail(ToEmails, subject, body, query.ReferenceNumber, false, AppSettingKeys.EservicesDefaultEmailTemplate, recipientName: "", bccList: (ToEmails != adminEmails.Value) ? adminEmails.Value : null);

                result = "success";
            }

            var data = new
            {
                status = result
            };

            return Json(data);
        }

        /// <summary>
        /// Changes the status of the query.
        /// </summary>
        /// <param name="id">The Query Id</param>
        /// <param name="statusId">The Status Id</param>
        /// <param name="comment">The Comment</param>
        /// <param name="isSendNotification">Is Notify Customer</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ResolveQuery(int id, int statusId, string comment, bool isSendNotification)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);
            var email = new Email();
            try
            {
                var query = _context.Queries.Include(s => s.Status).Include(t => t.QueryType).FirstOrDefault(s => s.Id == id);
                if (query == null) throw new Exception("Invalid Query");

                var status = _context.Status.FirstOrDefault(s => s.Id == statusId);
                if (null == status) throw new Exception("Invalid Status");

                var isStatusChanged = query.StatusId != statusId;
                query.ResolutionComment = comment;
                query.ResolvedDateTime = DateTime.Now;
                query.StatusId = statusId;

                _context.Entry(query).State = EntityState.Modified;
                var sucess = _context.SaveChanges();

                // Send notification to customer if status changed.
                if (sucess > 0 && isStatusChanged)
                {
                    //Generate Email to Customer
                    var customerEmail = query.Email;
                    var body = "";
                    const string footer = "Customer Care Line: 031 324 5000<br/>Fax Number: 031 324 5111<br/>Email: jayan@xetgroup.com";
                    if (!string.IsNullOrEmpty(customerEmail))
                    {
                        var subject = "Siyakhokha: Status Change Notification";

                        if (query.Id > 0)
                            subject += string.Format(" : Ticket No {0}", query.Id);
                        switch (status.Key)
                        {
                            case StatusKeys.CustomerQueryInProgress:
                                body = body + comment;
                                break;
                            case StatusKeys.CustomerQueryResolved:
                                body = body +
                                       string.Format(
                                           "<p>According to our information your query has been attended to and was Resolved.</p><p><b>Query Category:</b> {0}<br/><b>Description:</b><br/>{1}</p><p><b>Resolution Comment:</b><br/>{2}</p>",
                                           query.QueryType.Name, query.Comment, comment);
                                break;
                        }
                        if (isSendNotification || status.Key == StatusKeys.CustomerQueryResolved)
                        {
                            body = body + string.Format("<p style='color:red'>Please do not respond to the sender as this mail box does not receive return mail. Use details below to reply and qoute ticket number as reference.</p><br/>{0}", footer);
                            email.GenerateEmail(customerEmail, subject, body, query.Id.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, query.FullName);
                        }
                       
                    }
                }
            }
            catch (Exception ex)
            {
                SecurityHelper.LogError(ex, "Error");
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Get comment history of the query
        /// </summary>
        /// <param name="queryId">The Query Id</param>
        /// <returns></returns>
        public JsonResult GetCommentHistory(int queryId)
        {
            var rows = new List<object>();
            var total = 0;
            try
            {
                var history = db.QueryAudits.Include(s => s.ModifiedBySystemUser).Include(s => s.Status).Where(q => q.Id == queryId).ToList().OrderByDescending(q => q.ModifiedDateTime).ToList();
                if (history.Any())
                {
                    foreach (var audit in history)
                    {
                        var date = string.Format("{0}", audit.ModifiedDateTime);
                        rows.Add(new
                        {
                            ModifiedByUser = (audit.ModifiedBySystemUser == null) ? string.Empty : audit.ModifiedBySystemUser.FullName,
                            Comment = audit.ResolutionComment,
                            ModifiedDate = date,
                            Status = audit.Status.Name
                        });
                    }
                }

                total = history.Count();

            }
            catch (Exception ex)
            {
                SecurityHelper.LogError(ex, "Error");
            }
            return Json(new { rows, total }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get the Query
        /// </summary>
        /// <param name="queryId">The Query Id</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult GetQuery(int queryId)
        {
            var query = new Query();
            var statuses = new List<Status>();
            try
            {
                query = db.Queries.Include(s => s.Status).FirstOrDefault(q => q.Id == queryId);
                var statusType = db.StatusTypes.FirstOrDefault(s => s.Key == QueryTypeKeys.CustomerQuery && s.IsActive && !s.IsDeleted);
                if (null != statusType)
                {
                    statuses =
                        db.Status.Where(s => s.StatusTypeId == statusType.Id && s.IsActive && !s.IsDeleted && (bool)!s.IsLocked).ToList();
                }
            }
            catch (Exception ex)
            {
                SecurityHelper.LogError(ex, "Error");
            }

            return Json(new { query, statuses }, JsonRequestBehavior.AllowGet);

        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Query query = db.Queries.Include(q => q.QueryType).FirstOrDefault(q => q.Id == id);
            if (query == null)
            {
                return HttpNotFound();
            }
            return View(query);
        }
        
    }
}