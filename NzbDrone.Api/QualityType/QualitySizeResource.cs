using System;
using System.Linq;

namespace NzbDrone.Api.QualityType
{
    public class QualitySizeResource
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public Int32 MinSize { get; set; }
        public Int32 MaxSize { get; set; }
    }
}