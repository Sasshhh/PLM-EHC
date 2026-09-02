using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        try
        {
            var assembly = Assembly.LoadFrom(@"C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\bin\C8.eServices.Mvc.dll");
            var aesType = assembly.GetType("C8.eServices.Mvc.Helpers.AesCrypto");
            var aesInstance = Activator.CreateInstance(aesType);
            var decryptMethod = aesType.GetMethod("Decrypt", new Type[] { typeof(string) });
            
            string q = "47jvoAIOb8dP9FfLQxMRWcATpD9GTwzYjza3LS2pHvNZRJOLhFtBmyR114XF9vvUeQvsq0UyFpvmb+JAMDlKYQ==";
            q = Uri.UnescapeDataString(q); // Since it has %2B for + and %3D for =
            var result = decryptMethod.Invoke(aesInstance, new object[] { q });
            Console.WriteLine("DECRYPTED: " + result);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
