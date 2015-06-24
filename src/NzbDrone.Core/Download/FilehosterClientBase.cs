using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public abstract class FilehosterClientBase<TSettings> : DownloadClientBase<TSettings>, IFilehostLinkChecker
        where TSettings : IProviderConfig, new()
    {
        protected FilehosterClientBase(IConfigService configService, IDiskProvider diskProvider, IRemotePathMappingService remotePathMappingService, Logger logger) : base(configService, diskProvider, remotePathMappingService, logger)
        {
        }

        public override DownloadProtocol Protocol { get{return  DownloadProtocol.Filehoster;} }
        public abstract IList<FilehostLinkCheckInfo> CheckLinks(IList<ReleaseInfo> releases);
    }
}