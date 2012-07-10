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

        private IEnumerable<MetadataBase> _metadataProviders;
        private readonly TvDbProvider _tvDbProvider;

        [Inject]
        public MetadataProvider(IDatabase database, IEnumerable<MetadataBase> metadataProviders, TvDbProvider tvDbProvider)
        {
            _database = database;
            _metadataProviders = metadataProviders;
            _tvDbProvider = tvDbProvider;
        }

        public MetadataProvider()
        {

        }

        public virtual List<MetadataDefinition> All()
        {
            return _database.Fetch<MetadataDefinition>();
        }

        public virtual void SaveSettings(MetadataDefinition settings)
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

        public virtual MetadataDefinition GetSettings(Type type)
        {
            return _database.SingleOrDefault<MetadataDefinition>("WHERE MetadataProviderType = @0", type.ToString());
        }

        public virtual IList<MetadataBase> GetEnabledMetabaseProviders()
        {
            var all = All();
            return _metadataProviders.Where(i => all.Exists(c => c.MetadataProviderType == i.GetType().ToString() && c.Enable)).ToList();
        }

        public virtual void Initialize(IList<MetadataBase> metabaseProviders)
        {
            Logger.Debug("Initializing metabases. Count {0}", metabaseProviders.Count);

            _metadataProviders = metabaseProviders;

            var currentNotifiers = All();

            foreach (var notificationProvider in metabaseProviders)
            {
                MetadataBase metadataProviderLocal = notificationProvider;
                if (!currentNotifiers.Exists(c => c.MetadataProviderType == metadataProviderLocal.GetType().ToString()))
                {
                    var settings = new MetadataDefinition
                                       {
                                           Enable = false,
                                           MetadataProviderType = metadataProviderLocal.GetType().ToString(),
                                           Name = metadataProviderLocal.Name
                                       };

                    SaveSettings(settings);
                }
            }
        }

        public virtual void CreateForSeries(Series series)
        {
            var tvDbSeries = _tvDbProvider.GetSeries(series.SeriesId, false, true);

            foreach (var provider in _metadataProviders.Where(i => GetSettings(i.GetType()).Enable))
            {
                provider.ForSeries(series, tvDbSeries);
            }
        }

        public virtual void CreateForEpisodeFile(EpisodeFile episodeFile)
        {
            var tvDbSeries = _tvDbProvider.GetSeries(episodeFile.SeriesId, true, true);

            foreach (var provider in _metadataProviders.Where(i => GetSettings(i.GetType()).Enable))
            {
                provider.ForEpisodeFile(episodeFile, tvDbSeries);
            }
        }
    }
}