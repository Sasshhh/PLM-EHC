using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace C8.eServices.Mvc.Helpers
{
    public static class RenderHelper
    {
        public static string PartialView( Controller controller, string viewName, object model )
        {
            controller.ViewData.Model = model;

            using ( var sw = new StringWriter() )
            {
                var viewResult = ViewEngines.Engines.FindPartialView( controller.ControllerContext, viewName );
                var viewContext = new ViewContext( controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw );

                viewResult.View.Render( viewContext, sw );
                viewResult.ViewEngine.ReleaseView( controller.ControllerContext, viewResult.View );

                return sw.ToString();
            }
        }

        /// <summary>
        /// L.M.20160720 - Trancates the string for display [Max 25 characters (Spaces inclusive) 
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns></returns>
        public static string Truncate(string value)
        {
            var length = value.Count();

            if (length > 25)
            {
                var indexOfFirstSpace = value.IndexOf("", 25, StringComparison.Ordinal);
                value = value.Substring(0, indexOfFirstSpace -2) + "...";
            }
            return value;
        }
    }
}