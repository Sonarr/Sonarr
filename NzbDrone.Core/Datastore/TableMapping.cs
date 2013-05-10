using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data;
using Marr.Data.Mapping;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.ExternalNotification;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Datastore
{
    public static class TableMapping
    {

        private static readonly FluentMappings Mapper = new FluentMappings(true);

        public static void Map()
        {
            RegisterMappers();

            Mapper.Entity<Config>().RegisterModel("Config");
            Mapper.Entity<RootFolder>().RegisterModel("RootFolders").Ignore(r => r.FreeSpace);

            Mapper.Entity<IndexerDefinition>().RegisterModel("IndexerDefinitions");
            Mapper.Entity<ScheduledTask>().RegisterModel("ScheduledTasks");
            Mapper.Entity<ExternalNotificationDefinition>().RegisterModel("ExternalNotificationDefinitions");

            Mapper.Entity<SceneMapping>().RegisterModel("SceneMappings");

            Mapper.Entity<History.History>().RegisterModel("History")
                  .Relationship()
                  .HasOne(h => h.Episode, h => h.EpisodeId)
                  .HasOne(h => h.Series, h => h.SeriesId);

            Mapper.Entity<Series>().RegisterModel("Series")
                  .Ignore(s => s.Path)
                  .Relationship()
                  .HasOne(s => s.RootFolder, s => s.RootFolderId)
                  .HasOne(s => s.QualityProfile, s => s.QualityProfileId);

            Mapper.Entity<Season>().RegisterModel("Seasons");

            Mapper.Entity<Episode>().RegisterModel("Episodes")
                  .Ignore(e => e.SeriesTitle)
                  .Relationships.AutoMapICollectionOrComplexProperties();

            Mapper.Entity<EpisodeFile>().RegisterModel("EpisodeFiles");

            Mapper.Entity<QualityProfile>().RegisterModel("QualityProfiles");

            Mapper.Entity<QualitySize>().RegisterModel("QualitySizes");

            Mapper.Entity<Log>().RegisterModel("Logs");

            Mapper.Entity<NamingConfig>().RegisterModel("NamingConfig");

            Mapper.Entity<SeriesStatistics>().MapResultSet();
        }

        private static void RegisterMappers()
        {
            RegisterEmbeddedConverter();

            MapRepository.Instance.RegisterTypeConverter(typeof(Int32), new Int32Converter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Boolean), new BooleanIntConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Enum), new EnumIntConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Quality), new QualityIntConverter());
        }

        private static void RegisterEmbeddedConverter()
        {
            var embeddedTypes = typeof(IEmbeddedDocument).Assembly.GetTypes()
                                                          .Where(c => c.GetInterfaces().Any(i => i == typeof(IEmbeddedDocument)));


            var embeddedConvertor = new EmbeddedDocumentConverter(new JsonSerializer());
            var genericListDefinition = typeof(List<>).GetGenericTypeDefinition();
            foreach (var embeddedType in embeddedTypes)
            {
                var embeddedListType = genericListDefinition.MakeGenericType(embeddedType);
                MapRepository.Instance.RegisterTypeConverter(embeddedType, embeddedConvertor);
                MapRepository.Instance.RegisterTypeConverter(embeddedListType, embeddedConvertor);
            }
        }
    }
}