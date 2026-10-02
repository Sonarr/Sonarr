using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class MissingYearImportValidatorFixture : CoreTest<MissingYearImportValidator>
    {
        private Series _series;
        private Episode _episode;
        private TrackedDownload _download;
        private List<EpisodeHistory> _history;
        private List<Series> _library;

        [SetUp]
        public void Setup()
        {
            _series = new Series { Id = 1, TvdbId = 123, Title = "Lab Mystery (2025)", SeriesType = SeriesTypes.Standard };
            _episode = new Episode { Id = 3, SeriesId = 1, SeasonNumber = 1, EpisodeNumber = 2 };
            var item = new DownloadClientItem
            {
                Title = "Lab.Mystery.S01E02.1080p.WEB-DL-LAB",
                DownloadId = "lab-download",
                OutputPath = new OsPath("/downloads/lab")
            };
            _download = new TrackedDownload
            {
                DownloadItem = item,
                ImportItem = item,
                RemoteEpisode = new RemoteEpisode { Series = _series, Episodes = new List<Episode> { _episode } }
            };
            _history = new List<EpisodeHistory>
            {
                new EpisodeHistory
                {
                    SeriesId = 1,
                    EpisodeId = 3,
                    DownloadId = item.DownloadId,
                    SourceTitle = item.Title,
                    EventType = EpisodeHistoryEventType.Grabbed,
                    Data = new Dictionary<string, string> { { "seriesMatchType", "Id" }, { "tvdbId", "123" } }
                }
            };
            _library = new List<Series> { _series };
            Mocker.GetMock<IConfigService>().SetupGet(s => s.AllowMissingYearImport).Returns(true);
            Mocker.GetMock<ISeriesService>().Setup(s => s.GetAllSeries()).Returns(_library);
            Mocker.GetMock<IEpisodeService>().Setup(s => s.GetEpisode(3)).Returns(_episode);
            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneNames(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                  .Returns(new List<string>());
            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderExists("/downloads/lab")).Returns(true);
            GivenFiles("/downloads/lab/Lab.Mystery.S01E02.1080p.WEB-DL-LAB.mkv");
        }

        private void GivenFiles(params string[] files)
        {
            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetFiles("/downloads/lab", true)).Returns(files);
        }

        [Test]
        public void should_allow_verified_missing_year()
        {
            Subject.IsValid(_download, _series, _history).Should().BeTrue();
        }

        [Test]
        public void should_keep_existing_guard_when_disabled()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.AllowMissingYearImport).Returns(false);
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFiles(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [TestCase("Unrelated.S01E02.mkv")]
        [TestCase("Lab.Mystery.2020.S01E02.mkv")]
        [TestCase("Lab.Mystery.S01E03.mkv")]
        [TestCase("Lab.Mystery.S02E02.mkv")]
        [TestCase("Lab.Mystery.S01E02E03.mkv")]
        [TestCase("Lab.Mystery.S01.mkv")]
        [TestCase("obfuscated.mkv")]
        public void should_reject_unverified_file(string file)
        {
            GivenFiles("/downloads/lab/" + file);
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }

        [TestCase("Unrelated.S01E02")]
        [TestCase("Lab.Mystery.2020.S01E02")]
        [TestCase("Lab.Mystery.S01E03")]
        [TestCase("Lab.Mystery.S01")]
        public void should_reject_unverified_download_title(string title)
        {
            _download.DownloadItem.Title = title;
            _history[0].SourceTitle = title;
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }

        [TestCase("tvdbId", "456")]
        [TestCase("tvdbId", "")]
        [TestCase("seriesMatchType", "Title")]
        public void should_reject_unverified_history(string key, string value)
        {
            _history[0].Data[key] = value;
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }

        [Test]
        public void should_reject_missing_history()
        {
            _history.Clear();
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }

        [Test]
        public void should_reject_conflicting_history()
        {
            _history.Add(new EpisodeHistory { SeriesId = 2, EpisodeId = 3, EventType = EpisodeHistoryEventType.Grabbed });
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }

        [Test]
        public void should_reject_existing_episode_file()
        {
            _episode.EpisodeFileId = 42;
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }

        [TestCase("Lab Mystery")]
        [TestCase("Lab Mystery (2020)")]
        public void should_reject_colliding_series(string title)
        {
            _library.Add(new Series { Id = 2, TvdbId = 456, Title = title });
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }

        [Test]
        public void should_reject_colliding_alias()
        {
            _library.Add(new Series { Id = 2, TvdbId = 456, Title = "Other" });
            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneNames(456, It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                  .Returns(new List<string> { "Lab Mystery (2020)" });
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }

        [Test]
        public void should_reject_alias_mapped_to_another_tvdb_id()
        {
            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.FindTvdbId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(456);
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }

        [TestCase(SeriesTypes.Daily)]
        [TestCase(SeriesTypes.Anime)]
        public void should_reject_non_standard_series(SeriesTypes type)
        {
            _series.SeriesType = type;
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }

        [Test]
        public void should_reject_scene_numbering()
        {
            _series.UseSceneNumbering = true;
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }

        [Test]
        public void should_reject_multiple_files()
        {
            GivenFiles("/downloads/lab/Lab.Mystery.S01E02.mkv", "/downloads/lab/Lab.Mystery.S01E03.mkv");
            Subject.IsValid(_download, _series, _history).Should().BeFalse();
        }
    }
}
