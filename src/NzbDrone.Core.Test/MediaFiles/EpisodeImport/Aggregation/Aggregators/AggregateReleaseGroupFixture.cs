using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateReleaseGroupFixture : CoreTest<AggregateReleaseGroup>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew().Build();
        }

        [Test]
        public void should_not_use_downloadclient_for_full_season()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.WEB-DL-Wizzy");
            var downloadClientEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01.WEB-DL-Viva");
            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                DownloadClientEpisodeInfo = downloadClientEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.S01\Series.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Series = _series
            };

            Subject.Aggregate(localEpisode, null);

            localEpisode.ReleaseGroup.Should().Be("Wizzy");
        }

        [Test]
        public void should_not_use_folder_for_full_season()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.WEB-DL-Wizzy");
            var folderEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01.WEB-DL-Drone");
            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                FolderEpisodeInfo = folderEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.S01\Series.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Series = _series
            };

            Subject.Aggregate(localEpisode, null);

            localEpisode.ReleaseGroup.Should().Be("Wizzy");
        }

        [Test]
        public void should_prefer_downloadclient()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.WEB-DL-Wizzy");
            var folderEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.WEB-DL-Drone");
            var downloadClientEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.WEB-DL-Viva");
            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                FolderEpisodeInfo = folderEpisodeInfo,
                DownloadClientEpisodeInfo = downloadClientEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.S01\Series.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Series = _series
            };

            Subject.Aggregate(localEpisode, null);

            localEpisode.ReleaseGroup.Should().Be("Viva");
        }

        [Test]
        public void should_prefer_folder()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.WEB-DL-Wizzy");
            var folderEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.WEB-DL-Drone");
            var downloadClientEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.WEB-DL");
            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                FolderEpisodeInfo = folderEpisodeInfo,
                DownloadClientEpisodeInfo = downloadClientEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.S01\Series.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Series = _series
            };

            Subject.Aggregate(localEpisode, null);

            localEpisode.ReleaseGroup.Should().Be("Drone");
        }

        [Test]
        public void should_fallback_to_file()
        {
            var fileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.WEB-DL-Wizzy");
            var folderEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.WEB-DL");
            var downloadClientEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.WEB-DL");
            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                FolderEpisodeInfo = folderEpisodeInfo,
                DownloadClientEpisodeInfo = downloadClientEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.S01\Series.Title.S01E01.mkv".AsOsAgnostic(),
                Series = _series
            };

            Subject.Aggregate(localEpisode, null);

            localEpisode.ReleaseGroup.Should().Be("Wizzy");
        }
    }
}
