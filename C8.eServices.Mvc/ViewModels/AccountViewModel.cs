using C8.eServices.Mvc.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace C8.eServices.Mvc.ViewModels
{
    using Compare = System.ComponentModel.DataAnnotations.CompareAttribute;
    public class ManageUserViewModel
    {
        [Required]
        [DataType( DataType.Password )]
        [Display( Name = "Current password" )]
        public string OldPassword { get; set; }

        [Required]
        [StringLength( 100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6 )]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,15}$", ErrorMessage = "The password must contain at least one uppercase character, one lowercase character, one numeric and one special character")]
        [DataType( DataType.Password )]
        [Display( Name = "New password" )]
        public string NewPassword { get; set; }

        [DataType( DataType.Password )]
        [Display( Name = "Confirm new password" )]
        [Compare( "NewPassword", ErrorMessage = "The new password and confirmation password do not match." )]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Mobile Number")]
        [DataType(DataType.PhoneNumber)]
        public string MobileNumber { get; set; }
        [Display(Name = "Preferred Method Of Communication")]
 
        public int PrefMethodOfCommunication { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display( Name = "Username" )]
        public string UserName { get; set; }

        [Required]
        [DataType( DataType.Password )]
        [Display( Name = "Password" )]
        public string Password { get; set; }

        [Display( Name = "Remember me?" )]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display( Name = "First Name" )]
        [RegularExpression( @"^[a-zA-Z ]+$", ErrorMessage = "The FirstName field should consist of characters only" )]
        public string FirstName { get; set; }

        [Required]
        [Display( Name = "Surname" )]
        [RegularExpression(@"^[a-zA-Z0-9_ ]*$", ErrorMessage = "The LastName field should consist of characters only")]
        public string LastName { get; set; }

        [Display( Name = "Email Address" )]
        [Required]
        [EmailAddress( ErrorMessage = "Invalid Email Address" )]
        [DataType( DataType.EmailAddress )]
        public string EmailAddress { get; set; }

        [Display(Name = "Confirm Email Address")]
        [DataType(DataType.EmailAddress)]
        [Required]
        [Compare("EmailAddress", ErrorMessage = "The email and confirmation email do not match.")]
        public string ConfirmEmailAddress { get; set; }

        [Phone(ErrorMessage = "Invalid Mobile Number")]
        [Display(Name = "Cellphone Number")]
        [Required]
        [MaxLength(10)]
        [MinLength(10,ErrorMessage = "{0} must have minimum of {1} numeric character")]
        [DataType(DataType.PhoneNumber)]
        public string MobileNumber { get; set; }

        [Display(Name = "Confirm Cellphone Number")]
        [DataType(DataType.PhoneNumber)]
        [Required]
        [MaxLength(10)]
        [MinLength(10,ErrorMessage = "{0} must have minimum of {1} numeric character")]
        [Compare("MobileNumber", ErrorMessage = "The mobile number and confirmation mobile number do not match.")]
        public string ConfirmMobileNumber { get; set; }

        [Display( Name = "Designation" )]
        public string Designation { get; set; }

        [Display( Name = "Company Name" )]
        public string CompanyName { get; set; }
        [Required]
        [Display(Name = "RSA ID No")]
        [MaxLength(13)]
        [MinLength(13)]
        public string IdentificationNumber { get; set; }

        [Required]
        [Display( Name = "Username" )]
        //[RegularExpression(@"^[a-zA-Z0-9!@#$&()\\-`.+,/\"]*$", ErrorMessage = "The Username field must be 6 to 20 characters long, without any special characters eg. !,@,#,$,%,&,*.")]
        [MaxLength(20)]
        [MinLength(6)]
        public string UserName { get; set; }

        [Required]
        [StringLength( 100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6 )]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,15}$", ErrorMessage = "The password must contain at least one uppercase character, one lowercase character, one numeric and one special character")]
        [DataType( DataType.Password )]
        [Display( Name = "Password" )]
        public string Password { get; set; }

        [DataType( DataType.Password )]
        [Display( Name = "Confirm password" )]
        [Compare( "Password", ErrorMessage = "The password and confirmation password do not match." )]
        public string ConfirmPassword { get; set; }
        
        [Display( Name = "System User Type" )]
        public string SystemUserTypeId { get; set; }
        
        [Display( Name = "Status" )]
        public string StatusId { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required]
        [Display(Name = "Preferred Method of Communication")]
        public int NotificationTypeId { get; set; }
    }


    public class InternalRegisterViewModel
    {
        [Required]
        [Display(Name = "CCC")]
        public int CCC { get; set; }
        public int ClerkRegID { get; set; }

        public bool AdUser { get; set; }

        public List<ApplicationUserRole>  UserRole { get; set; }
   
        public IEnumerable<string> SelectedFeatures { get; set; }

        public IEnumerable<SelectListItem> CCCTypes { get; set; }
        public IEnumerable<SelectListItem> Features { get; set; }
        [Required]
        [Display(Name = "First Name")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "The FirstName field should consist of characters only")]
        public string FirstName { get; set; }
        [Display(Name = "Area Manager")]

        public string AreaManager { get; set; }

        [Required]
        [Display(Name = "Employee Number")]
        public string EmployeeNumber { get; set; }

        [Display(Name = "Area Manager")]
        public string AM { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [RegularExpression(@"^[a-zA-Z0-9_ ]*$", ErrorMessage = "The LastName field should consist of characters only")]
        public string LastName { get; set; }

        [Display(Name = "Email Address")]
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Display(Name = "Confirm Email Address")]
        [DataType(DataType.EmailAddress)]
        [Required]
        [Compare("EmailAddress", ErrorMessage = "The email and confirmation email do not match.")]
        public string ConfirmEmailAddress { get; set; }

        [Phone(ErrorMessage = "Invalid Mobile Number")]
        [Display(Name = "Mobile Number")]
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string MobileNumber { get; set; }

        [Display(Name = "Confirm Mobile Number")]
        [DataType(DataType.PhoneNumber)]
        [Required]
        [Compare("MobileNumber", ErrorMessage = "The mobile number and confirmation mobile number do not match.")]
        public string ConfirmMobileNumber { get; set; }

        [Display(Name = "Designation")]
        public string Designation { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        //[Required]
        [Display(Name = "RSA ID No")]
        [MaxLength(13)]
        [MinLength(13)]
        public string IdentificationNumber { get; set; }

        [Required]
        [Display(Name = "Username")]
        [MaxLength(20)]
        [MinLength(5)]
        public string UserName { get; set; }

        [Display(Name = "Department Name")]
        public int? DepartmentId { get; set; }

        [Display(Name = "System User Type")]
        public string SystemUserTypeId { get; set; }

        [Display(Name = "Status")]
        public string StatusId { get; set; }
    }

    public class RegisterViewModelApi
    {
        [Required]
        [Display(Name = "First Name")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "The FirstName field should consist of characters only")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [RegularExpression(@"^[a-zA-Z0-9_ ]*$", ErrorMessage = "The LastName field should consist of characters only")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Display(Name = "Designation")]
        public string Designation { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "Username")]
        [MaxLength(20)]
        [MinLength(6)] 
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,15}$", ErrorMessage = "The password must contain at least one uppercase character, one lowercase character, one numeric and one special character")]
        //[DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "System User Type")]
        public string SystemUserTypeId { get; set; }

        [Display(Name = "Status")]
        public string StatusId { get; set; }

        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [Display(Name = "ID/ Passport No")]
        [MaxLength(100)]
        public string IdentificationNumber { get; set; }

        public string CellPhoneNumber { get; set; }
        public string WorkPhoneNumber { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        
        [EmailAddress]
        [Display( Name = "Email" )]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }
    }
}