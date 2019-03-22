using Nancy;
using Sonarr.Http.Exceptions;

namespace Sonarr.Http.REST
{
    public class MethodNotAllowedException : ApiException
    {
        public MethodNotAllowedException(object content = null)
            : base(HttpStatusCode.MethodNotAllowed, content)
        {
        }
    }
}
