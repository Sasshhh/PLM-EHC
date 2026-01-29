using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class BillInfo
    {
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string DueDate { get; set; }
        public string RunDate { get; set; }
        public string Total { get; set; }
        public Decimal CycleNo { get; set; }
        public string InvDate { get; set; }

        public DateTime DueDateConvert
        {
            get
            {
                DateTime retval = DateTime.Parse(DueDate);
                return retval;
            }
        }

        public DateTime RunDateConvert
        {
            get
            {
                DateTime retval = DateTime.Parse(RunDate);
                return retval;
            }
        }

        public DateTime InvDateConvert
        {
            get
            {
                DateTime retval = DateTime.Parse(InvDate);
                return retval;
            }
        }
        public Decimal TotalConvert
        {
            get
            {
                decimal retval = decimal.Parse(Total.Replace('.',','));
                return retval;
            }
        }
        public string CycleNoStr
        {
            get
            {
                string retval = CycleNo.ToString();
                return retval;
            }
        }
        public string InvMonth
        {
            get
            {
                string retval = InvDateConvert.ToString("MMMMM");
                return retval;
            }
        }
    }
}