using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Security;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Microsoft.AspNet.Identity;
using C8.eServices.Mvc.DataAccessLayer;

namespace C8.eServices.Mvc.Helpers
{
    public static class AccountHelper
    {
        public static string AccountType(this bool isRms)
        {
            return (isRms) ? "RMS" : "Coins";
        }

        /// <summary>
        /// L.M.20170612 - Gets account type based on Service Unit 
        /// </summary>
        /// <param name="ratesAccount"></param>
        /// <returns></returns>
        public static AccountType GetAccountType(RatesAccount ratesAccount)
        {
            var accountType = new AccountType();
            var context = new eServicesDbContext();

            switch (ratesAccount.ServiceUnitCode)
            {
                case ServiceUnitCodeKeys.MonthlyRates:
                    accountType =
                        context.AccountTypes.FirstOrDefault(
                            at =>
                                at.Key == AccountTypeKeys.Rates && at.IsActive && !at.IsDeleted);
                    if (null == accountType)
                        throw new Exception(string.Format(
                            "Invalid/ missing account type key - {0}", AccountTypeKeys.Rates));
                    break;
                case ServiceUnitCodeKeys.Electricity:
                    accountType =
                        context.AccountTypes.FirstOrDefault(
                            at =>
                                at.Key == AccountTypeKeys.Electricity && at.IsActive &&
                                !at.IsDeleted);
                    if (null == accountType)
                        throw new Exception(string.Format(
                            "Invalid/ missing account type key - {0}",
                            AccountTypeKeys.Electricity));
                    break;
                case ServiceUnitCodeKeys.DomesticRefuse:
                    accountType =
                        context.AccountTypes.FirstOrDefault(
                            at =>
                                at.Key == AccountTypeKeys.Waste && at.IsActive && !at.IsDeleted);
                    if (null == accountType)
                        throw new Exception(string.Format(
                            "Invalid/ missing account type key - {0}", AccountTypeKeys.Waste));
                    break;
                case ServiceUnitCodeKeys.Water:
                    accountType =
                        context.AccountTypes.FirstOrDefault(
                            at =>
                                at.Key == AccountTypeKeys.Water && at.IsActive && !at.IsDeleted);
                    if (null == accountType)
                        throw new Exception(string.Format(
                            "Invalid/ missing account type key - {0}", AccountTypeKeys.Water));
                    break;
            }
            return accountType;
        }
    }
}