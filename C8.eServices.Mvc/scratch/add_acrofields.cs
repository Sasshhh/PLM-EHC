using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

public class AddAcroFields
{
    public static void Main(string[] args)
    {
        string baseDir = @"C8.eServices.Mvc\PDFTemplates\";
        
        string cancelDraft = Path.Combine(baseDir, "Cancellation_Lease_Template_Draft.pdf");
        string cancelFinal = Path.Combine(baseDir, "Cancellation_Lease_Template.pdf");
        
        string vacateDraft = Path.Combine(baseDir, "Notice_To_Vacate_Template_Draft.pdf");
        string vacateFinal = Path.Combine(baseDir, "Notice_To_Vacate_Template.pdf");

        AddCancelFields(cancelDraft, cancelFinal);
        AddVacateFields(vacateDraft, vacateFinal);
    }

    private static void AddCancelFields(string src, string dest)
    {
        Console.WriteLine("Adding fields to: " + src);
        PdfReader reader = new PdfReader(src);
        using (FileStream fs = new FileStream(dest, FileMode.Create))
        {
            using (PdfStamper stamper = new PdfStamper(reader, fs))
            {
                // Date: x=98.65, y=624.22
                AddField(stamper, "Date", 125, 623, 120, 18);

                // To: x=89.17, y=603.82
                AddField(stamper, "TenantFullName", 110, 601, 180, 18);

                // ID Number: x=125.87, y=591.34
                AddField(stamper, "IdentityNumber", 150, 589, 150, 18);

                // Complex: let's place it below ID Number, e.g. y=575
                AddField(stamper, "Complex", 145, 577, 180, 18);

                // effect from: let's place it where the line is (approx y=485)
                AddField(stamper, "TerminationDate", 250, 481, 150, 18);

                // amounts to R: approx y=308 or y=287 in text (X=381)
                AddField(stamper, "OutstandingBalance", 395, 283, 100, 18);

                // Revenue Manager Signature (near bottom: Y=181 is Madimetja, let's place RM sig at Y=190, X=72)
                AddField(stamper, "RevenueManagerSignature", 72, 190, 150, 40);

                // RM Name & Info: Y=150, X=72
                AddField(stamper, "RMName", 72, 140, 150, 15);
                AddField(stamper, "RMOfficialNumber", 72, 120, 150, 15);
                AddField(stamper, "RMSignDate", 72, 100, 150, 15);
            }
        }
        reader.Close();
        Console.WriteLine("Saved Cancellation Lease Template: " + dest);
    }

    private static void AddVacateFields(string src, string dest)
    {
        Console.WriteLine("Adding fields to: " + src);
        PdfReader reader = new PdfReader(src);
        using (FileStream fs = new FileStream(dest, FileMode.Create))
        {
            using (PdfStamper stamper = new PdfStamper(reader, fs))
            {
                // Date: x=98.65, y=615.0 approx (in notice)
                AddField(stamper, "Date", 125, 613, 120, 18);

                // To: x=89.17, y=596.86
                AddField(stamper, "TenantFullName", 110, 594, 180, 18);

                // ID Number: x=125.87, y=581.98
                AddField(stamper, "IdentityNumber", 150, 579, 150, 18);

                // Complex: Y=567.07, X=120
                AddField(stamper, "Complex", 145, 565, 180, 18);

                // vacate on or before: Y=445.75, X=72
                AddField(stamper, "VacateDate", 72, 441, 150, 18);

                // CEO Signature (Y=181 is Madimetja Kekana, let's place CEO sig at Y=190, X=72)
                AddField(stamper, "CEOSignature", 72, 190, 150, 40);

                // CEO Name & Info: Y=140
                AddField(stamper, "CEOName", 72, 140, 150, 15);
                AddField(stamper, "CEOOfficialNumber", 72, 120, 150, 15);
                AddField(stamper, "CEOSignDate", 72, 100, 150, 15);
            }
        }
        reader.Close();
        Console.WriteLine("Saved Notice to Vacate Template: " + dest);
    }

    private static void AddField(PdfStamper stamper, string name, float x, float y, float w, float h)
    {
        TextField tf = new TextField(stamper.Writer, new Rectangle(x, y, x + w, y + h), name);
        tf.FontSize = 10;
        tf.TextColor = BaseColor.BLACK;
        PdfFormField field = tf.GetTextField();
        stamper.AddAnnotation(field, 1);
    }
}
