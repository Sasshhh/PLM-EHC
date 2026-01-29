using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Keys
{
    public class PaymentGatewayKeys
    {
        //Payment gateway AES Cipher
        public const string PGCipher = "pg_cipher_phrase";

        // Payment Types
        public const string PTMasterpass = "pg_masterpass";
        public const string PTIEFT = "pg_ieft";
        public const string PTPending = "pg_pendingtype";

        //Domain Master Data
        public const string PGDomain = "pg_domain";



        //Merchants (3rd party Applications)
        public const string CemeterySystem = "pg_app_cms";
        public const string HallBookingsSystem = "pg_app_hbs";
        public const string TrafficFinesSystem = "pg_app_tfs";
        public const string TownPlanningSystem = "pg_app_tps";
        public const string PrepaidElectricityToken = "pg_app_pet";

        public const string CemeterySystemEFT = "pg_app_cms_eft";
        public const string HallBookingsSystemEFT = "pg_app_hbs_eft";
        public const string TrafficFinesSystemEFT = "pg_app_tfs_eft";
        public const string TownPlanningSystemEFT = "pg_app_tps_eft";
        public const string PrepaidElectricityTokenEFT = "pg_app_pet_eft";

        //Reference Generator Seq Keys
        //cemetery
        public const string CMSMpassBatchSequence = "cms_mpass_daily_sequence_counter";
        public const string CMSMpassBatchSequenceLimit = "cms_mpass_daily_sequence_limiter";
        public const string CMSEFTBatchSequence = "cms_eft_daily_sequence_counter";
        public const string CMSEFTBatchSequenceLimit = "cms_eft_daily_sequence_limiter";
        //halls
        public const string HBSMpassBatchSequence = "hbs_mpass_daily_sequence_counter";
        public const string HBSMpassBatchSequenceLimit = "hbs_mpass_daily_sequence_limiter";
        public const string HBSEFTBatchSequence = "hbs_eft_daily_sequence_counter";
        public const string HBSEFTBatchSequenceLimit = "hbs_eft_daily_sequence_limiter";
        //fines
        public const string TFSMpassBatchSequence = "tfs_mpass_daily_sequence_counter";
        public const string TFSMpassBatchSequenceLimit = "tfs_mpass_daily_sequence_limiter";
        public const string TFSEFTBatchSequence = "tfs_eft_daily_sequence_counter";
        public const string TFSEFTBatchSequenceLimit = "tfs_eft_daily_sequence_limiter";
        //Town Planning
        public const string TPSMpassBatchSequence = "tps_mpass_daily_sequence_counter";
        public const string TPSMpassBatchSequenceLimit = "tps_mpass_daily_sequence_limiter";
        public const string TPSEFTBatchSequence = "tps_eft_daily_sequence_counter";
        public const string TPSEFTBatchSequenceLimit = "tps_eft_daily_sequence_limiter";

        //Prepaid Electricity Tokens
        public const string PETMpassBatchSequence = "pet_mpass_daily_sequence_counter";
        public const string PETMpassBatchSequenceLimit = "pet_mpass_daily_sequence_limiter";
        public const string PETEFTBatchSequence = "pet_eft_daily_sequence_counter";
        public const string PETEFTBatchSequenceLimit = "pet_eft_daily_sequence_limiter";

  

    }
}