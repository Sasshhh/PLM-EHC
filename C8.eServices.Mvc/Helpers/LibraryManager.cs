using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace C8.eServices.Mvc.Helpers
{
    public class LibraryManager
    {
        private NetworkCredential _credential = null;
        private string _site = string.Empty;
        private string _library = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryManager"/> class.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="library">The library.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public LibraryManager( string site, string library, string domain, string username, string password )
        {
            _credential = new NetworkCredential( username, password, domain );
            _site = site;
            _library = library;
        }

        /// <summary>
        /// Creates the folder.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        public bool CreateFolder( string folderName )
        {
            bool isCreated = true;

            try
            {
                using ( ClientContext ctx = new ClientContext( _site ) )
                {
                    ctx.Credentials = _credential;

                    List docs = ctx.Web.Lists.GetByTitle( _library );
                    ctx.Load( docs.RootFolder );
                    ctx.ExecuteQuery();

                    docs.RootFolder.Folders.Add( folderName );
                    ctx.ExecuteQuery();
                }
            }
            catch ( Exception x )
            {
                // Show some concern.
                isCreated = false;
            }

            return isCreated;
        }

        /// <summary>
        /// Adds the document to the folder as filename with the respective data byte array.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public string AddDocument( string folderName, string fileName, byte[] data )
        {
            string url = string.Empty;

            try
            {
                using ( ClientContext ctx = new ClientContext( _site ) )
                {
                    ctx.Credentials = _credential;

                    List docs = ctx.Web.Lists.GetByTitle( _library );
                    ctx.Load( docs.RootFolder );
                    ctx.ExecuteQuery();

                    url = string.Format( "{0}/{1}/{2}", docs.RootFolder.ServerRelativeUrl, folderName, fileName );

                    Microsoft.SharePoint.Client.File.SaveBinaryDirect( ctx, url, new MemoryStream( data ), true );
                }
            }
            catch ( Exception )
            {
                // Show some concern.
                url = null;
            }

            return url;
        }

        /// <summary>
        /// Gets the document from a specific relative url to the SharePoint site.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public byte[] GetDocument( string url )
        {
            byte[] data = null;

            try
            {
                using ( ClientContext ctx = new ClientContext( _site ) )
                {
                    ctx.Credentials = _credential;

                    var fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect( ctx, url );
                    data = ReadToEnd( fileInfo.Stream );
                }
            }
            catch ( Exception )
            {
                // Show some concern.
            }

            return data;
        }

        /// <summary>
        /// Gets the document from a folder with the filename.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public byte[] GetDocument( string folderName, string fileName )
        {
            byte[] data = null;

            try
            {
                using ( ClientContext ctx = new ClientContext( _site ) )
                {
                    ctx.Credentials = _credential;

                    List docs = ctx.Web.Lists.GetByTitle( _library );
                    ctx.Load( docs.RootFolder );
                    ctx.ExecuteQuery();

                    var url = string.Format( "{0}/{1}/{2}", docs.RootFolder.ServerRelativeUrl, folderName, fileName );

                    var fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect( ctx, url );
                    data = ReadToEnd( fileInfo.Stream );
                }
            }
            catch ( Exception )
            {
                // Show some concern.
            }

            return data;
        }

        /// <summary>
        /// Removes the document from a folder with the filename.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public bool RemoveDocument( string folderName, string fileName )
        {
            bool isDeleted = true;

            try
            {
                using ( ClientContext ctx = new ClientContext( _site ) )
                {
                    ctx.Credentials = _credential;

                    List docs = ctx.Web.Lists.GetByTitle( _library );
                    ctx.Load( docs.RootFolder );
                    ctx.ExecuteQuery();

                    var url = string.Format( "{0}/{1}/{2}", docs.RootFolder.ServerRelativeUrl, folderName, fileName );

                    var web = ctx.Web;
                    Microsoft.SharePoint.Client.File f = web.GetFileByServerRelativeUrl( url );
                    ctx.Load( f );
                    f.DeleteObject();
                    ctx.ExecuteQuery();
                }
            }
            catch ( Exception )
            {
                // Show some concern.
                isDeleted = false;
            }

            return isDeleted;
        }

        /// <summary>
        /// Removes the document.
        /// </summary>
        /// <param name="url">The URL where the document resides.</param>
        /// <returns></returns>
        public bool RemoveDocument( string url )
        {
            bool isDeleted = true;

            try
            {
                using ( ClientContext ctx = new ClientContext( _site ) )
                {
                    ctx.Credentials = _credential;

                    List docs = ctx.Web.Lists.GetByTitle( _library );
                    ctx.Load( docs.RootFolder );
                    ctx.ExecuteQuery();

                    var web = ctx.Web;
                    Microsoft.SharePoint.Client.File f = web.GetFileByServerRelativeUrl( url );
                    ctx.Load( f );
                    f.DeleteObject();
                    ctx.ExecuteQuery();
                }
            }
            catch ( Exception )
            {
                // Show some concern.
                isDeleted = false;
            }

            return isDeleted;
        }

        /// <summary>
        /// Reads an IO stream from start to end.
        /// </summary>
        /// <param name="stream">The IO stream.</param>
        /// <returns></returns>
        private static byte[] ReadToEnd( Stream stream )
        {
            long originalPosition = 0;

            if ( stream.CanSeek )
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[ 4096 ];

                int totalBytesRead = 0;
                int bytesRead;

                while ( ( bytesRead = stream.Read( readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead ) ) > 0 )
                {
                    totalBytesRead += bytesRead;

                    if ( totalBytesRead == readBuffer.Length )
                    {
                        int nextByte = stream.ReadByte();
                        if ( nextByte != -1 )
                        {
                            byte[] temp = new byte[ readBuffer.Length * 2 ];
                            Buffer.BlockCopy( readBuffer, 0, temp, 0, readBuffer.Length );
                            Buffer.SetByte( temp, totalBytesRead, ( byte ) nextByte );
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if ( readBuffer.Length != totalBytesRead )
                {
                    buffer = new byte[ totalBytesRead ];
                    Buffer.BlockCopy( readBuffer, 0, buffer, 0, totalBytesRead );
                }
                return buffer;
            }
            finally
            {
                if ( stream.CanSeek )
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}
