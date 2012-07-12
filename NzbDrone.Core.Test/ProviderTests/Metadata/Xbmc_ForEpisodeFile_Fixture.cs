using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Metadata;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;
using NzbDrone.Test.Common;
using TvdbLib.Data;
using TvdbLib.Data.Banner;

namespace NzbDrone.Core.Test.ProviderTests.Metadata
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class Xbmc_ForEpisoddeFile_Fixture : CoreTest
    {
        private Series series;
        private EpisodeFile episodeFile;
        private TvdbSeries tvdbSeries;

        [SetUp]
        public void Setup()
        {
            WithTempAsAppPath();

            series = Builder<Series>
                    .CreateNew()
                    .With(s => s.SeriesId == 79488)
                    .With(s => s.Title == "30 Rock")
                    .Build();

            episodeFile = Builder<EpisodeFile>.CreateNew()
                    .With(f => f.SeriesId = 79488)
                    .With(f => f.SeasonNumber = 1)
                    .With(f => f.Path = @"C:\Test\30 Rock\Season 01\30 Rock - S01E01 - Pilot.avi")
                    .Build();

            var tvdbEpisodes = Builder<TvdbEpisode>.CreateListOfSize(2)
                    .All()
                    .With(e => e.SeriesId = 79488)
                    .With(e => e.SeasonNumber = 1)
                    .With(e => e.Directors = new List<string>{ "Fake Director" })
                    .With(e => e.Writer = new List<string>{ "Fake Writer" })
                    .With(e => e.GuestStars = new List<string> { "Guest Star 1", "Guest Star 2", "Guest Star 3", "" })
                    .Build();

            var seasonBanners = Builder<TvdbSeasonBanner>
                    .CreateListOfSize(4)
                    .TheFirst(2)
                    .With(b => b.Season = 1)
                    .TheLast(2)
                    .With(b => b.Season = 2)
                    .TheFirst(1)
                    .With(b => b.BannerType = TvdbSeasonBanner.Type.season)
                    .With(b => b.BannerPath = "seasons/79488-1-1.jpg")
                    .TheNext(2)
                    .With(b => b.BannerType = TvdbSeasonBanner.Type.seasonwide)
                    .With(b => b.BannerPath = "banners/seasons/79488-test.jpg")
                    .TheLast(1)
                    .With(b => b.BannerType = TvdbSeasonBanner.Type.season)
                    .With(b => b.BannerPath = "seasons/79488-2-1.jpg")
                    .Build();

            var seriesActors = Builder<TvdbActor>
                    .CreateListOfSize(5)
                    .Build();

            tvdbSeries = Builder<TvdbSeries>
                    .CreateNew()
                    .With(s => s.Id = 79488)
                    .With(s => s.SeriesName = "30 Rock")
                    .With(s => s.TvdbActors = seriesActors.ToList())
                    .With(s => s.Episodes = tvdbEpisodes.ToList())
                    .Build();

            tvdbSeries.Banners.AddRange(seasonBanners);
        }

        private void WithUseBanners()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.MetadataUseBanners).Returns(true);
        }

        private void WithSingleEpisodeFile()
        {
            var episode = Builder<Episode>.CreateNew()
                    .With(e => e.SeasonNumber = 1)
                    .With(e => e.SeriesId = 79488)
                    .With(e => e.EpisodeNumber = 1)
                    .Build();

            Mocker.GetMock<EpisodeProvider>()
                        .Setup(s => s.GetEpisodesByFileId(episodeFile.EpisodeFileId))
                        .Returns(new List<Episode> { episode });
        }

        private void WithMultiEpisodeFile()
        {
            var episodes = Builder<Episode>.CreateListOfSize(2)
                    .All()
                    .With(e => e.SeriesId = 79488)
                    .With(e => e.SeasonNumber = 1)
                    .Build();

            Mocker.GetMock<EpisodeProvider>()
                        .Setup(s => s.GetEpisodesByFileId(episodeFile.EpisodeFileId))
                        .Returns(episodes.ToList());
        }

        [Test]
        public void should_not_blowup()
        {
            WithSingleEpisodeFile();
            Mocker.Resolve<Xbmc>().CreateForEpisodeFile(episodeFile, tvdbSeries);
        }

        [Test]
        public void should_call_diskprovider_writeAllText_once_for_single_episode()
        {
            WithSingleEpisodeFile();
            Mocker.Resolve<Xbmc>().CreateForEpisodeFile(episodeFile, tvdbSeries);
            Mocker.GetMock<DiskProvider>().Verify(v => v.WriteAllText(episodeFile.Path.Replace("avi", "nfo"), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_call_diskprovider_writeAllText_once_for_multi_episode()
        {
            WithMultiEpisodeFile();
            Mocker.Resolve<Xbmc>().CreateForEpisodeFile(episodeFile, tvdbSeries);
            Mocker.GetMock<DiskProvider>().Verify(v => v.WriteAllText(episodeFile.Path.Replace("avi", "nfo"), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_download_thumbnail_when_thumbnail_path_is_not_null()
        {
            WithSingleEpisodeFile();
            Mocker.Resolve<Xbmc>().CreateForEpisodeFile(episodeFile, tvdbSeries);
            Mocker.GetMock<BannerProvider>().Verify(v => v.Download(tvdbSeries.Episodes.First().BannerPath, episodeFile.Path.Replace("avi", "tbn")), Times.Once());
        }
    }
}