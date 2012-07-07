using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Metadata;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class MetadataProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDatabase _database;

        private IEnumerable<MetadataBase> _metadataBases;

        [Inject]
        public MetadataProvider(IDatabase database, IEnumerable<MetadataBase> metadataBases)
        {
            _database = database;
            _metadataBases = metadataBases;
        }

        public MetadataProvider()
        {

        }

        public virtual List<MetabaseDefinition> All()
        {
            return _database.Fetch<MetabaseDefinition>();
        }

        public virtual void SaveSettings(MetabaseDefinition settings)
        {
            if (settings.Id == 0)
            {
                Logger.Debug("Adding Metabase definition for {0}", settings.Name);
                _database.Insert(settings);
            }

            else
            {
                Logger.Debug("Updating Metabase definition for {0}", settings.Name);
                _database.Update(settings);
            }
        }

        public virtual MetabaseDefinition GetSettings(Type type)
        {
            return _database.SingleOrDefault<MetabaseDefinition>("WHERE MetadataProviderType = @0", type.ToString());
        }

        public virtual IList<MetadataBase> GetEnabledExternalNotifiers()
        {
            var all = All();
            return _metadataBases.Where(i => all.Exists(c => c.MetadataProviderType == i.GetType().ToString() && c.Enable)).ToList();
        }

        public virtual void InitializeNotifiers(IList<MetadataBase> notifiers)
        {
            Logger.Debug("Initializing notifiers. Count {0}", notifiers.Count);

            _metadataBases = notifiers;

            var currentNotifiers = All();

            foreach (var notificationProvider in notifiers)
            {
                MetadataBase metadataProviderLocal = notificationProvider;
                if (!currentNotifiers.Exists(c => c.MetadataProviderType == metadataProviderLocal.GetType().ToString()))
                {
                    var settings = new MetabaseDefinition
                                       {
                                           Enable = false,
                                           MetadataProviderType = metadataProviderLocal.GetType().ToString(),
                                           Name = metadataProviderLocal.Name
                                       };

                    SaveSettings(settings);
                }
            }
        }

        public virtual void OnGrab(string message)
        {
            foreach (var notifier in _metadataBases.Where(i => GetSettings(i.GetType()).Enable))
            {
                notifier.OnGrab(message);
            }
        }

        public virtual void OnDownload(string message, Series series)
        {
            foreach (var notifier in _metadataBases.Where(i => GetSettings(i.GetType()).Enable))
            {
                notifier.OnDownload(message, series);
            }
        }

        public virtual void OnRename(string message, Series series)
        {
            foreach (var notifier in _metadataBases.Where(i => GetSettings(i.GetType()).Enable))
            {
                notifier.OnRename(message, series);
            }
        }

        public virtual void AfterRename(string message, Series series)
        {
            foreach (var notifier in _metadataBases.Where(i => GetSettings(i.GetType()).Enable))
            {
                notifier.AfterRename(message, series);
            }
        }
    }
}