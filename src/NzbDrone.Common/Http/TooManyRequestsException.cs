using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Common.Http
{
    public class TooManyRequestsException : HttpException
    {
        public TimeSpan RetryAfter { get; private set; }

        public TooManyRequestsException(HttpRequest request, HttpResponse response)
            : base(request, response)
        {
            if (response.Headers.ContainsKey("Retry-After"))
            {
                RetryAfter = TimeSpan.FromSeconds(int.Parse(response.Headers["Retry-After"].ToString()));
            }
        }
    }
}
