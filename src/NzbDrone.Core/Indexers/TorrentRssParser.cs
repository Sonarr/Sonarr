using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public class TorrentRssParser : RssParser
    {
        // Parse various seeder/leecher/peers formats in the description element to determine number of seeders.
        public Boolean ParseSeedersInDescription { get; set; }

        public TorrentRssParser()
        {

        }

        protected override ReleaseInfo CreateNewReleaseInfo()
        {
            return new TorrentInfo();
        }

        protected override ReleaseInfo ProcessItem(XElement item, ReleaseInfo releaseInfo)
        {
            var result = base.ProcessItem(item, releaseInfo) as TorrentInfo;

            result.InfoHash = GetInfoHash(item);
            result.MagnetUrl = GetMagnetUrl(item);
            result.Seeds = GetSeeders(item);
            result.Peers = GetPeers(item);

            return result;
        }

        protected virtual String GetInfoHash(XElement item)
        {
            return null;
        }

        protected virtual String GetMagnetUrl(XElement item)
        {
            return null;
        }

        protected virtual Int32? GetSeeders(XElement item)
        {
            if (ParseSeedersInDescription)
            {
                var match = ParseSeedersRegex.Match(item.Element("description").Value);

                if (match.Success)
                {
                    return Int32.Parse(match.Groups["value"].Value);
                }
            }

            return null;
        }

        protected virtual Int32? GetPeers(XElement item)
        {
            if (ParseSeedersInDescription)
            {
                var match = ParsePeersRegex.Match(item.Element("description").Value);

                if (match.Success)
                {
                    return Int32.Parse(match.Groups["value"].Value);
                }
            }

            return null;
        }

        private static readonly Regex ParseSeedersRegex = new Regex(@"(Seeder)s?:\s+(?<value>\d+)|(?<value>\d+)\s+(seeder)s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ParsePeersRegex = new Regex(@"(Leecher|Peer)s?:\s+(?<value>\d+)|(?<value>\d+)\s+(leecher|peer)s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
