using System;
using System.Web;

namespace Exceptron.Client
{
    /// <summary>
    /// Represents information that will be used to construct an exception report.
    /// </summary>
    public class ExceptionData
    {
        /// <summary>
        /// Exception that is being reported
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Component that experianced this exception.
        /// </summary>
        /// <remarks>
        /// It is common to use the logger name that was used to log the exception as the component.
        /// </remarks>
        /// <example>
        /// DataAccess, Configuration, Registration, etc.
        /// </example>
        public string Component { get; set; }

        /// <summary>
        /// ID that will uniquely identify the user
        /// </summary>
        /// <remarks>
        /// This Id does not have to be tied to the user's identity. 
        /// You can use a system generated unique ID such as GUID. 
        /// This field is used to report how many unique users are experiencing an error.
        /// </remarks>
        /// <example>
        /// "62E5C8EF-0CA2-43AB-B278-FC6994F776ED"
        /// "Timmy@aol.com"
        /// "26437"
        /// </example>
        public string UserId { get; set; }

        /// <summary>
        /// Any message that should be attached to this exceptions
        /// </summary>
        /// <example>
        /// Something went wrong while checking for application updates.
        /// </example>
        public string Message { get; set; }

        /// <summary>
        /// Severity of the exception being reported
        /// </summary>
        public ExceptionSeverity Severity { get; set; }


        /// <summary>
        /// <see cref="System.Web.HttpContext"/> that triggered this exception. If no <see cref="System.Web.HttpContext"/> is provided
        /// <see cref="ExceptronClient"/> will try to get the current <see cref="System.Web.HttpContext"/> from <see cref="System.Web.HttpContext.Current"/>
        /// </summary>
        public HttpContext HttpContext { get; set; }
    }
}