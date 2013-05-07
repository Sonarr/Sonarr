using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Api.Exceptions;
using NzbDrone.Api.Helpers;
using NzbDrone.Core.Providers.Core;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace NzbDrone.Api.Filters
{
    public sealed class ValidApiRequestAttribute : Attribute, IHasRequestFilter
    {
        public ApplyTo ApplyTo { get; set; }
        public int Priority { get; set; }

        public ConfigProvider _configProvider;

        public void RequestFilter(IHttpRequest req, IHttpResponse res, object requestDto)
        {
            //Verify the API Key here
            var apikey = req.GetApiKey();

            //if (String.IsNullOrWhiteSpace(apikey))
            //throw new InvalidApiKeyException();
        }

        public IHasRequestFilter Copy()
        {
            return (IHasRequestFilter)this.MemberwiseClone();
        }
    }
}
