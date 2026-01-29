namespace C8.eServices.Mvc.Models
{
    public class BulkUploadLog
    {
        public string CellId { get; set; }
        public bool IsSuccess { get; set; }
        public string AccountNumber { get; set; }
        public int LinkedAccountId { get; set; }
        public string Message { get; set; }
        
        public string Result 
        {
            get { return (IsSuccess) ? "Success" : "Error"; }
        }
    }
}