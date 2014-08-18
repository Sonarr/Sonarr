using System;
using System.Collections.Generic;
using Marr.Data;
using Marr.Data.Mapping;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Metadata;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.ThingiProvider;
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

            Mapper.Entity<IndexerDefinition>().RegisterModel("Indexers")
                  .Ignore(i => i.Protocol)
                  .Ignore(i => i.SupportsRss)
                  .Ignore(i => i.SupportsSearch);

            Mapper.Entity<ScheduledTask>().RegisterModel("ScheduledTasks");
            Mapper.Entity<NotificationDefinition>().RegisterModel("Notifications");
            Mapper.Entity<MetadataDefinition>().RegisterModel("Metadata");
            Mapper.Entity<DownloadClientDefinition>().RegisterModel("DownloadClients");

            Mapper.Entity<SceneMapping>().RegisterModel("SceneMappings");

            Mapper.Entity<History.History>().RegisterModel("History")
                  .AutoMapChildModels();

            Mapper.Entity<Series>().RegisterModel("Series")
                  .Ignore(s => s.RootFolderPath)
                  .Relationship()
                  .HasOne(s => s.Profile, s => s.ProfileId);

            Mapper.Entity<EpisodeFile>().RegisterModel("EpisodeFiles")
                  .Ignore(f => f.Path)
                  .Relationships.AutoMapICollectionOrComplexProperties();

            Mapper.Entity<Episode>().RegisterModel("Episodes")
                  .Ignore(e => e.SeriesTitle)
                  .Ignore(e => e.Series)
                  .Ignore(e => e.HasFile)
                  .Relationship()
                  .HasOne(episode => episode.EpisodeFile, episode => episode.EpisodeFileId);
            
            Mapper.Entity<Profile>().RegisterModel("Profiles");
            Mapper.Entity<QualityDefinition>().RegisterModel("QualityDefinitions");
            Mapper.Entity<Log>().RegisterModel("Logs");
            Mapper.Entity<NamingConfig>().RegisterModel("NamingConfig");
            Mapper.Entity<SeriesStatistics>().MapResultSet();
            Mapper.Entity<Blacklist>().RegisterModel("Blacklist");
            Mapper.Entity<MetadataFile>().RegisterModel("MetadataFiles");

            Mapper.Entity<PendingRelease>().RegisterModel("PendingReleases")
                  .Ignore(e => e.RemoteEpisode);
        }

        private static void RegisterMappers()
        {
            RegisterEmbeddedConverter();
            RegisterProviderSettingConverter();

            MapRepository.Instance.RegisterTypeConverter(typeof(Int32), new Int32Converter());
            MapRepository.Instance.RegisterTypeConverter(typeof(DateTime), new UtcConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Boolean), new BooleanIntConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Enum), new EnumIntConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Quality), new QualityIntConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(List<ProfileQualityItem>), new EmbeddedDocumentConverter(new QualityIntConverter()));
            MapRepository.Instance.RegisterTypeConverter(typeof(QualityModel), new EmbeddedDocumentConverter(new QualityIntConverter()));
            MapRepository.Instance.RegisterTypeConverter(typeof(Dictionary<string, string>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(List<int>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(List<string>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(ParsedEpisodeInfo), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(ReleaseInfo), new EmbeddedDocumentConverter());
        }

        private static void RegisterProviderSettingConverter()
        {
            var settingTypes = typeof(IProviderConfig).Assembly.ImplementationsOf<IProviderConfig>();

            var providerSettingConverter = new ProviderSettingConverter();
            foreach (var embeddedType in settingTypes)
            {
                MapRepository.Instance.RegisterTypeConverter(embeddedType, providerSettingConverter);
            }
        }

        private static void RegisterEmbeddedConverter()
        {
            var embeddedTypes = typeof(IEmbeddedDocument).Assembly.ImplementationsOf<IEmbeddedDocument>();

            var embeddedConvertor = new EmbeddedDocumentConverter();
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