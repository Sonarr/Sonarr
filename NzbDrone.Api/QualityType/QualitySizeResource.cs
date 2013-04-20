using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.QualityType
{
    public class QualitySizeResource : RestResource<QualitySizeResource>
    {
        public String Name { get; set; }
        public Int32 MinSize { get; set; }
        public Int32 MaxSize { get; set; }
    }
}