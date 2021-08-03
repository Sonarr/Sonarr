using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public abstract class DirectConnectClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected DirectConnectClientBase(
            IConfigService configService,
            IDiskProvider diskProvider,
            IRemotePathMappingService remotePathMappingService,
            Logger logger)
            : base(configService, diskProvider, remotePathMappingService, logger)
        {
        }

        public override DownloadProtocol Protocol => DownloadProtocol.DirectConnect;

        protected abstract string AddFromId(string id, string title);

        public override string Download(RemoteEpisode remoteEpisode)
        {
            var id = remoteEpisode.Release.DownloadUrl;
            _logger.Info("Adding report [{0}] to the queue.", remoteEpisode.Release.Title);
            return AddFromId(id, remoteEpisode.Release.Title);
        }
    }
}