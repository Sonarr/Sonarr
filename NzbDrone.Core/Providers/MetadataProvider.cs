using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Providers.Metadata;
using NzbDrone.Core.Repository;
using PetaPoco;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class MetadataProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDatabase _database;

        private IList<MetadataBase> _metadataProviders;
        private readonly TvDbProvider _tvDbProvider;

        public MetadataProvider(IDatabase database, IEnumerable<MetadataBase> metadataProviders,
                                TvDbProvider tvDbProvider)
        {
            _database = database;
            _metadataProviders = metadataProviders.ToList();
            _tvDbProvider = tvDbProvider;

            Initialize(_metadataProviders);
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

        private void Initialize(IList<MetadataBase> metabaseProviders)
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
            var tvDbSeries = _tvDbProvider.GetSeries(series.TvDbId, false, true);

            CreateForSeries(series, tvDbSeries);
        }

        public virtual void CreateForSeries(Series series, TvdbSeries tvDbSeries)
        {
            foreach (var provider in _metadataProviders.Where(i => GetSettings(i.GetType()).Enable))
            {
                provider.CreateForSeries(series, tvDbSeries);
            }
        }

        public virtual void CreateForEpisodeFile(EpisodeFile episodeFile)
        {
            var tvDbSeries = _tvDbProvider.GetSeries(episodeFile.SeriesId, true, true);

            CreateForEpisodeFile(episodeFile, tvDbSeries);
        }

        public virtual void CreateForEpisodeFile(EpisodeFile episodeFile, TvdbSeries tvDbSeries)
        {
            foreach (var provider in _metadataProviders.Where(i => GetSettings(i.GetType()).Enable))
            {
                provider.CreateForEpisodeFile(episodeFile, tvDbSeries);
            }
        }

        public virtual void CreateForEpisodeFiles(List<EpisodeFile> episodeFiles)
        {
            if (episodeFiles == null || !episodeFiles.Any())
            {
                Logger.Warn("No episode files, no metadata will be created.");
                throw new ArgumentException("EpsiodeFiles must not be null or empty", "episodeFiles");
            }

            Logger.Trace("Creating metadata for {0} files.", episodeFiles.Count);

            var tvDbSeries = _tvDbProvider.GetSeries(episodeFiles.First().SeriesId, true, true);

            foreach(var episodeFile in episodeFiles)
            {
                foreach (var provider in _metadataProviders.Where(i => GetSettings(i.GetType()).Enable))
                {
                    Logger.Trace("Creating {0} metadata for {1}", provider.Name, episodeFile.Id);
                    provider.CreateForEpisodeFile(episodeFile, tvDbSeries);
                }
            }
        }

        public virtual void RemoveForSeries(Series series)
        {
            foreach (var provider in _metadataProviders.Where(i => GetSettings(i.GetType()).Enable))
            {
                provider.RemoveForSeries(series);
            }
        }

        public virtual void RemoveForEpisodeFile(EpisodeFile episodeFile)
        {
            foreach (var provider in _metadataProviders.Where(i => GetSettings(i.GetType()).Enable))
            {
                provider.RemoveForEpisodeFile(episodeFile);
            }
        }

        public virtual void RemoveForEpisodeFiles(List<EpisodeFile> episodeFiles)
        {
            foreach (var episodeFile in episodeFiles)
            {
                foreach (var provider in _metadataProviders.Where(i => GetSettings(i.GetType()).Enable))
                {
                    provider.RemoveForEpisodeFile(episodeFile);
                }
            }
        }
    }
}