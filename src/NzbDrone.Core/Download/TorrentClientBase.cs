using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MonoTorrent;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public abstract class TorrentClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected readonly IHttpClient _httpClient;
        private readonly IBlocklistService _blocklistService;
        protected readonly ITorrentFileInfoReader _torrentFileInfoReader;

        protected TorrentClientBase(ITorrentFileInfoReader torrentFileInfoReader,
            IHttpClient httpClient,
            IConfigService configService,
            IDiskProvider diskProvider,
            IRemotePathMappingService remotePathMappingService,
            ILocalizationService localizationService,
            IBlocklistService blocklistService,
            Logger logger)
            : base(configService, diskProvider, remotePathMappingService, logger, localizationService)
        {
            _httpClient = httpClient;
            _blocklistService = blocklistService;
            _torrentFileInfoReader = torrentFileInfoReader;
        }

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;

        public virtual bool PreferTorrentFile => false;

        protected abstract string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink);
        protected abstract string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent);

        public override async Task<string> Download(RemoteEpisode remoteEpisode, IIndexer indexer)
        {
            var torrentInfo = remoteEpisode.Release as TorrentInfo;

            string magnetUrl = null;
            string torrentUrl = null;

            if (remoteEpisode.Release.DownloadUrl.IsNotNullOrWhiteSpace() && remoteEpisode.Release.DownloadUrl.StartsWith("magnet:"))
            {
                magnetUrl = remoteEpisode.Release.DownloadUrl;
            }
            else
            {
                torrentUrl = remoteEpisode.Release.DownloadUrl;
            }

            if (torrentInfo != null && !torrentInfo.MagnetUrl.IsNullOrWhiteSpace())
            {
                magnetUrl = torrentInfo.MagnetUrl;
            }

            if (PreferTorrentFile)
            {
                if (torrentUrl.IsNotNullOrWhiteSpace())
                {
                    try
                    {
                        return await DownloadFromWebUrl(remoteEpisode, indexer, torrentUrl);
                    }
                    catch (ReleaseBlockedException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        if (!magnetUrl.IsNullOrWhiteSpace())
                        {
                            throw;
                        }

                        _logger.Debug("Torrent download failed, trying magnet. ({0})", ex.Message);
                    }
                }

                if (magnetUrl.IsNotNullOrWhiteSpace())
                {
                    try
                    {
                        return DownloadFromMagnetUrl(remoteEpisode, indexer, magnetUrl);
                    }
                    catch (NotSupportedException ex)
                    {
                        throw new ReleaseDownloadException(remoteEpisode.Release, "Magnet not supported by download client. ({0})", ex.Message);
                    }
                }
            }
            else
            {
                if (magnetUrl.IsNotNullOrWhiteSpace())
                {
                    try
                    {
                        return DownloadFromMagnetUrl(remoteEpisode, indexer, magnetUrl);
                    }
                    catch (NotSupportedException ex)
                    {
                        if (torrentUrl.IsNullOrWhiteSpace())
                        {
                            throw new ReleaseDownloadException(remoteEpisode.Release, "Magnet not supported by download client. ({0})", ex.Message);
                        }

                        _logger.Debug("Magnet not supported by download client, trying torrent. ({0})", ex.Message);
                    }
                }

                if (torrentUrl.IsNotNullOrWhiteSpace())
                {
                    return await DownloadFromWebUrl(remoteEpisode, indexer, torrentUrl);
                }
            }

            return null;
        }

        private async Task<string> DownloadFromWebUrl(RemoteEpisode remoteEpisode, IIndexer indexer, string torrentUrl)
        {
            byte[] torrentFile = null;

            try
            {
                var request = indexer?.GetDownloadRequest(torrentUrl) ?? new HttpRequest(torrentUrl);
                request.RateLimitKey = remoteEpisode?.Release?.IndexerId.ToString();
                request.Headers.Accept = "application/x-bittorrent";
                request.AllowAutoRedirect = false;

                var response = await RetryStrategy
                    .ExecuteAsync(static async (state, token) => await state._httpClient.GetAsync(state.request, token), (_httpClient, request))
                    .ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.MovedPermanently ||
                    response.StatusCode == HttpStatusCode.Found ||
                    response.StatusCode == HttpStatusCode.SeeOther)
                {
                    var locationHeader = response.Headers.GetSingleValue("Location");

                    _logger.Trace("Torrent request is being redirected to: {0}", locationHeader);

                    if (locationHeader != null)
                    {
                        if (locationHeader.StartsWith("magnet:"))
                        {
                            return DownloadFromMagnetUrl(remoteEpisode, indexer, locationHeader);
                        }

                        request.Url += new HttpUri(locationHeader);

                        return await DownloadFromWebUrl(remoteEpisode, indexer, request.Url.ToString());
                    }

                    throw new WebException("Remote website tried to redirect without providing a location.");
                }

                torrentFile = response.ResponseData;

                _logger.Debug("Downloading torrent for episode '{0}' finished ({1} bytes from {2})", remoteEpisode.Release.Title, torrentFile.Length, torrentUrl);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                {
                    _logger.Error(ex, "Downloading torrent file for episode '{0}' failed since it no longer exists ({1})", remoteEpisode.Release.Title, torrentUrl);
                    throw new ReleaseUnavailableException(remoteEpisode.Release, "Downloading torrent failed", ex);
                }

                if ((int)ex.Response.StatusCode == 429)
                {
                    _logger.Error("API Grab Limit reached for {0}", torrentUrl);
                }
                else
                {
                    _logger.Error(ex, "Downloading torrent file for episode '{0}' failed ({1})", remoteEpisode.Release.Title, torrentUrl);
                }

                throw new ReleaseDownloadException(remoteEpisode.Release, "Downloading torrent failed", ex);
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Downloading torrent file for episode '{0}' failed ({1})", remoteEpisode.Release.Title, torrentUrl);

                throw new ReleaseDownloadException(remoteEpisode.Release, "Downloading torrent failed", ex);
            }

            var filename = string.Format("{0}.torrent", FileNameBuilder.CleanFileName(remoteEpisode.Release.Title));
            var hash = _torrentFileInfoReader.GetHashFromTorrentFile(torrentFile);

            EnsureReleaseIsNotBlocklisted(remoteEpisode, indexer, hash);
            EnsureTorrentDoesNotContainRejectedFiles(remoteEpisode, indexer, torrentFile);

            var actualHash = AddFromTorrentFile(remoteEpisode, hash, filename, torrentFile);

            if (actualHash.IsNotNullOrWhiteSpace() && hash != actualHash)
            {
                _logger.Debug(
                    "{0} did not return the expected InfoHash for '{1}', Sonarr could potentially lose track of the download in progress.",
                    Definition.Implementation,
                    remoteEpisode.Release.DownloadUrl);
            }

            return actualHash;
        }

        private string DownloadFromMagnetUrl(RemoteEpisode remoteEpisode, IIndexer indexer, string magnetUrl)
        {
            string hash = null;
            string actualHash = null;

            try
            {
                hash = MagnetLink.Parse(magnetUrl).InfoHashes.V1OrV2.ToHex();
            }
            catch (FormatException ex)
            {
                throw new ReleaseDownloadException(remoteEpisode.Release, "Failed to parse magnetlink for episode '{0}': '{1}'", ex, remoteEpisode.Release.Title, magnetUrl);
            }

            if (hash != null)
            {
                EnsureReleaseIsNotBlocklisted(remoteEpisode, indexer, hash);

                actualHash = AddFromMagnetLink(remoteEpisode, hash, magnetUrl);
            }

            if (actualHash.IsNotNullOrWhiteSpace() && hash != actualHash)
            {
                _logger.Debug(
                    "{0} did not return the expected InfoHash for '{1}', Sonarr could potentially lose track of the download in progress.",
                    Definition.Implementation,
                    remoteEpisode.Release.DownloadUrl);
            }

            return actualHash;
        }

        private void EnsureTorrentDoesNotContainRejectedFiles(RemoteEpisode remoteEpisode, IIndexer indexer, byte[] torrentFile)
        {
            var indexerSettings = indexer?.Definition?.Settings as ITorrentIndexerSettings;

            if (indexerSettings?.RejectTorrentFilesWithBlockedExtensionsWhileGrabbing != true)
            {
                return;
            }

            var failDownloads = indexerSettings.FailDownloads?
                .Select(f => (FailDownloads)f)
                .ToHashSet();

            if (failDownloads == null || failDownloads.Count == 0)
            {
                return;
            }

            List<string> fileNames;

            try
            {
                fileNames = _torrentFileInfoReader.GetFileNamesFromTorrentFile(torrentFile);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to parse file list from torrent for '{0}', skipping file extension check", remoteEpisode.Release.Title);
                return;
            }

            ValidateFileNames(remoteEpisode, fileNames, failDownloads);
        }

        private void ValidateFileNames(RemoteEpisode remoteEpisode, List<string> fileNames, HashSet<FailDownloads> failDownloads)
        {
            var userRejectedExtensions = failDownloads.Contains(FailDownloads.UserDefinedExtensions)
                ? FileExtensions.ParseUserRejectedExtensions(_configService.UserRejectedExtensions)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var dangerousExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var executableExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var userRejected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var fileName in fileNames)
            {
                var extension = Path.GetExtension(fileName);

                if (extension.IsNullOrWhiteSpace())
                {
                    continue;
                }

                if (failDownloads.Contains(FailDownloads.PotentiallyDangerous) &&
                    FileExtensions.DangerousExtensions.Contains(extension))
                {
                    dangerousExtensions.Add(extension);
                }
                else if (failDownloads.Contains(FailDownloads.Executables) &&
                    FileExtensions.ExecutableExtensions.Contains(extension))
                {
                    executableExtensions.Add(extension);
                }
                else if (userRejectedExtensions.Contains(extension))
                {
                    userRejected.Add(extension);
                }
            }

            var rejections = new List<string>();

            if (dangerousExtensions.Any())
            {
                rejections.Add($"Found potentially dangerous files with extensions: {string.Join(", ", dangerousExtensions)}");
            }

            if (executableExtensions.Any())
            {
                rejections.Add($"Found executables with extensions: {string.Join(", ", executableExtensions)}");
            }

            if (userRejected.Any())
            {
                rejections.Add($"Found files with user defined rejected extensions: {string.Join(", ", userRejected)}");
            }

            if (rejections.Count == 0)
            {
                return;
            }

            var rejection = $"Caution:{Environment.NewLine}{string.Join(Environment.NewLine, rejections)}";

            _logger.Warn("Torrent for '{0}' rejected: {1}. Blocklisting release.", remoteEpisode.Release.Title, rejection);
            _blocklistService.Block(remoteEpisode, rejection, "TorrentFileValidation");

            throw new ReleaseBlockedException(remoteEpisode.Release, rejection);
        }

        private void EnsureReleaseIsNotBlocklisted(RemoteEpisode remoteEpisode, IIndexer indexer, string hash)
        {
            var indexerSettings = indexer?.Definition?.Settings as ITorrentIndexerSettings;
            var torrentInfo = remoteEpisode.Release as TorrentInfo;
            var torrentInfoHash = torrentInfo?.InfoHash;

            // If the release didn't come from an interactive search,
            // the hash wasn't known during processing and the
            // indexer is configured to reject blocklisted releases
            // during grab check if it's already been blocklisted.

            if (torrentInfo != null && torrentInfoHash.IsNullOrWhiteSpace())
            {
                // If the hash isn't known from parsing we set it here so it can be used for blocklisting.
                torrentInfo.InfoHash = hash;

                if (remoteEpisode.ReleaseSource != ReleaseSourceType.InteractiveSearch &&
                    indexerSettings?.RejectBlocklistedTorrentHashesWhileGrabbing == true &&
                    _blocklistService.BlocklistedTorrentHash(remoteEpisode.Series.Id, hash))
                {
                    throw new ReleaseBlockedException(remoteEpisode.Release, "Release previously added to blocklist");
                }
            }
        }
    }
}
