using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class EntityAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Type Of Entity")]
        public int EntityTypeId { get; set; }
        [ForeignKey("EntityTypeId")]
        public EntityType EntityType { get; set; }

        [Column(Order = 12)]
        [MaxLength(100)]
        [Display(Name = "Registration No")]
        public string EntityRegistrationNumber { get; set; }

        [Column(Order = 13)]
        [MaxLength(500)]
        [Display(Name = "Registration Name")]
        public string EntityName { get; set; }

        [Column(Order = 14)]
        [MaxLength(100)]
        [Display(Name = "Branch/ Section")]
        public string BranchOrSectionName { get; set; }

        [Column(Order = 15)]
        [MaxLength(500)]
        [Display(Name = "Trading Name")]
        public string TradingName { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Display( Name = "Entity" )]
        public string EntityFullName
        {
            get
            {
                string name = string.Empty;

                name = string.Format( "{0}", EntityName );

                if ( string.IsNullOrEmpty( BranchOrSectionName ) )
                    name += string.Format( " ({0})", BranchOrSectionName );

                if ( string.IsNullOrEmpty( TradingName ) )
                    name += string.Format( " trading as {0}", TradingName );

                if ( EntityType != null )
                    name += string.Format( " ({0} - {1})", EntityType.Name, string.IsNullOrEmpty( EntityRegistrationNumber ) ? "Unknown" : EntityRegistrationNumber );

                return name;
    }
        }
    }
}