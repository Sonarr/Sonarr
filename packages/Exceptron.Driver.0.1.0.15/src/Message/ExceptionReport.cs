using System.Collections.Generic;
using System.ComponentModel;

namespace Exceptron.Driver.Message
{
    [EditorBrowsable(EditorBrowsableState.Never)]
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
        /// Environment that this exception occurred in. 
        /// </summary>
        /// <example>Dev, Stage, Production</example>
        public string env { get; set; }

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
        /// Name of the driver that generated and is sending this message
        /// </summary>
        public string dn { get; set; }

        /// <summary>
        /// Version of the driver that generated and is sending this message
        /// </summary>
        public string dv { get; set; }
    }
}