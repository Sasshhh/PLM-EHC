using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class SmsQueueItemBulk
    {

            public int SmsQueueId { get; set; }

            public DateTime QueueDateTime { get; set; }

            public int ApplicationId { get; set; }

            public int SmsAccountId { get; set; }

           
            public string MobileNumber { get; set; }

            public string TextMessage { get; set; }

            public int FailureCount { get; set; }

            public string ReferenceId { get; set; }

            public int ReferenceTypeId { get; set; }

            public int StatusId { get; set; }
    }
    }