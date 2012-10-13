using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EpisodeParseResultTest : CoreTest
    {
        [Test]
        public void tostring_single_season_episode()
        {
            var parseResult = new EpisodeParseResult();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.EpisodeNumbers = new List<int> { 3 };
            parseResult.FullSeason = false;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(QualityTypes.HDTV, false);


            parseResult.ToString().Should().Be("My Series - S12E03 HDTV");
        }

        [Test]
        public void tostring_single_season_episode_proper()
        {
            var parseResult = new EpisodeParseResult();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.EpisodeNumbers = new List<int> { 3 };
            parseResult.FullSeason = false;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(QualityTypes.HDTV, true);


            parseResult.ToString().Should().Be("My Series - S12E03 HDTV [proper]");
        }

        [Test]
        public void tostring_multi_season_episode()
        {
            var parseResult = new EpisodeParseResult();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.EpisodeNumbers = new List<int> { 3, 4, 5 };
            parseResult.FullSeason = false;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(QualityTypes.HDTV, false);


            parseResult.ToString().Should().Be("My Series - S12E03-04-05 HDTV");
        }

        [Test]
        public void tostring_multi_season_episode_proper()
        {
            var parseResult = new EpisodeParseResult();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.EpisodeNumbers = new List<int> { 3, 4, 5 };
            parseResult.FullSeason = false;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(QualityTypes.HDTV, true);


            parseResult.ToString().Should().Be("My Series - S12E03-04-05 HDTV [proper]");
        }


        [Test]
        public void tostring_full_season()
        {
            var parseResult = new EpisodeParseResult();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.FullSeason = true;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(QualityTypes.HDTV, false);


            parseResult.ToString().Should().Be("My Series - Season 12 HDTV");
        }


        [Test]
        public void tostring_full_season_proper()
        {
            var parseResult = new EpisodeParseResult();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.FullSeason = true;
            parseResult.AirDate = null;
            parseResult.Quality = new QualityModel(QualityTypes.HDTV, true);


            parseResult.ToString().Should().Be("My Series - Season 12 HDTV [proper]");
        }

        [Test]
        public void tostring_daily_show()
        {
            var parseResult = new EpisodeParseResult();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.FullSeason = true;
            parseResult.AirDate = new DateTime(2010, 12, 30);
            parseResult.Quality = new QualityModel(QualityTypes.HDTV, false);


            parseResult.ToString().Should().Be("My Series - 2010-12-30 HDTV");
        }

        [Test]
        public void tostring_daily_show_proper()
        {
            var parseResult = new EpisodeParseResult();
            parseResult.SeriesTitle = "My Series";
            parseResult.SeasonNumber = 12;
            parseResult.FullSeason = true;
            parseResult.AirDate = new DateTime(2010, 12, 30);
            parseResult.Quality = new QualityModel(QualityTypes.HDTV, true);


            parseResult.ToString().Should().Be("My Series - 2010-12-30 HDTV [proper]");
        }

    }
}
