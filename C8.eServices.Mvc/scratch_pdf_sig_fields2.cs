using System;
using iTextSharp.text.pdf;
using System.Linq;

class Program
{
    static void Main()
    {
        var reader = new PdfReader(@"C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\Revised Lease Agreement_v2.pdf");
        foreach(var key in reader.AcroFields.Fields.Keys)
        {
            var keyStr = key.ToString().ToLower();
            if (keyStr.Contains("sign") || keyStr.Contains("manag") || keyStr.Contains("ceo") || keyStr.Contains("rm") || keyStr.Contains("pm") || keyStr.Contains("tenant"))
            {
                Console.WriteLine("FIELD: " + key);
            }
        }
    }
}
