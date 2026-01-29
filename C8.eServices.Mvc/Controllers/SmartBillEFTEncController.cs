using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using System.Net.Http;
using C8.eServices.Mvc.Helpers;
using System.Text.RegularExpressions;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.DataAccessLayer;

namespace C8.eServices.Mvc.Controllers
{
    public class SmartBillEFTEncController : ApiController
    {

        public HttpResponseMessage Get(string q)
        {
            //string municipalAccount, string amount, string systemUser, string customer
            //http://localhost:3456/InstantEFT/SmartBillEFTPayment?municipalAccount=2200786995&amount=50&systemUId=2&customerId=3
            var response = Request.CreateResponse(HttpStatusCode.Found);
            eServicesDbContext context = new eServicesDbContext();
            AesCrypto aes = new AesCrypto();
            var decrypted = aes.Decrypt(q);
            var values = decrypted.Split('|');
            var municipalAccount = values[0];
            var amount = values[1];
            var systemUser = values[2];
            var customer = values[3];
            amount = amount + "z";
            municipalAccount = municipalAccount + "z";
            systemUser = systemUser + "z";
            customer = customer + "z";
            object obj = new { municipalAccount = municipalAccount, amount = amount, systemUId = systemUser, customerId = customer };

            string urlPath = HttpContext.Current.Request.Url.Authority;
            string absolute = HttpContext.Current.Request.Url.AbsoluteUri;
            var encry = SecureActionLinkExtension.Encrypt(obj);
            var test = encry.ToString().Replace("{", string.Empty);
            test = test.Replace("}", string.Empty);
            test = Regex.Replace(test, @"\s+", "");
           

            string urlI1 = "https://" + urlPath + "/InstantEFT/SmartBillEFTPayment?" + test;
            string urlI = "" + urlPath + "/InstantEFT/SmartBillEFTPayment?municipalAccount=" + municipalAccount + "&amount=" + amount + "&systemUId=" + systemUser + "&customerId=" + customer + "";
            response.Headers.Location = new Uri(urlI1);
            return response;
        }


    }
}
