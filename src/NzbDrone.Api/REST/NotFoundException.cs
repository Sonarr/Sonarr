using Nancy;
using NzbDrone.Api.ErrorManagement;

namespace NzbDrone.Api.REST
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(object content = null)
            : base(HttpStatusCode.NotFound, content)
        {
        }
    }
}