using System.Linq;
using Nancy;
using Nancy.ErrorHandling;
using NzbDrone.Api.Extensions;

namespace NzbDrone.Api.ErrorManagement
{
    public class ErrorHandler : IStatusCodeHandler
    {
        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return true;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            if (statusCode == HttpStatusCode.SeeOther || statusCode == HttpStatusCode.OK)
                return;

            if (statusCode == HttpStatusCode.Continue)
            {
                context.Response = new Response { StatusCode = statusCode };
                return;
            }

            if (context.Response.ContentType == "text/html" || context.Response.ContentType == "text/plain")
                context.Response = new ErrorModel
                    {
                            Message = statusCode.ToString()
                    }.AsResponse(statusCode);
        }
    }
}