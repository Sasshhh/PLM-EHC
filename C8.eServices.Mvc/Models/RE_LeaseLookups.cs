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
        // listed under the TELECOMMUNICATIONS "CCA" in the dashboard tabs (ASSUMPTION, DPRE-Q06).
        public static readonly string[] Classifications =
        {
            "Business Hub", "Commercial", "Outdoor Advertising", "Telecommunications", "Social", "Agricultural", "Servitude"
        };

        // ASSUMPTION (DPRE-Q06): how the 9 spreadsheet classifications map to the 7 UCXX classes until the BA confirms.
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

        /// <summary>UC05 Step 2: lease types offered under Lease Applications.</summary>
        public class LeaseApplicationType
        {
            public string Name { get; set; }
            public string RefCode { get; set; }       // used in the reference number, e.g. DPRE-BUS 2026/0001
            public string Description { get; set; }
            public bool OnlineFormAvailable { get; set; }
        }

        // Codes from the SDS: DPRE-BUS (Business/Commercial Hub), DPRE-OA, DPRE-TEL, DPRE-LH (Concession/Leasehold).
        // ASSUMPTIONS until DPRE-Q35/Q45 are answered: Commercial uses the Business/Commercial Hub form and DPRE-BUS;
        // Social, Agricultural and Servitude use DPRE-LH. OA, TEL and LH share one site form (RealEstate/CaptureSite).
        public static readonly LeaseApplicationType[] LeaseApplicationTypes =
        {
            new LeaseApplicationType { Name = "Business Hub", RefCode = "BUS", OnlineFormAvailable = true, Description = "Retail and office units in a municipal business hub facility." },
            new LeaseApplicationType { Name = "Commercial", RefCode = "BUS", OnlineFormAvailable = true, Description = "Commercial letting space in a municipal facility." },
            new LeaseApplicationType { Name = "Outdoor Advertising", RefCode = "OA", OnlineFormAvailable = true, Description = "Billboards and other outdoor advertising sites." },
            new LeaseApplicationType { Name = "Telecommunications", RefCode = "TEL", OnlineFormAvailable = true, Description = "Telecommunication masts, rooftop sites and equipment." },
            new LeaseApplicationType { Name = "Social", RefCode = "LH", OnlineFormAvailable = true, Description = "Community, NPO and social-purpose use of municipal property." },
            new LeaseApplicationType { Name = "Agricultural", RefCode = "LH", OnlineFormAvailable = true, Description = "Agricultural use of municipal land." },
            new LeaseApplicationType { Name = "Servitude", RefCode = "LH", OnlineFormAvailable = true, Description = "Servitudes and road reserve encroachments." },
        };

        public static LeaseApplicationType FindLeaseApplicationType(string name)
        {
            return LeaseApplicationTypes.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        // ---- UC05 site forms (Outdoor Advertising, Telecommunications, Concession/Leasehold) ----

        // ASSUMPTION (DPRE-Q38): one standard Applicant Type list for every lease type (the UCXX list).
        public static readonly string[] SignLocationTypes = { "An Individual ERF", "A Farm Portion/Agricultural Holding", "A Municipal Road Reserve (Side-walk)" };

        // ASSUMPTION (DPRE-Q44): "Portal Gantry" from the paper form is listed as "Portal Billboard" as in the SDS.
        public static readonly string[] SignTypes =
        {
            "Bridge Sign", "Cantilever Gantry", "Electronic Billboard", "Flat Sign", "Gantry", "Large Billboard", "Portal Billboard",
            "Service Facility Sign", "Small Billboard", "Super Billboard", "Undetermined Sign", "Other"
        };

        public static readonly string[] SignElevations = { "Single-Sided", "Double-Sided", "Triple-Sided", "Other" };

        public static readonly string[] OaTowns = { "Alberton", "Benoni", "Boksburg", "Brakpan", "Edenvale", "Germiston", "Kempton Park", "Nigel", "Springs" };

        public static readonly string[] TelecomInstallationTypes =
        {
            "Antenna/Repeater Affixed to Building/Mast/Tower",
            "Antenna/Repeater on Top of Advertisement Sign",
            "Antenna/Repeater on Water Tower with Base Station",
            "Cellular Installation on Rooftop",
            "Cellular Mast and Base Station Erected on Land Parcel",
            "Cellular Mast Disguised as a Streetlight Pole",
            "Free Standing Radio Mast/Pole",
            "Free Standing Wi-Fi Mast/Tower",
            "Security Camera Affixed to a Pole situated within a Road Reserve",
            "Wi-Fi Antenna/Repeater Affixed to a Streetlight Pole/Tower/Mast/Building",
            "Other"
        };

        public static readonly string[] IdentificationTypes =
        {
            "RSA ID Number", "Passport Number", "Company Registration Number", "NGO Registration Number", "NPO Registration Number",
            "Trust Registration Number", "Organ of State", "Other"
        };

        // Ekurhuleni bounding box used to sanity-check Telecom coordinates (ASSUMPTION, DPRE-Q49).
        public const decimal EkurhuleniMinLat = -26.55m, EkurhuleniMaxLat = -25.90m, EkurhuleniMinLng = 27.95m, EkurhuleniMaxLng = 28.75m;

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

        /// <summary>
        /// ASSUMPTION (DPRE-Q17): new Lease Reference Numbers continue the register's existing series:
        /// LR50xxxx Outdoor Advertising, LR70xxxx MOA, LR40xxxx no agreement in place, LR30xxxx everything else.
        /// Returns the series prefix, e.g. "LR30".
        /// </summary>
        public static string LeaseReferenceSeries(string classification, bool agreementInPlace, bool isMoa)
        {
            if (isMoa) return "LR70";
            if (string.Equals(classification, "Outdoor Advertising", StringComparison.OrdinalIgnoreCase)) return "LR50";
            if (!agreementInPlace) return "LR40";
            return "LR30";
        }

        /// <summary>Next number in a series, e.g. LR300448 -> LR300449. Six digits after "LR".</summary>
        public static string NextLeaseReference(string series, IEnumerable<string> existingInSeries)
        {
            int max = int.Parse(series.Substring(2)) * 10000;
            foreach (var r in existingInSeries)
            {
                int n;
                if (r != null && r.Length == 8 && int.TryParse(r.Substring(2), out n) && n > max) max = n;
            }
            return "LR" + (max + 1).ToString("D6");
        }

        /// <summary>
        /// ASSUMPTION (DPRE-Q15): escalations recur yearly on the Escalation Date's day and month; the dashboard lists escalations
        /// and lease ends due within the alert window, addressed to the Escalation Alerts Recipient. E-mail alerts are not sent yet.
        /// </summary>
        public static List<RE_LeaseAlert> UpcomingAlerts(IEnumerable<RE_Lease> leases, DateTime today, int windowDays)
        {
            var list = new List<RE_LeaseAlert>();
            var until = today.AddDays(windowDays);
            foreach (var l in leases)
            {
                var status = DeriveDashboardStatus(l, today);
                if (status == StatusInactive || status == StatusExpired) continue;
                if (l.EscalationDate.HasValue)
                {
                    var e = l.EscalationDate.Value.Date;
                    if (e < today)
                    {
                        int years = today.Year - e.Year;
                        e = SafeAddYears(e, years);
                        if (e < today) e = SafeAddYears(l.EscalationDate.Value.Date, years + 1);
                    }
                    var end = l.LeaseTerminationDate ?? l.LeaseEndDate;
                    if (e <= until && (!end.HasValue || e <= end.Value))
                        list.Add(new RE_LeaseAlert { Lease = l, Kind = "Escalation", DueDate = e });
                }
                var endDate = l.LeaseTerminationDate ?? l.LeaseEndDate;
                if (!l.LeaseIndefinite && endDate.HasValue && endDate.Value.Date > today && endDate.Value.Date <= until)
                    list.Add(new RE_LeaseAlert { Lease = l, Kind = "Lease ends", DueDate = endDate.Value.Date });
            }
            return list.OrderBy(a => a.DueDate).ToList();
        }

        private static DateTime SafeAddYears(DateTime d, int years)
        {
            // 29 Feb on a non-leap year becomes 28 Feb
            return d.AddYears(years);
        }

        public static string MapSourceClassification(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return null;
            string mapped;
            if (SourceClassificationMap.TryGetValue(source.Trim(), out mapped)) return mapped;
            return Classifications.FirstOrDefault(c => c.Equals(source.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    public class RE_LeaseAlert
    {
        public RE_Lease Lease { get; set; }
        public string Kind { get; set; }
        public DateTime DueDate { get; set; }
    }
}
