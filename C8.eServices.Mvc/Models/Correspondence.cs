using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using C8.eServices.Mvc.CorrespondenceSoapService;

namespace C8.eServices.Mvc.Models
{
    public class Correspondence : BaseModel
    {
        [Required]
        [Display(Name = "Reference Number")]
        public string ReferenceNumber { get; set; }

        [Required]
        [Display(Name = "Service Type")]
        public string ServiceType { get; set; }

        [Required]
        public string Category { get; set; }

        [Display(Name = "Sub Category")]
        public string SubCategory { get; set; }

        [Display(Name="Service Description")]
        [Required]
        public string ServiceDescription { get; set; }

        [Display(Name = "Service Unit Number")]
        public string ServiceUnitNumber { get; set; }

        [Display(Name = "Service Floor")]
        public string ServiceFloorNumber { get; set; }

        [Display(Name = "Service Section Number")]
        public string ServiceSectionNumber { get; set; }

        [Display(Name = "Service Building")]
        public string ServiceBuildingComplex { get; set; }

        [Required]
        [Display(Name = "Street Number")]
        public string ServiceAddress1 { get; set; }

        [Required]
        [Display(Name = "Street Name")]
        public string ServiceAddress2 { get; set; }

        [Required]
        [Display(Name = "Suburb")]
        public string ServiceAddress3 { get; set; }

