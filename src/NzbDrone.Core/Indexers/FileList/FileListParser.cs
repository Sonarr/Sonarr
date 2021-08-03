using System;
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
            var torrentInfos = new List<ReleaseInfo>();

            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse,
                    "Unexpected response status {0} code from API request",
                    indexerResponse.HttpResponse.StatusCode);
            }

            var queryResults = JsonConvert.DeserializeObject<List<FileListTorrent>>(indexerResponse.Content);

            foreach (var result in queryResults)
            {
                var id = result.Id;

                //if (result.FreeLeech)

                torrentInfos.Add(new TorrentInfo()
                {
                    Guid = $"FileList-{id}",
                    Title = result.Name,
                    Size = result.Size,
                    DownloadUrl = GetDownloadUrl(id),
                    InfoUrl = GetInfoUrl(id),
                    Seeders = result.Seeders,
                    Peers = result.Leechers + result.Seeders,
                    PublishDate = result.UploadDate.ToUniversalTime(),
                    ImdbId = result.ImdbId
                });
            }

            return torrentInfos.ToArray();
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
