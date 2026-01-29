using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class SAMSViewModel
    {
    }


    public class AssetResult
    {
        public string assetGroupsAddressCity { get; set; }
        public string asset_id { get; set; }
        public object AssetNo { get; set; }
        public object sg_number { get; set; }
        public object asset_category { get; set; }
        public object asset_subcategory { get; set; }
        public object asset_group_type { get; set; }
        public string asset_type { get; set; }
        public string address_street { get; set; }
        public string asset_groups_name { get; set; }
        public object asset_groups_address_zip { get; set; }
        public object asset_descriptor_size { get; set; }
        public string building_number { get; set; }
        public string floor_number { get; set; }
        public string room_number { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public object acquisition_date { get; set; }
        public double acquisition_amount { get; set; }
        public object revalued_amount_at_last_revaluation { get; set; }
        public object last_revaluation_date { get; set; }
        public double maintenance_budget { get; set; }

        [NotMapped]
        public object Error { get; set; }
    }

    public class AssetRequest
    {
        public string sg_number { get; set; }
        public string address_street { get; set; }
        public string building_number { get; set; }
        public string floor_number { get; set; }
        public string room_number { get; set; }
    }
}