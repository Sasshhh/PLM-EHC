using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.IO;
using C8.eServices.Mvc.Helpers;

namespace C8.eServices.Mvc.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DecryptParameterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {  
            var cc = new C8.eServices.Mvc.Controllers.HumanSettlementApplicationController();
            Dictionary<string, object> decryptedParameters = new Dictionary<string, object>();

            if (HttpContext.Current.Request.QueryString.Get("q") != null)
            {               
              
                string encryptedQueryString = HttpContext.Current.Request.QueryString.Get("q");
                if (encryptedQueryString.Contains(" "))
                    encryptedQueryString = encryptedQueryString.Replace(" ", "+");
                string decryptedString;

                try
                {
                    decryptedString = Decrypt(encryptedQueryString);
                }
                catch (Exception x)
                {
                    cc.ReturnError();
                    throw new Exception("Malicious Activity", x);
                }

                string[] delimiter = { "&" };
                string[] paramsArrs = decryptedString.Split(delimiter, StringSplitOptions.None);
                char[] paramDelimiter = { '=' };

                for (int i = 0; i < paramsArrs.Length; i++)
                {
                    string[] paramArr = paramsArrs[i].Split(paramDelimiter, 2);

                    if (Int32.TryParse(paramArr[1], out var oi64))
                        decryptedParameters.Add(paramArr[0], oi64);
                    else if (Int64.TryParse(paramArr[1], out var oi32))
                        decryptedParameters.Add(paramArr[0], oi32);
                    else if (DateTime.TryParse(paramArr[1], out var od))
                        decryptedParameters.Add(paramArr[0], od);
                    else if (decimal.TryParse(paramArr[1], out var odec))
                        decryptedParameters.Add(paramArr[0], odec);
                    else if (double.TryParse(paramArr[1], out var odo))
                        decryptedParameters.Add(paramArr[0], odo);
                    else if (float.TryParse(paramArr[1], out var of))
                        decryptedParameters.Add(paramArr[0], of);
                    else decryptedParameters.Add(paramArr[0], paramArr[1]);
                }
            }
            else
            {
                cc.ReturnError();
                throw new Exception("Malicious Activity");
            }
              

            for (int i = 0; i < decryptedParameters.Count; i++)
            {
                filterContext.ActionParameters[decryptedParameters.Keys.ElementAt(i)] = decryptedParameters.Values.ElementAt(i);
            }

            base.OnActionExecuting(filterContext);
        }

        private string Decrypt(string encryptedText)
        {
            var aes = new AesCrypto();
            var d = aes.Decrypt(encryptedText);

            return d;
        }
    }
}