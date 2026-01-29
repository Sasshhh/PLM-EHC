using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class ConsumptionController : Controller
    {
        public ConsumptionController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }

        public ConsumptionController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }

        public ConsumptionController(eServicesDbContext db)
        {
            UserManager =
            new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(db));
        }

        #region Report Init

        private void Initialise()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    IdentityManager = new IdentityManager(context);

                    if (User != null && User.Identity.IsAuthenticated)
                    {
                        IdentityManager.CurrentUser(User);
                        SystemUser = IdentityManager.CurrentUser(User);
                    }

                    if (SystemUser != null)
                    {
                        Customer =
                            context.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                                .Include(o => o.CustomerType)
                                .Include(o => o.Country)
                                .Include(o => o.IdentificationType)
                                .Include(o => o.TitleType)
                                .FirstOrDefault();

                        if (Customer != null)
                        {
                            Entity =
                                context.Entities.Where(o => o.CustomerId == Customer.Id)
                                    .Include(o => o.EntityType)
                                    .FirstOrDefault();
                            CustomerId = Customer.Id;
                        }

                    }

                    if (Customer != null)
                    {
                        Agent = context.Agents.FirstOrDefault(o => o.CustomerId == Customer.Id);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        #endregion

        public UserManager<SystemIdentityUser> UserManager { get; private set; }
        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }
        public byte[] Buffer { get; set; }

        // GET: Consumption
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            Initialise();

            if (Customer == null)
                return RedirectToAction("Index", "Profile");

            var consumptionVm = new ConsumptionViewModel() { };

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "Water", Value = "waterMeter" });
            items.Add(new SelectListItem() { Text = "Electricity", Value = "electricityMeter" });

            ViewBag.MeterTypes = new SelectList(items, "Value", "Text");

            using (var cxt = new eServicesDbContext())
            {
                var accounts = new List<Account>();
                var linkedAccounts = cxt.LinkedAccounts.Where(o => o.CustomerId == Customer.Id).Include(o => o.Account).ToList();
                foreach (var la in linkedAccounts)
                {
                    accounts.Add(la.Account);
                }

                ViewBag.AccountNumbers = new SelectList(accounts, "AccountNumber", "Description");
            }

            return View(consumptionVm);
        }

        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult GetConsumption(string meterType, string accountNumber, string startDate, string endDate)
        {
            if (meterType == null && accountNumber == null && startDate == null && endDate == null)
                return Json(string.Empty);

            // TODO: Move to AppSettings.
            string json =
                "{ " +
                    "\"serviceProperties\" : { " +
                        "\"systemCode\" : \"XET\", \"key\" : \"1234\" }, " +
                    "\"serviceRequest\" : { " +
                        "\"assetType\" : \"meter\", " +
                        "\"searchType\" : \"meterConsumption\", " +
                        "\"searchCriteria\" : [ " +
                            "{ \"name\" : \"meterType\", \"value\" : \"" + meterType + "\" }, " +
                            "{ \"name\" : \"accountNumber\", \"value\" : \"" + accountNumber + "\" },  " +
                            "{ \"name\" : \"startDate\", \"value\" : \"" + Convert.ToDateTime(startDate).ToString("yyyyMMdd") + "\" },  " +
                            "{ \"name\" : \"endDate\", \"value\" : \"" + Convert.ToDateTime(endDate).ToString("yyyyMMdd") + "\" } ] } } ";

            WebHelper webHelper = new C8.eServices.Mvc.Helpers.WebHelper();
            var assetUrl = new AppSetting();

            using (var cxt = new eServicesDbContext())
            {
                assetUrl = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.MeterConsumptionUrl);
            }

            Uri url = new Uri(string.Format(assetUrl.Value + HttpUtility.UrlEncode(json)));

            string response = webHelper.Get(url, json);
            dynamic jo = JObject.Parse(response);
            string consumption = "[";
            string reading, d;
            string meterNo = jo.assetProperties.meterProperties[1].meterIdentifiers.meterNumber;
            DateTime date;

            foreach (dynamic mc in jo.assetProperties.meterProperties[1].meterConsumption)
            {
                d = mc["date"].ToString();
                date = Convert.ToDateTime(d.Substring(0, 4) + "-" + d.Substring(4, 2) + "-" + d.Substring(6, 2));
                reading = mc["consumption"].ToString();
                consumption += "{ \"label\": \"" + meterNo + ": " + date.ToString("yyyy-MM-dd") + "\", \"y\": " + reading + "},";
            }
            consumption = consumption.Substring(0, consumption.Length - 1);
            consumption += "]";

            if (response != null)
            {
                //Handle your reponse here
                return Json(consumption);
            }
            else
            {
                //No Response from the server
                System.Diagnostics.Debug.WriteLine(response);
                return Json("No data available.");
            }
        }
    }
}