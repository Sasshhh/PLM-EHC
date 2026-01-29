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

namespace C8.eServices.Mvc.Controllers
{
    public class DownloadPDFController : ApiController
    {
        //protected internal FileContentResult File(byte[] fileContents, string contentType)
        //{
        //    return
        //}
        //public byte[] FileContents { get; }
        public HttpResponseMessage Get(string cusAcc, string StateYear, string stateMonth)
        {
            var PdfGen = new ReportController().GetPDFChatBot(cusAcc, StateYear, stateMonth);

            //var test = "";
            return PdfGen;
        }

    }
}
