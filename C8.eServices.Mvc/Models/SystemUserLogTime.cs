using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class SystemUserLogTime
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "System User")]
        public int SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public SystemUser SystemUser { get; set; }
        public string SessionId { get; set; }
        public string IPAddress { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime ? LogoutTime { get; set; }
    }
}