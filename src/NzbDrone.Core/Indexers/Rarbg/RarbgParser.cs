using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Rarbg
{
    public class RarbgParser : IParseIndexerResponse
    {
        private static readonly Regex RegexGuid = new Regex(@"^magnet:\?xt=urn:btih:([a-f0-9]+)", RegexOptions.Compiled);

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var results = new List<ReleaseInfo>();

            switch (indexerResponse.HttpResponse.StatusCode)
            {
                default:
                    if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new IndexerException(indexerResponse, "Indexer API call returned an unexpected StatusCode [{0}]", indexerResponse.HttpResponse.StatusCode);
                    }
                    break;
            }

            var jsonResponse = new HttpResponse<object>(indexerResponse.HttpResponse).Resource as JContainer;

            var errorResponse = jsonResponse as JObject;
            if (errorResponse != null && errorResponse["error"] != null)
            {
                var error = errorResponse["error"].ToString();
                if (error == "No results found")
                {
                    return results;
                }

                throw new IndexerException(indexerResponse, "Indexer API call returned an error [{0}]", errorResponse["error"]);
            }

            var torrentResponse = jsonResponse.ToObject<List<RarbgTorrent>>();

            foreach (var torrent in torrentResponse)
            {
                var torrentInfo = new TorrentInfo();

                torrentInfo.Guid = GetGuid(torrent);
                torrentInfo.Title = torrent.Title;
                torrentInfo.Size = torrent.Size;
                torrentInfo.DownloadUrl = torrent.DownloadUrl;
                torrentInfo.PublishDate = torrent.PublishDate;
                torrentInfo.Seeders = torrent.Seeders;
                torrentInfo.Peers = torrent.Leechers + torrent.Seeders;

                results.Add(torrentInfo);
            }

            return results;
        }

        private string GetGuid(RarbgTorrent torrent)
        {
            var match = RegexGuid.Match(torrent.DownloadUrl);

            if (match.Success)
            {
                return string.Format("rarbg-{0}", match.Groups[1].Value);
            }
            else
            {
                return string.Format("rarbg-{0}", torrent.DownloadUrl);
            }
        }

    }
}
