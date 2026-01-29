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

namespace C8.eServices.Mvc.Controllers
{
    public class SmartBillEFTController : ApiController
    {

        public HttpResponseMessage Get(string municipalAccount, string amount, string systemUser, string customer)
        {
            //http://localhost:3456/InstantEFT/SmartBillEFTPayment?municipalAccount=2200786995&amount=50&systemUId=2&customerId=3
            var response = Request.CreateResponse(HttpStatusCode.Found);
            amount = amount + "z";
            municipalAccount = municipalAccount + "z";
            systemUser = systemUser + "z";
            customer = customer + "z";
            object obj = new { municipalAccount = municipalAccount, amount = amount, systemUId = systemUser, customerId = customer };

            string urlPath = HttpContext.Current.Request.Url.Authority;
            string absolute = HttpContext.Current.Request.Url.AbsoluteUri;
            var encry= SecureActionLinkExtension.Encrypt(obj);
            var test = encry.ToString().Replace("{", string.Empty);
            test= test.Replace("}", string.Empty);
            test = Regex.Replace(test, @"\s+", "");
            string urlIQA= "http://" + urlPath + "/QA/InstantEFT/SmartBillEFTPayment?"+ test;
            string urlI1= "https://" + urlPath + "/InstantEFT/SmartBillEFTPayment?"+ test;
            string urlI= "" + urlPath + "/InstantEFT/SmartBillEFTPayment?municipalAccount=" + municipalAccount + "&amount=" + amount+"&systemUId=" + systemUser+"&customerId=" + customer + "";
            response.Headers.Location = new Uri(urlI1);
            return response;
        }

        //protected internal FileContentResult File(byte[] fileContents, string contentType)
        //{
        //    return
        //}
        //public byte[] FileContents { get; }
        //public RedirectToRouteResult Get(string municipalAccount, string amount, string systemUser, string customer)
        //{
        //    var PdfGen = new ReportController().GetPDFChatBot(cusAcc, StateYear, stateMonth);

        //    object obj = new { customerId = customerId, agentId = agentId };
        //    //return RedirectToAction("Index3", SecureActionLinkExtension.Encrypt(obj));
        //}
        //public HttpResponseMessage Get(string municipalAccount, string amount, string systemUser, string customer)
        //{

        //    //Here i need to redirect to a aspx page.
        //    var response = Request.CreateResponse(HttpStatusCode.Moved);
        //    response.Headers.Location = new Uri("your url....");
        //    return response;
        //    //object obj = new { customerId = customerId, agentId = agentId };
        //    //return RedirectToAction("Index3", SecureActionLinkExtension.Encrypt(obj));
        //}


    }
}
