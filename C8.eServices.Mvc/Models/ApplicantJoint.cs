using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace C8.eServices.Mvc.Models
{
    public class ApplicantJoint: BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Gender")]
        [StringLength(10)]
        public string Gender { get; set; }

        [Display(Name = "Title")]
        [Column(Order = 11)]
        public int? TitleTypeId { get; set; }
        public TitleType TitleType { get; set; }

        [Display(Name = "Marital Status")]
        [Column(Order = 12)]
        [StringLength(20)]
        public string MaritalStatus { get; set; }

        [Display(Name = "Initials")]
        [Column(Order = 13)]
        [StringLength(10)]
        public string JointInitial { get; set; }

        [Display(Name = "First Name")]
        [Column(Order = 14)]
        [StringLength(50)]
        public string JointFirstName { get; set; }

        [Display(Name = "Last Name")]
        [Column(Order = 15)]
        [StringLength(50)]
        public string JointLastName { get; set; }

        [Display(Name = "ID Number")]
        [Column(Order = 16)]
        [StringLength(25)]
        public string IDNo { get; set; }

        [Display(Name = "Date of Birth")]
        [Column(TypeName = "date", Order = 17)]
        public DateTime? DOB { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Mobile number is not valid")]
        [Display(Name = "Mobile Number")]
        [Column(Order = 18)]
        [StringLength(15)]
        public string JointCellNo { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 19)]
        [StringLength(50)]
        public string JointEmail { get; set; }

        [Display(Name = "PropertyLeaseApplication")]
        [Column(Order = 20)]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }
    }
}