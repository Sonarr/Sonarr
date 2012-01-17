using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.MediaFileProviderTests
{
    [TestFixture]
    public class CleanUpDatabaseFixture : CoreTest
    {

        [SetUp]
        public void Setup()
        {
            WithRealDb();
        }

        private void WithAutoIgnore(bool autoIgnore)
        {

            Mocker.GetMock<ConfigProvider>()
                    .SetupGet(c => c.AutoIgnorePreviouslyDownloadedEpisodes).Returns(autoIgnore);
        }



        [Test]
        public void CleanUpDatabse_should_detach_none_existing_file_from_episodes_with_auto_ignore()
        {
            WithAutoIgnore(true);
            
            var episodes = Builder<Episode>.CreateListOfSize(3)
                .All().With(c => c.GrabDate = DateTime.Now)
                      .And(c => c.Ignored = false)
                      .And(c => c.PostDownloadStatus = PostDownloadStatusType.NoError)
                .Build();


            Db.InsertMany(episodes);

            //Act
            Mocker.Resolve<MediaFileProvider>().CleanUpDatabase();
            var result = Db.Fetch<Episode>();

            //Assert
            result.Should().HaveSameCount(episodes);
            result.Should().OnlyContain(e => e.EpisodeFileId == 0);
            result.Should().OnlyContain(e => e.PostDownloadStatus == PostDownloadStatusType.Unknown);
            result.Should().OnlyContain(e => e.Ignored);
            result.Should().OnlyContain(e => e.GrabDate == null);
        }

        [Test]
        public void CleanUpDatabse_should_detach_none_existing_file_from_episodes_with_no_auto_ignore()
        {
            WithAutoIgnore(false);

            var episodes = Builder<Episode>.CreateListOfSize(3)
                .All().With(c => c.GrabDate = DateTime.Now)
                      .And(c => c.PostDownloadStatus = PostDownloadStatusType.NoError)
                .TheFirst(2).With(c => c.Ignored = true)
                .TheLast(1).With(c => c.Ignored = false)
                .Build();


            Db.InsertMany(episodes);

            //Act
            Mocker.Resolve<MediaFileProvider>().CleanUpDatabase();
            var result = Db.Fetch<Episode>();

            //Assert
            result.Should().HaveSameCount(episodes);
            result.Should().OnlyContain(e => e.EpisodeFileId == 0);
            result.Should().OnlyContain(e => e.PostDownloadStatus == PostDownloadStatusType.Unknown);
            result.Should().OnlyContain(e => e.GrabDate == null);
            result.Should().Contain(c => c.Ignored == true);
            result.Should().Contain(c => c.Ignored == false);
        }

        [Test]
        public void CleanUpDatabse_should_not_change_episodes_with_no_file_id()
        {
            //Setup
            var episodes = Builder<Episode>.CreateListOfSize(3)
                .All().With(c => c.GrabDate = DateTime.Now)
                      .And(c => c.Ignored = false)
                      .And(c => c.PostDownloadStatus = PostDownloadStatusType.NoError)
                      .And(c => c.EpisodeFileId = 0)
                .Build();

            Db.InsertMany(episodes);

            //Act
            Mocker.Resolve<MediaFileProvider>().CleanUpDatabase();
            var result = Db.Fetch<Episode>();

            //Assert
            result.Should().HaveSameCount(episodes);
            result.Should().OnlyContain(e => e.EpisodeFileId == 0);
            result.Should().NotContain(e => e.PostDownloadStatus == PostDownloadStatusType.Unknown);
            result.Should().NotContain(e => e.Ignored);
            result.Should().NotContain(e => e.GrabDate == null);
        }


        [Test]
        public void DeleteOrphanedEpisodeFiles()
        {
            //Setup
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10).Build();
            var episodes = Builder<Episode>.CreateListOfSize(5).Build();

            Db.InsertMany(episodes);
            Db.InsertMany(episodeFiles);

            //Act
            Mocker.Resolve<MediaFileProvider>().CleanUpDatabase();
            var result = Db.Fetch<EpisodeFile>();

            //Assert
            result.Should().HaveCount(5);
            result.Should().OnlyContain(e => e.EpisodeFileId > 0);
        }
    }
}
