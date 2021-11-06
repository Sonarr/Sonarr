using System.Net;
using Sonarr.Http.Exceptions;

namespace Sonarr.Http.REST
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(object content = null)
            : base(HttpStatusCode.BadRequest, content)
        {
        }
    }
}
