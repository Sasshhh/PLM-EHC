using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    public class UserSNotificationReport
    {

        [Column(Order = 10)]
        public int ID { get; set; }
        //[ForeignKey("CustomerId")]
        //public Customer Customer { get; set; }
        [Column(Order = 11)]
        public string SMS_Description { get; set; }
        //[ForeignKey("AccountId")]
        //public Account Account { get; set; }

        [Column(Order = 12)]
        //[Display(Name = "Linked  Account ID")]
        public int SMS_Total { get; set; }
        //[ForeignKey("LinkedAccountId")]
        //public LinkedAccount LinkedAccount { get; set; }

        [Column(Order = 13)]
        //[Display(Name = "Service Acc No")]
        public int SMS_Year { get; set; }

        [Column(Order = 14)]
        //[Display(Name = "Amount")]
        //[DataType(DataType.Currency)]
        public int SMS_Month { get; set; }
    }
}