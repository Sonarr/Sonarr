using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStationUsenetClient : UsenetClientBase<DownloadStationSettings>
    {
        private readonly IDownloadStationProxy _proxy;

        public DownloadStationUsenetClient(IDownloadStationProxy proxy,
                                           IHttpClient httpClient,
                                           IConfigService configService,
                                           IDiskProvider diskProvider,
                                           IRemotePathMappingService remotePathMappingService,
                                           Logger logger)
            : base(httpClient, configService, diskProvider, remotePathMappingService, logger)
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

        protected override string AddFromNzbFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent)
        {
            return _proxy.AddFromFile(remoteEpisode, filename, fileContent, Settings);
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            _proxy.Test(failures, Settings);
        }
    }
}
