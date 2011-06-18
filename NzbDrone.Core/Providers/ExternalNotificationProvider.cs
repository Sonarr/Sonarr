using System;
using System.Collections.Generic;
using Ninject;
using NLog;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class ExternalNotificationProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDatabase _database;

          [Inject]
        public ExternalNotificationProvider(IDatabase database)
        {
            _database = database;
        }

        public ExternalNotificationProvider()
        {

        }

        public virtual List<ExternalNotificationSetting> All()
        {
            return _database.Fetch<ExternalNotificationSetting>();
        }

        public virtual void SaveSettings(ExternalNotificationSetting settings)
        {
            if (settings.Id == 0)
            {
                Logger.Debug("Adding External Notification settings for {0}", settings.Name);
                _database.Insert(settings);
            }

            else
            {
                Logger.Debug("Updating External Notification settings for {0}", settings.Name);
                _database.Update(settings);
            }
        }

        public virtual ExternalNotificationSetting GetSettings(Type type)
        {
            return _database.SingleOrDefault<ExternalNotificationSetting>("WHERE NotifierName = @0", type.ToString());
        }

        public virtual ExternalNotificationSetting GetSettings(int id)
        {
            return _database.SingleOrDefault<ExternalNotificationSetting>(id);
        }

        public virtual void InitializeNotifiers(IList<ExternalNotificationProviderBase> notifiers)
        {
            Logger.Info("Initializing notifiers. Count {0}", notifiers.Count);

            var currentNotifiers = All();

            foreach (var feedProvider in notifiers)
            {
                ExternalNotificationProviderBase externalNotificationProviderLocal = feedProvider;
                if (!currentNotifiers.Exists(c => c.NotifierName == externalNotificationProviderLocal.GetType().ToString()))
                {
                    var settings = new ExternalNotificationSetting()
                                       {
                                           Enabled = false,
                                           NotifierName = externalNotificationProviderLocal.GetType().ToString(),
                                           Name = externalNotificationProviderLocal.Name
                                       };

                    SaveSettings(settings);
                }
            }
        }
    }
}