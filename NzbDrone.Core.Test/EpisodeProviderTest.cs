using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EpisodeProviderTest
    {
        [Test]
        public void RefreshEpisodeInfo()
        {
            //Arrange
            const int seriesId = 71663;
            const int episodeCount = 10;

            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               WhereAll()
                                               .Have(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());

            mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeEpisodes).Verifiable();

            //mocker.GetMock<IRepository>().SetReturnsDefault();


            //Act
            var sw = Stopwatch.StartNew();
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(seriesId);
            var actualCount = mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId);
            //Assert
            mocker.GetMock<TvDbProvider>().VerifyAll();
            Assert.Count(episodeCount, actualCount);
            Console.WriteLine("Duration: " + sw.Elapsed);
        }

        [Test]

        //Should Download
        [Row(QualityTypes.TV, true, QualityTypes.TV, false, true)]
        [Row(QualityTypes.DVD, true, QualityTypes.TV, true, true)]
        [Row(QualityTypes.DVD, true, QualityTypes.TV, true, true)]
        //Should Skip
        [Row(QualityTypes.Bluray720, true, QualityTypes.Bluray1080, false, false)]
        [Row(QualityTypes.TV, true, QualityTypes.HDTV, true, false)]
        public void Is_Needed_Tv_Dvd_BluRay_BluRay720_Is_Cutoff(QualityTypes reportQuality, Boolean isReportProper, QualityTypes fileQuality, Boolean isFileProper, bool excpected)
        {
            //Setup
            var parseResult = new EpisodeParseResult
                                  {
                                      SeriesId = 12,
                                      SeasonNumber = 2,
                                      Episodes = new List<int> { 3 },
                                      Quality = reportQuality,
                                      Proper = isReportProper
                                  };

            var epFile = new EpisodeFile()
            {
                Proper = isFileProper,
                Quality = fileQuality
            };

            var episodeInfo = new Episode
                                  {
                                      SeriesId = 12,
                                      SeasonNumber = 2,
                                      EpisodeNumber = 3,
                                      Series = new Series() { QualityProfileId = 1 },
                                      EpisodeFile = epFile

                                  };

            var seriesQualityProfile = new QualityProfile()
                                           {
                                               Allowed = new List<QualityTypes> { QualityTypes.TV, QualityTypes.DVD, QualityTypes.Bluray720, QualityTypes.Bluray1080 },
                                               Cutoff = QualityTypes.Bluray720,
                                               Name = "TV/DVD",
                                           };



            var mocker = new AutoMoqer();
            mocker.GetMock<IRepository>()
                .Setup(r => r.Single(It.IsAny<Expression<Func<Episode, Boolean>>>()))
                .Returns(episodeInfo);

            mocker.GetMock<QualityProvider>()
                .Setup(q => q.Find(1))
                .Returns(seriesQualityProfile);

            var result = mocker.Resolve<EpisodeProvider>().IsNeeded(parseResult);

            Assert.AreEqual(excpected, result);
        }

        [Test]
        public void Missing_episode_should_be_added()
        {
            //Setup
            var parseResult1 = new EpisodeParseResult
            {
                SeriesId = 12,
                SeasonNumber = 2,
                Episodes = new List<int> { 3 },
                Quality = QualityTypes.DVD

            };

            var parseResult2 = new EpisodeParseResult
            {
                SeriesId = 12,
                SeasonNumber = 3,
                Episodes = new List<int> { 3 },
                Quality = QualityTypes.DVD

            };

            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyRepository());

            var episodeProvider = mocker.Resolve<EpisodeProvider>();
            var result1 = episodeProvider.IsNeeded(parseResult1);
            var result2 = episodeProvider.IsNeeded(parseResult2);
            var episodes = episodeProvider.GetEpisodeBySeries(12);
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
            Assert.IsNotEmpty(episodes);
            Assert.Count(2, episodes);
        }

        [Test]
        [Row(1, new[] { 2 }, "My Episode Title", QualityTypes.DVD, false, "My Series Name - 1x2 - My Episode Title DVD")]
        [Row(1, new[] { 2 }, "My Episode Title", QualityTypes.DVD, true, "My Series Name - 1x2 - My Episode Title DVD [Proper]")]
        [Row(1, new[] { 2 }, "", QualityTypes.DVD, true, "My Series Name - 1x2 -  DVD [Proper]")]
        [Row(1, new[] { 2, 4 }, "My Episode Title", QualityTypes.HDTV, false, "My Series Name - 1x2-1x4 - My Episode Title HDTV")]
        [Row(1, new[] { 2, 4 }, "My Episode Title", QualityTypes.HDTV, true, "My Series Name - 1x2-1x4 - My Episode Title HDTV [Proper]")]
        [Row(1, new[] { 2, 4 }, "", QualityTypes.HDTV, true, "My Series Name - 1x2-1x4 -  HDTV [Proper]")]
        public void sab_title(int seasons, int[] episodes, string title, QualityTypes quality, bool proper, string excpected)
        {
            //Arrange
            var fakeSeries = new Series()
                                 {
                                     SeriesId = 12,
                                     Path = "C:\\TV Shows\\My Series Name"
                                 };

            var mocker = new AutoMoqer();
            mocker.GetMock<SeriesProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(12))
                .Returns(fakeSeries);

            var parsResult = new EpisodeParseResult()
                                 {
                                     SeriesId = 12,
                                     AirDate = DateTime.Now,
                                     Episodes = episodes.ToList(),
                                     Proper = proper,
                                     Quality = quality,
                                     SeasonNumber = seasons,
                                     EpisodeTitle = title
                                 };

            //Act
            var actual = mocker.Resolve<EpisodeProvider>().GetSabTitle(parsResult);

            //Assert
            Assert.AreEqual(excpected, actual);
        }


        [Test]
        [Explicit]
        public void Add_daily_show_episodes()
        {
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyRepository());
            mocker.Resolve<TvDbProvider>();
            const int tvDbSeriesId = 71256;
            //act
            var seriesProvider = mocker.Resolve<SeriesProvider>();

            seriesProvider.AddSeries("c:\\test\\", tvDbSeriesId, 0);
            var episodeProvider = mocker.Resolve<EpisodeProvider>();
            episodeProvider.RefreshEpisodeInfo(tvDbSeriesId);

            //assert
            var episodes = episodeProvider.GetEpisodeBySeries(tvDbSeriesId);
            Assert.IsNotEmpty(episodes);
        }
    }
}