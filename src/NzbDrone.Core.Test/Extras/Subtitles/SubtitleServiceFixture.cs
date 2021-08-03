using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Subtitles;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Subtitles
{
    [TestFixture]
    public class SubtitleServiceFixture : CoreTest<SubtitleService>
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
                                               .With(f => f.RelativePath = @"Season 1\Series Title - S01E01.mkv".AsOsAgnostic())
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

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetParentFolder(It.IsAny<string>()))
                  .Returns((string path) => Directory.GetParent(path).FullName);

            Mocker.GetMock<IDetectSample>().Setup(s => s.IsSample(It.IsAny<Series>(), It.IsAny<string>(), It.IsAny<bool>()))
                  .Returns(DetectSampleResult.NotSample);
        }

        [Test]
        [TestCase("Series.Title.S01E01.en.nfo")]
        public void should_not_import_non_subtitle_file(string filePath)
        {
            var files = new List<string> { Path.Combine(_episodeFolder, filePath).AsOsAgnostic() };

            var results = Subject.ImportFiles(_localEpisode, _episodeFile, files, true).ToList();

            results.Count().Should().Be(0);
        }

        [Test]
        [TestCase("Series Title - S01E01.srt", "Series Title - S01E01.srt")]
        [TestCase("Series.Title.S01E01.en.srt", "Series Title - S01E01.en.srt")]
        [TestCase("Series.Title.S01E01.english.srt", "Series Title - S01E01.en.srt")]
        [TestCase("Series-Title-S01E01-fr-cc.srt", "Series Title - S01E01.fr.srt")]
        [TestCase("Series Title S01E01_en_sdh_forced.srt", "Series Title - S01E01.en.srt")]
        [TestCase("Series_Title_S01E01 en.srt", "Series Title - S01E01.en.srt")]
        [TestCase(@"Subs\S01E01.en.srt", "Series Title - S01E01.en.srt")]
        [TestCase(@"Subs\Series.Title.S01E01\2_en.srt", "Series Title - S01E01.en.srt")]
        public void should_import_matching_subtitle_file(string filePath, string expectedOutputPath)
        {
            var files = new List<string> { Path.Combine(_episodeFolder, filePath).AsOsAgnostic() };

            var results = Subject.ImportFiles(_localEpisode, _episodeFile, files, true).ToList();

            results.Count().Should().Be(1);

            results[0].RelativePath.AsOsAgnostic().PathEquals(Path.Combine("Season 1", expectedOutputPath).AsOsAgnostic()).Should().Be(true);
        }

        [Test]
        public void should_import_multiple_subtitle_files_per_language()
        {
            var files = new List<string>
            {
                Path.Combine(_episodeFolder, "Series.Title.S01E01.en.srt").AsOsAgnostic(),
                Path.Combine(_episodeFolder, "Series.Title.S01E01.english.srt").AsOsAgnostic(),
                Path.Combine(_episodeFolder, "Subs", "Series_Title_S01E01_en_forced.srt").AsOsAgnostic(),
                Path.Combine(_episodeFolder, "Subs", "Series.Title.S01E01", "2_fr.srt").AsOsAgnostic()
            };

            var expectedOutputs = new string[]
            {
                "Series Title - S01E01.1.en.srt",
                "Series Title - S01E01.2.en.srt",
                "Series Title - S01E01.3.en.srt",
                "Series Title - S01E01.fr.srt",
            };

            var results = Subject.ImportFiles(_localEpisode, _episodeFile, files, true).ToList();

            results.Count().Should().Be(expectedOutputs.Length);

            for (int i = 0; i < expectedOutputs.Length; i++)
            {
                results[i].RelativePath.AsOsAgnostic().PathEquals(Path.Combine("Season 1", expectedOutputs[i]).AsOsAgnostic()).Should().Be(true);
            }
        }

        [Test]
        [TestCase("sub.srt", "Series Title - S01E01.srt")]
        [TestCase(@"Subs\2_en.srt", "Series Title - S01E01.en.srt")]
        public void should_import_unmatching_subtitle_file_if_only_episode(string filePath, string expectedOutputPath)
        {
            var subtitleFile = Path.Combine(_episodeFolder, filePath).AsOsAgnostic();

            var sampleFile = Path.Combine(_series.Path, "Season 1", "Series Title - S01E01.sample.mkv").AsOsAgnostic();

            var videoFiles = new string[]
            {
                _localEpisode.Path,
                sampleFile
            };

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetFiles(It.IsAny<string>(), SearchOption.AllDirectories))
                  .Returns(videoFiles);

            Mocker.GetMock<IDetectSample>().Setup(s => s.IsSample(It.IsAny<Series>(), sampleFile, It.IsAny<bool>()))
                  .Returns(DetectSampleResult.Sample);

            var results = Subject.ImportFiles(_localEpisode, _episodeFile, new List<string> { subtitleFile }, true).ToList();

            results.Count().Should().Be(1);

            results[0].RelativePath.AsOsAgnostic().PathEquals(Path.Combine("Season 1", expectedOutputPath).AsOsAgnostic()).Should().Be(true);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        [TestCase("sub.srt")]
        [TestCase(@"Subs\2_en.srt")]
        public void should_not_import_unmatching_subtitle_file_if_multiple_episodes(string filePath)
        {
            var subtitleFile = Path.Combine(_episodeFolder, filePath).AsOsAgnostic();

            var videoFiles = new string[]
            {
                _localEpisode.Path,
                Path.Combine(_series.Path, "Season 1", "Series Title - S01E01.sample.mkv").AsOsAgnostic()
            };

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetFiles(It.IsAny<string>(), SearchOption.AllDirectories))
                  .Returns(videoFiles);

            var results = Subject.ImportFiles(_localEpisode, _episodeFile, new List<string> { subtitleFile }, true).ToList();

            results.Count().Should().Be(0);
        }
    }
}
