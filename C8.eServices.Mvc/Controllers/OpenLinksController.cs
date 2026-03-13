using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace C8.eServices.Mvc.Controllers
{
    public class OpenLinksController : Controller
    {
        // GET: OpenLinks
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [EncryptedActionParameter]
        public ActionResult RefundProcessDocuments(int? id)
        {
            var dvm = new DepartmentsApprovalViewModel();
            ViewBag.Id = id;
            ViewBag.Message = "";
            return View(dvm);
        }

        [AllowAnonymous]
        [EncryptedActionParameter]
        [HttpPost]
        public ActionResult RefundProcessDocuments(int? id, string Password)
        {
            var dvm = new DocumentsViewModel();
            using (var _context = new eServicesDbContext())
            {
                var rcsApps = _context.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser)
                     .Include(r => r.Customer).Include(r => r.ModifiedBySystemUser)
                     .Include(r => r.HumanEHCOptions).Include(r => r.Status)
                     .Where(x => x.Id == id).FirstOrDefault();

                
                if (rcsApps.RefundProcessPassword == Password)
                {
                    try
                    {
                        var LeaseApplication = _context.LeaseDetails.OrderByDescending(x => x.Id).Where(x => x.PropertyLeaseApplicationId == rcsApps.Id && x.IsDeleted == false).FirstOrDefault();

                        var customer = _context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                                       .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == rcsApps.CustomerId);
                        if (customer == null) throw new Exception("Invalid Customer");
                        var referenceType = _context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                        if (referenceType == null) throw new Exception("Invalid reference type.");
                        var application = _context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                        if (application == null) throw new Exception(string.Format("Invalid/ missing application key {0}", ApplicationKeys.RatesClearanceSystem));

                        DocumentsViewModel refundDocs = new DocumentsViewModel();

                        MatchingHelper.DocumentUploadBakingDetailsProof(dvm, _context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, false);
                        MatchingHelper.DocumentDepositRefunds(refundDocs, _context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsApps.Id, false);

                        // Getting Client Services Officer (mapped from legacy Letting Officer complex mapping)
                        var clientServicesOfficer = GetBackOfficeId(_context, rcsApps.Id, true);
                        var StoredUser = Convert.ToInt16(_context.AppSettings.Where(x => x.Key == AppSettingKeys.LettingOfficer).FirstOrDefault().Value);
                        var activeDirectoryOn = clientServicesOfficer.Id != 0 ? clientServicesOfficer.Id : StoredUser;
                        var customerLettingOfficer = _context.Customers.Include(s => s.SystemUser).Include(s => s.Status)
                                   .Include(s => s.CustomerType).FirstOrDefault(c => c.Id == activeDirectoryOn);
                        if (customerLettingOfficer == null) throw new Exception("Invalid Client Services Officer");
                        var LeaseTermination = _context.LeaseTerminations.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == LeaseApplication.PropertyLeaseApplicationId);

                        var vm = new DepartmentsApprovalViewModel
                        {
                            PropertyLeaseApplications = rcsApps,
                            DocumentsViewModel = dvm,
                            DocumentsViewModelRefundDeposits = refundDocs,
                            LeaseTermination = LeaseTermination,
                            LeaseDetails = LeaseApplication
                        };
                        vm.Customer = customerLettingOfficer;
                        vm.PropertyLeaseApplications = rcsApps;
                        vm.LeaseDetails = LeaseApplication;
                        ViewBag.ApplicationId = application.Id;
                        ViewBag.Message = "success";
                        return View(vm);
                    }
                    catch (Exception ex)
                    {
                        EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                        throw;
                    }
                }
                else
                {
                    ViewBag.Message = "Please enter a valid Password";
                }
            }
            ViewBag.Id = id;
            return View(new DepartmentsApprovalViewModel());
        }
        private static Customer GetBackOfficeId(eServicesDbContext core, int Id, bool LF)
        {
            Customer UserId = new Customer();
            var AppUnit = core.ApplicantUnits.FirstOrDefault(x => x.PropertyLeaseApplicationId == Id);
            var Match = AppUnit != null ? core.MatchedUnits.FirstOrDefault(x => x.Id == AppUnit.MatchedID) : null;
            var Unit = Match != null ? core.Units.FirstOrDefault(x => x.Id == Match.UnitsId) : null;
            var Units = Match != null ? core.UnitsEkurhuleniHousingCompany.FirstOrDefault(x => x.Id == Match.UnitsEkurhuleniHousingCompanyId) : null;
            var pca = Units != null ? core.PreferredComplexAreas.FirstOrDefault(x => x.Id == Units.PreferredComplexAreaId) : null;
            if (pca != null)
            {
                UserId = core.Customers.FirstOrDefault(x => x.Id == pca.LettingOfficerId);
            }
            return UserId;
        }
    }
}