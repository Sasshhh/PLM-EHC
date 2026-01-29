using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.CorrespondenceSoapService;
using C8.eServices.Mvc.Helpers;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class CorrespondenceController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        private PortTypeClient service = new PortTypeClient();
        // GET: /Correspondence/
        //public ActionResult Index()
        //{
        //    var correspondences = db.Correspondences.Include(c => c.TitleType);
        //    return View(correspondences.ToList());
        //}

        // GET: /Correspondence/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Correspondence correspondence = db.Correspondences.Find(id);
        //    if (correspondence == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(correspondence);
        //}

        // GET: /Correspondence/Create
        public ActionResult Create()
        {
            //TODO: Retrive from Db
            var serviceTypes = new List<String> { "Query", "Complaint", "Compliment" };

            ViewBag.ServiceTypes = new SelectList(serviceTypes);
            ViewBag.TitleTypeId = new SelectList(db.TitleTypes, "Id", "Name");
            return View();
        }
        public ActionResult AshCorresponds()
        {
          
            return View();
        }

        // POST: /Correspondence/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ReferenceNumber,ServiceType,Category,SubCategory,ServiceDescription,ServiceUnitNumber," +
                                                   "ServiceFloorNumber,ServiceSectionNumber,ServiceBuildingComplex,ServiceAddress1,ServiceAddress2," +
                                                   "ServiceAddress3,ServiceAddress4,ServiceAddress5,ServiceXCoordinate,ServiceyCoordinatre,TitleTypeId," +
                                                   "FirstName,LastName,IdentificationNumber,WorkTelephoneNumber,HomeTelephoneNumber,CellPhoneNumber," +
                                                   "FaxNumber,EmailAddress,Relationship,StatusNotification,CorrespondanceNotification,WriterPhysicalAddress1," +
                                                   "WriterPhysicalAddress2,WriterPhysicalAddress3,WriterPhysicalAddress4,WriterPhysicalAddress5,WriterPostalAddress1," +
                                                   "WriterPostalAddress2,WriterPostalAddress3,WriterPostalAddress4,CustomerType,CustomerTitleId,CustomerFirstName," +
                                                   "CustomerLastName,CustomerIdentificationNumber,AccountNumber,RatesNumber,ConnectionNumber," +
                                                   "CustomerWorkTelephoneNumber,CustomerHomeTelephoneNumber,CustomerCellPhoneNumber,CustomerFaxNumber," +
                                                   "CustomerEmailAddress,BusinessName,BusinessRegistrationNumber,CustomerPhysicalAddress1,CustomerPhysicalAddress2," +
                                                   "CustomerPhysicalAddress3,CustomerPhysicalAddress4,CustomerPhysicalAddress5,CustomerPostalAddress1," +
                                                   "CustomerPostalAddress2,CustomerPostalAddress3,CustomerPostalAddress4,ServiceAddressType")] Correspondence correspondence,
            bool customerPostSameAsPhysical, bool writerPostSameAsPhysical, bool serviceAddressSameAsCustomer, bool isNewCustomer, bool isWriterAnonymous)
        {
            var captchaHelper = new CaptchaHelper();
            var captchaResponse = Request.Params["g-recaptcha-response"];

            if (captchaHelper.ValidateCaptcha(captchaResponse))
            {
                #region Initilise
                //Testing 
                //correspondence = new Correspondence();

                var customerTitle = db.TitleTypes.Find(correspondence.CustomerTitleId);
                var writerTitle = db.TitleTypes.Find(correspondence.TitleTypeId);

                if (null == customerTitle) throw new Exception("Invalid/ missing title");
                if (null == writerTitle) throw new Exception("Invalid/ missing title");

                var request = new CaseStartOffline_Request()
                {
                    CASE_DETAIL = new CASE_DETAIL
                    {
                        ReferenceNumber = correspondence.ReferenceNumber
                    },
                    SERVICE_DETAILS = new SERVICE_DETAILS()
                    {
                        Category = correspondence.Category,
                        ServiceDescription = correspondence.ServiceDescription,
                        ServiceType = correspondence.ServiceType,
                        SubCategory = correspondence.Category
                    },
                    SERVICE_ADDRESS_DETAILS = new SERVICE_ADDRESS_DETAILS()
                    {
                        BuildingComplex = correspondence.ServiceBuildingComplex,
                        CityTown = serviceAddressSameAsCustomer ? correspondence.CustomerPhysicalAddress4 : correspondence.ServiceAddress4,
                        FloorNo = correspondence.ServiceFloorNumber,
                        SameAsCustomer = serviceAddressSameAsCustomer ?
                                         new SameAsCustomer() { Yes = true, No = false } :
                                         new SameAsCustomer() { Yes = false, No = true },
                        SectionNo = correspondence.ServiceSectionNumber,
                        StreetCode = serviceAddressSameAsCustomer ? correspondence.CustomerPhysicalAddress5 : correspondence.ServiceAddress5,
                        StreetName = serviceAddressSameAsCustomer ? correspondence.CustomerPhysicalAddress1 : correspondence.ServiceAddress2,
                        StreetNo = serviceAddressSameAsCustomer ? correspondence.CustomerPhysicalAddress1 : correspondence.ServiceAddress1,
                        SuburbDistrict = serviceAddressSameAsCustomer ? correspondence.CustomerPhysicalAddress3 : correspondence.ServiceAddress3,
                        UnitNo = correspondence.ServiceUnitNumber,
                        XCoordinates = correspondence.ServiceXCoordinate,
                        YCoordinates = correspondence.ServiceYCoordinate
                    },
                    WRITER_DETAILS = new WRITER_DETAILS()
                    {
                        Anonymous = isWriterAnonymous ?
                                    new Anonymous() { Yes = true, No = false } :
                                    new Anonymous() { Yes = false, No = true },
                        CellNo = correspondence.CellPhoneNumber,
                        CorrespondenceNotification = correspondence.CorrespondanceNotification,
                        EmailAddress = correspondence.EmailAddress,
                        FaxNo = correspondence.FaxNumber,
                        IDNo = correspondence.IdentificationNumber,
                        Name = correspondence.FirstName,
                        RelationToCustomer = correspondence.Relationship,
                        StatusNotification = correspondence.StatusNotification,
                        Surname = correspondence.LastName,
                        TelHomeNo = correspondence.HomeTelephoneNumber,
                        TelWorkNo = correspondence.WorkTelephoneNumber,
                        Title = writerTitle.Name,
                    },
                    WRITER_ADDRESS_DETAILS = new WRITER_ADDRESS_DETAILS()
                    {
                        Writer_PostSameAsPhysical = writerPostSameAsPhysical ?
                                                new Writer_PostSameAsPhysical() { Yes = true, No = false } :
                                                new Writer_PostSameAsPhysical() { Yes = false, No = true },
                        WRITER_POSTAL = new WRITER_POSTAL()
                        {
                            CityTown = writerPostSameAsPhysical ? correspondence.WriterPhysicalAddress4 : correspondence.WriterPostalAddress3,
                            PostalBox = writerPostSameAsPhysical ? "" : correspondence.WriterPostalAddress2,
                            PostalCode = writerPostSameAsPhysical ? correspondence.WriterPhysicalAddress5 : correspondence.WriterPostalAddress4,
                            SuburbDistrict = writerPostSameAsPhysical ? correspondence.WriterPhysicalAddress3 : correspondence.WriterPostalAddress2
                        },
                        StreetNo = correspondence.WriterPhysicalAddress1,
                        StreetName = correspondence.WriterPhysicalAddress2,
                        SuburbDistrict = correspondence.WriterPhysicalAddress3,
                        CityTown = correspondence.WriterPhysicalAddress4,
                        StreetCode = correspondence.WriterPhysicalAddress5,
                        BuildingComplex = "",
                        FloorNo = "",
                        SectionNo = ""
                    },
                    CUSTOMER_DETAILS = new CUSTOMER_DETAILS
                    {
                        NewOrExistingCustomer = isNewCustomer ?
                                        new NewOrExistingCustomer() { Yes = true, No = false } :
                                        new NewOrExistingCustomer() { Yes = false, No = true },
                        Type = "",
                        Title = customerTitle.Name,
                        Name = correspondence.CustomerFirstName,
                        Surname = correspondence.CustomerLastName,
                        IDNo = correspondence.CustomerIdentificationNumber,
                        AccountNumber = correspondence.AccountNumber,
                        RatesNumber = correspondence.RatesNumber,
                        ConnectionNumber = correspondence.ConnectionNumber,
                        PassportNumber = "",
                        TelWorkNumber = correspondence.CustomerWorkTelephoneNumber,
                        TelHomeNumber = correspondence.CustomerHomeTelephoneNumber,
                        CellphoneNo = correspondence.CustomerCellPhoneNumber,
                        FaxNumber = correspondence.CustomerFaxNumber,
                        EmailAddress = correspondence.CustomerEmailAddress,
                        BusinessName = correspondence.BusinessName,
                        RegistrationNumber = correspondence.BusinessRegistrationNumber
                    },
                    CUSTOMER_ADDRESS_DETAILS = new CUSTOMER_ADDRESS_DETAILS
                    {
                        SameAsPhysical = customerPostSameAsPhysical ?
                                        new SameAsPhysical() { Yes = true, No = false } :
                                        new SameAsPhysical() { Yes = false, No = true },
                        StreetNo = correspondence.CustomerPhysicalAddress1,
                        StreetName = correspondence.CustomerPhysicalAddress2,
                        SuburbDistrict = correspondence.CustomerPhysicalAddress3,
                        CityTown = correspondence.CustomerPhysicalAddress4,
                        BuildingComplex = "",
                        FloorNo = "",
                        Post = new Post()
                        {
                            CityTown = customerPostSameAsPhysical ? correspondence.CustomerPhysicalAddress3 : correspondence.CustomerPostalAddress1,
                            PostalBox = customerPostSameAsPhysical ? "" : correspondence.CustomerPostalAddress1,
                            Code = customerPostSameAsPhysical ? correspondence.CustomerPhysicalAddress4 : correspondence.CustomerPostalAddress3,
                            SuburbDistrict = customerPostSameAsPhysical ? correspondence.CustomerPhysicalAddress2 : correspondence.CustomerPostalAddress4
                        },
                        SectionNo = "",
                        StreetCode = ""
                    }
                };

                #endregion

                var xmlObject = ObjectToXml(request);
                var result = service.CaseStart_OfflineForm_Operation(request);

                if (result.Status.StatusMsg == "SUCCESS")
                {
                    ViewBag.Success =
                        string.Format("Your query has been successfully logged. Your reference number is {0}",
                            result.ReferenceNo);
                }
                else
                {
                    ViewBag.Error = "Error occured while trying to process your query";
                }
                //db.Correspondences.Add(correspondence);
                //db.SaveChanges();
                // return RedirectToAction("Index");
            }

            //TODO: Retrive from Db
            var serviceTypes = new List<String> { "Query", "Complaint", "Compliment" };

            ViewBag.ServiceTypes = new SelectList(serviceTypes);
            ViewBag.TitleTypeId = new SelectList(db.TitleTypes, "Id", "Name", correspondence.TitleTypeId);
            return View(correspondence);
        }

        public static string ObjectToXml<T>(T objectToSerialise)
        {
            StringWriter Output = new StringWriter(new StringBuilder());
            XmlSerializer xs = new XmlSerializer(objectToSerialise.GetType());
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
            ns.Add("sch", "http://www.tibco.com/schemas/CaseStart_OfflineFormService/SharedResources/Schemas/Schema.xsd2"); // add as many or few as you need
            xs.Serialize(Output, objectToSerialise, ns);
            return Output.ToString();
        }
        // GET: /Correspondence/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Correspondence correspondence = db.Correspondences.Find(id);
        //    if (correspondence == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.TitleTypeId = new SelectList(db.TitleTypes, "Id", "Name", correspondence.TitleTypeId);
        //    return View(correspondence);
        //}

        // POST: /Correspondence/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include="Id,ReferenceNumber,ServiceType,Category,SubCategory,ServiceUnitNumber,ServiceFloorNumber,ServiceSectionNumber,ServiceBuildingComplex,ServiceAddress1,ServiceAddress2,ServiceAddress3,ServiceAddress4,ServiceAddress5,ServiceXCoordinate,ServiceyCoordinatre,TitleTypeId,FirstName,LastName,IdentificationNumber,WorkTelephoneNumber,HomeTelephoneNumber,CellPhoneNumber,FaxNumber,EmailAddress,Relationship,StatusNotification,CorrespondenceNotification,WriterPhysicalAddress1,WriterPhysicalAddress2,WriterPhysicalAddress3,WriterPhysicalAddress4,WriterPhysicalAddress5,WriterPostalAddress1,WriterPostalAddress2,WriterPostalAddress3,WriterPostalAddress4,CustomerType,CustomerTitle,CustomerFirstName,CustomerLastName,CustomerIdentificationNumber,AccountNumber,RatesNumber,ConnectionNumber,CustomerWorkTelephoneNumber,CustomerHomeTelephoneNumber,CustomerCellPhoneNumber,CustomerFaxNumber,CustomerEmailAddress,BusinessName,BusinessRegistrationNumber,CustomerPhysicalAddress1,CustomerPhysicalAddress2,CustomerPhysicalAddress3,CustomerPhysicalAddress4,CustomerPhysicalAddress5,CustomerPostalAddress1,CustomerPostalAddress2,CustomerPostalAddress3,CustomerPostalAddress4,ServiceAddressType")] Correspondence correspondence)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(correspondence).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.TitleTypeId = new SelectList(db.TitleTypes, "Id", "Name", correspondence.TitleTypeId);
        //    return View(correspondence);
        //}

        // GET: /Correspondence/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Correspondence correspondence = db.Correspondences.Find(id);
        //    if (correspondence == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(correspondence);
        //}

        // POST: /Correspondence/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Correspondence correspondence = db.Correspondences.Find(id);
        //    db.Correspondences.Remove(correspondence);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
