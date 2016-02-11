using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;

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
                CompareProtocol,
                CompareEpisodeCount,
                CompareEpisodeNumber,
                ComparePeers,
                CompareAge,
                CompareSize
            };

            return comparers.Select(comparer => comparer(x, y)).FirstOrDefault(result => result != 0);
        }

        private int Compare<TSubject, TValue>(TSubject left, TSubject right, Func<TSubject, TValue> funcValue)
            where TValue : IComparable<TValue>
        {
            var leftValue = funcValue(left);
            var rightValue = funcValue(right);

            return leftValue.CompareTo(rightValue);
        }

        private int CompareReverse<TSubject, TValue>(TSubject left, TSubject right, Func<TSubject, TValue> funcValue)
            where TValue : IComparable<TValue>
        {
            return Compare(left, right, funcValue)*-1;
        }

        private int Compare(params int[] comparers)
        {
            return comparers.Select(comparer => comparer).FirstOrDefault(result => result != 0);
        }

        private int CompareQuality(DownloadDecision x, DownloadDecision y)
        {
            return Compare(Compare(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Series.Profile.Value.Items.FindIndex(v => v.Quality == remoteEpisode.ParsedEpisodeInfo.Quality.Quality)),
                           Compare(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.ParsedEpisodeInfo.Quality.Revision.Real),
                           Compare(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.ParsedEpisodeInfo.Quality.Revision.Version));
        }

        private int CompareProtocol(DownloadDecision x, DownloadDecision y)
        {
            var result = Compare(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode =>
            {
                var delayProfile = _delayProfileService.BestForTags(remoteEpisode.Series.Tags);
                var downloadProtocol = remoteEpisode.Release.DownloadProtocol;
                return downloadProtocol == delayProfile.PreferredProtocol;
            });

            return result;
        }

        private int CompareEpisodeCount(DownloadDecision x, DownloadDecision y)
        {
            return Compare(Compare(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.ParsedEpisodeInfo.FullSeason),
                           CompareReverse(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Episodes.Count));
        }

        private int CompareEpisodeNumber(DownloadDecision x, DownloadDecision y)
        {
            return CompareReverse(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Episodes.Select(e => e.EpisodeNumber).MinOrDefault());
        }

        private int ComparePeers(DownloadDecision x, DownloadDecision y)
        {
            // Different protocols should get caught when checking the preferred protocol,
            // since we're dealing with the same series in our comparisions
            if (x.RemoteEpisode.Release.DownloadProtocol != DownloadProtocol.Torrent ||
                y.RemoteEpisode.Release.DownloadProtocol != DownloadProtocol.Torrent)
            {
                return 0;
            }

            return Compare(Compare(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode =>
            {
                var seeders = TorrentInfo.GetSeeders(remoteEpisode.Release);

                return seeders ?? 0;
            }),
                Compare(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode =>
                {
                    var peers = TorrentInfo.GetPeers(remoteEpisode.Release);

                    return peers ?? 0;
                }));
        }

        private int CompareAge(DownloadDecision x, DownloadDecision y)
        {
            if (x.RemoteEpisode.Release.DownloadProtocol != DownloadProtocol.Usenet ||
                y.RemoteEpisode.Release.DownloadProtocol != DownloadProtocol.Usenet)
            {
                return 0;
            }

            return CompareReverse(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Release.AgeMinutes);
        }

        private int CompareSize(DownloadDecision x, DownloadDecision y)
        {
            // TODO: Is smaller better? Smaller for usenet could mean no par2 files.

            return Compare(x.RemoteEpisode, y.RemoteEpisode, remoteEpisode => remoteEpisode.Release.Size.Round(200.Megabytes()));
        }
    }
}
