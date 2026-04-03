using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class TenantViewModel : BaseModel
    {
        public PropertyResident PropertyResident { get; set; }
        public List<PropertyResident> PropertyResidentList { get; set; }
        public List<PropertyResident> OldResidents { get; set; }
        public Units Unit { get; set; }
        public ApplicantUnit ApplicantUnit { get; set; }
        public HumanSettlementApplication HumanSettlementApplication { get; set; }
        public HumanSettlementLeaseMaster HumanSettlementLeaseMaster { get; set; }
        public HumanSettlementLeaseDetails HumanSettlementLeaseDetails { get; set; }
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }
        public PropertyLeaseAgreementMaster PropertyLeaseAgreementMaster { get; set; }
        public LeaseDetails Lease { get; set; }
        public LeaseCaptureSheet LeaseCaptureSheet { get; set; }
        public LeaseCaptureAddressContact LeaseCaptureAddressContact { get; set; }
        public DocumentsViewModel DocumentsViewModel { get; set; }
        public int ViewId { get; set; }
        public int HumanSettlementLeaseMasterId { get; set; }
        public int HumanSettlementLeaseDetailsId { get; set; }
        public string WaterSessionList { get; set; }
        public string DocName { get; set; }
        public string DocDesc { get; set; }
        public string UrlReturn { get; set; }
        public string ViewName { get; set; }
        public bool _Make_tenant { get; set; }
        public bool DocumentsVerified { get; set; }
        string TenantList { get; set; }
        public DocumentsViewModel Document { get; set; }
        public UnitsEkurhuleniHousingCompany UnitsEkurhuleniHousingCompany { get; set; }
        public HSUnitOccupant HSUnitOccupant { get; set; }
        public List<HSUnitOccupant> ListHSUnitOccupants { get; set; }
        public List<HSUnitOccupant> DeacListHSUnitOccupants { get; set; }
        public ApplicationAllocatedProperty ApplicationAllocatedProperty { get; set; }
    }
    //

}
