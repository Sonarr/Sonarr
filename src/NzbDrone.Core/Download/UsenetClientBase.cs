using System.Net;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public abstract class UsenetClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected readonly IHttpClient _httpClient;
        private readonly IValidateNzbs _nzbValidationService;

        protected UsenetClientBase(IHttpClient httpClient,
                                   IConfigService configService,
                                   IDiskProvider diskProvider,
                                   IRemotePathMappingService remotePathMappingService,
                                   IValidateNzbs nzbValidationService,
                                   Logger logger,
                                   ILocalizationService localizationService)
            : base(configService, diskProvider, remotePathMappingService, logger, localizationService)
        {
            _httpClient = httpClient;
            _nzbValidationService = nzbValidationService;
        }

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

        protected abstract string AddFromNzbFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent);

        public override async Task<string> Download(RemoteEpisode remoteEpisode, IIndexer indexer)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var filename =  FileNameBuilder.CleanFileName(remoteEpisode.Release.Title) + ".nzb";

            byte[] nzbData;

            try
            {
                var request = indexer?.GetDownloadRequest(url) ?? new HttpRequest(url);
                request.RateLimitKey = remoteEpisode?.Release?.IndexerId.ToString();

                var response = await RetryStrategy
                    .ExecuteAsync(static async (state, _) => await state._httpClient.GetAsync(state.request), (_httpClient, request))
                    .ConfigureAwait(false);

                nzbData = response.ResponseData;

                _logger.Debug("Downloaded nzb for episode '{0}' finished ({1} bytes from {2})", remoteEpisode.Release.Title, nzbData.Length, url);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.Error(ex, "Downloading nzb file for episode '{0}' failed since it no longer exists ({1})", remoteEpisode.Release.Title, url);
                    throw new ReleaseUnavailableException(remoteEpisode.Release, "Downloading nzb failed", ex);
                }

                if ((int)ex.Response.StatusCode == 429)
                {
                    _logger.Error("API Grab Limit reached for {0}", url);
                }
                else
                {
                    _logger.Error(ex, "Downloading nzb for episode '{0}' failed ({1})", remoteEpisode.Release.Title, url);
                }

                throw new ReleaseDownloadException(remoteEpisode.Release, "Downloading nzb failed", ex);
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Downloading nzb for episode '{0}' failed ({1})", remoteEpisode.Release.Title, url);

                throw new ReleaseDownloadException(remoteEpisode.Release, "Downloading nzb failed", ex);
            }

            _nzbValidationService.Validate(filename, nzbData);

            _logger.Info("Adding report [{0}] to the queue.", remoteEpisode.Release.Title);
            return AddFromNzbFile(remoteEpisode, filename, nzbData);
        }
    }
}
