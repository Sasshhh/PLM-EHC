using System;
using iTextSharp.text.pdf;
using System.Linq;

class Program
{
    static void Main()
    {
        var reader = new PdfReader(@"C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\Revised Lease Agreement_v2.pdf");
        var pdfStamper = new PdfStamper(reader, new System.IO.MemoryStream());
        var pdfFormFields = pdfStamper.AcroFields;
        
        foreach(var key in new string[] { "PropertyManagerSignature", "RevenueManagerSignature", "TenantSignature" })
        {
            var positions = pdfFormFields.GetFieldPositions(key);
            if(positions != null && positions.Count > 0)
            {
                var rect = positions[0].position;
                Console.WriteLine(string.Format("{0} -> W: {1}, H: {2}, L: {3}, B: {4}", key, rect.Width, rect.Height, rect.Left, rect.Bottom));
            }
            else
            {
                Console.WriteLine(string.Format("{0} -> NOT FOUND OR NO POSITIONS", key));
            }
        }
    }
}
