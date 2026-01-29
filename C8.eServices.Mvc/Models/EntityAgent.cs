using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class EntityAgent: BaseModel
    {
        [Column(Order = 10)]
        public int AgentId { get; set; }
        [ForeignKey("AgentId")]
        public Agent Agent { get; set; }

        [Column(Order = 11)]
        public int EntityId { get; set; }
        [ForeignKey("EntityId")]
        public Entity Entity { get; set; }
    }
}