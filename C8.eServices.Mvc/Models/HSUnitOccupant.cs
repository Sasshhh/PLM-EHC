using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class HSUnitOccupant : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "First Name")]
        [MaxLength(30)]
        public string FirstName { get; set; }

        [Column(Order = 11)]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Human Settlement Application")]
        public int HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Human Settlement LeaseDetails")]
        public int? HumanSettlementLeaseDetailsId { get; set; }
        [ForeignKey("HumanSettlementLeaseDetailsId")]
        public HumanSettlementLeaseDetails HumanSettlementLeaseDetails { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Human Settlement LeaseMaster")]
        public int? HumanSettlementLeaseMasterId { get; set; }
        [ForeignKey("HumanSettlementLeaseMasterId")]
        public HumanSettlementLeaseMaster HumanSettlementLeaseMaster { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Identification Number")]
        [MaxLength(100)]
        public string IDNo { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Cell No.")]
        [MaxLength(100)]
        public string CellNo { get; set; }

        [Column(Order = 18)]
        [Display(Name = "Email")]
        [MaxLength(100)]
        //[EmailAddress]
        public string Email { get; set; }

        [Column(Order = 19)]
        [Display(Name = "Relationship")]
        [MaxLength(100)]
        public string Relationship { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Title")]
        public int TitleTypeId { get; set; }
        [ForeignKey("TitleTypeId")]
        public TitleType TitleType { get; set; }

        [Column(Order = 21)]
        [Display(Name = "Nominated")]
        public bool IsNominated { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Full Name")]
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }
    }
}