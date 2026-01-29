using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using System;
using System.Linq;
using System.Data.Entity;

namespace C8.eServices.Mvc.Helpers
{
    public static class EscalationHelper
    {
        static bool CheckIfValueExist(string x, string y, string t, decimal? c) => (!String.IsNullOrEmpty(x) && !String.IsNullOrEmpty(y) && !String.IsNullOrEmpty(t) && c.HasValue) ? true : false;
        static bool CheckIfMeetCondition(bool y, bool t) => (y && t) ? true : false;
        public static void Esacalations(eServicesDbContext core)
        {
            EscalateYear(core);
            decimal PayableRental = 0;
            var Recs = (EscalationMaster)null;
            var Applications = core.HumanSettlementApplications
                .Include(d => d.UnitTypology)
                .Include(d => d.UnitCategory)
                .Include(d => d.HSIncomeBracket)
                .Include(d => d.Customer)
                .Where(x => x.IsActive).ToList();

            foreach (var item in Applications)
            {
                var Category = item.UnitCategory;
                var Typology = item.UnitTypology;
                var IncomeBraks = item.HSIncomeBracket;

                var master = core.HumanSettlementLeaseMasters.FirstOrDefault(r => r.HumanSettlementApplicationId == item.Id);
                var details = core.HumanSettlementLeaseDetails.FirstOrDefault(r => r.HumanSettlementApplicationId == item.Id);
                var payable = master?.RentalAmount ?? details?.RentalAmount;
                var result = CheckIfValueExist(Category?.Key, Typology?.Key, IncomeBraks?.Key, payable);

                if (result)
                {
                    Recs = core.EscalationMaster
                        .Include(d => d.HSUnitTypology)
                        .Include(d => d.HSUnitCategory)
                        .Include(d => d.HSIncomeBracket)
                        .Include(d => d.Slot1EscalationYears)
                        .Include(d => d.Slot2EscalationYears)
                        .Include(d => d.Slot3EscalationYears)
                        .Include(d => d.Slot4EscalationYears)
                        .Include(d => d.Slot5EscalationYears)
                        .Include(d => d.Slot6EscalationYears)
                        .Where(r => r.HSUnitCategoryId == item.UnitCategory.Id && r.HSUnitTypologyId == item.UnitTypology.Id && r.HSIncomeBracketId == item.HSIncomeBracket.Id)
                        .FirstOrDefault();

                    var year = core.HSEscalationYears.FirstOrDefault(r => r.Id == Recs.HSEscalationYearsId);
                    var condtion = CheckIfMeetCondition(DateTime.Now > Convert.ToDateTime(string.Format("{0}/09/26", year.Name.ToString().Substring(0, 4))),
                        DateTime.Now < Convert.ToDateTime(string.Format("{0}{1}/09/28", DateTime.Now.Year.ToString().Substring(0, 2), year.Name.ToString().Substring(5, 2))));

                    if (condtion)
                    {
                        var emailboodyId = core.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.AnnualRentalEscalation).Id;
                        var ActivityTrackerMessage = core.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AnnualRentalEscalation).Description;
                        if (payable == Recs.Slot1Price)
                        {
                            if (year?.Key != Recs.Slot1EscalationYears.Key)
                            {
                                PayableRental = Recs.Slot2Price;
                                ActivityTrackerMessage = string.Format(ActivityTrackerMessage, Recs.Slot1Price, PayableRental);
                                if (item.IsMaster) master.RentalAmount = PayableRental;
                                if (item.IsMaster) core.Entry(master).State = EntityState.Modified;
                                if (!item.IsMaster) details.RentalAmount = PayableRental;
                                if (!item.IsMaster) core.Entry(details).State = EntityState.Modified;
                                core.SaveChanges();
                                MatchingHelper.ActivityTrackerHuman(core, item.Id, ActivityTrackerMessage, item.Customer.Id);
                                EmailHelper.CustomerEmailOrSMSNotification(core, item.Id, emailboodyId);    
                            }
                        }
                        else if (payable == Recs.Slot2Price)
                        {
                            if (year?.Key != Recs.Slot2EscalationYears.Key)
                            {
                                PayableRental = Recs.Slot3Price;
                                ActivityTrackerMessage = string.Format(ActivityTrackerMessage, Recs.Slot2Price, PayableRental);
                                if (item.IsMaster) master.RentalAmount = PayableRental;
                                if (item.IsMaster) core.Entry(master).State = EntityState.Modified;
                                if (!item.IsMaster) details.RentalAmount = PayableRental;
                                if (!item.IsMaster) core.Entry(details).State = EntityState.Modified;
                                core.SaveChanges();
                                MatchingHelper.ActivityTrackerHuman(core, item.Id, ActivityTrackerMessage, item.Customer.Id);
                                EmailHelper.CustomerEmailOrSMSNotification(core, item.Id, emailboodyId);
                            }
                        }
                        else if (payable == Recs.Slot3Price)
                        {
                            if (year?.Key != Recs.Slot3EscalationYears.Key)
                            {
                                PayableRental = Recs.Slot4Price;
                                ActivityTrackerMessage = string.Format(ActivityTrackerMessage, Recs.Slot3Price, PayableRental);
                                if (item.IsMaster) master.RentalAmount = PayableRental;
                                if (item.IsMaster) core.Entry(master).State = EntityState.Modified;
                                if (!item.IsMaster) details.RentalAmount = PayableRental;
                                if (!item.IsMaster) core.Entry(details).State = EntityState.Modified;
                                core.SaveChanges();
                                MatchingHelper.ActivityTrackerHuman(core, item.Id, ActivityTrackerMessage, item.Customer.Id);
                                EmailHelper.CustomerEmailOrSMSNotification(core, item.Id, emailboodyId);
                            }
                        }
                        else if (payable == Recs.Slot4Price)
                        {
                            if (year?.Key != Recs.Slot4EscalationYears.Key)
                            {
                                PayableRental = Recs.Slot5Price;
                                ActivityTrackerMessage = string.Format(ActivityTrackerMessage, Recs.Slot4Price, PayableRental);
                                if (item.IsMaster) master.RentalAmount = PayableRental;
                                if (item.IsMaster) core.Entry(master).State = EntityState.Modified;
                                if (!item.IsMaster) details.RentalAmount = PayableRental;
                                if (!item.IsMaster) core.Entry(details).State = EntityState.Modified;
                                core.SaveChanges();
                                MatchingHelper.ActivityTrackerHuman(core, item.Id, ActivityTrackerMessage, item.Customer.Id);
                                EmailHelper.CustomerEmailOrSMSNotification(core, item.Id, emailboodyId);
                            }
                        }
                        else if (payable == Recs.Slot5Price)
                        {
                            if (year?.Key != Recs.Slot5EscalationYears.Key)
                            {
                                PayableRental = Recs.Slot6Price;
                                ActivityTrackerMessage = string.Format(ActivityTrackerMessage, Recs.Slot5Price, PayableRental);
                                if (item.IsMaster) master.RentalAmount = PayableRental;
                                if (item.IsMaster) core.Entry(master).State = EntityState.Modified;
                                if (!item.IsMaster) details.RentalAmount = PayableRental;
                                if (!item.IsMaster) core.Entry(details).State = EntityState.Modified;
                                core.SaveChanges();
                                MatchingHelper.ActivityTrackerHuman(core, item.Id, ActivityTrackerMessage, item.Customer.Id);
                                EmailHelper.CustomerEmailOrSMSNotification(core, item.Id, emailboodyId);
                            }
                        }
                        else if (payable == Recs.Slot6Price)
                        {
                            if (year?.Key != Recs.Slot6EscalationYears.Key)
                            {
                                PayableRental = Recs.Slot1Price;
                                ActivityTrackerMessage = string.Format(ActivityTrackerMessage, Recs.Slot6Price, PayableRental);
                                if (item.IsMaster) master.RentalAmount = PayableRental;
                                if (item.IsMaster) core.Entry(master).State = EntityState.Modified;
                                if (!item.IsMaster) details.RentalAmount = PayableRental;
                                if (!item.IsMaster) core.Entry(details).State = EntityState.Modified;
                                core.SaveChanges();
                                MatchingHelper.ActivityTrackerHuman(core, item.Id, ActivityTrackerMessage, item.Customer.Id);
                                EmailHelper.CustomerEmailOrSMSNotification(core, item.Id, emailboodyId);
                            }
                        }
                    }
                }
            }
        }
        public static eServicesDbContext EscalateYear(eServicesDbContext core)
        {
            var Year = DateTime.Now.Year;
            var findItem = core.HSEscalationYears.FirstOrDefault(r => r.Name.Contains(Year.ToString())) ?? null;
            if (findItem == null)
            {
                var Nex = Year + 1; //2022/23
                var Name = string.Format("{0}/{1}", Year, Nex.ToString().Substring(2, 2));
                var description = string.Format("Financial Year Escalation {0}-{1}", Year, Nex);
                var Key = string.Format("fye_{0}_{1}", Year, Nex);

                var es = new HSEscalationYears
                {
                    Name = Name,
                    Description = description,
                    Key = Key,
                    IsActive = true,
                    DepartmentId = 7
                };

                core.HSEscalationYears.Add(es);
                core.SaveChanges();

                var mse = core.EscalationMaster
                        .Include(d => d.Slot1EscalationYears)
                        .Include(d => d.Slot2EscalationYears)
                        .Include(d => d.Slot3EscalationYears)
                        .Include(d => d.Slot4EscalationYears)
                        .Include(d => d.Slot5EscalationYears)
                        .Include(d => d.HSEscalationYears)
                        .Include(d => d.Slot6EscalationYears).ToList();

                foreach (var item in mse)
                {
                    if (!item.HSEscalationYears.Name.Contains(Year.ToString()))
                    {
                        var price = 0.0;
                        if (item.HSEscalationYears?.Key == item.Slot6EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot6Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot1Price = Convert.ToDecimal(price);
                            item.Slot1EscalationYearsId = es.Id;
                            item.HSEscalationYearsId = es.Id;
                            core.SaveChanges();
                        }
                        else if (item.HSEscalationYears?.Key == item.Slot1EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot1Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot2Price = Convert.ToDecimal(price);
                            item.Slot2EscalationYearsId = es.Id;
                            item.HSEscalationYearsId = es.Id;
                            core.SaveChanges();
                        }
                        else if (item.HSEscalationYears?.Key == item.Slot2EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot2Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot3Price = Convert.ToDecimal(price);
                            item.Slot3EscalationYearsId = es.Id;
                            item.HSEscalationYearsId = es.Id;
                            core.SaveChanges();
                        }
                        else if (item.HSEscalationYears?.Key == item.Slot3EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot3Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot4Price = Convert.ToDecimal(price);
                            item.Slot4EscalationYearsId = es.Id;
                            item.HSEscalationYearsId = es.Id;
                            core.SaveChanges();
                        }
                        else if (item.HSEscalationYears?.Key == item.Slot4EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot4Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot5Price = Convert.ToDecimal(price);
                            item.Slot5EscalationYearsId = es.Id;
                            item.HSEscalationYearsId = es.Id;
                            core.SaveChanges();
                        }
                        else if (item.HSEscalationYears?.Key == item.Slot5EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot5Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot6Price = Convert.ToDecimal(price);
                            item.Slot6EscalationYearsId = es.Id;
                            item.HSEscalationYearsId = es.Id;
                            core.SaveChanges();
                        }
                    }
                }
                return core;
            }
            else
            {
                var mse = core.EscalationMaster
                       .Include(d => d.Slot1EscalationYears)
                       .Include(d => d.Slot2EscalationYears)
                       .Include(d => d.Slot3EscalationYears)
                       .Include(d => d.Slot4EscalationYears)
                       .Include(d => d.Slot5EscalationYears)
                       .Include(d => d.HSEscalationYears)
                       .Include(d => d.Slot6EscalationYears).ToList();
                foreach (var item in mse)
                {
                    if (!item.HSEscalationYears.Name.Contains(Year.ToString()))
                    {
                        var price = 0.0;
                        if (item.HSEscalationYears?.Key == item.Slot6EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot6Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot1Price = Convert.ToDecimal(price);
                            item.Slot1EscalationYearsId = findItem.Id;
                            item.HSEscalationYearsId = findItem.Id;
                            core.SaveChanges();
                        }
                        else if (item.HSEscalationYears?.Key == item.Slot1EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot1Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot2Price = Convert.ToDecimal(price);
                            item.Slot2EscalationYearsId = findItem.Id;
                            item.HSEscalationYearsId = findItem.Id;
                            core.SaveChanges();
                        }
                        else if (item.HSEscalationYears?.Key == item.Slot2EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot2Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot3Price = Convert.ToDecimal(price);
                            item.Slot3EscalationYearsId = findItem.Id;
                            item.HSEscalationYearsId = findItem.Id;
                            core.SaveChanges();
                        }
                        else if (item.HSEscalationYears?.Key == item.Slot3EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot3Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot4Price = Convert.ToDecimal(price);
                            item.Slot4EscalationYearsId = findItem.Id;
                            item.HSEscalationYearsId = findItem.Id;
                            core.SaveChanges();
                        }
                        else if (item.HSEscalationYears?.Key == item.Slot4EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot4Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot5Price = Convert.ToDecimal(price);
                            item.Slot5EscalationYearsId = findItem.Id;
                            item.HSEscalationYearsId = findItem.Id;
                            core.SaveChanges();
                        }
                        else if (item.HSEscalationYears?.Key == item.Slot5EscalationYears.Key)
                        {
                            price = Convert.ToDouble(item.Slot5Price);
                            price += Math.Round(price * Convert.ToDouble(item.EscalationPercentage / 100), 2);
                            item.Slot6Price = Convert.ToDecimal(price);
                            item.Slot6EscalationYearsId = findItem.Id;
                            item.HSEscalationYearsId = findItem.Id;
                            core.SaveChanges();
                        }
                    }
                }
                return core;
            }
        }
    }
}