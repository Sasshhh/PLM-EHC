using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class MatchedUnits: BaseModel
    {

        [Column(Order = 10)]
        [Display(Name = "Lease Details")]
        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Units")]
        public int? UnitsId { get; set; }
        [ForeignKey("UnitsId")]
        public Units Units { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Property Lease Application")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Units")]
        public string LeaseReferenceNo  { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Property Lease Application")]
        public string PropertyLeaseReferenceNo { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Accepted Status")]
        public bool IsAccepted { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order = 17)]
        [Display(Name = "SystemUser")]
        public int? SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public SystemUser SystemUser { get; set; }

        [Column(Order = 18)]
        [Display(Name = "Rejected Status")]
        public bool RejectedProperty { get; set; }

        [Column(Order = 19)]
        [Display(Name = "UnitsEkurhuleniHousingCompany")]
        public int? UnitsEkurhuleniHousingCompanyId { get; set; }
        [ForeignKey("UnitsEkurhuleniHousingCompanyId")]
        public UnitsEkurhuleniHousingCompany UnitsEkurhuleniHousingCompany { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Human Settlement Application")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

        [Column(Order = 21)]
        [Display(Name = "UnitsHumanSettlement01")]
        public int? UnitsHumanSettlement01Id { get; set; }
        [ForeignKey("UnitsHumanSettlement01Id")]
        public UnitsHumanSettlement01 UnitsHumanSettlement01 { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Application Allocated Property")]
        public int? ApplicationAllocatedPropertyId { get; set; }
        [ForeignKey("ApplicationAllocatedPropertyId")]
        public ApplicationAllocatedProperty ApplicationAllocatedProperty { get; set; }
    }
}