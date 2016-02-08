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

        private int CompareQuality(DownloadDecision x, DownloadDecision y)
        {
            var qualityX = x.RemoteEpisode.ParsedEpisodeInfo.Quality;
            var qualityY = y.RemoteEpisode.ParsedEpisodeInfo.Quality;
            var indexX = x.RemoteEpisode.Series.Profile.Value.Items.FindIndex(v => v.Quality == qualityX.Quality);
            var indexY = y.RemoteEpisode.Series.Profile.Value.Items.FindIndex(v => v.Quality == qualityY.Quality);

            if (indexX > indexY)
            {
                return 1;
            }

            if (indexX < indexY)
            {
                return -1;
            }

            if (qualityX.Revision.Real > qualityY.Revision.Real)
            {
                return 1;
            }

            if (qualityX.Revision.Real < qualityY.Revision.Real)
            {
                return -1;
            }

            if (qualityX.Revision.Version > qualityY.Revision.Version)
            {
                return 1;
            }

            if (qualityX.Revision.Version < qualityY.Revision.Version)
            {
                return -1;
            }

            return 0;
        }

        private int CompareProtocol(DownloadDecision x, DownloadDecision y)
        {
            var delayProfileX = _delayProfileService.BestForTags(x.RemoteEpisode.Series.Tags);
            var delayProfileY = _delayProfileService.BestForTags(y.RemoteEpisode.Series.Tags);
            var downloadProtocolX = x.RemoteEpisode.Release.DownloadProtocol;
            var downloadProtocolY = y.RemoteEpisode.Release.DownloadProtocol;
            var preferredX = downloadProtocolX == delayProfileX.PreferredProtocol;
            var preferredY = downloadProtocolY == delayProfileY.PreferredProtocol;

            if (preferredX && !preferredY)
            {
                return 1;
            }

            if (!preferredX && preferredY)
            {
                return -1;
            }

            return 0;
        }

        private int CompareEpisodeCount(DownloadDecision x, DownloadDecision y)
        {
            // Prefer season packs, otherwise prefer less episodes

            var fullSeasonX = x.RemoteEpisode.ParsedEpisodeInfo.FullSeason;
            var fullSeasonY = y.RemoteEpisode.ParsedEpisodeInfo.FullSeason;
            var episodesX = x.RemoteEpisode.Episodes.Count;
            var episodesY = y.RemoteEpisode.Episodes.Count;

            if (fullSeasonX && !fullSeasonY)
            {
                return 1;
            }

            if (!fullSeasonX && fullSeasonY)
            {
                return -1;
            }

            if (episodesX < episodesY)
            {
                return 1;
            }

            if (episodesX > episodesY)
            {
                return -1;
            }

            return 0;
        }

        private int CompareEpisodeNumber(DownloadDecision x, DownloadDecision y)
        {
            // Prefer lower episode numbers

            var episodeNumberX = x.RemoteEpisode.Episodes.Select(e => e.EpisodeNumber).MinOrDefault();
            var episodeNumberY = y.RemoteEpisode.Episodes.Select(e => e.EpisodeNumber).MinOrDefault();

            if (episodeNumberX < episodeNumberY)
            {
                return 1;
            }

            if (episodeNumberX > episodeNumberY)
            {
                return -1;
            }

            return 0;
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

            var seedersX = TorrentInfo.GetSeeders(x.RemoteEpisode.Release);
            var seedersY = TorrentInfo.GetSeeders(y.RemoteEpisode.Release);
            var peersX = TorrentInfo.GetPeers(x.RemoteEpisode.Release);
            var peersY = TorrentInfo.GetPeers(y.RemoteEpisode.Release);

            if (seedersX > seedersY)
            {
                return 1;
            }

            if (seedersX < seedersY)
            {
                return -1;
            }

            if (peersX > peersY)
            {
                return 1;
            }

            if (peersX < peersY)
            {
                return -1;
            }

            return 0;
        }

        private int CompareAge(DownloadDecision x, DownloadDecision y)
        {
            if (x.RemoteEpisode.Release.DownloadProtocol != DownloadProtocol.Usenet ||
                y.RemoteEpisode.Release.DownloadProtocol != DownloadProtocol.Usenet)
            {
                return 0;
            }

            var minutesX = x.RemoteEpisode.Release.AgeMinutes;
            var minutesY = y.RemoteEpisode.Release.AgeMinutes;

            if (minutesX < minutesY)
            {
                return 1;
            }

            if (minutesX > minutesY)
            {
                return -1;
            }

            return 0;
        }

        private int CompareSize(DownloadDecision x, DownloadDecision y)
        {
            // TODO: Is smaller better? Smaller for usenet could mean no par2 files.

            var sizeX = x.RemoteEpisode.Release.Size.Round(200.Megabytes());
            var sizeY = y.RemoteEpisode.Release.Size.Round(200.Megabytes());

            if (sizeX < sizeY)
            {
                return 1;
            }

            if (sizeX > sizeY)
            {
                return -1;
            }

            return 0;
        }
    }
}
