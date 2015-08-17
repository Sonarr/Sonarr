using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using Omu.ValueInjecter;
using System.Linq;

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

            release.InjectFrom(decision.RemoteEpisode.Release);
            release.InjectFrom(decision.RemoteEpisode.ParsedEpisodeInfo);
            release.InjectFrom(decision);
            release.Rejections = decision.Rejections.Select(r => r.Reason).ToList();
            release.DownloadAllowed = decision.RemoteEpisode.DownloadAllowed;
            release.ReleaseWeight = initialWeight;

            if (decision.RemoteEpisode.Series != null)
            {
                release.QualityWeight = decision.RemoteEpisode
                                                        .Series
                                                        .Profile
                                                        .Value
                                                        .Items
                                                        .FindIndex(v => v.Quality == release.Quality.Quality) * 100;
            }

            release.QualityWeight += release.Quality.Revision.Real * 10;
            release.QualityWeight += release.Quality.Revision.Version;

            var torrentRelease = decision.RemoteEpisode.Release as TorrentInfo;

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
