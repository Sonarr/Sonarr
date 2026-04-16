using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource.Tmdb;
using NzbDrone.Core.MetadataSource.Tmdb.Resource;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSource.Tmdb
{
    [TestFixture]
    public class TmdbProxyFixture : CoreTest<TmdbProxy>
    {
        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(c => c.TmdbApiKey)
                  .Returns("tmdb-api-key");
        }

        [Test]
        public void should_map_search_results_with_tvdb_ids()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Get<TmdbTvSearchResponse>(It.Is<HttpRequest>(r => r.Url.FullUri.Contains("/search/tv"))))
                  .Returns<HttpRequest>(r => CreateResponse(r, new TmdbTvSearchResponse
                  {
                      Results = new List<TmdbTvSearchResult>
                      {
                          new TmdbTvSearchResult
                          {
                              Id = 1399,
                              Name = "Game of Thrones",
                              Overview = "Winter is coming",
                              FirstAirDate = "2011-04-17",
                              PosterPath = "/poster.jpg",
                              BackdropPath = "/backdrop.jpg",
                              OriginalLanguage = "en",
                              OriginCountry = new List<string> { "US" }
                          }
                      }
                  }));

            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Get<TmdbExternalIdsResource>(It.Is<HttpRequest>(r => r.Url.FullUri.Contains("/tv/1399/external_ids"))))
                  .Returns<HttpRequest>(r => CreateResponse(r, new TmdbExternalIdsResource
                  {
                      TvdbId = 121361,
                      ImdbId = "tt0944947"
                  }));

            var result = Subject.SearchForNewSeries("Game of Thrones");

            result.Should().ContainSingle();
            result[0].Title.Should().Be("Game of Thrones");
            result[0].TvdbId.Should().Be(121361);
            result[0].TmdbId.Should().Be(1399);
            result[0].ImdbId.Should().Be("tt0944947");
        }

        [Test]
        public void should_map_series_details_and_episodes()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Get<TmdbTvDetailsResource>(It.Is<HttpRequest>(r => r.Url.FullUri.Contains("/tv/1399?"))))
                  .Returns<HttpRequest>(r => CreateResponse(r, new TmdbTvDetailsResource
                  {
                      Id = 1399,
                      Name = "Game of Thrones",
                      Overview = "Winter is coming",
                      FirstAirDate = "2011-04-17",
                      LastAirDate = "2019-05-19",
                      EpisodeRunTime = new List<int> { 55 },
                      Status = "Ended",
                      Networks = new List<TmdbNetworkResource> { new TmdbNetworkResource { Name = "HBO" } },
                      OriginCountry = new List<string> { "US" },
                      OriginalLanguage = "en",
                      PosterPath = "/poster.jpg",
                      BackdropPath = "/backdrop.jpg",
                      VoteAverage = 8.4m,
                      VoteCount = 100,
                      ExternalIds = new TmdbExternalIdsResource { TvdbId = 121361, ImdbId = "tt0944947" },
                      AggregateCredits = new TmdbAggregateCreditsResource
                      {
                          Cast = new List<TmdbCastResource>
                          {
                              new TmdbCastResource
                              {
                                  Name = "Kit Harington",
                                  ProfilePath = "/kit.jpg",
                                  Roles = new List<TmdbRoleResource> { new TmdbRoleResource { Character = "Jon Snow" } }
                              }
                          }
                      },
                      ContentRatings = new TmdbContentRatingsResource
                      {
                          Results = new List<TmdbContentRatingResource>
                          {
                              new TmdbContentRatingResource { CountryCode = "US", Rating = "TV-MA" }
                          }
                      },
                      Images = new TmdbImagesResource
                      {
                          Posters = new List<TmdbImageResource> { new TmdbImageResource { FilePath = "/poster2.jpg" } },
                          Backdrops = new List<TmdbImageResource> { new TmdbImageResource { FilePath = "/backdrop2.jpg" } },
                          Logos = new List<TmdbImageResource> { new TmdbImageResource { FilePath = "/logo.png" } }
                      },
                      Seasons = new List<TmdbSeasonSummaryResource>
                      {
                          new TmdbSeasonSummaryResource { SeasonNumber = 1, PosterPath = "/season1.jpg" }
                      }
                  }));

            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Get<TmdbSeasonDetailsResource>(It.Is<HttpRequest>(r => r.Url.FullUri.Contains("/tv/1399/season/1?"))))
                  .Returns<HttpRequest>(r => CreateResponse(r, new TmdbSeasonDetailsResource
                  {
                      SeasonNumber = 1,
                      PosterPath = "/season1.jpg",
                      Episodes = new List<TmdbEpisodeResource>
                      {
                          new TmdbEpisodeResource
                          {
                              Name = "Winter Is Coming",
                              Overview = "Ned visits King's Landing.",
                              AirDate = "2011-04-17",
                              EpisodeNumber = 1,
                              SeasonNumber = 1,
                              Runtime = 62,
                              VoteAverage = 8.0m,
                              VoteCount = 10,
                              StillPath = "/still.jpg",
                              EpisodeType = "finale"
                          }
                      }
                  }));

            var result = Subject.GetSeriesInfo(121361, 1399);

            result.Item1.Title.Should().Be("Game of Thrones");
            result.Item1.TvdbId.Should().Be(121361);
            result.Item1.TmdbId.Should().Be(1399);
            result.Item1.Network.Should().Be("HBO");
            result.Item1.Certification.Should().Be("TV-MA");
            result.Item1.Actors.Should().ContainSingle(a => a.Name == "Kit Harington" && a.Character == "Jon Snow");
            result.Item2.Should().ContainSingle();
            result.Item2[0].Title.Should().Be("Winter Is Coming");
            result.Item2[0].Runtime.Should().Be(62);
            result.Item2[0].Images.Should().NotBeEmpty();
        }

        private static HttpResponse<T> CreateResponse<T>(HttpRequest request, T resource)
            where T : new()
        {
            var response = new HttpResponse(request, new HttpHeader { { "Content-Type", "application/json" } }, resource.ToJson());

            return new HttpResponse<T>(response);
        }
    }
}
