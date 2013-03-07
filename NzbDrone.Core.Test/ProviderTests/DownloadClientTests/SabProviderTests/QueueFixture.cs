// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Download.Clients.Sabnzbd;

using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.DownloadClientTests.SabProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class QueueFixture : CoreTest
    {
        [SetUp]
        public void Setup()
        {
            //Setup
            string sabHost = "192.168.5.55";
            int sabPort = 2222;
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";
            string cat = "tv";

            var fakeConfig = Mocker.GetMock<IConfigService>();
            fakeConfig.SetupGet(c => c.SabHost).Returns(sabHost);
            fakeConfig.SetupGet(c => c.SabPort).Returns(sabPort);
            fakeConfig.SetupGet(c => c.SabApiKey).Returns(apikey);
            fakeConfig.SetupGet(c => c.SabUsername).Returns(username);
            fakeConfig.SetupGet(c => c.SabPassword).Returns(password);
            fakeConfig.SetupGet(c => c.SabTvCategory).Returns(cat);
        }

        private void WithFullQueue()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(
                           s =>
                           s.DownloadString(
                                            "http://192.168.5.55:2222/api?mode=queue&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns(File.ReadAllText(@".\Files\Queue.txt"));
        }

        private void WithEmptyQueue()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(
                           s =>
                           s.DownloadString(
                                            "http://192.168.5.55:2222/api?mode=queue&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns(File.ReadAllText(@".\Files\QueueEmpty.txt"));
        }

        private void WithFailResponse()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString(It.IsAny<String>())).Returns(File.ReadAllText(@".\Files\JsonError.txt"));
        }

        private void WithUnknownPriorityQueue()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(
                           s =>
                           s.DownloadString(
                                            "http://192.168.5.55:2222/api?mode=queue&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns(File.ReadAllText(@".\Files\QueueUnknownPriority.txt"));
        }

        [Test]
        public void GetQueue_should_return_an_empty_list_when_the_queue_is_empty()
        {
            WithEmptyQueue();

            var result = Mocker.Resolve<SabProvider>().GetQueue();

            result.Should().BeEmpty();
        }

        [Test]
        public void GetQueue_should_throw_when_there_is_an_error_getting_the_queue()
        {
            WithFailResponse();

            Assert.Throws<ApplicationException>(() => Mocker.Resolve<SabProvider>().GetQueue(), "API Key Incorrect");
        }

        [Test]
        public void GetQueue_should_return_a_list_with_items_when_the_queue_has_items()
        {
            WithFullQueue();

            var result = Mocker.Resolve<SabProvider>().GetQueue();

            result.Should().HaveCount(7);
        }

        [Test]
        public void GetQueue_should_return_a_list_with_items_even_when_priority_is_non_standard()
        {
            WithUnknownPriorityQueue();

            var result = Mocker.Resolve<SabProvider>().GetQueue();

            result.Should().HaveCount(7);
            result.Should().OnlyContain(i => i.Priority == SabPriorityType.Normal);
        }

        [Test]
        public void is_in_queue_should_find_if_exact_episode_is_in_queue()
        {
            WithFullQueue();

            var parseResult = new EpisodeParseResult
                                  {
                                      EpisodeTitle = "Title",
                                      EpisodeNumbers = new List<int> { 5 },
                                      SeasonNumber = 1,
                                      Quality = new QualityModel { Quality = Quality.SDTV, Proper = false },
                                      Series = new Series { Title = "30 Rock", CleanTitle = Parser.NormalizeTitle("30 Rock") },
                                  };


            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeTrue();
        }

        [Test]
        public void is_in_queue_should_find_if_exact_daily_episode_is_in_queue()
        {
            WithFullQueue();

            var parseResult = new EpisodeParseResult
            {
                Quality = new QualityModel { Quality = Quality.Bluray720p, Proper = false },
                AirDate = new DateTime(2011, 12, 01),
                Series = new Series { Title = "The Dailyshow", CleanTitle = Parser.NormalizeTitle("The Dailyshow"), SeriesTypes = SeriesTypes.Daily },
            };


            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeTrue();
        }

        [Test]
        public void is_in_queue_should_find_if_exact_full_season_release_is_in_queue()
        {
            WithFullQueue();


            var parseResult = new EpisodeParseResult
            {
                Quality = new QualityModel { Quality = Quality.Bluray720p, Proper = false },
                FullSeason = true,
                SeasonNumber = 5,
                Series = new Series { Title = "My Name is earl", CleanTitle = Parser.NormalizeTitle("My Name is earl") },
            };

            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeTrue();
        }

        public static object[] DifferentEpisodeCases =
        {
            new object[] { 2, new[] { 5 }, "30 Rock", Quality.Bluray1080p, true }, //Same Series, Different Season, Episode
            new object[] { 1, new[] { 6 }, "30 Rock", Quality.Bluray1080p, true }, //Same series, different episodes
            new object[] { 1, new[] { 6, 7, 8 }, "30 Rock", Quality.Bluray1080p, true }, //Same series, different episodes
            new object[] { 1, new[] { 6 }, "Some other show", Quality.Bluray1080p, true }, //Different series, same season, episode
            new object[] { 1, new[] { 5 }, "Rock", Quality.Bluray1080p, true }, //Similar series, same season, episodes
            new object[] { 1, new[] { 5 }, "30 Rock", Quality.Bluray720p, false }, //Same series, higher quality
            new object[] { 1, new[] { 5 }, "30 Rock", Quality.HDTV720p, true } //Same series, higher quality
        };

        [Test, TestCaseSource("DifferentEpisodeCases")]
        public void IsInQueue_should_not_find_diffrent_episode_queue(int season, int[] episodes, string title, Quality qualityType, bool proper)
        {
            WithFullQueue();

            var parseResult = new EpisodeParseResult
                                  {
                                      EpisodeTitle = "Title",
                                      EpisodeNumbers = new List<int>(episodes),
                                      SeasonNumber = season,
                                      Quality = new QualityModel { Quality = qualityType, Proper = proper },
                                      Series = new Series { Title = title, CleanTitle = Parser.NormalizeTitle(title) },
                                  };

            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeFalse();
        }

        public static object[] LowerQualityCases =
        {
            new object[] { 1, new[] { 5 }, "30 Rock", Quality.SDTV, false }, //Same Series, lower quality
            new object[] { 1, new[] { 5 }, "30 rocK", Quality.SDTV, false }, //Same Series, different casing
            new object[] { 1, new[] { 5 }, "30 RocK", Quality.HDTV720p, false }, //Same Series, same quality
            new object[] { 1, new[] { 5, 6 }, "30 RocK", Quality.HDTV720p, false }, //Same Series, same quality, one different episode
            new object[] { 1, new[] { 5, 6 }, "30 RocK", Quality.HDTV720p, false }, //Same Series, same quality, one different episode
            new object[] { 4, new[] { 8 }, "Parks and Recreation", Quality.WEBDL720p, false }, //Same Series, same quality
        };

        [Test, TestCaseSource("LowerQualityCases")]
        public void IsInQueue_should_find_same_or_lower_quality_episode_queue(int season, int[] episodes, string title, Quality qualityType, bool proper)
        {
            WithFullQueue();

            var parseResult = new EpisodeParseResult
                                  {
                                      EpisodeTitle = "Title",
                                      EpisodeNumbers = new List<int>(episodes),
                                      SeasonNumber = season,
                                      Quality = new QualityModel { Quality = qualityType, Proper = proper },
                                      Series = new Series { Title = title, CleanTitle = Parser.NormalizeTitle(title) },
                                  };

            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeTrue();
        }

        public static object[] DuplicateItemsCases =
        {
            new object[] { 5, new[] { 13 }, "The Big Bang Theory", Quality.SDTV, false }, //Same Series, lower quality
            new object[] { 5, new[] { 13 }, "The Big Bang Theory", Quality.HDTV720p, false }, //Same Series, same quality
            new object[] { 5, new[] { 13 }, "The Big Bang Theory", Quality.HDTV720p, true }, //Same Series, same quality
            new object[] { 5, new[] { 13, 14 }, "The Big Bang Theory", Quality.HDTV720p, false } //Same Series, same quality, one diffrent episode
        };

        [Test, TestCaseSource("DuplicateItemsCases")]
        public void IsInQueue_should_find_items_marked_as_duplicate(int season, int[] episodes, string title, Quality qualityType, bool proper)
        {
            WithFullQueue();

            var parseResult = new EpisodeParseResult
            {
                EpisodeTitle = "Title",
                EpisodeNumbers = new List<int>(episodes),
                SeasonNumber = season,
                Quality = new QualityModel { Quality = qualityType, Proper = proper },
                Series = new Series { Title = title, CleanTitle = Parser.NormalizeTitle(title) },
            };

            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeTrue();
        }

        public static object[] DoubleEpisodeCases =
        {
            new object[] { 3, new[] { 14, 15 }, "My Name Is Earl", Quality.Bluray720p, false },
            new object[] { 3, new[] { 15 }, "My Name Is Earl", Quality.DVD, false },
            new object[] { 3, new[] { 14 }, "My Name Is Earl", Quality.HDTV720p, false },
            new object[] { 3, new[] { 15, 16 }, "My Name Is Earl", Quality.SDTV, false }
        };

        [Test, TestCaseSource("DoubleEpisodeCases")]
        public void IsInQueue_should_find_double_episodes_(int season, int[] episodes, string title, Quality qualityType, bool proper)
        {
            WithFullQueue();

            var parseResult = new EpisodeParseResult
            {
                EpisodeTitle = "Title",
                EpisodeNumbers = new List<int>(episodes),
                SeasonNumber = season,
                Quality = new QualityModel { Quality = qualityType, Proper = proper },
                Series = new Series { Title = title, CleanTitle = Parser.NormalizeTitle(title) },
            };

            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeTrue();
        }

        [Test]
        public void IsInQueue_should_return_false_if_queue_is_empty()
        {
            WithEmptyQueue();

            var parseResult = new EpisodeParseResult
            {
                EpisodeTitle = "Title",
                EpisodeNumbers = new List<int> { 1 },
                SeasonNumber = 2,
                Quality = new QualityModel { Quality = Quality.Bluray1080p, Proper = true },
                Series = new Series { Title = "Test", CleanTitle = Parser.NormalizeTitle("Test") },
            };

            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeFalse();
        }

        [Test]
        public void GetQueue_should_parse_timeleft_with_hours_greater_than_24_hours()
        {
            WithFullQueue();

            var result = Mocker.Resolve<SabProvider>().GetQueue();

            result.Should().NotBeEmpty();
            var timeleft = result.First(q => q.Id == "SABnzbd_nzo_qv6ilb").Timeleft;
            timeleft.Days.Should().Be(2);
            timeleft.Hours.Should().Be(9);
            timeleft.Minutes.Should().Be(27);
            timeleft.Seconds.Should().Be(45);
        }

        [TearDown]
        public void TearDown()
        {
            ExceptionVerification.IgnoreWarns();
        }


    }
}