using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Api.Frontend
{
    public interface IMapHttpRequestsToDisk
    {
        string Map(string resourceUrl);
        RequestType IHandle { get; }
    }

    public enum RequestType
    {
        StaticResources,
        MediaCovers
    }
}
