using System;
using System.Net;

namespace C8.eServices.Mvc.Helpers
{
    public class CaptchaHelper
    {
        public bool ValidateCaptcha(string response)
        {
            var success = false;
            try
            {
                var validateString = string.Format(
                    "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}",
                    "6Le0Pw8cAAAAAI5AZSdKo9gH0Ur5Im7JUU1GpoSP",
                    response);

                var request = (HttpWebRequest)WebRequest.Create(validateString);
                request.Timeout = 5000;
                request.ReadWriteTimeout = 5000;

                using (var webResponse = (HttpWebResponse)request.GetResponse())
                using (var stream = webResponse.GetResponseStream())
                using (var reader = new System.IO.StreamReader(stream))
                {
                    var recaptchaResult = reader.ReadToEnd();
                    if (recaptchaResult.ToLower().Contains("true"))
                    {
                        success = true;
                    }
                }
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }
    }
}