using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class MultiSeasonPackSpecificationFixture : CoreTest<MultiSeasonPackSpecification>
    {
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _localEpisode = new LocalEpisode
            {
                Path = @"C:\Test\Series.Title.S01-S03\Series.Title.S02E05.mkv".AsOsAgnostic(),
                Series = Builder<Series>.CreateNew().With(s => s.SeriesType = SeriesTypes.Standard).Build(),
                DownloadClientEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01-S03.1080p.BluRay.x264-RlsGrp"),
                FileEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S02E05.1080p.BluRay.x264-RlsGrp")
            };
        }

        private void GivenFileName(string fileName)
        {
            _localEpisode.FileEpisodeInfo = Parser.Parser.ParseTitle(fileName);
        }

        [Test]
        public void should_accept_if_download_is_not_multi_season()
        {
            _localEpisode.DownloadClientEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01.1080p.BluRay.x264-RlsGrp");
            _localEpisode.FileEpisodeInfo = null;

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_file_has_season_number()
        {
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_existing_file()
        {
            _localEpisode.ExistingFile = true;
            _localEpisode.FileEpisodeInfo = null;

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_if_file_could_not_be_parsed()
        {
            _localEpisode.FileEpisodeInfo = null;

            var result = Subject.IsSatisfiedBy(_localEpisode, null);

            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.MultiSeason);
        }

        [Test]
        public void should_reject_if_folder_is_multi_season_and_file_has_no_season()
        {
            _localEpisode.DownloadClientEpisodeInfo = null;
            _localEpisode.FolderEpisodeInfo = Parser.Parser.ParseTitle("Series.Title.S01-S03.1080p.BluRay.x264-RlsGrp");
            GivenFileName("Series.Title.E05.1080p.BluRay.x264-RlsGrp");

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_reject_if_file_season_was_assumed_for_mini_series()
        {
            GivenFileName("Series.Title.E05.1080p.BluRay.x264-RlsGrp");

            _localEpisode.FileEpisodeInfo.IsMiniSeries.Should().BeTrue();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_reject_absolute_numbered_file_for_standard_series()
        {
            GivenFileName("[RlsGrp] Series Title - 05 [1080p]");

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_absolute_numbered_file_for_anime_series()
        {
            _localEpisode.Series.SeriesType = SeriesTypes.Anime;
            GivenFileName("[RlsGrp] Series Title - 05 [1080p]");

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_daily_file()
        {
            GivenFileName("Series.Title.2024.03.05.1080p.WEB-DL.x264-RlsGrp");

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }
    }
}
