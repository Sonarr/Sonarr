using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine
{
    public class DownloadDecisionComparer : IComparer<DownloadDecision>
    {
        private static readonly string[] IgnoredStrings = new string[] { "-xpost" };

        private readonly IConfigService _configService;
        private readonly IDelayProfileService _delayProfileService;
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public delegate int CompareDelegate(DownloadDecision x, DownloadDecision y);
        public delegate int CompareDelegate<TSubject, TValue>(DownloadDecision x, DownloadDecision y);

        public DownloadDecisionComparer(IConfigService configService, IDelayProfileService delayProfileService, IQualityDefinitionService qualityDefinitionService)
        {
            _configService = configService;
            _delayProfileService = delayProfileService;
            _qualityDefinitionService = qualityDefinitionService;
        }

        public int Compare(DownloadDecision x, DownloadDecision y)
        {
            var comparers = new List<CompareDelegate>
            {
                CompareQuality,
                CompareCustomFormatScore,
                CompareProtocol,
                CompareEpisodeCount,
                CompareEpisodeNumber,
                CompareIndexerPriority,
                ComparePeersIfTorrent,
                CompareAgeIfUsenet,
                CompareSize
            };

            return comparers.Select(comparer => comparer(x, y)).FirstOrDefault(result => result != 0);
        }

        private int CompareBy<TSubject, TValue>(TSubject left, TSubject right, Func<TSubject, TValue> funcValue)
            where TValue : IComparable<TValue>
        {
            var leftValue = funcValue(left);
            var rightValue = funcValue(right);

            return leftValue.CompareTo(rightValue);
        }

        private int CompareByReverse<TSubject, TValue>(TSubject left, TSubject right, Func<TSubject, TValue> funcValue)
            where TValue : IComparable<TValue>
        {
            return CompareBy(left, right, funcValue) * -1;
        }

        private int CompareAll(params int[] comparers)
        {
            return comparers.Select(comparer => comparer).FirstOrDefault(result => result != 0);
        }

        private int CompareIndexerPriority(DownloadDecision x, DownloadDecision y)
        {
            return CompareByReverse(x.RemoteEpisode.Release, y.RemoteEpisode.Release, release => release.IndexerPriority);
        }

        private int CompareQuality(DownloadDecision x, DownloadDecision y)
        {
            if (_configService.DownloadPropersAndRepacks == ProperDownloadTypes.DoNotPrefer)
            {
                return CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Series.QualityProfile.Value.GetIndex(remoteEpisode.ParsedEpisodeInfo.Quality.Quality));
            }

            return CompareAll(
                CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Series.QualityProfile.Value.GetIndex(remoteEpisode.ParsedEpisodeInfo.Quality.Quality)),
                CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.ParsedEpisodeInfo.Quality.Revision));
        }

        private int CompareCustomFormatScore(DownloadDecision x, DownloadDecision y)
        {
            return CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteMovie => remoteMovie.CustomFormatScore);
        }

        private int CompareProtocol(DownloadDecision x, DownloadDecision y)
        {
            var result = CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode =>
            {
                var delayProfile = _delayProfileService.BestForTags(remoteEpisode.Series.Tags);
                var downloadProtocol = remoteEpisode.Release.DownloadProtocol;
                return downloadProtocol == delayProfile.PreferredProtocol;
            });

            return result;
        }

        private int CompareEpisodeCount(DownloadDecision x, DownloadDecision y)
        {
            var seasonPackCompare = CompareBy(x.RemoteEpisode,
                y.RemoteEpisode,
                remoteEpisode => remoteEpisode.ParsedEpisodeInfo.FullSeason);

            if (seasonPackCompare != 0)
            {
                return seasonPackCompare;
            }

            if (x.RemoteEpisode.Series.SeriesType == SeriesTypes.Anime &
                y.RemoteEpisode.Series.SeriesType == SeriesTypes.Anime)
            {
                return CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Episodes.Count);
            }

            return CompareByReverse(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Episodes.Count);
        }

        private int CompareEpisodeNumber(DownloadDecision x, DownloadDecision y)
        {
            return CompareByReverse(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Episodes.Select(e => e.EpisodeNumber).MinOrDefault());
        }

        private int ComparePeersIfTorrent(DownloadDecision x, DownloadDecision y)
        {
            // Different protocols should get caught when checking the preferred protocol,
            // since we're dealing with the same series in our comparisons
            if (x.RemoteEpisode.Release.DownloadProtocol != DownloadProtocol.Torrent ||
                y.RemoteEpisode.Release.DownloadProtocol != DownloadProtocol.Torrent)
            {
                return 0;
            }

            return CompareAll(
                CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode =>
                {
                    var seeders = TorrentInfo.GetSeeders(remoteEpisode.Release);

                    return seeders.HasValue && seeders.Value > 0 ? Math.Round(Math.Log10(seeders.Value)) : 0;
                }),
                CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode =>
                {
                    var peers = TorrentInfo.GetPeers(remoteEpisode.Release);

                    return peers.HasValue && peers.Value > 0 ? Math.Round(Math.Log10(peers.Value)) : 0;
                }));
        }

        private int CompareAgeIfUsenet(DownloadDecision x, DownloadDecision y)
        {
            if (x.RemoteEpisode.Release.DownloadProtocol != DownloadProtocol.Usenet ||
                y.RemoteEpisode.Release.DownloadProtocol != DownloadProtocol.Usenet)
            {
                return 0;
            }

            var sanitizedTitleX = SanitizeReleaseName(x.RemoteEpisode.Release.Title);
            var sanitizedTitleY = SanitizeReleaseName(y.RemoteEpisode.Release.Title);

            var titlesMatch = string.Equals(sanitizedTitleX, sanitizedTitleY, StringComparison.OrdinalIgnoreCase);
            var sizesMatch = GetRoundedSize(x.RemoteEpisode.Release.Size) == GetRoundedSize(y.RemoteEpisode.Release.Size);

            if (titlesMatch && sizesMatch)
            {
                // Compare by age, as both releases have the same sanitized name and rounded size
                return CompareByReverse(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Release.AgeHours);
            }
            else
            {
                // Use original sorting logic if the releases are not equal
                return CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode =>
                {
                    var ageHours = remoteEpisode.Release.AgeHours;
                    var age = remoteEpisode.Release.Age;

                    if (ageHours < 1)
                    {
                        return 1000;
                    }

                    if (ageHours <= 24)
                    {
                        return 100;
                    }

                    if (age <= 7)
                    {
                        return 10;
                    }

                    return 1;
                });
            }
        }

        private int CompareSize(DownloadDecision x, DownloadDecision y)
        {
            var sizeCompare = CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode =>
            {
                var preferredSize = _qualityDefinitionService.Get(remoteEpisode.ParsedEpisodeInfo.Quality.Quality).PreferredSize;

                // If no value for preferred it means unlimited so fallback to sort largest is best
                if (preferredSize.HasValue && remoteEpisode.Series.Runtime > 0)
                {
                    var preferredMovieSize = remoteEpisode.Series.Runtime * preferredSize.Value.Megabytes();

                    // Calculate closest to the preferred size
                    return Math.Abs((remoteEpisode.Release.Size - preferredMovieSize).Round(200.Megabytes())) * (-1);
                }
                else
                {
                    return remoteEpisode.Release.Size.Round(200.Megabytes());
                }
            });

            return sizeCompare;
        }

        private long GetRoundedSize(long size)
        {
            var roundingRules = new List<(long threshold, long roundTo)>
                {
                    (2.5.Gigabytes(), 300.Megabytes()),
                    (15.Gigabytes(), 800.Megabytes()),
                    (30.Gigabytes(), 1600.Megabytes())
                };

            foreach (var (threshold, roundTo) in roundingRules)
            {
                if (size < threshold)
                {
                    return size.Round(roundTo);
                }
            }

            return size.Round(4500.Megabytes()); // Default rounding for sizes >= 30GB
        }

        private string SanitizeReleaseName(string releaseName)
        {
            // Some indexers add strings like -xpost to the release which can be ignored

            foreach (var ignoredString in IgnoredStrings)
            {
                if (releaseName.EndsWith(ignoredString, StringComparison.OrdinalIgnoreCase))
                {
                    return releaseName[..^ignoredString.Length].Trim();
                }
            }

            return releaseName.Trim();
        }
    }
}
