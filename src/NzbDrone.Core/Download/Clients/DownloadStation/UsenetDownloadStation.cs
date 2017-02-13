using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class UsenetDownloadStation : UsenetClientBase<DownloadStationSettings>
    {
        public UsenetDownloadStation(IHttpClient httpClient,
                                     IConfigService configService,
                                     IDiskProvider diskProvider,
                                     IRemotePathMappingService remotePathMappingService,
                                     Logger logger)
            : base(httpClient, configService, diskProvider, remotePathMappingService, logger)
        {
        }

        public override string Name
        {
            get
            {
                throw new NotImplementedException();
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
