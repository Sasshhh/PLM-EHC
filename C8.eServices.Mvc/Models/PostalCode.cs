using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    public class PostalCode : BaseModel
    {
        [Column( Order = 10 )]
        [MaxLength(100)]
        [Display(Name = "Area")]
        public string Area { get; set; }

        [Column( Order = 11 )]
        [MaxLength(30)]
        [Display(Name = "Postal Code")]
        public string PCode { get; set; }

        [Column( Order = 12 )]
        [MaxLength(100)]
        [Display(Name = "Street Box")]
        public string StreetBox { get; set; }

        [Column(Order = 13)]
        [MaxLength(100)]
        [Display(Name = "Town")]
        public string Town { get; set; }


    }
}