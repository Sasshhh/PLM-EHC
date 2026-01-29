using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.IO;

namespace C8.eServices.Mvc.Helpers
{
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
    public class EncryptedActionParameterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting( ActionExecutingContext filterContext )
        {
            Dictionary<string, object> decryptedParameters = new Dictionary<string, object>();

            if ( HttpContext.Current.Request.QueryString.Get( "q" ) != null )
            {
                string encryptedQueryString = HttpContext.Current.Request.QueryString.Get( "q" );
                if (encryptedQueryString.Contains(" "))
                    encryptedQueryString = encryptedQueryString.Replace(" ", "+");
                string decryptedString;

                try
                {
                    decryptedString = Decrypt( encryptedQueryString );
                    if ( decryptedString[ 0 ] == 'q' && decryptedString[ 1 ] == '=' )
                        decryptedString =
                            Decrypt( HttpUtility.UrlDecode( decryptedString.Substring( 2, decryptedString.Length - 2 ) ) );
                }
                catch ( Exception x )
                {
                    throw new Exception( "Malicious Activity", x );
                }

                string[] delimiter = { "??" };
                string[] paramsArrs = decryptedString.Split( delimiter, StringSplitOptions.None );
                char[] paramDelimiter = { '=' };
                int outParam = 0;

                for ( int i = 0; i < paramsArrs.Length; i++ )
                {
                    string[] paramArr = paramsArrs[ i ].Split( paramDelimiter, 2 );

                    if ( Int32.TryParse( paramArr[ 1 ], out outParam ) )
                        decryptedParameters.Add( paramArr[ 0 ], outParam );
                    else
                        decryptedParameters.Add( paramArr[ 0 ], paramArr[ 1 ] );
                }
            } 
            else 
                throw new Exception( "Malicious Activity" );

            for ( int i = 0; i < decryptedParameters.Count; i++ )
            {
                filterContext.ActionParameters[ decryptedParameters.Keys.ElementAt( i ) ] = decryptedParameters.Values.ElementAt( i );
            }

            base.OnActionExecuting( filterContext );
        }

        private string Decrypt( string encryptedText )
        {
            string key = "th3w1l0fGurud3v79$";
            byte[] DecryptKey = { };
            byte[] IV = { 55, 34, 87, 64, 87, 195, 54, 21 };
            byte[] inputByte = new byte[ encryptedText.Length ];

            DecryptKey = System.Text.Encoding.UTF8.GetBytes( key.Substring( 0, 8 ) );
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            //var urlDecoded = HttpUtility.UrlDecode( encryptedText );
            inputByte = Convert.FromBase64String( encryptedText );
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream( ms, des.CreateDecryptor( DecryptKey, IV ), CryptoStreamMode.Write );
            cs.Write( inputByte, 0, inputByte.Length );
            cs.FlushFinalBlock();
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;

            return encoding.GetString( ms.ToArray() );
        }
    }
}