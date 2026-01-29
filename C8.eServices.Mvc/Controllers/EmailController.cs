using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Controllers
{
    public class EmailController : Controller
    {
        ////
        //// GET: /Email/
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public ActionResult SendEmail()
        {
            return View();
        }

        [HttpPost]
        [Authorize( Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" )]
        public ActionResult SendEmail(string mailTo, string Cc, string Bcc, string subject, string body)
        {
            var result = "failed";         
            try
            {
                if (!string.IsNullOrEmpty(mailTo))
                {
                    #region Initialize parameters

                    var toList = mailTo != null ? new Email().ExtractEmails(mailTo) : null;
                    var bccList = Bcc != null ? new Email().ExtractEmails(Bcc) : null;
                    var ccList = Cc != null ? new Email().ExtractEmails(Cc) : null;
                    var bcc = "";
                    var to = "";
                    var cc = "";

                    if (toList != null)
                    {
                        foreach (var _to in toList)
                        {
                            to = to + ";" + _to;
                        }
                    }
                    if (ccList != null)
                    {
                        foreach (var _cc in ccList)
                        {
                            cc = cc + ";" + _cc;
                        }
                    }
                    if (bccList != null)
                    {
                        foreach (var _bcc in bccList)
                        {
                            bcc = bcc + "," + _bcc;
                        }
                    }

                    var _subject = subject;
                    var _body = body;

                    #endregion

                    //Sends Email
                    var email = new Email();
                    email.GenerateEmail(to, _subject, _body, null, false,AppSettingKeys.EservicesDefaultEmailTemplate, ccList: cc, bccList: bcc);
                    result = "success";
                }
                
                var data = new
                {
                    status = result,
                };

                return Json(data);
            }
            catch (Exception ex)
            {
               throw ex;
            }
        }
    }
}