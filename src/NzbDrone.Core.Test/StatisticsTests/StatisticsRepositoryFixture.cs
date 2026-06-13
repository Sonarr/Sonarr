using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Statistics;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.StatisticsTests;

[TestFixture]
public class StatisticsRepositoryFixture : DbTest<StatisticsRepository, Series>
{
    private int _nextTvdbId = 1;
    private int _nextEpisodeNumber = 1;
    private int _nextEpisodeFileNumber = 1;
    private int _nextQualityProfileNumber = 1;

    private Series GivenSeries(bool monitored = true,
                               SeriesStatusType status = SeriesStatusType.Continuing,
                               SeriesTypes seriesType = SeriesTypes.Standard,
                               int qualityProfileId = 1,
                               string path = null,
                               HashSet<int> tags = null)
    {
        var tvdbId = _nextTvdbId++;

        var series = Builder<Series>.CreateNew()
                                    .With(s => s.TvdbId = tvdbId)
                                    .With(s => s.TitleSlug = $"title-slug-{tvdbId}")
                                    .With(s => s.Monitored = monitored)
                                    .With(s => s.Status = status)
                                    .With(s => s.SeriesType = seriesType)
                                    .With(s => s.QualityProfileId = qualityProfileId)
                                    .With(s => s.Path = (path ?? $"/tv/Title {tvdbId}").AsOsAgnostic())
                                    .With(s => s.Tags = tags ?? new HashSet<int>())
                                    .BuildNew();

        series.Id = Db.Insert(series).Id;

        return series;
    }

    private void GivenEpisode(int seriesId, bool monitored = true, DateTime? airDateUtc = null, int episodeFileId = 0, int seasonNumber = 1)
    {
        var episode = Builder<Episode>.CreateNew()
                                      .With(e => e.SeriesId = seriesId)
                                      .With(e => e.SeasonNumber = seasonNumber)
                                      .With(e => e.EpisodeNumber = _nextEpisodeNumber++)
                                      .With(e => e.Monitored = monitored)
                                      .With(e => e.AirDateUtc = airDateUtc)
                                      .With(e => e.EpisodeFileId = episodeFileId)
                                      .BuildNew();

        Db.Insert(episode);
    }

    private void GivenEpisodeFile(int seriesId, long size, Quality quality = null)
    {
        var episodeFile = Builder<EpisodeFile>.CreateNew()
                                              .With(f => f.SeriesId = seriesId)
                                              .With(f => f.RelativePath = $"Season 1\\Episode {_nextEpisodeFileNumber++}.mkv")
                                              .With(f => f.Size = size)
                                              .With(f => f.Quality = new QualityModel(quality ?? Quality.HDTV720p))
                                              .With(f => f.Languages = new List<Language> { Language.English })
                                              .BuildNew();

        Db.Insert(episodeFile);
    }

    private Tag GivenTag(string label)
    {
        var tag = new Tag { Label = label };

        tag.Id = Db.Insert(tag).Id;

        return tag;
    }

    private QualityProfile GivenQualityProfile()
    {
        var profile = Builder<QualityProfile>.CreateNew()
                                             .With(p => p.Name = $"Profile {_nextQualityProfileNumber++}")
                                             .With(p => p.Items = Qualities.QualityFixture.GetDefaultQualities())
                                             .With(p => p.FormatItems = new List<ProfileFormatItem>())
                                             .BuildNew();

        profile.Id = Db.Insert(profile).Id;

        return profile;
    }

    [Test]
    public void should_return_zeros_when_library_is_empty()
    {
        var stats = Subject.GetLibraryStatistics();

        stats.SeriesCount.Should().Be(0);
        stats.MonitoredSeriesCount.Should().Be(0);
        stats.CompletedSeriesCount.Should().Be(0);
        stats.SeasonCount.Should().Be(0);
        stats.CompletedSeasonCount.Should().Be(0);
        stats.ContinuingSeriesCount.Should().Be(0);
        stats.EndedSeriesCount.Should().Be(0);
        stats.UpcomingSeriesCount.Should().Be(0);
        stats.DeletedSeriesCount.Should().Be(0);
        stats.StandardSeriesCount.Should().Be(0);
        stats.DailySeriesCount.Should().Be(0);
        stats.AnimeSeriesCount.Should().Be(0);
        stats.TotalEpisodeCount.Should().Be(0);
        stats.MonitoredEpisodeCount.Should().Be(0);
        stats.DownloadedEpisodeCount.Should().Be(0);
        stats.MissingEpisodeCount.Should().Be(0);
        stats.UnairedEpisodeCount.Should().Be(0);
        stats.EpisodeFileCount.Should().Be(0);
        stats.SizeOnDisk.Should().Be(0);
        stats.QualityProfileStatistics.Should().BeEmpty();
        stats.QualityStatistics.Should().BeEmpty();
        stats.TagStatistics.Should().BeEmpty();
    }

