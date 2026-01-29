using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class WaitingQueueChangeRequest : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Human Settlement Application")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

        [Display(Name = "Unit Category")]
        [Column(Order = 11)]
        public int? UnitCategoryId { get; set; }
        [ForeignKey("UnitCategoryId")]
        public HSUnitCategory UnitCategory { get; set; }

        [Display(Name = "Unit Typology")]
        [Column(Order = 12)]
        public int? UnitTypologyId { get; set; }
        [ForeignKey("UnitTypologyId")]
        public HSUnitTypology UnitTypology { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Requested By User")]
        public int? RequestByUserId { get; set; }
        [ForeignKey("RequestByUserId")]
        public SystemUser RequestByUser { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Allocated By User")]
        public int? AllocatedByUserId { get; set; }
        [ForeignKey("AllocatedByUserId")]
        public SystemUser AllocatedByUser { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Preferred Complex Area")]
        public int? PreferredComplexAreaOneId { get; set; }
        [ForeignKey("PreferredComplexAreaOneId")]
        public PreferredComplexArea PreferredComplexAreaOne { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Preferred Complex Area")]
        public int? PreferredComplexAreaTwoId { get; set; }
        [ForeignKey("PreferredComplexAreaTwoId")]
        public PreferredComplexArea PreferredComplexAreaTwo { get; set; }
    }
}