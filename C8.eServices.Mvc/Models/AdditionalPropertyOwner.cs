using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace C8.eServices.Mvc.Models
{
    public class AdditionalPropertyOwner : BaseModel
    {
        [Column( Order = 10 )]
        [MaxLength(100)]
        [Display( Name = "Rate No" )]
        public string RatesNumber { get; set; }

        [Column( Order = 11 )]
        [Display( Name = "Executor No" )]
        [MaxLength(100)]
        public string ExecutorContactNumber { get; set; }

        [Column( Order = 12 )]
        [Display( Name = "Executor Name" )]
        [MaxLength(100)]
        public string ExecutorName { get; set; }

        [Column( Order = 13 )]
        [Display( Name = "Deceased" )]
        public bool IsDeceased { get; set; }

        [Column( Order = 14 )]
        [Display( Name = "Registered Owner" )]
        public bool IsRegisteredOwner { get; set; }

        [Column( Order = 15 )]
        [Display( Name = "Owner ID No" )]
        [MaxLength(100)]
        public string OwnerIdentificationNumber { get; set; }

        [Column( Order = 16 )]
        [Display( Name = "Owner" )]
        [MaxLength(100)]
        public string OwnerName { get; set; }

        [Column( Order = 17 )]
        [Display( Name = "Reason the person is no longer a registered owner" )]
        [MaxLength(250)]
        public string NoLongerRegisteredOwnerReason { get; set; }

        [Column( Order = 18 )]
        [Display( Name = "Property" )]
        public int PropertyId { get; set; }
        [ForeignKey( "PropertyId" )]
        public Property Property { get; set; }

        [Column( Order = 19 )]
        [Display( Name = "Status" )]
        public int StatusId { get; set; }
        [ForeignKey( "StatusId" )]
        public Status Status { get; set; }

        [Column( Order = 20 )]
        [Display( Name = "Country Of Issue" )]
        public int? CountryOfIssueTypeId { get; set; }
        [ForeignKey( "CountryOfIssueTypeId" )]
        public Country Country { get; set; }

        [Column( Order = 21 )]
        [Display( Name = "Identification Type" )]
        public int IdentificationTypeId { get; set; }
        [ForeignKey( "IdentificationTypeId" )]
        public IdentificationType IdentificationType { get; set; }

        // JK.20160209a - Used to compare objects based on Owner Identification Number.
        #region Comparison

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals( object obj )
        {
            // Try to cast the object to compare to to be a Person
            var person = obj as AdditionalPropertyOwner;

            return Equals( person );
        }

        /// <summary>
        /// Returns an identifier for this instance
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return OwnerIdentificationNumber.GetHashCode();
        }

        /// <summary>
        /// Checks if the provided Person is equal to the current Person
        /// </summary>
        /// <param name="ownerToCompareTo">The owner to compare to.</param>
        /// <returns>
        /// True if equal, false if not
        /// </returns>
        public bool Equals( AdditionalPropertyOwner ownerToCompareTo )
        {
            // Check if person is being compared to a non person. In that case always return false.
            if ( ownerToCompareTo == null ) return false;

            // If the person to compare to does not have a Name assigned yet, we can't define if it's the same. Return false.
            if ( string.IsNullOrEmpty( ownerToCompareTo.OwnerIdentificationNumber ) ) return false;

            // Check if both person objects contain the same Name. In that case they're assumed equal.
            return OwnerIdentificationNumber.Equals( ownerToCompareTo.OwnerIdentificationNumber );
        }

        /// <summary>
        /// Compares the specified compare owner, looking at all fields.
        /// </summary>
        /// <param name="compareOwner">The compare owner.</param>
        /// <returns></returns>
        public bool Compare( AdditionalPropertyOwner compareOwner )
        {
            var isSame = true;

            if ( !OwnerIdentificationNumber.Equals( compareOwner.OwnerIdentificationNumber ) ) isSame = false;
            if ( !OwnerName.Equals( compareOwner.OwnerName ) ) isSame = false;
            if ( !IsDeceased.Equals( compareOwner.IsDeceased ) ) isSame = false;
            if ( !IsRegisteredOwner.Equals( compareOwner.IsRegisteredOwner ) ) isSame = false;
            if ( !RatesNumber.Equals( compareOwner.RatesNumber ) ) isSame = false;
            if ( !ExecutorName.Equals( compareOwner.ExecutorName ) ) isSame = false;
            if ( !ExecutorContactNumber.Equals( compareOwner.ExecutorContactNumber ) ) isSame = false;

            return isSame;
        }

        #endregion Comparison
    }
}