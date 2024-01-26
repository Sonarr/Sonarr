using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class TruncatedReleaseGroupFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private List<Episode> _episodes;
        private EpisodeFile _episodeFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "Series Title")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.MultiEpisodeStyle = 0;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episodes = new List<Episode>
                        {
                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Episode Title 1")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.EpisodeNumber = 1)
                                            .Build()
                        };

            _episodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "SonarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        private void GivenProper()
        {
            _episodeFile.Quality.Revision.Version = 2;
        }

        [Test]
        public void should_truncate_from_beginning()
        {
            _series.Title = "The Fantastic Life of Mr. Sisko";

            _episodeFile.Quality.Quality = Quality.Bluray1080p;
            _episodeFile.ReleaseGroup = "IWishIWasALittleBitTallerIWishIWasABallerIWishIHadAGirlWhoLookedGoodIWouldCallHerIWishIHadARabbitInAHatWithABatAndASixFourImpala";
            _episodes = _episodes.Take(1).ToList();
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}-{ReleaseGroup:12}";

            var result = Subject.BuildFileName(_episodes, _series, _episodeFile, ".mkv");
            result.Length.Should().BeLessOrEqualTo(255);
            result.Should().Be("The Fantastic Life of Mr. Sisko - S01E01 - Episode Title 1 Bluray-1080p-IWishIWas....mkv");
        }

        [Test]
        public void should_truncate_from_from_end()
        {
            _series.Title = "The Fantastic Life of Mr. Sisko";

            _episodeFile.Quality.Quality = Quality.Bluray1080p;
            _episodeFile.ReleaseGroup = "IWishIWasALittleBitTallerIWishIWasABallerIWishIHadAGirlWhoLookedGoodIWouldCallHerIWishIHadARabbitInAHatWithABatAndASixFourImpala";
            _episodes = _episodes.Take(1).ToList();
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}-{ReleaseGroup:-17}";

            var result = Subject.BuildFileName(_episodes, _series, _episodeFile, ".mkv");
            result.Length.Should().BeLessOrEqualTo(255);
            result.Should().Be("The Fantastic Life of Mr. Sisko - S01E01 - Episode Title 1 Bluray-1080p-...ASixFourImpala.mkv");
        }
    }
}
