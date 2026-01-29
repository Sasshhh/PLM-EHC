using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class LeaseCaptureSheet: BaseModel
    {
        [Display(Name ="Lease Referrence No.")]
        [Column(Order = 10)]
        public string LeaseReferenceNo { get; set; }

        [Display(Name = "Tenant Code")]
        [Column(Order = 11)]
        public string TenantCode { get; set; }

        [Display(Name ="Property Code")]
        [Column(Order = 12)]
        public string PropertyCode { get; set; }

        [Display(Name = "High Level Property Code")]
        [Column(Order = 13)]
        public string HighLevelPropertyCode { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Solar Account Number")]
        public string SolarAccountNumber { get; set; }

        [Display(Name = "Property Officer")]
        [Column(Order = 15)]
        public string PropertyOfficerName { get; set; }

        [Display(Name = "Signiture")]
        [Column(Order = 16)]
        public string Signiture { get; set; }


        //tenant details: Domicile
       
        [Display(Name = "Tenant Name")]
        [Column(Order = 17)]
        public string TenantName { get; set; }

        [Display(Name = "Tenant Type")]
        [Column(Order = 18)]
        public string TenantType { get; set; }

        [Display(Name = "Company Registration No")]
        [Column(Order = 19)]
        public string CompanyRegistrationNo { get; set; 

        }[Display(Name = "VAT Registration No")]
        [Column(Order = 20)]
        public string VAtRegistrationNo { get; set; }

        [Display(Name ="ID No.")]
        [Column(Order = 21)]
        public string IDNo { get; set; }

        [Display(Name ="Building Number & Name")]
        [Column(Order = 22)]
        public string BuildingNumberName { get; set; }

        //Tenant Details: Invoicing

        [Display(Name = "Contact Name")]
        [Column(Order = 23)]
        public string ContactName { get; set; }

        [Display(Name = "Contact Surname")]
        [Column(Order = 24)]
        public string ContactSurname { get; set; }

        [Display(Name = "Representative C/O")]
        [Column(Order = 25)]
        public string RepresentativeCO { get; set; }

        

        //Lease Negotiations
        [Display(Name = "Contact Name")]
        [Column(Order = 26)]
        public string LeaNegContactName { get; set; }

        [Display(Name = "Contact Surname")]
        [Column(Order = 27)]
        public string LeaNegContactSurname { get; set; }

        [Display(Name = "Representative C/O")]
        [Column(Order = 28)]
        public string LeaNegRepresentativeCO { get; set; }

        [Display(Name = "CCA")]
        [Column(Order = 29)]
        public string CCA { get; set; }

        [Display(Name = "Lattitude")]
        [Column(Order = 30)]
        public string Lattitude { get; set; }

        [Display(Name = "Longitude")]
        [Column(Order = 31)]
        public string Longitude { get; set; }

        [Display(Name = "RED Reference No.")]
        [Column(Order = 32)]
        public string REDReferenceNo { get; set; }

        [Display(Name = "Client Advertising Sign Reference No.")]
        [Column(Order = 33)]
        public string ClientAdvertisingSignReferenceNo { get; set; }


        [Display(Name = "City Panningre ference No")]
        [Column(Order = 34)]
        public string CityPanningreferenceNo { get; set; }

        [Display(Name = "Building Name")]
        [Column(Order = 35)]
        public string BuildingName { get; set; }

        [Display(Name = "Usage Of Premises")]
        [Column(Order = 36)]
        public string UsageOfPremises { get; set; }

        [Display(Name = "Area Leased (m^2)")]
        [Column(Order = 37)]
        public string AreaLeased { get; set; }

        //lease details

        [Display(Name = "Lease Classification")]
        [Column(Order = 38)]
        public string LeaseClassification { get; set; }

        [Display(Name = "Admin Officer Collection")]
        [Column(Order = 39)]
        public string AdminOfficerCollection { get; set; }

        [Display(Name = "Lease Start Date")]
        [Column(Order = 40)]
        public string LeaseStartDate { get; set; }

        [Display(Name = "Lease End Date")]
        [Column(Order = 41)]
        public string LeaseEndDate { get; set; }

        [Display(Name = "Date Of First Rental Escalation")]
        [Column(Order = 42)]
        public string DateOfFirstRentalEscalation { get; set; }

        [Display(Name = "Commencement Rental")]
        [Column(Order = 43)]
        public string CommencementRental { get; set; }

        [Display(Name = "Legal Action Taken")]
        [Column(Order = 44)]
        public string LegalActionTaken { get; set; }

        [Display(Name = "Deposit Paid")]
        [Column(Order = 45)]
        public string DepositPaid { get; set; }

        [Display(Name = "Billing Address")]
        [Column(Order = 46)]
        public string BillingAddress { get; set; }
    }
}