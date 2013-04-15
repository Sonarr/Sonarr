/*
using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    
    public class EpisodeParseResultTest : CoreTest
    {
        [Test]
        public void tostring_single_season_episode()
        {
            var parseResult = new RemoteEpisode();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.EpisodeNumbers = new List<int> { 3 };
            parseResult.FullSeason = false;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(Quality.HDTV720p, false);


            parseResult.ToString().Should().Be("My Series - S12E03 HDTV-720p");
        }

        [Test]
        public void tostring_single_season_episode_proper()
        {
            var parseResult = new RemoteEpisode();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.EpisodeNumbers = new List<int> { 3 };
            parseResult.FullSeason = false;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(Quality.HDTV720p, true);


            parseResult.ToString().Should().Be("My Series - S12E03 HDTV-720p [proper]");
        }

        [Test]
        public void tostring_multi_season_episode()
        {
            var parseResult = new RemoteEpisode();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.EpisodeNumbers = new List<int> { 3, 4, 5 };
            parseResult.FullSeason = false;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(Quality.HDTV720p, false);


            parseResult.ToString().Should().Be("My Series - S12E03-04-05 HDTV-720p");
        }

        [Test]
        public void tostring_multi_season_episode_proper()
        {
            var parseResult = new RemoteEpisode();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.EpisodeNumbers = new List<int> { 3, 4, 5 };
            parseResult.FullSeason = false;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(Quality.HDTV720p, true);


            parseResult.ToString().Should().Be("My Series - S12E03-04-05 HDTV-720p [proper]");
        }


        [Test]
        public void tostring_full_season()
        {
            var parseResult = new RemoteEpisode();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.FullSeason = true;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(Quality.HDTV720p, false);


            parseResult.ToString().Should().Be("My Series - Season 12 HDTV-720p");
        }


        [Test]
        public void tostring_full_season_proper()
        {
            var parseResult = new RemoteEpisode();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.FullSeason = true;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(Quality.HDTV720p, true);


            parseResult.ToString().Should().Be("My Series - Season 12 HDTV-720p [proper]");
        }

        [Test]
        public void tostring_daily_show()
        {
            var parseResult = new RemoteEpisode();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.FullSeason = true;
            parseResult.AirDate = new DateTime(2010, 12, 30);
            parseResult.Quality = new QualityModel(Quality.HDTV720p, false);


            parseResult.ToString().Should().Be("My Series - 2010-12-30 HDTV-720p");
        }

        [Test]
        public void tostring_daily_show_proper()
        {
            var parseResult = new RemoteEpisode();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.FullSeason = true;
            parseResult.AirDate = new DateTime(2010, 12, 30);
            parseResult.Quality = new QualityModel(Quality.HDTV720p, true);


            parseResult.ToString().Should().Be("My Series - 2010-12-30 HDTV-720p [proper]");
        }



        public static readonly object[] SabNamingCases =
        {
            new object[] { 1, new[] { 2 }, "My Episode Title", Quality.DVD, false, "My Series Name - 1x02 - My Episode Title [DVD]" },
            new object[] { 1, new[] { 2 }, "My Episode Title", Quality.DVD, true, "My Series Name - 1x02 - My Episode Title [DVD] [Proper]" },
            new object[] { 1, new[] { 2 }, "", Quality.DVD, true, "My Series Name - 1x02 -  [DVD] [Proper]" },
            new object[] { 1, new[] { 2, 4 }, "My Episode Title", Quality.HDTV720p, false, "My Series Name - 1x02-1x04 - My Episode Title [HDTV-720p]" },
            new object[] { 1, new[] { 2, 4 }, "My Episode Title", Quality.HDTV720p, true, "My Series Name - 1x02-1x04 - My Episode Title [HDTV-720p] [Proper]" },
            new object[] { 1, new[] { 2, 4 }, "", Quality.HDTV720p, true, "My Series Name - 1x02-1x04 -  [HDTV-720p] [Proper]" },
        };


        [Test, TestCaseSource("SabNamingCases")]
        public void create_proper_sab_titles(int seasons, int[] episodes, string title, Quality quality, bool proper, string expected)
        {
            var series = Builder<Series>.CreateNew()
                    .With(c => c.Title = "My Series Name")
                    .Build();

            var fakeEpisodes = new List<Episode>();

            foreach (var episode in episodes)
                fakeEpisodes.Add(Builder<Episode>
                    .CreateNew()
                    .With(e => e.EpisodeNumber = episode)
                    .With(e => e.Title = title)
                    .Build());

            var parsResult = new RemoteEpisode()
            {
                AirDate = DateTime.Now,
                EpisodeNumbers = episodes.ToList(),
                Quality = new QualityModel(quality, proper),
                SeasonNumber = seasons,
                Series = series,
                EpisodeTitle = title,
                Episodes = fakeEpisodes
            };

            parsResult.GetDownloadTitle().Should().Be(expected);
        }

        [TestCase(true, Result = "My Series Name - Season 1 [Bluray720p] [Proper]")]
        [TestCase(false, Result = "My Series Name - Season 1 [Bluray720p]")]
        public string create_proper_sab_season_title(bool proper)
        {
            var series = Builder<Series>.CreateNew()
                                .With(c => c.Title = "My Series Name")
                                .Build();

            var parsResult = new RemoteEpisode()
            {
                AirDate = DateTime.Now,
                Quality = new QualityModel(Quality.Bluray720p, proper),
                SeasonNumber = 1,
                Series = series,
                EpisodeTitle = "My Episode Title",
                FullSeason = true
            };

            return parsResult.GetDownloadTitle();
        }

        [TestCase(true, Result = "My Series Name - 2011-12-01 - My Episode Title [Bluray720p] [Proper]")]
        [TestCase(false, Result = "My Series Name - 2011-12-01 - My Episode Title [Bluray720p]")]
        public string create_proper_sab_daily_titles(bool proper)
        {
            var series = Builder<Series>.CreateNew()
                    .With(c => c.SeriesType = SeriesTypes.Daily)
                    .With(c => c.Title = "My Series Name")
                    .Build();

            var episode = Builder<Episode>.CreateNew()
                    .With(e => e.Title = "My Episode Title")
                    .Build();

            var parsResult = new RemoteEpisode
            {
                AirDate = new DateTime(2011, 12, 1),
                Quality = new QualityModel(Quality.Bluray720p, proper),
                Series = series,
                EpisodeTitle = "My Episode Title",
                Episodes = new List<Episode> { episode }
            };

            return parsResult.GetDownloadTitle();
        }

        [Test]
        public void should_not_repeat_the_same_episode_title()
        {
            var series = Builder<Series>.CreateNew()
                    .With(c => c.Title = "My Series Name")
                    .Build();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(2)
                    .All()
                    .With(e => e.SeasonNumber = 5)
                    .TheFirst(1)
                    .With(e => e.Title = "My Episode Title (1)")
                    .TheLast(1)
                    .With(e => e.Title = "My Episode Title (2)")
                    .Build();

            var parsResult = new RemoteEpisode
            {
                AirDate = DateTime.Now,
                EpisodeNumbers = new List<int> { 10, 11 },
                Quality = new QualityModel(Quality.HDTV720p, false),
                SeasonNumber = 35,
                Series = series,
                Episodes = fakeEpisodes
            };

            parsResult.GetDownloadTitle().Should().Be("My Series Name - 5x01-5x02 - My Episode Title [HDTV-720p]");
        }

    }
}
*/
