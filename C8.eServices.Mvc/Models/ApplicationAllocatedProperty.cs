using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace C8.eServices.Mvc.Models
{
    public class ApplicationAllocatedProperty : BaseModel
    {
        [Column(Order = 2)]
        [Display(Name = "Space Unit Size")]
        public Double SpaceUnitSize { get; set; }

        [Column(Order = 3)]
        [Display(Name = "Space Unit Number")]
        public String SpaceUnitNumber { get; set; }

        [Column(Order = 4)]
        [Display(Name = "Solar Reference")]
        public String SolarReference { get; set; }

        [Column(Order = 5)]
        [Display(Name = "Monthly Rental Amount")]
        public Double MonthlyRentalAmount { get; set; }

        [Column(Order = 6)]
        [Display(Name = "Requied Deposit Amount")]
        public Double RequiedDepositAmount { get; set; }

        [Column(Order = 7)]
        [Display(Name = "Number Of Beds")]
        public Int32 NumOfBeds { get; set; }

        [Column(Order = 8)]
        [Display(Name = "Street Name")]
        public String StreetName { get; set; }

        [Column(Order = 9)]
        [Display(Name = "Township")]
        public String Township { get; set; }

        [Column(Order = 10)]
        [Display(Name = "Postal")]
        public String Postal { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Offered Complex")]
        public Int32 OfferedComplexId { get; set; }
        [ForeignKey("OfferedComplexId")]
        public PreferredComplexArea OfferedComplex { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Allocated By User")]
        public Int32 AllocatedByUserId { get; set; }
        [ForeignKey("AllocatedByUserId")]
        public SystemUser AllocatedByUser { get; set; }

        [Column(Order = 13)]
        [Display(Name = "IsTaken")]
        public Boolean IsTaken { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Letting Requirements")]
        public String LettingRequirements { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Bathroom(s)")]
        public Int32 Shower { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Electricity Meter")]
        public String ElectricityMeter { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Key Number")]
        public String KeyNumber { get; set; }

        [NotMapped]
        public int PropertyLeaseApplicationId { get; set; }
        [NotMapped]
        public String Address { get { return String.Format("{0} {1}, {2}", StreetName, Township, Postal); } }

        [Column(Order = 18)]
        [Display(Name = "Water")]
        public double Water { get; set; }

        [Column(Order = 19)]
        [Display(Name = "Refuse")]
        public double Refuse { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Sewer")]
        public double Sewer { get; set; }

        [Column(Order = 21)]
        [Display(Name = "EHC Options")]
        public int? HumanEHCOptionId { get; set; }
        [ForeignKey("HumanEHCOptionId")]
        public HumanEHCOptions HumanEHCOption { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Total Charges")]
        public double TotalCharges { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Inspections")]
        public bool Inspection { get; set; }

        [Column(Order = 24)]
        [Display(Name = "Building Name")]
        public string BuildingName { get; set; }

        [NotMapped]
        public double TotalMonthlyCharges { get { return (MonthlyRentalAmount + Water + Refuse + Sewer); } }
    }

    public class AllocatedUnitHistory : BaseModel
    {
        [Column(Order = 2)]
        [Display(Name = "Postal")]
        public String State { get; set; }

        [Column(Order = 3)]
        [Display(Name = "Postal")]
        public String Description { get; set; }

        [Column(Order = 4)]
        [Display(Name = "Property Lease Application")]
        public int PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 5)]
        [Display(Name = "Application Allocated Property")]
        public int ApplicationAllocatedPropertyId { get; set; }
        [ForeignKey("ApplicationAllocatedPropertyId")]
        public ApplicationAllocatedProperty ApplicationAllocatedProperty { get; set; }

        [Column(Order = 6)]
        public Int32 ByLettingOfficerId { get; set; }
        [ForeignKey("ByLettingOfficerId")]
        [Display(Name = "By Letting Officer")]
        public SystemUser ByLettingOfficer { get; set; }
    }
}