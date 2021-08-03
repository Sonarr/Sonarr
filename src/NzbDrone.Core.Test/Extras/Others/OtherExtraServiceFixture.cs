using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Others
{
    [TestFixture]
    public class OtherExtraServiceFixture : CoreTest<OtherExtraService>
    {
        private Series _series;
        private EpisodeFile _episodeFile;
        private LocalEpisode _localEpisode;

        private string _seriesFolder;
        private string _episodeFolder;

        [SetUp]
        public void Setup()
        {
            _seriesFolder = @"C:\Test\TV\Series Title".AsOsAgnostic();
            _episodeFolder = @"C:\Test\Unsorted TV\Series.Title.S01".AsOsAgnostic();

            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Path = _seriesFolder)
                                     .Build();

            var episodes = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .Build()
                                           .ToList();

            _episodeFile = Builder<EpisodeFile>.CreateNew()
                                               .With(f => f.Path = Path.Combine(_series.Path, "Season 1", "Series Title - S01E01.mkv").AsOsAgnostic())
                                               .With(f => f.RelativePath = @"Season 1\Series Title - S01E01.mkv")
                                               .Build();

            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Series = _series)
                                                 .With(l => l.Episodes = episodes)
                                                 .With(l => l.Path = Path.Combine(_episodeFolder, "Series.Title.S01E01.mkv").AsOsAgnostic())
                                                 .With(l => l.FileEpisodeInfo = new ParsedEpisodeInfo
                                                 {
                                                     SeasonNumber = 1,
                                                     EpisodeNumbers = new[] { 1 }
                                                 })
                                                 .Build();
        }

        [Test]
        [TestCase("Series Title - S01E01.nfo", "Series Title - S01E01.nfo")]
        [TestCase("Series.Title.S01E01.nfo", "Series Title - S01E01.nfo")]
        [TestCase("Series-Title-S01E01.nfo", "Series Title - S01E01.nfo")]
        [TestCase("Series Title S01E01.nfo", "Series Title - S01E01.nfo")]
        [TestCase("Series_Title_S01E01.nfo", "Series Title - S01E01.nfo")]
        [TestCase("S01E01.thumb.jpg", "Series Title - S01E01.jpg")]
        [TestCase(@"Series.Title.S01E01\thumb.jpg", "Series Title - S01E01.jpg")]
        public void should_import_matching_file(string filePath, string expectedOutputPath)
        {
            var files = new List<string> { Path.Combine(_episodeFolder, filePath).AsOsAgnostic() };

            var results = Subject.ImportFiles(_localEpisode, _episodeFile, files, true).ToList();

            results.Count().Should().Be(1);

            results[0].RelativePath.AsOsAgnostic().PathEquals(Path.Combine("Season 1", expectedOutputPath).AsOsAgnostic()).Should().Be(true);
        }

        [Test]
        public void should_not_import_multiple_nfo_files()
        {
            var files = new List<string>
            {
                Path.Combine(_episodeFolder, "Series.Title.S01E01.nfo").AsOsAgnostic(),
                Path.Combine(_episodeFolder, "Series_Title_S01E01.nfo").AsOsAgnostic(),
            };

            var results = Subject.ImportFiles(_localEpisode, _episodeFile, files, true).ToList();

            results.Count().Should().Be(1);
        }
    }
}
