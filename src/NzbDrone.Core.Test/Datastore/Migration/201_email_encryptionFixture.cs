using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Notifications.Email;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class email_encryptionFixture : MigrationTest<email_encryption>
    {
        [Test]
        public void should_convert_do_not_require_encryption_to_auto()
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
                    Name = "Mail Sonarr",
                    Implementation = "Email",
                    Tags = "[]",
                    Settings = new EmailSettings200
                    {
                        Server = "smtp.gmail.com",
                        Port = 563,
                        To = new List<string> { "dont@email.me" },
                        RequireEncryption = false
                    }.ToJson(),
                    ConfigContract = "EmailSettings"
                });
            });

            var items = db.Query<NotificationDefinition201>("SELECT * FROM \"Notifications\"");

            items.Should().HaveCount(1);
            items.First().Implementation.Should().Be("Email");
            items.First().ConfigContract.Should().Be("EmailSettings");
            items.First().Settings.UseEncryption.Should().Be((int)EmailEncryptionType.Preferred);
        }

        [Test]
        public void should_convert_require_encryption_to_always()
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
                    Name = "Mail Sonarr",
                    Implementation = "Email",
                    Tags = "[]",
                    Settings = new EmailSettings200
                    {
                        Server = "smtp.gmail.com",
                        Port = 563,
                        To = new List<string> { "dont@email.me" },
                        RequireEncryption = true
                    }.ToJson(),
                    ConfigContract = "EmailSettings"
                });
            });

            var items = db.Query<NotificationDefinition201>("SELECT * FROM \"Notifications\"");

            items.Should().HaveCount(1);
            items.First().Implementation.Should().Be("Email");
            items.First().ConfigContract.Should().Be("EmailSettings");
            items.First().Settings.UseEncryption.Should().Be((int)EmailEncryptionType.Always);
        }
    }

    public class NotificationDefinition201
    {
        public int Id { get; set; }
        public string Implementation { get; set; }
        public string ConfigContract { get; set; }
        public EmailSettings201 Settings { get; set; }
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

    public class EmailSettings200
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool RequireEncryption { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public IEnumerable<string> To { get; set; }
        public IEnumerable<string> Cc { get; set; }
        public IEnumerable<string> Bcc { get; set; }
    }

    public class EmailSettings201
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public int UseEncryption { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public IEnumerable<string> To { get; set; }
        public IEnumerable<string> Cc { get; set; }
        public IEnumerable<string> Bcc { get; set; }
    }
}
