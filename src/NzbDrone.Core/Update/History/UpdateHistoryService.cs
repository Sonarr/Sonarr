using System;
using System.Collections.Generic;
using NzbDrone.Common.EnvironmentInfo;
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

    public class UpdateHistoryService : IUpdateHistoryService, IHandle<ApplicationStartedEvent>, IHandleAsync<ApplicationStartedEvent>
    {
        private readonly IUpdateHistoryRepository _repository;
        private readonly IEventAggregator _eventAggregator;
        private Version _prevVersion;

        public UpdateHistoryService(IUpdateHistoryRepository repository, IEventAggregator eventAggregator)
        {
            _repository = repository;
            _eventAggregator = eventAggregator;
        }

        public Version PreviouslyInstalled()
        {
            var history = _repository.PreviouslyInstalled();

            return history?.Version;
        }

        public List<UpdateHistory> InstalledSince(DateTime dateTime)
        {
            return _repository.InstalledSince(dateTime);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (BuildInfo.Version.Major == 10)
            {
                // Don't save dev versions, they change constantly
                return;
            }

            var history = _repository.LastInstalled();

            if (history == null || history.Version != BuildInfo.Version)
            {
                _prevVersion = history.Version;

                _repository.Insert(new UpdateHistory
                {
                    Date = DateTime.UtcNow,
                    Version = BuildInfo.Version,
                    EventType = UpdateHistoryEventType.Installed
                });
            }
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            if (_prevVersion != null)
            {
                _eventAggregator.PublishEvent(new UpdateInstalledEvent(_prevVersion, BuildInfo.Version));
            }
        }
    }
}
