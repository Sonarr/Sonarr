using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class TruncatedEpisodeTitlesFixture : CoreTest<FileNameBuilder>
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
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episodes = new List<Episode>
                        {
                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Episode Title 1")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.EpisodeNumber = 1)
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Another Episode Title")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.EpisodeNumber = 2)
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Yet Another Episode Title")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.EpisodeNumber = 3)
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Yet Another Episode Title Take 2")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.EpisodeNumber = 4)
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Yet Another Episode Title Take 3")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.EpisodeNumber = 5)
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Yet Another Episode Title Take 4")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.EpisodeNumber = 6)
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "A Really Really Really Really Long Episode Title")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.EpisodeNumber = 7)
                                            .Build()
                        };

            _episodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "SonarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        private void GivenProper()
        {
            _episodeFile.Quality.Revision.Version = 2;
        }

        [Test]
        public void should_truncate_with_extension()
        {
            _series.Title = "The Fantastic Life of Mr. Sisko";

            _episodes[0].SeasonNumber = 2;
            _episodes[0].EpisodeNumber = 18;
            _episodes[0].Title = "This title has to be 197 characters in length, combined with the series title, quality and episode number it becomes 254ish and the extension puts it above the 255 limit and triggers the truncation";
            _episodeFile.Quality.Quality = Quality.Bluray1080p;
            _episodes = _episodes.Take(1).ToList();
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}";

            var result = Subject.BuildFileName(_episodes, _series, _episodeFile, ".mkv");
            result.Length.Should().BeLessOrEqualTo(255);
            result.Should().Be("The Fantastic Life of Mr. Sisko - S02E18 - This title has to be 197 characters in length, combined with the series title, quality and episode number it becomes 254ish and the extension puts it above the 255 limit and triggers the trunc... Bluray-1080p.mkv");
        }

        [Test]
        public void should_truncate_with_ellipsis_between_first_and_last_episode_titles()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}";

            var result = Subject.BuildFileName(_episodes, _series, _episodeFile);
            result.Length.Should().BeLessOrEqualTo(255);
            result.Should().Be("Series Title - S01E01-02-03-04-05-06-07 - Episode Title 1...A Really Really Really Really Long Episode Title HDTV-720p");
        }

        [Test]
        public void should_truncate_with_ellipsis_if_only_first_episode_title_fits()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes";
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}";

            var result = Subject.BuildFileName(_episodes, _series, _episodeFile);
            result.Length.Should().BeLessOrEqualTo(255);
            result.Should().Be("Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes - S01E01-02-03-04-05-06-07 - Episode Title 1... HDTV-720p");
        }

        [Test]
        public void should_truncate_first_episode_title_with_ellipsis_if_only_partially_fits()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum";
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.Length.Should().BeLessOrEqualTo(255);
            result.Should().Be("Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum - S01E01 - Episode Ti... HDTV-720p");
        }

        [Test]
        public void should_truncate_titles_measuring_series_title_bytes()
        {
            _series.Title = "Lor\u00E9m ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum";
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.GetByteCount().Should().BeLessOrEqualTo(255);

            result.Should().Be("Lor\u00E9m ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum - S01E01 - Episode T... HDTV-720p");
        }

        [Test]
        public void should_truncate_titles_measuring_episode_title_bytes()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum";
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}";

            _episodes.First().Title = "Episod\u00E9 Title";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.GetByteCount().Should().BeLessOrEqualTo(255);

            result.Should().Be("Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum - S01E01 - Episod\u00E9 T... HDTV-720p");
        }

        [Test]
        public void should_truncate_titles_measuring_episode_title_bytes_middle()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum";
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}";

            _episodes.First().Title = "Episode T\u00E9tle";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.GetByteCount().Should().BeLessOrEqualTo(255);

            result.Should().Be("Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum - S01E01 - Episode T... HDTV-720p");
        }
    }
}
