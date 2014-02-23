using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class EpisodesWhereCutoffUnmetFixture : DbTest<EpisodeRepository, Episode>
    {
        private Series _monitoredSeries;
        private Series _unmonitoredSeries;
        private PagingSpec<Episode> _pagingSpec;
        private List<QualitiesBelowCutoff> _qualitiesBelowCutoff;

        [SetUp]
        public void Setup()
        {
            var qualityProfile = new QualityProfile 
            {  
                Id = 1,
                Cutoff = Quality.WEBDL480p,
                Items = new List<QualityProfileItem> 
                { 
                    new QualityProfileItem { Allowed = true, Quality = Quality.SDTV },
                    new QualityProfileItem { Allowed = true, Quality = Quality.WEBDL480p },
                    new QualityProfileItem { Allowed = true, Quality = Quality.RAWHD }
                }
            };

            _monitoredSeries = Builder<Series>.CreateNew()
                                              .With(s => s.TvRageId = RandomNumber)
                                              .With(s => s.Runtime = 30)
                                              .With(s => s.Monitored = true)
                                              .With(s => s.TitleSlug = "Title3")
                                              .With(s => s.Id = qualityProfile.Id)
                                              .BuildNew();

            _unmonitoredSeries = Builder<Series>.CreateNew()
                                                .With(s => s.TvdbId = RandomNumber)
                                                .With(s => s.Runtime = 30)
                                                .With(s => s.Monitored = false)
                                                .With(s => s.TitleSlug = "Title2")
                                                .With(s => s.Id = qualityProfile.Id)
                                                .BuildNew();

            _monitoredSeries.Id = Db.Insert(_monitoredSeries).Id;
            _unmonitoredSeries.Id = Db.Insert(_unmonitoredSeries).Id;

            _pagingSpec = new PagingSpec<Episode>
                              {
                                  Page = 1,
                                  PageSize = 10,
                                  SortKey = "AirDate",
                                  SortDirection = SortDirection.Ascending
                              };

            _qualitiesBelowCutoff = new List<QualitiesBelowCutoff>
                                    {
                                        new QualitiesBelowCutoff(qualityProfile.Id, new[] {Quality.SDTV.Id})
                                    };

            var qualityMet = new EpisodeFile { Path = "a", Quality = new QualityModel { Quality = Quality.WEBDL480p } };
            var qualityUnmet = new EpisodeFile { Path = "b", Quality = new QualityModel { Quality = Quality.SDTV } };
            var qualityRawHD = new EpisodeFile { Path = "c", Quality = new QualityModel { Quality = Quality.RAWHD } };

            MediaFileRepository fileRepository = Mocker.Resolve<MediaFileRepository>();

            qualityMet = fileRepository.Insert(qualityMet);
            qualityUnmet = fileRepository.Insert(qualityUnmet);
            qualityRawHD = fileRepository.Insert(qualityRawHD);

            var monitoredSeriesEpisodes = Builder<Episode>.CreateListOfSize(4)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _monitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityUnmet.Id)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .With(e => e.EpisodeFileId = qualityMet.Id)
                                           .TheNext(1)
                                           .With(e => e.EpisodeFileId = qualityRawHD.Id)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 0)
                                           .Build();

            var unmonitoredSeriesEpisodes = Builder<Episode>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _unmonitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityUnmet.Id)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .With(e => e.EpisodeFileId = qualityMet.Id)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 0)
                                           .Build();


            var unairedEpisodes           = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _monitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityUnmet.Id)
                                           .Build();
            
            Db.InsertMany(monitoredSeriesEpisodes);
            Db.InsertMany(unmonitoredSeriesEpisodes);
            Db.InsertMany(unairedEpisodes);
        }

        private void GivenMonitoredFilterExpression()
        {
            _pagingSpec.FilterExpression = e => e.Monitored == true && e.Series.Monitored == true;
        }

        private void GivenUnmonitoredFilterExpression()
        {
            _pagingSpec.FilterExpression = e => e.Monitored == false || e.Series.Monitored == false;
        }

        [Test]
        public void should_include_episodes_where_cutoff_has_not_be_met()
        {
            GivenMonitoredFilterExpression();

            var spec = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, _qualitiesBelowCutoff, false);

            spec.Records.Should().HaveCount(1);
            spec.Records.Should().OnlyContain(e => e.EpisodeFile.Value.Quality.Quality == Quality.SDTV);
        }

        [Test]
        public void should_only_contain_monitored_episodes()
        {
            GivenMonitoredFilterExpression();

            var spec = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, _qualitiesBelowCutoff, false);

            spec.Records.Should().HaveCount(1);
            spec.Records.Should().OnlyContain(e => e.Monitored);
        }

        [Test]
        public void should_only_contain_episode_with_monitored_series()
        {
            GivenMonitoredFilterExpression();

            var spec = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, _qualitiesBelowCutoff, false);

            spec.Records.Should().HaveCount(1);
            spec.Records.Should().OnlyContain(e => e.Series.Monitored);
        }
    }
}
