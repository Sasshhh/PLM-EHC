using System;
using System.IO;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

public class FindTextCoordinates
{
    public class SimpleListener : ITextExtractionStrategy
    {
        private StringBuilder result = new StringBuilder();
        public void BeginTextBlock() {}
        public void EndTextBlock() {}
        public void RenderImage(ImageRenderInfo renderInfo) {}

        public void RenderText(TextRenderInfo renderInfo)
        {
            var segment = renderInfo.GetBaseline();
            var startPoint = segment.GetStartPoint();
            string text = renderInfo.GetText();
            if (!string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("Text: '{0}' at X={1:F2}, Y={2:F2}", text, startPoint[0], startPoint[1]);
            }
        }

        public string GetResultantText()
        {
            return result.ToString();
        }
    }

    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: find_text.exe <pdf-file>");
            return;
        }

        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: {0}", filePath);
            return;
        }

        Console.WriteLine("Analyzing file: {0}", filePath);
        PdfReader reader = new PdfReader(filePath);
        for (int page = 1; page <= reader.NumberOfPages; page++)
        {
            Console.WriteLine("--- Page {page} ---");
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            SimpleListener listener = new SimpleListener();
            parser.ProcessContent(page, listener);
        }
        reader.Close();
    }
}