    [Test]
    public void should_count_series_by_monitored_and_status()
    {
        GivenSeries(monitored: true, status: SeriesStatusType.Continuing);
        GivenSeries(monitored: false, status: SeriesStatusType.Ended);
        GivenSeries(monitored: true, status: SeriesStatusType.Upcoming);
        GivenSeries(monitored: false, status: SeriesStatusType.Deleted);

        var stats = Subject.GetLibraryStatistics();

        stats.SeriesCount.Should().Be(4);
        stats.MonitoredSeriesCount.Should().Be(2);
        stats.ContinuingSeriesCount.Should().Be(1);
        stats.EndedSeriesCount.Should().Be(1);
        stats.UpcomingSeriesCount.Should().Be(1);
        stats.DeletedSeriesCount.Should().Be(1);
    }

    [Test]
    public void should_count_series_by_type()
    {
        GivenSeries(seriesType: SeriesTypes.Standard);
        GivenSeries(seriesType: SeriesTypes.Standard);
        GivenSeries(seriesType: SeriesTypes.Daily);
        GivenSeries(seriesType: SeriesTypes.Anime);

        var stats = Subject.GetLibraryStatistics();

        stats.StandardSeriesCount.Should().Be(2);
        stats.DailySeriesCount.Should().Be(1);
        stats.AnimeSeriesCount.Should().Be(1);
    }

    [Test]
    public void should_count_completed_series()
    {
        var aired = DateTime.UtcNow.AddDays(-5);
        var unaired = DateTime.UtcNow.AddDays(5);

        var completeSeries = GivenSeries();
        var incompleteSeries = GivenSeries();
        GivenSeries();

        GivenEpisode(completeSeries.Id, monitored: true, airDateUtc: aired, episodeFileId: 1);
        GivenEpisode(completeSeries.Id, monitored: true, airDateUtc: unaired);
        GivenEpisode(incompleteSeries.Id, monitored: true, airDateUtc: aired);

        var stats = Subject.GetLibraryStatistics();

        stats.SeriesCount.Should().Be(3);
        stats.CompletedSeriesCount.Should().Be(1);
    }

    [Test]
    public void should_count_seasons_excluding_specials()
    {
        var series = GivenSeries();
        var aired = DateTime.UtcNow.AddDays(-5);

        GivenEpisode(series.Id, monitored: true, airDateUtc: aired, episodeFileId: 1, seasonNumber: 0);
        GivenEpisode(series.Id, monitored: true, airDateUtc: aired, episodeFileId: 1, seasonNumber: 1);
        GivenEpisode(series.Id, monitored: true, airDateUtc: aired, seasonNumber: 2);

        var stats = Subject.GetLibraryStatistics();

        stats.SeasonCount.Should().Be(2);
        stats.CompletedSeasonCount.Should().Be(1);
    }

    [Test]
    public void should_count_episodes()
    {
        var series = GivenSeries();
        var aired = DateTime.UtcNow.AddDays(-5);
        var unaired = DateTime.UtcNow.AddDays(5);

        GivenEpisode(series.Id, monitored: true, airDateUtc: aired);
        GivenEpisode(series.Id, monitored: true, airDateUtc: aired, episodeFileId: 1);
        GivenEpisode(series.Id, monitored: false, airDateUtc: aired);
        GivenEpisode(series.Id, monitored: true, airDateUtc: unaired);
        GivenEpisode(series.Id, monitored: true, airDateUtc: null);

        var stats = Subject.GetLibraryStatistics();

        stats.TotalEpisodeCount.Should().Be(5);
        stats.MonitoredEpisodeCount.Should().Be(4);
        stats.DownloadedEpisodeCount.Should().Be(1);
        stats.MissingEpisodeCount.Should().Be(1);
        stats.UnairedEpisodeCount.Should().Be(2);
    }

    [Test]
    public void should_not_count_unaired_episode_with_file_as_unaired()
    {
        var series = GivenSeries();

        GivenEpisode(series.Id, monitored: true, airDateUtc: DateTime.UtcNow.AddDays(5), episodeFileId: 1);

        var stats = Subject.GetLibraryStatistics();

        stats.DownloadedEpisodeCount.Should().Be(1);
        stats.UnairedEpisodeCount.Should().Be(0);
    }

