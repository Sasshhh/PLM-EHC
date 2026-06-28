using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.ViewModels;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using C8.eServices.Mvc.Helpers;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using System.Web.Routing;
using System.Data.Entity.Infrastructure;
using System.Text;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();

        public CustomerController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }

        public CustomerController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
            var provider = new DpapiDataProtectionProvider("eServices");
            UserManager.UserTokenProvider = new DataProtectorTokenProvider<SystemIdentityUser>(
                                            provider.Create("EmailConfirmation"))
            {
                TokenLifespan = TimeSpan.FromDays(14)
            };
        }

        public CustomerController(eServicesDbContext db)
        {
            UserManager =
            new UserManager<SystemIdentityUser>(
            new UserStore<SystemIdentityUser>(db));
        }

        public UserManager<SystemIdentityUser> UserManager { get; private set; }
        public IdentityManager IdentityManager { get; set; }


        public bool ViewSuccess { get; set; }
        public bool SendEmailRequest { get; set; }
        private int _systemUserId = -1;



        #region Init
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }


        private void Initialise()
        {
            try
            {
                IdentityManager = new IdentityManager(db);

                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    SystemUser = IdentityManager.CurrentUser(User);
                }

                if (SystemUser != null)
                {
                    Customer =
                        db.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                            .Include(o => o.CustomerType)
                            .Include(o => o.Country)
                            .Include(o => o.IdentificationType)
                            .Include(o => o.TitleType)
                              .Include(o => o.Status)
                            .FirstOrDefault();

                    if (Customer != null)
                    {
                        Entity =
                            db.Entities.Where(o => o.CustomerId == Customer.Id)
                                .Include(o => o.EntityType)
                                .FirstOrDefault();
                        CustomerId = Customer.Id;
                    }

                }

                if (Customer != null)
                {
                    Agent = db.Agents.FirstOrDefault(o => o.CustomerId == Customer.Id);
                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }

        }

        #endregion


        #region Customer Index GET
        // GET: /Customer/
        [EncryptedActionParameter]
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public ActionResult Index(int id)
        {
            try
            {
                Initialise();
                String customerProfileStatus;
                switch (id)
                {
                    case 0:
                        customerProfileStatus = "Pending Approval";
                        break;
                    case 1:
                        customerProfileStatus = "Pending Documentation";
                        break;
                    default:
                        customerProfileStatus = "Active";
                        break;
                }
                String currentUserDepartment = db.ApplicationEntities.Find(SystemUser.DepartmentId)?.Key;
                List<Customer> customerModels = db.Customers
                    .Include(c => c.CustomerType)
                    .Include(c => c.Status)
                    .Include(c => c.Department)
                    .Include(c => c.SystemUser)
                    .Where(c => c.SystemUserId != null && c.IsActive && !c.IsDeleted).ToList();

                if (!String.IsNullOrEmpty(currentUserDepartment))
                {
                    customerModels = customerModels.Where(c => c.SystemUser.Department != null).ToList();
                    customerModels = customerModels.Where(c => c.SystemUser.Department.Key == currentUserDepartment).ToList();
                }
                    

                var profilePendingApproval = db.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                var profilePendingDocuments = db.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingDocuments);
                var profileActive = db.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);

                if (profilePendingApproval == null) throw new Exception("Invalid status");
                if (profilePendingDocuments == null) throw new Exception("Invalid status");
                if (profileActive == null) throw new Exception("Invalid status");

                switch (id)
                {
                    case 0:
                        customerModels = customerModels.Where(c => c.StatusId == profilePendingApproval.Id).ToList();
                        break;
                    case 1:
                        customerModels = customerModels.Where(c => c.StatusId == profilePendingDocuments.Id).ToList();
                        break;
                    default:
                        customerModels = customerModels.Where(c => c.StatusId == profileActive.Id).ToList();
                        break;
                }

                ViewBag.CustomerProfileStatus = customerProfileStatus;
                ViewBag.CustomerProfileStatusId = id;

                return View(customerModels);
            }
            catch (Exception)
            {
                return View("_Error");
            }
        }
        #endregion

        #region Customer Load Customers GET
        [HttpGet]
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public JsonResult LoadCustomers(int id, string startDate, string endDate, int pageNumber, int pageSize, string searchText)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                var rows = new List<object>();
                List<Customer> customers;
                var total = 0;
                var skip = (pageNumber - 1) * pageSize;
                const string goLiveDate = "2019/07/01";
                var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
                var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);

                var customerModels = context.Customers.Include(c => c.CustomerType).Include(c => c.Status)
                                     .Include(c => c.SystemUser).Include(c => c.SystemUser.Department)
                                     .Where(c => c.SystemUserId != null
                                     && c.IsActive && !c.IsDeleted &&
                                    (DbFunctions.TruncateTime(c.CreatedDateTime) >= startDateTime && DbFunctions.TruncateTime(c.CreatedDateTime) <= endDateTime));

                var currentUserDepartment = context.ApplicationEntities.Find(_base.SystemUser.DepartmentId)?.Key;
                if (!string.IsNullOrEmpty(currentUserDepartment))
                {
                    customerModels = customerModels.Where(c => c.SystemUser.Department != null && c.SystemUser.Department.Key == currentUserDepartment);
                }

                var profilePendingApproval = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                var profilePendingDocuments = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingDocuments);
                var profileActive = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);

                if (profilePendingApproval == null) throw new Exception("Invalid status");
                if (profilePendingDocuments == null) throw new Exception("Invalid status");
                if (profileActive == null) throw new Exception("Invalid status");

                switch (id)
                {
                    case 0:
                        customerModels = customerModels.Where(c => c.StatusId == profilePendingApproval.Id);
                        break;
                    case 1:
                        customerModels = customerModels.Where(c => c.StatusId == profilePendingDocuments.Id);
                        break;
                    default:
                        customerModels = customerModels.Where(c => c.StatusId == profileActive.Id);
                        break;
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    customers = customerModels.GroupJoin(
                    context.Entities,
                    customer => customer.Id,
                    entity => entity.CustomerId,
                    (customer, entity) => new { Customer = customer, Entity = entity })
                    .SelectMany(
                        x => x.Entity.DefaultIfEmpty(),
                       (x, y) => new { Customer = x.Customer, Entity = y })
                    .Where(ce => (ce.Customer.FirstName ?? "").Contains(searchText)
                    || (ce.Customer.LastName ?? "").Contains(searchText)
                    || (ce.Customer.IdentificationNumber ?? "").Contains(searchText)
                    || (ce.Customer.EmailAddress ?? "").Contains(searchText)
                    || (ce.Customer.CustomerType.Name ?? "").Contains(searchText)
                    || (ce.Entity.EntityRegistrationNumber ?? "").Contains(searchText))
                    .Select(c => c.Customer).Include(c => c.SystemUser)
                    .Include(c => c.Status).Include(c => c.CustomerType)
                    .ToList();

                    total = customers.Count();
                    customers = customers.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    customers = customerModels.OrderBy(l => l.Id)
                     .Skip(skip).Take(pageSize).ToList();
                    total = customerModels.Count();
                }

                foreach (var customer in customers)
                {
                    if (customer.SystemUser == null) throw new Exception("Invalid System user");
                    // JK.20190717a - Users may not have emails, using email confirmation for both mobile and email.
                    //var isEmailConfirmed = UserManager.FindByEmail(customer.SystemUser.EmailAddress).EmailConfirmed;
                    var isEmailConfirmed = UserManager.FindByName(customer.SystemUser.UserName).EmailConfirmed;
                    var status = (isEmailConfirmed) ? "Active" : "Pending";
                    var identificationNumber = "";

                    switch (customer.CustomerType.Key)
                    {
                        case CustomerTypeKeys.Entity:
                            var entity = context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id);
                            if (entity != null)
                                identificationNumber = entity.EntityRegistrationNumber;
                            break;
                        case CustomerTypeKeys.ManagingAgent:
                            var agent = context.Agents.FirstOrDefault(a => a.CustomerId == customer.Id);

                            if (agent != null)
                            {
                                var entityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);

                                identificationNumber = (entityAgent != null)
                                    ? context.Entities.Find(entityAgent.EntityId).EntityRegistrationNumber
                                    : context.Entities.First(e => e.CustomerId == agent.CustomerId).EntityRegistrationNumber;
                            }
                            break;
                        default:
                            identificationNumber = customer.IdentificationNumber;
                            break;
                    }

                    rows.Add(new
                    {
                        Id = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", customer.Id))),
                        FullName = customer.FirstName + " " + customer.LastName,
                        IDNumber = identificationNumber,
                        EmailAddress = customer.EmailAddress,
                        CustomerType = customer.CustomerType.Name,
                        DateCreated = String.Format("{0:yyyy/MM/dd}", customer.CreatedDateTime),
                        ProfileStatus = customer.Status.Name,
                        AccountStatus = status,
                        CustomerId = customer.Id,
                        SystemUserId = customer.SystemUserId
                    });
                }

                return Json(new { total, rows }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Customer Details GET
        // GET: /Customer/Details/5
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        [EncryptedActionParameter]
        public ActionResult Details(int? id)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                try
                {
                    var customer = context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                                   .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == id);
                    if (customer == null) throw new Exception("Invalid Customer");

                    var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                    if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.LinkedAccounts));

                    var documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.CustomerProfile);
                    if (documentReferenceType == null)
                        throw new Exception(string.Format("Invalid/ missing reference type key {0}",
                            ReferenceTypeKeys.CustomerProfile));

                    var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                                            .Include(d => d.ReferenceType)
                                            .Where(dc => dc.ApplicationId == application.Id &&
                                            dc.ReferenceTypeId == documentReferenceType.Id);

                    var vm = new CustomerProfileViewModel();
                    var documents = context.Documents.Where(d => d.CustomerId == customer.Id).ToList();
                    var notes = context.Notes.Where(n => n.ReferenceId == customer.Id).ToList();
                    var customerDocuments = context.Documents.Include(d => d.File)
                                                    .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                                && d.IsActive
                                && d.IsDeleted == false).ToList();

                    foreach (var doc in customerDocuments)
                    {
                        doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    }

                    var dvm = new DocumentsViewModel
                    {
                        ApplicationId = application.Id,
                        CustomerId = customer.Id,
                        Documents = customerDocuments,
                        IsUploadView = false,
                        DocumentCheckLists = documentCheckLists.ToList(),
                    };

                    Entity entity = null;
                    Agent agent = null;

                    if (customer.CustomerType.Key == CustomerTypeKeys.Entity)
                    {
                        entity = context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id);
                    }
                    else if (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent)
                    {
                        agent = context.Agents.FirstOrDefault(a => a.CustomerId == customer.Id);

                        if (agent != null)
                        {
                            var entityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);

                            //entity = (entityAgent != null)
                            //    ? context.Entities.Find(entityAgent.EntityId)
                            //    : context.Entities.First(e => e.CustomerId == agent.CustomerId);
                        }
                    }

                    vm.Customer = customer;
                    vm.Entity = entity;
                    vm.Agent = agent;
                    vm.CustomerDocuments = documents;
                    vm.Notes = notes;
                    vm.Document = dvm;

                    ViewBag.ReferenceTypeId = documentReferenceType.Id;
                    ViewBag.ApplicationId = application.Id;

                    var obj = new
                    {
                        IdentificationNumber = vm.Customer.IdentificationNumber,
                        FullName = vm.Customer.FullName,
                        EmailAddress = vm.Customer.SystemUser == null ? vm.Customer.EmailAddress : vm.Customer.SystemUser.EmailAddress,
                        SystemUserId = vm.Customer.SystemUser == null ? 0 : vm.Customer.SystemUserId,
                        CustomerId = vm.Customer.Id
                    };

                    ViewBag.CustomerModel = obj;

                    return View(vm);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

        }
        #endregion

        #region Customer Activate Profile for Entity/Agent
        public JsonResult ActivateCustomerProfile(int customerId)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                var email = new Email();
                try
                {
                    var customer = context.Customers.Include(s => s.SystemUser.Department).FirstOrDefault(c => c.Id == customerId);
                    if (customer == null) throw new Exception("Invalid Customer");
                    var status = context.Status.FirstOrDefault(s => s.Key.Equals(StatusKeys.CustomerActive));
                    if (status == null) throw new Exception(string.Format("Invalid/ missing status key {0}", StatusKeys.CustomerActive));

                    customer.StatusId = status.Id;
                    context.Entry(customer).State = EntityState.Modified;
                    context.SaveChanges();

                    var documentReferenceType = context.ReferenceTypes.FirstOrDefault(r => r.Key == ReferenceTypeKeys.CustomerProfile);
                    if (documentReferenceType == null) throw new Exception("Invalid reference type");

                    DocumentHelper.VerifyDocuments(customer.Id, documentReferenceType);

                    //Notify customer
                    string subject = "Property Lease Management Customer Profile Activation";
                    string body = "Your Profile has been activated on Property Lease Management System.";

                    if (customer.SystemUser != null && customer.SystemUser.Department != null &&
                        customer.SystemUser.Department.Key == ApplicationEntityKeys.RealEstateDevelopment)
                    {
                        subject = "Real Estate Development Profile Approved";
                        body = "You have been granted access into Real Estate. Your profile has been approved, please login to the system.";
                    }

                    email.GenerateEmail(customer.SystemUser.EmailAddress, subject, body, customer.Id.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, customer.FullName);

                    return Json(new { status = "success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion


        #region Notify Convayener Docs Missing or Incorrect
        public JsonResult IncompleteDocsMail(int customerId)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                var email = new Email();
                try
                {
                    var customer = context.Customers.Include(s => s.SystemUser).FirstOrDefault(c => c.Id == customerId);
                    if (customer == null) throw new Exception("Invalid Customer");
                    
                    //Notify customer
                    const string body = "Please Note the documents that have been uploaded are missing or incorrect please update your profile with the correct documentation required before it can be activated.";
                    email.GenerateEmail(customer.SystemUser.EmailAddress, "PLM Profile Activation", body, customer.Id.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, customer.FullName);

                    return Json(new { status = "success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region Customer Add Note - Send Email
        public JsonResult AddNote(int customerId, string note)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    var email = new Email();
                    var referenceType = context.ReferenceTypes.FirstOrDefault(r => r.Key == ReferenceTypeKeys.CustomerProfile);
                    if (referenceType == null) throw new Exception(string.Format("Invalid/ Missing reference type key: {0}", ReferenceTypeKeys.CustomerProfile));

                    var noteType = context.NoteTypes.FirstOrDefault(n => n.Key == NoteTypeKeys.Email);
                    if (noteType == null) throw new Exception(string.Format("Invalid/ Missing note type key: {0}", NoteTypeKeys.Email));

                    if (customerId > 0 && !string.IsNullOrEmpty(note))
                    {
                        var customer = context.Customers.Include(c => c.SystemUser).FirstOrDefault(c => c.Id == customerId);

                        if (customer == null) throw new Exception("Invalid customer");

                        var linkedAccountNote = new Note
                        {
                            ReferenceId = customerId,
                            ReferenceTypeId = referenceType.Id,
                            NoteTypeId = noteType.Id,
                            NoteEntry = note,
                            IsActive = true,
                            IsDeleted = false
                        };

                        context.Notes.Add(linkedAccountNote);

                        var count = context.SaveChanges();

                        if (count > 0)
                        {
                            var systemUser = new SystemUser();
                            //Check if linked account is created by Managing Agent
                            if (context.AgentCustomers.Any(c => c.CustomerId == customerId))
                            {
                                var agentCustomer = context.AgentCustomers.FirstOrDefault(a => a.CustomerId == customerId);
                                var agent =
                                    context.Agents.Include(a => a.Customer)
                                        .Include(a => a.Customer.SystemUser)
                                        .FirstOrDefault(a => a.Id == agentCustomer.AgentId);
                                if (agent != null)
                                {
                                    systemUser = agent.Customer.SystemUser;
                                }
                            }
                            else
                            {
                                systemUser = customer.SystemUser;
                            }

                            var subject = string.Format("Siyakhokha Linked Account [{0}]",
                                customer.IdentificationNumber);
                            var body = note;
                            var toList = systemUser.EmailAddress;

                            email.GenerateEmail(toList, subject, body, customerId.ToString(CultureInfo.InvariantCulture),
                                false, AppSettingKeys.EservicesDefaultEmailTemplate);
                        }

                        context.SaveChanges();
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


        #region Customer Create GET
        // GET: /Customer/Create
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public ActionResult Create()
        {
            ViewBag.CountryOfIssueTypeId = new SelectList(db.Countries, "Id", "Name");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.CustomerTypeId = new SelectList(db.CustomerTypes, "Id", "Name");
            ViewBag.IdentificationTypeId = new SelectList(db.IdentificationTypes, "Id", "Name");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name");
            ViewBag.SystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.TitleTypeId = new SelectList(db.TitleTypes, "Id", "Name");
            return View();
        }
        #endregion
        #region Customer Create POST
        // POST: /Customer/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CustomerTypeId,IdentificationTypeId,CountryOfIssueTypeId,IdentificationNumber,TitleTypeId,FirstName,LastName,Gender,IsDeceased,EmailAddress,WorkPhoneNumber,HomePhoneNumber,CellPhoneNumber,PhysicalAddress1,PhysicalAddress2,PhysicalAddress3,PhysicalAddress4,PhysicalAddress5,PhysicalAddressCode,PostalAddress1,PostalAddress2,PostalAddress3,PostalAddress4,PostalAddress5,PostalAddressCode,SystemUserId,StatusId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.CountryOfIssueTypeId = new SelectList(db.Countries, "Id", "Name", customer.CountryOfIssueTypeId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.CreatedBySystemUserId);
            ViewBag.CustomerTypeId = new SelectList(db.CustomerTypes, "Id", "Name", customer.CustomerTypeId);
            ViewBag.IdentificationTypeId = new SelectList(db.IdentificationTypes, "Id", "Name", customer.IdentificationTypeId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.ModifiedBySystemUserId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", customer.StatusId);
            ViewBag.SystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.SystemUserId);
            ViewBag.TitleTypeId = new SelectList(db.TitleTypes, "Id", "Name", customer.TitleTypeId);
            return View(customer);
        }
        #endregion


        #region Profile Manage Profile GET
        //View Codes
        //1 - Edit Agent Customer Details
        //2 - Create Third Party Profile (Rates Rebate)
        //3 - Add Agent Customer
        //4 - Create Customer Profile, Individual (Rates Rebate)
        [HttpGet]
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(int? agentId, int? customerId, int? viewId, int Id/*, int sysuserid*/)
        {
            customerId = Id;

            var getagents = db.Agents.Where(x => x.CustomerId == Id).ToList().FirstOrDefault();
            if (getagents != null)
            {
                agentId = getagents.CustomerId;
            }



            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    _systemUserId = _base.SystemUser.Id;

                    Agent agent = null;
                    Customer customer = null;
                    EntityAgent entityAgent = null;
                    Entity entity = null;
                    DocumentsViewModel dvm = null;
                    Models.ReferenceType documentReferenceType = null;
                    var customerDetails = new List<object>();
                    var customerDocuments = new List<Document>();
                    var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                    documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.CustomerProfile);
                    var customerTypes = context.CustomerTypes.Where(l => l.IsActive && l.IsDeleted == false);
                    var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                                            .Include(d => d.ReferenceType)
                                            .Where(dc => dc.ApplicationId == application.Id &&
                                            dc.ReferenceTypeId == documentReferenceType.Id);
                    var resName = User.Identity.GetUserName();
                    var getSystemUserId = context.Customers.Where(x => x.CreatedBySystemUserId == customer.CreatedBySystemUserId);
                    ViewBag.CusManIDNo = context.SystemUsers.FirstOrDefault(e => e.UserName == resName
                                                                          && e.IsActive && e.IsDeleted == false);
                    if (application == null) throw new Exception("Invalid Application");
                    if (documentReferenceType == null) throw new Exception("Invalid Document reference type");

                    if (agentId != null && agentId != 0)
                    {
                        agent = context.Agents.Find(agentId);
                        entityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);
                    }

                    if (customerId != null && customerId != 0)
                    {
                        customer = context.Customers.AsNoTracking().Include(c => c.CustomerType)
                                   .Include(c => c.IdentificationType)
                                   .Include(c => c.Country)
                                   .Include(c => c.SystemUser)
                                   .Include(c => c.Status)
                                   .Include(c => c.TitleType)
                                   .Include(c => c.SystemUser)
                                   .Include(c=>c.ModifiedBySystemUser)
                                   .Include(c=>c.CreatedBySystemUser)
                                   .SingleOrDefault(c => c.Id == customerId);

                        if (customer == null) throw new Exception("Invalid customer");

                        entity = (customer.CustomerType.Key == CustomerTypeKeys.Entity) ? context.Entities.FirstOrDefault(e => e.CustomerId == customerId)
                        : (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent && entityAgent != null) ? context.Entities.FirstOrDefault(e => e.Id == entityAgent.EntityId)
                        : null;

                        customerDetails.Add(new
                        {
                            customerTypeId = customer.CustomerType.Key,
                            entityTypeId = (entity != null) ? entity.EntityTypeId : 0,
                            identificationTypeId = customer.IdentificationType.Key,
                            countryOfIssueTypeId = customer.CountryOfIssueTypeId ?? 0,
                            titleTypeId = customer.TitleTypeId,
                            emailAddress = customer.EmailAddress,
                            isDeceased = customer.IsDeceased,
                            gender = customer.Gender,
                            identificationNo = customer.IdentificationNumber
                        });

                        customerDocuments = context.Documents.Include(d => d.File)
                                            .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                        && d.IsActive
                        && d.IsDeleted == false).ToList();

                        foreach (var doc in customerDocuments)
                        {
                            doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                        }

                        dvm = new DocumentsViewModel
                        {
                            ApplicationId = application.Id,
                            CustomerId = customer.Id,
                            Documents = customerDocuments,
                            IsUploadView = false,
                            DocumentCheckLists = documentCheckLists.ToList(),
                        };
                    }

                    var pvm = new CustomerProfileViewModel
                    {
                        IsManagingAgent = agent != null,
                        Agent = agent,
                        Customer = customer,
                        Entity = entity,
                        CustomerTypes = context.CustomerTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(),
                        CustomerDocuments = customerDocuments,
                        Document = dvm,
                        DocumentsVerified = customerDocuments.Any(c => c.IsLocked != null && (bool)c.IsLocked)
                    };

                    if (viewId == ViewCodeKeys.CreateAgentCustomerProfile || viewId == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile)
                    {
                        customerTypes = customerTypes.Where(l => l.Key != CustomerTypeKeys.ManagingAgent);
                        documentReferenceType =
                            context.ReferenceTypes.FirstOrDefault(
                                r => r.Key == ReferenceTypeKeys.LinkedAccountsManagingAgent);
                        if (null == documentReferenceType) throw new Exception(string.Format("Invalid/ Missing Reference Type Key ({0})", ReferenceTypeKeys.LinkedAccountsManagingAgent));
                    }
                    //Taken out because Back Office-SD 25/09/19
                    //if (null != pvm.Customer)
                    //{
                    //    //Checks if current customer profile belongs to the system user
                    //    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, pvm.Customer, ViewCodeKeys.IndividualCustomerProfile))
                    //    {
                    //        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                    //        return RedirectToAction("Index", "Error");

                    //    }
                    //}

                    ViewBag.ReferenceTypeId = documentReferenceType.Id;
                    ViewBag.ApplicationId = application.Id;
                    ViewBag.CustomerDetails = customerDetails;
                    ViewBag.ViewCode = viewId;
                    ViewBag.ViewSuccess = true;
                    ViewBag.CustomerTypeId = new SelectList(customerTypes.ToList(), "Key", "Name");
                    ViewBag.EntityTypeId = new SelectList(context.EntityTypes.Where(et => et.IsActive && et.IsDeleted == false).ToList(), "Id", "Name");
                    ViewBag.IdentificationTypeId = new SelectList(context.IdentificationTypes.Where(i => i.IsActive && !i.IsDeleted).ToList(), "Key", "Name");
                    ViewBag.CountryOfIssueTypeId = new SelectList(context.Countries.Where(o => o.IsActive && o.IsDeleted == false).ToList().OrderBy(c => c.Name), "Id", "Name");
                    ViewBag.TitleTypeId = new SelectList(context.TitleTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Id", "Name");

                    return View(pvm);
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion
        #region Profile Manage Profile POST
        //View Codes
        //1 - Edit Agent Customer Details
        //2 - Create Third Party Profile (Rates Rebate)
        //3 - Add Agent Customer
        //4 - Create Customer Profile, Individual (Rates Rebate)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(CustomerProfileViewModel customerviewmodel, string CustomerTypeId, string EntityTypeId, string IdentificationTypeId,
            string CountryOfIssueTypeId, string TitleTypeId, string IsDeceased, string emailAddress, int? ViewCode)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    // var captchaHelper = new CaptchaHelper();
                    //var captchaResponse = Request.Params["g-recaptcha-response"];

                    _base.Initialise(context);
                _systemUserId = _base.SystemUser.Id;

                var customer = customerviewmodel.Customer;
                var customerSystemUser = context.Customers.AsNoTracking().FirstOrDefault(sc => sc.SystemUserId == _systemUserId);
                var sysUserOwnershipCode = (ViewCode == ViewCodeKeys.CreateAgentCustomerProfile) ? ViewCodeKeys.CreateAgentCustomerProfile : ViewCodeKeys.IndividualCustomerProfile;

                //Checks if current customer profile belongs to the system user
                //Removed this for back office- SD 25/09/19
                //if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, customer, sysUserOwnershipCode))
                //{
                //    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                //    return RedirectToAction("Index", "Error");
                //}

                //Removed captcha validation for back office
                var customerType = context.CustomerTypes.FirstOrDefault(ct => ct.Key == CustomerTypeId);
                var customerActiveStatus = context.Status.FirstOrDefault(st => st.Key == StatusKeys.CustomerActive);
                var customerPendingStatus = context.Status.FirstOrDefault(st => st.Key == StatusKeys.CustomerPendingDocuments);
                var identificationType = context.IdentificationTypes.FirstOrDefault(it => it.Key == IdentificationTypeId);
                var agentStatus = context.Status.FirstOrDefault(st => st.Key == StatusKeys.AgentActive);
                var entityStatus = context.Status.FirstOrDefault(st => st.Key == StatusKeys.EntityActive);
                var sysUser = context.Customers.Where(x => x.SystemUserId == customer.Id).FirstOrDefault();
                

                if (customerType == null) throw new Exception("Invalid Customer type");
                if (customerActiveStatus == null) throw new Exception("Invalid Status");
                if (customerPendingStatus == null) throw new Exception("Invalid Status");
                if (identificationType == null) throw new Exception("Invalid Identification type");
                if (agentStatus == null) throw new Exception("Invalid status");
                if (entityStatus == null) throw new Exception("Invalid status");

                var titleTypeId = Convert.ToInt32(TitleTypeId);
                var countryOfIssueTypeId = (identificationType.Key == IdentificationTypeKey.SouthAfricanID) ? (int?)null : Convert.ToInt32(CountryOfIssueTypeId);
                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                var referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.CustomerProfile));
                var entityTypeId = (EntityTypeId != "") ? Convert.ToInt32(EntityTypeId) : 0;

                // JK.20190619a - Check what triggers this to be false.
                ViewSuccess = true;

                if (application == null) throw new Exception("Invalid Application");
                if (referenceType == null) throw new Exception("Invalid Reference type");

                customer.EmailAddress = emailAddress;
                customer.IsDeceased = Convert.ToBoolean(IsDeceased);
                customer.CustomerTypeId = Convert.ToInt16(customerType.Id); 
                customer.IdentificationTypeId = identificationType.Id;
                customer.IdentificationType = context.IdentificationTypes.Where(x => x.Id == identificationType.Id).FirstOrDefault();
                //customer.ModifiedBySystemUserId = context.SystemUsers.AsNoTracking().FirstOrDefault(sc => sc.Id == _systemUserId).Id;
                customer.SystemUserId = customer.SystemUserId;
                customer.CreatedBySystemUserId = customer.CreatedBySystemUserId;
                //customer.SystemUser = customer.SystemUser;
                customer.TitleTypeId = titleTypeId;
                customer.CountryOfIssueTypeId = countryOfIssueTypeId;
                customer.StatusId = customerPendingStatus.Id;
                customer.IsActive = true;
                customer.IsDeleted = false;
                customer.IsLocked = false;

                var agent = (ViewCode == ViewCodeKeys.CreateAgentCustomerProfile || ViewCode == ViewCodeKeys.UpdateAgentCustomerProfile) ? context.Agents.AsNoTracking().FirstOrDefault(a => a.CustomerId == customerSystemUser.Id) :
                             context.Agents.AsNoTracking().FirstOrDefault(a => a.CustomerId == customer.Id);

                if (agent != null)
                {
                    customerviewmodel.Agent = agent;
                    customerviewmodel.IsManagingAgent = true;
                }
                else
                {
                    customerviewmodel.Agent = new Agent();
                }

                switch (customerType.Key)
                {
                    case CustomerTypeKeys.ManagingAgent:
                        SaveCustomer(customer, customerSystemUser, ViewCode);

                        SaveAgent(customerviewmodel.Agent, customer, agentStatus);

                        if (customer.Id != 0)
                        {
                            customerviewmodel.Entity.Id = SaveEntity(customerviewmodel.Entity, customer, entityTypeId, entityStatus);
                            SaveEntityAgent(customerviewmodel.Entity, customerviewmodel.Agent);
                        }

                        SendApprovalNotification(customer);
                        break;
                    case CustomerTypeKeys.Entity:
                        SaveCustomer(customer, customerSystemUser, ViewCode);

                        if (customer.Id != 0)
                            SaveEntity(customerviewmodel.Entity, customer, entityTypeId, entityStatus);

                        SendApprovalNotification(customer);
                        break;
                    default:
                        if (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile ||
                            ViewCode == ViewCodeKeys.CreateAgentCustomerProfile)
                        {
                            customer.StatusId = customerPendingStatus.Id;
                            SaveCustomer(customer, customerSystemUser, ViewCode);

                            SendApprovalNotification(customer);
                        }
                        else
                        {
                            customer.StatusId = customerActiveStatus.Id;
                            SaveCustomer(customer, customerSystemUser, ViewCode);
                        }
                        break;
                }

                context.SaveChanges();

                if (customerviewmodel.IsManagingAgent && (ViewCode == ViewCodeKeys.CreateAgentCustomerProfile || ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile) && customer.Id > 0)
                {
                    SaveAgentCustomer(customerviewmodel.Agent, customer);
                }

                if (ViewSuccess)
                {
                    var customerDocuments = context.Documents.Include(d => d.File)
                                            .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == referenceType.Id
                    && d.IsActive
                    && d.IsDeleted == false).ToList();
                    //Not sure if this is needed
                    if (!customerDocuments.Any())
                    {
                        //var success = (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile || ViewCode == ViewCodeKeys.IndividualCustomerProfile) ? Url.Action("Create", "RatesRebate", SecureActionLinkExtension.Encrypt(new { customerId = customer.Id })) : Url.Action("Index2", SecureActionLinkExtension.Encrypt(new { agentId = customerviewmodel.Agent.Id, customerId = (customerviewmodel.Agent.Id != 0) ? customerviewmodel.Agent.CustomerId : customer.Id }));
                        //AJC 2019/11/14 Made a change to direct the page to Customer/Index, original code attempted to redirect the to Customer/Index2 which does not exist.
                        var success = (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile || ViewCode == ViewCodeKeys.IndividualCustomerProfile) ? Url.Action("Create", "RatesRebate", SecureActionLinkExtension.Encrypt(new { customerId = customer.Id })) : Url.Action("Index", SecureActionLinkExtension.Encrypt(new { id=2 }));
                        if (success != null)
                        {
                            if (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile ||
                                ViewCode == ViewCodeKeys.CreateAgentCustomerProfile ||
                                ViewCode == ViewCodeKeys.UpdateAgentCustomerProfile)
                            {
                                referenceType =
                                    context.ReferenceTypes.FirstOrDefault(
                                        r =>
                                            r.Key == ReferenceTypeKeys.LinkedAccountsManagingAgent && r.IsActive &&
                                            !r.IsDeleted);
                                if (null == referenceType) throw new Exception(string.Format("Invalid/ Missing Reference key ({0})", ReferenceTypeKeys.LinkedAccountsManagingAgent));
                            }
                            var returnUrl = success.ToString(CultureInfo.InvariantCulture);
                            return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = customer.Id, customerId = customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = customerviewmodel.Agent.Id, returnUrl = returnUrl })));
                        }
                    }
                    else
                    {
                            //AJC 2019/11/14 Made a change to direct the page to Customer/Index, original code attempted to redirect the to Customer/Index2 which does not exist.
                            return (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile || ViewCode == ViewCodeKeys.IndividualCustomerProfile) ? RedirectToAction("Create", "RatesRebate", SecureActionLinkExtension.Encrypt(new { customerId = customer.Id })) : RedirectToAction("Index", SecureActionLinkExtension.Encrypt(new { id = 2 }));
                    }
                }
                else
                {
                    ViewBag.CustomerTypeId = new SelectList(context.CustomerTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Key", "Name");
                    ViewBag.EntityTypeId = new SelectList(context.EntityTypes.Where(et => et.IsActive && et.IsDeleted == false).ToList(), "Id", "Name");
                    ViewBag.IdentificationTypeId = new SelectList(context.IdentificationTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Key", "Name");
                    ViewBag.CountryOfIssueTypeId = new SelectList(context.Countries.Where(o => o.IsActive && o.IsDeleted == false).OrderBy(c => c.Name).ToList(), "Id", "Name");
                    ViewBag.TitleTypeId = new SelectList(context.TitleTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Id", "Name");
                    ViewBag.CustomerDetails = new List<object>();
                    ViewBag.ViewCode = ViewCode;
                    ViewBag.ViewSuccess = ViewSuccess;
                        //AJC 2019/11/14 Made a change to direct the page to Customer/Index, original code attempted to redirect the to Customer/Index2 which does not exist.

                        ViewBag.ReturnUrl = (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile || ViewCode == ViewCodeKeys.IndividualCustomerProfile) ? Url.Action("Create", "RatesRebate", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { customerId = customer.Id }))) : Url.Action("Index", SecureActionLinkExtension.Encrypt(new { id = 2 }));
                    return View(customerviewmodel);
                }


                return View();
            }

            catch (Exception)
            {
                return View("_error");
            }
              
        }

    }
        #region Profile Verify Entity
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult VerifyEntity(string custType, string entityRegNo)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                //entity
                var entity = context.Entities.FirstOrDefault(e => e.EntityRegistrationNumber == entityRegNo);

                if (entity != null)
                {
                    return Json(entity, JsonRequestBehavior.AllowGet);
                }

                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Profile Save Customer
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        private void SaveCustomer(Customer customer, Customer currentSystemUser, int? viewCode)
        {
            using (var context = new eServicesDbContext())

            {

                try

                {

                    _base.Initialise(context);

                //var systemIdUser = User.Identity.GetUserId();

                //// Back Office User

                //var systemUser = new IdentityManager(context).CurrentUser(systemIdUser);// IdentityManager.CurrentUser(systemIdUser);



                if (viewCode == 0 || viewCode == ViewCodeKeys.IndividualCustomerProfile)

                {

                    //customer.SystemUserId = context.CurrentSystemUser.Id;



                    //if (currentSystemUser != null) customer.Id = currentSystemUser.Id;

                }



                if (customer.Id != 0)

                {
                        //AJC 2019/11/15 Update system user email and mobile number on update of customer.
                        var sysUser = context.SystemUsers.Where(x => x.Id == customer.SystemUserId).FirstOrDefault();
                        sysUser.EmailAddress = customer.EmailAddress;
                        sysUser.MobileNumber = customer.CellPhoneNumber;
//

                        var customerReference = context.Customers.AsNoTracking().First(c => c.Id == customer.Id);
                    context.CurrentSystemUser = _base.SystemUser;
                    customer.StatusId = customerReference.StatusId;
                    customer.CreatedBySystemUserId =customer.CreatedBySystemUserId;
                    customer.Id = customer.Id;
                    customer.CreatedDateTime = customerReference.CreatedDateTime;
                    customer.SystemUserId = context.Customers.Where(x=>x.Id==customer.Id).FirstOrDefault().SystemUserId;
                    customer.ModifiedBySystemUserId = context.CurrentSystemUser.Id;
                        // customer.ModifiedBySystemUserId = context.SystemUsers.Where(x => x.Id == _base.SystemUser.Id).FirstOrDefault().Id;
                        // customer.ModifiedBySystemUser = context.SystemUsers.Where(x => x.Id == customer.ModifiedBySystemUserId).FirstOrDefault().ModifiedBySystemUser;
                        ViewSuccess = true;
                        db.Entry(sysUser).State = EntityState.Modified;
                        db.Entry(customer).State = EntityState.Modified;
                    db.SaveChanges();
                }

                else

                {

                    var custIdExists = context.Customers.Any(c => c.IdentificationNumber == customer.IdentificationNumber);



                    if (custIdExists && viewCode == ViewCodeKeys.CreateAgentCustomerProfile)

                    {

                        TempData["IDError"] = "Customer profile exists for id number : " + customer.IdentificationNumber +

                            ".<br/>For more details please contact the administrator.";

                        ViewSuccess = false;

                    }

                    else if (custIdExists)

                    {

                        TempData["IDError"] = "Customer profile exists for id number : " + customer.IdentificationNumber +

                            ".<br/>New profile will be used. For more details please contact the administrator.";

                        ViewSuccess = false;

                        SendEmailRequest = true;

                        context.Customers.Add(customer);

                    }

                    else

                    {

                        SendEmailRequest = true;

                        context.Customers.Add(customer);

                    }

                }



                    //context.CurrentSystemUser = _base.SystemUser;
                    //context.SaveChanges();
                    //db.SaveChanges();


                }

                catch (Exception ex)

                {

                    throw;

                }

            }
        }

        #endregion

        #region Profile Save Agent
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        private void SaveAgent(Agent agent, Customer customer, Status agentStatus)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    agent.CustomerId = customer.Id;
                    agent.CustomerTypeId = customer.CustomerTypeId;
                    agent.IdentificationTypeId = customer.IdentificationTypeId;
                    agent.TitleTypeId = customer.TitleTypeId;
                    agent.CountryOfIssueTypeId = customer.CountryOfIssueTypeId;
                    agent.StatusId = agentStatus.Id;
                    agent.IdentificationNumber = customer.IdentificationNumber;
                    agent.FirstName = customer.FirstName;
                    agent.LastName = customer.LastName;
                    agent.EmailAddress = customer.EmailAddress;
                    agent.HomePhoneNumber = customer.HomePhoneNumber;
                    agent.CellPhoneNumber = customer.CellPhoneNumber;
                    agent.PhysicalAddress1 = customer.PhysicalAddress1;
                    agent.PhysicalAddress2 = customer.PhysicalAddress2;
                    agent.PhysicalAddress3 = customer.PhysicalAddress3;
                    agent.PhysicalAddress4 = customer.PhysicalAddress4;
                    agent.PhysicalAddress5 = customer.PhysicalAddress5;
                    agent.PhysicalAddressCode = customer.PhysicalAddressCode;
                    agent.PostalAddress1 = customer.PostalAddress1;
                    agent.PostalAddress2 = customer.PostalAddress2;
                    agent.PostalAddress3 = customer.PostalAddress3;
                    agent.PostalAddress4 = customer.PostalAddress4;
                    agent.PostalAddress5 = customer.PostalAddress5;
                    agent.PostalAddressCode = customer.PostalAddressCode;

                    agent.IsActive = true;
                    agent.IsDeleted = false;
                    agent.IsLocked = false;

                    if (agent.Id != 0)
                    {
                        context.Entry(agent).State = EntityState.Modified;
                    }
                    else
                    {
                        if (!context.Agents.Any(a => a.CustomerId == agent.CustomerId))
                        {
                            context.Agents.Add(agent);
                        }
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

        #region Save Entity
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        private int SaveEntity(Entity entity, Customer customer, int entityTypeId, Status entityStatus)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    entity.CustomerId = customer.Id;
                    entity.EntityTypeId = entityTypeId;
                    entity.StatusId = entityStatus.Id;
                    entity.IsActive = true;
                    entity.IsDeleted = false;

                    var currentEntity = context.Entities.AsNoTracking().FirstOrDefault(e => e.EntityRegistrationNumber == entity.EntityRegistrationNumber);

                    if (currentEntity == null)
                    {
                        if (!context.Entities.Any(e => e.CustomerId == entity.CustomerId))
                            context.Entities.Add(entity);
                    }
                    else
                    {
                        entity.Id = currentEntity.Id;
                        currentEntity.CreatedBySystemUserId = currentEntity.CreatedBySystemUserId;
                        currentEntity.CreatedDateTime = currentEntity.CreatedDateTime;
                    }

                    context.SaveChanges();
                    return entity.Id;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region Save Agent Customer
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        private void SaveAgentCustomer(Agent agent, Customer customer)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    if (!context.AgentCustomers.Any(ac => ac.CustomerId == customer.Id))
                    {
                        var agentcustomer = new AgentCustomer
                        {
                            AgentId = agent.Id,
                            CustomerId = customer.Id,
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false
                        };

                        context.AgentCustomers.Add(agentcustomer);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion
        #region Profile Save Entity Agent
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        private void SaveEntityAgent(Entity entity, Agent agent)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    if (!context.EntityAgents.Any(ea => ea.AgentId == agent.Id))
                    {
                        var entityagent = new EntityAgent
                        {
                            EntityId = entity.Id,
                            AgentId = agent.Id,
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false
                        };

                        context.EntityAgents.Add(entityagent);
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


        #region Profile Send Customer Profile Approval Notification
        public void SendApprovalNotification(Customer customer)
        {
            try
            {
                if (SendEmailRequest)
                {
                    if (customer.EmailAddress != null && customer.EmailAddress != "")
                    {
                        var email = new Email();
                        const string customerSubject = "Property Lease Management System: Customer Profile";
                        const string customerBody = " Your profile will be reviewed by the Property Lease Management administration team. Once approved, you will be able to create plm applications. " +
                                       "Please make sure that all required documents have been uploaded on your profile.";

                        email.GenerateEmail(customer.EmailAddress, customerSubject, customerBody, customer.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, customer.FullName);

                    }
                    else if (customer.EmailAddress == null && customer.CellPhoneNumber != null)
                    {
                        var sms = new CesarSMS();
                        var cxt = new eServicesDbContext();
                        var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                        sms.GenerateSMS(customer.CellPhoneNumber, "Your profile will be reviewed and approved before you can start capturing plm applications. Please supply all documents for a quick response. Thanks Property Lease Management Team.",
    customer.SystemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, customer.SystemUser.FullName);
                        //SmsHelper.Send(customer.CellPhoneNumber, "Your profile will be reviewed and approved to start linking accounts. Please supply all documents for a quick response. Thanks Siyakhokha Team.");

                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #endregion


        //#region Customer Edit GET
        //// GET: /Customer/Edit/5
        //[Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        //[EncryptedActionParameter]
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Customer customer = db.Customers.Find(id);
        //    if (customer == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.CountryOfIssueTypeId = new SelectList(db.Countries, "Id", "Name", customer.CountryOfIssueTypeId);
        //    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.CreatedBySystemUserId);
        //    ViewBag.CustomerTypeId = new SelectList(db.CustomerTypes, "Id", "Name", customer.CustomerTypeId);
        //    ViewBag.IdentificationTypeId = new SelectList(db.IdentificationTypes, "Id", "Name", customer.IdentificationTypeId);
        //    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.ModifiedBySystemUserId);
        //    ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", customer.StatusId);
        //    ViewBag.SystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.SystemUserId);
        //    ViewBag.TitleTypeId = new SelectList(db.TitleTypes, "Id", "Name", customer.TitleTypeId);
        //    return View(customer);
        //}
        //#endregion
        //#region Customer Edit Edit POST
        //// POST: /Customer/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,CustomerTypeId,IdentificationTypeId,CountryOfIssueTypeId,IdentificationNumber,TitleTypeId,FirstName,LastName,Gender,IsDeceased,EmailAddress,WorkPhoneNumber,HomePhoneNumber,CellPhoneNumber,PhysicalAddress1,PhysicalAddress2,PhysicalAddress3,PhysicalAddress4,PhysicalAddress5,PhysicalAddressCode,PostalAddress1,PostalAddress2,PostalAddress3,PostalAddress4,PostalAddress5,PostalAddressCode,SystemUserId,StatusId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Customer customer)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(customer).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.CountryOfIssueTypeId = new SelectList(db.Countries, "Id", "Name", customer.CountryOfIssueTypeId);
        //    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.CreatedBySystemUserId);
        //    ViewBag.CustomerTypeId = new SelectList(db.CustomerTypes, "Id", "Name", customer.CustomerTypeId);
        //    ViewBag.IdentificationTypeId = new SelectList(db.IdentificationTypes, "Id", "Name", customer.IdentificationTypeId);
        //    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.ModifiedBySystemUserId);
        //    ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", customer.StatusId);
        //    ViewBag.SystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.SystemUserId);
        //    ViewBag.TitleTypeId = new SelectList(db.TitleTypes, "Id", "Name", customer.TitleTypeId);
        //    return View(customer);
        //}
        //#endregion


        #region Customer Delete GET
        // GET: /Customer/Delete/5
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }
        #endregion
        #region Customer Delete POST
        // POST: /Customer/Delete/5
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);
            db.Customers.Remove(customer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        #endregion


        #region Customer Send Activation Email
        [HttpPost]
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public ActionResult SendActivationEmail(int systemUserId)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    //Confirmation email
                    var confirmationEmail = new Email();
                    var user = context.SystemUsers.SingleOrDefault(u => u.Id == systemUserId);
                    if (user == null) throw new Exception("Invalid system user");
                    var identityUser = UserManager.FindByName(user.UserName);
                    if (identityUser == null) throw new Exception("Invalid identity user");

                    var emailConfirmationCode = UserManager.GenerateEmailConfirmationToken(identityUser.Id);
                    emailConfirmationCode = HttpUtility.UrlEncode(emailConfirmationCode);
                    const string subject = "Siyakhokha: Email Confirmation";
                    var body = string.Format("Thank you for your registration, please click on the below link to complete your registration: <br/><a href=\"{0}\" title=\"User Email Confirm\">{0}</a>", Url.Action("ConfirmEmail", "Account", SecureActionLinkExtension.Encrypt(new { id = identityUser.Id, token = emailConfirmationCode }), protocol: Request.Url.Scheme));
                    confirmationEmail.GenerateEmail((!string.IsNullOrEmpty(identityUser.UnconfirmedEmail)) ? identityUser.UnconfirmedEmail : identityUser.Email, subject, body, user.Id.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.FullName);

                    return Json(new { result = "Success" });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion


        #region Customer Modify Email
        [HttpPost]
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public ActionResult ModifyCustomerEmail(int systemUserId, string email)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);


                    var systemUser = context.SystemUsers.Find(systemUserId);
                    if (systemUser == null) throw new Exception("Invalid system user");
                    var identityUser = UserManager.FindByName(systemUser.UserName);
                    if (identityUser == null) throw new Exception("Invalid identity user");

                    var emailAssigned = context.SystemUsers.Any(u => u.EmailAddress.ToLower() ==
                                          email.ToLower() && u.IsActive && u.IsDeleted == false);

                    if (emailAssigned)
                    {
                        return Json(new { success = false, result = "Error! Email address exists, please enter another email address." });
                    }

                    var confirmationEmail = new Email();
                    identityUser.UnconfirmedEmail = email;
                    identityUser.EmailConfirmed = false;

                    var result = UserManager.Update(identityUser);

                    if (result.Succeeded)
                    {
                        var emailConfirmationCode = UserManager.GenerateEmailConfirmationToken(identityUser.Id);
                        emailConfirmationCode = HttpUtility.UrlEncode(emailConfirmationCode);
                        const string subject = "Siyakhokha: Email Confirmation";
                        var body = string.Format("You have requested to update your email address, please click on the link below to reactivate your Siyakhokha account : <br/><a href=\"{0}\" title=\"User Email Confirm\">{0}</a>", Url.Action("ConfirmEmail", "Account", SecureActionLinkExtension.Encrypt(new { id = identityUser.Id, token = emailConfirmationCode }), protocol: System.Web.HttpContext.Current.Request.Url.Scheme));
                        confirmationEmail.GenerateEmail(email, subject, body, systemUser.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, systemUser.FullName);

                        return Json(new { success = true, result = "Activation email sent to customer. Once activated, email address will be updated." });
                    }

                    return Json(new { success = false, result = "An Error occurred. Please try again later" });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion


        #region Customer Missing Profile GET
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public ActionResult MissingProfile()
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

        #region Load System Users with missing customer profiles GET

        public JsonResult LoadSystemUsers(int pageSize, string startDate, string endDate, int pageNumber, string searchText)
        {
            var total = 0;
            var systemUsers = new List<SystemUser>();
            var rows = new List<object>();
            var skip = (pageNumber - 1) * pageSize;
            const string goLiveDate = "2019/07/01";
            var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);

            try
            {
                using (var context = new eServicesDbContext())
                {
                    //Gets system users with no customer profile
                    var systemUserModel = context.SystemUsers.Where(s => s.IsActive && !s.IsDeleted
                        && !context.Customers.Any(c => c.SystemUserId == s.Id)
                        && !context.ApplicationUserRoles.Any(a => a.SystemUserId == s.Id)
                        & (DbFunctions.TruncateTime(s.CreatedDateTime) >= startDateTime && DbFunctions.TruncateTime(s.CreatedDateTime) <= endDateTime));

                    //Filter
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        systemUsers = systemUserModel.Where(s => (s.FirstName ?? "").Contains(searchText) ||
                                                                 (s.LastName ?? "").Contains(searchText) ||
                                                                 (s.UserName ?? "").Contains(searchText) ||
                                                                 (s.EmailAddress ?? "").Contains(searchText)).ToList();

                        systemUsers = systemUsers.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                    }
                    else
                    {
                        systemUsers = systemUserModel.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                        total = systemUserModel.Count();
                    }

                    foreach (var systemUser in systemUsers)
                    {
                        var user = UserManager.FindByName(systemUser.UserName);
                        if (user == null) throw new Exception("Invalid user");
                        var status = (user.EmailConfirmed) ? "Active" : "Pending";

                        rows.Add(new
                        {
                            Id = systemUser.Id,
                            fullName = systemUser.FullName,
                            userName = systemUser.UserName,
                            emailAddress = systemUser.EmailAddress,
                            AccountStatus = status,
                            DateCreated = String.Format("{0:yyyy/MM/dd}", systemUser.CreatedDateTime)
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


        #region Delete System User
        public async Task<JsonResult> DeleteSystemUser(int id)
        {
            try
            {
                var result = "failed";
                using (var context = new eServicesDbContext())
                {
                    _base.Initialise(context);

                    // Delete system user
                    var systemUser = context.SystemUsers.Find(id);
                    if (systemUser == null) return Json(result, JsonRequestBehavior.AllowGet);

                    systemUser.IsActive = false;
                    systemUser.IsDeleted = true;
                    context.SaveChanges();

                    var user = UserManager.FindByName(systemUser.UserName.Trim());
                    //Remove user from Aspnet 
                    var status = UserManager.RemoveFromRole(user.Id, "Customers");
                    if (!status.Succeeded) return Json(new { result }, JsonRequestBehavior.AllowGet);

                    var removeStatus = UserManager.DeleteAsync(user);
                    result = removeStatus.Result.Succeeded ? "success" : result;
                }
                return Json(new { result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Delete Customer Profile
        [EncryptedActionParameter]
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public JsonResult DeleteCustomerProfile(int id)
        {
            try
            {
                var result = "failed";
                using (var context = new eServicesDbContext())
                {
                    _base.Initialise(context);

                    //Delete customer profile
                    var customer = context.Customers.FirstOrDefault(c => c.Id == id && c.IsActive && !c.IsDeleted);
                    if (null == customer) throw new Exception("Customer profile not found!");

                    customer.IsActive = false;
                    customer.IsDeleted = true;
                    context.Entry(customer).State = EntityState.Modified;

                    var entity =
                        context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id && e.IsActive && !e.IsDeleted);
                    if (null != entity)
                    {
                        entity.IsActive = false;
                        entity.IsDeleted = true;
                        context.Entry(entity).State = EntityState.Modified;
                    }
                    context.SaveChanges();

                    //Delete linked accounts
                    var customerLinkedAccounts = context.LinkedAccounts.Include(x => x.Account).Include(x => x.Customer)
                        .Include(x => x.Customer.Status)
                        .Include(x => x.Status)
                        .Where(x => x.CustomerId == customer.Id && x.Customer.IsActive
                                    && x.Customer.Status.Key == StatusKeys.CustomerActive && x.IsActive &&
                                    x.IsDeleted == false);
                    foreach (var la in customerLinkedAccounts)
                    {
                        la.IsActive = false;
                        la.IsDeleted = true;
                        context.Entry(la).State = EntityState.Modified;
                    }
                    context.SaveChanges();
                    // Delete system user
                    var systemUser = context.SystemUsers.Find(customer.SystemUserId);
                    if (systemUser == null) return Json(result, JsonRequestBehavior.AllowGet);

                    systemUser.IsActive = false;
                    systemUser.IsDeleted = true;
                    context.Entry(systemUser).State = EntityState.Modified;
                    context.SaveChanges();

                    var user = UserManager.FindByName(systemUser.UserName.Trim());

                    //Remove user from Aspnet 
                    var status = UserManager.RemoveFromRole(user.Id, "Customers");
                    if (!status.Succeeded) return Json(new { result }, JsonRequestBehavior.AllowGet);

                    var removeStatus = UserManager.DeleteAsync(user);
                    result = removeStatus.Result.Succeeded ? "success" : result;
                }
                return Json(new { result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Customer Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
        #region Load Agent Customers

        public ActionResult AgentCustomer()
        {
            return View();
        }

        public JsonResult LoadAgentCustomers(int pageNumber, int pageSize, string searchText)
        {
            var total = 0;
            var rows = new List<object>();
            var skip = (pageNumber - 1) * pageSize;
            var agentCustomers = new List<AgentCustomer>();

            try
            {
                using (var context = new eServicesDbContext())
                {
                    var agentCustomersModels =
                        context.AgentCustomers.Include(a => a.Agent)
                            .Include(ac => ac.Agent.Customer)
                            .Include(ac => ac.Agent.Customer.TitleType)
                            .Include(c => c.Customer)
                            .Include(c => c.Customer.TitleType)
                            .Include(s => s.Customer.Status).ToList();

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        agentCustomers = agentCustomersModels
                        .Where(ce => (ce.Customer.FirstName ?? "").Contains(searchText)
                        || (ce.Customer.LastName ?? "").Contains(searchText)
                        || (ce.Customer.IdentificationNumber ?? "").Contains(searchText)
                        || (ce.Customer.EmailAddress ?? "").Contains(searchText)
                        || (ce.Customer.Status.Name ?? "").Contains(searchText)
                        || (ce.Agent.FirstName ?? "").Contains(searchText)
                        || (ce.Agent.LastName ?? "").Contains(searchText))
                        .ToList();

                        total = agentCustomers.Count();
                        agentCustomers = agentCustomers.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                    }
                    else
                    {
                        agentCustomers = agentCustomersModels.OrderBy(l => l.Id)
                         .Skip(skip).Take(pageSize).ToList();
                        total = agentCustomersModels.Count();
                    }

                    foreach (var agentCustomer in agentCustomers)
                    {
                        rows.Add(new
                        {
                            Id = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", agentCustomer.Customer.Id))),
                            AgentFullName = agentCustomer.Agent.Customer.FullName,
                            CustomerIdNumber = agentCustomer.Customer.IdentificationNumber,
                            CustomerFullName = agentCustomer.Customer.FullName,
                            CustomerEmailAddress = agentCustomer.Customer.EmailAddress,
                            ProfileStatus = agentCustomer.Customer.Status.Name,
                            DateCreated = String.Format("{0:yyyy/MM/dd}", agentCustomer.Customer.CreatedDateTime),
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                SecurityHelper.LogError(ex, "Error occured while trying to load agent customers");
            }
            return Json(new { rows, total }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Deactivate User Account
        //AJC.20190930 Deactivate User Account.
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public ActionResult DeactivateUserAccount(int id)
        {
            try
            {
                //var result = "failed";
                using (var context = new eServicesDbContext())
                {
                    //_base.Initialise(context);
                    //Set Customer To Deleted
                    var customerReference = context.Customers.AsNoTracking().First(c => c.Id == id);
                    var customer = context.Customers.FirstOrDefault(c => c.Id ==id && c.IsActive && !c.IsDeleted);
                    if (null == customer) throw new Exception("Customer profile not found!");
                    Status stateActive = context.Status.FirstOrDefault(o => o.Key == StatusKeys.CustomerActive);
                    customer.IsActive = false;
                    customer.IsDeleted = true;
                    customer.StatusId = customerReference.StatusId;
                    db.Entry(customer).State = EntityState.Modified;
                    var entity =
                        context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id && e.IsActive && !e.IsDeleted);
                    if (null != entity)
                    {
                        entity.IsActive = false;
                        entity.IsDeleted = true;
                        db.Entry(entity).State = EntityState.Modified;
                    }

                    //Mark Deactivation Request To Resolved
                    var Status = db.Status.FirstOrDefault(x => x.Key == "s_support_query_resolved").Id;

                    var SupportRequest = db.SupportQueries.Include(o => o.Account).Include(o => o.QueryType).Where(o => o.CustomerId == customer.Id && o.IsActive && !o.IsDeleted && o.QueryType.Name == "Deactivate Account" && o.StatusId != Status).FirstOrDefault();
                    if (SupportRequest!=null)
                    {
                        SupportRequest.StatusId = db.Status.Where(x => x.Key == StatusKeys.SupportQueryResolved).FirstOrDefault().Id;
                        db.Entry(SupportRequest).State = EntityState.Modified;
                    }
                   

                    //Deactivate linked accounts
                    //var customerLinkedAccounts = context.LinkedAccounts.Include(x => x.Account).Include(x => x.Customer)
                    //    .Include(x => x.Customer.Status)
                    //    .Include(x => x.Status)
                    //    .Where(x => x.CustomerId == customer.Id && x.Customer.IsActive
                    //                && x.Customer.Status.Key == StatusKeys.CustomerActive && x.IsActive &&
                    //                x.IsDeleted == false);

                    //foreach (var la in customerLinkedAccounts)
                    //{
                    //    la.IsActive = false;
                    //    la.IsDeleted = true;
                    //    db.Entry(la).State = EntityState.Modified;
                    //}

                    // Remove system user
                    var systemUser = context.SystemUsers.Find(customer.SystemUserId);
                    systemUser.IsLocked = true;
                    systemUser.IsActive = false;
                    systemUser.IsDeleted = true;
                    db.Entry(systemUser).State = EntityState.Modified;

                    var user = UserManager.FindByName(systemUser.UserName.Trim());

                    //Add isLocked to aspnet User and Deactivate it
                    user.SystemUser.IsLocked = true;
                    user.SystemUser.IsActive = false;
                    user.SystemUser.IsDeleted = true;
                    user.LockoutEnabled = true;
                    UserManager.Update(user);
                    db.SaveChanges();
                }
                return RedirectToAction("DeactivatedUsers", "Customer");
        }
            catch (Exception ex)
            {
                throw ex;
            }
}
        #endregion

        #region Activate User Account
        //AJC.20190930 Reactivate User Account
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public ActionResult ActivateUserAccount(int id)
        {
            try
            {
                //var result = "failed";
                using (var context = new eServicesDbContext())
                {
                    //_base.Initialise(context);
                    //_systemUserId = _base.SystemUser.Id;

                    //Set customer profile to Active
                    var customer = context.Customers.FirstOrDefault(c => c.Id == id && !c.IsActive && c.IsDeleted);
                    if (customer == null) throw new Exception("Customer profile not found!");

                    customer.IsDeleted = false;
                    customer.IsActive = true;
                    //customer.StatusId = context.Status.FirstOrDefault(o => o.Key == StatusKeys.CustomerActive).Id;
                    db.Entry(customer).State = EntityState.Modified;

                    var entity = context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id && !e.IsActive && e.IsDeleted);
                    if (null != entity)
                    {
                        entity.IsActive = true;
                        entity.IsDeleted = false;
                        db.Entry(entity).State = EntityState.Modified;
                    }

                    //Re-Activate linked accounts
                    //var customerLinkedAccounts = context.LinkedAccounts.Include(x => x.Account).Include(x => x.Customer)
                    //    .Include(x => x.Customer.Status)
                    //    .Include(x => x.Status)
                    //    .Where(x => x.CustomerId == customer.Id && x.Customer.IsActive
                    //                && x.Customer.Status.Key == StatusKeys.CustomerActive && x.IsActive == false &&
                    //                x.IsDeleted);
                    //foreach (var la in customerLinkedAccounts)
                    //{
                    //    la.IsActive = true;
                    //    la.IsDeleted = false;
                    //    db.Entry(la).State = EntityState.Modified;
                    //}

                    // Re-Add system user
                    var systemUser = context.SystemUsers.Find(customer.SystemUserId);
                    systemUser.IsLocked = false;
                    systemUser.IsActive = true;
                    systemUser.IsDeleted = false;
                    db.Entry(systemUser).State = EntityState.Modified;
                    db.SaveChanges();

                    var user = UserManager.FindByName(systemUser.UserName.Trim());

                    //Removed isLocked and Activate it
                    user.SystemUser.IsLocked = false;
                    user.SystemUser.IsActive = true;
                    user.SystemUser.IsDeleted = false;
                    user.LockoutEnabled = false;
                    UserManager.Update(user);
                    db.SaveChanges();

                }

                return RedirectToAction("DeactivatedUsers", "Customer");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #region Customer Deactivated Users GET
        // GET: /Customer/
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public ActionResult DeactivatedUsers()
        {
            using (var context = new eServicesDbContext())
            {
                Initialise();
                try
                {
                    List<Customer> customers;
                    var customerModels = (List<Customer>)null;
                    var userKey = db.SystemUsers.Include(o => o.Department).FirstOrDefault(r => r.Id == SystemUser.Id).Department?.Key;
                    if (userKey != null)
                    {
                        customerModels = context.Customers.Include(c => c.CustomerType).Include(c => c.Status).Include(c => c.Department)
                                    .Include(c => c.SystemUser)
                                    .Where(c => c.SystemUserId != null
                                    && !c.IsActive && c.IsDeleted && c.Department.Key == userKey && c.Department.Key != null).ToList();
                    }
                    else
                    {
                    customerModels = context.Customers.Include(c => c.CustomerType).Include(c => c.Status).Include(c => c.Department)
                                    .Include(c => c.SystemUser)
                                    .Where(c => c.SystemUserId != null
                                    && !c.IsActive && c.IsDeleted).ToList();
                    }
                    var profilePendingApproval = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                    var profilePendingDocuments = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingDocuments);
                    var profileActive = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);

                    customers = customerModels.OrderBy(l => l.Id).ToList();
               

                    if (profilePendingApproval == null) throw new Exception("Invalid status");
                    if (profilePendingDocuments == null) throw new Exception("Invalid status");
                    if (profileActive == null) throw new Exception("Invalid status");
                    return View(customers);
                }
                catch (Exception)
                {
                    return View("_Error");
                }

                //var rows = new List<object>();
                
                //var total = 0;
                //var skip = (pageNumber - 1) * pageSize;
                //const string goLiveDate = "2019/07/01";
                //var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
                //var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);

                //var customerModels = context.Customers.Include(c => c.CustomerType).Include(c => c.Status)
                //                     .Include(c => c.SystemUser)
                //                     .Where(c => c.SystemUserId != null
                //                     && !c.IsActive && c.IsDeleted);

                //var profilePendingApproval = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                //var profilePendingDocuments = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingDocuments);
                //var profileActive = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);

                //if (profilePendingApproval == null) throw new Exception("Invalid status");
                //if (profilePendingDocuments == null) throw new Exception("Invalid status");
                //if (profileActive == null) throw new Exception("Invalid status");

                //if (!string.IsNullOrEmpty(searchText))
                //{
                //    customers = customerModels.GroupJoin(
                //    context.Entities,
                //    customer => customer.Id,
                //    entity => entity.CustomerId,
                //    (customer, entity) => new { Customer = customer, Entity = entity })
                //    .SelectMany(
                //        x => x.Entity.DefaultIfEmpty(),
                //       (x, y) => new { Customer = x.Customer, Entity = y })
                //    .Where(ce => (ce.Customer.FirstName ?? "").Contains(searchText)
                //    || (ce.Customer.LastName ?? "").Contains(searchText)
                //    || (ce.Customer.IdentificationNumber ?? "").Contains(searchText)
                //    || (ce.Customer.EmailAddress ?? "").Contains(searchText)
                //    || (ce.Customer.CustomerType.Name ?? "").Contains(searchText)
                //    || (ce.Entity.EntityRegistrationNumber ?? "").Contains(searchText))
                //    .Select(c => c.Customer).Include(c => c.SystemUser)
                //    .Include(c => c.Status).Include(c => c.CustomerType)
                //    .ToList();

                //    total = customers.Count();
                //    customers = customers.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                //}
                //else
                //{
                //    customers = customerModels.OrderBy(l => l.Id)
                //     .Skip(skip).Take(pageSize).ToList();
                //    total = customerModels.Count();
                //}

                //foreach (var customer in customers)
                //{
                //    if (customer.SystemUser == null) throw new Exception("Invalid System user");

                //    var identificationNumber = "";

                //    switch (customer.CustomerType.Key)
                //    {
                //        case CustomerTypeKeys.Entity:
                //            var entity = context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id);
                //            if (entity != null)
                //                identificationNumber = entity.EntityRegistrationNumber;
                //            break;
                //        case CustomerTypeKeys.ManagingAgent:
                //            var agent = context.Agents.FirstOrDefault(a => a.CustomerId == customer.Id);

                //            if (agent != null)
                //            {
                //                var entityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);

                //                identificationNumber = (entityAgent != null)
                //                    ? context.Entities.Find(entityAgent.EntityId).EntityRegistrationNumber
                //                    : context.Entities.First(e => e.CustomerId == agent.CustomerId).EntityRegistrationNumber;
                //            }
                //            break;
                //        default:
                //            identificationNumber = customer.IdentificationNumber;
                //            break;
                //    }

                    //rows.Add(new
                    //{
                    //    Id = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", customer.Id))),
                    //    FullName = customer.FirstName + " " + customer.LastName,
                    //    IDNumber = identificationNumber,
                    //    EmailAddress = customer.EmailAddress,
                    //    CustomerType = customer.CustomerType.Name,
                    //    DateCreated = String.Format("{0:yyyy/MM/dd}", customer.CreatedDateTime),
                    //    ProfileStatus = customer.Status.Name,
                    //    //AccountStatus = status,
                    //    CustomerId = customer.Id,
                    //    SystemUserId = customer.SystemUserId
                    //});
                

       
            }
       
        }
        #endregion

        #region Customer Load Deactivated Customers GET
        [HttpGet]
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public JsonResult LoadDeactivatedCustomers(string startDate, string endDate, int pageNumber, int pageSize, string searchText)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                var rows = new List<object>();
                List<Customer> customers;
                var total = 0;
                var skip = (pageNumber - 1) * pageSize;
                const string goLiveDate = "2019/07/01";
                var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
                var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);

                var customerModels = context.Customers.Include(c => c.CustomerType).Include(c => c.Status)
                                     .Include(c => c.SystemUser)
                                     .Where(c => c.SystemUserId != null
                                     && !c.IsActive && c.IsDeleted);

                var profilePendingApproval = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                var profilePendingDocuments = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingDocuments);
                var profileActive = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);

                if (profilePendingApproval == null) throw new Exception("Invalid status");
                if (profilePendingDocuments == null) throw new Exception("Invalid status");
                if (profileActive == null) throw new Exception("Invalid status");

                if (!string.IsNullOrEmpty(searchText))
                {
                    customers = customerModels.GroupJoin(
                    context.Entities,
                    customer => customer.Id,
                    entity => entity.CustomerId,
                    (customer, entity) => new { Customer = customer, Entity = entity })
                    .SelectMany(
                        x => x.Entity.DefaultIfEmpty(),
                       (x, y) => new { Customer = x.Customer, Entity = y })
                    .Where(ce => (ce.Customer.FirstName ?? "").Contains(searchText)
                    || (ce.Customer.LastName ?? "").Contains(searchText)
                    || (ce.Customer.IdentificationNumber ?? "").Contains(searchText)
                    || (ce.Customer.EmailAddress ?? "").Contains(searchText)
                    || (ce.Customer.CustomerType.Name ?? "").Contains(searchText)
                    || (ce.Entity.EntityRegistrationNumber ?? "").Contains(searchText))
                    .Select(c => c.Customer).Include(c => c.SystemUser)
                    .Include(c => c.Status).Include(c => c.CustomerType)
                    .ToList();

                    total = customers.Count();
                    customers = customers.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    customers = customerModels.OrderBy(l => l.Id)
                     .Skip(skip).Take(pageSize).ToList();
                    total = customerModels.Count();
                }

                foreach (var customer in customers)
                {
                    if (customer.SystemUser == null) throw new Exception("Invalid System user");
                   
                    var identificationNumber = "";

                    switch (customer.CustomerType.Key)
                    {
                        case CustomerTypeKeys.Entity:
                            var entity = context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id);
                            if (entity != null)
                                identificationNumber = entity.EntityRegistrationNumber;
                            break;
                        case CustomerTypeKeys.ManagingAgent:
                            var agent = context.Agents.FirstOrDefault(a => a.CustomerId == customer.Id);

                            if (agent != null)
                            {
                                var entityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);

                                identificationNumber = (entityAgent != null)
                                    ? context.Entities.Find(entityAgent.EntityId).EntityRegistrationNumber
                                    : context.Entities.First(e => e.CustomerId == agent.CustomerId).EntityRegistrationNumber;
                            }
                            break;
                        default:
                            identificationNumber = customer.IdentificationNumber;
                            break;
                    }

                    rows.Add(new
                    {
                        Id = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", customer.Id))),
                        FullName = customer.FirstName + " " + customer.LastName,
                        IDNumber = identificationNumber,
                        EmailAddress = customer.EmailAddress,
                        CustomerType = customer.CustomerType.Name,
                        DateCreated = String.Format("{0:yyyy/MM/dd}", customer.CreatedDateTime),
                        ProfileStatus = customer.Status.Name,
                        //AccountStatus = status,
                        CustomerId = customer.Id,
                        SystemUserId = customer.SystemUserId
                    });
                }

                return Json(new { total, rows }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Customer Locked Users GET
        // GET: /Customer/
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public ActionResult LockedUsers()
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

        #region Customer Load Locked Customers GET
        [HttpGet]
       [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Rates Clearance Senior Clerk"+ "," +  "Back Office System Administrator")]
        public JsonResult LoadLockedCustomers(string startDate, string endDate, int pageNumber, int pageSize, string searchText)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                var rows = new List<object>();
                List<SystemUser> customers;
                var total = 0;
                var skip = (pageNumber - 1) * pageSize;
                const string goLiveDate = "2019/07/01";
                var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
                var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);

             
                var customerModels = db.SystemUsers.Where(x=>x.IsLocked==true && x.IsActive==true && x.IsDeleted==false);
                

                var profilePendingApproval = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingApproval);
                var profilePendingDocuments = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerPendingDocuments);
                var profileActive = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);

                if (profilePendingApproval == null) throw new Exception("Invalid status");
                if (profilePendingDocuments == null) throw new Exception("Invalid status");
                if (profileActive == null) throw new Exception("Invalid status");

                if (!string.IsNullOrEmpty(searchText))
                {
                    customers = customerModels.Where(ce => (ce.FirstName ?? "").Contains(searchText)
                    || (ce.EmailAddress ?? "").Contains(searchText)).ToList();
                    total = customers.Count();
                    customers = customers.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    customers = customerModels.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                    total = customerModels.Count();
                }

                foreach (var customer in customers)
                {
                    if (customer == null) throw new Exception("Invalid System user");
                    
                    var identificationNumber = "";

                    //switch (customer.CustomerType.Key)
                    //{
                    //    case CustomerTypeKeys.Entity:
                    //        var entity = context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id);
                    //        if (entity != null)
                    //            identificationNumber = entity.EntityRegistrationNumber;
                    //        break;
                    //    case CustomerTypeKeys.ManagingAgent:
                    //        var agent = context.Agents.FirstOrDefault(a => a.CustomerId == customer.Id);

                    //        if (agent != null)
                    //        {
                    //            var entityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);

                    //            identificationNumber = (entityAgent != null)
                    //                ? context.Entities.Find(entityAgent.EntityId).EntityRegistrationNumber
                    //                : context.Entities.First(e => e.CustomerId == agent.CustomerId).EntityRegistrationNumber;
                    //        }
                    //        break;
                    //    default:
                    //        identificationNumber = customer.IdentificationNumber;
                    //        break;
                    //}

                    rows.Add(new
                    {
                        Id = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", customer.Id))),
                        FullName = customer.FirstName + " " + customer.LastName,
                        IDNumber = identificationNumber,
                        EmailAddress = customer.EmailAddress,
                        DateCreated = String.Format("{0:yyyy/MM/dd}", customer.CreatedDateTime),
                        CustomerId = customer.Id
                    });
                }

                return Json(new { total, rows }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region Activate User Account
        //AJC.20191021 Unlock User Account
        [EncryptedActionParameter]
        public ActionResult UnlockAccount(int id)
        {
            try
            {
                //var result = "failed";
                using (var context = new eServicesDbContext())
            {

                    // Unlock System User
                var systemUser = context.SystemUsers.Where(x=>x.Id==id).FirstOrDefault();
                systemUser.IsLocked = false;
                systemUser.IsActive = true;
                systemUser.IsDeleted = false;
                db.Entry(systemUser).State = EntityState.Modified;
                db.SaveChanges();

                var user = UserManager.FindByName(systemUser.UserName.Trim());

                //Removed isLocked and ensure account is active
                user.SystemUser.IsLocked = false;
                user.SystemUser.IsActive = true;
                user.SystemUser.IsDeleted = false;
                user.LockoutEnabled = false;
                UserManager.Update(user);
                db.SaveChanges();

            }
            return RedirectToAction("LockedUsers", "Customer");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion



        #region
        public void BulkResendActivationEmails()
        {
            try
            {
                using (var eServicesContext = new eServicesDbContext())
                {
                    var inactiveUsers = eServicesContext.Users.Where(a => a.EmailConfirmed == false).ToList();

                    foreach (var user in inactiveUsers)
                    {
                        SendActivationEmail(user.SystemUserId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
