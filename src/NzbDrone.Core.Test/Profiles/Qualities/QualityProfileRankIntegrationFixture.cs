using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.History;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityProfileRankIntegrationFixture : DbTest
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IQualityProfileRepository>(Mocker.Resolve<QualityProfileRepository>());
            Mocker.SetConstant<IQualityProfileRankRepository>(Mocker.Resolve<QualityProfileRankRepository>());
            Mocker.SetConstant<IQualityProfileRankService>(Mocker.Resolve<QualityProfileRankService>());
            Mocker.SetConstant<IQualityProfileService>(Mocker.Resolve<QualityProfileService>());
        }

        [Test]
        public void quality_sort_orders_history_across_two_profiles_by_normalized_score()
        {
            var profileA = AddProfile("A", Quality.HDTV720p, Quality.SDTV, Quality.HDTV720p);
            var profileB = AddProfile("B", Quality.Bluray1080p, Quality.SDTV, Quality.HDTV720p, Quality.WEBDL1080p, Quality.Bluray1080p);

            var seriesA = AddSeries("a", profileA.Id);
            var seriesB = AddSeries("b", profileB.Id);

            var episodeA = AddEpisode(seriesA.Id, 1, 1);
            var episodeB1 = AddEpisode(seriesB.Id, 1, 1);
            var episodeB2 = AddEpisode(seriesB.Id, 1, 2);

            Mocker.SetConstant<IHistoryRepository>(Mocker.Resolve<HistoryRepository>());

            InsertHistory(seriesA.Id, episodeA.Id, Quality.HDTV720p);
            InsertHistory(seriesB.Id, episodeB1.Id, Quality.HDTV720p);
            InsertHistory(seriesB.Id, episodeB2.Id, Quality.Bluray1080p);

            var spec = new PagingSpec<EpisodeHistory>
            {
                Page = 1,
                PageSize = 10,
                SortKey = "quality",
                SortDirection = SortDirection.Ascending
            };

            var result = Mocker.Resolve<IHistoryRepository>().GetPaged(spec, null, null);

            var rows = result.Records
                .Select(r => (seriesId: r.SeriesId, qualityId: r.Quality.Quality.Id))
                .ToList();

            rows.Should().HaveCount(3);
            rows[0].Should().Be((seriesB.Id, Quality.HDTV720p.Id));
            rows.Skip(1).Should().Contain((seriesA.Id, Quality.HDTV720p.Id));
            rows.Skip(1).Should().Contain((seriesB.Id, Quality.Bluray1080p.Id));
        }

        [Test]
        public void quality_sort_orders_blocklist_across_two_profiles_by_normalized_score()
        {
            var profileA = AddProfile("A", Quality.HDTV720p, Quality.SDTV, Quality.HDTV720p);
            var profileB = AddProfile("B", Quality.Bluray1080p, Quality.SDTV, Quality.HDTV720p, Quality.WEBDL1080p, Quality.Bluray1080p);

            var seriesA = AddSeries("a", profileA.Id);
            var seriesB = AddSeries("b", profileB.Id);

            InsertBlocklist(seriesA.Id, Quality.HDTV720p);
            InsertBlocklist(seriesB.Id, Quality.HDTV720p);
            InsertBlocklist(seriesB.Id, Quality.Bluray1080p);

            var spec = new PagingSpec<Blocklist>
            {
                Page = 1,
                PageSize = 10,
                SortKey = "quality",
                SortDirection = SortDirection.Ascending
            };

            var result = Mocker.Resolve<BlocklistRepository>().GetPaged(spec);

            var rows = result.Records
                .Select(b => (seriesId: b.SeriesId, qualityId: b.Quality.Quality.Id))
                .ToList();

            rows.Should().HaveCount(3);
            rows[0].Should().Be((seriesB.Id, Quality.HDTV720p.Id));
            rows.Skip(1).Should().Contain((seriesA.Id, Quality.HDTV720p.Id));
            rows.Skip(1).Should().Contain((seriesB.Id, Quality.Bluray1080p.Id));
        }

        [Test]
        public void quality_sort_orders_cutoff_unmet_by_normalized_score()
        {
            var profile = AddProfile("A", Quality.Bluray1080p, Quality.SDTV, Quality.HDTV720p, Quality.WEBDL1080p, Quality.Bluray1080p);
            var series = AddSeries("a", profile.Id);

            var sdFile = InsertEpisodeFile(Quality.SDTV);
            var hdFile = InsertEpisodeFile(Quality.HDTV720p);
            var webFile = InsertEpisodeFile(Quality.WEBDL1080p);

            AddEpisode(series.Id, 1, 1, sdFile.Id);
            AddEpisode(series.Id, 1, 2, hdFile.Id);
            AddEpisode(series.Id, 1, 3, webFile.Id);

            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>
            {
                new(profile.Id, new[] { Quality.SDTV.Id, Quality.HDTV720p.Id, Quality.WEBDL1080p.Id })
            };

            var spec = new PagingSpec<Episode>
            {
                Page = 1,
                PageSize = 10,
                SortKey = "quality",
                SortDirection = SortDirection.Ascending
            };

            var result = Mocker.Resolve<EpisodeRepository>().EpisodesWhereCutoffUnmet(spec, qualitiesBelowCutoff, false);

            result.Records.Select(e => e.EpisodeFileId)
                  .Should().Equal(sdFile.Id, hdFile.Id, webFile.Id);
        }

        private QualityProfile AddProfile(string name, Quality cutoff, params Quality[] qualities)
        {
            var profile = new QualityProfile
            {
                Name = name,
                Cutoff = cutoff.Id,
                Items = qualities.Select(q => new QualityProfileQualityItem { Quality = q, Allowed = true }).ToList()
            };

            Mocker.Resolve<IQualityProfileService>().Add(profile);

            return profile;
        }

        private Series AddSeries(string slug, int profileId)
        {
            var series = new Series
            {
                Title = slug,
                CleanTitle = slug,
                SortTitle = slug,
                TitleSlug = slug,
                Path = $"/series/{slug}",
                TvdbId = RandomNumber,
                QualityProfileId = profileId
            };

            series.Id = Db.Insert(series).Id;

            return series;
        }

        private Episode AddEpisode(int seriesId, int seasonNumber, int episodeNumber, int episodeFileId = 0)
        {
            var episode = new Episode
            {
                SeriesId = seriesId,
                SeasonNumber = seasonNumber,
                EpisodeNumber = episodeNumber,
                Title = $"s{seasonNumber}e{episodeNumber}",
                EpisodeFileId = episodeFileId,
                Monitored = true,
                AirDateUtc = DateTime.UtcNow.AddDays(-1)
            };

            episode.Id = Db.Insert(episode).Id;

            return episode;
        }

        private EpisodeFile InsertEpisodeFile(Quality quality)
        {
            return Mocker.Resolve<MediaFileRepository>().Insert(new EpisodeFile
            {
                RelativePath = $"file-{quality.Id}",
                Quality = new QualityModel(quality),
                Languages = new List<Language> { Language.English }
            });
        }

        private void InsertHistory(int seriesId, int episodeId, Quality quality)
        {
            Mocker.Resolve<IHistoryRepository>().Insert(new EpisodeHistory
            {
                SeriesId = seriesId,
                EpisodeId = episodeId,
                SourceTitle = "test",
                Date = DateTime.UtcNow,
                Quality = new QualityModel(quality),
                EventType = EpisodeHistoryEventType.Grabbed,
                Languages = new List<Language> { Language.English }
            });
        }

        private void InsertBlocklist(int seriesId, Quality quality)
        {
            Mocker.Resolve<BlocklistRepository>().Insert(new Blocklist
            {
                SeriesId = seriesId,
                EpisodeIds = new List<int>(),
                SourceTitle = "test",
                Quality = new QualityModel(quality),
                Date = DateTime.UtcNow,
                Languages = new List<Language> { Language.English }
            });
        }
    }
}
