using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine
{
    public class DownloadDecisionComparer : IComparer<DownloadDecision>
    {
        private readonly IDelayProfileService _delayProfileService;
        public delegate int CompareDelegate(DownloadDecision x, DownloadDecision y);
        public delegate int CompareDelegate<TSubject, TValue>(DownloadDecision x, DownloadDecision y);

        public DownloadDecisionComparer(IDelayProfileService delayProfileService)
        {
            _delayProfileService = delayProfileService;
        }

        public int Compare(DownloadDecision x, DownloadDecision y)
        {
            var comparers = new List<CompareDelegate>
            {
                CompareQuality,
                CompareLanguage,
                CompareProtocol,
                CompareEpisodeCount,
                CompareEpisodeNumber,
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
            return CompareBy(left, right, funcValue)*-1;
        }

        private int CompareAll(params int[] comparers)
        {
            return comparers.Select(comparer => comparer).FirstOrDefault(result => result != 0);
        }

        private int CompareQuality(DownloadDecision x, DownloadDecision y)
        {
            return CompareAll(CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Series.Profile.Value.GetIndex(remoteEpisode.ParsedEpisodeInfo.Quality.Quality)),
                           CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.ParsedEpisodeInfo.Quality.Revision.Real),
                           CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.ParsedEpisodeInfo.Quality.Revision.Version));
        }

        private int CompareLanguage(DownloadDecision x, DownloadDecision y)
        {
            return CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Series.LanguageProfile.Value.Languages.FindIndex(l => l.Language == remoteEpisode.ParsedEpisodeInfo.Language));
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
            var seasonPackCompare = CompareBy(x.RemoteEpisode, y.RemoteEpisode,
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
            // since we're dealing with the same series in our comparisions
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

        private int CompareSize(DownloadDecision x, DownloadDecision y)
        {
            // TODO: Is smaller better? Smaller for usenet could mean no par2 files.

            return CompareBy(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Release.Size.Round(200.Megabytes()));
        }
    }
}
