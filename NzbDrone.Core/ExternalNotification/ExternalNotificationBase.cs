using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Download;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ExternalNotification
{
    public abstract class ExternalNotificationBase
        : IHandle<EpisodeGrabbedEvent>,
          IHandle<EpisodeDownloadedEvent>,
          IHandle<SeriesRenamedEvent>
    {
        private readonly IExternalNotificationRepository _externalNotificationRepository;
        private readonly Logger _logger;

        protected ExternalNotificationBase(IExternalNotificationRepository externalNotificationRepository, Logger logger)
        {
            _externalNotificationRepository = externalNotificationRepository;
            _logger = logger;
        }

        public abstract string Name { get; }

        public bool NotifyOnGrab
        {
            get
            {
                return GetEnableStatus(c => c.OnGrab);
            }
            set
            {
                SetEnableStatus(c => c.OnGrab = value);
            }
        }

        public bool NotifyOnDownload
        {
            get
            {
                return GetEnableStatus(c => c.OnDownload);
            }
            set
            {
                SetEnableStatus(c => c.OnDownload = value);
            }
        }

        public bool NotifyOnRename
        {
            get
            {
                return GetEnableStatus(c => c.OnRename);
            }
            set
            {
                SetEnableStatus(c => c.OnRename = value);
            }
        }

        private void SetEnableStatus(Action<ExternalNotificationDefinition> updateAction)
        {
            var def = _externalNotificationRepository.Get(Name) ??
                      new ExternalNotificationDefinition { Name = Name };

            updateAction(def);
            _externalNotificationRepository.Upsert(def);
        }

        private bool GetEnableStatus(Func<ExternalNotificationDefinition, bool> readFunction)
        {
            var def = _externalNotificationRepository.Get(Name) ??
                      new ExternalNotificationDefinition { Name = Name };

            return readFunction(def);
        }



        public void Handle(EpisodeGrabbedEvent message)
        {
            if (NotifyOnGrab)
            {
                try
                {
                    _logger.Trace("Sending grab notification to {0}", Name);
                    OnGrab(message.ParseResult.GetDownloadTitle());

                }
                catch (Exception e)
                {
                    _logger.WarnException("Couldn't send grab notification to " + Name, e);
                }
            }
        }

        public void Handle(EpisodeDownloadedEvent message)
        {
            if (NotifyOnDownload)
            {
                try
                {
                    _logger.Trace("Sending download notification to {0}", Name);
                    OnDownload(message.ParseResult.GetDownloadTitle(), message.ParseResult.Series);
                }
                catch (Exception e)
                {
                    _logger.WarnException("Couldn't send download notification to " + Name, e);
                }
            }
        }

        public void Handle(SeriesRenamedEvent message)
        {
            if (NotifyOnRename)
            {
                try
                {
                    _logger.Trace("Sending rename notification to {0}", Name);
                    AfterRename(message.Series);

                }
                catch (Exception e)
                {
                    _logger.WarnException("Couldn't send rename notification to " + Name, e);
                }
            }
        }


        protected virtual void OnGrab(string message)
        {

        }

        protected virtual void OnDownload(string message, Series series)
        {

        }

        protected virtual void AfterRename(Series series)
        {

        }


    }
}
