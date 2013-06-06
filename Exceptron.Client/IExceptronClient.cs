using System;
using System.Web;
using Exceptron.Client.Configuration;
using Exceptron.Client.Message;

namespace Exceptron.Client
{
    public interface IExceptronClient
    {
        /// <summary>
        /// Client Configuration
        /// </summary>
        ExceptronConfiguration Configuration { get; }

        /// <summary>
        /// Submit an exception to exceptron Servers.
        /// </summary>
        /// <param name="exceptionData">Exception data to be reported to the server</param>
        ExceptionResponse SubmitException(ExceptionData exceptionData);

        /// <summary>
        /// Submit an exception to exceptron Servers.
        /// </summary>
        /// <param name="exception">Exception that is being reported</param>
        /// <param name="component" 
        /// example="DataAccess, Configuration, Registration, etc." 
        /// remarks="It is common to use the logger name that was used to log the exception as the component.">Component that experienced this exception.</param>
        /// <param name="severity">Severity of the exception being reported</param>
        /// <param name="message" 
        /// example="Something went wrong while checking for application updates.">Any message that should be attached to this exceptions</param>
        /// <param name="userId"
        /// remarks="This Id does not have to be tied to the user's identity. 
        /// You can use a system generated unique ID such as GUID. 
        /// This field is used to report how many unique users are experiencing an error." 
        /// example="
        /// 62E5C8EF-0CA2-43AB-B278-FC6994F776ED
        /// Timmy@aol.com
        /// 26437
        /// ">ID that will uniquely identify the user</param>
        /// <param name="httpContext"><see cref="System.Web.HttpContext"/> in which the exception occurred. If no <see cref="System.Web.HttpContext"/> is provided
        /// <see cref="ExceptronClient"/> will try to get the current <see cref="System.Web.HttpContext"/> from <see cref="System.Web.HttpContext.Current"/></param>
        /// <returns></returns>
        ExceptionResponse SubmitException(Exception exception, string component, ExceptionSeverity severity = ExceptionSeverity.None, string message = null, string userId = null, HttpContext httpContext = null);
    }
}