        [Required]
        [Display(Name = "City")]
        public string ServiceAddress4 { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string ServiceAddress5 { get; set; }

        [Display(Name = "X Co-ordinate")]
        public string ServiceXCoordinate { get; set; }

        [Display(Name = "Y Co-ordinate")]
        public string ServiceYCoordinate { get; set; }

        [Required]
        public int CustomerTitleId { get; set; }
        [ForeignKey("CustomerTitleId")]
        public TitleType CustomerTitleType { get; set; }

        //WRITER DETAILS
        [Required]
        [Display(Name = "Title")]
        public int TitleTypeId { get; set; }
        [ForeignKey("TitleTypeId")]
        public TitleType TitleType { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Display(Name = "ID Number")]
        public string IdentificationNumber { get; set; }

        [Display(Name = "Work Telephone")]
        [DataType(DataType.PhoneNumber)]
        public string WorkTelephoneNumber { get; set; }

        [Display(Name = "Home Telephone")]
        [DataType(DataType.PhoneNumber)]
        public string HomeTelephoneNumber { get; set; }

        [Display(Name = "CellPhone Number")]
        [Required]
        public string CellPhoneNumber { get; set; }

        [Display(Name = "Fax Number")]
        public string FaxNumber { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Required]
        [Display(Name="Relationship to Customer")]
        public string Relationship { get; set; }

        [Required]
        [Display(Name = "Notification Status")]
        public string StatusNotification { get; set; }

        [Required]
        [Display(Name = "Correspondance Notification")]
        public string CorrespondanceNotification { get; set; }

        [Required]
        [Display(Name = "Writer Street Number")]
        public string WriterPhysicalAddress1 { get; set; }

        [Required]
        [Display(Name = "Writer Street Name")]
        public string WriterPhysicalAddress2 { get; set; }

        [Required]
        [Display(Name = "Writer Suburb")]
        public string WriterPhysicalAddress3 { get; set; }

        [Required]
        [Display(Name = "Writer City")]
        public string WriterPhysicalAddress4 { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string WriterPhysicalAddress5 { get; set; }

        [Display(Name = "Writer P.O Box")]
        public string WriterPostalAddress1 { get; set; }

        [Display(Name = "Writer Suburb")]
        public string WriterPostalAddress2 { get; set; }

        [Display(Name = "Writer City/Town")]
        public string WriterPostalAddress3 { get; set; }

        [Display(Name = "Writer Postal Code")]
        public string WriterPostalAddress4 { get; set; }

        [Required]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; }

        [Required]
        [Display(Name = "Customer FirstName")]
        public string CustomerFirstName { get; set; }

        [Required]
        [Display(Name = "Customer LastName")]
        public string CustomerLastName { get; set; }

        [Required]
        [Display(Name = "Customer ID Number")]
        public string CustomerIdentificationNumber { get; set; }

        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [Display(Name = "Rates Number")]
        public string RatesNumber { get; set; }

        [Display(Name = "Connection Number")]
        public string ConnectionNumber { get; set; }

        [Required]
        [Display(Name = "Work Number")]
        public string CustomerWorkTelephoneNumber { get; set; }

        [Display(Name = "Telephone Number")]
        public string CustomerHomeTelephoneNumber { get; set; }

        [Required]
        [Display(Name = "CellPhone Number")]
        public string CustomerCellPhoneNumber { get; set; }

        [Display(Name = "Fax Number")]
        public string CustomerFaxNumber { get; set; }

        [Required]
        [Display(Name = "EmailAddress")]
        public string CustomerEmailAddress { get; set; }

        [Display(Name = "Business Name")]
        public string BusinessName { get; set; }

        [Display(Name = "Business Registration Number")]
        public string BusinessRegistrationNumber { get; set; }

        [Display(Name = "Street Number")]
        public string CustomerPhysicalAddress1 { get; set; }

        [Display(Name = "Street Name")]
        public string CustomerPhysicalAddress2 { get; set; }

        [Display(Name = "Town/Suburb")]
        public string CustomerPhysicalAddress3 { get; set; }

        [Display(Name = "City")]
        public string CustomerPhysicalAddress4 { get; set; }

        [Display(Name = "Code")]
        public string CustomerPhysicalAddress5 { get; set; }

        [Display(Name = "Postal Box")]
        public string CustomerPostalAddress1 { get; set; }

        [Display(Name = "Postal Suburb")]
        public string CustomerPostalAddress2 { get; set; }

        [Display(Name = "City")]
        public string CustomerPostalAddress3 { get; set; }

        [Display(Name = "Code")]
        public string CustomerPostalAddress4 { get; set; }

        [Display(Name = "Service Address Type")]
        public string ServiceAddressType { get; set; }


        public virtual List<string> ServiceTypes { get; set; }
        public virtual List<string> Categories { get; set; }
        public virtual List<string> SubCategories { get; set; }
        public virtual List<string> ServiceAddressTypes { get; set; }

        public Correspondence()
        {
            ReferenceNumber = "";
            //Service Details
            ServiceType = "";
            Category = "";
            SubCategory = "";
            ServiceDescription = "";
            //Service Details Addres
            ServiceUnitNumber = "";
            ServiceFloorNumber = "";
            ServiceSectionNumber = "";
            ServiceBuildingComplex = "";
            ServiceAddress1 = "";
            ServiceAddress2 = "";
            ServiceAddress3 = "";
            ServiceAddress4 = "";
            ServiceAddress5 = "";
            ServiceXCoordinate = "";
            ServiceYCoordinate = "";
            //Writer Details
            FirstName = "";
            LastName = "";
            IdentificationNumber = "";
            WorkTelephoneNumber = "";
            HomeTelephoneNumber = "";
            CellPhoneNumber = "";
            FaxNumber = "";
            TitleTypeId = 1;
            EmailAddress = "";
            Relationship = "";
            StatusNotification = "";
            CorrespondanceNotification = "";
            //Writer Address Details
            WriterPhysicalAddress1 = "";
            WriterPhysicalAddress2 = "";
            WriterPhysicalAddress3 = "";
            WriterPhysicalAddress4 = "";
            WriterPhysicalAddress5 = "";
            //Customer Details
            CustomerTitleId = 1;
            CustomerFirstName = "";
            CustomerLastName = "";
            IdentificationNumber = "";
            WorkTelephoneNumber = "";
            CellPhoneNumber = "";
            EmailAddress = "";
            BusinessName = "";
            BusinessRegistrationNumber = "";
            AccountNumber = "";
            RatesNumber = "";

            CustomerPhysicalAddress1 = "";
            CustomerPhysicalAddress2 = "";
            CustomerPhysicalAddress3 = "";
            CustomerPhysicalAddress4 = "";
            CustomerPhysicalAddress5 = "";
            
        }
    }
}