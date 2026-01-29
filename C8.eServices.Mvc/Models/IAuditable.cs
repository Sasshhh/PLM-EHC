using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models
{
    interface IAuditable
    {
        int Id { get; set; }

        int? CreatedBySystemUserId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd hh:mm:ss}")]
        DateTime? CreatedDateTime { get; set; }

        int? ModifiedBySystemUserId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd hh:mm:ss}")]
        DateTime? ModifiedDateTime { get; set; }
    }
}