    [Test]
    public void should_not_count_missing_episode_when_series_is_unmonitored()
    {
        var series = GivenSeries(monitored: false);

        GivenEpisode(series.Id, monitored: true, airDateUtc: DateTime.UtcNow.AddDays(-5));

        var stats = Subject.GetLibraryStatistics();

        stats.TotalEpisodeCount.Should().Be(1);
        stats.MissingEpisodeCount.Should().Be(0);
    }

    [Test]
    public void should_not_count_missing_episode_when_episode_has_file()
    {
        var series = GivenSeries();

        GivenEpisode(series.Id, monitored: true, airDateUtc: DateTime.UtcNow.AddDays(-5), episodeFileId: 1);

        var stats = Subject.GetLibraryStatistics();

        stats.MissingEpisodeCount.Should().Be(0);
    }

    [Test]
    public void should_sum_episode_file_count_and_size_on_disk()
    {
        var series = GivenSeries();

        GivenEpisodeFile(series.Id, 100);
        GivenEpisodeFile(series.Id, 200);

        var stats = Subject.GetLibraryStatistics();

        stats.EpisodeFileCount.Should().Be(2);
        stats.SizeOnDisk.Should().Be(300);
    }

    [Test]
    public void should_count_series_and_files_per_quality_profile()
    {
        var profile1 = GivenQualityProfile();
        var profile2 = GivenQualityProfile();

        var series1 = GivenSeries(qualityProfileId: profile1.Id);
        GivenSeries(qualityProfileId: profile1.Id);

        GivenEpisodeFile(series1.Id, 100);
        GivenEpisodeFile(series1.Id, 200);

        var stats = Subject.GetLibraryStatistics();

        stats.QualityProfileStatistics.Should().HaveCount(2);

        var profile1Stats = stats.QualityProfileStatistics[0];
        profile1Stats.QualityProfileId.Should().Be(profile1.Id);
        profile1Stats.Name.Should().Be(profile1.Name);
        profile1Stats.SeriesCount.Should().Be(2);
        profile1Stats.EpisodeFileCount.Should().Be(2);
        profile1Stats.SizeOnDisk.Should().Be(300);

        var profile2Stats = stats.QualityProfileStatistics[1];
        profile2Stats.QualityProfileId.Should().Be(profile2.Id);
        profile2Stats.Name.Should().Be(profile2.Name);
        profile2Stats.SeriesCount.Should().Be(0);
        profile2Stats.EpisodeFileCount.Should().Be(0);
        profile2Stats.SizeOnDisk.Should().Be(0);
    }

    [Test]
    public void should_filter_series_counts_by_root_folder()
    {
        GivenSeries(path: "/tv/Show 1".AsOsAgnostic());
        GivenSeries(path: "/tv2/Show 2".AsOsAgnostic());
        GivenSeries(path: "/films/Show 3".AsOsAgnostic());

        Subject.GetLibraryStatistics().SeriesCount.Should().Be(3);
        Subject.GetLibraryStatistics(new StatisticsFilter { RootFolderPaths = new List<string> { "/tv".AsOsAgnostic() } }).SeriesCount.Should().Be(1);
        Subject.GetLibraryStatistics(new StatisticsFilter { RootFolderPaths = new List<string> { "/tv/".AsOsAgnostic() } }).SeriesCount.Should().Be(1);
        Subject.GetLibraryStatistics(new StatisticsFilter { RootFolderPaths = new List<string> { "/music".AsOsAgnostic() } }).SeriesCount.Should().Be(0);
    }

    [Test]
    public void should_filter_episode_and_file_counts_by_root_folder()
    {
        var series1 = GivenSeries(path: "/tv/Show 1".AsOsAgnostic());
        var series2 = GivenSeries(path: "/tv2/Show 2".AsOsAgnostic());

        GivenEpisode(series1.Id, monitored: true, airDateUtc: DateTime.UtcNow.AddDays(-5));
        GivenEpisode(series2.Id, monitored: true, airDateUtc: DateTime.UtcNow.AddDays(-5));

        GivenEpisodeFile(series1.Id, 100, Quality.HDTV720p);
        GivenEpisodeFile(series2.Id, 200, Quality.Bluray1080p);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter { RootFolderPaths = new List<string> { "/tv".AsOsAgnostic() } });

