using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Configuration;
using NLog;

namespace NzbDrone.Core.Download
{
    public abstract class DownloadClientBase<TSettings> : IDownloadClient
        where TSettings : IProviderConfig, new()
    {
        protected readonly IConfigService _configService;
        protected readonly IDiskProvider _diskProvider;
        protected readonly IParsingService _parsingService;
        protected readonly Logger _logger;

        public Type ConfigContract
        {
            get
            {
                return typeof(TSettings);
            }
        }

        public IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                return new List<ProviderDefinition>();
            }
        }

        public ProviderDefinition Definition { get; set; }
        public abstract ValidationResult Test();

        protected TSettings Settings
        {
            get
            {
                return (TSettings)Definition.Settings;
            }
        }

        protected DownloadClientBase(IConfigService configService, IDiskProvider diskProvider, IParsingService parsingService, Logger logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _parsingService = parsingService;
            _logger = logger;
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        public abstract DownloadProtocol Protocol
        {
            get;
        }

        public abstract string Download(RemoteEpisode remoteEpisode);
        public abstract IEnumerable<DownloadClientItem> GetItems();
        public abstract void RemoveItem(string id);
        public abstract void RetryDownload(string id);
        public abstract DownloadClientStatus GetStatus();

        protected RemoteEpisode GetRemoteEpisode(String title)
        {
            var parsedEpisodeInfo = Parser.Parser.ParseTitle(title);
            if (parsedEpisodeInfo == null) return null;

            var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, 0);
            if (remoteEpisode.Series == null) return null;

            return remoteEpisode;
        }

        protected ValidationFailure TestFolder(String folder, String propertyName, Boolean mustBeWritable = true)
        {
            if (!_diskProvider.FolderExists(folder))
            {
                return new ValidationFailure(propertyName, "Folder does not exist");
            }

            if (mustBeWritable)
            {
                try
                {
                    var testPath = Path.Combine(folder, "drone_test.txt");
                    _diskProvider.WriteAllText(testPath, DateTime.Now.ToString());
                    _diskProvider.DeleteFile(testPath);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex.Message, ex);
                    return new ValidationFailure(propertyName, "Unable to write to folder");
                }
            }

            return null;
        }
    }
}
