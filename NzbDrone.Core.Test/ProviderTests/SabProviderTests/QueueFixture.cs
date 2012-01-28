// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SabProviderTests
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

            var fakeConfig = Mocker.GetMock<ConfigProvider>();
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

            result.Should().HaveCount(4);
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
                                      Quality = new Quality { QualityType = QualityTypes.SDTV, Proper = false },
                                      Series = new Series { Title = "30 Rock", CleanTitle = Parser.NormalizeTitle("30 Rock") },
                                  };


            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeTrue();
        }

        [TestCase(2, new[] { 5 }, "30 Rock", QualityTypes.Bluray1080p, true, Description = "Same Series, Diffrent Season, Episode")]
        [TestCase(1, new[] { 6 }, "30 Rock", QualityTypes.Bluray1080p, true, Description = "Same series, diffrent episodes")]
        [TestCase(1, new[] { 6, 7, 8 }, "30 Rock", QualityTypes.Bluray1080p, true, Description = "Same series, diffrent episodes")]
        [TestCase(1, new[] { 6 }, "Some other show", QualityTypes.Bluray1080p, true, Description = "Diffrent series, same season, episdoe")]
        [TestCase(1, new[] { 5 }, "Rock", QualityTypes.Bluray1080p, true, Description = "Similar series, same season, episodes")]
        [TestCase(1, new[] { 5 }, "30 Rock", QualityTypes.Bluray720p, false, Description = "Same series, higher quality")]
        [TestCase(1, new[] { 5 }, "30 Rock", QualityTypes.HDTV, true, Description = "Same series, higher quality")]
        public void IsInQueue_should_not_find_diffrent_episode_queue(int season, int[] episodes, string title, QualityTypes qualityType, bool proper)
        {
            WithFullQueue();

            var parseResult = new EpisodeParseResult
                                  {
                                      EpisodeTitle = "Title",
                                      EpisodeNumbers = new List<int>(episodes),
                                      SeasonNumber = season,
                                      Quality = new Quality { QualityType = qualityType, Proper = proper },
                                      Series = new Series { Title = title },
                                  };

            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeFalse();
        }

        [TestCase(1, new[] { 5 }, "30 Rock", QualityTypes.SDTV, false, Description = "Same Series, lower quality")]
        [TestCase(1, new[] { 5 }, "30 rocK", QualityTypes.SDTV, false, Description = "Same Series, diffrent casing")]
        [TestCase(1, new[] { 5 }, "30 RocK", QualityTypes.HDTV, false, Description = "Same Series, same quality")]
        [TestCase(1, new[] { 5, 6 }, "30 RocK", QualityTypes.HDTV, false, Description = "Same Series, same quality, one diffrent episode")]
        [TestCase(1, new[] { 5, 6 }, "30 RocK", QualityTypes.HDTV, false, Description = "Same Series, same quality, one diffrent episode")]
        [TestCase(4, new[] { 8 }, "Parks and Recreation", QualityTypes.WEBDL, false, Description = "Same Series, same quality")]
        public void IsInQueue_should_find_same_or_lower_quality_episode_queue(int season, int[] episodes, string title, QualityTypes qualityType, bool proper)
        {
            WithFullQueue();

            var parseResult = new EpisodeParseResult
                                  {
                                      EpisodeTitle = "Title",
                                      EpisodeNumbers = new List<int>(episodes),
                                      SeasonNumber = season,
                                      Quality = new Quality { QualityType = qualityType, Proper = proper },
                                      Series = new Series { Title = title, CleanTitle = Parser.NormalizeTitle(title) },
                                  };

            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeTrue();
        }

        [TestCase(5, new[] { 13 }, "The Big Bang Theory", QualityTypes.SDTV, false, Description = "Same Series, lower quality")]
        [TestCase(5, new[] { 13 }, "The Big Bang Theory", QualityTypes.HDTV, false, Description = "Same Series, same quality")]
        [TestCase(5, new[] { 13 }, "The Big Bang Theory", QualityTypes.HDTV, true, Description = "Same Series, same quality")]
        [TestCase(5, new[] { 13, 14 }, "The Big Bang Theory", QualityTypes.HDTV, false, Description = "Same Series, same quality, one diffrent episode")]
        public void IsInQueue_should_find_items_marked_as_duplicate(int season, int[] episodes, string title, QualityTypes qualityType, bool proper)
        {
            WithFullQueue();

            var parseResult = new EpisodeParseResult
            {
                EpisodeTitle = "Title",
                EpisodeNumbers = new List<int>(episodes),
                SeasonNumber = season,
                Quality = new Quality { QualityType = qualityType, Proper = proper },
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
                Quality = new Quality { QualityType = QualityTypes.Bluray1080p, Proper = true },
                Series = new Series { Title = "Test", CleanTitle = Parser.NormalizeTitle("Test") },
            };

            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeFalse();
        }

        [TearDown]
        public void TearDown()
        {
            ExceptionVerification.IgnoreWarns();
        }


    }
}