using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.ProgressMessaging
{
    public class ProgressMessageResource : RestResource
    {
        public DateTime Time { get; set; }
        public String CommandId { get; set; }
        public String Message { get; set; }
    }
}