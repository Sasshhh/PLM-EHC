using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class HumanSettlementLeaseMaster : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "HumanSettlementApplication")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

        [Column(Order = 11)]
        [Display(Name = "PropertyLeaseApplication")]
        public int? MasterByUserId { get; set; }
        [ForeignKey("MasterByUserId")]
        public SystemUser MasterByUser { get; set; }

        [Display(Name = "Surname")]
        [Column(Order = 12)]
        public string LastName { get; set; }

        [Display(Name = "First Name(s)")]
        [Column(Order = 13)]
        public string FirstNames { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Tenant Type")]
        public int? PurchaserTypeId { get; set; }
        [ForeignKey("PurchaserTypeId")]
        public PurchaserType PurchaserType { get; set; }

        [Display(Name = "Start Date")]
        [Column(TypeName = "date", Order = 15)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Period in months")]
        [Column(Order = 16)]
        public int? PeriodInMonths { get; set; }

        [Display(Name = "End Date")]
        [Column(TypeName = "date", Order = 17)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Renewal Notice")]
        [Column(TypeName = "date", Order = 18)]
        public DateTime? RenewalNotice { get; set; }

        [Display(Name = "Termination Notice")]
        [Column(TypeName = "date", Order = 19)]
        public DateTime? TerminationNotice { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Deposite Amount")]
        public decimal DepositeAmount { get; set; }

        [Display(Name = "Rental Amount (Excl VAT)")]
        [Column(Order = 21)]
        public decimal RentalAmount { get; set; }

        [Display(Name = "VAT Amount ")]
        [Column(Order = 22)]
        public decimal VATAmount { get; set; }

        [Display(Name = "Total including VAT")]
        [Column(Order = 23)]
        public decimal TotalIncludingVAT { get; set; }


        [Display(Name = "Statement Date ")]
        [Column(TypeName = "date", Order = 24)]
        public DateTime? StatementDate { get; set; }

        [Display(Name = "Escalation Date")]
        [Column(TypeName = "date", Order = 25)]
        public DateTime? EscalationDate { get; set; }

        [Display(Name = "Email")]
        [Column(Order = 26)]
        public bool Email { get; set; }

        [Display(Name = "SMS")]
        [Column(Order = 27)]
        public bool SMS { get; set; }

        [Display(Name = "Postal")]
        [Column(Order = 28)]
        public bool Postal { get; set; }

        [Display(Name = "ID Number")]
        [Column(Order = 29)]
        [StringLength(25)]
        public string IDNo { get; set; }

        [Display(Name = "Address")]
        [Column(Order = 3)]
        [StringLength(500)]
        public string LeasedAddress { get; set; }

        [Display(Name = "Postal Code")]
        [Column(Order = 34)]
        [StringLength(50)]
        public string LeasedPostal { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 35)]
        //[StringLength(10)]
        public string LeasedSuburb { get; set; }

        [Display(Name = "Building Name")]
        [Column(Order = 36)]
        public string buildingName { get; set; }

        [Display(Name = "Space/Unit Number")]
        [Column(Order = 38)]
        public string SpaceUnitNo { get; set; }

        [Display(Name = "Office Park Name")]
        [Column(Order = 40)]
        public string OfficeParkName { get; set; }

        [Column(Order = 41)]
        [Display(Name = "Status")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 42)]
        [Display(Name = "SystemUser")]
        public int? SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public SystemUser SystemUser { get; set; }

        [Column(Order = 43)]
        [Display(Name = "IsNew")]
        public bool IsNew { get; set; }

        [Column(Order = 44)]
        [Display(Name = "Renewed")]
        public bool IsRenewed { get; set; }

        [Column(Order = 45)]
        [Display(Name = "Details Updated")]
        public bool? DetailsUpdated { get; set; }

        [Column(Order = 46)]
        [Display(Name = "Preparation Fee")]
        public double? PreparationFee { get; set; }

        [Column(Order = 47)]
        [Display(Name = "CreditCheck Fee")]
        public double? CreditCheckFee { get; set; }

        [Column(Order = 48)]
        [Display(Name = "Calculated As Folllows")]
        public double? CalculatedAsFolllows { get; set; }

        [Column(Order = 49)]
        [Display(Name = "Initial Deposit Premises")]
        public double? InitialDepositPremises { get; set; }

        [Column(Order = 50)]
        [Display(Name = "Deposit Tenant Contribution")]
        public double? DepositTenantContribution { get; set; }

        [Column(Order = 51)]
        [Display(Name = "Shade Port Parking")]
        public double? ShadePortParking { get; set; }

        [Column(Order = 52)]
        [Display(Name = "Shade PortParking")]
        public bool? SPP { get; set; }

        [Column(Order = 53)]
        [Display(Name = "Open Parking")]
        public double? OpenParking { get; set; }

        [Column(Order = 54)]
        [Display(Name = "Open Parking")]
        public bool? OPP { get; set; }

        [Column(Order = 55)]
        [Display(Name = "Store Rooms")]
        public double? StoreRooms { get; set; }

        [Column(Order = 56)]
        [Display(Name = "Open Parking")]
        public bool? STR { get; set; }

        [Column(Order = 57)]
        [Display(Name = "Electricity")]
        public double? Electricity { get; set; }

        [Column(Order = 58)]
        [Display(Name = "Electricity")]
        public bool? ELEC { get; set; }

        [Column(Order = 59)]
        [Display(Name = "Refuse")]
        public double? Refuse { get; set; }

        [Column(Order = 60)]
        [Display(Name = "Security Fee")]
        public double? SecurityFee { get; set; }

        [Column(Order = 61)]
        [Display(Name = "Electricity")]
        public bool? SEC { get; set; }

        [Column(Order = 62)]
        [Display(Name = "Sewerage")]
        public double? Sewerage { get; set; }

        [Column(Order = 63)]
        [Display(Name = "Water")]
        public double? Water { get; set; }

        [Column(Order = 64)]
        [Display(Name = "Electricity")]
        public bool? WTR { get; set; }

        [Column(Order = 65)]
        [Display(Name = "Termination Date")]
        public DateTime? TerminationDate { get; set; }

        //[Column(Order = 66)]
        //[Column(Order = 67)]
        //[Display(Name = "Open Parking Bay Number")]
        //public string OPenParkingBayNumber { get; set; }

        //[Column(Order = 68)]
        //[Display(Name = "Shade Port Bay Number")]
        //public string ShadePortBayNumber { get; set; }

        [Column(Order = 69)]
        [Display(Name = "Open Parking Bay")]
        public double? CarportParkingBay { get; set; }

        //[Column(Order = 70)]
        //[Display(Name = "Administraion Fee")]
        //public double? LeaseAdministrationFee { get; set; }

        //[Column(Order = 71)]
        //[Display(Name = "Floor Number")]
        //public string FloorNumber { get; set; }

        [Column(Order = 72)]
        [Display(Name = "Total Monthly Charges")]
        public double? TotalMonthlyCharges { get; set; }

        //[Column(Order = 73)]
        //[Display(Name = "Applicant Comment")]
        //public string ApplicantComment { get; set; }

        [Column(Order = 74)]
        [Display(Name = "Months Offered")]
        public int MonthsOffered { get; set; }


        [Column(Order = 75)]
        [Display(Name = "Completed")]
        public bool Completed { get; set; }

        [Column(Order = 76)]
        [Display(Name = "Deposit Held")]
        public double? DepositHeld { get; set; }

        [Column(Order = 77)]
        [Display(Name = "Notice Date")]
        public DateTime? NoticeDate { get; set; }

        [Column(Order = 78)]
        [Display(Name = "Termination Reminder")]
        public DateTime? TerminationReminder { get; set; }

        [Column(Order = 79)]
        [Display(Name = "Solar Reference")]
        public string Solarreference { get; set; }
    }
}