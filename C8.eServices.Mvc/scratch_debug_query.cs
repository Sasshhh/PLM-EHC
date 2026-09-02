using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Keys;

namespace TestApp
{
    public class Program
    {
        public static void Main()
        {
            using (var db = new eServicesDbContext())
            {
                var Keys = db.Status;
                int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;
                
                var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.AgreementApproval).FirstOrDefault().Id;
                var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.PropertyManager).FirstOrDefault().Value);

                var rrq = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.Clerk.SystemUser)
                    .Where(x => x.ResponsibilityTypeId == ResponsibilityTypeId && x.Clerk.Id == activeDirectoryOn && x.StatusId == SubmittedId)
                    .ToList();
                
                var list = rrq.Select(x => x.PropertyLeaseApplicationId).ToList();

                int AwaitingLeaseAgreementApprovalId = db.Status.FirstOrDefault(i => i.Key == StatusKeys.AwaitingLeaseAgreementApproval).Id;

                var MasterApplication = db.propertyLeaseAgreementMasters
                    .Where(x => x.PropertyManagerSigned == false && list.Contains(x.PropertyLeaseApplicationId))
                    .ToList();
                
                var ListMaster = MasterApplication.Select(x => x.PropertyLeaseApplicationId).ToList();

                var rCSApplicationStatus = db.PropertyLeaseApplications
                    .Where(x => x.IsDeleted == false && ListMaster.Contains(x.Id) && x.StatusId == AwaitingLeaseAgreementApprovalId)
                    .ToList();

                Console.WriteLine("RRQ Count: " + rrq.Count);
                Console.WriteLine("list Count: " + list.Count);
                if(list.Count > 0) Console.WriteLine("list[0]: " + list[0]);
                Console.WriteLine("MasterApplication Count: " + MasterApplication.Count);
                if(ListMaster.Count > 0) Console.WriteLine("ListMaster[0]: " + ListMaster[0]);
                Console.WriteLine("rCSApplicationStatus Count: " + rCSApplicationStatus.Count);
            }
        }
    }
}
