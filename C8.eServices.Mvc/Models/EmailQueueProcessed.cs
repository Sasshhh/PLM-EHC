using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class EmailQueueProcessed
    {
        public int EmailQueueId { get; set; }

        public int ApplicationId { get; set; }

        public int EmailAccountId { get; set; }

        public DateTime QueueDateTime { get; set; }

        public string ToList { get; set; }

        public string CcList { get; set; }

        public string BccList { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public bool IsHtml { get; set; }

        public int FailureCount { get; set; }

        public string ReferenceId { get; set; }

        public int ReferenceTypeId { get; set; }

        public bool HasAttachments { get; set; }

        public DateTime ProcessedDateTime { get; set; }
    }
}