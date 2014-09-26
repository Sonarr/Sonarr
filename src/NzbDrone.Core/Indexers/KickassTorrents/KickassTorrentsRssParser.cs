using System;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.KickassTorrents
{
    public class KickassTorrentsRssParser : EzrssTorrentRssParser
    {
        public KickassTorrentsRssParser()
        {

        }

        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            if (indexerResponse.HttpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            return base.PreProcess(indexerResponse);
        }
    }
}