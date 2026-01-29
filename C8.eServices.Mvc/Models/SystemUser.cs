using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class SystemUser : BaseModel
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
        public string CompanyName { get; set; }

        [Column(Order = 14)]
        public string Designation { get; set; }

        [Column(Order = 15)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Column(Order = 16)]
        public string SystemUserTypeId { get; set; }

        [Column(Order = 17)]
        public string StatusId { get; set; }

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
        [Display(Name = "Service Number")]
        public string ServiceNo { get; set; }
        [Column(Order = 25)]
        [Display(Name = "CCC")]
        public int? CCCId { get; set; }
        [ForeignKey("CCCId")]
        public CCC CCC { get; set; }

        public bool isInternalUser { get; set; }
        public bool isActiveDirectoryUser { get; set; }

        [Display(Name = "User")]
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }
        [Column(Order = 26)]
        public bool IsIAMRegistered { get; set; }
        public string UserFullName
        {
            get
            {
                string name = string.Empty;


                name = string.Format("{0} {1}", FirstName, LastName);

                //if ( CustomerType != null )
                //    name += string.Format( " ({0}{1})", CustomerType.Name,
                //        string.IsNullOrEmpty( IdentificationNumber )
                //            ? ""
                //            : string.Format( " - {0}", IdentificationNumber ) );

                return name;
            }
        }
    }
}