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
            throw new NotImplementedException();
        }

        public override DownloadClientStatus GetStatus()
        {
            throw new NotImplementedException();
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            throw new NotImplementedException();
        }

        protected override string AddFromNzbFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent)
        {
            throw new NotImplementedException();
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            throw new NotImplementedException();
        }
    }
}
