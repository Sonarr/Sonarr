using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.FileList
{
    public class FileListParser : IParseIndexerResponse
    {
        private readonly FileListSettings _settings;

        public FileListParser(FileListSettings settings)
        {
            _settings = settings;
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse,
                    "Unexpected response status {0} code from API request",
                    indexerResponse.HttpResponse.StatusCode);
            }

            if (!indexerResponse.HttpResponse.Headers.ContentType.Contains(HttpAccept.Json.Value))
            {
                throw new IndexerException(indexerResponse, "Unexpected response header '{0}' from indexer request, expected '{1}'", indexerResponse.HttpResponse.Headers.ContentType, HttpAccept.Json.Value);
            }

            var torrentInfos = new List<ReleaseInfo>();

            var queryResults = JsonConvert.DeserializeObject<List<FileListTorrent>>(indexerResponse.Content);

            foreach (var result in queryResults)
            {
                var id = result.Id;

                var torrentInfo = new TorrentInfo
                {
                    Guid = $"FileList-{id}",
                    Title = result.Name,
                    Size = result.Size,
                    DownloadUrl = GetDownloadUrl(id),
                    InfoUrl = GetInfoUrl(id),
                    Seeders = result.Seeders,
                    Peers = result.Leechers + result.Seeders,
                    PublishDate = result.UploadDate.ToUniversalTime(),
                    IndexerFlags = GetIndexerFlags(result)
                };

                if (result.ImdbId is { Length: > 2 } && int.TryParse(result.ImdbId.TrimStart('t'), out var imdbId) && imdbId > 0)
                {
                    torrentInfo.ImdbId = $"tt{imdbId:D7}";
                }

                torrentInfos.Add(torrentInfo);
            }

            return torrentInfos.ToArray();
        }

        private static IndexerFlags GetIndexerFlags(FileListTorrent item)
        {
            IndexerFlags flags = 0;

            if (item.FreeLeech)
            {
                flags |= IndexerFlags.Freeleech;
            }

            if (item.Internal)
            {
                flags |= IndexerFlags.Internal;
            }

            return flags;
        }

        private string GetDownloadUrl(string torrentId)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("download.php")
                .AddQueryParam("id", torrentId)
                .AddQueryParam("passkey", _settings.Passkey);

            return url.FullUri;
        }

        private string GetInfoUrl(string torrentId)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("details.php")
                .AddQueryParam("id", torrentId);

            return url.FullUri;
        }
    }
}
