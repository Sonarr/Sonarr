using System;
using System.Linq.Expressions;
using System.Reflection;
using Marr.Data;
using Marr.Data.Mapping;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.ExternalNotification;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Tv;
using BooleanIntConverter = NzbDrone.Core.Datastore.Converters.BooleanIntConverter;
using System.Linq;

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

            Mapper.Entity<Indexer>().RegisterModel("IndexerDefinitions");
            Mapper.Entity<NewznabDefinition>().RegisterModel("NewznabDefinitions");
            Mapper.Entity<JobDefinition>().RegisterModel("JobDefinitions");
            Mapper.Entity<ExternalNotificationDefinition>().RegisterModel("ExternalNotificationDefinitions");

            Mapper.Entity<SceneMapping>().RegisterModel("SceneMappings");

            Mapper.Entity<History.History>().RegisterModel("History")
                .HasOne(h => h.Episode, h => h.EpisodeId);

            Mapper.Entity<Series>().RegisterModel("Series");

            Mapper.Entity<Season>().RegisterModel("Seasons");
            Mapper.Entity<Episode>().RegisterModel("Episodes");
            Mapper.Entity<EpisodeFile>().RegisterModel("EpisodeFiles");
            Mapper.Entity<QualityProfile>().RegisterModel("QualityProfiles").For(q => q.DbAllowed).SetColumnName("Allowed").Ignore(q => q.Allowed);
            Mapper.Entity<QualitySize>().RegisterModel("QualitySizes");

            Mapper.Entity<Log>().RegisterModel("Logs");
        }


        private static void RegisterMappers()
        {
            MapRepository.Instance.RegisterTypeConverter(typeof(Int32), new Int32Converter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Boolean), new BooleanIntConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Enum), new EnumIntConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(QualityModel), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(MediaCover.MediaCover), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Quality), new QualityIntConverter());
        }
    }
}