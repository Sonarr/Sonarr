using NzbDrone.Core.Instrumentation;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Logs
{
    public class LogResource : RestResource
    {
        public DateTime Time { get; set; }
        public string? Exception { get; set; }
        public string? ExceptionType { get; set; }
        public required string Level { get; set; }
        public required string Logger { get; set; }
        public required string Message { get; set; }
    }

    public static class LogResourceMapper
    {
        public static LogResource ToResource(this Log model)
        {
            return new LogResource
            {
                Id = model.Id,
                Time = model.Time,
                Exception = model.Exception,
                ExceptionType = model.ExceptionType,
                Level = model.Level.ToLowerInvariant(),
                Logger = model.Logger,
                Message = model.Message
            };
        }
    }
}
