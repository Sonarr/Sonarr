//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System.IO;
using System.Text;
using System.Web;

#endregion

namespace CassiniDev
{
    /// <summary>
    /// TODO: get this into resources
    /// </summary>
    internal static class Messages
    {
        private const string _dirListingDirFormat =
            @"{0,38:dddd, MMMM dd, yyyy hh:mm tt}        &lt;dir&gt; <A href=""{1}/"">{2}</A>
";

        private const string _dirListingFileFormat =
            @"{0,38:dddd, MMMM dd, yyyy hh:mm tt} {1,12:n0} <A href=""{2}"">{3}</A>
";

        private const string _dirListingFormat1 =
            @"<html>
    <head>
    <title>Directory Listing -- {0}</title>
";

        private const string _dirListingFormat2 =
            @"    </head>
    <body bgcolor=""white"">

    <h2> <i>Directory Listing -- {0}</i> </h2></span>

            <hr width=100% size=1 color=silver>

<PRE>
";

        private const string _dirListingParentFormat =
            @"<A href=""{0}"">[To Parent Directory]</A>

";

        private const string _httpErrorFormat1 =
            @"<html>
    <head>
        <title>{0}</title>
";

        private const string _httpStyle =
            @"        <style>
        	body {font-family:""Verdana"";font-weight:normal;font-size: 8pt;color:black;} 
        	p {font-family:""Verdana"";font-weight:normal;color:black;margin-top: -5px}
        	b {font-family:""Verdana"";font-weight:bold;color:black;margin-top: -5px}
        	h1 { font-family:""Verdana"";font-weight:normal;font-size:18pt;color:red }
        	h2 { font-family:""Verdana"";font-weight:normal;font-size:14pt;color:maroon }
        	pre {font-family:""Lucida Console"";font-size: 8pt}
        	.marker {font-weight: bold; color: black;text-decoration: none;}
        	.version {color: gray;}
        	.error {margin-bottom: 10px;}
        	.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }
        </style>
";

        private static readonly string _dirListingTail =
            @"</PRE>
            <hr width=100% size=1 color=silver>

            <b>Version Information:</b>&nbsp;Cassini Web Server " +
            VersionString + @"

            </font>

    </body>
</html>
";

        private static readonly string _httpErrorFormat2 =
            @"    </head>
    <body bgcolor=""white"">

            <span><h1>Server Error in '{0}' Application.<hr width=100% size=1 color=silver></h1>

            <h2> <i>HTTP Error {1} - {2}.</i> </h2></span>

            <hr width=100% size=1 color=silver>

            <b>Version Information:</b>&nbsp;Cassini Web Server " +
            VersionString + @"

            </font>

    </body>
</html>
";

        public static string VersionString = typeof (Server).Assembly.GetName().Version.ToString();

        public static string FormatDirectoryListing(string dirPath, string parentPath, FileSystemInfo[] elements)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format(_dirListingFormat1, dirPath));
            sb.Append(_httpStyle);
            sb.Append(string.Format(_dirListingFormat2, dirPath));

            if (parentPath != null)
            {
                if (!parentPath.EndsWith("/"))
                {
                    parentPath += "/";
                }

                sb.Append(string.Format(_dirListingParentFormat, parentPath));
            }

            if (elements != null)
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i] is FileInfo)
                    {
                        FileInfo fi = (FileInfo) elements[i];
                        sb.Append(string.Format(_dirListingFileFormat,
                                                fi.LastWriteTime, fi.Length, fi.Name, fi.Name));
                    }
                    else if (elements[i] is DirectoryInfo)
                    {
                        DirectoryInfo di = (DirectoryInfo) elements[i];
                        sb.Append(string.Format(_dirListingDirFormat,
                                                di.LastWriteTime, di.Name, di.Name));
                    }
                }
            }

            sb.Append(_dirListingTail);
            return sb.ToString();
        }

        public static string FormatErrorMessageBody(int statusCode, string appName)
        {
            string desc = HttpWorkerRequest.GetStatusDescription(statusCode);

            return string.Format(_httpErrorFormat1, desc)
                   + _httpStyle
                   + string.Format(_httpErrorFormat2, appName, statusCode, desc);
        }
    }
}