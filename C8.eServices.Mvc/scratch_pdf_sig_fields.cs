using System;
using System.IO;
using System.Linq;
using System.Reflection;

class Program
{
    static void Main()
    {
        var asm = Assembly.LoadFrom(@"C:\REPO\PLM V1\PLM-EHC\packages\iTextSharp.5.5.12\lib\itextsharp.dll");
        var readerType = asm.GetType("iTextSharp.text.pdf.PdfReader");
        var reader = Activator.CreateInstance(readerType, new object[] { @"C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\Revised Lease Agreement_v2.pdf" });
        var acroFieldsProp = readerType.GetProperty("AcroFields");
        var acroFields = acroFieldsProp.GetValue(reader, null);
        var fieldsField = acroFields.GetType().GetField("fields", BindingFlags.NonPublic | BindingFlags.Instance);
        var fieldsProp = acroFields.GetType().GetProperty("Fields");
        
        System.Collections.IDictionary fields = null;
        if (fieldsProp != null) {
            fields = fieldsProp.GetValue(acroFields, null) as System.Collections.IDictionary;
        } else if (fieldsField != null) {
            fields = fieldsField.GetValue(acroFields) as System.Collections.IDictionary;
        }

        if (fields != null) {
            foreach (var key in fields.Keys)
            {
                var keyStr = key.ToString().ToLower();
                if (keyStr.Contains("sign") || keyStr.Contains("manager") || keyStr.Contains("ceo") || keyStr.Contains("rm") || keyStr.Contains("pm") || keyStr.Contains("tenant"))
                {
                    Console.WriteLine("FIELD: " + key);
                }
            }
        } else {
            Console.WriteLine("Could not find Fields property or field.");
        }
    }
}
