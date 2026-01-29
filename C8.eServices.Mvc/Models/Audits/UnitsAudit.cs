using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class UnitsAudit : BaseModelAudit
    {
        public int OccupationID { get; set; }
        public int SettlementID { get; set; }

        [Column(Order = 10)]
        [Display(Name = "UnitBuilding Name")]
        public string UnitBuildingName { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Unit Type")]
        public int? OccupationTypeId { get; set; }
        [ForeignKey("OccupationTypeId")]
        public HumanEHCOptions HumanEHCOptions { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Complex/Area")]
        public int PreferredComplexAreaId { get; set; }
        [ForeignKey("PreferredComplexAreaId")]
        public PreferredComplexArea PreferredComplexArea { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Is Taken")]
        public bool IsTaken { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Surburb")]
        public string Surburb { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Geo-Location")]
        public string GeoLocation { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Bedroom Count")]
        public int BedroomCount { get; set; }

        [Column(Order = 18)]
        [Display(Name = "Bathroom Count")]
        public int BathroomCount { get; set; }

        [Column(Order = 19)]
        [Display(Name = "Property Size")]
        public int PropertySize { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Property Price")]
        public double PropertyPrice { get; set; }

        [Column(Order = 21)]
        [Display(Name = "Property Deposit")]
        public double PropertyDeposit { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Letting Requirements")]
        public string LettingRequirements { get; set; }

        [Column(Order = 23)]
        [Display(Name = "First Name")]
        public string AgentName { get; set; }

        [Column(Order = 24)]
        [Display(Name = "Surname")]
        public string AgentLastName { get; set; }

        [Column(Order = 25)]
        [Display(Name = "Cell No.")]
        public string AgentCell { get; set; }
        [Column(Order = 26)]
        [Display(Name = "Work No.")]
        public string AgentWorkNo { get; set; }
        [Column(Order = 27)]
        [Display(Name = "ID No.")]
        public string AgentIDNo { get; set; }
        [Column(Order = 28)]
        [Display(Name = "E-mail")]
        public string AgentEmail { get; set; }
        [Column(Order = 29)]
        [Display(Name = "Space/Unit No.")]
        public string SpaceUnitNo { get; set; }

        [Column(Order = 30)]
        [Display(Name = "Postal")]
        public string Postal { get; set; }

        //billing infommation
        [Column(Order = 31)]
        [Display(Name = "Postal")]
        public double Rental { get; set; }

        [Column(Order = 32)]
        [Display(Name = "Postal")]
        public double Water { get; set; }

        [Column(Order = 33)]
        [Display(Name = "Postal")]
        public double Refuse { get; set; }

        [Column(Order = 34)]
        [Display(Name = "Postal")]
        public double Sewer { get; set; }

        [Column(Order = 35)]
        [Display(Name = "Postal")]
        public double TotalCharges { get; set; }

        [Column(Order = 36)]
        [Display(Name = "Postal")]
        public double DepositRequired { get; set; }

        [Column(Order = 37)]
        [Display(Name = "Postal")]
        public double DepositHeld { get; set; }

        [Column(Order = 38)]
        [Display(Name = "Postal")]
        public bool Inspection { get; set; }


    }
}