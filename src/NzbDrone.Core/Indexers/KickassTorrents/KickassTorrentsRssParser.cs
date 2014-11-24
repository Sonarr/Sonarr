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

            return base.PostProcess(item, releaseInfo);
        }
    }
}