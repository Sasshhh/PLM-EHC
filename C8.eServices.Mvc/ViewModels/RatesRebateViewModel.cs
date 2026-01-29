using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    public class RatesRebateViewModel
    {
        public RatesRebateViewModel()
        {
            CurrentCustomerProfileViewModel = new CustomerProfileViewModel()
            {
                Agent = new Agent(),
                Customer = new Customer(),
                Entity = new Entity()
            };

            OwnerCustomerProfileViewModel = new CustomerProfileViewModel()
            {
                Agent = new Agent(),
                Customer = new Customer(),
                Entity = new Entity()
            };

            RatesRebate = new RatesRebate();
            ApplicantCapacities = GetApplicantCapacities();
        }

        [Display(Name="Same as physical")]
        public bool IsDomiciliumSameAsPhysicalAddress { get; set; }

        [Display(Name="Same as applicant")]
        public bool IsExecutorSameAsApplicant { get; set; }

        public bool IsAgent { get; set; }

        public List<ApplicantCapacity> ApplicantCapacities { get; set; }

        public int? ThirdPartyOwnerCustomerId { get; set; }

        public int? ApplicantCustomerId { get; set; }

        [Required]
        public int? ApplicantCapacityId { get; set; }

        public CustomerProfileViewModel CurrentCustomerProfileViewModel { get; set; }

        public CustomerProfileViewModel OwnerCustomerProfileViewModel { get; set; }

        public RatesRebate RatesRebate { get; set; }

        public IEnumerable<RatesRebateProperty> RatesRebateProperties { get; set; }

        public IEnumerable<RatesRebatePropertyViewModel> RatesRebatePropertiesVm { get; set; }


        public bool Declaration { get; set; }

        [Display(Name = "Application Status")]
        public  int RatesRebateStatusId { get; set; }


        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)] 
        public string Notes { get; set; }


        public Entity Entity { get; set; }

        public List<Note> NoteList { get; set; } 
        
        /// <summary>
        /// Gets the applicant capacities.
        /// Checks if the application is done 
        /// </summary>
        /// <returns></returns>
        public List<ApplicantCapacity> GetApplicantCapacities()
        {
            return new[]
            {
                new ApplicantCapacity {Id = 1, Label = "In your own capacity"},
                new ApplicantCapacity
                {
                    Id = 2,
                    Label = "3rd Party (If '3rd Party', please attach a 'Letter of Authority')"
                }
            }.ToList();
        }
    }

    public class ApplicantCapacity
    {
        public int Id { get; set; }
        public string Label { get; set; }
    }
}