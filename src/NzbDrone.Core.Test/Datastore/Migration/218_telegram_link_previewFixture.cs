using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Notifications.Telegram;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class telegram_link_previewFixture : MigrationTest<telegram_link_preview>
    {
        [Test]
        public void should_set_link_preview_to_none_when_no_metadata_links()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Notifications").Row(new
                {
                    OnGrab = true,
                    OnDownload = true,
                    OnUpgrade = true,
                    OnHealthIssue = true,
                    IncludeHealthWarnings = true,
                    OnRename = true,
                    Name = "Telegram Sonarr",
                    Implementation = "Telegram",
                    Tags = "[]",
                    Settings = new TelegramSettings217
                    {
                        BotToken = "secret",
                        ChatId = "12345",
                        SendSilently = false,
                        IncludeAppNameInTitle = false,
                        IncludeInstanceNameInTitle = false,
                        MetadataLinks = new List<int>()
                    }.ToJson(),
                    ConfigContract = "TelegramSettings"
                });
            });

            var items = db.Query<NotificationDefinition218>("SELECT * FROM \"Notifications\"");

            items.Should().HaveCount(1);
            items.First().Implementation.Should().Be("Telegram");
            items.First().ConfigContract.Should().Be("TelegramSettings");
            items.First().Settings.LinkPreview.Should().Be((int)MetadataLinkPreviewType.None);
        }

        [Test]
        public void should_set_link_preview_to_first_metadata_link_when_not_tvdb()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Notifications").Row(new
                {
                    OnGrab = true,
                    OnDownload = true,
                    OnUpgrade = true,
                    OnHealthIssue = true,
                    IncludeHealthWarnings = true,
                    OnRename = true,
                    Name = "Telegram Sonarr",
                    Implementation = "Telegram",
                    Tags = "[]",
                    Settings = new TelegramSettings217
                    {
                        BotToken = "secret",
                        ChatId = "12345",
                        SendSilently = false,
                        IncludeAppNameInTitle = false,
                        IncludeInstanceNameInTitle = false,
                        MetadataLinks = new List<int> { 0, 1 }
                    }.ToJson(),
                    ConfigContract = "TelegramSettings"
                });
            });

            var items = db.Query<NotificationDefinition218>("SELECT * FROM \"Notifications\"");

            items.Should().HaveCount(1);
            items.First().Implementation.Should().Be("Telegram");
            items.First().ConfigContract.Should().Be("TelegramSettings");
            items.First().Settings.LinkPreview.Should().Be((int)MetadataLinkPreviewType.Imdb);
        }

        [Test]
        public void should_set_link_preview_to_first_metadata_link_that_is_not_tvdb()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Notifications").Row(new
                {
                    OnGrab = true,
                    OnDownload = true,
                    OnUpgrade = true,
                    OnHealthIssue = true,
                    IncludeHealthWarnings = true,
                    OnRename = true,
                    Name = "Telegram Sonarr",
                    Implementation = "Telegram",
                    Tags = "[]",
                    Settings = new TelegramSettings217
                    {
                        BotToken = "secret",
                        ChatId = "12345",
                        SendSilently = false,
                        IncludeAppNameInTitle = false,
                        IncludeInstanceNameInTitle = false,
                        MetadataLinks = new List<int> { 1, 2 }
                    }.ToJson(),
                    ConfigContract = "TelegramSettings"
                });
            });

            var items = db.Query<NotificationDefinition218>("SELECT * FROM \"Notifications\"");

            items.Should().HaveCount(1);
            items.First().Implementation.Should().Be("Telegram");
            items.First().ConfigContract.Should().Be("TelegramSettings");
            items.First().Settings.LinkPreview.Should().Be((int)MetadataLinkPreviewType.Tvmaze);
        }

        [Test]
        public void should_set_link_preview_to_none_when_only_metadata_link_is_tvdb()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Notifications").Row(new
                {
                    OnGrab = true,
                    OnDownload = true,
                    OnUpgrade = true,
                    OnHealthIssue = true,
                    IncludeHealthWarnings = true,
                    OnRename = true,
                    Name = "Telegram Sonarr",
                    Implementation = "Telegram",
                    Tags = "[]",
                    Settings = new TelegramSettings217
                    {
                        BotToken = "secret",
                        ChatId = "12345",
                        SendSilently = false,
                        IncludeAppNameInTitle = false,
                        IncludeInstanceNameInTitle = false,
                        MetadataLinks = new List<int> { 1 }
                    }.ToJson(),
                    ConfigContract = "TelegramSettings"
                });
            });

            var items = db.Query<NotificationDefinition218>("SELECT * FROM \"Notifications\"");

            items.Should().HaveCount(1);
            items.First().Implementation.Should().Be("Telegram");
            items.First().ConfigContract.Should().Be("TelegramSettings");
            items.First().Settings.LinkPreview.Should().Be((int)MetadataLinkPreviewType.None);
        }
    }

    public class NotificationDefinition218
    {
        public int Id { get; set; }
        public string Implementation { get; set; }
        public string ConfigContract { get; set; }
        public TelegramSettings218 Settings { get; set; }
        public string Name { get; set; }
        public bool OnGrab { get; set; }
        public bool OnDownload { get; set; }
        public bool OnUpgrade { get; set; }
        public bool OnRename { get; set; }
        public bool OnSeriesDelete { get; set; }
        public bool OnEpisodeFileDelete { get; set; }
        public bool OnEpisodeFileDeleteForUpgrade { get; set; }
        public bool OnHealthIssue { get; set; }
        public bool OnApplicationUpdate { get; set; }
        public bool OnManualInteractionRequired { get; set; }
        public bool OnSeriesAdd { get; set; }
        public bool OnHealthRestored { get; set; }
        public bool OnImportComplete { get; set; }
        public bool SupportsOnGrab { get; set; }
        public bool SupportsOnDownload { get; set; }
        public bool SupportsOnUpgrade { get; set; }
        public bool SupportsOnRename { get; set; }
        public bool SupportsOnSeriesDelete { get; set; }
        public bool SupportsOnEpisodeFileDelete { get; set; }
        public bool SupportsOnEpisodeFileDeleteForUpgrade { get; set; }
        public bool SupportsOnHealthIssue { get; set; }
        public bool IncludeHealthWarnings { get; set; }
        public List<int> Tags { get; set; }
    }

    public class TelegramSettings217
    {
        public string BotToken { get; set; }
        public string ChatId { get; set; }
        public int? TopicId { get; set; }
        public bool SendSilently { get; set; }
        public bool IncludeAppNameInTitle { get; set; }
        public bool IncludeInstanceNameInTitle { get; set; }
        public IEnumerable<int> MetadataLinks { get; set; }
    }

    public class TelegramSettings218
    {
        public string BotToken { get; set; }
        public string ChatId { get; set; }
        public int? TopicId { get; set; }
        public bool SendSilently { get; set; }
        public bool IncludeAppNameInTitle { get; set; }
        public bool IncludeInstanceNameInTitle { get; set; }
        public IEnumerable<int> MetadataLinks { get; set; }
        public int LinkPreview { get; set; }
    }
}
