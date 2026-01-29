using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Routing;
using System.Security.Cryptography;
using System.IO;

namespace C8.eServices.Mvc.Helpers
{
    public static class SecureActionLinkExtension
    {
        public static MvcHtmlString EncodedActionLink( this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes )
        {
            string queryString = string.Empty;
            string htmlAttributesString = string.Empty;
            string baseUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                             HttpContext.Current.Request.ApplicationPath.TrimEnd('/');

            if ( routeValues != null )
            {
                RouteValueDictionary d = new RouteValueDictionary( routeValues );
                for ( int i = 0; i < d.Keys.Count; i++ )
                {
                    if ( i > 0 )
                    {
                        queryString += "??";    
                    }
                    queryString += d.Keys.ElementAt( i ) + "=" + d.Values.ElementAt( i );
                }
            }

            if ( htmlAttributes != null )
            {
                RouteValueDictionary d = new RouteValueDictionary( htmlAttributes );
                for ( int i = 0; i < d.Keys.Count; i++ )
                {
                    htmlAttributesString += " " + d.Keys.ElementAt( i ) + "='" + d.Values.ElementAt( i ) + "'";
                }
            }

            StringBuilder ancor = new StringBuilder();

            ancor.Append( "<a " );

            if ( htmlAttributesString != string.Empty )
            {
                ancor.Append( htmlAttributesString );
            }

            ancor.Append( " href='" );

            if (baseUrl != string.Empty)
            {
                ancor.Append(baseUrl);
            }

            if ( controllerName != string.Empty )
            {
                ancor.Append( "/" + controllerName );
            }

            if ( actionName != "none" )
            {
                ancor.Append( "/" + actionName );
            }

            if ( queryString != string.Empty )
            {
                ancor.Append( "?q=" + HttpUtility.UrlEncode( Encrypt( queryString ) ) );
            }

            ancor.Append( "'" );
            ancor.Append( ">" );
            ancor.Append( linkText );
            ancor.Append( "</a>" );

            return new MvcHtmlString( ancor.ToString() );
        }

        public static string Encrypt( string plainText )
        {
            string key = "th3w1l0fGurud3v79$";
            byte[] EncryptKey = { };
            byte[] IV = { 55, 34, 87, 64, 87, 195, 54, 21 };

            EncryptKey = System.Text.Encoding.UTF8.GetBytes( key.Substring( 0, 8 ) );
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] inputByte = Encoding.UTF8.GetBytes( plainText );
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream( mStream, des.CreateEncryptor( EncryptKey, IV ), CryptoStreamMode.Write );
            cStream.Write( inputByte, 0, inputByte.Length );
            cStream.FlushFinalBlock();
            var base64 = Convert.ToBase64String( mStream.ToArray() );

            return base64;
        }

        public static object Encrypt( object obj )
        {
            RouteValueDictionary d = new RouteValueDictionary( obj );
            var queryString = string.Empty;

            for ( int i = 0; i < d.Keys.Count; i++ )
            {
                if ( i > 0 )
                {
                    queryString += "??";
                }

                queryString += d.Keys.ElementAt( i ) + "=" + d.Values.ElementAt( i );
            }

            return new { q = Encrypt( queryString ) };
        }

        public static string EncryptToString(object obj)
        {
            RouteValueDictionary d = new RouteValueDictionary(obj);
            var queryString = string.Empty;

            for (int i = 0; i < d.Keys.Count; i++)
            {
                if (i > 0)
                {
                    queryString += "??";
                }

                queryString += d.Keys.ElementAt(i) + "=" + d.Values.ElementAt(i);
            }

            return Encrypt(queryString);
        }
    }
}