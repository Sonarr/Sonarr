using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    [TestFixture]
    public class EpisodeInfoRefreshedSearchFixture : CoreTest<EpisodeSearchService>
    {
        private Series _series;
        private IList<Episode> _added;
        private IList<Episode> _updated;
            
        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Added = DateTime.UtcNow.AddDays(-7))
                                     .With(s => s.Monitored = true)
                                     .Build();

            _added = new List<Episode>();
            _updated = new List<Episode>();
        }

        private void GivenUpdated()
        {
            _updated.Add(Builder<Episode>.CreateNew().Build());
        }

        [Test]
        public void should_not_search_if_no_episodes_were_upgraded()
        {
            _added.Add(new Episode());

            Subject.Handle(new EpisodeInfoRefreshedEvent(_series, _added, _updated));

            VerifyNoSearch();
        }

        [Test]
        public void should_not_search_if_series_was_added_within_the_last_day()
        {
            GivenUpdated();

            _series.Added = DateTime.UtcNow;
            _added.Add(new Episode());
            


            Subject.Handle(new EpisodeInfoRefreshedEvent(_series, _added, _updated));

            VerifyNoSearch();
        }

        [Test]
        public void should_not_search_if_no_episodes_were_added()
        {
            GivenUpdated();

            _updated.Add(new Episode());

            Subject.Handle(new EpisodeInfoRefreshedEvent(_series, _added, _updated));

            VerifyNoSearch();
        }

        [Test]
        public void should_not_search_if_air_date_doesnt_have_a_value()
        {
            GivenUpdated();

            _added.Add(new Episode());

            Subject.Handle(new EpisodeInfoRefreshedEvent(_series, _added, _updated));

            VerifyNoSearch();
        }

        [Test]
        public void should_not_search_if_episodes_air_in_the_future()
        {
            GivenUpdated();

            _added.Add(new Episode { AirDateUtc = DateTime.UtcNow.AddDays(7) });

            Subject.Handle(new EpisodeInfoRefreshedEvent(_series, _added, _updated));

            VerifyNoSearch();
        }

        [Test]
        public void should_not_search_if_series_is_not_monitored()
        {
            GivenUpdated();

            _series.Monitored = false;

            Subject.Handle(new EpisodeInfoRefreshedEvent(_series, _added, _updated));

            VerifyNoSearch();
        }

        [Test]
        public void should_not_search_if_episode_is_not_monitored()
        {
            GivenUpdated();

            _added.Add(new Episode { AirDateUtc = DateTime.UtcNow, Monitored = false });

            Subject.Handle(new EpisodeInfoRefreshedEvent(_series, _added, _updated));

            VerifyNoSearch();
        }

        [Test]
        public void should_search_for_a_newly_added_episode()
        {
            GivenUpdated();

            _added.Add(new Episode { AirDateUtc = DateTime.UtcNow, Monitored = true });

            Mocker.GetMock<IProcessDownloadDecisions>()
                  .Setup(s => s.ProcessDecisions(It.IsAny<List<DownloadDecision>>()))
                  .Returns(new ProcessedDecisions(new List<DownloadDecision>(), new List<DownloadDecision>(), new List<DownloadDecision>()));

            Subject.Handle(new EpisodeInfoRefreshedEvent(_series, _added, _updated));

            Mocker.GetMock<ISearchForNzb>()
                  .Verify(v => v.EpisodeSearch(It.IsAny<Episode>()), Times.Once());
        }

        private void VerifyNoSearch()
        {
            Mocker.GetMock<ISearchForNzb>()
                  .Verify(v => v.EpisodeSearch(It.IsAny<Episode>()), Times.Never());
        }
    }
}
