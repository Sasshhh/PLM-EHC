using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class Customer : BaseModel
    {
        [Column( Order = 10 )]
        [Display( Name = "Type Of Customer" )]
        public int CustomerTypeId { get; set; }
        [ForeignKey( "CustomerTypeId" )]
        public CustomerType CustomerType { get; set; }

        [Column( Order = 11 )]
        [Display( Name = "Type Of ID" )]
        public int ? IdentificationTypeId { get; set; }
        [ForeignKey( "IdentificationTypeId" )]
        public IdentificationType IdentificationType { get; set; }

        [Column( Order = 12 )]
        [Display( Name = "Country Of Issue" )]
        public int ? CountryOfIssueTypeId { get; set; }
        [ForeignKey( "CountryOfIssueTypeId" )]
        public Country Country { get; set; }

        [Column( Order = 13 )]
        [Display( Name = "ID/ Passport No" )]
        [MaxLength(100)]
        public string IdentificationNumber { get; set; }

        [Column( Order = 14 )]
        [Display( Name = "Title" )]
        public int ? TitleTypeId { get; set; }
        [ForeignKey( "TitleTypeId" )]
        public TitleType TitleType { get; set; }

        [Column( Order = 15 )]
        [Display( Name = "First Name" )]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Column( Order = 16 )]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Column( Order = 17 )]
        [MaxLength(25)]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Column( Order = 18 )]
        [Display( Name = "Deceased" )]
        public bool ? IsDeceased { get; set; }

        [Column( Order = 19 )]
        [MaxLength(100)]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }

        [Column( Order = 20 )]
        [MaxLength(100)]
        [Display(Name = "Work Phone")]
        public string WorkPhoneNumber { get; set; }

        [Column( Order = 21 )]
        [MaxLength(100)]
        [Display(Name = "Home Phone")]
        public string HomePhoneNumber { get; set; }

        [Column( Order = 22 )]
        [MaxLength(100)]
        [Display(Name = "Cell Phone")]
        public string CellPhoneNumber { get; set; }

        [Column( Order = 23 )]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Unit number  must be numeric")]
        [MaxLength(25)]
        [Display(Name = "Unit No")]
        public string PhysicalAddress1 { get; set; }

        [Column( Order = 24 )]
        [MaxLength(100)]
        [Display(Name = "Block/ Complex Name")]
        public string PhysicalAddress2 { get; set; }

        [Column( Order = 25 )]
        [MaxLength(100)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Street number must be numeric")]
        [Display(Name = "Street No")]
        public string PhysicalAddress3 { get; set; }

        [Column( Order = 26 )]
        [MaxLength(100)]
        [Display(Name = "Street Name/ Postal Box")]
        public string PhysicalAddress4 { get; set; }

        [Column( Order = 27 )]
        [MaxLength(100)]
        [Display(Name = "Suburb/ Postal Area")]
        public string PhysicalAddress5 { get; set; }

        [Column( Order = 28 )]
        [Display(Name = "Postal Code")]
        public int PhysicalAddressCode { get; set; }

        [Column( Order = 29 )]
        [MaxLength(25)]
        [Display(Name = "Unit No")]
        public string PostalAddress1 { get; set; }

        [Column( Order = 30 )]
        [MaxLength(100)]
        [Display(Name = "Block/ Complex Name")]
        public string PostalAddress2 { get; set; }

        [Column( Order = 31 )]
        [MaxLength(100)]
        [Display(Name = "Street No")]
        public string PostalAddress3 { get; set; }

        [Column( Order = 32 )]
        [MaxLength(100)]
        [Display(Name = "Street Name/ Postal Box")]
        public string PostalAddress4 { get; set; }

        [Column( Order = 33 )]
        [MaxLength(100)]
        [Display(Name = "Suburb/ Postal Area")]
        public string PostalAddress5 { get; set; }

        [Column( Order = 34 )]
        [Display(Name = "Postal Code")]
        public int PostalAddressCode { get; set; }

        [Column( Order = 35 )]
        [Display( Name = "System User" )]
        public int ? SystemUserId { get; set; }
        [ForeignKey( "SystemUserId" )]
        public SystemUser SystemUser { get; set; }

        [Column( Order = 36 )]
        [Display( Name = "Status" )]
        public int StatusId { get; set; }
        [ForeignKey( "StatusId" )]
        public Status Status { get; set; }

        [Column(Order = 37)]
        [MaxLength(100)]
        [Display(Name = "Attorney Code")]
        public string AttorneyCode { get; set; }

        [Display( Name = "Accounts" )]
        public IEnumerable<LinkedAccount> LinkedAccounts { get; set; }

        [Display( Name = "Name" )]
        public string FullName
        {
            get
            {
                string name = string.Empty;

                if ( TitleType != null )
                    name = string.Format( "{0} {1} {2}", TitleType.Name, FirstName, LastName );
                else
                    name = string.Format( "{0} {1}", FirstName, LastName );

                //if ( CustomerType != null )
                //    name += string.Format( " ({0}{1})", CustomerType.Name,
                //        string.IsNullOrEmpty( IdentificationNumber )
                //            ? ""
                //            : string.Format( " - {0}", IdentificationNumber ) );

                return name;
            }
        }

        [Display(Name = "Name")]
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

        [Display( Name = "Identification" )]
        public string Identification
        {
            get
            {
                if ( IdentificationType != null )
                    return string.Format( "{0} {1} {2}", IdentificationType.Name, IdentificationNumber, Country != null ? string.Format( "({0})", Country.Name ) : "");
                else
                    return string.Format( "{0} (ZA)", IdentificationNumber );
            }
        }

        [Display( Name = "Contacts" )]
        public string ContactNumbers
        {
            get
            {
                string contact = string.Empty;

                if ( !string.IsNullOrEmpty( CellPhoneNumber ) ) contact = string.Format( "(C) {0} | ", CellPhoneNumber );
                if ( !string.IsNullOrEmpty( HomePhoneNumber ) ) contact += string.Format( "(H) {0} | ", HomePhoneNumber );
                if ( !string.IsNullOrEmpty( WorkPhoneNumber ) ) contact += string.Format( "(W) {0} | ", WorkPhoneNumber );

                return contact;
            }
        }

        public string InternationalCellPhoneNumber
        {
            get
            {
                string inter = string.Empty;

                if (CellPhoneNumber[0] == '0') return "27" + CellPhoneNumber.Substring(1).Replace(" ", "");
                else return CellPhoneNumber.Replace(" ", "");
            }
        }

        [Display( Name = "Physical Address" )]
        public string PhysicalAddress
        {
            get
            {
                return string.Format( "{0} {1}  {2} {3} {4}  {5}",
                    PhysicalAddress1,
                    PhysicalAddress2,
                    PhysicalAddress3,
                    PhysicalAddress4,
                    PhysicalAddress5,
                    (PhysicalAddressCode == 0) ? string.Empty : PhysicalAddressCode.ToString() );
            }
        }

        [Display( Name = "Postal Address" )]
        public string PostalAddress
        {
            get
            {
                return string.Format( "{0} {1}  {2} {3} {4}  {5}",
                    PostalAddress1,
                    PostalAddress2,
                    PostalAddress3,
                    PostalAddress4,
                    PostalAddress5,
                    (PostalAddressCode == 0) ? string.Empty : PostalAddressCode.ToString() );
            }
        }       
    }
}