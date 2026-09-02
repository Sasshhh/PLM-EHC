using System;
using System.IO;
using System.Reflection;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Revised Lease Agreement_v2.pdf ===");
        DumpFields(@"c:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\Revised Lease Agreement_v2.pdf");

        var renewal = @"c:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\LA_Template.pdf";
        if (File.Exists(renewal))
        {
            Console.WriteLine("\n=== LA_Template.pdf (Renewal) ===");
            DumpFields(renewal);
        }
    }

    static void DumpFields(string path)
    {
        var asm = Assembly.LoadFrom(@"C:\REPO\PLM V1\PLM-EHC\packages\iTextSharp.5.5.12\lib\itextsharp.dll");
        var readerType = asm.GetType("iTextSharp.text.pdf.PdfReader");
        var reader = Activator.CreateInstance(readerType, new object[] { path });
        var acroFieldsProp = readerType.GetProperty("AcroFields");
        var acroFields = acroFieldsProp.GetValue(reader);
        var fieldsProp = acroFields.GetType().GetProperty("Fields");
        var fields = fieldsProp.GetValue(acroFields) as System.Collections.IDictionary;

        Console.WriteLine("Total fields: " + fields.Count);
        foreach (var key in fields.Keys)
            Console.WriteLine("  " + key);
    }
}
