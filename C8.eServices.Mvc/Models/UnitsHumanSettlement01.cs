using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class UnitsHumanSettlement01 : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Building Name")]
        public string BuildingName { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Unit")]
        public string SpaceUnitNumber { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Complex/Area")]
        public int? PreferredComplexAreaId { get; set; }
        [ForeignKey("PreferredComplexAreaId")]
        public PreferredComplexArea PreferredComplexArea { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Unit Size")]
        public decimal UnitSize { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Bedroom Count")]
        public int BedroomCount { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Bathroom Count")]
        public int BathroomCount { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Property Price")]
        public double Rental { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Water")]
        public double Water { get; set; }

        [Column(Order = 18)]
        [Display(Name = "Refuse")]
        public double Refuse { get; set; }

        [Column(Order = 19)]
        [Display(Name = "Sewer")]
        public double Sewer { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Total Monthly Charges")]
        public double TotalMonthlyCharges { get; set; }

        [Column(Order = 21)]
        [Display(Name = "Deposit Required")]
        public double DepositRequired { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Deposit Held")]
        public double DepositHeld { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Property Price")]
        public bool IsTaken { get; set; }

        [Column(Order = 24)]
        [Display(Name = "EHC Options")]
        public int? HumanEHCOptionId { get; set; }
        [ForeignKey("HumanEHCOptionId")]
        public HumanEHCOptions HumanEHCOption { get; set; }

        [Column(Order = 25)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Column(Order = 26)]
        [Display(Name = "Surburb")]
        public string Surburb { get; set; }

        [Column(Order = 27)]
        [Display(Name = "Geo-Location")]
        public string GeoLocation { get; set; }

        [Column(Order = 28)]
        [Display(Name = "Property Size")]
        public int PropertySize { get; set; }

        [Column(Order = 29)]
        [Display(Name = "Property Price")]
        public double PropertyPrice { get; set; }

        [Column(Order = 30)]
        [Display(Name = "Property Deposit")]
        public double PropertyDeposit { get; set; }

        [Column(Order = 31)]
        [Display(Name = "Letting Requirements")]
        public string LettingRequirements { get; set; }

        [Column(Order = 32)]
        [Display(Name = "Space/Unit No.")]
        public string SpaceUnitNo { get; set; }

        [Column(Order = 33)]
        [Display(Name = "Postal")]
        public string Postal { get; set; }

        [Column(Order = 34)]
        [Display(Name = "Total Charges")]
        public double TotalCharges { get; set; }

        [Column(Order = 35)]
        [Display(Name = "Inspections")]
        public bool Inspection { get; set; }

        [Column(Order = 36)]
        [Display(Name = "Date Available")]
        public DateTime? DateAvailable { get; set; }

        [Display(Name = "Unit Category")]
        [Column(Order = 37)]
        public int? UnitCategoryId { get; set; }
        [ForeignKey("UnitCategoryId")]
        public HSUnitCategory UnitCategory { get; set; }

        [Display(Name = "Unit Typology")]
        [Column(Order = 38)]
        public int? UnitTypologyId { get; set; }
        [ForeignKey("UnitTypologyId")]
        public HSUnitTypology UnitTypology { get; set; }

    }
}