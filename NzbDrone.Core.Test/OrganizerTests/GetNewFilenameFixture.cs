using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class FileNameBuilderFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;

        private NamingConfig namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();


            namingConfig = new NamingConfig();
            namingConfig.RenameEpisodes = true;


            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(namingConfig);


        }

        [Test]
        public void GetNewFilename_Series_Episode_Quality_S01E05_Dash()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("South Park - S15E06 - City Sushi [HDTV-720p]");
        }

        [Test]
        public void GetNewFilename_Episode_Quality_1x05_Dash()
        {
            namingConfig.IncludeSeriesTitle = false;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 0;
            namingConfig.ReplaceSpaces = false;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("15x06 - City Sushi [HDTV-720p]");
        }

        [Test]
        public void GetNewFilename_Series_Quality_01x05_Space()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = false;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = " ";
            namingConfig.NumberStyle = 1;
            namingConfig.ReplaceSpaces = false;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("South Park 05x06 [HDTV-720p]");
        }

        [Test]
        public void GetNewFilename_Series_s01e05_Space()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = false;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " ";
            namingConfig.NumberStyle = 3;
            namingConfig.ReplaceSpaces = false;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("South Park s05e06");
        }

        [Test]
        public void GetNewFilename_Series_Episode_s01e05_Periods()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " ";
            namingConfig.NumberStyle = 3;
            namingConfig.ReplaceSpaces = true;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("South.Park.s05e06.City.Sushi");
        }

        [Test]
        public void GetNewFilename_Series_Episode_s01e05_Dash_Periods_Quality()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 3;
            namingConfig.ReplaceSpaces = true;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("South.Park.-.s05e06.-.City.Sushi.[HDTV-720p]");
        }

        [Test]
        public void GetNewFilename_S01E05_Dash()
        {
            namingConfig.IncludeSeriesTitle = false;
            namingConfig.IncludeEpisodeTitle = false;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;


            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("S15E06");
        }

        [Test]
        public void GetNewFilename_multi_Series_Episode_Quality_S01E05_Scene_Dash()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;
            namingConfig.MultiEpisodeStyle = 3;

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episodeOne, episodeTwo }, new Series { Title = "The Mentalist" }, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("The Mentalist - S03E23-E24 - Strawberries and Cream [HDTV-720p]");
        }

        [Test]
        public void GetNewFilename_multi_Episode_Quality_1x05_Repeat_Dash()
        {
            namingConfig.IncludeSeriesTitle = false;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 0;
            namingConfig.ReplaceSpaces = false;
            namingConfig.MultiEpisodeStyle = 2;

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episodeOne, episodeTwo }, new Series { Title = "The Mentalist" }, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("3x23x24 - Strawberries and Cream [HDTV-720p]");
        }

        [Test]
        public void GetNewFilename_multi_Episode_Quality_01x05_Repeat_Space()
        {
            namingConfig.IncludeSeriesTitle = false;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = " ";
            namingConfig.NumberStyle = 0;
            namingConfig.ReplaceSpaces = false;
            namingConfig.MultiEpisodeStyle = 2;

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episodeOne, episodeTwo }, new Series { Title = "The Mentalist" }, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("3x23x24 Strawberries and Cream [HDTV-720p]");
        }

        [Test]
        public void GetNewFilename_multi_Series_Episode_s01e05_Duplicate_Period()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " ";
            namingConfig.NumberStyle = 3;
            namingConfig.ReplaceSpaces = true;
            namingConfig.MultiEpisodeStyle = 1;

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episodeOne, episodeTwo }, new Series { Title = "The Mentalist" }, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("The.Mentalist.s03e23.s03e24.Strawberries.and.Cream");
        }

        [Test]
        public void GetNewFilename_multi_Series_S01E05_Extend_Dash_Period()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = false;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = true;
            namingConfig.MultiEpisodeStyle = 0;

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episodeOne, episodeTwo }, new Series { Title = "The Mentalist" }, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("The.Mentalist.-.S03E23-24");
        }

        [Test]
        public void GetNewFilename_multi_1x05_Repeat_Dash_Period()
        {
            namingConfig.IncludeSeriesTitle = false;
            namingConfig.IncludeEpisodeTitle = false;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 0;
            namingConfig.ReplaceSpaces = true;
            namingConfig.MultiEpisodeStyle = 2;

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episodeOne, episodeTwo }, new Series { Title = "The Mentalist" }, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("3x23x24");
        }

        [Test]
        public void GetNewFilename_should_append_proper_when_proper_and_append_quality_is_true()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p, true) });


            result.Should().Be("South Park - S15E06 - City Sushi [HDTV-720p] [Proper]");
        }

        [Test]
        public void GetNewFilename_should_not_append_proper_when_not_proper_and_append_quality_is_true()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("South Park - S15E06 - City Sushi [HDTV-720p]");
        }

        [Test]
        public void GetNewFilename_should_not_append_proper_when_proper_and_append_quality_is_false()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p, true) });


            result.Should().Be("South Park - S15E06 - City Sushi");
        }

        [Test]
        public void GetNewFilename_should_order_multiple_episode_files_in_numerical_order()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;
            namingConfig.MultiEpisodeStyle = 3;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Hey, Baby, What's Wrong? (1)")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            var episode2 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Hey, Baby, What's Wrong? (2)")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 7)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode2, episode }, new Series { Title = "30 Rock" }, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("30 Rock - S06E06-E07 - Hey, Baby, What's Wrong!");
        }

        [Test]
        public void GetNewFilename_Series_Episode_Quality_S01E05_Period()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = ".";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("South Park.S15E06.City Sushi [HDTV-720p]");
        }

        [Test]
        public void GetNewFilename_Episode_Quality_1x05_Period()
        {
            namingConfig.IncludeSeriesTitle = false;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = "."; ;
            namingConfig.NumberStyle = 0;
            namingConfig.ReplaceSpaces = false;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("15x06.City Sushi [HDTV-720p]");
        }

        [Test]
        public void GetNewFilename_UseSceneName_when_sceneName_isNull()
        {
            namingConfig.IncludeSeriesTitle = false;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = "."; ;
            namingConfig.NumberStyle = 0;
            namingConfig.ReplaceSpaces = false;
            namingConfig.RenameEpisodes = false;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                    .With(e => e.SceneName = null)
                    .With(e => e.Path = @"C:\Test\TV\30 Rock - S01E01 - Test")
                    .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, episodeFile);


            result.Should().Be(Path.GetFileNameWithoutExtension(episodeFile.Path));
        }

        [Test]
        public void GetNewFilename_UseSceneName_when_sceneName_isNotNull()
        {
            namingConfig.IncludeSeriesTitle = false;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = ".";
            namingConfig.NumberStyle = 0;
            namingConfig.ReplaceSpaces = false;
            namingConfig.RenameEpisodes = false;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                    .With(e => e.SceneName = "30.Rock.S01E01.xvid-LOL")
                    .With(e => e.Path = @"C:\Test\TV\30 Rock - S01E01 - Test")
                    .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode }, _series, episodeFile);


            result.Should().Be(episodeFile.SceneName);
        }

        [Test]
        public void should_only_have_one_episodeTitle_when_episode_titles_are_the_same()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;
            namingConfig.MultiEpisodeStyle = 3;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Hey, Baby, What's Wrong? (1)")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            var episode2 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Hey, Baby, What's Wrong? (2)")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 7)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode2, episode }, new Series { Title = "30 Rock" }, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("30 Rock - S06E06-E07 - Hey, Baby, What's Wrong!");
        }

        [Test]
        public void should_have_two_episodeTitles_when_episode_titles_are_not_the_same()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;
            namingConfig.MultiEpisodeStyle = 3;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Hello")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            var episode2 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "World")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 7)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode2, episode }, new Series { Title = "30 Rock" }, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("30 Rock - S06E06-E07 - Hello + World");
        }

        [Test]
        public void should_have_two_episodeTitles_when_distinct_count_is_two()
        {
            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;
            namingConfig.MultiEpisodeStyle = 3;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Hello (3)")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            var episode2 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Hello (2)")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 7)
                            .Build();

            var episode3 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "World")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 8)
                            .Build();


            string result = Subject.BuildFilename(new List<Episode> { episode, episode2, episode3 }, new Series { Title = "30 Rock" }, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });


            result.Should().Be("30 Rock - S06E06-E07-E08 - Hello + World");
        }

        [Test]
        public void should_use_airDate_if_series_isDaily()
        {

            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = true;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;

            var series = Builder<Series>
                    .CreateNew()
                    .With(s => s.SeriesType = SeriesTypes.Daily)
                    .With(s => s.Title = "The Daily Show with Jon Stewart")
                    .Build();

            var episodes = Builder<Episode>
                    .CreateListOfSize(1)
                    .All()
                    .With(e => e.AirDate = new DateTime(2012, 12, 13))
                    .With(e => e.Title = "Kristen Stewart")
                    .Build();

            var result = Subject
                               .BuildFilename(episodes, series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });
            result.Should().Be("The Daily Show with Jon Stewart - 2012-12-13 - Kristen Stewart [HDTV-720p]");
        }

        [Test]
        public void should_use_airDate_if_series_isDaily_no_episode_title()
        {

            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = false;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;

            var series = Builder<Series>
                    .CreateNew()
                    .With(s => s.SeriesType = SeriesTypes.Daily)
                    .With(s => s.Title = "The Daily Show with Jon Stewart")
                    .Build();

            var episodes = Builder<Episode>
                    .CreateListOfSize(1)
                    .All()
                    .With(e => e.AirDate = new DateTime(2012, 12, 13))
                    .With(e => e.Title = "Kristen Stewart")
                    .Build();

            var result = Subject
                               .BuildFilename(episodes, series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });
            result.Should().Be("The Daily Show with Jon Stewart - 2012-12-13");
        }

        [Test]
        public void should_set_airdate_to_unknown_if_not_available()
        {

            namingConfig.IncludeSeriesTitle = true;
            namingConfig.IncludeEpisodeTitle = true;
            namingConfig.IncludeQuality = false;
            namingConfig.Separator = " - ";
            namingConfig.NumberStyle = 2;
            namingConfig.ReplaceSpaces = false;

            var series = Builder<Series>
                    .CreateNew()
                    .With(s => s.SeriesType = SeriesTypes.Daily)
                    .With(s => s.Title = "The Daily Show with Jon Stewart")
                    .Build();

            var episodes = Builder<Episode>
                    .CreateListOfSize(1)
                    .All()
                    .With(e => e.AirDate = null)
                    .With(e => e.Title = "Kristen Stewart")
                    .Build();

            var result = Subject
                               .BuildFilename(episodes, series, new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) });
            result.Should().Be("The Daily Show with Jon Stewart - Unknown - Kristen Stewart");
        }
    }
}