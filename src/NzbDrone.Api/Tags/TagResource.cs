using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Tags
{
    public class TagResource : RestResource
    {
        public string Label { get; set; }
    }
}
