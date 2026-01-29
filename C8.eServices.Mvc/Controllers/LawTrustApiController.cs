using C8.eServices.Mvc.ApiServices;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace C8.eServices.Mvc.Controllers
{
    public class LawTrustApiController : ApiController
    {



        //public string lawtrustcall(string filename ,byte[] newBytes,string email)
        //{

        //    LawTrustApi law = new LawTrustApi();


        //    string link = law.lawtrustintegrration(filename, newBytes,email);
        //    return link;
            
        //}

        [HttpGet]
        public async Task<string> Upload(string filename)
        {
            var IntegrationLink = "";
            try
            {
              
                var httpRequest = System.Web.HttpContext.Current.Request;

                if (httpRequest.Headers["Docbytes"].Count() > 0)
                {
                    var re = httpRequest.Headers["Docbytes"];
                    var se = httpRequest.Headers["Docbytes2"];
                    string sss = re + se;
                    byte[] newBytes = Convert.FromBase64String(sss);
                    String email="suhail.dada@xetgroup.com";
                   //base64EncodedBytes = System.Convert.FromBase64String();

                    //IntegrationLink = lawtrustcall(filename, newBytes,email);
                }

            }
            catch (Exception e)
            {

            }
            return IntegrationLink;
        }




        //public HttpResponseMessage Get()
        //{
        //    var baseContext = HttpContext.Current;
        //    var IntegrationLink = lawtrustcall() ?? null;
        //    if (IntegrationLink != null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, IntegrationLink);
        //    }
        //    else
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Integration Link Not Returned");
        //    }
        //}





    }
}
