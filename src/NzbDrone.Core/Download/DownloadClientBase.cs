using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Configuration;
using NLog;
using FluentValidation.Results;
using NzbDrone.Core.Validation;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download
{
    public abstract class DownloadClientBase<TSettings> : IDownloadClient
        where TSettings : IProviderConfig, new()
    {
        protected readonly IConfigService _configService;
        protected readonly IDiskProvider _diskProvider;
        protected readonly IParsingService _parsingService;
        protected readonly IRemotePathMappingService _remotePathMappingService;
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

        protected TSettings Settings
        {
            get
            {
                return (TSettings)Definition.Settings;
            }
        }

        protected DownloadClientBase(IConfigService configService, IDiskProvider diskProvider, IParsingService parsingService, IRemotePathMappingService remotePathMappingService, Logger logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _parsingService = parsingService;
            _remotePathMappingService = remotePathMappingService;
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

        public abstract String Download(RemoteEpisode remoteEpisode);
        public abstract IEnumerable<DownloadClientItem> GetItems();
        public abstract void RemoveItem(string id);
        public abstract String RetryDownload(string id);
        public abstract DownloadClientStatus GetStatus();

        protected RemoteEpisode GetRemoteEpisode(String title)
        {
            var parsedEpisodeInfo = Parser.Parser.ParseTitle(title);
            if (parsedEpisodeInfo == null) return null;

            var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, 0);
            if (remoteEpisode.Series == null) return null;

            return remoteEpisode;
        }

        public ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();
            
            try
            {
                Test(failures);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Test aborted due to exception", ex);
                failures.Add(new ValidationFailure(string.Empty, "Test was aborted due to an error: " + ex.Message));
            }

            return new ValidationResult(failures);
        }

        protected abstract void Test(List<ValidationFailure> failures);

        protected ValidationFailure TestFolder(String folder, String propertyName, Boolean mustBeWritable = true)
        {
            if (!_diskProvider.FolderExists(folder))
            {
                return new NzbDroneValidationFailure(propertyName, "Folder does not exist")
                {
                    DetailedDescription = "The folder you specified does not exist or is inaccessible. Please verify the folder permissions for the user account that is used to execute NzbDrone."
                };
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
                    return new NzbDroneValidationFailure(propertyName, "Unable to write to folder")
                    {
                        DetailedDescription = "The folder you specified is not writable. Please verify the folder permissions for the user account that is used to execute NzbDrone."
                    };
                }
            }

            return null;
        }
    }
}
