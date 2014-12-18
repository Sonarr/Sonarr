using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentMigrator;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.History;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class history_downloadIdFixture : MigrationTest<history_downloadId>
    {
        [Test]
        public void should_move_grab_id_from_date_to_columns()
        {
            WithTestDb(c =>
            {
                InsertHistory(c, new Dictionary<string, string>
                {
                    {"indexer","test"},
                    {"downloadClientId","123"}
                });

                InsertHistory(c, new Dictionary<string, string>
                {
                    {"indexer","test"},
                    {"downloadClientId","abc"}
                });

            });

            var allProfiles = Mocker.Resolve<HistoryRepository>().All().ToList();

            allProfiles.Should().HaveCount(2);
            allProfiles.Should().NotContain(c => c.Data.ContainsKey("downloadClientId"));
            allProfiles.Should().Contain(c => c.DownloadId == "123");
            allProfiles.Should().Contain(c => c.DownloadId == "abc");
        }


        [Test]
        public void should_leave_items_with_no_grabid()
        {
            WithTestDb(c =>
            {
                InsertHistory(c, new Dictionary<string, string>
                {
                    {"indexer","test"},
                    {"downloadClientId","123"}
                });

                InsertHistory(c, new Dictionary<string, string>
                {
                    {"indexer","test"}
                });

            });

            var allProfiles = Mocker.Resolve<HistoryRepository>().All().ToList();

            allProfiles.Should().HaveCount(2);
            allProfiles.Should().NotContain(c => c.Data.ContainsKey("downloadClientId"));
            allProfiles.Should().Contain(c => c.DownloadId == "123");
            allProfiles.Should().Contain(c => c.DownloadId == null);
        }

        [Test]
        public void should_leave_other_data()
        {
            WithTestDb(c =>
            {
                InsertHistory(c, new Dictionary<string, string>
                {
                    {"indexer","test"},
                    {"group","test2"},
                    {"downloadClientId","123"}
                });
            });

            var allProfiles = Mocker.Resolve<HistoryRepository>().All().Single();

            allProfiles.Data.Should().NotContainKey("downloadClientId");
            allProfiles.Data.Should().Contain(new KeyValuePair<string, string>("indexer", "test"));
            allProfiles.Data.Should().Contain(new KeyValuePair<string, string>("group", "test2"));

            allProfiles.DownloadId.Should().Be("123");
        }


        private void InsertHistory(MigrationBase migrationBase, Dictionary<string, string> data)
        {
            migrationBase.Insert.IntoTable("History").Row(new
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
