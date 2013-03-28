using System;
using System.Collections.Generic;
using System.Linq;

using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Jobs.Implementations;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    
    public class SeasonSearchJobTest : CoreTest
    {
        private List<Episode> _episodes; 

        private ProgressNotification notification;

        [SetUp]
        public void Setup()
        {
             notification = new ProgressNotification("Search");

             _episodes = Builder<Episode>.CreateListOfSize(5)
                 .All()
                 .With(e => e.SeriesId = 1)
                 .With(e => e.SeasonNumber = 1)
                 .With(e => e.Ignored = false)
                 .With(e => e.AirDate = DateTime.Today.AddDays(-1))
                 .Build().ToList();

             Mocker.GetMock<IEpisodeService>()
                 .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(_episodes);
        }

        [Test]
        public void SeasonSearch_partial_season_success()
        {
            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(notification, 1, 1))
                .Returns(_episodes.Select(e => e.EpisodeNumber).ToList());

            
            Mocker.Resolve<SeasonSearchJob>().Start(notification, new { SeriesId = 1, SeasonNumber = 1 });

            
            Mocker.VerifyAllMocks();
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(notification, 1, 1), Times.Once());
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, new { EpisodeId =  It.IsAny<int>() }), Times.Never());
        }

        [Test]
        public void SeasonSearch_partial_season_failure()
        {
            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(notification, 1, 1))
                .Returns(new List<int>());

            
            Mocker.Resolve<SeasonSearchJob>().Start(notification, new { SeriesId = 1, SeasonNumber = 1 });

            
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(notification, 1, 1), Times.Once());
        }

        [Test]
        public void SeasonSearch_should_not_search_for_episodes_that_havent_aired_yet_or_air_tomorrow()
        {
            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .With(e => e.AirDate = DateTime.Today.AddDays(-1))
                .TheLast(2)
                .With(e => e.AirDate = DateTime.Today.AddDays(2))
                .Build().ToList();

            Mocker.GetMock<IEpisodeService>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(episodes);

            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(notification, 1, 1))
                .Returns(new List<int>{1});


            
            Mocker.Resolve<SeasonSearchJob>().Start(notification, new { SeriesId = 1, SeasonNumber = 1 });

            
            Mocker.VerifyAllMocks();
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(notification, 1, 1), Times.Once());
        }

        [Test]
        public void SeasonSearch_should_allow_searching_of_season_zero()
        {
            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(notification, 1, 0)).Returns(new List<int>());

            
            Mocker.Resolve<SeasonSearchJob>().Start(notification, new { SeriesId = 1, SeasonNumber = 0 });

            
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(notification, 1, 1), Times.Never());
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, new { EpisodeId = It.IsAny<int>() }), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_search_for_individual_episodes_when_no_partial_results_are_returned()
        {
            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(notification, 1, 1)).Returns(new List<int>());
            
            Mocker.Resolve<SeasonSearchJob>().Start(notification, new { SeriesId = 1, SeasonNumber = 1 });

            Mocker.GetMock<EpisodeSearchJob>().Verify(v => v.Start(notification, It.Is<object>(o => o.GetPropertyValue<Int32>("EpisodeId") > 0)), Times.Exactly(_episodes.Count));
        }
    }
}