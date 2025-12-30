using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.LlmMatching
{
    /// <summary>
    /// Decorator service that adds rate limiting to the LLM matching service.
    /// Prevents excessive API costs by limiting the number of calls per hour.
    /// </summary>
    public class RateLimitedLlmSeriesMatchingService(
        CachedLlmSeriesMatchingService innerService,
        IConfigService configService,
        Logger logger) : ILlmSeriesMatchingService
    {
        private readonly ILlmSeriesMatchingService _innerService = innerService;
        private readonly Queue<DateTime> _callTimestamps = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public virtual bool IsEnabled => _innerService.IsEnabled;

        public virtual async Task<LlmMatchResult> TryMatchSeriesAsync(
            ParsedEpisodeInfo parsedEpisodeInfo,
            IEnumerable<Series> availableSeries)
        {
            if (!await TryAcquireRateLimitSlotAsync())
            {
                logger.Warn(
                    "LLM rate limit exceeded. Skipping LLM matching for '{0}'",
                    parsedEpisodeInfo?.SeriesTitle ?? "unknown");

                return null;
            }

            return await _innerService.TryMatchSeriesAsync(parsedEpisodeInfo, availableSeries);
        }

        public virtual async Task<LlmMatchResult> TryMatchSeriesAsync(
            string releaseTitle,
            IEnumerable<Series> availableSeries)
        {
            if (!await TryAcquireRateLimitSlotAsync())
            {
                logger.Warn("LLM rate limit exceeded. Skipping LLM matching for '{0}'", releaseTitle);
                return null;
            }

            return await _innerService.TryMatchSeriesAsync(releaseTitle, availableSeries);
        }

        private async Task<bool> TryAcquireRateLimitSlotAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                var maxCallsPerHour = configService.LlmMaxCallsPerHour;
                var now = DateTime.UtcNow;
                var windowStart = now.AddHours(-1);

                while (_callTimestamps.Count > 0 && _callTimestamps.Peek() < windowStart)
                {
                    _callTimestamps.Dequeue();
                }

                if (_callTimestamps.Count >= maxCallsPerHour)
                {
                    var oldestCall = _callTimestamps.Peek();
                    var timeUntilSlotAvailable = oldestCall.AddHours(1) - now;

                    logger.Debug(
                        "LLM rate limit reached ({0}/{1} calls in last hour). Next slot available in {2}",
                        _callTimestamps.Count,
                        maxCallsPerHour,
                        timeUntilSlotAvailable);

                    return false;
                }

                _callTimestamps.Enqueue(now);

                logger.Trace(
                    "LLM rate limit: {0}/{1} calls in last hour",
                    _callTimestamps.Count,
                    maxCallsPerHour);

                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
