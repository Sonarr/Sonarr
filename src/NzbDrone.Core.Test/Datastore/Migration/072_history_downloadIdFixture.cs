using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class history_downloadIdFixture : MigrationTest<history_downloadId>
    {
        [Test]
        public void should_move_grab_id_from_date_to_columns()
        {
            var db = WithMigrationTestDb(c =>
            {
                InsertHistory(c, new Dictionary<string, string>
                {
                    { "indexer", "test" },
                    { "downloadClientId", "123" }
                });

                InsertHistory(c, new Dictionary<string, string>
                {
                    { "indexer", "test" },
                    { "downloadClientId", "abc" }
                });
            });

            var history = db.Query<History72>("SELECT DownloadId, Data FROM History");

            history.Should().HaveCount(2);
            history.Should().NotContain(c => c.Data.ContainsKey("downloadClientId"));
            history.Should().Contain(c => c.DownloadId == "123");
            history.Should().Contain(c => c.DownloadId == "abc");
        }

        [Test]
        public void should_leave_items_with_no_grabid()
        {
            var db = WithMigrationTestDb(c =>
            {
                InsertHistory(c, new Dictionary<string, string>
                {
                    { "indexer", "test" },
                    { "downloadClientId", "123" }
                });

                InsertHistory(c, new Dictionary<string, string>
                {
                    { "indexer", "test" }
                });
            });

            var history = db.Query<History72>("SELECT DownloadId, Data FROM History");

            history.Should().HaveCount(2);
            history.Should().NotContain(c => c.Data.ContainsKey("downloadClientId"));
            history.Should().Contain(c => c.DownloadId == "123");
            history.Should().Contain(c => c.DownloadId == null);
        }

        [Test]
        public void should_leave_other_data()
        {
            var db = WithMigrationTestDb(c =>
            {
                InsertHistory(c, new Dictionary<string, string>
                {
                    { "indexer", "test" },
                    { "group", "test2" },
                    { "downloadClientId", "123" }
                });
            });

            var history = db.Query<History72>("SELECT DownloadId, Data FROM History").Single();

            history.Data.Should().NotContainKey("downloadClientId");
            history.Data.Should().Contain(new KeyValuePair<string, string>("indexer", "test"));
            history.Data.Should().Contain(new KeyValuePair<string, string>("group", "test2"));

            history.DownloadId.Should().Be("123");
        }

        private void InsertHistory(NzbDroneMigrationBase migration, Dictionary<string, string> data)
        {
            migration.Insert.IntoTable("History").Row(new
            {
                EpisodeId = 1,
                SeriesId = 1,
                SourceTitle = "Test",
                Date = DateTime.Now,
                Quality = "{}",
                Data = data.ToJson(),
                EventType = 1
            });
        }
    }
}
