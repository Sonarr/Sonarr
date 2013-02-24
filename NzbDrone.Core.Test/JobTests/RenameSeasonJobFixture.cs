using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    public class RenameSeasonJobFixture : TestBase
    {
        private ProgressNotification _testNotification;
        private Series _series;
        private IList<EpisodeFile> _episodeFiles;
            
        [SetUp]
        public void Setup()
        {
            _testNotification = new ProgressNotification("TEST");

            _series = Builder<Series>
                    .CreateNew()
                    .Build();

            _episodeFiles = Builder<EpisodeFile>
                    .CreateListOfSize(5)
                    .All()
                    .With(e => e.SeasonNumber = 5)
                    .Build();

            Mocker.GetMock<ISeriesRepository>()
                  .Setup(s => s.Get(_series.OID))
                  .Returns(_series);

            Mocker.GetMock<MediaFileProvider>()
                  .Setup(s => s.GetSeasonFiles(_series.OID, 5))
                  .Returns(_episodeFiles);
        }

        private void WithMovedFiles()
        {
            Mocker.GetMock<DiskScanProvider>()
                  .Setup(s => s.MoveEpisodeFile(It.IsAny<EpisodeFile>(), false))
                  .Returns(_episodeFiles.First());
        }

        [Test]
        public void should_throw_if_seriesId_is_zero()
        {
            Assert.Throws<ArgumentException>(() => 
                Mocker.Resolve<RenameSeasonJob>().Start(_testNotification, new { SeriesId = 0, SeasonNumber = 10 }));
        }

        [Test]
        public void should_throw_if_seasonId_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => 
                Mocker.Resolve<RenameSeasonJob>().Start(_testNotification, new { SeriesId = _series.OID, SeasonNumber = -10 }));
        }

        [Test]
        public void should_log_warning_if_no_episode_files_are_found()
        {
            Mocker.Resolve<RenameSeasonJob>().Start(_testNotification, new { SeriesId = _series.OID, SeasonNumber = 10 });

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_if_no_episodes_are_moved()
        {
            Mocker.Resolve<RenameSeasonJob>().Start(_testNotification, new { SeriesId = _series.OID, SeasonNumber = 5 });

            Mocker.GetMock<MetadataProvider>().Verify(v => v.RemoveForEpisodeFiles(It.IsAny<List<EpisodeFile>>()), Times.Never());
        }

        [Test]
        public void should_return_process_metadata_if_files_are_moved()
        {
            WithMovedFiles();
            Mocker.Resolve<RenameSeasonJob>().Start(_testNotification, new { SeriesId = _series.OID, SeasonNumber = 5 });

            Mocker.GetMock<MetadataProvider>().Verify(v => v.RemoveForEpisodeFiles(It.IsAny<List<EpisodeFile>>()), Times.Once());
        }
    }
}
