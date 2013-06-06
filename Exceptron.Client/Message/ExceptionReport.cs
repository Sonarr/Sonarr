using System.Collections.Generic;

namespace Exceptron.Client.Message
{
    internal class ExceptionReport
    {
        /// <summary>
        /// API key
        /// </summary>
        public string ap { get; set; }

        /// <summary>
        /// Application Version
        /// </summary>
        public string aver { get; set; }

        /// <summary>
        /// Exception Severity
        /// </summary>
        public int sv { get; set; }

        /// <summary>
        /// User or Instance ID
        /// </summary>
        public string uid { get; set; }

        /// <summary>
        /// Type of exception
        /// </summary>
        public string ext { get; set; }

        /// <summary>
        /// Exception message
        /// </summary>
        public string exm { get; set; }

        /// <summary>
        /// List of frames that make up the StackTrace of the exception
        /// </summary>
        public List<Frame> stk { get; set; }

        /// <summary>
        /// Component that experienced this exception
        /// </summary>
        public string cmp { get; set; }

        /// <summary>
        /// Message that was logged along with the exception.
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// User's culture in 
        /// </summary>
        /// <remarks>http://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo.name.aspx</remarks>
        public string cul { get; set; }

        /// <summary>
        /// OS Version 
        /// </summary>
        public string os { get; set; }

        /// <summary>
        /// Name of the Client that generated and is sending this message
        /// </summary>
        public string dn { get; set; }

        /// <summary>
        /// Version of the Client that generated and is sending this message
        /// </summary>
        public string dv { get; set; }

        /// <summary>
        /// Host name of the machine that encountered this exception
        /// </summary>
        public string hn { get; set; }

        /// <summary>
        /// Request url
        /// <remarks>Only used for exception in context of a web request/</remarks>
        public string url { get; set; }

        /// <summary>
        /// Browser useragent
        /// </summary>
        /// <remarks>Only used for exception in context of a web request/</remarks>
        public string ua { get; set; }

        /// <summary>
        /// HTTP response status code
        /// </summary>
        /// <remarks>Only used for exception in context of a web request/</remarks>
        public int sc { get; set; }

        /// <summary>
        /// Indicates the HTTP data transfer method used by the client.
        /// </summary>
        /// <example>GET, POST, PUT, DELETE</example>
        public string hm { get; set; }

        /// <summary>
        /// Framework Version (CLR) of the Host Application
        /// </summary>
        public string fv { get; set; }

        /// <summary>
        /// Framework Type of the Host Application
        /// </summary>
        public string ft { get; set; }
    }
}