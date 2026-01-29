using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace C8.eServices.Mvc.Controllers
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/
        public ViewResult Index()
        {
            return View("_Error");
        }
        public ViewResult NotFound()
        {
            //Response.StatusCode = 404;  //you may want to set this to 200
            return View("_Error");
        }
	}
}