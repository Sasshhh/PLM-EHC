using System.Web.Mvc;
using System.IO;

namespace C8.eServices.Mvc.Controllers
{
    [AllowAnonymous]
    public class TestingToolsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetTestState()
        {
            var path = Server.MapPath("~/Tests/Playwright/test-state.json");
            if (System.IO.File.Exists(path))
            {
                var content = System.IO.File.ReadAllText(path);
                return Json(content, JsonRequestBehavior.AllowGet);
            }
            return Json("{}", JsonRequestBehavior.AllowGet);
        }
    }
}