        stats.TotalEpisodeCount.Should().Be(1);
        stats.MissingEpisodeCount.Should().Be(1);
        stats.SeasonCount.Should().Be(1);
        stats.CompletedSeriesCount.Should().Be(0);
        stats.CompletedSeasonCount.Should().Be(0);
        stats.EpisodeFileCount.Should().Be(1);
        stats.SizeOnDisk.Should().Be(100);
        stats.QualityStatistics.Should().HaveCount(1);
        stats.QualityStatistics[0].Quality.Should().Be(Quality.HDTV720p);
    }

    [Test]
    public void should_keep_all_quality_profiles_when_filtering_by_root_folder()
    {
        var profile1 = GivenQualityProfile();
        var profile2 = GivenQualityProfile();

        var series1 = GivenSeries(qualityProfileId: profile1.Id, path: "/tv/Show 1".AsOsAgnostic());
        var series2 = GivenSeries(qualityProfileId: profile2.Id, path: "/tv2/Show 2".AsOsAgnostic());

        GivenEpisodeFile(series1.Id, 100);
        GivenEpisodeFile(series2.Id, 200);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter { RootFolderPaths = new List<string> { "/tv".AsOsAgnostic() } });

        stats.QualityProfileStatistics.Should().HaveCount(2);

        var profile1Stats = stats.QualityProfileStatistics[0];
        profile1Stats.SeriesCount.Should().Be(1);
        profile1Stats.EpisodeFileCount.Should().Be(1);
        profile1Stats.SizeOnDisk.Should().Be(100);

        var profile2Stats = stats.QualityProfileStatistics[1];
        profile2Stats.SeriesCount.Should().Be(0);
        profile2Stats.EpisodeFileCount.Should().Be(0);
        profile2Stats.SizeOnDisk.Should().Be(0);
    }

    [Test]
    public void should_count_series_and_files_per_tag()
    {
        var hdTag = GivenTag("hd");
        var animeTag = GivenTag("anime");
        GivenTag("unused");

        var series1 = GivenSeries(tags: new HashSet<int> { hdTag.Id });
        var series2 = GivenSeries(tags: new HashSet<int> { hdTag.Id, animeTag.Id });
        GivenSeries();

        GivenEpisodeFile(series1.Id, 100);
        GivenEpisodeFile(series2.Id, 200);

        var stats = Subject.GetLibraryStatistics();

        stats.TagStatistics.Should().HaveCount(3);

        var animeStats = stats.TagStatistics[0];
        animeStats.Label.Should().Be("anime");
        animeStats.SeriesCount.Should().Be(1);
        animeStats.EpisodeFileCount.Should().Be(1);
        animeStats.SizeOnDisk.Should().Be(200);

        var hdStats = stats.TagStatistics[1];
        hdStats.Label.Should().Be("hd");
        hdStats.SeriesCount.Should().Be(2);
        hdStats.EpisodeFileCount.Should().Be(2);
        hdStats.SizeOnDisk.Should().Be(300);

        var unusedStats = stats.TagStatistics[2];
        unusedStats.Label.Should().Be("unused");
        unusedStats.SeriesCount.Should().Be(0);
        unusedStats.EpisodeFileCount.Should().Be(0);
        unusedStats.SizeOnDisk.Should().Be(0);
    }

    [Test]
    public void should_keep_all_tags_when_filtering_by_root_folder()
    {
        var tag = GivenTag("hd");

        var series1 = GivenSeries(path: "/tv/Show 1".AsOsAgnostic(), tags: new HashSet<int> { tag.Id });
        var series2 = GivenSeries(path: "/tv2/Show 2".AsOsAgnostic(), tags: new HashSet<int> { tag.Id });

        GivenEpisodeFile(series1.Id, 100);
        GivenEpisodeFile(series2.Id, 200);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter { RootFolderPaths = new List<string> { "/tv".AsOsAgnostic() } });

        stats.TagStatistics.Should().HaveCount(1);
        stats.TagStatistics[0].SeriesCount.Should().Be(1);
        stats.TagStatistics[0].EpisodeFileCount.Should().Be(1);
        stats.TagStatistics[0].SizeOnDisk.Should().Be(100);
    }

    [Test]
    public void should_filter_by_monitored()
    {
        var monitored = GivenSeries(monitored: true);
        var unmonitored = GivenSeries(monitored: false);

        GivenEpisodeFile(monitored.Id, 100);
        GivenEpisodeFile(unmonitored.Id, 200);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter { Monitored = true });

        stats.SeriesCount.Should().Be(1);
        stats.EpisodeFileCount.Should().Be(1);
        stats.SizeOnDisk.Should().Be(100);
    }

    [Test]
    public void should_filter_by_quality_profile()
    {
        var profile1 = GivenQualityProfile();
        var profile2 = GivenQualityProfile();

        var series1 = GivenSeries(qualityProfileId: profile1.Id);
        var series2 = GivenSeries(qualityProfileId: profile2.Id);

        GivenEpisodeFile(series1.Id, 100);
        GivenEpisodeFile(series2.Id, 200);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter { QualityProfileIds = new List<int> { profile2.Id } });

        stats.SeriesCount.Should().Be(1);
        stats.EpisodeFileCount.Should().Be(1);
        stats.SizeOnDisk.Should().Be(200);
    }

    [Test]
    public void should_filter_by_series_type()
    {
        var anime = GivenSeries(seriesType: SeriesTypes.Anime);
        var standard = GivenSeries(seriesType: SeriesTypes.Standard);

        GivenEpisodeFile(anime.Id, 100);
        GivenEpisodeFile(standard.Id, 200);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter { SeriesTypes = new List<SeriesTypes> { SeriesTypes.Anime } });

        stats.SeriesCount.Should().Be(1);
        stats.AnimeSeriesCount.Should().Be(1);
        stats.StandardSeriesCount.Should().Be(0);
        stats.EpisodeFileCount.Should().Be(1);
        stats.SizeOnDisk.Should().Be(100);
    }

    [Test]
    public void should_filter_by_tag()
    {
        var tag = GivenTag("4k");
        var otherTag = GivenTag("kids");

        var tagged = GivenSeries(tags: new HashSet<int> { tag.Id });
        var otherTagged = GivenSeries(tags: new HashSet<int> { otherTag.Id });
        GivenSeries();

        GivenEpisodeFile(tagged.Id, 100);
        GivenEpisodeFile(otherTagged.Id, 200);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter { TagIds = new List<int> { tag.Id } });

        stats.SeriesCount.Should().Be(1);
        stats.EpisodeFileCount.Should().Be(1);
        stats.SizeOnDisk.Should().Be(100);

        stats.TagStatistics.Should().HaveCount(2);
        stats.TagStatistics[0].Label.Should().Be("4k");
        stats.TagStatistics[0].SeriesCount.Should().Be(1);
        stats.TagStatistics[1].Label.Should().Be("kids");
        stats.TagStatistics[1].SeriesCount.Should().Be(0);
    }

    [Test]
    public void should_filter_by_multiple_root_folders()
    {
        var series1 = GivenSeries(path: "/tv/Show 1".AsOsAgnostic());
        var series2 = GivenSeries(path: "/films/Show 2".AsOsAgnostic());
        GivenSeries(path: "/tv2/Show 3".AsOsAgnostic());

        GivenEpisodeFile(series1.Id, 100);
        GivenEpisodeFile(series2.Id, 200);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter
        {
            RootFolderPaths = new List<string> { "/tv".AsOsAgnostic(), "/films".AsOsAgnostic() }
        });

        stats.SeriesCount.Should().Be(2);
        stats.EpisodeFileCount.Should().Be(2);
        stats.SizeOnDisk.Should().Be(300);
    }

    [Test]
    public void should_filter_by_multiple_tags()
    {
        var tag1 = GivenTag("4k");
        var tag2 = GivenTag("kids");

        var series1 = GivenSeries(tags: new HashSet<int> { tag1.Id });
        var series2 = GivenSeries(tags: new HashSet<int> { tag2.Id });
        GivenSeries();

        GivenEpisodeFile(series1.Id, 100);
        GivenEpisodeFile(series2.Id, 200);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter
        {
            TagIds = new List<int> { tag1.Id, tag2.Id }
        });

        stats.SeriesCount.Should().Be(2);
        stats.EpisodeFileCount.Should().Be(2);
        stats.SizeOnDisk.Should().Be(300);
    }

    [Test]
    public void should_filter_by_multiple_quality_profiles()
    {
        var profile1 = GivenQualityProfile();
        var profile2 = GivenQualityProfile();
        var profile3 = GivenQualityProfile();

        GivenSeries(qualityProfileId: profile1.Id);
        GivenSeries(qualityProfileId: profile2.Id);
        GivenSeries(qualityProfileId: profile3.Id);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter
        {
            QualityProfileIds = new List<int> { profile1.Id, profile3.Id }
        });

        stats.SeriesCount.Should().Be(2);
    }

    [Test]
    public void should_filter_by_multiple_series_types()
    {
        GivenSeries(seriesType: SeriesTypes.Standard);
        GivenSeries(seriesType: SeriesTypes.Daily);
        GivenSeries(seriesType: SeriesTypes.Anime);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter
        {
            SeriesTypes = new List<SeriesTypes> { SeriesTypes.Daily, SeriesTypes.Anime }
        });

        stats.SeriesCount.Should().Be(2);
        stats.DailySeriesCount.Should().Be(1);
        stats.AnimeSeriesCount.Should().Be(1);
        stats.StandardSeriesCount.Should().Be(0);
    }

    [Test]
    public void should_exclude_root_folders()
    {
        GivenSeries(path: "/tv/Show 1".AsOsAgnostic());
        GivenSeries(path: "/tv2/Show 2".AsOsAgnostic());
        GivenSeries(path: "/films/Show 3".AsOsAgnostic());

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter
        {
            RootFolderPaths = new List<string> { "/tv".AsOsAgnostic() },
            RootFolderPathsNot = true
        });

        stats.SeriesCount.Should().Be(2);
    }

    [Test]
    public void should_exclude_tags()
    {
        var tag1 = GivenTag("4k");
        var tag2 = GivenTag("kids");

        GivenSeries(tags: new HashSet<int> { tag1.Id });
        GivenSeries(tags: new HashSet<int> { tag2.Id });
        GivenSeries();

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter
        {
            TagIds = new List<int> { tag1.Id, tag2.Id },
            TagIdsNot = true
        });

        stats.SeriesCount.Should().Be(1);
    }

    [Test]
    public void should_exclude_quality_profiles()
    {
        var profile1 = GivenQualityProfile();
        var profile2 = GivenQualityProfile();

        GivenSeries(qualityProfileId: profile1.Id);
        GivenSeries(qualityProfileId: profile2.Id);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter
        {
            QualityProfileIds = new List<int> { profile1.Id },
            QualityProfileIdsNot = true
        });

        stats.SeriesCount.Should().Be(1);
    }

    [Test]
    public void should_exclude_series_types()
    {
        GivenSeries(seriesType: SeriesTypes.Standard);
        GivenSeries(seriesType: SeriesTypes.Daily);
        GivenSeries(seriesType: SeriesTypes.Anime);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter
        {
            SeriesTypes = new List<SeriesTypes> { SeriesTypes.Anime },
            SeriesTypesNot = true
        });

        stats.SeriesCount.Should().Be(2);
        stats.AnimeSeriesCount.Should().Be(0);
    }

    [Test]
    public void should_combine_filters()
    {
        var tag = GivenTag("4k");

        var taggedMonitored = GivenSeries(monitored: true, tags: new HashSet<int> { tag.Id });
        var taggedUnmonitored = GivenSeries(monitored: false, tags: new HashSet<int> { tag.Id });
        GivenSeries(monitored: true);

        GivenEpisodeFile(taggedMonitored.Id, 100);
        GivenEpisodeFile(taggedUnmonitored.Id, 200);

        var stats = Subject.GetLibraryStatistics(new StatisticsFilter { TagIds = new List<int> { tag.Id }, Monitored = true });

        stats.SeriesCount.Should().Be(1);
        stats.EpisodeFileCount.Should().Be(1);
        stats.SizeOnDisk.Should().Be(100);
    }

    [Test]
    public void should_count_episode_files_per_quality()
    {
        var series = GivenSeries();

        GivenEpisodeFile(series.Id, 100, Quality.HDTV720p);
        GivenEpisodeFile(series.Id, 200, Quality.HDTV720p);
        GivenEpisodeFile(series.Id, 400, Quality.Bluray1080p);

        var stats = Subject.GetLibraryStatistics();

        stats.QualityStatistics.Should().HaveCount(2);

        var hdtvStats = stats.QualityStatistics[0];
        hdtvStats.Quality.Should().Be(Quality.HDTV720p);
        hdtvStats.EpisodeFileCount.Should().Be(2);
        hdtvStats.SizeOnDisk.Should().Be(300);

        var blurayStats = stats.QualityStatistics[1];
        blurayStats.Quality.Should().Be(Quality.Bluray1080p);
        blurayStats.EpisodeFileCount.Should().Be(1);
        blurayStats.SizeOnDisk.Should().Be(400);
    }
}
