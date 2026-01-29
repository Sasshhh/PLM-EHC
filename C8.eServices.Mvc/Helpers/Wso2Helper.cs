using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using System.Data.Entity;



namespace C8.eServices.Mvc.Helpers
{
    public class Wso2Helper
    {
        /// <summary>
        /// Used to encrypt sensitive data for migration to WSO2 IAM.
        /// </summary>
        /// <param name="referenceId"></param>
        /// <param name="cacheText"></param>
        public static void Store(Int64 referenceId, string cacheText)
        {
            try
            {
                using (var core = new eServicesDbContext())
                {
                    var cache = new Wso2Cache();
                    if (!core.Wso2Caches.Any(o => o.ReferenceId == referenceId))
                    {
                        cache.ReferenceId = referenceId;
                        cache.CacheText = new AesCrypto().Encrypt(cacheText);
                        cache.ModifiedDateTime = DateTime.Now;
                        core.Wso2Caches.Add(cache);
                        core.SaveChanges();
                    }
                    else
                    {
                        cache = core.Wso2Caches.FirstOrDefault(o => o.ReferenceId == referenceId);
                        cache.CacheText = new AesCrypto().Encrypt(cacheText);
                        cache.ModifiedDateTime = DateTime.Now;
                        core.Entry(cache).State = EntityState.Modified;
                        core.SaveChanges();
                    }
                }
            }
            catch (Exception x)
            {

                throw x;
            }
        }
    }
}
