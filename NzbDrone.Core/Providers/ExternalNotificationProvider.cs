using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class ExternalNotificationProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _repository;

        public ExternalNotificationProvider(IRepository repository)
        {
            _repository = repository;
        }

        public ExternalNotificationProvider()
        {

        }

        public virtual List<ExternalNotificationSetting> All()
        {
            return _repository.All<ExternalNotificationSetting>().ToList();
        }

        public virtual void SaveSettings(ExternalNotificationSetting settings)
        {
            if (settings.Id == 0)
            {
                Logger.Debug("Adding External Notification settings for {0}", settings.Name);
                _repository.Add(settings);
            }
            else
            {
                Logger.Debug("Updating External Notification settings for {0}", settings.Name);
                _repository.Update(settings);
            }
        }

        public virtual ExternalNotificationSetting GetSettings(Type type)
        {
            return _repository.Single<ExternalNotificationSetting>(s => s.NotifierName == type.ToString());
        }

        public virtual ExternalNotificationSetting GetSettings(int id)
        {
            return _repository.Single<ExternalNotificationSetting>(s => s.Id == id);
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