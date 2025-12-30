using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.LlmMatching
{
    /// <summary>
    /// Decorator service that adds caching to the LLM matching service.
    /// Helps reduce API costs by caching responses for identical queries.
    /// </summary>
    public class CachedLlmSeriesMatchingService : ILlmSeriesMatchingService
    {
        private readonly ILlmSeriesMatchingService _innerService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;
        private readonly ConcurrentDictionary<string, CachedResult> _cache;

        private DateTime _lastCleanup = DateTime.UtcNow;

        public CachedLlmSeriesMatchingService(
            OpenAiSeriesMatchingService innerService,
            IConfigService configService,
            Logger logger)
        {
            _innerService = innerService;
            _configService = configService;
            _logger = logger;
            _cache = new ConcurrentDictionary<string, CachedResult>(StringComparer.OrdinalIgnoreCase);
        }

        public virtual bool IsEnabled => _innerService.IsEnabled;

        public virtual async Task<LlmMatchResult> TryMatchSeriesAsync(
            ParsedEpisodeInfo parsedEpisodeInfo,
            IEnumerable<Series> availableSeries)
        {
            if (!_configService.LlmCacheEnabled)
            {
                return await _innerService.TryMatchSeriesAsync(parsedEpisodeInfo, availableSeries);
            }

            var cacheKey = GenerateCacheKey(
                parsedEpisodeInfo?.ReleaseTitle ?? parsedEpisodeInfo?.SeriesTitle,
                availableSeries);

            if (TryGetFromCache(cacheKey, availableSeries, out var cachedResult))
            {
                _logger.Trace("LLM cache hit for '{0}'", parsedEpisodeInfo?.SeriesTitle);
                return cachedResult;
            }

            var result = await _innerService.TryMatchSeriesAsync(parsedEpisodeInfo, availableSeries);

            if (result != null)
            {
                AddToCache(cacheKey, result);
            }

            return result;
        }

        public virtual async Task<LlmMatchResult> TryMatchSeriesAsync(
            string releaseTitle,
            IEnumerable<Series> availableSeries)
        {
            if (!_configService.LlmCacheEnabled)
            {
                return await _innerService.TryMatchSeriesAsync(releaseTitle, availableSeries);
            }

            var cacheKey = GenerateCacheKey(releaseTitle, availableSeries);

            if (TryGetFromCache(cacheKey, availableSeries, out var cachedResult))
            {
                _logger.Trace("LLM cache hit for '{0}'", releaseTitle);
                return cachedResult;
            }

            var result = await _innerService.TryMatchSeriesAsync(releaseTitle, availableSeries);

            if (result != null)
            {
                AddToCache(cacheKey, result);
            }

            return result;
        }

        private string GenerateCacheKey(string title, IEnumerable<Series> availableSeries)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            var normalizedTitle = title.CleanSeriesTitle();
            var seriesHash = availableSeries?.Sum(s => s.TvdbId) ?? 0;

            return $"{normalizedTitle}|{seriesHash}";
        }

        private bool TryGetFromCache(
            string cacheKey,
            IEnumerable<Series> availableSeries,
            out LlmMatchResult result)
        {
            result = null;

            if (cacheKey.IsNullOrWhiteSpace())
            {
                return false;
            }

            CleanupExpiredEntriesIfNeeded();

            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                var cacheDuration = TimeSpan.FromHours(_configService.LlmCacheDurationHours);

                if (DateTime.UtcNow - cached.Timestamp < cacheDuration)
                {
                    result = RehydrateResult(cached.Result, availableSeries);
                    return result != null;
                }

                _cache.TryRemove(cacheKey, out _);
            }

            return false;
        }

        private void AddToCache(string cacheKey, LlmMatchResult result)
        {
            if (cacheKey.IsNullOrWhiteSpace() || result == null)
            {
                return;
            }

            var cached = new CachedResult
            {
                Timestamp = DateTime.UtcNow,
                Result = DehydrateResult(result)
            };

            _cache.AddOrUpdate(cacheKey, cached, (_, _) => cached);

            _logger.Trace("Added LLM result to cache for key: {0}", cacheKey);
        }

        private LlmMatchResult DehydrateResult(LlmMatchResult result)
        {
            return new LlmMatchResult
            {
                Series = result.Series != null ? new Series { TvdbId = result.Series.TvdbId } : null,
                Confidence = result.Confidence,
                Reasoning = result.Reasoning,
                Alternatives = result.Alternatives?.Select(a => new AlternativeMatch
                {
                    Series = a.Series != null ? new Series { TvdbId = a.Series.TvdbId } : null,
                    Confidence = a.Confidence,
                    Reasoning = a.Reasoning
                }).ToList() ?? new List<AlternativeMatch>()
            };
        }

        private LlmMatchResult RehydrateResult(LlmMatchResult cached, IEnumerable<Series> availableSeries)
        {
            var seriesLookup = availableSeries?.ToDictionary(s => s.TvdbId) ?? new Dictionary<int, Series>();

            var result = new LlmMatchResult
            {
                Confidence = cached.Confidence,
                Reasoning = cached.Reasoning
            };

            if (cached.Series?.TvdbId > 0 && seriesLookup.TryGetValue(cached.Series.TvdbId, out var series))
            {
                result.Series = series;
            }
            else if (cached.Series != null)
            {
                return null;
            }

            result.Alternatives = cached.Alternatives?
                .Select(a =>
                {
                    if (a.Series?.TvdbId > 0 && seriesLookup.TryGetValue(a.Series.TvdbId, out var altSeries))
                    {
                        return new AlternativeMatch
                        {
                            Series = altSeries,
                            Confidence = a.Confidence,
                            Reasoning = a.Reasoning
                        };
                    }

                    return null;
                })
                .Where(a => a != null)
                .ToList() ?? new List<AlternativeMatch>();

            return result;
        }

        private void CleanupExpiredEntriesIfNeeded()
        {
            if (DateTime.UtcNow - _lastCleanup < TimeSpan.FromMinutes(10))
            {
                return;
            }

            _lastCleanup = DateTime.UtcNow;
            var cacheDuration = TimeSpan.FromHours(_configService.LlmCacheDurationHours);
            var cutoff = DateTime.UtcNow - cacheDuration;

            var expiredKeys = _cache
                .Where(kvp => kvp.Value.Timestamp < cutoff)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }

            if (expiredKeys.Any())
            {
                _logger.Debug("Cleaned up {0} expired LLM cache entries", expiredKeys.Count);
            }
        }

        private class CachedResult
        {
            public DateTime Timestamp { get; set; }

            public LlmMatchResult Result { get; set; }
        }
    }
}
