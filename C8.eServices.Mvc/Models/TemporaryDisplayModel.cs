using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class TemporaryDisplayModel
    {
        public HumanSettlementApplication HumanSettlementApplication { get; set; }
        public WaitingListQueueHuman WaitingHumanQueue { get; set; }
        public int WaitingListQueueId { get; set; }
        public int PropertyLeaseApplicationId { get; set; }
        public int HumanSettlementApplicationId { get; set; }
        public int Id { get; set; }
        public int Order { get; set; }
        public int Queue { get; set; }
        public string WaitingListQueueDate { get; set; }
        public string ApplicantFullName { get; set; }
        public string Position { get; set; }
        public string ComplexAreas { get; set; }
        public string BedRooms { get; set; }
        public string ApplicationReferenceNo { get; set; }
        public bool IsRelisted { get; set; }
        public int MoveToPosition { get; set; }
        public int ItemPosition { get; set; }
        public string field9 { get; set; }
        public string Data { get; set; }
        public string Data2 { get; set; }
        public string ColorCode { get; set; }
        public bool WaitingQueueChangedApproved { get; set; }
        public bool WaitingQueueChangedDisapproved { get; set; }
        public bool PendingAllocationTransfer { get; set; }
        public string AvailabilityKey { get; set; }

    }
}