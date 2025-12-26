using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Update.History.Events;

namespace NzbDrone.Core.Update.History
{
    public interface IUpdateHistoryService
    {
        Version PreviouslyInstalled();
        List<UpdateHistory> InstalledSince(DateTime dateTime);
    }

    public class UpdateHistoryService : IUpdateHistoryService, IHandleAsync<ApplicationStartedEvent>
    {
        private readonly IUpdateHistoryRepository _repository;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;
        private Version _prevVersion;

        public UpdateHistoryService(IUpdateHistoryRepository repository, IEventAggregator eventAggregator, IConfigFileProvider configFileProvider, Logger logger)
        {
            _repository = repository;
            _eventAggregator = eventAggregator;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public Version PreviouslyInstalled()
        {
            try
            {
                var history = _repository.PreviouslyInstalledAsync().GetAwaiter().GetResult();

                return history?.Version;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to determine previously installed version");
                return null;
            }
        }

        public List<UpdateHistory> InstalledSince(DateTime dateTime)
        {
            try
            {
                return _repository.InstalledSinceAsync(dateTime).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to get list of previously installed versions");
                return new List<UpdateHistory>();
            }
        }

        public async Task HandleAsync(ApplicationStartedEvent message, CancellationToken cancellationToken)
        {
            if (BuildInfo.Version.Major == 10 || !_configFileProvider.LogDbEnabled)
            {
                // Don't save dev versions, they change constantly
                return;
            }

            UpdateHistory history;
            try
            {
                history = await _repository.LastInstalledAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Cleaning corrupted update history");
                await _repository.PurgeAsync(false, cancellationToken).ConfigureAwait(false);
                history = null;
            }

            if (history == null || history.Version != BuildInfo.Version)
            {
                _prevVersion = history?.Version;

                await _repository.InsertAsync(new UpdateHistory
                {
                    Date = DateTime.UtcNow,
                    Version = BuildInfo.Version,
                    EventType = UpdateHistoryEventType.Installed
                },
                cancellationToken).ConfigureAwait(false);
            }

            if (_prevVersion != null)
            {
                await _eventAggregator.PublishEventAsync(new UpdateInstalledEvent(_prevVersion, BuildInfo.Version), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
