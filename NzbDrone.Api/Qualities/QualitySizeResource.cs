using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Qualities
{
    public class QualitySizeResource : RestResource
    {
        public String Name { get; set; }
        public Int32 MinSize { get; set; }
        public Int32 MaxSize { get; set; }
    }
}