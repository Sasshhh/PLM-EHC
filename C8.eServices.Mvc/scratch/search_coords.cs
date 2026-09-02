using System;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

public class SearchCoords
{
    public class SimpleListener : ITextExtractionStrategy
    {
        public void BeginTextBlock() {}
        public void EndTextBlock() {}
        public void RenderImage(ImageRenderInfo renderInfo) {}

        public void RenderText(TextRenderInfo renderInfo)
        {
            var segment = renderInfo.GetBaseline();
            var startPoint = segment.GetStartPoint();
            string text = renderInfo.GetText();
            if (startPoint[1] > 350)
            {
                Console.WriteLine("'{0}' at X={1:F2}, Y={2:F2}", text, startPoint[0], startPoint[1]);
            }
        }

        public string GetResultantText() { return ""; }
    }

    public static void Main(string[] args)
    {
        string[] files = new string[] {
            @"C8.eServices.Mvc\PDFTemplates\Cancellation_Lease_Template_Draft.pdf",
            @"C8.eServices.Mvc\PDFTemplates\Notice_To_Vacate_Template_Draft.pdf"
        };
        foreach (var file in files)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("File: " + file);
            PdfReader reader = new PdfReader(file);
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            parser.ProcessContent(1, new SimpleListener());
            reader.Close();
        }
    }
}
