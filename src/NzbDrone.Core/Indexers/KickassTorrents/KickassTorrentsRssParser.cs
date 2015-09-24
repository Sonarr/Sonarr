using System;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.KickassTorrents
{
    public class KickassTorrentsRssParser : EzrssTorrentRssParser
    {
        public KickassTorrentsSettings Settings { get; set; }

        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            if (indexerResponse.HttpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            return base.PreProcess(indexerResponse);
        }

        protected override ReleaseInfo PostProcess(XElement item, ReleaseInfo releaseInfo)
        {
            var verified = item.FindDecendants("verified").SingleOrDefault();

            if (Settings != null && Settings.VerifiedOnly && (string)verified == "0")
            {
                return null;
            }

            // Atm, Kickass supplies 0 as seeders and leechers on the rss feed for recent releases, so set it to null if there aren't any peers.
            // But only for releases younger than 12h (the real number seems to be close to 14h, but it depends on a number of factors).
            var torrentInfo = releaseInfo as TorrentInfo;
            if (torrentInfo.Peers.HasValue && torrentInfo.Peers.Value == 0 && torrentInfo.PublishDate > DateTime.UtcNow.AddHours(-12))
            {
                torrentInfo.Seeders = null;
                torrentInfo.Peers = null;
            }

            return base.PostProcess(item, releaseInfo);
        }
    }
}