using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Queue
{
    public class QueueStatusResource : RestResource
    {
        public int Count { get; set; }
        public bool Errors { get; set; }
        public bool Warnings { get; set; }
    }
}
