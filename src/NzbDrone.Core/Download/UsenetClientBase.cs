using System;
using System.Net;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Configuration;
using NLog;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download
{
    public abstract class UsenetClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected readonly IHttpClient _httpClient;

        protected UsenetClientBase(IHttpClient httpClient,
                                   IConfigService configService,
                                   IDiskProvider diskProvider,
                                   IRemotePathMappingService remotePathMappingService,
                                   Logger logger)
            : base(configService, diskProvider, remotePathMappingService, logger)
        {
            _httpClient = httpClient;
        }
        
        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Usenet;
            }
        }

        protected abstract String AddFromNzbFile(RemoteEpisode remoteEpisode, String filename, Byte[] fileContent);

        public override String Download(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var filename =  FileNameBuilder.CleanFileName(remoteEpisode.Release.Title) + ".nzb";

            Byte[] nzbData;

            try
            {
                nzbData = _httpClient.Get(new HttpRequest(url)).ResponseData;
            }
            catch (HttpException ex)
            {
                _logger.ErrorException(String.Format("Downloading nzb for episode '{0}' failed ({1})",
                    remoteEpisode.Release.Title, url), ex);

                throw new ReleaseDownloadException(remoteEpisode.Release, "Downloading nzb failed", ex);
            }
            catch (WebException ex)
            {
                _logger.ErrorException(String.Format("Downloading nzb for episode '{0}' failed ({1})",
                    remoteEpisode.Release.Title, url), ex);

                throw new ReleaseDownloadException(remoteEpisode.Release, "Downloading nzb failed", ex);
            }

            _logger.Info("Adding report [{0}] to the queue.", remoteEpisode.Release.Title);
            return AddFromNzbFile(remoteEpisode, filename, nzbData);
        }
    }
}
