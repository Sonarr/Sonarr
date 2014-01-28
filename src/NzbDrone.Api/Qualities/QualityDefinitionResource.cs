using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Qualities
{
    public class QualityDefinitionResource : RestResource
    {
        public Quality Quality { get; set; }

        public String Title { get; set; }

        public Int32 Weight { get; set; }

        public Int32 MinSize { get; set; }
        public Int32 MaxSize { get; set; }
    }
}