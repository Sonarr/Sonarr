using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource.Tvdb;
using NzbDrone.Core.MetadataSource.Tvdb.Resource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MetadataSource.Tvdb
{
    [TestFixture]
    public class TvdbApiClientFixture : CoreTest<TvdbApiClient>
    {
        private const string FakeToken = "fake-bearer-token-12345";
        private const string FakeApiKey = "test-api-key";

        [SetUp]
        public void Setup()
        {
            // Use a real CacheManager so that cache.Get(key, factory) invokes the
            // factory lambda, exercising the private Fetch* HTTP methods.
            Mocker.SetConstant<ICacheManager>(new CacheManager());

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.TvdbApiKey)
                .Returns(FakeApiKey);
        }

        /// <summary>
        /// Build an HttpResponse&lt;T&gt; by serialising the resource to JSON and letting
        /// the generic constructor deserialise it — matching the real runtime behaviour.
        /// </summary>
        private static HttpResponse<T> BuildResponse<T>(string url, T resource, HttpStatusCode statusCode = HttpStatusCode.OK)
            where T : new()
        {
            var json = resource.ToJson();
            var httpResponse = new HttpResponse(
                new HttpRequest(url),
                new HttpHeader(),
                json,
                statusCode);
            return new HttpResponse<T>(httpResponse);
        }

        private static HttpResponse<T> BuildErrorResponse<T>(string url, HttpStatusCode statusCode)
            where T : new()
        {
            var httpResponse = new HttpResponse(
                new HttpRequest(url),
                new HttpHeader(),
                "{}",
                statusCode);
            return new HttpResponse<T>(httpResponse);
        }

        private void GivenSuccessfulAuth()
        {
            var tokenResponse = new TvdbResponseResource<TvdbTokenResource>
            {
                Status = "success",
                Data = new TvdbTokenResource { Token = FakeToken }
            };

            var response = BuildResponse("https://api4.thetvdb.com/v4/login", tokenResponse);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Post<TvdbResponseResource<TvdbTokenResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("/login"))))
                .Returns(response);
        }

        private void GivenEpisodeResponse(int tvdbSeriesId, string seasonType, List<TvdbEpisodeResource> episodes)
        {
            var pageResource = new TvdbResponseResource<TvdbEpisodePageResource>
            {
                Status = "success",
                Data = new TvdbEpisodePageResource { Episodes = episodes }
            };

            var response = BuildResponse(
                $"https://api4.thetvdb.com/v4/series/{tvdbSeriesId}/episodes/{seasonType}/eng",
                pageResource);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get<TvdbResponseResource<TvdbEpisodePageResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains($"series/{tvdbSeriesId}/episodes/{seasonType}"))))
                .Returns(response);
        }

        private void GivenAvailableOrderingsResponse(int tvdbSeriesId, List<TvdbSeasonResource> seasons)
        {
            var extendedResponse = new TvdbResponseResource<TvdbSeriesExtendedResource>
            {
                Status = "success",
                Data = new TvdbSeriesExtendedResource
                {
                    Id = tvdbSeriesId,
                    Name = "Test Series",
                    Seasons = seasons
                }
            };

            var response = BuildResponse(
                $"https://api4.thetvdb.com/v4/series/{tvdbSeriesId}/extended",
                extendedResponse);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get<TvdbResponseResource<TvdbSeriesExtendedResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains($"series/{tvdbSeriesId}/extended"))))
                .Returns(response);
        }

        [Test]
        public void should_throw_when_api_key_not_configured()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.TvdbApiKey)
                .Returns(string.Empty);

            Assert.Throws<InvalidOperationException>(
                () => Subject.GetEpisodesByOrdering(78874, EpisodeOrderType.Dvd));
        }

        [Test]
        public void should_fetch_episodes_with_dvd_ordering()
        {
            GivenSuccessfulAuth();

            var episodes = new List<TvdbEpisodeResource>
            {
                new TvdbEpisodeResource { Id = 297999, SeasonNumber = 1, Number = 1, Name = "Serenity" },
                new TvdbEpisodeResource { Id = 297989, SeasonNumber = 1, Number = 2, Name = "The Train Job" }
            };

            GivenEpisodeResponse(78874, "dvd", episodes);

            var result = Subject.GetEpisodesByOrdering(78874, EpisodeOrderType.Dvd);

            result.Should().HaveCount(2);
            result[0].Id.Should().Be(297999);
            result[0].Name.Should().Be("Serenity");
            result[0].Number.Should().Be(1);
        }

        [Test]
        public void should_return_empty_list_when_ordering_not_found()
        {
            GivenSuccessfulAuth();

            var response = BuildErrorResponse<TvdbResponseResource<TvdbEpisodePageResource>>(
                "https://api4.thetvdb.com/v4/series/78874/episodes/regional/eng",
                HttpStatusCode.NotFound);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get<TvdbResponseResource<TvdbEpisodePageResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("regional"))))
                .Returns(response);

            var result = Subject.GetEpisodesByOrdering(78874, EpisodeOrderType.Regional);

            result.Should().BeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_fetch_available_orderings()
        {
            GivenSuccessfulAuth();

            var seasons = new List<TvdbSeasonResource>
            {
                new TvdbSeasonResource { Id = 1, Number = 1, Type = new TvdbSeasonTypeResource { Type = "official", Name = "Aired Order" } },
                new TvdbSeasonResource { Id = 2, Number = 1, Type = new TvdbSeasonTypeResource { Type = "dvd", Name = "DVD Order" } },
                new TvdbSeasonResource { Id = 3, Number = 1, Type = new TvdbSeasonTypeResource { Type = "absolute", Name = "Absolute Order" } },
                new TvdbSeasonResource { Id = 4, Number = 0, Type = new TvdbSeasonTypeResource { Type = "official", Name = "Aired Order" } }
            };

            GivenAvailableOrderingsResponse(78874, seasons);

            var result = Subject.GetAvailableOrderings(78874);

            result.Should().HaveCount(3);
            result.Should().Contain("official");
            result.Should().Contain("dvd");
            result.Should().Contain("absolute");
        }

        [Test]
        public void should_return_empty_orderings_when_no_seasons()
        {
            GivenSuccessfulAuth();
            GivenAvailableOrderingsResponse(99999, new List<TvdbSeasonResource>());

            var result = Subject.GetAvailableOrderings(99999);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_use_correct_season_type_in_url_for_each_order_type()
        {
            GivenSuccessfulAuth();
            GivenEpisodeResponse(78874, "absolute", new List<TvdbEpisodeResource>());

            Subject.GetEpisodesByOrdering(78874, EpisodeOrderType.Absolute);

            Mocker.GetMock<IHttpClient>()
                .Verify(v => v.Get<TvdbResponseResource<TvdbEpisodePageResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("episodes/absolute"))),
                    Times.Once());
        }

        [TestCase(EpisodeOrderType.Default, "official")]
        [TestCase(EpisodeOrderType.Dvd, "dvd")]
        [TestCase(EpisodeOrderType.Absolute, "absolute")]
        [TestCase(EpisodeOrderType.Alternate, "alternate")]
        [TestCase(EpisodeOrderType.AltDvd, "altdvd")]
        [TestCase(EpisodeOrderType.Regional, "regional")]
        public void map_order_type_should_produce_correct_season_type(EpisodeOrderType orderType, string expected)
        {
            TvdbApiClient.MapOrderTypeToSeasonType(orderType).Should().Be(expected);
        }

        [TestCase("official", EpisodeOrderType.Default)]
        [TestCase("dvd", EpisodeOrderType.Dvd)]
        [TestCase("absolute", EpisodeOrderType.Absolute)]
        [TestCase("alternate", EpisodeOrderType.Alternate)]
        [TestCase("altdvd", EpisodeOrderType.AltDvd)]
        [TestCase("regional", EpisodeOrderType.Regional)]
        [TestCase("bogus", EpisodeOrderType.Default)]
        public void map_season_type_should_produce_correct_order_type(string seasonType, EpisodeOrderType expected)
        {
            TvdbApiClient.MapSeasonTypeToOrderType(seasonType).Should().Be(expected);
        }

        [Test]
        public void should_paginate_when_first_page_returns_500_episodes()
        {
            GivenSuccessfulAuth();

            // First page: exactly 500 episodes (triggers pagination)
            var page1Episodes = new List<TvdbEpisodeResource>();
            for (var i = 0; i < 500; i++)
            {
                page1Episodes.Add(new TvdbEpisodeResource { Id = i, SeasonNumber = 1, Number = i + 1, Name = $"Ep{i + 1}" });
            }

            var page1Resource = new TvdbResponseResource<TvdbEpisodePageResource>
            {
                Status = "success",
                Data = new TvdbEpisodePageResource { Episodes = page1Episodes }
            };

            var page1Response = BuildResponse(
                "https://api4.thetvdb.com/v4/series/78874/episodes/dvd/eng",
                page1Resource);

            // Second page: fewer than 500 (final page)
            var page2Episodes = new List<TvdbEpisodeResource>
            {
                new TvdbEpisodeResource { Id = 500, SeasonNumber = 2, Number = 1, Name = "Ep501" }
            };

            var page2Resource = new TvdbResponseResource<TvdbEpisodePageResource>
            {
                Status = "success",
                Data = new TvdbEpisodePageResource { Episodes = page2Episodes }
            };

            var page2Response = BuildResponse(
                "https://api4.thetvdb.com/v4/series/78874/episodes/dvd/eng?page=1",
                page2Resource);

            // First call (no page param) returns page 1
            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get<TvdbResponseResource<TvdbEpisodePageResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("episodes/dvd") && !r.Url.ToString().Contains("page="))))
                .Returns(page1Response);

            // Second call (page=1) returns page 2
            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get<TvdbResponseResource<TvdbEpisodePageResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("episodes/dvd") && r.Url.ToString().Contains("page="))))
                .Returns(page2Response);

            var result = Subject.GetEpisodesByOrdering(78874, EpisodeOrderType.Dvd);

            result.Should().HaveCount(501);
            result[0].Id.Should().Be(0);
            result[500].Id.Should().Be(500);
        }

        [Test]
        public void should_return_partial_results_on_non_404_http_error()
        {
            GivenSuccessfulAuth();

            var response = BuildErrorResponse<TvdbResponseResource<TvdbEpisodePageResource>>(
                "https://api4.thetvdb.com/v4/series/78874/episodes/dvd/eng",
                HttpStatusCode.InternalServerError);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get<TvdbResponseResource<TvdbEpisodePageResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("episodes/dvd"))))
                .Returns(response);

            var result = Subject.GetEpisodesByOrdering(78874, EpisodeOrderType.Dvd);

            result.Should().BeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_empty_orderings_when_http_error()
        {
            GivenSuccessfulAuth();

            var response = BuildErrorResponse<TvdbResponseResource<TvdbSeriesExtendedResource>>(
                "https://api4.thetvdb.com/v4/series/78874/extended",
                HttpStatusCode.InternalServerError);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get<TvdbResponseResource<TvdbSeriesExtendedResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("extended"))))
                .Returns(response);

            var result = Subject.GetAvailableOrderings(78874);

            result.Should().BeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_empty_orderings_when_data_is_null()
        {
            GivenSuccessfulAuth();

            var extendedResponse = new TvdbResponseResource<TvdbSeriesExtendedResource>
            {
                Status = "success",
                Data = null
            };

            var response = BuildResponse(
                "https://api4.thetvdb.com/v4/series/78874/extended",
                extendedResponse);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get<TvdbResponseResource<TvdbSeriesExtendedResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("extended"))))
                .Returns(response);

            var result = Subject.GetAvailableOrderings(78874);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_throw_on_auth_http_error()
        {
            var response = BuildErrorResponse<TvdbResponseResource<TvdbTokenResource>>(
                "https://api4.thetvdb.com/v4/login",
                HttpStatusCode.Unauthorized);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Post<TvdbResponseResource<TvdbTokenResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("/login"))))
                .Returns(response);

            Assert.Throws<InvalidOperationException>(
                () => Subject.GetEpisodesByOrdering(78874, EpisodeOrderType.Dvd));
        }

        [Test]
        public void should_throw_when_auth_returns_empty_token()
        {
            var tokenResponse = new TvdbResponseResource<TvdbTokenResource>
            {
                Status = "success",
                Data = new TvdbTokenResource { Token = string.Empty }
            };

            var response = BuildResponse("https://api4.thetvdb.com/v4/login", tokenResponse);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Post<TvdbResponseResource<TvdbTokenResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("/login"))))
                .Returns(response);

            Assert.Throws<InvalidOperationException>(
                () => Subject.GetEpisodesByOrdering(78874, EpisodeOrderType.Dvd));
        }

        [Test]
        public void should_throw_when_auth_returns_null_token_data()
        {
            var tokenResponse = new TvdbResponseResource<TvdbTokenResource>
            {
                Status = "success",
                Data = null
            };

            var response = BuildResponse("https://api4.thetvdb.com/v4/login", tokenResponse);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Post<TvdbResponseResource<TvdbTokenResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("/login"))))
                .Returns(response);

            Assert.Throws<InvalidOperationException>(
                () => Subject.GetEpisodesByOrdering(78874, EpisodeOrderType.Dvd));
        }

        [Test]
        public void should_break_loop_when_episode_data_is_null()
        {
            GivenSuccessfulAuth();

            var pageResource = new TvdbResponseResource<TvdbEpisodePageResource>
            {
                Status = "success",
                Data = new TvdbEpisodePageResource { Episodes = null }
            };

            var response = BuildResponse(
                "https://api4.thetvdb.com/v4/series/78874/episodes/dvd/eng",
                pageResource);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get<TvdbResponseResource<TvdbEpisodePageResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("episodes/dvd"))))
                .Returns(response);

            var result = Subject.GetEpisodesByOrdering(78874, EpisodeOrderType.Dvd);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_break_loop_when_response_data_is_null()
        {
            GivenSuccessfulAuth();

            var pageResource = new TvdbResponseResource<TvdbEpisodePageResource>
            {
                Status = "success",
                Data = null
            };

            var response = BuildResponse(
                "https://api4.thetvdb.com/v4/series/78874/episodes/dvd/eng",
                pageResource);

            Mocker.GetMock<IHttpClient>()
                .Setup(s => s.Get<TvdbResponseResource<TvdbEpisodePageResource>>(
                    It.Is<HttpRequest>(r => r.Url.ToString().Contains("episodes/dvd"))))
                .Returns(response);

            var result = Subject.GetEpisodesByOrdering(78874, EpisodeOrderType.Dvd);

            result.Should().BeEmpty();
        }

        [Test]
        public void map_order_type_should_return_official_for_unknown_enum_value()
        {
            TvdbApiClient.MapOrderTypeToSeasonType((EpisodeOrderType)99).Should().Be("official");
        }
    }
}
