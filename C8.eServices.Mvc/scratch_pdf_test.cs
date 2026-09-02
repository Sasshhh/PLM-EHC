using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

class Program
{
    static void Main()
    {
        string pdfTemplate = @"C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\Revised Lease Agreement_v2.pdf";
        string newFile = @"C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\scratch_test_sig.pdf";

        PdfReader pdfReader = new PdfReader(pdfTemplate);
        PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
        AcroFields pdfFormFields = pdfStamper.AcroFields;

        // Generate a dummy red image
        var bmp = new System.Drawing.Bitmap(500, 150);
        using(var g = System.Drawing.Graphics.FromImage(bmp))
        {
            g.Clear(System.Drawing.Color.Red);
        }
        using(var ms = new MemoryStream())
        {
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var sigImage = iTextSharp.text.Image.GetInstance(ms.ToArray());
            
            var positions = pdfFormFields.GetFieldPositions("PropertyManagerSignature");
            if (positions != null && positions.Count > 0)
            {
                var sigPos = positions[0];
                iTextSharp.text.Rectangle rect = sigPos.position;
                Console.WriteLine(string.Format("Placing image at Page {0}, Rect: {1}, {2}, {3}, {4}", sigPos.page, rect.Left, rect.Bottom, rect.Width, rect.Height));
                sigImage.ScaleToFit(rect.Width, rect.Height);
                sigImage.SetAbsolutePosition(rect.Left, rect.Bottom);
                PdfContentByte cb = pdfStamper.GetOverContent(sigPos.page);
                cb.AddImage(sigImage);
            }
        }
        pdfStamper.FormFlattening = true;
        pdfStamper.Close();
        Console.WriteLine("Done.");
    }
}
