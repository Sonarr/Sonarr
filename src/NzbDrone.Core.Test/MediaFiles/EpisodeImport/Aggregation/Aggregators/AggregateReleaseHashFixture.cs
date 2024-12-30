using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using Workarr.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using Workarr.Parser;
using Workarr.Parser.Model;
using Workarr.Tv;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateReleaseHashFixture : CoreTest<AggregateReleaseHash>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew().Build();
        }

        [Test]
        public void should_prefer_file()
        {
            var fileEpisodeInfo = Parser.ParseTitle("[DHD] Series Title! - 08 (1280x720 10bit AAC) [ABCDEFGH]");
            var folderEpisodeInfo = Parser.ParseTitle("[DHD] Series Title! - 08 [12345678]");
            var downloadClientEpisodeInfo = Parser.ParseTitle("[DHD] Series Title! - 08 (1280x720 10bit AAC) [ABCD1234]");
            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                FolderEpisodeInfo = folderEpisodeInfo,
                DownloadClientEpisodeInfo = downloadClientEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.S01\Series.Title.S01E01.mkv".AsOsAgnostic(),
                Series = _series
            };

            Subject.Aggregate(localEpisode, null);

            localEpisode.ReleaseHash.Should().Be("ABCDEFGH");
        }

        [Test]
        public void should_fallback_to_downloadclient()
        {
            var fileEpisodeInfo = Parser.ParseTitle("[DHD] Series Title! - 08 (1280x720 10bit AAC)");
            var downloadClientEpisodeInfo = Parser.ParseTitle("[DHD] Series Title! - 08 (1280x720 10bit AAC) [ABCD1234]");
            var folderEpisodeInfo = Parser.ParseTitle("[DHD] Series Title! - 08 [12345678]");
            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                FolderEpisodeInfo = folderEpisodeInfo,
                DownloadClientEpisodeInfo = downloadClientEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.S01\Series.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Series = _series
            };

            Subject.Aggregate(localEpisode, null);

            localEpisode.ReleaseHash.Should().Be("ABCD1234");
        }

        [Test]
        public void should_fallback_to_folder()
        {
            var fileEpisodeInfo = Parser.ParseTitle("[DHD] Series Title! - 08 (1280x720 10bit AAC)");
            var downloadClientEpisodeInfo = Parser.ParseTitle("[DHD] Series Title! - 08 (1280x720 10bit AAC)");
            var folderEpisodeInfo = Parser.ParseTitle("[DHD] Series Title! - 08 [12345678]");
            var localEpisode = new LocalEpisode
            {
                FileEpisodeInfo = fileEpisodeInfo,
                FolderEpisodeInfo = folderEpisodeInfo,
                DownloadClientEpisodeInfo = downloadClientEpisodeInfo,
                Path = @"C:\Test\Unsorted TV\Series.Title.S01\Series.Title.S01E01.WEB-DL.mkv".AsOsAgnostic(),
                Series = _series
            };

            Subject.Aggregate(localEpisode, null);

            localEpisode.ReleaseHash.Should().Be("12345678");
        }
    }
}
