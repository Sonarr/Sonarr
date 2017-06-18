using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class EpisodesWhereCutoffUnmetFixture : DbTest<EpisodeRepository, Episode>
    {
        private Series _monitoredSeries;
        private Series _unmonitoredSeries;
        private PagingSpec<Episode> _pagingSpec;
        private List<QualitiesBelowCutoff> _qualitiesBelowCutoff;
        private List<LanguagesBelowCutoff> _languagesBelowCutoff;
        private List<Episode> _unairedEpisodes;
            
        [SetUp]
        public void Setup()
        {
            var profile = new Profile 
            {  
                Id = 1,
                Cutoff = Quality.WEBDL480p.Id,
                Items = new List<ProfileQualityItem> 
                { 
                    new ProfileQualityItem { Allowed = true, Quality = Quality.SDTV },
                    new ProfileQualityItem { Allowed = true, Quality = Quality.WEBDL480p },
                    new ProfileQualityItem { Allowed = true, Quality = Quality.RAWHD }
                }
            };

            var langProfile = new LanguageProfile
            {
                Id = 1,
                Languages = Languages.LanguageFixture.GetDefaultLanguages(),
                Cutoff = Language.Spanish
            };

            _monitoredSeries = Builder<Series>.CreateNew()
                                              .With(s => s.TvRageId = RandomNumber)
                                              .With(s => s.Runtime = 30)
                                              .With(s => s.Monitored = true)
                                              .With(s => s.TitleSlug = "Title3")
                                              .With(s => s.ProfileId = profile.Id)
                                              .With(s => s.LanguageProfileId = langProfile.Id)
                                              .BuildNew();

            _unmonitoredSeries = Builder<Series>.CreateNew()
                                                .With(s => s.TvdbId = RandomNumber)
                                                .With(s => s.Runtime = 30)
                                                .With(s => s.Monitored = false)
                                                .With(s => s.TitleSlug = "Title2")
                                                .With(s => s.ProfileId = profile.Id)
                                                .With(s => s.LanguageProfileId = langProfile.Id)
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
                                        new QualitiesBelowCutoff(profile.Id, new[] {Quality.SDTV.Id})
                                    };

            _languagesBelowCutoff = new List<LanguagesBelowCutoff>
                                    {
                                        new LanguagesBelowCutoff(profile.Id, new[] {Language.English.Id})
                                    };

            var qualityMetLanguageUnmet = new EpisodeFile { RelativePath = "a", Quality = new QualityModel { Quality = Quality.WEBDL480p } , Language = Language.English };
            var qualityMetLanguageMet = new EpisodeFile { RelativePath = "b", Quality = new QualityModel { Quality = Quality.WEBDL480p }, Language = Language.Spanish };
            var qualityMetLanguageExceed = new EpisodeFile { RelativePath = "c", Quality = new QualityModel { Quality = Quality.WEBDL480p }, Language = Language.French };
            var qualityUnmetLanguageUnmet = new EpisodeFile { RelativePath = "d", Quality = new QualityModel { Quality = Quality.SDTV }, Language = Language.English };
            var qualityUnmetLanguageMet = new EpisodeFile { RelativePath = "e", Quality = new QualityModel { Quality = Quality.SDTV }, Language = Language.Spanish };
            var qualityUnmetLanguageExceed = new EpisodeFile { RelativePath = "f", Quality = new QualityModel { Quality = Quality.SDTV }, Language = Language.French };
            var qualityRawHDLanguageUnmet = new EpisodeFile { RelativePath = "g", Quality = new QualityModel { Quality = Quality.RAWHD }, Language = Language.English };
            var qualityRawHDLanguageMet = new EpisodeFile { RelativePath = "h", Quality = new QualityModel { Quality = Quality.RAWHD }, Language = Language.Spanish };
            var qualityRawHDLanguageExceed = new EpisodeFile { RelativePath = "i", Quality = new QualityModel { Quality = Quality.RAWHD }, Language = Language.French };

            MediaFileRepository fileRepository = Mocker.Resolve<MediaFileRepository>();

            qualityMetLanguageUnmet = fileRepository.Insert(qualityMetLanguageUnmet);
            qualityMetLanguageMet = fileRepository.Insert(qualityMetLanguageMet);
            qualityMetLanguageExceed = fileRepository.Insert(qualityMetLanguageExceed);
            qualityUnmetLanguageUnmet = fileRepository.Insert(qualityUnmetLanguageUnmet);
            qualityUnmetLanguageMet = fileRepository.Insert(qualityUnmetLanguageMet);
            qualityUnmetLanguageExceed = fileRepository.Insert(qualityUnmetLanguageExceed);
            qualityRawHDLanguageUnmet = fileRepository.Insert(qualityRawHDLanguageUnmet);
            qualityRawHDLanguageMet = fileRepository.Insert(qualityRawHDLanguageMet);
            qualityRawHDLanguageExceed = fileRepository.Insert(qualityRawHDLanguageExceed);

            var monitoredSeriesEpisodes = Builder<Episode>.CreateListOfSize(4)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _monitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityUnmetLanguageUnmet.Id)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .With(e => e.EpisodeFileId = qualityMetLanguageMet.Id)
                                           .TheNext(1)
                                           .With(e => e.EpisodeFileId = qualityRawHDLanguageExceed.Id)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 0)
                                           .Build();

            var unmonitoredSeriesEpisodes = Builder<Episode>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _unmonitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityRawHDLanguageUnmet.Id)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .With(e => e.EpisodeFileId = qualityMetLanguageMet.Id)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 0)
                                           .Build();


            _unairedEpisodes             = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _monitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityUnmetLanguageUnmet.Id)
                                           .Build()
                                           .ToList();
            
            Db.InsertMany(monitoredSeriesEpisodes);
            Db.InsertMany(unmonitoredSeriesEpisodes);
        }

        private void GivenMonitoredFilterExpression()
        {
            _pagingSpec.FilterExpressions.Add(e => e.Monitored == true && e.Series.Monitored == true);
        }

        private void GivenUnmonitoredFilterExpression()
        {
            _pagingSpec.FilterExpressions.Add(e => e.Monitored == false || e.Series.Monitored == false);
        }

        [Test]
        public void should_include_episodes_where_cutoff_has_not_be_met()
        {
            GivenMonitoredFilterExpression();

            var spec = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, _qualitiesBelowCutoff, _languagesBelowCutoff, false);

            spec.Records.Should().HaveCount(1);
            spec.Records.Should().OnlyContain(e => e.EpisodeFile.Value.Quality.Quality == Quality.SDTV);
        }

        [Test]
        public void should_only_contain_monitored_episodes()
        {
            GivenMonitoredFilterExpression();

            var spec = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, _qualitiesBelowCutoff, _languagesBelowCutoff, false);

            spec.Records.Should().HaveCount(1);
            spec.Records.Should().OnlyContain(e => e.Monitored);
        }

        [Test]
        public void should_only_contain_episode_with_monitored_series()
        {
            GivenMonitoredFilterExpression();

            var spec = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, _qualitiesBelowCutoff, _languagesBelowCutoff, false);

            spec.Records.Should().HaveCount(1);
            spec.Records.Should().OnlyContain(e => e.Series.Monitored);
        }

        [Test]
        public void should_contain_unaired_episodes_if_file_does_not_meet_cutoff()
        {
            Db.InsertMany(_unairedEpisodes);

            GivenMonitoredFilterExpression();

            var spec = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, _qualitiesBelowCutoff, _languagesBelowCutoff, false);

            spec.Records.Should().HaveCount(2);
            spec.Records.Should().OnlyContain(e => e.Series.Monitored);
        }
    }
}
