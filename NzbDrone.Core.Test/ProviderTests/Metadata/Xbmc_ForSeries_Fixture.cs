using System;
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
    public class Xbmc_ForSeries_Fixture : CoreTest
    {
        private Series series;
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
                    .All()
                    .With(a => a.ActorImage = Builder<TvdbActorBanner>.CreateNew().Build())
                    .Build();

            tvdbSeries = Builder<TvdbSeries>
                    .CreateNew()
                    .With(s => s.Id = 79488)
                    .With(s => s.SeriesName = "30 Rock")
                    .With(s => s.TvdbActors = seriesActors.ToList())
                    .Build();

            tvdbSeries.Banners.AddRange(seasonBanners);
        }

        private void WithUseBanners()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.MetadataUseBanners).Returns(true);
        }

        private void WithSpecials()
        {
            var seasonBanners = Builder<TvdbSeasonBanner>
                    .CreateListOfSize(2)
                    .All()
                    .With(b => b.Season = 0)
                    .TheFirst(1)
                    .With(b => b.BannerType = TvdbSeasonBanner.Type.season)
                    .With(b => b.BannerPath = "seasons/79488-0-1.jpg")
                    .TheLast(1)
                    .With(b => b.BannerType = TvdbSeasonBanner.Type.seasonwide)
                    .With(b => b.BannerPath = "banners/seasons/79488-0-1.jpg")
                    .Build();

            var seriesActors = Builder<TvdbActor>
                    .CreateListOfSize(5)
                    .All()
                    .With(a => a.ActorImage = Builder<TvdbActorBanner>.CreateNew().Build())
                    .Build();

            tvdbSeries = Builder<TvdbSeries>
                    .CreateNew()
                    .With(s => s.Id = 79488)
                    .With(s => s.SeriesName = "30 Rock")
                    .With(s => s.TvdbActors = seriesActors.ToList())
                    .Build();

            tvdbSeries.Banners.AddRange(seasonBanners);
        }

        [Test]
        public void should_not_blowup()
        {
            Mocker.Resolve<Xbmc>().CreateForSeries(series, tvdbSeries);
        }

        [Test]
        public void should_call_diskprovider_writeAllText()
        {
            Mocker.Resolve<Xbmc>().CreateForSeries(series, tvdbSeries);
            Mocker.GetMock<DiskProvider>().Verify(v => v.WriteAllText(Path.Combine(series.Path, "tvshow.nfo"), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_download_fanart()
        {
            Mocker.Resolve<Xbmc>().CreateForSeries(series, tvdbSeries);
            Mocker.GetMock<BannerProvider>().Verify(v => v.Download(tvdbSeries.FanartPath, Path.Combine(series.Path, "fanart.jpg")), Times.Once());
        }

        [Test]
        public void should_download_poster_when_useBanners_is_false()
        {
            Mocker.Resolve<Xbmc>().CreateForSeries(series, tvdbSeries);
            Mocker.GetMock<BannerProvider>().Verify(v => v.Download(tvdbSeries.PosterPath, Path.Combine(series.Path, "folder.jpg")), Times.Once());
        }

        [Test]
        public void should_download_banner_when_useBanners_is_true()
        {
            WithUseBanners();
            Mocker.Resolve<Xbmc>().CreateForSeries(series, tvdbSeries);
            Mocker.GetMock<BannerProvider>().Verify(v => v.Download(tvdbSeries.BannerPath, Path.Combine(series.Path, "folder.jpg")), Times.Once());
        }

        [Test]
        public void should_download_season_poster_when_useBanners_is_false()
        {
            Mocker.Resolve<Xbmc>().CreateForSeries(series, tvdbSeries);
            Mocker.GetMock<BannerProvider>().Verify(v => v.Download(It.Is<string>(s => !s.Contains("banners")), It.IsRegex(@"season\d{2}\.tbn")), Times.Exactly(2));
        }

        [Test]
        public void should_download_season_banner_when_useBanners_is_true()
        {
            WithUseBanners();
            Mocker.Resolve<Xbmc>().CreateForSeries(series, tvdbSeries);
            Mocker.GetMock<BannerProvider>().Verify(v => v.Download(It.Is<string>(s => s.Contains("banners")), It.IsRegex(@"season\d{2}\.tbn")), Times.Exactly(2));
        }

        [Test]
        public void should_download_special_thumb_with_proper_name()
        {
            WithUseBanners();
            WithSpecials();
            Mocker.Resolve<Xbmc>().CreateForSeries(series, tvdbSeries);
            Mocker.GetMock<BannerProvider>().Verify(v => v.Download(It.Is<string>(s => s.Contains("banners")), It.IsRegex(@"season-specials.tbn")), Times.Exactly(1));
        }

        [Test]
        public void should_not_try_to_download_fanart_if_fanart_path_is_empty()
        {
            WithUseBanners();
            tvdbSeries.FanartPath = String.Empty;

            Mocker.Resolve<Xbmc>().CreateForSeries(series, tvdbSeries);
            Mocker.GetMock<BannerProvider>().Verify(v => v.Download(It.IsAny<String>(), Path.Combine(series.Path, "fanart.jpg")), Times.Never());
        }

        [Test]
        public void should_not_try_to_download_banner_if_banner_path_is_empty()
        {
            WithUseBanners();
            tvdbSeries.BannerPath = String.Empty;

            Mocker.Resolve<Xbmc>().CreateForSeries(series, tvdbSeries);
            Mocker.GetMock<BannerProvider>().Verify(v => v.Download(It.IsAny<String>(), Path.Combine(series.Path, "folder.jpg")), Times.Never());
        }

        [Test]
        public void should_not_try_to_download_poster_if_poster_path_is_empty()
        {
            tvdbSeries.PosterPath = String.Empty;

            Mocker.Resolve<Xbmc>().CreateForSeries(series, tvdbSeries);
            Mocker.GetMock<BannerProvider>().Verify(v => v.Download(It.IsAny<String>(), Path.Combine(series.Path, "folder.jpg")), Times.Never());
        }
    }
}