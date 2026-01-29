using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class ClerkRegistration : BaseModel
    {
        [Column(Order = 10)]
        //[RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "The FirstName field should consist of characters only")]
        public string FirstName { get; set; }

        [Column(Order = 11)]
        //[RegularExpression(@"^[a-zA-Z]+$", ErrorMessage =  "The LastName field should consist of characters only")]
        public string LastName { get; set; }

        [Column(Order = 12)]
        public string UserName { get; set; }

        [Column(Order = 13)]
        public string EmployeeNumber { get; set; }

        [Column(Order = 14)]
        public string RoleArray { get; set; }

        [Column(Order = 15)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Column(Order = 16)]
        public string SystemUserTypeId { get; set; }


        [Column(Order = 17)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 18)]
        public bool IsPasswordReset { get; set; }

        [Column(Order = 19)]
        public bool IsTemporaryPassword { get; set; }
        [Column(Order = 22)]
        [Display(Name = "ID/ Passport No")]
        [MaxLength(100)]
        public string IdentificationNumber { get; set; }
        [Column(Order = 20)]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }


        [Column(Order = 21)]
        public string Code { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Type of Notification")]
        public int? NotificationTypeId { get; set; }
        [ForeignKey("NotificationTypeId")]
        public NotificationType NotificationType { get; set; }
        [Column(Order = 24)]
        [Display(Name = "CCC")]
        public int CCCTypeId { get; set; }
        [ForeignKey("CCCTypeId")]
        public CCCType CCCType { get; set; }
        [Column(Order = 25)]
        [Display(Name = "CCC Name")]
        public int? CCCId { get; set; }
        [ForeignKey("CCCId")]
        public CCC CCC { get; set; }
        [Column(Order = 26)]
        [Display(Name = "Active Directory")]
        public bool IsActiveDirectoryUser { get; set; }

        [Display(Name = "User")]
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }
    }
}