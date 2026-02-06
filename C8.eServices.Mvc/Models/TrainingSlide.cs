using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    /// <summary>
    /// Stores the 24 training slides content
    /// </summary>
    public class TrainingSlide : BaseModel
    {
        [Column(Order = 10)]
        [Required]
        [Display(Name = "Slide Number")]
        public int SlideNumber { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Title")]
        [MaxLength(500)]
        public string Title { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Image Path")]
        [MaxLength(500)]
        public string ImagePath { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }
    }
}
