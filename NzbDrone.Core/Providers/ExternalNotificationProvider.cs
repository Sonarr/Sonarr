using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class ExternalNotificationProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDatabase _database;

        private IEnumerable<ExternalNotificationBase> _notifiers;

        [Inject]
        public ExternalNotificationProvider(IDatabase database, IEnumerable<ExternalNotificationBase> notifiers)
        {
            _database = database;
            _notifiers = notifiers;
        }

        public ExternalNotificationProvider()
        {

        }

        public virtual List<ExternalNotificationDefinition> All()
        {
            return _database.Fetch<ExternalNotificationDefinition>();
        }

        public virtual void SaveSettings(ExternalNotificationDefinition settings)
        {
            if (settings.Id == 0)
            {
                Logger.Debug("Adding External Notification definition for {0}", settings.Name);
                _database.Insert(settings);
            }

            else
            {
                Logger.Debug("Updating External Notification definition for {0}", settings.Name);
                _database.Update(settings);
            }
        }

        public virtual ExternalNotificationDefinition GetSettings(Type type)
        {
            return _database.SingleOrDefault<ExternalNotificationDefinition>("WHERE ExternalNotificationProviderType = @0", type.ToString());
        }

        public virtual IList<ExternalNotificationBase> GetEnabledExternalNotifiers()
        {
            var all = All();
            return _notifiers.Where(i => all.Exists(c => c.ExternalNotificationProviderType == i.GetType().ToString() && c.Enable)).ToList();
        }

        public virtual void InitializeNotifiers(IList<ExternalNotificationBase> notifiers)
        {
            Logger.Debug("Initializing notifiers. Count {0}", notifiers.Count);

            _notifiers = notifiers;

            var currentNotifiers = All();

            foreach (var notificationProvider in notifiers)
            {
                ExternalNotificationBase externalNotificationProviderLocal = notificationProvider;
                if (!currentNotifiers.Exists(c => c.ExternalNotificationProviderType == externalNotificationProviderLocal.GetType().ToString()))
                {
                    var settings = new ExternalNotificationDefinition
                                       {
                                           Enable = false,
                                           ExternalNotificationProviderType = externalNotificationProviderLocal.GetType().ToString(),
                                           Name = externalNotificationProviderLocal.Name
                                       };

                    SaveSettings(settings);
                }
            }
        }

        public virtual void OnGrab(string message)
        {
            foreach (var notifier in _notifiers.Where(i => GetSettings(i.GetType()).Enable))
            {
                notifier.OnGrab(message);
            }
        }

        public virtual void OnDownload(string message, Series series)
        {
            foreach (var notifier in _notifiers.Where(i => GetSettings(i.GetType()).Enable))
            {
                notifier.OnDownload(message, series);
            }
        }

        public virtual void OnRename(string message, Series series)
        {
            foreach (var notifier in _notifiers.Where(i => GetSettings(i.GetType()).Enable))
            {
                notifier.OnRename(message, series);
            }
        }

        public virtual void AfterRename(string message, Series series)
        {
            foreach (var notifier in _notifiers.Where(i => GetSettings(i.GetType()).Enable))
            {
                notifier.AfterRename(message, series);
            }
        }
    }
}