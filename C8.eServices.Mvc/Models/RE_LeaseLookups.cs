using System;
using System.Collections.Generic;
using System.Linq;

namespace C8.eServices.Mvc.Models
{
    /// <summary>
    /// DPRE lease dropdown values and rules. Values are ASSUMPTIONS until the BA answers the referenced
    /// DPRE-Q questions; change them here only (no database change needed).
    /// </summary>
    public static class RE_LeaseLookups
    {
        // UCXX Step 5 list. The Master Data has no Telecommunications class; the import assigns it to the 57 leases
        // listed under the TELECOMMUNICATIONS "CCA" in the dashboard tabs (ASSUMPTION, see PR notes).
        public static readonly string[] Classifications =
        {
            "Business Hub", "Commercial", "Outdoor Advertising", "Telecommunications", "Social", "Agricultural", "Servitude"
        };

        // ASSUMPTION: how the 9 spreadsheet classifications map to the 7 UCXX classes until the BA confirms.
        // Unmapped/uncertain values keep the original in SourceClassification so nothing is lost.
        public static readonly Dictionary<string, string> SourceClassificationMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Commercial", "Commercial" },
            { "Outdoor Advertising", "Outdoor Advertising" },
            { "Road Reserves", "Servitude" },
            { "SRAC/ Parks", "Social" },
            { "Residential", "Social" },
            { "Housing", "Social" },
            { "EMM Services - Social Responsibilities", "Social" },
            { "EMM Services - Old Age Home", "Social" },
            { "EMM Services - School", "Social" },
        };

        public static readonly string[] LeaseTerms =
        {
            "Long-Term/Indefinite", "Lease Agreement", "User Agreement (36 months)",
            "Temporary Occupation (PTO: 12 months)", "Month-To-Month (Extension)"
        };

        public static readonly string[] ApplicantTypes =
        {
            "Company", "Close Corporation", "Individual/Natural Person", "NGO/NPO", "Partnership", "Trust",
            "Organ of State/Government Entity", "Other"
        };

        public static readonly string[] ContactTypes = { "Tenant", "Authorised Representative" };
        public static readonly string[] RecordStatuses = { "Active", "Inactive" };
        public static readonly string[] PaymentFrequencies = { "Monthly", "Quarterly", "Bi-Annually", "Annually" };

        // CCA list used by the Lease Dashboard tabs (11 values incl. two portfolio "CCAs").
        public static readonly string[] Ccas =
        {
            "ALBERTON", "BENONI", "BOKSBURG", "BRAKPAN", "EDENVALE", "GERMISTON", "KEMPTON PARK", "NIGEL", "SPRINGS",
            "OUTDOOR ADVERTISING", "TELECOMMUNICATIONS"
        };

        public const string StatusMoa = "MOA";
        public const string StatusNoLease = "No Lease";
        public const string StatusIndefinite = "Indefinite";
        public const string StatusExpired = "Expired";
        public const string StatusActive = "Active";
        public const string StatusInactive = "Inactive";

        public static readonly string[] DashboardStatuses = { StatusActive, StatusExpired, StatusIndefinite, StatusNoLease, StatusMoa, StatusInactive };

        /// <summary>
        /// Same buckets as the Lease Dashboard tabs. This order reproduces the spreadsheet's 5 tabs for all 684 leases
        /// (checked against the 31 Aug 2026 workbook): MOA, then Indefinite, then by end date (on or before today = Expired,
        /// later = Active), and no end date = No Lease in Place. "Lease Agreement in Place?" does not drive the tabs.
        /// Inactive records (manually closed) sit outside the five tabs.
        /// </summary>
        public static string DeriveDashboardStatus(RE_Lease lease, DateTime today)
        {
            if (string.Equals(lease.RecordStatus, StatusInactive, StringComparison.OrdinalIgnoreCase)) return StatusInactive;
            if (lease.IsMemorandumOfAgreement) return StatusMoa;
            if (lease.LeaseIndefinite) return StatusIndefinite;
            var end = lease.LeaseTerminationDate ?? lease.LeaseEndDate;
            if (!end.HasValue) return StatusNoLease;
            return end.Value.Date <= today.Date ? StatusExpired : StatusActive;
        }

        public static string MapSourceClassification(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return null;
            string mapped;
            if (SourceClassificationMap.TryGetValue(source.Trim(), out mapped)) return mapped;
            return Classifications.FirstOrDefault(c => c.Equals(source.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
