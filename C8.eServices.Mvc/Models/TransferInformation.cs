using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace C8.eServices.Mvc.Models
{
    public class TransferInformation : BaseModel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TransferInformation()
        {
            //QueryApplicationTbs = new HashSet<QueryApplicationTb>();
        }

 

        [Column(Order = 12)]
        [MaxLength(100)]
        [Display(Name = "Type Of Property")]
        public string PropertyType { get; set; }


        [Column(Order = 13)]
        [Display(Name = "Type Of Transfer")]
        public int? TransferType { get; set; }
        [ForeignKey("TransferType")]
        public TransferType TransferTypes { get; set; }

        [Column(Order = 14)]
        [MaxLength(10)]
        [Display(Name = "Rates Number")]
        public string RatesNumber { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Selling Price")]
        public decimal? SellingPrice { get; set; }


        [Column(Order = 16)]
        [MaxLength(30)]
        [Display(Name = "Property Key")]
        public string PropertyKey { get; set; }


        [Column(Order = 17)]
      
        [Display(Name = "Organ of State")]
        public bool OrganOfState { get; set; }


        [Column(Order = 18)]
        [Display(Name = "Final date")]
        public DateTime? DateOfFinal { get; set; }

        [Column(Order = 19)]
        [Display(Name = "New Sub Confirm")]
        [MaxLength(50)]
        public string NewSubConfirm { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Consolidation")]
        [MaxLength(50)]
        public string consolidation { get; set; }

        //[StringLength(2048)]
        //[Column(Order = 10)]
        //[Display(Name = "Division Decimal")]
        //public string Division_dec { get; set; }

        //[StringLength(2048)]
        //public string Portion_dec { get; set; }

        //[StringLength(50)]
        //public string deeds_info { get; set; }


        [Column(Order = 21)]
        [Display(Name = "Executor Name")]
        [MaxLength(50)]
        public string ExecutorName { get; set; }

        [MaxLength(22)]
        [Column(Order = 10)]
        [Display(Name = "Executor ID")]
        public string ExecutorID { get; set; }

        [MaxLength(23)]
        [Column(Order = 11)]
        [Display(Name = "Executor Cell Number")]
        public string ExecutorCell { get; set; }

        [MaxLength(50)]
        [Column(Order = 24)]
        [Display(Name = "Executor Email")]
        public string ExecutorEmail { get; set; }

        //[StringLength(100)]
        //public string executorDeceased { get; set; }

        //[StringLength(20)]
        //public string executorDeceasedID { get; set; }

        //[StringLength(50)]
        //[Display(Name = "Follow on linked")]
        //public string followOnLinked { get; set; }

        [MaxLength(100)]
        [Column(Order = 25)]
        [Display(Name = "ERF Description")]
        public string ErfDescription { get; set; }

        [MaxLength(100)]
        [Column(Order = 26)]
        [Display(Name = "Extent")]
        public string Extent { get; set; }

        [Column(Order = 27)]
        [Display(Name = "Type Of Category")]
        public int? Category { get; set; }

        [MaxLength(200)]
        [Column(Order = 28)]
        [Display(Name = "Physical Address")]
        public string PhysicalAddress { get; set; }

        //[StringLength(20)]
        //[Display(Name = "Real Rights")]
        //public string Real_Rights { get; set; }

        //[StringLength(50)]
        //[Display(Name = "Exclusive User Area")]
        //public string Exclusive_UserArea { get; set; }

        //[StringLength(50)]
        //[Display(Name = "Real Right Confirmation")]
        //public string realRightConfirm { get; set; }

        //[StringLength(25)]
        //[Display(Name = "Scheme Name")]
        //public string schemeName { get; set; }

        //[StringLength(50)]
        //public string originalSsNo { get; set; }

        //[StringLength(50)]
        //public string newSsNo { get; set; }

        //[StringLength(2048)]
        //public string realRightDescription { get; set; }

        [MaxLength(50)]
        [Column(Order = 29)]
        [Display(Name = "Deeds Town")]
        public string DeedsTown { get; set; }

        [MaxLength(50)]
        [Column(Order = 30)]
        [Display(Name = "Property Owner Confirmation")]
        public string PropertyOwnerConfirm { get; set; }


        [Column(Order = 31)]
        [Display(Name = "Application Number")]
        public string AppNumber { get; set; }

        //public int? Property_info { get; set; }

        [MaxLength(10)]
        [Column(Order = 32)]
        [Display(Name = "Rate Number Entered")]
        public string RateNumEntered { get; set; }

        //public int? sub_AgentData { get; set; }

        //[StringLength(20)]
        //public string App_Status { get; set; }

        //public int? firm_owner { get; set; }

        //[StringLength(2048)]
        //public string con_Com { get; set; }


        [Column(Order = 33)]
        [Display(Name = "Date of Sale")]
        public DateTime? SaleDate { get; set; }


        [MaxLength(50)]
        [Column(Order = 34)]
        [Display(Name = "Property Description")]
        public string PropertyDescription { get; set; }

        [MaxLength(50)]
        [Column(Order = 35)]
        [Display(Name = "Suburb")]
        public string Suburb { get; set; }


        [MaxLength(50)]
        [Column(Order = 36)]
        [Display(Name = "Town")]
        public string Town { get; set; }


        [MaxLength(500)]
        [Column(Order = 37)]
        [Display(Name = "Extent")]
        public string ExtentSize { get; set; }

        [Column(Order = 38)]
        [Display(Name = "Confirm property and owner details are correct")]

        public bool ConfirmPropertydetails { get; set; }


        [MaxLength(100)]
        [Column(Order = 39)]
        [Display(Name = "ST Scheme Name")]
        public string STSchemeName { get; set; }

        [MaxLength(100)]
        [Column(Order = 40)]
        [Display(Name = "ST Complex Name")]
        public string STComplexName { get; set; }

        [MaxLength(100)]
        [Column(Order = 41)]
        [Display(Name = "ST Unit Number")]
        public string STUnitNumber { get; set; }

        [MaxLength(100)]
        [Column(Order = 42)]
        [Display(Name = "ST Door Number")]
        public string STDoorNumber { get; set; }

        [MaxLength(100)]
        [Column(Order = 43)]
        [Display(Name = "Managing Agent")]
        public string ManagingAgent { get; set; }

        [MaxLength(100)]
        [Column(Order = 44)]
        [Display(Name = "ST Scheme Number")]
        public string STSchemeNumber { get; set; }

        [MaxLength(100)]
        [Column(Order = 45)]
        [Display(Name = "Transfer Type Name")]
        public string TransferTypeName { get; set; }

        //[StringLength(50)]
        //public string VolumeNumber { get; set; }

        //[StringLength(10)]
        //public string useCode { get; set; }


        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<QueryApplicationTb> QueryApplicationTbs { get; set; }
    }
}