using System;
using Nancy;
using NzbDrone.Api.ErrorManagement;

namespace NzbDrone.Api.REST
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(object content = null)
            : base(HttpStatusCode.BadRequest, content)
        {
        }
    }
}