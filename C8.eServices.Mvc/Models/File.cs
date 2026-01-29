using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class File : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "File Name")]
        [StringLength(1024)]
        public string FileName { get; set; }

        [Column( Order = 11 )]
        [Display( Name = "Content Type" )]
        [StringLength( 256 )]
        public string ContentType { get; set; }

        [Column( Order = 12 )]
        [Display( Name = "Content" )]
        public byte[] Content { get; set; }

        [Column( Order = 13 )]
        [Display( Name = "File Size (Bytes)" )]
        public Int64 FileSize { get; set; }
    }
}