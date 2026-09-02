using System;
using System.IO;

namespace TempSpace {
    class Program {
        static void Main() {
            var lines = File.ReadAllLines(@"c:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Controllers\ServiceRequestsController.cs");
            for(int i=0; i<lines.Length; i++) {
                if (lines[i].Contains("SaveDocuments(model.Id, uploadDocuments);")) {
                    lines[i] = "System.IO.File.WriteAllText(@\"c:\\REPO\\PLM V1\\PLM-EHC\\debug_log.txt\", \"Files received: \" + uploadDocuments.Length);" + lines[i];
                    break;
                }
            }
            File.WriteAllLines(@"c:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Controllers\ServiceRequestsController.cs", lines);
        }
    }
}
