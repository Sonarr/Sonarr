using System;
using System.IO;
using System.Collections.Generic;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Indexers;
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

        public object ConnectData(string stage, IDictionary<string, object> query) { return null; }

        protected TSettings Settings
        {
            get
            {
                return (TSettings)Definition.Settings;
            }
        }

        protected DownloadClientBase(IConfigService configService, 
            IDiskProvider diskProvider, 
            IRemotePathMappingService remotePathMappingService,
            Logger logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
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
        public abstract void RemoveItem(string downloadId, bool deleteData);
        public abstract DownloadClientStatus GetStatus();

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
                    DetailedDescription = string.Format("The folder you specified does not exist or is inaccessible. Please verify the folder permissions for the user account '{0}', which is used to execute Sonarr.", Environment.UserName)
                };
            }

            if (mustBeWritable && !_diskProvider.FolderWritable(folder))
            {
                _logger.Error("Folder '{0}' is not writable.", folder);
                return new NzbDroneValidationFailure(propertyName, "Unable to write to folder")
                {
                    DetailedDescription = string.Format("The folder you specified is not writable. Please verify the folder permissions for the user account '{0}', which is used to execute Sonarr.", Environment.UserName)
                };
            }

            return null;
        }
    }
}
