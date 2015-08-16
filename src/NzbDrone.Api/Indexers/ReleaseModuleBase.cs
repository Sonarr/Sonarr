using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Indexers
{
    public abstract class ReleaseModuleBase : NzbDroneRestModule<ReleaseResource>
    {
        protected virtual List<ReleaseResource> MapDecisions(IEnumerable<DownloadDecision> decisions)
        {
            var result = new List<ReleaseResource>();

            foreach (var downloadDecision in decisions)
            {
                var release = MapDecision(downloadDecision, result.Count);

                result.Add(release);
            }

            return result;
        }

        protected virtual ReleaseResource MapDecision(DownloadDecision decision, int initialWeight)
        {
            var release = new ReleaseResource();

            release.InjectFrom(decision.RemoteItem.Release);
            release.InjectFrom(decision.RemoteItem.ParsedInfo);
            release.InjectFrom(decision);
            release.Title = decision.RemoteItem.Release.Title;

            if (decision.RemoteItem is RemoteEpisode)
            {
                release.SeriesTitle = decision.RemoteItem.ParsedInfo.Title;
            }
            else if (decision.RemoteItem is RemoteMovie)
            {
                release.MovieTitle = decision.RemoteItem.ParsedInfo.Title;
            }

            release.Rejections = decision.Rejections.Select(r => r.Reason).ToList();
            release.DownloadAllowed = decision.RemoteItem.DownloadAllowed;
            release.ReleaseWeight = initialWeight;

            if (decision.RemoteItem.Media != null)
            {
                release.QualityWeight = decision.RemoteItem
                                                        .Media
                                                        .Profile
                                                        .Value
                                                        .Items
                                                        .FindIndex(v => v.Quality == release.Quality.Quality) * 100;
            }

            release.QualityWeight += release.Quality.Revision.Real * 10;
            release.QualityWeight += release.Quality.Revision.Version;

            var torrentRelease = decision.RemoteItem.Release as TorrentInfo;

            if (torrentRelease != null)
            {
                release.Protocol = DownloadProtocol.Torrent;
                release.Seeders = torrentRelease.Seeders;
                //TODO: move this up the chains
                release.Leechers = torrentRelease.Peers - torrentRelease.Seeders;
            }
            else
            {
                release.Protocol = DownloadProtocol.Usenet;
            }

            return release;
        }
    }
}
