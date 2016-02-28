using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStationTorrentClient : TorrentClientBase<DownloadStationSettings>
    {
        private IDownloadStationProxy _proxy;

        public DownloadStationTorrentClient(IDownloadStationProxy proxy,
                                            ITorrentFileInfoReader torrentFileInfoReader,
                                            IHttpClient httpClient,
                                            IConfigService configService,
                                            IDiskProvider diskProvider,
                                            IRemotePathMappingService remotePathMappingService,
                                            Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, logger)
        {
            _proxy = proxy;
        }

        public override string Name
        {
            get
            {
                return "Download Station";
            }
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            return _proxy.GetItems(Settings);
        }

        public override DownloadClientStatus GetStatus()
        {
            return _proxy.GetStatus(Settings);
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            _proxy.RemoveItem(downloadId, deleteData, Settings);
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            return _proxy.AddFromUrl(magnetLink, Settings);
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            return _proxy.AddFromFile(filename, fileContent, Settings);
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            _proxy.Test(failures, Settings);
        }
    }
}
