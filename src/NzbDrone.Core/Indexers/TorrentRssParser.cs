using System;
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
            result.Seeders = GetSeeders(item);
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
                var matchSeeders = ParseSeedersRegex.Match(item.Element("description").Value);

                if (matchSeeders.Success)
                {
                    return Int32.Parse(matchSeeders.Groups["value"].Value);
                }

                var matchPeers = ParsePeersRegex.Match(item.Element("description").Value);
                var matchLeechers = ParseLeechersRegex.Match(item.Element("description").Value);

                if (matchPeers.Success && matchLeechers.Success)
                {
                    return Int32.Parse(matchPeers.Groups["value"].Value) - Int32.Parse(matchLeechers.Groups["value"].Value);
                }
            }

            return null;
        }

        protected virtual Int32? GetPeers(XElement item)
        {
            if (ParseSeedersInDescription)
            {
                var matchPeers = ParsePeersRegex.Match(item.Element("description").Value);

                if (matchPeers.Success)
                {
                    return Int32.Parse(matchPeers.Groups["value"].Value);
                }

                var matchSeeders = ParseSeedersRegex.Match(item.Element("description").Value);
                var matchLeechers = ParseLeechersRegex.Match(item.Element("description").Value);

                if (matchSeeders.Success && matchLeechers.Success)
                {
                    return Int32.Parse(matchSeeders.Groups["value"].Value) + Int32.Parse(matchLeechers.Groups["value"].Value);
                }
            }

            return null;
        }

        private static readonly Regex ParseSeedersRegex = new Regex(@"(Seeder)s?:\s+(?<value>\d+)|(?<value>\d+)\s+(seeder)s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ParseLeechersRegex = new Regex(@"(Leecher)s?:\s+(?<value>\d+)|(?<value>\d+)\s+(leecher)s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ParsePeersRegex = new Regex(@"(Peer)s?:\s+(?<value>\d+)|(?<value>\d+)\s+(peer)s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
