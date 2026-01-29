using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class NotificationSubscription: BaseModel
    {

        [Column(Order = 10)]
        [Display(Name = "System User")]
        public int? SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public SystemUser SystemUser { get; set; }
        [Column(Order = 11)]
        [Display(Name = "Notification")]
        public int? NotificationId { get; set; }
        [ForeignKey("NotificationId")]
        public Notification Notification { get; set; }


        [Column(Order = 12)]
        [Display(Name = "Type of Notification")]
        public int? NotificationTypeId { get; set; }
        [ForeignKey("NotificationTypeId")]
        public NotificationType NotificationType { get; set; }



    }
}